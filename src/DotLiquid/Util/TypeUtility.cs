using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotLiquid.Util
{
    internal static class TypeUtility
    {
        private static ConditionalWeakTable<Type, ReflectionCacheValue> _cache = new ConditionalWeakTable<Type, ReflectionCacheValue>();

        private static ConditionalWeakTable<MethodInfo, LiquidFilterAttribute> _filterAttributeCache = new ConditionalWeakTable<MethodInfo, LiquidFilterAttribute>();

        private static ConditionalWeakTable<Type, LiquidTypeAttribute> _typeAttributeCache = new ConditionalWeakTable<Type, LiquidTypeAttribute>();


        public static bool IsAnonymousType(Type t)
        {
            return _cache.GetValue(t, (key) => new ReflectionCacheValue(t)).IsAnonymous;
        }

        public static LiquidFilterAttribute GetLiquidFilterAttribute(MethodInfo method)
        {
            return _filterAttributeCache.GetValue(method, (key) => method.GetCustomAttribute<LiquidFilterAttribute>());
        }

        public static LiquidTypeAttribute GetLiquidTypeAttribute(Type type)
        {
            return _typeAttributeCache.GetValue(type, (key) => type
#if NETSTANDARD1_3
                .GetTypeInfo()
#endif
                .GetCustomAttribute<LiquidTypeAttribute>());
        }
    }
}
