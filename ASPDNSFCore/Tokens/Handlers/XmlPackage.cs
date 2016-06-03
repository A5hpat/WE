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
	public class XmlPackage : IParameterizedTokenHandler
	{
		const string NameParam = "name";
		const string RuntimeParams = "runtimeParams";

		readonly string[] Tokens = { "xmlpackage" };
		readonly string[] ParameterNames = { NameParam, RuntimeParams };

		readonly TokenParameterConverter TokenParameterConverter;

		public XmlPackage(TokenParameterConverter tokenParameterConverter)
		{
			TokenParameterConverter = tokenParameterConverter;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Parameters.ContainsKey(NameParam) || string.IsNullOrWhiteSpace(context.Parameters[NameParam]))
				throw new TokenParameterMissingException(NameParam);

			var xmlPackageName = context
				.Parameters[NameParam]
				.Trim();

			// All runtime param keys need to be merged with their values into rumtime param strings
			var runtimeParameters = string.Join(
				"&",
				context
					.Parameters
					.Keys
					.Except(ParameterNames, StringComparer.OrdinalIgnoreCase)
					.Select(key => string.Format(
						"{0}={1}",
						key,
						context.Parameters[key])));

			if(context.Parameters.ContainsKey(RuntimeParams) && !string.IsNullOrWhiteSpace(context.Parameters[RuntimeParams]))
				runtimeParameters = string.Join(
					"&",
					runtimeParameters,
					context.Parameters[RuntimeParams].Trim());

			return AppLogic.RunXmlPackage(
				xmlPackage: new AspDotNetStorefrontCore.XmlPackage(
					packageName: xmlPackageName,
					customer: context.Customer,
					additionalRuntimeParms: runtimeParameters),
				UseParser: new Parser(),
				ThisCustomer: context.Customer,
				SkinID: context.Customer.SkinID,
				ReplaceTokens: true,
				WriteExceptionMessage: true);
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
