// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Checkout
{
	public class AddressHeaderProvider
	{
		public string GetHeaderText(int? addressId, AddressTypes addressType)
		{
			//Does this address already exist?
			var editing = addressId != null;

			//This way the entire final string can be properly translated
			switch(addressType)
			{
				case AddressTypes.Billing:
					return editing
						? "Edit Billing Address"
                        : "Add New Billing Address";
				case AddressTypes.Shipping:
					return editing
						? "Edit Shipping Address"
                        : "Add New BillingShipping Address";
                default:
					return "Edit Address";
			}
		}
	}
}
