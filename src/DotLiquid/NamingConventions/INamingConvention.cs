using System;

namespace DotLiquid.NamingConventions
{
    public interface INamingConvention
    {
        StringComparer StringComparer { get; }
        StringComparer ConditionComparer { get; }
        string GetMemberName(string name);
    }
}
