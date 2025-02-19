using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class StatementTests
    {
        [Test]
        public void TestTrueEqlTrue()
        {
            Assert.That(Template.Parse(" {% if true == true %} true {% else %} false {% endif %} ").Render(), Is.EqualTo("  true  "));
        }

        [Test]
        public void TestTrueNotEqlTrue()
        {
            Assert.That(Template.Parse(" {% if true != true %} true {% else %} false {% endif %} ").Render(), Is.EqualTo("  false  "));
        }

        [Test]
        public void TestTrueLqTrue()
        {
            Assert.That(Template.Parse(" {% if 0 > 0 %} true {% else %} false {% endif %} ").Render(), Is.EqualTo("  false  "));
        }

        [Test]
        public void TestOneLqZero()
        {
            Assert.That(Template.Parse(" {% if 1 > 0 %} true {% else %} false {% endif %} ").Render(), Is.EqualTo("  true  "));
        }

        [Test]
        public void TestZeroLqOne()
        {
            Assert.That(Template.Parse(" {% if 0 < 1 %} true {% else %} false {% endif %} ").Render(), Is.EqualTo("  true  "));
        }

        [Test]
        public void TestZeroLqOrEqualOne()
        {
            Assert.That(Template.Parse(" {% if 0 <= 0 %} true {% else %} false {% endif %} ").Render(), Is.EqualTo("  true  "));
        }

        [Test]
        public void TestZeroLqOrEqualOneInvolvingNil()
        {
            Assert.That(Template.Parse(" {% if null <= 0 %} true {% else %} false {% endif %} ").Render(), Is.EqualTo("  false  "));
            Assert.That(Template.Parse(" {% if 0 <= null %} true {% else %} false {% endif %} ").Render(), Is.EqualTo("  false  "));
        }

        [Test]
        public void TestZeroLqqOrEqualOne()
        {
            Assert.That(Template.Parse(" {% if 0 >= 0 %} true {% else %} false {% endif %} ").Render(), Is.EqualTo("  true  "));
        }

        [Test]
        public void TestStrings()
        {
            Assert.That(Template.Parse(" {% if 'test' == 'test' %} true {% else %} false {% endif %} ").Render(), Is.EqualTo("  true  "));
        }

        [Test]
        public void TestStringsNotEqual()
        {
            Assert.That(Template.Parse(" {% if 'test' != 'test' %} true {% else %} false {% endif %} ").Render(), Is.EqualTo("  false  "));
        }

        [Test]
        public void TestVarAndStringEqual()
        {
            Assert.That(Template.Parse(" {% if var == 'hello there!' %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = "hello there!" })), Is.EqualTo("  true  "));
        }

        [Test]
        public void TestVarAndStringAreEqualBackwards()
        {
            Assert.That(Template.Parse(" {% if 'hello there!' == var %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = "hello there!" })), Is.EqualTo("  true  "));
        }

        [Test]
        public void TestIsCollectionEmpty()
        {
            Assert.That(Template.Parse(" {% if array == empty %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { array = new object[] { } })), Is.EqualTo("  true  "));
        }

        [Test]
        public void TestIsNotCollectionEmpty()
        {
            Assert.That(Template.Parse(" {% if array == empty %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } })), Is.EqualTo("  false  "));
        }

        [Test]
        public void TestNil()
        {
            Assert.That(Template.Parse(" {% if var == nil %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = (object)null })), Is.EqualTo("  true  "));
            Assert.That(Template.Parse(" {% if var == null %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = (object)null })), Is.EqualTo("  true  "));
        }

        [Test]
        public void TestNotNil()
        {
            Assert.That(Template.Parse(" {% if var != nil %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = 1 })), Is.EqualTo("  true  "));
            Assert.That(Template.Parse(" {% if var != null %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = 1 })), Is.EqualTo("  true  "));
        }
    }
}
