using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Xlnt.Stuff;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ServiceProcess;

namespace Concoct
{
    public class ConfigurationErrorException : Exception { }

    public class ConcoctApplication 
    {        
        const string OptionPrefix = "--";
        readonly ConcoctConfiguration config;
        MvcHost host;

        public ConcoctApplication(ConcoctConfiguration config) {
            this.config = config;
        }

        public void OnStart(string[] args) {
            try {
                var site = Assembly.LoadFrom(config.ApplicationAssemblyPath);
                var types = site.GetTypes();
                var httpApplicationType = types.Where(x => x.IsTypeOf<HttpApplication>()).First();
                host = MvcHost.Create(config.GetEndPoint(), config.VirtualDirectoryOrPrefix, Environment.CurrentDirectory, httpApplicationType);

                host.Start();
                Console.WriteLine("Listening for connections.");

            } catch(ReflectionTypeLoadException loadError) {
                Console.Error.WriteLine("Error applications.");
                foreach(var item in loadError.LoaderExceptions) {
                    Console.Error.WriteLine(item);
                }
                throw new ApplicationException("Failed to load application", loadError);
            }
        }

        public void OnStop() {
            host.Stop();
        }

        public static ConcoctConfiguration ParseConfiguration(string[] args) {
            var configuration = new ConcoctConfiguration();
            var r = new Regex(string.Format("^{0}(?<name>.+)=(?<value>.+$)", OptionPrefix));
            var items = new List<string>();

            for(int i = 0; i != args.Length; ++i) {
                var item = args[i];
                if(item.StartsWith(OptionPrefix)) {
                    var m = r.Match(item);
                    if(m.Success)
                        switch(m.Groups["name"].Value) {
                            case "port": configuration.Port = Int32.Parse(m.Groups["value"].Value); break;
                        }
                }
                else
                    items.Add(item);
            }

            if(items.Count != 2)
                throw new ConfigurationErrorException();

            configuration.ApplicationAssemblyPath = items[0];
            configuration.VirtualDirectoryOrPrefix = items[1];
            return configuration;
        }
    }

    public class Program : ServiceBase
    {
        readonly TextWriter log;
        readonly ConcoctApplication application;

        public Program(TextWriter log, ConcoctConfiguration config) {
            application = new ConcoctApplication(config);
            this.log = log;
        }

        static int Main(string[] args) {
            var config = ConcoctApplication.ParseConfiguration(args);
            if(Environment.UserInteractive)
                return new Program(Console.Out, config).RunInteractive();
            else {
                using(var log = new StreamWriter(File.OpenWrite("C:\\Concoct.log")))
                return new Program(log, config).RunService();
            }
        }

        int RunInteractive() {
            try {
                OnStart(new string[0]);

                Console.WriteLine("<press any key to quit>");
                Console.ReadKey();
                OnStop();
            }
            catch(ApplicationException) {
                return -1;
            }
            catch(ConfigurationErrorException) {
                Console.Error.WriteLine("Usage is {0} <assembly> <virtual-directory>", Path.GetFileName(typeof(Program).Assembly.Location));
            }
            return 0; 

        }

        int RunService() {          
            ServiceBase.Run(this);
            return 0;
        }

        protected override void OnStart(string[] args) {
            application.OnStart(args);
        }

        protected override void OnStop() {
            application.OnStop();
        }
    }
}