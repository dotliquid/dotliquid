using System.Collections;

namespace DotLiquid.Util
{
	public static class EnumerableExtensionMethods
	{
		public static IEnumerable Flatten(this IEnumerable array)
		{
			foreach (var item in array)
				if (item is string)
					yield return item;
				else
				{
					var enumerable = item as IEnumerable;
					if (enumerable != null)
						foreach (var subitem in Flatten(enumerable))
						{
							yield return subitem;
						}
					else
						yield return item;
				}
		}
	}
}