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
	public class ObjectLink : IParameterizedTokenHandler
	{
		const string ObjectIdParam = "objectId";
		const string SeNameParam = "seName";
		const string ObjectNameParam = "objectName";

		readonly string[] Tokens = { "objectlink" };
		readonly string[] ParameterNames = { ObjectIdParam, SeNameParam, ObjectNameParam };

		readonly TokenParameterConverter TokenParameterConverter;
		readonly UrlHelper UrlHelper;

		public ObjectLink(TokenParameterConverter tokenParameterConverter, UrlHelper urlHelper)
		{
			TokenParameterConverter = tokenParameterConverter;
			UrlHelper = urlHelper;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			var objectId = context.Parameters.ContainsKey(ObjectIdParam)
				? Localization.ParseUSInt(context.Parameters[ObjectIdParam])
				: 0;

			var seName = context.Parameters.ContainsKey(SeNameParam)
				? context.Parameters[SeNameParam] ?? string.Empty
				: string.Empty;

			var objectType = context.Parameters.ContainsKey(ObjectNameParam)
				? context.Parameters[ObjectNameParam] ?? string.Empty
				: string.Empty;

			var routeValues = context
				.Parameters
				.Keys
				.Except(ParameterNames, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(
					key => key,
					key => (object)context.Parameters[key]);

			if(AppLogic.ro_SupportedEntities.Contains(objectType, StringComparer.OrdinalIgnoreCase))
				return UrlHelper.BuildEntityLink(objectType, objectId, seName, routeValues);
			else if(StringComparer.OrdinalIgnoreCase.Equals(objectType, "product"))
				return UrlHelper.BuildProductLink(objectId, seName, routeValues);
			else
				return string.Empty;
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			// Note that we can't use wildcards to support additional route values as they have to have named keys.
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
