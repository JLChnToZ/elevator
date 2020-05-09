using System;
using System.Security;

namespace Elevator {
    internal static class StringHelper {
        public static bool Is(this string first, string second) =>
            string.Equals(first, second, StringComparison.OrdinalIgnoreCase);

        public static SecureString ToSecureString(this string value) {
            var ss = new SecureString();
            foreach(var c in value)
                ss.AppendChar(c);
            return ss;
        }

        public static SecureString PromptPassword(string prompt, char mask = '*') {
            var password = new SecureString();
            Console.Write(prompt);
            while(true) {
                var input = Console.ReadKey(true);
                switch(input.Key) {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return password;
                    case ConsoleKey.Backspace:
                        if(password.Length == 0)
                            break;
                        Console.Write("\b \b");
                        password.RemoveAt(password.Length - 1);
                        break;
                    default:
                        char c = input.KeyChar;
                        if(char.IsControl(c))
                            break;
                        Console.Write(mask);
                        password.AppendChar(c);
                        break;
                }
            }

        }
    }
}