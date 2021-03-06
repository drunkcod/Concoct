using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Concoct.Web
{
    class HttpListenerResponseAdapter : HttpResponseBase
    {
		static readonly Encoding DefaultEncoding = new UTF8Encoding(false);
        readonly HttpListenerResponse response;
        readonly MemoryStream outputStream;
        TextWriter output;

		public HttpContext Context;

        public HttpListenerResponseAdapter(HttpListenerResponse response) {
            this.response = response;
            this.outputStream = new MemoryStream();
        }

        public override System.Collections.Specialized.NameValueCollection Headers {
            get { return response.Headers; }
        }

        public override string Status {
            get { return response.StatusDescription; }
            set { response.StatusDescription = value; }
        }

        public override int StatusCode {
            get { return response.StatusCode; }
            set { response.StatusCode = value; }
        }

        public override Encoding ContentEncoding {
            get { return response.ContentEncoding ?? DefaultEncoding; }
            set {
                ResetOutput();
                response.ContentEncoding = value;
            }
        }

        public override string ContentType {
            get { return response.ContentType; }
            set {
				response.ContentType = value;
				if(Context != null) Context.Response.ContentType = value;
			}
        }

        public override string RedirectLocation {
            get { return response.RedirectLocation; }
            set { response.RedirectLocation = value; }
        }

        public override TextWriter Output {
            get { return output ?? (output = new StreamWriter(OutputStream, ContentEncoding)); }
        }

        public override Stream OutputStream { get { return outputStream; } }

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