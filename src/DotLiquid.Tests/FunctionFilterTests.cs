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
            _context = new Context(CultureInfo.InvariantCulture);
        }

        [Test]
        public void AddingFunctions()
        {
            _context["var"] = 2;
            _context.AddFilter<int, string>("AddTwo", i => (i + 2).ToString(CultureInfo.InvariantCulture));
            Assert.That(new Variable("var | add_two").Render(_context), Is.EqualTo("4"));
        }

        [Test]
        public void AddingAnonimousFunctionWithClosure()
        {
            _context["var"] = 2;
            int x = 2;

            // (x=(i + x)) is to forbid JITC to inline x and force it to create non-static closure

            _context.AddFilter<int, string>("AddTwo", i => (x=(i + x)).ToString(CultureInfo.InvariantCulture));
            Assert.That(new Variable("var | add_two").Render(_context), Is.EqualTo("4"));

            //this is done, to forbid JITC to inline x 
            Assert.That(x, Is.EqualTo(4));
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