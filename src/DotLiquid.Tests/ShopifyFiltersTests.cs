using System.Collections;
using System.Globalization;
using DotLiquid.Exceptions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ShopifyFiltersTests
    {
        private Context _context;

        [OneTimeSetUp]
        public void SetUp()
        {
            _context = new Context(CultureInfo.InvariantCulture)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22a
            };
        }

        [Test]
        public void TestMd5()
        {
            Assert.AreEqual(null, ShopifyFilters.Md5(null));
            Assert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", ShopifyFilters.Md5(""));
            Assert.AreEqual(
                expected: "11de0bf2a16fdb9d4f3780a0d2fd95c7",
                actual: ShopifyFilters.Md5("ShopifyIsAwesome!"));

            Helper.AssertTemplateResult(
                expected: @"<img src=""https://www.gravatar.com/avatar/80846a33ae3e3603c1c5d6ce72834924"" />",
                template: @"<img src=""https://www.gravatar.com/avatar/{{ comment.email | remove: ' ' | strip_newlines | downcase | md5 }}"" />",
                localVariables: Hash.FromAnonymousObject(new { comment = new { email = " Joe.Bloggs@Shopify.com " } }),
                localFilters: new[] { typeof(ShopifyFilters) });
        }

        [Test]
        public void TestSha1()
        {
            Assert.AreEqual(null, ShopifyFilters.Sha1(null));
            Assert.AreEqual("da39a3ee5e6b4b0d3255bfef95601890afd80709", ShopifyFilters.Sha1(""));
            Assert.AreEqual(
                expected: "c7322e3812d3da7bc621300ca1797517c34f63b6",
                actual: ShopifyFilters.Sha1("ShopifyIsAwesome!"));

            Helper.AssertTemplateResult(
                expected: @"
My encoded string is: c7322e3812d3da7bc621300ca1797517c34f63b6",
                template: @"{% assign my_secret_string = ""ShopifyIsAwesome!"" | sha1 %}
My encoded string is: {{ my_secret_string }}",
                localVariables: null,
                localFilters: new[] { typeof(ShopifyFilters) });
        }

        [Test]
        public void TestSha256()
        {
            Assert.AreEqual(null, ShopifyFilters.Sha256(null));
            Assert.AreEqual("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", ShopifyFilters.Sha256(""));
            Assert.AreEqual(
                expected: "c29cce758876791f34b8a1543f0ec3f8e886b5271004d473cfe75ac3148463cb",
                actual: ShopifyFilters.Sha256("ShopifyIsAwesome!"));

            Helper.AssertTemplateResult(
                expected: @"
My encoded string is: c29cce758876791f34b8a1543f0ec3f8e886b5271004d473cfe75ac3148463cb",
                template: @"{% assign my_secret_string = ""ShopifyIsAwesome!"" | sha256 %}
My encoded string is: {{ my_secret_string }}",
                localVariables: null,
                localFilters: new[] { typeof(ShopifyFilters) });
        }

        [Test]
        public void TestHmacSha1()
        {
            Assert.AreEqual(null, ShopifyFilters.HmacSha1(null, null));
            Assert.AreEqual("", ShopifyFilters.HmacSha1("", null));
            Assert.AreEqual(null, ShopifyFilters.HmacSha1(null, ""));
            Assert.AreEqual("fbdb1d1b18aa6c08324b7d64b71fb76370690e1d", ShopifyFilters.HmacSha1("", ""));
            Assert.AreEqual(
                expected: "30ab3459e46e7b209b45dba8378fcbba67297304",
                actual: ShopifyFilters.HmacSha1("ShopifyIsAwesome!", "secret_key"));

            // Test for the ranges of UTF-8 characters for the secret-key
            Assert.AreEqual("e45659e65fef13dfa71554d14718718a080acb11", ShopifyFilters.HmacSha1("ShopifyIsAwesome!", "\u0000")); //NULL
            Assert.AreEqual("17434e86f6ed25cfcc31ab7901cdedee29c988da", ShopifyFilters.HmacSha1("ShopifyIsAwesome!", "\uDB40\uDDEF")); //VARIATION SELECTOR-256

            Helper.AssertTemplateResult(
                expected: @"
My encoded string is: 30ab3459e46e7b209b45dba8378fcbba67297304",
                template: @"{% assign my_secret_string = ""ShopifyIsAwesome!"" | hmac_sha1: ""secret_key"" %}
My encoded string is: {{ my_secret_string }}",
                localVariables: null,
                localFilters: new[] { typeof(ShopifyFilters) });
        }

        [Test]
        public void TestHmacSha256()
        {
            Assert.AreEqual(null, ShopifyFilters.HmacSha256(null, null));
            Assert.AreEqual("", ShopifyFilters.HmacSha256("", null));
            Assert.AreEqual(null, ShopifyFilters.HmacSha256(null, ""));
            Assert.AreEqual("b613679a0814d9ec772f95d778c35fc5ff1697c493715653c6c712144292c5ad", ShopifyFilters.HmacSha256("", ""));

            // NOTE: the Shopify sample incorrectly shows the hmac_sha1 response,
            // as reported in https://community.shopify.com/c/Technical-Q-A/Using-Liquid-hmac-sha256-filter/m-p/559613#M1019
            Assert.AreEqual(
                expected: "c21f97cf997fac667c9bac39462a5813b1a41ce1b811743b0e9157393efbcc3c",
                actual: ShopifyFilters.HmacSha256("ShopifyIsAwesome!", "secret_key"));

            Helper.AssertTemplateResult(
                expected: @"
My encoded string is: c21f97cf997fac667c9bac39462a5813b1a41ce1b811743b0e9157393efbcc3c",
                template: @"{% assign my_secret_string = ""ShopifyIsAwesome!"" | hmac_sha256: ""secret_key"" %}
My encoded string is: {{ my_secret_string }}",
                localVariables: null,
                localFilters: new[] { typeof(ShopifyFilters) });
        }

        [Test]
        public void TestCamelize()
        {
            Assert.AreEqual(null, ShopifyFilters.Camelize(_context, null));
            Assert.AreEqual("VariableName", ShopifyFilters.Camelize(_context, "variable-name"));
            Assert.AreEqual("VariableName", ShopifyFilters.Camelize(_context, "Variable Name"));
            Assert.AreEqual("Foobar", ShopifyFilters.Camelize(_context, "FooBar"));
            Assert.AreEqual("", ShopifyFilters.Camelize(_context, ""));

            Helper.AssertTemplateResult(
                expected: "VariableName",
                template: "{{ 'variable-name' | camelize }}",
                localVariables: null,
                localFilters: new[] { typeof(ShopifyFilters) });
        }

        [Test]
        public void TestPluralize()
        {
            Assert.AreEqual("item", ShopifyFilters.Pluralize(1, "item", "items"));
            Assert.AreEqual("items", ShopifyFilters.Pluralize(2, "item", "items"));
            Assert.AreEqual("item", ShopifyFilters.Pluralize("1", "item", "items"));
            Assert.AreEqual("items", ShopifyFilters.Pluralize("2", "item", "items"));
            Assert.AreEqual(null, ShopifyFilters.Pluralize(null, null, null));
            Assert.AreEqual(null, ShopifyFilters.Pluralize(1, null, "items"));
            Assert.AreEqual(null, ShopifyFilters.Pluralize(2, "item", null));
            Assert.AreEqual(null, ShopifyFilters.Pluralize("invalid_number_string", "item", "items"));

            Helper.AssertTemplateResult(
                expected: "Cart item count: 2 items",
                template: "Cart item count: {{ cart.item_count }} {{ cart.item_count | pluralize: 'item', 'items' }}",
                localVariables: Hash.FromAnonymousObject(new { cart = new { item_count = 2 }}),
                localFilters: new[] { typeof(ShopifyFilters) });
        }

        [Test]
        public void TestUrlEscape()
        {
            Assert.AreEqual(null, ShopifyFilters.UrlEscape(null));
            Assert.AreEqual("foo", ShopifyFilters.UrlEscape("foo"));
            Assert.AreEqual("%3Cp%3EHealth%20&%20Love%20potions%3C/p%3E", ShopifyFilters.UrlEscape("<p>Health & Love potions</p>"));

            Helper.AssertTemplateResult(
                expected: "%3Cp%3EHealth%20&%20Love%20potions%3C/p%3E",
                template: "{{ '<p>Health & Love potions</p>' | url_escape }}",
                localVariables: null,
                localFilters: new[] { typeof(ShopifyFilters) });
        }

        [Test]
        public void TestBase64Encode()
        {
            Assert.AreEqual(null, ShopifyFilters.Base64Encode(null));
            Assert.AreEqual("", ShopifyFilters.Base64Encode(""));
            Assert.AreEqual("b25lIHR3byB0aHJlZQ==", ShopifyFilters.Base64Encode("one two three"));

            Helper.AssertTemplateResult(
                expected: "b25lIHR3byB0aHJlZQ==",
                template: "{{ 'one two three' | base64_encode }}",
                localVariables: null,
                localFilters: new[] { typeof(ShopifyFilters) });
        }

        [Test]
        public void TestBase64Decode()
        {
            Assert.AreEqual(null, ShopifyFilters.Base64Decode(null));
            Assert.AreEqual("", ShopifyFilters.Base64Decode(""));
            Assert.Throws<ArgumentException>(() => ShopifyFilters.Base64Decode("invalid base64 string"));
            Assert.AreEqual("one two three", ShopifyFilters.Base64Decode("b25lIHR3byB0aHJlZQ=="));

            Helper.AssertTemplateResult(
                expected: "one two three",
                template: "{{ 'b25lIHR3byB0aHJlZQ==' | base64_decode }}",
                localVariables: null,
                localFilters: new[] { typeof(ShopifyFilters) });
        }

        [Test]
        public void TestBase64UrlSafeEncode()
        {
            Assert.AreEqual(null, ShopifyFilters.Base64UrlSafeEncode(null));
            Assert.AreEqual("", ShopifyFilters.Base64UrlSafeEncode(""));
            Assert.AreEqual("b25lIHR3byB0aHJlZQ==", ShopifyFilters.Base64UrlSafeEncode("one two three"));
            Assert.AreEqual("V2hhdCBkb2VzIDIgKyAyLjEgZXF1YWw_PyB-IDQ=", ShopifyFilters.Base64UrlSafeEncode("What does 2 + 2.1 equal?? ~ 4"));

            Helper.AssertTemplateResult(
                expected: "b25lIHR3byB0aHJlZQ==",
                template: "{{ 'one two three' | base64_url_safe_encode }}",
                localVariables: null,
                localFilters: new[] { typeof(ShopifyFilters) });
        }

        [Test]
        public void TestBase64UrlSafeDecode()
        {
            Assert.AreEqual(null, ShopifyFilters.Base64UrlSafeDecode(null));
            Assert.AreEqual("", ShopifyFilters.Base64UrlSafeDecode(""));
            Assert.AreEqual("one two three", ShopifyFilters.Base64UrlSafeDecode("b25lIHR3byB0aHJlZQ=="));
            Assert.AreEqual("What does 2 + 2.1 equal?? ~ 4", ShopifyFilters.Base64UrlSafeDecode("V2hhdCBkb2VzIDIgKyAyLjEgZXF1YWw_PyB-IDQ="));
            Assert.Throws<ArgumentException>(() => ShopifyFilters.Base64UrlSafeDecode("invalid base64 string"));

            Helper.AssertTemplateResult(
                expected: "one two three",
                template: "{{ 'b25lIHR3byB0aHJlZQ==' | base64_url_safe_decode }}",
                localVariables: null,
                localFilters: new[] { typeof(ShopifyFilters) });
        }
    }
}
