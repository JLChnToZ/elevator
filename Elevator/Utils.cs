using System;
using System.Reflection;

namespace Elevator {
    internal static class Utils {
        public static TDelegate GetMethodToDelegate<TDelegate>(this Type targetType, string methodName) where TDelegate : MulticastDelegate {
            if(targetType == null) throw new ArgumentNullException(nameof(targetType));
            if(methodName == null) throw new ArgumentNullException(nameof(methodName));
            var delegateType = typeof(TDelegate);
            return Delegate.CreateDelegate(
                delegateType,
                GetMatchingMethod(
                    targetType,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                    methodName,
                    delegateType
                )
            ) as TDelegate;
        }

        public static TDelegate GetMethodToDelegate<TDelegate>(object target, string methodName) where TDelegate : MulticastDelegate {
            if(target == null) throw new ArgumentNullException(nameof(target));
            if(methodName == null) throw new ArgumentNullException(nameof(methodName));
            var delegateType = typeof(TDelegate);
            return Delegate.CreateDelegate(
                delegateType,
                target,
                GetMatchingMethod(
                    target.GetType(),
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    methodName,
                    delegateType
                )
            ) as TDelegate;
        }

        private static MethodInfo GetMatchingMethod(Type targetType, BindingFlags bindingFlags, string methodName, Type delegateType) =>
            targetType.GetMethod(
                methodName,
                bindingFlags,
                null,
                Array.ConvertAll(
                    delegateType
                        .GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public)
                        .GetParameters(),
                    ParameterInfoToType
                ),
                null
            );

        private static Type ParameterInfoToType(ParameterInfo paramInfo) =>
            paramInfo.ParameterType;
    }
}
