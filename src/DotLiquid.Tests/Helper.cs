using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    public class Helper
    {
        public static void AssertTemplateResult(string expected, string template, Hash localVariables, INamingConvention namingConvention = null)
        {
            var currentNamingConvention = Template.NamingConvention;
            if (namingConvention == null)
            {
                Template.NamingConvention = new RubyNamingConvention();
            }
            else
            {
                Template.NamingConvention = namingConvention;
            }

            try
            {
                Assert.AreEqual(expected, Template.Parse(template).Render(localVariables));
            }
            catch
            {
                throw;
            }
            finally
            {
                Template.NamingConvention = currentNamingConvention;
            }
        }

        public static void AssertTemplateResult(string expected, string template)
        {
            AssertTemplateResult(expected, template, null);
        }
    }
}
