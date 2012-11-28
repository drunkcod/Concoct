using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;

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

			public override int GetPreloadedEntityBody(byte[] buffer, int offset) {
				return 0;
			}

			public override int GetPreloadedEntityBodyLength() {
				return 0;
			}

			public override bool IsEntireEntityBodyIsPreloaded() {
				return false;
			}

			public override byte[] GetPreloadedEntityBody() {
				return new byte[0];
			}

			public override int ReadEntityBody(byte[] buffer, int size) {
				return ReadEntityBody(buffer, 0, size);
			}

			public override int ReadEntityBody(byte[] buffer, int offset, int size) {
				var bytesRead = Request.InputStream.Read(buffer, offset, size);
				return bytesRead;
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

            public override string GetQueryString() { return Request.Url.Query; }

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
				switch(index) {
					case HttpWorkerRequest.HeaderContentType: Response.ContentType = value; break;
					default: SendUnknownResponseHeader(KnownHttpHeaders.FxHeaders[index], value); break;
				}
            }

            public override void SendResponseFromFile(IntPtr handle, long offset, long length)
            {
                throw new NotImplementedException();
            }

            public override void SendResponseFromFile(string filename, long offset, long length)
            {
                throw new NotImplementedException();
            }

            public override void SendResponseFromMemory(byte[] data, int length) { 
                Response.OutputStream.Write(data, 0, length); 
            }

            public override void SendStatus(int statusCode, string statusDescription) {
                Response.Status = statusDescription;
                Response.StatusCode = statusCode;
            }

            public override void SendUnknownResponseHeader(string name, string value) {
                Response.Headers.Add(name, value);
            }

            public override string GetKnownRequestHeader(int index) {
                return GetUnknownRequestHeader(KnownHttpHeaders.FxHeaders[index]);
            }

			public override string GetUnknownRequestHeader(string name) {
				return Request.Headers[name];
			}
        }

		class HttpListenerSessionState : HttpSessionStateBase
		{
			readonly Dictionary<string, object> state = new Dictionary<string,object>();

			public override object this[string name]
			{
				get {
 					object value;
					state.TryGetValue(name, out value);
					return value; 
				}
				
				set { state[name] = value; }
			}
		}

        readonly HttpListenerRequestAdapter request;
        readonly HttpListenerResponseAdapter response;
        readonly HttpServerUtilityBase server;
        readonly Cache cache;
		readonly HttpSessionStateBase session;

        public HttpListenerContextAdapter(HttpListenerContext context, string virtualPath, string physicalPath) {
            this.request = new HttpListenerRequestAdapter(context.Request, virtualPath, MakeRelativeUriFunc(context.Request.Url, virtualPath));
            this.response = new HttpListenerResponseAdapter(context.Response);
            this.server = new ConcoctHttpServerUtility(physicalPath);
            this.cache = new Cache();
			this.session = new HttpListenerSessionState();
        }

		public override Exception[] AllErrors { get { throw new NotImplementedException(); } }
        public override HttpRequestBase Request { get { return request; } }
        public override HttpSessionStateBase Session { get { return session; } }
        public override HttpResponseBase Response { get { return response; } }
        public override HttpServerUtilityBase Server { get { return server; } }
        public override Cache Cache { get { return cache; } }
		public SessionStateBehavior SessionStateBehavior { get; private set; }

		public override void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior)
		{ 
			SessionStateBehavior = sessionStateBehavior;
		}

        public void AsHttpContext(Action<HttpContext> action, bool alignResponses)
        {
            var worker = new HttpListenerWorkerRequest(this);
            var context = new HttpContext(worker);
			HttpContext.Current = context;
            try {
                action(context);
            } finally {
				if(alignResponses)
					AlignResponses(context);
                context.Response.Flush();
				HttpContext.Current = null;
            }
        }

		private void AlignResponses(HttpContext context) {
			context.Response.ContentType = response.ContentType ;
		}

        public static Func<Uri,string> MakeRelativeUriFunc(Uri request, string virtualPath){
            var baseUri = new Uri(string.Format("{0}://{1}:{2}{3}{4}", 
                request.Scheme, 
                request.Host,
                request.Port,
                virtualPath.StartsWith("/") ? string.Empty : "/",
                virtualPath));
            return uri => "~/" + baseUri.MakeRelativeUri(uri);
        }
    }
}