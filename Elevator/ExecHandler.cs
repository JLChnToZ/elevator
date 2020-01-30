using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Elevator {
    [Priority(0)]
    internal class ExecHandler: ProcessStartHandler {
        public ExecHandler(ParsedInfo info) : base(info) { }

        protected override void InitStartInfo(ParsedInfo info) {
            startInfo.FileName = info.args[info.stopIndex];
            startInfo.UseShellExecute = false;
        }

        protected override void HandleArgument(int index, Match match) {
            if(match == null || !match.Groups.TryGetValue(2, out string arg) || arg.Length == 0)
                return;
            _ = this.HandleArgument(match);
        }

        protected override void FinalizeStartInfo(string[] args) {
            stopIndex++;
            base.FinalizeStartInfo(args);
        }

        [ArgumentEntry("cd")]
        public void HandleChdir(string value) =>
            startInfo.WorkingDirectory = value;

        [ArgumentEntry("w", Prefixed = true)]
        public void HandleWindowStyle(ProcessWindowStyle value) =>
            startInfo.WindowStyle = value;

        [ArgumentEntry("v", Prefixed = true)]
        public void HandleWindowStyle(string value) =>
            startInfo.Verb = value;

        [ArgumentEntry("nowindow")]
        public void HandleNoWindow() =>
            startInfo.CreateNoWindow = true;

        [ArgumentEntry("nowait")]
        public void HandleNoWait() =>
            wait = WaitMode.NoWait;
    }
}
