using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Concoct.Web
{
    class MultiPartBoundaryBuffer 
    {
        readonly byte[] bytes;
        readonly Action<byte> overflow;
        int position;
        int size;
        byte lastByte;

        public MultiPartBoundaryBuffer(int size, Action<byte> overflow) {
            this.bytes = new byte[size];
            this.overflow = overflow;
        }

        public int Size { get { return size; } }

        public void Discard() {
            position = size = 0;    
        }

        public void WriteByte(byte value) {
            if(size == bytes.Length) 
                overflow(bytes[position]);
            else 
                ++size;
            lastByte = bytes[position] = value;
            position = (position + 1) % bytes.Length;
        }

        public bool Matches(params byte[] other) {
            return size == other.Length
                && EndsWith(other);
        }

        public bool EndsWith(params byte[] tail) {
            if(size < tail.Length || lastByte != tail[tail.Length - 1])
                return false;
            for(int i = 0, offset = size - tail.Length; i != tail.Length; ++i)
                if(bytes[Index(offset + i)] != tail[i])
                    return false;
            return true;
        }

        int Index(int x) {  return (bytes.Length + position - size + x) % bytes.Length; }
    }

    public class MimeBodyPartDataEventArgs : EventArgs 
    {
        public MimePart Part;
    }

    public class MultiPartStream 
    {
        const int BufferSize = 1 << 20;
        const byte Dash = (byte)'-';
        const byte CR = 13;
        const byte LF = 10;
        static readonly byte[] LineSeparator = new[]{ CR, LF };
        static readonly byte[] HeaderSeparator = new[]{ CR, LF, CR, LF };
        static readonly byte[] BoundaryPrefix = new[]{ CR, LF, Dash, Dash };
        IMultiPartStreamState state;

        interface IMultiPartStreamState 
        {
            bool IsFinished { get; }
            IMultiPartStreamState ProcessByte(byte value);
        }

        class HeaderReader : IMultiPartStreamState
        {
            readonly byte[] boundaryBytes;        
            readonly byte[] header;
            readonly IMultiPartStreamState next;
            int position;

            public HeaderReader(byte[] boundaryBytes, IMultiPartStreamState next) {
                this.boundaryBytes = boundaryBytes;
                this.header = new byte[boundaryBytes.Length];
                this.next = next;
            }

            public bool IsFinished { get { return false; } }

            public IMultiPartStreamState ProcessByte(byte value) {
                header[position++] = value;
                if(position < boundaryBytes.Length)
                    return this;               
                
                if(!ElementEquals(header, boundaryBytes, 0, 2, header.Length - 2))
                    throw new InvalidOperationException("invalid header");
                
                return next;
            }

            static bool ElementEquals(byte[] x, byte[] y, int offsetX, int offsetY, int count) {
                for(var i = 0; i != count; ++i)
                    if(x[offsetX + i] != y[offsetY + i])
                        return false;
                return true;
            }
        }

        class BodyReader : IMultiPartStreamState
        {
            enum State {
                Buffering, 
                AtBoundary
            }

            public struct LineSegment
            {
                public int Start, Count;
            }

            public delegate void PartReadyHandler(byte[] data, List<LineSegment> lines, int headerEndPosition);

            readonly MemoryStream partData = new MemoryStream();
            readonly MultiPartBoundaryBuffer boundary;
            readonly byte[] boundaryBytes;
            readonly PartReadyHandler partReady;
            State state;
            readonly List<LineSegment> lines = new List<LineSegment>();
            int lineStart;
            int bodyStartPosition;
            bool isFinished;

            public BodyReader(byte[] boundaryBytes, PartReadyHandler partReady) {
                this.boundaryBytes = boundaryBytes;
                this.boundary = new MultiPartBoundaryBuffer(boundaryBytes.Length, partData.WriteByte);
                this.state = State.Buffering;
                this.partReady = partReady;
            }

            public bool IsFinished { get { return isFinished; } }

            public IMultiPartStreamState ProcessByte(byte value) {
                boundary.WriteByte(value);
                switch(state) {
                    case State.Buffering:               
                        if(bodyStartPosition == 0) {
                            var index = (int)partData.Position + boundary.Size;
                            if(boundary.EndsWith(HeaderSeparator))
                                bodyStartPosition = index;
                            else if(boundary.EndsWith(LineSeparator)) {
                                lines.Add(new LineSegment { Start = lineStart, Count = index - lineStart - LineSeparator.Length });
                                lineStart = index;
                            }
                        }
                        if(boundary.Matches(boundaryBytes)) {
                            boundary.Discard();
                            state = State.AtBoundary;
                        }
                        break;

                    case State.AtBoundary:
                        if(boundary.Size < 2)
                            break;

                        var partSeparator = boundary.Matches(LineSeparator);
                        isFinished = boundary.Matches(Dash, Dash);
                        Debug.Assert(partSeparator || isFinished);

                        boundary.Discard();
                        if(HasPartData)
                            PartReady();

                        state = State.Buffering;
                        break;
                }
                return this;
            }

            bool HasPartData { get { return partData.Position != 0; } }

            void PartReady() {
                partReady(partData.ToArray(), lines, bodyStartPosition);
                partData.Position = 0;
                bodyStartPosition = 0;
                lineStart = 0;
                lines.Clear();
            }
        }

        public MultiPartStream(string boundary) {
            var boundryLength = Encoding.GetByteCount(boundary);
            var boundaryBytes = new byte[BoundaryPrefix.Length + boundryLength];
           
            BoundaryPrefix.CopyTo(boundaryBytes, 0);
            Encoding.GetBytes(boundary, 0, boundary.Length, boundaryBytes, BoundaryPrefix.Length);

            var body = new BodyReader(boundaryBytes, (bytes, lines, headerEndPosition) => OnPartReady(ReadPart(bytes, lines, headerEndPosition)));
            state = new HeaderReader(boundaryBytes, body);
        }

        public event EventHandler<MimeBodyPartDataEventArgs> PartReady;

        public void Read(Stream stream) {
            while(!state.IsFinished)
                ProcessByte(ReadByte(stream)); 
        }

        public void Read(Stream stream, int count) {
            var buffer = new byte[BufferSize];
            for(int remaining = count, block; 
                remaining != 0
                && (block = stream.Read(buffer, 0, Math.Min(remaining, buffer.Length))) != 0;) 
            {
                remaining -= block;
                Process(buffer, block);
            }
        }

        void Process(byte[] bytes, int count) {
            for(var i = 0; i != count && !state.IsFinished; ++i)
                ProcessByte(bytes[i]);
        }

        void ProcessByte(byte value) {
            state = state.ProcessByte(value);
        }

        public static KeyValuePair<string, string> ParseHeader(string input) {
            var boundary = input.IndexOf(':');
            return new KeyValuePair<string,string>(input.Substring(0, boundary), input.Substring(boundary + 1).TrimStart());
        }

        byte ReadByte(Stream stream) {
            var b = stream.ReadByte();      
            if(b < 0) throw new InvalidDataException();
            return (byte)b;                  
        }

        MimePart ReadPart(byte[] bytes, List<BodyReader.LineSegment> lines, int headerEndPosition) {
            var part = new MimePart(new ArraySegment<byte>(bytes, headerEndPosition, bytes.Length - headerEndPosition));
            
            for(var i = 0; i != lines.Count; ++i) {
                var segment = lines[i];
                var line = Encoding.GetString(bytes, segment.Start, segment.Count);
                var header = ParseHeader(line);
                part.Add(header.Key, header.Value);
            }

            return part;
        }

        void OnPartReady(MimePart part) {
            var x = PartReady;
            if(x == null)
                return;
            x(this, new MimeBodyPartDataEventArgs { Part = part });
        }

        Encoding Encoding { get { return Encoding.ASCII; } }
    }
}
