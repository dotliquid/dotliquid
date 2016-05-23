using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class PocoTest
    {
        [Test]
        public void TestUnrestrictedPoco()
        {
            var template = "{{TestObject.FirstName}}";
            var t = Template.Parse(template);
            var hash = new Hash(new TestObject());
            var result = t.Render(hash);

            Assert.AreEqual("Max", result);
        }
    }

    public class TestObject
    {
        public string FirstName { get; set; } = "Max";
    }
}
