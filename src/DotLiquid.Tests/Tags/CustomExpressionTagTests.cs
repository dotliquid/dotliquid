using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class CustomExpressionTagTests
    {
        private static readonly String _prefix = "I see the the dereferenced value: ";

        [Test]
        public void TestExpressionInTag()
        {
            const string template = "Test {% testtag test %}.";

            Template.RegisterTag<MyTagThatUsesAnExpression>("testtag");

            Template liquidTemplate = Template.Parse(template); // Parses and compiles the template

            Dictionary<String, Object> localVariables = new Dictionary<string, object> { { "test", "123" } };

            var localVariableHash = Hash.FromDictionary(localVariables);

            var result = liquidTemplate.Render(localVariableHash);
            Console.WriteLine(result);

            Assert.AreEqual("Test "+_prefix + "123.", result);
        }

        private class MyTagThatUsesAnExpression : Tag
        {
            private MarkupExpression _expression;

            public override void Initialize(string tagName, string markup, List<string> tokens)
            {
                base.Initialize(tagName, markup, tokens);

                MarkupParser parser = new MarkupParser();
                var result = parser.Parse(markup);
                _expression = new MarkupExpression(result.Name, result.Filters);

            }

            
            public override void Render(Context context, System.IO.TextWriter result)
            {
                var obj = _expression.Evaluate(context);
                result.Write(_prefix + obj);
            }
        

        }

        private static class TestFilters
        {
            public static String Prependhello(String orig)
            {
                return "hello, " +orig ;
            }

        }
    }
}
