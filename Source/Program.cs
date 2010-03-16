using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Web;
using System.Web.Mvc;

namespace Concoct
{
    interface IRequestHandler
    {
        void Process(HttpListenerContext context);
    }

    public interface IApplication
    {
        void Start();
    }

    public class Program
    {

        static MethodInfo Method<T>(Expression<Action<T>> expression) { return (expression.Body as MethodCallExpression).Method; }

        static void Main(string[] args)
        {
            var host = new Program();
            var site = Assembly.LoadFrom(args[0]);

            var types = site.GetTypes();

            var httpApplicationType = types.Where(x => typeof (HttpApplication).IsAssignableFrom(x)).First();

            IApplication application = CreateApplicationProxyFromHttpApplication(httpApplicationType);
            application.Start();
            var controllerFactory = new BasicControllerFactory();
            controllerFactory.Register(types);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
            host.Start(args[1]);
        }

        private static IApplication CreateApplicationProxyFromHttpApplication(Type httpApplicationType)
        {
            var generated = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Concoct.Generated"), AssemblyBuilderAccess.Run);
            var module = generated.DefineDynamicModule("Main");
            var proxy = ApplicationBuilder.CreateIn(module, httpApplicationType);
            proxy.DynamicEventWireUp(Method<IApplication>(x => x.Start()), "Application_Start");
            return proxy.CreateType();
        }

        void Start(string virtualDirectory)
        {
            var acceptor = new HttpListenerAcceptor(
                new IPEndPoint(IPAddress.Any, 80),
                virtualDirectory,
                new MvcRequestHandler());
            acceptor.Start();
            Console.WriteLine("Waiting for connections.");
            Console.ReadKey();
            acceptor.Stop();
        }
    }
}