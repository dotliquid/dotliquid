using DotLiquid.Exceptions;
using NUnit.Framework;
using System;
using System.Globalization;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class ExtendedFilterTests
    {
        private Context _context;

        [OneTimeSetUp]
        public void SetUp()
        {
            _context = new Context(CultureInfo.InvariantCulture);
            Template.RegisterFilter(typeof(ExtendedFilters));
        }

        [Test]
        public void TestTitleize()
        {
            var context = _context;
            Assert.AreEqual(null, ExtendedFilters.Titleize(context: context, input: null));
            Assert.AreEqual("", ExtendedFilters.Titleize(context: context, input: ""));
            Assert.AreEqual(" ", ExtendedFilters.Titleize(context: context, input: " "));
            Assert.AreEqual("That Is One Sentence.", ExtendedFilters.Titleize(context: context, input: "That is one sentence."));

            Helper.AssertTemplateResult(
                expected: "Title",
                template: "{{ 'title' | titleize }}");
        }

        [Test]
        public void TestUpcaseFirst()
        {
            var context = _context;
            Assert.AreEqual(null, ExtendedFilters.UpcaseFirst(context: context, input: null));
            Assert.AreEqual("", ExtendedFilters.UpcaseFirst(context: context, input: ""));
            Assert.AreEqual(" ", ExtendedFilters.UpcaseFirst(context: context, input: " "));
            Assert.AreEqual(" My boss is Mr. Doe.", ExtendedFilters.UpcaseFirst(context: context, input: " my boss is Mr. Doe."));

            Helper.AssertTemplateResult(
                expected: "My great title",
                template: "{{ 'my great title' | upcase_first }}");
        }

        // Unexpected input scenarios
        [TestCase(null, null, null, null)]
        [TestCase("", null, null, "")]
        [TestCase(0, null, null, 0)]
        [TestCase(true, "UTC", "MMMM", true)] // cannot be converted to a date-time
        [TestCase("1.0", null, null, "1.0")] // decimals are ignored
        [TestCase("1,000", null, null, "1,000")] // currency or number with separators are ignored.
        // Range checking scenarios
        [TestCase(0, null, "yyyy-MM-ddThh:mm:sszzz", "1970-01-01T12:00:00+00:00")] // The UNIX Epoch
        [TestCase("0", null, "yyyy-MM-ddThh:mm:sszzz", "1970-01-01T12:00:00+00:00")] // The UNIX Epoch
        [TestCase("-1", null, "yyyy-MM-ddThh:mm:sszzz", "1969-12-31T11:59:59+00:00")] // 1 second before the UNIX Epoch
        [TestCase("978307200000", null, "yyyy-MM-ddThh:mm:sszzz", "2001-01-01T12:00:00+00:00")] // milliseconds, the millennium
        [TestCase("13042800000", null, "yyyy-MM-ddThh:mm:sszzz", "1970-05-31T11:00:00+00:00")] // milliseconds, shortly after the epoch
        [TestCase("-62135596801", null, "yyyy-MM-ddThh:mm:sszzz", "1968-01-12T08:06:43+00:00")] // milliseconds, less than range supported by DateTimeOffset.FromUnixTimeSeconds
        [TestCase("253402300800", null, "yyyy-MM-ddThh:mm:sszzz", "1978-01-11T09:31:40+00:00")] // milliseconds, more than range supported by DateTimeOffset.FromUnixTimeSeconds
        // Timezone conversion scenarios
        [TestCase("1582967411000", "W. Europe Standard Time", "yyyy-MM-ddThh:mm:sszzz", "2020-02-29T10:10:11+01:00")] // milliseconds, Convert to CET
        [TestCase(1582967411000L, "W. Europe Standard Time", "yyyy-MM-ddThh:mm:sszzz", "2020-02-29T10:10:11+01:00")] // milliseconds, Convert to CET
        [TestCase(1582967411000L, "Eastern Standard Time", "yyyy-MM-ddThh:mm:sszzz", "2020-02-29T04:10:11-05:00")] // milliseconds, Convert to EST
        [TestCase(1590999011000, "W. Europe Standard Time", "yyyy-MM-ddThh:mm:sszzz", "2020-06-01T10:10:11+02:00")] // milliseconds, european summer time (DST)
        // DateTime.Parse scenarios
        [TestCase("01/Apr/2021", null, "yyyy-MM-dd", "2021-04-01")]
        [TestCase("2006-05-05 10:00:00.345", null, "ffffff", "345000")]
        [TestCase("01/Apr/2021", null, " ", "01/Apr/2021")] // Empty format won't change the input
        [TestCase("01/Apr/2021", null, null, "01/Apr/2021")] // null format won't change the input
        [TestCase("2006-06-05 10:00:00", null, "MMMM", "June")]
        [TestCase("2021-05-20T12:14:15-08:00", null, "yyyy-MM-ddThh:mm:sszzz", "2021-05-20T12:14:15-08:00")] // Ensure input timezone is preserved
        [TestCase("2021-06-20T12:14:15+09:00", null, "yyyy-MM-ddThh:mm:sszzz", "2021-06-20T12:14:15+09:00")] // Ensure input timezone is preserved
        [TestCase("2021-06-20T12:14:15+09:00", "UTC", "yyyy-MM-ddThh:mm:sszzz", "2021-06-20T03:14:15+00:00")] // Ensure input timezone is converted to UTC
        // Special word scenarios
        [TestCase("now", null, null, "now")]
        [TestCase("today", null, "  ", "today")]
        [TestCase("hi", null, "MMMM", "hi")] // Not a special word!
        public void TestConvertTime_Dotnet(object input, string convertToTimezoneId, string format, object expectedValue)
        {
            Assert.AreEqual(
                expected: expectedValue,
                actual: ExtendedFilters.ConvertTime(context: null, input: input, format: format, convertToTimezoneId: convertToTimezoneId));
        }

        [TestCase(1582967411000L, "Eastern Standard Time", "%s", "1582967411")] // %s (seconds since epoch) should not be affected by timeone
        [TestCase(1582967411000L, "W. Europe Standard Time", "%FT%T%:z", "2020-02-29T10:10:11+01:00")] // milliseconds, Convert to CET
        [TestCase(1582967411000L, "Eastern Standard Time", "%FT%T%:z", "2020-02-29T04:10:11-05:00")] // milliseconds, Convert to EST
        [TestCase(1590999011000, "W. Europe Standard Time", "%FT%T%:z", "2020-06-01T10:10:11+02:00")] // milliseconds, european summer time (DST)
        [TestCase(0L, "UTC", "%s", "0")]
        public void TestConvertTime_Ruby(object input, string convertToTimezoneId, string format, object expectedValue)
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                Liquid.UseRubyDateFormat = true;
                Assert.AreEqual(
                    expected: expectedValue,
                    actual: ExtendedFilters.ConvertTime(context: null, input: input, format: format, convertToTimezoneId: convertToTimezoneId));
            });
        }

        [Test]
        public void TestConvertTime()
        {
            Context context = new Context(CultureInfo.CurrentCulture);

            // DateTimeOffset / DateTime tests
            Assert.AreEqual(DateTime.Now.ToString("2020-02-29T09:10:11+00:00"), ExtendedFilters.ConvertTime(context: context, input: DateTimeOffset.Parse("2020-02-29T10:10:11+01:00"), format: "yyyy-MM-ddThh:mm:sszzz", convertToTimezoneId: "UTC"));
            Assert.AreEqual(DateTime.Now.ToString("2020-02-29T09:10:11+00:00"), ExtendedFilters.ConvertTime(context: context, input: DateTime.Parse("2020-02-29T10:10:11+01:00"), format: "yyyy-MM-ddThh:mm:sszzz", convertToTimezoneId: "UTC"));

            // Special Words (- ).NET)
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), ExtendedFilters.ConvertTime(context: context, input: "now", format: "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), ExtendedFilters.ConvertTime(context: context, input: "today", format: "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), ExtendedFilters.ConvertTime(context: context, input: "Now", format: "MM/dd/yyyy"));
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), ExtendedFilters.ConvertTime(context: context, input: "Today", format: "MM/dd/yyyy"));

            // Special Words (Ruby)
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                Liquid.UseRubyDateFormat = true; // TODO: use context.UseRubyDates if PR #450 is merged
                Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), ExtendedFilters.ConvertTime(context: context, input: "now", format: "%m/%d/%Y"));
                Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), ExtendedFilters.ConvertTime(context: context, input: "today", format: "%m/%d/%Y"));
                Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), ExtendedFilters.ConvertTime(context: context, input: "Now", format: "%m/%d/%Y"));
                Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), ExtendedFilters.ConvertTime(context: context, input: "Today", format: "%m/%d/%Y"));
            });

            Assert.Throws<SyntaxException>(() => ExtendedFilters.ConvertTime(context: context, input: 1590999011000, format: "yyyy-MM-ddThh:mm:sszzz", convertToTimezoneId: "Not a Real Timezone"));
        }
    }
}
