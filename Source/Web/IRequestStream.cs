using System.IO;

namespace Concoct.Web
{
    public interface IRequestStream 
    {
        string ContentType { get; }
        long ContentLength64 { get; }
        Stream InputStream { get; }
    }
}
