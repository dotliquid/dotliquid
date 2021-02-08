using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    public class Helper
    {
        public static void AssertTemplateResult(string expected, string template, object anonymousObject, INamingConvention namingConvention)
        {
            //Have to lock Template.NamingConvention for this test to
            //prevent other tests from being run simultaneously that
            //require the default naming convention.
            var currentNamingConvention = Template.NamingConvention;
            lock(Template.NamingConvention)
            {
                Template.NamingConvention = namingConvention;

                try
                {
                    var localVariables = anonymousObject == null ? null : Hash.FromAnonymousObject(anonymousObject);
                    AssertTemplateResult(expected, template, localVariables);
                }
                finally
                {
                    Template.NamingConvention = currentNamingConvention;
                }
            }
        }

        public static void AssertTemplateResult(string expected, string template, INamingConvention namingConvention)
        {
            AssertTemplateResult(expected: expected, template: template, anonymousObject: null, namingConvention: namingConvention);
        }

        public static void AssertTemplateResult(string expected, string template, Hash localVariables)
        {
            Assert.AreEqual(expected, Template.Parse(template).Render(localVariables));
        }

        public static void AssertTemplateResult(string expected, string template)
        {
            AssertTemplateResult(expected: expected, template: template, localVariables: null);
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
