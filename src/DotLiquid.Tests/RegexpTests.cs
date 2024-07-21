using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using DotLiquid.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
                foreach (var t in parent.GetTypeInfo().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (t.FieldType == typeof(Regex))
                    {
                        ClassicAssert.AreNotEqual(0, RegexOptions.Compiled & ((Regex) t.GetValue(null)).Options);
                    }
                }
            }
        }

        [Test]
        public void TestEmpty()
        {
            ClassicAssert.IsEmpty(Run(string.Empty, Liquid.QuotedFragment));
        }

        [Test]
        public void TestQuote()
        {
            ClassicAssert.AreEqual(new[] { "\"arg 1\"" }, Run("\"arg 1\"", Liquid.QuotedFragment));
        }

        [Test]
        public void TestWords()
        {
            ClassicAssert.AreEqual(new[] { "arg1", "arg2" }, Run("arg1 arg2", Liquid.QuotedFragment));
        }

        [Test]
        public void TestTags()
        {
            ClassicAssert.AreEqual(new[] { "<tr>", "</tr>" }, Run("<tr> </tr>", Liquid.QuotedFragment));
            ClassicAssert.AreEqual(new[] { "<tr></tr>" }, Run("<tr></tr>", Liquid.QuotedFragment));
            ClassicAssert.AreEqual(new[] { "<style", "class=\"hello\">", "</style>" }, Run("<style class=\"hello\">' </style>", Liquid.QuotedFragment));
        }

        [Test]
        public void TestQuotedWords()
        {
            ClassicAssert.AreEqual(new[] { "arg1", "arg2", "\"arg 3\"" }, Run("arg1 arg2 \"arg 3\"", Liquid.QuotedFragment));
        }

        [Test]
        public void TestQuotedWords2()
        {
            ClassicAssert.AreEqual(new[] { "arg1", "arg2", "'arg 3'" }, Run("arg1 arg2 'arg 3'", Liquid.QuotedFragment));
        }

        [Test]
        public void TestQuotedWordsInTheMiddle()
        {
            ClassicAssert.AreEqual(new[] { "arg1", "arg2", "\"arg 3\"", "arg4" }, Run("arg1 arg2 \"arg 3\" arg4", Liquid.QuotedFragment));
        }

        private static List<string> Run(string input, string pattern)
        {
            return R.Scan(input, new Regex(pattern));
        }
    }
}
