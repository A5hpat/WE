// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Filters
{
	public class PageTypeFilterAttribute : FilterAttribute, IActionFilter
	{
		readonly string PageType;

		public PageTypeFilterAttribute(string pageType)
		{
			if(String.IsNullOrEmpty(pageType))
				throw new ArgumentNullException("pageType");

			PageType = pageType;
		}

		public virtual void OnActionExecuting(ActionExecutingContext filterContext)
		{
			filterContext.RouteData.Values.Add(RouteDataKeys.PageType, PageType);
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{ }
	}
}
