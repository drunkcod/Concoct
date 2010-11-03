using System;
using System.Net;
using System.Web;
using System.IO;

namespace Concoct.Web
{
    public class HttpListenerContextAdapter : HttpContextBase
    {
        class HttpListenerWorkerRequest : HttpWorkerRequest
        {
            readonly HttpListenerContextAdapter context;

            public HttpListenerWorkerRequest(HttpListenerContextAdapter context)
            {
                this.context = context;
            }

            HttpListenerRequestAdapter Request { get { return context.request; } }
            HttpListenerResponseAdapter Response { get { return context.response; } }

            public override void EndOfRequest()
            {
                throw new NotImplementedException();
            }

            public override void FlushResponse(bool finalFlush) {
                Response.Flush();
            }

            public override string GetHttpVerbName() {
                return Request.HttpMethod;
            }

            public override string GetHttpVersion()
            {
                return Request.HttpVersion;
            }

            public override string GetLocalAddress()
            {
                throw new NotImplementedException();
            }

            public override int GetLocalPort()
            {
                throw new NotImplementedException();
            }

            public override string GetQueryString()
            {
                throw new NotImplementedException();
            }

            public override string GetRawUrl() { return Request.RawUrl; }

            public override string GetRemoteAddress()
            {
                throw new NotImplementedException();
            }

            public override int GetRemotePort()
            {
                throw new NotImplementedException();
            }

            public override string GetUriPath() { return Request.RawUrl; }

            public override void SendKnownResponseHeader(int index, string value) {
                SendUnknownResponseHeader(GetKnownRequestHeader(index), value);
            }

            public override void SendResponseFromFile(IntPtr handle, long offset, long length)
            {
                throw new NotImplementedException();
            }

            public override void SendResponseFromFile(string filename, long offset, long length)
            {
                throw new NotImplementedException();
            }

            public override void SendResponseFromMemory(byte[] data, int length)
            {
                throw new NotImplementedException();
            }

            public override void SendStatus(int statusCode, string statusDescription) {
                Response.Write(string.Format("{0} {1}", statusCode, statusDescription));
            }

            public override void SendUnknownResponseHeader(string name, string value) {
                Response.Write(string.Format("{0}: {1}\r\n", name, value));
            }
        }

        readonly HttpListenerRequestAdapter request;
        readonly HttpListenerResponseAdapter response;
        readonly HttpServerUtilityBase server;

        public HttpListenerContextAdapter(HttpListenerContext context, string virtualPath, string physicalPath) {
            this.request = new HttpListenerRequestAdapter(context.Request, virtualPath, MakeRelativeUriFunc(context.Request.Url, virtualPath));
            this.response = new HttpListenerResponseAdapter(context.Response);
            this.server = new ConcoctHttpServerUtility(physicalPath);
        }

        public override HttpRequestBase Request { get { return request; } }
        public override HttpSessionStateBase Session { get { return null; } }
        public override HttpResponseBase Response { get { return response; } }
        public override HttpServerUtilityBase Server { get { return server; } }

        public HttpContext AsHttpContext()
        {
            var worker = new HttpListenerWorkerRequest(this);
            return new HttpContext(worker);
        }

        public static Func<Uri,string> MakeRelativeUriFunc(Uri request, string virtualPath){
            var baseUri = new Uri(string.Format("{0}://{1}{2}{3}/", 
                request.Scheme, 
                request.Host, 
                virtualPath.StartsWith("/") ? string.Empty : "/",
                virtualPath));
            return uri => {
                var relative = "~/" + baseUri.MakeRelativeUri(uri);
                if (relative.StartsWith("~/../"))
                    return "~/";
                else
                    return relative;
            };
        }
    }
}