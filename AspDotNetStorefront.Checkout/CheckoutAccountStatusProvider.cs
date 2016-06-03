// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout
{
	public class CheckoutAccountStatusProvider : ICheckoutAccountStatusProvider
	{
		readonly AppConfigProvider AppConfigProvider;
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public CheckoutAccountStatusProvider(AppConfigProvider appConfigProvider, ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			AppConfigProvider = appConfigProvider;
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		public CheckoutAccountStatus GetCheckoutAccountStatus(Customer customer, string email)
		{
			var guestCheckoutType = AppConfigProvider.GetAppConfigValue<GuestCheckoutType>("GuestCheckout");

			// If the customer is already logged in, we've got everything we need.
			// Just set the mode to "Registered" to indicate we have a "locked" registered customer.
			if(customer.IsRegistered)
				return new CheckoutAccountStatus(
					email: customer.EMail,
					state: CheckoutAccountState.Registered,
					nextAction: CheckoutAccountAction.None,
					requireRegisteredCustomer: false);

			// If the customer is not logged in and hasn't given us an email, we don't have anything to work with yet.
			// Set the mode to "EnterEmail" to indicate we need that first.
			if(string.IsNullOrWhiteSpace(email))
				return new CheckoutAccountStatus(
					email: string.Empty,
					state: CheckoutAccountState.Unvalidated,
					nextAction: CheckoutAccountAction.MustProvideEmail,
					requireRegisteredCustomer: false);

			// If we have an email address and we're forcing guest checkouts, then we will never need anything more.
			// Set the mode to "Guest" to indicate we have a "locked" guest customer.
			if(guestCheckoutType == GuestCheckoutType.PasswordNeverRequestedAtCheckout)
				return new CheckoutAccountStatus(
					email: email,
					state: CheckoutAccountState.Guest,
					nextAction: CheckoutAccountAction.None,
					requireRegisteredCustomer: false);

			// If we got this far, it means we have an email that may or may not be registered. The remaining modes
			// depends on knowing which.
			var emailIsAlreadyRegistered = Customer.EmailInUse(
				email: email,
				customerId: customer.CustomerID,
				excludeUnregisteredUsers: true);

			// Finally, decide if we require a logged in user.
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());
			var requireRegisteredCustomer =
				cart.HasRecurringComponents()
				|| guestCheckoutType == GuestCheckoutType.Disabled
				|| (emailIsAlreadyRegistered && guestCheckoutType != GuestCheckoutType.AllowRegisteredCustomers);

			// Now we can return a status that indicates the following:
			//	- The email address is already registered and the user must login to continue using it
			//	- The email address is already registered, but the user may continue using it without logging in
			//	- The email address is unregistered, but must create an account to continue using it
			//	- The email address is unregistered and the user may continue using it without creating an account
			return new CheckoutAccountStatus(
				email: email,
				state: requireRegisteredCustomer
					? CheckoutAccountState.Unvalidated
					: CheckoutAccountState.Guest,
				nextAction: emailIsAlreadyRegistered
					? CheckoutAccountAction.CanLogin
					: CheckoutAccountAction.CanCreateAccount,
				requireRegisteredCustomer: requireRegisteredCustomer);
		}
	}
}
