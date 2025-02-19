using System.Collections;
using System.Globalization;
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

        private static class FiltersWithArgumentsInt
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

        private static class FiltersWithArgumentsLong
        {
            public static string Adjust(long input, long offset = 10)
            {
                return string.Format("[{0:d}]", input + offset);
            }

            public static string AddSub(long input, long plus, long minus = 20)
            {
                return string.Format("[{0:d}]", input + plus - minus);
            }
        }

        private static class FiltersWithMultipleMethodSignatures
        {
            public static string Concatenate(string one, string two)
            {
                return string.Concat(one, two);
            }

            public static string Concatenate(string one, string two, string three)
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

        private static class FiltersWithMultipleMethodSignaturesDifferentClassesOne
        {
            public static string Concatenate(string one, string two)
            {
                return string.Concat(one, two);
            }
        }


        private static class FilterWithSameMethodSignatureDifferentClassOne
        {
            public static string Concatenate(string one, string two)
            {
                return string.Concat(one, two, "Class One");
            }
        }

        private static class FilterWithSameMethodSignatureDifferentClassTwo
        {
            public static string Concatenate(string one, string two)
            {
                return string.Concat(one, two, "Class Two");
            }
        }

        private static class FiltersWithMultipleMethodSignaturesDifferentClassesTwo
        {
            public static string Concatenate(Context context, string one, string two, string three)
            {
                return string.Concat(one, two, three);
            }
        }

        private static class FiltersWithMultipleMethodSignaturesDifferentClassesWithContextParamTwo
        {
            public static string ConcatWithContext(Context context, string one, string two, string three)
            {
                return string.Concat(one, two, three);
            }
        }

        private static class FiltersWithMultipleMethodSignaturesDifferentClassesWithContextParamOne
        {
            public static string ConcatWithContext(Context context, string one, string two)
            {
                return string.Concat(one, two);
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
        public void TestLocalFilter()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(MoneyFilter));
            Assert.That(new Variable("var | money").Render(_context), Is.EqualTo(" 1000$ "));
        }

        [Test]
        public void TestUnderscoreInFilterName()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(MoneyFilter));
            Assert.That(new Variable("var | money_with_underscore").Render(_context), Is.EqualTo(" 1000$ "));
        }

        [Test]
        public void TestFilterWithNumericArgument()
        {
            _context["var"] = 1000L;
            _context.AddFilters(typeof(FiltersWithArgumentsInt));
            Assert.That(new Variable("var | adjust: 5").Render(_context), Is.EqualTo("[1005]"));
        }

        [Test]
        public void TestFilterWithNegativeArgument()
        {
            _context["var"] = 1000L;
            _context.AddFilters(typeof(FiltersWithArgumentsInt));
            Assert.That(new Variable("var | adjust: -5").Render(_context), Is.EqualTo("[995]"));
        }

        [Test]
        public void TestFilterWithDefaultArgument()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(FiltersWithArgumentsInt));
            Assert.That(new Variable("var | adjust").Render(_context), Is.EqualTo("[1010]"));
        }

        [Test]
        public void TestFilterWithTwoArguments()
        {
            _context["var"] = 1000L;
            _context.AddFilters(typeof(FiltersWithArgumentsInt));
            Assert.That(new Variable("var | add_sub: 200, 50").Render(_context), Is.EqualTo("[1150]"));
        }

        [Test]
        public void TestFilterWithNumericArgumentLong()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(FiltersWithArgumentsLong));
            Assert.That(new Variable("var | adjust: 5").Render(_context), Is.EqualTo("[1005]"));
        }

        [Test]
        public void TestFilterWithNegativeArgumentLong()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(FiltersWithArgumentsLong));
            Assert.That(new Variable("var | adjust: -5").Render(_context), Is.EqualTo("[995]"));
        }

        [Test]
        public void TestFilterWithDefaultArgumentLong()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(FiltersWithArgumentsLong));
            Assert.That(new Variable("var | adjust").Render(_context), Is.EqualTo("[1010]"));
        }

        [Test]
        public void TestFilterWithTwoArgumentsLong()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(FiltersWithArgumentsLong));
            Assert.That(new Variable("var | add_sub: 200, 50").Render(_context), Is.EqualTo("[1150]"));
        }

        [Test]
        public void TestFilterWithMultipleMethodSignatures()
        {
            Template.RegisterFilter(typeof(FiltersWithMultipleMethodSignatures));

            Assert.That(Template.Parse("{{'A' | concatenate : 'B'}}").Render(), Is.EqualTo("AB"));
            Assert.That(Template.Parse("{{'A' | concatenate : 'B', 'C'}}").Render(), Is.EqualTo("ABC"));
        }

        [Test]
        public void TestFilterInContextWithMultipleMethodSignatures()
        {
            _context.AddFilters(typeof(FiltersWithMultipleMethodSignatures));

            Assert.That(new Variable("'A' | concatenate : 'B'").Render(_context), Is.EqualTo("AB"));
            Assert.That(new Variable("'A' | concatenate : 'B', 'C'").Render(_context), Is.EqualTo("ABC"));
        }

        [Test]
        public void TestFilterWithMultipleMethodSignaturesAndContextParam()
        {
            Template.RegisterFilter(typeof(FiltersWithMultipleMethodSignaturesAndContextParam));

            Assert.That(Template.Parse("{{'A' | concat_with_context : 'B'}}").Render(), Is.EqualTo("AB"));
            Assert.That(Template.Parse("{{'A' | concat_with_context : 'B', 'C'}}").Render(), Is.EqualTo("ABC"));
        }

        [Test]
        public void TestFilterInContextWithMultipleMethodSignaturesAndContextParam()
        {
            _context.AddFilters(typeof(FiltersWithMultipleMethodSignaturesAndContextParam));

            Assert.That(new Variable("'A' | concat_with_context : 'B'").Render(_context), Is.EqualTo("AB"));
            Assert.That(new Variable("'A' | concat_with_context : 'B', 'C'").Render(_context), Is.EqualTo("ABC"));
        }

        [Test]
        public void TestFilterWithMultipleMethodSignaturesDifferentClasses()
        {
            Template.RegisterFilter(typeof(FiltersWithMultipleMethodSignaturesDifferentClassesOne));
            Template.RegisterFilter(typeof(FiltersWithMultipleMethodSignaturesDifferentClassesTwo));

            Assert.That(Template.Parse("{{'A' | concatenate : 'B'}}").Render(), Is.EqualTo("AB"));
            Assert.That(Template.Parse("{{'A' | concatenate : 'B', 'C'}}").Render(), Is.EqualTo("ABC"));
        }

        [Test]
        public void TestFilterInContextWithMultipleMethodSignaturesDifferentClasses()
        {
            _context.AddFilters(typeof(FiltersWithMultipleMethodSignaturesDifferentClassesOne));
            _context.AddFilters(typeof(FiltersWithMultipleMethodSignaturesDifferentClassesTwo));

            Assert.That(new Variable("'A' | concatenate : 'B'").Render(_context), Is.EqualTo("AB"));
            Assert.That(new Variable("'A' | concatenate : 'B', 'C'").Render(_context), Is.EqualTo("ABC"));
        }

        [Test]
        public void TestFilterAsLocalFilterWithMultipleMethodSignaturesDifferentClasses()
        {
            Helper.AssertTemplateResult(
                expected: "AB // ABC",
                template: "{{'A' | concatenate : 'B'}} // {{'A' | concatenate : 'B', 'C'}}",
                localVariables: null,
                localFilters: new[] { typeof(FiltersWithMultipleMethodSignaturesDifferentClassesOne), typeof(FiltersWithMultipleMethodSignaturesDifferentClassesTwo) });
        }

        [Test]
        public void TestFilterWithMultipleMethodSignaturesAndContextParamInDifferentClasses()
        {
            Template.RegisterFilter(typeof(FiltersWithMultipleMethodSignaturesDifferentClassesWithContextParamTwo));
            Template.RegisterFilter(typeof(FiltersWithMultipleMethodSignaturesDifferentClassesWithContextParamOne));
            Assert.That(Template.Parse("{{'A' | concat_with_context : 'B'}}").Render(), Is.EqualTo("AB"));
            Assert.That(Template.Parse("{{'A' | concat_with_context : 'B', 'C'}}").Render(), Is.EqualTo("ABC"));
        }

        [Test]
        public void TestFilterAsLocalFilterWithMultipleMethodSignaturesAndContextDifferentClasses()
        {

            Helper.AssertTemplateResult(
                expected: "AB // ABC",
                template: "{{'A' | concat_with_context : 'B'}} // {{'A' | concat_with_context : 'B', 'C'}}",
                localVariables: null,
                localFilters: new[] { typeof(FiltersWithMultipleMethodSignaturesDifferentClassesWithContextParamOne), typeof(FiltersWithMultipleMethodSignaturesDifferentClassesWithContextParamTwo) });
        }

        [Test]
        public void TestFilterInContextWithMultipleMethodSignaturesAndContextParamInDifferentClasses()
        {
            _context.AddFilters(typeof(FiltersWithMultipleMethodSignaturesDifferentClassesWithContextParamOne));
            _context.AddFilters(typeof(FiltersWithMultipleMethodSignaturesDifferentClassesWithContextParamTwo));

            Assert.That(new Variable("'A' | concat_with_context : 'B'").Render(_context), Is.EqualTo("AB"));
            Assert.That(new Variable("'A' | concat_with_context : 'B', 'C'").Render(_context), Is.EqualTo("ABC"));
        }

        [Test]
        // When two methods with the same name and method signature are registered, the method that is added last is preferred.
        // This allows overriding any existing methods, including methods defined in the DotLiqid library.
        // This is useful in cases where a defined method may need to have a different behavior in certain contexts.
        public void TestFilterOverridesMethodWithSameMethodSignaturesDifferentClasses()
        {
            Template.RegisterFilter(typeof(FilterWithSameMethodSignatureDifferentClassTwo));
            Template.RegisterFilter(typeof(FilterWithSameMethodSignatureDifferentClassOne));

            Assert.That(Template.Parse("{{'A' | concatenate : 'B'}}").Render(), Is.EqualTo("ABClass One"));
            Assert.That(Template.Parse("{{'A' | concatenate : 'B'}}").Render(), Is.Not.EqualTo("ABClass Two"));
        }

        [Test]
        public void TestFilterInContextOverridesMethodWithSameMethodSignaturesDifferentClasses()
        {
            _context.AddFilters(typeof(FilterWithSameMethodSignatureDifferentClassOne));
            _context.AddFilters(typeof(FilterWithSameMethodSignatureDifferentClassTwo));

            Assert.That(new Variable("'A' | concatenate : 'B'").Render(_context), Is.EqualTo("ABClass Two"));
            Assert.That(new Variable("'A' | concatenate : 'B'").Render(_context), Is.Not.EqualTo("ABClass One"));
        }

        [Test]
        public void TestFilterAsLocalOverridesMethodWithSameMethodSignaturesDifferentClasses()
        {
            Helper.AssertTemplateResult(
                           expected: "ABClass One",
                           template: "{{'A' | concatenate : 'B'}}",
                           localVariables: null,
                           localFilters: new[] { typeof(FilterWithSameMethodSignatureDifferentClassTwo), typeof(FilterWithSameMethodSignatureDifferentClassOne) });
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
        public void TestSecondFilterOverwritesFirst()
        {
            _context["var"] = 1000;
            _context.AddFilters(typeof(MoneyFilter));
            _context.AddFilters(typeof(CanadianMoneyFilter));
            Assert.That(new Variable("var | money").Render(_context), Is.EqualTo(" 1000$ CAD "));
        }

        [Test]
        public void TestSize()
        {
            _context["var"] = "abcd";
            _context.AddFilters(typeof(MoneyFilter));
            Assert.That(new Variable("var | size").Render(_context), Is.EqualTo(4));
        }

        [Test]
        public void TestJoin()
        {
            _context["var"] = new[] { 1, 2, 3, 4 };
            Assert.That(new Variable("var | join").Render(_context), Is.EqualTo("1 2 3 4"));
        }

        [Test]
        public void TestSort()
        {
            _context["value"] = 3;
            _context["numbers"] = new[] { 2, 1, 4, 3 };
            _context["words"] = new[] { "expected", "as", "alphabetic" };
            _context["arrays"] = new[] { new[] { "flattened" }, new[] { "are" } };

            Assert.That(new Variable("numbers | sort").Render(_context) as IEnumerable, Is.EqualTo(new[] { 1, 2, 3, 4 }).AsCollection);
            Assert.That(new Variable("words | sort").Render(_context) as IEnumerable, Is.EqualTo(new[] { "alphabetic", "as", "expected" }).AsCollection);
            Assert.That(new Variable("value | sort").Render(_context) as IEnumerable, Is.EqualTo(new[] { 3 }).AsCollection);
            Assert.That(new Variable("arrays | sort").Render(_context) as IEnumerable, Is.EqualTo(new[] { "are", "flattened" }).AsCollection);
        }

        [Test]
        public void TestSplit()
        {
            _context["var"] = "a~b";
            Assert.That(new Variable("var | split:'~'").Render(_context), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void TestStripHtml()
        {
            _context["var"] = "<b>bla blub</a>";
            Assert.That(new Variable("var | strip_html").Render(_context), Is.EqualTo("bla blub"));
        }

        [Test]
        public void Capitalize()
        {
            _context["var"] = "blub";
            Assert.That(new Variable("var | capitalize").Render(_context), Is.EqualTo("Blub"));
        }

        [Test]
        public void Slice()
        {
            _context["var"] = "blub";
            Assert.That(new Variable("var | slice: 0, 1").Render(_context), Is.EqualTo("b"));
            Assert.That(new Variable("var | slice: 0, 2").Render(_context), Is.EqualTo("bl"));
            Assert.That(new Variable("var | slice: 1").Render(_context), Is.EqualTo("l"));
            Assert.That(new Variable("var | slice: 4, 1").Render(_context), Is.EqualTo(""));
            Assert.That(new Variable("var | slice: -2, 2").Render(_context), Is.EqualTo("ub"));
            Assert.That(new Variable("var | slice: 5, 1").Render(_context), Is.EqualTo(null));
        }

        [Test]
        public void TestLocalGlobal()
        {
            Template.RegisterFilter(typeof(MoneyFilter));

            Assert.That(Template.Parse("{{1000 | money}}").Render(), Is.EqualTo(" 1000$ "));
            Assert.That(Template.Parse("{{1000 | money}}").Render(new RenderParameters(CultureInfo.InvariantCulture) { Filters = new[] { typeof(CanadianMoneyFilter) } }), Is.EqualTo(" 1000$ CAD "));
            Assert.That(Template.Parse("{{1000 | money}}").Render(new RenderParameters(CultureInfo.InvariantCulture) { Filters = new[] { typeof(CanadianMoneyFilter) } }), Is.EqualTo(" 1000$ CAD "));
        }

        [Test]
        public void TestContextFilter()
        {
            _context["var"] = 1000;
            _context["name"] = "King Kong";
            _context.AddFilters(typeof(ContextFilters));
            Assert.That(new Variable("var | bank_statement").Render(_context), Is.EqualTo(" King Kong has 1000$ "));
        }
    }
}
