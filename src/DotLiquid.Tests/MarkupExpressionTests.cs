using System;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class MarkupExpressionTests
    {
        [Test]
        public void TestExpressionEvaluation()
        {
            Context context = new Context();
            context["test"] = "TEST";
            context.AddFilters(typeof (TestFilters));
            //MarkupExpression expression = new MarkupExpression("test | blah1 | blah2");
            MarkupExpression expression = new MarkupExpression("test",new[] {
                                                                        new FilterRequest("blah1"),
                                                                        new FilterRequest("blah2")
                                                                    });
            var result = expression.Evaluate(context);

            Assert.AreEqual("TEST BLAH1 BLAH2", result);

        }

        private static class TestFilters
        {
            public static String Blah1(String orig)
            {
                return orig + " BLAH1";
            }
            public static String Blah2(String orig)
            {
                return orig + " BLAH2";
            }
        }

    }
}
