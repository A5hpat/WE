// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web;
using System.Web.Routing;

namespace AspDotNetStorefront.Routing
{
	public class RedirectToRoutePreserveMethodHttpHandler : IHttpHandler
	{
		public bool IsReusable
		{ get { return false; } }

		readonly RouteValueDictionary RouteDataValues;

		public RedirectToRoutePreserveMethodHttpHandler(RouteValueDictionary routeDataValues)
		{
			RouteDataValues = routeDataValues;
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.RedirectToRoute(RouteDataValues);
			context.Response.StatusCode = 307;
			context.ApplicationInstance.CompleteRequest();
		}
	}
}
