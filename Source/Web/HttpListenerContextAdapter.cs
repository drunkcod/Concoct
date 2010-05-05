using System;
using System.Net;
using System.Web;

namespace Concoct.Web
{
    class HttpListenerContextAdapter : HttpContextBase
    {
        class HttpListenerWorkerRequest : HttpWorkerRequest
        {
            readonly HttpListenerContextAdapter context;

            public HttpListenerWorkerRequest(HttpListenerContextAdapter context)
            {
                this.context = context;
            }

            public override void EndOfRequest()
            {
                throw new NotImplementedException();
            }

            public override void FlushResponse(bool finalFlush)
            {
                context.Response.Flush();
            }

            public override string GetHttpVerbName()
            {
                return context.Request.HttpMethod;
            }

            public override string GetHttpVersion()
            {
                return context.request.HttpVersion;
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

            public override string GetRawUrl()
            {
                throw new NotImplementedException();
            }

            public override string GetRemoteAddress()
            {
                throw new NotImplementedException();
            }

            public override int GetRemotePort()
            {
                throw new NotImplementedException();
            }

            public override string GetUriPath() { return context.Request.RawUrl; }

            public override void SendKnownResponseHeader(int index, string value)
            {
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

            public override void SendStatus(int statusCode, string statusDescription)
            {
                context.Response.Write(string.Format("{0} {1}", statusCode, statusDescription));
            }

            public override void SendUnknownResponseHeader(string name, string value)
            {
                context.Response.Write(string.Format("{0}: {1}\r\n", name, value));
            }
        }

        readonly HttpListenerRequestAdapter request;
        readonly HttpResponseBase response;

        public HttpListenerContextAdapter(HttpListenerContext context, string virtualPath) {
            this.request = new HttpListenerRequestAdapter(context.Request, virtualPath, MakeRelativeUriFunc(context.Request.Url, virtualPath));
            this.response = new HttpListenerResponseAdapter(context.Response);
        }

        public override HttpRequestBase Request { get { return request; } }
        public override HttpSessionStateBase Session { get { return null; } }
        public override HttpResponseBase Response { get { return response; } }

        public HttpContext AsHttpContext()
        {
            var worker = new HttpListenerWorkerRequest(this);
            return new HttpContext(worker);
        }

        static Func<Uri,string> MakeRelativeUriFunc(Uri request, string virtualPath){
            var baseUri = new Uri(string.Format("{0}://{1}{2}/", request.Scheme, request.Host, virtualPath));
            return uri => "~/" + baseUri.MakeRelativeUri(uri);
        }
    }
}