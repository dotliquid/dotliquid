using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DotLiquid.Exceptions;
using DotLiquid.Tags;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class ParamTests
    {
        private Context _contextV20;

        [OneTimeSetUp]
        public void SetUp()
        {
            _contextV20 = new Context(new CultureInfo("jp-JP")) // Pre-select a langauge not required for any tests (jp-JP)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid20
            };
        }

        [Test]
        public void TestInitialize_SyntaxValidation()
        {
            var tokens = new List<string>();
            Assert.Throws<SyntaxException>(() => new Param().Initialize(tagName: "param", markup: "", tokens: tokens));
            Assert.Throws<SyntaxException>(() => new Param().Initialize(tagName: "param", markup: "   ", tokens: tokens));
            Assert.Throws<SyntaxException>(() => new Param().Initialize(tagName: "param", markup: "date_format", tokens: tokens));
            Assert.Throws<SyntaxException>(() => new Param().Initialize(tagName: "param", markup: "date_format=", tokens: tokens));
        }

        [Test]
        public void TestInvalidOptions()
        {
            Helper.AssertTemplateResult(
                expected: "Liquid syntax error: An unsupported parameter was passed to the Param tag: 'useCSharp'",
                template: "{% param useCSharp='true'%}{{ test }}");
        }

        [Test]
        public void TestSyntaxCompatibility()
        {
            // Initialise as DotLiquid20, then assert that the DotLiquid21 rules for Capitalize are followed.
            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{% param Syntax= 'DotLiquid21'%}{{ 'my great title' | capitalize }}",
                syntax: SyntaxCompatibility.DotLiquid20);
        }

        [Test]
        public void TestSyntaxCompatibility_InvalidOption()
        {
            Helper.AssertTemplateResult(
                expected: "Liquid syntax error: The SyntaxCompatibility 'UnknownValue' is invalid, supported options are: " + string.Join(",", System.Enum.GetNames(typeof(SyntaxCompatibility))),
                template: "{% param syntax='UnknownValue'%}");
        }

        [TestCase("date_format='ruby'", ExpectedResult = true)]
        [TestCase("date_format = 'cSharp'", ExpectedResult = false)]
        [TestCase("date_format='unknown'", ExpectedResult = false)]
        [TestCase("date_format=' '", ExpectedResult = false)]
        public bool TestDateFormat(string markup)
        {
            var param = new Param();
            param.Initialize(tagName: "param", markup: markup, tokens: null);
            param.Render(_contextV20, new StringWriter());
            return _contextV20.UseRubyDateFormat;
        }

        [Test]
        public void TestRubyDates()
        {
            Helper.AssertTemplateResult(
                expected: "C#=2020, Ruby=2020, C#=2020",
                template: "C#={{sourceDate | date: 'yyyy'}}{%param date_format='ruBy'%}, Ruby={{sourceDate | date: '%Y'}}{%param DATE_FORMAT='csharp'%}, C#={{sourceDate | date: 'yyyy'}}",
                localVariables: Hash.FromAnonymousObject(new { sourceDate = "2020-02-03T12:13:14Z" }));
        }

        [Test]
        public void TestCulture()
        {
            using (CultureHelper.SetCulture("en-US")) // Pre-select a thread culture for Before value (jp-JP)
            {
                Helper.AssertTemplateResult(
                    expected: "Before=$1,000.50, After=£1,000.50",
                    template: "Before={{ amount | currency }}{% param culture=cultureValue%}, After={{ amount | currency }}",
                    localVariables: Hash.FromAnonymousObject(new { amount = 1000.4999d, cultureValue = "en-GB" })
                );
            }
        }

        [Test]
        public void TestCulture_InvalidCulture()
        {
            // Ensure the default/thread culture is 'en-US'
            using (CultureHelper.SetCulture("en-US"))
            {
                Helper.AssertTemplateResult(
                    expected: "Liquid syntax error: Culture 'xxx-YYY' is not supported$7,000.00",
                    template: "{% param culture='xxx-YYY'%}{{ 7000 | currency }}"); // Unknown culture
            }
        }

        [Test]
        public void TestUsing()
        {
            // using param is not included, so verify the value is not hashed.
            Helper.AssertTemplateResult(
                expected: @"
Before: ShopifyIsAwesome!
After:  c7322e3812d3da7bc621300ca1797517c34f63b6",
                template: @"
Before: {{ 'ShopifyIsAwesome!' | sha1 }}
{%-param using='ShopifyFilters'-%}
After:  {{ 'ShopifyIsAwesome!' | sha1 }}"
                // ,localVariables: Hash.FromDictionary(dictionary)
                );
        }

        [TestCase("'DotLiquid.ShopifyFilters'")] // Fully qualified class names are invalid (even if they match a safelisted Type)
        [TestCase("'DotLiquid.Template'")] // Fully qualified class names are invalid
        [TestCase("'Template'")] // Classes in the DotLiquid namespace are not available by default
        public void TestUsing_NotSafelisted(string template)
        {
            var filter = new Param();
            filter.Initialize("param", "using=" + template, null);
            Assert.Throws<FilterNotFoundException>(() => filter.Render(new Context(CultureInfo.InvariantCulture), null));
        }
    }
}