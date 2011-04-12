using System.IO;
using System.Web;
using Concoct.IO;

namespace Concoct.Web
{
    public class FileHttpHandler : IHttpHandler
    {
        readonly string path;

        public FileHttpHandler(string path) { 
            this.path = path;
        }

        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context) {
            var info = new FileInfo(path);
            context.Response.ContentType = ContentTypeFromExtension(info.Extension);
            using(var file = info.OpenRead())
                file.CopyTo(context.Response.OutputStream);
            context.Response.Flush();
        }

        string ContentTypeFromExtension(string extension) {
            switch(extension) {
                case ".css": return "text/css";
                default: return "application/octet-stream";
            }
        }
    }
}
