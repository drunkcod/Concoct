using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;

namespace Concoct
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class MixerTargetAttribute : Attribute {}

    class TypeMixer
    {
        readonly Type targetType;
        readonly TypeBuilder type;
        readonly FieldInfo targetField;

        public TypeMixer(Type targetType, TypeBuilder type) {
            this.targetType = targetType;
            this.type = type;
            this.targetField = GetTargetField(targetType, type);
        }

        FieldInfo GetTargetField(Type targetType, TypeBuilder type) {
            var existingField = FindTargetField(type.BaseType, targetType);
            if(existingField != null)
                return existingField;
            return type.DefineField("$", targetType, FieldAttributes.InitOnly | FieldAttributes.Private);
        }

        FieldInfo FindTargetField(Type baseType, Type targetType){
            return baseType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(TargetField(targetType));
        }

        static Func<FieldInfo, bool> TargetField(Type targetType)
        {
            return x => x.FieldType == targetType && x.GetCustomAttributes(typeof(MixerTargetAttribute), true).Length != 0;
        }

        public ConstructorInfo DefineConstructor() {
            var ctorParameters = new[] { targetType };
            var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, ctorParameters);
            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, targetField);
            il.Emit(OpCodes.Ret);

            return type.CreateType().GetConstructor(ctorParameters);
        }

        public void OverrideMatchingMethods(IEnumerable<MethodInfo> methodsToMix) {
            foreach (var wantedMethod in methodsToMix) {
                foreach (var targetMethod in targetType.GetMethods()) {
                    if (CantImplement(targetMethod, wantedMethod))
                        continue;
                    DefineOverride(wantedMethod, targetMethod);
                }
            }
        }

        private static bool CantImplement(MethodInfo targetMethod, MethodInfo wantedMethod) {
            return targetMethod.Name != wantedMethod.Name || targetMethod.ReturnType != wantedMethod.ReturnType;
        }

        void DefineOverride(MethodInfo wantedMethod, MethodInfo targetMethod)
        {
            var impl = type.DefineMethod(wantedMethod.Name,
                MethodAttributes.HideBySig | MethodAttributes.Virtual | (wantedMethod.Attributes & MethodAttributes.MemberAccessMask),
                wantedMethod.ReturnType,
                wantedMethod.GetParameters().Select(x => x.ParameterType).ToArray());
            var il = impl.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, targetField);
            il.Emit(OpCodes.Tailcall);
            il.Emit(OpCodes.Callvirt, targetMethod);
            il.Emit(OpCodes.Ret);
        }
    }

    public static class TypeMixer<TWanted>
    {
        static readonly Dictionary<Type, Func<object, TWanted>> constructorCache = new Dictionary<Type, Func<object, TWanted>>();

        public static TWanted MixWith(object target) {
            return GetConstructor(target.GetType())(target);
        }

        static Func<object, TWanted> GetConstructor(Type targetType) {
            Func<object, TWanted> constructor;
            if (constructorCache.TryGetValue(targetType, out constructor))
                return constructor;
            return CreateConstructor(targetType);
        }

        static Func<object, TWanted> CreateConstructor(Type targetType) {
            var proxies = AppDomain.CurrentDomain.DefineDynamicAssembly(Assembly.GetExecutingAssembly().GetName(), AssemblyBuilderAccess.Run);
            var module = proxies.DefineDynamicModule("Mixers");

            var mixer = new TypeMixer(targetType, DefineType(module, targetType));
            mixer.OverrideMatchingMethods(MethodsToMix());

            return CreateLambda(targetType, mixer.DefineConstructor());
        }

        private static Func<object, TWanted> CreateLambda(Type targetType, ConstructorInfo constructor) {
            var input = Expression.Parameter(typeof(object), "target");
            var body = Expression.New(constructor, Expression.Convert(input, targetType));
            return constructorCache[targetType] = Expression.Lambda<Func<object, TWanted>>(body, input).Compile();
        }

        static TypeBuilder DefineType(ModuleBuilder module, Type targetType) {
            return module.DefineType(targetType.FullName + "Mixer", TypeAttributes.Class, typeof(TWanted));
        }

        static IEnumerable<MethodInfo> MethodsToMix() {
            return typeof(TWanted).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.IsVirtual && x.DeclaringType != typeof(object));
        }
    }
}
