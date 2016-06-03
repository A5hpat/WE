// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc.Html;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefrontCore.Tokens.Handlers
{
	public class AddToCartForm : IParameterizedTokenHandler
	{
		const string ProductIdParam = "productId";
		const string VariantIdParam = "variantId";

		readonly string[] Tokens = { "addtocartform" };
		readonly string[] ParameterNames = { ProductIdParam, VariantIdParam };

		readonly TokenParameterConverter TokenParameterConverter;
		readonly TokenHtmlHelper TokenHtmlHelper;

		public AddToCartForm(TokenParameterConverter tokenParameterConverter, TokenHtmlHelper tokenHtmlHelper)
		{
			TokenParameterConverter = tokenParameterConverter;
			TokenHtmlHelper = tokenHtmlHelper;
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

			var showWishlistButtons = AppLogic.AppConfigBool("ShowWishButtons");

			return TokenHtmlHelper
				.Action(
					actionName: ActionNames.AddToCartForm,
					controllerName: ControllerNames.ShoppingCart,
					routeValues: new
					{
						productId = productId,
						variantId = variantId,
						showWishlistButton = showWishlistButtons
					})
				.ToString();
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
