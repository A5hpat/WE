// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Validation.AddressValidator
{
	public class AvalaraAddressValidationProvider : IAddressValidationProvider
	{
		public AddressValidationResult Validate(Address address, AddressTypes addressType = AddressTypes.Unknown)
		{
			if(address.Country != "United States") // Avalara doesn't validate other countries
				return new AddressValidationResult();

			var correctedAddress = new Address();
			correctedAddress.LoadFromDB(address.AddressID);

			try
			{
				var result = new AvaTax()
					.ValidateAddress(
						customer: AppLogic.GetCurrentCustomer(),
						inputAddress: address,
						ResultAddress: out correctedAddress);

				if(string.IsNullOrEmpty(result))
					return new AddressValidationResult(
						correctedAddresses: new[]
						{
							correctedAddress
						});
				else
					return new AddressValidationResult(
						status: AddressValidationStatus.Failure,
						message: result,
						correctedAddresses: new[]
						{
							correctedAddress
						});
			}
			catch(Exception exception)
			{
				return new AddressValidationResult(
					status: AddressValidationStatus.Failure,
					message: exception.Message);
			}
		}
	}
}
