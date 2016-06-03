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

namespace AspDotNetStorefrontCore.Tokens.Handlers
{
	public class ProductLink : IParameterizedTokenHandler
	{
		const string ProductIdParam = "productId";
		const string SeNameParam = "seName";

		readonly string[] Tokens =
		{
			"productlink",
			"productandcategorylink",
			"productandentitylink",
			"productandmanufacturerlink",
			"productandsectionlink",
		};

		readonly string[] ParameterNames = { ProductIdParam, SeNameParam };

		readonly TokenParameterConverter TokenParameterConverter;
		readonly UrlHelper UrlHelper;

		public ProductLink(TokenParameterConverter tokenParameterConverter, UrlHelper urlHelper)
		{
			TokenParameterConverter = tokenParameterConverter;
			UrlHelper = urlHelper;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			var productId = context.Parameters.ContainsKey(ProductIdParam)
				? Localization.ParseUSInt(context.Parameters[ProductIdParam])
				: 0;

			var seName = context.Parameters.ContainsKey(SeNameParam)
				? context.Parameters[SeNameParam] ?? string.Empty
				: string.Empty;

			var routeValues = context
				.Parameters
				.Keys
				.Except(ParameterNames, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(
					key => key,
					key => (object)context.Parameters[key]);

			return UrlHelper.BuildProductLink(productId, seName, routeValues);
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			// Note that we can't use wildcards to support additional route values as they have to have named keys.
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
