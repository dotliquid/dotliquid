using System;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests.Filters
{
    [TestFixture]
    public class StandardFiltersLatestFrenchTests : StandardFiltersV24Tests
    {
        public override IFormatProvider FormatProvider => new CultureInfo("fr-FR");
        public override SyntaxCompatibility SyntaxCompatibilityLevel => SyntaxCompatibility.DotLiquidLatest;

        [OneTimeSetUp]
        public void SetUpLatest()
        {
            Assert.That(SyntaxCompatibility.DotLiquidLatest, Is.EqualTo(SyntaxCompatibility.DotLiquid24),
                "Please update the base class to the latest test class");
        }

    }
}
