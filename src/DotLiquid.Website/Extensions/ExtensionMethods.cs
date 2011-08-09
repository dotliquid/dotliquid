using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

public static class ExtensionMethods
{
	public static bool IsCurrentAction(this HtmlHelper helper, string actionName, string controllerName)
	{
		string currentControllerName = (string) helper.ViewContext.RouteData.Values["controller"];
		string currentActionName = (string) helper.ViewContext.RouteData.Values["action"];

		if (currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase) && (actionName == null || currentActionName.Equals(actionName, StringComparison.CurrentCultureIgnoreCase)))
		{
			return true;
		}

		return false;
	}
}