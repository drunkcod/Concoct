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
            ErrorFormatter = new BasicInternalErrorFormatter();
        }

        public IInternalServerErrorFormatter ErrorFormatter { get; set; }

        public void Process(HttpListenerContext context) {
            var httpContext = new HttpListenerContextAdapter(context, virtualPath, physicalPath);
            try {
                var data = GetRouteData(httpContext);
                var request = new RequestContext(httpContext, data);
                var handler = data.RouteHandler.GetHttpHandler(request);
                httpContext.AsHttpContext(handler.ProcessRequest);
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
