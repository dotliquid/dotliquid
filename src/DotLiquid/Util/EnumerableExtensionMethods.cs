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
				else if (item is IEnumerable)
					foreach (var subitem in Flatten((IEnumerable) item))
					{
						yield return subitem;
					}
				else
					yield return item;
		}
	}
}