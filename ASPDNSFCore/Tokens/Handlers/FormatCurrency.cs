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
	public class FormatCurrency : IParameterizedTokenHandler
	{
		const string ValueParam = "value";

		readonly string[] Tokens = { "formatCurrency" };
		readonly string[] ParameterNames = { ValueParam };

		readonly TokenParameterConverter TokenParameterConverter;

		public FormatCurrency(TokenParameterConverter tokenParameterConverter)
		{
			TokenParameterConverter = tokenParameterConverter;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Parameters.ContainsKey(ValueParam) || string.IsNullOrWhiteSpace(context.Parameters[ValueParam]))
				throw new TokenParameterMissingException(ValueParam);

			return Localization.CurrencyStringForDisplayWithExchangeRate(
				Localization.ParseUSDecimal(context.Parameters[ValueParam]),
				context.Customer.CurrencySetting);
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
