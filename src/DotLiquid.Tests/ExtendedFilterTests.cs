using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ExtendedFilterTests
    {
        private Context _context;

        [OneTimeSetUp]
        public void SetUp()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            Template.RegisterFilter(typeof(ExtendedFilters));
        }

        [Test]
        public void TestTitleize()
        {
            var context = _context;
            Assert.That(ExtendedFilters.Titleize(context: context, input: null), Is.EqualTo(null));
            Assert.That(ExtendedFilters.Titleize(context: context, input: ""), Is.EqualTo(""));
            Assert.That(ExtendedFilters.Titleize(context: context, input: " "), Is.EqualTo(" "));
            Assert.That(ExtendedFilters.Titleize(context: context, input: "That is one sentence."), Is.EqualTo("That Is One Sentence."));

            Helper.AssertTemplateResult(
                expected: "Title",
                template: "{{ 'title' | titleize }}");
        }

        [Test]
        public void TestUpcaseFirst()
        {
            var context = _context;
            Assert.That(ExtendedFilters.UpcaseFirst(context: context, input: null), Is.EqualTo(null));
            Assert.That(ExtendedFilters.UpcaseFirst(context: context, input: ""), Is.EqualTo(""));
            Assert.That(ExtendedFilters.UpcaseFirst(context: context, input: " "), Is.EqualTo(" "));
            Assert.That(ExtendedFilters.UpcaseFirst(context: context, input: " my boss is Mr. Doe."), Is.EqualTo(" My boss is Mr. Doe."));

            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{{ 'my great title' | upcase_first }}");
        }

        [Test]
        public void TestRegexReplace()
        {
            Assert.That(actual: ExtendedFilters.RegexReplace(input: "a A A a", pattern: "[Aa]", replacement: "b"), Is.EqualTo(expected: "b b b b"));
        }
    }
}
