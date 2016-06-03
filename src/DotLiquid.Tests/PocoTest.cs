using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class PocoTest
    {
        [SetUp]
        public void SetUp()
        {
            Template.NamingConvention = new CSharpNamingConvention();
        }

        [Test]
        public void TestUnrestrictedPoco()
        {
            var template = "{{C.FirstName}}";
            var t = Template.Parse(template);

            var dic = new Dictionary<string, object>();
            dic.Add("C", new Customer());
            var hash = Hash.FromDictionary(dic);

            var result = t.Render(hash, false);

            Assert.AreEqual("Max", result);
        }

        [TearDown]
        public void TearDown()
        {
            Template.NamingConvention = new RubyNamingConvention();
        }
    }

    public class Customer
    {
        public string FirstName { get; set; } = "Max";
    }
}
