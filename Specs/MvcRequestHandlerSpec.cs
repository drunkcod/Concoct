using System;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using Concoct.Controllers;
using Cone;
using Xlnt.Web.Mvc;
using Cone.Helpers;
using System.Reflection;

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
	public class MvcRequestHandlerSpec : HttpServiceFixture
	{
        Action<MvcHost> ConfigureHost;

        [BeforeEach]
        public void EstablishWorld() {
            ConfigureHost = _ => { };
        }

		public void handles_changed_ContentType() {
			WithResponseFrom("http://localhost:8080/Test/Xml", response => 
				Verify.That(() => response.ContentType == "text/xml")
			);
		}

        public void raises_BeginRequest() {
            var beginRequest = new EventSpy<MvcRequestHandlerEventArgs>();
            ConfigureHost = host => {
                host.RequestHandler.BeginRequest += beginRequest;
            };

            WithResponseFrom("http://localhost:8080", response => { });

            Verify.That(() => beginRequest.HasBeenCalled);
            Verify.Guard(() =>
                beginRequest.Then((s, e) => Verify.That(() => e.Request.Url.ToString() == "http://localhost:8080/"))
            );
        }

		protected override IServiceController CreateService() {
            var host = MvcHost.Create(new IPEndPoint(IPAddress.Any, 8080), "/", Environment.CurrentDirectory, typeof(TestApplication));
            ConfigureHost(host);
            return host;
		}
	}
}
