﻿using System.Web;
using System.Web.Routing;
using Moq;
using NUnit.Framework;

namespace Concoct
{
    [TestFixture]
    public class BasicControllerFactoryTests
    {
        static RequestContext EmptyContext() { return new RequestContext(new Mock<HttpContextBase>().Object, new RouteData()); }
        [Test]
        public void Should_return_MissingController_if_no_matching_controller_available() {
            var factory = new BasicControllerFactory();
            Assert.That(factory.CreateController(EmptyContext(), "MissingController"), Is.TypeOf(typeof(MissingController)));
        }
    }
}