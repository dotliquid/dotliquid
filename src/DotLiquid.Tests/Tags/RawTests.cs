using NUnit.Framework;
using System.Threading.Tasks;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class RawTests
    {
        [Test]
        public async Task TestTagInRaw ()
        {
            await Helper.AssertTemplateResultAsync ("{% comment %} test {% endcomment %}",
                "{% raw %}{% comment %} test {% endcomment %}{% endraw %}");
        }

        [Test]
        public async Task TestOutputInRaw ()
        {
            await Helper.AssertTemplateResultAsync ("{{ test }}",
                "{% raw %}{{ test }}{% endraw %}");
        }
    }
}
