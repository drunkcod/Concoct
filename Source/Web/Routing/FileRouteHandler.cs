using System;
using System.IO;
using System.Web;
using System.Web.Routing;
using Concoct.IO;

namespace Concoct.Web.Routing
{
    public class FileRouteHandler : IRouteHandler
    {
        internal static Func<string, string> MapPath = path => HttpContext.Current.Server.MapPath(path);

        readonly string root;

        public FileRouteHandler(string root) {
            this.root = MapPath(root);
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext) {
            var file = requestContext.RouteData.Values["file"].ToString();
            return new FileHttpHandler(new SystemFileInfo(Path.Combine(root, file)));
        }
    }
}
