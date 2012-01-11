using System.Web.Mvc;

namespace DotLiquid.Website.Controllers
{
	[HandleError]
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}