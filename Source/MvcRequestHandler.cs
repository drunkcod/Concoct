using System;
using System.Net;
using System.Web;
using System.Web.Routing;
using Concoct.Web;

namespace Concoct
{
    public class MvcRequestHandler : IHttpListenerRequestHandler
    {
        readonly string virtualPath;
        readonly string physicalPath;

        public MvcRequestHandler(string virtualPath, string physicalPath) {
            this.virtualPath = virtualPath;
            this.physicalPath = physicalPath;
        }

        public void Process(HttpListenerContext context) {
            var httpContext = new HttpListenerContextAdapter(context, virtualPath, physicalPath);
            try {
                var request = new RequestContext(httpContext, GetRouteData(httpContext));
                var handler = data.RouteHandler.GetHttpHandler(request);
                handler.ProcessRequest(httpContext.AsHttpContext());
            } catch (Exception e) {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Write(e.ToString());
            }
            httpContext.Response.End();
        }

        public void GetRouteData(HttpContextBase httpContext) {
            return RouteTable.Routes.GetRouteData(httpContext);
        }
    }
}
