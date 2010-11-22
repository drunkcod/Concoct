using System.Net;

namespace Concoct
{
    public class ConcoctConfiguration
    {
        public ConcoctConfiguration() {
            Port = 80;
            Host = IPAddress.Any;
        }

        public string ApplicationAssemblyPath;
        public int Port;
        public IPAddress Host;
        public string VirtualDirectoryOrPrefix;

        public IPEndPoint GetEndPoint() { return new IPEndPoint(Host, Port); }
    }
}
