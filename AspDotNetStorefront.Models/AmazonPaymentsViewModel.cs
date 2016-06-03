// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Models
{
	public class AmazonPaymentsViewModel
	{
		public readonly string ClientId;
		public readonly string MerchantId;
		public readonly string ScriptUrl;

		public AmazonPaymentsViewModel(string clientId = null, string merchantId = null, string scriptUrl = null)
		{
			ClientId = clientId;
			MerchantId = merchantId;
			ScriptUrl = scriptUrl;
		}

		public string AmazonOrderReferenceId
		{ get; set; }

		public AmazonPaymentsCheckoutStep CheckoutStep
		{ get; set; }

		public AmazonPaymentsViewModel()
		{
			CheckoutStep = AmazonPaymentsCheckoutStep.Login;
		}
	}

	public enum AmazonPaymentsCheckoutStep
	{
		Login,
		SelectAddress,
	}
}
