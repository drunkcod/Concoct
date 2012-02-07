using System;
using System.Net;
using Cone;
using Concoct.Web;
using Xlnt.Web.Mvc;
using System.Web.Mvc;
using System.Web.Routing;
using Concoct.Controllers;

namespace Concoct.Controllers 
{
	public class TestController : Controller
	{
		public string Index() { return "Hello World!"; }

		public string Xml() {
			Response.ContentType = "text/xml";
			return "<message>Hello World!</message>";
		}
	}

}

namespace Concoct
{    
	public class TestApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Test", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start() {           
            var controllerFactory = new BasicControllerFactory();
            controllerFactory.Register(new[]{ typeof(TestController) });
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
            ViewEngines.Engines.Clear();

			RouteTable.Routes.Clear();
            RegisterRoutes(RouteTable.Routes);
        }
    }

	[Describe(typeof(MvcRequestHandler))]
	public class MvcRequestHandlerSpec
	{
		public void handles_changed_ContentType() {
			WithResponseFrom("/Test/Xml", response => 
				Verify.That(() => response.ContentType == "text/xml")
			);
		}

		void WithResponseFrom(string url, Action<WebResponse> withResponse) {
			var host = MvcHost.Create(new IPEndPoint(IPAddress.Any, 8080), "/", Environment.CurrentDirectory, typeof(TestApplication));
            try {
                host.Start();
                var request = WebRequest.Create("http://localhost:8080" + url);
                using(var response = request.GetResponse())
                    withResponse(response);
            } finally {
                host.Stop();
            }
		}
	}
}
