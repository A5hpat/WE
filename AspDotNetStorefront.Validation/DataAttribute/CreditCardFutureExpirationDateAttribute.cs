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
using System.Web.Mvc;

namespace AspDotNetStorefront.Validation.DataAttribute
{
	public sealed class CreditCardFutureExpirationDateAttribute : ValidationAttribute, IClientValidatable
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			// Allow empty values
			var stringValue = Convert.ToString(value);
			if(string.IsNullOrEmpty(stringValue))
				return ValidationResult.Success;

			// This array will contain the month and year
			var parts = new int[2];

			// Parse out the string into numeric month and year
			var parseResults = stringValue
				.Split(new[] { '/', '-' }, 2, StringSplitOptions.RemoveEmptyEntries)
				.Select(part => part.Trim())
				.Select((part, index) => int.TryParse(part, out parts[index]));

			// If the value can't be parsed, pass. This validator is only for correct inputs.
			if(parseResults.Count() != 2 || !parseResults.All(succes => succes))
				return ValidationResult.Success;

			// Convert two digit years to four digits
			if(parts[1] < 100)
				parts[1] += 2000;

			// Compare the parsed date to the current date
			if(parts[1] < DateTime.Now.Year)
				return new ValidationResult(ErrorMessage);

			if(parts[1] == DateTime.Now.Year && parts[0] < DateTime.Now.Month)
				return new ValidationResult(ErrorMessage);

			return ValidationResult.Success;
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			yield return new ModelClientValidationRule
			{
				ValidationType = "creditcardfutureexpirationdate",
				ErrorMessage = ErrorMessage,
			};
		}
	}
}
