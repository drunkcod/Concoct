using System;
using Cone;

namespace Concoct.Web
{
    [Describe(typeof(HttpListenerContextAdapter))]
    public class HttpListenerContextAdapterSpec
    {
        [Row("http://example.com/foo", "~/")]
        public void MakeRelativeUriFunc(string request, string expected) {
            var baseUrl = new Uri("http://example.com");
            var makeRelative = HttpListenerContextAdapter.MakeRelativeUriFunc(baseUrl, "/foo");
            
            Verify.That(() => makeRelative(new Uri(request)) == expected);
        }

        [Row("http://example.com:8080/foo", "~/")]
        public void MakeRelativeUriFunc_with_non_standard_port(string request, string expected) {
            var baseUrl = new Uri("http://example.com:8080");
            var makeRelative = HttpListenerContextAdapter.MakeRelativeUriFunc(baseUrl, "/foo");
            
            Verify.That(() => makeRelative(new Uri(request)) == expected);
        }

		public void can_be_rooted_at_top_level() {
            var baseUrl = new Uri("http://example.com/");
            var makeRelative = HttpListenerContextAdapter.MakeRelativeUriFunc(baseUrl, "/");
            
            Verify.That(() => makeRelative(new Uri("http://example.com/uri-stem")) == "~/uri-stem");
		}

        public void handles_virtual_directory_missing_initial_slash() {
            var baseUrl = new Uri("http://example.com/");
            var makeRelative = HttpListenerContextAdapter.MakeRelativeUriFunc(baseUrl, "foo");
            
            Verify.That(() => makeRelative(new Uri("http://example.com/foo")) == "~/");
        }
    }
}
