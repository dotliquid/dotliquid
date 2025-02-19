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
        [Test]
        public void TestInitialize_SyntaxValidation()
        {
            var tokens = new List<string>();
            Assert.Throws<SyntaxException>(() => new Param().Initialize(tagName: "param", markup: "", tokens: tokens));
            Assert.Throws<SyntaxException>(() => new Param().Initialize(tagName: "param", markup: "   ", tokens: tokens));
            Assert.Throws<SyntaxException>(() => new Param().Initialize(tagName: "param", markup: "date_format", tokens: tokens));
            Assert.Throws<SyntaxException>(() => new Param().Initialize(tagName: "param", markup: "date_format=", tokens: tokens));
            Assert.Throws<SyntaxException>(() => new Param().Initialize(tagName: "param", markup: "useDotNet='true'", tokens: tokens));
        }

        [TestCase("date_format='unknown'")] // unknown is not a valid date_format
        [TestCase("syntax='UnknownValue'")] // UnknownValue is not a valid syntax version
        [TestCase("using='DotLiquid.ShopifyFilters'")] // Fully qualified class names are invalid (even if they match a safelisted Type)
        [TestCase("using='DotLiquid.Template'")] // Fully qualified class names are invalid
        [TestCase("using='Template'")] // Ensure classes in the DotLiquid namespace are not available by accident.
        [TestCase("Syntax = 'DotLiquid21' | replace: '21', '10'")] // Filters is not valid in this tag
        public void TestInvalidOptions(string markup)
        {
            var tag = new Param();
            tag.Initialize(tagName: "param", markup: markup, tokens: null);

            var context = new Context(new CultureInfo("en-US"));
            Assert.Throws<SyntaxException>(() => tag.Render(context, new StringWriter()));
        }

        [TestCase("param Syntax='DotLiquid21'")]
        [TestCase(" param Syntax= 'DotLiquid21'")]
        [TestCase("param Syntax= 'DotLiquid21'")]
        [TestCase("param Syntax= 'DotLiquid21' ")]
        [TestCase("param Syntax = 'DotLiquid21' ")]
        public void TestSyntaxCompatibility(string markup)
        {
            // Initialize as DotLiquid20, then assert that the DotLiquid21 rules for Capitalize are followed.
            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{%" + markup + "%}{{ 'my great title' | capitalize }}",
                syntax: SyntaxCompatibility.DotLiquid20);
        }

        [TestCase("param Syntax=LiquidVersion")]
        [TestCase("param Syntax = LiquidVersion ")]
        public void TestSyntaxCompatibilityDynamic(string markup)
        {
            // Initialize as DotLiquid20, then assert that the DotLiquid21 rules for Capitalize are followed.
            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{% assign LiquidVersion = 'DotLiquid21'%}{%" + markup + "%}{{ 'my great title' | capitalize }}",
                syntax: SyntaxCompatibility.DotLiquid20);
        }

        [Test]
        public void TestDateFormats()
        {
            Helper.AssertTemplateResult(
                expected: ".NET=2020, Ruby=2020, .NET=2020",
                template: ".NET={{sourceDate | date: 'yyyy'}}{%param date_format='ruBy'%}, Ruby={{sourceDate | date: '%Y'}}{%param DATEFORMAT = 'dotnet'%}, .NET={{sourceDate | date: 'yyyy'}}",
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

                Helper.AssertTemplateResult(
                    expected: "Before=$1,000.50, After=¤1,000.50",
                    template: "Before={{ amount | currency }}{% param culture=cultureValue%}, After={{ amount | currency }}",
                    localVariables: Hash.FromAnonymousObject(new { amount = 1000.4999d, cultureValue = "" }) // ""=InvariantCulture
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
                );
        }
    }
}