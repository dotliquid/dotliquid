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
    public class StandardFiltersV22aTests : StandardFiltersTestsBase
    {
        public override SyntaxCompatibility SyntaxCompatibilityLevel => SyntaxCompatibility.DotLiquid22a;
        public override CapitalizeDelegate Capitalize => i => StandardFilters.Capitalize(i);
        public override PlusDelegate Plus => (i, o) => StandardFilters.Plus(_context, i, o);
        public override ReplaceDelegate Replace => (i, s, r) => StandardFilters.Replace(i, s, r);
        public override ReplaceFirstDelegate ReplaceFirst => (a, b, c) => StandardFilters.ReplaceFirst(a, b, c);
        public override SliceDelegate Slice => (a, b, c) => c.HasValue ? StandardFilters.Slice(a, b, c.Value) : StandardFilters.Slice(a, b);
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
        public void TestSlice()
        {
            // Verify Liquid compliance from V22a syntax:
            Assert.That(Slice(null, 1), Is.EqualTo("")); // DotLiquid test case
            Assert.That(Slice("", 10), Is.EqualTo("")); // DotLiquid test case

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
