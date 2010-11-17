using System.Net;

namespace Concoct
{
    public interface IHttpListenerRequestHandler
    {
        void Process(HttpListenerContext context);
    }
}
