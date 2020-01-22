using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Elevator {
    internal static class EnvironmentHelper {
        public static string ExecPath => Assembly.GetEntryAssembly().Location;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeConsole();

        public static string GetEnvValue(string key, StringDictionary overrides = null) {
            try {
                if(overrides != null && overrides.ContainsKey(key))
                    return overrides[key];
                return Environment.GetEnvironmentVariable(key);
            } catch {
                return string.Empty;
            }
        }

        public static void ReattachConsole(uint processId) {
            FreeConsole();
            AttachConsole(processId);
        }
    }
}