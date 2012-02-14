using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Concoct
{
    [Serializable]
    public class ConcoctConfiguration
    {
        const string OptionPrefix = "--";

        string workingDirectory;
		string configurationFile;

        public ConcoctConfiguration() {
            Port = 80;
            Host = IPAddress.Any;
			LogFile = "Concoct.log";
        }

        public static ConcoctConfiguration Parse(string[] args) {
            var configuration = new ConcoctConfiguration();
            var r = new Regex(string.Format("^{0}(?<name>.+)=(?<value>.+$)", OptionPrefix));
            var items = new List<string>();

            for(int i = 0; i != args.Length; ++i) {
                var item = args[i];
                var m = r.Match(item);
                if(m.Success) {
                    var value = m.Groups["value"].Value;
                    switch(m.Groups["name"].Value) {
                        case "port": configuration.Port = int.Parse(value); break;
                        case "path": configuration.WorkingDirectory = value; break;
						case "log" : configuration.LogFile = value; break;
						case "config" : configuration.configurationFile = value; break;
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

        public string ApplicationAssemblyPath;
        public int Port;
        public IPAddress Host;
        public string VirtualDirectoryOrPrefix;
        public string WorkingDirectory 
        {
            get { return string.IsNullOrEmpty(workingDirectory) ? Path.GetDirectoryName(Path.GetFullPath(ApplicationAssemblyPath)) : workingDirectory; }
            set { workingDirectory = value; }
        }
		public string PrivateBinPath { get { return Path.Combine(WorkingDirectory, "Bin"); } }
		public string ConfigurationFile { 
			get { 
				if(string.IsNullOrEmpty(configurationFile))
					return AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
				return configurationFile;
			} 
		}
        public string LogFile;

        public IPEndPoint GetEndPoint() { return new IPEndPoint(Host, Port); }
    }
}
