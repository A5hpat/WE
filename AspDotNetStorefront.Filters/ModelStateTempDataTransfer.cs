// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;

namespace AspDotNetStorefront.Filters
{
	public class ExportModelStateToTempData : ActionFilterAttribute
	{
		public const string Key = "ExportedModelState";

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			// Only export when ModelState is not valid
			if(!filterContext.Controller.ViewData.ModelState.IsValid)
				// Export if we are redirecting
				if((filterContext.Result is RedirectResult) || (filterContext.Result is RedirectToRouteResult))
					filterContext.Controller.TempData[Key] = filterContext.Controller.ViewData.ModelState;

			base.OnActionExecuted(filterContext);
		}
	}

	public class ImportModelStateFromTempData : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var modelState = filterContext.Controller.TempData[ExportModelStateToTempData.Key] as ModelStateDictionary;

			// Only Import if we are viewing
			if(modelState != null && filterContext.Result is ViewResultBase)
				filterContext.Controller.ViewData.ModelState.Merge(modelState);

			base.OnActionExecuted(filterContext);
		}
	}
}
