using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class NamingConventionTests
	{
		[Test]
		public void TestRubySimpleName()
		{
			RubyNamingConvention namingConvention = new RubyNamingConvention();
			Assert.AreEqual("test", namingConvention.GetMemberName("Test"));
		}

		[Test]
		public void TestRubyComplexName()
		{
			RubyNamingConvention namingConvention = new RubyNamingConvention();
			Assert.AreEqual("hello_world", namingConvention.GetMemberName("HelloWorld"));
		}

		[Test]
		public void TestRubyMoreComplexName()
		{
			RubyNamingConvention namingConvention = new RubyNamingConvention();
			Assert.AreEqual("hello_cruel_world", namingConvention.GetMemberName("HelloCruelWorld"));
		}

		[Test]
		public void TestRubyFullUpperCase()
		{
			RubyNamingConvention namingConvention = new RubyNamingConvention();
			Assert.AreEqual("id", namingConvention.GetMemberName("ID"));
			Assert.AreEqual("hellocruelworld", namingConvention.GetMemberName("HELLOCRUELWORLD"));
		}

		[Test]
		public void TestCSharpConventionDoesNothing()
		{
			CSharpNamingConvention namingConvention = new CSharpNamingConvention();
			Assert.AreEqual("Test", namingConvention.GetMemberName("Test"));
		}
	}
}