using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Concoct
{

    interface IRequestHandler
    {
        void Process(HttpListenerContext context);
    }

    class HttpListenerAcceptor
    {
        readonly HttpListener listener = new HttpListener();
        readonly HttpListenerAcceptorContext[] contexts = new HttpListenerAcceptorContext[1];
        readonly WaitHandle[] backlog = new WaitHandle[1];
        readonly IRequestHandler handler;

        public HttpListenerAcceptor(int port, IRequestHandler handler) {
            listener.Prefixes.Add(string.Format("http://*:{0}/", port));
            this.handler = handler;
            for(int i = 0; i != contexts.Length; ++i)
                contexts[i] = new HttpListenerAcceptorContext {
                    Listener = this,
                    Backlog = backlog,
                    Offset = i
                };
        }

        public void Start() {
            listener.Start();
            for(int i = 0; i != contexts.Length; ++i)
                contexts[i].Listen();
        }

        public void Stop() {
            listener.Stop();
            WaitHandle.WaitAll(backlog);
        }

        internal WaitHandle BeginGetContext(AsyncCallback callback, object state) {
            return listener.BeginGetContext(callback, state).AsyncWaitHandle;
        }

        internal void EndGetContext(IAsyncResult asyncResult) {
            handler.Process(listener.EndGetContext(asyncResult));
        }
    }

    class HttpListenerAcceptorContext
    {
        public HttpListenerAcceptor Listener;
        public WaitHandle[] Backlog;
        public int Offset;

        static void BeginRequest(IAsyncResult async)
        {
            var acceptor = (HttpListenerAcceptorContext)async.AsyncState;            
            acceptor.DispatchRequest(async);
        }

        public void Listen()
        {
            Backlog[Offset] = Listener.BeginGetContext(BeginRequest, this);
        }

        void DispatchRequest(IAsyncResult async)
        {
            Listen();
            Listener.EndGetContext(async);
        }
    }

    class ThreadPoolRequestHandler : IRequestHandler
    {
        readonly WaitCallback process;

        public ThreadPoolRequestHandler(WaitCallback process) {
            this.process = process;
        }

        void IRequestHandler.Process(HttpListenerContext context) {
            ThreadPool.QueueUserWorkItem(process, context);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var acceptor = new HttpListenerAcceptor(8080, new ThreadPoolRequestHandler(SendHello));
            acceptor.Start();
            Console.WriteLine("Waiting for connections.");


            Console.ReadKey();
            acceptor.Stop();
        }

        static void SendHello(object obj)
        {
            var context = (HttpListenerContext)obj;
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.WriteLine("Hello World {0}", Thread.CurrentThread.ManagedThreadId);
            }
            context.Response.Close();
        }
    }
}
