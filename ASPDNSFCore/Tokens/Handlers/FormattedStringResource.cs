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
	public class FormattedStringResource : IParameterizedTokenHandler
	{
		const string NameParam = "name";
		const string FormatStringParam = "formatString";

		readonly string[] Tokens = { "formattedstringresource" };
		readonly string[] ParameterNames = { NameParam, FormatStringParam };

		readonly TokenParameterConverter TokenParameterConverter;

		public FormattedStringResource(TokenParameterConverter tokenParameterConverter)
		{
			TokenParameterConverter = tokenParameterConverter;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Parameters.ContainsKey(NameParam) || string.IsNullOrWhiteSpace(context.Parameters[NameParam]))
				throw new TokenParameterMissingException(NameParam);

			if(!context.Parameters.ContainsKey(FormatStringParam) || string.IsNullOrWhiteSpace(context.Parameters[FormatStringParam]))
				throw new TokenParameterMissingException(FormatStringParam);

			var stringResourceKey = context
				.Parameters[NameParam]
				.Trim();

			var formatString = context
				.Parameters[FormatStringParam]
				.Trim()
				.ToLower();

			return string.Format(
				formatString,
				AppLogic.GetString(stringResourceKey, context.Customer.LocaleSetting));
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
