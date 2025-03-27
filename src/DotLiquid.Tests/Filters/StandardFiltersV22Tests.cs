using System;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests.Filters
{
    [TestFixture]
    public class StandardFiltersV22Tests : StandardFiltersTestsBase
    {
        public override IFormatProvider FormatProvider => CultureInfo.InvariantCulture;
        public override SyntaxCompatibility SyntaxCompatibilityLevel => SyntaxCompatibility.DotLiquid22;
        public override CapitalizeDelegate Capitalize => i => StandardFilters.Capitalize(i);
        public override MathDelegate Divide => (i, o) => StandardFilters.DividedBy(_context, i, o);
        public override MathDelegate Plus => (i, o) => StandardFilters.Plus(_context, i, o);
        public override MathDelegate Minus => (i, o) => StandardFilters.Minus(_context, i, o);
        public override MathDelegate Modulo => (i, o) => StandardFilters.Modulo(_context, i, o);
        public override RemoveFirstDelegate RemoveFirst => (a, b) => LegacyFilters.RemoveFirstV21(a, b);
        public override ReplaceDelegate Replace => (i, s, r) => StandardFilters.Replace(i, s, r);
        public override ReplaceFirstDelegate ReplaceFirst => (i, s, r) => LegacyFilters.ReplaceFirstV21(i, s, r);
        public override SliceDelegate Slice => (i, s, l) => l.HasValue ? LegacyFilters.Slice(i, s, l.Value) : LegacyFilters.Slice(i, s);
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
        public void TestCapitalizeDowncaseAllButFirst()
        {
            Assert.That(Capitalize(input: "my boss is Mr. Doe."), Is.EqualTo("My boss is mr. doe."));
            Assert.That(Capitalize(input: "my Great Title"), Is.EqualTo("My great title"));
        }

        [Test]
        public void TestSlice()
        {
            // Verify backwards compatibility for pre-22a syntax (DotLiquid returns null for null input or empty slice)
            Assert.That(Slice(null, 1), Is.EqualTo(null)); // DotLiquid test case
            Assert.That(Slice("", 10), Is.EqualTo(null)); // DotLiquid test case
            Assert.That(Slice(123, 1), Is.EqualTo(123)); // Ignore invalid input

            Assert.That(Slice(null, 0), Is.EqualTo(null)); // Liquid test case
            Assert.That(Slice("foobar", 100, 10), Is.EqualTo(null)); // Liquid test case
        }
    }
}
