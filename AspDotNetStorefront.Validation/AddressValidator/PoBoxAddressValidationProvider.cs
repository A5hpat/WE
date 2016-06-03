// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Text.RegularExpressions;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Validation.AddressValidator
{
	public class PoBoxAddressValidationProvider : IAddressValidationProvider
	{
		public AddressValidationResult Validate(Address address, AddressTypes addressType = AddressTypes.Unknown)
		{
			if(!AppLogic.AppConfigBool("DisallowShippingToPOBoxes"))
				return new AddressValidationResult();

			if(addressType != AddressTypes.Shipping)
				return new AddressValidationResult();

			var regEx = new Regex(@"(?i)\b(?:p(?:ost)?\.?\s*[o0](?:ffice)?\.?\s*b(?:[o0]x)?|b[o0]x)");
			if(regEx.IsMatch(address.Address1))
				return new AddressValidationResult(AddressValidationStatus.Failure, "P.O. Box ship-to addresses are not supported. Please enter a full street residential or commercial address. Thanks");
			else
				return new AddressValidationResult();
		}
	}
}
