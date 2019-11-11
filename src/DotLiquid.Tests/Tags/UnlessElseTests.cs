using NUnit.Framework;
using System.Threading.Tasks;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class UnlessElseTests
    {
        [Test]
        public async Task TestUnless()
        {
            await Helper.AssertTemplateResultAsync("  ", " {% unless true %} this text should not go into the output {% endunless %} ");
            await Helper.AssertTemplateResultAsync("  this text should go into the output  ", " {% unless false %} this text should go into the output {% endunless %} ");
            await Helper.AssertTemplateResultAsync("  you rock ?", "{% unless true %} you suck {% endunless %} {% unless false %} you rock {% endunless %}?");
        }

        [Test]
        public async Task TestUnlessElse()
        {
            await Helper.AssertTemplateResultAsync(" YES ", "{% unless true %} NO {% else %} YES {% endunless %}");
            await Helper.AssertTemplateResultAsync(" YES ", "{% unless false %} YES {% else %} NO {% endunless %}");
            await Helper.AssertTemplateResultAsync(" YES ", "{% unless 'foo' %} NO {% else %} YES {% endunless %}");
        }

        [Test]
        public async Task TestUnlessInLoop()
        {
            await Helper.AssertTemplateResultAsync("23", "{% for i in choices %}{% unless i %}{{ forloop.index }}{% endunless %}{% endfor %}",
                Hash.FromAnonymousObject(new { choices = new object[] { 1, null, false } }));
        }

        [Test]
        public async Task TestUnlessElseInLoop()
        {
            await Helper.AssertTemplateResultAsync(" TRUE  2  3 ", "{% for i in choices %}{% unless i %} {{ forloop.index }} {% else %} TRUE {% endunless %}{% endfor %}",
                Hash.FromAnonymousObject(new { choices = new object[] { 1, null, false } }));
        }
    }
}
