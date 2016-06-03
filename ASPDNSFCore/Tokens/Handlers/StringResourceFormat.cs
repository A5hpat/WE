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
	public class StringResourceFormat : IParameterizedTokenHandler
	{
		const string NameParam = "name";

		readonly string[] Tokens = { "stringresourceformat" };
		readonly string[] ParameterNames = { NameParam };

		readonly TokenParameterConverter TokenParameterConverter;

		public StringResourceFormat(TokenParameterConverter tokenParameterConverter)
		{
			TokenParameterConverter = tokenParameterConverter;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Parameters.ContainsKey(NameParam) || string.IsNullOrWhiteSpace(context.Parameters[NameParam]))
				throw new TokenParameterMissingException(NameParam);

			var primaryStringResourceName = context
				.Parameters[NameParam]
				.Trim();

			var formatArgumentStringResources = context
				.Parameters
				.Keys
				.Except(ParameterNames, StringComparer.OrdinalIgnoreCase)
				.Select(key => AppLogic.GetString(context.Parameters[key], context.Customer.LocaleSetting));

			return string.Format(
				AppLogic.GetString(primaryStringResourceName, context.Customer.LocaleSetting),
				formatArgumentStringResources.ToArray());
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(
				positionalParameters,
				ParameterNames,
				mapExtraParametersToWildcards: true);
		}
	}
}
