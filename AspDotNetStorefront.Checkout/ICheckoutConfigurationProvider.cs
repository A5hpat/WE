// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Checkout
{
	public interface ICheckoutConfigurationProvider
	{
		CheckoutConfiguration GetCheckoutConfiguration();
	}

	public class CheckoutConfiguration
	{
		public readonly int AgeCartDays;
		public readonly decimal CartMinOrderAmount;
		public readonly int MinCartItemsBeforeCheckout;
		public readonly int MaxCartItemsBeforeCheckout;
		public readonly bool SkipPaymentEntryOnZeroDollarCheckout;
		public readonly bool SkipShippingOnCheckout;
		public readonly bool RequireOver13Checked;
		public readonly bool RequireTermsAccepted;
		public readonly string GuestCheckout;

		public CheckoutConfiguration(int ageCartDays,
			decimal cartMinOrderAmount,
			int minCartItemsBeforeCheckout,
			int maxCartItemsBeforeCheckout,
			bool skipPaymentEntryOnZeroDollarCheckout,
			bool skipShippingOnCheckout,
			bool requireOver13Checked,
			bool requireTermsAccepted,
			string guestCheckout)
		{
			AgeCartDays = ageCartDays;
			CartMinOrderAmount = cartMinOrderAmount;
			MinCartItemsBeforeCheckout = minCartItemsBeforeCheckout;
			MaxCartItemsBeforeCheckout = maxCartItemsBeforeCheckout;
			SkipPaymentEntryOnZeroDollarCheckout = skipPaymentEntryOnZeroDollarCheckout;
			SkipShippingOnCheckout = skipShippingOnCheckout;
			RequireOver13Checked = requireOver13Checked;
			RequireTermsAccepted = requireTermsAccepted;
			GuestCheckout = guestCheckout;
		}
	}
}
