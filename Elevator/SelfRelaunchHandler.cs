using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Elevator {
    internal abstract class SelfRelaunchHandler: ProcessStartHandler {
        protected bool attached;

        protected SelfRelaunchHandler(ParsedInfo info) : base(info) { }

        protected override void InitStartInfo(ParsedInfo info) {
            startInfo.FileName = EnvironmentHelper.ExecPath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
        }

        protected void AppendEmptyArgument(bool isRestSeparator) =>
            AppendArgument(isRestSeparator ? "--" : string.Empty);

        protected bool AppendResolvedEnvironment(Match match) {
            var arg = match.Groups.SuccessOrEmpty(2);
            if(arg.StartsWith("e", StringComparison.OrdinalIgnoreCase) && !match.Groups.TryGetValue(3, out string _)) {
                var key = arg.Substring(1);
                AppendArgument($"/e{key}={EnvironmentHelper.GetEnvValue(key)}");
                return true;
            }
            return false;
        }

        protected override void FinalizeStartInfo(string[] args) {
            if(!attached)
                AppendArgument($"/x={Process.GetCurrentProcess().Id}");
            base.FinalizeStartInfo(args);
        }
    }
}
