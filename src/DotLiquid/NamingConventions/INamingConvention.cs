namespace DotLiquid.NamingConventions
{
    public interface INamingConvention
    {
        System.StringComparer StringComparer { get; }
        string GetMemberName(string name);
    }
}
