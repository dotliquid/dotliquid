using System;

namespace DotLiquid.NamingConventions
{
    public class CSharpNamingConvention : INamingConvention
    {
        public StringComparer StringComparer
        {
            get { return StringComparer.Ordinal; }
        }

        public StringComparer ConditionComparer
        {
            get { return StringComparer.OrdinalIgnoreCase; }
        }

        public string GetMemberName(string name)
        {
            return name;
        }
    }
}
