using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ShopifyFiltersTests
    {
        [Test]
        public void TestMd5()
        {
           ClassicAssert.AreEqual(null, ShopifyFilters.Md5(null));
           ClassicAssert.AreEqual("d41d8cd98f00b204e9800998ecf8427e", ShopifyFilters.Md5(""));
           ClassicAssert.AreEqual(
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
           ClassicAssert.AreEqual(null, ShopifyFilters.Sha1(null));
           ClassicAssert.AreEqual("da39a3ee5e6b4b0d3255bfef95601890afd80709", ShopifyFilters.Sha1(""));
           ClassicAssert.AreEqual(
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
           ClassicAssert.AreEqual(null, ShopifyFilters.Sha256(null));
           ClassicAssert.AreEqual("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", ShopifyFilters.Sha256(""));
           ClassicAssert.AreEqual(
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
           ClassicAssert.AreEqual(null, ShopifyFilters.HmacSha1(null, null));
           ClassicAssert.AreEqual("", ShopifyFilters.HmacSha1("", null));
           ClassicAssert.AreEqual(null, ShopifyFilters.HmacSha1(null, ""));
           ClassicAssert.AreEqual("fbdb1d1b18aa6c08324b7d64b71fb76370690e1d", ShopifyFilters.HmacSha1("", ""));
           ClassicAssert.AreEqual(
                expected: "30ab3459e46e7b209b45dba8378fcbba67297304",
                actual: ShopifyFilters.HmacSha1("ShopifyIsAwesome!", "secret_key"));

            // Test for the ranges of UTF-8 characters for the secret-key
           ClassicAssert.AreEqual("e45659e65fef13dfa71554d14718718a080acb11", ShopifyFilters.HmacSha1("ShopifyIsAwesome!", "\u0000")); //NULL
           ClassicAssert.AreEqual("17434e86f6ed25cfcc31ab7901cdedee29c988da", ShopifyFilters.HmacSha1("ShopifyIsAwesome!", "\uDB40\uDDEF")); //VARIATION SELECTOR-256

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
           ClassicAssert.AreEqual(null, ShopifyFilters.HmacSha256(null, null));
           ClassicAssert.AreEqual("", ShopifyFilters.HmacSha256("", null));
           ClassicAssert.AreEqual(null, ShopifyFilters.HmacSha256(null, ""));
           ClassicAssert.AreEqual("b613679a0814d9ec772f95d778c35fc5ff1697c493715653c6c712144292c5ad", ShopifyFilters.HmacSha256("", ""));

            // NOTE: the Shopify sample incorrectly shows the hmac_sha1 response,
            // as reported in https://community.shopify.com/c/Technical-Q-A/Using-Liquid-hmac-sha256-filter/m-p/559613#M1019
           ClassicAssert.AreEqual(
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
    }
}
