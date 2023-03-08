using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ConditionTests
    {
        #region Classes used in tests
        public class Car : Drop, System.IEquatable<Car>, System.IEquatable<string>
        {
            public string Make { get; set; }
            public string Model { get; set; }

            public override string ToString()
            {
                return $"{Make} {Model}";
            }

            public override bool Equals(object other)
            {
                if (other is Car @car)
                    return Equals(@car);

                if (other is string @string)
                    return Equals(@string);

                return false;
            }

            public bool Equals(Car other)
            {
                return other.Make == this.Make && other.Model == this.Model;
            }

            public bool Equals(string other)
            {
                return other == this.ToString();
            }
        }

        public class DummyDrop : Drop
        {
        }
        #endregion

        // NOTE(David Burg): This forces sequential execution of tests, risk side effect resulting in non deterministic behavior.
        // Context should be passed as a parameter instead.
        private Context _context;

        [Test]
        public void TestBasicCondition()
        {
            Assert.AreEqual(expected: false, actual: new Condition(left: "1", @operator: "==", right: "2").Evaluate(context: null, formatProvider: CultureInfo.InvariantCulture));
            Assert.AreEqual(expected: true, actual: new Condition(left: "1", @operator: "==", right: "1").Evaluate(context: null, formatProvider: CultureInfo.InvariantCulture));

            // NOTE(David Burg): Validate that type conversion order preserves legacy behavior
            // Even if it's out of Shopify spec compliance (all type but null and false should evaluate to true).
            Helper.AssertTemplateResult(expected: "TRUE", template: "{% if true == 'true' %}TRUE{% else %}FALSE{% endif %}");
            Helper.AssertTemplateResult(expected: "FALSE", template: "{% if 'true' == true %}TRUE{% else %}FALSE{% endif %}");

            Helper.AssertTemplateResult(expected: "TRUE", template: "{% if true %}TRUE{% endif %}");
            Helper.AssertTemplateResult(expected: "", template: "{% if false %}TRUE{% endif %}");
            Helper.AssertTemplateResult(expected: "TRUE", template: "{% if true %}TRUE{% else %}FALSE{% endif %}");
            Helper.AssertTemplateResult(expected: "FALSE", template: "{% if false %}TRUE{% else %}FALSE{% endif %}");
            Helper.AssertTemplateResult(expected: "TRUE", template: "{% if '1' == '1' %}TRUE{% else %}FALSE{% endif %}");
            Helper.AssertTemplateResult(expected: "FALSE", template: "{% if '1' == '2' %}TRUE{% else %}FALSE{% endif %}");
            Helper.AssertTemplateResult(expected: "This condition will always be true.", template: "{% assign tobi = 'Tobi' %}{% if tobi %}This condition will always be true.{% endif %}");

            Helper.AssertTemplateResult(expected: "TRUE", template: "{% if true == true %}TRUE{% else %}FALSE{% endif %}");
            Helper.AssertTemplateResult(expected: "FALSE", template: "{% if true == false %}TRUE{% else %}FALSE{% endif %}");
            Helper.AssertTemplateResult(expected: "TRUE", template: "{% if false == false %}TRUE{% else %}FALSE{% endif %}");
            Helper.AssertTemplateResult(expected: "FALSE", template: "{% if false == true %}TRUE{% else %}FALSE{% endif %}");

            Helper.AssertTemplateResult(expected: "FALSE", template: "{% if true != true %}TRUE{% else %}FALSE{% endif %}");
            Helper.AssertTemplateResult(expected: "TRUE", template: "{% if true != false %}TRUE{% else %}FALSE{% endif %}");
            Helper.AssertTemplateResult(expected: "FALSE", template: "{% if false != false %}TRUE{% else %}FALSE{% endif %}");
            Helper.AssertTemplateResult(expected: "TRUE", template: "{% if false != true %}TRUE{% else %}FALSE{% endif %}");

            // NOTE(David Burg): disabled test due to https://github.com/dotliquid/dotliquid/issues/394
            ////Helper.AssertTemplateResult(expected: "This text will always appear if \"name\" is defined.", template: "{% assign name = 'Tobi' %}{% if name == true %}This text will always appear if \"name\" is defined.{% endif %}");
        }

        [Test]
        public void TestDefaultOperatorsEvaluateTrue()
        {
            this.AssertEvaluatesTrue(left: "1", op: "==", right: "1");
            this.AssertEvaluatesTrue(left: "1", op: "!=", right: "2");
            this.AssertEvaluatesTrue(left: "1", op: "<>", right: "2");
            this.AssertEvaluatesTrue(left: "1", op: "<", right: "2");
            this.AssertEvaluatesTrue(left: "2", op: ">", right: "1");
            this.AssertEvaluatesTrue(left: "1", op: ">=", right: "1");
            this.AssertEvaluatesTrue(left: "2", op: ">=", right: "1");
            this.AssertEvaluatesTrue(left: "1", op: "<=", right: "2");
            this.AssertEvaluatesTrue(left: "1", op: "<=", right: "1");
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
        public void TestContainsWorksOnIntArrays()
        {
            // NOTE(daviburg): DotLiquid is in violation of explicit non-support of arrays for contains operators, quote:
            // "contains can only search strings. You cannot use it to check for an object in an array of objects."
            // https://shopify.github.io/liquid/basics/operators/
            // This is a rather harmless violation as all it does in generate useful output for a request which would fail
            // in the canonical Shopify implementation.
            _context = new Context(CultureInfo.InvariantCulture);
            _context["array"] = new[] { 1, 2, 3, 4, 5 };

            AssertEvaluatesTrue(left: "array", op: "contains", right: "1");
            AssertEvaluatesFalse(left: "array", op: "contains", right: "0");
            AssertEvaluatesTrue(left: "array", op: "contains", right: "2");
            AssertEvaluatesTrue(left: "array", op: "contains", right: "3");
            AssertEvaluatesTrue(left: "array", op: "contains", right: "4");
            AssertEvaluatesTrue(left: "array", op: "contains", right: "5");
            AssertEvaluatesFalse(left: "array", op: "contains", right: "6");

            // NOTE(daviburg): Historically testing for equality cross integer and string boundaries resulted in not equal.
            AssertEvaluatesFalse(left: "array", op: "contains", right: "'1'");
        }

        [Test]
        public void TestContainsWorksOnLongArrays()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            _context["array"] = new long[] { 1, 2, 3, 4, 5 };

            AssertEvaluatesTrue("array", "contains", "1");
            AssertEvaluatesFalse("array", "contains", "0");
            AssertEvaluatesTrue("array", "contains", "1.0");
            AssertEvaluatesTrue("array", "contains", "2");
            AssertEvaluatesTrue("array", "contains", "3");
            AssertEvaluatesTrue("array", "contains", "4");
            AssertEvaluatesTrue("array", "contains", "5");
            AssertEvaluatesFalse("array", "contains", "6");

            AssertEvaluatesFalse("array", "contains", "'1'");
        }

        [Test]
        public void TestStringArrays()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            var _array = new List<string>() { "Apple", "Orange", null, "Banana" };
            _context["array"] = _array.ToArray();
            _context["first"] = _array.First();
            _context["last"] = _array.Last();

            AssertEvaluatesTrue(left: "array", op: "contains", right: "'Apple'");
            AssertEvaluatesTrue(left: "array", op: "startsWith", right: "first");
            AssertEvaluatesTrue(left: "array.first", op: "==", right: "first");
            AssertEvaluatesFalse(left: "array", op: "contains", right: "'apple'");
            AssertEvaluatesFalse(left: "array", op: "startsWith", right: "'apple'");
            AssertEvaluatesFalse(left: "array.first", op: "==", right: "'apple'");
            AssertEvaluatesFalse(left: "array", op: "contains", right: "'Mango'");
            AssertEvaluatesTrue(left: "array", op: "contains", right: "'Orange'");
            AssertEvaluatesTrue(left: "array", op: "contains", right: "'Banana'");
            AssertEvaluatesTrue(left: "array", op: "endsWith", right: "last");
            AssertEvaluatesFalse(left: "array", op: "contains", right: "'Orang'");
        }

        [Test]
        public void TestClassArrays()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            var _array = new List<Car>() { new Car() { Make = "Honda", Model = "Accord" }, new Car() { Make = "Ford", Model = "Explorer" } };
            _context["array"] = _array.ToArray();
            _context["first"] = _array.First();
            _context["last"] = _array.Last();
            _context["clone"] = new Car() { Make = "Honda", Model = "Accord" };
            _context["camry"] = new Car() { Make = "Toyota", Model = "Camry" };

            AssertEvaluatesTrue(left: "array", op: "contains", right: "first");
            AssertEvaluatesTrue(left: "array", op: "startsWith", right: "first");
            AssertEvaluatesTrue(left: "array.first", op: "==", right: "first");
            AssertEvaluatesTrue(left: "array", op: "contains", right: "clone");
            AssertEvaluatesTrue(left: "array", op: "startsWith", right: "clone");
            AssertEvaluatesTrue(left: "array", op: "endsWith", right: "last");
            AssertEvaluatesFalse(left: "array", op: "contains", right: "camry");
        }

        [Test]
        public void TestTruthyArray()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            var _array = new List<bool>() { true };
            _context["array"] = _array.ToArray();
            _context["first"] = _array.First();

            AssertEvaluatesTrue(left: "array", op: "contains", right: "first");
            AssertEvaluatesTrue(left: "array", op: "startsWith", right: "first");
            AssertEvaluatesTrue(left: "array.first", op: "==", right: "'true'");
            AssertEvaluatesTrue(left: "array", op: "startsWith", right: "'true'");

            AssertEvaluatesFalse(left: "array", op: "contains", right: "'true'"); // to be re-evaluated in #362
        }

        [Test]
        public void TestCharArrays()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            var _array = new List<char> { 'A', 'B', 'C' };
            _context["array"] = _array.ToArray();
            _context["first"] = _array.First();
            _context["last"] = _array.Last();

            AssertEvaluatesTrue(left: "array", op: "contains", right: "'A'");
            AssertEvaluatesTrue(left: "array", op: "contains", right: "first");
            AssertEvaluatesTrue(left: "array", op: "startsWith", right: "first");
            AssertEvaluatesTrue(left: "array.first", op: "==", right: "first");
            AssertEvaluatesFalse(left: "array", op: "contains", right: "'a'");
            AssertEvaluatesFalse(left: "array", op: "contains", right: "'X'");
            AssertEvaluatesTrue(left: "array", op: "contains", right: "'B'");
            AssertEvaluatesTrue(left: "array", op: "contains", right: "'C'");
            AssertEvaluatesTrue(left: "array", op: "endsWith", right: "last");
        }

        [Test]
        public void TestByteArrays()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            var _array = new List<byte> { 0x01, 0x02, 0x03, 0x30 };
            _context["array"] = _array.ToArray();
            _context["first"] = _array.First();
            _context["last"] = _array.Last();

            AssertEvaluatesFalse(left: "array", op: "contains", right: "0");
            AssertEvaluatesFalse(left: "array", op: "contains", right: "'0'");
            AssertEvaluatesTrue(left: "array", op: "startsWith", right: "first");
            AssertEvaluatesTrue(left: "array.first", op: "==", right: "first");
            AssertEvaluatesTrue(left: "array", op: "contains", right: "first");
            AssertEvaluatesFalse(left: "array", op: "contains", right: "1");
            AssertEvaluatesTrue(left: "array", op: "endsWith", right: "last");
        }

        [Test]
        public void TestContainsWorksOnDoubleArrays()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            _context["array"] = new double[] { 1.0, 2.1, 3.25, 4.333, 5.0 };

            AssertEvaluatesTrue("array", "contains", "1.0");
            AssertEvaluatesFalse("array", "contains", "0");
            AssertEvaluatesTrue("array", "contains", "2.1");
            AssertEvaluatesFalse("array", "contains", "3");
            AssertEvaluatesFalse("array", "contains", "4.33");
            AssertEvaluatesTrue("array", "contains", "5.00");
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
        public void TestCapitalInCustomOperatorInt()
        {
            try
            {
                Condition.Operators["IsMultipleOf"] =
                    (left, right) => (int)left % (int)right == 0;

                // exact match
                AssertEvaluatesTrue("16", "IsMultipleOf", "4");
                AssertEvaluatesTrue("2147483646", "IsMultipleOf", "2");
                AssertError("2147483648", "IsMultipleOf", "2", typeof(System.InvalidCastException));
                AssertEvaluatesFalse("16", "IsMultipleOf", "5");

                // lower case: compatibility
                AssertEvaluatesTrue("16", "ismultipleof", "4");
                AssertEvaluatesFalse("16", "ismultipleof", "5");

                AssertEvaluatesTrue("16", "is_multiple_of", "4");
                AssertEvaluatesFalse("16", "is_multiple_of", "5");

                // camel case : incompatible
                AssertError("16", "isMultipleOf", "4", typeof(ArgumentException));

                //Run tests through the template to verify that capitalization rules are followed through template parsing
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 IsMultipleOf 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("", "{% if 14 IsMultipleOf 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 ismultipleof 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("", "{% if 14 ismultipleof 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 is_multiple_of 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("", "{% if 14 is_multiple_of 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("Liquid error: Unknown operator isMultipleOf", "{% if 16 isMultipleOf 4 %} TRUE {% endif %}");
            }
            finally
            {
                Condition.Operators.Remove("IsMultipleOf");
            }
        }

        [Test]
        public void TestCapitalInCustomOperatorLong()
        {
            try
            {
                Condition.Operators["IsMultipleOf"] =
                    (left, right) => System.Convert.ToInt64(left) % System.Convert.ToInt64(right) == 0;

                // exact match
                AssertEvaluatesTrue("16", "IsMultipleOf", "4");
                AssertEvaluatesTrue("2147483646", "IsMultipleOf", "2");
                AssertEvaluatesTrue("2147483648", "IsMultipleOf", "2");
                AssertEvaluatesFalse("16", "IsMultipleOf", "5");

                // lower case: compatibility
                AssertEvaluatesTrue("16", "ismultipleof", "4");
                AssertEvaluatesFalse("16", "ismultipleof", "5");

                AssertEvaluatesTrue("16", "is_multiple_of", "4");
                AssertEvaluatesFalse("16", "is_multiple_of", "5");

                // camel case : incompatible
                AssertError("16", "isMultipleOf", "4", typeof(ArgumentException));

                //Run tests through the template to verify that capitalization rules are followed through template parsing
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 IsMultipleOf 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("", "{% if 14 IsMultipleOf 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 ismultipleof 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("", "{% if 14 ismultipleof 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 is_multiple_of 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("", "{% if 14 is_multiple_of 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("Liquid error: Unknown operator isMultipleOf", "{% if 16 isMultipleOf 4 %} TRUE {% endif %}");
            }
            finally
            {
                Condition.Operators.Remove("IsMultipleOf");
            }
        }

        [Test]
        public void TestCapitalInCustomCSharpOperatorInt()
        {
            //have to run this test in a lock because it requires
            //changing the globally static NamingConvention
            lock (Template.NamingConvention)
            {
                var oldconvention = Template.NamingConvention;
                Template.NamingConvention = new CSharpNamingConvention();

                try
                {
                    Condition.Operators["DivisibleBy"] =
                        (left, right) => (int)left % (int)right == 0;

                    // exact match
                    AssertEvaluatesTrue("16", "DivisibleBy", "4");
                    AssertEvaluatesTrue("2147483646", "DivisibleBy", "2");
                    AssertError("2147483648", "DivisibleBy", "2", typeof(System.InvalidCastException));
                    AssertEvaluatesFalse("16", "DivisibleBy", "5");

                    // lower case: compatibility
                    AssertEvaluatesTrue("16", "divisibleby", "4");
                    AssertEvaluatesFalse("16", "divisibleby", "5");

                    // camel case : compatibility
                    AssertEvaluatesTrue("16", "divisibleBy", "4");
                    AssertEvaluatesFalse("16", "divisibleBy", "5");

                    // snake case : incompatible
                    AssertError("16", "divisible_by", "4", typeof(ArgumentException));

                    //Run tests through the template to verify that capitalization rules are followed through template parsing
                    Helper.AssertTemplateResult(" TRUE ", "{% if 16 DivisibleBy 4 %} TRUE {% endif %}");
                    Helper.AssertTemplateResult("", "{% if 16 DivisibleBy 5 %} TRUE {% endif %}");
                    Helper.AssertTemplateResult(" TRUE ", "{% if 16 divisibleby 4 %} TRUE {% endif %}");
                    Helper.AssertTemplateResult("", "{% if 16 divisibleby 5 %} TRUE {% endif %}");
                    Helper.AssertTemplateResult("Liquid error: Unknown operator divisible_by", "{% if 16 divisible_by 4 %} TRUE {% endif %}");
                }
                finally
                {
                    Condition.Operators.Remove("DivisibleBy");
                    Template.NamingConvention = oldconvention;
                }
            }
        }

        [Test]
        public void TestCapitalInCustomCSharpOperatorLong()
        {
            //have to run this test in a lock because it requires
            //changing the globally static NamingConvention
            lock (Template.NamingConvention)
            {
                var oldconvention = Template.NamingConvention;
                Template.NamingConvention = new CSharpNamingConvention();

                try
                {
                    Condition.Operators["DivisibleBy"] =
                        (left, right) => System.Convert.ToInt64(left) % System.Convert.ToInt64(right) == 0;

                    // exact match
                    AssertEvaluatesTrue("16", "DivisibleBy", "4");
                    AssertEvaluatesTrue("2147483646", "DivisibleBy", "2");
                    AssertEvaluatesTrue("2147483648", "DivisibleBy", "2");
                    AssertEvaluatesFalse("16", "DivisibleBy", "5");

                    // lower case: compatibility
                    AssertEvaluatesTrue("16", "divisibleby", "4");
                    AssertEvaluatesFalse("16", "divisibleby", "5");

                    // camel case: compatibility
                    AssertEvaluatesTrue("16", "divisibleBy", "4");
                    AssertEvaluatesFalse("16", "divisibleBy", "5");

                    // snake case: incompatible
                    AssertError("16", "divisible_by", "4", typeof(ArgumentException));

                    //Run tests through the template to verify that capitalization rules are followed through template parsing
                    Helper.AssertTemplateResult(" TRUE ", "{% if 16 DivisibleBy 4 %} TRUE {% endif %}");
                    Helper.AssertTemplateResult("", "{% if 16 DivisibleBy 5 %} TRUE {% endif %}");
                    Helper.AssertTemplateResult(" TRUE ", "{% if 16 divisibleby 4 %} TRUE {% endif %}");
                    Helper.AssertTemplateResult("", "{% if 16 divisibleby 5 %} TRUE {% endif %}");
                    Helper.AssertTemplateResult("Liquid error: Unknown operator divisible_by", "{% if 16 divisible_by 4 %} TRUE {% endif %}");
                }
                finally
                {
                    Condition.Operators.Remove("DivisibleBy");
                    Template.NamingConvention = oldconvention;
                }
            }
        }

        [Test]
        public void TestLessThanDecimal()
        {
            var model = new { value = new decimal(-10.5) };

            string output = Template.Parse("{% if model.value < 0 %}passed{% endif %}")
                .Render(Hash.FromAnonymousObject(new { model }));

            Assert.AreEqual("passed", output);
        }

        [Test]
        public void TestCompareBetweenDifferentTypes()
        {
            var row = new System.Collections.Generic.Dictionary<string, object>();

            short id = 1;
            row.Add("MyID", id);

            var current = "MyID is {% if MyID == 1 %}1{%endif%}";
            var parse = DotLiquid.Template.Parse(current);
            var parsedOutput = parse.Render(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = Hash.FromDictionary(row) });
            Assert.AreEqual("MyID is 1", parsedOutput);
        }

        [Test]
        public void TestShouldAllowCustomProcOperatorCapitalized()
        {
            try
            {
                Condition.Operators["StartsWith"] =
                    (left, right) => Regex.IsMatch(left.ToString(), string.Format("^{0}", right.ToString()));

                Helper.AssertTemplateResult("", "{% if 'bob' StartsWith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
                AssertEvaluatesTrue("'bob'", "StartsWith", "'b'");
                AssertEvaluatesFalse("'bob'", "StartsWith", "'o'");
            }
            finally
            {
                Condition.Operators.Remove("StartsWith");
            }
        }

        [Test]
        public void TestRuby_LowerCaseAccepted()
        {
            Helper.AssertTemplateResult("", "{% if 'bob' startswith 'B' %} YES {% endif %}");
            Helper.AssertTemplateResult(" YES ", "{% if 'Bob' startswith 'B' %} YES {% endif %}");
        }

        [Test]
        public void TestRuby_SnakeCaseAccepted()
        {
            Helper.AssertTemplateResult("", "{% if 'bob' starts_with 'B' %} YES {% endif %}");
            Helper.AssertTemplateResult(" YES ", "{% if 'Bob' starts_with 'B' %} YES {% endif %}");
        }

        [Test]
        public void TestRuby_PascalCaseNotAccepted()
        {
            Helper.AssertTemplateResult("Liquid error: Unknown operator StartsWith", "{% if 'bob' StartsWith 'B' %} YES {% endif %}");
        }

        [Test]
        public void TestCSharp_LowerCaseAccepted()
        {
            Helper.AssertTemplateResult("", "{% if 'bob' startswith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
            Helper.AssertTemplateResult(" YES ", "{% if 'Bob' startswith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
        }

        [Test]
        public void TestCSharp_PascalCaseAccepted()
        {
            Helper.AssertTemplateResult("", "{% if 'bob' StartsWith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
            Helper.AssertTemplateResult(" YES ", "{% if 'Bob' StartsWith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
        }

        [Test]
        public void TestCSharp_LowerPascalCaseAccepted()
        {
            Helper.AssertTemplateResult("", "{% if 'bob' startsWith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
            Helper.AssertTemplateResult(" YES ", "{% if 'Bob' startsWith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
        }

        [Test]
        public void TestCSharp_SnakeCaseNotAccepted()
        {
            Helper.AssertTemplateResult("Liquid error: Unknown operator starts_with", "{% if 'bob' starts_with 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
        }

        private enum TestEnum { Yes, No }

        [Test]
        public void TestEqualOperatorsWorksOnEnum()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            _context["enum"] = TestEnum.Yes;

            AssertEvaluatesTrue("enum", "==", "'Yes'");
            AssertEvaluatesTrue("enum", "!=", "'No'");

            AssertEvaluatesFalse("enum", "==", "'No'");
            AssertEvaluatesFalse("enum", "!=", "'Yes'");
        }

        [Test]
        public void TestBlankObject()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            _context["dictionary"] = new Dictionary<string, string> { { "abc", "xyz" } };
            _context["empty_dictionary"] = new Dictionary<string, string> { };
            _context["list"] = new List<string> { "abc" };
            _context["empty_list"] = new List<string> { };
            _context["array"] = new string[] { "foo" };
            _context["empty_array"] = new string[] { };
            _context["a_drop"] = new DummyDrop();

            // self check
            AssertEvaluatesFalse(left: "blank", op: "==", right: "blank");
            AssertEvaluatesTrue(left: "blank", op: "!=", right: "blank");
            AssertEvaluatesTrue(left: "blank", op: "<>", right: "blank");

            // blank truthy
            AssertEvaluatesTrue(left: "''", op: "==", right: "blank");
            AssertEvaluatesTrue(left: "'  '", op: "==", right: "blank");
            AssertEvaluatesTrue(left: "false", op: "==", right: "blank");
            AssertEvaluatesTrue(left: "nil", op: "==", right: "blank");
            AssertEvaluatesTrue(left: "not_assigned", op: "==", right: "blank");
            AssertEvaluatesTrue(left: "empty_dictionary", op: "==", right: "blank");
            AssertEvaluatesTrue(left: "empty_list", op: "==", right: "blank");
            AssertEvaluatesTrue(left: "empty_array", op: "==", right: "blank");

            // blank falsy
            AssertEvaluatesTrue(left: "1", op: "!=", right: "blank");
            AssertEvaluatesTrue(left: "0", op: "!=", right: "blank");
            AssertEvaluatesTrue(left: "true", op: "!=", right: "blank");
            AssertEvaluatesTrue(left: "a_drop", op: "!=", right: "blank");
            AssertEvaluatesTrue(left: "dictionary", op: "!=", right: "blank");
            AssertEvaluatesTrue(left: "list", op: "!=", right: "blank");
            AssertEvaluatesTrue(left: "array", op: "!=", right: "blank");
        }

        [Test]
        public void TestEmptyObject()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            _context["dictionary"] = new Dictionary<string, string> { { "abc", "xyz" } };
            _context["empty_dictionary"] = new Dictionary<string, string> { };
            _context["list"] = new List<string> { "abc" };
            _context["empty_list"] = new List<string> { };
            _context["array"] = new string[] { "foo" };
            _context["empty_array"] = new string[] { };
            _context["a_drop"] = new DummyDrop();

            // self check
            AssertEvaluatesFalse(left: "empty", op: "==", right: "empty");
            AssertEvaluatesTrue(left: "empty", op: "!=", right: "empty");
            AssertEvaluatesTrue(left: "empty", op: "<>", right: "empty");

            // empty truthy
            AssertEvaluatesTrue(left: "''", op: "==", right: "empty");
            AssertEvaluatesTrue(left: "empty_dictionary", op: "==", right: "empty");
            AssertEvaluatesTrue(left: "empty_list", op: "==", right: "empty");
            AssertEvaluatesTrue(left: "empty_array", op: "==", right: "empty");

            // empty falsy
            AssertEvaluatesTrue(left: "'  '", op: "!=", right: "empty");
            AssertEvaluatesTrue(left: "false", op: "!=", right: "empty");
            AssertEvaluatesTrue(left: "nil", op: "!=", right: "empty");
            AssertEvaluatesTrue(left: "no_assigned", op: "!=", right: "empty");
            AssertEvaluatesTrue(left: "1", op: "!=", right: "empty");
            AssertEvaluatesTrue(left: "0", op: "!=", right: "empty");
            AssertEvaluatesTrue(left: "true", op: "!=", right: "empty");
            AssertEvaluatesTrue(left: "a_drop", op: "!=", right: "empty");
            AssertEvaluatesTrue(left: "dictionary", op: "!=", right: "empty");
            AssertEvaluatesTrue(left: "list", op: "!=", right: "empty");
            AssertEvaluatesTrue(left: "array", op: "!=", right: "empty");
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
