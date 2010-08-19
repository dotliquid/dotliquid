using NUnit.Framework;

namespace DotLiquid.Tests
{
	public class Helper
	{
		public static void AssertTemplateResult(string expected, string template, Hash localVariables)
		{
			Assert.AreEqual(expected, Template.Parse(template).Render(localVariables));
		}

		public static void AssertTemplateResult(string expected, string template)
		{
			AssertTemplateResult(expected, template, null);
		}
	}
}