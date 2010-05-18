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
        [Test]
        public void MakeRelativeUriFunc_should_handle_slashless_root_uri() {
            var request = new Uri("http://example.com");
            var makeRelative = HttpListenerContextAdapter.MakeRelativeUriFunc(request, "/");
            
            Assert.That(makeRelative(new Uri("http://example.com")), Is.EqualTo("~/"));
        }
    }
}
