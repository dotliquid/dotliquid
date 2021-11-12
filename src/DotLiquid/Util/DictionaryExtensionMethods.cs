using System;
using System.Collections.Generic;

namespace DotLiquid.Util
{
    /// <summary>
    /// Extensions for Dictionary
    /// </summary>
    public static class DictionaryExtensionMethods
    {
        /// <summary>
        /// Try to add value to dictionary by Func
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static V TryAdd<K, V>(this IDictionary<K, V> dic, K key, Func<V> factory)
        {
            if (!dic.TryGetValue(key, out V found))
                return dic[key] = factory();
            return found;
        }
    }
}
