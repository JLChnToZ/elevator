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
    }
}