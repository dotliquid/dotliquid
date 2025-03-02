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
            Assert.That(_context["string"], Is.EqualTo("string"));

            _context["EscapedCharacter"] = "EscapedCharacter\"";
            Assert.That(_context["EscapedCharacter"], Is.EqualTo("EscapedCharacter\""));

            _context["num"] = 5;
            Assert.That(_context["num"], Is.EqualTo(5));

            _context["decimal"] = 5m;
            Assert.That(_context["decimal"], Is.EqualTo(5m));

            _context["float"] = 5.0f;
            Assert.That(_context["float"], Is.EqualTo(5.0f));

            _context["double"] = 5.0;
            Assert.That(_context["double"], Is.EqualTo(5.0));

            _context["time"] = TimeSpan.FromDays(1);
            Assert.That(_context["time"], Is.EqualTo(TimeSpan.FromDays(1)));

            _context["date"] = DateTime.Today;
            Assert.That(_context["date"], Is.EqualTo(DateTime.Today));

            DateTime now = DateTime.Now;
            _context["datetime"] = now;
            Assert.That(_context["datetime"], Is.EqualTo(now));

            DateTimeOffset offset = new DateTimeOffset(2013, 9, 10, 0, 10, 32, new TimeSpan(1, 0, 0));
            _context["datetimeoffset"] = offset;
            Assert.That(_context["datetimeoffset"], Is.EqualTo(offset));

            Guid guid = Guid.NewGuid();
            _context["guid"] = guid;
            Assert.That(_context["guid"], Is.EqualTo(guid));

            _context["bool"] = true;
            Assert.That(_context["bool"], Is.EqualTo(true));

            _context["bool"] = false;
            Assert.That(_context["bool"], Is.EqualTo(false));

            _context["nil"] = null;
            Assert.That(_context["nil"], Is.EqualTo(null));
            Assert.That(_context["nil"], Is.EqualTo(null));
        }

        [Test]
        public void TestVariablesArray()
        {
            List<int> list = new List<int> { 1, 2, 3, 4, 5 };
            _context["list"] = list;
            Assert.That(_context["list"], Is.EqualTo(list));
            Assert.That(_context["list[0]"], Is.EqualTo(1));
            Assert.That(_context["list[-1]"], Is.EqualTo(5));
            Assert.That(_context["list[12]"], Is.Null);
            Assert.That(_context["list[-12]"], Is.Null);

            List<string> emptyList = new List<string>();
            _context["empty_list"] = emptyList;
            Assert.That(_context["empty_list"], Is.EqualTo(emptyList));
            Assert.That(_context["empty_list[0]"], Is.Null);
            Assert.That(_context["empty_list[-1]"], Is.Null);
        }

#if NET6_0_OR_GREATER
        [Test]
        public void TestVariables_NET60()
        {
            var dateOnly = new DateOnly(year: 2013, month: 9, day: 10);
            _context["dateonly"] = dateOnly;
            Assert.That(_context["dateOnly"], Is.EqualTo(dateOnly));
            var timeOnly = new TimeOnly(hour: 0, minute: 10, second: 32);
            _context["timeonly"] = timeOnly;
            Assert.That(_context["timeonly"], Is.EqualTo(timeOnly));
        }
#endif

        private enum TestEnum { Yes, No }

        [Test]
        public void TestGetVariable_Enum()
        {
            _context["yes"] = TestEnum.Yes;
            _context["no"] = TestEnum.No;
            _context["not_enum"] = TestEnum.Yes.ToString();

            Assert.That(_context["yes"], Is.EqualTo(TestEnum.Yes));
            Assert.That(_context["no"], Is.EqualTo(TestEnum.No));
            Assert.That(_context["not_enum"], Is.Not.EqualTo(TestEnum.Yes));
        }

        [Test]
        public void TestVariablesNotExisting()
        {
            Assert.That(_context["does_not_exist"], Is.EqualTo(null));
        }

        [Test]
        public void TestVariableNotFoundErrors()
        {
            Template template = Template.Parse("{{ does_not_exist }}");
            string rendered = template.Render();

            Assert.That(rendered, Is.EqualTo(""));
            Assert.That(template.Errors.Count, Is.EqualTo(1));
            Assert.That(template.Errors[0].Message, Is.EqualTo(string.Format(Liquid.ResourceManager.GetString("VariableNotFoundException"), "does_not_exist")));
        }

        [Test]
        public void TestVariableNotFoundFromAnonymousObject()
        {
            Template template = Template.Parse("{{ first.test }}{{ second.test }}");
            string rendered = template.Render(Hash.FromAnonymousObject(new { second = new { foo = "hi!" } }));

            Assert.That(rendered, Is.EqualTo(""));
            Assert.That(template.Errors.Count, Is.EqualTo(2));
            Assert.That(template.Errors[0].Message, Is.EqualTo(string.Format(Liquid.ResourceManager.GetString("VariableNotFoundException"), "first.test")));
            Assert.That(template.Errors[1].Message, Is.EqualTo(string.Format(Liquid.ResourceManager.GetString("VariableNotFoundException"), "second.test")));
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

            Assert.That(rendered, Is.EqualTo(""));
            Assert.That(template.Errors.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestVariableNotFoundExceptionIgnoredForUnlessStatement()
        {
            Template template = Template.Parse("{% unless does_not_exist %}abc{% endunless %}");
            string rendered = template.Render();

            Assert.That(rendered, Is.EqualTo("abc"));
            Assert.That(template.Errors.Count, Is.EqualTo(0));
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
            Assert.That(_context["numbers.size"], Is.EqualTo(4));

            _context["numbers"] = new Dictionary<int, int>
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 },
                { 4, 4 }
            };
            Assert.That(_context["numbers.size"], Is.EqualTo(4));

            _context["numbers"] = new Dictionary<object, int>
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 },
                { 4, 4 },
                { "size", 1000 }
            };
            Assert.That(_context["numbers.size"], Is.EqualTo(1000));
        }

        [Test]
        public void TestHyphenatedVariable()
        {
            _context["oh-my"] = "godz";
            Assert.That(_context["oh-my"], Is.EqualTo("godz"));
        }

        [Test]
        public void TestAddFilter()
        {
            Context context = new Context(CultureInfo.InvariantCulture);
            context.AddFilters(new[] { typeof(TestFilters) });
            Assert.That(context.Invoke("hi", new List<object> { "hi?" }), Is.EqualTo("hi? hi!"));
            context.SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22;
            Assert.That(context.Invoke("hi", new List<object> { "hi?" }), Is.EqualTo("hi? hi!"));

            context = new Context(CultureInfo.InvariantCulture);
            Assert.That(context.Invoke("hi", new List<object> { "hi?" }), Is.EqualTo("hi?"));
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
            Assert.That(context.Invoke("hi", new List<object> { "hi?" }), Is.EqualTo("hi? hi from King Kong!"));
            context.SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22;
            Assert.That(context.Invoke("hi", new List<object> { "hi?" }), Is.EqualTo("hi? hi from King Kong!"));

            context = new Context(CultureInfo.InvariantCulture) { SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid20 };
            Assert.That(context.Invoke("hi", new List<object> { "hi?" }), Is.EqualTo("hi?"));
            context.SyntaxCompatibilityLevel = SyntaxCompatibility.DotLiquid22;
            Assert.Throws<FilterNotFoundException>(() => context.Invoke("hi", new List<object> { "hi?" }));
        }

        [Test]
        public void TestOverrideGlobalFilter()
        {
            Template.RegisterFilter(typeof(GlobalFilters));
            Assert.That(Template.Parse("{{'test' | notice }}").Render(), Is.EqualTo("Global test"));
            Assert.That(Template.Parse("{{'test' | notice }}").Render(new RenderParameters(CultureInfo.InvariantCulture) { Filters = new[] { typeof(LocalFilters) } }), Is.EqualTo("Local test"));
        }

        [Test]
        public void TestOnlyIntendedFiltersMakeItThere()
        {
            Context context = new Context(CultureInfo.InvariantCulture);
            var methodsBefore = context.Strainer.Methods.Select(mi => mi.Name).ToList();
            context.AddFilters(new[] { typeof(TestFilters) });
            var methodsAfter = context.Strainer.Methods.Select(mi => mi.Name).ToList();
            Assert.That(
                methodsAfter.OrderBy(s => s).ToList(), Is.EqualTo(methodsBefore.Concat(new[] { "Hi" }).OrderBy(s => s).ToList()).AsCollection);
        }

        [Test]
        public void TestAddItemInOuterScope()
        {
            _context["test"] = "test";
            _context.Push(new Hash());
            Assert.That(_context["test"], Is.EqualTo("test"));
            _context.Pop();
            Assert.That(_context["test"], Is.EqualTo("test"));
        }

        [Test]
        public void TestAddItemInInnerScope()
        {
            _context.Push(new Hash());
            _context["test"] = "test";
            Assert.That(_context["test"], Is.EqualTo("test"));
            _context.Pop();
            Assert.That(_context["test"], Is.EqualTo(null));
        }

        [Test]
        public void TestHierarchicalData()
        {
            _context["hash"] = new { name = "tobi" };
            Assert.That(_context["hash.name"], Is.EqualTo("tobi"));
            Assert.That(_context["hash['name']"], Is.EqualTo("tobi"));
        }

        [Test]
        public void TestKeywords()
        {
            Assert.That(_context["true"], Is.EqualTo(true));
            Assert.That(_context["false"], Is.EqualTo(false));
        }

        [Test]
        public void TestDigits()
        {
            Assert.That(_context["100"], Is.EqualTo(100));
            Assert.That(_context[string.Format("100{0}00", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)], Is.EqualTo(100.00));
        }

        [Test]
        public void TestStrings()
        {
            Assert.That(_context["'hello!'"], Is.EqualTo("hello!"));
            Assert.That(_context["'hello!'"], Is.EqualTo("hello!"));
        }

        [Test]
        public void TestMerge()
        {
            _context.Merge(new Hash { { "test", "test" } });
            Assert.That(_context["test"], Is.EqualTo("test"));
            _context.Merge(new Hash { { "test", "newvalue" }, { "foo", "bar" } });
            Assert.That(_context["test"], Is.EqualTo("newvalue"));
            Assert.That(_context["foo"], Is.EqualTo("bar"));
        }

        [Test]
        public void TestArrayNotation()
        {
            _context["test"] = new[] { 1, 2, 3, 4, 5 };

            Assert.That(_context["test[0]"], Is.EqualTo(1));
            Assert.That(_context["test[1]"], Is.EqualTo(2));
            Assert.That(_context["test[2]"], Is.EqualTo(3));
            Assert.That(_context["test[3]"], Is.EqualTo(4));
            Assert.That(_context["test[4]"], Is.EqualTo(5));
        }

        [Test]
        public void TestRecursiveArrayNotation()
        {
            _context["test"] = new { test = new[] { 1, 2, 3, 4, 5 } };

            Assert.That(_context["test.test[0]"], Is.EqualTo(1));

            _context["test"] = new[] { new { test = "worked" } };

            Assert.That(_context["test[0].test"], Is.EqualTo("worked"));
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

            Assert.That(_context["colors.Blue[0]"], Is.EqualTo("003366"));
            Assert.That(_context["colors.Red[3]"], Is.EqualTo("FF9999"));
        }

        [Test]
        public void TestFirstLastSize()
        {
            _context["test"] = new[] { 1, 2, 3, 4, 5 };

            Assert.That(_context["test.first"], Is.EqualTo(1));
            Assert.That(_context["test.last"], Is.EqualTo(5));
            Assert.That(_context["test.size"], Is.EqualTo(5));

            _context["test"] = new { test = new[] { 1, 2, 3, 4, 5 } };

            Assert.That(_context["test.test.first"], Is.EqualTo(1));
            Assert.That(_context["test.test.last"], Is.EqualTo(5));
            Assert.That(_context["test.test.size"], Is.EqualTo(5));

            _context["test"] = new[] { 1 };

            Assert.That(_context["test.first"], Is.EqualTo(1));
            Assert.That(_context["test.last"], Is.EqualTo(1));
            Assert.That(_context["test.size"], Is.EqualTo(1));
        }

        [Test]
        public void TestAccessHashesWithHashNotation()
        {
            _context["products"] = new { count = 5, tags = new[] { "deepsnow", "freestyle" } };
            _context["product"] = new { variants = new[] { new { title = "draft151cm" }, new { title = "element151cm" } } };

            Assert.That(_context["products[\"count\"]"], Is.EqualTo(5));
            Assert.That(_context["products['tags'][0]"], Is.EqualTo("deepsnow"));
            Assert.That(_context["products['tags'][1]"], Is.EqualTo("freestyle"));
            Assert.That(_context["products['tags'][-1]"], Is.EqualTo("freestyle"));
            Assert.That(_context["products['tags'][-2]"], Is.EqualTo("deepsnow"));
            Assert.That(_context["products['tags'].first"], Is.EqualTo("deepsnow"));
            Assert.That(_context["products['tags'].last"], Is.EqualTo("freestyle"));
            Assert.That(_context["products['tags'].size"], Is.EqualTo(2));
            Assert.That(_context["product['variants'][0][\"title\"]"], Is.EqualTo("draft151cm"));
            Assert.That(_context["product['variants'][1]['title']"], Is.EqualTo("element151cm"));
            Assert.That(_context["product['variants'][0]['title']"], Is.EqualTo("draft151cm"));
            Assert.That(_context["product['variants'].last['title']"], Is.EqualTo("element151cm"));
        }

        [Test]
        public void TestAccessVariableWithHashNotation()
        {
            _context["foo"] = "baz";
            _context["bar"] = "foo";

            Assert.That(_context["[\"foo\"]"], Is.EqualTo("baz"));
            Assert.That(_context["[bar]"], Is.EqualTo("baz"));
        }

        [Test]
        public void TestAccessHashesWithHashAccessVariables()
        {
            _context["var"] = "tags";
            _context["nested"] = new { var = "tags" };
            _context["products"] = new { count = 5, tags = new[] { "deepsnow", "freestyle" } };

            Assert.That(_context["products[var].first"], Is.EqualTo("deepsnow"));
            Assert.That(_context["products[nested.var].last"], Is.EqualTo("freestyle"));
        }

        [Test]
        public void TestHashNotationOnlyForHashAccess()
        {
            _context["array"] = new[] { 1, 2, 3, 4, 5 };
            _context["hash"] = new { first = "Hello" };

            Assert.That(_context["array.first"], Is.EqualTo(1));
            Assert.That(_context["array['first']"], Is.EqualTo(null));
            Assert.That(_context["hash['first']"], Is.EqualTo("Hello"));
        }

        [Test]
        public void TestFirstCanAppearInMiddleOfCallchain()
        {
            _context["product"] = new { variants = new[] { new { title = "draft151cm" }, new { title = "element151cm" } } };

            Assert.That(_context["product.variants[0].title"], Is.EqualTo("draft151cm"));
            Assert.That(_context["product.variants[1].title"], Is.EqualTo("element151cm"));
            Assert.That(_context["product.variants.first.title"], Is.EqualTo("draft151cm"));
            Assert.That(_context["product.variants.last.title"], Is.EqualTo("element151cm"));
        }

        [Test]
        public void TestCents()
        {
            _context.Merge(Hash.FromAnonymousObject(new { cents = new HundredCents() }));
            Assert.That(_context["cents"], Is.EqualTo(100));
        }

        [Test]
        public void TestNestedCents()
        {
            _context.Merge(Hash.FromAnonymousObject(new { cents = new { amount = new HundredCents() } }));
            Assert.That(_context["cents.amount"], Is.EqualTo(100));

            _context.Merge(Hash.FromAnonymousObject(new { cents = new { cents = new { amount = new HundredCents() } } }));
            Assert.That(_context["cents.cents.amount"], Is.EqualTo(100));
        }

        [Test]
        public void TestCentsThroughDrop()
        {
            _context.Merge(Hash.FromAnonymousObject(new { cents = new CentsDrop() }));
            Assert.That(_context["cents.amount"], Is.EqualTo(100));
        }

        [Test]
        public void TestNestedCentsThroughDrop()
        {
            _context.Merge(Hash.FromAnonymousObject(new { vars = new { cents = new CentsDrop() } }));
            Assert.That(_context["vars.cents.amount"], Is.EqualTo(100));
        }

        [Test]
        public void TestDropMethodsWithQuestionMarks()
        {
            _context.Merge(Hash.FromAnonymousObject(new { cents = new CentsDrop() }));
            Assert.That(_context["cents.non_zero"], Is.EqualTo(true));
        }

        [Test]
        public void TestContextFromWithinDrop()
        {
            _context.Merge(Hash.FromAnonymousObject(new { test = "123", vars = new ContextSensitiveDrop() }));
            Assert.That(_context["vars.test"], Is.EqualTo("123"));
        }

        [Test]
        public void TestNestedContextFromWithinDrop()
        {
            _context.Merge(Hash.FromAnonymousObject(new { test = "123", vars = new { local = new ContextSensitiveDrop() } }));
            Assert.That(_context["vars.local.test"], Is.EqualTo("123"));
        }

        [Test]
        public void TestRanges()
        {
            _context.Merge(Hash.FromAnonymousObject(new { test = 5 }));
            Assert.That(_context["(1..5)"] as IEnumerable, Is.EqualTo(Enumerable.Range(1, 5)).AsCollection);
            Assert.That(_context["(1..test)"] as IEnumerable, Is.EqualTo(Enumerable.Range(1, 5)).AsCollection);
            Assert.That(_context["(test..test)"] as IEnumerable, Is.EqualTo(Enumerable.Range(5, 1)).AsCollection);
        }

        [Test]
        public void TestCentsThroughDropNestedly()
        {
            _context.Merge(Hash.FromAnonymousObject(new { cents = new { cents = new CentsDrop() } }));
            Assert.That(_context["cents.cents.amount"], Is.EqualTo(100));

            _context.Merge(Hash.FromAnonymousObject(new { cents = new { cents = new { cents = new CentsDrop() } } }));
            Assert.That(_context["cents.cents.cents.amount"], Is.EqualTo(100));
        }

        [Test]
        public void TestDropWithVariableCalledOnlyOnce()
        {
            _context["counter"] = new CounterDrop();

            Assert.That(_context["counter.count"], Is.EqualTo(1));
            Assert.That(_context["counter.count"], Is.EqualTo(2));
            Assert.That(_context["counter.count"], Is.EqualTo(3));
        }

        [Test]
        public void TestDropWithKeyOnlyCalledOnce()
        {
            _context["counter"] = new CounterDrop();

            Assert.That(_context["counter['count']"], Is.EqualTo(1));
            Assert.That(_context["counter['count']"], Is.EqualTo(2));
            Assert.That(_context["counter['count']"], Is.EqualTo(3));
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
            Assert.That(
                actual: Template
                    .Parse("{{context}}")
                    .Render(Hash.FromAnonymousObject(new { context = new LiquidizableList() })), Is.EqualTo(expected: "text1text2"));
        }

        [Test]
        public void TestWrappedListRendering()
        {
            Assert.That(
                actual: Template
                    .Parse("{{context}}")
                    .Render(Hash.FromAnonymousObject(new { context = new IndexableLiquidizable() })), Is.EqualTo(expected: string.Empty));

            Assert.That(
                actual: Template
                    .Parse("{{context.thekey}}")
                    .Render(Hash.FromAnonymousObject(new { context = new IndexableLiquidizable() })), Is.EqualTo(expected: "text1text2"));
        }

        [Test]
        public void TestDictionaryRendering()
        {
            Assert.That(
                actual: Template
                    .Parse("{{context}}")
                    .Render(Hash.FromAnonymousObject(new { context = new Dictionary<string, object> { ["lambda"] = "Hello", ["alpha"] = "bet" } })), Is.EqualTo(expected: "[lambda, Hello][alpha, bet]"));
        }

        [Test]
        public void TestDictionaryAsVariable()
        {
            _context["dynamic"] = Hash.FromDictionary(new Dictionary<string, object> { ["lambda"] = "Hello" });

            Assert.That(_context["dynamic.lambda"], Is.EqualTo("Hello"));
        }

        [Test]
        public void TestNestedDictionaryAsVariable()
        {
            _context["dynamic"] = Hash.FromDictionary(new Dictionary<string, object> { ["lambda"] = new Dictionary<string, object> { ["name"] = "Hello" } });

            Assert.That(_context["dynamic.lambda.name"], Is.EqualTo("Hello"));
        }

        [Test]
        public void TestDynamicAsVariable()
        {
            dynamic expandoObject = new ExpandoObject();
            expandoObject.lambda = "Hello";
            _context["dynamic"] = Hash.FromDictionary(expandoObject);

            Assert.That(_context["dynamic.lambda"], Is.EqualTo("Hello"));
        }

        [Test]
        public void TestNestedDynamicAsVariable()
        {
            dynamic root = new ExpandoObject();
            root.lambda = new ExpandoObject();
            root.lambda.name = "Hello";
            _context["dynamic"] = Hash.FromDictionary(root);

            Assert.That(_context["dynamic.lambda.name"], Is.EqualTo("Hello"));
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
            Assert.That(actual: template.Render(Hash.FromAnonymousObject(model)), Is.EqualTo(expected: "Int: '23'; String: 'from string property'; Expando: 'ExpandoObject Key1 value'"));
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
            Assert.That(actual: Template.Parse(template).Render(modelHash), Is.EqualTo(expected: "State Is:Texas"));
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
            Assert.That(actual: Template.Parse(template).Render(modelHash), Is.EqualTo(expected: "Zero\r\nZero"));
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
            Assert.That(actual: Template.Parse(template).Render(modelHash), Is.EqualTo(expected: string.Join(String.Empty, Enumerable.Repeat("Zero\r\n", arrayOfZeroTypes.Count))));
        }

        [Test]
        public void TestProcAsVariable()
        {
            _context["dynamic"] = (Proc)delegate { return "Hello"; };

            Assert.That(_context["dynamic"], Is.EqualTo("Hello"));
        }

        [Test]
        public void TestLambdaAsVariable()
        {
            _context["dynamic"] = (Proc)(c => "Hello");

            Assert.That(_context["dynamic"], Is.EqualTo("Hello"));
        }

        [Test]
        public void TestNestedLambdaAsVariable()
        {
            _context["dynamic"] = Hash.FromAnonymousObject(new { lambda = (Proc)(c => "Hello") });

            Assert.That(_context["dynamic.lambda"], Is.EqualTo("Hello"));
        }

        [Test]
        public void TestArrayContainingLambdaAsVariable()
        {
            _context["dynamic"] = new object[] { 1, 2, (Proc)(c => "Hello"), 4, 5 };

            Assert.That(_context["dynamic[2]"], Is.EqualTo("Hello"));
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

            Assert.That(_context["callcount"], Is.EqualTo("1"));
            Assert.That(_context["callcount"], Is.EqualTo("1"));
            Assert.That(_context["callcount"], Is.EqualTo("1"));
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

            Assert.That(_context["callcount.lambda"], Is.EqualTo("1"));
            Assert.That(_context["callcount.lambda"], Is.EqualTo("1"));
            Assert.That(_context["callcount.lambda"], Is.EqualTo("1"));
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

            Assert.That(_context["callcount[2]"], Is.EqualTo("1"));
            Assert.That(_context["callcount[2]"], Is.EqualTo("1"));
            Assert.That(_context["callcount[2]"], Is.EqualTo("1"));
        }

        [Test]
        public void TestAccessToContextFromProc()
        {
            _context.Registers["magic"] = 345392;

            _context["magic"] = (Proc)(c => _context.Registers["magic"]);

            Assert.That(_context["magic"], Is.EqualTo(345392));
        }

        [Test]
        public void TestToLiquidAndContextAtFirstLevel()
        {
            _context["category"] = new Category("foobar");
            Assert.That(_context["category"], Is.InstanceOf<CategoryDrop>());
            Assert.That(((CategoryDrop)_context["category"]).Context, Is.EqualTo(_context));
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
            Assert.That(variableSplitterFunc(""), Is.Empty);
            Assert.That(variableSplitterFunc("var"), Is.EqualTo(new[] { "var" }).AsCollection);
            Assert.That(variableSplitterFunc("var.method"), Is.EqualTo(new[] { "var", "method" }).AsCollection);
            Assert.That(variableSplitterFunc("var[method]"), Is.EqualTo(new[] { "var", "[method]" }).AsCollection);
            Assert.That(variableSplitterFunc("var[method][0]"), Is.EqualTo(new[] { "var", "[method]", "[0]" }).AsCollection);
            Assert.That(variableSplitterFunc("var[\"method\"][0]"), Is.EqualTo(new[] { "var", "[\"method\"]", "[0]" }).AsCollection);
            Assert.That(variableSplitterFunc("var[method][0].method"), Is.EqualTo(new[] { "var", "[method]", "[0]", "method" }).AsCollection);
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
            Assert.That(context.SyntaxCompatibilityLevel, Is.EqualTo(Template.DefaultSyntaxCompatibilityLevel));
            Assert.That(context.UseRubyDateFormat, Is.EqualTo(Liquid.UseRubyDateFormat));
            Assert.That(context.CurrentCulture.Name, Is.EqualTo("jp-JP"));
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
            Assert.That(context.CurrentCulture, Is.SameAs(CultureInfo.CurrentCulture));
        }
    }
}
