// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Routing
{
	public class RoutingConfigurationProvider : IRoutingConfigurationProvider
	{
		const string UrlModeGlobalConfigName = "UrlMode";
		const string SeNameOnlyRoutesEnabledGlobalConfigName = "EnableSeNameOnlyUrls";

		const string LegacyMode = "Legacy Only";
		const string Legacy301Mode = "Modern with Legacy 301 Redirects";
		const string ModernMode = "Modern Only";

		public RoutingConfiguration GetRoutingConfiguration()
		{
			var urlMode = AppLogic.GlobalConfig(UrlModeGlobalConfigName);
			var seNameOnlyRoutesEnabled = AppLogic.GlobalConfigBool(SeNameOnlyRoutesEnabledGlobalConfigName);

			return new RoutingConfiguration(
				legacyRouteGenerationEnabled: StringComparer.OrdinalIgnoreCase.Equals(LegacyMode, urlMode),
				legacyRouteRecognitionEnabled: new[] { LegacyMode, Legacy301Mode, }.Contains(urlMode, StringComparer.OrdinalIgnoreCase),
				legacyRoutes301RedirectEnabled: StringComparer.OrdinalIgnoreCase.Equals(Legacy301Mode, urlMode),
				seNameOnlyRoutesEnabled: seNameOnlyRoutesEnabled);
		}
	}
}
