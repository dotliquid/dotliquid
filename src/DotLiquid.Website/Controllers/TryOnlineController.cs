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
			string result = LiquifyInternal(templateCode);

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
					Cities = new List<City>
					{
						new City { Name = "Rajkot" },
						new City { Name = "Ahmedabad" }
					}
					
				}
			}));
		}
	}

	public class User : Drop
	{
		public string Name { get; set; }
		public List<Task> Tasks { get; set; }
		public List<City> Cities {get; set;}
	}

	public class Task : Drop
	{
		public string Name { get; set; }
	}
	
	public class City : Drop
	{
		public String Name { get; set;}
	}
}
