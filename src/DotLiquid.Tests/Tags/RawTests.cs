using System;
using System.Collections.Generic;
using DotLiquid.Exceptions;
using NUnit.Framework;
using DotLiquid.Tags;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class RawTests
    {
        [Test]
        public void TestTagInRow()
        {
			Template t = Template.Parse("{% raw %}{% comment %} test {% endcomment %}{% endraw %}");
            Assert.AreEqual("{% comment %} test {% endcomment %}", t.Render());
        }

		[Test]
		public void TestOutputInRow()
		{
			Template t = Template.Parse("{% raw %}{{ test }}{% endraw %}");
			Assert.AreEqual("{{ test }}", t.Render());
		}
	}
}