using System.IO;

namespace Concoct.Web
{
    public interface IRequestStream 
    {
        string ContentType { get; }
        int ContentLength { get; }
        Stream InputStream { get; }
    }
}
