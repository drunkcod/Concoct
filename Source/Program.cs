using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using Xlnt.Stuff;

namespace Concoct
{
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
            var host = MvcHost.Create(new IPEndPoint(IPAddress.Any, 80), args[1], Environment.CurrentDirectory, httpApplicationType);

            host.Start();
            Console.WriteLine("Listening for connections.");
            Console.WriteLine("<press any key to quit>");
            Console.ReadKey();
            host.Stop();

            return 0;
        }
    }
}