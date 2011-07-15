using System.Collections.Generic;
using System.Web;

namespace Concoct.Web
{
    class BasicHttpFileCollection : HttpFileCollectionBase
    {
        readonly List<HttpPostedFileBase> files = new List<HttpPostedFileBase>();
        public void Add(HttpPostedFileBase file) {
            files.Add(file);
        }

        public override int Count { get { return files.Count; } }
        public override System.Collections.IEnumerator GetEnumerator() { return files.GetEnumerator(); }
    }
}
