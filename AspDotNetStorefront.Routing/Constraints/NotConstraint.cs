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
	public class NotConstraint : IRouteConstraint
	{
		readonly IRouteConstraint Constraint;

		public NotConstraint(IRouteConstraint constraint)
		{
			Constraint = constraint;
		}

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			return !Constraint.Match(httpContext, route, parameterName, values, routeDirection);
		}
	}
}
