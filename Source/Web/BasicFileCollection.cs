using System.Collections.Generic;
using System.Web;

namespace Concoct.Web
{
    class BasicHttpPostedFile : HttpPostedFileBase
    {
        readonly string name;
        readonly string contentType;
        readonly byte[] bytes;

        public BasicHttpPostedFile(string name, string contentType, byte[] bytes) {
            this.name = name;
            this.contentType = contentType;
            this.bytes = bytes;
        }

        public override int ContentLength {
            get { return bytes.Length; }
        }

        public override string ContentType {
            get {
                return contentType;
            }
        }

        public override string FileName {
            get { return name; }
        }
    }

    class BasicHttpFileCollection : HttpFileCollectionBase
    {
        readonly List<HttpPostedFileBase> files = new List<HttpPostedFileBase>();
        public void Add(HttpPostedFileBase file) {
            files.Add(file);
        }

        public override int Count { get { return files.Count; } }
        public override System.Collections.IEnumerator GetEnumerator() { return files.GetEnumerator(); }

        public override HttpPostedFileBase this[int index] {
            get { return files[index]; }
        }
    }
}
