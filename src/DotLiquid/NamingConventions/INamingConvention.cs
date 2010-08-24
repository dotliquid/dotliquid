using System;

namespace DotLiquid.NamingConventions
{
	public interface INamingConvention
	{
		StringComparer StringComparer { get; }
		string GetMemberName(string name);
	}
}