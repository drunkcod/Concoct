using System;
using System.Web;
using System.IO;

namespace Concoct.Web
{
    public class ConcoctHttpServerUtility : HttpServerUtilityBase
    {
        readonly string physicalPath;

        public ConcoctHttpServerUtility(string physicalPath) {
            this.physicalPath = physicalPath;
        }

        public override string MapPath(string path) {
            if(path.StartsWith("./"))
                return MapPath(path.Substring(1));
            return Path.GetFullPath(Path.Combine(physicalPath, path.TrimStart('/', '~')));
        }
    }
}
