using System;
using System.Collections;
using System.Collections.Generic;

namespace DotLiquid.Util
{
    public static class EnumerableExtensionMethods
    {
        public static IEnumerable Flatten(this IEnumerable array)
        {
            foreach (var item in array)
            {
                if (item is string || !(item is IEnumerable) || item is IDictionary<string, object>)
                {
                    yield return item;
                }
                else
                {
                    foreach (var subitem in Flatten((IEnumerable)item))
                    {
                        yield return subitem;
                    }
                }
            }
        }

        public static void EachWithIndex(this IEnumerable<object> array, Action<object, int> callback)
        {
            int index = 0;
            foreach (object item in array)
            {
                callback(item, index);
                ++index;
            }
        }

        /// <summary>
        /// Determines whether a sequence contains any elements.
        /// </summary>
        /// <param name="source">The <see cref="IEnumerable"/> to check for emptiness.</param>
        /// <returns><see langword="true"/> if the source sequence contains any elements; otherwise, <see langword="false"/>.</returns>
        public static bool Any(this IEnumerable source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            IEnumerator enumerator = source.GetEnumerator();
            // Unfortunately unlike IEnumerator<T>, IEnumerator does not implement IDisposable. (A design flaw fixed when IEnumerator<T> was added).
            // We need to test whether disposal is required or not.
            if (enumerator is IDisposable disposableEnumerator)
            {
                using (disposableEnumerator)
                    return enumerator.MoveNext();
            }

            return enumerator.MoveNext();
        }
    }
}
