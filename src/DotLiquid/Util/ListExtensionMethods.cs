using System.Collections.Generic;

namespace DotLiquid.Util
{
	public static class ListExtensionMethods
	{
		/// <summary>
		/// Removes the first element from the list and returns it,
		/// or null if the list is empty.
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