using System.Net;

namespace Concoct
{
    static class MvcHost
    {
        public static HttpListenerAcceptor Create(IPEndPoint bindTo, string virtualPath) {
            return new HttpListenerAcceptor(
                bindTo,
                virtualPath,
                new MvcRequestHandler(virtualPath));
        }
    }
}
