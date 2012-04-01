using System;
using System.Globalization;
using System.Threading;
using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests.Util
{
	[TestFixture]
	public class StrFTimeTests
	{
		[SetCulture("en-GB")]
		[TestCase("%a", Result = "Mon")]
		[TestCase("%A", Result = "Monday")]
		[TestCase("%b", Result = "Jan")]
		[TestCase("%B", Result = "January")]
		[TestCase("%c", Result = "Mon Jan 09 14:32:14 2012")]
		[TestCase("%d", Result = "09")]
		[TestCase("%e", Result = " 9")]
		[TestCase("%H", Result = "14")]
		[TestCase("%I", Result = "02")]
		[TestCase("%j", Result = "009")]
		[TestCase("%m", Result = "01")]
		[TestCase("%M", Result = "32")]
		[TestCase("%p", Result = "PM")]
		[TestCase("%S", Result = "14")]
		[TestCase("%U", Result = "02")]
		[TestCase("%W", Result = "03")]
		[TestCase("%w", Result = "1")]
		[TestCase("%x", Result = "09/01/2012")]
		[TestCase("%X", Result = "14:32:14")]
		[TestCase("%y", Result = "12")]
		[TestCase("%Y", Result = "2012")]
		[TestCase("%", Result = "%")]
		public string TestFormat(string format)
		{
			return new DateTime(2012, 1, 9, 14, 32, 14).ToStrFTime(format);
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