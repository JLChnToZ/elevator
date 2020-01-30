using System;
using System.Reflection;

namespace Elevator {
    internal static class Utils {
        public static TDelegate MethodToDelegate<TDelegate>(Type targetType, string methodName) where TDelegate : MulticastDelegate {
            var delegateType = typeof(TDelegate);
            return Delegate.CreateDelegate(delegateType,
                targetType.GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    Array.ConvertAll(
                        delegateType
                            .GetMethod("Invoke",
                                BindingFlags.Instance | BindingFlags.Public)
                            .GetParameters(),
                        ParameterInfoToType),
                    null),
                false) as TDelegate;
        }

        private static Type ParameterInfoToType(ParameterInfo paramInfo) =>
            paramInfo.ParameterType;
    }
}
