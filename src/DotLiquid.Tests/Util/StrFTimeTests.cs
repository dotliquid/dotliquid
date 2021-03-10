using System;
using System.Globalization;
using System.Threading;
using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests.Util
{
    [Description("See https://help.shopify.com/themes/liquid/filters/additional-filters#date")]
    [TestFixture]
    public class StrFTimeTests
    {
        [TestCase("%a", ExpectedResult = "Sun")]
        [TestCase("%A", ExpectedResult = "Sunday")]
        [TestCase("%b", ExpectedResult = "Jan")]
        [TestCase("%B", ExpectedResult = "January")]
        [TestCase("%c", ExpectedResult = "Sun Jan 08 14:32:14 2012")]
        [TestCase("%C", ExpectedResult = "20")]
        [TestCase("%d", ExpectedResult = "08")]
        [TestCase("%e", ExpectedResult = " 8")]
        [TestCase("%h", ExpectedResult = "Jan")]
        [TestCase("%H", ExpectedResult = "14")]
        [TestCase("%I", ExpectedResult = "02")]
        [TestCase("%j", ExpectedResult = "008")]
        [TestCase("%k", ExpectedResult = "14")]
        [TestCase("%l", ExpectedResult = "2")]
        [TestCase("%m", ExpectedResult = "01")]
        [TestCase("%M", ExpectedResult = "32")]
        [TestCase("%P", ExpectedResult = "pm")]
        [TestCase("%p", ExpectedResult = "PM")]
        [TestCase("%S", ExpectedResult = "14")]
        [TestCase("%u", ExpectedResult = "7")]
        [TestCase("%U", ExpectedResult = "02")]
        [TestCase("%W", ExpectedResult = "01")]
        [TestCase("%w", ExpectedResult = "0")]
        [TestCase("%x", ExpectedResult = "08/01/2012")]
        [TestCase("%X", ExpectedResult = "14:32:14")]
        [TestCase("%y", ExpectedResult = "12")]
        [TestCase("%Y", ExpectedResult = "2012")]
        [TestCase("%", ExpectedResult = "%")]
        public string TestFormat(string format) {
            using (CultureHelper.SetCulture("en-GB")) {
                Assert.That(CultureInfo.CurrentCulture, Is.EqualTo(new CultureInfo("en-GB")));
                var date = new DateTime(2012, 1, 8, 14, 32, 14);
                var localResult = date.ToStrFTime(format);
                var utcResult = new DateTimeOffset(date, TimeSpan.FromHours(0)).ToStrFTime(format);
                Assert.AreEqual(localResult, utcResult);
                var estResult = new DateTimeOffset(date, TimeSpan.FromHours(-5)).ToStrFTime(format);
                Assert.AreEqual(utcResult, estResult);
                return localResult;
            }
        }

        [Test]
        public void TestEpoch() {
            using (CultureHelper.SetCulture("en-GB")) {
                Assert.That(CultureInfo.CurrentCulture, Is.EqualTo(new CultureInfo("en-GB")));
                var date = new DateTime(2012, 1, 8, 14, 32, 14);
                var localResult = date.ToStrFTime("%s");
                Assert.AreEqual("1326033134", localResult);
                var utcResult = new DateTimeOffset(2012, 1, 8, 14, 32, 14, TimeSpan.FromHours(0)).ToStrFTime("%s");
                Assert.AreEqual("1326033134", utcResult);
                var estResult = new DateTimeOffset(2012, 1, 8, 9, 32, 14, TimeSpan.FromHours(-5)).ToStrFTime("%s");
                Assert.AreEqual("1326033134", estResult);
            }
        }

        [Test]
        public void TestTimeZone()
        {
            var now = DateTimeOffset.Now;
            string timeZoneOffset = now.ToString("zzz");
            Assert.That(now.DateTime.ToStrFTime("%Z"), Is.EqualTo(timeZoneOffset));
        }
    }
}
