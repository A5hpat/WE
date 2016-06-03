// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;

namespace AspDotNetStorefront.Routing
{
	public class AdnsfRoute : Route
	{
		readonly IEnumerable<RouteDataValueTransform> RouteDataValueTransforms;
		readonly IEnumerable<string> QueryStringRouteDataValues;

		public AdnsfRoute(string url, RouteValueDictionary defaults = null, RouteValueDictionary constraints = null, RouteValueDictionary dataTokens = null, IRouteHandler routeHandler = null, IEnumerable<RouteDataValueTransform> routeDataValueTransforms = null, IEnumerable<string> queryStringRouteDataValues = null)
			: base(url, defaults, constraints, dataTokens, routeHandler)
		{
			RouteDataValueTransforms = routeDataValueTransforms ?? Enumerable.Empty<RouteDataValueTransform>();
			QueryStringRouteDataValues = queryStringRouteDataValues ?? Enumerable.Empty<string>();
		}

		public override RouteData GetRouteData(HttpContextBase httpContext)
		{
			// Invoke the internal RouteParser class via reflection to create a ParsedRoute instance from the route's URL template.
			var parsedRoute = new RouteParserReflectionProxy().Parse(Url);

			// Build the full requested path
			var requestedVirtualPath = string.Format(
				"{0}{1}",
				httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2),
				httpContext.Request.PathInfo);

			// Parse the requested url into a RouteValueDictionary, including the route's defaults
			var routeDataValues = parsedRoute.Match(requestedVirtualPath, Defaults);
			if(routeDataValues == null)
				return null;

			// Apply customizaed ADNSF behavior
			ApplyQueryStringToRouteDataValues(httpContext.Request.QueryString, routeDataValues);
			ApplyRouteDataValueTransformations(routeDataValues, RouteDirection.IncomingRequest);

			// Validate all constraints are met
			if(Constraints != null)
				foreach(var constraint in Constraints)
				{
					var constraintResult = ProcessConstraint(httpContext, constraint.Value, constraint.Key, routeDataValues, RouteDirection.IncomingRequest);
					if(!constraintResult)
						return null;
				}

			// Prepare the RouteData object to return
			var routeData = new RouteData(this, RouteHandler);

			// Copy in the route data values, including the query string includes and transformations
			foreach(var routeDataEntry in routeDataValues)
				routeData.Values.Add(routeDataEntry.Key, routeDataEntry.Value);

			// Copy in any data tokens
			if(DataTokens != null)
				foreach(var dataToken in DataTokens)
					routeData.DataTokens.Add(dataToken.Key, dataToken.Value);

			return routeData;
		}

		public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
		{
			// We need to modify the route data values for purposes of generating a URL, but we don't want to modify the collection 
			// that's passed in. We'll clone it and modify the clone.
			var routeDataValues = new RouteValueDictionary(values);

			// Apply customizaed ADNSF behavior
			ApplyQueryStringToRouteDataValues(requestContext.HttpContext.Request.QueryString, routeDataValues);
			ApplyRouteDataValueTransformations(routeDataValues, RouteDirection.UrlGeneration);

			// Invoke the internal RouteParser class via reflection to create a ParsedRoute instance from the route's URL template.
			var parsedRoute = new RouteParserReflectionProxy().Parse(Url);

			// Invoke the internal Bind method to create a URL and new route data values from the provided route data.
			var boundUrl = parsedRoute.Bind(requestContext.RouteData.Values, routeDataValues, Defaults, Constraints);
			if(boundUrl == null)
				return null;

			// Run the constraints against the new route data values
			if(Constraints != null)
				foreach(var constraint in Constraints)
				{
					var constraintResult = ProcessConstraint(requestContext.HttpContext, constraint.Value, constraint.Key, boundUrl.Values, RouteDirection.UrlGeneration);
					if(!constraintResult)
						return null;
				}

			// Construct the return value from the bound URL and this route
			var virtualPathData = new VirtualPathData(this, boundUrl.Url);

			// Copy in any data tokens
			if(DataTokens != null)
				foreach(var dataToken in DataTokens)
					virtualPathData.DataTokens[dataToken.Key] = dataToken.Value;

			return virtualPathData;
		}

		void ApplyQueryStringToRouteDataValues(NameValueCollection queryString, RouteValueDictionary routeDataValues)
		{
			// Pull in any query string values that match the QueryStringRouteDataValues collection.
			// Don't overwrite any existing route data values, though.
			var queryStringPairsToApply = queryString
				.Keys
				.Cast<string>()
				.Intersect(QueryStringRouteDataValues, StringComparer.OrdinalIgnoreCase)
				.Where(key => !routeDataValues.ContainsKey(key));

			foreach(var queryKey in queryStringPairsToApply)
				routeDataValues[queryKey] = queryString[queryKey];
		}

		void ApplyRouteDataValueTransformations(RouteValueDictionary routeDataValues, RouteDirection direction)
		{
			// Apply the transformations of the route values based on the key
			foreach(var transformer in RouteDataValueTransforms)
				transformer(routeDataValues, direction);
		}

		#region Reflected Proxy Classes

		/*
			==== DO NOT MODIFY THESE CLASSES ====
			These proxy classes exist only to expose internal .NET framework classes via reflection so we can use the 
			same API's that the .NET framework's Route class uses. They are not intended as extension points to 
			customize ADNSF functionality.

			All of these classes follow the same pattern: they wrap a reflected instance of the proxied type and invoke
			expose a subset of the members of that type, which are themselves invoked via reflection.
		*/

		/// <summary>
		/// Proxies the RouteParser class to support <see cref="AdnsfRoute"/>. DO NOT MODIFY.
		/// </summary>
		class RouteParserReflectionProxy
		{
			readonly MethodInfo ParseMethodInfo;

			public RouteParserReflectionProxy()
			{
				// The RouteParser class is static, so we don't need to store an instance
				var routeParserType = Type.GetType("System.Web.Routing.RouteParser, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
				ParseMethodInfo = routeParserType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
			}

			public ParsedRouteReflectedProxy Parse(string routeUrl)
			{
				var parsedRoute = ParseMethodInfo.Invoke(null, new[] { routeUrl });
				if(parsedRoute == null)
					return null;

				return new ParsedRouteReflectedProxy(parsedRoute);
			}
		}

		/// <summary>
		/// Proxies the ParsedRoute class to support <see cref="AdnsfRoute"/>. DO NOT MODIFY.
		/// </summary>
		class ParsedRouteReflectedProxy
		{
			readonly object ParsedRoute;
			readonly MethodInfo MatchMethodInfo;
			readonly MethodInfo BindMethodInfo;

			public ParsedRouteReflectedProxy(object parsedRoute)
			{
				ParsedRoute = parsedRoute;

				var parsedRouteType = parsedRoute.GetType();
				MatchMethodInfo = parsedRouteType.GetMethod("Match", BindingFlags.Public | BindingFlags.Instance);
				BindMethodInfo = parsedRouteType.GetMethod("Bind", BindingFlags.Public | BindingFlags.Instance);
			}

			public RouteValueDictionary Match(string virtualPath, RouteValueDictionary defaultValues)
			{
				return (RouteValueDictionary)MatchMethodInfo.Invoke(ParsedRoute, new object[] { virtualPath, defaultValues });
			}

			public BoundUrlReflectedProxy Bind(RouteValueDictionary currentValues, RouteValueDictionary values, RouteValueDictionary defaultValues, RouteValueDictionary constraints)
			{
				var boundUrl = BindMethodInfo.Invoke(ParsedRoute, new object[] { currentValues, values, defaultValues, constraints });
				if(boundUrl == null)
					return null;

				return new BoundUrlReflectedProxy(boundUrl);
			}
		}

		/// <summary>
		/// Proxies the BoundUrl class to support <see cref="AdnsfRoute"/>. DO NOT MODIFY.
		/// </summary>
		class BoundUrlReflectedProxy
		{
			public readonly string Url;
			public readonly RouteValueDictionary Values;

			public BoundUrlReflectedProxy(object boundUrl)
			{
				var boundUrlType = boundUrl.GetType();
				Url = (string)boundUrlType.GetProperty("Url").GetValue(boundUrl);
				Values = (RouteValueDictionary)boundUrlType.GetProperty("Values").GetValue(boundUrl);
			}
		}

		#endregion
	}
}
