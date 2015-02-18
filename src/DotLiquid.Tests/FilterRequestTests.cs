using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class FilterRequestTests
    {
        private Context _context;

        [SetUp]
        public void Setup()
        {
            _context = new Context();
            _context.AddFilters(new[] { typeof(TestFilters) });
        }

        [Test]
        public void TestFilterApplication()
        {
            // Arrange
            const string origText = "hello";
            FilterRequest filterRequest = new FilterRequest("addthere", new String[] {});

            // Act
            var result = filterRequest.Apply(_context, origText);

            // Assert
            Assert.AreEqual(origText + " there", result);
        }

        private static class TestFilters
        {
            public static String Addthere(String txt)
            {
				return string.Format(txt + " there");
			}

        }

    }
}
