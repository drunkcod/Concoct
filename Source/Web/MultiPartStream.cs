﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

    public class MimePart
    {
        public NameValueCollection Headers;
        public byte[] Body;

    }

    public class MultiPartStream 
    {
        const byte Dash = (byte)'-';
        const byte CR = 13;
        const byte LF = 10;
        readonly byte[] LineSeparator = new[]{ CR, LF };
        readonly byte[] HeaderSeparator = new[]{ CR, LF, CR, LF };
        readonly byte[] BoundaryPrefix = new[]{ CR, LF, Dash, Dash };
        
        readonly byte[] boundaryBytes;

        public MultiPartStream(string boundary) {
            var boundryLength = Encoding.GetByteCount(boundary);
            boundaryBytes = new byte[BoundaryPrefix.Length + boundryLength];
           
            BoundaryPrefix.CopyTo(boundaryBytes, 0);
            Encoding.GetBytes(boundary, 0, boundary.Length, boundaryBytes, BoundaryPrefix.Length);
        }

        public event EventHandler<MimeBodyPartDataEventArgs> PartReady;

        public void Read(Stream stream) {
            EnsureStartingBoundary(stream);
            var partData = new MemoryStream();
            var boundry = new MultiPartBoundaryBuffer(boundaryBytes.Length, partData.WriteByte);
            int headerEndPosition = 0;
            for(;;) {
                boundry.WriteByte(ReadByte(stream));
               
                if(headerEndPosition == 0 && boundry.EndsWith(HeaderSeparator))
                    headerEndPosition = (int)partData.Position + boundry.Size;
                if(boundry.Matches(boundaryBytes)) {
                    boundry.Discard();
                    boundry.WriteByte(ReadByte(stream));
                    boundry.WriteByte(ReadByte(stream));

                    var partSeparator = boundry.Matches(LineSeparator);
                    var lastPart = boundry.Matches(Dash, Dash);

                    if(partSeparator || lastPart) {
                        boundry.Discard();
                        if(partData.Position != 0) {
                            OnPartReady(ReadPart(partData.ToArray(), headerEndPosition));
                            partData.Position = 0;
                            headerEndPosition = 0;
                        }
                    }
                    if(lastPart)
                        return;
                }
            }
        }

        public static KeyValuePair<string, string> ParseHeader(string input) {
            var boundary = input.IndexOf(':');
            return new KeyValuePair<string,string>(input.Substring(0, boundary), input.Substring(boundary + 1).Trim());
        }

        void EnsureStartingBoundary(Stream stream) {
            var header = new byte[boundaryBytes.Length];
            if(stream.Read(header, 0, header.Length) != header.Length
            || !ElementEquals(header, boundaryBytes, 0, 2, header.Length - 2))
                throw new InvalidOperationException("invalid header");
        }

        bool ElementEquals(byte[] x, byte[] y, int offsetX, int offsetY, int count) {
            for(var i = 0; i != count; ++i)
                if(x[offsetX + i] != y[offsetY + i])
                    return false;
            return true;
        }

        byte ReadByte(Stream stream) {
            var b = stream.ReadByte();      
            if(b < 0) throw new InvalidDataException();
            return (byte)b;                  
        }

        MimePart ReadPart(byte[] bytes, int headerEndPosition) {
            var headers = new NameValueCollection();
            using(var headerReader = new StreamReader(new MemoryStream(bytes, 0, headerEndPosition), Encoding.ASCII)) {
                for(string line; (line = headerReader.ReadLine()) != "";) {
                    var header = ParseHeader(line);
                    headers.Add(header.Key, header.Value);
                }
            }
            var body = new byte[bytes.Length - headerEndPosition];
            Array.Copy(bytes, headerEndPosition, body, 0, body.Length);
            return new MimePart {
                Headers = headers,
                Body = body
            };
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
