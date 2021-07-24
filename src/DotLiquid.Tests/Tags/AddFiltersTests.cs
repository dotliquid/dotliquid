using System.Globalization;
using System.Collections.Generic;
using DotLiquid.Exceptions;
using DotLiquid.Tags;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class AddFiltersTests
    {
        [Test]
        public void TestInitialise()
        {
            Assert.Throws<SyntaxException>(() => new AddFilters().Initialize("addfilters", "", null)); // empty markup
            Assert.Throws<SyntaxException>(() => new AddFilters().Initialize("addfilters", "   ", null)); // whitespace markup
            Assert.Throws<SyntaxException>(() => new AddFilters().Initialize("addfilters", "ShopifyFilters", null)); // Not quoted
        }

        [Test]
        public void TestWithoutAddFilter()
        {
            var dictionary = new Dictionary<string, object> { { "a_value_to_be_hashed", "ShopifyIsAwesome!" } };

            // addfilter is not included, so verify the value is not hashed.
            Helper.AssertTemplateResult(
                expected: @"
Before: ShopifyIsAwesome!
After:  c7322e3812d3da7bc621300ca1797517c34f63b6",
                template: @"
Before: {{ a_value_to_be_hashed | sha1 }}
{%- addfilters 'ShopifyFilters' -%}
After:  {{ a_value_to_be_hashed | sha1 }}",
                localVariables: Hash.FromDictionary(dictionary));
        }

        [TestCase("'ShopifyFilters'")] // correct case, single quoted
        [TestCase("\"ShopifyFilters\"")] // correct case, double quoted
        [TestCase("'shopifyfilters'")] // lower case
        [TestCase("'SHOPIFYFILTERS'")] // upper case
        public void TestWhitelisted(string aliasLiteral)
        {
            // addfilter is included, so ensure the value is sha1 hashed.
            Helper.AssertTemplateResult(
                expected: @"
My encoded string is: c7322e3812d3da7bc621300ca1797517c34f63b6",
                template: @"{%addfilters " + aliasLiteral + @"%}{% assign my_secret_string = ""ShopifyIsAwesome!"" | sha1 %}
My encoded string is: {{ my_secret_string }}");
        }

        [TestCase("'DotLiquid.ShopifyFilters'")] // Fully qualified class names are invalid (even if they match a whitelisted Type)
        [TestCase("'DotLiquid.Template'")] // Fully qualified class names are invalid
        [TestCase("'Template'")] // Classes in the DotLiquid namespace are not available by default
        public void TestNotWhitelisted(string template)
        {
            var filter = new AddFilters();
            filter.Initialize("addfilters", template, null);
            Assert.Throws<FilterNotFoundException>(() => filter.Render(new Context(CultureInfo.InvariantCulture), null));
        }
    }
}