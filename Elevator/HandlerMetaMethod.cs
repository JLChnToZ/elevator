using System;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Elevator {
    internal struct HandlerMetaMethod {
        private readonly MethodInfo methodInfo;
        private readonly ArgumentEntryAttribute attribute;

        public HandlerMetaMethod(MethodInfo methodInfo, ArgumentEntryAttribute attribute) {
            this.methodInfo = methodInfo;
            this.attribute = attribute;
        }

        public bool Matches(string arg, char chr, bool priortized) =>
            (priortized && attribute.Unpriortized) ? false :
            attribute.Prefixed ?
                attribute.FirstChar == chr :
                attribute.Key.Is(arg);

        public bool Invoke(IHandler handler, Match match, string arg) {
            var parameters = methodInfo.GetParameters();
            string value;
            switch(parameters.Length) {
                case 0:
                    if(TryInvokeHandler(parameters, handler))
                        return true;
                    break;
                case 1:
                    if(attribute.Prefixed ?
                        TryInvokeHandler(parameters, handler, GetPrefixedArgument(arg)) :
                        (match.Groups.TryGetValue(3, out value) &&
                        TryInvokeHandler(parameters, handler, value)))
                        return true;
                    break;
                case 2:
                    if(match.Groups.TryGetValue(3, out value) &&
                        TryInvokeHandler(parameters, handler, GetPrefixedArgument(arg), value))
                        return true;
                    break;
            }
            return false;
        }

        private string GetPrefixedArgument(string arg) =>
            arg.Substring(attribute.Key.Length);

        private bool TryInvokeHandler(ParameterInfo[] paramInfos, IHandler handler, params object[] values) {
            try {
                if(values == null)
                    values = new object[paramInfos.Length];
                for(int i = 0; i < paramInfos.Length; i++) {
                    var type = paramInfos[i].ParameterType;
                    if(values[i] == null && !type.IsValueType)
                        continue;
                    values[i] = Convert.ChangeType(values[i], type);
                }
            } catch {
                return false;
            }
            methodInfo.Invoke(handler, values);
            return true;
        }
    }
}
