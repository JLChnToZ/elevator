using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Elevator {
    using static HandlerMeta;

    public static class HandlerMeta {
        private delegate bool ComparePriorityDelegate(Match match, ref int priority);
        private delegate bool HandleArgumentDelegate(IHandler handler, Match match);

        private static readonly Dictionary<Type, ComparePriorityDelegate> comparePriorityCache;
        private static readonly Dictionary<Type, HandleArgumentDelegate> handleArgumentCache;

        internal static readonly Dictionary<Type, int> priortizedCache;

        static HandlerMeta() {
            priortizedCache = new Dictionary<Type, int>();
            comparePriorityCache = new Dictionary<Type, ComparePriorityDelegate>();
            handleArgumentCache = new Dictionary<Type, HandleArgumentDelegate>();
            LoadAssembly(Assembly.GetExecutingAssembly(), false);
        }

        private static void InvokeMain(Assembly assembly) {
            var entryPoint = assembly.EntryPoint;
            if(entryPoint == null) return;
            var paramInfos = entryPoint.GetParameters();
            object[] paramValues;
            switch(paramInfos.Length) {
                case 0:
                    paramValues = new object[0];
                    break;
                case 1:
                    if(paramInfos[0].ParameterType != typeof(string[]))
                        throw new NotSupportedException();
                    paramValues = new object[] { Environment.GetCommandLineArgs() };
                    break;
                default:
                    throw new NotSupportedException();
            }
            entryPoint.Invoke(null, paramValues);
        }

        public static void LoadAssembly(Assembly assembly, bool runEntryPoint = true) {
            if(runEntryPoint) InvokeMain(assembly);
            var handlerType = typeof(IHandler);
            foreach(Type type in assembly.GetTypes())
                if(handlerType.IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    PriorityAttribute.IsDefined(type))
                    RuntimeHelpers.RunClassConstructor(GetHandlerMetaType(type).TypeHandle);
        }

        public static void DoTypeMatch(Match match, ref Type result, ref int currentPriority) {
            foreach(var kv in priortizedCache)
                if(ComparePriority(kv.Key, match, ref currentPriority))
                    result = kv.Key;
        }

        public static bool ComparePriority<THandler>(Match match, ref int priority) where THandler : IHandler =>
            HandlerMeta<THandler>.ComparePriority(match, ref priority);

        public static bool ComparePriority(Type handlerType, Match match, ref int priority) {
            if(!comparePriorityCache.TryGetValue(handlerType, out var comparePriority))
                comparePriorityCache[handlerType] = comparePriority =
                    GetHandlerMetaType(handlerType).GetMethodToDelegate<ComparePriorityDelegate>("ComparePriority");
            return comparePriority(match, ref priority);
        }

        public static bool HandleArgument<THandler>(this THandler handler, Match match) where THandler : IHandler =>
            HandlerMeta<THandler>.HandleArgument(handler, match);

        public static bool HandleArgument(IHandler handler, Match match) {
            var handlerType = handler.GetType();
            if(!handleArgumentCache.TryGetValue(handlerType, out var handleArgument))
                handleArgumentCache[handlerType] = handleArgument =
                    GetHandlerMetaType(handlerType).GetMethodToDelegate<HandleArgumentDelegate>("HandleArgument");
            return handleArgument(handler, match);
        }

        internal static IEnumerable<HandlerMetaMethod> EnumerateMatches(Dictionary<char, HandlerMetaMethod[]> methodDict, Match match, bool priortized) {
            var arg = match.Groups.SuccessOrEmpty(2);
            var chr = char.ToLower(arg[0]);
            if(!methodDict.TryGetValue(chr, out var array))
                yield break;
            foreach(var entry in array)
                if(entry.Matches(arg, chr, priortized))
                    yield return entry;
        }

        private static Type GetHandlerMetaType(Type handlerType) =>
            typeof(HandlerMeta<>).MakeGenericType(handlerType);
    }

    public static class HandlerMeta<THandler> where THandler : IHandler {
        private static readonly Dictionary<char, HandlerMetaMethod[]> methodDict;
        private static readonly int handlerPriority;

        static HandlerMeta() {
            var handlerType = typeof(THandler);
            handlerPriority = (PriorityAttribute.GetCustomAttribute(handlerType)?.Prioirty).GetValueOrDefault(int.MinValue);
            priortizedCache.Add(handlerType, handlerPriority);
            methodDict = new Dictionary<char, HandlerMetaMethod[]>();
            var tempDict = new Dictionary<char, List<HandlerMetaMethod>>();
            foreach(var method in typeof(THandler).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
                var attribute = ArgumentEntryAttribute.GetCustomAttribute(method);
                if(attribute == null)
                    continue;
                var chr = attribute.FirstChar;
                if(!tempDict.TryGetValue(chr, out var list))
                    tempDict.Add(chr, list = new List<HandlerMetaMethod>());
                list.Add(new HandlerMetaMethod(method, attribute));
            }
            foreach(var kv in tempDict)
                methodDict[kv.Key] = kv.Value.ToArray();
        }

        public static bool ComparePriority(Match match, ref int priority) {
            using(var enumerator = EnumerateMatches(methodDict, match, true).GetEnumerator())
                if(enumerator.MoveNext() && handlerPriority > priority) {
                    priority = handlerPriority;
                    return true;
                }
            return false;
        }

        public static bool HandleArgument(IHandler handler, Match match) {
            string arg = match.Groups.SuccessOrEmpty(1);
            foreach(var entry in EnumerateMatches(methodDict, match, false))
                if(entry.Invoke(handler, match, arg))
                    return true;
            return false;
        }
    }
}
