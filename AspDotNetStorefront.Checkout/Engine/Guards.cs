// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Linq;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront.Checkout.Engine
{
	public delegate bool Guard(CheckoutEngine.StateMachineContext context);

	public class Guards
	{
		readonly ICachedShippingMethodCollectionProvider CachedShippingMethodCollectionProvider;
		readonly GiftCardManager GiftCardManager;
		readonly AppConfigProvider AppConfigProvider;
		readonly ICheckoutAccountStatusProvider CheckoutAccountStatusProvider;

		public Guards(
			ICachedShippingMethodCollectionProvider cachedShippingMethodCollectionProvider,
			GiftCardManager giftCardManager,
			AppConfigProvider appConfigProvider,
			ICheckoutAccountStatusProvider checkoutAccountStatusProvider)
		{
			CachedShippingMethodCollectionProvider = cachedShippingMethodCollectionProvider;
			GiftCardManager = giftCardManager;
			AppConfigProvider = appConfigProvider;
			CheckoutAccountStatusProvider = checkoutAccountStatusProvider;
		}

		public Guard Not(Guard guard)
		{
			return context => !guard(context);
		}

		public bool Always(CheckoutEngine.StateMachineContext context)
		{
			return true;
		}

		public bool ShoppingCartIsEmpty(CheckoutEngine.StateMachineContext context)
		{
			return context.ShoppingCart.IsEmpty();
		}

		public bool SubtotalDoesNotMeetMinimumAmount(CheckoutEngine.StateMachineContext context)
		{
			return !context.ShoppingCart.MeetsMinimumOrderAmount(context.Configuration.CartMinOrderAmount);
		}

		public bool CartItemsLessThanMinimumItemCount(CheckoutEngine.StateMachineContext context)
		{
			return !context.ShoppingCart.MeetsMinimumOrderQuantity(context.Configuration.MinCartItemsBeforeCheckout);
		}

		public bool CartItemsGreaterThanMaximumItemCount(CheckoutEngine.StateMachineContext context)
		{
			return context.ShoppingCart.ExceedsMaximumOrderQuantity(context.Configuration.MaxCartItemsBeforeCheckout);
		}

		public bool RecurringScheduleMismatchOnItems(CheckoutEngine.StateMachineContext context)
		{
			return context.ShoppingCart.RecurringScheduleConflict;
		}

		public bool PaymentMethodRequired(CheckoutEngine.StateMachineContext context)
		{
			return !(context.ShoppingCart.Total(true) == 0
				&& context.Configuration.SkipPaymentEntryOnZeroDollarCheckout)
				&& !context.ShoppingCart.GiftCardCoversTotal();
		}

		public bool MicroPayBalanceIsInsufficient(CheckoutEngine.StateMachineContext context)
		{
			return context.ShoppingCart.Total(true) > context.Customer.MicroPayBalance;
		}

		public bool PaymentMethodPresent(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedPaymentMethod != null
				&& !string.IsNullOrEmpty(context.Selections.SelectedPaymentMethod.Name);
		}

		public bool BillingAddressRequired(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedPaymentMethod == null
				|| context.Selections.SelectedPaymentMethod.RequiresBillingSelection;
		}

		public bool BillingAddressPresent(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedBillingAddress != null
				&& context.Selections.SelectedBillingAddress.AddressID != 0;
		}

		public bool ShippingMethodRequired(CheckoutEngine.StateMachineContext context)
		{
			if(context.ShoppingCart.IsAllFreeShippingComponents()
				&& !AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
				return false;

			return context.ShoppingCart.CartAllowsShippingMethodSelection
				&& !context.ShoppingCart.IsAllSystemComponents()
				&& !context.ShoppingCart.IsAllDownloadComponents()
				&& !context.ShoppingCart.NoShippingRequiredComponents()
				&& !context.ShoppingCart.IsAllEmailGiftCards();
		}

		public bool ShippingMethodIsValid(CheckoutEngine.StateMachineContext context)
		{
			if(context.Selections.SelectedShippingMethodId == null)
				return false;

			var shippingMethods = CachedShippingMethodCollectionProvider.Get(context.Customer, context.Selections.SelectedShippingAddress, context.ShoppingCart.CartItems, context.StoreId);
			return shippingMethods
				.Select(shippingMethod => shippingMethod.Id)
				.Contains(context.Selections.SelectedShippingMethodId.Value);
		}

		public bool ShippingMethodPresent(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedShippingMethodId != null;
		}

		public bool ShippingDisabled(CheckoutEngine.StateMachineContext context)
		{
			return context.Configuration.SkipShippingOnCheckout
				|| !AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo");
		}

		public bool ShippingAddressRequired(CheckoutEngine.StateMachineContext context)
		{
			return !context.ShoppingCart.IsAllDownloadComponents()
				&& !context.ShoppingCart.IsAllSystemComponents()
				&& !context.ShoppingCart.NoShippingRequiredComponents()
				&& !context.ShoppingCart.IsAllEmailGiftCards();
		}

		public bool ShippingAddressPresent(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedShippingAddress != null
				&& context.Selections.SelectedShippingAddress.AddressID != 0;
		}

		public bool RequireCustomerOver13(CheckoutEngine.StateMachineContext context)
		{
			return context.Configuration.RequireOver13Checked;
		}

		public bool CustomerIsNotOver13(CheckoutEngine.StateMachineContext context)
		{
			return !context.Selections.Over13Checked && !context.Customer.IsOver13;
		}

		public bool CartContainsGiftCard(CheckoutEngine.StateMachineContext context)
		{
			return context
				.ShoppingCart
				.CartItems
				.Where(cartItem => GiftCard.ProductIsEmailGiftCard(cartItem.ProductID))
				.Any();
		}

		public bool GiftCardSetupComplete(CheckoutEngine.StateMachineContext context)
		{
			return context
				.ShoppingCart
				.CartItems
				.Where(cartItem => GiftCard.ProductIsEmailGiftCard(cartItem.ProductID))
				.Select(cartItem => GiftCardManager.LoadByShoppingCartRecordId(cartItem.ShoppingCartRecordID))
				.All(giftCard => giftCard != null
					&& !string.IsNullOrWhiteSpace(giftCard.EMailName)
					&& !string.IsNullOrWhiteSpace(giftCard.EMailTo));
		}

		public bool PaymentMethodIsOffsite(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedPaymentMethod != null
				&& context.Selections.SelectedPaymentMethod.Location == PaymentMethodLocation.Offsite;
		}

		public bool PaymentMethodIsCreditCard(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedPaymentMethod != null
				&& context.Selections.SelectedPaymentMethod.Name == AppLogic.ro_PMCreditCard;
		}

		public bool CreditCardDetailsMissing(CheckoutEngine.StateMachineContext context)
		{
			if(AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWBRAINTREE)
				return context.Selections.Braintree == null;
			else
				return context.Selections.CreditCard == null;
		}

		public bool PaymentMethodIsPurchaseOrder(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedPaymentMethod != null
				&& context.Selections.SelectedPaymentMethod.Name == AppLogic.ro_PMPurchaseOrder;
		}

		public bool PurchaseOrderDetailsMissing(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.PurchaseOrder == null;
		}

		public bool TermsAndConditionsRequired(CheckoutEngine.StateMachineContext context)
		{
			return context.Configuration.RequireTermsAccepted;
		}

		public bool TermsAndConditionsNotAccepted(CheckoutEngine.StateMachineContext context)
		{
			return !context.Selections.TermsAndConditionsAccepted;
		}

		public bool PaymentMethodIsPayPalExpress(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedPaymentMethod != null
				&& context.Selections.SelectedPaymentMethod.Name == AppLogic.ro_PMPayPalExpress;
		}

		public static bool PaymentMethodIsAmazonPayments(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedPaymentMethod != null
				&& context.Selections.SelectedPaymentMethod.Name == AppLogic.ro_PMAmazonPayments;
		}

		public bool PaymentMethodIsMicroPay(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedPaymentMethod != null
				&& context.Selections.SelectedPaymentMethod.Name == AppLogic.ro_PMMicropay;
		}

		public static bool AmazonPaymentsDetailsMissing(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.AmazonPayments == null;
		}

		public bool ShippingAddressMustMatchBillingAddress(CheckoutEngine.StateMachineContext context)
		{
			return !AppConfigProvider.GetAppConfigValue<bool>("AllowShipToDifferentThanBillTo");
		}

		public bool ShippingAddressDoesNotMatchBillingAddress(CheckoutEngine.StateMachineContext context)
		{
			return context.Selections.SelectedShippingAddress != null
				&& context.Selections.SelectedBillingAddress != null
				&& context.Selections.SelectedBillingAddress.AddressID != 0
				&& context.Selections.SelectedBillingAddress.AddressID != context.Selections.SelectedShippingAddress.AddressID;
		}

		public bool CustomerAccountRequired(CheckoutEngine.StateMachineContext context)
		{
			var checkoutAccountStatus = CheckoutAccountStatusProvider.GetCheckoutAccountStatus(context.Customer, context.Selections.Email);
			return checkoutAccountStatus.State == CheckoutAccountState.Unvalidated;
		}
	}
}
