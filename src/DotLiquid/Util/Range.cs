using System;
using System.Collections.Generic;

namespace DotLiquid.Util
{
    /// <summary>
    /// Provides enumerators over a range of values.
    /// </summary>
    internal static class Range
    {
        /// <summary>
        /// Enumerate from start to finish, inclusive
        /// </summary>
        /// <param name="start">The first value.</param>
        /// <param name="finish">The last value.</param>
        /// <returns></returns>
        public static IEnumerable<int> Inclusive(int start, int finish)
        {
            for (int value = start; value <= finish; value++)
            {
                yield return value;
            }
        }
    }
}
