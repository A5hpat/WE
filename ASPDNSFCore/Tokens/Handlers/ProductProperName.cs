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
	public class ProductProperName : IParameterizedTokenHandler
	{
		const string ProductIdParam = "productId";
		const string VariantIdParam = "variantId";

		readonly string[] Tokens = { "productpropername" };
		readonly string[] ParameterNames = { ProductIdParam, VariantIdParam };

		readonly TokenParameterConverter TokenParameterConverter;

		public ProductProperName(TokenParameterConverter tokenParameterConverter)
		{
			TokenParameterConverter = tokenParameterConverter;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			var productId = context.Parameters.ContainsKey(ProductIdParam)
				? Localization.ParseUSInt(context.Parameters[ProductIdParam])
				: 0;

			var variantId = context.Parameters.ContainsKey(VariantIdParam)
				? Localization.ParseUSInt(context.Parameters[VariantIdParam])
				: AppLogic.GetDefaultProductVariant(productId);

			return AppLogic.MakeProperProductName(productId, variantId, context.Customer.LocaleSetting);
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
