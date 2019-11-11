using System;
using System.Collections.Generic;
using DotLiquid.Exceptions;
using NUnit.Framework;
using DotLiquid.Tags;
using System.Threading.Tasks;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class LiteralTests
    {
        [Test]
        public async Task TestEmptyLiteral()
        {
            Template t = Template.Parse("{% literal %}{% endliteral %}");
            Assert.AreEqual(string.Empty, await t.RenderAsync());
            t = Template.Parse("{{{}}}");
            Assert.AreEqual(string.Empty, await t.RenderAsync());
        }

        [Test]
        public async Task TestSimpleLiteralValue()
        {
            Template t = Template.Parse("{% literal %}howdy{% endliteral %}");
            Assert.AreEqual("howdy", await t.RenderAsync());
        }

        [Test]
        public async Task TestLiteralsIgnoreLiquidMarkup()
        {
            Template t = Template.Parse("{% literal %}{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}{% endliteral %}");
            Assert.AreEqual("{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}", await t.RenderAsync());
        }

        [Test]
        public async Task TestShorthandSyntax()
        {
            Template t = Template.Parse("{{{{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}}}}");
            Assert.AreEqual("{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}", await t.RenderAsync());
        }

        [Test]
        public async Task TestLiteralsDontRemoveComments()
        {
            Template t = Template.Parse("{{{ {# comment #} }}}");
            Assert.AreEqual("{# comment #}", await t.RenderAsync());
        }

        [Test]
        public void TestFromShorthand()
        {
            Assert.AreEqual("{% literal %}gnomeslab{% endliteral %}", Literal.FromShortHand("{{{gnomeslab}}}"));
        }

        [Test]
        public void TestFromShorthandIgnoresImproperSyntax()
        {
            Assert.AreEqual("{% if 'hi' == 'hi' %}hi{% endif %}", Literal.FromShortHand("{% if 'hi' == 'hi' %}hi{% endif %}"));
        }
    }
}
