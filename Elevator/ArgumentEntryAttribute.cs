using System;
using System.Reflection;

namespace Elevator {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ArgumentEntryAttribute: Attribute {
        internal char FirstChar =>
            char.ToLower(Key[0]);

        public string Key { get; }

        public bool Prefixed { get; set; }

        public bool Unpriortized { get; set; }

        public ArgumentEntryAttribute(string key) =>
            Key = key;

        public static ArgumentEntryAttribute GetCustomAttribute(MethodInfo method) =>
            GetCustomAttribute(method, typeof(ArgumentEntryAttribute)) as ArgumentEntryAttribute;
    }
}
