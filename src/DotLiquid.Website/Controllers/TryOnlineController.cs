using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DotLiquid.Website.Controllers
{
    public class TryOnlineController : Controller
    {
        public async Task<ActionResult> Index()
        {
            const string templateCode = @"&lt;p&gt;{{ user.name | upcase }} has to do:&lt;/p&gt;

&lt;ul&gt;
{% for item in user.tasks -%}
  &lt;li&gt;{{ item.name }}&lt;/li&gt;
{% endfor -%}
&lt;/ul&gt;";

            string result = await LiquifyInternalAsync(templateCode);

            ViewData["TemplateCode"] = templateCode;
            ViewData["Result"] = result;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LiquifyAsync(string templateCode)
        {
            string result = await LiquifyInternalAsync(templateCode);

            return new ContentResult
            {
                Content = result
            };
        }

        private static Task<string> LiquifyInternalAsync(string templateCode)
        {
            Template template = Template.Parse(templateCode);
            return template.RenderAsync(Hash.FromAnonymousObject(new
            {
                user = new User
                {
                    Name = "Tim Jones",
                    Tasks = new List<Task>
                    {
                        new Task { Name = "Documentation" },
                        new Task { Name = "Code comments" }
                    }
                }
            }));
        }
    }

    public class User : Drop
    {
        public string Name { get; set; }
        public List<Task> Tasks { get; set; }
    }

    public class Task : Drop
    {
        public string Name { get; set; }
    }
}
