using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace Concoct.Web
{
    class MultiPartBoundryBuffer 
    {
        readonly byte[] bytes;
        readonly Action<byte> overflow;
        int position;
        int size;

        public MultiPartBoundryBuffer(int size, Action<byte> overflow) {
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
            bytes[position] = value;
            position = (position + 1) % bytes.Length;
        }

        public bool Matches(params byte[] other) {
            if(size != other.Length)
                return false;
            for(var i = 0; i != size; ++i)
                if(bytes[Index(i)] != other[i])
                    return false;
            return true;
        }

        public bool EndsWith(params byte[] tail) {
            if(size < tail.Length)
                return false;
            for(var i = 0; i != tail.Length; ++i)
                if(bytes[Index(size - tail.Length + i)] != tail[i])
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
        readonly byte[] BoundryPrefix = new[]{ CR, LF, Dash, Dash };
        
        readonly byte[] boundryBytes;

        public MultiPartStream(string boundry) {
            var boundryLength = Encoding.GetByteCount(boundry);
            boundryBytes = new byte[BoundryPrefix.Length + boundryLength];
           
            BoundryPrefix.CopyTo(boundryBytes, 0);
            Encoding.GetBytes(boundry, 0, boundry.Length, boundryBytes, BoundryPrefix.Length);
        }

        public event EventHandler<MimeBodyPartDataEventArgs> PartReady;

        public void Read(Stream stream) {
            var partData = new MemoryStream();
            var boundry = new MultiPartBoundryBuffer(boundryBytes.Length, partData.WriteByte);
            int headerEndPosition = 0;
            for(;;) {
                var value = ReadByte(stream);
                boundry.WriteByte(value);
               
                if(headerEndPosition == 0 && boundry.EndsWith(HeaderSeparator))
                    headerEndPosition = (int)partData.Position + boundry.Size;
                if(boundry.Matches(boundryBytes)) {
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

        byte ReadByte(Stream stream) {
            var b = stream.ReadByte();      
            if(b < 0) throw new InvalidDataException();
            return (byte)b;                  
        }

        MimePart ReadPart(byte[] bytes, int headerEndPosition) {
            var headers = new NameValueCollection();
            using(var headerReader = new StreamReader(new MemoryStream(bytes, 0, headerEndPosition), Encoding.ASCII)) {
                for(string line; (line = headerReader.ReadLine()) != "";) {
                    var parts = line.Split(':');
                    headers.Add(parts[0], parts[1]);
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
