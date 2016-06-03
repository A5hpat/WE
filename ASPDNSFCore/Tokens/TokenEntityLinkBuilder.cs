// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AspDotNetStorefrontCore.Tokens
{
	public class TokenEntityLinkBuilder
	{
		readonly UrlHelper UrlHelper;

		public TokenEntityLinkBuilder(UrlHelper urlHelper)
		{
			UrlHelper = urlHelper;
		}

		public int GetEntityIdParameterValue(TokenHandlerContext context, string entityIdParam)
		{
			return context.Parameters.ContainsKey(entityIdParam)
				? Localization.ParseUSInt(context.Parameters[entityIdParam])
				: 0;
		}

		public string GetSeNameParameterValue(TokenHandlerContext context, string seNameParam)
		{
			return context.Parameters.ContainsKey(seNameParam)
				? context.Parameters[seNameParam] ?? string.Empty
				: string.Empty;

		}

		public string GetEntityTypeParameterValue(TokenHandlerContext context, string entityNameParam)
		{
			return context.Parameters.ContainsKey(entityNameParam)
				? context.Parameters[entityNameParam] ?? string.Empty
				: string.Empty;
		}

		public Dictionary<string, object> BuildRouteValues(TokenHandlerContext context, IEnumerable<string> excludedParameterNames)
		{
			return context
				.Parameters
				.Keys
				.Except(excludedParameterNames, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(
					key => key,
					key => (object)context.Parameters[key]);
		}

		public string BuildEntityLink(string entityType, int entityId, string seName, Dictionary<string, object> routeValues)
		{
			return UrlHelper.BuildEntityLink(entityType, entityId, seName, routeValues);
		}
	}
}
