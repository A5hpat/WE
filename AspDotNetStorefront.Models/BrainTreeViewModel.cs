// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Models
{
	public class BraintreeViewModel
	{
		public readonly string Token;
		public readonly string ScriptUrl;

		public BraintreeViewModel(
			string token,
			string scriptUrl)
		{
			Token = token;
			ScriptUrl = scriptUrl;
		}
	}

	public class BraintreeThreeDSecureViewModel
	{
		public readonly string Nonce;
		public readonly string ScriptUrl;
		public readonly string Token;
		public readonly string Total;

		public BraintreeThreeDSecureViewModel(
			string nonce,
			string scriptUrl,
            string token,
			string total)
		{
			Nonce = nonce;
			ScriptUrl = scriptUrl;
			Token = token;
			Total = total;
		}
	}
}
