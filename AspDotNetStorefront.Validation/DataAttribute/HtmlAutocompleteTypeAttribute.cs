// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace AspDotNetStorefront.Validation.DataAttribute
{
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public class HtmlAutocompleteTypeAttribute : Attribute
	{
		public readonly HtmlAutocomplete Autocomplete;

		// This is a positional argument
		public HtmlAutocompleteTypeAttribute(HtmlAutocomplete autocomplete)
		{
			Autocomplete = autocomplete;
		}

		public string GetAutocompleteAttributeValue()
		{
			switch(Autocomplete)
			{
				case HtmlAutocomplete.Off:
					return "off";

				case HtmlAutocomplete.On:
					return "on";

				case HtmlAutocomplete.Name:
					return "name";

				case HtmlAutocomplete.HonorificPrefix:
					return "honorific-prefix";

				case HtmlAutocomplete.GivenName:
					return "given-name";

				case HtmlAutocomplete.AdditionalName:
					return "additional-name";

				case HtmlAutocomplete.FamilyName:
					return "family-name";

				case HtmlAutocomplete.HonorificSuffix:
					return "honorific-suffix";

				case HtmlAutocomplete.Nickname:
					return "nickname";

				case HtmlAutocomplete.Username:
					return "username";

				case HtmlAutocomplete.NewPassword:
					return "new-password";

				case HtmlAutocomplete.CurrentPassword:
					return "current-password";

				case HtmlAutocomplete.OrganizationTitle:
					return "organization-title";

				case HtmlAutocomplete.Organization:
					return "organization";

				case HtmlAutocomplete.StreetAddress:
					return "street-address";

				case HtmlAutocomplete.AddressLine1:
					return "address-line1";

				case HtmlAutocomplete.AddressLine2:
					return "address-line2";

				case HtmlAutocomplete.AddressLine3:
					return "address-line3";

				case HtmlAutocomplete.AddressLevel4:
					return "address-level4";

				case HtmlAutocomplete.AddressLevel3:
					return "address-level3";

				case HtmlAutocomplete.AddressLevel2:
					return "address-level2";

				case HtmlAutocomplete.AddressLevel1:
					return "address-level1";

				case HtmlAutocomplete.Country:
					return "country";

				case HtmlAutocomplete.CountryName:
					return "country-name";

				case HtmlAutocomplete.PostalCode:
					return "postal-code";

				case HtmlAutocomplete.CcName:
					return "cc-name";

				case HtmlAutocomplete.CcGivenName:
					return "cc-given-name";

				case HtmlAutocomplete.CcAdditionalName:
					return "cc-additional-name";

				case HtmlAutocomplete.CcFamilyName:
					return "cc-family-name";

				case HtmlAutocomplete.CcNumber:
					return "cc-number";

				case HtmlAutocomplete.CcExp:
					return "cc-exp";

				case HtmlAutocomplete.CcExpMonth:
					return "cc-exp-month";

				case HtmlAutocomplete.CcExpYear:
					return "cc-exp-year";

				case HtmlAutocomplete.CcCsc:
					return "cc-csc";

				case HtmlAutocomplete.CcType:
					return "cc-type";

				case HtmlAutocomplete.Email:
					return "email";

				case HtmlAutocomplete.TransactionCurrency:
					return "transaction-currency";

				case HtmlAutocomplete.TransactionAmount:
					return "transaction-amount";

				case HtmlAutocomplete.Language:
					return "language";

				case HtmlAutocomplete.Bday:
					return "bday";

				case HtmlAutocomplete.BdayDay:
					return "bday-day";

				case HtmlAutocomplete.BdayMonth:
					return "bday-month";

				case HtmlAutocomplete.BdayYear:
					return "bday-year";

				case HtmlAutocomplete.Sex:
					return "sex";

				case HtmlAutocomplete.Url:
					return "url";

				case HtmlAutocomplete.Photo:
					return "photo";

				case HtmlAutocomplete.Telephone:
					return "tel";

				default:
					return string.Empty;
			}
		}
	}

	public enum HtmlAutocomplete
	{
		Off,
		On,
		Name,
		HonorificPrefix,
		GivenName,
		AdditionalName,
		Email,
		FamilyName,
		HonorificSuffix,
		Nickname,
		Username,
		NewPassword,
		CurrentPassword,
		OrganizationTitle,
		Organization,
		StreetAddress,
		AddressLine1,
		AddressLine2,
		AddressLine3,
		AddressLevel4,
		AddressLevel3,
		AddressLevel2,
		AddressLevel1,
		Country,
		CountryName,
		PostalCode,
		CcName,
		CcGivenName,
		CcAdditionalName,
		CcFamilyName,
		CcNumber,
		CcExp,
		CcExpMonth,
		CcExpYear,
		CcCsc,
		CcType,
		TransactionCurrency,
		TransactionAmount,
		Language,
		Bday,
		BdayDay,
		BdayMonth,
		BdayYear,
		Sex,
		Url,
		Photo,
		Telephone
	}
}
