namespace DotLiquid.NamingConventions
{
    public interface INamingConvention
    {
        System.StringComparer StringComparer { get; }

        string GetMemberName(string name);

        bool OperatorEquals(string testedOperator, string referenceOperator);
    }
}
