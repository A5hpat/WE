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
using System.Web.Routing;

namespace AspDotNetStorefrontCore.Tokens.Handlers
{
	public class Url : IParameterizedTokenHandler
	{
		const string ActionNameParam = "actionname";
		const string ControllerNameParam = "controllername";

		readonly string[] Tokens = { "url" };
		readonly string[] ParameterNames = { ActionNameParam, ControllerNameParam };

		readonly TokenParameterConverter TokenParameterConverter;
		readonly UrlHelper UrlHelper;

		public Url(TokenParameterConverter tokenParameterConverter, UrlHelper urlHelper)
		{
			TokenParameterConverter = tokenParameterConverter;
			UrlHelper = urlHelper;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Parameters.ContainsKey(ActionNameParam) || string.IsNullOrWhiteSpace(context.Parameters[ActionNameParam]))
				throw new TokenParameterMissingException(ActionNameParam);

			if(!context.Parameters.ContainsKey(ControllerNameParam) || string.IsNullOrWhiteSpace(context.Parameters[ControllerNameParam]))
				throw new TokenParameterMissingException(ControllerNameParam);

			var routeValues = context
				.Parameters
				.Keys
				.Except(ParameterNames, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(
					key => key,
					key => (object)context.Parameters[key]);

			return UrlHelper.Action(
				actionName: context.Parameters[ActionNameParam], 
				controllerName: context.Parameters[ControllerNameParam], 
				routeValues: new RouteValueDictionary(routeValues));
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			// Note that we can't use wildcards to support additional route values as they have to have named keys.
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
