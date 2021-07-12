using DotLiquid.Exceptions;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class RawTests
    {
        [Test]
        public void TestTagInRaw()
        {
            Helper.AssertTemplateResult("{% comment %} test {% endcomment %}",
                "{% raw %}{% comment %} test {% endcomment %}{% endraw %}");
        }

        [Test]
        public void TestOutputInRaw()
        {
            Helper.AssertTemplateResult("{{ test }}",
                "{% raw %}{{ test }}{% endraw %}");
        }

        [Test]
        public void TestRawWithErbLikeTrimmingWhitespace()
        {
            Helper.AssertTemplateResult("{{ test }}", "{%- raw %}{{ test }}{%- endraw %}");
            Helper.AssertTemplateResult("{{ test }}", "{% raw -%}{{ test }}{% endraw -%}");
            Helper.AssertTemplateResult("{{ test }}", "{%- raw -%}{{ test }}{%- endraw -%}");
            Helper.AssertTemplateResult("{{ test }}", "{%-raw-%}{{ test }}{%-endraw-%}");
        }

        [Test]
        public void TestPartialInRaw()
        {
            Helper.AssertTemplateResult(" Foobar {% invalid ", "{% raw %} Foobar {% invalid {% endraw %}");
            Helper.AssertTemplateResult(" Foobar invalid %} ", "{% raw %} Foobar invalid %} {% endraw %}");
            Helper.AssertTemplateResult(" Foobar {{ invalid ", "{% raw %} Foobar {{ invalid {% endraw %}");
            Helper.AssertTemplateResult(" Foobar invalid }} ", "{% raw %} Foobar invalid }} {% endraw %}");
            Helper.AssertTemplateResult(" Foobar {% invalid {% {% endraw ", "{% raw %} Foobar {% invalid {% {% endraw {% endraw %}");
            Helper.AssertTemplateResult(" Foobar {% {% {% ", "{% raw %} Foobar {% {% {% {% endraw %}");
            Helper.AssertTemplateResult(" test {% raw %} {% endraw %}", "{% raw %} test {% raw %} {% {% endraw %}endraw %}");
            Helper.AssertTemplateResult(" Foobar {{ invalid 1", "{% raw %} Foobar {{ invalid {% endraw %}{{ 1 }}");
            Helper.AssertTemplateResult(" Foobar {% foo {% bar %}", "{% raw %} Foobar {% foo {% bar %}{% endraw %}");
        }

        [Test]
        public void TestInvalidRaw()
        {
            Assert.Throws<SyntaxException>(() => Template.Parse("{% raw %} foo"));
            Assert.Throws<SyntaxException>(() => Template.Parse("{% raw } foo {% endraw %}"));
            Assert.Throws<SyntaxException>(() => Template.Parse("{% raw } foo %}{% endraw %}"));
        }
    }
}
