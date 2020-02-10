using System;

namespace Elevator {
    internal static class Program {
        private static void Main(string[] args) {
            try {
                PluginLoader.LoadPlugins();
                var handler = new ParsedInfo(args).GetHandler();
                Environment.Exit(handler.Launch());
            } catch(Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}