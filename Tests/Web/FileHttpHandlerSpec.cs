using System;
using System.IO;
using System.Net;
using Concoct.IO;
using Cone;

namespace Concoct.Web
{
    [Describe(typeof(FileHttpHandler))]
    public class FileHttpHandlerSpec
    {
        [Row(".???", "application/octet-stream")
        ,Row(".css", "text/css")
        ,DisplayAs("{0} => {1}")]
        public void content_type(string extension, string mimeType) {
            var message = "Hello World!";
            var fileInfo = new Moq.Mock<IFileInfo>();
            fileInfo.Setup(x => x.Extension).Returns(extension);
            fileInfo.Setup(x => x.OpenRead()).Returns(StreamWith(message));

            WithResponseFrom(fileInfo.Object, response => {
                Verify.That(() => response.ContentType == mimeType);
            });
        }

        public void content_matches() {
            var message = "Hello World!";
            var fileInfo = new Moq.Mock<IFileInfo>();
            fileInfo.Setup(x => x.Extension).Returns(".bin");
            fileInfo.Setup(x => x.OpenRead()).Returns(StreamWith(message));

            WithResponseFrom(fileInfo.Object, response => {
                Verify.That(() => response.ContentLength == message.Length);
                using(var reader = new StreamReader(response.GetResponseStream()))
                    Verify.That(() => reader.ReadToEnd() == message);
            });
        }

        void WithResponseFrom(IFileInfo fileInfo, Action<WebResponse> withResponse) {
            var listener = new HttpListenerAcceptor(new IPEndPoint(IPAddress.Any, 8080), new Uri("/", UriKind.Relative), new BasicRequestHandler("/", ".", new FileHttpHandler(fileInfo)));
            try {
                listener.Start();
                var request = WebRequest.Create("http://localhost:8080/");
                using(var response = request.GetResponse())
                    withResponse(response);
            } finally {
                listener.Stop();
            }
        }

        Stream StreamWith(string message) {
            var content = new MemoryStream();
            var writer = new StreamWriter(content);
            writer.Write(message);                
            writer.Flush();
            content.Position = 0;
            return content;
        }
    }
}
