// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Validation.DataAttribute
{
	public class PostalCodeAttribute : ValidationAttribute, IClientValidatable
	{
		public override bool RequiresValidationContext
		{ get { return true; } }

		readonly string CountryProperty;

		public PostalCodeAttribute(string countryProperty)
		{
			if(string.IsNullOrEmpty(countryProperty))
				throw new ArgumentException("The countryProperty parameter must not be null or an empty string.");

			CountryProperty = countryProperty;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			// Get the country name from the indicated property
			var countryPropertyInfo = validationContext.ObjectType.GetProperty(CountryProperty);
			if(countryPropertyInfo == null)
				throw new Exception(string.Format("Model does not contain the country property named \"{0}\"", CountryProperty));

			var countryPropertyValue = countryPropertyInfo.GetValue(validationContext.ObjectInstance, null);
			var countryName = (countryPropertyValue ?? string.Empty).ToString();

			// Look up the country's postal code regex and compare it to the value. If the country 
			// does not exist in the database or has no postal code regex, treat it as valid.
			var postalCodeRegexMap = GetCountryPostalCodeRegularExpressions();
			if(!postalCodeRegexMap.ContainsKey(countryName))
				return ValidationResult.Success;

			var postalCodeRegex = postalCodeRegexMap[countryName];
			if(string.IsNullOrWhiteSpace(postalCodeRegex))
				return ValidationResult.Success;

			var stringifiedValue = (value ?? string.Empty).ToString();
			if(!Regex.IsMatch(stringifiedValue, postalCodeRegex))
				return new ValidationResult(ErrorMessage);

			return ValidationResult.Success;
		}

		IDictionary<string, string> GetCountryPostalCodeRegularExpressions()
		{
			return Country
				.GetAll()
				.ToDictionary(
					country => country.Name,
					country => country.PostalCodeRegex);
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			yield return new ModelClientValidationRule
			{
				ValidationType = "postalcoderegexlookup",
				ValidationParameters = {
					{ "lookupkeyname", string.Format("*.{0}", CountryProperty) },
				},
				ErrorMessage = ErrorMessage,
			};
		}
	}
}
