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

        protected override void HandleArgument(int index, Match match) {
            if(match == null) {
                AppendEmptyArgument(index == stopIndex - 1);
                return;
            }
            if(!HandlerMeta.HandleArgument(this as IHandler, match))
                AppendArgument(match);
        }

        protected void AppendEmptyArgument(bool isRestSeparator) =>
            AppendArgument(isRestSeparator ? "--" : string.Empty);

        protected override void FinalizeStartInfo(string[] args) {
            if(!attached)
                AppendArgument($"/x={Process.GetCurrentProcess().Id}");
            base.FinalizeStartInfo(args);
        }

        public override void HandleConsoleAttach(uint value) {
            base.HandleConsoleAttach(value);
            attached = true;
            AppendArgument($"/x={value}");
        }
    }
}
