using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone;

namespace Concoct.Web
{
    [Describe(typeof(MimePart))]
    public class MimePartSpec
    {
        public void headers_are_case_insensitive() {
            var part = new MimePart(new ArraySegment<byte>()) {
                {"Content-Type", "text/plain" }
            };

            Verify.That(() => part["Content-Type"] == part["content-type"]);
        }
    }
}
