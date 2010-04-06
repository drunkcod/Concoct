using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Xlnt.Stuff;
using Xlnt.Web.Mvc;
using System.Reflection;

namespace Concoct
{
    public interface IHttpListenerRequestHandler
    {
        void Process(HttpListenerContext context);
    }

    public interface IApplication
    {
        void Start();
    }

    public class Program
    {
        static void Main(string[] args) {
            var site = Assembly.LoadFrom(args[0]);

            var types = site.GetTypes();
            var httpApplicationType = types.Where(x => x.IsTypeOf<HttpApplication>()).First();
          
            var host = MvcHost.Create(new IPEndPoint(IPAddress.Any, 80), args[1], httpApplicationType);
            host.Starting += (_, e) => {
                var controllerFactory = new BasicControllerFactory();
                controllerFactory.Register(types);
                ControllerBuilder.Current.SetControllerFactory(controllerFactory);
            };
            host.Start();
            Console.WriteLine("Listening for connections.");
            Console.ReadKey();
            host.Stop();
        }
    }
}