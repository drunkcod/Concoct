using System;
using System.Net;
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
                var data = RouteTable.Routes.GetRouteData(httpContext);
                var request = new RequestContext(httpContext, data);
                var handler = data.RouteHandler.GetHttpHandler(request);
                handler.ProcessRequest(httpContext.AsHttpContext());
            } catch (Exception e) {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Write(e.ToString());
            }
            httpContext.Response.End();
        }
    }
}
