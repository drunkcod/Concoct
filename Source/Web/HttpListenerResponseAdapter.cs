using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Concoct.Web
{
    class HttpListenerResponseAdapter : HttpResponseBase
    {
        readonly HttpListenerResponse response;
        MemoryStream outputStream;
        TextWriter output;

        public HttpListenerResponseAdapter(HttpListenerResponse response) {
            this.response = response;
            this.response.ContentEncoding = new UTF8Encoding(false);
            this.outputStream = new MemoryStream();
            ContentType = "text/html";
        }

        public override int StatusCode {
            get { return response.StatusCode; }
            set { response.StatusCode = value; }
        }

        public override Encoding ContentEncoding {
            get { return response.ContentEncoding; }
            set {
                ResetOutput();
                response.ContentEncoding = value;
            }
        }

        public override string ContentType {
            get { return response.ContentType; }
            set { response.ContentType = value + "; charset=" + ContentEncoding.WebName; }
        }

        public override string RedirectLocation {
            get { return response.RedirectLocation; }
            set { response.RedirectLocation = value; }
        }

        public override TextWriter Output {
            get { return output ?? (output = new StreamWriter(OutputStream, ContentEncoding)); }
        }

        public override Stream OutputStream { get {return outputStream; } }

        public override string ApplyAppPathModifier(string virtualPath) {
            return virtualPath;
        }

        public override void Write(string s) { Output.Write(s); }

        public override void Flush() { Output.Flush(); }

        public override void End() {
            Flush();
            response.Close(outputStream.ToArray(), false);
        }

        void ResetOutput()
        {
            if(output != null)
                output.Flush();
            output = null;
        }
    }
}