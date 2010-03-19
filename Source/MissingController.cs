using System.Web.Mvc;
using System.Web.Routing;

namespace Concoct
{
    public class MissingController : IController
    {
        public void Execute(RequestContext requestContext) {
            requestContext.HttpContext.Response.StatusCode = 404;
        }
    }
}