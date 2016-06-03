// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;

namespace AspDotNetStorefront.Classes
{
	public static class ViewEngineCollectionExtensions
	{
		public static string FindViewPath(this ViewEngineCollection viewEngines, ControllerContext controllerContext, string viewName, string masterName = null)
		{
			var viewResult = ViewEngines.Engines.FindView(controllerContext, viewName, masterName ?? string.Empty);
			if(viewResult == null)
				throw new Exception(string.Format("The specified view {0} could not be found.", viewName));

			var view = viewResult.View as RazorView;
			if(viewResult == null)
				throw new Exception(string.Format("The specified view {0} must be a Razor view.", viewName));

			return view.ViewPath;
		}
	}
}
