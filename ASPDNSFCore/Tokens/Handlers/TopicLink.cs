// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AspDotNetStorefrontCore.Tokens.Handlers
{
	public class TopicLink : IParameterizedTokenHandler
	{
		const string NameParam = "name";

		readonly string[] Tokens = { "topiclink" };
		readonly string[] ParameterNames = { NameParam };

		readonly TokenParameterConverter TokenParameterConverter;
		readonly UrlHelper UrlHelper;

		public TopicLink(TokenParameterConverter tokenParameterConverter, UrlHelper urlHelper)
		{
			TokenParameterConverter = tokenParameterConverter;
			UrlHelper = urlHelper;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Parameters.ContainsKey(NameParam) || string.IsNullOrWhiteSpace(context.Parameters[NameParam]))
				throw new TokenParameterMissingException(NameParam);

			return UrlHelper.BuildTopicLink(context.Parameters[NameParam]);
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
