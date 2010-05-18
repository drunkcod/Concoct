using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concoct.Samples.ServiceDashboard.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() {
            return Content("Hello MVC World!");
        }
    }
}
