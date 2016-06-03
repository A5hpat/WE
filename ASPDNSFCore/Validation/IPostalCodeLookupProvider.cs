// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefrontCore.Validation
{
	public interface IPostalCodeLookupProvider
	{
		bool IsEnabled(string country);

		PostalCodeLookupResult Lookup(string postalCode, string country);
	}

	public class PostalCodeLookupResult
	{
		public readonly string City;
		public readonly string State;
		public readonly string Country;

		public PostalCodeLookupResult(string city, string state, string country)
		{
			City = city;
			State = state;
			Country = country;
		}
	}
}
