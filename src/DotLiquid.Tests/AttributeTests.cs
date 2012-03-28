using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    class AttributeTests
    {

        [LiquidType]
        public class MyLiquidType
        {
            public string Name { get; set; }
        }

        [Test]
        public void TestLiquidTypeAttribute()
        {
            Template template = Template.Parse("{{context.Name}}");

            var output = template.Render(Hash.FromAnonymousObject(new { context = new MyLiquidType() { Name = "worked" } }));

            Assert.AreEqual("worked", output);
        }
    }
}
