using System;
using System.Linq;

namespace DotLiquid.Util
{
    /// <summary>
    /// An implementation of <see cref="StringComparer"/> that treats identifiers in PascalCase, camelCase, and snake_case as equivalent. 
    /// </summary>
    /// <remarks>
    /// Since identifiers are converted to PascalCase before comparison, use of this comparer can impact performance. 
    /// </remarks>
    public class PermissiveStringComparer : StringComparer
    {
        /// <summary>
        /// Converts an identifier from camelCase or snake_case to PascalCase.
        /// </summary>
        public static string ToPascalCase(string str)
        {
            return str.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries).Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1)).Aggregate(string.Empty, (s1, s2) => s1 + s2);
        }

        public override int Compare(string x, string y)
        {
            return string.CompareOrdinal(ToPascalCase(x), ToPascalCase(y));
        }

        public override bool Equals(string x, string y)
        {
            return string.Equals(ToPascalCase(x), ToPascalCase(y));
        }

        public override int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}
