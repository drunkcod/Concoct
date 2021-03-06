﻿using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Concoct.Demo.Models;
using System.Configuration;

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

		public string Message() {
			return ConfigurationManager.AppSettings["message"];
		}

		public string Xml() 
		{
			Response.ContentType = "text/xml";
			return "<message>Hello Xml World</message>";
		}

        IEnumerable<HttpPostedFileBase> PostedFiles {
            get {
                foreach(string name in Request.Files)
                    yield return Request.Files[name];
            }
        }
    }
}
