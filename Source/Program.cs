using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Web;
using System.Web.Mvc;
using Xlnt.Stuff;
using Xlnt.Web.Mvc;

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

            var application = CreateApplicationProxy(httpApplicationType);
            application.Start();
            
            var controllerFactory = new BasicControllerFactory();
            controllerFactory.Register(types);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            var host = MvcHost.Create(new IPEndPoint(IPAddress.Any, 80), args[1]);
            host.Start();
            Console.WriteLine("Listening for connections.");
            Console.ReadKey();
            host.Stop();
        }

        private static IApplication CreateApplicationProxy(Type httpApplicationType) {
            var generated = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Concoct.Generated"), AssemblyBuilderAccess.Run);
            var module = generated.DefineDynamicModule("Main");
            var proxy = ApplicationBuilder.CreateIn(module, httpApplicationType);
            proxy.DynamicEventWireUp(x => x.Start(), "Application_Start");
            return proxy.CreateType();
        }
    }
}