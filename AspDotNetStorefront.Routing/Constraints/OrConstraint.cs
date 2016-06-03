// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace AspDotNetStorefront.Routing.Constraints
{
	public class OrConstraint : IRouteConstraint
	{
		readonly IEnumerable<IRouteConstraint> Constraints;

		public OrConstraint(params IRouteConstraint[] constraints)
		{
			Constraints = constraints;
		}

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			return Constraints
				.Where(constraint => constraint.Match(httpContext, route, parameterName, values, routeDirection))
				.Any();
		}
	}
}
