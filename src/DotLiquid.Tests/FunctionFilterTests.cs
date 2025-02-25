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
        public void AddFilterWithOneArg()
        {
            _context["var"] = 2;
            _context.AddFilter<int, string>("PlusTwo", i => (i + 2).ToString(CultureInfo.InvariantCulture));
            Assert.That(new Variable("var | plus_two").Render(_context), Is.EqualTo("4"));
        }

        [Test]
        public void AddFilterWithOneArgAnonymousFunctionWithClosure()
        {
            _context["var"] = 2;
            int x = 2;

            // (x=(i + x)) is to forbid JITC to inline x and force it to create non-static closure

            _context.AddFilter<int, string>("PlusTwo", i => (x=(i + x)).ToString(CultureInfo.InvariantCulture));
            Assert.That(new Variable("var | plus_two").Render(_context), Is.EqualTo("4"));

            //this is done, to forbid JITC to inline x 
            Assert.That(x, Is.EqualTo(4));
        }

        [Test]
        public void AddFilterWithTwoArgs()
        {
            _context["var"] = 2;
            _context.AddFilter<int, int, string>("AddPlusTwo", (i, j) => (i + j + 2).ToString(CultureInfo.InvariantCulture));
            Assert.That(new Variable("var | add_plus_two: 3").Render(_context), Is.EqualTo("7"));
        }

        [Test]
        public void AddFilterWithTwoArgsAnonymousFunctionWithClosure()
        {
            _context["var"] = 2;
            int x = 2;

            // (x=(i + j + x)) is to forbid JITC to inline x and force it to create non-static closure

            _context.AddFilter<int, int, string>("AddPlusTwo", (i, j) => (x=(i + j + 2)).ToString(CultureInfo.InvariantCulture));
            Assert.That(new Variable("var | add_plus_two: 3").Render(_context), Is.EqualTo("7"));

            //this is done, to forbid JITC to inline x 
            Assert.That(x, Is.EqualTo(7));
        }
    }
}