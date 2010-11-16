using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone;
using System.Net;

namespace Concoct
{
    class NullHttpListenerRequestHandler : IHttpListenerRequestHandler 
    {
        public void Process(HttpListenerContext context) { }
    }

    [Describe(typeof(HttpListenerAcceptorSpec))]
    public class HttpListenerAcceptorSpec
    {
        public void with_fully_qualified_path() {
            var listener = new HttpListenerAcceptor(new IPEndPoint(IPAddress.Any, 80), "http://example.com", new NullHttpListenerRequestHandler());
            Verify.That(() => listener.Prefix == "http://example.com:80/");
        }
        
        public void with_relative_path() {
            var listener = new HttpListenerAcceptor(new IPEndPoint(IPAddress.Any, 80), "/", new NullHttpListenerRequestHandler());
            Verify.That(() => listener.Prefix == "http://*:80/");
        }
    }
}
