// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Routing;

namespace AspDotNetStorefront.Routing
{
	/// <param name="key">The key in the route data value collection that triggered this transform</param>
	/// <param name="value">The route data value for <paramref name="key"/></param>
	/// <param name="values">The route data value collection</param>
	public delegate void RouteDataValueTransform(RouteValueDictionary values, RouteDirection direction);

	public static class RouteDataValueTransformations
	{
		/// <summary>
		/// Copies the route data value from one key to another. Does not overwrite the value if one already exists with that key.
		/// </summary>
		/// <param name="target">The key to copy the triggering key's value to.</param>
		public static RouteDataValueTransform Copy(string fromKey, string toKey)
		{
			return (values, direction) =>
			{
				if(!values.ContainsKey(fromKey))
					return;

				if(values.ContainsKey(toKey))
					return;

				values[toKey] = values[fromKey];
			};
		}

		/// <summary>
		/// Removes the indicated key from the route value collection.
		/// </summary>
		public static RouteDataValueTransform Remove(string key)
		{
			return (values, direction) => values.Remove(key);
		}

		/// <summary>
		/// Wraps a <see cref="RouteDataValueTransform"/> so that it only triggers when generating URL's.
		/// </summary>
		public static RouteDataValueTransform ForUrlGeneration(this RouteDataValueTransform transform)
		{
			return (values, direction) =>
			{
				if(direction == RouteDirection.UrlGeneration)
					transform(values, direction);
			};
		}

		/// <summary>
		/// Wraps a <see cref="RouteDataValueTransform"/> so that it only triggers when matching incoming requests.
		/// </summary>
		public static RouteDataValueTransform ForIncomingRequests(this RouteDataValueTransform transform)
		{
			return (values, direction) =>
			{
				if(direction == RouteDirection.IncomingRequest)
					transform(values, direction);
			};
		}
	}
}
