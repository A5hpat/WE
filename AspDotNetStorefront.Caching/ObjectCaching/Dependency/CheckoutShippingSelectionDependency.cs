// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AspDotNetStorefront.Checkout;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Caching.ObjectCaching.Dependency
{
	/// <summary>
	/// Handles a specialized dependency that tracks changes to the customer's shipping selections.
	/// </summary>
	public class CheckoutShippingSelectionDependencyStateManager : IDependencyStateManager
	{
		readonly AppConfigProvider AppConfigProvider;
		readonly ICheckoutSelectionProvider CheckoutSelectionProvider;
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;
		readonly IPaymentMethodInfoProvider PaymentMethodInfoProvider;
		readonly HashProvider HashProvider;

		public CheckoutShippingSelectionDependencyStateManager(
			AppConfigProvider appConfigProvider,
			ICheckoutSelectionProvider checkoutSelectionProvider,
			IPersistedCheckoutContextProvider persistedCheckoutContextProvider,
			IPaymentMethodInfoProvider paymentMethodInfoProvider,
			HashProvider hashProvider)
		{
			AppConfigProvider = appConfigProvider;
			CheckoutSelectionProvider = checkoutSelectionProvider;
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
			PaymentMethodInfoProvider = paymentMethodInfoProvider;
			HashProvider = hashProvider;
		}

		public DependencyState GetState(DependencyStateContext context)
		{
			if(!(context is CheckoutShippingSelectionDependencyStateContext))
				return null;

			// Proceed through a chain of calls to get the customer's selected shipping method, 
			// shipping address, and billing address.
			var checkoutStateContext = (CheckoutShippingSelectionDependencyStateContext)context;

			var customer = new Customer(checkoutStateContext.CustomerId, true);

			var persistedCheckoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var selectedPaymentMethod = PaymentMethodInfoProvider.GetPaymentMethodInfo(
				paymentMethod: customer.RequestedPaymentMethod,
				gateway: AppLogic.ActivePaymentGatewayCleaned());

			var checkoutState = CheckoutSelectionProvider.GetCheckoutSelection(
				customer: customer,
				selectedPaymentMethod: selectedPaymentMethod,
				persistedCheckoutContext: persistedCheckoutContext);

			// Return a state that is the hash of the shipping method ID, billing address, and shipping address.
			return new DependencyState(
				context: context,
				state: HashProvider.Hash(
					new object[] { checkoutState.SelectedShippingMethodId }
					.Concat(GetAddressState(checkoutState.SelectedBillingAddress))
					.Concat(GetAddressState(checkoutState.SelectedShippingAddress))));
		}

		public bool? HasStateChanged(DependencyState establishedState)
		{
			if(!(establishedState.Context is CheckoutShippingSelectionDependencyStateContext))
				return null;

			var currentState = GetState(establishedState.Context);
			var stateChanged = currentState.State != establishedState.State;

			if(stateChanged && AppConfigProvider.GetAppConfigValue<bool>("ObjectCacheDebuggingEnabled"))
				Debug.WriteLine(string.Format(
					"ObjCache - Checkout Shipping Selection Changed - Customer {0}",
					((CheckoutShippingSelectionDependencyStateContext)establishedState.Context).CustomerId));

			return stateChanged;
		}

		IEnumerable<object> GetAddressState(Address address)
		{
			return address == null
				// Null addresses are treated as a collection of zeroes
				? new object[]
					{
						0,
						0,
						0,
						0,
						0,
						0,
						0,
					}
				// Non-null addresses track the address type plus the basic address fields
				: new object[]
					{
						(int)address.AddressType,
						address.Address1,
						address.Address2,
						address.City,
						address.State,
						address.Zip,
						address.Country,
					};
		}
	}

	[DebuggerDisplay("CheckoutSelection: {CustomerId}")]
	public class CheckoutShippingSelectionDependencyStateContext : DependencyStateContext
	{
		public readonly int CustomerId;

		public CheckoutShippingSelectionDependencyStateContext(int customerId)
		{
			CustomerId = customerId;
		}
	}
}
