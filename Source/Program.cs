using System;
using System.IO;
using System.ServiceProcess;

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
            try {
                var config = ConcoctConfiguration.Parse(args);
                if(Environment.UserInteractive)
                    return new Program(Console.Out, config).RunInteractive();
                else {
                    var log = new StreamWriter(File.OpenWrite(config.LogFile));
                    return new Program(log, config).RunService();
                }
            } catch(ConfigurationErrorException configurationError) {
                Console.Error.WriteLine(configurationError);
                Console.ReadKey();
                return -1;
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