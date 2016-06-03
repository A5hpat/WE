// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Text.RegularExpressions;

namespace AspDotNetStorefrontCore.Validation
{
	public class POBoxAddressVerifier
	{
		public bool IsValid(Address address)
		{
			return IsValid(address.Address1);
		}

		public bool IsValid(string address)
		{
			Regex regEx = new Regex(@"(?i)\b(?:p(?:ost)?\.?\s*[o0](?:ffice)?\.?\s*b(?:[o0]x)?|b[o0]x)");
			return !regEx.IsMatch(address);
		}
	}
}
