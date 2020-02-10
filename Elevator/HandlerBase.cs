using System.Text;
using System.Text.RegularExpressions;

namespace Elevator {
    internal abstract class HandlerBase: IHandler {
        private readonly StringBuilder newArgs = new StringBuilder();
        protected WaitMode wait = WaitMode.Wait;
        protected int stopIndex;

        public string NewArguments => newArgs.ToString();

        protected HandlerBase(ParsedInfo parsedInfo) {
            stopIndex = parsedInfo.stopIndex;

            InitStartInfo(parsedInfo);
            for(int i = 0; i < parsedInfo.stopIndex; i++)
                HandleArgument(i, parsedInfo.matches[i]);
            FinalizeStartInfo(parsedInfo.args);
        }

        public abstract int Launch();

        protected abstract void InitStartInfo(ParsedInfo info);

        protected abstract void HandleArgument(int index, Match match);

        protected virtual void FinalizeStartInfo(string[] args) {
            for(int i = stopIndex; i < args.Length; i++)
                newArgs.AppendArgument(args[i]);
        }

        protected void AppendArgument(Match m) =>
            newArgs.AppendArgument(m.Value);

        protected void AppendArgument(string arg) =>
            newArgs.AppendArgument(arg);

        [ArgumentEntry("x", Unpriortized = true)]
        public virtual void HandleConsoleAttach(uint value) {
            EnvironmentHelper.ReattachConsole(value);
        }
    }
}
