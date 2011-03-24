using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Web;
using Xlnt.Stuff;

namespace Concoct
{
    public class ConfigurationErrorException : Exception { }

    public class Program : ServiceBase
    {
        readonly TextWriter log;
        readonly ConcoctApplication application;

        public Program(TextWriter log, ConcoctConfiguration config) {
            application = new ConcoctApplication(config);
            this.log = log;
        }

        static int Main(string[] args) {
            var config = ConcoctConfiguration.Parse(args);
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