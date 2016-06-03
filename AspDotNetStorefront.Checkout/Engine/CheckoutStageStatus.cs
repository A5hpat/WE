// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Checkout.Engine
{
	public class CheckoutStageContext
	{
		public readonly CheckoutStageStatus Account;
		public readonly CheckoutStageStatus PaymentMethod;
		public readonly CheckoutStageStatus BillingAddress;
		public readonly CheckoutStageStatus ShippingAddress;
		public readonly CheckoutStageStatus ShippingMethod;
		public readonly CheckoutStageStatus GiftCardSetup;
		public readonly CheckoutStageStatus PlaceOrderButton;

		public CheckoutStageContext(
			CheckoutStageStatus account,
			CheckoutStageStatus paymentMethod,
			CheckoutStageStatus billingAddress,
			CheckoutStageStatus shippingAddress,
			CheckoutStageStatus shippingMethod,
			CheckoutStageStatus giftCardSetup,
			CheckoutStageStatus placeOrderButton)
		{
			Account = account;
			PaymentMethod = paymentMethod;
			BillingAddress = billingAddress;
			ShippingAddress = shippingAddress;
			ShippingMethod = shippingMethod;
			GiftCardSetup = giftCardSetup;
			PlaceOrderButton = placeOrderButton;
		}
	}

	public class CheckoutStageStatus
	{
		public readonly bool? Required;
		public readonly bool? Available;
		public readonly bool? Fulfilled;
		public readonly bool? Disabled;

		public CheckoutStageStatus(bool? required, bool? available, bool? fulfilled, bool? disabled)
		{
			Required = required;
			Available = available;
			Fulfilled = fulfilled;
			Disabled = disabled;
		}
	}

	public static class CheckoutStageContextExtensions
	{
		public static CheckoutStageContext UpdateAccount(this CheckoutStageContext context, CheckoutStageStatus account)
		{
			return new CheckoutStageContext(
				account: account,
				paymentMethod: context.PaymentMethod,
				billingAddress: context.BillingAddress,
				shippingAddress: context.ShippingAddress,
				shippingMethod: context.ShippingMethod,
				giftCardSetup: context.GiftCardSetup,
				placeOrderButton: context.PlaceOrderButton);
		}

		public static CheckoutStageContext UpdatePaymentMethod(this CheckoutStageContext context, CheckoutStageStatus paymentMethod)
		{
			return new CheckoutStageContext(
				account: context.Account,
				paymentMethod: paymentMethod,
				billingAddress: context.BillingAddress,
				shippingAddress: context.ShippingAddress,
				shippingMethod: context.ShippingMethod,
				giftCardSetup: context.GiftCardSetup,
				placeOrderButton: context.PlaceOrderButton);
		}

		public static CheckoutStageContext UpdateBillingAddress(this CheckoutStageContext context, CheckoutStageStatus billingAddress)
		{
			return new CheckoutStageContext(
				account: context.Account,
				paymentMethod: context.PaymentMethod,
				billingAddress: billingAddress,
				shippingAddress: context.ShippingAddress,
				shippingMethod: context.ShippingMethod,
				giftCardSetup: context.GiftCardSetup,
				placeOrderButton: context.PlaceOrderButton);
		}

		public static CheckoutStageContext UpdateShippingAddress(this CheckoutStageContext context, CheckoutStageStatus shippingAddress)
		{
			return new CheckoutStageContext(
				account: context.Account,
				paymentMethod: context.PaymentMethod,
				billingAddress: context.BillingAddress,
				shippingAddress: shippingAddress,
				shippingMethod: context.ShippingMethod,
				giftCardSetup: context.GiftCardSetup,
				placeOrderButton: context.PlaceOrderButton);
		}

		public static CheckoutStageContext UpdateShippingMethod(this CheckoutStageContext context, CheckoutStageStatus shippingMethod)
		{
			return new CheckoutStageContext(
				account: context.Account,
				paymentMethod: context.PaymentMethod,
				billingAddress: context.BillingAddress,
				shippingAddress: context.ShippingAddress,
				shippingMethod: shippingMethod,
				giftCardSetup: context.GiftCardSetup,
				placeOrderButton: context.PlaceOrderButton);
		}

		public static CheckoutStageContext UpdateGiftCardSetup(this CheckoutStageContext context, CheckoutStageStatus giftCardSetup)
		{
			return new CheckoutStageContext(
				account: context.Account,
				paymentMethod: context.PaymentMethod,
				billingAddress: context.BillingAddress,
				shippingAddress: context.ShippingAddress,
				shippingMethod: context.ShippingMethod,
				giftCardSetup: giftCardSetup,
				placeOrderButton: context.PlaceOrderButton);
		}
	}

	public static class CheckoutStageStatusExtensions
	{
		public static CheckoutStageStatus UpdateRequired(this CheckoutStageStatus status, bool? required)
		{
			return new CheckoutStageStatus(
				required: required,
				available: status.Available,
				fulfilled: status.Fulfilled,
				disabled: status.Disabled);
		}

		public static CheckoutStageStatus UpdateAvailable(this CheckoutStageStatus status, bool? available)
		{
			return new CheckoutStageStatus(
				required: status.Required,
				available: available,
				fulfilled: status.Fulfilled,
				disabled: status.Disabled);
		}

		public static CheckoutStageStatus UpdateFulfilled(this CheckoutStageStatus status, bool? fulfilled)
		{
			return new CheckoutStageStatus(
				required: status.Required,
				available: status.Available,
				fulfilled: fulfilled,
				disabled: status.Disabled);
		}

		public static CheckoutStageStatus UpdateDisabled(this CheckoutStageStatus status, bool? disabled)
		{
			return new CheckoutStageStatus(
				required: status.Required,
				available: status.Available,
				fulfilled: status.Fulfilled,
				disabled: disabled);
		}
	}
}
