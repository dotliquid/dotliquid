using System;
using System.Collections.Generic;
using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    public class Helper
    {
        public static void LockTemplateStaticVars(INamingConvention namingConvention, Action test)
        {
            //Have to lock Template.NamingConvention for this test to
            //prevent other tests from being run simultaneously that
            //require the default naming convention.
            var currentNamingConvention = Template.NamingConvention;
            var currentSyntax = Template.DefaultSyntaxCompatibilityLevel;
            var currentIsRubyDateFormat = Liquid.UseRubyDateFormat;
            lock (Template.NamingConvention)
            {
                Template.NamingConvention = namingConvention;

                try
                {
                    test();
                }
                finally
                {
                    Template.NamingConvention = currentNamingConvention;
                    Template.DefaultSyntaxCompatibilityLevel = currentSyntax;
                    Liquid.UseRubyDateFormat = currentIsRubyDateFormat;
                }
            }
        }


        public static void AssertTemplateResult(string expected, string template, object anonymousObject, INamingConvention namingConvention, SyntaxCompatibility syntax = SyntaxCompatibility.DotLiquid20)
        {
            LockTemplateStaticVars(namingConvention, () =>
            {
                var localVariables = anonymousObject == null ? null : Hash.FromAnonymousObject(anonymousObject);
                var parameters = new RenderParameters(System.Globalization.CultureInfo.CurrentCulture)
                {
                    LocalVariables = localVariables,
                    SyntaxCompatibilityLevel = syntax
                };
                Assert.AreEqual(expected, Template.Parse(template).Render(parameters));
            });
        }

        public static void AssertTemplateResult(string expected, string template, INamingConvention namingConvention)
        {
            AssertTemplateResult(expected: expected, template: template, anonymousObject: null, namingConvention: namingConvention);
        }

        public static void AssertTemplateResult(string expected, string template, Hash localVariables, IEnumerable<Type> localFilters, SyntaxCompatibility syntax = SyntaxCompatibility.DotLiquid20)
        {
            var parameters = new RenderParameters(System.Globalization.CultureInfo.CurrentCulture)
            {
                LocalVariables = localVariables,
                SyntaxCompatibilityLevel = syntax,
                Filters = localFilters
            };
            Assert.AreEqual(expected, Template.Parse(template).Render(parameters));
        }

        public static void AssertTemplateResult(string expected, string template, Hash localVariables, SyntaxCompatibility syntax = SyntaxCompatibility.DotLiquid20)
        {
            AssertTemplateResult(expected: expected, template: template, localVariables: localVariables, localFilters: null, syntax: syntax);
        }

        public static void AssertTemplateResult(string expected, string template, SyntaxCompatibility syntax = SyntaxCompatibility.DotLiquid20)
        {
            AssertTemplateResult(expected: expected, template: template, localVariables: null, syntax: syntax);
        }

        [LiquidTypeAttribute("PropAllowed")]
        public class DataObject
        {
            public string PropAllowed { get; set; }
            public string PropDisallowed { get; set; }
        }

        public class DataObjectRegistered
        {
            static DataObjectRegistered()
            {
                Template.RegisterSafeType(typeof(DataObjectRegistered), new[] { "PropAllowed" });
            }
            public string PropAllowed { get; set; }
            public string PropDisallowed { get; set; }
        }

        public class DataObjectDrop : Drop
        {
            public string Prop { get; set; }
        }
    }
}
