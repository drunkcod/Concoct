using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace Concoct
{
    class BasicControllerFactory : IControllerFactory
    {
        readonly Dictionary<string, Func<IController>> controllers = new Dictionary<string, Func<IController>>();

        public IController CreateController(RequestContext requestContext, string controllerName) {
            return controllers[controllerName]();
        }

        public void RegisterController(string name, Type type) {
            var builder = (Func<IController>)Expression.Lambda(
                typeof(Func<IController>),
                Expression.New(type))
            .Compile();
            controllers.Add(name, builder);
        }

        public void Register(IEnumerable<Type> types) {
            foreach (var item in types.Where(x => typeof(IController).IsAssignableFrom(x)))
                RegisterController(item.Name.Replace("Controller", string.Empty), item);
        }

        public void ReleaseController(IController controller) {}
    }
}
