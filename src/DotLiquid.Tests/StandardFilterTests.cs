using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class StandardFilterTests
    {
        private Context _contextV20;
        private Context _contextV21;
        private Context _contextV22;
        private Context _contextV22a;

        [OneTimeSetUp]
        public void SetUp()
        {
            _contextV20 = new Context(CultureInfo.InvariantCulture)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid20
            };
            _contextV21 = new Context(CultureInfo.InvariantCulture)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid21
            };
            _contextV22 = new Context(CultureInfo.InvariantCulture)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22
            };
            _contextV22a = new Context(CultureInfo.InvariantCulture)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22a
            };
        }

        [Test]
        public void TestSize()
        {
            Assert.AreEqual(3, StandardFilters.Size(new[] { 1, 2, 3 }));
            Assert.AreEqual(0, StandardFilters.Size(new object[] { }));
            Assert.AreEqual(0, StandardFilters.Size(null));
        }

        [Test]
        public void TestDowncase()
        {
            Assert.AreEqual("testing", StandardFilters.Downcase("Testing"));
            Assert.AreEqual(null, StandardFilters.Downcase(null));
        }

        [Test]
        public void TestUpcase()
        {
            Assert.AreEqual("TESTING", StandardFilters.Upcase("Testing"));
            Assert.AreEqual(null, StandardFilters.Upcase(null));
        }

        [Test]
        public void TestTruncate()
        {
            Assert.AreEqual(expected: null, actual: StandardFilters.Truncate(null));
            Assert.AreEqual(expected: "", actual: StandardFilters.Truncate(""));
            Assert.AreEqual(expected: "1234...", actual: StandardFilters.Truncate("1234567890", 7));
            Assert.AreEqual(expected: "1234567890", actual: StandardFilters.Truncate("1234567890", 20));
            Assert.AreEqual(expected: "...", actual: StandardFilters.Truncate("1234567890", 0));
            Assert.AreEqual(expected: "1234567890", actual: StandardFilters.Truncate("1234567890"));
            Helper.AssertTemplateResult(expected: "H...", template: "{{ 'Hello' | truncate:4 }}");

            Helper.AssertTemplateResult(expected: "Ground control to...", template: "{{ \"Ground control to Major Tom.\" | truncate: 20}}");
            Helper.AssertTemplateResult(expected: "Ground control, and so on", template: "{{ \"Ground control to Major Tom.\" | truncate: 25, \", and so on\"}}");
            Helper.AssertTemplateResult(expected: "Ground control to Ma", template: "{{ \"Ground control to Major Tom.\" | truncate: 20, \"\"}}");
            Helper.AssertTemplateResult(expected: "...", template: "{{ \"Ground control to Major Tom.\" | truncate: 0}}");
            Helper.AssertTemplateResult(expected: "Liquid error: Value was either too large or too small for an Int32.", template: $"{{{{ \"Ground control to Major Tom.\" | truncate: {((long)int.MaxValue) + 1}}}}}");
            Helper.AssertTemplateResult(expected: "...", template: "{{ \"Ground control to Major Tom.\" | truncate: -1}}");
        }

        [Test]
        public void TestEscape()
        {
            Assert.AreEqual(null, StandardFilters.Escape(null));
            Assert.AreEqual("", StandardFilters.Escape(""));
            Assert.AreEqual("&lt;strong&gt;", StandardFilters.Escape("<strong>"));
            Assert.AreEqual("&lt;strong&gt;", StandardFilters.H("<strong>"));

            Helper.AssertTemplateResult(
                 expected: "Have you read &#39;James &amp; the Giant Peach&#39;?",
                 template: @"{{ ""Have you read 'James & the Giant Peach'?"" | escape }}");

            Helper.AssertTemplateResult(
                 expected: "Tetsuro Takara",
                 template: "{{ 'Tetsuro Takara' | escape }}");
        }

        [Test]
        public void TestEscapeOnce()
        {
            Assert.AreEqual(null, StandardFilters.EscapeOnce(null));
            Assert.AreEqual("", StandardFilters.EscapeOnce(""));
            Assert.AreEqual("&amp;xxx; looks like an escaped character, but isn&#39;t", StandardFilters.EscapeOnce("&xxx; looks like an escaped character, but isn't"));
            Assert.AreEqual("1 &lt; 2 &amp; 3", StandardFilters.EscapeOnce("1 &lt; 2 &amp; 3"));
            Assert.AreEqual("&lt;element&gt;1 &lt; 2 &amp; 3&lt;/element&gt;", StandardFilters.EscapeOnce("<element>1 &lt; 2 &amp; 3</element>"));

            Helper.AssertTemplateResult(
                 expected: "1 &lt; 2 &amp; 3",
                 template: "{{ '1 < 2 & 3' | escape_once }}");

            Helper.AssertTemplateResult(
                 expected: "1 &lt; 2 &amp; 3",
                 template: "{{ '1 &lt; 2 &amp; 3' | escape_once }}");
        }

        [Test]
        public void TestTruncateWords()
        {
            Assert.AreEqual(null, StandardFilters.TruncateWords(null));
            Assert.AreEqual("", StandardFilters.TruncateWords(""));
            Assert.AreEqual("one two three", StandardFilters.TruncateWords("one two three", 4));
            Assert.AreEqual("one two...", StandardFilters.TruncateWords("one two three", 2));
            Assert.AreEqual("one two three", StandardFilters.TruncateWords("one two three"));
            Assert.AreEqual("Two small (13&#8221; x 5.5&#8221; x 10&#8221; high) baskets fit inside one large basket (13&#8221;...", StandardFilters.TruncateWords("Two small (13&#8221; x 5.5&#8221; x 10&#8221; high) baskets fit inside one large basket (13&#8221; x 16&#8221; x 10.5&#8221; high) with cover.", 15));

            Helper.AssertTemplateResult(expected: "Ground control to...", template: "{{ \"Ground control to Major Tom.\" | truncate_words: 3}}");
            Helper.AssertTemplateResult(expected: "Ground control to--", template: "{{ \"Ground control to Major Tom.\" | truncate_words: 3, \"--\"}}");
            Helper.AssertTemplateResult(expected: "Ground control to", template: "{{ \"Ground control to Major Tom.\" | truncate_words: 3, \"\"}}");
            Helper.AssertTemplateResult(expected: "...", template: "{{ \"Ground control to Major Tom.\" | truncate_words: 0}}");
            Helper.AssertTemplateResult(expected: "...", template: "{{ \"Ground control to Major Tom.\" | truncate_words: -1}}");
            Helper.AssertTemplateResult(expected: "Liquid error: Value was either too large or too small for an Int32.", template: $"{{{{ \"Ground control to Major Tom.\" | truncate_words: {((long)int.MaxValue) + 1}}}}}");
        }

        [Test]
        public void TestSplit()
        {
            CollectionAssert.AreEqual(new[] { "This", "is", "a", "sentence" }, StandardFilters.Split("This is a sentence", " "));
            CollectionAssert.AreEqual(new string[] { null }, StandardFilters.Split(null, null));

            // A string with no pattern should be split into a string[], as required for the Liquid Reverse filter
            CollectionAssert.AreEqual(new[] { "Y", "M", "C", "A" }, StandardFilters.Split("YMCA", null));
            CollectionAssert.AreEqual(new[] { "Y", "M", "C", "A" }, StandardFilters.Split("YMCA", ""));
            CollectionAssert.AreEqual(new[] { " " }, StandardFilters.Split(" ", ""));
        }

        [Test]
        public void TestStripHtml()
        {
            Assert.AreEqual("test", StandardFilters.StripHtml("<div>test</div>"));
            Assert.AreEqual("test", StandardFilters.StripHtml("<div id='test'>test</div>"));
            Assert.AreEqual("", StandardFilters.StripHtml("<script type='text/javascript'>document.write('some stuff');</script>"));
            Assert.AreEqual("", StandardFilters.StripHtml("<style type='text/css'>foo bar</style>"));
            Assert.AreEqual("", StandardFilters.StripHtml("<STYLE type='text/css'>foo bar</style>"));
            Assert.AreEqual("test", StandardFilters.StripHtml("<div\nclass='multiline'>test</div>"));
            Assert.AreEqual("test", StandardFilters.StripHtml("<!-- foo bar \n test -->test"));
            Assert.AreEqual(null, StandardFilters.StripHtml(null));

            // Quirk of the existing implementation
            Assert.AreEqual("foo;", StandardFilters.StripHtml("<<<script </script>script>foo;</script>"));
        }

        [Test]
        public void TestStrip()
        {
            Assert.AreEqual("test", StandardFilters.Strip("  test  "));
            Assert.AreEqual("test", StandardFilters.Strip("   test"));
            Assert.AreEqual("test", StandardFilters.Strip("test   "));
            Assert.AreEqual("test", StandardFilters.Strip("test"));
            Assert.AreEqual(null, StandardFilters.Strip(null));
        }

        [Test]
        public void TestLStrip()
        {
            Assert.AreEqual("test  ", StandardFilters.Lstrip("  test  "));
            Assert.AreEqual("test", StandardFilters.Lstrip("   test"));
            Assert.AreEqual("test   ", StandardFilters.Lstrip("test   "));
            Assert.AreEqual("test", StandardFilters.Lstrip("test"));
            Assert.AreEqual(null, StandardFilters.Lstrip(null));
        }

        [Test]
        public void TestRStrip()
        {
            Assert.AreEqual("  test", StandardFilters.Rstrip("  test  "));
            Assert.AreEqual("   test", StandardFilters.Rstrip("   test"));
            Assert.AreEqual("test", StandardFilters.Rstrip("test   "));
            Assert.AreEqual("test", StandardFilters.Rstrip("test"));
            Assert.AreEqual(null, StandardFilters.Rstrip(null));
        }

        [Test]
        public void TestSlice_V22()
        {
            Context context = _contextV22;

            // Verify backwards compatibility for pre-22a syntax (DotLiquid returns null for null input or empty slice)
            Assert.AreEqual(null, StandardFilters.Slice(context, null, 1)); // DotLiquid test case
            Assert.AreEqual(null, StandardFilters.Slice(context, "", 10)); // DotLiquid test case

            Assert.AreEqual(null, StandardFilters.Slice(context, null, 0)); // Liquid test case
            Assert.AreEqual(null, StandardFilters.Slice(context, "foobar", 100, 10)); // Liquid test case

            // Verify DotLiquid is consistent with Liquid for everything else
            TestSliceString(context);
            TestSliceArrays(context);
        }

        [Test]
        public void TestSlice_V22a()
        {
            Context context = _contextV22a;

            // Verify Liquid compliance from V22a syntax:
            Assert.AreEqual("", StandardFilters.Slice(context, null, 1)); // DotLiquid test case
            Assert.AreEqual("", StandardFilters.Slice(context, "", 10)); // DotLiquid test case

            Assert.AreEqual("", StandardFilters.Slice(context, null, 0)); // Liquid test case
            Assert.AreEqual("", StandardFilters.Slice(context, "foobar", 100, 10)); // Liquid test case

            // Verify DotLiquid is consistent with Liquid for everything else
            TestSliceString(context);
            TestSliceArrays(context);
        }

        private void TestSliceString(Context context)
        {
            Assert.AreEqual("abc", StandardFilters.Slice(context, "abcdefg", 0, 3));
            Assert.AreEqual("bcd", StandardFilters.Slice(context, "abcdefg", 1, 3));
            Assert.AreEqual("efg", StandardFilters.Slice(context, "abcdefg", -3, 3));
            Assert.AreEqual("efg", StandardFilters.Slice(context, "abcdefg", -3, 30));
            Assert.AreEqual("efg", StandardFilters.Slice(context, "abcdefg", 4, 30));
            Assert.AreEqual("a", StandardFilters.Slice(context, "abc", -4, 2));
            Assert.AreEqual("", StandardFilters.Slice(context, "abcdefg", -10, 1));

            // Test replicated from the Ruby library (https://github.com/Shopify/liquid/blob/master/test/integration/standard_filter_test.rb)
            Assert.AreEqual("oob", StandardFilters.Slice(context, "foobar", 1, 3));
            Assert.AreEqual("oobar", StandardFilters.Slice(context, "foobar", 1, 1000));
            Assert.AreEqual("", StandardFilters.Slice(context, "foobar", 1, 0));
            Assert.AreEqual("o", StandardFilters.Slice(context, "foobar", 1, 1));
            Assert.AreEqual("bar", StandardFilters.Slice(context, "foobar", 3, 3));
            Assert.AreEqual("ar", StandardFilters.Slice(context, "foobar", -2, 2));
            Assert.AreEqual("ar", StandardFilters.Slice(context, "foobar", -2, 1000));
            Assert.AreEqual("r", StandardFilters.Slice(context, "foobar", -1));
            Assert.AreEqual("", StandardFilters.Slice(context, "foobar", -100, 10));
            Assert.AreEqual("oob", StandardFilters.Slice(context, "foobar", 1, 3));
        }

        private void TestSliceArrays(Context context)
        {
            // Test replicated from the Ruby library
            var testArray = new[] { "f", "o", "o", "b", "a", "r" };
            CollectionAssert.AreEqual(ToStringArray("oob"), (IEnumerable<object>)StandardFilters.Slice(context, testArray, 1, 3));
            CollectionAssert.AreEqual(ToStringArray("oobar"), (IEnumerable<object>)StandardFilters.Slice(context, testArray, 1, 1000));
            CollectionAssert.AreEqual(ToStringArray(""), (IEnumerable<object>)StandardFilters.Slice(context, testArray, 1, 0));
            CollectionAssert.AreEqual(ToStringArray("o"), (IEnumerable<object>)StandardFilters.Slice(context, testArray, 1, 1));
            CollectionAssert.AreEqual(ToStringArray("bar"), (IEnumerable<object>)StandardFilters.Slice(context, testArray, 3, 3));
            CollectionAssert.AreEqual(ToStringArray("ar"), (IEnumerable<object>)StandardFilters.Slice(context, testArray, -2, 2));
            CollectionAssert.AreEqual(ToStringArray("ar"), (IEnumerable<object>)StandardFilters.Slice(context, testArray, -2, 1000));
            CollectionAssert.AreEqual(ToStringArray("r"), (IEnumerable<object>)StandardFilters.Slice(context, testArray, -1));
            CollectionAssert.AreEqual(ToStringArray(""), (IEnumerable<object>)StandardFilters.Slice(context, testArray, 100, 10));
            CollectionAssert.AreEqual(ToStringArray(""), (IEnumerable<object>)StandardFilters.Slice(context, testArray, -100, 10));

            // additional tests
            CollectionAssert.AreEqual(ToStringArray("fo"), (IEnumerable<object>)StandardFilters.Slice(context, testArray, -6, 2));
            CollectionAssert.AreEqual(ToStringArray("fo"), (IEnumerable<object>)StandardFilters.Slice(context, testArray, -8, 4));

            // Non-string arrays tests
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, (IEnumerable<object>)StandardFilters.Slice(context, new[] { 1, 2, 3, 4, 5 }, 1, 3));
            CollectionAssert.AreEqual(new[] { 'b', 'c', 'd' }, (IEnumerable<object>)StandardFilters.Slice(context, new[] { 'a', 'b', 'c', 'd', 'e' }, -4, 3));
        }

        /// <summary>
        /// Convert a string into a string[] where each character is mapped into an array element.
        /// </summary>
        private static string[] ToStringArray(string input)
        {
            return input.ToCharArray().Select(character => character.ToString()).ToArray();
        }

        [Test]
        public void TestSliceShopifySamples()
        {
            // Test from Liquid specification at https://shopify.github.io/liquid/filters/slice/
            Helper.AssertTemplateResult(
                expected: @"
PaulGeorge",
                template: @"{% assign beatles = 'John, Paul, George, Ringo' | split: ', ' %}
{{ beatles | slice: 1, 2 }}");

            Helper.AssertTemplateResult(
                expected: "ui",
                template: "{{ 'Liquid' | slice: -3, 2 }}");
        }

        [Test]
        public void TestJoin()
        {
            Assert.AreEqual(null, StandardFilters.Join(null));
            Assert.AreEqual("", StandardFilters.Join(""));
            Assert.AreEqual("1 2 3 4", StandardFilters.Join(new[] { 1, 2, 3, 4 }));
            Assert.AreEqual("1 - 2 - 3 - 4", StandardFilters.Join(new[] { 1, 2, 3, 4 }, " - "));

            // Sample from specification at https://shopify.github.io/liquid/filters/join/
            Helper.AssertTemplateResult(
                expected: "\r\nJohn and Paul and George and Ringo",
                template: @"{% assign beatles = ""John, Paul, George, Ringo"" | split: "", "" %}
{{ beatles | join: "" and "" }}");
        }

        [Test]
        public void TestSortV20()
        {
            var ints = new[] { 10, 3, 2, 1 };
            Assert.AreEqual(null, StandardFilters.Sort(_contextV20, null));
            CollectionAssert.AreEqual(new string[] { }, StandardFilters.Sort(_contextV20, new string[] { }));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 10 }, StandardFilters.Sort(_contextV20, ints));
            CollectionAssert.AreEqual(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 10 } },
                StandardFilters.Sort(_contextV20, new[] { new { a = 10 }, new { a = 3 }, new { a = 1 }, new { a = 2 } }, "a"));

            // Issue #393 - Incorrect (Case-Insensitve) Alphabetic Sort
            var strings = new[] { "zebra", "octopus", "giraffe", "Sally Snake" };
            CollectionAssert.AreEqual(new[] { "giraffe", "octopus", "Sally Snake", "zebra" },
                StandardFilters.Sort(_contextV20, strings));

            var hashes = new List<Hash>();
            for (var i = 0; i < strings.Length; i++)
                hashes.Add(CreateHash(ints[i], strings[i]));
            CollectionAssert.AreEqual(new[] { hashes[2], hashes[1], hashes[3], hashes[0] },
                StandardFilters.Sort(_contextV20, hashes, "content"));
            CollectionAssert.AreEqual(new[] { hashes[3], hashes[2], hashes[1], hashes[0] },
                StandardFilters.Sort(_contextV20, hashes, "sortby"));
        }

        [Test]
        public void TestSortV22()
        {
            var ints = new[] { 10, 3, 2, 1 };
            Assert.AreEqual(null, StandardFilters.Sort(_contextV22, null));
            CollectionAssert.AreEqual(new string[] { }, StandardFilters.Sort(_contextV22, new string[] { }));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 10 }, StandardFilters.Sort(_contextV22, ints));
            CollectionAssert.AreEqual(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 10 } },
                StandardFilters.Sort(_contextV22, new[] { new { a = 10 }, new { a = 3 }, new { a = 1 }, new { a = 2 } }, "a"));

            var strings = new[] { "zebra", "octopus", "giraffe", "Sally Snake" };
            CollectionAssert.AreEqual(new[] { "Sally Snake", "giraffe", "octopus", "zebra" },
                StandardFilters.Sort(_contextV22, strings));

            var hashes = new List<Hash>();
            for (var i = 0; i < strings.Length; i++)
                hashes.Add(CreateHash(ints[i], strings[i]));
            CollectionAssert.AreEqual(new[] { hashes[3], hashes[2], hashes[1], hashes[0] },
                StandardFilters.Sort(_contextV22, hashes, "content"));
            CollectionAssert.AreEqual(new[] { hashes[3], hashes[2], hashes[1], hashes[0] },
                StandardFilters.Sort(_contextV22, hashes, "sortby"));
        }

        [Test]
        public void TestSortNatural()
        {
            var ints = new[] { 10, 3, 2, 1 };
            Assert.AreEqual(null, StandardFilters.SortNatural(null));
            CollectionAssert.AreEqual(new string[] { }, StandardFilters.SortNatural(new string[] { }));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 10 }, StandardFilters.SortNatural(ints));
            CollectionAssert.AreEqual(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 10 } },
                StandardFilters.SortNatural(new[] { new { a = 10 }, new { a = 3 }, new { a = 1 }, new { a = 2 } }, "a"));

            var strings = new[] { "zebra", "octopus", "giraffe", "Sally Snake" };
            CollectionAssert.AreEqual(new[] { "giraffe", "octopus", "Sally Snake", "zebra" },
                StandardFilters.SortNatural(strings));

            var hashes = new List<Hash>();
            for (var i = 0; i < strings.Length; i++)
                hashes.Add(CreateHash(ints[i], strings[i]));
            CollectionAssert.AreEqual(new[] { hashes[2], hashes[1], hashes[3], hashes[0] },
                StandardFilters.SortNatural(hashes, "content"));
            CollectionAssert.AreEqual(new[] { hashes[3], hashes[2], hashes[1], hashes[0] },
                StandardFilters.SortNatural(hashes, "sortby"));
        }

        [Test]
        public void TestSort_OnHashList_WithProperty_DoesNotFlattenList()
        {
            var list = new List<Hash>();
            var hash1 = CreateHash(1, "Text1");
            var hash2 = CreateHash(2, "Text2");
            var hash3 = CreateHash(3, "Text3");
            list.Add(hash3);
            list.Add(hash1);
            list.Add(hash2);

            var result = StandardFilters.Sort(_contextV20, list, "sortby").Cast<Hash>().ToArray();
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(hash1["content"], result[0]["content"]);
            Assert.AreEqual(hash2["content"], result[1]["content"]);
            Assert.AreEqual(hash3["content"], result[2]["content"]);
        }

        [Test]
        public void TestSort_OnDictionaryWithPropertyOnlyInSomeElement_ReturnsSortedDictionary()
        {
            var list = new List<Hash>();
            var hash1 = CreateHash(1, "Text1");
            var hash2 = CreateHash(2, "Text2");
            var hashWithNoSortByProperty = new Hash();
            hashWithNoSortByProperty.Add("content", "Text 3");
            list.Add(hash2);
            list.Add(hashWithNoSortByProperty);
            list.Add(hash1);

            var result = StandardFilters.Sort(_contextV20, list, "sortby").Cast<Hash>().ToArray();
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(hashWithNoSortByProperty["content"], result[0]["content"]);
            Assert.AreEqual(hash1["content"], result[1]["content"]);
            Assert.AreEqual(hash2["content"], result[2]["content"]);
        }

        [Test]
        public void TestSort_Indexable()
        {
            var packages = new [] {
                new Package(numberOfPiecesPerPackage: 2, test: "p1"),
                new Package(numberOfPiecesPerPackage: 1, test: "p2"),
                new Package(numberOfPiecesPerPackage: 3, test: "p3"),
            };
            var expectedPackages= packages.OrderBy(p => p["numberOfPiecesPerPackage"]).ToArray();

            Helper.LockTemplateStaticVars(new RubyNamingConvention(), () =>
            {
                CollectionAssert.AreEqual(
                    expected: expectedPackages,
                    actual: StandardFilters.Sort(_contextV20, packages, "numberOfPiecesPerPackage"));
            });
        }

        [Test]
        public void TestSort_ExpandoObject()
        {
            dynamic package1 = new ExpandoObject();
            package1.numberOfPiecesPerPackage = 2;
            package1.test = "p1";
            dynamic package2 = new ExpandoObject();
            package2.numberOfPiecesPerPackage = 1;
            package2.test = "p2";
            dynamic package3 = new ExpandoObject();
            package3.numberOfPiecesPerPackage = 3;
            package3.test = "p3";
            var packages = new List<ExpandoObject> { package1, package2, package3 };
            var expectedPackages = new List<ExpandoObject> { package2, package1, package3 };

            Assert.AreEqual(
                expected: expectedPackages,
                actual: StandardFilters.Sort(_contextV20, packages, property: "numberOfPiecesPerPackage"));
        }

        private static Hash CreateHash(int sortby, string content) =>
            new Hash
            {
                { "sortby", sortby },
                { "content", content }
            };

        [Test]
        public void TestMap()
        {
            CollectionAssert.AreEqual(new string[] { }, StandardFilters.Map(new string[] { }, "a"));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 },
                StandardFilters.Map(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 4 } }, "a"));
            Helper.AssertTemplateResult("abc", "{{ ary | map:'foo' | map:'bar' }}",
                Hash.FromAnonymousObject(
                    new
                    {
                        ary =
                            new[]
                    {
                        Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = "a" }) }), Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = "b" }) }),
                        Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = "c" }) })
                    }
                    }));

            Assert.AreEqual(null, StandardFilters.Map(null, "a"));
            CollectionAssert.AreEqual(new object[] { null }, StandardFilters.Map(new object[] { null }, "a"));

            var hash = Hash.FromAnonymousObject(new
            {
                ary = new[] {
                    new Helper.DataObject { PropAllowed = "a", PropDisallowed = "x" },
                    new Helper.DataObject { PropAllowed = "b", PropDisallowed = "y" },
                    new Helper.DataObject { PropAllowed = "c", PropDisallowed = "z" },
                }
            });

            Helper.AssertTemplateResult("abc", "{{ ary | map:'prop_allowed' | join:'' }}", hash);
            Helper.AssertTemplateResult("", "{{ ary | map:'no_prop' | join:'' }}", hash);

            hash = Hash.FromAnonymousObject(new
            {
                ary = new[] {
                    new Helper.DataObjectDrop { Prop = "a" },
                    new Helper.DataObjectDrop { Prop = "b" },
                    new Helper.DataObjectDrop { Prop = "c" },
                }
            });

            Helper.AssertTemplateResult("abc", "{{ ary | map:'prop' | join:'' }}", hash);
            Helper.AssertTemplateResult("", "{{ ary | map:'no_prop' | join:'' }}", hash);
        }

        /// <summary>
        /// Test case for [Issue #520](https://github.com/dotliquid/dotliquid/issues/520)
        /// </summary>
        [Test]
        public void TestMapInvalidProperty()
        {
            var nullObjectArray = new object[] { null };
            // Anonymous Type
            CollectionAssert.AreEqual(nullObjectArray,
                StandardFilters.Map(new[] { new { a = 1 } }, "no_prop"));

            // Drop
            CollectionAssert.AreEqual(nullObjectArray,
                StandardFilters.Map(new[] { new Helper.DataObjectDrop { Prop = "a" }}, "no_prop"));

            // Dictionary
            CollectionAssert.AreEqual(nullObjectArray,
                StandardFilters.Map(Hash.FromDictionary(new Dictionary<string, object>() { { "a", 1 } }), "no_prop"));

            // Expando Array
            var expandoJson = "[{\"a\": 1}]";
            var expandoObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject[]>(expandoJson);
            CollectionAssert.AreEqual(nullObjectArray,
                StandardFilters.Map(expandoObj, "no_prop"));
        }

        /// <summary>
        /// Test case for [Issue #275](https://github.com/dotliquid/dotliquid/issues/275)
        /// </summary>
        [Test]
        public void TestMapDisallowedProperty() {
            var hash = Hash.FromAnonymousObject(new
            {
                safe = new[] { new Helper.DataObjectRegistered { PropAllowed = "a", PropDisallowed = "x" }},
                attr = new[] { new Helper.DataObject { PropAllowed = "a", PropDisallowed = "x" } }
            });

            Helper.AssertTemplateResult("", "{{ safe | map:'prop_disallowed' | join:'' }}", hash);
            Helper.AssertTemplateResult("", "{{ attr | map:'prop_disallowed' | join:'' }}", hash);
        }

        /// <summary>
        /// Tests map filter per Shopify specification sample
        /// </summary>
        /// <remarks><see href="https://shopify.github.io/liquid/filters/map/"/></remarks>
        [Test]
        public void TestMapSpecificationSample()
        {
            var hash = Hash.FromAnonymousObject(new
            {
                site = new
                {
                    pages = new[] {
                        new { category = "business" },
                        new { category = "celebrities" },
                        new { category = "lifestyle" },
                        new { category = "sports" },
                        new { category = "technology" }
                    }
                }
            });

            Helper.AssertTemplateResult(
                expected: "\r\n- business\r\n\r\n- celebrities\r\n\r\n- lifestyle\r\n\r\n- sports\r\n\r\n- technology\r\n",
                template: @"{% assign all_categories = site.pages | map: ""category"" %}{% for item in all_categories %}
- {{ item }}
{% endfor %}",
                localVariables: hash);
        }

        /// <summary>
        /// Tests map filter per Shopify specification sample
        /// </summary>
        /// <remarks>In this variant of the test we add another property to the items in the collection to ensure the map filter does its job of removing other properties</remarks>
        [Test]
        public void TestMapSpecificationSampleVariant()
        {
            var hash = Hash.FromAnonymousObject(new
            {
                site = new
                {
                    pages = new[] {
                        new { category = "business", author = "Joe" },
                        new { category = "celebrities", author = "Jon" },
                        new { category = "lifestyle", author = "John" },
                        new { category = "sports", author = "Joan" },
                        new { category = "technology", author = "Jean" }
                    }
                }
            });

            Helper.AssertTemplateResult(
                expected: "\r\n- business\r\n\r\n- celebrities\r\n\r\n- lifestyle\r\n\r\n- sports\r\n\r\n- technology\r\n",
                template: @"{% assign all_categories = site.pages | map: ""category"" %}{% for item in all_categories %}
- {{ item }}
{% endfor %}", localVariables: hash);
        }

        [Test]
        public void TestMapShipmentPackage()
        {
            var hash = Hash.FromAnonymousObject(new
            {
                content = new
                {
                    carrierSettings = new[] {
                        new
                        {
                            numberOfPiecesPerPackage = 10,
                            test = "test"
                        },
                        new
                        {
                            numberOfPiecesPerPackage = 12,
                            test = "test1"
                        }
                    }
                }
            });

            Helper.AssertTemplateResult(
                expected: "{\r\n\r\n\"tests\" : [\r\n            {\r\n                \"numberOfPiecesPerPackage\" : \"10\"\r\n      },\r\n      \r\n            {\r\n                \"numberOfPiecesPerPackage\" : \"12\"\r\n      },\r\n      ]\r\n}",
                template: @"{
{% assign test1 = content.carrierSettings | map: ""numberOfPiecesPerPackage"" %}
""tests"" : [{% for test in test1 %}
            {
                ""numberOfPiecesPerPackage"" : ""{{ test }}""
      },
      {% endfor %}]
}",
                localVariables: hash);

            Helper.AssertTemplateResult(
                expected: "{\r\n\r\n\"tests\" : 1012\r\n}",
                template: @"{
{% assign test1 = content.carrierSettings | map: ""numberOfPiecesPerPackage"" %}
""tests"" : {{test1}}
}",
                localVariables: hash);
        }

        private class Package : IIndexable, ILiquidizable
        {
            private readonly int numberOfPiecesPerPackage;

            private readonly string test;

            public Package(int numberOfPiecesPerPackage, string test)
            {
                this.numberOfPiecesPerPackage = numberOfPiecesPerPackage;
                this.test = test;
            }

            public object this[object key] => key as string == "numberOfPiecesPerPackage"
                ? this.numberOfPiecesPerPackage as object
                : key as string == "test"
                    ? test
                    : null;

            public bool ContainsKey(object key)
            {
                return new List<string> { nameof(numberOfPiecesPerPackage), nameof(test) }
                    .Contains(key);
            }

            public object ToLiquid()
            {
                return this;
            }
        };

        [Test]
        public void TestMapIndexable()
        {
            var hash = Hash.FromAnonymousObject(new
            {
                content = new
                {
                    carrierSettings = new[]
                    {
                        new Package(numberOfPiecesPerPackage:10, test:"test"),
                        new Package(numberOfPiecesPerPackage:12, test:"test1"),
                    }
                }
            });

            Helper.AssertTemplateResult(
                expected: "{\r\n\r\n\"tests\" : [\r\n            {\r\n                \"numberOfPiecesPerPackage\" : \"10\"\r\n      },\r\n      \r\n            {\r\n                \"numberOfPiecesPerPackage\" : \"12\"\r\n      },\r\n      ]\r\n}",
                template: @"{
{% assign test1 = content.carrierSettings | map: ""numberOfPiecesPerPackage"" %}
""tests"" : [{% for test in test1 %}
            {
                ""numberOfPiecesPerPackage"" : ""{{ test }}""
      },
      {% endfor %}]
}",
                localVariables: hash);

            Helper.AssertTemplateResult(
                expected: "{\r\n\r\n\"tests\" : 1012\r\n}",
                template: @"{
{% assign test1 = content.carrierSettings | map: ""numberOfPiecesPerPackage"" %}
""tests"" : {{test1}}
}",
                localVariables: hash);
        }

        [Test]
        public void TestMapExpandoObject()
        {
            dynamic product1 = new ExpandoObject();
            product1.title = "Vacuum";
            product1.type = "cleaning";
            dynamic product2 = new ExpandoObject();
            product2.title = "Spatula";
            product2.type = "kitchen";
            dynamic product3 = new ExpandoObject();
            product3.title = "Television";
            product3.type = "lounge";
            dynamic product4 = new ExpandoObject();
            product4.title = "Garlic press";
            product4.type = "kitchen";
            var products = new List<ExpandoObject> { product1, product2, product3, product4 };

            Assert.AreEqual(
                expected: new List<string>{"Vacuum", "Spatula", "Television", "Garlic press"},
                actual: StandardFilters.Map(products, "title"));
        }

        [Test]
        public void TestMapJoin()
        {
            var hash = Hash.FromAnonymousObject(new
            {
                content = new
                {
                    carrierSettings = new[] {
                        new
                        {
                            numberOfPiecesPerPackage = 10,
                            test = "test"
                        },
                        new
                        {
                            numberOfPiecesPerPackage = 12,
                            test = "test1"
                        }
                    }
                }
            });

            Helper.AssertTemplateResult(
                expected: "\r\n{ \"test\": \"10, 12\"}",
                template: @"{% assign test = content.carrierSettings | map: ""numberOfPiecesPerPackage"" | join: "", ""%}
{ ""test"": ""{{test}}""}",
                localVariables: hash);
        }

        [TestCase("6.72", "$6.72")]
        [TestCase("6000", "$6,000.00")]
        [TestCase("6000000", "$6,000,000.00")]
        [TestCase("6000.4", "$6,000.40")]
        [TestCase("6000000.4", "$6,000,000.40")]
        [TestCase("6.8458", "$6.85")]
        public void TestAmericanCurrencyFromString(string input, string expected)
        {
            // Set the thread culture and test for backward compatibility
            using (CultureHelper.SetCulture("en-US"))
            {
                Helper.AssertTemplateResult(
                    expected: expected,
                    template: "{{ input | currency }}",
                    localVariables: Hash.FromAnonymousObject(new { input = input }));
            }

            _contextV20.CurrentCulture = new CultureInfo("en-US"); // _contextV20 is initialized with InvariantCulture, these tests require en-US
            Assert.AreEqual(expected, StandardFilters.Currency(context: _contextV20, input: input));
        }

        [TestCase("6.72", "6,72 €", "de-DE")]
        [TestCase("6000", "6.000,00 €", "de-DE")]
        [TestCase("6000000", "6.000.000,00 €", "de-DE")]
        [TestCase("6000.4", "6.000,40 €", "de-DE")]
        [TestCase("6000000.4", "6.000.000,40 €", "de-DE")]
        [TestCase("6.8458", "6,85 €", "de-DE")]
        [TestCase(6000001d, "6.000.001,00 €", "de-DE")]
        [TestCase("6000000.00", "6 000 000,00 €", "fr-FR")]
        [TestCase("99.999", "100,00 €", "es")]
        [TestCase("99.999", "100,00 €", "pt-PT")]
        [TestCase(7000, "¤7,000.00", "")] // "" = InvariantCulture
        [TestCase(7000, "¤7,000.00", " ")] // "" = InvariantCulture
        [TestCase(int.MaxValue, "2 147 483 647,00 €", "fr-FR")]
        [TestCase(long.MaxValue, "9 223 372 036 854 775 807,00 €", "fr")]
        public void TestEuroCurrencyFromString(object input, string expected, string languageTag)
        {
            // Set the thread culture and test for backward compatibility
            using (CultureHelper.SetCulture("en-US"))
            {
                Helper.AssertTemplateResult(
                    expected: expected,
                    template: "{{ input | currency: languageTag }}",
                    localVariables: Hash.FromAnonymousObject(new { input = input, languageTag = languageTag }));
            }

            _contextV20.CurrentCulture = new CultureInfo("en-US"); // _contextV20 is initialized with InvariantCulture, these tests require en-US
            Assert.AreEqual(expected, StandardFilters.Currency(context: _contextV20, input: input, languageTag: languageTag));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void TestNullOrEmptyInputCurrency(string input)
        {
            // Set the thread culture and test for backward compatibility
            using (CultureHelper.SetCulture("en-US"))
            {
                Helper.AssertTemplateResult(
                    expected: string.Empty,
                    template: "{{ input | currency: 'de-DE' }}",
                    localVariables: Hash.FromAnonymousObject(new { input = input }));
            }

            // _contextV20 is initialized with InvariantCulture
            Assert.AreEqual(
                expected: input,
                actual: StandardFilters.Currency(context: _contextV20, input: input, languageTag: "de-DE"));
        }        

        [Test]
        public void TestMalformedCurrency()
        {
            // Set the thread culture and test for backward compatibility
            using (CultureHelper.SetCulture("en-US"))
            {
                Helper.AssertTemplateResult(
                    expected: "teststring",
                    template: "{{ 'teststring' | currency: 'de-DE' }}");
            }

            // _contextV20 is initialized with InvariantCulture
            Assert.AreEqual("teststring", StandardFilters.Currency(context: _contextV20, input: "teststring", languageTag: "de-DE"));
        }

        [Test]
        public void TestCurrencyWithinTemplateRender()
        {
            using (CultureHelper.SetCulture("en-US"))
            {
                Template dollarTemplate = Template.Parse(@"{{ amount | currency }}");
                Template euroTemplate = Template.Parse(@"{{ amount | currency: ""de-DE"" }}");

                Assert.AreEqual("$7,000.00", dollarTemplate.Render(Hash.FromAnonymousObject(new { amount = "7000" })));
                Assert.AreEqual("7.000,00 €", euroTemplate.Render(Hash.FromAnonymousObject(new { amount = 7000 })));
            }
        }

        [Test]
        public void TestCurrencyFromDoubleInput()
        {
            Assert.AreEqual("$6.85", StandardFilters.Currency(context: _contextV20, input: 6.8458, languageTag: "en-US"));
            Assert.AreEqual("$6.72", StandardFilters.Currency(context: _contextV20, input: 6.72, languageTag: "en-CA"));
            Assert.AreEqual("6.000.000,00 €", StandardFilters.Currency(context: _contextV20, input: 6000000, languageTag: "de-DE"));
            Assert.AreEqual("6.000.000,78 €", StandardFilters.Currency(context: _contextV20, input: 6000000.78, languageTag: "de-DE"));
        }

        [Test]
        public void TestCurrencyLanguageTag()
        {
            Assert.AreEqual("6.000.000,00 €", StandardFilters.Currency(context: _contextV20, input: 6000000, languageTag: "de-DE")); // language+country
            Assert.AreEqual("6.000.000,00 €", StandardFilters.Currency(context: _contextV20, input: 6000000, languageTag: "de")); // language only
            Assert.Throws<CultureNotFoundException>(() => StandardFilters.Currency(context: _contextV20, input: "teststring", languageTag: "german")); // invalid language
        }

        [Test]
        public void TestDate()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                TestDate(_contextV20);
            });
        }

        private void TestDate(Context context)
        {
            context.UseRubyDateFormat = false;
            DateTimeFormatInfo dateTimeFormat = context.CurrentCulture.DateTimeFormat;

            Assert.AreEqual(dateTimeFormat.GetMonthName(5), StandardFilters.Date(context: context, input: DateTime.Parse("2006-05-05 10:00:00"), format: "MMMM"));
            Assert.AreEqual(dateTimeFormat.GetMonthName(6), StandardFilters.Date(context: context, input: DateTime.Parse("2006-06-05 10:00:00"), format: "MMMM"));
            Assert.AreEqual(dateTimeFormat.GetMonthName(7), StandardFilters.Date(context: context, input: DateTime.Parse("2006-07-05 10:00:00"), format: "MMMM"));

            Assert.AreEqual(dateTimeFormat.GetMonthName(5), StandardFilters.Date(context: context, input: "2006-05-05 10:00:00", format: "MMMM"));
            Assert.AreEqual(dateTimeFormat.GetMonthName(6), StandardFilters.Date(context: context, input: "2006-06-05 10:00:00", format: "MMMM"));
            Assert.AreEqual(dateTimeFormat.GetMonthName(7), StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "MMMM"));

            Assert.AreEqual("08/01/2006 10:00:00", StandardFilters.Date(context: context, input: "08/01/2006 10:00:00", format: string.Empty));
            Assert.AreEqual("08/02/2006 10:00:00", StandardFilters.Date(context: context, input: "08/02/2006 10:00:00", format: null));
            Assert.AreEqual(new DateTime(2006, 8, 3, 10, 0, 0).ToString(context.CurrentCulture), StandardFilters.Date(context: context, input: new DateTime(2006, 8, 3, 10, 0, 0), format: string.Empty));
            Assert.AreEqual(new DateTime(2006, 8, 4, 10, 0, 0).ToString(context.CurrentCulture), StandardFilters.Date(context: context, input: new DateTime(2006, 8, 4, 10, 0, 0), format: null));

            Assert.AreEqual(new DateTime(2006, 7, 5).ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "MM/dd/yyyy"));

            Assert.AreEqual(new DateTime(2004, 7, 16).ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "Fri Jul 16 2004 01:00:00", format: "MM/dd/yyyy"));

            Assert.AreEqual(null, StandardFilters.Date(context: context, input: null, format: "MMMM"));

            Assert.AreEqual("hi", StandardFilters.Date(context: context, input: "hi", format: "MMMM"));

            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "now", format: "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "today", format: "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "Now", format: "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), StandardFilters.Date(context: context, input: "Today", format: "MM/dd/yyyy"));

            Assert.AreEqual("345000", StandardFilters.Date(context: context, input: DateTime.Parse("2006-05-05 10:00:00.345"), format: "ffffff"));

            Template template = Template.Parse(@"{{ hi | date:""MMMM"" }}");
            Assert.AreEqual("hi", template.Render(Hash.FromAnonymousObject(new { hi = "hi" })));
        }

        [Test]
        public void TestDateV20()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                var context = _contextV20;
                // Legacy parser doesn't except Unix Epoch https://github.com/dotliquid/dotliquid/issues/322
                Assert.AreEqual("0", StandardFilters.Date(context: context, input: 0, format: null));
                Assert.AreEqual("2147483648", StandardFilters.Date(context: context, input: 2147483648, format: null)); // Beyond Int32 boundary

                // Legacy parser loses specified offset https://github.com/dotliquid/dotliquid/issues/149
                var testDate = new DateTime(2006, 8, 4, 10, 0, 0);
                Assert.AreEqual(new DateTimeOffset(testDate).ToString("zzz"), StandardFilters.Date(context: context, input: new DateTimeOffset(testDate, TimeSpan.FromHours(-14)), format: "zzz"));

                // Legacy parser doesn't handle local offset & explicit offset in calculating epoch
                Liquid.UseRubyDateFormat = true; // ensure all Contexts created within tests are defaulted to Ruby date format
                var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
                var unixEpochOffset = new DateTimeOffset(unixEpoch).Offset.TotalSeconds;
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = unixEpoch.ToUniversalTime() }));
                Helper.AssertTemplateResult(expected: unixEpochOffset.ToString(), template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = unixEpoch }));
                Helper.AssertTemplateResult(expected: unixEpochOffset.ToString(), template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = DateTime.SpecifyKind(unixEpoch.ToLocalTime(), DateTimeKind.Unspecified) }));
                Helper.AssertTemplateResult(expected: unixEpochOffset.ToString(), template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = new DateTimeOffset(unixEpoch) }));
                Helper.AssertTemplateResult(expected: unixEpochOffset.ToString(), template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = new DateTimeOffset(unixEpoch).ToOffset(TimeSpan.FromHours(-14)) }));

                // Legacy parser defaults to the .NET default format
                Assert.AreEqual(DateTime.Now.ToString(context.CurrentCulture), StandardFilters.Date(context: context, input: "now", format: null));
                Assert.AreEqual(DateTime.Now.ToString(context.CurrentCulture), StandardFilters.Date(context: context, input: "today", format: null));
                Assert.AreEqual(DateTime.Now.ToString(context.CurrentCulture), StandardFilters.Date(context: context, input: "now", format: string.Empty));
                Assert.AreEqual(DateTime.Now.ToString(context.CurrentCulture), StandardFilters.Date(context: context, input: "today", format: string.Empty));
            });
        }

        [Test]
        public void TestDateV21()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                var context = _contextV21;// _contextV21 specifies InvariantCulture
                var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
                Assert.AreEqual(unixEpoch.ToString("g", context.FormatProvider), StandardFilters.Date(context: context, input: 0, format: "g"));
                Assert.AreEqual(unixEpoch.AddSeconds(Int32.MaxValue).AddSeconds(1).ToString("g", context.FormatProvider), StandardFilters.Date(context: context, input: 2147483648, format: "g")); // Beyond Int32 boundary
                Assert.AreEqual(unixEpoch.AddSeconds(UInt32.MaxValue).AddSeconds(1).ToString("g", context.FormatProvider), StandardFilters.Date(context: context, input: 4294967296, format: "g")); // Beyond UInt32 boundary
                Helper.AssertTemplateResult(expected: unixEpoch.ToString("g"), template: "{{ 0 | date: 'g' }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: unixEpoch.AddSeconds(Int32.MaxValue).AddSeconds(1).ToString("g"), template: "{{ 2147483648 | date: 'g' }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: unixEpoch.AddSeconds(UInt32.MaxValue).AddSeconds(1).ToString("g"), template: "{{ 4294967296 | date: 'g' }}", syntax: context.SyntaxCompatibilityLevel);

                var testDate = new DateTime(2006, 8, 4, 10, 0, 0, DateTimeKind.Unspecified);
                Assert.AreEqual("-14:00", StandardFilters.Date(context: context, input: new DateTimeOffset(testDate, TimeSpan.FromHours(-14)), format: "zzz"));
                Helper.AssertTemplateResult(expected: "+00:00", template: "{{ '" + testDate.ToString("u") + "' | date: 'zzz' }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "-14:00", template: "{{ '" + testDate.ToString("u").Replace("Z", "-14:00") + "' | date: 'zzz' }}", syntax: context.SyntaxCompatibilityLevel);

                // ISO-8601 date-time handling with and without timezone
                Helper.AssertTemplateResult(expected: "2021-05-20T12:14:15-08:00", template: "{{ iso8601DateTime | date: 'yyyy-MM-ddThh:mm:sszzz' }}", localVariables: Hash.FromAnonymousObject(new { iso8601DateTime = "2021-05-20T12:14:15-08:00" }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "2021-05-20T12:14:15+09:00", template: "{{ iso8601DateTime | date: 'yyyy-MM-ddThh:mm:sszzz' }}", localVariables: Hash.FromAnonymousObject(new { iso8601DateTime = "2021-05-20T12:14:15+09:00" }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "2021-05-20", template: "{{ iso8601DateTime | date: 'yyyy-MM-dd' }}", localVariables: Hash.FromAnonymousObject(new { iso8601DateTime = "2021-05-20T12:14:15+01:00" }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "2021-05-20T12:14:15+00:00", template: "{{ iso8601DateTime | date: 'yyyy-MM-ddThh:mm:sszzz' }}", localVariables: Hash.FromAnonymousObject(new { iso8601DateTime = "2021-05-20T12:14:15Z" }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "2021-05-20T12:14:15", template: "{{ iso8601DateTime | date: 'yyyy-MM-ddThh:mm:ss' }}", localVariables: Hash.FromAnonymousObject(new { iso8601DateTime = "2021-05-20T12:14:15" }), syntax: context.SyntaxCompatibilityLevel);

                Liquid.UseRubyDateFormat = true; // ensure all Contexts created within tests are defaulted to Ruby date format
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = 0 }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "2147483648", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = 2147483648 }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "4294967296", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = 4294967296 }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = unixEpoch.ToUniversalTime() }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = unixEpoch }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = DateTime.SpecifyKind(unixEpoch, DateTimeKind.Unspecified) }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = new DateTimeOffset(unixEpoch) }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "0", template: "{{ epoch | date: '%s' }}", localVariables: Hash.FromAnonymousObject(new { epoch = new DateTimeOffset(unixEpoch).ToOffset(TimeSpan.FromHours(-14)) }), syntax: context.SyntaxCompatibilityLevel);

                Assert.AreEqual("now", StandardFilters.Date(context: context, input: "now", format: null));
                Assert.AreEqual("today", StandardFilters.Date(context: context, input: "today", format: null));
                Assert.AreEqual("now", StandardFilters.Date(context: context, input: "now", format: string.Empty));
                Assert.AreEqual("today", StandardFilters.Date(context: context, input: "today", format: string.Empty));

                TestDate(context);
            });
        }

        [Test]
        public void TestStrFTime()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                var context = _contextV20;
                context.UseRubyDateFormat = true;
                context.CurrentCulture = new CultureInfo("en-US"); // _contextV20 is initialized with InvariantCulture, these tests require en-US

                Assert.AreEqual("May", StandardFilters.Date(context: context, input: DateTime.Parse("2006-05-05 10:00:00"), format: "%B"));
                Assert.AreEqual("June", StandardFilters.Date(context: context, input: DateTime.Parse("2006-06-05 10:00:00"), format: "%B"));
                Assert.AreEqual("July", StandardFilters.Date(context: context, input: DateTime.Parse("2006-07-05 10:00:00"), format: "%B"));

                Assert.AreEqual("May", StandardFilters.Date(context: context, input: "2006-05-05 10:00:00", format: "%B"));
                Assert.AreEqual("June", StandardFilters.Date(context: context, input: "2006-06-05 10:00:00", format: "%B"));
                Assert.AreEqual("July", StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "%B"));

                Assert.AreEqual("05/07/2006 10:00:00", StandardFilters.Date(context: context, input: "05/07/2006 10:00:00", format: string.Empty));
                Assert.AreEqual("05/07/2006 10:00:00", StandardFilters.Date(context: context, input: "05/07/2006 10:00:00", format: null));
                Assert.AreEqual(new DateTime(2006, 8, 3, 10, 0, 0).ToString(context.FormatProvider), StandardFilters.Date(context: context, input: new DateTime(2006, 8, 3, 10, 0, 0), format: string.Empty));
                Assert.AreEqual(new DateTime(2006, 8, 4, 10, 0, 0).ToString(context.FormatProvider), StandardFilters.Date(context: context, input: new DateTime(2006, 8, 4, 10, 0, 0), format: null));

                Assert.AreEqual("07/05/2006", StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "%m/%d/%Y"));

                Assert.AreEqual("07/16/2004", StandardFilters.Date(context: context, input: "Fri Jul 16 2004 01:00:00", format: "%m/%d/%Y"));

                Assert.AreEqual(null, StandardFilters.Date(context: context, input: null, format: "%M"));

                Assert.AreEqual("hi", StandardFilters.Date(context: context, input: "hi", format: "%M"));

                Liquid.UseRubyDateFormat = true; // ensure all Context objects created within tests are defaulted to Ruby date format
                Template template = Template.Parse(@"{{ hi | date:""%M"" }}");
                Assert.AreEqual("hi", template.Render(Hash.FromAnonymousObject(new { hi = "hi" })));

                Helper.AssertTemplateResult(
                    expected: "14, 16",
                    template: "{{ \"March 14, 2016\" | date: \"%d, %y\" }}",
                    syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(
                    expected: "Mar 14, 16",
                    template: "{{ \"March 14, 2016\" | date: \"%b %d, %y\" }}",
                    syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(
                    expected: $"This page was last updated at {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}.",
                    template: "This page was last updated at {{ 'now' | date: '%Y-%m-%d %H:%M' }}.",
                    syntax: context.SyntaxCompatibilityLevel);
            });
        }

        [Test]
        public void TestFirstLastUsingRuby()
        {
            var namingConvention = new NamingConventions.RubyNamingConvention();
            TestFirstLast(namingConvention, (name) => namingConvention.GetMemberName(name));
        }

        [Test]
        public void TestFirstLastUsingCSharp()
        {
            var namingConvention = new NamingConventions.CSharpNamingConvention();
            TestFirstLast(namingConvention, (name) => char.ToUpperInvariant(name[0]) + name.Substring(1));
        }

        private void TestFirstLast(NamingConventions.INamingConvention namingConvention, Func<string, string> filterNameFunc)
        {
            var splitFilter = filterNameFunc("split");
            var firstFilter = filterNameFunc("first");
            var lastFilter = filterNameFunc("last");

            Assert.Null(StandardFilters.First(null));
            Assert.Null(StandardFilters.Last(null));
            Assert.AreEqual(1, StandardFilters.First(new[] { 1, 2, 3 }));
            Assert.AreEqual(3, StandardFilters.Last(new[] { 1, 2, 3 }));
            Assert.Null(StandardFilters.First(new object[] { }));
            Assert.Null(StandardFilters.Last(new object[] { }));

            Helper.AssertTemplateResult(
                expected: ".",
                template: "{{ 'Ground control to Major Tom.' | " + lastFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "Tom.",
                template: "{{ 'Ground control to Major Tom.' | " + splitFilter + ": ' ' | " + lastFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "tiger",
                template: "{% assign my_array = 'zebra, octopus, giraffe, tiger' | " + splitFilter + ": ', ' %}{{ my_array." + lastFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "There goes a tiger!",
                template: "{% assign my_array = 'zebra, octopus, giraffe, tiger' | " + splitFilter + ": ', ' %}{% if my_array." + lastFilter + " == 'tiger' %}There goes a tiger!{% endif %}",
                namingConvention: namingConvention);

            Helper.AssertTemplateResult(
                expected: "G",
                template: "{{ 'Ground control to Major Tom.' | " + firstFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "Ground",
                template: "{{ 'Ground control to Major Tom.' | " + splitFilter + ": ' ' | " + firstFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "zebra",
                template: "{% assign my_array = 'zebra, octopus, giraffe, tiger' | " + splitFilter + ": ', ' %}{{ my_array." + firstFilter + " }}",
                namingConvention: namingConvention);
            Helper.AssertTemplateResult(
                expected: "There goes a zebra!",
                template: "{% assign my_array = 'zebra, octopus, giraffe, tiger' | " + splitFilter + ": ', ' %}{% if my_array." + firstFilter + " == 'zebra' %}There goes a zebra!{% endif %}",
                namingConvention: namingConvention);
        }

        [Test]
        public void TestReplace()
        {
            TestReplace(_contextV20);
        }

        public void TestReplace(Context context)
        {
            Assert.Null(StandardFilters.Replace(context: context, input: null, @string: "a", replacement: "b"));
            Assert.AreEqual(expected: "", actual: StandardFilters.Replace(context: context, input: "", @string: "a", replacement: "b"));
            Assert.AreEqual(expected: "a a a a", actual: StandardFilters.Replace(context: context, input: "a a a a", @string: null, replacement: "b"));
            Assert.AreEqual(expected: "a a a a", actual: StandardFilters.Replace(context: context, input: "a a a a", @string: "", replacement: "b"));
            Assert.AreEqual(expected: "b b b b", actual: StandardFilters.Replace(context: context, input: "a a a a", @string: "a", replacement: "b"));

            Assert.AreEqual(expected: "Tesvalue\\\"", actual: StandardFilters.Replace(context: context, input: "Tesvalue\"", @string: "\"", replacement: "\\\""));
            Helper.AssertTemplateResult(expected: "Tesvalue\\\"", template: "{{ 'Tesvalue\"' | replace: '\"', '\\\"' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(
                expected: "Tesvalue\\\"",
                template: "{{ context | replace: '\"', '\\\"' }}",
                localVariables: Hash.FromAnonymousObject(new { context = "Tesvalue\"" }),
                syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestReplaceRegexV20()
        {
            var context = _contextV20;
            Assert.AreEqual(expected: "b b b b", actual: StandardFilters.Replace(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"));
        }

        [Test]
        public void TestReplaceRegexV21()
        {
            var context = _contextV21;
            Assert.AreEqual(expected: "a A A a", actual: StandardFilters.Replace(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"));
            TestReplace(context);
        }

        [Test]
        public void TestReplaceChain()
        {
            var assign = @"{%assign az='azerty'%}";
            Helper.AssertTemplateResult("qzerty", assign + "{{az |replace: 'a','q'}}");
            Helper.AssertTemplateResult("q zerty", assign + "{{az |replace: 'a','q '}}");
            Helper.AssertTemplateResult("qw erty", assign + "{{az |replace: 'a','q'   |replace: 'z','w '}}");
            Helper.AssertTemplateResult("q werty", assign + "{{az |replace: 'a','q '  |replace: 'z','w'}}");
            Helper.AssertTemplateResult("q rwerty", assign + "{{az |replace: 'a','q r' |replace: 'z','w'}}");
            Helper.AssertTemplateResult(" qwerty", assign + "{{az |replace: 'a',' q'  |replace: 'z','w'}}");
        }

        [Test]
        public void TestReplaceFirst()
        {
            TestReplaceFirst(_contextV20);
        }

        public void TestReplaceFirst(Context context)
        {
            Assert.Null(StandardFilters.ReplaceFirst(context: context, input: null, @string: "a", replacement: "b"));
            Assert.AreEqual("", StandardFilters.ReplaceFirst(context: context, input: "", @string: "a", replacement: "b"));
            Assert.AreEqual("a a a a", StandardFilters.ReplaceFirst(context: context, input: "a a a a", @string: null, replacement: "b"));
            Assert.AreEqual("a a a a", StandardFilters.ReplaceFirst(context: context, input: "a a a a", @string: "", replacement: "b"));
            Assert.AreEqual("b a a a", StandardFilters.ReplaceFirst(context: context, input: "a a a a", @string: "a", replacement: "b"));
            Helper.AssertTemplateResult(expected: "b a a a", template: "{{ 'a a a a' | replace_first: 'a', 'b' }}", syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestReplaceFirstRegexV20()
        {
            var context = _contextV20;
            Assert.AreEqual(expected: "b A A a", actual: StandardFilters.ReplaceFirst(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"));
        }

        [Test]
        public void TestReplaceFirstRegexV21()
        {
            var context = _contextV21;
            Assert.AreEqual(expected: "a A A a", actual: StandardFilters.ReplaceFirst(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"));
            TestReplaceFirst(context);
        }

        [Test]
        public void TestRemove()
        {
            TestRemove(_contextV20);
        }

        public void TestRemove(Context context)
        {

            Assert.AreEqual("   ", StandardFilters.Remove("a a a a", "a"));
            Assert.AreEqual("a a a", StandardFilters.RemoveFirst(context: context, input: "a a a a", @string: "a "));
            Helper.AssertTemplateResult(expected: "a a a", template: "{{ 'a a a a' | remove_first: 'a ' }}", syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestRemoveFirstRegexV20()
        {
            var context = _contextV20;
            Assert.AreEqual(expected: "r. Jones", actual: StandardFilters.RemoveFirst(context: context, input: "Mr. Jones", @string: "."));
        }

        [Test]
        public void TestRemoveFirstRegexV21()
        {
            var context = _contextV21;
            Assert.AreEqual(expected: "Mr Jones", actual: StandardFilters.RemoveFirst(context: context, input: "Mr. Jones", @string: "."));
            TestRemove(context);
        }

        [Test]
        public void TestPipesInStringArguments()
        {
            Helper.AssertTemplateResult("foobar", "{{ 'foo|bar' | remove: '|' }}");
        }

        [Test]
        public void TestStripWindowsNewlines()
        {
            Helper.AssertTemplateResult("abc", "{{ source | strip_newlines }}", Hash.FromAnonymousObject(new { source = "a\r\nb\r\nc" }));
            Helper.AssertTemplateResult("ab", "{{ source | strip_newlines }}", Hash.FromAnonymousObject(new { source = "a\r\n\r\n\r\nb" }));
        }

        [Test]
        public void TestStripUnixNewlines()
        {
            Helper.AssertTemplateResult("abc", "{{ source | strip_newlines }}", Hash.FromAnonymousObject(new { source = "a\nb\nc" }));
            Helper.AssertTemplateResult("ab", "{{ source | strip_newlines }}", Hash.FromAnonymousObject(new { source = "a\n\n\nb" }));
        }

        [Test]
        public void TestWindowsNewlinesToBr()
        {
            Helper.AssertTemplateResult("a<br />\r\nb<br />\r\nc",
                "{{ source | newline_to_br }}",
                Hash.FromAnonymousObject(new { source = "a\r\nb\r\nc" }));
        }

        [Test]
        public void TestUnixNewlinesToBr()
        {
            Helper.AssertTemplateResult("a<br />\nb<br />\nc",
                "{{ source | newline_to_br }}",
                Hash.FromAnonymousObject(new { source = "a\nb\nc" }));
        }

        [Test]
        public void TestPlus()
        {
            TestPlus(_contextV20);
        }

        private void TestPlus(Context context)
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult(expected: "2", template: "{{ 1 | plus:1 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "5.5", template: "{{ 2  | plus:3.5 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "5.5", template: "{{ 3.5 | plus:2 }}", syntax: context.SyntaxCompatibilityLevel);

                // Test that decimals are not introducing rounding-precision issues
                Helper.AssertTemplateResult(expected: "148397.77", template: "{{ 148387.77 | plus:10 }}", syntax: context.SyntaxCompatibilityLevel);

                Helper.AssertTemplateResult(
                    expected: "2147483648",
                    template: "{{ i | plus: i2 }}",
                    localVariables: Hash.FromAnonymousObject(new { i = (int)Int32.MaxValue, i2 = (Int64)1 }),
                    syntax: context.SyntaxCompatibilityLevel);
            }
        }

        [Test]
        public void TestPlusStringV20()
        {
            var context = _contextV20;
            Helper.AssertTemplateResult(expected: "11", template: "{{ '1' | plus: 1 }}", syntax: context.SyntaxCompatibilityLevel);
            var renderParams = new RenderParameters(CultureInfo.InvariantCulture) { ErrorsOutputMode = ErrorsOutputMode.Rethrow, SyntaxCompatibilityLevel = context.SyntaxCompatibilityLevel };
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ 1 | plus: '1' }}").Render(renderParams));
        }

        [Test]
        public void TestPlusStringV21()
        {
            var context = _contextV21;
            Helper.AssertTemplateResult(expected: "2", template: "{{ '1' | plus: 1 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "2", template: "{{ 1 | plus: '1' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "2", template: "{{ '1' | plus: '1' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "5.5", template: "{{ 2 | plus: '3.5' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "5.5", template: "{{ '3.5' | plus: 2 }}", syntax: context.SyntaxCompatibilityLevel);
            TestPlus(context);
        }

        [Test]
        public void TestMinus()
        {
            TestMinus(_contextV20);
        }

        private void TestMinus(Context context)
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult(expected: "4", template: "{{ input | minus:operand }}", localVariables: Hash.FromAnonymousObject(new { input = 5, operand = 1 }), syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "-1.5", template: "{{ 2  | minus:3.5 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "1.5", template: "{{ 3.5 | minus:2 }}", syntax: context.SyntaxCompatibilityLevel);
            }
        }

        [Test]
        public void TestMinusStringV20()
        {
            var renderParams = new RenderParameters(CultureInfo.InvariantCulture) { ErrorsOutputMode = ErrorsOutputMode.Rethrow, SyntaxCompatibilityLevel = _contextV20.SyntaxCompatibilityLevel };
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ '2' | minus: 1 }}").Render(renderParams));
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ 2 | minus: '1' }}").Render(renderParams));
        }

        [Test]
        public void TestMinusStringV21()
        {
            var context = _contextV21;
            Helper.AssertTemplateResult(expected: "1", template: "{{ '2' | minus: 1 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "1", template: "{{ 2 | minus: '1' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "-1.5", template: "{{ 2 | minus: '3.5' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "-1.5", template: "{{ '2.5' | minus: 4 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "-1", template: "{{ '2.5' | minus: '3.5' }}", syntax: context.SyntaxCompatibilityLevel);
            TestMinus(context);
        }

        [Test]
        public void TestPlusCombinedWithMinus()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                // This detects rounding issues not visible with single operation.
                Helper.AssertTemplateResult("0.1", "{{ 0.1 | plus: 10 | minus: 10 }}");
            }
        }

        [Test]
        public void TestMinusWithFrenchDecimalSeparator()
        {
            using (CultureHelper.SetCulture("fr-FR"))
            {
                Helper.AssertTemplateResult(string.Format("1{0}2", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    "{{ 3,2 | minus:2 | round:1 }}");
            }
        }

        [Test]
        public void TestRound()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult("1.235", "{{ 1.234678 | round:3 }}");
                Helper.AssertTemplateResult("1", "{{ 1 | round }}");

                Assert.Null(StandardFilters.Round("1.2345678", "two"));
            }
        }

        [Test]
        public void TestCeil()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult("2", "{{ 1.2 | ceil }}");
                Helper.AssertTemplateResult("2", "{{ 2.0 | ceil }}");
                Helper.AssertTemplateResult("184", "{{ 183.357 | ceil }}");
                Helper.AssertTemplateResult("4", "{{ \"3.5\" | ceil }}");

                Assert.Null(StandardFilters.Ceil(_contextV20, ""));
                Assert.Null(StandardFilters.Ceil(_contextV20, "two"));
            }
        }

        [Test]
        public void TestFloor()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult("1", "{{ 1.2 | floor }}");
                Helper.AssertTemplateResult("2", "{{ 2.0 | floor }}");
                Helper.AssertTemplateResult("183", "{{ 183.357 | floor }}");
                Helper.AssertTemplateResult("3", "{{ \"3.5\" | floor }}");

                Assert.Null(StandardFilters.Floor(_contextV20, ""));
                Assert.Null(StandardFilters.Floor(_contextV20, "two"));
            }
        }

        [Test]
        public void TestTimes()
        {
            TestTimes(_contextV20);
        }

        private void TestTimes(Context context)
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult(expected: "12", template: "{{ 3 | times:4 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "125", template: "{{ 10 | times:12.5 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "125", template: "{{ 10.0 | times:12.5 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "125", template: "{{ 12.5 | times:10 }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: "125", template: "{{ 12.5 | times:10.0 }}", syntax: context.SyntaxCompatibilityLevel);

                // Test against overflows when we try to be precise but the result exceeds the range of the input type.
                Helper.AssertTemplateResult(
                    expected: ((double)((decimal.MaxValue / 100) + (decimal).1) * (double)((decimal.MaxValue / 100) + (decimal).1)).ToString(),
                    template: $"{{{{ {(decimal.MaxValue / 100) + (decimal).1} | times:{(decimal.MaxValue / 100) + (decimal).1} }}}}",
                    syntax: context.SyntaxCompatibilityLevel);

                // Test against overflows going beyond the double precision float type's range
                Helper.AssertTemplateResult(
                    expected: double.NegativeInfinity.ToString(),
                    template: $"{{{{ 12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.0 | times:-12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.0 }}}}",
                    syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(
                    expected: double.PositiveInfinity.ToString(),
                    template: $"{{{{ 12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.0 | times:12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.0 }}}}",
                    syntax: context.SyntaxCompatibilityLevel);

                // Ensures no underflow exception is thrown when the result doesn't fit the precision of double.
                Helper.AssertTemplateResult(expected: "0",
                    template: $"{{{{ 0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001 | times:0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001 }}}}",
                    syntax: context.SyntaxCompatibilityLevel);
            }

            Assert.AreEqual(8.43, StandardFilters.Times(context: context, input: 0.843m, operand: 10));
            Assert.AreEqual(412, StandardFilters.Times(context: context, input: 4.12m, operand: 100));
            Assert.AreEqual(7556.3, StandardFilters.Times(context: context, input: 7.5563m, operand: 1000));
        }

        [Test]
        public void TestTimesStringV20()
        {
            var context = _contextV20;
            Helper.AssertTemplateResult(expected: "foofoofoofoo", template: "{{ 'foo' | times:4 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "3333", template: "{{ '3' | times:4 }}", syntax: context.SyntaxCompatibilityLevel);
            var renderParams = new RenderParameters(CultureInfo.InvariantCulture) { ErrorsOutputMode = ErrorsOutputMode.Rethrow, SyntaxCompatibilityLevel = context.SyntaxCompatibilityLevel };
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ 3 | times: '4' }}").Render(renderParams));
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ '3' | times: '4' }}").Render(renderParams));
        }

        [Test]
        public void TestTimesStringV21()
        {
            var context = _contextV21;
            Helper.AssertTemplateResult(expected: "12", template: "{{ '3' | times: 4 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "12", template: "{{ 3 | times: '4' }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "12", template: "{{ '3' | times: '4' }}", syntax: context.SyntaxCompatibilityLevel);
            TestTimes(context);
        }

        [Test]
        public void TestAppend()
        {
            Hash assigns = Hash.FromAnonymousObject(new { a = "bc", b = "d" });
            Helper.AssertTemplateResult(expected: "bcd", template: "{{ a | append: 'd'}}", localVariables: assigns);
            Helper.AssertTemplateResult(expected: "bcd", template: "{{ a | append: b}}", localVariables: assigns);
            Helper.AssertTemplateResult(expected: "/my/fancy/url.html", template: "{{ '/my/fancy/url' | append: '.html' }}");
            Helper.AssertTemplateResult(expected: "website.com/index.html", template: "{% assign filename = '/index.html' %}{{ 'website.com' | append: filename }}");
        }

        [Test]
        public void TestPrepend()
        {
            Hash assigns = Hash.FromAnonymousObject(new { a = "bc", b = "a" });
            Helper.AssertTemplateResult("abc", "{{ a | prepend: 'a'}}", assigns);
            Helper.AssertTemplateResult("abc", "{{ a | prepend: b}}", assigns);
        }

        [Test]
        public void TestDividedBy()
        {
            TestDividedBy(_contextV20);
        }

        private void TestDividedBy(Context context)
        {
            Helper.AssertTemplateResult(expected: "4", template: "{{ 12 | divided_by:3 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "4", template: "{{ 14 | divided_by:3 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "5", template: "{{ 15 | divided_by:3 }}", syntax: context.SyntaxCompatibilityLevel);
            Assert.Null(StandardFilters.DividedBy(context: context, input: null, operand: 3));
            Assert.Null(StandardFilters.DividedBy(context: context, input: 4, operand: null));

            // Ensure we preserve floating point behavior for division by zero, and don't start throwing exceptions.
            Helper.AssertTemplateResult(expected: double.PositiveInfinity.ToString(), template: "{{ 1.0 | divided_by:0.0 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: double.NegativeInfinity.ToString(), template: "{{ -1.0 | divided_by:0.0 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "NaN", template: "{{ 0.0 | divided_by:0.0 }}", syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestDividedByStringV20()
        {
            var renderParams = new RenderParameters(CultureInfo.InvariantCulture) { ErrorsOutputMode = ErrorsOutputMode.Rethrow, SyntaxCompatibilityLevel = _contextV20.SyntaxCompatibilityLevel };
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ '12' | divided_by: 3 }}").Render(renderParams));
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ 12 | divided_by: '3' }}").Render(renderParams));
        }

        [Test]
        public void TestDividedByStringV21()
        {
            var context = _contextV21;
            Helper.AssertTemplateResult(expected: "4", template: "{{ '12' | divided_by: 3 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "4", template: "{{ 12 | divided_by: '3' }}", syntax: context.SyntaxCompatibilityLevel);
            TestDividedBy(context);
        }

        [Test]
        public void TestInt32DividedByInt64()
        {
            int a = 20;
            long b = 5;
            var c = a / b;
            Assert.AreEqual(c, (long)4);


            Hash assigns = Hash.FromAnonymousObject(new { a = a, b = b });
            Helper.AssertTemplateResult("4", "{{ a | divided_by:b }}", assigns);
        }

        [Test]
        public void TestModulo()
        {
            TestModulo(_contextV20);
        }

        private void TestModulo(Context context)
        {
            Helper.AssertTemplateResult(expected: "1", template: "{{ 3 | modulo:2 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "7.77", template: "{{ 148387.77 | modulo:10 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "5.32", template: "{{ 3455.32 | modulo:10 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "3.12", template: "{{ 23423.12 | modulo:10 }}", syntax: context.SyntaxCompatibilityLevel);
            Assert.Null(StandardFilters.Modulo(context: context, input: null, operand: 3));
            Assert.Null(StandardFilters.Modulo(context: context, input: 4, operand: null));
        }

        public void TestModuloStringV20()
        {
            var renderParams = new RenderParameters(CultureInfo.InvariantCulture) { ErrorsOutputMode = ErrorsOutputMode.Rethrow, SyntaxCompatibilityLevel = _contextV20.SyntaxCompatibilityLevel };
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ '3' | modulo: 2 }}").Render(renderParams));
            Assert.Throws<InvalidOperationException>(() => Template.Parse("{{ 3 | modulo: '2' }}").Render(renderParams));
        }

        [Test]
        public void TestModuloStringV21()
        {
            var context = _contextV21;
            Helper.AssertTemplateResult(expected: "1", template: "{{ '3' | modulo: 2 }}", syntax: context.SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(expected: "1", template: "{{ 3 | modulo: '2' }}", syntax: context.SyntaxCompatibilityLevel);
            TestModulo(context);
        }

        [Test]
        public void TestUrlencode()
        {
            Assert.AreEqual("http%3A%2F%2Fdotliquidmarkup.org%2F", StandardFilters.UrlEncode("http://dotliquidmarkup.org/"));
            Assert.AreEqual("Tetsuro+Takara", StandardFilters.UrlEncode("Tetsuro Takara"));
            Assert.AreEqual("john%40liquid.com", StandardFilters.UrlEncode("john@liquid.com"));
            Assert.AreEqual(null, StandardFilters.UrlEncode(null));
        }

        [Test]
        public void TestUrldecode()
        {
            Assert.AreEqual("'Stop!' said Fred", StandardFilters.UrlDecode("%27Stop%21%27+said+Fred"));
            Assert.AreEqual(null, StandardFilters.UrlDecode(null));
        }


        [Test]
        public void TestDefault()
        {
            Hash assigns = Hash.FromAnonymousObject(new { var1 = "foo", var2 = "bar" });
            Helper.AssertTemplateResult("foo", "{{ var1 | default: 'foobar' }}", assigns);
            Helper.AssertTemplateResult("bar", "{{ var2 | default: 'foobar' }}", assigns);
            Helper.AssertTemplateResult("foobar", "{{ unknownvariable | default: 'foobar' }}", assigns);
        }

        [Test]
        public void TestCapitalizeV20()
        {
            var context = _contextV20;
            Assert.AreEqual(null, StandardFilters.Capitalize(context: context, input: null));
            Assert.AreEqual("", StandardFilters.Capitalize(context: context, input: ""));
            Assert.AreEqual(" ", StandardFilters.Capitalize(context: context, input: " "));
            Assert.AreEqual("That Is One Sentence.", StandardFilters.Capitalize(context: context, input: "That is one sentence."));

            Helper.AssertTemplateResult(
                expected: "Title",
                template: "{{ 'title' | capitalize }}",
                syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestCapitalizeV21()
        {
            var context = _contextV21;
            Assert.AreEqual(null, StandardFilters.Capitalize(context: context, input: null));
            Assert.AreEqual("", StandardFilters.Capitalize(context: context, input: ""));
            Assert.AreEqual(" ", StandardFilters.Capitalize(context: context, input: " "));
            Assert.AreEqual(" My boss is Mr. Doe.", StandardFilters.Capitalize(context: context, input: " my boss is Mr. Doe."));

            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{{ 'my great title' | capitalize }}",
                syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestCapitalizeV22()
        {
            var context = _contextV22;
            Assert.AreEqual(null, StandardFilters.Capitalize(context: context, input: null));
            Assert.AreEqual("", StandardFilters.Capitalize(context: context, input: ""));
            Assert.AreEqual(" ", StandardFilters.Capitalize(context: context, input: " "));
            Assert.AreEqual("My boss is mr. doe.", StandardFilters.Capitalize(context: context, input: "my boss is Mr. Doe."));

            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{{ 'my Great Title' | capitalize }}",
                syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestUniq()
        {
            CollectionAssert.AreEqual(new[] { "ants", "bugs", "bees" }, StandardFilters.Uniq(new string[] { "ants", "bugs", "bees", "bugs", "ants" }));
            CollectionAssert.AreEqual(new string[] { }, StandardFilters.Uniq(new string[] { }));
            Assert.AreEqual(null, StandardFilters.Uniq(null));
            Assert.AreEqual(new List<object> { 5 }, StandardFilters.Uniq(5));
        }

        [Test]
        public void TestAbs()
        {
            Assert.AreEqual(0, StandardFilters.Abs(_contextV20, "notNumber"));
            Assert.AreEqual(10, StandardFilters.Abs(_contextV20, 10));
            Assert.AreEqual(5, StandardFilters.Abs(_contextV20, -5));
            Assert.AreEqual(19.86, StandardFilters.Abs(_contextV20, 19.86));
            Assert.AreEqual(19.86, StandardFilters.Abs(_contextV20, -19.86));
            Assert.AreEqual(10, StandardFilters.Abs(_contextV20, "10"));
            Assert.AreEqual(5, StandardFilters.Abs(_contextV20, "-5"));
            Assert.AreEqual(30.60, StandardFilters.Abs(_contextV20, "30.60"));
            Assert.AreEqual(0, StandardFilters.Abs(_contextV20, "30.60a"));

            Helper.AssertTemplateResult(
                expected: "17",
                template: "{{ -17 | abs }}");
            Helper.AssertTemplateResult(
                expected: "17",
                template: "{{ 17 | abs }}");
            Helper.AssertTemplateResult(
                expected: "4",
                template: "{{ 4 | abs }}");
            Helper.AssertTemplateResult(
                expected: "19.86",
                template: "{{ '-19.86' | abs }}");
        }

        [Test]
        public void TestAtLeast()
        {
            Assert.AreEqual("notNumber", StandardFilters.AtLeast(_contextV20, "notNumber", 5));
            Assert.AreEqual(5, StandardFilters.AtLeast(_contextV20, 5, 5));
            Assert.AreEqual(5, StandardFilters.AtLeast(_contextV20, 3, 5));
            Assert.AreEqual(6, StandardFilters.AtLeast(_contextV20, 6, 5));
            Assert.AreEqual(10, StandardFilters.AtLeast(_contextV20, 10, 5));
            Assert.AreEqual(9.85, StandardFilters.AtLeast(_contextV20, 9.85, 5));
            Assert.AreEqual(5, StandardFilters.AtLeast(_contextV20, 3.56, 5));
            Assert.AreEqual(10, StandardFilters.AtLeast(_contextV20, "10", 5));
            Assert.AreEqual(5, StandardFilters.AtLeast(_contextV20, "4", 5));
            Assert.AreEqual("10a", StandardFilters.AtLeast(_contextV20, "10a", 5));
            Assert.AreEqual("4b", StandardFilters.AtLeast(_contextV20, "4b", 5));

            Helper.AssertTemplateResult(
                expected: "5",
                template: "{{ 4 | at_least: 5 }}");
            Helper.AssertTemplateResult(
                expected: "4",
                template: "{{ 4 | at_least: 3 }}");
        }

        [Test]
        public void TestAtMost()
        {
            Assert.AreEqual("notNumber", StandardFilters.AtMost(_contextV20, "notNumber", 5));
            Assert.AreEqual(5, StandardFilters.AtMost(_contextV20, 5, 5));
            Assert.AreEqual(3, StandardFilters.AtMost(_contextV20, 3, 5));
            Assert.AreEqual(5, StandardFilters.AtMost(_contextV20, 6, 5));
            Assert.AreEqual(5, StandardFilters.AtMost(_contextV20, 10, 5));
            Assert.AreEqual(5, StandardFilters.AtMost(_contextV20, 9.85, 5));
            Assert.AreEqual(3.56, StandardFilters.AtMost(_contextV20, 3.56, 5));
            Assert.AreEqual(5, StandardFilters.AtMost(_contextV20, "10", 5));
            Assert.AreEqual(4, StandardFilters.AtMost(_contextV20, "4", 5));
            Assert.AreEqual("4a", StandardFilters.AtMost(_contextV20, "4a", 5));
            Assert.AreEqual("10b", StandardFilters.AtMost(_contextV20, "10b", 5));

            Helper.AssertTemplateResult(
                expected: "4",
                template: "{{ 4 | at_most: 5 }}");
            Helper.AssertTemplateResult(
                expected: "3",
                template: "{{ 4 | at_most: 3 }}");
        }

        [Test]
        public void TestCompact()
        {
            CollectionAssert.AreEqual(new[] { "business", "celebrities", "lifestyle", "sports", "technology" }, StandardFilters.Compact(new string[] { "business", null, "celebrities", null, null, "lifestyle", "sports", null, "technology", null }));
            CollectionAssert.AreEqual(new[] { "business", "celebrities" }, StandardFilters.Compact(new string[] { "business", "celebrities" }));
            Assert.AreEqual(new List<object> { 5 }, StandardFilters.Compact(5));
            CollectionAssert.AreEqual(new string[] { }, StandardFilters.Compact(new string[] { }));
            Assert.AreEqual(null, StandardFilters.Compact(null));

            var siteAnonymousObject = new
            {
                site = new
                {
                    pages = new[]
                    {
                        new { title = "Shopify", category = "business" },
                        new { title = "Rihanna", category = "celebrities" },
                        new { title = "foo", category = null as string },
                        new { title = "World traveler", category = "lifestyle" },
                        new { title = "Soccer", category = "sports" },
                        new { title = "foo", category = null as string },
                        new { title = "Liquid", category = "technology" },
                    }
                }
            };

            Helper.AssertTemplateResult(
                expected: @"
- business
- celebrities
- 
- lifestyle
- sports
- 
- technology
",
                template: @"{% assign site_categories = site.pages | map: 'category' %}
{% for category in site_categories %}- {{ category }}
{% endfor %}",
                localVariables: Hash.FromAnonymousObject(siteAnonymousObject));

            Helper.AssertTemplateResult(
                expected: @"
- business
- celebrities
- lifestyle
- sports
- technology
",
                template: @"{% assign site_categories = site.pages | map: 'category' | compact %}
{% for category in site_categories %}- {{ category }}
{% endfor %}",
                localVariables: Hash.FromAnonymousObject(siteAnonymousObject));
        }

        [Test]
        public void TestWhere()
        {
            var products = new[] {
                new { title = "Vacuum", type = "cleaning" },
                new { title = "Spatula", type = "kitchen" },
                new { title = "Television", type = "lounge" },
                new { title = "Garlic press", type = "kitchen" }
            };

            // Check graceful handling of null and empty lists
            Assert.AreEqual(expected: null, actual: StandardFilters.Where(null, propertyName: "property"));
            CollectionAssert.AreEqual(expected: new string[] { }, actual: StandardFilters.Where("a string object", propertyName: "property"));
            CollectionAssert.AreEqual(expected: new string[] { }, actual: StandardFilters.Where(new string[] { }, propertyName: "property"));

            // Ensure error reported if the property name is not provided.
            Assert.Throws<ArgumentNullException>(() => StandardFilters.Where(input: products, propertyName: " "));

            // Test filtering by value of a property
            var expectedKitchenProducts = new[] {
                new { title = "Spatula", type = "kitchen" },
                new { title = "Garlic press", type = "kitchen" }
            };
            CollectionAssert.AreEqual(expected: expectedKitchenProducts,
                actual: StandardFilters.Where(products, propertyName: "type", targetValue: "kitchen"));

            // Test filtering for existence of a property
            CollectionAssert.AreEqual(expected: products,
                actual: StandardFilters.Where(products, propertyName: "type"));

            // Test filtering for non-existent property
            var emptyArray = Array.Empty<object>();
            CollectionAssert.AreEqual(expected: emptyArray,
                actual: StandardFilters.Where(products, propertyName: "non_existent_property"));

            // Confirm what happens to enumerable content that is a value type
            var values = new[] { 1, 2, 3, 4, 5 };
            Assert.AreEqual(expected: new string[] { }, actual: StandardFilters.Where(values, propertyName: "value", targetValue: "xxx"));

            // Ensure null elements are handled gracefully
            var productsWithNullEntry = new[] {
                new { title = "Vacuum", type = "cleaning" },
                new { title = "Spatula", type = "kitchen" },
                null,
                new { title = "Cushion", type = (string)null },
                new { title = "Television", type = "lounge" },
                new { title = "Garlic press", type = "kitchen" }
            };
            Assert.AreEqual(expected: expectedKitchenProducts, actual: StandardFilters.Where(productsWithNullEntry, propertyName: "type", targetValue: "kitchen"));
        }

        [Test]
        public void TestWhere_Indexable()
        {
            var products = new [] {
                new ProductDrop { Title = "Vacuum", Type = "cleaning" },
                new ProductDrop { Title = "Spatula", Type = "kitchen" },
                new ProductDrop { Title = "Television", Type = "lounge" },
                new ProductDrop { Title = "Garlic press", Type = "kitchen" }
            };
            var expectedProducts = products.Where(p => p.Type == "kitchen").ToArray();

            Helper.LockTemplateStaticVars(new RubyNamingConvention(), () =>
            {
                CollectionAssert.AreEqual(
                    expected: expectedProducts,
                    actual: StandardFilters.Where(products, propertyName: "type", targetValue: "kitchen"));
            });
        }

        [Test]
        public void TestWhere_ExpandoObject()
        {
            dynamic product1 = new ExpandoObject();
            product1.title = "Vacuum";
            product1.type = "cleaning";
            dynamic product2 = new ExpandoObject();
            product2.title = "Spatula";
            product2.type = "kitchen";
            dynamic product3 = new ExpandoObject();
            product3.title = "Television";
            product3.type = "lounge";
            dynamic product4 = new ExpandoObject();
            product4.title = "Garlic press";
            product4.type = "kitchen";
            var products = new List<ExpandoObject> { product1, product2, product3, product4 };
            var expectedProducts = new List<ExpandoObject> { product2, product4 };

            Assert.AreEqual(
                expected: expectedProducts,
                actual: StandardFilters.Where(products, propertyName: "type", targetValue: "kitchen"));
        }

        // First sample from specification at https://shopify.github.io/liquid/filters/where/
        // In this example, assume you have a list of products and you want to show your kitchen products separately.
        // Using where, you can create an array containing only the products that have a "type" of "kitchen"
        [Test]
        public void TestWhere_ShopifySample1()
        {
            var products = new[] {
                new { title = "Vacuum", type = "cleaning" },
                new { title = "Spatula", type = "kitchen" },
                new { title = "Television", type = "lounge" },
                new { title = "Garlic press", type = "kitchen" }
            };

            // First Shopify sample
            Helper.AssertTemplateResult(
                expected: "\r\n\r\nKitchen products:\r\n\r\n- Spatula\r\n\r\n- Garlic press\r\n",
                template: @"{% assign kitchen_products = products | where: ""type"", ""kitchen"" %}

Kitchen products:
{% for product in kitchen_products %}
- {{ product.title }}
{% endfor %}",
                localVariables: Hash.FromAnonymousObject(new { products }));
        }

        // Second sample from specification at https://shopify.github.io/liquid/filters/where/
        // Say instead you have a list of products and you only want to show those that are available to buy.
        // You can where with a property name but no target value to include all products with a truthy "available" value.
        [Test]
        public void TestWhere_ShopifySample2()
        {
            List<Hash> products = new List<Hash> {
                new Hash { { "title", "Coffee mug" }, { "available", true } },
                new Hash { { "title", "Limited edition sneakers" } }, // no 'available' property
                new Hash { { "title", "Limited edition sneakers" }, { "available", false } }, // 'available' = false
                new Hash { { "title", "Boring sneakers" }, { "available", true } }
            };

            Helper.AssertTemplateResult(
                expected: "\r\n\r\nAvailable products:\r\n\r\n- Coffee mug\r\n\r\n- Boring sneakers\r\n",
                template: @"{% assign available_products = products | where: ""available"" %}

Available products:
{% for product in available_products %}
- {{ product.title }}
{% endfor %}",
                localVariables: Hash.FromAnonymousObject(new { products }));
        }

        // Third sample from specification at https://shopify.github.io/liquid/filters/where/
        // The where filter can also be used to find a single object in an array when combined with the first filter.
        // For example, say you want to show off the shirt in your new fall collection.
        [Test]
        public void TestWhere_ShopifySample3()
        {
            List<Hash> products = new List<Hash> {
                new Hash { { "title", "Little black dress" }, { "type", "dress" } },
                new Hash { { "title", "Tartan flat cap" } }, // no 'type' property
                new Hash { { "title", "leather driving gloves" }, { "type", null } }, // 'type' exists, value is null
                new Hash { { "title", "Hawaiian print sweater vest" }, { "type", "shirt" }  }
            };

            Helper.AssertTemplateResult(
                expected: "\r\n\r\nFeatured product: Hawaiian print sweater vest",
                template: @"{% assign new_shirt = products | where: ""type"", ""shirt"" | first %}

Featured product: {{ new_shirt.title }}",
                localVariables: Hash.FromAnonymousObject(new { products }));
        }

        [Test]
        public void TestWhere_NonStringCompare()
        {
            var products = new[] {
                new { title = "Vacuum", type = "cleaning", price = 199.99f },
                new { title = "Spatula", type = "kitchen", price = 7.0f },
                new { title = "Television", type = "lounge", price = 1299.99f },
                new { title = "Garlic press", type = "kitchen", price = 25.00f }
            };

            // The products array has a price of 199.99f, compare to the value 199.99
            Helper.AssertTemplateResult(
                expected: "\r\n\r\nCheapest products:\r\n\r\n- Vacuum\r\n",
                template: @"{% assign cheap_products = products | where: ""price"", 199.99 %}

Cheapest products:
{% for product in cheap_products %}
- {{ product.title }}
{% endfor %}",
                localVariables: Hash.FromAnonymousObject(new { products }));

            // The products array has a price of 7.0f, compare to the integer 7
            Helper.AssertTemplateResult(
                expected: "\r\n\r\nCheapest products:\r\n\r\n- Spatula\r\n",
                template: @"{% assign cheap_products = products | where: ""price"", 7 %}

Cheapest products:
{% for product in cheap_products %}
- {{ product.title }}
{% endfor %}",
                localVariables: Hash.FromAnonymousObject(new { products }));

            // The products array has a price of 7.0f, compare to the string '7.0'
            Helper.AssertTemplateResult(
                expected: "\r\n\r\nCheapest products:\r\n\r\n- Spatula\r\n",
                template: @"{% assign cheap_products = products | where: ""price"", '7.0' %}

Cheapest products:
{% for product in cheap_products %}
- {{ product.title }}
{% endfor %}",
                localVariables: Hash.FromAnonymousObject(new { products }));
        }

        [Test]
        public void TestWhere_RespectIndexable()
        {
            var products = new [] {
                new ProductDrop { Title = "Vacuum", Type = "cleaning" },
                new ProductDrop { Title = "Spatula", Type = "kitchen" },
                new ProductDrop { Title = "Television", Type = "lounge" },
                new ProductDrop { Title = "Garlic press", Type = "kitchen" }
            };
            var expectedKitchenProducts = products.Where(p => p.Type == "kitchen").ToArray();

            Helper.LockTemplateStaticVars(new RubyNamingConvention(), () =>
            {
                CollectionAssert.AreEqual(
                    expected: expectedKitchenProducts,
                    actual: StandardFilters.Where(products, propertyName: "type", targetValue: "kitchen"));
            });
        }

        [Test]
        public void TestConcat()
        {
            var array1 = new String[] { "one", "two" };
            var array2 = new String[] { "alpha", "bravo" };

            CollectionAssert.AreEqual(null, StandardFilters.Concat(null, null));
            CollectionAssert.AreEqual(array1, StandardFilters.Concat(array1, null));
            CollectionAssert.AreEqual(array1, StandardFilters.Concat(null, array1));
            CollectionAssert.AreEqual(new[] { "one", "two", "alpha", "bravo" }, StandardFilters.Concat(array1, array2));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, StandardFilters.Concat(new[] { 1, 2 }, new[] { 3, 4 }));
        }

        [Test]
        public void TestConcat_LiquidSample_SingleFilter()
        {
            Helper.AssertTemplateResult(
                expected: "\r\n\r\n\r\n\r\n- apples\r\n\r\n- oranges\r\n\r\n- peaches\r\n\r\n- carrots\r\n\r\n- turnips\r\n\r\n- potatoes\r\n",
                template: @"{% assign fruits = 'apples, oranges, peaches' | split: ', ' %}
{% assign vegetables = 'carrots, turnips, potatoes' | split: ', ' %}
{% assign everything = fruits | concat: vegetables %}
{% for item in everything %}
- {{ item }}
{% endfor %}");
        }

        [Test]
        public void TestConcat_LiquidSample_ChainedFilters()
        {
            Helper.AssertTemplateResult(
                expected: "\r\n\r\n\r\n\r\n\r\n- apples\r\n\r\n- oranges\r\n\r\n- peaches\r\n\r\n- carrots\r\n\r\n- turnips\r\n\r\n- potatoes\r\n\r\n- chairs\r\n\r\n- tables\r\n\r\n- shelves\r\n",
                template: @"{% assign fruits = 'apples, oranges, peaches' | split: ', ' %}
{% assign vegetables = 'carrots, turnips, potatoes' | split: ', ' %}
{% assign furniture = 'chairs, tables, shelves' | split: ', ' %}
{% assign everything = fruits | concat: vegetables | concat: furniture %}
{% for item in everything %}
- {{ item }}
{% endfor %}");
        }

        [Test]
        public void TestReverse()
        {
            var array = new String[] { "one", "two", "three" };
            var arrayReversed = new String[] { "three", "two", "one" };

            CollectionAssert.AreEqual(null, StandardFilters.Reverse(null));
            CollectionAssert.AreEqual(arrayReversed, StandardFilters.Reverse(array));
            CollectionAssert.AreEqual(array, StandardFilters.Reverse(arrayReversed));
            CollectionAssert.AreEqual(new[] { 3, 2, 2, 1 }, StandardFilters.Reverse(new[] { 1, 2, 2, 3 }));
            Assert.AreEqual("Ground control to Major Tom.", StandardFilters.Reverse("Ground control to Major Tom."));
        }

        /// <summary>
        /// Reverses the order of the items in an array. reverse cannot reverse a string.
        /// </summary>
        [Test]
        public void TestReverse_LiquidSample()
        {
            Helper.AssertTemplateResult(
                expected: "\r\nplums, peaches, oranges, apples",
                template: @"{% assign my_array = 'apples, oranges, peaches, plums' | split: ', ' %}
{{ my_array | reverse | join: ', ' }}");
        }

        /// <summary>
        /// Although reverse cannot be used directly on a string, you can split a string into an array,
        ///  reverse the array, and rejoin it by chaining together filters.
        /// </summary>
        [Test]
        public void TestReverse_LiquidSample_StringOnly()
        {
            Helper.AssertTemplateResult(
                expected: ".moT rojaM ot lortnoc dnuorG",
                template: "{{ 'Ground control to Major Tom.' | split: '' | reverse | join: '' }}");
        }

        private class ProductDrop : Drop
        {
            public string Title { get; set; }
            public string Type { get; set; }
        }
    }
}
