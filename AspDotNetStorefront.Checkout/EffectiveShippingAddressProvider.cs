// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout
{
	/// <summary>
	/// During checkout, a customer may be estimating shipping or have actually entered an address. This class determines
	/// what the current case is and returns the currect address.
	/// </summary>
	public class EffectiveShippingAddressProvider : IEffectiveShippingAddressProvider
	{
		readonly IPersistedCheckoutContextProvider PersistedCheckoutContextProvider;

		public EffectiveShippingAddressProvider(IPersistedCheckoutContextProvider persistedCheckoutContextProvider)
		{
			PersistedCheckoutContextProvider = persistedCheckoutContextProvider;
		}

		public Address GetEffectiveShippingAddress(Customer customer)
		{
			var checkoutContext = PersistedCheckoutContextProvider.LoadCheckoutContext(customer);

			var addressIsFromShippingEstimator = customer.PrimaryShippingAddressID == 0
				&& checkoutContext.ShippingEstimateDetails != null;

			return addressIsFromShippingEstimator
				? BuildEstimatorAddressFromContext(checkoutContext.ShippingEstimateDetails, customer)
				: customer.PrimaryShippingAddress;
		}

		Address BuildEstimatorAddressFromContext(ShippingEstimateDetails shippingEstimateDetails, Customer customer)
		{
			var address = new Address(customer.CustomerID, AddressTypes.Shipping);

			if(shippingEstimateDetails == null)
				return address;

			if(!string.IsNullOrEmpty(shippingEstimateDetails.Country))
				address.Country = shippingEstimateDetails.Country;

			if(!string.IsNullOrEmpty(shippingEstimateDetails.City))
				address.City = shippingEstimateDetails.City;

			if(!string.IsNullOrEmpty(shippingEstimateDetails.State))
				address.State = shippingEstimateDetails.State;

			if(!string.IsNullOrEmpty(shippingEstimateDetails.PostalCode))
				address.Zip = shippingEstimateDetails.PostalCode;

			return address;
		}
	}
}
