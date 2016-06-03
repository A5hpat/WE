// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Checkout.Engine
{
	public enum CheckoutState
	{
		Start = 0,

		ShoppingCart_Validating,
		ShoppingCartIsEmpty,
		SubtotalDoesNotMeetMinimumAmount,
		CartItemsLessThanMinimumItemCount,
		CartItemsGreaterThanMaximumItemCount,
		RecurringScheduleMismatchOnItems,

		Account_Validating,
		CustomerAccountRequired,

		PaymentMethod_Cleaning,

		PaymentMethod_Validating,
		PaymentMethodRequired,
		PaymentMethod_Valid,

		PaymentMethod_ValidatingMicropayDetails,
		MicroPayBalanceIsInsufficient,

		PaymentMethod_ValidatingPayPalExpressDetails,

		PaymentMethod_ValidatingAmazonPaymentsDetails,
		AmazonPaymentsDetailsRequired,

		PaymentMethod_ValidatingCreditCardDetails,
		CreditCardDetailsRequired,

		PaymentMethod_ValidatingPurchaseOrderDetails,
		PurchaseOrderDetailsRequired,

		BillingAddress_Validating,
		BillingAddressRequired,
		BillingAddress_Valid,

		ShippingAddress_Validating,
		ShippingAddressRequired,
		ShippingAddress_Valid,

		ShippingMethod_Validating,
		ShippingMethodRequired,
		ShippingMethodsAreInconsistent,
		ShippingMethodIsInvalid,
		ShippingMethod_Valid,

		GiftCardSetup_Validating,
		GiftCardRequiresSetup,

		Checkout_Validating,
		CustomerIsNotOver13,
		ShippingAddressDoesNotMatchBillingAddress,
		TermsAndConditionsRequired,

		Valid,
	}
}
