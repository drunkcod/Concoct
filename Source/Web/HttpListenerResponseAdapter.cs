using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Concoct.Web
{
    class HttpListenerResponseAdapter : HttpResponseBase
    {
        readonly HttpListenerResponse response;
        TextWriter output;
        
        public HttpListenerResponseAdapter(HttpListenerResponse response) {
            this.response = response;
            this.response.ContentEncoding = Encoding.UTF8;
            this.output = new StreamWriter(response.OutputStream);
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

        public override TextWriter Output {
            get { return output ?? (output = new StreamWriter(response.OutputStream, response.ContentEncoding)); }
        }

        public override void Write(string s) { Output.Write(s); }

        public override void Flush() { Output.Flush(); }

        public override void End() { Output.Close(); }

        void ResetOutput()
        {
            if(output != null)
                output.Flush();
            output = null;
        }
    }
}