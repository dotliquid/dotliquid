using DotLiquid.NamingConventions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace DotLiquid.Tests
{
    public class Helper
    {
        public static async Task AssertTemplateResultAsync(string expected, string template, Hash localVariables, INamingConvention namingConvention)
        {
            //Have to lock Template.NamingConvention for this test to
            //prevent other tests from being run simultaneously that
            //require the default naming convention.
            var currentNamingConvention = Template.NamingConvention;
            Template.NamingConvention = namingConvention;

            try
            {
                await AssertTemplateResultAsync(expected, template, localVariables);
            }
            finally
            {
                Template.NamingConvention = currentNamingConvention;
            }
        }

        public static async Task AssertTemplateResultAsync(string expected, string template, Hash localVariables)
        {
            Assert.AreEqual(expected, await Template.Parse(template).RenderAsync(localVariables));
        }

        public static Task AssertTemplateResultAsync(string expected, string template)
        {
            return AssertTemplateResultAsync(expected, template, null);
        }

        [LiquidTypeAttribute("PropAllowed")]
        public class DataObject
        {
            public string PropAllowed { get; set; }
            public string PropDisallowed { get; set; }
        }

        public class DataObjectDrop : Drop
        {
            public string Prop { get; set; }
        }
    }
}
