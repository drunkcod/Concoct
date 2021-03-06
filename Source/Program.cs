﻿using System;
using System.IO;
using System.ServiceProcess;

namespace Concoct
{
    public class ConfigurationErrorException : Exception { }

	public interface ILog 
	{
		void Info(string format, params object[] args);
		void Error(string format, params object[] args);
	}

    public class Program : ServiceBase, ILog
    {
        readonly TextWriter log;
        readonly ConcoctApplication application;

        public Program(TextWriter log, ConcoctConfiguration config) {
            application = new ConcoctApplication(config, this);
            this.log = log;
        }

        static int Main(string[] args) {
            try {
                var config = ConcoctConfiguration.Parse(args);
                if(Environment.UserInteractive) {
					return new Program(Console.Out, config).RunInteractive();
                } else {
                    var log = new StreamWriter(File.Open(config.LogFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write));
                    return new Program(log, config).RunService();
                }
            } catch(ConfigurationErrorException) {
                using(var usage = typeof(Program).Assembly.GetManifestResourceStream("Concoct.Usage.txt")) {
                    var reader = new StreamReader(usage);;
                    Console.Error.WriteLine(reader.ReadToEnd());
                }
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
            return 0;
        }

        int RunService() {          
            ServiceBase.Run(this);
            return 0;
        }

        protected override void OnStart(string[] args) {
			Info("Started");
            application.Start();
        }

        protected override void OnStop() {
			Info("Stopped");
            application.Stop();
        }

		public void Info(string format, params object[] args) {
			log.WriteLine(string.Format("[{0}] {1}", DateTime.Now, format), args);
		}

		public void Error(string format, params object[] args) {
			log.WriteLine(string.Format("[{0}] ERROR: {1}", DateTime.Now, format), args);
		}
    }
}