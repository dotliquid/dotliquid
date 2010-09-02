using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DotLiquid.Website.Controllers
{
	[HandleError]
	public class TryOnlineController : Controller
	{
		public ActionResult Index()
		{
			string templateCode =
				@"{{ user.name }} has to do:
{% for item in user.tasks %}
  * {{ item.name }}
{% endfor %}";

			ViewData["TemplateCode"] = templateCode;

			return Liquify(templateCode);
		}

		[HttpPost]
		public ActionResult Liquify(string templateCode)
		{
			Template template = Template.Parse(templateCode);
			string templateResult = template.Render(Hash.FromAnonymousObject(new
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

			// Replace line breaks with <br />'s for display.
			templateResult = templateResult
				.Replace(" ", "&nbsp;")
				.Replace(Environment.NewLine, "<br />");

			ViewData["TemplateResult"] = templateResult;

			return View("Index");
		}
	}

	public class User : Drop
	{
		public string Name { get; set; }
		public List<Task> Tasks { get; set; }
	}

	public class Task : Drop
	{
		public string Name { get; set;	 }
	}
}