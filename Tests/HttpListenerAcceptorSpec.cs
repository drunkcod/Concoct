using System;
using System.IO;
using System.Net;
using Concoct.Web;
using Cone;

namespace Concoct
{
    class NullHttpListenerRequestHandler : IHttpListenerRequestHandler 
    {
        public void Process(HttpListenerContext context) { }
    }

    [Describe(typeof(HttpListenerAcceptorSpec))]
    public class HttpListenerAcceptorSpec
    {
        public void with_absolute_path() {
            var listener = new HttpListenerAcceptor(new IPEndPoint(IPAddress.Any, 80), new Uri("http://example.com"), new NullHttpListenerRequestHandler());
            Verify.That(() => listener.Prefix == "http://example.com:80/");
        }
        
        public void with_relative_path() {
            var listener = new HttpListenerAcceptor(new IPEndPoint(IPAddress.Any, 80), new Uri("/Foo", UriKind.Relative), new NullHttpListenerRequestHandler());
            Verify.That(() => listener.Prefix == "http://*:80/Foo/");
        }

        [Context("trippin'")]
        public class TestTest 
        {
            class MessageRequestHandler : IHttpListenerRequestHandler 
            {
                readonly string message;

                public MessageRequestHandler(string message){
                    this.message = message;
                }

                public void Process(HttpListenerContext context) {
                    context.Response.SendChunked = false;
                    var adapter = new HttpListenerContextAdapter(context, "/", ".");
                    var output = adapter.Response.Output;
                    output.Write(message);
                    adapter.Response.End();
                }
            }

            public void send_message() {
                var message = "Hello World!";
                var listener = new HttpListenerAcceptor(new IPEndPoint(IPAddress.Any, 8080), new Uri("/", UriKind.Relative), new MessageRequestHandler(message));
                try {
                    listener.Start();
                    var request = WebRequest.Create("http://localhost:8080/");
                    var response = request.GetResponse();
                    Verify.That(() => response.ContentLength == message.Length);
                    using(var reader = new StreamReader(response.GetResponseStream()))
                        Verify.That(() => reader.ReadToEnd() == message);
                } finally {
                    listener.Stop();
                }
            }
        }
    }
}
