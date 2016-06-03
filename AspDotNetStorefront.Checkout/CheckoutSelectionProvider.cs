// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout
{
	public class CheckoutSelectionProvider : ICheckoutSelectionProvider
	{
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;

		public CheckoutSelectionProvider(IPersistedCheckoutContextProvider persistedCheckoutContextProvider)
		{
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
		}

		public CheckoutSelectionContext GetCheckoutSelection(Customer customer, PaymentMethodInfo selectedPaymentMethod, PersistedCheckoutContext persistedCheckoutContext)
		{
			var validatedBillingAddress = LoadAddress(customer.PrimaryBillingAddressID);
			var validatedShippingAddress = LoadAddress(customer.PrimaryShippingAddressID);

			// Build up the selections context
			return new CheckoutSelectionContext(
				selectedPaymentMethod: selectedPaymentMethod,
				selectedBillingAddress: validatedBillingAddress,
				selectedShippingAddress: validatedShippingAddress,
				selectedShippingMethodId: persistedCheckoutContext.SelectedShippingMethodId,
				creditCard: persistedCheckoutContext.CreditCard,
				payPalExpress: persistedCheckoutContext.PayPalExpress,
				amazonPayments: persistedCheckoutContext.AmazonPayments,
				purchaseOrder: persistedCheckoutContext.PurchaseOrder,
				braintree: persistedCheckoutContext.Braintree,
				termsAndConditionsAccepted: persistedCheckoutContext.TermsAndConditionsAccepted,
				over13Checked: persistedCheckoutContext.Over13Checked || customer.IsOver13,
				email: persistedCheckoutContext.Email ?? customer.EMail);
		}

		Address LoadAddress(int? addressId)
		{
			if(addressId == null || addressId == 0)
				return null;

			var address = new Address();
			address.LoadFromDB(addressId.Value);

			if(address.AddressID == 0)
				return null;

			return address;
		}

		public CheckoutSelectionApplicationResult ApplyCheckoutSelections(Customer customer, CheckoutSelectionContext context)
		{
			// Gate customer updates so we don't needlessly invalidate the cache
			if((context.SelectedPaymentMethod == null && customer.RequestedPaymentMethod != null)
				|| context.SelectedPaymentMethod.Name != customer.RequestedPaymentMethod
				|| (context.SelectedBillingAddress == null && customer.PrimaryBillingAddressID != 0)
				|| (context.SelectedBillingAddress != null && context.SelectedBillingAddress.AddressID != customer.PrimaryBillingAddressID)
				|| (context.SelectedShippingAddress == null && customer.PrimaryShippingAddressID != 0)
				|| (context.SelectedShippingAddress != null && context.SelectedShippingAddress.AddressID != customer.PrimaryShippingAddressID)
				|| context.Email != customer.EMail)
			{
				// While "Over 13" is put into the CheckoutSelectionContext above, it's not persisted back until the order is placed.
				customer.UpdateCustomer(
					requestedPaymentMethod: context.SelectedPaymentMethod == null
						? string.Empty
						: context.SelectedPaymentMethod.Name,
					billingAddressId: context.SelectedBillingAddress == null
						? 0
						: context.SelectedBillingAddress.AddressID,
					shippingAddressId: context.SelectedShippingAddress == null
						? 0
						: context.SelectedShippingAddress.AddressID,
					email: string.IsNullOrWhiteSpace(context.Email) || customer.IsRegistered
						? customer.EMail
						: context.Email);

				customer = new Customer(customer.CustomerID);
			}

			var originalPersistedCheckoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var updatedPersistedCheckoutContext = new PersistedCheckoutContext(
				creditCard: context.CreditCard,
				payPalExpress: context.PayPalExpress,
				purchaseOrder: context.PurchaseOrder,
				braintree: context.Braintree,
				amazonPayments: context.AmazonPayments,
				termsAndConditionsAccepted: context.TermsAndConditionsAccepted,
				over13Checked: context.Over13Checked,
				shippingEstimateDetails: originalPersistedCheckoutContext.ShippingEstimateDetails,
				offsiteRequiresBillingAddressId: originalPersistedCheckoutContext.OffsiteRequiresBillingAddressId,
				offsiteRequiresShippingAddressId: originalPersistedCheckoutContext.OffsiteRequiresShippingAddressId,
				email: context.Email,
				selectedShippingMethodId: context.SelectedShippingMethodId);

			PersistedCheckoutContextProvider.SaveCheckoutContext(customer, updatedPersistedCheckoutContext);

			return new CheckoutSelectionApplicationResult(
				customer: customer,
				selectedPaymentMethod: context.SelectedPaymentMethod,
				persistedCheckoutContext: updatedPersistedCheckoutContext);
		}
	}
}
