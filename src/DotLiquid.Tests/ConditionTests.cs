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
        private INamingConvention NamingConvention { get; } = new RubyNamingConvention();

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


        [Test]
        public void TestBasicCondition()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);

            Assert.AreEqual(expected: false, actual: new Condition(left: "1", @operator: "==", right: "2").Evaluate(context: context, formatProvider: CultureInfo.InvariantCulture));
            Assert.AreEqual(expected: true, actual: new Condition(left: "1", @operator: "==", right: "1").Evaluate(context: context, formatProvider: CultureInfo.InvariantCulture));

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
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            this.AssertEvaluatesTrue(context, left: "1", op: "==", right: "1");
            this.AssertEvaluatesTrue(context, left: "1", op: "!=", right: "2");
            this.AssertEvaluatesTrue(context, left: "1", op: "<>", right: "2");
            this.AssertEvaluatesTrue(context, left: "1", op: "<", right: "2");
            this.AssertEvaluatesTrue(context, left: "2", op: ">", right: "1");
            this.AssertEvaluatesTrue(context, left: "1", op: ">=", right: "1");
            this.AssertEvaluatesTrue(context, left: "2", op: ">=", right: "1");
            this.AssertEvaluatesTrue(context, left: "1", op: "<=", right: "2");
            this.AssertEvaluatesTrue(context, left: "1", op: "<=", right: "1");
        }

        [Test]
        public void TestDefaultOperatorsEvaluateFalse()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            AssertEvaluatesFalse(context, "1", "==", "2");
            AssertEvaluatesFalse(context, "1", "!=", "1");
            AssertEvaluatesFalse(context, "1", "<>", "1");
            AssertEvaluatesFalse(context, "1", "<", "0");
            AssertEvaluatesFalse(context, "2", ">", "4");
            AssertEvaluatesFalse(context, "1", ">=", "3");
            AssertEvaluatesFalse(context, "2", ">=", "4");
            AssertEvaluatesFalse(context, "1", "<=", "0");
            AssertEvaluatesFalse(context, "1", "<=", "0");
        }

        [Test]
        public void TestContainsWorksOnStrings()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);

            AssertEvaluatesTrue(context, "'bob'", "contains", "'o'");
            AssertEvaluatesTrue(context, "'bob'", "contains", "'b'");
            AssertEvaluatesTrue(context, "'bob'", "contains", "'bo'");
            AssertEvaluatesTrue(context, "'bob'", "contains", "'ob'");
            AssertEvaluatesTrue(context, "'bob'", "contains", "'bob'");

            AssertEvaluatesFalse(context, "'bob'", "contains", "'bob2'");
            AssertEvaluatesFalse(context, "'bob'", "contains", "'a'");
            AssertEvaluatesFalse(context, "'bob'", "contains", "'---'");
        }

        [Test]
        public void TestContainsWorksOnIntArrays()
        {
            // NOTE(daviburg): DotLiquid is in violation of explicit non-support of arrays for contains operators, quote:
            // "contains can only search strings. You cannot use it to check for an object in an array of objects."
            // https://shopify.github.io/liquid/basics/operators/
            // This is a rather harmless violation as all it does in generate useful output for a request which would fail
            // in the canonical Shopify implementation.
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            context["array"] = new[] { 1, 2, 3, 4, 5 };

            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "1");
            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "0");
            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "2");
            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "3");
            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "4");
            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "5");
            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "6");

            // NOTE(daviburg): Historically testing for equality cross integer and string boundaries resulted in not equal.
            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "'1'");
        }

        [Test]
        public void TestContainsWorksOnLongArrays()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            context["array"] = new long[] { 1, 2, 3, 4, 5 };

            AssertEvaluatesTrue(context, "array", "contains", "1");
            AssertEvaluatesFalse(context, "array", "contains", "0");
            AssertEvaluatesTrue(context, "array", "contains", "1.0");
            AssertEvaluatesTrue(context, "array", "contains", "2");
            AssertEvaluatesTrue(context, "array", "contains", "3");
            AssertEvaluatesTrue(context, "array", "contains", "4");
            AssertEvaluatesTrue(context, "array", "contains", "5");
            AssertEvaluatesFalse(context, "array", "contains", "6");

            AssertEvaluatesFalse(context, "array", "contains", "'1'");
        }

        [Test]
        public void TestStringArrays()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            var _array = new List<string>() { "Apple", "Orange", null, "Banana" };
            context["array"] = _array.ToArray();
            context["first"] = _array.First();
            context["last"] = _array.Last();

            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "'Apple'");
            AssertEvaluatesTrue(context, left: "array", op: "startsWith", right: "first");
            AssertEvaluatesTrue(context, left: "array.first", op: "==", right: "first");
            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "'apple'");
            AssertEvaluatesFalse(context, left: "array", op: "startsWith", right: "'apple'");
            AssertEvaluatesFalse(context, left: "array.first", op: "==", right: "'apple'");
            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "'Mango'");
            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "'Orange'");
            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "'Banana'");
            AssertEvaluatesTrue(context, left: "array", op: "endsWith", right: "last");
            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "'Orang'");
        }

        [Test]
        public void TestClassArrays()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            var _array = new List<Car>() { new Car() { Make = "Honda", Model = "Accord" }, new Car() { Make = "Ford", Model = "Explorer" } };
            context["array"] = _array.ToArray();
            context["first"] = _array.First();
            context["last"] = _array.Last();
            context["clone"] = new Car() { Make = "Honda", Model = "Accord" };
            context["camry"] = new Car() { Make = "Toyota", Model = "Camry" };

            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "first");
            AssertEvaluatesTrue(context,left: "array", op: "startsWith", right: "first");
            AssertEvaluatesTrue(context,left: "array.first", op: "==", right: "first");
            AssertEvaluatesTrue(context,left: "array", op: "contains", right: "clone");
            AssertEvaluatesTrue(context,left: "array", op: "startsWith", right: "clone");
            AssertEvaluatesTrue(context,left: "array", op: "endsWith", right: "last");
            AssertEvaluatesFalse(context,left: "array", op: "contains", right: "camry");
        }

        [Test]
        public void TestTruthyArray()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            var _array = new List<bool>() { true };
            context["array"] = _array.ToArray();
            context["first"] = _array.First();

            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "first");
            AssertEvaluatesTrue(context, left: "array", op: "startsWith", right: "first");
            AssertEvaluatesTrue(context, left: "array.first", op: "==", right: "'true'");
            AssertEvaluatesTrue(context, left: "array", op: "startsWith", right: "'true'");

            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "'true'"); // to be re-evaluated in #362
        }

        [Test]
        public void TestCharArrays()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            var _array = new List<char> { 'A', 'B', 'C' };
            context["array"] = _array.ToArray();
            context["first"] = _array.First();
            context["last"] = _array.Last();

            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "'A'");
            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "first");
            AssertEvaluatesTrue(context, left: "array", op: "startsWith", right: "first");
            AssertEvaluatesTrue(context, left: "array.first", op: "==", right: "first");
            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "'a'");
            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "'X'");
            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "'B'");
            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "'C'");
            AssertEvaluatesTrue(context, left: "array", op: "endsWith", right: "last");
        }

        [Test]
        public void TestByteArrays()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            var _array = new List<byte> { 0x01, 0x02, 0x03, 0x30 };
            context["array"] = _array.ToArray();
            context["first"] = _array.First();
            context["last"] = _array.Last();

            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "0");
            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "'0'");
            AssertEvaluatesTrue(context, left: "array", op: "startsWith", right: "first");
            AssertEvaluatesTrue(context, left: "array.first", op: "==", right: "first");
            AssertEvaluatesTrue(context, left: "array", op: "contains", right: "first");
            AssertEvaluatesFalse(context, left: "array", op: "contains", right: "1");
            AssertEvaluatesTrue(context, left: "array", op: "endsWith", right: "last");
        }

        [Test]
        public void TestContainsWorksOnDoubleArrays()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            context["array"] = new double[] { 1.0, 2.1, 3.25, 4.333, 5.0 };

            AssertEvaluatesTrue(context, "array", "contains", "1.0");
            AssertEvaluatesFalse(context, "array", "contains", "0");
            AssertEvaluatesTrue(context, "array", "contains", "2.1");
            AssertEvaluatesFalse(context, "array", "contains", "3");
            AssertEvaluatesFalse(context, "array", "contains", "4.33");
            AssertEvaluatesTrue(context, "array", "contains", "5.00");
            AssertEvaluatesFalse(context, "array", "contains", "6");

            AssertEvaluatesFalse(context, "array", "contains", "'1'");
        }

        [Test]
        public void TestContainsReturnsFalseForNilCommands()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            AssertEvaluatesFalse(context, "not_assigned", "contains", "0");
            AssertEvaluatesFalse(context, "0", "contains", "not_assigned");
        }

        [Test]
        public void TestStartsWithWorksOnStrings()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);

            AssertEvaluatesTrue(context, "'dave'", "startswith", "'d'");
            AssertEvaluatesTrue(context, "'dave'", "startswith", "'da'");
            AssertEvaluatesTrue(context, "'dave'", "startswith", "'dav'");
            AssertEvaluatesTrue(context, "'dave'", "startswith", "'dave'");

            AssertEvaluatesFalse(context, "'dave'", "startswith", "'ave'");
            AssertEvaluatesFalse(context, "'dave'", "startswith", "'e'");
            AssertEvaluatesFalse(context, "'dave'", "startswith", "'---'");
        }

        [Test]
        public void TestStartsWithWorksOnArrays()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            context["array"] = new[] { 1, 2, 3, 4, 5 };

            AssertEvaluatesFalse(context, "array", "startswith", "0");
            AssertEvaluatesTrue(context, "array", "startswith", "1");
        }

        [Test]
        public void TestStartsWithReturnsFalseForNilCommands()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            AssertEvaluatesFalse(context, "not_assigned", "startswith", "0");
            AssertEvaluatesFalse(context, "0", "startswith", "not_assigned");
        }

        [Test]
        public void TestEndsWithWorksOnStrings()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            AssertEvaluatesTrue(context,"'dave'", "endswith", "'e'");
            AssertEvaluatesTrue(context,"'dave'", "endswith", "'ve'");
            AssertEvaluatesTrue(context,"'dave'", "endswith", "'ave'");
            AssertEvaluatesTrue(context,"'dave'", "endswith", "'dave'");

            AssertEvaluatesFalse(context,"'dave'", "endswith", "'dav'");
            AssertEvaluatesFalse(context,"'dave'", "endswith", "'d'");
            AssertEvaluatesFalse(context,"'dave'", "endswith", "'---'");
        }

        [Test]
        public void TestEndsWithWorksOnArrays()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            context["array"] = new[] { 1, 2, 3, 4, 5 };

            AssertEvaluatesFalse(context,"array", "endswith", "0");
            AssertEvaluatesTrue(context,"array", "endswith", "5");
        }

        [Test]
        public void TestEndsWithReturnsFalseForNilCommands()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            AssertEvaluatesFalse(context,"not_assigned", "endswith", "0");
            AssertEvaluatesFalse(context,"0", "endswith", "not_assigned");
        }

        [Test]
        public void TestDictionaryHasKey()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            System.Collections.Generic.Dictionary<string, string> testDictionary = new System.Collections.Generic.Dictionary<string, string>
            {
                { "dave", "0" },
                { "bob", "4" }
            };
            context["dictionary"] = testDictionary;

            AssertEvaluatesTrue(context,"dictionary", "haskey", "'bob'");
            AssertEvaluatesFalse(context,"dictionary", "haskey", "'0'");
        }

        [Test]
        public void TestDictionaryHasValue()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            System.Collections.Generic.Dictionary<string, string> testDictionary = new System.Collections.Generic.Dictionary<string, string>
            {
                { "dave", "0" },
                { "bob", "4" }
            };
            context["dictionary"] = testDictionary;

            AssertEvaluatesTrue(context,"dictionary", "hasvalue", "'0'");
            AssertEvaluatesFalse(context,"dictionary", "hasvalue", "'bob'");
        }

        [Test]
        public void TestOrCondition()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);

            Condition condition = new Condition("1", "==", "2");
            Assert.IsFalse(condition.Evaluate(context, CultureInfo.InvariantCulture));

            condition.Or(new Condition("2", "==", "1"));
            Assert.IsFalse(condition.Evaluate(context, CultureInfo.InvariantCulture));

            condition.Or(new Condition("1", "==", "1"));
            Assert.IsTrue(condition.Evaluate(context, CultureInfo.InvariantCulture));
        }

        [Test]
        public void TestAndCondition()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);

            Condition condition = new Condition("1", "==", "1");
            Assert.IsTrue(condition.Evaluate(context, CultureInfo.InvariantCulture));

            condition.And(new Condition("2", "==", "2"));
            Assert.IsTrue(condition.Evaluate(context, CultureInfo.InvariantCulture));

            condition.And(new Condition("2", "==", "1"));
            Assert.IsFalse(condition.Evaluate(context, CultureInfo.InvariantCulture));
        }

        [Test]
        public void TestShouldAllowCustomProcOperator()
        {
            try
            {
                var context = new Context(CultureInfo.InvariantCulture, NamingConvention);

                Condition.Operators["starts_with"] =
                    (left, right) => Regex.IsMatch(left.ToString(), string.Format("^{0}", right.ToString()));

                AssertEvaluatesTrue(context,"'bob'", "starts_with", "'b'");
                AssertEvaluatesFalse(context,"'bob'", "starts_with", "'o'");
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
                var context = new Context(CultureInfo.InvariantCulture, NamingConvention);

                Condition.Operators["IsMultipleOf"] =
                    (left, right) => (int)left % (int)right == 0;

                // exact match
                AssertEvaluatesTrue(context,"16", "IsMultipleOf", "4");
                AssertEvaluatesTrue(context,"2147483646", "IsMultipleOf", "2");
                AssertError(context,"2147483648", "IsMultipleOf", "2", typeof(System.InvalidCastException));
                AssertEvaluatesFalse(context,"16", "IsMultipleOf", "5");

                // lower case: compatibility
                AssertEvaluatesTrue(context,"16", "ismultipleof", "4");
                AssertEvaluatesFalse(context,"16", "ismultipleof", "5");

                AssertEvaluatesTrue(context,"16", "is_multiple_of", "4");
                AssertEvaluatesFalse(context,"16", "is_multiple_of", "5");

                // camel case : incompatible
                AssertError(context,"16", "isMultipleOf", "4", typeof(ArgumentException));

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
                var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
                Condition.Operators["IsMultipleOff"] =
                    (left, right) => System.Convert.ToInt64(left) % System.Convert.ToInt64(right) == 0;

                // exact match
                AssertEvaluatesTrue(context,"16", "IsMultipleOff", "4");
                AssertEvaluatesTrue(context,"2147483646", "IsMultipleOff", "2");
                AssertEvaluatesTrue(context,"2147483648", "IsMultipleOff", "2");
                AssertEvaluatesFalse(context,"16", "IsMultipleOff", "5");

                // lower case: compatibility
                AssertEvaluatesTrue(context,"16", "ismultipleoff", "4");
                AssertEvaluatesFalse(context,"16", "ismultipleoff", "5");

                AssertEvaluatesTrue(context,"16", "is_multiple_off", "4");
                AssertEvaluatesFalse(context,"16", "is_multiple_off", "5");

                // camel case : incompatible
                AssertError(context,"16", "isMultipleOff", "4", typeof(ArgumentException));

                //Run tests through the template to verify that capitalization rules are followed through template parsing
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 IsMultipleOff 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("", "{% if 14 IsMultipleOff 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 ismultipleoff 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("", "{% if 14 ismultipleoff 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 is_multiple_off 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("", "{% if 14 is_multiple_off 4 %} TRUE {% endif %}");
                Helper.AssertTemplateResult("Liquid error: Unknown operator isMultipleOff", "{% if 16 isMultipleOff 4 %} TRUE {% endif %}");
            }
            finally
            {
                Condition.Operators.Remove("IsMultipleOff");
            }
        }

        [Test]
        public void TestCapitalInCustomCSharpOperatorInt()
        {
            var namingConvention = new CSharpNamingConvention();
            try
            {
                var context = new Context(CultureInfo.InvariantCulture, namingConvention);
                Condition.Operators["DivisibleBy"] =
                    (left, right) => (int)left % (int)right == 0;

                // exact match
                AssertEvaluatesTrue(context, "16", "DivisibleBy", "4");
                AssertEvaluatesTrue(context, "2147483646", "DivisibleBy", "2");
                AssertError(context, "2147483648", "DivisibleBy", "2", typeof(System.InvalidCastException));
                AssertEvaluatesFalse(context, "16", "DivisibleBy", "5");

                // lower case: compatibility
                AssertEvaluatesTrue(context, "16", "divisibleby", "4");
                AssertEvaluatesFalse(context, "16", "divisibleby", "5");

                // camel case : compatibility
                AssertEvaluatesTrue(context, "16", "divisibleBy", "4");
                AssertEvaluatesFalse(context, "16", "divisibleBy", "5");

                // snake case : incompatible
                AssertError(context, "16", "divisible_by", "4", typeof(ArgumentException));

                //Run tests through the template to verify that capitalization rules are followed through template parsing
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 DivisibleBy 4 %} TRUE {% endif %}", namingConvention);
                Helper.AssertTemplateResult("", "{% if 16 DivisibleBy 5 %} TRUE {% endif %}", namingConvention);
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 divisibleby 4 %} TRUE {% endif %}", namingConvention);
                Helper.AssertTemplateResult("", "{% if 16 divisibleby 5 %} TRUE {% endif %}", namingConvention);
                Helper.AssertTemplateResult("Liquid error: Unknown operator divisible_by", "{% if 16 divisible_by 4 %} TRUE {% endif %}", namingConvention);
            }
            finally
            {
                Condition.Operators.Remove("DivisibleBy");
            }
        }

        [Test]
        public void TestCapitalInCustomCSharpOperatorLong()
        {
            var namingConvention = new CSharpNamingConvention();

            try
            {
                var context = new Context(CultureInfo.InvariantCulture, namingConvention);
                Condition.Operators["DivisibleByy"] =
                    (left, right) => System.Convert.ToInt64(left) % System.Convert.ToInt64(right) == 0;

                // exact match
                AssertEvaluatesTrue(context, "16", "DivisibleByy", "4");
                AssertEvaluatesTrue(context, "2147483646", "DivisibleByy", "2");
                AssertEvaluatesTrue(context, "2147483648", "DivisibleByy", "2");
                AssertEvaluatesFalse(context,"16", "DivisibleByy", "5");

                // lower case: compatibility
                AssertEvaluatesTrue(context,"16", "divisiblebyy", "4");
                AssertEvaluatesFalse(context,"16", "divisiblebyy", "5");

                // camel case: compatibility
                AssertEvaluatesTrue(context,"16", "divisibleByy", "4");
                AssertEvaluatesFalse(context,"16", "divisibleByy", "5");

                // snake case: incompatible
                AssertError(context,"16", "divisible_byy", "4", typeof(ArgumentException));

                //Run tests through the template to verify that capitalization rules are followed through template parsing
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 DivisibleByy 4 %} TRUE {% endif %}", namingConvention);
                Helper.AssertTemplateResult("", "{% if 16 DivisibleByy 5 %} TRUE {% endif %}", namingConvention);
                Helper.AssertTemplateResult(" TRUE ", "{% if 16 divisiblebyy 4 %} TRUE {% endif %}", namingConvention);
                Helper.AssertTemplateResult("", "{% if 16 divisiblebyy 5 %} TRUE {% endif %}", namingConvention);
                Helper.AssertTemplateResult("Liquid error: Unknown operator divisible_byy", "{% if 16 divisible_byy 4 %} TRUE {% endif %}", namingConvention);
            }
            finally
            {
                Condition.Operators.Remove("DivisibleByy");
            }

        }

        [Test]
        public void TestLessThanDecimal()
        {
            var model = new { value = new decimal(-10.5) };

            string output = Template.Parse("{% if model.value < 0 %}passed{% endif %}", NamingConvention)
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
            var parse = DotLiquid.Template.Parse(current, NamingConvention);
            var parsedOutput = parse.Render(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = Hash.FromDictionary(row) });
            Assert.AreEqual("MyID is 1", parsedOutput);
        }

        [Test]
        public void TestShouldAllowCustomProcOperatorCapitalized()
        {
            try
            {
                var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
                Condition.Operators["StartsWith"] =
                    (left, right) => Regex.IsMatch(left.ToString(), string.Format("^{0}", right.ToString()));

                Helper.AssertTemplateResult("", "{% if 'bob' StartsWith 'B' %} YES {% endif %}", null, new CSharpNamingConvention());
                AssertEvaluatesTrue(context, "'bob'", "StartsWith", "'b'");
                AssertEvaluatesFalse(context, "'bob'", "StartsWith", "'o'");
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
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            context["enum"] = TestEnum.Yes;

            AssertEvaluatesTrue(context, "enum", "==", "'Yes'");
            AssertEvaluatesTrue(context, "enum", "!=", "'No'");

            AssertEvaluatesFalse(context, "enum", "==", "'No'");
            AssertEvaluatesFalse(context, "enum", "!=", "'Yes'");
        }

        [Test]
        public void TestBlankObject()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            context["dictionary"] = new Dictionary<string, string> { { "abc", "xyz" } };
            context["empty_dictionary"] = new Dictionary<string, string> { };
            context["list"] = new List<string> { "abc" };
            context["empty_list"] = new List<string> { };
            context["array"] = new string[] { "foo" };
            context["empty_array"] = new string[] { };
            context["a_drop"] = new DummyDrop();

            // self check
            AssertEvaluatesFalse(context, left: "blank", op: "==", right: "blank");
            AssertEvaluatesTrue(context, left: "blank", op: "!=", right: "blank");
            AssertEvaluatesTrue(context, left: "blank", op: "<>", right: "blank");

            // blank truthy
            AssertEvaluatesTrue(context, left: "''", op: "==", right: "blank");
            AssertEvaluatesTrue(context, left: "'  '", op: "==", right: "blank");
            AssertEvaluatesTrue(context, left: "false", op: "==", right: "blank");
            AssertEvaluatesTrue(context, left: "nil", op: "==", right: "blank");
            AssertEvaluatesTrue(context, left: "not_assigned", op: "==", right: "blank");
            AssertEvaluatesTrue(context, left: "empty_dictionary", op: "==", right: "blank");
            AssertEvaluatesTrue(context, left: "empty_list", op: "==", right: "blank");
            AssertEvaluatesTrue(context, left: "empty_array", op: "==", right: "blank");

            // blank falsy
            AssertEvaluatesTrue(context, left: "1", op: "!=", right: "blank");
            AssertEvaluatesTrue(context, left: "0", op: "!=", right: "blank");
            AssertEvaluatesTrue(context, left: "true", op: "!=", right: "blank");
            AssertEvaluatesTrue(context, left: "a_drop", op: "!=", right: "blank");
            AssertEvaluatesTrue(context, left: "dictionary", op: "!=", right: "blank");
            AssertEvaluatesTrue(context, left: "list", op: "!=", right: "blank");
            AssertEvaluatesTrue(context, left: "array", op: "!=", right: "blank");
        }

        [Test]
        public void TestEmptyObject()
        {
            var context = new Context(CultureInfo.InvariantCulture, NamingConvention);
            context["dictionary"] = new Dictionary<string, string> { { "abc", "xyz" } };
            context["empty_dictionary"] = new Dictionary<string, string> { };
            context["list"] = new List<string> { "abc" };
            context["empty_list"] = new List<string> { };
            context["array"] = new string[] { "foo" };
            context["empty_array"] = new string[] { };
            context["a_drop"] = new DummyDrop();

            // self check
            AssertEvaluatesFalse(context, left: "empty", op: "==", right: "empty");
            AssertEvaluatesTrue(context, left: "empty", op: "!=", right: "empty");
            AssertEvaluatesTrue(context, left: "empty", op: "<>", right: "empty");

            // empty truthy
            AssertEvaluatesTrue(context, left: "''", op: "==", right: "empty");
            AssertEvaluatesTrue(context, left: "empty_dictionary", op: "==", right: "empty");
            AssertEvaluatesTrue(context, left: "empty_list", op: "==", right: "empty");
            AssertEvaluatesTrue(context, left: "empty_array", op: "==", right: "empty");

            // empty falsy
            AssertEvaluatesTrue(context, left: "'  '", op: "!=", right: "empty");
            AssertEvaluatesTrue(context, left: "false", op: "!=", right: "empty");
            AssertEvaluatesTrue(context, left: "nil", op: "!=", right: "empty");
            AssertEvaluatesTrue(context, left: "no_assigned", op: "!=", right: "empty");
            AssertEvaluatesTrue(context, left: "1", op: "!=", right: "empty");
            AssertEvaluatesTrue(context, left: "0", op: "!=", right: "empty");
            AssertEvaluatesTrue(context, left: "true", op: "!=", right: "empty");
            AssertEvaluatesTrue(context, left: "a_drop", op: "!=", right: "empty");
            AssertEvaluatesTrue(context, left: "dictionary", op: "!=", right: "empty");
            AssertEvaluatesTrue(context, left: "list", op: "!=", right: "empty");
            AssertEvaluatesTrue(context, left: "array", op: "!=", right: "empty");
        }

        #region Helper methods

        private void AssertEvaluatesTrue(Context context, string left, string op, string right)
        {
            Assert.IsTrue(new Condition(left, op, right).Evaluate(context, CultureInfo.InvariantCulture),
                "Evaluated false: {0} {1} {2}", left, op, right);
        }

        private void AssertEvaluatesFalse(Context context, string left, string op, string right)
        {
            Assert.IsFalse(new Condition(left, op, right).Evaluate(context, CultureInfo.InvariantCulture),
                "Evaluated true: {0} {1} {2}", left, op, right);
        }

        private void AssertError(Context context, string left, string op, string right, System.Type errorType)
        {
            Assert.Throws(errorType, () => new Condition(left, op, right).Evaluate(context, CultureInfo.InvariantCulture));
        }

        #endregion
    }
}
