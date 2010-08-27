using System.Web.Mvc;
using DotLiquid.Website.Controllers;
using NUnit.Framework;

namespace DotLiquid.Website.Tests.Controllers
{
	[TestFixture]
	public class HomeControllerTests
	{
		[Test]
		public void IndexActionTest()
		{
			// Arrange
			HomeController controller = new HomeController();

			// Act
			ViewResult result = controller.Index() as ViewResult;

			// Assert
			Assert.IsNotNull(result);
		}
	}
}