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

        // Positive cases...
        [TestCase("1582967411000", "2020-02-29T09:10:11+00:00")] // string
        [TestCase(1582967411000d, "2020-02-29T09:10:11+00:00")] // double
        [TestCase(1582967411000L, "2020-02-29T09:10:11+00:00")] // long
        [TestCase(1582967411000UL, "2020-02-29T09:10:11+00:00")] // ulong
        // Negative cases...
        [TestCase(null, null)] // null safe
        [TestCase("xxxxxxxx", "xxxxxxxx")] // ignore non-numeric string
        [TestCase("1,582,967,411,000", "1,582,967,411,000")] // ignore string with thousand separator
        [TestCase(0L, "1970-01-01T00:00:00+00:00")] // Epoch as long
        [TestCase(1582967411000f, 1582967411000f)] // Ignore float as its precision of ~6-9 digits cannot accurately represent a timestamp in milliseconds
        [TestCase(Int32.MaxValue, Int32.MaxValue)] // Ignore int, MaxValue ~= 1970-01-25T20:31:23+00:00
        [TestCase(UInt32.MaxValue, UInt32.MaxValue)] // Ignore uint, MaxValue ~= 1970-02-19T17:02:47+00:00
        public void TestUnixMs(object input, object expectedValue)
        {
            Helper.LockTemplateStaticVars(Template.NamingConvention, () =>
            {
                var actual = ExtendedFilters.UnixMs(context: _context, input: input);
                Assert.AreEqual(
                    expected: expectedValue,
                    actual: actual is DateTimeOffset actualDateTimeOffset ? actualDateTimeOffset.ToString("yyyy-MM-ddTHH:mm:sszzz") : actual);
            });
        }

        [Test]
        public void TestTimeZone()
        {
            Context context = new Context(CultureInfo.CurrentCulture);

            // Test UTC specified DateTime
            var utcDateTime = new DateTime(2020, 2, 29, 12, 10, 11, DateTimeKind.Utc);
            Assert.AreEqual("2020-02-29T12:10:11+00:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: utcDateTime, convertToTimezoneId: "UTC")));
            Assert.AreEqual("2020-02-29T12:10:11+00:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: utcDateTime, convertToTimezoneId: "Local")));
            Assert.AreEqual("2020-02-29T02:10:11-10:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: utcDateTime, convertToTimezoneId: "Hawaiian Standard Time")));

            // Test Local DateTime
            var localDateTime = new DateTime(2020, 2, 29, 12, 10, 11, DateTimeKind.Local);
            Assert.AreEqual("2020-02-29T12:10:11+00:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: localDateTime, convertToTimezoneId: "UTC")));
            Assert.AreEqual("2020-02-29T12:10:11+00:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: localDateTime, convertToTimezoneId: "Local")));
            Assert.AreEqual("2020-02-29T07:10:11-05:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: localDateTime, convertToTimezoneId: "Eastern Standard Time")));

            // Test Unspecified DateTime
            var unspecifiedDateTime = new DateTime(2020, 2, 29, 12, 10, 11, DateTimeKind.Unspecified);
            Assert.AreEqual("2020-02-29T12:10:11+00:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: unspecifiedDateTime, convertToTimezoneId: "UTC")));
            Assert.AreEqual("2020-02-29T12:10:11+00:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: unspecifiedDateTime, convertToTimezoneId: "Local")));
            Assert.AreEqual("2020-02-29T13:10:11+01:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: unspecifiedDateTime, convertToTimezoneId: "W. Europe Standard Time")));

            // Date string tests
            var stringDateTime = "2020-02-29T12:10:11";
            Assert.AreEqual("2020-02-29T12:10:11+00:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: stringDateTime, convertToTimezoneId: "UTC")));
            Assert.AreEqual("2020-02-29T12:10:11+00:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: stringDateTime, convertToTimezoneId: "Local")));
            Assert.AreEqual("2020-02-29T02:10:11-10:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: stringDateTime, convertToTimezoneId: "Hawaiian Standard Time")));

            // DateTimeOffset tests
            var dateTimeOffset = DateTimeOffset.Parse("2020-02-29T10:10:11+01:00");
            Assert.AreEqual("2020-02-29T09:10:11+00:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: dateTimeOffset, convertToTimezoneId: "UTC")));
            Assert.AreEqual("2020-02-29T09:10:11+00:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: dateTimeOffset, convertToTimezoneId: "Local")));
            Assert.AreEqual("2020-02-28T23:10:11-10:00", ToIso8601DateString(ExtendedFilters.TimeZone(context: context, input: dateTimeOffset, convertToTimezoneId: "Hawaiian Standard Time")));

            // Ignore non-date types
            Assert.AreEqual("xxxxx", ExtendedFilters.TimeZone(context: context, input: "xxxxx", convertToTimezoneId: "UTC"));
            Assert.AreEqual(1234567890L, ExtendedFilters.TimeZone(context: context, input: 1234567890L, convertToTimezoneId: "UTC"));
            Assert.AreEqual(1.0d, ExtendedFilters.TimeZone(context: context, input: 1.0d, convertToTimezoneId: "UTC"));

            // IANA timezone identifier (not be available until .NET6))
            Assert.Throws<SyntaxException>(() => ExtendedFilters.TimeZone(context: context, input: dateTimeOffset, convertToTimezoneId: "Europe/Paris"));

            // Unknown timezone
            Assert.Throws<SyntaxException>(() => ExtendedFilters.TimeZone(context: context, input: dateTimeOffset, convertToTimezoneId: "Not a Real Timezone"));
        }

        private static string ToIso8601DateString(object input)
        {
            return input is DateTimeOffset dateTimeOffset ? dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:sszzz") : input.ToString();
        }

        [Test]
        public void IntegrationTestTimeZone()
        {
            // Epoch millis long to ISO-8601 in UTC (1646136000000 -> 2022-03-01T12:00:00Z)
            Helper.AssertTemplateResult(
                expected: "2022-03-01T12:00:00+00:00",
                template: "{{1646136000000 | unix_ms | date: 'yyyy-MM-ddTHH:mm:sszzz'}}",
                syntax: SyntaxCompatibility.DotLiquid21); // DotLiquid21 to avoid legacy date parsing

            // Epoch millis string to ISO-8601 in EST (1646136000000 -> 2022-03-01T07:00:00-05:00)
            Helper.AssertTemplateResult(
                expected: "2022-03-01T07:00:00-05:00",
                template: "{{'1646136000000' | unix_ms | time_zone: 'Eastern Standard Time' | date: 'yyyy-MM-ddTHH:mm:sszzz'}}",
                syntax: SyntaxCompatibility.DotLiquid21); // DotLiquid21 to avoid legacy date parsing

            // Epoch seconds to ISO-8601 in EST (1646136000000 -> 2022-03-01T07:00:00-05:00)
            Helper.AssertTemplateResult(
                expected: "2022-03-01T07:00:00-05:00",
                template: "{{1646136000 | date: 'yyyy-MM-ddTHH:mm:sszzz' | time_zone: 'Eastern Standard Time' | date: 'yyyy-MM-ddTHH:mm:sszzz'}}",
                syntax: SyntaxCompatibility.DotLiquid21); // DotLiquid21 to avoid legacy date parsing

            // ISO-8601 UTC to ISO-8601 in EST (2000-01-01T00:00:00Z -> 1999-12-31T19:00:00-05:00)
            Helper.AssertTemplateResult(
                expected: "1999-12-31T19:00:00-05:00",
                template: "{{'2000-01-01T00:00:00Z' | time_zone: 'Eastern Standard Time' | date: 'yyyy-MM-ddTHH:mm:sszzz'}}",
                syntax: SyntaxCompatibility.DotLiquid21); // DotLiquid21 to avoid legacy date parsing
        }
    }
}
