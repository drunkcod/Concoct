using System.Web;

namespace Concoct.Web
{
    class EmptyHttpFileCollection : HttpFileCollectionBase
    {
        public override int Count { get { return 0; } }
    }
}
