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
	public class StringFormat : IParameterizedTokenHandler
	{
		const string FormatStringParam = "formatString";

		readonly string[] Tokens = { "stringformat" };
		readonly string[] ParameterNames = { FormatStringParam };

		readonly TokenParameterConverter TokenParameterConverter;

		public StringFormat(TokenParameterConverter tokenParameterConverter)
		{
			TokenParameterConverter = tokenParameterConverter;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Parameters.ContainsKey(FormatStringParam) || string.IsNullOrWhiteSpace(context.Parameters[FormatStringParam]))
				throw new TokenParameterMissingException(FormatStringParam);

			return string.Format(
				format: context.Parameters[FormatStringParam],
				args: context
					.Parameters
					.Keys
					.Except(ParameterNames, StringComparer.OrdinalIgnoreCase)
					.Select(key => context.Parameters[key])
					.ToArray());
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
