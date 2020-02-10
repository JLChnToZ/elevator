using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Elevator {
    public static class ArgumentHelper {
        private static readonly Regex validateChar = new Regex("[\x00\x0a\x0d]");
        private static readonly Regex checkQuotesRequired = new Regex("\\s|\\\"\\\"");
        private static readonly Regex escapeMatcher = new Regex("(\\\\*)(\\\"\\\"|$)");

        public static void AppendArgument(this StringBuilder newArgs, string arg) {
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
