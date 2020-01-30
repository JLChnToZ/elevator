namespace Elevator {
    [Priority(50)]
    internal class SetEnvHandler: SelfRelaunchHandler {
        public SetEnvHandler(ParsedInfo info) : base(info) { }

        protected override void InitStartInfo(ParsedInfo info) {
            base.InitStartInfo(info);
            startInfo.UseShellExecute = false;
        }

        [ArgumentEntry("a", Prefixed = true)]
        public void HandleAppendEnvironment(string key, string value) {
            if(value == null) return;
            var newEnv = startInfo.EnvironmentVariables;
            var env = EnvironmentHelper.GetEnvValue(key, newEnv);
            newEnv[key] = env + value;
        }

        [ArgumentEntry("e", Prefixed = true)]
        public void HandleResolveEnvironment(string key, string value) {
            startInfo.EnvironmentVariables[key] = value ??
                EnvironmentHelper.GetEnvValue(key);
        }

        [ArgumentEntry("p", Prefixed = true)]
        public void HandlePrefixEnvironment(string key, string value) {
            if(value == null) return;
            var newEnv = startInfo.EnvironmentVariables;
            var env = EnvironmentHelper.GetEnvValue(key, newEnv);
            newEnv[key] = value + env;
        }
    }
}
