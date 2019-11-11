using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using DotLiquid.Exceptions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class FilterTests
    {
        #region Classes used in tests

        private static class MoneyFilter
        {
            public static string Money(object input)
            {
                return string.Format(" {0:d}$ ", input);
            }

            public static string MoneyWithUnderscore(object input)
            {
                return string.Format(" {0:d}$ ", input);
            }
        }

        private static class CanadianMoneyFilter
        {
            public static string Money(object input)
            {
                return string.Format(" {0:d}$ CAD ", input);
            }
        }

        private static class FiltersWithArguments
        {
            public static string Adjust(int input, int offset = 10)
            {
                return string.Format("[{0:d}]", input + offset);
            }

            public static string AddSub(int input, int plus, int minus = 20)
            {
                return string.Format("[{0:d}]", input + plus - minus);
            }
        }

        private static class FiltersWithMulitpleMethodSignatures
        {
            public static string Concat(string one, string two)
            {
                return string.Concat(one, two);
            }

            public static string Concat(string one, string two, string three)
            {
                return string.Concat(one, two, three);
            }
        }

        private static class FiltersWithMultipleMethodSignaturesAndContextParam
        {
            public static string ConcatWithContext(Context context, string one, string two)
            {
                return string.Concat(one, two);
            }

            public static string ConcatWithContext(Context context, string one, string two, string three)
            {
                return string.Concat(one, two, three);
            }
        }

        private static class ContextFilters
        {
            public static string BankStatement(Context context, object input)
            {
                return string.Format(" " + context["name"] + " has {0:d}$ ", input);
            }
        }

        #endregion

        private Context _context;

        [OneTimeSetUp]
        public void SetUp()
        {
            _context = new Context(CultureInfo.InvariantCulture);
        }

        /*[Test]
        public void TestNonExistentFilter()
        {
            _context["var"] = 1000;
            Assert.Throws<FilterNotFoundException>(() => new Variable("var | syzzy").Render(_context));
        }*/

        [Test]
        public async Task TestLocalFilter()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(MoneyFilter));
            Assert.AreEqual(" 1000$ ", await new Variable("var | money").RenderAsync(_context));
        }

        [Test]
        public async Task TestUnderscoreInFilterName()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(MoneyFilter));
            Assert.AreEqual(" 1000$ ", await new Variable("var | money_with_underscore").RenderAsync(_context));
        }

        [Test]
        public async Task TestFilterWithNumericArgument()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(FiltersWithArguments));
            Assert.AreEqual("[1005]", await new Variable("var | adjust: 5").RenderAsync(_context));
        }

        [Test]
        public async Task TestFilterWithNegativeArgument()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(FiltersWithArguments));
            Assert.AreEqual("[995]", await new Variable("var | adjust: -5").RenderAsync(_context));
        }

        [Test]
        public async Task TestFilterWithDefaultArgument()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(FiltersWithArguments));
            Assert.AreEqual("[1010]", await new Variable("var | adjust").RenderAsync(_context));
        }

        [Test]
        public async Task TestFilterWithTwoArguments()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(FiltersWithArguments));
            Assert.AreEqual("[1150]", await new Variable("var | add_sub: 200, 50").RenderAsync(_context));
        }

        [Test]
        public async Task TestFilterWithMultipleMethodSignatures()
        {
            Template.RegisterFilter(typeof(FiltersWithMulitpleMethodSignatures));

            Assert.AreEqual("AB", await Template.Parse("{{'A' | concat : 'B'}}").RenderAsync());
            Assert.AreEqual("ABC", await Template.Parse("{{'A' | concat : 'B', 'C'}}").RenderAsync());
        }

        [Test]
        public async Task TestFilterWithMultipleMethodSignaturesAndContextParam()
        {
            Template.RegisterFilter(typeof(FiltersWithMultipleMethodSignaturesAndContextParam));

            Assert.AreEqual("AB", await Template.Parse("{{'A' | concat_with_context : 'B'}}").RenderAsync());
            Assert.AreEqual("ABC", await Template.Parse("{{'A' | concat_with_context : 'B', 'C'}}").RenderAsync());
        }

        /*/// <summary>
        /// ATM the trailing value is silently ignored. Should raise an exception?
        /// </summary>
        [Test]
        public void TestFilterWithTwoArgumentsNoComma()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(FiltersWithArguments));
            Assert.AreEqual("[1150]", string.Join(string.Empty, new Variable("var | add_sub: 200 50").Render(_context));
        }*/

        [Test]
        public async Task TestSecondFilterOverwritesFirst()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(MoneyFilter));
            _context.AddFilters(typeof(CanadianMoneyFilter));
            Assert.AreEqual(" 1000$ CAD ", await new Variable("var | money").RenderAsync(_context));
        }

        [Test]
        public async Task TestSize()
        {
            _context["var"] = "abcd";
            _context.AddFilters(typeof(MoneyFilter));
            Assert.AreEqual(4, await new Variable("var | size").RenderAsync(_context));
        }

        [Test]
        public async Task TestJoin()
        {
            _context["var"] = new[] { 1, 2, 3, 4 };
            Assert.AreEqual("1 2 3 4", await new Variable("var | join").RenderAsync(_context));
        }

        [Test]
        public async Task TestSort()
        {
            _context["value"] = 3;
            _context["numbers"] = new[] { 2, 1, 4, 3 };
            _context["words"] = new[] { "expected", "as", "alphabetic" };
            _context["arrays"] = new[] { new[] { "flattened" }, new[] { "are" } };

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, await new Variable("numbers | sort").RenderAsync(_context) as IEnumerable);
            CollectionAssert.AreEqual(new[] { "alphabetic", "as", "expected" }, await new Variable("words | sort").RenderAsync(_context) as IEnumerable);
            CollectionAssert.AreEqual(new[] { 3 }, await new Variable("value | sort").RenderAsync(_context) as IEnumerable);
            CollectionAssert.AreEqual(new[] { "are", "flattened" }, await new Variable("arrays | sort").RenderAsync(_context) as IEnumerable);
        }

        [Test]
        public async Task TestSplit()
        {
            _context["var"] = "a~b";
            Assert.AreEqual(new[] { "a", "b" }, await new Variable("var | split:'~'").RenderAsync(_context));
        }

        [Test]
        public async Task TestStripHtml()
        {
            _context["var"] = "<b>bla blub</a>";
            Assert.AreEqual("bla blub", await new Variable("var | strip_html").RenderAsync(_context));
        }

        [Test]
        public async Task Capitalize()
        {
            _context["var"] = "blub";
            Assert.AreEqual("Blub", await new Variable("var | capitalize").RenderAsync(_context));
        }

        [Test]
        public async Task Slice()
        {
            _context["var"] = "blub";
            Assert.AreEqual("b", await new Variable("var | slice: 0, 1").RenderAsync(_context));
            Assert.AreEqual("bl", await new Variable("var | slice: 0, 2").RenderAsync(_context));
            Assert.AreEqual("l", await new Variable("var | slice: 1").RenderAsync(_context));
            Assert.AreEqual("", await new Variable("var | slice: 4, 1").RenderAsync(_context));
            Assert.AreEqual("ub", await new Variable("var | slice: -2, 2").RenderAsync(_context));
            Assert.AreEqual(null, await new Variable("var | slice: 5, 1").RenderAsync(_context));
        }

        [Test]
        public async Task TestLocalGlobal()
        {
            Template.RegisterFilter(typeof(MoneyFilter));

            Assert.AreEqual(" 1000$ ", await Template.Parse("{{1000 | money}}").RenderAsync());
            Assert.AreEqual(" 1000$ CAD ", await Template.Parse("{{1000 | money}}").RenderAsync(new RenderParameters(CultureInfo.InvariantCulture) { Filters = new[] { typeof(CanadianMoneyFilter) } }));
            Assert.AreEqual(" 1000$ CAD ", await Template.Parse("{{1000 | money}}").RenderAsync(new RenderParameters(CultureInfo.InvariantCulture) { Filters = new[] { typeof(CanadianMoneyFilter) } }));
        }

        [Test]
        public async Task TestContextFilter()
        {
            _context["var"] = 1000;
            _context["name"] = "King Kong";
            _context.AddFilters(typeof(ContextFilters));
            Assert.AreEqual(" King Kong has 1000$ ", await new Variable("var | bank_statement").RenderAsync(_context));
        }
    }
}
