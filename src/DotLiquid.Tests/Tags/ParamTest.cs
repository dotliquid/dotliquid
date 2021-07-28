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
        public void TestInitialize()
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
                expected: "Liquid syntax error: The specified SyntaxCompatibility in invalid, supported options are: DotLiquid20,DotLiquid21,DotLiquid22",
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

        [TestCase(7000, "", ExpectedResult = "¤7,000.00")] // "" = InvariantCulture
        [TestCase("7000", "en-US", ExpectedResult = "$7,000.00")]
        [TestCase(7000, "en-US", ExpectedResult = "$7,000.00")]
        [TestCase("6,72", "de-DE", ExpectedResult = "6,72 €")]
        [TestCase(6.72d, "de-DE", ExpectedResult = "6,72 €")]
        [TestCase(9.999d, "de-DE", ExpectedResult = "10,00 €")]
        [TestCase("7000.49999", "en-US", ExpectedResult = "$7,000.50")]
        [TestCase("7000,49999", "fr-FR", ExpectedResult = "7 000,50 €")]
        [TestCase(int.MaxValue, "fr-FR", ExpectedResult = "2 147 483 647,00 €")]
        [TestCase(long.MaxValue, "en-GB", ExpectedResult = "£9,223,372,036,854,775,807.00")]
        public string TestCultures(object @amount, string cultureValue)
        {
            using (CultureHelper.SetCulture("jp-JP")) // Pre-select a thread culture not required for the tests (jp-JP)
            {
                return Template.Parse("{% param culture=cultureValue%}{{ amount | currency }}")
                    .Render(localVariables: Hash.FromAnonymousObject(new { amount = @amount, cultureValue = cultureValue }));
            }
        }

        [Test]
        public void TestCulture_InvalidCulture()
        {
            // Ensure the default/thread culture is 'en-US'
            using (CultureHelper.SetCulture("en-US"))
            {
                Helper.AssertTemplateResult(
                    expected: "Liquid error: Culture is not supported. (Parameter 'name')\r\nxxx-YYY is an invalid culture identifier.$7,000.00",
                    template: "{% param culture='xxx-YYY'%}{{ 7000 | currency }}"); // Unknown culture
            }
        }
    }
}