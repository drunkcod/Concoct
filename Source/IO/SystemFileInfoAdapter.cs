using System.IO;

namespace Concoct.IO
{
    class SystemFileInfo : IFileInfo
    {
        readonly FileInfo info;

        public SystemFileInfo(string fileName) {
            this.info = new FileInfo(fileName);
        }
         
        public string Extension { get { return info.Extension; } }

        public Stream OpenRead() { return info.OpenRead(); }
    }
}
