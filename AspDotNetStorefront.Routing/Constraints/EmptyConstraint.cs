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
	public class EmptyConstraint : IRouteConstraint
	{
		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			// The key has to exist, but be empty
			if(!values.ContainsKey(parameterName))
				return false;

			// If it's null, no matter the type, we treat that as empty
			if(values[parameterName] == null)
				return true;

			// If it's not null and not a string, we treat that as not empty
			if(!(values[parameterName] is string))
				return false;

			// Since it has to be a string at this point, check if it's empty
			return (string)values[parameterName] == string.Empty;
		}
	}
}
