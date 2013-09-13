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
        private static readonly String _prefix = "The final value: ";

        [Test]
        public void TestDictionaryExpressionInTag()
        {

            const string template = "Test {% testtag test %}.";
            var localVars = new Dictionary<string, object>
                {
                    { "test", "123" }
                };
            var result = RenderTemplate(template, localVars);

            Assert.AreEqual("Test "+_prefix + "123.", result);
        }

        [Test]
        public void TestNestedDictionaryExpressionInTag()
        {
            const string template = "Test {% testtag person.address %}.";
            var localVars = new Dictionary<string, object>
                {
                    {"person", new Dictionary<String, Object> {{"address", "123 Main St"}}}
                };
            var result = RenderTemplate(template, localVars);

            Assert.AreEqual("Test " + _prefix + "123 Main St.", result);
        }



        [Test]
        public void TestNestedDictionaryExpressionWithFilterInTag()
        {
            const string template = "Test {% testtag person.address | toupper %}.";

            var localVars = new Dictionary<string, object>
                {
                    { "person", new Dictionary<String, Object> { { "address", "123 Main St" } } }
                };
            var result = RenderTemplate(template, localVars);
            Console.WriteLine(result);

            Assert.AreEqual("Test " + _prefix + "123 MAIN ST.", result);
        }


        private static string RenderTemplate(string template, Dictionary<String, Object> localVariables)
        {
            Template.RegisterTag<MyTagThatUsesAnExpression>("testtag");
            Template.RegisterFilter(typeof(TestFilters));
            Template liquidTemplate = Template.Parse(template); // Parses and compiles the template


            var localVariableHash = Hash.FromDictionary(localVariables);

            var result = liquidTemplate.Render(localVariableHash);
            return result;
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
            public static String Toupper(String orig)
            {
                return (orig ?? "").ToUpper();
            }
        }
    }
}
