using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DotLiquid.Website.Controllers
{
    public class DocsController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GettingStarted()
        {
            return View();
        }

        public IActionResult Extending()
        {
            return View();
        }

        public IActionResult Formatting()
        {
            return View();
        }

        public IActionResult Drops()
        {
            return View();
        }

        public IActionResult Filters()
        {
            return View();
        }
    }
}
