using System.IO;
using System.Text;
using Cone;

namespace Concoct
{
    class MultiPartStream 
    {
        const byte Dash = (byte)'-';
        const byte CR = 13;
        const byte LF = 10;
        const int BoundryPrefixLength = 2;
        
        readonly byte[] boundryBytes;
        readonly Stream stream;

        public MultiPartStream(string boundry, Stream stream) {
            var boundryLength = Encoding.GetByteCount(boundry);
            boundryBytes = new byte[BoundryPrefixLength + boundryLength];
            boundryBytes[0] = boundryBytes[1] = Dash;
            Encoding.GetBytes(boundry, 0, boundry.Length, boundryBytes, BoundryPrefixLength);
            this.stream = stream;
        }

        public bool ReadBoundry() { 
            var buff = new byte[boundryBytes.Length + 2];
            var len = stream.Read(buff, 0, buff.Length);
            if(len != buff.Length)
                throw new InvalidDataException();
            for(var i = 0; i != boundryBytes.Length; ++i)
                if(buff[i] != boundryBytes[i])
                    return false;
            if(buff[buff.Length - 2] == CR && buff[buff.Length - 1] == LF)
                return true;
            if(stream.ReadByte() == CR && stream.ReadByte() == LF && buff[buff.Length - 2] == Dash && buff[buff.Length - 1] == Dash)
                return false;
            throw new InvalidDataException();
        }
        public void ReadHeaders() { }
        public void ReadBody(Stream target) { }

        Encoding Encoding { get { return Encoding.ASCII; } }
    }

    [Describe(typeof(MultiPartStream))]
    public class MultiPartStreamSpec
    {
        static Stream CreateStream(params string[] lines) {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            foreach(var line in lines)
                writer.WriteLine(line);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public void ReadBoundry_indicates_that_part_follow() {
            Verify.That(() => new MultiPartStream("boundry", CreateStream("--boundry")).ReadBoundry() == true);
        }

        public void ReadBoundry_indicate_end_of_stream() {
            Verify.That(() => new MultiPartStream("boundry", CreateStream("--boundry--")).ReadBoundry() == false);
        }

        [Row("")
        ,Row("-")
        ,Row("--q")
        ,Row("--boundryX")
        ,Row("--boundry--x")]
        public void ReadBoundry_raise_error_for_malformed_stream(string input) {
            Verify.Throws<InvalidDataException>.When(() => new MultiPartStream("boundry", CreateStream(input)).ReadBoundry());
        }

        [Context("multipart/form-data sample with field and file")]
        public class MultiPartSampleFormdataAndSingleFileContext 
        {
            //http://www.w3.org/TR/html401/interact/forms.html#h-17.13.4.2
            readonly string[] MultiPartSampleFormdataAndSingleFile = new[] {
                "--AaB03x",
                "Content-Disposition: form-data; name=\"submit-name\"",
                "",
                "Larry",
                "--AaB03x",
                "Content-Disposition: form-data; name=\"files\"; filename=\"file1.txt\"",
                "Content-Type: text/plain",
                "",
                "... contents of file1.txt ...",
                "--AaB03x--"
            };
            const string Boundry = "AaB03x";

            [Pending(Reason = "Long way to go....")]
            public void sample_contains_two_parts() {
                var data = new MultiPartStream(Boundry, CreateStream(MultiPartSampleFormdataAndSingleFile));
                var parts = 0;
                while(data.ReadBoundry()) {
                    data.ReadHeaders();
                    data.ReadBody(Stream.Null);
                    ++parts;
                }

                Verify.That(() => parts == 2);
            }
        }
    }
}
