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
	public class GlobalConfig : IParameterizedTokenHandler
	{
		const string NameParam = "name";

		readonly string[] Tokens = { "globalconfig" };
		readonly string[] ParameterNames = { NameParam };

		readonly TokenParameterConverter TokenParameterConverter;

		public GlobalConfig(TokenParameterConverter tokenParameterConverter)
		{
			TokenParameterConverter = tokenParameterConverter;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Parameters.ContainsKey(NameParam) || string.IsNullOrWhiteSpace(context.Parameters[NameParam]))
				throw new TokenParameterMissingException(NameParam);

			var globalConfigName = context
				.Parameters[NameParam]
				.Trim();

			var globalConfig = AspDotNetStorefrontCore.GlobalConfig.GetGlobalConfig(globalConfigName);

			return globalConfig == null
				? null
				: globalConfig.ConfigValue;
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
