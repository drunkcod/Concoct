using System.Web.Mvc;
using System.Web.Routing;
using Concoct.Demo.Controllers;
using Spark.Web.Mvc;
using Xlnt.Web.Mvc;

namespace Concoct.Demo
{
    public class ConcoctDemoApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Demo", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start() {
            
            var controllerFactory = new BasicControllerFactory();
            controllerFactory.Register(typeof(DemoController).Assembly.GetTypes());
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            ViewEngines.Engines.Clear();
            SparkEngineStarter.RegisterViewEngine(ViewEngines.Engines);

            RegisterRoutes(RouteTable.Routes);
        }
    }
}