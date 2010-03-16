using System;
using System.Net;
using System.Web.Routing;
using Concoct.Web;

namespace Concoct
{
    class MvcRequestHandler : IRequestHandler
    {
        public void Process(HttpListenerContext context)
        {
            var httpContext = new HttpListenerContextAdapter(context);
            var data = RouteTable.Routes.GetRouteData(httpContext);
            var request = new RequestContext(httpContext, data);
            var handler = data.RouteHandler.GetHttpHandler(request);
            try
            {
                handler.ProcessRequest(httpContext.AsHttpContext());
            }
            catch (Exception e)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Write(e.ToString());
            }
            httpContext.Response.End();
        }
    }
}
