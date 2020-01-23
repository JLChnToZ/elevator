using System;

namespace Elevator {
    internal static class Program {
        private static void Main(string[] args) {
            try {
                HandlerBase handler;
                var info = new ParsedInfo(args);
                switch(info.mode) {
                    case RunMode.Exec:
                    case RunMode.Shell:
                        handler = new ExecHandler(info);
                        break;
                    case RunMode.SetEnv:
                        handler = new SetEnvHandler(info);
                        break;
                    case RunMode.RunAs:
                        handler = new RunAsHandler(info);
                        break;
                    case RunMode.Login:
                        handler = new LoginHandler(info);
                        break;
                    default:
                        throw new ArgumentException("Invalid mode");
                }
                var exitCode = handler.Launch();
                Environment.Exit(exitCode);
            } catch(Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}