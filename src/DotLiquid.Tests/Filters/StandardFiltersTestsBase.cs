using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace DotLiquid.Tests.Filters
{
    [TestFixture]
    public abstract class StandardFiltersTestsBase
    {
        public abstract IFormatProvider FormatProvider { get; }
        public abstract SyntaxCompatibility SyntaxCompatibilityLevel { get; }
        public abstract CapitalizeDelegate Capitalize { get; }
        public abstract MathDelegate Divide { get; }
        public abstract MathDelegate Plus { get; }
        public abstract MathDelegate Minus { get; }
        public abstract MathDelegate Modulo { get; }
        public abstract RemoveFirstDelegate RemoveFirst { get; }
        public abstract ReplaceDelegate Replace { get; }
        public abstract ReplaceFirstDelegate ReplaceFirst { get; }
        public abstract RoundDelegate Round { get; }
        public abstract TwoInputDelegate AtLeast { get; }
        public abstract TwoInputDelegate AtMost { get; }
        public abstract OneInputDelegate Abs { get; }
        public abstract OneInputDelegate Ceil { get; }
        public abstract OneInputDelegate Floor { get; }
        public abstract SliceDelegate Slice { get; }
        public abstract SplitDelegate Split { get; }
        public abstract SumDelegate Sum { get; }
        public abstract MathDelegate Times { get; }
        public abstract TruncateWordsDelegate TruncateWords { get; }

        public delegate string CapitalizeDelegate(string input);
        public delegate object MathDelegate(object input, object operand);
        public delegate string RemoveFirstDelegate(string input, string @string);
        public delegate string ReplaceDelegate(string input, string @string, string replacement);
        public delegate string ReplaceFirstDelegate(string input, string @string, string replacement);
        public delegate object RoundDelegate(object input, object places = null);
        public delegate object SliceDelegate(object input, int start, int? len = null);
        public delegate object OneInputDelegate(object input);
        public delegate object TwoInputDelegate(object input, object parameter);
        public delegate string[] SplitDelegate(string input, string pattern);
        public delegate object SumDelegate(IEnumerable input, string propertyName = null);
        public delegate string TruncateWordsDelegate(string input, int? words = null, string truncateString = null);

        protected Context _context;

        [OneTimeSetUp]
        public void SetUp()
        {
            _context = new Context(FormatProvider)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibilityLevel
            };
        }

        [Test]
        public void TestCapitalize()
        {
            Assert.That(Capitalize(input: null), Is.EqualTo(null));
            Assert.That(Capitalize(input: ""), Is.EqualTo(""));
            Assert.That(Capitalize(input: " "), Is.EqualTo(" "));
        }

        [Test]
        public void TestDividedBy()
        {
            Assert.That(Divide(input: 12, operand: 3), Is.EqualTo(4));
            Assert.That(Divide(input: 14, operand: 3), Is.EqualTo(4));
            Assert.That(Divide(input: 15, operand: 3), Is.EqualTo(5));
            Assert.That(Divide(input: null, operand: 3), Is.Null);
            Assert.That(Divide(input: 4, operand: null), Is.Null);

            // Ensure we preserve floating point behavior for division by zero, and don't start throwing exceptions.
            Assert.That(Divide(input: 1.0, operand: 0.0), Is.EqualTo(double.PositiveInfinity));
            Assert.That(Divide(input: -1.0, operand: 0.0), Is.EqualTo(double.NegativeInfinity));
            Assert.That(Divide(input: 0.0, operand: 0.0), Is.EqualTo(double.NaN));
        }

        [Test]
        public void TestPlus()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Plus(input: 1, operand: 1), Is.EqualTo(2));
                Assert.That(Plus(input: 2, operand: 3.5), Is.EqualTo(5.5));
                Assert.That(Plus(input: 3.5, operand: 2), Is.EqualTo(5.5));

                // Test that decimals are not introducing rounding-precision issues
                Assert.That(Plus(input: 148387.77, operand: 10), Is.EqualTo(148397.77));

                // Test that mix of 32-bit and 64-bit int returns 64-bit
                Assert.That(Plus(input: int.MaxValue, operand: (long)1), Is.EqualTo(2147483648));
            });
        }

        [Test]
        public void TestMinus()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Minus(input: 5, operand: 1), Is.EqualTo(4));
                Assert.That(Minus(input: 2, operand: 3.5), Is.EqualTo(-1.5));
                Assert.That(Minus(input: 3.5, operand: 2), Is.EqualTo(1.5));
            });
        }

        [Test]
        public void TestModulo()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Modulo(input: 3, operand: 2), Is.EqualTo(1));
                Assert.That(Modulo(input: 148387.77, operand: 10), Is.EqualTo(7.77));
                Assert.That(Modulo(input: 3455.32, operand: 10), Is.EqualTo(5.32));
                Assert.That(Modulo(input: 23423.12, operand: 10), Is.EqualTo(3.12));
                Assert.That(Modulo(input: null, operand: 3), Is.Null);
                Assert.That(Modulo(input: 4, operand: null), Is.Null);
            });
        }

        [Test]
        public void TestRemoveFirst()
        {
            Assert.That(RemoveFirst(input: null, @string: "a"), Is.Null);
            Assert.That(RemoveFirst(input: "", @string: "a"), Is.EqualTo(""));
            Assert.That(RemoveFirst(input: "a a a a", @string: null), Is.EqualTo("a a a a"));
            Assert.That(RemoveFirst(input: "a a a a", @string: ""), Is.EqualTo("a a a a"));
            Assert.That(RemoveFirst(input: "a a a a", @string: "a "), Is.EqualTo("a a a"));
        }

        [Test]
        public void TestReplace()
        {
            Assert.That(actual: Replace(null, "a", "b"), Is.Null);
            Assert.That(actual: Replace("", "a", "b"), Is.EqualTo(expected: ""));
            Assert.That(actual: Replace("a a a a", null, "b"), Is.EqualTo(expected: "a a a a"));
            Assert.That(actual: Replace("a a a a", "", "b"), Is.EqualTo(expected: "a a a a"));
            Assert.That(actual: Replace("a a a a", "a", "b"), Is.EqualTo(expected: "b b b b"));

            Assert.That(actual: Replace("Tesvalue\"", "\"", "\\\""), Is.EqualTo(expected: "Tesvalue\\\""));
            Helper.AssertTemplateResult(expected: "Tesvalue\\\"", template: "{{ 'Tesvalue\"' | replace: '\"', '\\\"' }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult(
                expected: "Tesvalue\\\"",
                template: "{{ context | replace: '\"', '\\\"' }}",
                localVariables: Hash.FromAnonymousObject(new { context = "Tesvalue\"" }),
                syntax: SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestReplaceFirst()
        {
            Assert.That(ReplaceFirst(input: null, @string: "a", replacement: "b"), Is.Null);
            Assert.That(ReplaceFirst(input: "", @string: "a", replacement: "b"), Is.EqualTo(""));
            Assert.That(ReplaceFirst(input: "a a a a", @string: "a", replacement: "b"), Is.EqualTo("b a a a"));
            Helper.AssertTemplateResult(expected: "b a a a", template: "{{ 'a a a a' | replace_first: 'a', 'b' }}", syntax: SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestRound()
        {
            Helper.AssertTemplateResult("1.235", "{{ 1.234678 | round: 3 }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("1", "{{ 1 | round }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("1", "{{ 1.234678 | round }}", syntax: SyntaxCompatibilityLevel);

            Helper.AssertTemplateResult("1", "{{ 1.234678 | round: nonesuch }}", syntax: SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestRoundHandlesBadInput()
        {
            Helper.AssertTemplateResult("0", "{{ nonesuch | round }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("0", "{{ nonesuch | round: 3 }}", syntax: SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestAtLeast()
        {
            Helper.AssertTemplateResult("5", "{{ 4 | at_least: 5 }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("4", "{{ 4 | at_least: 3 }}", syntax: SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestAtMost()
        {
            Helper.AssertTemplateResult("4", "{{ 4 | at_most: 5 }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("3", "{{ 4 | at_most: 3 }}", syntax: SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestAbs()
        {
            Helper.AssertTemplateResult("17", "{{ -17 | abs }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("17", "{{ 17 | abs }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("4", "{{ 4 | abs }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("19.86", "{{ '-19.86' | abs }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("19.86", "{{ -19.86 | abs }}", syntax: SyntaxCompatibilityLevel);

            Helper.AssertTemplateResult("0", "{{ 'notNumber' | abs }}", syntax: SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestCeil()
        {
            Helper.AssertTemplateResult("-1", "{{ -1.2 | ceil }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("2", "{{ 1.2 | ceil }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("2", "{{ 2.0 | ceil }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("184", "{{ 183.357 | ceil }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("4", "{{ \"3.5\" | ceil }}", syntax: SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestFloor()
        {
            Helper.AssertTemplateResult("-2", "{{ -1.2 | floor }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("1", "{{ 1.2 | floor }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("2", "{{ 2.0 | floor }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("183", "{{ 183.57 | floor }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("3", "{{ \"3.5\" | floor }}", syntax: SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestSliceString()
        {
            Assert.That(Slice("abcdefg", 0, 3), Is.EqualTo("abc"));
            Assert.That(Slice("abcdefg", 1, 3), Is.EqualTo("bcd"));
            Assert.That(Slice("abcdefg", -3, 3), Is.EqualTo("efg"));
            Assert.That(Slice("abcdefg", -3, 30), Is.EqualTo("efg"));
            Assert.That(Slice("abcdefg", 4, 30), Is.EqualTo("efg"));
            Assert.That(Slice("abc", -4, 2), Is.EqualTo("a"));
            Assert.That(Slice("abcdefg", -10, 1), Is.EqualTo(""));

            // Test replicated from the Ruby library (https://github.com/Shopify/liquid/blob/master/test/integration/standard_filter_test.rb)
            Assert.That(Slice("foobar", 1, 3), Is.EqualTo("oob"));
            Assert.That(Slice("foobar", 1, 1000), Is.EqualTo("oobar"));
            Assert.That(Slice("foobar", 1, 0), Is.EqualTo(""));
            Assert.That(Slice("foobar", 1, 1), Is.EqualTo("o"));
            Assert.That(Slice("foobar", 3, 3), Is.EqualTo("bar"));
            Assert.That(Slice("foobar", -2, 2), Is.EqualTo("ar"));
            Assert.That(Slice("foobar", -2, 1000), Is.EqualTo("ar"));
            Assert.That(Slice("foobar", -1), Is.EqualTo("r"));
            Assert.That(Slice("foobar", -100, 10), Is.EqualTo(""));
            Assert.That(Slice("foobar", 1, 3), Is.EqualTo("oob"));
        }

        [Test]
        public void TestSliceArrays()
        {
            // Test replicated from the Ruby library
            var testArray = new[] { "f", "o", "o", "b", "a", "r" };
            Assert.That((IEnumerable<object>)Slice(testArray, 1, 3), Is.EqualTo(ToStringArray("oob")).AsCollection);
            Assert.That((IEnumerable<object>)Slice(testArray, 1, 1000), Is.EqualTo(ToStringArray("oobar")).AsCollection);
            Assert.That((IEnumerable<object>)Slice(testArray, 1, 0), Is.EqualTo(ToStringArray("")).AsCollection);
            Assert.That((IEnumerable<object>)Slice(testArray, 1, 1), Is.EqualTo(ToStringArray("o")).AsCollection);
            Assert.That((IEnumerable<object>)Slice(testArray, 3, 3), Is.EqualTo(ToStringArray("bar")).AsCollection);
            Assert.That((IEnumerable<object>)Slice(testArray, -2, 2), Is.EqualTo(ToStringArray("ar")).AsCollection);
            Assert.That((IEnumerable<object>)Slice(testArray, -2, 1000), Is.EqualTo(ToStringArray("ar")).AsCollection);
            Assert.That((IEnumerable<object>)Slice(testArray, -1), Is.EqualTo(ToStringArray("r")).AsCollection);
            Assert.That((IEnumerable<object>)Slice(testArray, 100, 10), Is.EqualTo(ToStringArray("")).AsCollection);
            Assert.That((IEnumerable<object>)Slice(testArray, -100, 10), Is.EqualTo(ToStringArray("")).AsCollection);

            // additional tests
            Assert.That((IEnumerable<object>)Slice(testArray, -6, 2), Is.EqualTo(ToStringArray("fo")).AsCollection);
            Assert.That((IEnumerable<object>)Slice(testArray, -8, 4), Is.EqualTo(ToStringArray("fo")).AsCollection);

            // Non-string arrays tests
            Assert.That((IEnumerable<object>)Slice(new[] { 1, 2, 3, 4, 5 }, 1, 3), Is.EqualTo(new[] { 2, 3, 4 }).AsCollection);
            Assert.That((IEnumerable<object>)Slice(new[] { 'a', 'b', 'c', 'd', 'e' }, -4, 3), Is.EqualTo(new[] { 'b', 'c', 'd' }).AsCollection);
        }

        [Test]
        public void TestSplit()
        {
            Assert.That(Split("This is a sentence", " "), Is.EqualTo(new[] { "This", "is", "a", "sentence" }).AsCollection);

            // A string with no pattern should be split into a string[], as required for the Liquid Reverse filter
            Assert.That(Split("YMCA", null), Is.EqualTo(new[] { "Y", "M", "C", "A" }).AsCollection);
            Assert.That(Split("YMCA", ""), Is.EqualTo(new[] { "Y", "M", "C", "A" }).AsCollection);
            Assert.That(Split(" ", ""), Is.EqualTo(new[] { " " }).AsCollection);
        }

        [Test]
        public void TestSum_Numeric()
        {
            int[] intArray = new int[] { 1, 2, 3, 4, 5 };
            decimal[] decimalArray = new decimal[] { 1.1m, 2.2m, 3.3m, 4.4m, 5.5m };
            string[] stringArray = new string[] { "1", "2", "-3", (4.4).ToString(_context.CurrentCulture), (5.1).ToString(_context.CurrentCulture) };
            object[] mixedArray = new object[] {
                null, "banana",
                "1",
                (decimal)1, (float)1, (double)1,
                (sbyte)1, (byte)1, (short)1, (ushort)1, (int)1, (uint)1, (long)1, (ulong)1,
            };

            Assert.That(Sum(intArray), Is.EqualTo(15));
            Assert.That(Sum(decimalArray), Is.EqualTo(16.5m));
            Assert.That(Sum(stringArray), Is.EqualTo(9.5));
            Assert.That(Sum(mixedArray), Is.EqualTo(12m));

            Assert.That(Sum(null), Is.EqualTo(0));
            Assert.That(Sum(null, "UnknownProperty"), Is.EqualTo(0));
            Assert.That(Sum(intArray, "UnknownProperty"), Is.EqualTo(0));
        }

        [Test]
        public void TestSum_StringToNumeric()
        {
            // Ensure that the different types of numeric values can be converted from strings
            // Ensure that the sum is non-zero
            string[] intStringArray = new string[] {
                int.MaxValue.ToString(_context.FormatProvider),
                int.MinValue.ToString(_context.FormatProvider),
            };
            string[] longStringArray = new string[] {
                long.MaxValue.ToString(_context.FormatProvider),
                long.MinValue.ToString(_context.FormatProvider),
            };
            string[] decimalStringArray = new string[] {
                decimal.MaxValue.ToString(_context.FormatProvider),
                "-1",
                decimal.MinValue.ToString(_context.FormatProvider),
            };
            // These will parse as double, but not decimal
            string[] doubleStringArray = new string[] {
                "1e203",
                "-1e202",
            };

            Assert.That(Sum(intStringArray), Is.EqualTo(-1));
            Assert.That(Sum(longStringArray), Is.EqualTo(-1));
            Assert.That(Sum(decimalStringArray), Is.EqualTo(-1.0m));
            Assert.That(Sum(doubleStringArray), Is.EqualTo(9e202));
        }

        [Test]
        public void TestSum_PropertyName()
        {
            var kIntArray = new object[] {
                new { k = 1 },
                new { k = 2 },
                new { k = 3 }
            };
            var kStringArray = new object[] {
                new { k = "1" },
                new { k = "2" },
                new { k = "3" }
            };
            var mixedKeyIntArray = new object[] {
                new { k = 1 },
                new { k = 2 },
                new { x = 5 }
            };

            Assert.That(Sum(kIntArray, "k"), Is.EqualTo(6));
            Assert.That(Sum(kIntArray, "x"), Is.EqualTo(0));
            Assert.That(Sum(kStringArray, "k"), Is.EqualTo(6));
            Assert.That(Sum(kStringArray, "x"), Is.EqualTo(0));
            Assert.That(Sum(mixedKeyIntArray, "k"), Is.EqualTo(3));
            Assert.That(Sum(mixedKeyIntArray, "x"), Is.EqualTo(5));
        }

        [Test]
        public void TestTruncateWords()
        {
            Assert.That(TruncateWords(null), Is.EqualTo(null));
            Assert.That(TruncateWords(""), Is.EqualTo(""));
            Assert.That(TruncateWords("one two three", 4), Is.EqualTo("one two three"));
            Assert.That(TruncateWords("one two three", 2), Is.EqualTo("one two..."));
            Assert.That(TruncateWords("one two three"), Is.EqualTo("one two three"));
            Assert.That(TruncateWords("Two small (13&#8221; x 5.5&#8221; x 10&#8221; high) baskets fit inside one large basket (13&#8221; x 16&#8221; x 10.5&#8221; high) with cover.", 15), Is.EqualTo("Two small (13&#8221; x 5.5&#8221; x 10&#8221; high) baskets fit inside one large basket (13&#8221;..."));
        }

        /// <summary>
        /// Convert a string into a string[] where each character is mapped into an array element.
        /// </summary>
        private static string[] ToStringArray(string input)
        {
            return input.ToCharArray().Select(character => character.ToString()).ToArray();
        }
    }
}
