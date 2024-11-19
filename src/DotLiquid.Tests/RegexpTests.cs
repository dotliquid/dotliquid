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
#if !NETCOREAPP1_0
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
                        Assert.That(RegexOptions.Compiled & ((Regex) t.GetValue(null)).Options, Is.Not.EqualTo(RegexOptions.None));
                    }
                }
            }
        }
#endif

        [Test]
        public void TestEmpty()
        {
            Assert.That(Run(string.Empty, Liquid.QuotedFragment), Is.Empty);
        }

        [Test]
        public void TestQuote()
        {
            Assert.That(Run("\"arg 1\"", Liquid.QuotedFragment), Is.EqualTo(new[] { "\"arg 1\"" }).AsCollection);
        }

        [Test]
        public void TestWords()
        {
            Assert.That(Run("arg1 arg2", Liquid.QuotedFragment), Is.EqualTo(new[] { "arg1", "arg2" }).AsCollection);
        }

        [Test]
        public void TestTags()
        {
            Assert.That(Run("<tr> </tr>", Liquid.QuotedFragment), Is.EqualTo(new[] { "<tr>", "</tr>" }).AsCollection);
            Assert.That(Run("<tr></tr>", Liquid.QuotedFragment), Is.EqualTo(new[] { "<tr></tr>" }).AsCollection);
            Assert.That(Run("<style class=\"hello\">' </style>", Liquid.QuotedFragment), Is.EqualTo(new[] { "<style", "class=\"hello\">", "</style>" }).AsCollection);
        }

        [Test]
        public void TestQuotedWords()
        {
            Assert.That(Run("arg1 arg2 \"arg 3\"", Liquid.QuotedFragment), Is.EqualTo(new[] { "arg1", "arg2", "\"arg 3\"" }).AsCollection);
        }

        [Test]
        public void TestQuotedWords2()
        {
            Assert.That(Run("arg1 arg2 'arg 3'", Liquid.QuotedFragment), Is.EqualTo(new[] { "arg1", "arg2", "'arg 3'" }).AsCollection);
        }

        [Test]
        public void TestQuotedWordsInTheMiddle()
        {
            Assert.That(Run("arg1 arg2 \"arg 3\" arg4", Liquid.QuotedFragment), Is.EqualTo(new[] { "arg1", "arg2", "\"arg 3\"", "arg4" }).AsCollection);
        }

        private static List<string> Run(string input, string pattern)
        {
            return R.Scan(input, new Regex(pattern));
        }
    }
}
