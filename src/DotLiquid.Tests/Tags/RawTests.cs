<<<<<<< HEAD
﻿using System;
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
=======
﻿using NUnit.Framework;

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
	}
>>>>>>> cf6181022b76a9ba0fc8ff6b4a7356ba2ac6570d
}