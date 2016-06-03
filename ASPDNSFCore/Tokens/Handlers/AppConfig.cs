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
	public class AppConfig : IParameterizedTokenHandler
	{
		const string NameParam = "name";

		readonly string[] Tokens = { "appconfig" };
		readonly string[] ParameterNames = { NameParam };

		readonly TokenParameterConverter TokenParameterConverter;
		readonly AppConfigProvider AppConfigProvider;

		public AppConfig(TokenParameterConverter tokenParameterConverter, AppConfigProvider appConfigProvider)
		{
			TokenParameterConverter = tokenParameterConverter;
			AppConfigProvider = appConfigProvider;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Parameters.ContainsKey(NameParam) || string.IsNullOrWhiteSpace(context.Parameters[NameParam]))
				throw new TokenParameterMissingException(NameParam);

			var appConfigName = context
				.Parameters[NameParam]
				.Trim();

			return AppConfigProvider.GetAppConfigValue(appConfigName);
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
