using System.Collections;
using System.Threading.Tasks;
using DotLiquid.Exceptions;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class IfElseTests
    {
        [Test]
        public async Task TestIf()
        {
            await Helper.AssertTemplateResultAsync("  ", " {% if false %} this text should not go into the output {% endif %} ");
            await Helper.AssertTemplateResultAsync("  this text should go into the output  ", " {% if true %} this text should go into the output {% endif %} ");
            await Helper.AssertTemplateResultAsync("  you rock ?", "{% if false %} you suck {% endif %} {% if true %} you rock {% endif %}?");
        }

        [Test]
        public async Task TestIfElse()
        {
            await Helper.AssertTemplateResultAsync(" YES ", "{% if false %} NO {% else %} YES {% endif %}");
            await Helper.AssertTemplateResultAsync(" YES ", "{% if true %} YES {% else %} NO {% endif %}");
            await Helper.AssertTemplateResultAsync(" YES ", "{% if 'foo' %} YES {% else %} NO {% endif %}");
        }

        [Test]
        public async Task TestIfBoolean()
        {
            await Helper.AssertTemplateResultAsync(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = true }));
        }

        [Test]
        public async Task TestIfOr()
        {
            await Helper.AssertTemplateResultAsync(" YES ", "{% if a or b %} YES {% endif %}", Hash.FromAnonymousObject(new { a = true, b = true }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if a or b %} YES {% endif %}", Hash.FromAnonymousObject(new { a = true, b = false }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if a or b %} YES {% endif %}", Hash.FromAnonymousObject(new { a = false, b = true }));
            await Helper.AssertTemplateResultAsync("", "{% if a or b %} YES {% endif %}", Hash.FromAnonymousObject(new { a = false, b = false }));

            await Helper.AssertTemplateResultAsync(" YES ", "{% if a or b or c %} YES {% endif %}",
                Hash.FromAnonymousObject(new { a = false, b = false, c = true }));
            await Helper.AssertTemplateResultAsync("", "{% if a or b or c %} YES {% endif %}",
                Hash.FromAnonymousObject(new { a = false, b = false, c = false }));
        }

        [Test]
        public async Task TestIfOrWithOperators()
        {
            await Helper.AssertTemplateResultAsync(" YES ", "{% if a == true or b == true %} YES {% endif %}",
                Hash.FromAnonymousObject(new { a = true, b = true }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if a == true or b == false %} YES {% endif %}",
                Hash.FromAnonymousObject(new { a = true, b = true }));
            await Helper.AssertTemplateResultAsync("", "{% if a == false or b == false %} YES {% endif %}",
                Hash.FromAnonymousObject(new { a = true, b = true }));
        }

        [Test]
        public void TestComparisonOfStringsContainingAndOrOr()
        {
            Assert.DoesNotThrow(() =>
            {
                const string awfulMarkup = "a == 'and' and b == 'or' and c == 'foo and bar' and d == 'bar or baz' and e == 'foo' and foo and bar";
                Hash assigns = Hash.FromAnonymousObject(new { a = "and", b = "or", c = "foo and bar", d = "bar or baz", e = "foo", foo = true, bar = true });
                Helper.AssertTemplateResultAsync(" YES ", "{% if " + awfulMarkup + " %} YES {% endif %}", assigns).GetAwaiter().GetResult();
            });
        }

        [Test]
        public async Task TestIfAnd()
        {
            await Helper.AssertTemplateResultAsync(" YES ", "{% if true and true %} YES {% endif %}");
            await Helper.AssertTemplateResultAsync("", "{% if false and true %} YES {% endif %}");
            await Helper.AssertTemplateResultAsync("", "{% if false and true %} YES {% endif %}");
        }

        [Test]
        public async Task TestHashMissGeneratesFalse()
        {
            await Helper.AssertTemplateResultAsync("", "{% if foo.bar %} NO {% endif %}", Hash.FromAnonymousObject(new { foo = new Hash() }));
        }

        [Test]
        public async Task TestIfFromVariable()
        {
            const object nullValue = null;

            await Helper.AssertTemplateResultAsync("", "{% if var %} NO {% endif %}", Hash.FromAnonymousObject(new { var = false }));
            await Helper.AssertTemplateResultAsync("", "{% if var %} NO {% endif %}", Hash.FromAnonymousObject(new { var = nullValue }));
            await Helper.AssertTemplateResultAsync("", "{% if foo.bar %} NO {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = false }) }));
            await Helper.AssertTemplateResultAsync("", "{% if foo.bar %} NO {% endif %}", Hash.FromAnonymousObject(new { foo = new Hash() }));
            await Helper.AssertTemplateResultAsync("", "{% if foo.bar %} NO {% endif %}", Hash.FromAnonymousObject(new { foo = nullValue }));
            await Helper.AssertTemplateResultAsync("", "{% if foo.bar %} NO {% endif %}", Hash.FromAnonymousObject(new { foo = true }));

            await Helper.AssertTemplateResultAsync(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = "text" }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = true }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = 1 }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = new Hash() }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = new object[] { } }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if 'foo' %} YES {% endif %}");
            await Helper.AssertTemplateResultAsync(" YES ", "{% if foo.bar %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = true }) }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if foo.bar %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = "text" }) }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if foo.bar %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = 1 }) }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if foo.bar %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = new Hash() }) }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if foo.bar %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = new object[] { } }) }));

            await Helper.AssertTemplateResultAsync(" YES ", "{% if var %} NO {% else %} YES {% endif %}", Hash.FromAnonymousObject(new { var = false }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if var %} NO {% else %} YES {% endif %}", Hash.FromAnonymousObject(new { var = nullValue }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if var %} YES {% else %} NO {% endif %}", Hash.FromAnonymousObject(new { var = true }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if 'foo' %} YES {% else %} NO {% endif %}", Hash.FromAnonymousObject(new { var = "text" }));

            await Helper.AssertTemplateResultAsync(" YES ", "{% if foo.bar %} NO {% else %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = false }) }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if foo.bar %} YES {% else %} NO {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = true }) }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if foo.bar %} YES {% else %} NO {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = "text" }) }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if foo.bar %} NO {% else %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { notbar = true }) }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if foo.bar %} NO {% else %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = new Hash() }));
            await Helper.AssertTemplateResultAsync(" YES ", "{% if foo.bar %} NO {% else %} YES {% endif %}",
                Hash.FromAnonymousObject(new { notfoo = Hash.FromAnonymousObject(new { bar = true }) }));
        }

        [Test]
        public async Task TestNestedIf()
        {
            await Helper.AssertTemplateResultAsync("", "{% if false %}{% if false %} NO {% endif %}{% endif %}");
            await Helper.AssertTemplateResultAsync("", "{% if false %}{% if true %} NO {% endif %}{% endif %}");
            await Helper.AssertTemplateResultAsync("", "{% if true %}{% if false %} NO {% endif %}{% endif %}");
            await Helper.AssertTemplateResultAsync(" YES ", "{% if true %}{% if true %} YES {% endif %}{% endif %}");

            await Helper.AssertTemplateResultAsync(" YES ",
                "{% if true %}{% if true %} YES {% else %} NO {% endif %}{% else %} NO {% endif %}");
            await Helper.AssertTemplateResultAsync(" YES ",
                "{% if true %}{% if false %} NO {% else %} YES {% endif %}{% else %} NO {% endif %}");
            await Helper.AssertTemplateResultAsync(" YES ",
                "{% if false %}{% if true %} NO {% else %} NONO {% endif %}{% else %} YES {% endif %}");
        }

        [Test]
        public async Task TestComparisonsOnNull()
        {
            await Helper.AssertTemplateResultAsync("", "{% if null < 10 %} NO {% endif %}");
            await Helper.AssertTemplateResultAsync("", "{% if null <= 10 %} NO {% endif %}");
            await Helper.AssertTemplateResultAsync("", "{% if null >= 10 %} NO {% endif %}");
            await Helper.AssertTemplateResultAsync("", "{% if null > 10 %} NO {% endif %}");

            await Helper.AssertTemplateResultAsync("", "{% if 10 < null %} NO {% endif %}");
            await Helper.AssertTemplateResultAsync("", "{% if 10 <= null %} NO {% endif %}");
            await Helper.AssertTemplateResultAsync("", "{% if 10 >= null %} NO {% endif %}");
            await Helper.AssertTemplateResultAsync("", "{% if 10 > null %} NO {% endif %}");
        }

        [Test]
        public async Task TestElseIf()
        {
            await Helper.AssertTemplateResultAsync("0", "{% if 0 == 0 %}0{% elsif 1 == 1%}1{% else %}2{% endif %}");
            await Helper.AssertTemplateResultAsync("1", "{% if 0 != 0 %}0{% elsif 1 == 1%}1{% else %}2{% endif %}");
            await Helper.AssertTemplateResultAsync("2", "{% if 0 != 0 %}0{% elsif 1 != 1%}1{% else %}2{% endif %}");

            await Helper.AssertTemplateResultAsync("elsif", "{% if false %}if{% elsif true %}elsif{% endif %}");
        }

        [Test]
        public void TestSyntaxErrorNoVariable()
        {
            Assert.Throws<SyntaxException>(() => Helper.AssertTemplateResultAsync("", "{% if jerry == 1 %}").GetAwaiter().GetResult());
        }

        [Test]
        public void TestSyntaxErrorNoExpression()
        {
            Assert.Throws<SyntaxException>(() => Helper.AssertTemplateResultAsync("", "{% if %}").GetAwaiter().GetResult());
        }

        [Test]
        public async Task TestIfWithCustomCondition()
        {
            DotLiquid.ConditionOperatorDelegate oldCondition = Condition.Operators["contains"];
            Condition.Operators["contains"] = (left, right) => (left is IList) ? ((IList) left).Contains(right) : ((left is string) ? ((string) left).Contains((string) right) : false);

            try
            {
                await Helper.AssertTemplateResultAsync("yes", "{% if 'bob' contains 'o' %}yes{% endif %}");
                await Helper.AssertTemplateResultAsync("no", "{% if 'bob' contains 'f' %}yes{% else %}no{% endif %}");
            }
            finally
            {
                Condition.Operators["contains"] = oldCondition;
            }
        }

        [Test]
        public void TestIfMaxConditions()
        {
            var se = Assert.Throws<SyntaxException>(() => Helper.AssertTemplateResultAsync("", "{% if 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 and 1 %}too many conditions{% endif %}").GetAwaiter().GetResult());

            StringAssert.Contains("'if'", se.Message);
            StringAssert.Contains("tag", se.Message);
            StringAssert.Contains("500", se.Message);
        }

    }
}
