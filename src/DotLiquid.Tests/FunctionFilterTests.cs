using System;
using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    public class FunctionFilterTests
    {
        private Context _context;

        [SetUp]
        public void Setup()
        {
            _context = new Context();
        }

        [Test]
        public void AddingFunctions()
        {
            _context["var"] = 2;
            _context.AddFilter<int, string>("AddTwo", i => (i + 2).ToString(CultureInfo.InvariantCulture));
            Assert.That(new Variable("var | add_two").Render(_context), Is.EqualTo("4"));
        }

        [Test]
        public void AddingMethodInfo()
        {
            _context["var"] = 2;
            _context.AddFilter<int, string>("AddTwo", i => (i + 2).ToString(CultureInfo.InvariantCulture));
            Assert.That(new Variable("var | add_two").Render(_context), Is.EqualTo("4"));
        }
    }
}