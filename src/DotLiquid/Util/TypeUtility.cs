using System;
using System.Runtime.CompilerServices;

namespace DotLiquid.Util
{
    internal static class TypeUtility
    {
        private static ConditionalWeakTable<Type, ReflectionCache> _cache = new ConditionalWeakTable<Type, ReflectionCache>();

        public static bool IsAnonymousType(Type t)
        {
            return _cache.GetValue(t, (key) => new ReflectionCache(t)).IsAnonymous;
        }
    }
}
