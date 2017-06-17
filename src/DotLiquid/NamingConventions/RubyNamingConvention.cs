using System;
using System.Text.RegularExpressions;
using DotLiquid.Util;

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
        private static readonly Regex _regex1 = R.C(@"([A-Z]+)([A-Z][a-z])");
        private static readonly Regex _regex2 = R.C(@"([a-z\d])([A-Z])");

        public StringComparer StringComparer
        {
            get { return StringComparer.OrdinalIgnoreCase; }
        }

        public string GetMemberName(string name)
        {
            // Replace any capital letters, apart from the first character, with _x, the same way Ruby does
            return _regex2.Replace(_regex1.Replace(name, "$1_$2"), "$1_$2").ToLowerInvariant();
        }

        public bool OperatorEquals(string testedOperator, string referenceOperator)
        {
            return GetMemberName(testedOperator).Equals(referenceOperator);
        }
    }
}
