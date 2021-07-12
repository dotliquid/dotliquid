using System.Collections;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ShopifyFiltersTests
    {
        [Test]
        public void TestMd5()
        {
            Helper.AssertTemplateResult(
                expected: @"<img src=""https://www.gravatar.com/avatar/80846a33ae3e3603c1c5d6ce72834924"" />",
                template: @"<img src=""https://www.gravatar.com/avatar/{{ comment.email | remove: ' ' | strip_newlines | downcase | md5 }}"" />",
                localVariables: Hash.FromAnonymousObject(new { comment = new { email = " Joe.Bloggs@Shopify.com " } }));
        }

        [Test]
        public void TestSha1()
        {
            Assert.AreEqual(null, ShopifyFilters.Sha1(null));
            Assert.AreEqual("", ShopifyFilters.Sha1(""));
            Assert.AreEqual(" ", ShopifyFilters.Sha1(" "));
            Assert.AreEqual("c7322e3812d3da7bc621300ca1797517c34f63b6",
                ShopifyFilters.Sha1("ShopifyIsAwesome!"));

            Helper.AssertTemplateResult(
                expected: "\r\nMy encoded string is: c7322e3812d3da7bc621300ca1797517c34f63b6",
                template: @"{% assign my_secret_string = ""ShopifyIsAwesome!"" | sha1 %}
My encoded string is: {{ my_secret_string }}"
                );
        }

        [Test]
        public void TestSha256()
        {
            Assert.AreEqual(null, ShopifyFilters.Sha256(null));
            Assert.AreEqual("", ShopifyFilters.Sha256(""));
            Assert.AreEqual(" ", ShopifyFilters.Sha256(" "));
            Assert.AreEqual("c29cce758876791f34b8a1543f0ec3f8e886b5271004d473cfe75ac3148463cb",
                ShopifyFilters.Sha256("ShopifyIsAwesome!"));

            Helper.AssertTemplateResult(
                expected: "\r\nMy encoded string is: c29cce758876791f34b8a1543f0ec3f8e886b5271004d473cfe75ac3148463cb",
                template: @"{% assign my_secret_string = ""ShopifyIsAwesome!"" | sha256 %}
My encoded string is: {{ my_secret_string }}"
                );
        }

        [Test]
        public void TestHmacSha1()
        {
            Assert.AreEqual(null, ShopifyFilters.HmacSha1(null, null));
            Assert.AreEqual("", ShopifyFilters.HmacSha1("", null));
            Assert.AreEqual(" ", ShopifyFilters.HmacSha1(" ", null));
            Assert.AreEqual(null, ShopifyFilters.HmacSha1(null, ""));
            Assert.AreEqual("", ShopifyFilters.HmacSha1("", ""));
            Assert.AreEqual("", ShopifyFilters.HmacSha1("", " "));
            Assert.AreEqual("30ab3459e46e7b209b45dba8378fcbba67297304",
                ShopifyFilters.HmacSha1("ShopifyIsAwesome!", "secret_key"));

            Helper.AssertTemplateResult(
                expected: "\r\nMy encoded string is: 30ab3459e46e7b209b45dba8378fcbba67297304",
                template: @"{% assign my_secret_string = ""ShopifyIsAwesome!"" | hmac_sha1: ""secret_key"" %}
My encoded string is: {{ my_secret_string }}"
                );
        }

        [Test]
        public void TestHmacSha256()
        {
            Assert.AreEqual(null, ShopifyFilters.HmacSha256(null, null));
            Assert.AreEqual("", ShopifyFilters.HmacSha256("", null));
            Assert.AreEqual(" ", ShopifyFilters.HmacSha256(" ", null));
            Assert.AreEqual(null, ShopifyFilters.HmacSha256(null, ""));
            Assert.AreEqual("", ShopifyFilters.HmacSha256("", ""));
            Assert.AreEqual("", ShopifyFilters.HmacSha256("", " "));

            // NOTE: the Shopify sample incorrectly shows the hmac_sha1 response,
            // as reported in https://community.shopify.com/c/Technical-Q-A/Using-Liquid-hmac-sha256-filter/m-p/559613#M1019
            Assert.AreEqual("c21f97cf997fac667c9bac39462a5813b1a41ce1b811743b0e9157393efbcc3c",
                ShopifyFilters.HmacSha256("ShopifyIsAwesome!", "secret_key"));

            Helper.AssertTemplateResult(
                expected: "\r\nMy encoded string is: c21f97cf997fac667c9bac39462a5813b1a41ce1b811743b0e9157393efbcc3c",
                template: @"{% assign my_secret_string = ""ShopifyIsAwesome!"" | hmac_sha256: ""secret_key"" %}
My encoded string is: {{ my_secret_string }}"
                );
        }

#if NETSTANDARD2_0
        [TestCase("Hello World!", "\"Hello World!\"")]
        [TestCase("\"", "\"\\u0022\"")]
        [TestCase("'", "\"\\u0027\"")]
        [TestCase(123, "123")]
        [TestCase(123.12, "123.12")]
        [TestCase(-123.12, "-123.12")]
        [TestCase(null, null)]
        [TestCase("", "\"\"")]
        public void TestJson(object value, string expected)
        {
            Assert.AreEqual(expected, ShopifyFilters.Json(value));
        }
#endif

#if NETSTANDARD2_0
        [TestCase(new int[] { 1, 2, 3 }, "[1,2,3]")]
        [TestCase(new string[] { "a", "b", "c" }, "[\"a\",\"b\",\"c\"]")]
        [TestCase(new object[0], "[]")]
        [TestCase(new object[] { 1, "a", true }, "[1,\"a\",true]")]
        [TestCase(new string[] { "a", "b", "c" }, "[\"a\",\"b\",\"c\"]")]
        public void TestJsonCollections(object value, string expected)
        {
            var result = ShopifyFilters.Json(value);
            Assert.IsInstanceOf<IEnumerable>(result);
            CollectionAssert.AreEqual(expected, (IEnumerable)result);
        }
#endif

#if NETSTANDARD2_0
        [Test]
        public void TestJson_ShopifyCollectionsSample()
        {
            var anonymousCollections = new
            {
                collections = new
                {
                    featured = new
                    {
                        products = new[]
                        {
                            new { title = "Shopify", category = "business" },
                            new { title = "Rihanna", category = "celebrities" },
                            new { title = "foo", category = null as string },
                            new { title = "World traveller", category = "lifestyle" },
                            new { title = "Soccer", category = "sports" },
                            new { title = "foo", category = null as string },
                            new { title = "Liquid", category = "technology" },
                        }
                    }
                }
            };

            // Shopify Sample: `var json_product = {{ collections.featured.products.first | json }};`
            Helper.AssertTemplateResult(
                expected: "{\"title\":\"Shopify\",\"category\":\"business\"}",
                template: "{{ collections.featured.products.first | json }}",
                localVariables: Hash.FromAnonymousObject(anonymousCollections)
                );
        }
#endif

#if NETSTANDARD2_0
        [Test]
        public void TestJson_ShopifyCartSample()
        {
            // A cart sample with bool, int, float and string values.
            var anonymousCart = new
            {
                cart = new
                {
                    loggedIn = false,
                    total = 19.98f,
                    items = new[]
                    {
                        new {sku="njkdw82", unitPrice=9.99f, quantity=2}
                    }
                }
            };

            // Sample: `var json_cart = {{ cart | json }};`
            Helper.AssertTemplateResult(
                expected: "{\"loggedIn\":false,\"total\":19.98,\"items\":[{\"sku\":\"njkdw82\",\"unitPrice\":9.99,\"quantity\":2}]}",
                template: "{{ cart | json }}",
                localVariables: Hash.FromAnonymousObject(anonymousCart));
        }
#endif
    }
}
