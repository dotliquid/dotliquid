using System.Web.Mvc;

namespace DotLiquid.Website.Controllers
{
	[HandleError]
	public class ErrorController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult PageNotFound()
		{
			return View();
		}
	}
}