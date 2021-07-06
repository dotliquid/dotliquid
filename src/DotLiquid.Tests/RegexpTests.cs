using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class RegexpTests
    {
        [Test]
        public void TestAllRegexesAreCompiled()
        {
            var assembly = typeof (Template).GetTypeInfo().Assembly;
            foreach (Type parent in assembly.GetTypes())
            {
                foreach (var t in parent.GetTypeInfo().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (t.FieldType == typeof(Regex))
                    {
                        if (t.IsStatic)
                        {
                            Assert.AreNotEqual(0, RegexOptions.Compiled & ((Regex) t.GetValue(null)).Options);
                        }
                        else
                        {
                            Assert.AreNotEqual(0, RegexOptions.Compiled & ((Regex)t.GetValue(parent)).Options);
                        }

                        //Trace.TraceInformation(parent.Name + ": " + t.Name);
                    }
                }
            }
        }

        [Test]
        public void TestEmpty()
        {
            CollectionAssert.IsEmpty(Run(string.Empty, Liquid.QuotedFragment));
        }

        [Test]
        public void TestQuote()
        {
            CollectionAssert.AreEqual(new[] { "\"arg 1\"" }, Run("\"arg 1\"", Liquid.QuotedFragment));
        }

        [Test]
        public void TestWords()
        {
            CollectionAssert.AreEqual(new[] { "arg1", "arg2" }, Run("arg1 arg2", Liquid.QuotedFragment));
        }

        [Test]
        public void TestTags()
        {
            CollectionAssert.AreEqual(new[] { "<tr>", "</tr>" }, Run("<tr> </tr>", Liquid.QuotedFragment));
            CollectionAssert.AreEqual(new[] { "<tr></tr>" }, Run("<tr></tr>", Liquid.QuotedFragment));
            CollectionAssert.AreEqual(new[] { "<style", "class=\"hello\">", "</style>" }, Run("<style class=\"hello\">' </style>", Liquid.QuotedFragment));
        }

        [Test]
        public void TestQuotedWords()
        {
            CollectionAssert.AreEqual(new[] { "arg1", "arg2", "\"arg 3\"" }, Run("arg1 arg2 \"arg 3\"", Liquid.QuotedFragment));
        }

        [Test]
        public void TestQuotedWords2()
        {
            CollectionAssert.AreEqual(new[] { "arg1", "arg2", "'arg 3'" }, Run("arg1 arg2 'arg 3'", Liquid.QuotedFragment));
        }

        [Test]
        public void TestQuotedWordsInTheMiddle()
        {
            CollectionAssert.AreEqual(new[] { "arg1", "arg2", "\"arg 3\"", "arg4" }, Run("arg1 arg2 \"arg 3\" arg4", Liquid.QuotedFragment));
        }

        [Test]
        public void TestVariableParser()
        {
            CollectionAssert.AreEqual(new[] { "var" }, Run("var", Liquid.VariableParser));
            CollectionAssert.AreEqual(new[] { "var", "method" }, Run("var.method", Liquid.VariableParser));
            CollectionAssert.AreEqual(new[] { "var", "[method]" }, Run("var[method]", Liquid.VariableParser));
            CollectionAssert.AreEqual(new[] { "var", "[method]", "[0]" }, Run("var[method][0]", Liquid.VariableParser));
            CollectionAssert.AreEqual(new[] { "var", "[\"method\"]", "[0]" }, Run("var[\"method\"][0]", Liquid.VariableParser));
            CollectionAssert.AreEqual(new[] { "var", "[method]", "[0]", "method" }, Run("var[method][0].method", Liquid.VariableParser));
        }

        private static List<string> Run(string input, string pattern)
        {
            return R.Scan(input, new Regex(pattern));
        }
    }
}
