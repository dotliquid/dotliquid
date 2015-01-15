using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace DotLiquid.Website.Controllers
{
	[HandleError]
	public class TryOnlineController : Controller
	{
		public ActionResult Index()
		{
			const string templateCode = @"&lt;p&gt;{{ user.name | upcase }} has to do:&lt;/p&gt;

&lt;ul&gt;
{% for item in user.tasks -%}
  &lt;li&gt;{{ item.name }}&lt;/li&gt;
{% endfor -%}
&lt;/ul&gt;";

			string result = LiquifyInternal(templateCode);

			ViewData["TemplateCode"] = templateCode;
			ViewData["Result"] = result;

			return View();
		}

		[HttpPost]
		public ActionResult Liquify(string templateCode)
		{
			string result = LiquifyInternal(HttpUtility.HtmlDecode(templateCode));

			return new ContentResult
			{
				Content = result
			};
		}

		private static string LiquifyInternal(string templateCode)
		{
			Template template = Template.Parse(templateCode);
			return template.Render(Hash.FromAnonymousObject(new
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