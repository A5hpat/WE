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
	public class AddressValidationProviderFactory : IAddressValidationProviderFactory
	{
		const string UspsProviderName = "usps";
		const string AvalaraProviderName = "avalara";

		public IEnumerable<IAddressValidationProvider> Create()
		{
			yield return new PoBoxAddressValidationProvider();
			yield return new ZipCodeAddressValidatorProvider();

			var addressValidationProviderName = AppLogic.AppConfig("VerifyAddressesProvider");
			if(string.IsNullOrEmpty(addressValidationProviderName))
				yield break;

			switch(addressValidationProviderName.ToLowerInvariant())
			{
				case UspsProviderName:
					yield return new UspsAddressValidationProvider();
					break;

				case AvalaraProviderName:
					yield return new AvalaraAddressValidationProvider();
					break;
			}
		}
	}
}
