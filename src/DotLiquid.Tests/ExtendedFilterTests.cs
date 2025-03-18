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
            Assert.That(ExtendedFilters.UpcaseFirst(input: null), Is.EqualTo(null));
            Assert.That(ExtendedFilters.UpcaseFirst(input: ""), Is.EqualTo(""));
            Assert.That(ExtendedFilters.UpcaseFirst(input: " "), Is.EqualTo(" "));
            Assert.That(ExtendedFilters.UpcaseFirst(input: " my boss is Mr. Doe."), Is.EqualTo(" My boss is Mr. Doe."));

            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{{ 'my great title' | upcase_first }}");
        }

        [Test]
        public void TestRegexReplace()
        {
            Assert.That(actual: ExtendedFilters.RegexReplace(input: "a A A a", pattern: "[Aa]", replacement: "b"), Is.EqualTo(expected: "b b b b"));
        }

        [Test]
        public void TestRubySplit()
        {
            Assert.That(ExtendedFilters.RubySplit("This is a sentence", " "), Is.EqualTo(new[] { "This", "is", "a", "sentence" }).AsCollection);

            // A string with no pattern should be split into a string[], as required for the Liquid Reverse filter
            Assert.That(ExtendedFilters.RubySplit("YMCA", null), Is.EqualTo(new[] { "Y", "M", "C", "A" }).AsCollection);
            Assert.That(ExtendedFilters.RubySplit("YMCA", ""), Is.EqualTo(new[] { "Y", "M", "C", "A" }).AsCollection);
            Assert.That(ExtendedFilters.RubySplit(" ", ""), Is.EqualTo(new[] { " " }).AsCollection);
        }

        [Test]
        public void TestRubySplitWhitespace()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ExtendedFilters.RubySplit("    one    two three    four  ", " "), Is.EqualTo(new[] { "one", "two", "three", "four" }).AsCollection);
                Assert.That(ExtendedFilters.RubySplit("one  two\tthree\nfour", " "), Is.EqualTo(new[] { "one", "two", "three", "four" }).AsCollection);
                Assert.That(ExtendedFilters.RubySplit("one  two\tthree\nfour", "\n"), Is.EqualTo(new[] { "one  two\tthree", "four" }).AsCollection);

                Assert.That(ExtendedFilters.RubySplit("abracadabra", "ab"), Is.EqualTo(new[] { "", "racad", "ra" }).AsCollection);
                Assert.That(ExtendedFilters.RubySplit("aaabcdaaa", "a"), Is.EqualTo(new[] { "", "", "", "bcd" }).AsCollection);
                Assert.That(ExtendedFilters.RubySplit("", "a"), Has.Exactly(0).Items);
            });
        }
    }
}
