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
        [SetCulture("en-GB")]
        [TestCase("%a", ExpectedResult = "Sun")]
        [TestCase("%A", ExpectedResult = "Sunday")]
        [TestCase("%b", ExpectedResult = "Jan")]
        [TestCase("%B", ExpectedResult = "January")]
        [TestCase("%c", ExpectedResult = "Sun Jan 08 14:32:14 2012")]
        [TestCase("%d", ExpectedResult = "08")]
        [TestCase("%e", ExpectedResult = " 8")]
        [TestCase("%H", ExpectedResult = "14")]
        [TestCase("%I", ExpectedResult = "02")]
        [TestCase("%j", ExpectedResult = "008")]
        [TestCase("%m", ExpectedResult = "01")]
        [TestCase("%M", ExpectedResult = "32")]
        [TestCase("%p", ExpectedResult = "PM")]
        [TestCase("%S", ExpectedResult = "14")]
        [TestCase("%U", ExpectedResult = "02")]
        [TestCase("%W", ExpectedResult = "01")]
        [TestCase("%w", ExpectedResult = "0")]
        [TestCase("%x", ExpectedResult = "08/01/2012")]
        [TestCase("%X", ExpectedResult = "14:32:14")]
        [TestCase("%y", ExpectedResult = "12")]
        [TestCase("%Y", ExpectedResult = "2012")]
        [TestCase("%", ExpectedResult = "%")]
        public string TestFormat(string format)
        {
            Assert.That(CultureInfo.CurrentCulture, Is.EqualTo(new CultureInfo("en-GB")));
            return new DateTime(2012, 1, 8, 14, 32, 14).ToStrFTime(format);
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
