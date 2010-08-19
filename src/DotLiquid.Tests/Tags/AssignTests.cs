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
				Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } } ));
			Helper.AssertTemplateResult(".bar.", "{% assign foo = values %}.{{ foo[1] }}.",
				Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
		}
	}
}