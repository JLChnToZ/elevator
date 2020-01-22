using System.Text.RegularExpressions;

namespace Elevator {
    internal static class RegexHelper {
        public static bool TryGetValue(this GroupCollection groups, int index, out string value) {
            if(groups == null || groups.Count <= index) {
                value = string.Empty;
                return false;
            }
            var group = groups[index];
            var success = group.Success;
            value = success ? group.Value : string.Empty;
            return success;
        }

        public static bool TryGetValue(this GroupCollection groups, int index, out uint value) {
            if(groups.TryGetValue(index, out string strValue))
                return uint.TryParse(strValue, out value);
            value = 0;
            return false;
        }

        public static string SuccessOrEmpty(this GroupCollection groups, int index) {
            if(groups == null || groups.Count <= index)
                return string.Empty;
            var group = groups[index];
            return group.Success ? group.Value : string.Empty;
        }
    }
}