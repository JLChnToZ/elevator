using System.Text.RegularExpressions;

namespace Elevator {
    internal class LoginHandler: SelfRelaunchHandler {

        public LoginHandler(ParsedInfo info) : base(info) { }

        protected override void InitStartInfo(ParsedInfo info) {
            base.InitStartInfo(info);
            startInfo.UseShellExecute = false;
        }

        protected override void HandleArgument(int index, Match match) {
            if(match == null) {
                AppendEmptyArgument(index == stopIndex - 1);
                return;
            }
            var arg = match.Groups[1].Value;
            string strValue;
            if(arg.Is("x") && match.Groups.TryGetValue(3, out uint value)) {
                EnvironmentHelper.ReattachConsole(value);
                attached = true;
            }
            if(arg.Is("login")) {
                if(match.Groups.TryGetValue(3, out strValue))
                    startInfo.UserName = strValue;
                return;
            }
            if(arg.Is("loginpw")) {
                if(match.Groups.TryGetValue(3, out strValue))
                    startInfo.Password = strValue.ToSecureString();
                return;
            }
            if(arg.Is("loaduserprofile")) {
                startInfo.LoadUserProfile = true;
                return;
            }
            if(AppendResolvedEnvironment(match))
                return;
            AppendArgument(match);
        }
    }
}
