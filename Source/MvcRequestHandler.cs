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
            var listenerContext = new HttpListenerContextAdapter(context, virtualPath, physicalPath);
            try {
                BeginRequest.Raise(this, new MvcRequestHandlerEventArgs(listenerContext));
                listenerContext.AsActiveHttpContext(httpContext => {
					var data = GetRouteData(listenerContext);
					var request = new RequestContext(listenerContext, data);
					var handler = data.RouteHandler.GetHttpHandler(request);
					handler.ProcessRequest(httpContext);
				});
            } catch (Exception e) {
                listenerContext.Response.StatusCode = 500;
                listenerContext.Response.Write(ErrorFormatter.Format(e));
            }
            listenerContext.Response.End();
        }

        public RouteData GetRouteData(HttpContextBase httpContext) {
            return RouteTable.Routes.GetRouteData(httpContext);
        }
    }
}
