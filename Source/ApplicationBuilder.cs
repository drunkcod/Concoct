using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Concoct
{
    class ApplicationBuilder
    {
        readonly Type applicationType;
        readonly TypeBuilder typeBuilder;

        public static ApplicationBuilder CreateIn(ModuleBuilder module, Type applicationType) {
            var typeBuilder = module.DefineType("ApplicationProxyFor" + applicationType.Name,
                                        TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Class,
                                        applicationType,
                                        new[] { typeof(IApplication) });
            return new ApplicationBuilder(applicationType, typeBuilder);
        }

        ApplicationBuilder(Type applicationType, TypeBuilder typeBuilder) {
            this.applicationType = applicationType;
            this.typeBuilder = typeBuilder;
        }

        public void DynamicEventWireUp(MethodInfo method, string eventName) {
            var start = typeBuilder.DefineMethod(method.Name, MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.Final);
            var il = start.GetILGenerator();
            var appStart = applicationType.GetMethod(eventName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (appStart != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Tailcall);
                il.Emit(OpCodes.Call, appStart);
            }
            il.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(start, method);
        }

        public IApplication CreateType() {
            return (IApplication)typeBuilder.CreateType().GetConstructor(Type.EmptyTypes).Invoke(null);
        }
    }
}
