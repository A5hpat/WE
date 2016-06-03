// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspDotNetStorefrontCore.Tokens.Handlers
{
	public class CategoryLink : IParameterizedTokenHandler
	{
		const string CategoryIdParam = "categoryId";
		const string SeNameParam = "seName";

		readonly string[] Tokens = { "categorylink" };
		readonly string[] ParameterNames = { CategoryIdParam, SeNameParam };

		readonly TokenParameterConverter TokenParameterConverter;
		readonly TokenEntityLinkBuilder EntityLinkBuilder;

		public CategoryLink(TokenParameterConverter tokenParameterConverter, TokenEntityLinkBuilder entityLinkBuilder)
		{
			TokenParameterConverter = tokenParameterConverter;
			EntityLinkBuilder = entityLinkBuilder;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			var entityId = EntityLinkBuilder.GetEntityIdParameterValue(context, CategoryIdParam);
			var seName = EntityLinkBuilder.GetSeNameParameterValue(context, SeNameParam);
			var routeValues = EntityLinkBuilder.BuildRouteValues(context, ParameterNames);

			return EntityLinkBuilder.BuildEntityLink(EntityTypes.Category, entityId, seName, routeValues);
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			// Note that we can't use wildcards to support additional route values as they have to have named keys.
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
