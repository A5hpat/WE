// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspDotNetStorefrontCore.Tokens
{
	public class TokenExecutor
	{
		readonly IEnumerable<ITokenHandler> TokenHandlers;

		public TokenExecutor(IEnumerable<ITokenHandler> tokenHandlers)
		{
			TokenHandlers = tokenHandlers;
		}

		public string GetTokenValue(Customer customer, string token, IEnumerable<string> parameters)
		{
			if(customer == null)
				throw new ArgumentNullException("customer");

			return TokenHandlers
				.Select(handler => handler.RenderToken(new TokenHandlerContext(
					customer,
					token,
					handler is IParameterizedTokenHandler
						? ((IParameterizedTokenHandler)handler).ConvertPositionalParametersToNamedParameters(parameters)
						: new Dictionary<string, string>())))
				.Where(result => result != null)
				.FirstOrDefault();
		}

		public string GetTokenValue(Customer customer, string token, IDictionary<string, string> parameters)
		{
			if(customer == null)
				throw new ArgumentNullException("customer");

			var effectiveParamaters = parameters == null
				? new Dictionary<string, string>()
				: new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);

			var tokenHandlerContext = new TokenHandlerContext(customer, token, effectiveParamaters);

			return TokenHandlers
				.Select(handler => handler.RenderToken(tokenHandlerContext))
				.Where(result => result != null)
				.FirstOrDefault();
		}
	}
}
