// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Models.Converter
{
	public class AddressViewModelConverter
	{
		public AddressViewModel ConvertToAddressViewModel(Address address, Customer customer)
		{
			return new AddressViewModel
			{
				Id = address.AddressID > 0
					? address.AddressID
					: (int?)null,
				NickName = address.NickName,
				FirstName = address.FirstName,
				LastName = address.LastName,
				Company = address.Company,
				Address1 = address.Address1,
				Address2 = address.Address2,
				Suite = address.Suite,
				City = address.City,
				State = address.State,
				Zip = address.Zip,
				Country = address.Country,
				Phone = address.Phone,
				ResidenceType = address.ResidenceType.ToString(),
				IsPrimaryBillingAddress = address.AddressID == customer.PrimaryBillingAddressID,
				IsPrimaryShippingAddress = address.AddressID == customer.PrimaryShippingAddressID,
				OffsiteSource = address.OffsiteSource
			};
		}

		public Address ConvertToAddress(AddressViewModel addressViewModel, Customer customer)
		{
			return new Address
			{
				CustomerID = customer.CustomerID,
				AddressID = addressViewModel.Id ?? -1,
				NickName = addressViewModel.NickName ?? string.Empty,
				FirstName = addressViewModel.FirstName ?? string.Empty,
				LastName = addressViewModel.LastName ?? string.Empty,
				Company = addressViewModel.Company ?? string.Empty,
				Address1 = addressViewModel.Address1 ?? string.Empty,
				Address2 = addressViewModel.Address2 ?? string.Empty,
				Suite = addressViewModel.Suite ?? string.Empty,
				City = addressViewModel.City ?? string.Empty,
				State = addressViewModel.State ?? string.Empty,
				Zip = addressViewModel.Zip ?? string.Empty,
				Country = addressViewModel.Country ?? string.Empty,
				Phone = addressViewModel.Phone ?? string.Empty,
				ResidenceType = (ResidenceTypes)Enum.Parse(typeof(ResidenceTypes), addressViewModel.ResidenceType ?? ResidenceTypes.Residential.ToString())
			};
		}
	}
}
