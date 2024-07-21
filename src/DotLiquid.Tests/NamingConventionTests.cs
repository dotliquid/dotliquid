using DotLiquid.NamingConventions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class NamingConventionTests
    {
        [Test]
        public void TestRubySimpleName()
        {
            RubyNamingConvention namingConvention = new RubyNamingConvention();
           ClassicAssert.AreEqual("test", namingConvention.GetMemberName("Test"));
        }

        [Test]
        public void TestRubyComplexName()
        {
            RubyNamingConvention namingConvention = new RubyNamingConvention();
           ClassicAssert.AreEqual("hello_world", namingConvention.GetMemberName("HelloWorld"));
        }

        [Test]
        public void TestRubyMoreComplexName()
        {
            RubyNamingConvention namingConvention = new RubyNamingConvention();
           ClassicAssert.AreEqual("hello_cruel_world", namingConvention.GetMemberName("HelloCruelWorld"));
        }

        [Test]
        public void TestRubyFullUpperCase()
        {
            RubyNamingConvention namingConvention = new RubyNamingConvention();
           ClassicAssert.AreEqual("id", namingConvention.GetMemberName("ID"));
           ClassicAssert.AreEqual("hellocruelworld", namingConvention.GetMemberName("HELLOCRUELWORLD"));
        }

        [Test]
        public void TestRubyWithTurkishCulture()
        {
            using (CultureHelper.SetCulture("tr-TR"))
            {
                RubyNamingConvention namingConvention = new RubyNamingConvention();

                // in Turkish ID.ToLower() returns a localized i, and this fails
               ClassicAssert.AreEqual("id", namingConvention.GetMemberName("ID"));
            }
        }

        [Test]
        public void TestCSharpConventionDoesNothing()
        {
            CSharpNamingConvention namingConvention = new CSharpNamingConvention();
           ClassicAssert.AreEqual("Test", namingConvention.GetMemberName("Test"));
        }
    }
}
