// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Validation.DataAttribute
{
	public class CompareIfAppConfigTrueAttribute : CompareAttribute
	{
		readonly string AppConfigName;

		public CompareIfAppConfigTrueAttribute(string otherProperty, string appConfigName)
			: base(otherProperty)
		{
			AppConfigName = appConfigName;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if(!AppLogic.AppConfigBool(AppConfigName))
				return ValidationResult.Success;

			return base.IsValid(value, validationContext);
		}
	}
}
