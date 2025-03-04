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

        private static List<string> Run(string input, string pattern)
        {
            return R.Scan(input, new Regex(pattern));
        }

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

        [Test]
        public void TestScanWithCallbackNoMatch()
        {
            string markup = "nomatch";
            Dictionary<string, string> d = new Dictionary<string, string>();
            R.Scan(markup, Liquid.TagAttributesRegex, (key, value) => d[key] = value);

            Assert.That(d.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestScanWithCallbackOneMatch()
        {
            string markup = "hello:world";
            Dictionary<string, string> d = new Dictionary<string, string>();
            R.Scan(markup, Liquid.TagAttributesRegex, (key, value) => d[key] = value);

            Assert.That(d.Count, Is.EqualTo(1));
            Assert.That(d["hello"], Is.EqualTo("world"));
        }

        [Test]
        public void TestScanWithCallbackTwoMatches()
        {
            string markup = "hello:world, hello_again:another_world";
            Dictionary<string, string> d = new Dictionary<string, string>();
            R.Scan(markup, Liquid.TagAttributesRegex, (key, value) => d[key] = value);

            Assert.That(d.Count, Is.EqualTo(2));
            Assert.That(d["hello"], Is.EqualTo("world"));
            Assert.That(d["hello_again"], Is.EqualTo("another_world"));
        }

        #region Tests of obsolete functions

        [Obsolete("Tests obsolete function")]
        private static List<string> Run_Obsolete(string input, string pattern)
        {
            return R.Scan(input, pattern);
        }

        [Test]
        [Obsolete("Tests obsolete function")]
        public void TestEmpty_Obsolete()
        {
            Assert.That(Run_Obsolete(string.Empty, Liquid.QuotedFragment), Is.Empty);
        }

        [Test]
        [Obsolete("Tests obsolete function")]
        public void TestQuote_Obsolete()
        {
            Assert.That(Run_Obsolete("\"arg 1\"", Liquid.QuotedFragment), Is.EqualTo(new[] { "\"arg 1\"" }).AsCollection);
        }

        [Test]
        [Obsolete("Tests obsolete function")]
        public void TestWords_Obsolete()
        {
            Assert.That(Run_Obsolete("arg1 arg2", Liquid.QuotedFragment), Is.EqualTo(new[] { "arg1", "arg2" }).AsCollection);
        }

        [Test]
        [Obsolete("Tests obsolete function")]
        public void TestTags_Obsolete()
        {
            Assert.That(Run_Obsolete("<tr> </tr>", Liquid.QuotedFragment), Is.EqualTo(new[] { "<tr>", "</tr>" }).AsCollection);
            Assert.That(Run_Obsolete("<tr></tr>", Liquid.QuotedFragment), Is.EqualTo(new[] { "<tr></tr>" }).AsCollection);
            Assert.That(Run_Obsolete("<style class=\"hello\">' </style>", Liquid.QuotedFragment), Is.EqualTo(new[] { "<style", "class=\"hello\">", "</style>" }).AsCollection);
        }

        [Test]
        [Obsolete("Tests obsolete function")]
        public void TestQuotedWords_Obsolete()
        {
            Assert.That(Run_Obsolete("arg1 arg2 \"arg 3\"", Liquid.QuotedFragment), Is.EqualTo(new[] { "arg1", "arg2", "\"arg 3\"" }).AsCollection);
        }

        [Test]
        [Obsolete("Tests obsolete function")]
        public void TestQuotedWords2_Obsolete()
        {
            Assert.That(Run_Obsolete("arg1 arg2 'arg 3'", Liquid.QuotedFragment), Is.EqualTo(new[] { "arg1", "arg2", "'arg 3'" }).AsCollection);
        }

        [Test]
        [Obsolete("Tests obsolete function")]
        public void TestQuotedWordsInTheMiddle_Obsolete()
        {
            Assert.That(Run_Obsolete("arg1 arg2 \"arg 3\" arg4", Liquid.QuotedFragment), Is.EqualTo(new[] { "arg1", "arg2", "\"arg 3\"", "arg4" }).AsCollection);
        }

        [Test]
        [Obsolete("Tests obsolete function")]
        public void TestScanWithCallbackNoMatch_Obsolete()
        {
            string markup = "nomatch";
            Dictionary<string, string> d = new Dictionary<string, string>();

            R.Scan(markup, Liquid.TagAttributesRegex.ToString(), (key, value) => d[key] = value);

            Assert.That(d.Count, Is.EqualTo(0));
        }

        [Test]
        [Obsolete("Tests obsolete function")]
        public void TestScan_Obsolete()
        {
            string markup = "nomatch";
            List<string> matches = R.Scan(markup, Liquid.TagAttributesRegex);

            Assert.That(matches, Is.Not.Null);
            Assert.That(matches.Count, Is.EqualTo(0));
        }

        [Test]
        [Obsolete("Tests obsolete function")]
        public void TestScanWithCallbackOneMatch_Obsolete()
        {
            string markup = "hello:world";
            Dictionary<string, string> d = new Dictionary<string, string>();
            R.Scan(markup, Liquid.TagAttributesRegex.ToString(), (key, value) => d[key] = value);

            Assert.That(d.Count, Is.EqualTo(1));
            Assert.That(d["hello"], Is.EqualTo("world"));
        }

        [Test]
        [Obsolete("Tests obsolete function")]
        public void TestScanWithCallbackTwoMatches_Obsolete()
        {
            string markup = "hello:world, hello_again:another_world";
            Dictionary<string, string> d = new Dictionary<string, string>();
            R.Scan(markup, Liquid.TagAttributesRegex.ToString(), (key, value) => d[key] = value);

            Assert.That(d.Count, Is.EqualTo(2));
            Assert.That(d["hello"], Is.EqualTo("world"));
            Assert.That(d["hello_again"], Is.EqualTo("another_world"));
        }

        #endregion
    }
}
