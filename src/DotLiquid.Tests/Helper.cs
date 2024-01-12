using System;
using System.Collections.Generic;
using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    public class Helper
    {
        public static void LockTemplateStaticVars(Action test)
        {
            //Have to lock Template.FileSystem for this test to
            //prevent other tests from being run simultaneously that
            //require the default naming convention.
            var currentSyntax = Template.DefaultSyntaxCompatibilityLevel;
            var currentIsRubyDateFormat = Liquid.UseRubyDateFormat;
            lock (Template.FileSystem)
            {
                try
                {
                    test();
                }
                finally
                {
                    Template.DefaultSyntaxCompatibilityLevel = currentSyntax;
                    Liquid.UseRubyDateFormat = currentIsRubyDateFormat;
                }
            }
        }


        public static void AssertTemplateResult(string expected, string template, object anonymousObject, INamingConvention namingConvention, SyntaxCompatibility syntax = SyntaxCompatibility.DotLiquid20)
        {
            namingConvention = namingConvention ?? new RubyNamingConvention();
            LockTemplateStaticVars(() =>
            {
                var localVariables = anonymousObject == null ? null : Hash.FromAnonymousObject(anonymousObject, namingConvention);
                var parameters = new RenderParameters(System.Globalization.CultureInfo.CurrentCulture)
                {
                    LocalVariables = localVariables,
                    SyntaxCompatibilityLevel = syntax
                };
                Assert.AreEqual(expected, Template.Parse(template, namingConvention).Render(parameters));
            });
        }

        public static void AssertTemplateResult(string expected, string template, INamingConvention namingConvention)
        {
            AssertTemplateResult(expected: expected, template: template, anonymousObject: null, namingConvention: namingConvention);
        }

        public static void AssertTemplateResult(string expected, string template, Hash localVariables, IEnumerable<Type> localFilters, SyntaxCompatibility syntax = SyntaxCompatibility.DotLiquid20, INamingConvention namingConvention = null)
        {
            namingConvention = namingConvention ?? new RubyNamingConvention();
            var parameters = new RenderParameters(System.Globalization.CultureInfo.CurrentCulture)
            {
                LocalVariables = localVariables,
                SyntaxCompatibilityLevel = syntax,
                Filters = localFilters
            };
            Assert.AreEqual(expected, Template.Parse(template, namingConvention).Render(parameters));
        }

        public static void AssertTemplateResult(string expected, string template, Hash localVariables, SyntaxCompatibility syntax = SyntaxCompatibility.DotLiquid20, INamingConvention namingConvention = null)
        {
            AssertTemplateResult(expected: expected, template: template, localVariables: localVariables, localFilters: null, syntax: syntax, namingConvention);
        }

        public static void AssertTemplateResult(string expected, string template, SyntaxCompatibility syntax = SyntaxCompatibility.DotLiquid20, INamingConvention namingConvention = null)
        {
            AssertTemplateResult(expected: expected, template: template, localVariables: null, syntax: syntax, namingConvention);
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
