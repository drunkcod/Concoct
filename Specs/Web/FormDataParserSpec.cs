using System.IO;
using System.Text;
using Cone;

namespace Concoct.Web
{
    class SimpleRequestStream : IRequestStream
    {
        readonly MemoryStream data;

        public SimpleRequestStream() : this(new MemoryStream()) { }

        public SimpleRequestStream(string body) {
            data = new MemoryStream();
            var writer = new StreamWriter(data);
            writer.Write(body);
            writer.Flush();
            data.Position = 0;
        }

        public SimpleRequestStream(MemoryStream data) {
            this.data = data;
        }

        public string ContentType {
            get; set;
        }

        public MemoryStream Data { get { return data; } }

        public int ContentLength { get { return (int)data.Length; } }

        public Stream InputStream { get { return Data; } }

    }

    [Describe(typeof(FormDataParser))]
    public class FormDataParserSpec
    {
        [Context(FormDataParser.ContentTypeFormUrlEncoded)]
        public class FormUrlEncoded
        {
            const string SampleInput = "hello=world&foo=bar"; 
            [Context(SampleInput)]
            public class Sample1
            {
                FormDataParser FormData;

                [BeforeEach]
                public void parse_sample() {
                    FormData = new FormDataParser();
                    var request = new SimpleRequestStream();
                    request.ContentType = FormDataParser.ContentTypeFormUrlEncoded;
                    var data = Encoding.UTF8.GetBytes(SampleInput);
                    request.Data.Write(data, 0, data.Length);
                    request.Data.Position = 0;
                    FormData.ParseFormAndFiles(request);
                }

                public void has_2_parameters() {
                    Verify.That(() => FormData.Fields.Count == 2);
                }

                [Row("hello", "world")
                ,Row("foo", "bar")
                ,DisplayAs("{0} = {1}")]
                public void expected_parameter_values(string field, string value) {
                    Verify.That(() => FormData.Fields[field] == value);
                }
            }
        }

        [Context(FormDataParser.ContentTypeMultipartFormData + " sample")]
        public class MultipartData
        {
            FormDataParser FormData;

            [BeforeEach]
            public void parse_sample() {
                FormData = new FormDataParser();
                var request = new SimpleRequestStream(MultiPartFormDataSample.CreateSampleStream());
                request.ContentType = FormDataParser.ContentTypeMultipartFormData + "; boundary=" + MultiPartFormDataSample.Boundry;
                Verify.That(() => FormData.ParseFormAndFiles(request));
            }

            public void contains_a_field() {
                Verify.That(() => FormData.Fields.Count == 1);
            }

            public void field_matches_sample() {
                Verify.That(() => FormData.Fields["submit-name"] == "Larry");
            }

            public void contains_a_file() {
                Verify.That(() => FormData.Files.Count == 1);
            }

            [DisplayAs("the file is name file1.txt")]
            public void file_name() {
                Verify.That(() => FormData.Files[0].FileName == "file1.txt");
            }

            public void file_content_type_matches_sample() {
                Verify.That(() => FormData.Files[0].ContentType == "text/plain");
            }

            public void file_content_length_matches_sample() {
                Verify.That(() => FormData.Files[0].ContentLength == MultiPartFormDataSample.ContentsOfFile1.Length);
            }
        }
        
        public void whats_going_on() {
var body =
@"-----------------------------7db18c1726147c
Content-Disposition: form-data; name=""product""

RawFeasibility
-----------------------------7db18c1726147c
Content-Disposition: form-data; name=""environment""

beta
-----------------------------7db18c1726147c--";
                var formData = new FormDataParser();
                var request = new SimpleRequestStream(body);
                request.ContentType = "multipart/form-data; boundary=---------------------------7db18c1726147c";
                Verify.That(() => formData.ParseFormAndFiles(request));
                Verify.That(() => formData.Fields["environment"] == "beta");
                Verify.That(() => formData.Fields["product"] == "RawFeasibility");
        }

        public void fails_to_parse_if_content_type_is_null() {
            var formData = new FormDataParser();
            var request = new SimpleRequestStream();
            request.ContentType = null;
            Verify.That(() => formData.ParseFormAndFiles(request) == false);
        }
    }
}
