using System;
using System.Text.RegularExpressions;

namespace Elevator {
    public struct ParsedInfo {
        private static readonly Regex rParseArg = new Regex("^(?:-{1,2}|\\/)((.+?)(?:=(.+?))?)$");

        public readonly Type handlerType;
        public readonly string[] args;
        public readonly Match[] matches;
        public readonly int stopIndex;

        public ParsedInfo(string[] args) {
            this.args = args;
            handlerType = Parse(args, out matches, out stopIndex);
        }

        private static Type Parse(string[] args, out Match[] matches, out int stopIndex) {
            var handlerType = typeof(ExecHandler);
            int priority = int.MinValue;
            matches = new Match[args.Length];
            for(int i = 0; i < args.Length; i++) {
                var m = rParseArg.Match(args[i]);
                matches[i] = m;
                if(!m.Success) {
                    stopIndex = i;
                    return handlerType;
                }
                if(string.IsNullOrEmpty(m.Groups.SuccessOrEmpty(2))) {
                    stopIndex = i + 1;
                    return handlerType;
                }
                HandlerMeta.DoTypeMatch(m, ref handlerType, ref priority);
            }
            stopIndex = args.Length;
            return handlerType;
        }

        public IHandler GetHandler() =>
            Activator.CreateInstance(handlerType, this) as IHandler;
    }
}
