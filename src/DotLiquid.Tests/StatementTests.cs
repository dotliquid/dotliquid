using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class StatementTests
	{
		[Test]
		public void TestTrueEqlTrue()
		{
			Assert.AreEqual("  true  ", Template.Parse(" {% if true == true %} true {% else %} false {% endif %} ").Render());
		}

		[Test]
		public void TestTrueNotEqlTrue()
		{
			Assert.AreEqual("  false  ", Template.Parse(" {% if true != true %} true {% else %} false {% endif %} ").Render());
		}

		[Test]
		public void TestTrueLqTrue()
		{
			Assert.AreEqual("  false  ", Template.Parse(" {% if 0 > 0 %} true {% else %} false {% endif %} ").Render());
		}

		[Test]
		public void TestOneLqZero()
		{
			Assert.AreEqual("  true  ", Template.Parse(" {% if 1 > 0 %} true {% else %} false {% endif %} ").Render());
		}

		[Test]
		public void TestZeroLqOne()
		{
			Assert.AreEqual("  true  ", Template.Parse(" {% if 0 < 1 %} true {% else %} false {% endif %} ").Render());
		}

		[Test]
		public void TestZeroLqOrEqualOne()
		{
			Assert.AreEqual("  true  ", Template.Parse(" {% if 0 <= 0 %} true {% else %} false {% endif %} ").Render());
		}

		[Test]
		public void TestZeroLqOrEqualOneInvolvingNil()
		{
			Assert.AreEqual("  false  ", Template.Parse(" {% if null <= 0 %} true {% else %} false {% endif %} ").Render());
			Assert.AreEqual("  false  ", Template.Parse(" {% if 0 <= null %} true {% else %} false {% endif %} ").Render());
		}

		[Test]
		public void TestZeroLqqOrEqualOne()
		{
			Assert.AreEqual("  true  ", Template.Parse(" {% if 0 >= 0 %} true {% else %} false {% endif %} ").Render());
		}

		[Test]
		public void TestStrings()
		{
			Assert.AreEqual("  true  ", Template.Parse(" {% if 'test' == 'test' %} true {% else %} false {% endif %} ").Render());
		}

		[Test]
		public void TestStringsNotEqual()
		{
			Assert.AreEqual("  false  ", Template.Parse(" {% if 'test' != 'test' %} true {% else %} false {% endif %} ").Render());
		}

		[Test]
		public void TestVarAndStringEqual()
		{
			Assert.AreEqual("  true  ", Template.Parse(" {% if var == 'hello there!' %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = "hello there!" })));
		}

		[Test]
		public void TestVarAndStringAreEqualBackwards()
		{
			Assert.AreEqual("  true  ", Template.Parse(" {% if 'hello there!' == var %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = "hello there!" })));
		}

		[Test]
		public void TestIsCollectionEmpty()
		{
			Assert.AreEqual("  true  ", Template.Parse(" {% if array == empty %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { array = new object[] { } })));
		}

		[Test]
		public void TestIsNotCollectionEmpty()
		{
			Assert.AreEqual("  false  ", Template.Parse(" {% if array == empty %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } })));
		}

		[Test]
		public void TestNil()
		{
			Assert.AreEqual("  true  ", Template.Parse(" {% if var == nil %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = (object) null })));
			Assert.AreEqual("  true  ", Template.Parse(" {% if var == null %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = (object) null })));
		}

		[Test]
		public void TestNotNil()
		{
			Assert.AreEqual("  true  ", Template.Parse(" {% if var != nil %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = 1 })));
			Assert.AreEqual("  true  ", Template.Parse(" {% if var != null %} true {% else %} false {% endif %} ").Render(Hash.FromAnonymousObject(new { var = 1 })));
		}
	}
}