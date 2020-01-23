using System.Text.RegularExpressions;

namespace Elevator {
    internal class SetEnvHandler: SelfRelaunchHandler {
        public SetEnvHandler(ParsedInfo info) : base(info) { }

        protected override void InitStartInfo(ParsedInfo info) {
            base.InitStartInfo(info);
            startInfo.UseShellExecute = false;
        }

        protected override void HandleArgument(int index, Match match) {
            if(match == null) {
                AppendEmptyArgument(index == stopIndex - 1);
                return;
            }
            var newEnv = startInfo.EnvironmentVariables;
            var arg = match.Groups.SuccessOrEmpty(2);
            string strValue;
            if(!string.IsNullOrEmpty(arg))
                switch(arg[0]) {
                    case 'A':
                    case 'a': {
                        var key = arg.Substring(1);
                        var env = EnvironmentHelper.GetEnvValue(key, newEnv);
                        if(match.Groups.TryGetValue(3, out strValue))
                            newEnv[key] = env + strValue;
                        return;
                    }
                    case 'E':
                    case 'e': {
                        var key = arg.Substring(1);
                        if(match.Groups.TryGetValue(3, out strValue))
                            newEnv[key] = strValue;
                        else
                            newEnv[key] = EnvironmentHelper.GetEnvValue(key);
                        return;
                    }
                    case 'P':
                    case 'p': {
                        var key = arg.Substring(1);
                        var env = EnvironmentHelper.GetEnvValue(key, newEnv);
                        if(match.Groups.TryGetValue(3, out strValue))
                            newEnv[key] = strValue + env;
                        return;
                    }
                    case 'X':
                    case 'x': {
                        if(arg.Is("x") && match.Groups.TryGetValue(3, out uint value)) {
                            EnvironmentHelper.ReattachConsole(value);
                            attached = true;
                        }
                        break;
                    }
                }
            AppendArgument(match);
        }
    }
}
