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
	public class ManufacturerLink : IParameterizedTokenHandler
	{
		const string ManufacturerIdParam = "manufacturerid";
		const string SeNameParam = "seName";

		readonly string[] Tokens = { "manufacturerlink" };
		readonly string[] ParameterNames = { ManufacturerIdParam, SeNameParam };

		readonly TokenParameterConverter TokenParameterConverter;
		readonly TokenEntityLinkBuilder EntityLinkBuilder;

		public ManufacturerLink(TokenParameterConverter tokenParameterConverter, TokenEntityLinkBuilder entityLinkBuilder)
		{
			TokenParameterConverter = tokenParameterConverter;
			EntityLinkBuilder = entityLinkBuilder;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			var entityId = EntityLinkBuilder.GetEntityIdParameterValue(context, ManufacturerIdParam);
			var seName = EntityLinkBuilder.GetSeNameParameterValue(context, SeNameParam);
			var routeValues = EntityLinkBuilder.BuildRouteValues(context, ParameterNames);

			return EntityLinkBuilder.BuildEntityLink(EntityTypes.Manufacturer, entityId, seName, routeValues);
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			// Note that we can't use wildcards to support additional route values as they have to have named keys.
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
