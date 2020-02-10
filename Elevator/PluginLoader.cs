using System.IO;
using System.Reflection;

namespace Elevator {
    internal static class PluginLoader {

        public static void LoadPlugins() {
            foreach(var file in Directory.GetFiles(Path.GetDirectoryName(EnvironmentHelper.ExecPath), "*.elevator-plugin.dll")) {
                try {
                    HandlerMeta.LoadAssembly(Assembly.LoadFile(file));
                } catch { }
            }
        }
    }
}
