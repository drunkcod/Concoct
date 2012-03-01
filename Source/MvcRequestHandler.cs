using System;
using System.Net;
using System.Web;
using System.Web.Routing;
using Concoct.Web;

namespace Concoct
{
    static internal class EventHandlerExtensions
    {
        public static void Raise<T>(this EventHandler<T> self, object sender, T arg) where T : EventArgs {
            if(self != null)
                self(sender, arg);
        }
    }

    public class MvcRequestHandlerEventArgs : EventArgs 
    {
        readonly HttpListenerContextAdapter context;

        internal MvcRequestHandlerEventArgs(HttpListenerContextAdapter context) {
            this.context = context;
        }

        public HttpRequestBase Request { get { return context.Request; } }
    }

    public class MvcRequestHandler : IHttpListenerRequestHandler
    {
        readonly string virtualPath;
        readonly string physicalPath;

        public MvcRequestHandler(string virtualPath, string physicalPath) {
            this.virtualPath = virtualPath;
            this.physicalPath = physicalPath;
            ErrorFormatter = new BasicInternalErrorFormatter();
        }

        public IInternalServerErrorFormatter ErrorFormatter { get; set; }

        public event EventHandler<MvcRequestHandlerEventArgs> BeginRequest;

        public void Process(HttpListenerContext context) {
            var httpContext = new HttpListenerContextAdapter(context, virtualPath, physicalPath);
            try {
                var data = GetRouteData(httpContext);
                var request = new RequestContext(httpContext, data);
                var handler = data.RouteHandler.GetHttpHandler(request);
                BeginRequest.Raise(this, new MvcRequestHandlerEventArgs(httpContext));
                httpContext.AsHttpContext(handler.ProcessRequest, true);
            } catch (Exception e) {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Write(ErrorFormatter.Format(e));
            }
            httpContext.Response.End();
        }

        public RouteData GetRouteData(HttpContextBase httpContext) {
            return RouteTable.Routes.GetRouteData(httpContext);
        }
    }
}
