using System.Collections.Generic;
using System.Web;

namespace Concoct.Web
{
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
