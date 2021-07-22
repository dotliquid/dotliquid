using DotLiquid.Exceptions;
using DotLiquid.Tags;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class RegisterFiltersTests
    {
        [Test]
        public void TestInitialise()
        {
            new RegisterFilters().Initialize("register_filters", " DotLiquid.ShopifyFilters", null); // no quotes
            new RegisterFilters().Initialize("register_filters", "'DotLiquid.ShopifyFilters' ", null); // single quoted literal
            new RegisterFilters().Initialize("register_filters", " \"DotLiquid.ShopifyFilters\" ", null); // double  quoted literal

            Assert.Throws<SyntaxException>(() => new RegisterFilters().Initialize("register_filters", "", null)); // empty markup
            Assert.Throws<SyntaxException>(() => new RegisterFilters().Initialize("register_filters", "   ", null)); // whitespace markup
            Assert.Throws<FilterNotFoundException>(() => new RegisterFilters().Initialize("register_filters", "DotLiquid.NotARealClassName", null)); // Invalid class name
        }

        [Test]
        public void TestFilterNotAdded()
        {
            // The filter is not registered, so verify the value is not hashed.
            Helper.AssertTemplateResult(
                expected: @"
My encoded string is: ShopifyIsAwesome!",
                template: @"{% assign my_secret_string = ""ShopifyIsAwesome!"" | sha1 %}
My encoded string is: {{ my_secret_string }}");
        }

        [Test]
        public void TestFilterAdded()
        {
            // DotLiquid.ShopifyFilters is registered, so ensure the value is sha1 hashed.
            Helper.AssertTemplateResult(
                expected: @"
My encoded string is: c7322e3812d3da7bc621300ca1797517c34f63b6",
                template: @"{%register_filters 'DotLiquid.ShopifyFilters'%}{% assign my_secret_string = ""ShopifyIsAwesome!"" | sha1 %}
My encoded string is: {{ my_secret_string }}");
        }
    }
}