﻿using System;
using DotLiquid.Util;
using NUnit.Framework;

namespace DotLiquid.Tests.Util
{
	[TestFixture]
	public class StrFTimeTests
	{
		[SetCulture("en-GB")]
		[TestCase("%a", Result = "Sun")]
		[TestCase("%A", Result = "Sunday")]
		[TestCase("%b", Result = "Jan")]
		[TestCase("%B", Result = "January")]
		[TestCase("%^B", Result = "JANUARY")]
		[TestCase("%^_10B", Result = "   JANUARY")]
		[TestCase("%_^10B", Result = "   JANUARY")]
		[TestCase("%c", Result = "Sun Jan 08 14:32:14 2012")]
		[TestCase("%d", Result = "08")]
		[TestCase("%-d", Result = "8")]
		[TestCase("%e", Result = " 8")]
		[TestCase("%H", Result = "14")]
		[TestCase("%I", Result = "02")]
		[TestCase("%j", Result = "008")]
		[TestCase("%m", Result = "01")]
		[TestCase("%-m", Result = "1")]
		[TestCase("%_m", Result = " 1")]
		[TestCase("%_3m", Result = "  1")]
		[TestCase("%_10m", Result = "         1")]
		[TestCase("%010m", Result = "0000000001")]
		[TestCase("%M", Result = "32")]
		[TestCase("%p", Result = "PM")]
		[TestCase("%S", Result = "14")]
		[TestCase("%U", Result = "02")]
		[TestCase("%-U", Result = "2")]
		[TestCase("%W", Result = "01")]
		[TestCase("%w", Result = "0")]
		[TestCase("%x", Result = "08/01/2012")]
		[TestCase("%X", Result = "14:32:14")]
		[TestCase("%y", Result = "12")]
		[TestCase("%Y", Result = "2012")]
		[TestCase("%_2Y", Result = "2012")]
		[TestCase("%", Result = "%")]
		public string TestFormat(string format)
		{
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