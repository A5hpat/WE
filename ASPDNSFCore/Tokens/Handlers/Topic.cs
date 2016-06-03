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
	public class Topic : IParameterizedTokenHandler
	{
		const string IdParam = "id";
		const string TopicIdParam = "topicId";
		const string NameParam = "name";
		const string UseParserParam = "useParser";

		readonly string[] Tokens = { "topic" };
		readonly string[] ParameterNames = { NameParam, UseParserParam };

		readonly TokenParameterConverter TokenParameterConverter;

		public Topic(TokenParameterConverter tokenParameterConverter)
		{
			TokenParameterConverter = tokenParameterConverter;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			// Try parsing the ID parameter then the legacy TopicID parameter
			int topicId;
			if(context.Parameters.ContainsKey(IdParam) && !string.IsNullOrEmpty(context.Parameters[IdParam]))
				int.TryParse(context.Parameters[IdParam], out topicId);
			else if(context.Parameters.ContainsKey(TopicIdParam) && !string.IsNullOrEmpty(context.Parameters[TopicIdParam]))
				int.TryParse(context.Parameters[TopicIdParam], out topicId);
			else
				topicId = 0;

			if(topicId != 0)
			{
				var topicById = new AspDotNetStorefrontCore.Topic(topicId, context.Customer.LocaleSetting, context.Customer.SkinID);
				return topicById.Contents;
			}

			// Try loading a topic by name
			if(!context.Parameters.ContainsKey(NameParam) || string.IsNullOrWhiteSpace(context.Parameters[NameParam]))
				throw new TokenParameterMissingException(NameParam);

			var topicName = context
				.Parameters[NameParam]
				.Trim();

			// Decide whether to use a parser on this token
			var useParser = true;
			if(context.Parameters.ContainsKey(UseParserParam))
				useParser = StringComparer.OrdinalIgnoreCase.Equals(context.Parameters[UseParserParam], bool.TrueString);

			var parser = useParser
				? new Parser()
				: null;

			var topic = new AspDotNetStorefrontCore.Topic(
				topicName, 
				context.Customer.LocaleSetting, 
				context.Customer.SkinID, 
				parser);
			return topic.Contents;
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
