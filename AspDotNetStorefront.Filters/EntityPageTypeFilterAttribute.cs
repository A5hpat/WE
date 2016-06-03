// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Filters
{
	/// <summary>
	/// Sets the page type based on the entity type route data value.
	/// </summary>
	public class EntityPageTypeFilterAttribute : PageTypeFilterAttribute
	{
		public EntityPageTypeFilterAttribute()
			: base(PageTypes.Entity)
		{ }

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var entityType = filterContext.RouteData.Values[RouteDataKeys.EntityType] as string;
			if(string.IsNullOrEmpty(entityType))
				return;

			filterContext.RouteData.Values.Add(RouteDataKeys.PageType, entityType.ToLower());
		}
	}
}
