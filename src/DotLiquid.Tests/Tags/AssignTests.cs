using System.Globalization;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class AssignTests
    {
        [Test]
        public async Task TestAssignedVariable()
        {
            await Helper.AssertTemplateResultAsync(".foo.", "{% assign foo = values %}.{{ foo[0] }}.",
                Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
            await Helper.AssertTemplateResultAsync(".bar.", "{% assign foo = values %}.{{ foo[1] }}.",
                Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
        }

        [Test]
        public async Task TestAssignDecimal()
        {
            await Helper.AssertTemplateResultAsync(string.Format("10{0}05", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                "{% assign foo = decimal %}{{ foo }}",
                Hash.FromAnonymousObject(new { @decimal = 10.05d }));
        }

        [Test]
        public async Task TestAssignDecimalInlineWithEnglishDecimalSeparator()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                await Helper.AssertTemplateResultAsync(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    "{% assign foo = 2.5 %}{{ foo }}");
            }
        }

        [Test]
        public async Task TestAssignDecimalInlineWithEnglishGroupSeparator()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                await Helper.AssertTemplateResultAsync("2500",
                    "{% assign foo = 2,500 %}{{ foo }}");
            }
        }

        [Test]
        public async Task TestAssignDecimalInlineWithFrenchDecimalSeparator()
        {
            using (CultureHelper.SetCulture("fr-FR"))
            {
                await Helper.AssertTemplateResultAsync(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    "{% assign foo = 2,5 %}{{ foo }}");
            }
        }

        [Test]
        public async Task TestAssignDecimalInlineWithInvariantDecimalSeparatorInFrenchCulture()
        {
            using (CultureHelper.SetCulture("fr-FR"))
            {
                await Helper.AssertTemplateResultAsync(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    "{% assign foo = 2.5 %}{{ foo }}");
            }
        }

        [Test]
        public async Task TestAssignWithFilter()
        {
            await Helper.AssertTemplateResultAsync(".bar.", "{% assign foo = values | split: ',' %}.{{ foo[1] }}.",
                Hash.FromAnonymousObject(new { values = "foo,bar,baz" }));
        }

        private class AssignDrop : Drop
        {
            public string MyProperty
            {
                get { return "MyValue"; }
            }
        }

        [Test]
        public async Task TestAssignWithDrop()
        {
            await Helper.AssertTemplateResultAsync(".MyValue.", @"{% assign foo = value %}.{{ foo.my_property }}.",
                Hash.FromAnonymousObject(new { value = new AssignDrop() }));
        }
    }
}
