using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Concoct
{
    interface IRequestHandler
    {
        void Process(HttpListenerContext context);
    }

    class HttpListenerAcceptor
    {
        class HttpListenerAcceptorContext
        {
            public HttpListenerAcceptor Listener;
            public int Offset;

            public void BeginDispatch(IAsyncResult async){
                Listener.Dispatch(this, async);
            }
        }

        readonly HttpListener listener = new HttpListener();
        readonly HttpListenerAcceptorContext[] contexts;
        readonly WaitHandle[] backlog;
        readonly IRequestHandler handler;

        public HttpListenerAcceptor(IPEndPoint bindTo, IRequestHandler handler) {
            listener.Prefixes.Add(string.Format("http://{0}:{1}/", PrefixFrom(bindTo.Address), bindTo.Port));
            contexts = new HttpListenerAcceptorContext[1];
            backlog = new WaitHandle[contexts.Length];
            this.handler = handler;
            for(int i = 0; i != contexts.Length; ++i)
                contexts[i] = new HttpListenerAcceptorContext {
                    Listener = this,
                    Offset = i
                };
        }

        static string PrefixFrom(IPAddress address) {
            if (address == IPAddress.Any)
                return "*";
            return address.ToString();
        }

        public void Start() {
            listener.Start();
            for(int i = 0; i != contexts.Length; ++i)
                BeginGetContext(contexts[i]);
        }

        public void Stop() {
            listener.Stop();
            WaitHandle.WaitAll(backlog);
        }

        void Dispatch(HttpListenerAcceptorContext context, IAsyncResult asyncResult) {
            BeginGetContext(context);
            handler.Process(listener.EndGetContext(asyncResult));
        }

        void BeginGetContext(HttpListenerAcceptorContext context)
        {
            backlog[context.Offset] = listener.BeginGetContext(BeginRequest, context).AsyncWaitHandle;
        }

        static void BeginRequest(IAsyncResult async) {
            var acceptor = (HttpListenerAcceptorContext)async.AsyncState;
            acceptor.BeginDispatch(async);
        }
    }

    class EmptyHttpFileCollection : HttpFileCollectionBase
    {
        public override int Count { get { return 0; } }
    }

    class HttpListenerRequestAdapter : HttpRequestBase
    {
        readonly HttpListenerRequest request;
        private NameValueCollection form;

        public HttpListenerRequestAdapter(HttpListenerRequest request) {
            this.request = request;
        }

        public override string AppRelativeCurrentExecutionFilePath { get { return "~" + RawUrl; } }
        public override string PathInfo { get { return string.Empty; } }
        public override string RawUrl { get { return request.Url.AbsolutePath; } }
        public string HttpVersion { get { return request.ProtocolVersion.ToString(); } }
        public override string HttpMethod { get { return request.HttpMethod; } }
        public override NameValueCollection Form { 
            get {
                if(form != null)
                    return form;
                form = new NameValueCollection();
                if (request.ContentType != "application/x-www-form-urlencoded")
                    return form;
                var reader = new StreamReader(request.InputStream);
                var data = reader.ReadToEnd();
                foreach(var item in data.Split(new []{ '&'}, StringSplitOptions.RemoveEmptyEntries)){
                    var parts = item.Split('=');
                    form.Add(parts[0], HttpUtility.UrlDecode(parts[1]));                                            
                }
                return form;
            }
        }
        public override NameValueCollection QueryString { get { return request.QueryString; } }
        public override HttpFileCollectionBase Files { get { return new EmptyHttpFileCollection(); } }
        public override NameValueCollection Headers { get { return request.Headers; } }
        public override void ValidateInput() { }        
    }

    class HttpListenerResponseAdapter : HttpResponseBase
    {
        readonly HttpListenerResponse response;
        private readonly TextWriter output;
        public HttpListenerResponseAdapter(HttpListenerResponse response) {
            this.response = response;
            this.output = new StreamWriter(response.OutputStream);
        }

        public override void Write(string s) {
            output.Write(s);
        }

        public override void Flush() {
            output.Flush();
        }

        public override void End() {
            output.Close();
        }
    }

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

        public HttpListenerContextAdapter(HttpListenerContext context) {
            this.request = new HttpListenerRequestAdapter(context.Request);
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
    }

    class MvcRequestHandler : IRequestHandler
    {
        public void Process(HttpListenerContext context)
        {
            var httpContext = new HttpListenerContextAdapter(context);
            var data = RouteTable.Routes.GetRouteData(httpContext);
            var request = new RequestContext(httpContext, data);
            var handler = data.RouteHandler.GetHttpHandler(request);
            handler.ProcessRequest(httpContext.AsHttpContext());
            httpContext.Response.End();
        }
    }

    class BasicControllerFactory : IControllerFactory
    {
        readonly Dictionary<string, Func<IController>> controllers = new Dictionary<string, Func<IController>>();
        
        public IController CreateController(RequestContext requestContext, string controllerName) {
            return controllers[controllerName]();
        }

        public void RegisterController(string name, Type type) {
            var builder = (Func<IController>)Expression.Lambda(
                typeof(Func<IController>),
                Expression.New(type))
            .Compile();
            controllers.Add(name, builder);    
        }

        public void ReleaseController(IController controller)
        {}
    }

    class Program
    {
        static void Main(string[] args)
        {
            RouteTable.Routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Home", action = "Index", id=(string)null});
            var controllerFactory = new BasicControllerFactory();
            controllerFactory.RegisterController("Home", typeof(Controllers.HomeController));
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
            var acceptor = new HttpListenerAcceptor(
                new IPEndPoint(IPAddress.Any, 8080), 
                new MvcRequestHandler());
            acceptor.Start();
            Console.WriteLine("Waiting for connections.");
            Console.ReadKey();
            acceptor.Stop();
        }
    }
}

namespace Concoct.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public string Index()
        {
            return string.Format("<form method='POST' action='#'><input type='submit' name='id' value='Foo'/></form>");           
        }
        [HttpPost]
        public string Index(string id)
        {
            return string.Format("Hello {0} World!", id ?? "Mvc");
        }
    }
}
