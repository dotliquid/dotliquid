using System;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests.Filters
{
    [TestFixture]
    public class StandardFiltersV24Tests : StandardFiltersTestsBase
    {
        public override SyntaxCompatibility SyntaxCompatibilityLevel => SyntaxCompatibility.DotLiquid24;
        public override CapitalizeDelegate Capitalize => i => StandardFilters.Capitalize(i);
        public override MathDelegate Divide => (i, o) => StandardFilters.DividedBy(_context, i, o);
        public override MathDelegate Plus => (i, o) => StandardFilters.Plus(_context, i, o);
        public override MathDelegate Minus => (i, o) => StandardFilters.Minus(_context, i, o);
        public override MathDelegate Modulo => (i, o) => StandardFilters.Modulo(_context, i, o);
        public override RemoveFirstDelegate RemoveFirst => (a, b) => StandardFilters.RemoveFirst(a, b);
        public override ReplaceDelegate Replace => (i, s, r) => StandardFilters.Replace(i, s, r);
        public override ReplaceFirstDelegate ReplaceFirst => (a, b, c) => StandardFilters.ReplaceFirst(a, b, c);
        public override SliceDelegate Slice => (a, b, c) => c.HasValue ? StandardFilters.Slice(a, b, c.Value) : StandardFilters.Slice(a, b);
        public override SplitDelegate Split => (i, p) => StandardFilters.Split(i, p);
        public override MathDelegate Times => (i, o) => StandardFilters.Times(_context, i, o);
        public override TruncateWordsDelegate TruncateWords => (i, w, s) =>
        {
            if (w.HasValue)
                return s == null ? StandardFilters.TruncateWords(i, w.Value) : StandardFilters.TruncateWords(i, w.Value, s);
            return StandardFilters.TruncateWords(i);
        };

        private Context _context;

        [OneTimeSetUp]
        public void SetUp()
        {
            _context = new Context(CultureInfo.InvariantCulture)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibilityLevel
            };
        }

        [Test]
        public void TestSplitNullReturnsEmptyArray()
        {
            Assert.That(Split(null, null), Has.Exactly(0).Items);
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
    }
}
