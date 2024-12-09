using System;
using System.Text;

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
        /// <inheritdoc />
        public StringComparer StringComparer
        {
            get { return StringComparer.OrdinalIgnoreCase; }
        }

        /// <inheritdoc />
        public string GetMemberName(string name)
        {
            var nameEnumerator = new DotLiquid.Util.CharEnumerator(name);
            var nameBuilder = new StringBuilder(name.Length + 2);
            while (nameEnumerator.MoveNext())
            {
                var letter = nameEnumerator.Current;
                var letterLower = char.IsUpper(letter) ? Char.ToLowerInvariant(letter) : letter;

                if (nameEnumerator.Position == 1 || letter == letterLower)
                    nameBuilder.Append(letterLower);
                else if (char.IsLower(nameEnumerator.Previous) || (nameEnumerator.HasNext() && char.IsLower(nameEnumerator.Next)))
                    nameBuilder.Append("_" + letterLower);
                else
                    nameBuilder.Append(letterLower);
            }
            return nameBuilder.ToString();
        }

        /// <inheritdoc />
        public bool OperatorEquals(string testedOperator, string referenceOperator)
        {
            return GetMemberName(testedOperator).Equals(referenceOperator);
        }
    }
}
