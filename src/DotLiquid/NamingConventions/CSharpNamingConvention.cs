namespace DotLiquid.NamingConventions
{
    public class CSharpNamingConvention : INamingConvention
    {
        public System.StringComparer StringComparer
        {
            get { return System.StringComparer.Ordinal; }
        }

        public string GetMemberName(string name)
        {
            return name;
        }
    }
}
