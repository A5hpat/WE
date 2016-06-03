// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace AspDotNetStorefrontCore.Tokens
{
	public interface ITokenHandler
	{
		string RenderToken(TokenHandlerContext context);
	}

	public interface IParameterizedTokenHandler : ITokenHandler
	{
		IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters);
	}
}
