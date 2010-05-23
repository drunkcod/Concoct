using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Concoct.Samples.ServiceDashboard.Models;

namespace Concoct.Samples.ServiceDashboard.Controllers
{
    public class ServicesController : Controller
    {
        public ActionResult Index() {
            var result = new StringBuilder("<?xml version=\"1.0\"?>");
            result.Append("<services>");
            Singleton<ServiceContainer>.Instance.Services.ForEach(x => {
                result.AppendFormat("<service id='{0}'><link rel='self' href='/{0}'/></service>", x.Id);
            });
            result.Append("</services>");
            return Content(result.ToString());
        }

    }
}
