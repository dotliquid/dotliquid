using DotLiquid.Exceptions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ParsingQuirksTests
    {
        [Test]
        public async Task TestErrorWithCss()
        {
            const string text = " div { font-weight: bold; } ";
            Template template = Template.Parse(text);
            Assert.AreEqual(text, await template.RenderAsync());
            Assert.AreEqual(1, template.Root.NodeList.Count);
            Assert.IsInstanceOf<string>(template.Root.NodeList[0]);
        }

        [Test]
        public void TestRaiseOnSingleCloseBrace()
        {
            Assert.Throws<SyntaxException>(() => Template.Parse("text {{method} oh nos!"));
        }

        [Test]
        public void TestRaiseOnLabelAndNoCloseBrace()
        {
            Assert.Throws<SyntaxException>(() => Template.Parse("TEST {{ "));
        }

        [Test]
        public void TestRaiseOnLabelAndNoCloseBracePercent()
        {
            Assert.Throws<SyntaxException>(() => Template.Parse("TEST {% "));
        }

        [Test]
        public void TestErrorOnEmptyFilter()
        {
            Assert.DoesNotThrow(() =>
            {
                Template.Parse("{{test |a|b|}}");
                Template.Parse("{{test}}");
                Template.Parse("{{|test|}}");
            });
        }

        [Test]
        public async Task TestMeaninglessParens()
        {
            Hash assigns = Hash.FromAnonymousObject(new { b = "bar", c = "baz" });
            await Helper.AssertTemplateResultAsync(" YES ", "{% if a == 'foo' or (b == 'bar' and c == 'baz') or false %} YES {% endif %}", assigns);
        }

        [Test]
        public async Task TestUnexpectedCharactersSilentlyEatLogic()
        {
            await Helper.AssertTemplateResultAsync(" YES ", "{% if true && false %} YES {% endif %}");
            await Helper.AssertTemplateResultAsync("", "{% if false || true %} YES {% endif %}");
        }
    }
}
