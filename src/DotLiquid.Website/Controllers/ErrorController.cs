using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Net;

namespace DotLiquid.Website.Controllers
{
    public class ErrorController : Controller
    {
        private static readonly MediaTypeHeaderValue _textHtmlMediaType = new MediaTypeHeaderValue("text/html");

        public IActionResult Index() => HttpStatusCodeHandler((HttpStatusCode)Response.StatusCode);

        [Route("Error/{statusCode:required:range(400,599)}")]
        public IActionResult HttpStatusCodeHandler(HttpStatusCode statusCode)
        {
            HttpContext.Response.StatusCode = (int)statusCode;

            // If the client ask for HTML return friendly errors
            var headers = HttpContext.Request.GetTypedHeaders();
            if (headers.Accept?.Any(h => h.IsSubsetOf(_textHtmlMediaType)) == true)
            {
                switch (statusCode)
                {
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.InternalServerError:
                        return View(statusCode.ToString());
                }
            }

            return new EmptyResult();
        }
    }
}