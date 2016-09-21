using Microsoft.AspNetCore.Mvc;

namespace DotLiquid.Website.Controllers
{
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