using DotLiquid.NamingConventions;
using DotLiquid.Tags;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class CommentTests
    {
        private INamingConvention NamingConvention { get; } = new RubyNamingConvention();

        [Test]
        public void TestEmptyComment()
        {
            Assert.AreEqual(string.Empty, Template.Parse("{% comment %}{% endcomment %}", NamingConvention).Render());

            // Next test is specific to legacy parser and was removed from Ruby Liquid. Test that it is ignored is in TestShortHandSyntaxIsIgnored
            Assert.AreEqual(string.Empty, Template.Parse("{##}",NamingConvention, SyntaxCompatibility.DotLiquid20).Render());
        }

        [Test]
        public void TestSimpleCommentValue()
        {
            Assert.AreEqual("", Template.Parse("{% comment %}howdy{% endcomment %}", NamingConvention).Render());
        }

        [Test]
        public void TestCommentsIgnoreLiquidMarkup()
        {
            Assert.AreEqual(
                expected: "",
                actual: Template.Parse("{% comment %}{% if 'gnomeslab' contains 'liquid' %}yes{% else %}no{ % endif %}{% endcomment %}", NamingConvention).Render());
        }

        [Test]
        public void TestCommentShorthand()
        {
            Assert.AreEqual("{% comment %}gnomeslab{% endcomment %}", Comment.FromShortHand("{# gnomeslab #}"));
            Assert.AreEqual(null, Comment.FromShortHand(null));

            Assert.AreEqual(
                expected: "",
                actual: Template.Parse("{#{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}#}", NamingConvention, SyntaxCompatibility.DotLiquid20).Render());
        }
    }
}
