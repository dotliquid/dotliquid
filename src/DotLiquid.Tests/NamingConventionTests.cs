using System.Globalization;
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
            Assert.That(namingConvention.GetMemberName("Test"), Is.EqualTo("test"));
        }

        [Test]
        public void TestRubyComplexName()
        {
            RubyNamingConvention namingConvention = new RubyNamingConvention();
            Assert.That(namingConvention.GetMemberName("HelloWorld"), Is.EqualTo("hello_world"));
        }

        [Test]
        public void TestRubyMoreComplexName()
        {
            RubyNamingConvention namingConvention = new RubyNamingConvention();
            Assert.That(namingConvention.GetMemberName("HelloCruelWorld"), Is.EqualTo("hello_cruel_world"));
        }

        [Test]
        public void TestRubyFullUpperCase()
        {
            RubyNamingConvention namingConvention = new RubyNamingConvention();
            Assert.That(namingConvention.GetMemberName("ID"), Is.EqualTo("id"));
            Assert.That(namingConvention.GetMemberName("HELLOCRUELWORLD"), Is.EqualTo("hellocruelworld"));
        }

        [Test]
        public void TestRubyWithTurkishCulture()
        {
            using (CultureHelper.SetCulture("tr-TR"))
            {
                RubyNamingConvention namingConvention = new RubyNamingConvention();

                // in Turkish ID.ToLower() returns a localized i, and this fails
                Assert.That(namingConvention.GetMemberName("ID"), Is.EqualTo("id"));
            }
        }

        [Test]
        public void TestCSharpConventionDoesNothing()
        {
            CSharpNamingConvention namingConvention = new CSharpNamingConvention();
            Assert.That(namingConvention.GetMemberName("Test"), Is.EqualTo("Test"));
        }
    }
}
