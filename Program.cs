using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Concoct
{
    class AcceptorContext
    {
        public const int Shutdown = -1;
        public HttpListener Listener;
        public WaitHandle[] Backlog;
        public int Offset;
        public Action<HttpListenerContext> OnRequest;

        static void BeginRequest(IAsyncResult async)
        {
            var acceptor = (AcceptorContext)async.AsyncState;            
            acceptor.DispatchRequest(async);
        }

        public void Listen()
        {
            Backlog[Offset] = Listener.BeginGetContext(BeginRequest, this).AsyncWaitHandle;
        }

        void DispatchRequest(IAsyncResult async)
        {
            if(Thread.VolatileRead(ref Offset) == Shutdown)
                return;
            Listen();
            OnRequest(Listener.EndGetContext(async));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://*:8080/");
            listener.Start();
            Console.WriteLine("Waiting for connections.");

            var workers = new Thread[2];


            var contexts = new AcceptorContext[4];
            var backlog = new WaitHandle[contexts.Length];

            ParameterizedThreadStart beginListening = x => {
                    var n = (int[])x;
                    for (var i = 0; i != n[0]; ++i){
                        var offset = n[1] + i;
                        var handler = new AcceptorContext
                        {
                            Listener = listener,
                            Backlog = backlog,
                            Offset = offset,
                            OnRequest = request => ThreadPool.QueueUserWorkItem(SendHello, request)
                        };
                        handler.Listen();
                        contexts[offset] = handler;
                    }
                };
            beginListening(new[] {contexts.Length / 2, 0});
            var worker = new Thread(beginListening);
            worker.Start(new[] { contexts.Length / 2, contexts.Length / 2 });

            Console.ReadKey();
            for (var i = 0; i != backlog.Length; ++i)
                contexts[i].Offset = AcceptorContext.Shutdown;
            listener.Stop();
            WaitHandle.WaitAll(backlog);
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
