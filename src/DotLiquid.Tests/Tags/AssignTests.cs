using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
	[TestFixture]
	public class AssignTests
	{
		[Test]
		public void TestAssignedVariable()
		{
			Helper.AssertTemplateResult(".foo.", "{% assign foo = values %}.{{ foo[0] }}.",
				Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
			Helper.AssertTemplateResult(".bar.", "{% assign foo = values %}.{{ foo[1] }}.",
				Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
		}

		[Test]
		public void TestAssignDecimal()
		{
			Helper.AssertTemplateResult(string.Format("10{0}05", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
				"{% assign foo = decimal %}{{ foo }}",
				Hash.FromAnonymousObject(new { @decimal = 10.05d }));
		}

		[Test, SetCulture("en-GB")]
		public void TestAssignDecimalInlineWithEnglishDecimalSeparator()
		{
			Helper.AssertTemplateResult(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
				"{% assign foo = 2.5 %}{{ foo }}");
		}

		[Test, SetCulture("en-GB")]
		public void TestAssignDecimalInlineWithEnglishGroupSeparator()
		{
			Helper.AssertTemplateResult("2500",
				"{% assign foo = 2,500 %}{{ foo }}");
		}

		[Test, SetCulture("fr-FR")]
		public void TestAssignDecimalInlineWithFrenchDecimalSeparator()
		{
			Helper.AssertTemplateResult(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
				"{% assign foo = 2,5 %}{{ foo }}");
		}

		[Test, SetCulture("fr-FR")]
		public void TestAssignDecimalInlineWithInvariantDecimalSeparatorInFrenchCulture()
		{
			Helper.AssertTemplateResult(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
				"{% assign foo = 2.5 %}{{ foo }}");
		}

		[Test]
		public void TestAssignWithFilter()
		{
			Helper.AssertTemplateResult(".bar.", "{% assign foo = values | split: ',' %}.{{ foo[1] }}.", 
				Hash.FromAnonymousObject(new { values = "foo,bar,baz" }));
		}

		private class AssignDrop : Drop
		{
			public string MyProperty
			{
				get { return "MyValue"; }
			}
		}

		[Test]
		public void TestAssignWithDrop()
		{
			Helper.AssertTemplateResult(".MyValue.", @"{% assign foo = value %}.{{ foo.my_property }}.",
				Hash.FromAnonymousObject(new { value = new AssignDrop() }));
		}
	}
}