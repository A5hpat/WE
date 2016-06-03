// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web;
using System.Web.Routing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Routing.Constraints
{
	public class IsEntityType : IRouteConstraint
	{
		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			if(!values.ContainsKey(parameterName))
				return false;

			var value = values[parameterName] as string;
			if(string.IsNullOrEmpty(value))
				return false;

			return AppLogic.ro_SupportedEntities.Contains(value, StringComparer.OrdinalIgnoreCase);
		}
	}
}
