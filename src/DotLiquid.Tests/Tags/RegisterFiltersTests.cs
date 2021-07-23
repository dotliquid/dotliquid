using System.Globalization;
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
            // addfilter is not included, so verify the value is not hashed.
            Helper.AssertTemplateResult(
                expected: @"
My encoded string is: ShopifyIsAwesome!",
                template: @"{% assign my_secret_string = ""ShopifyIsAwesome!"" | sha1 %}
My encoded string is: {{ my_secret_string }}");
        }

        [Test]
        public void TestWhitelisted()
        {
            // addfilter is included, so ensure the value is sha1 hashed.
            Helper.AssertTemplateResult(
                expected: @"
My encoded string is: c7322e3812d3da7bc621300ca1797517c34f63b6",
                template: @"{%addfilters 'shopifyFilters'%}{% assign my_secret_string = ""ShopifyIsAwesome!"" | sha1 %}
My encoded string is: {{ my_secret_string }}");
        }

        [TestCase("'DotLiquid.ShopifyFilters'")]
        [TestCase("'DotLiquid.Template'")]
        [TestCase("'Template'")]
        public void TestNotWhitelisted(string template)
        {
            var filter = new AddFilters();
            filter.Initialize("addfilters", "'DotLiquid.ShopifyFilters'", null);
            Assert.Throws<FilterNotFoundException>(() => filter.Render(new Context(CultureInfo.InvariantCulture), null));
        }
    }
}