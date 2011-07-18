using System.IO;
using Cone;
using System.Collections.Generic;

namespace Concoct.Web
{
    class MultiPartFormDataSample 
    {
        //http://www.w3.org/TR/html401/interact/forms.html#h-17.13.4.2
        public const string ContentsOfFile1 = "... contents of file1.txt ...";
        public static readonly string[] MultiPartSampleFormdataAndSingleFile = new[] {
                "--AaB03x",
                "Content-Disposition: form-data; name=\"submit-name\"",
                "",
                "Larry",
                "--AaB03x",
                "Content-Disposition: form-data; name=\"files\"; filename=\"file1.txt\"",
                "Content-Type: text/plain",
                "",
                ContentsOfFile1,
                "--AaB03x--"
            };

        public const string Boundry = "AaB03x";

        public static MemoryStream CreateSampleStream() {
            return CreateStream(MultiPartSampleFormdataAndSingleFile);
        }

        static MemoryStream CreateStream(params string[] lines) {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            foreach(var line in lines)
                writer.WriteLine(line);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }

    [Describe(typeof(MultiPartStream))]
    public class MultiPartStreamSpec
    {
        [Context("multipart/form-data sample with field and file")]
        public class MultiPartSampleFormdataAndSingleFileContext 
        {
            public void sample_contains_two_parts() {
                var data = new MultiPartStream(MultiPartFormDataSample.Boundry);
                var parts = 0;
                data.PartReady += (s, e) => ++parts;
                data.Read(MultiPartFormDataSample.CreateSampleStream());
                Verify.That(() => parts == 2);
            }
        }
        [Context("header parsing")]
        public class HeaderParsing
        {
            [Row("Content-Disposition: form-data; filename=\"R:\\foo.txt\"", "Content-Disposition", "form-data; filename=\"R:\\foo.txt\"")
            ,DisplayAs("{0} -> {{{1}, {2}}}")]
            public void parse_header(string input, string name, string value) {
                var header = MultiPartStream.ParseHeader(input);
                Verify.That(() => header.Key == name);
                Verify.That(() => header.Value== value);
            }
        }
    }
}
