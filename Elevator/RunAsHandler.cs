using System.Text.RegularExpressions;

namespace Elevator {
    internal class RunAsHandler: SelfRelaunchHandler {
        public RunAsHandler(ParsedInfo info) : base(info) { }

        protected override void InitStartInfo(ParsedInfo info) {
            base.InitStartInfo(info);
            startInfo.UseShellExecute = true;
            startInfo.Verb = "runas";
        }

        protected override void HandleArgument(int index, Match match) {
            if(match == null) {
                AppendEmptyArgument(index == stopIndex - 1);
                return;
            }
            var arg = match.Groups[1].Value;
            if(arg.Is("x") && match.Groups.TryGetValue(3, out uint value)) {
                EnvironmentHelper.ReattachConsole(value);
                attached = true;
            } else if(AppendResolvedEnvironment(match))
                return;
            if(!arg.Is("runas"))
                AppendArgument(match);
        }
    }
}
