using System;
using System.Reflection;

namespace Elevator {
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class PriorityAttribute: Attribute {
        public int Prioirty { get; }

        public PriorityAttribute(int priority) =>
            Prioirty = priority;

        public static PriorityAttribute GetCustomAttribute(Type member) =>
            GetCustomAttribute(member, typeof(PriorityAttribute)) as PriorityAttribute;

        public static bool IsDefined(Type member) =>
            IsDefined(member, typeof(PriorityAttribute));
    }
}
