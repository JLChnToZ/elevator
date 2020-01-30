using System;

namespace Elevator {
    internal static class Program {
        private static void Main(string[] args) {
            try {
                Environment.Exit(new ParsedInfo(args).GetHandler().Launch());
            } catch(Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}