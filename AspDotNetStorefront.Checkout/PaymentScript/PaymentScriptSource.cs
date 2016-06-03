// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Checkout.PaymentScript
{
	public class PaymentScriptSource : IPaymentScript
	{
		public string Endpoint;
		public bool Async;

		public PaymentScriptSource(string endpoint, bool @async = false)
		{
			Endpoint = endpoint;
			Async = @async;
		}

		public string Render()
		{
			return string.Format("<script src='{0}' {1}></script>",
				Endpoint,
				Async ? "async" : "");
		}
	}
}
