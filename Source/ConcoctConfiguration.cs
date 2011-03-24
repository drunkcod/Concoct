using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Concoct
{
    public class ConcoctConfiguration
    {
        const string OptionPrefix = "--";

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
                    if(m.Success)
                        switch(m.Groups["name"].Value) {
                            case "port": configuration.Port = int.Parse(m.Groups["value"].Value); break;
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

        public IPEndPoint GetEndPoint() { return new IPEndPoint(Host, Port); }
    }
}
