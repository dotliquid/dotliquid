using System.Collections.Generic;

namespace DotLiquid.Util
{
    public static class ListExtensionMethods
    {
        /// <summary>
        /// Returns the element at a certain position in the list, or null if there is no such element.
        /// The list is not modified.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="list">list</param>
        /// <param name="index">reverse index</param>
        /// <returns>element</returns>
        public static T TryGetAtIndex<T>(this List<T> list, int index)
            where T : class
        {
            if (list != null && list.Count > index && index >= 0)
            {
                return list[index];
            }

            return null;
        }

        /// <summary>
        /// Removes the first element from the list and returns it,
        /// or null if the list is empty.
        /// WARNING: The RemoveAt() operation is O(N). 
        /// If the element does not actually need to be removed from the list, use TryGetAtIndex() instead.ed
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Shift<T>(this List<T> list)
            where T : class
        {
            if (list == null || list.Count == 0)
                return null;

            T result = list[0];
            list.RemoveAt(0);

            return result;
        }

        /// <summary>
        /// Removes the last element from the list and returns it,
        /// or null if the list is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Pop<T>(this List<T> list)
            where T : class
        {
            if (list == null || list.Count == 0)
                return null;

            T result = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);

            return result;
        }
    }
}
