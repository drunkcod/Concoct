using System;
using System.Net;
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
    }
}
