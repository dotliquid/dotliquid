using System;
using System.Collections.Generic;
using System.Reflection;
using DotLiquid.FileSystems;
using DotLiquid.NamingConventions;
using DotLiquid.Tests.Util;
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
            var currentFileSystem = Template.FileSystem;
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
                    Template.FileSystem = currentFileSystem;
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
                Assert.That(Template.Parse(template).Render(parameters), Is.EqualTo(expected));
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
            Assert.That(Template.Parse(template).Render(parameters), Is.EqualTo(expected));
        }

        public static void AssertTemplateResult(string expected, string template, Hash localVariables, SyntaxCompatibility syntax = SyntaxCompatibility.DotLiquid20)
        {
            AssertTemplateResult(expected: expected, template: template, localVariables: localVariables, localFilters: null, syntax: syntax);
        }

        public static void AssertTemplateResult(string expected, string template, SyntaxCompatibility syntax = SyntaxCompatibility.DotLiquid20)
        {
            AssertTemplateResult(expected: expected, template: template, localVariables: null, syntax: syntax);
        }

        public static void WithCustomTag<T>(string tagName, Action action) where T : Tag, new()
        {
            Type tagType = Template.UnregisterTag(tagName);
            try
            {
                Template.RegisterTag<T>(tagName);
                action();
            }
            finally
            {
                Template.UnregisterTag(tagName);
                if (tagType != null)
                {
                    // Call RegisterTag with the original tagType to restore the original tag
                    typeof(Template)
                        .GetMethod("RegisterTag")
                        .MakeGenericMethod(tagType)
                        .Invoke(null, new object[] { tagName });
                }
            }
        }

        public static void WithFileSystem(IFileSystem fs, Action action)
        {
            var oldFileSystem = Template.FileSystem;
            Template.FileSystem = fs;
            try
            {
                action.Invoke();
            }
            finally
            {
                Template.FileSystem = oldFileSystem;
            }
        }

        public static void WithDictionaryFileSystem(IDictionary<string, string> data, Action action)
        {
            WithFileSystem(new DictionaryFileSystem(data), action);
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
