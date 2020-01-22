using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Elevator {
    internal static class ArgumentsFactory {
        private static readonly Regex validateChar = new Regex("[\x00\x0a\x0d]");
        private static readonly Regex checkQuotesRequired = new Regex("\\s|\\\"\\\"");
        private static readonly Regex escapeMatcher = new Regex("(\\\\*)(\\\"\\\"|$)");

        public static void PushArg(this StringBuilder newArgs, Match m) =>
            newArgs.PushArg(m.Value);

        public static void PushArg(this StringBuilder newArgs, string arg) {
            if(arg == null) arg = string.Empty;
            if(validateChar.IsMatch(arg))
                throw new ArgumentOutOfRangeException(nameof(arg));
            if(newArgs.Length > 0)
                newArgs.Append(' ');
            if(arg == string.Empty) {
                newArgs.Append("\"\"");
                return;
            }
            if(!checkQuotesRequired.IsMatch(arg)) {
                newArgs.Append(arg);
                return;
            }
            newArgs
                .Append('"')
                .Append(escapeMatcher.Replace(arg, EscapeReplace))
                .Append('"');
        }

        public static void PushRestSeparator(this StringBuilder newArgs, bool isRest) =>
            newArgs.PushArg(isRest ? "--" : string.Empty);

        public static void PushRestArgs(this StringBuilder newArgs, string[] args, int stopIndex) {
            for(int i = stopIndex; i < args.Length; i++)
                newArgs.PushArg(args[i]);
        }

        public static void PushProcessId(this StringBuilder newArgs) =>
            newArgs.PushArg($"/x={Process.GetCurrentProcess().Id}");

        public static bool PushResolvedEnv(this StringBuilder newArgs, Match match) {
            var arg = match.Groups.SuccessOrEmpty(2);
            if(arg.StartsWith("e", StringComparison.OrdinalIgnoreCase) && !match.Groups.TryGetValue(3, out string _)) {
                var key = arg.Substring(1);
                newArgs.PushArg($"/e{key}={EnvironmentHelper.GetEnvValue(key)}");
                return true;
            }
            return false;
        }

        private static string EscapeReplace(Match match) {
            var group1 = match.Groups[1].Value;
            return string.Join(
                string.Empty,
                group1,
                group1,
                match.Groups[2].Value == "\"" ? "\\\"" : string.Empty
            );
        }
    }
}