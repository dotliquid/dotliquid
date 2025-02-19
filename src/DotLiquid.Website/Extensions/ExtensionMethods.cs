using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DotLiquid.Website
{
    public static class ExtensionMethods
    {
        public static bool IsCurrentAction(this IHtmlHelper helper, string actionName, string controllerName)
        {
            string currentControllerName = (string)helper.ViewContext.RouteData.Values["controller"];
            string currentActionName = (string)helper.ViewContext.RouteData.Values["action"];

            if (currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase) && (actionName == null || currentActionName.Equals(actionName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }
    }
}