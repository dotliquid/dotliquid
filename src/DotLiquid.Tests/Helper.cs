using System;
using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    public class Helper
    {
        public static void AssertTemplateResult(string expected, string template, Hash localVariables, INamingConvention namingConvention)
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
                    AssertTemplateResult(expected, template, localVariables);
                }
                finally
                {
                    Template.NamingConvention = currentNamingConvention;
                }
            }
        }

        public static void AssertTemplateResult(string expected, string template, Hash localVariables)
        {
            Assert.AreEqual(expected, Template.Parse(template).Render(localVariables));
        }

        public static void AssertTemplateResult(string expected, string template)
        {
            AssertTemplateResult(expected, template, null);
        }
    }
}
