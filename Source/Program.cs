using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Hosting;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Reflection.Emit;

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

        public HttpListenerAcceptor(IPEndPoint bindTo, IRequestHandler handler) : this(bindTo, string.Empty, handler)
        { }

        public HttpListenerAcceptor(IPEndPoint bindTo, string subfix, IRequestHandler handler)
        {
            listener.Prefixes.Add(string.Format("http://{0}:{1}{2}/", PrefixFrom(bindTo.Address), bindTo.Port, subfix));
            contexts = new HttpListenerAcceptorContext[1];
            backlog = new WaitHandle[contexts.Length];
            this.handler = handler;
            for (int i = 0; i != contexts.Length; ++i)
                contexts[i] = new HttpListenerAcceptorContext
                {
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
        public override Encoding ContentEncoding {
            get { return request.ContentEncoding; }
            set { throw new NotSupportedException(); }
        }
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
                var bytes = new byte[request.ContentLength64];
                var bytesRead = request.InputStream.Read(bytes, 0, bytes.Length);
                var data = HttpUtility.UrlDecode(bytes, 0, bytesRead, Encoding.UTF8);
                foreach(var item in data.Split(new []{ '&' }, StringSplitOptions.RemoveEmptyEntries)){
                    var parts = item.Split('=');
                    form.Add(parts[0], parts[1]);
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
            try {
                handler.ProcessRequest(httpContext.AsHttpContext());
            } catch(Exception e) {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Write(e.ToString());                
            }
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

        public void Register(IEnumerable<Type> types)
        {
            foreach(var item in types.Where(x => typeof(IController).IsAssignableFrom(x)))
                RegisterController(item.Name.Replace("Controller", string.Empty), item);
        }

        public void ReleaseController(IController controller)
        {}
    }

    public interface IApplicationLifecycle
    {
        void Start();
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var host = new Program();
            var site = Assembly.LoadFrom(args[0]);

            var types = site.GetTypes();

            var applicationType = types.Where(x => typeof (HttpApplication).IsAssignableFrom(x)).FirstOrDefault();

            var generated = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Concoct.Generated"), AssemblyBuilderAccess.Run);
            var module = generated.DefineDynamicModule("Main");
            var appLife = module.DefineType("ApplicationLifecycle", 
                TypeAttributes.NotPublic | TypeAttributes.Sealed,
                applicationType,
                new[] {typeof (IApplicationLifecycle)});
           
            var start = appLife.DefineMethod("Start", MethodAttributes.Private | MethodAttributes.Virtual);
            var startIl = start.GetILGenerator();

            var appStart = applicationType.GetMethod("Application_Start", BindingFlags.NonPublic | BindingFlags.Instance);
            if(appStart != null) {
                startIl.Emit(OpCodes.Ldarg_0);
                startIl.Emit(OpCodes.Call, appStart);
            }
            startIl.Emit(OpCodes.Ret);
          
            appLife.DefineMethodOverride(start, typeof(IApplicationLifecycle).GetMethod("Start"));

            var foo = (IApplicationLifecycle)appLife.CreateType().GetConstructor(Type.EmptyTypes).Invoke(null);
            foo.Start();
            var controllerFactory = new BasicControllerFactory();
            controllerFactory.Register(types);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
            host.Start(args[1]);
        }

        void Start(string vdir)
        {
            var acceptor = new HttpListenerAcceptor(
                new IPEndPoint(IPAddress.Any, 80),
                vdir,
                new MvcRequestHandler());
            acceptor.Start();
            Console.WriteLine("Waiting for connections.");
            Console.ReadKey();
            acceptor.Stop();
        }
    }
}