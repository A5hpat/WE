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
	public class RedirectToRoutePermanentHttpHandler : IHttpHandler
	{
		public bool IsReusable
		{ get { return false; } }

		readonly RouteValueDictionary RouteDataValues;

		public RedirectToRoutePermanentHttpHandler(RouteValueDictionary routeDataValues)
		{
			RouteDataValues = routeDataValues;
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.RedirectToRoutePermanent(RouteDataValues);
			context.ApplicationInstance.CompleteRequest();
		}
	}
}
