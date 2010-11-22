using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Xlnt.Stuff;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
            } catch(ConfigurationErrorException) {
                Console.Error.WriteLine("Usage is {0} <assembly> <virtual-directory>", Path.GetFileName(typeof(Program).Assembly.Location));
                return -1;
            }
        }

        const string OptionPrefix = "--";

        public static ConcoctConfiguration ParseConfiguration(string[] args) {
            var options = new List<string>();            
            var items = new List<string>();

            for(int i = 0; i != args.Length; ++i)
                if(args[i].StartsWith(OptionPrefix))
                    options.Add(args[i]);
                else
                    items.Add(args[i]);

            if(items.Count != 2)
                throw new ConfigurationErrorException();

            var configuration = new ConcoctConfiguration {
                ApplicationAssemblyPath = items[0],
                VirtualDirectoryOrPrefix = items[1]
            };

            var r = new Regex("^--(?<name>.+)=(?<value>.+$)");
            foreach(var item in options) {
                var m = r.Match(item);
                if(m.Success)
                    switch(m.Groups["name"].Value) {
                        case "port": configuration.Port = Int32.Parse(m.Groups["value"].Value); break; 
                    }
            }
                
            return configuration;
        }
    }
}