using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concoct.Samples.ServiceDashboard.Controllers
{
    public class ServicesController : Controller
    {
        public ActionResult Index() {
            return Content("<?xml version=\"1.0\"?><services><service id=\"Foo\"><link rel=\"self\" href=\"/\"/></service></services>");
        }

    }
}
