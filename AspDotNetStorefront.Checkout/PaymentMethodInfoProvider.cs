// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront.Checkout
{
	public class PaymentMethodInfoProvider : IPaymentMethodInfoProvider
	{
		readonly Dictionary<string, string> PaymentMethodDisplayNames;

		public PaymentMethodInfoProvider()
		{
			PaymentMethodDisplayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{ AppLogic.ro_PMPayPalExpress,"PayPal" },
				{ AppLogic.ro_PMPayPalCredit, "PayPal Credit" },
				{ AppLogic.ro_PMPayPalEmbeddedCheckout, "Credit Card" },
				{ AppLogic.ro_PMCreditCard, "Credit Card" },
				{ AppLogic.ro_PMRequestQuote, "Request Quote" },
				{ AppLogic.ro_PMPurchaseOrder, "Purchase Order" },
				{ AppLogic.ro_PMCheckByMail, "Check By Mail" },
				{ AppLogic.ro_PMCOD, "C.O.D." },
				{ AppLogic.ro_PMMicropay, "Micropay" },
			};
		}

		public PaymentMethodInfo GetPaymentMethodInfo(string paymentMethod, string gateway)
		{
			if(string.IsNullOrEmpty(paymentMethod))
				return null;

			return new PaymentMethodInfo(
				name: paymentMethod,
				displayName: GetDisplayName(paymentMethod, gateway),
				location: GetLocation(paymentMethod, gateway),
				requiresBillingSelection: GetRequiresBillingSelection(paymentMethod, gateway));
		}

		string GetDisplayName(string paymentMethod, string gateway)
		{
			return PaymentMethodDisplayNames.ContainsKey(paymentMethod)
				? PaymentMethodDisplayNames[paymentMethod]
				: null;
		}

		PaymentMethodLocation GetLocation(string paymentMethod, string gateway)
		{
			if(paymentMethod == AppLogic.ro_PMPayPalEmbeddedCheckout)
				return PaymentMethodLocation.Offsite;

			if(paymentMethod == AppLogic.ro_PMCreditCard
				&& gateway == Gateway.ro_GWTWOCHECKOUT)
				return PaymentMethodLocation.Offsite;

			return PaymentMethodLocation.Onsite;
		}

		bool GetRequiresBillingSelection(string paymentMethod, string gateway)
		{
			if(paymentMethod == AppLogic.ro_PMAmazonPayments)
				return false;

			return true;
		}
	}
}
