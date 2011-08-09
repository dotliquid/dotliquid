using System;
using System.Text.RegularExpressions;

namespace DotLiquid.NamingConventions
{
	/// <summary>
	/// Converts C# member names to Ruby-style names for access by Liquid templates.
	/// </summary>
	/// <example>
	/// Input: Text
	/// Output: text
	/// 
	/// Input: ScopesAsArray
	/// Output: scopes_as_array
	/// </example>
	public class RubyNamingConvention : INamingConvention
	{
		private readonly Regex _regex1 = new Regex(@"([A-Z]+)([A-Z][a-z])");
		private readonly Regex _regex2 = new Regex(@"([a-z\d])([A-Z])");

		public StringComparer StringComparer
		{
			get { return StringComparer.OrdinalIgnoreCase; }
		}

		public string GetMemberName(string name)
		{
			// Replace any capital letters, apart from the first character, with _x, the same way Ruby does
			return _regex2.Replace(_regex1.Replace(name, "$1_$2"), "$1_$2").ToLower();
		}
	}
}