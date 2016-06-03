// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspDotNetStorefrontCore.Tokens.Handlers
{
	public class EscapedStringResource : IParameterizedTokenHandler
	{
		const string NameParam = "name";
		const string EscapingParam = "escaping";

		readonly string[] Tokens = { "escapedstringresource" };
		readonly string[] ParameterNames = { NameParam, EscapingParam };

		readonly TokenParameterConverter TokenParameterConverter;

		public EscapedStringResource(TokenParameterConverter tokenParameterConverter)
		{
			TokenParameterConverter = tokenParameterConverter;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Parameters.ContainsKey(NameParam) || string.IsNullOrWhiteSpace(context.Parameters[NameParam]))
				throw new TokenParameterMissingException(NameParam);

			if(!context.Parameters.ContainsKey(EscapingParam) || string.IsNullOrWhiteSpace(context.Parameters[EscapingParam]))
				throw new TokenParameterMissingException(EscapingParam);

			var stringResourceName = context
				.Parameters[NameParam]
				.Trim();

			var stringResource = AppLogic.GetString(stringResourceName, context.Customer.LocaleSetting);

			var escaping = context
				.Parameters[EscapingParam]
				.Trim()
				.ToLower();

			switch(escaping)
			{
				case "javascript":
					return HttpUtility.JavaScriptStringEncode(stringResource);

				case "html":
					return HttpUtility.HtmlEncode(stringResource);

				case "html-attribute":
					return HttpUtility.HtmlAttributeEncode(stringResource);

				case "url":
					return HttpUtility.UrlEncode(stringResource);

				default:
					return stringResource;
			}
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
