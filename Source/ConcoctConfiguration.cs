using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System;

namespace Concoct
{
    [Serializable]
    public class ConcoctConfiguration
    {
        const string OptionPrefix = "--";

        string workingDirectory;

        public ConcoctConfiguration() {
            Port = 80;
            Host = IPAddress.Any;
        }

        public static ConcoctConfiguration Parse(string[] args) {
            var configuration = new ConcoctConfiguration();
            var r = new Regex(string.Format("^{0}(?<name>.+)=(?<value>.+$)", OptionPrefix));
            var items = new List<string>();

            for(int i = 0; i != args.Length; ++i) {
                var item = args[i];
                if(item.StartsWith(OptionPrefix)) {
                    var m = r.Match(item);
                    if(m.Success) {
                        var value = m.Groups["value"].Value;
                        switch(m.Groups["name"].Value) {
                            case "port": configuration.Port = int.Parse(value); break;
                            case "path": configuration.WorkingDirectory = value; break;
                        }
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
        public string LogFile = "Concoct.log";

        public IPEndPoint GetEndPoint() { return new IPEndPoint(Host, Port); }
    }
}
