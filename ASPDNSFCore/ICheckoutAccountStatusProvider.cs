// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefrontCore
{
	public interface ICheckoutAccountStatusProvider
	{
		/// <summary>
		/// Gets the status of a email address provided during checkout for the purposes of determining
		/// registered or guest status.
		/// </summary>
		/// <param name="customer">The customer checking out</param>
		/// <param name="email">The email the customer entered</param>
		CheckoutAccountStatus GetCheckoutAccountStatus(Customer customer, string email);
	}

	public class CheckoutAccountStatus
	{
		/// <summary>
		/// The Email this status applies to.
		/// </summary>
		public readonly string Email;

		/// <summary>
		/// Indicates that the customer must log into or create an account - they can not proceed as a guest.
		/// </summary>
		public readonly bool RequireRegisteredCustomer;

		/// <summary>
		/// The current account state of the email address, indicating if the customer is verified to complete 
		/// checkout as a guest or registered customer.
		/// </summary>
		public readonly CheckoutAccountState State;

		/// <summary>
		/// The next available action that the customer can take with the provided account.
		/// </summary>
		public readonly CheckoutAccountAction NextAction;

		public CheckoutAccountStatus(string email, CheckoutAccountState state, CheckoutAccountAction nextAction, bool requireRegisteredCustomer)
		{
			Email = email;
			State = state;
			NextAction = nextAction;
			RequireRegisteredCustomer = requireRegisteredCustomer;
		}
	}

	/// <summary>
	/// Indicates the next available account action during checkout, taking into account guest checkout 
	/// configuration, duplicate email rules, and other factors.
	/// </summary>
	public enum CheckoutAccountAction
	{
		/// <summary>
		/// The customer must first provide an email address.
		/// </summary>
		MustProvideEmail,

		/// <summary>
		/// The customer is has provided an email address that they can log into if they provide the
		/// password.
		/// </summary>
		CanLogin,

		/// <summary>
		/// The customer has provided an email address that they can create a new account for if
		/// they provide a new password for it.
		/// </summary>
		CanCreateAccount,

		/// <summary>
		/// The customer is in a final state where they cannot provide any further login information.
		/// </summary>
		None,
	}

	/// <summary>
	/// Indicates the state of a customer's account during checkout.
	/// </summary>
	public enum CheckoutAccountState
	{
		/// <summary>
		/// The customer has not provided enough information to be considered either a guest or registered.
		/// </summary>
		Unvalidated,

		/// <summary>
		/// The customer can complete checkout as a guest.
		/// </summary>
		Guest,

		/// <summary>
		/// The customer can complete checkout as a registered customer.
		/// </summary>
		Registered,
	}
}
