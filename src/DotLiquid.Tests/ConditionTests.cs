using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotLiquid.Exceptions;
using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ConditionTests
    {
        private Context _context;

        [Test]
        public void TestBasicCondition()
        {
            Assert.AreEqual(false, new Condition("1", "==", "2").Evaluate(null, CultureInfo.InvariantCulture));
            Assert.AreEqual(true, new Condition("1", "==", "1").Evaluate(null, CultureInfo.InvariantCulture));
        }

        [Test]
        public void TestDefaultOperatorsEvaluateTrue()
        {
            AssertEvaluatesTrue("1", "==", "1");
            AssertEvaluatesTrue("1", "!=", "2");
            AssertEvaluatesTrue("1", "<>", "2");
            AssertEvaluatesTrue("1", "<", "2");
            AssertEvaluatesTrue("2", ">", "1");
            AssertEvaluatesTrue("1", ">=", "1");
            AssertEvaluatesTrue("2", ">=", "1");
            AssertEvaluatesTrue("1", "<=", "2");
            AssertEvaluatesTrue("1", "<=", "1");
        }

        [Test]
        public void TestDefaultOperatorsEvaluateFalse()
        {
            AssertEvaluatesFalse("1", "==", "2");
            AssertEvaluatesFalse("1", "!=", "1");
            AssertEvaluatesFalse("1", "<>", "1");
            AssertEvaluatesFalse("1", "<", "0");
            AssertEvaluatesFalse("2", ">", "4");
            AssertEvaluatesFalse("1", ">=", "3");
            AssertEvaluatesFalse("2", ">=", "4");
            AssertEvaluatesFalse("1", "<=", "0");
            AssertEvaluatesFalse("1", "<=", "0");
        }

        [Test]
        public void TestContainsWorksOnStrings()
        {
            AssertEvaluatesTrue("'bob'", "contains", "'o'");
            AssertEvaluatesTrue("'bob'", "contains", "'b'");
            AssertEvaluatesTrue("'bob'", "contains", "'bo'");
            AssertEvaluatesTrue("'bob'", "contains", "'ob'");
            AssertEvaluatesTrue("'bob'", "contains", "'bob'");

            AssertEvaluatesFalse("'bob'", "contains", "'bob2'");
            AssertEvaluatesFalse("'bob'", "contains", "'a'");
            AssertEvaluatesFalse("'bob'", "contains", "'---'");
        }

        [Test]
        public void TestContainsWorksOnArrays()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            _context["array"] = new[] { 1, 2, 3, 4, 5 };

            AssertEvaluatesFalse("array", "contains", "0");
            AssertEvaluatesTrue("array", "contains", "1");
            AssertEvaluatesTrue("array", "contains", "2");
            AssertEvaluatesTrue("array", "contains", "3");
            AssertEvaluatesTrue("array", "contains", "4");
            AssertEvaluatesTrue("array", "contains", "5");
            AssertEvaluatesFalse("array", "contains", "6");

            AssertEvaluatesFalse("array", "contains", "'1'");
        }

        [Test]
        public void TestContainsReturnsFalseForNilCommands()
        {
            AssertEvaluatesFalse("not_assigned", "contains", "0");
            AssertEvaluatesFalse("0", "contains", "not_assigned");
        }

        [Test]
        public void TestStartsWithWorksOnStrings()
        {
            AssertEvaluatesTrue("'dave'", "startswith", "'d'");
            AssertEvaluatesTrue("'dave'", "startswith", "'da'");
            AssertEvaluatesTrue("'dave'", "startswith", "'dav'");
            AssertEvaluatesTrue("'dave'", "startswith", "'dave'");

            AssertEvaluatesFalse("'dave'", "startswith", "'ave'");
            AssertEvaluatesFalse("'dave'", "startswith", "'e'");
            AssertEvaluatesFalse("'dave'", "startswith", "'---'");
        }

        [Test]
        public void TestStartsWithWorksOnArrays()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            _context["array"] = new[] { 1, 2, 3, 4, 5 };

            AssertEvaluatesFalse("array", "startswith", "0");
            AssertEvaluatesTrue("array", "startswith", "1");
        }

        [Test]
        public void TestStartsWithReturnsFalseForNilCommands()
        {
            AssertEvaluatesFalse("not_assigned", "startswith", "0");
            AssertEvaluatesFalse("0", "startswith", "not_assigned");
        }

        [Test]
        public void TestEndsWithWorksOnStrings()
        {
            AssertEvaluatesTrue("'dave'", "endswith", "'e'");
            AssertEvaluatesTrue("'dave'", "endswith", "'ve'");
            AssertEvaluatesTrue("'dave'", "endswith", "'ave'");
            AssertEvaluatesTrue("'dave'", "endswith", "'dave'");

            AssertEvaluatesFalse("'dave'", "endswith", "'dav'");
            AssertEvaluatesFalse("'dave'", "endswith", "'d'");
            AssertEvaluatesFalse("'dave'", "endswith", "'---'");
        }

        [Test]
        public void TestEndsWithWorksOnArrays()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            _context["array"] = new[] { 1, 2, 3, 4, 5 };

            AssertEvaluatesFalse("array", "endswith", "0");
            AssertEvaluatesTrue("array", "endswith", "5");
        }

        [Test]
        public void TestEndsWithReturnsFalseForNilCommands()
        {
            AssertEvaluatesFalse("not_assigned", "endswith", "0");
            AssertEvaluatesFalse("0", "endswith", "not_assigned");
        }

        [Test]
        public void TestDictionaryHasKey()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            System.Collections.Generic.Dictionary<string, string> testDictionary = new System.Collections.Generic.Dictionary<string, string>
            {
                { "dave", "0" },
                { "bob", "4" }
            };
            _context["dictionary"] = testDictionary;

            AssertEvaluatesTrue("dictionary", "haskey", "'bob'");
            AssertEvaluatesFalse("dictionary", "haskey", "'0'");
        }

        [Test]
        public void TestDictionaryHasValue()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            System.Collections.Generic.Dictionary<string, string> testDictionary = new System.Collections.Generic.Dictionary<string, string>
            {
                { "dave", "0" },
                { "bob", "4" }
            };
            _context["dictionary"] = testDictionary;

            AssertEvaluatesTrue("dictionary", "hasvalue", "'0'");
            AssertEvaluatesFalse("dictionary", "hasvalue", "'bob'");
        }

        [Test]
        public void TestOrCondition()
        {
            Condition condition = new Condition("1", "==", "2");
            Assert.IsFalse(condition.Evaluate(null, CultureInfo.InvariantCulture));

            condition.Or(new Condition("2", "==", "1"));
            Assert.IsFalse(condition.Evaluate(null, CultureInfo.InvariantCulture));

            condition.Or(new Condition("1", "==", "1"));
            Assert.IsTrue(condition.Evaluate(null, CultureInfo.InvariantCulture));
        }

        [Test]
        public void TestAndCondition()
        {
            Condition condition = new Condition("1", "==", "1");
            Assert.IsTrue(condition.Evaluate(null, CultureInfo.InvariantCulture));

            condition.And(new Condition("2", "==", "2"));
            Assert.IsTrue(condition.Evaluate(null, CultureInfo.InvariantCulture));

            condition.And(new Condition("2", "==", "1"));
            Assert.IsFalse(condition.Evaluate(null, CultureInfo.InvariantCulture));
        }

        [Test]
        public void TestShouldAllowCustomProcOperator()
        {
            try
            {
                Condition.Operators["starts_with"] =
                    (left, right) => Regex.IsMatch(left.ToString(), string.Format("^{0}", right.ToString()));

                AssertEvaluatesTrue("'bob'", "starts_with", "'b'");
                AssertEvaluatesFalse("'bob'", "starts_with", "'o'");
            }
            finally
            {
                Condition.Operators.Remove("starts_with");
            }
        }

        [Test]
        public async Task TestCapitalInCustomOperator()
        {
            try
            {
                Condition.Operators["IsMultipleOf"] =
                    (left, right) => (int)left % (int)right == 0;

                // exact match
                AssertEvaluatesTrue("16", "IsMultipleOf", "4");
                AssertEvaluatesFalse("16", "IsMultipleOf", "5");

                // lower case: compatibility
                AssertEvaluatesTrue("16", "ismultipleof", "4");
                AssertEvaluatesFalse("16", "ismultipleof", "5");

                AssertEvaluatesTrue("16", "is_multiple_of", "4");
                AssertEvaluatesFalse("16", "is_multiple_of", "5");

                AssertError("16", "isMultipleOf", "4", typeof(ArgumentException));

                //Run tests through the template to verify that capitalization rules are followed through template parsing
                await Helper.AssertTemplateResultAsync(" TRUE ", "{% if 16 IsMultipleOf 4 %} TRUE {% endif %}");
                await Helper.AssertTemplateResultAsync("", "{% if 14 IsMultipleOf 4 %} TRUE {% endif %}");
                await Helper.AssertTemplateResultAsync(" TRUE ", "{% if 16 ismultipleof 4 %} TRUE {% endif %}");
                await Helper.AssertTemplateResultAsync("", "{% if 14 ismultipleof 4 %} TRUE {% endif %}");
                await Helper.AssertTemplateResultAsync(" TRUE ", "{% if 16 is_multiple_of 4 %} TRUE {% endif %}");
                await Helper.AssertTemplateResultAsync("", "{% if 14 is_multiple_of 4 %} TRUE {% endif %}");
                await Helper.AssertTemplateResultAsync("Liquid error: Unknown operator isMultipleOf", "{% if 16 isMultipleOf 4 %} TRUE {% endif %}");
            }
            finally
            {
                Condition.Operators.Remove("IsMultipleOf");
            }
        }

        [Test]
        public async Task TestCapitalInCustomCSharpOperator()
        {

            var oldconvention = Template.NamingConvention;
            Template.NamingConvention = new CSharpNamingConvention();

            try
            {
                Condition.Operators["DivisibleBy"] =
                    (left, right) => (int)left % (int)right == 0;

                // exact match
                AssertEvaluatesTrue("16", "DivisibleBy", "4");
                AssertEvaluatesFalse("16", "DivisibleBy", "5");

                // lower case: compatibility
                AssertEvaluatesTrue("16", "divisibleby", "4");
                AssertEvaluatesFalse("16", "divisibleby", "5");

                AssertError("16", "divisibleBy", "4", typeof(ArgumentException));

                //Run tests through the template to verify that capitalization rules are followed through template parsing
                await Helper.AssertTemplateResultAsync(" TRUE ", "{% if 16 DivisibleBy 4 %} TRUE {% endif %}");
                await Helper.AssertTemplateResultAsync("", "{% if 16 DivisibleBy 5 %} TRUE {% endif %}");
                await Helper.AssertTemplateResultAsync(" TRUE ", "{% if 16 divisibleby 4 %} TRUE {% endif %}");
                await Helper.AssertTemplateResultAsync("", "{% if 16 divisibleby 5 %} TRUE {% endif %}");
                await Helper.AssertTemplateResultAsync("Liquid error: Unknown operator divisibleBy", "{% if 16 divisibleBy 4 %} TRUE {% endif %}");
            }
            finally
            {
                Condition.Operators.Remove("DivisibleBy");
            }

            Template.NamingConvention = oldconvention;
        }

        [Test]
        public async Task TestLessThanDecimal()
        {
            var model = new { value = new decimal(-10.5) };

            string output = await Template.Parse("{% if model.value < 0 %}passed{% endif %}")
                .RenderAsync(Hash.FromAnonymousObject(new { model }));

            Assert.AreEqual("passed", output);
        }

        [Test]
        public async Task TestCompareBetweenDifferentTypes()
        {
            var row = new System.Collections.Generic.Dictionary<string, object>();

            short id = 1;
            row.Add("MyID", id);

            var current = "MyID is {% if MyID == 1 %}1{%endif%}";
            var parse = DotLiquid.Template.Parse(current);
            var parsedOutput = await parse.RenderAsync(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = Hash.FromDictionary(row) });
            Assert.AreEqual("MyID is 1", parsedOutput);
        }

        [Test]
        public async Task TestShouldAllowCustomProcOperatorCapitalized()
        {
            try
            {
                Condition.Operators["StartsWith"] =
                    (left, right) => Regex.IsMatch(left.ToString(), string.Format("^{0}", right.ToString()));

                await Helper.AssertTemplateResultAsync("", "{% if 'bob' StartsWith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
                AssertEvaluatesTrue("'bob'", "StartsWith", "'b'");
                AssertEvaluatesFalse("'bob'", "StartsWith", "'o'");
            }
            finally
            {
                Condition.Operators.Remove("StartsWith");
            }
        }

        [Test]
        public async Task TestRuby_LowerCaseAccepted()
        {
            await Helper.AssertTemplateResultAsync("", "{% if 'bob' startswith 'B' %} YES {% endif %}");
            await Helper.AssertTemplateResultAsync(" YES ", "{% if 'Bob' startswith 'B' %} YES {% endif %}");
        }

        [Test]
        public async Task TestRuby_SnakeCaseAccepted()
        {
            await Helper.AssertTemplateResultAsync("", "{% if 'bob' starts_with 'B' %} YES {% endif %}");
            await Helper.AssertTemplateResultAsync(" YES ", "{% if 'Bob' starts_with 'B' %} YES {% endif %}");
        }

        [Test]
        public async Task TestRuby_PascalCaseNotAccepted()
        {
            await Helper.AssertTemplateResultAsync("Liquid error: Unknown operator StartsWith", "{% if 'bob' StartsWith 'B' %} YES {% endif %}");
        }

        [Test]
        public async Task TestCSharp_LowerCaseAccepted()
        {
            await Helper.AssertTemplateResultAsync("", "{% if 'bob' startswith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
            await Helper.AssertTemplateResultAsync(" YES ", "{% if 'Bob' startswith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
        }

        [Test]
        public async Task TestCSharp_PascalCaseAccepted()
        {
            await Helper.AssertTemplateResultAsync("", "{% if 'bob' StartsWith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
            await Helper.AssertTemplateResultAsync(" YES ", "{% if 'Bob' StartsWith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
        }

        [Test]
        public async Task TestCSharp_LowerPascalCaseAccepted()
        {
            await Helper.AssertTemplateResultAsync("", "{% if 'bob' startsWith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
            await Helper.AssertTemplateResultAsync(" YES ", "{% if 'Bob' startsWith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
        }

        [Test]
        public async Task TestCSharp_SnakeCaseNotAccepted()
        {
            await Helper.AssertTemplateResultAsync("Liquid error: Unknown operator starts_with", "{% if 'bob' starts_with 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
        }


        #region Helper methods

        private void AssertEvaluatesTrue(string left, string op, string right)
        {
            Assert.IsTrue(new Condition(left, op, right).Evaluate(_context ?? new Context(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
                "Evaluated false: {0} {1} {2}", left, op, right);
        }

        private void AssertEvaluatesFalse(string left, string op, string right)
        {
            Assert.IsFalse(new Condition(left, op, right).Evaluate(_context ?? new Context(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
                "Evaluated true: {0} {1} {2}", left, op, right);
        }

        private void AssertError(string left, string op, string right, System.Type errorType)
        {
            Assert.Throws(errorType, () => new Condition(left, op, right).Evaluate(_context ?? new Context(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture));
        }

        #endregion
    }
}
