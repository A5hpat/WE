// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web;
using System.Web.Routing;

namespace AspDotNetStorefront.Routing.Constraints
{
	public class LegacyRouteGenerationEnabled : IRouteConstraint
	{
		protected readonly RoutingConfigurationProvider RoutingConfigurationProvider;

		public LegacyRouteGenerationEnabled()
		{
			RoutingConfigurationProvider = new RoutingConfigurationProvider();
		}

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			if(routeDirection != RouteDirection.UrlGeneration)
				return true;

			return RoutingConfigurationProvider
				.GetRoutingConfiguration()
				.LegacyRouteGenerationEnabled;
		}
	}
}
