using DotLiquid.Website.Controllers;
using Microsoft.AspNetCore.Mvc;
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
            Assert.That(result, Is.Not.Null);
        }
    }
}
