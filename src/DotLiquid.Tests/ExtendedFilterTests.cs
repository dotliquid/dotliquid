using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Globalization;

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
            ClassicAssert.AreEqual(null, ExtendedFilters.Titleize(context: context, input: null));
            ClassicAssert.AreEqual("", ExtendedFilters.Titleize(context: context, input: ""));
            ClassicAssert.AreEqual(" ", ExtendedFilters.Titleize(context: context, input: " "));
            ClassicAssert.AreEqual("That Is One Sentence.", ExtendedFilters.Titleize(context: context, input: "That is one sentence."));

            Helper.AssertTemplateResult(
                expected: "Title",
                template: "{{ 'title' | titleize }}");
        }

        [Test]
        public void TestUpcaseFirst()
        {
            var context = _context;
            ClassicAssert.AreEqual(null, ExtendedFilters.UpcaseFirst(context: context, input: null));
            ClassicAssert.AreEqual("", ExtendedFilters.UpcaseFirst(context: context, input: ""));
            ClassicAssert.AreEqual(" ", ExtendedFilters.UpcaseFirst(context: context, input: " "));
            ClassicAssert.AreEqual(" My boss is Mr. Doe.", ExtendedFilters.UpcaseFirst(context: context, input: " my boss is Mr. Doe."));

            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{{ 'my great title' | upcase_first }}");
        }

        [Test]
        public void TestRegexReplace()
        {
            ClassicAssert.AreEqual(expected: "b b b b", actual: ExtendedFilters.RegexReplace(input: "a A A a", pattern: "[Aa]", replacement: "b"));
        }
    }
}
