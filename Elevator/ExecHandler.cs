using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Elevator {
    internal class ExecHandler: ProcessStartHandler {
        public ExecHandler(ParsedInfo info) : base(info) { }

        protected override void InitStartInfo(ParsedInfo info) {
            startInfo.FileName = info.args[info.stopIndex];
            startInfo.UseShellExecute = false;
        }

        protected override void HandleArgument(int index, Match match) {
            if(match == null || !match.Groups.TryGetValue(2, out string arg) || arg.Length == 0)
                return;
            switch(arg[0]) {
                case 'C':
                case 'c': {
                    if(arg.Is("cd") && match.Groups.TryGetValue(3, out string value))
                        startInfo.WorkingDirectory = value;
                    break;
                }
                case 'V':
                case 'v':
                    startInfo.UseShellExecute = true;
                    startInfo.Verb = arg.Substring(1);
                    break;
                case 'W':
                case 'w': {
                    if(Enum.TryParse(arg.Substring(1), true, out ProcessWindowStyle value))
                        startInfo.WindowStyle = value;
                    break;
                }
                case 'N':
                case 'n':
                    if(arg.Is("nowindow")) {
                        startInfo.CreateNoWindow = true;
                        break;
                    }
                    if(arg.Is("nowait")) {
                        wait = WaitMode.NoWait;
                        break;
                    }
                    break;
                case 'X':
                case 'x': {
                    if(arg.Is("x") && match.Groups.TryGetValue(3, out uint value))
                        EnvironmentHelper.ReattachConsole(value);
                    break;
                }
            }
        }

        protected override void FinalizeStartInfo(string[] args) {
            stopIndex++;
            base.FinalizeStartInfo(args);
        }
    }
}
