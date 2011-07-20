using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Concoct.Web
{
    public class MimePart : IEnumerable
    {
        public readonly NameValueCollection Headers = new NameValueCollection();
        public readonly ArraySegment<byte> Body;

        public MimePart(ArraySegment<byte> body) {
            this.Body = body;
        }

        public void Add(string headerName, string value) {
            Headers.Add(headerName, value);
        }

        public string GetBodyText(Encoding encoding) {
            return encoding.GetString(Body.Array, Body.Offset, Body.Count);
        }

        public string this[string headerName] { get { return Headers[headerName]; } }

        IEnumerator IEnumerable.GetEnumerator() { return Headers.GetEnumerator(); }
    }
}
