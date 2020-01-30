using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elevator {
    internal abstract class ResolveEnvHandler: SelfRelaunchHandler {
        protected ResolveEnvHandler(ParsedInfo info) : base(info) { }

        [ArgumentEntry("e", Prefixed = true, Unpriortized = true)]
        public void HandleResolveEnvironment(string key, string value) =>
            AppendArgument($"/e{key}={value ?? EnvironmentHelper.GetEnvValue(key)}");
    }
}
