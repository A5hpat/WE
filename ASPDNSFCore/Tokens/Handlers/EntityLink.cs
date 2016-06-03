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
	public class EntityLink : IParameterizedTokenHandler
	{
		const string EntityIdParam = "entityId";
		const string SeNameParam = "seName";
		const string EntityNameParam = "entityname";

		readonly string[] Tokens = { "entitylink" };
		readonly string[] ParameterNames = { EntityIdParam, SeNameParam, EntityNameParam };

		readonly TokenParameterConverter TokenParameterConverter;
		readonly TokenEntityLinkBuilder EntityLinkBuilder;

		public EntityLink(TokenParameterConverter tokenParameterConverter, TokenEntityLinkBuilder entityLinkBuilder)
		{
			TokenParameterConverter = tokenParameterConverter;
			EntityLinkBuilder = entityLinkBuilder;
		}

		public virtual string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			var entityId = EntityLinkBuilder.GetEntityIdParameterValue(context, EntityIdParam);
			var seName = EntityLinkBuilder.GetSeNameParameterValue(context, SeNameParam);
			var entityType = EntityLinkBuilder.GetEntityTypeParameterValue(context, EntityNameParam);
			var routeValues = EntityLinkBuilder.BuildRouteValues(context, ParameterNames);

			return EntityLinkBuilder.BuildEntityLink(entityType, entityId, seName, routeValues);
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			// Note that we can't use wildcards to support additional route values as they have to have named keys.
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
