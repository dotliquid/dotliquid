namespace DotLiquid
{
    public class RubyFilters
    {
        public static string Xml_escape(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            try
            {
                return input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("&#34;", "&quot;").Replace("'", "&apos;");
            }
            catch
            {
                return input;
            }
        }
    }
}