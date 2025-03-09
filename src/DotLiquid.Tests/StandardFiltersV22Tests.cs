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
    public class StandardFiltersV22Tests : StandardFiltersTestsBase
    {
        public override SyntaxCompatibility SyntaxCompatibilityLevel => SyntaxCompatibility.DotLiquid22;
        public override CapitalizeDelegate Capitalize => i => StandardFilters.Capitalize(i);
        public override PlusDelegate Plus => (i, o) => StandardFilters.Plus(_context, i, o);
        public override ReplaceDelegate Replace => (i, s, r) => StandardFilters.Replace(i, s, r);
        public override ReplaceFirstDelegate ReplaceFirst => (i, s, r) => StandardFilters.ReplaceFirst(i, s, r);
        public override SliceDelegate Slice => (i, s, l) => l.HasValue ? LegacyFilters.Slice(i, s, l.Value) : LegacyFilters.Slice(i, s);
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

            Assert.That(Slice(null, 0), Is.EqualTo(null)); // Liquid test case
            Assert.That(Slice("foobar", 100, 10), Is.EqualTo(null)); // Liquid test case
        }
    }
}
