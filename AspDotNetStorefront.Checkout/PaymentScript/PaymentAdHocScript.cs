// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Checkout.PaymentScript
{
	public class PaymentAdHocScript : IPaymentScript
	{
		public string Content;

		public PaymentAdHocScript(string content)
		{
			Content = content;
		}

		public string Render()
		{
			return string.Format(@"<script type='text/javascript'>{0}</script>", Content);
		}
	}
}
