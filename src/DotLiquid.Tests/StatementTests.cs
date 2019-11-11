using NUnit.Framework;
using System.Threading.Tasks;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class StatementTests
    {
        [Test]
        public async Task TestTrueEqlTrue()
        {
            Assert.AreEqual("  true  ", await Template.Parse(" {% if true == true %} true {% else %} false {% endif %} ").RenderAsync());
        }

        [Test]
        public async Task TestTrueNotEqlTrue()
        {
            Assert.AreEqual("  false  ", await Template.Parse(" {% if true != true %} true {% else %} false {% endif %} ").RenderAsync());
        }

        [Test]
        public async Task TestTrueLqTrue()
        {
            Assert.AreEqual("  false  ", await Template.Parse(" {% if 0 > 0 %} true {% else %} false {% endif %} ").RenderAsync());
        }

        [Test]
        public async Task TestOneLqZero()
        {
            Assert.AreEqual("  true  ", await Template.Parse(" {% if 1 > 0 %} true {% else %} false {% endif %} ").RenderAsync());
        }

        [Test]
        public async Task TestZeroLqOne()
        {
            Assert.AreEqual("  true  ", await Template.Parse(" {% if 0 < 1 %} true {% else %} false {% endif %} ").RenderAsync());
        }

        [Test]
        public async Task TestZeroLqOrEqualOne()
        {
            Assert.AreEqual("  true  ", await Template.Parse(" {% if 0 <= 0 %} true {% else %} false {% endif %} ").RenderAsync());
        }

        [Test]
        public async Task TestZeroLqOrEqualOneInvolvingNil()
        {
            Assert.AreEqual("  false  ", await Template.Parse(" {% if null <= 0 %} true {% else %} false {% endif %} ").RenderAsync());
            Assert.AreEqual("  false  ", await Template.Parse(" {% if 0 <= null %} true {% else %} false {% endif %} ").RenderAsync());
        }

        [Test]
        public async Task TestZeroLqqOrEqualOne()
        {
            Assert.AreEqual("  true  ", await Template.Parse(" {% if 0 >= 0 %} true {% else %} false {% endif %} ").RenderAsync());
        }

        [Test]
        public async Task TestStrings()
        {
            Assert.AreEqual("  true  ", await Template.Parse(" {% if 'test' == 'test' %} true {% else %} false {% endif %} ").RenderAsync());
        }

        [Test]
        public async Task TestStringsNotEqual()
        {
            Assert.AreEqual("  false  ", await Template.Parse(" {% if 'test' != 'test' %} true {% else %} false {% endif %} ").RenderAsync());
        }

        [Test]
        public async Task TestVarAndStringEqual()
        {
            Assert.AreEqual("  true  ", await Template.Parse(" {% if var == 'hello there!' %} true {% else %} false {% endif %} ").RenderAsync(Hash.FromAnonymousObject(new { var = "hello there!" })));
        }

        [Test]
        public async Task TestVarAndStringAreEqualBackwards()
        {
            Assert.AreEqual("  true  ", await Template.Parse(" {% if 'hello there!' == var %} true {% else %} false {% endif %} ").RenderAsync(Hash.FromAnonymousObject(new { var = "hello there!" })));
        }

        [Test]
        public async Task TestIsCollectionEmpty()
        {
            Assert.AreEqual("  true  ", await Template.Parse(" {% if array == empty %} true {% else %} false {% endif %} ").RenderAsync(Hash.FromAnonymousObject(new { array = new object[] { } })));
        }

        [Test]
        public async Task TestIsNotCollectionEmpty()
        {
            Assert.AreEqual("  false  ", await Template.Parse(" {% if array == empty %} true {% else %} false {% endif %} ").RenderAsync(Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } })));
        }

        [Test]
        public async Task TestNil()
        {
            Assert.AreEqual("  true  ", await Template.Parse(" {% if var == nil %} true {% else %} false {% endif %} ").RenderAsync(Hash.FromAnonymousObject(new { var = (object) null })));
            Assert.AreEqual("  true  ", await Template.Parse(" {% if var == null %} true {% else %} false {% endif %} ").RenderAsync(Hash.FromAnonymousObject(new { var = (object) null })));
        }

        [Test]
        public async Task TestNotNil()
        {
            Assert.AreEqual("  true  ", await Template.Parse(" {% if var != nil %} true {% else %} false {% endif %} ").RenderAsync(Hash.FromAnonymousObject(new { var = 1 })));
            Assert.AreEqual("  true  ", await Template.Parse(" {% if var != null %} true {% else %} false {% endif %} ").RenderAsync(Hash.FromAnonymousObject(new { var = 1 })));
        }
    }
}
