using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class StandardFiltersV20Tests : StandardFiltersTestsBase
    {
        public override SyntaxCompatibility SyntaxCompatibilityLevel => SyntaxCompatibility.DotLiquid20;
        public override CapitalizeDelegate Capitalize => i => LegacyFilters.Capitalize(_context, i);
        public override PlusDelegate Plus => (i, o) => LegacyFilters.Plus(_context, i, o);
        public override ReplaceDelegate Replace => (i, s, r) => LegacyFilters.Replace(i, s, r);
        public override ReplaceFirstDelegate ReplaceFirst => (a, b, c) => LegacyFilters.ReplaceFirst(a, b, c);
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
        public void TestCapitalizeBehavesLikeTitleize()
        {
            Assert.That(Capitalize(input: "That is one sentence."), Is.EqualTo("That Is One Sentence."));
            Assert.That(Capitalize(input: "title"), Is.EqualTo("Title"));
        }


        [Test]
        public void TestPlusStringConcatenates()
        {
            Assert.That(Plus(input: "1", operand: 1), Is.EqualTo("11"));
            Assert.Throws<InvalidOperationException>(() => Plus(input: 1, operand: "1"));
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
