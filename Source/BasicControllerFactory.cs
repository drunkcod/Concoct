using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace Concoct
{
    public class BasicControllerFactory : IControllerFactory
    {
        readonly Dictionary<string, Func<IController>> controllers = new Dictionary<string, Func<IController>>();

        public IController CreateController(RequestContext requestContext, string controllerName) {
            Func<IController> controller;
            if (controllers.TryGetValue(controllerName, out controller))
                return controller();
            return new MissingController();
        }

        public void RegisterController(string name, Type type) {
            var builder = (Func<IController>)Expression.Lambda(
                typeof(Func<IController>),
                Expression.New(type))
            .Compile();
            controllers.Add(name, builder);
        }

        public void Register(IEnumerable<Type> types) {
            foreach (var item in types.Where(x => x.IsTypeOf<IController>()))
                RegisterController(item.Name.Replace("Controller", string.Empty), item);
        }

        public void ReleaseController(IController controller) {}
    }
}
