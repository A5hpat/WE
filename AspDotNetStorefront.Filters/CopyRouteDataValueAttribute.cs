// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;

namespace AspDotNetStorefront.Filters
{
	/// <summary>
	/// Copies the specified route data value into the route tokens. Intended to support legacy routing values.
	/// </summary>
	public class CopyRouteDataValueAttribute : FilterAttribute, IActionFilter
	{
		public bool Overwrite
		{ get; set; }

		readonly string SourceKey;
		readonly string DestinationKey;

		public CopyRouteDataValueAttribute(string sourceKey, string destinationKey)
		{
			SourceKey = sourceKey;
			DestinationKey = destinationKey;
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var sourceExists = filterContext.RequestContext.RouteData.Values.ContainsKey(SourceKey);
			if(!sourceExists)
				return;

			var destinationExists = filterContext.RequestContext.RouteData.DataTokens.ContainsKey(DestinationKey);
			if(destinationExists && !Overwrite)
				return;

			var sourceValue = filterContext.RequestContext.RouteData.Values[SourceKey];
			filterContext.RequestContext.RouteData.DataTokens[DestinationKey] = sourceValue;
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{ }
	}
}
