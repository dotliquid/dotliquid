using System;
using DotLiquid.Util;

namespace DotLiquid.NamingConventions
{
    /// <summary>
    /// A DotLiquid naming convention that treats PascalCase, camelCase, and snake_case identifiers as equivalent.
    /// </summary>
    public class PermissiveNamingConvention : INamingConvention
    {
        public StringComparer StringComparer { get; }

        public PermissiveNamingConvention()
        {
            StringComparer = new PermissiveStringComparer();
        }

        public string GetMemberName(string name)
        {
            return PermissiveStringComparer.ToPascalCase(name);
        }

        public bool OperatorEquals(string testedOperator, string referenceOperator)
        {
            return StringComparer.Equals(testedOperator, referenceOperator);
        }
    }
}
