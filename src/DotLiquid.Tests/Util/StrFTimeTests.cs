using System;
using System.Globalization;
using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests.Util
{
    [Description("See https://help.shopify.com/themes/liquid/filters/additional-filters#date")]
    [TestFixture]
    public class StrFTimeTests
    {
        private static readonly CultureInfo EN_GB = new CultureInfo("en-GB");

        // Descriptions below are taken from the Ruby Time.strftime documentation.
        [TestCase("%a", ExpectedResult = "Sun")] // Abbreviated weekday name (Mon)
        [TestCase("%A", ExpectedResult = "Sunday")] // Full weekday name (Monday)
        [TestCase("%b", ExpectedResult = "Jan")] // Abbreviated month name (Jan)
        [TestCase("%B", ExpectedResult = "January")] // Full month name (January)
        [TestCase("%^B", ExpectedResult = "JANUARY")] // Upcased month name
        [TestCase("%^_10B", ExpectedResult = "   JANUARY")] // Upcase, then Space-pad
        [TestCase("%_^10B", ExpectedResult = "   JANUARY")] // Space-pad, then Upcase
        [TestCase("%c", ExpectedResult = "Sun Jan 08 14:32:14 2012")] // Date and time representation (Thu Aug 23 14:55:02 2001)
        [TestCase("%C", ExpectedResult = "20")] //Century (20 in 2009)
        [TestCase("%d", ExpectedResult = "08")] // Day of Month, zero-padded (01..31)
        [TestCase("%-d", ExpectedResult = "8")] // Remove zero-padding
        [TestCase("%D", ExpectedResult = "01/08/12")] // Short date (%m/%d/%y)
        [TestCase("%e", ExpectedResult = " 8")] // Day of month, space-padded ( 1..31)
        // %E - not specified
        // %f - not specified
        [TestCase("%F", ExpectedResult = "2012-01-08")] // ISO 8601 date (2001-08-23)
        [TestCase("%g", ExpectedResult = "12")] // Week-based year, last two digits (00..99)
        [TestCase("%G", ExpectedResult = "2012")]  // Week based year (2001)
        [TestCase("%h", ExpectedResult = "Jan")] // Abbreviated month name (Jan)
        [TestCase("%H", ExpectedResult = "14")] // Hour in 24h format, zero-padded (00..23)
        // %i - not specified
        [TestCase("%I", ExpectedResult = "02")] // Hour in 12h format, zero-padded (01..12)
        [TestCase("%j", ExpectedResult = "008")] // Day of year (001..366)
        // %J - not specified
        [TestCase("%k", ExpectedResult = "14")] //Hour in 24-hour format, blank-padded ( 0..23)
        // %K - not specified
        [TestCase("%l", ExpectedResult = " 2")] //Hour in 12-hour format, blank-padded ( 0..12)
        [TestCase("%L", ExpectedResult = "123")] // Millisecond of the second (000..999). The digits under millisecond are truncated to not produce 1000.
        [TestCase("%m", ExpectedResult = "01")] // Month of the year, zero-padded (01..12)
        [TestCase("%-m", ExpectedResult = "1")] // Remove zero-padding
        [TestCase("%_m", ExpectedResult = " 1")] // Replace zero-pad with space-padding
        [TestCase("%_3m", ExpectedResult = "  1")] // Replace single zero-pad with multi-space-pad
        [TestCase("%010m", ExpectedResult = "0000000001")] // Replace single zero-pad with multi-zero-pad
        [TestCase("%M", ExpectedResult = "32")] // Minute of the hour (00..59)
        [TestCase("%n", ExpectedResult = "\n")] // new-line character (\n)
        [TestCase("%N", ExpectedResult = "123000")] // Fractional seconds digits, default is 9 digits (nanosecond) --> C# DateTime does not support nanosecond precision
        [TestCase("%3N", ExpectedResult = "123")] // ... millisecond (3 digits)
        [TestCase("%6N", ExpectedResult = "123000")] // ... microsecond (6 digits)
        // %9N - nanosecond (9 digits) --> C# DateTime does not support nanosecond precision
        // %o - not specified
        // %O - not specified
        [TestCase("%p", ExpectedResult = "PM")] // Meridian indicator, uppercase (AM/PM)
        [TestCase("%P", ExpectedResult = "pm")] // Meridian indicator, lowercase (am/pm)
        // %q - not specified
        // %Q - not specified
        [TestCase("%r", ExpectedResult = "02:32:14 PM")] // 12-hour clock time (02:55:02 PM)
        [TestCase("%R", ExpectedResult = "14:32")] // 24-hour HH:MM time, equivalent to %H:%M (14:55)
        // %s - see TestEpoch
        [TestCase("%S", ExpectedResult = "14")] //Second (00..60)
        [TestCase("%t", ExpectedResult = "\t")] // Tab character (\t)
        [TestCase("%T", ExpectedResult = "14:32:14")] // ISO 8601 time format (14:55:02)
        [TestCase("%u", ExpectedResult = "7")] // Day of the week, Monday is 1 (1..7)
        [TestCase("%U", ExpectedResult = "02")] // Week of the year, Sunday as the first day of week one (01..53)
        [TestCase("%v", ExpectedResult = " 8-JAN-2012")] // VMS date (%e-%^b-%4Y)
        [TestCase("%V", ExpectedResult = "01")] // Week number of the week-based year (01..53)
        [TestCase("%w", ExpectedResult = "0")] // Day of the week, Sunday is 0 (0..6)
        [TestCase("%W", ExpectedResult = "01")] // Week number with the first Monday as the first day of week one (00..53)
        [TestCase("%x", ExpectedResult = "01/08/12")] // Locale Date representation (08/23/01)
        [TestCase("%X", ExpectedResult = "14:32:14")] // Locale Time representation (14:55:02)
        [TestCase("%y", ExpectedResult = "12")] // Year, last two digits (00-99)
        [TestCase("%Y", ExpectedResult = "2012")] // Year with century (2001)
        [TestCase("%_2Y", ExpectedResult = "2012")] // Ensure space-pad does not truncate.
        // %z, %:z, %::z --> See TestTimeZoneUTC/TestTimeZoneLocal
        [TestCase("%", ExpectedResult = "%")]
        [TestCase("%%", ExpectedResult = "%")]
        public string TestFormat(string format)
        {
            var date = new DateTime(2012, 1, 8, 14, 32, 14, 123);
            var localResult = date.ToStrFTime(format, EN_GB);
            var utcResult = new DateTimeOffset(date, TimeSpan.FromHours(0)).ToStrFTime(format, EN_GB);
            Assert.AreEqual(localResult, utcResult);
            var estResult = new DateTimeOffset(date, TimeSpan.FromHours(-5)).ToStrFTime(format, EN_GB);
            Assert.AreEqual(utcResult, estResult);
            return localResult;
        }

        [Test]
        // %s - Gives number of seconds since 1970-01-01 00:00:00 UTC
        public void TestEpoch()
        {
            var date = new DateTime(2012, 1, 8, 14, 32, 14, 123);
            var localResult = date.ToStrFTime("%s", EN_GB);
            Assert.AreEqual("1326033134", localResult);
            var utcResult = new DateTimeOffset(date, TimeSpan.FromHours(0)).ToStrFTime("%s", EN_GB);
            Assert.AreEqual("1326033134", utcResult);
            var estResult = new DateTimeOffset(date, TimeSpan.FromHours(-5)).ToStrFTime("%s", EN_GB);
            Assert.AreEqual("1326051134", estResult);
        }

        [Test]
        public void TestTimeZone()
        {
            var now = DateTimeOffset.Now;
            string timeZoneOffset = now.ToString("zzz");
            Assert.That(now.DateTime.ToStrFTime("%Z", EN_GB), Is.EqualTo(timeZoneOffset));
        }

        [TestCase("%z", ExpectedResult = "+0000")] // hour and minute offset from UTC without a colon
        [TestCase("%:z", ExpectedResult = "+00:00")] // hour and minute offset from UTC with a colon
        public string TestTimeZoneUTC(string format)
        {
            return DateTimeOffset.UtcNow.ToStrFTime(format, EN_GB);
        }

        [TestCase("%z", ExpectedResult = "-0500")] // hour and minute offset from UTC without a colon
        [TestCase("%:z", ExpectedResult = "-05:00")] // hour and minute offset from UTC with a colon
        public string TestTimeZoneLocal(string format)
        {
            return DateTimeOffset.Parse("2012-06-10T14:32:14-05:00").ToStrFTime(format, EN_GB);
        }

        // '2012-12-31' is considered to be in the first week of 2013 (according to ISO-8601).
        [TestCase("%G", ExpectedResult = "2013")]
        [TestCase("%g", ExpectedResult = "13")]
        [TestCase("%V", ExpectedResult = "01")]
        public string TestIso8601WeekBasedDates(string format)
        {
            return DateTime.Parse("2012-12-31").ToStrFTime(format, EN_GB);
        }

        [Test]
        public void TestToStrFTime_ArgumentException()
        {
            var dateTime = new DateTime(2012, 1, 8, 14, 32, 14, 123);
            var format = "%g";
            CultureInfo cultureInfo = null;
            Assert.Throws<ArgumentException>(() => StrFTime.ToStrFTime(dateTime, format, cultureInfo));

            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.FromHours(0));
            Assert.Throws<ArgumentException>(() => StrFTime.ToStrFTime(dateTimeOffset, format, cultureInfo));
        }
    }
}
