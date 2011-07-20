using System;
using System.IO;
using System.Web;

namespace Concoct.Web
{
    public class BasicHttpPostedFile : HttpPostedFileBase
    {
        readonly string name;
        readonly string contentType;
        readonly ArraySegment<byte> bytes;

        public BasicHttpPostedFile(string name, string contentType, byte[] bytes) :
            this(name, contentType, new ArraySegment<byte>(bytes)) { }

        public BasicHttpPostedFile(string name, string contentType, ArraySegment<byte> bytes) {
            this.name = name;
            this.contentType = contentType;
            this.bytes = bytes;
        }

        public override int ContentLength {
            get { return bytes.Count; }
        }

        public override string ContentType {
            get {
                return contentType;
            }
        }

        public override string FileName {
            get { return name; }
        }

        public override void SaveAs(string filename) {
            using(var output = File.Create(filename))
                output.Write(bytes.Array, bytes.Offset, bytes.Count);
        }
    }
}
