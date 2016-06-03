// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;

namespace AspDotNetStorefrontCore.Tokens
{
	public class TokenHandlerContext
	{
		public readonly Customer Customer;
		public readonly string Token;
		public readonly IDictionary<string, string> Parameters;

		public TokenHandlerContext(Customer customer, string token, IDictionary<string, string> parameters)
		{
			Customer = customer;
			Token = token;
			Parameters = parameters ?? new Dictionary<string, string>();
		}
	}
}
