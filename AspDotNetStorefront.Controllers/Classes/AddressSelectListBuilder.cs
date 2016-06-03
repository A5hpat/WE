// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Classes
{
	public class AddressSelectListBuilder
	{
		const string SelectListValueField = "Value";
		const string SelectListDataField = "Text";

		public SelectList BuildResidenceTypeSelectList(string selectedValue)
		{
			return new SelectList(
				items: Enum.GetNames(typeof(ResidenceTypes))
					.Where(residenceType => residenceType != ResidenceTypes.Unknown.ToString())
					.Select(residenceType => new SelectListItem
					{
						Text = residenceType,
						Value = residenceType
					}),
				dataValueField: SelectListValueField,
				dataTextField: SelectListDataField,
				selectedValue: selectedValue);
		}

		public SelectList BuildStateSelectList(string country = null, string selectedValue = null)
		{
			var countryId = 0;
			if(string.IsNullOrEmpty(country))
				countryId = AppLogic.GetCountryIDFromTwoLetterISOCode("US");
			else
				countryId = AppLogic.GetCountryID(country);

			return new SelectList(
				items: State
					.GetAllStatesForCountry(countryId)
					.Select(state => new SelectListItem
					{
						Text = state.Name,
						Value = state.Abbreviation
					}),
				dataValueField: SelectListValueField,
				dataTextField: SelectListDataField,
				selectedValue: selectedValue);
		}

		public SelectList BuildCountrySelectList(string selectedValue = null)
		{
			if(string.IsNullOrEmpty(selectedValue))
				selectedValue = AppLogic.GetCountryNameFromTwoLetterISOCode("US");

			return new SelectList(
				items: Country
					.GetAll()
					.Select(country => new SelectListItem
					{
						Text = country.Name,
						Value = country.Name
					}),
				dataValueField: SelectListValueField,
				dataTextField: SelectListDataField,
				selectedValue: selectedValue);
		}
	}
}
