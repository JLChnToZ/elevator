using System.Text.RegularExpressions;

namespace Elevator {
    internal struct ParsedInfo {
        public readonly RunMode mode;
        public readonly string[] args;
        public readonly Match[] matches;
        public readonly int stopIndex;

        public ParsedInfo(string[] args) {
            this.args = args;
            mode = Parse(args, out matches, out stopIndex);
        }

        private static RunMode Parse(string[] args, out Match[] matches, out int stopIndex) {
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
    }
}
