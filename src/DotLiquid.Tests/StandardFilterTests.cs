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
        private Context _contextV20EnUS;
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
            _contextV20EnUS = new Context(new CultureInfo("en-US"))
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
            Assert.That(StandardFilters.Size(new[] { 1, 2, 3 }), Is.EqualTo(3));
            Assert.That(StandardFilters.Size(new object[] { }), Is.EqualTo(0));
            Assert.That(StandardFilters.Size(null), Is.EqualTo(0));
        }

        [Test]
        public void TestDowncase()
        {
            Assert.That(StandardFilters.Downcase("Testing"), Is.EqualTo("testing"));
            Assert.That(StandardFilters.Downcase(null), Is.EqualTo(null));
        }

        [Test]
        public void TestUpcase()
        {
            Assert.That(StandardFilters.Upcase("Testing"), Is.EqualTo("TESTING"));
            Assert.That(StandardFilters.Upcase(null), Is.EqualTo(null));
        }

        [Test]
        public void TestTruncate()
        {
            Assert.That(actual: StandardFilters.Truncate(null), Is.EqualTo(expected: null));
            Assert.That(actual: StandardFilters.Truncate(""), Is.EqualTo(expected: ""));
            Assert.That(actual: StandardFilters.Truncate("1234567890", 7), Is.EqualTo(expected: "1234..."));
            Assert.That(actual: StandardFilters.Truncate("1234567890", 20), Is.EqualTo(expected: "1234567890"));
            Assert.That(actual: StandardFilters.Truncate("1234567890", 0), Is.EqualTo(expected: "..."));
            Assert.That(actual: StandardFilters.Truncate("1234567890"), Is.EqualTo(expected: "1234567890"));
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
            Assert.That(StandardFilters.Escape(null), Is.EqualTo(null));
            Assert.That(StandardFilters.Escape(""), Is.EqualTo(""));
            Assert.That(StandardFilters.Escape("<strong>"), Is.EqualTo("&lt;strong&gt;"));
            Assert.That(StandardFilters.H("<strong>"), Is.EqualTo("&lt;strong&gt;"));

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
            Assert.That(StandardFilters.EscapeOnce(null), Is.EqualTo(null));
            Assert.That(StandardFilters.EscapeOnce(""), Is.EqualTo(""));
            Assert.That(StandardFilters.EscapeOnce("&xxx; looks like an escaped character, but isn't"), Is.EqualTo("&amp;xxx; looks like an escaped character, but isn&#39;t"));
            Assert.That(StandardFilters.EscapeOnce("1 &lt; 2 &amp; 3"), Is.EqualTo("1 &lt; 2 &amp; 3"));
            Assert.That(StandardFilters.EscapeOnce("<element>1 &lt; 2 &amp; 3</element>"), Is.EqualTo("&lt;element&gt;1 &lt; 2 &amp; 3&lt;/element&gt;"));

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
            Assert.That(StandardFilters.TruncateWords(null), Is.EqualTo(null));
            Assert.That(StandardFilters.TruncateWords(""), Is.EqualTo(""));
            Assert.That(StandardFilters.TruncateWords("one two three", 4), Is.EqualTo("one two three"));
            Assert.That(StandardFilters.TruncateWords("one two three", 2), Is.EqualTo("one two..."));
            Assert.That(StandardFilters.TruncateWords("one two three"), Is.EqualTo("one two three"));
            Assert.That(StandardFilters.TruncateWords("Two small (13&#8221; x 5.5&#8221; x 10&#8221; high) baskets fit inside one large basket (13&#8221; x 16&#8221; x 10.5&#8221; high) with cover.", 15), Is.EqualTo("Two small (13&#8221; x 5.5&#8221; x 10&#8221; high) baskets fit inside one large basket (13&#8221;..."));

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
            Assert.That(StandardFilters.Split("This is a sentence", " "), Is.EqualTo(new[] { "This", "is", "a", "sentence" }).AsCollection);
            Assert.That(StandardFilters.Split(null, null), Is.EqualTo(new string[] { null }).AsCollection);

            // A string with no pattern should be split into a string[], as required for the Liquid Reverse filter
            Assert.That(StandardFilters.Split("YMCA", null), Is.EqualTo(new[] { "Y", "M", "C", "A" }).AsCollection);
            Assert.That(StandardFilters.Split("YMCA", ""), Is.EqualTo(new[] { "Y", "M", "C", "A" }).AsCollection);
            Assert.That(StandardFilters.Split(" ", ""), Is.EqualTo(new[] { " " }).AsCollection);
        }

        [Test]
        public void TestStripHtml()
        {
            Assert.That(StandardFilters.StripHtml("<div>test</div>"), Is.EqualTo("test"));
            Assert.That(StandardFilters.StripHtml("<div id='test'>test</div>"), Is.EqualTo("test"));
            Assert.That(StandardFilters.StripHtml("<script type='text/javascript'>document.write('some stuff');</script>"), Is.EqualTo(""));
            Assert.That(StandardFilters.StripHtml("<style type='text/css'>foo bar</style>"), Is.EqualTo(""));
            Assert.That(StandardFilters.StripHtml("<STYLE type='text/css'>foo bar</style>"), Is.EqualTo(""));
            Assert.That(StandardFilters.StripHtml("<div\nclass='multiline'>test</div>"), Is.EqualTo("test"));
            Assert.That(StandardFilters.StripHtml("<!-- foo bar \n test -->test"), Is.EqualTo("test"));
            Assert.That(StandardFilters.StripHtml(null), Is.EqualTo(null));

            // Quirk of the existing implementation
            Assert.That(StandardFilters.StripHtml("<<<script </script>script>foo;</script>"), Is.EqualTo("foo;"));
        }

        [Test]
        public void TestStrip()
        {
            Assert.That(StandardFilters.Strip("  test  "), Is.EqualTo("test"));
            Assert.That(StandardFilters.Strip("   test"), Is.EqualTo("test"));
            Assert.That(StandardFilters.Strip("test   "), Is.EqualTo("test"));
            Assert.That(StandardFilters.Strip("test"), Is.EqualTo("test"));
            Assert.That(StandardFilters.Strip(null), Is.EqualTo(null));
        }

        [Test]
        public void TestLStrip()
        {
            Assert.That(StandardFilters.Lstrip("  test  "), Is.EqualTo("test  "));
            Assert.That(StandardFilters.Lstrip("   test"), Is.EqualTo("test"));
            Assert.That(StandardFilters.Lstrip("test   "), Is.EqualTo("test   "));
            Assert.That(StandardFilters.Lstrip("test"), Is.EqualTo("test"));
            Assert.That(StandardFilters.Lstrip(null), Is.EqualTo(null));
        }

        [Test]
        public void TestRStrip()
        {
            Assert.That(StandardFilters.Rstrip("  test  "), Is.EqualTo("  test"));
            Assert.That(StandardFilters.Rstrip("   test"), Is.EqualTo("   test"));
            Assert.That(StandardFilters.Rstrip("test   "), Is.EqualTo("test"));
            Assert.That(StandardFilters.Rstrip("test"), Is.EqualTo("test"));
            Assert.That(StandardFilters.Rstrip(null), Is.EqualTo(null));
        }

        [Test]
        public void TestSlice_V22()
        {
            Context context = _contextV22;

            // Verify backwards compatibility for pre-22a syntax (DotLiquid returns null for null input or empty slice)
            Assert.That(StandardFilters.Slice(context, null, 1), Is.EqualTo(null)); // DotLiquid test case
            Assert.That(StandardFilters.Slice(context, "", 10), Is.EqualTo(null)); // DotLiquid test case

            Assert.That(StandardFilters.Slice(context, null, 0), Is.EqualTo(null)); // Liquid test case
            Assert.That(StandardFilters.Slice(context, "foobar", 100, 10), Is.EqualTo(null)); // Liquid test case

            // Verify DotLiquid is consistent with Liquid for everything else
            TestSliceString(context);
            TestSliceArrays(context);
        }

        [Test]
        public void TestSlice_V22a()
        {
            Context context = _contextV22a;

            // Verify Liquid compliance from V22a syntax:
            Assert.That(StandardFilters.Slice(context, null, 1), Is.EqualTo("")); // DotLiquid test case
            Assert.That(StandardFilters.Slice(context, "", 10), Is.EqualTo("")); // DotLiquid test case

            Assert.That(StandardFilters.Slice(context, null, 0), Is.EqualTo("")); // Liquid test case
            Assert.That(StandardFilters.Slice(context, "foobar", 100, 10), Is.EqualTo("")); // Liquid test case

            // Verify DotLiquid is consistent with Liquid for everything else
            TestSliceString(context);
            TestSliceArrays(context);
        }

        private void TestSliceString(Context context)
        {
            Assert.That(StandardFilters.Slice(context, "abcdefg", 0, 3), Is.EqualTo("abc"));
            Assert.That(StandardFilters.Slice(context, "abcdefg", 1, 3), Is.EqualTo("bcd"));
            Assert.That(StandardFilters.Slice(context, "abcdefg", -3, 3), Is.EqualTo("efg"));
            Assert.That(StandardFilters.Slice(context, "abcdefg", -3, 30), Is.EqualTo("efg"));
            Assert.That(StandardFilters.Slice(context, "abcdefg", 4, 30), Is.EqualTo("efg"));
            Assert.That(StandardFilters.Slice(context, "abc", -4, 2), Is.EqualTo("a"));
            Assert.That(StandardFilters.Slice(context, "abcdefg", -10, 1), Is.EqualTo(""));

            // Test replicated from the Ruby library (https://github.com/Shopify/liquid/blob/master/test/integration/standard_filter_test.rb)
            Assert.That(StandardFilters.Slice(context, "foobar", 1, 3), Is.EqualTo("oob"));
            Assert.That(StandardFilters.Slice(context, "foobar", 1, 1000), Is.EqualTo("oobar"));
            Assert.That(StandardFilters.Slice(context, "foobar", 1, 0), Is.EqualTo(""));
            Assert.That(StandardFilters.Slice(context, "foobar", 1, 1), Is.EqualTo("o"));
            Assert.That(StandardFilters.Slice(context, "foobar", 3, 3), Is.EqualTo("bar"));
            Assert.That(StandardFilters.Slice(context, "foobar", -2, 2), Is.EqualTo("ar"));
            Assert.That(StandardFilters.Slice(context, "foobar", -2, 1000), Is.EqualTo("ar"));
            Assert.That(StandardFilters.Slice(context, "foobar", -1), Is.EqualTo("r"));
            Assert.That(StandardFilters.Slice(context, "foobar", -100, 10), Is.EqualTo(""));
            Assert.That(StandardFilters.Slice(context, "foobar", 1, 3), Is.EqualTo("oob"));
        }

        private void TestSliceArrays(Context context)
        {
            // Test replicated from the Ruby library
            var testArray = new[] { "f", "o", "o", "b", "a", "r" };
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, 1, 3), Is.EqualTo(ToStringArray("oob")).AsCollection);
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, 1, 1000), Is.EqualTo(ToStringArray("oobar")).AsCollection);
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, 1, 0), Is.EqualTo(ToStringArray("")).AsCollection);
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, 1, 1), Is.EqualTo(ToStringArray("o")).AsCollection);
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, 3, 3), Is.EqualTo(ToStringArray("bar")).AsCollection);
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, -2, 2), Is.EqualTo(ToStringArray("ar")).AsCollection);
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, -2, 1000), Is.EqualTo(ToStringArray("ar")).AsCollection);
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, -1), Is.EqualTo(ToStringArray("r")).AsCollection);
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, 100, 10), Is.EqualTo(ToStringArray("")).AsCollection);
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, -100, 10), Is.EqualTo(ToStringArray("")).AsCollection);

            // additional tests
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, -6, 2), Is.EqualTo(ToStringArray("fo")).AsCollection);
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, testArray, -8, 4), Is.EqualTo(ToStringArray("fo")).AsCollection);

            // Non-string arrays tests
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, new[] { 1, 2, 3, 4, 5 }, 1, 3), Is.EqualTo(new[] { 2, 3, 4 }).AsCollection);
            Assert.That((IEnumerable<object>)StandardFilters.Slice(context, new[] { 'a', 'b', 'c', 'd', 'e' }, -4, 3), Is.EqualTo(new[] { 'b', 'c', 'd' }).AsCollection);
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
            Assert.That(StandardFilters.Join(null), Is.EqualTo(null));
            Assert.That(StandardFilters.Join(""), Is.EqualTo(""));
            Assert.That(StandardFilters.Join(new[] { 1, 2, 3, 4 }), Is.EqualTo("1 2 3 4"));
            Assert.That(StandardFilters.Join(new[] { 1, 2, 3, 4 }, " - "), Is.EqualTo("1 - 2 - 3 - 4"));

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
            Assert.That(StandardFilters.Sort(_contextV20, null), Is.EqualTo(null));
            Assert.That(StandardFilters.Sort(_contextV20, new string[] { }), Is.EqualTo(new string[] { }).AsCollection);
            Assert.That(StandardFilters.Sort(_contextV20, ints), Is.EqualTo(new[] { 1, 2, 3, 10 }).AsCollection);
            Assert.That(StandardFilters.Sort(_contextV20, new[] { new { a = 10 }, new { a = 3 }, new { a = 1 }, new { a = 2 } }, "a"), Is.EqualTo(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 10 } }).AsCollection);

            // Issue #393 - Incorrect (Case-Insensitve) Alphabetic Sort
            var strings = new[] { "zebra", "octopus", "giraffe", "Sally Snake" };
            Assert.That(StandardFilters.Sort(_contextV20, strings), Is.EqualTo(new[] { "giraffe", "octopus", "Sally Snake", "zebra" }).AsCollection);

            var hashes = new List<Hash>();
            for (var i = 0; i < strings.Length; i++)
                hashes.Add(CreateHash(ints[i], strings[i]));
            Assert.That(StandardFilters.Sort(_contextV20, hashes, "content"), Is.EqualTo(new[] { hashes[2], hashes[1], hashes[3], hashes[0] }).AsCollection);
            Assert.That(StandardFilters.Sort(_contextV20, hashes, "sortby"), Is.EqualTo(new[] { hashes[3], hashes[2], hashes[1], hashes[0] }).AsCollection);
        }

        [Test]
        public void TestSortV22()
        {
            var ints = new[] { 10, 3, 2, 1 };
            Assert.That(StandardFilters.Sort(_contextV22, null), Is.EqualTo(null));
            Assert.That(StandardFilters.Sort(_contextV22, new string[] { }), Is.EqualTo(new string[] { }).AsCollection);
            Assert.That(StandardFilters.Sort(_contextV22, ints), Is.EqualTo(new[] { 1, 2, 3, 10 }).AsCollection);
            Assert.That(StandardFilters.Sort(_contextV22, new[] { new { a = 10 }, new { a = 3 }, new { a = 1 }, new { a = 2 } }, "a"), Is.EqualTo(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 10 } }).AsCollection);

            var strings = new[] { "zebra", "octopus", "giraffe", "Sally Snake" };
            Assert.That(StandardFilters.Sort(_contextV22, strings), Is.EqualTo(new[] { "Sally Snake", "giraffe", "octopus", "zebra" }).AsCollection);

            var hashes = new List<Hash>();
            for (var i = 0; i < strings.Length; i++)
                hashes.Add(CreateHash(ints[i], strings[i]));
            Assert.That(StandardFilters.Sort(_contextV22, hashes, "content"), Is.EqualTo(new[] { hashes[3], hashes[2], hashes[1], hashes[0] }).AsCollection);
            Assert.That(StandardFilters.Sort(_contextV22, hashes, "sortby"), Is.EqualTo(new[] { hashes[3], hashes[2], hashes[1], hashes[0] }).AsCollection);
        }

        [Test]
        public void TestSortNatural()
        {
            var ints = new[] { 10, 3, 2, 1 };
            Assert.That(StandardFilters.SortNatural(null), Is.EqualTo(null));
            Assert.That(StandardFilters.SortNatural(new string[] { }), Is.EqualTo(new string[] { }).AsCollection);
            Assert.That(StandardFilters.SortNatural(ints), Is.EqualTo(new[] { 1, 2, 3, 10 }).AsCollection);
            Assert.That(StandardFilters.SortNatural(new[] { new { a = 10 }, new { a = 3 }, new { a = 1 }, new { a = 2 } }, "a"), Is.EqualTo(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 10 } }).AsCollection);

            var strings = new[] { "zebra", "octopus", "giraffe", "Sally Snake" };
            Assert.That(StandardFilters.SortNatural(strings), Is.EqualTo(new[] { "giraffe", "octopus", "Sally Snake", "zebra" }).AsCollection);

            var hashes = new List<Hash>();
            for (var i = 0; i < strings.Length; i++)
                hashes.Add(CreateHash(ints[i], strings[i]));
            Assert.That(StandardFilters.SortNatural(hashes, "content"), Is.EqualTo(new[] { hashes[2], hashes[1], hashes[3], hashes[0] }).AsCollection);
            Assert.That(StandardFilters.SortNatural(hashes, "sortby"), Is.EqualTo(new[] { hashes[3], hashes[2], hashes[1], hashes[0] }).AsCollection);
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
            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result[0]["content"], Is.EqualTo(hash1["content"]));
            Assert.That(result[1]["content"], Is.EqualTo(hash2["content"]));
            Assert.That(result[2]["content"], Is.EqualTo(hash3["content"]));
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
            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result[0]["content"], Is.EqualTo(hashWithNoSortByProperty["content"]));
            Assert.That(result[1]["content"], Is.EqualTo(hash1["content"]));
            Assert.That(result[2]["content"], Is.EqualTo(hash2["content"]));
        }

        [Test]
        public void TestSort_Indexable()
        {
            var packages = new[] {
                new Package(numberOfPiecesPerPackage: 2, test: "p1"),
                new Package(numberOfPiecesPerPackage: 1, test: "p2"),
                new Package(numberOfPiecesPerPackage: 3, test: "p3"),
            };
            var expectedPackages = packages.OrderBy(p => p["numberOfPiecesPerPackage"]).ToArray();

            Helper.LockTemplateStaticVars(new RubyNamingConvention(), () =>
            {
                Assert.That(
                    actual: StandardFilters.Sort(_contextV20, packages, "numberOfPiecesPerPackage"), Is.EqualTo(expected: expectedPackages).AsCollection);
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

            Assert.That(
                actual: StandardFilters.Sort(_contextV20, packages, property: "numberOfPiecesPerPackage"), Is.EqualTo(expected: expectedPackages));
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
            Assert.That(StandardFilters.Map(new string[] { }, "a"), Is.EqualTo(new string[] { }).AsCollection);
            Assert.That(StandardFilters.Map(new[] { new { a = 1 }, new { a = 2 }, new { a = 3 }, new { a = 4 } }, "a"), Is.EqualTo(new[] { 1, 2, 3, 4 }).AsCollection);
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

            Assert.That(StandardFilters.Map(null, "a"), Is.EqualTo(null));
            Assert.That(StandardFilters.Map(new object[] { null }, "a"), Is.EqualTo(new object[] { null }).AsCollection);

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
            Assert.That(StandardFilters.Map(new[] { new { a = 1 } }, "no_prop"), Is.EqualTo(nullObjectArray).AsCollection);

            // Drop
            Assert.That(StandardFilters.Map(new[] { new Helper.DataObjectDrop { Prop = "a" } }, "no_prop"), Is.EqualTo(nullObjectArray).AsCollection);

            // Dictionary
            Assert.That(StandardFilters.Map(Hash.FromDictionary(new Dictionary<string, object>() { { "a", 1 } }), "no_prop"), Is.EqualTo(nullObjectArray).AsCollection);

            // Expando Array
            var expandoJson = "[{\"a\": 1}]";
            var expandoObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject[]>(expandoJson);
            Assert.That(StandardFilters.Map(expandoObj, "no_prop"), Is.EqualTo(nullObjectArray).AsCollection);
        }

        /// <summary>
        /// Test case for [Issue #275](https://github.com/dotliquid/dotliquid/issues/275)
        /// </summary>
        [Test]
        public void TestMapDisallowedProperty()
        {
            var hash = Hash.FromAnonymousObject(new
            {
                safe = new[] { new Helper.DataObjectRegistered { PropAllowed = "a", PropDisallowed = "x" } },
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

            Assert.That(
                actual: StandardFilters.Map(products, "title"), Is.EqualTo(expected: new List<string> { "Vacuum", "Spatula", "Television", "Garlic press" }));
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
            Assert.That(StandardFilters.Currency(context: _contextV20, input: input), Is.EqualTo(expected));
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
            // Ignoring the space used, whether narrow non-breaking space or non-breaking space
            using (CultureHelper.SetCulture("en-US"))
            {
                Helper.AssertTemplateResult(
                    expected: expected,
                    template: "{{ input | currency: languageTag | replace: ' ',' ' }}",
                    localVariables: Hash.FromAnonymousObject(new { input = input, languageTag = languageTag }));
            }

            _contextV20.CurrentCulture = new CultureInfo("en-US"); // _contextV20 is initialized with InvariantCulture, these tests require en-US
            Assert.That(StandardFilters.Currency(context: _contextV20, input: input, languageTag: languageTag).Replace("\u202f", "\u00A0"), Is.EqualTo(expected));
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
            Assert.That(
                actual: StandardFilters.Currency(context: _contextV20, input: input, languageTag: "de-DE"), Is.EqualTo(expected: input));
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
            Assert.That(StandardFilters.Currency(context: _contextV20, input: "teststring", languageTag: "de-DE"), Is.EqualTo("teststring"));
        }

        [Test]
        public void TestCurrencyWithinTemplateRender()
        {
            using (CultureHelper.SetCulture("en-US"))
            {
                Template dollarTemplate = Template.Parse(@"{{ amount | currency }}");
                Template euroTemplate = Template.Parse(@"{{ amount | currency: ""de-DE"" }}");

                Assert.That(dollarTemplate.Render(Hash.FromAnonymousObject(new { amount = "7000" })), Is.EqualTo("$7,000.00"));
                Assert.That(euroTemplate.Render(Hash.FromAnonymousObject(new { amount = 7000 })), Is.EqualTo("7.000,00 €"));
            }
        }

        [Test]
        public void TestCurrencyFromDoubleInput()
        {
            Assert.That(StandardFilters.Currency(context: _contextV20, input: 6.8458, languageTag: "en-US"), Is.EqualTo("$6.85"));
            Assert.That(StandardFilters.Currency(context: _contextV20, input: 6.72, languageTag: "en-CA"), Is.EqualTo("$6.72"));
            Assert.That(StandardFilters.Currency(context: _contextV20, input: 6000000, languageTag: "de-DE"), Is.EqualTo("6.000.000,00 €"));
            Assert.That(StandardFilters.Currency(context: _contextV20, input: 6000000.78, languageTag: "de-DE"), Is.EqualTo("6.000.000,78 €"));
        }

        [Test]
        public void TestCurrencyLanguageTag()
        {
            Assert.That(StandardFilters.Currency(context: _contextV20, input: 6000000, languageTag: "de-DE"), Is.EqualTo("6.000.000,00 €")); // language+country
            Assert.That(StandardFilters.Currency(context: _contextV20, input: 6000000, languageTag: "de"), Is.EqualTo("6.000.000,00 €")); // language only
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

            Assert.That(StandardFilters.Date(context: context, input: DateTime.Parse("2006-05-05 10:00:00"), format: "MMMM"), Is.EqualTo(dateTimeFormat.GetMonthName(5)));
            Assert.That(StandardFilters.Date(context: context, input: DateTime.Parse("2006-06-05 10:00:00"), format: "MMMM"), Is.EqualTo(dateTimeFormat.GetMonthName(6)));
            Assert.That(StandardFilters.Date(context: context, input: DateTime.Parse("2006-07-05 10:00:00"), format: "MMMM"), Is.EqualTo(dateTimeFormat.GetMonthName(7)));

            Assert.That(StandardFilters.Date(context: context, input: "2006-05-05 10:00:00", format: "MMMM"), Is.EqualTo(dateTimeFormat.GetMonthName(5)));
            Assert.That(StandardFilters.Date(context: context, input: "2006-06-05 10:00:00", format: "MMMM"), Is.EqualTo(dateTimeFormat.GetMonthName(6)));
            Assert.That(StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "MMMM"), Is.EqualTo(dateTimeFormat.GetMonthName(7)));

            Assert.That(StandardFilters.Date(context: context, input: "08/01/2006 10:00:00", format: string.Empty), Is.EqualTo("08/01/2006 10:00:00"));
            Assert.That(StandardFilters.Date(context: context, input: "08/02/2006 10:00:00", format: null), Is.EqualTo("08/02/2006 10:00:00"));
            Assert.That(StandardFilters.Date(context: context, input: new DateTime(2006, 8, 3, 10, 0, 0), format: string.Empty), Is.EqualTo(new DateTime(2006, 8, 3, 10, 0, 0).ToString(context.CurrentCulture)));
            Assert.That(StandardFilters.Date(context: context, input: new DateTime(2006, 8, 4, 10, 0, 0), format: null), Is.EqualTo(new DateTime(2006, 8, 4, 10, 0, 0).ToString(context.CurrentCulture)));

            Assert.That(StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "MM/dd/yyyy"), Is.EqualTo(new DateTime(2006, 7, 5).ToString("MM/dd/yyyy")));

            Assert.That(StandardFilters.Date(context: context, input: "Fri Jul 16 2004 01:00:00", format: "MM/dd/yyyy"), Is.EqualTo(new DateTime(2004, 7, 16).ToString("MM/dd/yyyy")));

            Assert.That(StandardFilters.Date(context: context, input: null, format: "MMMM"), Is.EqualTo(null));

            Assert.That(StandardFilters.Date(context: context, input: "hi", format: "MMMM"), Is.EqualTo("hi"));

            Assert.That(StandardFilters.Date(context: context, input: "now", format: "MM/dd/yyyy"), Is.EqualTo(DateTime.Now.ToString("MM/dd/yyyy")));
            Assert.That(StandardFilters.Date(context: context, input: "today", format: "MM/dd/yyyy"), Is.EqualTo(DateTime.Now.ToString("MM/dd/yyyy")));
            Assert.That(StandardFilters.Date(context: context, input: "Now", format: "MM/dd/yyyy"), Is.EqualTo(DateTime.Now.ToString("MM/dd/yyyy")));
            Assert.That(StandardFilters.Date(context: context, input: "Today", format: "MM/dd/yyyy"), Is.EqualTo(DateTime.Now.ToString("MM/dd/yyyy")));

            Assert.That(StandardFilters.Date(context: context, input: DateTime.Parse("2006-05-05 10:00:00.345"), format: "ffffff"), Is.EqualTo("345000"));

            Template template = Template.Parse(@"{{ hi | date:""MMMM"" }}");
            Assert.That(template.Render(Hash.FromAnonymousObject(new { hi = "hi" })), Is.EqualTo("hi"));
        }

        [Test]
        public void TestDateV20()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                var context = _contextV20;
                // Legacy parser doesn't except Unix Epoch https://github.com/dotliquid/dotliquid/issues/322
                Assert.That(StandardFilters.Date(context: context, input: 0, format: null), Is.EqualTo("0"));
                Assert.That(StandardFilters.Date(context: context, input: 2147483648, format: null), Is.EqualTo("2147483648")); // Beyond Int32 boundary

                // Legacy parser loses specified offset https://github.com/dotliquid/dotliquid/issues/149
                var testDate = new DateTime(2006, 8, 4, 10, 0, 0);
                Assert.That(StandardFilters.Date(context: context, input: new DateTimeOffset(testDate, TimeSpan.FromHours(-14)), format: "zzz"), Is.EqualTo(new DateTimeOffset(testDate).ToString("zzz")));

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
                Assert.That(StandardFilters.Date(context: context, input: "now", format: null), Is.EqualTo(DateTime.Now.ToString(context.CurrentCulture)));
                Assert.That(StandardFilters.Date(context: context, input: "today", format: null), Is.EqualTo(DateTime.Now.ToString(context.CurrentCulture)));
                Assert.That(StandardFilters.Date(context: context, input: "now", format: string.Empty), Is.EqualTo(DateTime.Now.ToString(context.CurrentCulture)));
                Assert.That(StandardFilters.Date(context: context, input: "today", format: string.Empty), Is.EqualTo(DateTime.Now.ToString(context.CurrentCulture)));
            });
        }

        [Test]
        public void TestDateV21()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                Liquid.UseRubyDateFormat = false;
                var context = _contextV21;// _contextV21 specifies InvariantCulture
                var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
                Assert.That(StandardFilters.Date(context: context, input: 0, format: "g"), Is.EqualTo(unixEpoch.ToString("g", context.FormatProvider)));
                Assert.That(StandardFilters.Date(context: context, input: 2147483648, format: "g"), Is.EqualTo(unixEpoch.AddSeconds(Int32.MaxValue).AddSeconds(1).ToString("g", context.FormatProvider))); // Beyond Int32 boundary
                Assert.That(StandardFilters.Date(context: context, input: 4294967296, format: "g"), Is.EqualTo(unixEpoch.AddSeconds(UInt32.MaxValue).AddSeconds(1).ToString("g", context.FormatProvider))); // Beyond UInt32 boundary
                Helper.AssertTemplateResult(expected: unixEpoch.ToString("g"), template: "{{ 0 | date: 'g' }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: unixEpoch.AddSeconds(Int32.MaxValue).AddSeconds(1).ToString("g"), template: "{{ 2147483648 | date: 'g' }}", syntax: context.SyntaxCompatibilityLevel);
                Helper.AssertTemplateResult(expected: unixEpoch.AddSeconds(UInt32.MaxValue).AddSeconds(1).ToString("g"), template: "{{ 4294967296 | date: 'g' }}", syntax: context.SyntaxCompatibilityLevel);

                var testDate = new DateTime(2006, 8, 4, 10, 0, 0, DateTimeKind.Unspecified);
                Assert.That(StandardFilters.Date(context: context, input: new DateTimeOffset(testDate, TimeSpan.FromHours(-14)), format: "zzz"), Is.EqualTo("-14:00"));
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

                Assert.That(StandardFilters.Date(context: context, input: "now", format: null), Is.EqualTo("now"));
                Assert.That(StandardFilters.Date(context: context, input: "today", format: null), Is.EqualTo("today"));
                Assert.That(StandardFilters.Date(context: context, input: "now", format: string.Empty), Is.EqualTo("now"));
                Assert.That(StandardFilters.Date(context: context, input: "today", format: string.Empty), Is.EqualTo("today"));

                TestDate(context);
            });
        }

#if NET6_0_OR_GREATER
        [Test]
        public void TestDateOnly()
        {
            var currentIsRubyDateFormat = _contextV20.UseRubyDateFormat;
            try
            {
                var dateOnly = new DateOnly(year: 2006, month: 8, day: 3);
                Assert.Multiple(() =>
                {
                    _contextV20.UseRubyDateFormat = false;
                    Assert.That(StandardFilters.Date(context: _contextV20, input: dateOnly, format: "MM/dd/yyyy"), Is.EqualTo("08/03/2006"));
                    Assert.That(StandardFilters.Date(context: _contextV20EnUS, input: dateOnly, format: string.Empty), Is.EqualTo("8/3/2006"));
                    Assert.That(StandardFilters.Date(context: _contextV20EnUS, input: dateOnly, format: null), Is.EqualTo("8/3/2006"));
                    Assert.Throws<FormatException>(() => StandardFilters.Date(context: _contextV20, input: dateOnly, format: "HH:mm:ss"));
                    _contextV20.UseRubyDateFormat = true;
                    Assert.That(StandardFilters.Date(context: _contextV20, input: dateOnly, format: "%D"), Is.EqualTo("08/03/06"));
                    Assert.That(StandardFilters.Date(context: _contextV20EnUS, input: dateOnly, format: string.Empty), Is.EqualTo("8/3/2006"));
                    Assert.That(StandardFilters.Date(context: _contextV20EnUS, input: dateOnly, format: null), Is.EqualTo("8/3/2006"));
                    Assert.Throws<FormatException>(() => StandardFilters.Date(context: _contextV20, input: dateOnly, format: "%T"));
                });
            }
            finally
            {
                _contextV20.UseRubyDateFormat = currentIsRubyDateFormat;
            }
        }

        [Test]
        public void TestTimeOnly()
        {
            var currentIsRubyDateFormat = _contextV20.UseRubyDateFormat;
            try
            {
                var timeOnly = new TimeOnly(hour: 12, minute: 14, second: 15);
                Assert.Multiple(() =>
                {
                    _contextV20.UseRubyDateFormat = false;
                    Assert.That(StandardFilters.Date(context: _contextV20, input: timeOnly, format: "HH:mm:ss"), Is.EqualTo("12:14:15"));
                    Assert.That(StandardFilters.Date(context: _contextV20EnUS, input: timeOnly, format: string.Empty), Is.EqualTo("12:14 PM"));
                    Assert.That(StandardFilters.Date(context: _contextV20EnUS, input: timeOnly, format: null), Is.EqualTo("12:14 PM"));
                    Assert.Throws<FormatException>(() => StandardFilters.Date(context: _contextV20, input: timeOnly, format: "MM/dd/yyyy"));
                    _contextV20.UseRubyDateFormat = true;
                    Assert.That(StandardFilters.Date(context: _contextV20, input: timeOnly, format: "%T"), Is.EqualTo("12:14:15"));
                    Assert.That(StandardFilters.Date(context: _contextV20EnUS, input: timeOnly, format: string.Empty), Is.EqualTo("12:14 PM"));
                    Assert.That(StandardFilters.Date(context: _contextV20EnUS, input: timeOnly, format: null), Is.EqualTo("12:14 PM"));
                    Assert.Throws<FormatException>(() => StandardFilters.Date(context: _contextV20, input: timeOnly, format: "%D"));
                });
            }
            finally
            {
                _contextV20.UseRubyDateFormat = currentIsRubyDateFormat;
            }
        }
#endif

        [Test]
        public void TestStrFTime()
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                var context = _contextV20EnUS;
                context.UseRubyDateFormat = true;

                Assert.That(StandardFilters.Date(context: context, input: DateTime.Parse("2006-05-05 10:00:00"), format: "%B"), Is.EqualTo("May"));
                Assert.That(StandardFilters.Date(context: context, input: DateTime.Parse("2006-06-05 10:00:00"), format: "%B"), Is.EqualTo("June"));
                Assert.That(StandardFilters.Date(context: context, input: DateTime.Parse("2006-07-05 10:00:00"), format: "%B"), Is.EqualTo("July"));

                Assert.That(StandardFilters.Date(context: context, input: "2006-05-05 10:00:00", format: "%B"), Is.EqualTo("May"));
                Assert.That(StandardFilters.Date(context: context, input: "2006-06-05 10:00:00", format: "%B"), Is.EqualTo("June"));
                Assert.That(StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "%B"), Is.EqualTo("July"));

                Assert.That(StandardFilters.Date(context: context, input: "05/07/2006 10:00:00", format: string.Empty), Is.EqualTo("05/07/2006 10:00:00"));
                Assert.That(StandardFilters.Date(context: context, input: "05/07/2006 10:00:00", format: null), Is.EqualTo("05/07/2006 10:00:00"));
                Assert.That(StandardFilters.Date(context: context, input: new DateTime(2006, 8, 3, 10, 0, 0), format: string.Empty), Is.EqualTo(new DateTime(2006, 8, 3, 10, 0, 0).ToString(context.FormatProvider)));
                Assert.That(StandardFilters.Date(context: context, input: new DateTime(2006, 8, 4, 10, 0, 0), format: null), Is.EqualTo(new DateTime(2006, 8, 4, 10, 0, 0).ToString(context.FormatProvider)));

                Assert.That(StandardFilters.Date(context: context, input: "2006-07-05 10:00:00", format: "%m/%d/%Y"), Is.EqualTo("07/05/2006"));

                Assert.That(StandardFilters.Date(context: context, input: "Fri Jul 16 2004 01:00:00", format: "%m/%d/%Y"), Is.EqualTo("07/16/2004"));

                Assert.That(StandardFilters.Date(context: context, input: null, format: "%M"), Is.EqualTo(null));

                Assert.That(StandardFilters.Date(context: context, input: "hi", format: "%M"), Is.EqualTo("hi"));

                Liquid.UseRubyDateFormat = true; // ensure all Context objects created within tests are defaulted to Ruby date format
                Template template = Template.Parse(@"{{ hi | date:""%M"" }}");
                Assert.That(template.Render(Hash.FromAnonymousObject(new { hi = "hi" })), Is.EqualTo("hi"));

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

            Assert.That(StandardFilters.First(null), Is.Null);
            Assert.That(StandardFilters.Last(null), Is.Null);
            Assert.That(StandardFilters.First(new[] { 1, 2, 3 }), Is.EqualTo(1));
            Assert.That(StandardFilters.Last(new[] { 1, 2, 3 }), Is.EqualTo(3));
            Assert.That(StandardFilters.First(new object[] { }), Is.Null);
            Assert.That(StandardFilters.Last(new object[] { }), Is.Null);

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
            Assert.That(StandardFilters.Replace(context: context, input: null, @string: "a", replacement: "b"), Is.Null);
            Assert.That(actual: StandardFilters.Replace(context: context, input: "", @string: "a", replacement: "b"), Is.EqualTo(expected: ""));
            Assert.That(actual: StandardFilters.Replace(context: context, input: "a a a a", @string: null, replacement: "b"), Is.EqualTo(expected: "a a a a"));
            Assert.That(actual: StandardFilters.Replace(context: context, input: "a a a a", @string: "", replacement: "b"), Is.EqualTo(expected: "a a a a"));
            Assert.That(actual: StandardFilters.Replace(context: context, input: "a a a a", @string: "a", replacement: "b"), Is.EqualTo(expected: "b b b b"));

            Assert.That(actual: StandardFilters.Replace(context: context, input: "Tesvalue\"", @string: "\"", replacement: "\\\""), Is.EqualTo(expected: "Tesvalue\\\""));
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
            Assert.That(actual: StandardFilters.Replace(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"), Is.EqualTo(expected: "b b b b"));
        }

        [Test]
        public void TestReplaceRegexV21()
        {
            var context = _contextV21;
            Assert.That(actual: StandardFilters.Replace(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"), Is.EqualTo(expected: "a A A a"));
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
            Assert.That(StandardFilters.ReplaceFirst(context: context, input: null, @string: "a", replacement: "b"), Is.Null);
            Assert.That(StandardFilters.ReplaceFirst(context: context, input: "", @string: "a", replacement: "b"), Is.EqualTo(""));
            Assert.That(StandardFilters.ReplaceFirst(context: context, input: "a a a a", @string: null, replacement: "b"), Is.EqualTo("a a a a"));
            Assert.That(StandardFilters.ReplaceFirst(context: context, input: "a a a a", @string: "", replacement: "b"), Is.EqualTo("a a a a"));
            Assert.That(StandardFilters.ReplaceFirst(context: context, input: "a a a a", @string: "a", replacement: "b"), Is.EqualTo("b a a a"));
            Helper.AssertTemplateResult(expected: "b a a a", template: "{{ 'a a a a' | replace_first: 'a', 'b' }}", syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestReplaceFirstRegexV20()
        {
            var context = _contextV20;
            Assert.That(actual: StandardFilters.ReplaceFirst(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"), Is.EqualTo(expected: "b A A a"));
        }

        [Test]
        public void TestReplaceFirstRegexV21()
        {
            var context = _contextV21;
            Assert.That(actual: StandardFilters.ReplaceFirst(context: context, input: "a A A a", @string: "[Aa]", replacement: "b"), Is.EqualTo(expected: "a A A a"));
            TestReplaceFirst(context);
        }

        [Test]
        public void TestRemove()
        {
            TestRemove(_contextV20);
        }

        public void TestRemove(Context context)
        {

            Assert.That(StandardFilters.Remove("a a a a", "a"), Is.EqualTo("   "));
            Assert.That(StandardFilters.RemoveFirst(context: context, input: "a a a a", @string: "a "), Is.EqualTo("a a a"));
            Helper.AssertTemplateResult(expected: "a a a", template: "{{ 'a a a a' | remove_first: 'a ' }}", syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestRemoveFirstRegexV20()
        {
            var context = _contextV20;
            Assert.That(actual: StandardFilters.RemoveFirst(context: context, input: "Mr. Jones", @string: "."), Is.EqualTo(expected: "r. Jones"));
        }

        [Test]
        public void TestRemoveFirstRegexV21()
        {
            var context = _contextV21;
            Assert.That(actual: StandardFilters.RemoveFirst(context: context, input: "Mr. Jones", @string: "."), Is.EqualTo(expected: "Mr Jones"));
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

                Assert.That(StandardFilters.Round("1.2345678", "two"), Is.Null);
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

                Assert.That(StandardFilters.Ceil(_contextV20, ""), Is.Null);
                Assert.That(StandardFilters.Ceil(_contextV20, "two"), Is.Null);
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

                Assert.That(StandardFilters.Floor(_contextV20, ""), Is.Null);
                Assert.That(StandardFilters.Floor(_contextV20, "two"), Is.Null);
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

            Assert.That(StandardFilters.Times(context: context, input: 0.843m, operand: 10), Is.EqualTo(8.43));
            Assert.That(StandardFilters.Times(context: context, input: 4.12m, operand: 100), Is.EqualTo(412));
            Assert.That(StandardFilters.Times(context: context, input: 7.5563m, operand: 1000), Is.EqualTo(7556.3));
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
            Helper.AssertTemplateResult(expected: "hi", template: "{{ nonesuch | append: 'hi' }}");
            Helper.AssertTemplateResult(expected: "hi", template: "{{ 'hi' | append: nonesuch }}");
            Helper.AssertTemplateResult(expected: string.Empty, template: "{{ alsononesuch | append: nonesuch }}");
        }

        [Test]
        public void TestPrepend()
        {
            Hash assigns = Hash.FromAnonymousObject(new { a = "bc", b = "a" });
            Helper.AssertTemplateResult(expected: "abc", template: "{{ a | prepend: 'a'}}", localVariables: assigns);
            Helper.AssertTemplateResult(expected: "abc", template: "{{ a | prepend: b}}", localVariables: assigns);
            Helper.AssertTemplateResult(expected: "hi", template: "{{ nonesuch | prepend: 'hi' }}");
            Helper.AssertTemplateResult(expected: "hi", template: "{{ 'hi' | prepend: nonesuch }}");
            Helper.AssertTemplateResult(expected: string.Empty, template: "{{ alsononesuch | prepend: nonesuch }}");
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
            Assert.That(StandardFilters.DividedBy(context: context, input: null, operand: 3), Is.Null);
            Assert.That(StandardFilters.DividedBy(context: context, input: 4, operand: null), Is.Null);

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
            Assert.That(c, Is.EqualTo(4L));


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
            Assert.That(StandardFilters.Modulo(context: context, input: null, operand: 3), Is.Null);
            Assert.That(StandardFilters.Modulo(context: context, input: 4, operand: null), Is.Null);
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
            Assert.That(StandardFilters.UrlEncode("http://dotliquidmarkup.org/"), Is.EqualTo("http%3A%2F%2Fdotliquidmarkup.org%2F"));
            Assert.That(StandardFilters.UrlEncode("Tetsuro Takara"), Is.EqualTo("Tetsuro+Takara"));
            Assert.That(StandardFilters.UrlEncode("john@liquid.com"), Is.EqualTo("john%40liquid.com"));
            Assert.That(StandardFilters.UrlEncode(null), Is.EqualTo(null));
        }

        [Test]
        public void TestUrldecode()
        {
            Assert.That(StandardFilters.UrlDecode("%27Stop%21%27+said+Fred"), Is.EqualTo("'Stop!' said Fred"));
            Assert.That(StandardFilters.UrlDecode(null), Is.EqualTo(null));
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
            Assert.That(StandardFilters.Capitalize(context: context, input: null), Is.EqualTo(null));
            Assert.That(StandardFilters.Capitalize(context: context, input: ""), Is.EqualTo(""));
            Assert.That(StandardFilters.Capitalize(context: context, input: " "), Is.EqualTo(" "));
            Assert.That(StandardFilters.Capitalize(context: context, input: "That is one sentence."), Is.EqualTo("That Is One Sentence."));

            Helper.AssertTemplateResult(
                expected: "Title",
                template: "{{ 'title' | capitalize }}",
                syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestCapitalizeV21()
        {
            var context = _contextV21;
            Assert.That(StandardFilters.Capitalize(context: context, input: null), Is.EqualTo(null));
            Assert.That(StandardFilters.Capitalize(context: context, input: ""), Is.EqualTo(""));
            Assert.That(StandardFilters.Capitalize(context: context, input: " "), Is.EqualTo(" "));
            Assert.That(StandardFilters.Capitalize(context: context, input: " my boss is Mr. Doe."), Is.EqualTo(" My boss is Mr. Doe."));

            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{{ 'my great title' | capitalize }}",
                syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestCapitalizeV22()
        {
            var context = _contextV22;
            Assert.That(StandardFilters.Capitalize(context: context, input: null), Is.EqualTo(null));
            Assert.That(StandardFilters.Capitalize(context: context, input: ""), Is.EqualTo(""));
            Assert.That(StandardFilters.Capitalize(context: context, input: " "), Is.EqualTo(" "));
            Assert.That(StandardFilters.Capitalize(context: context, input: "my boss is Mr. Doe."), Is.EqualTo("My boss is mr. doe."));

            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{{ 'my Great Title' | capitalize }}",
                syntax: context.SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestUniq()
        {
            Assert.That(StandardFilters.Uniq(new string[] { "ants", "bugs", "bees", "bugs", "ants" }), Is.EqualTo(new[] { "ants", "bugs", "bees" }).AsCollection);
            Assert.That(StandardFilters.Uniq(new string[] { }), Is.EqualTo(new string[] { }).AsCollection);
            Assert.That(StandardFilters.Uniq(null), Is.EqualTo(null));
            Assert.That(StandardFilters.Uniq(5), Is.EqualTo(new List<object> { 5 }));
        }

        [Test]
        public void TestAbs()
        {
            Assert.That(StandardFilters.Abs(_contextV20, "notNumber"), Is.EqualTo(0));
            Assert.That(StandardFilters.Abs(_contextV20, 10), Is.EqualTo(10));
            Assert.That(StandardFilters.Abs(_contextV20, -5), Is.EqualTo(5));
            Assert.That(StandardFilters.Abs(_contextV20, 19.86), Is.EqualTo(19.86));
            Assert.That(StandardFilters.Abs(_contextV20, -19.86), Is.EqualTo(19.86));
            Assert.That(StandardFilters.Abs(_contextV20, "10"), Is.EqualTo(10));
            Assert.That(StandardFilters.Abs(_contextV20, "-5"), Is.EqualTo(5));
            Assert.That(StandardFilters.Abs(_contextV20, "30.60"), Is.EqualTo(30.60));
            Assert.That(StandardFilters.Abs(_contextV20, "30.60a"), Is.EqualTo(0));

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
            Assert.That(StandardFilters.AtLeast(_contextV20, "notNumber", 5), Is.EqualTo("notNumber"));
            Assert.That(StandardFilters.AtLeast(_contextV20, 5, 5), Is.EqualTo(5));
            Assert.That(StandardFilters.AtLeast(_contextV20, 3, 5), Is.EqualTo(5));
            Assert.That(StandardFilters.AtLeast(_contextV20, 6, 5), Is.EqualTo(6));
            Assert.That(StandardFilters.AtLeast(_contextV20, 10, 5), Is.EqualTo(10));
            Assert.That(StandardFilters.AtLeast(_contextV20, 9.85, 5), Is.EqualTo(9.85));
            Assert.That(StandardFilters.AtLeast(_contextV20, 3.56, 5), Is.EqualTo(5));
            Assert.That(StandardFilters.AtLeast(_contextV20, "10", 5), Is.EqualTo(10));
            Assert.That(StandardFilters.AtLeast(_contextV20, "4", 5), Is.EqualTo(5));
            Assert.That(StandardFilters.AtLeast(_contextV20, "10a", 5), Is.EqualTo("10a"));
            Assert.That(StandardFilters.AtLeast(_contextV20, "4b", 5), Is.EqualTo("4b"));

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
            Assert.That(StandardFilters.AtMost(_contextV20, "notNumber", 5), Is.EqualTo("notNumber"));
            Assert.That(StandardFilters.AtMost(_contextV20, 5, 5), Is.EqualTo(5));
            Assert.That(StandardFilters.AtMost(_contextV20, 3, 5), Is.EqualTo(3));
            Assert.That(StandardFilters.AtMost(_contextV20, 6, 5), Is.EqualTo(5));
            Assert.That(StandardFilters.AtMost(_contextV20, 10, 5), Is.EqualTo(5));
            Assert.That(StandardFilters.AtMost(_contextV20, 9.85, 5), Is.EqualTo(5));
            Assert.That(StandardFilters.AtMost(_contextV20, 3.56, 5), Is.EqualTo(3.56));
            Assert.That(StandardFilters.AtMost(_contextV20, "10", 5), Is.EqualTo(5));
            Assert.That(StandardFilters.AtMost(_contextV20, "4", 5), Is.EqualTo(4));
            Assert.That(StandardFilters.AtMost(_contextV20, "4a", 5), Is.EqualTo("4a"));
            Assert.That(StandardFilters.AtMost(_contextV20, "10b", 5), Is.EqualTo("10b"));

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
            Assert.That(StandardFilters.Compact(new string[] { "business", null, "celebrities", null, null, "lifestyle", "sports", null, "technology", null }), Is.EqualTo(new[] { "business", "celebrities", "lifestyle", "sports", "technology" }).AsCollection);
            Assert.That(StandardFilters.Compact(new string[] { "business", "celebrities" }), Is.EqualTo(new[] { "business", "celebrities" }).AsCollection);
            Assert.That(StandardFilters.Compact(5), Is.EqualTo(new List<object> { 5 }));
            Assert.That(StandardFilters.Compact(new string[] { }), Is.EqualTo(new string[] { }).AsCollection);
            Assert.That(StandardFilters.Compact(null), Is.EqualTo(null));

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
            Assert.That(actual: StandardFilters.Where(null, propertyName: "property"), Is.EqualTo(expected: null));
            Assert.That(actual: StandardFilters.Where("a string object", propertyName: "property"), Is.EqualTo(expected: new string[] { }).AsCollection);
            Assert.That(actual: StandardFilters.Where(new string[] { }, propertyName: "property"), Is.EqualTo(expected: new string[] { }).AsCollection);

            // Ensure error reported if the property name is not provided.
            Assert.Throws<ArgumentNullException>(() => StandardFilters.Where(input: products, propertyName: " "));

            // Test filtering by value of a property
            var expectedKitchenProducts = new[] {
                new { title = "Spatula", type = "kitchen" },
                new { title = "Garlic press", type = "kitchen" }
            };
            Assert.That(actual: StandardFilters.Where(products, propertyName: "type", targetValue: "kitchen"), Is.EqualTo(expected: expectedKitchenProducts).AsCollection);

            // Test filtering for existence of a property
            Assert.That(actual: StandardFilters.Where(products, propertyName: "type"), Is.EqualTo(expected: products).AsCollection);

            // Test filtering for non-existent property
            var emptyArray = Array.Empty<object>();
            Assert.That(actual: StandardFilters.Where(products, propertyName: "non_existent_property"), Is.EqualTo(expected: emptyArray).AsCollection);

            // Confirm what happens to enumerable content that is a value type
            var values = new[] { 1, 2, 3, 4, 5 };
            Assert.That(actual: StandardFilters.Where(values, propertyName: "value", targetValue: "xxx"), Is.EqualTo(expected: new string[] { }));

            // Ensure null elements are handled gracefully
            var productsWithNullEntry = new[] {
                new { title = "Vacuum", type = "cleaning" },
                new { title = "Spatula", type = "kitchen" },
                null,
                new { title = "Cushion", type = (string)null },
                new { title = "Television", type = "lounge" },
                new { title = "Garlic press", type = "kitchen" }
            };
            Assert.That(actual: StandardFilters.Where(productsWithNullEntry, propertyName: "type", targetValue: "kitchen"), Is.EqualTo(expected: expectedKitchenProducts));
        }

        [Test]
        public void TestWhere_Indexable()
        {
            var products = new[] {
                new ProductDrop { Title = "Vacuum", Type = "cleaning" },
                new ProductDrop { Title = "Spatula", Type = "kitchen" },
                new ProductDrop { Title = "Television", Type = "lounge" },
                new ProductDrop { Title = "Garlic press", Type = "kitchen" }
            };
            var expectedProducts = products.Where(p => p.Type == "kitchen").ToArray();

            Helper.LockTemplateStaticVars(new RubyNamingConvention(), () =>
            {
                Assert.That(
                    actual: StandardFilters.Where(products, propertyName: "type", targetValue: "kitchen"), Is.EqualTo(expected: expectedProducts).AsCollection);
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

            Assert.That(
                actual: StandardFilters.Where(products, propertyName: "type", targetValue: "kitchen"), Is.EqualTo(expected: expectedProducts));
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
            var products = new[] {
                new ProductDrop { Title = "Vacuum", Type = "cleaning" },
                new ProductDrop { Title = "Spatula", Type = "kitchen" },
                new ProductDrop { Title = "Television", Type = "lounge" },
                new ProductDrop { Title = "Garlic press", Type = "kitchen" }
            };
            var expectedKitchenProducts = products.Where(p => p.Type == "kitchen").ToArray();

            Helper.LockTemplateStaticVars(new RubyNamingConvention(), () =>
            {
                Assert.That(
                    actual: StandardFilters.Where(products, propertyName: "type", targetValue: "kitchen"), Is.EqualTo(expected: expectedKitchenProducts).AsCollection);
            });
        }

        [Test]
        public void TestConcat()
        {
            var array1 = new String[] { "one", "two" };
            var array2 = new String[] { "alpha", "bravo" };

            Assert.That(StandardFilters.Concat(null, null), Is.EqualTo(null).AsCollection);
            Assert.That(StandardFilters.Concat(array1, null), Is.EqualTo(array1).AsCollection);
            Assert.That(StandardFilters.Concat(null, array1), Is.EqualTo(array1).AsCollection);
            Assert.That(StandardFilters.Concat(array1, array2), Is.EqualTo(new[] { "one", "two", "alpha", "bravo" }).AsCollection);
            Assert.That(StandardFilters.Concat(new[] { 1, 2 }, new[] { 3, 4 }), Is.EqualTo(new[] { 1, 2, 3, 4 }).AsCollection);
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

            Assert.That(StandardFilters.Reverse(null), Is.EqualTo(null).AsCollection);
            Assert.That(StandardFilters.Reverse(array), Is.EqualTo(arrayReversed).AsCollection);
            Assert.That(StandardFilters.Reverse(arrayReversed), Is.EqualTo(array).AsCollection);
            Assert.That(StandardFilters.Reverse(new[] { 1, 2, 2, 3 }), Is.EqualTo(new[] { 3, 2, 2, 1 }).AsCollection);
            Assert.That(StandardFilters.Reverse("Ground control to Major Tom."), Is.EqualTo("Ground control to Major Tom."));
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

        [Test]
        public void TestBase64Encode_LiquidSample()
        {
            Assert.Multiple(() =>
            {
                Assert.That(StandardFilters.Base64Encode("one two three"), Is.EqualTo("b25lIHR3byB0aHJlZQ=="));
                Assert.That(StandardFilters.Base64Encode(null), Is.EqualTo(string.Empty));
                Assert.That(StandardFilters.Base64Encode(string.Empty), Is.EqualTo(string.Empty)); // Similar test proven to be true
            });
        }

        [Test]
        public void TestBase64Decode_LiquidSample()
        {
            Assert.Multiple(() =>
            {
                Assert.That(StandardFilters.Base64Decode("b25lIHR3byB0aHJlZQ=="), Is.EqualTo("one two three"));
                Assert.That(StandardFilters.Base64Decode("4pyF"), Is.EqualTo("✅"));

                // Intentionally skipped test for "/w==" as .NET always uses UTF-16 for strings
                Assert.Throws<ArgumentException>(() => StandardFilters.Base64Decode("invalidbase64"));
                Helper.AssertTemplateResult(expected: "Liquid error: Invalid base64 provided to base64_decode", template: "{{ \"invalidbase64\" | base64_decode }}");
            });
        }

        [Test]
        public void TestBase64Decode_MandatoryPadding()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(() => StandardFilters.Base64Decode("QQ"));
                Assert.Throws<ArgumentException>(() => StandardFilters.Base64Decode("QUI"));
            });
        }

        [Test]
        public void TestBase64Decode_NullStrings()
        {
            Assert.Multiple(() =>
            {
                Assert.That(StandardFilters.Base64Decode(null), Is.EqualTo(string.Empty));
                Assert.That(StandardFilters.Base64Decode(string.Empty), Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public void TestBase64UrlSafeEncode_LiquidSample()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    StandardFilters.Base64UrlSafeEncode("abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ 1234567890 !@#$%^&*()-=_+/?.:;[]{}\\|"),
                    Is.EqualTo("YWJjZGVmZ2hpamtsbW5vcHFyc3R1dnd4eXogQUJDREVGR0hJSktMTU5PUFFSU1RVVldYWVogMTIzNDU2Nzg5MCAhQCMkJV4mKigpLT1fKy8_Ljo7W117fVx8"));
                Assert.That(StandardFilters.Base64UrlSafeEncode(null), Is.EqualTo(string.Empty));
                Assert.That(StandardFilters.Base64UrlSafeEncode(string.Empty), Is.EqualTo(string.Empty)); // Similar test proven to be true
            });
        }

        [Test]
        public void TestBase64UrlSafeDecode_LiquidSample()
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    StandardFilters.Base64UrlSafeDecode("YWJjZGVmZ2hpamtsbW5vcHFyc3R1dnd4eXogQUJDREVGR0hJSktMTU5PUFFSU1RVVldYWVogMTIzNDU2Nzg5MCAhQCMkJV4mKigpLT1fKy8_Ljo7W117fVx8"),
                    Is.EqualTo("abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ 1234567890 !@#$%^&*()-=_+/?.:;[]{}\\|"));
                Assert.That(StandardFilters.Base64UrlSafeDecode("4pyF"), Is.EqualTo("✅"));

                // Intentionally skipped test for "_w==" as .NET always uses UTF-16 for strings
                Assert.Throws<ArgumentException>(() => StandardFilters.Base64UrlSafeDecode("invalidbase64"));
                Helper.AssertTemplateResult(expected: "Liquid error: Invalid base64 provided to base64_url_safe_decode", template: "{{ \"invalidbase64\" | base64_url_safe_decode }}");
            });
        }

        [Test]
        public void TestBase64UrlSafeDecode_OptionalPadding()
        {
            Assert.Multiple(() =>
            {
                Assert.That(StandardFilters.Base64UrlSafeDecode("QQ"), Is.EqualTo("A"));
                Assert.That(StandardFilters.Base64UrlSafeDecode("QUI"), Is.EqualTo("AB"));
                Assert.That(StandardFilters.Base64UrlSafeDecode("QQ=="), Is.EqualTo("A"));

                Assert.Throws<ArgumentException>(() => StandardFilters.Base64UrlSafeDecode("QQ="));
            });
        }

        [Test]
        public void TestBase64UrlSafeDecode_NullStrings()
        {
            Assert.Multiple(() =>
            {
                Assert.That(StandardFilters.Base64UrlSafeDecode(null), Is.EqualTo(string.Empty));
                Assert.That(StandardFilters.Base64UrlSafeDecode(string.Empty), Is.EqualTo(string.Empty));
            });
        }

        private class ProductDrop : Drop
        {
            public string Title { get; set; }
            public string Type { get; set; }
        }
    }
}
