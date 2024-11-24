using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class CultureFilterTests
    {
        private CultureInfo _cultureEn;
        private CultureInfo _cultureGr;
        private Context _contextV22;
        private Context _contextV22a;

        [OneTimeSetUp]
        public void SetUp()
        {
            _cultureEn = new CultureInfo("en-US");
            _cultureGr = new CultureInfo("el-GR");
        }

        [TestCase(2.5, "4")]
        [TestCase(1022.5, "1024")]
        [TestCase("1,022.5", "1024")]
        public void TestCeilEnglishCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureGr))
            {
                Assert.That(Template.Parse("{{ val | ceil | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureEn), Is.EqualTo(expected));
            }
        }

        [TestCase(2.5, "4")]
        [TestCase(1022.5, "1024")]
        [TestCase("1.022,5", "1024")]
        public void TestCeilGreekCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureEn))
            {
                Assert.That(Template.Parse("{{ val | ceil | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureGr), Is.EqualTo(expected));
            }
        }

        [TestCase(3.5, "5")]
        [TestCase(1023.5, "1025")]
        [TestCase("1,023.5", "1025")]
        public void TestRoundEnglishCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureGr))
            {
                Assert.That(Template.Parse("{{ val | round | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureEn), Is.EqualTo(expected));
            }
        }

        [TestCase(3.5, "5")]
        [TestCase(1023.5, "1025")]
        [TestCase("1.023,5", "1025")]
        public void TestRoundGreekCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureEn))
            {
                Assert.That(Template.Parse("{{ val | round | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureGr), Is.EqualTo(expected));
            }
        }

        [TestCase(3.5, "4")]
        [TestCase(1023.5, "1024")]
        [TestCase("1,023.5", "1024")]
        public void TestFloorEnglishCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureGr))
            {
                Assert.That(Template.Parse("{{ val | floor | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureEn), Is.EqualTo(expected));
            }
        }

        [TestCase(3.5, "4")]
        [TestCase(1023.5, "1024")]
        [TestCase("1.023,5", "1024")]
        public void TestFloorGreekCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureEn))
            {
                Assert.That(Template.Parse("{{ val | floor | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureGr), Is.EqualTo(expected));
            }
        }

        [TestCase(-3.5, "4.5")]
        [TestCase(1023.5, "1024.5")]
        [TestCase("1,023", "1024")]
        public void TestAbsEnglishCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureGr))
            {
                Assert.That(Template.Parse("{{ val | abs | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureEn), Is.EqualTo(expected));
            }
        }

        [TestCase(-3.5, "4,5")]
        [TestCase(1023.5, "1024,5")]
        [TestCase("1.023", "1024")]
        public void TestAbsGreekCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureEn))
            {
                Assert.That(Template.Parse("{{ val | abs | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureGr), Is.EqualTo(expected));
            }
        }

        [TestCase(3.5, "4.5")]
        [TestCase(1023.5, "1024.5")]
        [TestCase("1,023", "1024")]
        public void TestAtLeastEnglishCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureGr))
            {
                Assert.That(Template.Parse("{{ 1 | at_least: val | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureEn), Is.EqualTo(expected));
            }
        }

        [TestCase(3.5, "4,5")]
        [TestCase(1023.5, "1024,5")]
        [TestCase("1.023", "1024")]
        public void TestAtLeastGreekCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureEn))
            {
                Assert.That(Template.Parse("{{ 1 | at_least: val | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureGr), Is.EqualTo(expected));
            }
        }

        [TestCase(3.5, "4.5")]
        [TestCase(1023.5, "1024.5")]
        [TestCase("1,023", "1024")]
        public void TestAtMostEnglishCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureGr))
            {
                Assert.That(Template.Parse("{{ 2000 | at_most: val | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureEn), Is.EqualTo(expected));
            }
        }

        [TestCase(3.5, "4,5")]
        [TestCase(1023.5, "1024,5")]
        [TestCase("1.023", "1024")]
        public void TestAtMostGreekCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureEn))
            {
                Assert.That(Template.Parse("{{ 2000 | at_most: val | plus: 1 }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureGr), Is.EqualTo(expected));
            }
        }

        [TestCase(4.5, "$4.50")]
        [TestCase(1024.5, "$1,024.50")]
        [TestCase("1,024", "$1,024.00")]
        public void TestCurrencyEnglishCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureGr))
            {
                Assert.That(Template.Parse("{{ val | currency }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureEn), Is.EqualTo(expected));
            }
        }

        [TestCase(4.5, "4,50 €")]
        [TestCase(1024.5, "1.024,50 €")]
        [TestCase("1.024", "1.024,00 €")]
        public void TestCurrencyGreekCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureEn))
            {
                Assert.That(Template.Parse("{{ val | currency }}").Render(Hash.FromAnonymousObject(new { val = value }), _cultureGr), Is.EqualTo(expected));
            }
        }

        [TestCase(3.5, "4.5")]
        [TestCase(1023.5, "1024.5")]
        [TestCase("1,023", "1024")]
        public void TestPlusEnglishCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureGr))
            {
                var parameters = new RenderParameters(_cultureEn)
                {
                    LocalVariables = Hash.FromAnonymousObject(new { val = value }),
                    SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22,
                };
                Assert.That(Template.Parse("{{ 0 | plus: val | plus: 1 }}").Render(parameters), Is.EqualTo(expected));
            }
        }

        [TestCase(3.5, "4,5")]
        [TestCase(1023.5, "1024,5")]
        [TestCase("1.023", "1024")]
        public void TestPlusGreekCulture(object value, string expected)
        {
            using (CultureHelper.SetCulture(_cultureEn))
            {
                var parameters = new RenderParameters(_cultureGr)
                {
                    LocalVariables = Hash.FromAnonymousObject(new { val = value }),
                    SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22,
                };
                Assert.That(Template.Parse("{{ 0 | plus: val | plus: 1 }}").Render(parameters), Is.EqualTo(expected));
            }
        }
    }
}
