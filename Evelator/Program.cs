using System;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;
using System.Security;

namespace Evelator {
    internal enum RunMode {
        Exec = 0,
        Shell = 1,
        SetEnv = 2,
        RunAs = 3,
    }

    internal static class Program {
        private static readonly Lazy<Regex> rParseArg = new Lazy<Regex>(InitRParseArg);
        private static readonly Lazy<Regex> cVaildateChar = new Lazy<Regex>(InitCVaildateChar);
        private static readonly Lazy<Regex> cQuotesRequired = new Lazy<Regex>(InitCQuotesRequired);
        private static readonly Lazy<Regex> rEscape = new Lazy<Regex>(InitREscape);

        private static Regex RParseArg => rParseArg.Value;
        private static Regex CVaildateChar => cVaildateChar.Value;
        private static Regex CQuotesRequired => cQuotesRequired.Value;
        private static Regex REscape => rEscape.Value;
        private static string ExecPath => Assembly.GetEntryAssembly().Location;

        private static Regex InitRParseArg() =>
            new Regex("^(?:-{1,2}|\\/)((.+?)(?:=(.+?))?)$", RegexOptions.Compiled);

        private static Regex InitCVaildateChar() =>
            new Regex("[\x00\x0a\x0d]", RegexOptions.Compiled);

        private static Regex InitCQuotesRequired() =>
            new Regex("\\s|\\\"\\\"", RegexOptions.Compiled);

        private static Regex InitREscape() =>
            new Regex("(\\\\*)(\\\"\\\"|$)", RegexOptions.Compiled);

        private static string GetEnv(string key, StringDictionary overrides = null) {
            try {
                if(overrides != null && overrides.ContainsKey(key))
                    return overrides[key];
                return Environment.GetEnvironmentVariable(key);
            } catch {
                return string.Empty;
            }
        }

        private static void PushNewArg(StringBuilder newArgs, string arg) {
            if(arg == null) arg = string.Empty;
            if(CVaildateChar.IsMatch(arg))
                throw new ArgumentOutOfRangeException(nameof(arg));
            if(newArgs.Length > 0)
                newArgs.Append(' ');
            if(arg == string.Empty) {
                newArgs.Append("\"\"");
                return;
            }
            if(!CQuotesRequired.IsMatch(arg)) {
                newArgs.Append(arg);
                return;
            }
            newArgs
                .Append('"')
                .Append(REscape.Replace(arg, ArgEscapeReplace))
                .Append('"');
        }

        private static string ArgEscapeReplace(Match match) {
            var group1 = match.Groups[1].Value;
            return string.Join(
                string.Empty,
                group1,
                group1,
                match.Groups[2].Value == "\"" ? "\\\"" : string.Empty
            );
        }

        private static void Main(string[] args) {
            try {
                ProcessStartInfo startInfo = null;
                switch(Priortize(args, out var matches, out int stopIndex)) {
                    case RunMode.Exec:
                    case RunMode.Shell:
                        startInfo = Exec(args, matches, stopIndex);
                        break;
                    case RunMode.SetEnv:
                        startInfo = SetEnv(args, matches, stopIndex);
                        break;
                    case RunMode.RunAs:
                        startInfo = RunAs(args, matches, stopIndex);
                        break;
                    default:
                        throw new ArgumentException("Invalid mode");
                }
                Process.Start(startInfo);
            } catch {
                Environment.Exit(1);
            }
        }

        private static RunMode Priortize(string[] args, out Match[] matches, out int stopIndex) {
            var mode = RunMode.Exec;
            matches = new Match[args.Length];
            for(int i = 0; i < args.Length; i++) {
                var m = RParseArg.Match(args[i]);
                matches[i] = m;
                if(!m.Success) {
                    stopIndex = i;
                    return mode;
                }
                var arg = m.Groups[2].Success ?
                    m.Groups[2].Value :
                    string.Empty;
                if(string.IsNullOrEmpty(arg)) {
                    stopIndex = i + 1;
                    return mode;
                }
                switch(arg[0]) {
                    case 'R':
                    case 'r':
                        if(string.Equals(arg, "runas", StringComparison.OrdinalIgnoreCase) && mode < RunMode.RunAs)
                            mode = RunMode.RunAs;
                        break;
                    case 'A':
                    case 'a':
                    case 'E':
                    case 'e':
                    case 'P':
                    case 'p':
                        if(mode < RunMode.SetEnv)
                            mode = RunMode.SetEnv;
                        break;
                    case 'V':
                    case 'v':
                        if(mode < RunMode.Shell)
                            mode = RunMode.Shell;
                        break;
                    case 'C':
                    case 'c':
                    case 'U':
                    case 'u':
                    case 'W':
                    case 'w':
                    case 'N':
                    case 'n':
                    default:
                        break;
                }
            }
            stopIndex = args.Length;
            return mode;
        }

        private static ProcessStartInfo RunAs(string[] args, Match[] matches, int stopIndex) {
            var info = new ProcessStartInfo(ExecPath) {
                UseShellExecute = true,
                Verb = "runas",
            };
            var newArgs = new StringBuilder();
            for(int i = 0; i < stopIndex; i++) {
                var match = matches[i];
                if(match == null) {
                    PushNewArg(newArgs, i == stopIndex - 1 ? "--" : string.Empty);
                    continue;
                }
                var arg = match.Groups[1].Value;
                if(!string.Equals(arg, "runas", StringComparison.OrdinalIgnoreCase))
                    PushNewArg(newArgs, match.Value);
            }
            for(int i = stopIndex; i < args.Length; i++)
                PushNewArg(newArgs, args[i]);
            info.Arguments = newArgs.ToString();
            return info;
        }

        private static ProcessStartInfo SetEnv(string[] args, Match[] matches, int stopIndex) {
            var info = new ProcessStartInfo(ExecPath) {
                UseShellExecute = false,
            };
            var newArgs = new StringBuilder();
            var newEnv = info.EnvironmentVariables;
            for(int i = 0; i < stopIndex; i++) {
                var m = matches[i];
                if(m == null) {
                    PushNewArg(newArgs, i == stopIndex - 1 ? "--" : string.Empty);
                    continue;
                }
                var arg = m.Groups[2].Success ?
                    m.Groups[2].Value :
                    string.Empty;
                if(!string.IsNullOrEmpty(arg))
                    switch(arg[0]) {
                        case 'A':
                        case 'a': {
                            var key = arg.Substring(1);
                            var env = GetEnv(key, newEnv);
                            var valueGroup = m.Groups[3];
                            newEnv[key] = valueGroup.Success ?
                                env + valueGroup.Value :
                                env;
                            continue;
                        }
                        case 'E':
                        case 'e': {
                            var key = arg.Substring(1);
                            var valueGroup = m.Groups[3];
                            newEnv[key] = valueGroup.Success ?
                                valueGroup.Value :
                                string.Empty;
                            continue;
                        }
                        case 'P':
                        case 'p': {
                            var key = arg.Substring(1);
                            var env = GetEnv(key, newEnv);
                            var valueGroup = m.Groups[3];
                            newEnv[key] = valueGroup.Success ?
                                valueGroup.Value + env :
                                env;
                            continue;
                        }
                    }
                PushNewArg(newArgs, m.Value);
            }
            for(int i = stopIndex; i < args.Length; i++)
                PushNewArg(newArgs, args[i]);
            info.Arguments = newArgs.ToString();
            return info;
        }

        private static ProcessStartInfo Exec(string[] args, Match[] matches, int stopIndex) {
            var info = new ProcessStartInfo(args[stopIndex]) {
                UseShellExecute = false,
            };
            var newArgs = new StringBuilder();
            for(int i = 0; i < stopIndex; i++) {
                var m = matches[i];
                var arg = m.Groups[2].Success ?
                    m.Groups[2].Value :
                    string.Empty;
                if(m == null)
                    continue;
                if(!string.IsNullOrEmpty(arg))
                    switch(arg[0]) {
                        case 'C':
                        case 'c':
                            if(string.Equals(arg, "cd", StringComparison.OrdinalIgnoreCase)) {
                                var valueGroup = m.Groups[3];
                                if(valueGroup.Success)
                                    info.WorkingDirectory = valueGroup.Value;
                                continue;
                            }
                            if(string.Equals(arg, "cusername", StringComparison.OrdinalIgnoreCase)) {
                                var valueGroup = m.Groups[3];
                                if(valueGroup.Success)
                                    info.UserName = valueGroup.Value;
                                continue;
                            }
                            if(string.Equals(arg, "cpassword", StringComparison.OrdinalIgnoreCase)) {
                                var valueGroup = m.Groups[3];
                                if(valueGroup.Success) {
                                    var pw = new SecureString();
                                    foreach(var c in valueGroup.Value)
                                        pw.AppendChar(c);
                                    info.Password = pw;
                                }
                                continue;
                            }
                            break;
                        case 'U':
                        case 'u':
                            if(string.Equals(arg, "userprofile", StringComparison.OrdinalIgnoreCase)) {
                                info.LoadUserProfile = true;
                                continue;
                            }
                            break;
                        case 'V':
                        case 'v':
                            info.UseShellExecute = true;
                            info.Verb = arg.Substring(1);
                            continue;
                        case 'W':
                        case 'w':
                            info.WindowStyle = (ProcessWindowStyle)Enum.Parse(typeof(ProcessWindowStyle), arg.Substring(1), true);
                            continue;
                        case 'N':
                        case 'n':
                            if(string.Equals(arg, "nowindow", StringComparison.OrdinalIgnoreCase)) {
                                info.CreateNoWindow = true;
                                continue;
                            }
                            if(string.Equals(arg, "nouserprofile", StringComparison.OrdinalIgnoreCase)) {
                                info.LoadUserProfile = false;
                                continue;
                            }
                            break;
                    }
            }
            for(int i = stopIndex + 1; i < args.Length; i++)
                PushNewArg(newArgs, args[i]);
            info.Arguments = newArgs.ToString();
            return info;
        }
    }
}
