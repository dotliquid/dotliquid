using System;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests.Filters
{
    [TestFixture]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar Code Smell", "S101:Types should be named in camel case", Justification = "<Pending>")]
    public class StandardFiltersV22aTests : StandardFiltersTestsBase
    {
        public override SyntaxCompatibility SyntaxCompatibilityLevel => SyntaxCompatibility.DotLiquid22a;
        public override CapitalizeDelegate Capitalize => i => StandardFilters.Capitalize(i);
        public override MathDelegate Divide => (i, o) => StandardFilters.DividedBy(_context, i, o);
        public override MathDelegate Plus => (i, o) => StandardFilters.Plus(_context, i, o);
        public override MathDelegate Minus => (i, o) => StandardFilters.Minus(_context, i, o);
        public override MathDelegate Modulo => (i, o) => StandardFilters.Modulo(_context, i, o);
        public override RemoveFirstDelegate RemoveFirst => (a, b) => LegacyFilters.RemoveFirstV21(a, b);
        public override ReplaceDelegate Replace => (i, s, r) => StandardFilters.Replace(i, s, r);
        public override ReplaceFirstDelegate ReplaceFirst => (i, s, r) => LegacyFilters.ReplaceFirstV21(i, s, r);
        public override SliceDelegate Slice => (a, b, c) => c.HasValue ? StandardFilters.Slice(a, b, c.Value) : StandardFilters.Slice(a, b);
        public override SplitDelegate Split => (i, p) => LegacyFilters.Split(i, p);
        public override MathDelegate Times => (i, o) => StandardFilters.Times(_context, i, o);
        public override TruncateWordsDelegate TruncateWords => (i, w, s) =>
        {
            if (w.HasValue)
                return s == null ? LegacyFilters.TruncateWords(i, w.Value) : LegacyFilters.TruncateWords(i, w.Value, s);
            return LegacyFilters.TruncateWords(i);
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
        public void TestReplaceFirstInvalidSearchReturnsInput()
        {
            Assert.That(ReplaceFirst(input: "a a a a", @string: null, replacement: "b"), Is.EqualTo("a a a a"));
            Assert.That(ReplaceFirst(input: "a a a a", @string: "", replacement: "b"), Is.EqualTo("a a a a"));
        }

        [Test]
        public void TestSlice()
        {
            // Verify Liquid compliance from V22a syntax:
            Assert.That(Slice(null, 1), Is.EqualTo("")); // DotLiquid test case
            Assert.That(Slice("", 10), Is.EqualTo("")); // DotLiquid test case
            Assert.That(Slice(123, 1), Is.EqualTo(123)); // Ignore invalid input

            Assert.That(Slice(null, 0), Is.EqualTo("")); // Liquid test case
            Assert.That(Slice("foobar", 100, 10), Is.EqualTo("")); // Liquid test case
        }

        [Test]
        public void TestSplitNullReturnsArrayWithNull()
        {
            Assert.That(Split(null, null), Is.EqualTo(new string[] { null }).AsCollection);

        }

        [Test]
        public void TestTruncateWordsLessOneWordAllowed()
        {
            Assert.That(TruncateWords("Ground control to Major Tom.", 0), Is.EqualTo("..."));
            Assert.That(TruncateWords("Ground control to Major Tom.", -1), Is.EqualTo("..."));
        }
    }
}
