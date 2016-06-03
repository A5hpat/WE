// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Routing
{
	public interface IRoutingConfigurationProvider
	{
		RoutingConfiguration GetRoutingConfiguration();
	}

	public class RoutingConfiguration
	{
		public readonly bool LegacyRouteGenerationEnabled;
		public readonly bool LegacyRouteRecognitionEnabled;
		public readonly bool LegacyRoutes301RedirectEnabled;
		public readonly bool SeNameOnlyRoutesEnabled;

		public RoutingConfiguration(bool legacyRouteGenerationEnabled, bool legacyRouteRecognitionEnabled, bool legacyRoutes301RedirectEnabled, bool seNameOnlyRoutesEnabled)
		{
			LegacyRouteGenerationEnabled = legacyRouteGenerationEnabled;
			LegacyRouteRecognitionEnabled = legacyRouteRecognitionEnabled;
			LegacyRoutes301RedirectEnabled = legacyRoutes301RedirectEnabled;
			SeNameOnlyRoutesEnabled = seNameOnlyRoutesEnabled;
		}
	}
}
