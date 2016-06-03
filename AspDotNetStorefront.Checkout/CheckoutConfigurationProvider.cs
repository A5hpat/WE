// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout
{
	public class CheckoutConfigurationProvider : ICheckoutConfigurationProvider
	{
		public CheckoutConfiguration GetCheckoutConfiguration()
		{
			return new CheckoutConfiguration(
				ageCartDays: AppLogic.AppConfigUSInt("AgeCartDays"),
				cartMinOrderAmount: AppLogic.AppConfigUSDecimal("CartMinOrderAmount"),
				minCartItemsBeforeCheckout: AppLogic.AppConfigUSInt("MinCartItemsBeforeCheckout"),
				maxCartItemsBeforeCheckout: AppLogic.AppConfigUSInt("MaxCartItemsBeforeCheckout"),
				skipPaymentEntryOnZeroDollarCheckout: AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"),
				skipShippingOnCheckout: AppLogic.AppConfigBool("SkipShippingOnCheckout"),
				requireOver13Checked: AppLogic.AppConfigBool("RequireOver13Checked"),
				requireTermsAccepted: AppLogic.AppConfigBool("RequireTermsAndConditionsAtCheckout"),
				guestCheckout: AppLogic.AppConfig("GuestCheckout"));
		}
	}
}
