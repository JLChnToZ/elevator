using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Elevator {
    internal static class Program {

        private static void Main(string[] args) {
            try {
                ProcessStartInfo startInfo;
                WaitMode wait;
                switch(Priortize(args, out var matches, out int stopIndex)) {
                    case RunMode.Exec:
                    case RunMode.Shell:
                        startInfo = Exec(args, matches, stopIndex, out wait);
                        break;
                    case RunMode.SetEnv:
                        startInfo = SetEnv(args, matches, stopIndex, out wait);
                        break;
                    case RunMode.RunAs:
                        startInfo = RunAs(args, matches, stopIndex, out wait);
                        break;
                    case RunMode.Login:
                        startInfo = Login(args, matches, stopIndex, out wait);
                        break;
                    default:
                        throw new ArgumentException("Invalid mode");
                }
                var process = Process.Start(startInfo);
                if(wait == WaitMode.Wait) {
                    process.WaitForExit();
                    Environment.Exit(process.ExitCode);
                }
            } catch {
                Environment.Exit(1);
            }
        }

        private static RunMode Priortize(string[] args, out Match[] matches, out int stopIndex) {
            var mode = RunMode.Exec;
            matches = new Match[args.Length];
            var rParseArg = new Regex("^(?:-{1,2}|\\/)((.+?)(?:=(.+?))?)$");
            for(int i = 0; i < args.Length; i++) {
                var m = rParseArg.Match(args[i]);
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
                        if(arg.Is("runas") && mode < RunMode.RunAs)
                            mode = RunMode.RunAs;
                        break;
                    case 'L':
                    case 'l':
                        if(arg.Is("login") && mode < RunMode.Login)
                            mode = RunMode.Login;
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
                }
            }
            stopIndex = args.Length;
            return mode;
        }

        private static ProcessStartInfo RunAs(string[] args, Match[] matches, int stopIndex, out WaitMode wait) {
            wait = WaitMode.Wait;
            var info = new ProcessStartInfo(EnvironmentHelper.ExecPath) {
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
            };
            var attached = false;
            var newArgs = new StringBuilder();
            for(int i = 0; i < stopIndex; i++) {
                var m = matches[i];
                if(m == null) {
                    newArgs.PushRestSeparator(i == stopIndex - 1);
                    continue;
                }
                var arg = m.Groups[1].Value;
                if(arg.Is("x") && m.Groups.TryGetValue(3, out uint value)) {
                    EnvironmentHelper.ReattachConsole(value);
                    attached = true;
                } else if(newArgs.PushResolvedEnv(m))
                    continue;
                if(!arg.Is("runas"))
                    newArgs.PushArg(m);
            }
            if(!attached)
                newArgs.PushProcessId();
            newArgs.PushRestArgs(args, stopIndex);
            info.Arguments = newArgs.ToString();
            return info;
        }

        private static ProcessStartInfo Login(string[] args, Match[] matches, int stopIndex, out WaitMode wait) {
            wait = WaitMode.Wait;
            var info = new ProcessStartInfo(EnvironmentHelper.ExecPath) {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
            };
            var attached = false;
            var newArgs = new StringBuilder();
            for(int i = 0; i < stopIndex; i++) {
                var m = matches[i];
                if(m == null) {
                    newArgs.PushRestSeparator(i == stopIndex - 1);
                    continue;
                }
                var arg = m.Groups[1].Value;
                string strValue;
                if(arg.Is("x") && m.Groups.TryGetValue(3, out uint value)) {
                    EnvironmentHelper.ReattachConsole(value);
                    attached = true;
                }
                if(arg.Is("login")) {
                    if(m.Groups.TryGetValue(3, out strValue))
                        info.UserName = strValue;
                    continue;
                }
                if(arg.Is("loginpw")) {
                    if(m.Groups.TryGetValue(3, out strValue))
                        info.Password = strValue.ToSecureString();
                    continue;
                }
                if(arg.Is("loaduserprofile")) {
                    info.LoadUserProfile = true;
                    continue;
                }
                if(newArgs.PushResolvedEnv(m))
                    continue;
                newArgs.PushArg(m);
            }
            if(!attached)
                newArgs.PushProcessId();
            newArgs.PushRestArgs(args, stopIndex);
            info.Arguments = newArgs.ToString();
            return info;
        }

        private static ProcessStartInfo SetEnv(string[] args, Match[] matches, int stopIndex, out WaitMode wait) {
            wait = WaitMode.Wait;
            var info = new ProcessStartInfo(EnvironmentHelper.ExecPath) {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
            };
            var attached = false;
            var newArgs = new StringBuilder();
            var newEnv = info.EnvironmentVariables;
            for(int i = 0; i < stopIndex; i++) {
                var m = matches[i];
                if(m == null) {
                    newArgs.PushRestSeparator(i == stopIndex - 1);
                    continue;
                }
                var arg = m.Groups.SuccessOrEmpty(2);
                string strValue;
                if(!string.IsNullOrEmpty(arg))
                    switch(arg[0]) {
                        case 'A':
                        case 'a': {
                            var key = arg.Substring(1);
                            var env = EnvironmentHelper.GetEnvValue(key, newEnv);
                            if(m.Groups.TryGetValue(3, out strValue))
                                newEnv[key] = env + strValue;
                            continue;
                        }
                        case 'E':
                        case 'e': {
                            var key = arg.Substring(1);
                            if(m.Groups.TryGetValue(3, out strValue))
                                newEnv[key] = strValue;
                            else
                                newEnv[key] = EnvironmentHelper.GetEnvValue(key);
                            continue;
                        }
                        case 'P':
                        case 'p': {
                            var key = arg.Substring(1);
                            var env = EnvironmentHelper.GetEnvValue(key, newEnv);
                            if(m.Groups.TryGetValue(3, out strValue))
                                newEnv[key] = strValue + env;
                            continue;
                        }
                        case 'X':
                        case 'x': {
                            if(arg.Is("x") && m.Groups.TryGetValue(3, out uint value)) {
                                EnvironmentHelper.ReattachConsole(value);
                                attached = true;
                            }
                            newArgs.PushArg(m);
                            continue;
                        }
                    }
                newArgs.PushArg(m);
            }
            if(!attached)
                newArgs.PushProcessId();
            newArgs.PushRestArgs(args, stopIndex);
            info.Arguments = newArgs.ToString();
            return info;
        }

        private static ProcessStartInfo Exec(string[] args, Match[] matches, int stopIndex, out WaitMode wait) {
            wait = WaitMode.Wait;
            var info = new ProcessStartInfo(args[stopIndex]) {
                UseShellExecute = false,
            };
            var newArgs = new StringBuilder();
            for(int i = 0; i < stopIndex; i++) {
                var m = matches[i];
                if(m == null)
                    continue;
                var arg = m.Groups.SuccessOrEmpty(2);
                string strValue;
                if(!string.IsNullOrEmpty(arg))
                    switch(arg[0]) {
                        case 'C':
                        case 'c':
                            if(arg.Is("cd")) {
                                if(m.Groups.TryGetValue(3, out strValue))
                                    info.WorkingDirectory = strValue;
                                break;
                            }
                            break;
                        case 'V':
                        case 'v':
                            info.UseShellExecute = true;
                            info.Verb = arg.Substring(1);
                            break;
                        case 'W':
                        case 'w': {
                            if(Enum.TryParse(arg.Substring(1), true, out ProcessWindowStyle value))
                                info.WindowStyle = value;
                            break;
                        }
                        case 'N':
                        case 'n':
                            if(arg.Is("nowindow")) {
                                info.CreateNoWindow = true;
                                break;
                            }
                            if(arg.Is("nowait")) {
                                wait = WaitMode.NoWait;
                                break;
                            }
                            break;
                        case 'X':
                        case 'x': {
                            if(arg.Is("x") && m.Groups.TryGetValue(3, out uint value))
                                EnvironmentHelper.ReattachConsole(value);
                            break;
                        }
                    }
            }
            newArgs.PushRestArgs(args, stopIndex + 1);
            info.Arguments = newArgs.ToString();
            return info;
        }
    }
}