using System;
using System.Runtime.CompilerServices;

namespace DotLiquid.Util
{
    internal static class TypeUtility
    {
        private static ConditionalWeakTable<Type, ReflectionCacheValue> _cache = new ConditionalWeakTable<Type, ReflectionCacheValue>();

        public static bool IsAnonymousType(Type t)
        {
            return _cache.GetValue(t, (key) => new ReflectionCacheValue(t)).IsAnonymous;
        }
    }
}
