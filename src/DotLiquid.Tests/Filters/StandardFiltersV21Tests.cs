using System;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests.Filters
{
    [TestFixture]
    public class StandardFiltersV21Tests : StandardFiltersTestsBase
    {
        public override IFormatProvider FormatProvider => CultureInfo.InvariantCulture;
        public override SyntaxCompatibility SyntaxCompatibilityLevel => SyntaxCompatibility.DotLiquid21;
        public override CapitalizeDelegate Capitalize => i => LegacyFilters.CapitalizeV21(i);
        public override MathDelegate Divide => (i, o) => StandardFilters.DividedBy(_context, i, o);
        public override MathDelegate Plus => (i, o) => StandardFilters.Plus(_context, i, o);
        public override MathDelegate Minus => (i, o) => StandardFilters.Minus(_context, i, o);
        public override MathDelegate Modulo => (i, o) => StandardFilters.Modulo(_context, i, o);
        public override RemoveFirstDelegate RemoveFirst => (a, b) => LegacyFilters.RemoveFirstV21(a, b);
        public override ReplaceDelegate Replace => (i, s, r) => StandardFilters.Replace(i, s, r);
        public override ReplaceFirstDelegate ReplaceFirst => (a, b, c) => LegacyFilters.ReplaceFirstV21(a, b, c);
        public override RoundDelegate Round => (i, p) => LegacyFilters.Round(i, p);
        public override TwoInputDelegate AtLeast => (i, p) => LegacyFilters.AtLeast(_context, i, p);
        public override TwoInputDelegate AtMost => (i, p) => LegacyFilters.AtMost(_context, i, p);
        public override OneInputDelegate Abs => i => LegacyFilters.Abs(_context, i);
        public override OneInputDelegate Ceil => i => LegacyFilters.Ceil(_context, i);
        public override OneInputDelegate Floor => i => LegacyFilters.Floor(_context, i);
        public override SliceDelegate Slice => (a, b, c) => c.HasValue ? LegacyFilters.Slice(a, b, c.Value) : LegacyFilters.Slice(a, b);
        public override SplitDelegate Split => (i, p) => LegacyFilters.Split(i, p);
        public override SumDelegate Sum => (i, p) => StandardFilters.Sum(_context, i, p);
        public override MathDelegate Times => (i, o) => StandardFilters.Times(_context, i, o);
        public override TruncateWordsDelegate TruncateWords => (i, w, s) =>
        {
            if (w.HasValue)
                return s == null ? LegacyFilters.TruncateWords(i, w.Value) : LegacyFilters.TruncateWords(i, w.Value, s);
            return LegacyFilters.TruncateWords(i);
        };

        [Test]
        public void TestCapitalizeBehavesLikeUpcaseFirst()
        {
            Assert.That(Capitalize(input: " my boss is Mr. Doe."), Is.EqualTo(" My boss is Mr. Doe."));
            Assert.That(Capitalize(input: "my great title"), Is.EqualTo("My great title"));
        }

        [Test]
        public void TestDividedByStringIsParsed()
        {
            Assert.That(Divide(input: "12", operand: 3), Is.EqualTo(4));
            Assert.That(Divide(input: 12, operand: "3"), Is.EqualTo(4));
        }

        [Test]
        public void TestPlusStringAdds()
        {
            Assert.That(Plus(input: "1", operand: 1), Is.EqualTo(2));
            Assert.That(Plus(input: 1, operand: "1"), Is.EqualTo(2));
            Assert.That(Plus(input: "1", operand: "1"), Is.EqualTo(2));
            Assert.That(Plus(input: 2, operand: "3.5"), Is.EqualTo(5.5));
            Assert.That(Plus(input: "3.5", operand: 2), Is.EqualTo(5.5));
        }

        [Test]
        public void TestMinusStringIsParsed()
        {
            Assert.That(Minus(input: "2", operand: 1), Is.EqualTo(1));
            Assert.That(Minus(input: 2, operand: 1), Is.EqualTo(1));
            Assert.That(Minus(input: 2, operand: 3.5), Is.EqualTo(-1.5));
            Assert.That(Minus(input: "2.5", operand: 4), Is.EqualTo(-1.5));
            Assert.That(Minus(input: "2.5", operand: "3.5"), Is.EqualTo(-1));
        }

        [Test]
        public void TestModuloStringIsParsed()
        {
            Assert.That(Modulo(input: "3", operand: 2), Is.EqualTo(1));
            Assert.That(Modulo(input: 3, operand: "2"), Is.EqualTo(1));
        }

        [Test]
        public void TestTimesStringIsParsed()
        {
            Assert.That(Times(input: "3", operand: 4), Is.EqualTo(12));
            Assert.That(Times(input: 3, operand: "4"), Is.EqualTo(12));
            Assert.That(Times(input: "3", operand: "4"), Is.EqualTo(12));
        }

        [Test]
        public void TestRemoveFirstRegexFails()
        {
            Assert.That(RemoveFirst(input: "Mr. Jones", @string: "."), Is.EqualTo(expected: "Mr Jones"));
            Assert.That(RemoveFirst(input: "a a a a", @string: "[Aa] "), Is.EqualTo("a a a a"));
        }

        [Test]
        public void TestReplaceRegexFails()
        {
            Assert.That(Replace(input: "a A A a", @string: "[Aa]", replacement: "b"), Is.EqualTo(expected: "a A A a"));
        }

        [Test]
        public void TestReplaceFirstRegexFails()
        {
            Assert.That(ReplaceFirst(input: "a A A a", @string: "[Aa]", replacement: "b"), Is.EqualTo(expected: "a A A a"));
        }

        [Test]
        public void TestRoundHandlesBadParams()
        {
            Assert.That(Round("1.2345678", "two"), Is.Null);
            Assert.That(Round("1.2345678", -2), Is.Null);
            Assert.That(Round(1.123456789012345678901234567890123m, 50), Is.Null);

            Assert.That(Round("1.2345678", 2.7), Is.EqualTo(1.235m));

            Helper.AssertTemplateResult("1.235", "{{ 1.234678 | round: 2.7 }}", syntax: SyntaxCompatibilityLevel);
            Helper.AssertTemplateResult("1.235", "{{ 1.234678 | round: 3.1 }}", syntax: SyntaxCompatibilityLevel);

            Helper.AssertTemplateResult("", "{{ 1.234678 | round: -3 }}", syntax: SyntaxCompatibilityLevel);
        }

        [Test]
        public void TestAtLeastFloatingPointTypes()
        {
            Assert.That(AtLeast("notNumber", 5), Is.EqualTo("notNumber"));
            Assert.That(AtLeast(5, "notNumber"), Is.EqualTo(5).And.TypeOf(typeof(int)));
            Assert.That(AtLeast(5, 5), Is.EqualTo(5).And.TypeOf(typeof(double)));
            Assert.That(AtLeast(3, 5), Is.EqualTo(5).And.TypeOf(typeof(double)));
            Assert.That(AtLeast(6, 5), Is.EqualTo(6).And.TypeOf(typeof(double)));
            Assert.That(AtLeast(10, 5), Is.EqualTo(10).And.TypeOf(typeof(double)));
            Assert.That(AtLeast(9.85, 5), Is.EqualTo(9.85).And.TypeOf(typeof(double)));
            Assert.That(AtLeast(3.56, 5), Is.EqualTo(5).And.TypeOf(typeof(double)));
            Assert.That(AtLeast("10", 5), Is.EqualTo(10).And.TypeOf(typeof(double)));
            Assert.That(AtLeast("4", 5), Is.EqualTo(5).And.TypeOf(typeof(double)));
            Assert.That(AtLeast("10a", 5), Is.EqualTo("10a"));
            Assert.That(AtLeast("4b", 5), Is.EqualTo("4b"));
            Assert.That(AtLeast(null, 5), Is.Null);
            Assert.That(AtLeast(5, null), Is.EqualTo(5).And.TypeOf(typeof(int)));
        }

        [Test]
        public void TestAtMostFloatingPointTypes()
        {
            Assert.That(AtMost("notNumber", 5), Is.EqualTo("notNumber"));
            Assert.That(AtMost(5, "notNumber"), Is.EqualTo(5).And.TypeOf(typeof(int)));
            Assert.That(AtMost(5, 5), Is.EqualTo(5).And.TypeOf(typeof(double)));
            Assert.That(AtMost(3, 5), Is.EqualTo(3).And.TypeOf(typeof(double)));
            Assert.That(AtMost(6, 5), Is.EqualTo(5).And.TypeOf(typeof(double)));
            Assert.That(AtMost(10, 5), Is.EqualTo(5).And.TypeOf(typeof(double)));
            Assert.That(AtMost(9.85, 5), Is.EqualTo(5).And.TypeOf(typeof(double)));
            Assert.That(AtMost(3.56, 5), Is.EqualTo(3.56).And.TypeOf(typeof(double)));
            Assert.That(AtMost("10", 5), Is.EqualTo(5).And.TypeOf(typeof(double)));
            Assert.That(AtMost("4", 5), Is.EqualTo(4).And.TypeOf(typeof(double)));
            Assert.That(AtMost("4a", 5), Is.EqualTo("4a"));
            Assert.That(AtMost("10b", 5), Is.EqualTo("10b"));
            Assert.That(AtMost(null, 5), Is.Null);
            Assert.That(AtMost(5, null), Is.EqualTo(5).And.TypeOf(typeof(int)));
        }

        [Test]
        public void TestAbsIntegerTypes()
        {
            long valueInt64 = ((long)Int32.MinValue) - 1;
            long absValueInt64 = Math.Abs(valueInt64);

            Assert.That(Abs(-1), Is.EqualTo(1).And.TypeOf(typeof(double)));
            Assert.That(Abs(valueInt64), Is.EqualTo(absValueInt64).And.TypeOf(typeof(double)));
        }

        [Test]
        public void TestAbsFloatingPointTypes()
        {
            Assert.That(Abs(10), Is.EqualTo(10).And.TypeOf(typeof(double)));
            Assert.That(Abs(-5), Is.EqualTo(5).And.TypeOf(typeof(double)));
            Assert.That(Abs(19.86), Is.EqualTo(19.86).And.TypeOf(typeof(double)));
            Assert.That(Abs(-19.86m), Is.EqualTo(19.86m).And.TypeOf(typeof(double)));
            Assert.That(Abs(-19.86m), Is.EqualTo(19.86).And.TypeOf(typeof(double)));
            Assert.That(Abs("10"), Is.EqualTo(10).And.TypeOf(typeof(double)));
            Assert.That(Abs(-5), Is.EqualTo(5).And.TypeOf(typeof(double)));
            Assert.That(Abs("30.60"), Is.EqualTo(30.60).And.TypeOf(typeof(double)));
        }

        [Test]
        public void TestAbsBadValues()
        {
            Assert.That(Abs("notNumber"), Is.EqualTo(0).And.TypeOf(typeof(double)));
            Assert.That(Abs("30.60a"), Is.EqualTo(0).And.TypeOf(typeof(double)));
            Assert.That(Abs(null), Is.EqualTo(0).And.TypeOf(typeof(double)));
        }

        [Test]
        public void TestCeilIntegerTypes()
        {
            long valueInt64 = ((long)Int32.MaxValue) + 1;

            Assert.That(Ceil(1), Is.EqualTo(1).And.TypeOf(typeof(decimal)));
            Assert.That(Ceil(valueInt64), Is.EqualTo(valueInt64).And.TypeOf(typeof(decimal)));
        }

        [Test]
        public void TestCeilFloatingPointTypes()
        {
            Assert.That(Ceil(1.9), Is.EqualTo(2).And.TypeOf(typeof(decimal)));
            Assert.That(Ceil(1.9m), Is.EqualTo(2).And.TypeOf(typeof(decimal)));
            Assert.That(Ceil("1.9"), Is.EqualTo(2).And.TypeOf(typeof(decimal)));
        }

        [Test]
        public void TestCeilBadInput()
        {
            Assert.That(Ceil(null), Is.Null);
            Assert.That(Ceil(""), Is.Null);
            Assert.That(Ceil("two"), Is.Null);
        }

        [Test]
        public void TestFloorIntegerTypes()
        {
            long valueInt64 = ((long)Int32.MaxValue) + 1;

            Assert.That(Floor(1), Is.EqualTo(1).And.TypeOf(typeof(decimal)));
            Assert.That(Floor(valueInt64), Is.EqualTo(valueInt64).And.TypeOf(typeof(decimal)));
        }

        [Test]
        public void TestFloorFloatingPointTypes()
        {
            Assert.That(Floor(1.9), Is.EqualTo(1).And.TypeOf(typeof(decimal)));
            Assert.That(Floor(1.9m), Is.EqualTo(1).And.TypeOf(typeof(decimal)));
            Assert.That(Floor("1.9"), Is.EqualTo(1).And.TypeOf(typeof(decimal)));
        }

        [Test]
        public void TestFloorBadInput()
        {
            Assert.That(Floor(null), Is.Null);
            Assert.That(Floor(""), Is.Null);
            Assert.That(Floor("two"), Is.Null);
        }
    }
}
