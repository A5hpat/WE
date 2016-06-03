// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AspDotNetStorefront.Validation.DataAttribute
{
	public class MinValueAttribute : ValidationAttribute, IClientValidatable
	{
		readonly int Value;

		public MinValueAttribute(int value)
		{
			Value = value;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if(value == null)
				return ValidationResult.Success;

			var stringifiedValue = value.ToString();

			int numericValue;
			if(!int.TryParse(stringifiedValue, out numericValue))
				return ValidationResult.Success;

			return numericValue >= Value
				? ValidationResult.Success
				: new ValidationResult(ErrorMessage);
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			return new[] {
				new ModelClientValidationRule
				{
					ValidationType = "min",
					ValidationParameters = {
						{ "val", Value }
					},
					ErrorMessage = ErrorMessage,
				},
			};
		}
	}
}
