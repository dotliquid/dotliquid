using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class StatementTests
    {
        [Test]
        public void TestTrueEqlTrue()
        {
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if true == true %} true {% else %} false {% endif %} ").Render());
        }

        [Test]
        public void TestTrueNotEqlTrue()
        {
           ClassicAssert.AreEqual("  false  ", Template.Parse(" {% if true != true %} true {% else %} false {% endif %} ").Render());
        }

        [Test]
        public void TestTrueLqTrue()
        {
           ClassicAssert.AreEqual("  false  ", Template.Parse(" {% if 0 > 0 %} true {% else %} false {% endif %} ").Render());
        }

        [Test]
        public void TestOneLqZero()
        {
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if 1 > 0 %} true {% else %} false {% endif %} ").Render());
        }

        [Test]
        public void TestZeroLqOne()
        {
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if 0 < 1 %} true {% else %} false {% endif %} ").Render());
        }

        [Test]
        public void TestZeroLqOrEqualOne()
        {
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if 0 <= 0 %} true {% else %} false {% endif %} ").Render());
        }

        [Test]
        public void TestZeroLqOrEqualOneInvolvingNil()
        {
           ClassicAssert.AreEqual("  false  ", Template.Parse(" {% if null <= 0 %} true {% else %} false {% endif %} ").Render());
           ClassicAssert.AreEqual("  false  ", Template.Parse(" {% if 0 <= null %} true {% else %} false {% endif %} ").Render());
        }

        [Test]
        public void TestZeroLqqOrEqualOne()
        {
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if 0 >= 0 %} true {% else %} false {% endif %} ").Render());
        }

        [Test]
        public void TestStrings()
        {
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if 'test' == 'test' %} true {% else %} false {% endif %} ").Render());
        }

        [Test]
        public void TestStringsNotEqual()
        {
           ClassicAssert.AreEqual("  false  ", Template.Parse(" {% if 'test' != 'test' %} true {% else %} false {% endif %} ").Render());
        }

        [Test]
        public void TestVarAndStringEqual()
        {
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if var == 'hello there!' %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = "hello there!" })));
        }

        [Test]
        public void TestVarAndStringAreEqualBackwards()
        {
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if 'hello there!' == var %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = "hello there!" })));
        }

        [Test]
        public void TestIsCollectionEmpty()
        {
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if array == empty %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { array = new object[] { } })));
        }

        [Test]
        public void TestIsNotCollectionEmpty()
        {
           ClassicAssert.AreEqual("  false  ", Template.Parse(" {% if array == empty %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } })));
        }

        [Test]
        public void TestNil()
        {
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if var == nil %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = (object) null })));
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if var == null %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = (object) null })));
        }

        [Test]
        public void TestNotNil()
        {
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if var != nil %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = 1 })));
           ClassicAssert.AreEqual("  true  ", Template.Parse(" {% if var != null %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = 1 })));
        }
    }
}
