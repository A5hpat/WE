// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AspDotNetStorefront.Validation.DataAttribute;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Models
{
	public class AddressIndexViewModel
	{
		public readonly IEnumerable<AddressViewModel> Addresses;
		public readonly AddressTypes AddressType;
		public readonly Uri ReturnUrl;
		public readonly bool AllowDifferentShipTo;

		public AddressIndexViewModel(IEnumerable<AddressViewModel> addresses, Uri returnUrl, bool allowDifferentShipTo, AddressTypes addressType = AddressTypes.Unknown)
		{
			Addresses = addresses;
			AddressType = addressType;
			ReturnUrl = returnUrl;
			AllowDifferentShipTo = allowDifferentShipTo;
		}
	}

	public class AddressDetailViewModel
	{
		public readonly AddressViewModel Address;
		public readonly AddressTypes AddressType;
		public readonly SelectList ResidenceTypeOptions;
		public readonly SelectList StateOptions;
		public readonly SelectList CountryOptions;
		public readonly string ReturnUrl;
		public readonly string Header;
		public readonly bool ShowCompanyField;
		public readonly bool ShowNickName;
		public readonly bool ShowResidenceTypeField;
		public readonly bool ShowPostalCodeLookup;
		public readonly bool MakePrimary;

		public AddressDetailViewModel(
			AddressViewModel address, 
			SelectList residenceTypeOptions, 
			SelectList stateOptions, 
			SelectList countryOptions, 
			bool showCompanyField, 
			bool showNickName,
			bool showResidenceTypeField, 
			string returnUrl, 
			string header,
			bool showPostalCodeLookup,
			AddressTypes addressType = AddressTypes.Unknown,
			bool makePrimary = false)
		{
			Address = address;
			AddressType = addressType;
			ResidenceTypeOptions = residenceTypeOptions;
			StateOptions = stateOptions;
			CountryOptions = countryOptions;
			ReturnUrl = returnUrl;
			Header = header;
			ShowCompanyField = showCompanyField;
			ShowNickName = showNickName;
			ShowResidenceTypeField = showResidenceTypeField;
			MakePrimary = makePrimary;
			ShowPostalCodeLookup = showPostalCodeLookup;
		}
	}

	public class AddressPostViewModel
	{
		public AddressViewModel Address
		{ get; set; }
	}

	public class AddressViewModel
	{
		public int? Id
		{ get; set; }

		[Display(Name = "address.nickname.label", Prompt = "address.nickname.example")]
		[StringLength(100, ErrorMessage = "address.nickname.length")]
		public string NickName
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.GivenName)]
		[Display(Name = "address.firstname.label", Prompt = "address.firstname.example")]
		[Required(ErrorMessage = "address.firstname.required")]
		[StringLength(100, ErrorMessage = "address.firstname.length")]
		public string FirstName
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.FamilyName)]
		[Display(Name = "address.lastname.label", Prompt = "address.lastname.example")]
		[Required(ErrorMessage = "address.lastname.required")]
		[StringLength(100, ErrorMessage = "address.lastname.length")]
		public string LastName
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.Organization)]
		[Display(Name = "address.company.label", Prompt = "address.company.example")]
		[StringLength(100, ErrorMessage = "address.company.length")]
		public string Company
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.AddressLine1)]
		[Display(Name = "address.address1.label", Prompt = "address.address1.example")]
		[Required(ErrorMessage = "address.address1.required")]
		[StringLength(100, ErrorMessage = "address.address1.length")]
		public string Address1
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.AddressLine2)]
		[Display(Name = "address.address2.label", Prompt = "address.address2.example")]
		[StringLength(100, ErrorMessage = "address.address2.length")]
		public string Address2
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.AddressLine3)]
		[Display(Name = "address.suite.label", Prompt = "address.suite.example")]
		[StringLength(50, ErrorMessage = "address.suite.length")]
		public string Suite
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.AddressLevel2)]
		[Display(Name = "address.city.label", Prompt = "address.city.example")]
		[Required(ErrorMessage = "address.city.required")]
		[StringLength(100, ErrorMessage = "address.city.length")]
		public string City
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.AddressLevel1)]
		[Display(Name = "address.state.label")]
		[Required(ErrorMessage = "address.state.required")]
		[StringLength(100, ErrorMessage = "address.state.length")]
		public string State
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.PostalCode)]
		[StringLength(10, ErrorMessage = "address.zip.length")]
		[RequiredIfContextValid(typeof(AddressDetailViewCountryCodeValidator), ErrorMessage = "address.zip.required")]
		[PostalCode("Country", ErrorMessage= "address.zip.invalid")]
		public string Zip
		{ get; set; }

		[HtmlAutocompleteType(HtmlAutocomplete.CountryName)]
		[Display(Name = "address.country.label")]
		[Required(ErrorMessage = "address.country.required")]
		[StringLength(100, ErrorMessage = "address.country.length")]
		public string Country
		{ get; set; }

		public string OriginShippingCountry
		{
			get { return AppLogic.GetCountryNameFromTwoLetterISOCode(AppLogic.AppConfig("RTShipping.OriginCountry")); }
		}

		[Phone(ErrorMessage = "address.phone.format")]
		[DataType(DataType.PhoneNumber)]
		[HtmlAutocompleteType(HtmlAutocomplete.Telephone)]
		[Display(Name = "address.phone.label", Prompt = "address.phone.example")]
		[RequiredIfAppConfigTrue("AddressPhoneRequired", ErrorMessage = "address.phone.required")]
		[StringLength(25, ErrorMessage = "address.phone.length")]
		public string Phone
		{ get; set; }

		[Display(Name = "address.type.label")]
		public string ResidenceType
		{ get; set; }

		public bool IsPrimaryShippingAddress
		{ get; set; }

		public bool IsPrimaryBillingAddress
		{ get; set; }

		public string OffsiteSource
		{ get; set; }
	}

	public class SelectAddressViewModel
	{
		public int SelectedAddressId
		{ get; set; }

		public AddressViewModel SelectedAddress
		{ get; set; }

		public IEnumerable<AddressViewModel> AddressOptions
		{ get; set; }

		public AddressTypes AddressType
		{ get; set; }

		public string PageTitle
		{ get; set; }

		public bool AddressSelectionLocked
		{ get; set; }
	}

	public class AddressDetailViewCountryCodeValidator : IContextValidationPlugin
	{
		public bool IsRequired(object value)
		{
			var model = value as AddressDetailViewModel;
			if(model == null)
				return false;

			var countryName = string.IsNullOrEmpty(model.Address.Country)
				? model.Address.OriginShippingCountry
				: model.Address.Country;

			if(string.IsNullOrEmpty(countryName))
				return false;
			
			return AppLogic.GetCountryPostalCodeRequired(
				CountryID: AppLogic.GetCountryID(countryName));
		}
	}
}
