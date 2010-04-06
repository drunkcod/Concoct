using System;
using System.Net;
using System.Threading;

namespace Concoct
{
    public class HttpListenerAcceptor
    {
        class HttpListenerAcceptorContext
        {
            public HttpListenerAcceptor Listener;
            public int Offset;

            public void BeginDispatch(IAsyncResult async)
            {
                Listener.Dispatch(this, async);
            }
        }

        readonly HttpListener listener = new HttpListener();
        readonly HttpListenerAcceptorContext[] contexts;
        readonly WaitHandle[] backlog;
        readonly IHttpListenerRequestHandler handler;
        volatile bool isStopping = false;

        public HttpListenerAcceptor(IPEndPoint bindTo, IHttpListenerRequestHandler handler)
            : this(bindTo, string.Empty, handler)
        { }

        public HttpListenerAcceptor(IPEndPoint bindTo, string virtualDirectory, IHttpListenerRequestHandler handler)
        {
            listener.Prefixes.Add(FormatPrefix(bindTo, virtualDirectory));
            contexts = new HttpListenerAcceptorContext[1];
            backlog = new WaitHandle[contexts.Length];
            this.handler = handler;
            for (int i = 0; i != contexts.Length; ++i)
                contexts[i] = new HttpListenerAcceptorContext {
                    Listener = this,
                    Offset = i
                };
        }

        public void Start() {
            listener.Start();
            for(int i = 0; i != contexts.Length; ++i)
                BeginGetContext(contexts[i]);
        }

        public void Stop() {
            isStopping = true;
            listener.Stop();
            WaitHandle.WaitAll(backlog);
        }

        static string FormatPrefix(IPEndPoint bindTo, string virtualDirectory)
        {
            return string.Format("http://{0}:{1}{2}", HostFrom(bindTo.Address), bindTo.Port, FormatVirtualDirectory(virtualDirectory));
        }

        static string FormatVirtualDirectory(string virtualDirectory) { return string.Format("/{0}/", virtualDirectory).Replace("//", "/"); }

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