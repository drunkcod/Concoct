using System;
using System.Net;
using System.Threading;

namespace Concoct
{
    class HttpListenerAcceptor
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
        readonly IRequestHandler handler;

        public HttpListenerAcceptor(IPEndPoint bindTo, IRequestHandler handler)
            : this(bindTo, string.Empty, handler)
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

        static string PrefixFrom(IPAddress address)
        {
            if (address == IPAddress.Any)
                return "*";
            return address.ToString();
        }

        public void Start()
        {
            listener.Start();
            for (int i = 0; i != contexts.Length; ++i)
                BeginGetContext(contexts[i]);
        }

        public void Stop()
        {
            listener.Stop();
            WaitHandle.WaitAll(backlog);
        }

        void Dispatch(HttpListenerAcceptorContext context, IAsyncResult asyncResult)
        {
            BeginGetContext(context);
            handler.Process(listener.EndGetContext(asyncResult));
        }

        void BeginGetContext(HttpListenerAcceptorContext context)
        {
            backlog[context.Offset] = listener.BeginGetContext(BeginRequest, context).AsyncWaitHandle;
        }

        static void BeginRequest(IAsyncResult async)
        {
            var acceptor = (HttpListenerAcceptorContext)async.AsyncState;
            acceptor.BeginDispatch(async);
        }
    }
}