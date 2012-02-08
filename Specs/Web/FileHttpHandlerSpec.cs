using System;
using System.IO;
using System.Net;
using Concoct.IO;
using Cone;

namespace Concoct.Web
{
    [Describe(typeof(FileHttpHandler))]
    public class FileHttpHandlerSpec : HttpServiceFixture
    {
		IFileInfo ResponseFile;

        [Row(".???", "application/octet-stream")
        ,Row(".css", "text/css")
        ,Row(".xml", "text/xml")
        ,Row(".txt", "text/plain")
        ,Row(".gif", "image/gif")
        ,Row(".png", "image/png")
        ,Row(".jpg", "image/jpeg")
        ,DisplayAs("{0} => {1}")]
        public void content_type(string extension, string mimeType) {
            var fileInfo = new Moq.Mock<IFileInfo>();
            fileInfo.Setup(x => x.Extension).Returns(extension);
            fileInfo.Setup(x => x.OpenRead()).Returns(StreamWith("<this doesn't matter>"));
			ResponseFile = fileInfo.Object;

            WithResponseFrom("http://localhost:8080", response => {
                Verify.That(() => response.ContentType == mimeType);
            });
        }


        public void content_matches() {
            var message = "Hello World!";
            var fileInfo = new Moq.Mock<IFileInfo>();
            fileInfo.Setup(x => x.Extension).Returns(".bin");
            fileInfo.Setup(x => x.OpenRead()).Returns(StreamWith(message));
			ResponseFile = fileInfo.Object;

            WithResponseFrom("http://localhost:8080", response => {
                Verify.That(() => response.ContentLength == message.Length);
                using(var reader = new StreamReader(response.GetResponseStream()))
                    Verify.That(() => reader.ReadToEnd() == message);
            });
        }

		protected override IServiceController CreateService() {
			return new HttpListenerAcceptor(new IPEndPoint(IPAddress.Any, 8080), new Uri("/", UriKind.Relative), new BasicRequestHandler("/", ".", new FileHttpHandler(ResponseFile)));
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
