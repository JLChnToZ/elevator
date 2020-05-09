using System.Diagnostics;

namespace Elevator {
    internal abstract class ProcessStartHandler: HandlerBase {
        protected readonly ProcessStartInfo startInfo = new ProcessStartInfo();

        protected ProcessStartHandler(ParsedInfo info) : base(info) { }

        public override int Launch() {
            var process = Process.Start(startInfo);
            if(wait != WaitMode.Wait || process == null)
                return 0;
            process.WaitForExit();
            return process.ExitCode;
        }

        protected override void FinalizeStartInfo(string[] args) {
            base.FinalizeStartInfo(args);
            startInfo.Arguments = NewArguments;
        }
    }
}
