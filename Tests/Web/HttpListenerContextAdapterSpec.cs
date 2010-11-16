using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Cone;

namespace Concoct.Web
{
    [Describe(typeof(HttpListenerContextAdapter))]
    public class HttpListenerContextAdapterSpec
    {
        [Row("http://example.com/foo", "~/"),
         Row("http://example.com/foo/", "~/")]
        public void MakeRelativeUriFunc(string request, string expected) {
            var baseUrl = new Uri("http://example.com");
            var makeRelative = HttpListenerContextAdapter.MakeRelativeUriFunc(baseUrl, "/foo");
            
            
            Verify.That(() => makeRelative(new Uri(request)) == expected);
        }

        public void handles_virtual_directory_missing_initial_slash()
        {
            var baseUrl = new Uri("http://example.com/");
            var makeRelative = HttpListenerContextAdapter.MakeRelativeUriFunc(baseUrl, "foo");
            
            Verify.That(() => makeRelative(new Uri("http://example.com/foo")) == "~/");
        }
    }
}
