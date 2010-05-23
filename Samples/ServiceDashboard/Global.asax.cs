using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Concoct.Samples.ServiceDashboard.Models;

namespace Concoct.Samples.ServiceDashboard
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801 
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.Clear();
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        public ICollection<ServiceEntry> Services { get { return Singleton<ServiceContainer>.Instance.Services; } }

        protected void Application_Start() {
            Services.Add(new ServiceEntry { Id = "Foo" });
            RegisterRoutes(RouteTable.Routes);
        }
    }
}