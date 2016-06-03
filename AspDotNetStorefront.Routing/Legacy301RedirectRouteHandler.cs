// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AspDotNetStorefront.Routing
{
	public class Legacy301RedirectRouteHandler : IRouteHandler
	{
		readonly IRouteHandler Handler;
		readonly RoutingConfigurationProvider RoutingConfigurationProvider;

		public Legacy301RedirectRouteHandler(IRouteHandler handler = null)
		{
			Handler = handler ?? new MvcRouteHandler();
			RoutingConfigurationProvider = new RoutingConfigurationProvider();
		}

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			if(RoutingConfigurationProvider.GetRoutingConfiguration().LegacyRoutes301RedirectEnabled)
				return requestContext.HttpContext.Request.HttpMethod.Equals("GET", System.StringComparison.InvariantCultureIgnoreCase)
					? new RedirectToRoutePermanentHttpHandler(requestContext.RouteData.Values) as IHttpHandler
					: new RedirectToRoutePreserveMethodHttpHandler(requestContext.RouteData.Values) as IHttpHandler;

			return Handler.GetHttpHandler(requestContext);
		}
	}
}
