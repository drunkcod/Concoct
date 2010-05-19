using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Concoct.Web
{
    [TestFixture]
    public class HttpListenerContextAdapterTests
    {
        [TestCase("http://example.com/foo", "~/"),
         TestCase("http://example.com/foo/", "~/")]
        public void MakeRelativeUriFunc(string request, string expected) {
            var baseUrl = new Uri("http://example.com");
            var makeRelative = HttpListenerContextAdapter.MakeRelativeUriFunc(baseUrl, "/foo");
            
            Assert.That(makeRelative(new Uri(request)), Is.EqualTo(expected));
        }
        [Test]
        public void Should_handle_virtual_directory_missing_initial_slash()
        {
            var baseUrl = new Uri("http://example.com/");
            var makeRelative = HttpListenerContextAdapter.MakeRelativeUriFunc(baseUrl, "foo");
            
            Assert.That(makeRelative(new Uri("http://example.com/foo")), Is.EqualTo("~/"));
        }
    }
}
