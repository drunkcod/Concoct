﻿using System;
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

            readonly MemoryStream partData = new MemoryStream();
            readonly MultiPartBoundaryBuffer boundry;
            readonly byte[] boundaryBytes;
            readonly Action<byte[], int> partReady;
            State state;
            int headerEndPosition;
            bool isFinished;

            public BodyReader(byte[] boundaryBytes, Action<byte[], int> partReady) {
                this.boundaryBytes = boundaryBytes;
                this.boundry = new MultiPartBoundaryBuffer(boundaryBytes.Length, partData.WriteByte);
                this.state = State.Buffering;
                this.partReady = partReady;
            }

            public bool IsFinished { get { return isFinished; } }

            public IMultiPartStreamState ProcessByte(byte value) {
                boundry.WriteByte(value);
                switch(state) {
                    case State.Buffering:               
                        if(headerEndPosition == 0 && boundry.EndsWith(HeaderSeparator))
                        headerEndPosition = (int)partData.Position + boundry.Size;
                        if(boundry.Matches(boundaryBytes)) {
                            boundry.Discard();
                            state = State.AtBoundary;
                        }
                        break;

                    case State.AtBoundary:
                        if(boundry.Size < 2)
                            break;

                        var partSeparator = boundry.Matches(LineSeparator);
                        isFinished = boundry.Matches(Dash, Dash);
                        Debug.Assert(partSeparator || isFinished);

                        boundry.Discard();
                        if(partData.Position != 0) {
                            partReady(partData.ToArray(), headerEndPosition);
                            partData.Position = 0;
                            headerEndPosition = 0;
                        }
                        state = State.Buffering;
                        break;
                }
                return this;
            }
        }

        public MultiPartStream(string boundary) {
            var boundryLength = Encoding.GetByteCount(boundary);
            var boundaryBytes = new byte[BoundaryPrefix.Length + boundryLength];
           
            BoundaryPrefix.CopyTo(boundaryBytes, 0);
            Encoding.GetBytes(boundary, 0, boundary.Length, boundaryBytes, BoundaryPrefix.Length);

            var body = new BodyReader(boundaryBytes, (bytes, headerEndPosition) => OnPartReady(ReadPart(bytes, headerEndPosition)));
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
            return new KeyValuePair<string,string>(input.Substring(0, boundary), input.Substring(boundary + 1).Trim());
        }

        byte ReadByte(Stream stream) {
            var b = stream.ReadByte();      
            if(b < 0) throw new InvalidDataException();
            return (byte)b;                  
        }

        MimePart ReadPart(byte[] bytes, int headerEndPosition) {
            var part = new MimePart(new ArraySegment<byte>(bytes, headerEndPosition, bytes.Length - headerEndPosition));
            
            using(var headerReader = new StreamReader(new MemoryStream(bytes, 0, headerEndPosition), Encoding.ASCII)) {
                for(string line; (line = headerReader.ReadLine()) != "";) {
                    var header = ParseHeader(line);
                    part.Add(header.Key, header.Value);
                }
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
