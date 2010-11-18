using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Xlnt.Stuff;

namespace Concoct
{
    public class ConfigurationErrorException : Exception
    { }

    public class Program
    {
        static int Main(string[] args) {
            try {

                var configuration = ParseConfiguration(args);
            
                var site = Assembly.LoadFrom(configuration.ApplicationAssemblyPath);
                var types = site.GetTypes();
                var httpApplicationType = types.Where(x => x.IsTypeOf<HttpApplication>()).First();
                var host = MvcHost.Create(configuration.GetEndPoint(), configuration.VirtualDirectoryOrPrefix, Environment.CurrentDirectory, httpApplicationType);

                host.Start();
                Console.WriteLine("Listening for connections.");
                Console.WriteLine("<press any key to quit>");
                Console.ReadKey();
                host.Stop();

                return 0;
            } catch(ConfigurationErrorException e) {
                Console.Error.WriteLine("Usage is {0} <assembly> <virtual-directory>", Path.GetFileName(typeof(Program).Assembly.Location));
                return -1;
            }
        }

        static ConcoctConfiguration ParseConfiguration(string[] args) {
            if(args.Length != 2)
                throw new ConfigurationErrorException();
            return new ConcoctConfiguration {
                ApplicationAssemblyPath = args[0],
                VirtualDirectoryOrPrefix = args[1]
            };
        }
    }
}