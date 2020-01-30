namespace Elevator {
    [Priority(200)]
    internal class RunAsHandler: ResolveEnvHandler {
        public RunAsHandler(ParsedInfo info) : base(info) { }

        [ArgumentEntry("runas")]
        public void HandleRunAs() {
            startInfo.UseShellExecute = true;
            startInfo.Verb = "runas";
        }
    }
}
