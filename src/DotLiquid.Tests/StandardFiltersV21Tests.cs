using System;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class StandardFiltersV21Tests : StandardFiltersTestsBase
    {
        public override SyntaxCompatibility SyntaxCompatibilityLevel => SyntaxCompatibility.DotLiquid21;
        public override CapitalizeDelegate Capitalize => i => LegacyFilters.CapitalizeV21(i);
        public override PlusDelegate Plus => (i, o) => StandardFilters.Plus(_context, i, o);
        public override ReplaceDelegate Replace => (i, s, r) => StandardFilters.Replace(i, s, r);
        public override ReplaceFirstDelegate ReplaceFirst => (a, b, c) => StandardFilters.ReplaceFirst(a, b, c);
        public override SliceDelegate Slice => (a, b, c) => c.HasValue ? LegacyFilters.Slice(a, b, c.Value) : LegacyFilters.Slice(a, b);
        public override SplitDelegate Split => (i, p) => LegacyFilters.Split(i, p);
        public override TruncateWordsDelegate TruncateWords => (i, w, s) => s == null ? LegacyFilters.TruncateWords(i, w) : LegacyFilters.TruncateWords(i, w, s);

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
        public void TestCapitalizeBehavesLikeUpcaseFirst()
        {
            Assert.That(Capitalize(input: " my boss is Mr. Doe."), Is.EqualTo(" My boss is Mr. Doe."));
            Assert.That(Capitalize(input: "my great title"), Is.EqualTo("My great title"));
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
        public void TestReplaceRegexFails()
        {
            Assert.That(Replace(input: "a A A a", @string: "[Aa]", replacement: "b"), Is.EqualTo(expected: "a A A a"));
        }

        [Test]
        public void TestReplaceFirstRegexFails()
        {
            Assert.That(ReplaceFirst(input: "a A A a", @string: "[Aa]", replacement: "b"), Is.EqualTo(expected: "a A A a"));
        }
    }
}
