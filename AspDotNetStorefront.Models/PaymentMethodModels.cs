// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AspDotNetStorefront.Checkout;

namespace AspDotNetStorefront.Models
{
	public class PaymentMethodRenderModel
	{
		public readonly string SelectedPaymentMethod;
		public readonly string SelectedPaymentMethodDisplayName;
		public readonly bool PaymentMethodComplete;
		public readonly bool AddBraintreeScripts;
		public readonly string EditUrl;
		public readonly string BraintreeScriptUrl;
		public readonly string BraintreeToken;
        public readonly IEnumerable<PaymentOption> OnSitePaymentOptions;
		public readonly IEnumerable<PaymentOption> AlternatePaymentOptions;

		public PaymentMethodRenderModel(
			string selectedPaymentMethod,
			string selectedPaymentMethodDisplayName,
			bool paymentMethodComplete,
			string editUrl,
			IEnumerable<PaymentOption> onSitePaymentOptions = null,
			IEnumerable<PaymentOption> alternatePaymentOptions = null)
		{
			SelectedPaymentMethod = selectedPaymentMethod;
			SelectedPaymentMethodDisplayName = selectedPaymentMethodDisplayName;
			PaymentMethodComplete = paymentMethodComplete;
			EditUrl = editUrl;
			OnSitePaymentOptions = onSitePaymentOptions ?? Enumerable.Empty<PaymentOption>();
			AlternatePaymentOptions = alternatePaymentOptions ?? Enumerable.Empty<PaymentOption>();
		}
	}

	public class PaymentMethodPostModel
	{
		[Required(ErrorMessage = "checkoutpayment.aspx.20")]
		public string SelectedPaymentMethod
		{ get; set; }
	}
}
