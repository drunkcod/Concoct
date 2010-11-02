using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Xlnt.Stuff;
using Xlnt.Web.Mvc;
using System.Reflection;
using System.IO;

namespace Concoct
{
    public interface IHttpListenerRequestHandler
    {
        void Process(HttpListenerContext context);
    }

    public class Program
    {
        static int Main(string[] args) {
            if(args.Length != 2) {
                Console.Error.WriteLine("Usage is {0} <assembly> <virtual-directory>", Path.GetFileName(typeof(Program).Assembly.Location));
                return -1;
            }
            var site = Assembly.LoadFrom(args[0]);
            var types = site.GetTypes();
            var httpApplicationType = types.Where(x => x.IsTypeOf<HttpApplication>()).First();
            var host = MvcHost.Create(new IPEndPoint(IPAddress.Any, 80), args[1], httpApplicationType);

            host.Start();
            Console.WriteLine("Listening for connections.");
            Console.ReadKey();
            host.Stop();

            return 0;
        }
    }
}