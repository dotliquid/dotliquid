using System;
using System.Collections;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests.Filters
{
    [TestFixture]
    public class StandardFiltersV20Tests : StandardFiltersTestsBase
    {
        public override IFormatProvider FormatProvider => CultureInfo.InvariantCulture;
        public override SyntaxCompatibility SyntaxCompatibilityLevel => SyntaxCompatibility.DotLiquid20;
        public override CapitalizeDelegate Capitalize => i => LegacyFilters.Capitalize(_context, i);
        public override MathDelegate Divide => (i, o) => StandardFilters.DividedBy(_context, i, o);
        public override MathDelegate Plus => (i, o) => LegacyFilters.Plus(_context, i, o);
        public override MathDelegate Minus => (i, o) => StandardFilters.Minus(_context, i, o);
        public override MathDelegate Modulo => (i, o) => StandardFilters.Modulo(_context, i, o);
        public override RemoveFirstDelegate RemoveFirst => (a, b) => LegacyFilters.RemoveFirst(a, b);
        public override ReplaceDelegate Replace => (i, s, r) => LegacyFilters.Replace(i, s, r);
        public override ReplaceFirstDelegate ReplaceFirst => (a, b, c) => LegacyFilters.ReplaceFirst(a, b, c);
        public override SliceDelegate Slice => (a, b, c) => c.HasValue ? LegacyFilters.Slice(a, b, c.Value) : LegacyFilters.Slice(a, b);
        public override SplitDelegate Split => (i, p) => LegacyFilters.Split(i, p);
        public override SumDelegate Sum => (i, p) => StandardFilters.Sum(_context, i, p);
        public override MathDelegate Times => (i, o) => LegacyFilters.Times(_context, i, o);
        public override TruncateWordsDelegate TruncateWords => (i, w, s) =>
        {
            if (w.HasValue)
                return s == null ? LegacyFilters.TruncateWords(i, w.Value) : LegacyFilters.TruncateWords(i, w.Value, s);
            return LegacyFilters.TruncateWords(i);
        };

        [Test]
        public void TestCapitalizeBehavesLikeTitleize()
        {
            Assert.That(Capitalize(input: "That is one sentence."), Is.EqualTo("That Is One Sentence."));
            Assert.That(Capitalize(input: "title"), Is.EqualTo("Title"));
        }

        [Test]
        public void TestDividedByStringThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => Divide(input: "12", operand: 3));
            Assert.Throws<InvalidOperationException>(() => Divide(input: 12, operand: "3"));
        }

        [Test]
        public void TestPlusStringConcatenates()
        {
            Assert.That(Plus(input: "1", operand: 1), Is.EqualTo("11"));
            Assert.Throws<InvalidOperationException>(() => Plus(input: 1, operand: "1"));
        }

        [Test]
        public void TestMinusStringThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => Minus(input: "2", operand: 1));
            Assert.Throws<InvalidOperationException>(() => Minus(input: 2, operand: "1"));
        }

        [Test]
        public void TestModuloStringThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => Modulo(input: "3", operand: 2));
            Assert.Throws<InvalidOperationException>(() => Modulo(input: 2, operand: "2"));
        }

        [Test]
        public void TestTimesStringReplicates()
        {
            Assert.That(StandardFilters.Join((IEnumerable)Times(input: "foo", operand: 4), ""), Is.EqualTo("foofoofoofoo"));
            Assert.That(StandardFilters.Join((IEnumerable)Times(input: "3", operand: 4), ""), Is.EqualTo("3333"));
            Assert.Throws<InvalidOperationException>(() => Times(input: 3, operand: "4"));
            Assert.Throws<InvalidOperationException>(() => Times(input: "3", operand: "4"));
        }

        [Test]
        public void TestRemoveFirstRegexWorks()
        {
            Assert.That(RemoveFirst(input: "Mr. Jones", @string: "."), Is.EqualTo(expected: "r. Jones"));
            Assert.That(RemoveFirst(input: "a a a a", @string: "[Aa] "), Is.EqualTo("a a a"));
        }

        [Test]
        public void TestReplaceRegexWorks()
        {
            Assert.That(actual: Replace(input: "a A A a", @string: "[Aa]", replacement: "b"), Is.EqualTo(expected: "b b b b"));
        }

        [Test]
        public void TestReplaceFirstRegexWorks()
        {
            Assert.That(ReplaceFirst(input: "a A A a", @string: "[Aa]", replacement: "b"), Is.EqualTo(expected: "b A A a"));
        }
    }
}
