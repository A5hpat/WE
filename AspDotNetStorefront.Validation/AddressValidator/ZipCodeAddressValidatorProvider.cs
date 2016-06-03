// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Validation.AddressValidator
{
	public class ZipCodeAddressValidatorProvider : IAddressValidationProvider
	{
		public AddressValidationResult Validate(Address address, AddressTypes addressType = AddressTypes.Unknown)
		{
			if(AppLogic.ValidatePostalCode(address.Zip, AppLogic.GetCountryID(address.Country)))
				return new AddressValidationResult();
			else
				return new AddressValidationResult(AddressValidationStatus.Failure, AppLogic.GetCountryIsInternational(address.Country)
					? "Invalid format of zip code:"
                    : "Invalid format of postal code:");
		}
	}
}
