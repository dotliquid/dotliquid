using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class VariableTests
    {
        [Test]
        public void TestVariable()
        {
            Variable var = new Variable("hello");
            Assert.That(var.Name, Is.EqualTo("hello"));
        }

        [Test]
        public void TestFilters()
        {
            Variable var = new Variable("hello | textileze");
            Assert.That(var.Name, Is.EqualTo("hello"));
            Assert.That(var.Filters.Count, Is.EqualTo(1));
            Assert.That(var.Filters[0].Name, Is.EqualTo("textileze"));
            Assert.That(var.Filters[0].Arguments.Length, Is.EqualTo(0));

            var = new Variable("hello | textileze | paragraph");
            Assert.That(var.Name, Is.EqualTo("hello"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("textileze", new string[] { }), new Variable.Filter("paragraph", new string[] { }) }, var.Filters);

            var = new Variable(" hello | strftime: '%Y'");
            Assert.That(var.Name, Is.EqualTo("hello"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("strftime", new[] { "'%Y'" }) }, var.Filters);

            var = new Variable(" 'typo' | link_to: 'Typo', true ");
            Assert.That(var.Name, Is.EqualTo("'typo'"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("link_to", new[] { "'Typo'", "true" }) }, var.Filters);

            var = new Variable(" 'typo' | link_to: 'Typo', false ");
            Assert.That(var.Name, Is.EqualTo("'typo'"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("link_to", new[] { "'Typo'", "false" }) }, var.Filters);

            var = new Variable(" 'foo' | repeat: 3 ");
            Assert.That(var.Name, Is.EqualTo("'foo'"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("repeat", new[] { "3" }) }, var.Filters);

            var = new Variable(" 'foo' | repeat: 3, 3 ");
            Assert.That(var.Name, Is.EqualTo("'foo'"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("repeat", new[] { "3", "3" }) }, var.Filters);

            var = new Variable(" 'foo' | repeat: 3, 3, 3 ");
            Assert.That(var.Name, Is.EqualTo("'foo'"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("repeat", new[] { "3", "3", "3" }) }, var.Filters);

            var = new Variable(" hello | strftime: '%Y, okay?'");
            Assert.That(var.Name, Is.EqualTo("hello"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("strftime", new[] { "'%Y, okay?'" }) }, var.Filters);

            var = new Variable(" hello | things: \"%Y, okay?\", 'the other one'");
            Assert.That(var.Name, Is.EqualTo("hello"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("things", new[] { "\"%Y, okay?\"", "'the other one'" }) }, var.Filters);
        }

        [Test]
        public void TestFilterWithDateParameter()
        {
            Variable var = new Variable(" '2006-06-06' | date: \"%m/%d/%Y\"");
            Assert.That(var.Name, Is.EqualTo("'2006-06-06'"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("date", new[] { "\"%m/%d/%Y\"" }) }, var.Filters);
        }

        [Test]
        public void TestFiltersWithoutWhitespace()
        {
            Variable var = new Variable("hello | textileze | paragraph");
            Assert.That(var.Name, Is.EqualTo("hello"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("textileze", new string[] { }), new Variable.Filter("paragraph", new string[] { }) }, var.Filters);

            var = new Variable("hello|textileze|paragraph");
            Assert.That(var.Name, Is.EqualTo("hello"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("textileze", new string[] { }), new Variable.Filter("paragraph", new string[] { }) }, var.Filters);
        }

        [Test]
        public void TestSymbol()
        {
            Variable var = new Variable("http://disney.com/logo.gif | image: 'med' ");
            Assert.That(var.Name, Is.EqualTo("http://disney.com/logo.gif"));
            AssertFiltersAreEqual(new[] { new Variable.Filter("image", new[] { "'med'" }) }, var.Filters);
        }

        [Test]
        public void TestStringSingleQuoted()
        {
            Variable var = new Variable(" 'hello' ");
            Assert.That(var.Name, Is.EqualTo("'hello'"));
        }

        [Test]
        public void TestStringDoubleQuoted()
        {
            Variable var = new Variable(" \"hello\" ");
            Assert.That(var.Name, Is.EqualTo("\"hello\""));
        }

        [Test]
        public void TestInteger()
        {
            Variable var = new Variable(" 1000 ");
            Assert.That(var.Name, Is.EqualTo("1000"));
        }

        [Test]
        public void TestFloat()
        {
            Variable var = new Variable(" 1000.01 ");
            Assert.That(var.Name, Is.EqualTo("1000.01"));
        }

        [Test]
        public void TestStringWithSpecialChars()
        {
            Variable var = new Variable(" 'hello! $!@.;\"ddasd\" ' ");
            Assert.That(var.Name, Is.EqualTo("'hello! $!@.;\"ddasd\" '"));
        }

        [Test]
        public void TestStringDot()
        {
            Variable var = new Variable(" test.test ");
            Assert.That(var.Name, Is.EqualTo("test.test"));
        }

        private static void AssertFiltersAreEqual(Variable.Filter[] expected, System.Collections.Generic.List<Variable.Filter> actual)
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.That(actual[i].Name, Is.EqualTo(expected[i].Name));
                Assert.That(actual[i].Arguments.Length, Is.EqualTo(expected[i].Arguments.Length));
                for (int j = 0; j < expected[i].Arguments.Length; ++j)
                    Assert.That(actual[i].Arguments[j], Is.EqualTo(expected[i].Arguments[j]));
            }
        }
    }
}
