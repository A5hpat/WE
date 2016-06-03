// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Validation.AddressValidator
{
	public interface IAddressValidationProviderFactory
	{
		IEnumerable<IAddressValidationProvider> Create();
	}

	public interface IAddressValidationProvider
	{
		AddressValidationResult Validate(Address address, AddressTypes addressType = AddressTypes.Unknown);
	}

	public class AddressValidationResult
	{
		public readonly AddressValidationStatus Status;
		public readonly string Message;
		public IEnumerable<Address> CorrectedAddresses;

		public AddressValidationResult(AddressValidationStatus status = AddressValidationStatus.Success, string message = null, IEnumerable<Address> correctedAddresses = null)
		{
			Status = status;
			Message = message;
			CorrectedAddresses = correctedAddresses;
		}
	}

	public enum AddressValidationStatus
	{
		Success,
		Failure
	}
}
