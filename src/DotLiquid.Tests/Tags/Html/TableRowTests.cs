using System;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags.Html
{
    [TestFixture]
    public class TableRowTests
    {
        [Test]
        public void TestHtmlTable()
        {
            Helper.AssertTemplateResult(
                expected: string.Format("<tr class=\"row1\">{0}<td class=\"col1\"> 1 </td><td class=\"col2\"> 2 </td><td class=\"col3\"> 3 </td></tr>{0}<tr class=\"row2\"><td class=\"col1\"> 4 </td><td class=\"col2\"> 5 </td><td class=\"col3\"> 6 </td></tr>{0}", Environment.NewLine),
                template: "{% tablerow n in numbers cols:3%} {{n}} {% endtablerow %}",
                localVariables: Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3, 4, 5, 6 } }));

            Helper.AssertTemplateResult(
                expected: string.Format("<tr class=\"row1\">{0}</tr>{0}", Environment.NewLine),
                template: "{% tablerow n in numbers cols:3%} {{n}} {% endtablerow %}",
                localVariables: Hash.FromAnonymousObject(new { numbers = new int[] { } }));
        }

        [Test]
        public void TestHtmlTableWithDefaultCols()
        {
            Helper.AssertTemplateResult(
                expected: string.Format("<tr class=\"row1\">{0}<td class=\"col1\"> 1 </td><td class=\"col2\"> 2 </td><td class=\"col3\"> 3 </td></tr>{0}", Environment.NewLine),
                template: "{% tablerow n in numbers %} {{n}} {% endtablerow %}",
                localVariables: Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3 } }));
        }

        [Test]
        public void TestHtmlTableWithDifferentCols()
        {
            Helper.AssertTemplateResult(
                expected: string.Format("<tr class=\"row1\">{0}<td class=\"col1\"> 1 </td><td class=\"col2\"> 2 </td><td class=\"col3\"> 3 </td><td class=\"col4\"> 4 </td><td class=\"col5\"> 5 </td></tr>{0}<tr class=\"row2\"><td class=\"col1\"> 6 </td></tr>{0}", Environment.NewLine),
                template: "{% tablerow n in numbers cols:5%} {{n}} {% endtablerow %}",
                localVariables: Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3, 4, 5, 6 } }));
        }

        [Test]
        public void TestHtmlColCounter()
        {
            Helper.AssertTemplateResult(
                expected: string.Format("<tr class=\"row1\">{0}<td class=\"col1\">1</td><td class=\"col2\">2</td></tr>{0}<tr class=\"row2\"><td class=\"col1\">1</td><td class=\"col2\">2</td></tr>{0}<tr class=\"row3\"><td class=\"col1\">1</td><td class=\"col2\">2</td></tr>{0}", Environment.NewLine),
                template: "{% tablerow n in numbers cols:2%}{{tablerowloop.col}}{% endtablerow %}",
                localVariables: Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3, 4, 5, 6 } }));
        }

        [Test]
        public void TestHtmlOffsetLimit()
        {
            Helper.AssertTemplateResult(
                expected: string.Format("<tr class=\"row1\">{0}<td class=\"col1\">2</td><td class=\"col2\">3</td></tr>{0}<tr class=\"row2\"><td class=\"col1\">4</td></tr>{0}", Environment.NewLine),
                template: "{% tablerow n in numbers cols:2 offset:1 limit:3 %}{{n}}{% endtablerow %}",
                localVariables: Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3, 4, 5, 6 } }));
        }

        [Test]
        public void TestDynamicVariableColsOffsetLimit()
        {
            Helper.AssertTemplateResult(
                expected: string.Format("<tr class=\"row1\">{0}<td class=\"col1\">2</td><td class=\"col2\">3</td></tr>{0}<tr class=\"row2\"><td class=\"col1\">4</td></tr>{0}", Environment.NewLine),
                template: "{% tablerow n in numbers cols:cols offset:1 limit:3 %}{{n}}{% endtablerow %}",
                localVariables: Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3, 4, 5, 6 }, cols = 2 }));

            Helper.AssertTemplateResult(
                expected: string.Format("<tr class=\"row1\">{0}<td class=\"col1\">2</td><td class=\"col2\">3</td></tr>{0}<tr class=\"row2\"><td class=\"col1\">4</td></tr>{0}", Environment.NewLine),
                template: "{% tablerow n in numbers cols:2 offset:offset limit:3 %}{{n}}{% endtablerow %}",
                localVariables: Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3, 4, 5, 6 }, offset = 1 }));

            Helper.AssertTemplateResult(
                expected: string.Format("<tr class=\"row1\">{0}<td class=\"col1\">2</td><td class=\"col2\">3</td></tr>{0}<tr class=\"row2\"><td class=\"col1\">4</td></tr>{0}", Environment.NewLine),
                template: "{% tablerow n in numbers cols:2 offset:1 limit:limit %}{{n}}{% endtablerow %}",
                localVariables: Hash.FromAnonymousObject(new { numbers = new[] { 1, 2, 3, 4, 5, 6 }, limit = 3 }));
        }
    }
}
