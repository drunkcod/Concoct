using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace Concoct
{
    public class HttpListenerAcceptor
    {
        struct HttpListenerAcceptorContext
        {
            public HttpListenerAcceptor Listener;
            public int Offset;

            public HttpListenerAcceptorContext(HttpListenerAcceptor listener, int offset) {
                Listener = listener;
                Offset = offset;
            }

            public void BeginDispatch(IAsyncResult async) {
                Listener.Dispatch(this, async);
            }
        }

        readonly HttpListener listener = new HttpListener();
        readonly HttpListenerAcceptorContext[] contexts;
        WaitHandle[] backlog;
        readonly IHttpListenerRequestHandler handler;
        volatile bool isStopping;

        public HttpListenerAcceptor(IPEndPoint bindTo, IHttpListenerRequestHandler handler)
            : this(bindTo, new Uri("/", UriKind.Relative), handler)
        { }

        public HttpListenerAcceptor(IPEndPoint bindTo, Uri uri, IHttpListenerRequestHandler handler)
        {
            listener.Prefixes.Add(EnsureTrailingSlash(FormatPrefix(bindTo, uri)));
            contexts = new HttpListenerAcceptorContext[1];
            this.handler = handler;
            for (int i = 0; i != contexts.Length; ++i)
                contexts[i] = new HttpListenerAcceptorContext(this, i);
        }

        public string Prefix { get { return listener.Prefixes.First(); } }

        public void Start() {
            listener.Start();
            backlog = new WaitHandle[contexts.Length];
            for(int i = 0; i != contexts.Length; ++i)
                BeginGetContext(contexts[i]);
        }

        public void Stop() {
            isStopping = true;
            listener.Stop();
            if(backlog != null)
                WaitHandle.WaitAll(backlog);
        }

        static string FormatPrefix(IPEndPoint bindTo, Uri uri) {
            if(uri.IsAbsoluteUri)
                return new UriBuilder(uri) { Port = bindTo.Port }.ToString();           
            return new UriBuilder("http", "*", bindTo.Port){ Path = uri.OriginalString }.ToString();  
        }

        static string EnsureTrailingSlash(string prefix) {
            if(prefix.EndsWith("/"))
                return prefix;
            return prefix + "/";
        }

        static string HostFrom(IPAddress address)
        {
            if (address == IPAddress.Any)
                return "*";
            return address.ToString();
        }

        void Dispatch(HttpListenerAcceptorContext context, IAsyncResult asyncResult)
        {
            if(BeginGetContext(context))
                handler.Process(listener.EndGetContext(asyncResult));
        }

        bool BeginGetContext(HttpListenerAcceptorContext context)
        {
            if(isStopping)
                return false;
            backlog[context.Offset] = listener.BeginGetContext(BeginRequest, context).AsyncWaitHandle;
            return true;
        }

        static void BeginRequest(IAsyncResult async)
        {
            var acceptor = (HttpListenerAcceptorContext)async.AsyncState;
            acceptor.BeginDispatch(async);
        }
    }
}