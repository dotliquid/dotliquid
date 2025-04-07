using System;
using System.Globalization;
using DotLiquid.Tests.Helpers;
using DotLiquid.Util;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotLiquid.Tests.Filters
{
    [TestFixture]
    public class StandardFiltersV24Tests : StandardFiltersTestsBase
    {
        public override IFormatProvider FormatProvider => CultureInfo.InvariantCulture;
        public override SyntaxCompatibility SyntaxCompatibilityLevel => SyntaxCompatibility.DotLiquid24;
        public override CapitalizeDelegate Capitalize => i => StandardFilters.Capitalize(i);
        public override MathDelegate DividedBy => (i, o) => StandardFilters.DividedBy(_context, i, o);
        public override MathDelegate Plus => (i, o) => StandardFilters.Plus(_context, i, o);
        public override MathDelegate Minus => (i, o) => StandardFilters.Minus(_context, i, o);
        public override MathDelegate Modulo => (i, o) => StandardFilters.Modulo(_context, i, o);
        public override RemoveFirstDelegate RemoveFirst => (a, b) => StandardFilters.RemoveFirst(a, b);
        public override ReplaceDelegate Replace => (i, s, r) => StandardFilters.Replace(i, s, r);
        public override ReplaceFirstDelegate ReplaceFirst => (a, b, c) => StandardFilters.ReplaceFirst(a, b, c);
        public override RoundDelegate Round => (i, p) => StandardFilters.Round(_context, i, p);
        public override TwoInputDelegate AtLeast => (i, p) => StandardFilters.AtLeast(_context, i, p);
        public override TwoInputDelegate AtMost => (i, p) => StandardFilters.AtMost(_context, i, p);
        public override OneInputDelegate Abs => i => StandardFilters.Abs(_context, i);
        public override OneInputDelegate Ceil => i => StandardFilters.Ceil(_context, i);
        public override OneInputDelegate Floor => i => StandardFilters.Floor(_context, i);
        public override SliceDelegate Slice => (a, b, c) => c.HasValue ? StandardFilters.Slice(a, b, c.Value) : StandardFilters.Slice(a, b);
        public override SplitDelegate Split => (i, p) => StandardFilters.Split(i, p);
        public override SumDelegate Sum => (i, p) => StandardFilters.Sum(_context, i, p);
        public override MathDelegate Times => (i, o) => StandardFilters.Times(_context, i, o);
        public override TruncateWordsDelegate TruncateWords => (i, w, s) =>
        {
            if (w.HasValue)
                return s == null ? StandardFilters.TruncateWords(i, w.Value) : StandardFilters.TruncateWords(i, w.Value, s);
            return StandardFilters.TruncateWords(i);
        };

        [Test]
        [TestCaseSource(typeof(NumericHelper), nameof(NumericHelper.GetNumericTypeCombinations))]
        public void TestDividedByTypeCombinations(ValueTuple<Type, Type> types)
        {
            var t1 = types.Item1;
            var t2 = types.Item2;
            var val1 = Convert.ChangeType(2, t1);
            var val2 = Convert.ChangeType(2, t2);

            Assert.That(DividedBy(val1, val2), Is.EqualTo(1));
            Assert.That(DividedBy(val2, val1), Is.EqualTo(1));
        }

        [Test]
        public void TestDividedByStringIsParsed()
        {
            Assert.That(DividedBy(input: "12", operand: 3), Is.EqualTo(4));
            Assert.That(DividedBy(input: 12, operand: "3"), Is.EqualTo(4));
        }

        [Test]
        public void TestDividedByBadValues()
        {
            Assert.Multiple(() =>
            {
                Assert.That(DividedBy(input: 1.0, operand: null), Is.EqualTo(double.PositiveInfinity));
                Assert.That(DividedBy(input: null, operand: 3), Is.Zero);
            });
        }

        [Test]
        public void TestDividedByZeroInteger()
        {
            Assert.Multiple(() =>
            {
                Assert.That(DividedBy(input: 1, operand: 0), Is.EqualTo(double.PositiveInfinity));
                Assert.That(DividedBy(input: -1, operand: 0), Is.EqualTo(double.NegativeInfinity));
            });
        }

        [Test]
        [TestCaseSource(typeof(NumericHelper), nameof(NumericHelper.GetNumericTypeCombinations))]
        public void TestMinusTypeCombinations(ValueTuple<Type, Type> types)
        {
            var t1 = types.Item1;
            var t2 = types.Item2;
            var val1 = Convert.ChangeType(2, t1);
            var val2 = Convert.ChangeType(2, t2);

            Assert.That(Minus(val1, val2), Is.EqualTo(0));
            Assert.That(Minus(val2, val1), Is.EqualTo(0));
        }

        [Test]
        [TestCaseSource(typeof(NumericHelper), nameof(NumericHelper.GetNumericTypeCombinations))]
        public void TestModuloTypeCombinations(ValueTuple<Type, Type> types)
        {
            var t1 = types.Item1;
            var t2 = types.Item2;
            var val1 = Convert.ChangeType(2, t1);
            var val2 = Convert.ChangeType(2, t2);

            Assert.That(Modulo(val1, val2), Is.EqualTo(0));
            Assert.That(Modulo(val2, val1), Is.EqualTo(0));
        }

        [Test]
        public void TestModuloBadValues()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Modulo(input: 1.0, operand: null), Is.NaN);
                Assert.That(Modulo(input: null, operand: 3), Is.Zero);
            });
        }

        [Test]
        public void TestModuloZeroInteger()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Modulo(input: 1, operand: 0), Is.NaN);
                Assert.That(Modulo(input: -1, operand: 0), Is.NaN);
            });
        }

        [Test]
        [TestCaseSource(typeof(NumericHelper), nameof(NumericHelper.GetNumericTypeCombinations))]
        public void TestPlusTypeCombinations(ValueTuple<Type, Type> types)
        {
            var t1 = types.Item1;
            var t2 = types.Item2;
            var val1 = Convert.ChangeType(1, t1);
            var val2 = Convert.ChangeType(2, t2);

            Assert.That(Plus(val1, val2), Is.EqualTo(3));
            Assert.That(Plus(val2, val1), Is.EqualTo(3));
        }

        [Test]
        public void TestReplaceFirstInvalidSearchPrepends()
        {
            Assert.That(ReplaceFirst(input: "a a a a", @string: null, replacement: "b"), Is.EqualTo("ba a a a"));
            Assert.That(ReplaceFirst(input: "a a a a", @string: "", replacement: "b"), Is.EqualTo("ba a a a"));
        }

        [Test]
        public void TestSplitNullReturnsEmptyArray()
        {
            Assert.That(Split(null, null), Has.Exactly(0).Items);
        }

        [Test]
        [TestCaseSource(typeof(NumericHelper), nameof(NumericHelper.GetNumericTypeCombinations))]
        public void TestTimesTypeCombinations(ValueTuple<Type, Type> types)
        {
            var t1 = types.Item1;
            var t2 = types.Item2;
            var val1 = Convert.ChangeType(1, t1);
            var val2 = Convert.ChangeType(2, t2);

            Assert.That(Times(val1, val2), Is.EqualTo(2));
            Assert.That(Times(val2, val1), Is.EqualTo(2));
        }

        [Test]
        public void TestTimesIntegerOverflow()
        {
            // Integers will be promoted to double on Overflow
            var expectedResult = ((double)ulong.MaxValue) * ((double)ulong.MaxValue);
            Assert.That(Times(input: ulong.MaxValue, operand: ulong.MaxValue), Is.EqualTo(expectedResult));
        }

        [Test]
        public void TestTruncateWordsLessOneWordIgnored()
        {
            Assert.That(TruncateWords("Ground control to Major Tom.", 0), Is.EqualTo("Ground..."));
            Assert.That(TruncateWords("Ground control to Major Tom.", -1), Is.EqualTo("Ground..."));
        }

        [Test]
        public void TestTruncateWordsWhitespaceCollapsed()
        {
            Assert.That(TruncateWords("    one    two three    four  ", 2), Is.EqualTo("one two..."));
            Assert.That(TruncateWords("one  two\tthree\nfour", 3), Is.EqualTo("one two three..."));
        }

        [Test]
        public void TestRoundTypes()
        {
            Assert.That(Round("1.2345678", 2.0), Is.EqualTo(1.23).And.TypeOf(typeof(decimal)));
            Assert.That(Round(1.2345678f, 2.0), Is.EqualTo(1.23).And.TypeOf(typeof(double)));
            Assert.That(Round(1.2345678, 2.0), Is.EqualTo(1.23).And.TypeOf(typeof(double)));
            Assert.That(Round(1.2345678m, 2.0), Is.EqualTo(1.23).And.TypeOf(typeof(decimal)));

            Assert.That(Round(1.2345678, 2), Is.EqualTo(1.23).And.TypeOf(typeof(double)));
            Assert.That(Round(1.2345678, 2.0f), Is.EqualTo(1.23).And.TypeOf(typeof(double)));
            Assert.That(Round(1.2345678, 2.0m), Is.EqualTo(1.23).And.TypeOf(typeof(double)));
            Assert.That(Round(1.2345678m, "2.0"), Is.EqualTo(1.23).And.TypeOf(typeof(decimal)));

            Assert.That(Round((byte)1, 2), Is.EqualTo(1).And.TypeOf(typeof(byte)));
            Assert.That(Round((sbyte)-1, 2), Is.EqualTo(-1).And.TypeOf(typeof(sbyte)));
            Assert.That(Round((ushort)1, 2), Is.EqualTo(1).And.TypeOf(typeof(ushort)));
            Assert.That(Round((short)-1, 2), Is.EqualTo(-1).And.TypeOf(typeof(short)));
            Assert.That(Round((uint)1, 2), Is.EqualTo(1).And.TypeOf(typeof(uint)));
            Assert.That(Round((int)-1, 2), Is.EqualTo(-1).And.TypeOf(typeof(int)));
            Assert.That(Round((ulong)1, 2), Is.EqualTo(1).And.TypeOf(typeof(ulong)));
            Assert.That(Round((long)-1, 2), Is.EqualTo(-1).And.TypeOf(typeof(long)));
        }

        [Test]
        public void TestRoundHandlesBadParams()
        {
            var value = String.Format(FormatProvider, "{0:0.0000000}", 1.2345678);

            Assert.That(Round(value, "two"), Is.EqualTo(1m).And.TypeOf(typeof(decimal)));
            Assert.That(Round(value, "-2"), Is.EqualTo(1m).And.TypeOf(typeof(decimal)));
            Assert.That(Round(1.123456789012345678901234567890123m, 50),
                Is.EqualTo(1.1234567890123456789012345679m).And.TypeOf(typeof(decimal))); // max = 28 places
            Assert.That(Round(value, "2.7"), Is.EqualTo(1.23m).And.TypeOf(typeof(decimal)));

            Assert.That(Round(value, 2.7), Is.EqualTo(1.23m));
            Assert.That(Round(value, 3.1), Is.EqualTo(1.235m));
        }

        [Test]
        [TestCaseSource(typeof(NumericHelper), nameof(NumericHelper.GetNumericTypeCombinations))]
        public void TestAtLeastTypes(ValueTuple<Type, Type> types)
        {
            var t1 = types.Item1;
            var t2 = types.Item2;
            var val1 = Convert.ChangeType(1, t1);
            var val2 = Convert.ChangeType(2, t2);

            var resultType = NumericConverter.GetBinaryResultType(t1, t2);
            Assert.That(AtLeast(val1, val2), Is.EqualTo(2).And.TypeOf(t2));

            resultType = NumericConverter.GetBinaryResultType(t2, t1);
            Assert.That(AtLeast(val2, val1), Is.EqualTo(2).And.TypeOf(t2));

            resultType = NumericConverter.GetBinaryResultType(t1, t1);
            Assert.That(AtLeast(val1, val1), Is.EqualTo(1).And.TypeOf(t1));

            resultType = NumericConverter.GetBinaryResultType(t2, t2);
            Assert.That(AtLeast(val2, val2), Is.EqualTo(2).And.TypeOf(t2));
        }

        [Test]
        public void TestAtLeastEdgeCases()
        {
            // We can't compare decimal and double using comparison operators
            Assert.That(AtLeast(1.2, decimal.MaxValue), Is.EqualTo(decimal.MaxValue).And.TypeOf(typeof(decimal)));

            // We can't compare decimal and double using comparison operators, and double.MaxValue will overflow decimal
            Assert.That(AtLeast(double.MaxValue, decimal.MaxValue), Is.EqualTo(double.MaxValue).And.TypeOf(typeof(double)));
        }

        [Test]
        public void TestAtLeastBadParams()
        {
            Assert.That(AtLeast("notNumber", 5), Is.EqualTo(5));
            Assert.That(AtLeast(5, "notNumber"), Is.EqualTo(5));
            Assert.That(AtLeast("10a", 5), Is.EqualTo(5));
            Assert.That(AtLeast("4b", 5), Is.EqualTo(5));
            Assert.That(AtLeast(null, 5), Is.EqualTo(5));
            Assert.That(AtLeast(5, null), Is.EqualTo(5));
        }

        [Test]
        [TestCaseSource(typeof(NumericHelper), nameof(NumericHelper.GetNumericTypeCombinations))]
        public void TestAtMostTypes(ValueTuple<Type, Type> types)
        {
            var t1 = types.Item1;
            var t2 = types.Item2;
            var val1 = Convert.ChangeType(1, t1);
            var val2 = Convert.ChangeType(2, t2);

            var resultType = NumericConverter.GetBinaryResultType(t1, t2);
            Assert.That(AtMost(val1, val2), Is.EqualTo(1).And.TypeOf(t1));

            resultType = NumericConverter.GetBinaryResultType(t2, t1);
            Assert.That(AtMost(val2, val1), Is.EqualTo(1).And.TypeOf(t1));

            resultType = NumericConverter.GetBinaryResultType(t1, t1);
            Assert.That(AtMost(val1, val1), Is.EqualTo(1).And.TypeOf(t1));

            resultType = NumericConverter.GetBinaryResultType(t2, t2);
            Assert.That(AtMost(val2, val2), Is.EqualTo(2).And.TypeOf(t2));
        }

        [Test]
        public void TestAtMostEdgeCases()
        {
            // We can't compare decimal and double using comparison operators
            Assert.That(AtMost(1.2, decimal.MaxValue), Is.EqualTo(1.2).And.TypeOf(typeof(double)));

            // We can't compare decimal and double using comparison operators, and double.MaxValue will overflow decimal
            Assert.That(AtMost(double.MaxValue, decimal.MaxValue), Is.EqualTo(decimal.MaxValue).And.TypeOf(typeof(decimal)));
        }

        [Test]
        public void TestAtMostBadParams()
        {
            Assert.That(AtMost("notNumber", 5), Is.EqualTo(0));
            Assert.That(AtMost(5, "notNumber"), Is.EqualTo(0));
            Assert.That(AtMost("4a", 5), Is.EqualTo(0));
            Assert.That(AtMost("10b", 5), Is.EqualTo(0));
            Assert.That(AtMost(null, 5), Is.EqualTo(0));
            Assert.That(AtMost(5, null), Is.EqualTo(0));
        }

        [Test]
        public void TestAbsIntegerTypes()
        {
            long valueInt64 = ((long)Int32.MinValue) - 1;
            long absValueInt64 = Math.Abs(valueInt64);
            Assert.Multiple(() =>
            {
                Assert.That(Abs("-1"), Is.EqualTo(1).And.TypeOf(typeof(int)));
                Assert.That(Abs($"{valueInt64}"), Is.EqualTo(absValueInt64).And.TypeOf(typeof(long)));
                Assert.That(Abs($"{ulong.MaxValue}"), Is.EqualTo(ulong.MaxValue).And.TypeOf(typeof(decimal)));

                Assert.That(Abs((byte)1), Is.EqualTo(1).And.TypeOf(typeof(byte)));
                Assert.That(Abs((sbyte)-1), Is.EqualTo(1).And.TypeOf(typeof(int)));
                Assert.That(Abs((ushort)1), Is.EqualTo(1).And.TypeOf(typeof(ushort)));
                Assert.That(Abs((short)-1), Is.EqualTo(1).And.TypeOf(typeof(int)));
                Assert.That(Abs((uint)1), Is.EqualTo(1).And.TypeOf(typeof(uint)));
                Assert.That(Abs((int)-1), Is.EqualTo(1).And.TypeOf(typeof(int)));
                Assert.That(Abs((ulong)1), Is.EqualTo(1).And.TypeOf(typeof(ulong)));
                Assert.That(Abs((long)-1), Is.EqualTo(1).And.TypeOf(typeof(long)));

                Assert.That(Abs(sbyte.MinValue), Is.EqualTo(128).And.TypeOf(typeof(int)));
                Assert.That(Abs(short.MinValue), Is.EqualTo(32768).And.TypeOf(typeof(int)));
                Assert.That(Abs(int.MinValue + 1), Is.EqualTo(int.MaxValue).And.TypeOf(typeof(int)));
                Assert.That(Abs(int.MinValue), Is.EqualTo(-1 * (long)int.MinValue).And.TypeOf(typeof(long)));
                Assert.That(Abs(long.MinValue), Is.EqualTo(-1 * (decimal)long.MinValue).And.TypeOf(typeof(decimal)));
            });
        }

        [Test]
        public void TestAbsFloatingPointTypes()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Abs(-30.6m), Is.EqualTo(30.6m).And.TypeOf(typeof(decimal)));
                Assert.That(Abs(-30.6f), Is.EqualTo(30.6m).And.TypeOf(typeof(float)));
                Assert.That(Abs(-30.6), Is.EqualTo(30.6m).And.TypeOf(typeof(double)));

                Assert.That(Abs("-30.60"), Is.EqualTo(30.6m).And.TypeOf(typeof(decimal)));

                Assert.That(Abs(decimal.MinValue), Is.EqualTo(-1 * decimal.MinValue).And.TypeOf(typeof(decimal)));
                Assert.That(Abs(float.MinValue), Is.EqualTo(-1 * float.MinValue).And.TypeOf(typeof(float)));
                Assert.That(Abs(double.MinValue), Is.EqualTo(-1 * double.MinValue).And.TypeOf(typeof(double)));
            });
        }

        [Test]
        public void TestAbsBadValues()
        {
            Assert.That(Abs("notNumber"), Is.EqualTo(0).And.TypeOf(typeof(int)));
            Assert.That(Abs("30.60a"), Is.EqualTo(0).And.TypeOf(typeof(int)));
            Assert.That(Abs(null), Is.EqualTo(0).And.TypeOf(typeof(int)));
        }

        [Test]
        [TestCaseSource(typeof(NumericHelper), nameof(NumericHelper.GetNumericTypeCombinations))]
        public void TestAtLeastTypeCombinations(ValueTuple<Type, Type> types)
        {
            var t1 = types.Item1;
            var t2 = types.Item2;
            var val1 = Convert.ChangeType(1, t1);
            var val2 = Convert.ChangeType(2, t2);

            Assert.That(AtLeast(val1, val2), Is.EqualTo(2));
            Assert.That(AtLeast(val2, val1), Is.EqualTo(2));
        }

        [Test]
        [TestCaseSource(typeof(NumericHelper), nameof(NumericHelper.GetNumericTypeCombinations))]
        public void TestAtMostTypeCombinations(ValueTuple<Type, Type> types)
        {
            var t1 = types.Item1;
            var t2 = types.Item2;
            var val1 = Convert.ChangeType(1, t1);
            var val2 = Convert.ChangeType(2, t2);

            Assert.That(AtMost(val1, val2), Is.EqualTo(1));
            Assert.That(AtMost(val2, val1), Is.EqualTo(1));
        }

        [Test]
        public void TestCeilIntegerTypes()
        {
            long valueInt64 = ((long)Int32.MaxValue) + 1;
            Assert.That(Ceil("1"), Is.EqualTo(1).And.TypeOf(typeof(int)));
            Assert.That(Ceil($"{valueInt64}"), Is.EqualTo(valueInt64).And.TypeOf(typeof(long)));

            Assert.That(Ceil((byte)1), Is.EqualTo(1).And.TypeOf(typeof(byte)));
            Assert.That(Ceil((sbyte)1), Is.EqualTo(1).And.TypeOf(typeof(sbyte)));
            Assert.That(Ceil((ushort)1), Is.EqualTo(1).And.TypeOf(typeof(ushort)));
            Assert.That(Ceil((short)1), Is.EqualTo(1).And.TypeOf(typeof(short)));
            Assert.That(Ceil((uint)1), Is.EqualTo(1).And.TypeOf(typeof(uint)));
            Assert.That(Ceil((int)1), Is.EqualTo(1).And.TypeOf(typeof(int)));
            Assert.That(Ceil((ulong)1), Is.EqualTo(1).And.TypeOf(typeof(ulong)));
            Assert.That(Ceil((long)1), Is.EqualTo(1).And.TypeOf(typeof(long)));
        }

        [Test]
        public void TestCeilFloatingPointTypes()
        {
            Assert.That(Ceil(1.9f), Is.EqualTo(2).And.TypeOf(typeof(double)));
            Assert.That(Ceil(1.9), Is.EqualTo(2).And.TypeOf(typeof(double)));
            Assert.That(Ceil(1.9m), Is.EqualTo(2).And.TypeOf(typeof(decimal)));
            Assert.That(Ceil("1.9"), Is.EqualTo(2).And.TypeOf(typeof(decimal)));
        }

        [Test]
        public void TestCeilBadInput()
        {
            Assert.That(Ceil(null), Is.EqualTo(0).And.TypeOf(typeof(int)));
            Assert.That(Ceil(""), Is.EqualTo(0).And.TypeOf(typeof(int)));
            Assert.That(Ceil("two"), Is.EqualTo(0).And.TypeOf(typeof(int)));
        }

        [Test]
        public void TestFloorIntegerTypes()
        {
            long valueInt64 = ((long)Int32.MaxValue) + 1;
            Assert.That(Floor("1"), Is.EqualTo(1).And.TypeOf(typeof(int)));
            Assert.That(Floor($"{valueInt64}"), Is.EqualTo(valueInt64).And.TypeOf(typeof(long)));

            Assert.That(Floor((byte)1), Is.EqualTo(1).And.TypeOf(typeof(byte)));
            Assert.That(Floor((sbyte)1), Is.EqualTo(1).And.TypeOf(typeof(sbyte)));
            Assert.That(Floor((ushort)1), Is.EqualTo(1).And.TypeOf(typeof(ushort)));
            Assert.That(Floor((short)1), Is.EqualTo(1).And.TypeOf(typeof(short)));
            Assert.That(Floor((uint)1), Is.EqualTo(1).And.TypeOf(typeof(uint)));
            Assert.That(Floor((int)1), Is.EqualTo(1).And.TypeOf(typeof(int)));
            Assert.That(Floor((ulong)1), Is.EqualTo(1).And.TypeOf(typeof(ulong)));
            Assert.That(Floor((long)1), Is.EqualTo(1).And.TypeOf(typeof(long)));
        }

        [Test]
        public void TestFloorFloatingPointTypes()
        {
            Assert.That(Floor(1.9f), Is.EqualTo(1).And.TypeOf(typeof(double)));
            Assert.That(Floor(1.9), Is.EqualTo(1).And.TypeOf(typeof(double)));
            Assert.That(Floor(1.9m), Is.EqualTo(1).And.TypeOf(typeof(decimal)));
            Assert.That(Floor("1.9"), Is.EqualTo(1).And.TypeOf(typeof(decimal)));
        }

        [Test]
        public void TestFloorBadInput()
        {
            Assert.That(Floor(null), Is.EqualTo(0).And.TypeOf(typeof(int)));
            Assert.That(Floor(""), Is.EqualTo(0).And.TypeOf(typeof(int)));
            Assert.That(Floor("two"), Is.EqualTo(0).And.TypeOf(typeof(int)));
        }
    }
}
