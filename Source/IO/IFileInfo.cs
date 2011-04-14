using System.IO;

namespace Concoct.IO
{
    public interface IFileInfo 
    {
        string Extension { get; }
        Stream OpenRead();
    }
}
