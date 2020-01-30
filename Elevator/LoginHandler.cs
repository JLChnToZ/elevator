namespace Elevator {
    [Priority(100)]
    internal class LoginHandler: ResolveEnvHandler {
        public LoginHandler(ParsedInfo info) : base(info) { }

        protected override void InitStartInfo(ParsedInfo info) {
            base.InitStartInfo(info);
            startInfo.UseShellExecute = false;
        }

        [ArgumentEntry("login")]
        public void HandleLogin(string value) =>
            startInfo.UserName = value;

        [ArgumentEntry("loginpw")]
        public void HandleLoginPw(string value) =>
            startInfo.Password = value.ToSecureString();

        [ArgumentEntry("loaduserprofile")]
        public void HandleLoadUserProfile() =>
            startInfo.LoadUserProfile = true;
    }
}
