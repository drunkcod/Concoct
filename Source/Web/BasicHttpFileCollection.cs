using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Concoct.Web
{
    public class BasicHttpPostedFile : HttpPostedFileBase
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

        public override void SaveAs(string filename) {
            File.WriteAllBytes(filename, bytes);
        }
    }

    public class BasicHttpFileCollection : HttpFileCollectionBase
    {
        readonly List<string> allKeys = new List<string>();
        readonly List<HttpPostedFileBase> files = new List<HttpPostedFileBase>();
        
        public void Add(string key, HttpPostedFileBase file) {
            files.Add(file);
            allKeys.Add(key);
        }

        public override string[] AllKeys {
            get { return allKeys.ToArray(); }
        }

        public override int Count 
            { get { return files.Count; } 
        }
        
        public override System.Collections.IEnumerator GetEnumerator() { 
            return allKeys.GetEnumerator(); 
        }

        public override HttpPostedFileBase this[int index] {
            get { return files[index]; }
        }

        public override HttpPostedFileBase this[string name] {
            get { 
                var index = allKeys.IndexOf(name);
                if(index == -1)
                    return null;
                return this[index]; 
            }
        }
    }
}
