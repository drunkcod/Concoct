using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Concoct.Demo.Models;

namespace Concoct.Demo.Controllers
{
    public class DemoController : Controller
    {
        public ActionResult Index()
        {
            return View(new DemoModel {
                Files = PostedFiles
            });
        }

        IEnumerable<HttpPostedFileBase> PostedFiles {
            get {
                foreach(string name in Request.Files)
                    yield return Request.Files[name];
            }
        }
    }
}
