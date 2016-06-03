// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefront.Validation.DataAttribute;

namespace AspDotNetStorefront.Models
{
	public enum CheckoutStageDisplayState
	{
		Unknown,
		Passing,
		Failing,
		Disabled
	}

	public enum CheckoutAction
	{
		Error,
		Complete,
		None,
		Empty,
	}

	public class CheckoutIndexPostModel
	{
		[Display(Name = "checkoutcard.aspx.14")]
		[RequiredTrueIfAppConfigTrue("RequireTermsAndConditionsAtCheckout", ErrorMessage = "checkoutpayment.aspx.15")]
		public bool TermsAndConditionsAccepted
		{ get; set; }

		[Display(Name = "createaccount.aspx.78")]
		[RequireOver13Checked(ErrorMessage = "checkout.over13required")]
		public bool Over13Selected
		{ get; set; }

		[Display(Name = "account.oktoemail.label")]
		public bool OkToEmailSelected
		{ get; set; }
	}

	public class CheckoutIndexViewModel : CheckoutIndexPostModel
	{
		public readonly PaymentMethodInfo SelectedPaymentMethod;
		public readonly AddressViewModel SelectedBillingAddress;
		public readonly AddressViewModel SelectedShippingAddress;
		public readonly bool CheckoutButtonDisabled;
		public readonly bool ShowTermsAndConditions;
		public readonly bool ShowOver13Required;
		public readonly bool ShowOkToEmail;
		public readonly bool DisplayGiftCardSetup; //This is for adding email addresses & messages when buying gift cards
		public readonly bool ShowOrderOptions;
		public readonly bool ShowOrderNotes;
		public readonly bool ShowRealTimeShippingInfo;
		public readonly bool AllowShipToDifferentThanBillTo;
		public readonly bool DisplayShippingSection;
		public readonly bool DisplayBillingSection;
		public readonly bool ShippingInfoIsRequired;
		public readonly bool DisplayTwoCheckoutText;
		public readonly bool DisplayContinueOffsite;
		public readonly bool ShowPromotions;
		public readonly bool ShowGiftCards; //This is for adding a gift card as payment
		public readonly bool GiftCardCoversTotal;
		public readonly bool CheckoutIsOffsiteOnly;
		public readonly string PageTitle;
		public readonly string PayPalBanner;
		public readonly string ContinueShoppingUrl;
		public readonly string OffsiteCheckoutError;
		public readonly CheckoutStageDisplayState AccountStageState;
		public readonly CheckoutStageDisplayState PaymentMethodStageState;
		public readonly CheckoutStageDisplayState BillingAddressStageState;
		public readonly CheckoutStageDisplayState ShippingAddressStageState;
		public readonly CheckoutStageDisplayState ShippingMethodStageState;
		public readonly CheckoutStageDisplayState GiftCardSetupStageState;

		public CheckoutIndexViewModel(
			PaymentMethodInfo selectedPaymentMethod,
			AddressViewModel selectedBillingAddress,
			AddressViewModel selectedShippingAddress,
			bool checkoutButtonDisabled,
			bool showOver13Required,
			bool showOkToEmail,
            bool showTermsAndConditions,
			bool displayGiftCardSetup,
			bool showOrderOptions,
			bool showOrderNotes,
			bool showRealTimeShippingInfo,
			bool allowShipToDifferentThanBillTo,
			bool displayShippingSection,
			bool displayBillingSection,
			bool shippingInfoIsRequired,
			bool displayTwoCheckoutText,
			bool displayContinueOffsite,
			bool showPromotions,
			bool showGiftCards,
			bool giftCardCoversTotal,
			bool checkoutIsOffsiteOnly,
			string offsiteCheckoutError,
			string pageTitle,
			string payPalBanner,
			string continueShoppingUrl,
			CheckoutStageDisplayState accountStageState,
			CheckoutStageDisplayState paymentMethodStageState,
			CheckoutStageDisplayState billingAddressStageState,
			CheckoutStageDisplayState shippingAddressStageState,
			CheckoutStageDisplayState shippingMethodStageState,
			CheckoutStageDisplayState giftCardSetupStageState)
		{
			SelectedPaymentMethod = selectedPaymentMethod;
			SelectedBillingAddress = selectedBillingAddress;
			SelectedShippingAddress = selectedShippingAddress;
			CheckoutButtonDisabled = checkoutButtonDisabled;
			ShowOver13Required = showOver13Required;
			ShowOkToEmail = showOkToEmail;
			ShowTermsAndConditions = showTermsAndConditions;
			DisplayGiftCardSetup = displayGiftCardSetup;
			ShowOrderOptions = showOrderOptions;
			ShowOrderNotes = showOrderNotes;
			ShowRealTimeShippingInfo = showRealTimeShippingInfo;
			AllowShipToDifferentThanBillTo = allowShipToDifferentThanBillTo;
			DisplayBillingSection = displayBillingSection;
			DisplayShippingSection = displayShippingSection;
			ShippingInfoIsRequired = shippingInfoIsRequired;
			DisplayTwoCheckoutText = displayTwoCheckoutText;
			DisplayContinueOffsite = displayContinueOffsite;
			ShowPromotions = showPromotions;
			ShowGiftCards = showGiftCards;
			GiftCardCoversTotal = giftCardCoversTotal;
			CheckoutIsOffsiteOnly = checkoutIsOffsiteOnly;
			PageTitle = pageTitle;
			PayPalBanner = payPalBanner;
			ContinueShoppingUrl = continueShoppingUrl;
			OffsiteCheckoutError = offsiteCheckoutError;
			AccountStageState = accountStageState;
			PaymentMethodStageState = paymentMethodStageState;
			BillingAddressStageState = billingAddressStageState;
			ShippingAddressStageState = shippingAddressStageState;
			ShippingMethodStageState = shippingMethodStageState;
			GiftCardSetupStageState = giftCardSetupStageState;
		}
	}
}
