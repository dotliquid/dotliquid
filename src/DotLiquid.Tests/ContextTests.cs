using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using DotLiquid.Exceptions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ContextTests
    {
        #region Classes used in tests

        private static class TestFilters
        {
            public static string Hi(string output)
            {
                return output + " hi!";
            }
        }

        private static class TestContextFilters
        {
            public static string Hi(Context context, string output)
            {
                return output + " hi from " + context["name"] + "!";
            }
        }

        private static class GlobalFilters
        {
            public static string Notice(string output)
            {
                return "Global " + output;
            }
        }

        private static class LocalFilters
        {
            public static string Notice(string output)
            {
                return "Local " + output;
            }
        }

        private class HundredCents : ILiquidizable
        {
            public object ToLiquid()
            {
                return 100;
            }
        }

        private class CentsDrop : Drop
        {
            public object Amount
            {
                get { return new HundredCents(); }
            }

            public bool NonZero
            {
                get { return true; }
            }
        }

        private class ContextSensitiveDrop : Drop
        {
            public object Test()
            {
                return Context["test"];
            }
        }

        private class Category : Drop
        {
            public string Name { get; set; }

            public Category(string name)
            {
                Name = name;
            }

            public override object ToLiquid()
            {
                return new CategoryDrop(this);
            }
        }

        private class CategoryDrop : IContextAware
        {
            public Category Category { get; set; }
            public Context Context { get; set; }

            public CategoryDrop(Category category)
            {
                Category = category;
            }
        }

        private class CounterDrop : Drop
        {
            private int _count;

            public int Count()
            {
                return ++_count;
            }
        }

        private class ArrayLike : ILiquidizable
        {
            private Dictionary<int, int> _counts = new Dictionary<int, int>();

            public object Fetch(int index)
            {
                return null;
            }

            public object this[int index]
            {
                get
                {
                    _counts[index] += 1;
                    return _counts[index];
                }
            }

            public object ToLiquid()
            {
                return this;
            }
        }

        private class IndexableLiquidizable : IIndexable, ILiquidizable
        {
            private const string theKey = "thekey";

            public object this[object key] => key as string == theKey ? new LiquidizableList() : null;

            public bool ContainsKey(object key)
            {
                return key as string == theKey;
            }

            public object ToLiquid()
            {
                return this;
            }
        }

        private class LiquidizableList : ILiquidizable
        {
            public object ToLiquid()
            {
                return new List<string>(new[] { "text1", "text2" });
            }
        }

        private class ExpandoModel
        {
            public int IntProperty { get; set; }
            public string StringProperty { get; set; }
            public ExpandoObject Properties { get; set; }
        }

        #endregion

        private Context _context;
        private Context _contextV22;

        [OneTimeSetUp]
        public void SetUp()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            _contextV22 = new Context(CultureInfo.InvariantCulture)
            {
                SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22
            };
        }

        [Test]
        public void TestVariables()
        {
            _context["string"] = "string";
            Assert.AreEqual("string", _context["string"]);

            _context["EscapedCharacter"] = "EscapedCharacter\"";
            Assert.AreEqual("EscapedCharacter\"", _context["EscapedCharacter"]);

            _context["num"] = 5;
            Assert.AreEqual(5, _context["num"]);

            _context["decimal"] = 5m;
            Assert.AreEqual(5m, _context["decimal"]);

            _context["float"] = 5.0f;
            Assert.AreEqual(5.0f, _context["float"]);

            _context["double"] = 5.0;
            Assert.AreEqual(5.0, _context["double"]);

            _context["time"] = TimeSpan.FromDays(1);
            Assert.AreEqual(TimeSpan.FromDays(1), _context["time"]);

            _context["date"] = DateTime.Today;
            Assert.AreEqual(DateTime.Today, _context["date"]);

            DateTime now = DateTime.Now;
            _context["datetime"] = now;
            Assert.AreEqual(now, _context["datetime"]);

            DateTimeOffset offset = new DateTimeOffset(2013, 9, 10, 0, 10, 32, new TimeSpan(1, 0, 0));
            _context["datetimeoffset"] = offset;
            Assert.AreEqual(offset, _context["datetimeoffset"]);

            Guid guid = Guid.NewGuid();
            _context["guid"] = guid;
            Assert.AreEqual(guid, _context["guid"]);

            _context["bool"] = true;
            Assert.AreEqual(true, _context["bool"]);

            _context["bool"] = false;
            Assert.AreEqual(false, _context["bool"]);

            _context["nil"] = null;
            Assert.AreEqual(null, _context["nil"]);
            Assert.AreEqual(null, _context["nil"]);
        }

        private enum TestEnum { Yes, No }

        [Test]
        public void TestGetVariable_Enum()
        {
            _context["yes"] = TestEnum.Yes;
            _context["no"] = TestEnum.No;
            _context["not_enum"] = TestEnum.Yes.ToString();

            Assert.AreEqual(TestEnum.Yes, _context["yes"]);
            Assert.AreEqual(TestEnum.No, _context["no"]);
            Assert.AreNotEqual(TestEnum.Yes, _context["not_enum"]);
        }

        [Test]
        public void TestVariablesNotExisting()
        {
            Assert.AreEqual(null, _context["does_not_exist"]);
        }

        [Test]
        public void TestVariableNotFoundErrors()
        {
            Template template = Template.Parse("{{ does_not_exist }}");
            string rendered = template.Render();

            Assert.AreEqual("", rendered);
            Assert.AreEqual(1, template.Errors.Count);
            Assert.AreEqual(string.Format(Liquid.ResourceManager.GetString("VariableNotFoundException"), "does_not_exist"), template.Errors[0].Message);
        }

        [Test]
        public void TestVariableNotFoundFromAnonymousObject()
        {
            Template template = Template.Parse("{{ first.test }}{{ second.test }}");
            string rendered = template.Render(Hash.FromAnonymousObject(new { second = new { foo = "hi!" } }));

            Assert.AreEqual("", rendered);
            Assert.AreEqual(2, template.Errors.Count);
            Assert.AreEqual(string.Format(Liquid.ResourceManager.GetString("VariableNotFoundException"), "first.test"), template.Errors[0].Message);
            Assert.AreEqual(string.Format(Liquid.ResourceManager.GetString("VariableNotFoundException"), "second.test"), template.Errors[1].Message);
        }

        [Test]
        public void TestVariableNotFoundException()
        {
            Assert.DoesNotThrow(() => Template.Parse("{{ does_not_exist }}").Render(new RenderParameters(CultureInfo.InvariantCulture)
            {
                RethrowErrors = true
            }));
        }

        [Test]
        public void TestVariableNotFoundExceptionIgnoredForIfStatement()
        {
            Template template = Template.Parse("{% if does_not_exist %}abc{% endif %}");
            string rendered = template.Render();

            Assert.AreEqual("", rendered);
            Assert.AreEqual(0, template.Errors.Count);
        }

        [Test]
        public void TestVariableNotFoundExceptionIgnoredForUnlessStatement()
        {
            Template template = Template.Parse("{% unless does_not_exist %}abc{% endunless %}");
            string rendered = template.Render();

            Assert.AreEqual("abc", rendered);
            Assert.AreEqual(0, template.Errors.Count);
        }

        [Test]
        public void TestScoping()
        {
            Assert.DoesNotThrow(() =>
            {
                _context.Push(null);
                _context.Pop();
            });

            Assert.Throws<ContextException>(() => _context.Pop());

            Assert.Throws<ContextException>(() =>
            {
                _context.Push(null);
                _context.Pop();
                _context.Pop();
            });
        }

        [Test]
        public void TestLengthQuery()
        {
            _context["numbers"] = new[] { 1, 2, 3, 4 };
            Assert.AreEqual(4, _context["numbers.size"]);

            _context["numbers"] = new Dictionary<int, int>
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 },
                { 4, 4 }
            };
            Assert.AreEqual(4, _context["numbers.size"]);

            _context["numbers"] = new Dictionary<object, int>
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 },
                { 4, 4 },
                { "size", 1000 }
            };
            Assert.AreEqual(1000, _context["numbers.size"]);
        }

        [Test]
        public void TestHyphenatedVariable()
        {
            _context["oh-my"] = "godz";
            Assert.AreEqual("godz", _context["oh-my"]);
        }

        [Test]
        public void TestAddFilter()
        {
            Context context = new Context(CultureInfo.InvariantCulture);
            context.AddFilters(new[] { typeof(TestFilters) });
            Assert.AreEqual("hi? hi!", context.Invoke("hi", new List<object> { "hi?" }));
            context.SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22;
            Assert.AreEqual("hi? hi!", context.Invoke("hi", new List<object> { "hi?" }));

            context = new Context(CultureInfo.InvariantCulture);
            Assert.AreEqual("hi?", context.Invoke("hi", new List<object> { "hi?" }));
            context.SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22;
            Assert.Throws<FilterNotFoundException>(() => context.Invoke("hi", new List<object> { "hi?" }));
        }

        [Test]
        public void TestAddContextFilter()
        {
            // This test differs from TestAddFilter only in that the Hi method within this class has a Context parameter in addition to the input string
            Context context = new Context(CultureInfo.InvariantCulture) { SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid20 };
            context["name"] = "King Kong";

            context.AddFilters(new[] { typeof(TestContextFilters) });
            Assert.AreEqual("hi? hi from King Kong!", context.Invoke("hi", new List<object> { "hi?" }));
            context.SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22;
            Assert.AreEqual("hi? hi from King Kong!", context.Invoke("hi", new List<object> { "hi?" }));

            context = new Context(CultureInfo.InvariantCulture) { SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid20 };
            Assert.AreEqual("hi?", context.Invoke("hi", new List<object> { "hi?" }));
            context.SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22;
            Assert.Throws<FilterNotFoundException>(() => context.Invoke("hi", new List<object> { "hi?" }));
        }

        [Test]
        public void TestOverrideGlobalFilter()
        {
            Template.RegisterFilter(typeof(GlobalFilters));
            Assert.AreEqual("Global test", Template.Parse("{{'test' | notice }}").Render());
            Assert.AreEqual("Local test", Template.Parse("{{'test' | notice }}").Render(new RenderParameters(CultureInfo.InvariantCulture) { Filters = new[] { typeof(LocalFilters) } }));
        }

        [Test]
        public void TestOnlyIntendedFiltersMakeItThere()
        {
            Context context = new Context(CultureInfo.InvariantCulture);
            var methodsBefore = context.Strainer.Methods.Select(mi => mi.Name).ToList();
            context.AddFilters(new[] { typeof(TestFilters) });
            var methodsAfter = context.Strainer.Methods.Select(mi => mi.Name).ToList();
            CollectionAssert.AreEqual(
                methodsBefore.Concat(new[] { "Hi" }).OrderBy(s => s).ToList(),
                methodsAfter.OrderBy(s => s).ToList());
        }

        [Test]
        public void TestAddItemInOuterScope()
        {
            _context["test"] = "test";
            _context.Push(new Hash());
            Assert.AreEqual("test", _context["test"]);
            _context.Pop();
            Assert.AreEqual("test", _context["test"]);
        }

        [Test]
        public void TestAddItemInInnerScope()
        {
            _context.Push(new Hash());
            _context["test"] = "test";
            Assert.AreEqual("test", _context["test"]);
            _context.Pop();
            Assert.AreEqual(null, _context["test"]);
        }

        [Test]
        public void TestHierarchicalData()
        {
            _context["hash"] = new { name = "tobi" };
            Assert.AreEqual("tobi", _context["hash.name"]);
            Assert.AreEqual("tobi", _context["hash['name']"]);
        }

        [Test]
        public void TestKeywords()
        {
            Assert.AreEqual(true, _context["true"]);
            Assert.AreEqual(false, _context["false"]);
        }

        [Test]
        public void TestDigits()
        {
            Assert.AreEqual(100, _context["100"]);
            Assert.AreEqual(100.00, _context[string.Format("100{0}00", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)]);
        }

        [Test]
        public void TestStrings()
        {
            Assert.AreEqual("hello!", _context["'hello!'"]);
            Assert.AreEqual("hello!", _context["'hello!'"]);
        }

        [Test]
        public void TestMerge()
        {
            _context.Merge(new Hash { { "test", "test" } });
            Assert.AreEqual("test", _context["test"]);
            _context.Merge(new Hash { { "test", "newvalue" }, { "foo", "bar" } });
            Assert.AreEqual("newvalue", _context["test"]);
            Assert.AreEqual("bar", _context["foo"]);
        }

        [Test]
        public void TestArrayNotation()
        {
            _context["test"] = new[] { 1, 2, 3, 4, 5 };

            Assert.AreEqual(1, _context["test[0]"]);
            Assert.AreEqual(2, _context["test[1]"]);
            Assert.AreEqual(3, _context["test[2]"]);
            Assert.AreEqual(4, _context["test[3]"]);
            Assert.AreEqual(5, _context["test[4]"]);
        }

        [Test]
        public void TestRecursiveArrayNotation()
        {
            _context["test"] = new { test = new[] { 1, 2, 3, 4, 5 } };

            Assert.AreEqual(1, _context["test.test[0]"]);

            _context["test"] = new[] { new { test = "worked" } };

            Assert.AreEqual("worked", _context["test[0].test"]);
        }

        [Test]
        public void TestHashToArrayTransition()
        {
            _context["colors"] = new
            {
                Blue = new[] { "003366", "336699", "6699CC", "99CCFF" },
                Green = new[] { "003300", "336633", "669966", "99CC99" },
                Yellow = new[] { "CC9900", "FFCC00", "FFFF99", "FFFFCC" },
                Red = new[] { "660000", "993333", "CC6666", "FF9999" }
            };

            Assert.AreEqual("003366", _context["colors.Blue[0]"]);
            Assert.AreEqual("FF9999", _context["colors.Red[3]"]);
        }

        [Test]
        public void TestFirstLastSize()
        {
            _context["test"] = new[] { 1, 2, 3, 4, 5 };

            Assert.AreEqual(1, _context["test.first"]);
            Assert.AreEqual(5, _context["test.last"]);
            Assert.AreEqual(5, _context["test.size"]);

            _context["test"] = new { test = new[] { 1, 2, 3, 4, 5 } };

            Assert.AreEqual(1, _context["test.test.first"]);
            Assert.AreEqual(5, _context["test.test.last"]);
            Assert.AreEqual(5, _context["test.test.size"]);

            _context["test"] = new[] { 1 };

            Assert.AreEqual(1, _context["test.first"]);
            Assert.AreEqual(1, _context["test.last"]);
            Assert.AreEqual(1, _context["test.size"]);
        }

        [Test]
        public void TestAccessHashesWithHashNotation()
        {
            _context["products"] = new { count = 5, tags = new[] { "deepsnow", "freestyle" } };
            _context["product"] = new { variants = new[] { new { title = "draft151cm" }, new { title = "element151cm" } } };

            Assert.AreEqual(5, _context["products[\"count\"]"]);
            Assert.AreEqual("deepsnow", _context["products['tags'][0]"]);
            Assert.AreEqual("freestyle", _context["products['tags'][1]"]);
            Assert.AreEqual("freestyle", _context["products['tags'][-1]"]);
            Assert.AreEqual("deepsnow", _context["products['tags'][-2]"]);
            Assert.AreEqual("deepsnow", _context["products['tags'].first"]);
            Assert.AreEqual("freestyle", _context["products['tags'].last"]);
            Assert.AreEqual(2, _context["products['tags'].size"]);
            Assert.AreEqual("draft151cm", _context["product['variants'][0][\"title\"]"]);
            Assert.AreEqual("element151cm", _context["product['variants'][1]['title']"]);
            Assert.AreEqual("draft151cm", _context["product['variants'][0]['title']"]);
            Assert.AreEqual("element151cm", _context["product['variants'].last['title']"]);
        }

        [Test]
        public void TestAccessVariableWithHashNotation()
        {
            _context["foo"] = "baz";
            _context["bar"] = "foo";

            Assert.AreEqual("baz", _context["[\"foo\"]"]);
            Assert.AreEqual("baz", _context["[bar]"]);
        }

        [Test]
        public void TestAccessHashesWithHashAccessVariables()
        {
            _context["var"] = "tags";
            _context["nested"] = new { var = "tags" };
            _context["products"] = new { count = 5, tags = new[] { "deepsnow", "freestyle" } };

            Assert.AreEqual("deepsnow", _context["products[var].first"]);
            Assert.AreEqual("freestyle", _context["products[nested.var].last"]);
        }

        [Test]
        public void TestHashNotationOnlyForHashAccess()
        {
            _context["array"] = new[] { 1, 2, 3, 4, 5 };
            _context["hash"] = new { first = "Hello" };

            Assert.AreEqual(1, _context["array.first"]);
            Assert.AreEqual(null, _context["array['first']"]);
            Assert.AreEqual("Hello", _context["hash['first']"]);
        }

        [Test]
        public void TestFirstCanAppearInMiddleOfCallchain()
        {
            _context["product"] = new { variants = new[] { new { title = "draft151cm" }, new { title = "element151cm" } } };

            Assert.AreEqual("draft151cm", _context["product.variants[0].title"]);
            Assert.AreEqual("element151cm", _context["product.variants[1].title"]);
            Assert.AreEqual("draft151cm", _context["product.variants.first.title"]);
            Assert.AreEqual("element151cm", _context["product.variants.last.title"]);
        }

        [Test]
        public void TestCents()
        {
            _context.Merge(Hash.FromAnonymousObject(new { cents = new HundredCents() }));
            Assert.AreEqual(100, _context["cents"]);
        }

        [Test]
        public void TestNestedCents()
        {
            _context.Merge(Hash.FromAnonymousObject(new { cents = new { amount = new HundredCents() } }));
            Assert.AreEqual(100, _context["cents.amount"]);

            _context.Merge(Hash.FromAnonymousObject(new { cents = new { cents = new { amount = new HundredCents() } } }));
            Assert.AreEqual(100, _context["cents.cents.amount"]);
        }

        [Test]
        public void TestCentsThroughDrop()
        {
            _context.Merge(Hash.FromAnonymousObject(new { cents = new CentsDrop() }));
            Assert.AreEqual(100, _context["cents.amount"]);
        }

        [Test]
        public void TestNestedCentsThroughDrop()
        {
            _context.Merge(Hash.FromAnonymousObject(new { vars = new { cents = new CentsDrop() } }));
            Assert.AreEqual(100, _context["vars.cents.amount"]);
        }

        [Test]
        public void TestDropMethodsWithQuestionMarks()
        {
            _context.Merge(Hash.FromAnonymousObject(new { cents = new CentsDrop() }));
            Assert.AreEqual(true, _context["cents.non_zero"]);
        }

        [Test]
        public void TestContextFromWithinDrop()
        {
            _context.Merge(Hash.FromAnonymousObject(new { test = "123", vars = new ContextSensitiveDrop() }));
            Assert.AreEqual("123", _context["vars.test"]);
        }

        [Test]
        public void TestNestedContextFromWithinDrop()
        {
            _context.Merge(Hash.FromAnonymousObject(new { test = "123", vars = new { local = new ContextSensitiveDrop() } }));
            Assert.AreEqual("123", _context["vars.local.test"]);
        }

        [Test]
        public void TestRanges()
        {
            _context.Merge(Hash.FromAnonymousObject(new { test = 5 }));
            CollectionAssert.AreEqual(Enumerable.Range(1, 5), _context["(1..5)"] as IEnumerable);
            CollectionAssert.AreEqual(Enumerable.Range(1, 5), _context["(1..test)"] as IEnumerable);
            CollectionAssert.AreEqual(Enumerable.Range(5, 1), _context["(test..test)"] as IEnumerable);
        }

        [Test]
        public void TestCentsThroughDropNestedly()
        {
            _context.Merge(Hash.FromAnonymousObject(new { cents = new { cents = new CentsDrop() } }));
            Assert.AreEqual(100, _context["cents.cents.amount"]);

            _context.Merge(Hash.FromAnonymousObject(new { cents = new { cents = new { cents = new CentsDrop() } } }));
            Assert.AreEqual(100, _context["cents.cents.cents.amount"]);
        }

        [Test]
        public void TestDropWithVariableCalledOnlyOnce()
        {
            _context["counter"] = new CounterDrop();

            Assert.AreEqual(1, _context["counter.count"]);
            Assert.AreEqual(2, _context["counter.count"]);
            Assert.AreEqual(3, _context["counter.count"]);
        }

        [Test]
        public void TestDropWithKeyOnlyCalledOnce()
        {
            _context["counter"] = new CounterDrop();

            Assert.AreEqual(1, _context["counter['count']"]);
            Assert.AreEqual(2, _context["counter['count']"]);
            Assert.AreEqual(3, _context["counter['count']"]);
        }

        [Test]
        public void TestSimpleVariablesRendering()
        {
            Helper.AssertTemplateResult(
                expected: "string",
                template: "{{context}}",
                localVariables: Hash.FromAnonymousObject(new { context = "string" }));

            Helper.AssertTemplateResult(
                expected: "EscapedCharacter\"",
                template: "{{context}}",
                localVariables: Hash.FromAnonymousObject(new { context = "EscapedCharacter\"" }));

            Helper.AssertTemplateResult(
                expected: "5",
                template: "{{context}}",
                localVariables: Hash.FromAnonymousObject(new { context = 5 }));

            Helper.AssertTemplateResult(
                expected: "5",
                template: "{{context}}",
                localVariables: Hash.FromAnonymousObject(new { context = 5m }));

            Helper.AssertTemplateResult(
                expected: "5",
                template: "{{context}}",
                localVariables: Hash.FromAnonymousObject(new { context = 5.0f }));

            Helper.AssertTemplateResult(
                expected: "5",
                template: "{{context}}",
                localVariables: Hash.FromAnonymousObject(new { context = 5.0 }));

            Helper.AssertTemplateResult(
                expected: "1.00:00:00",
                template: "{{context}}",
                localVariables: Hash.FromAnonymousObject(new { context = TimeSpan.FromDays(1) }));

            // The expected values are expressed in en-US, so ensure the template runs with that Culture.
            using (CultureHelper.SetCulture("en-US"))
            {
                Helper.AssertTemplateResult(
                    expected: "1/1/0001 12:00:00 AM",
                    template: "{{context}}",
                    localVariables: Hash.FromAnonymousObject(new { context = DateTime.MinValue }));

                Helper.AssertTemplateResult(
                    expected: "9/10/2013 12:10:32 AM +01:00",
                    template: "{{context}}",
                    localVariables: Hash.FromAnonymousObject(new { context = new DateTimeOffset(2013, 9, 10, 0, 10, 32, new TimeSpan(1, 0, 0)) }));
            }

            Helper.AssertTemplateResult(
                expected: "d0f28a51-9393-4658-af0b-8c4b4c5c31ff",
                template: "{{context}}",
                localVariables: Hash.FromAnonymousObject(new { context = new Guid("{D0F28A51-9393-4658-AF0B-8C4B4C5C31FF}") }));

            Helper.AssertTemplateResult(
                expected: "true",
                template: "{{context}}",
                localVariables: Hash.FromAnonymousObject(new { context = true }));

            Helper.AssertTemplateResult(
                expected: "false",
                template: "{{context}}",
                localVariables: Hash.FromAnonymousObject(new { context = false }));

            Helper.AssertTemplateResult(
                expected: "",
                template: "{{context}}",
                localVariables: Hash.FromAnonymousObject(new { context = null as string }));
        }

        [Test]
        public void TestListRendering()
        {
            Assert.AreEqual(
                expected: "text1text2",
                actual: Template
                    .Parse("{{context}}")
                    .Render(Hash.FromAnonymousObject(new { context = new LiquidizableList() })));
        }

        [Test]
        public void TestWrappedListRendering()
        {
            Assert.AreEqual(
                expected: string.Empty,
                actual: Template
                    .Parse("{{context}}")
                    .Render(Hash.FromAnonymousObject(new { context = new IndexableLiquidizable() })));

            Assert.AreEqual(
                expected: "text1text2",
                actual: Template
                    .Parse("{{context.thekey}}")
                    .Render(Hash.FromAnonymousObject(new { context = new IndexableLiquidizable() })));
        }

        [Test]
        public void TestDictionaryRendering()
        {
            Assert.AreEqual(
                expected: "[lambda, Hello][alpha, bet]",
                actual: Template
                    .Parse("{{context}}")
                    .Render(Hash.FromAnonymousObject(new { context = new Dictionary<string, object> { ["lambda"] = "Hello", ["alpha"] = "bet" } })));
        }

        [Test]
        public void TestDictionaryAsVariable()
        {
            _context["dynamic"] = Hash.FromDictionary(new Dictionary<string, object> { ["lambda"] = "Hello" });

            Assert.AreEqual("Hello", _context["dynamic.lambda"]);
        }

        [Test]
        public void TestNestedDictionaryAsVariable()
        {
            _context["dynamic"] = Hash.FromDictionary(new Dictionary<string, object> { ["lambda"] = new Dictionary<string, object> { ["name"] = "Hello" } });

            Assert.AreEqual("Hello", _context["dynamic.lambda.name"]);
        }

        [Test]
        public void TestDynamicAsVariable()
        {
            dynamic expandoObject = new ExpandoObject();
            expandoObject.lambda = "Hello";
            _context["dynamic"] = Hash.FromDictionary(expandoObject);

            Assert.AreEqual("Hello", _context["dynamic.lambda"]);
        }

        [Test]
        public void TestNestedDynamicAsVariable()
        {
            dynamic root = new ExpandoObject();
            root.lambda = new ExpandoObject();
            root.lambda.name = "Hello";
            _context["dynamic"] = Hash.FromDictionary(root);

            Assert.AreEqual("Hello", _context["dynamic.lambda.name"]);
        }

        /// <summary>
        /// Test case for [Issue #350](https://github.com/dotliquid/dotliquid/issues/350)
        /// </summary>
        [Test]
        public void TestNestedExpandoTemplate_Issue350()
        {
            var model = new ExpandoModel()
            {
                IntProperty = 23,
                StringProperty = "from string property",
                Properties = new ExpandoObject()
            };
            var dictionary = (IDictionary<string, object>)model.Properties;
            dictionary.Add("Key1", "ExpandoObject Key1 value");

            Template.RegisterSafeType(typeof(ExpandoModel), new[] { "IntProperty", "StringProperty", "Properties" });
            const string templateString = @"Int: '{{IntProperty}}'; String: '{{StringProperty}}'; Expando: '{{Properties.Key1}}'";
            var template = Template.Parse(templateString);
            Assert.AreEqual(expected: "Int: '23'; String: 'from string property'; Expando: 'ExpandoObject Key1 value'",
                            actual: template.Render(Hash.FromAnonymousObject(model)));
        }

        /// <summary>
        /// Test case for [Issue #417](https://github.com/dotliquid/dotliquid/issues/417)
        /// </summary>
        [Test]
        public void TestNestedExpandoTemplate_Issue417()
        {
            var modelString = "{\"States\": [{\"Name\": \"Texas\",\"Code\": \"TX\"}, {\"Name\": \"New York\",\"Code\": \"NY\"}]}";
            var template = "State Is:{{States[0].Name}}";

            var model = JsonConvert.DeserializeObject<ExpandoObject>(modelString);
            var modelHash = Hash.FromDictionary(model);
            Assert.AreEqual(expected: "State Is:Texas", actual: Template.Parse(template).Render(modelHash));
        }

        /// <summary>
        /// Test case for [Issue #474](https://github.com/dotliquid/dotliquid/issues/474)
        /// </summary>
        [Test]
        public void TestDecimalIndexer_Issue474()
        {
            var template = @"{% assign idx = fraction | minus: 0.01 -%}
{{ arr[0] }}
{{ arr[idx] }}";

            var modelHash = Hash.FromAnonymousObject(new { arr = new[] { "Zero", "One" }, fraction = 0.01 });
            Assert.AreEqual(expected: "Zero\r\nZero", actual: Template.Parse(template).Render(modelHash));
        }

        /// <summary>
        /// Test case for [Issue #474](https://github.com/dotliquid/dotliquid/issues/474)
        /// </summary>
        [Test]
        public void TestAllTypesIndexer_Issue474()
        {
            var zero = 0;
            var typesToTest = Util.ExpressionUtilityTest.GetNumericCombinations().Select(item => item.Item1).Distinct().ToList();
            var arrayOfZeroTypes = typesToTest.Select(type => Convert.ChangeType(zero, type)).ToList();

            var template = @"{% for idx in numerics -%}
{{ arr[idx] }}
{% endfor %}";

            var modelHash = Hash.FromAnonymousObject(new { arr = new[] { "Zero", "One" }, numerics = arrayOfZeroTypes });
            Assert.AreEqual(expected: string.Join(String.Empty, Enumerable.Repeat("Zero\r\n", arrayOfZeroTypes.Count)), actual: Template.Parse(template).Render(modelHash));
        }

        [Test]
        public void TestProcAsVariable()
        {
            _context["dynamic"] = (Proc)delegate { return "Hello"; };

            Assert.AreEqual("Hello", _context["dynamic"]);
        }

        [Test]
        public void TestLambdaAsVariable()
        {
            _context["dynamic"] = (Proc)(c => "Hello");

            Assert.AreEqual("Hello", _context["dynamic"]);
        }

        [Test]
        public void TestNestedLambdaAsVariable()
        {
            _context["dynamic"] = Hash.FromAnonymousObject(new { lambda = (Proc)(c => "Hello") });

            Assert.AreEqual("Hello", _context["dynamic.lambda"]);
        }

        [Test]
        public void TestArrayContainingLambdaAsVariable()
        {
            _context["dynamic"] = new object[] { 1, 2, (Proc)(c => "Hello"), 4, 5 };

            Assert.AreEqual("Hello", _context["dynamic[2]"]);
        }

        [Test]
        public void TestLambdaIsCalledOnce()
        {
            int global = 0;
            _context["callcount"] = (Proc)(c =>
            {
                ++global;
                return global.ToString();
            });

            Assert.AreEqual("1", _context["callcount"]);
            Assert.AreEqual("1", _context["callcount"]);
            Assert.AreEqual("1", _context["callcount"]);
        }

        [Test]
        public void TestNestedLambdaIsCalledOnce()
        {
            int global = 0;
            _context["callcount"] = Hash.FromAnonymousObject(new
            {
                lambda = (Proc)(c =>
                {
                    ++global;
                    return global.ToString();
                })
            });

            Assert.AreEqual("1", _context["callcount.lambda"]);
            Assert.AreEqual("1", _context["callcount.lambda"]);
            Assert.AreEqual("1", _context["callcount.lambda"]);
        }

        [Test]
        public void TestLambdaInArrayIsCalledOnce()
        {
            int global = 0;
            _context["callcount"] = new object[]
            { 1, 2, (Proc) (c =>
            {
                ++global;
                return global.ToString();
            }), 4, 5
            };

            Assert.AreEqual("1", _context["callcount[2]"]);
            Assert.AreEqual("1", _context["callcount[2]"]);
            Assert.AreEqual("1", _context["callcount[2]"]);
        }

        [Test]
        public void TestAccessToContextFromProc()
        {
            _context.Registers["magic"] = 345392;

            _context["magic"] = (Proc)(c => _context.Registers["magic"]);

            Assert.AreEqual(345392, _context["magic"]);
        }

        [Test]
        public void TestToLiquidAndContextAtFirstLevel()
        {
            _context["category"] = new Category("foobar");
            Assert.IsInstanceOf<CategoryDrop>(_context["category"]);
            Assert.AreEqual(_context, ((CategoryDrop)_context["category"]).Context);
        }

        [Test]
        public void TestVariableParserV21()
        {
            var regex = new System.Text.RegularExpressions.Regex(Liquid.VariableParser);
            TestVariableParser((input) => DotLiquid.Util.R.Scan(input, regex));
        }

        [Test]
        public void TestVariableParserV22()
        {
            TestVariableParser((input) => GetVariableParts(input));
        }

        private void TestVariableParser(Func<string, IEnumerable<string>> variableSplitterFunc)
        {
            CollectionAssert.IsEmpty(variableSplitterFunc(""));
            CollectionAssert.AreEqual(new[] { "var" }, variableSplitterFunc("var"));
            CollectionAssert.AreEqual(new[] { "var", "method" }, variableSplitterFunc("var.method"));
            CollectionAssert.AreEqual(new[] { "var", "[method]" }, variableSplitterFunc("var[method]"));
            CollectionAssert.AreEqual(new[] { "var", "[method]", "[0]" }, variableSplitterFunc("var[method][0]"));
            CollectionAssert.AreEqual(new[] { "var", "[\"method\"]", "[0]" }, variableSplitterFunc("var[\"method\"][0]"));
            CollectionAssert.AreEqual(new[] { "var", "[method]", "[0]", "method" }, variableSplitterFunc("var[method][0].method"));
        }

        private static IEnumerable<string> GetVariableParts(string input)
        {
            using (var enumerator = Tokenizer.GetVariableEnumerator(input))
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
        }

        [Test]
        public void TestConstructor()
        {
            var context = new Context(new CultureInfo("jp-JP"));
            Assert.AreEqual(Template.DefaultSyntaxCompatibilityLevel, context.SyntaxCompatibilityLevel);
            Assert.AreEqual(Liquid.UseRubyDateFormat, context.UseRubyDateFormat);
            Assert.AreEqual("jp-JP", context.CurrentCulture.Name);
        }

        /// <summary>
        /// The expectation is that a Context is created with a CultureInfo, however,
        /// the parameter is defined as an IFormatProvider so this is not enforced by
        /// the compiler.
        /// </summary>
        /// <remarks>
        /// This test verifies that a CultureInfo is returned by Context.CultureInfo even
        /// if Context was created with a non-CultureInfo
        /// </remarks>
        [Test]
        public void TestCurrentCulture_NotACultureInfo()
        {
            // Create context with an IFormatProvider that is not a CultureInfo
            Context context = new Context(CultureInfo.CurrentCulture.NumberFormat);
            Assert.AreSame(CultureInfo.CurrentCulture, context.CurrentCulture);
        }
    }
}
