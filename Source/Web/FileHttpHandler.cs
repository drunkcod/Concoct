using System.Web;
using Concoct.IO;

namespace Concoct.Web
{
    public class FileHttpHandler : IHttpHandler
    {
        readonly IFileInfo info;

        public FileHttpHandler(IFileInfo info) { 
            this.info = info;
        }

        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context) {
            var response = context.Response;
            response.ContentType = MimeTypeFromExtension(info.Extension);
            using(var file = info.OpenRead())
                file.CopyTo(response.OutputStream);
            response.Flush();
        }

        string MimeTypeFromExtension(string extension) {
            switch(extension) {
                case ".css": return "text/css";
                case ".gif": return "image/gif";
                case ".jpg": return "image/jpeg";
                case ".png": return "image/png";
                case ".txt": return "text/plain";
                case ".xml": return "text/xml";
                default: return "application/octet-stream";
            }
        }
    }
}
