// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Web.Mvc;
using AspDotNetStorefront.StringResource;

namespace AspDotNetStorefront.Validation
{
	/// <summary>
	/// Wraps a <see cref="ModelValidator"/> so that error messages can use ADNSF string resources.
	/// </summary>
	public class StringResourceModelValidator : ModelValidator
	{
		public override bool IsRequired
		{ get { return InnerValidator.IsRequired; } }

		readonly ModelValidator InnerValidator;
		readonly IStringResourceProvider StringResourceProvider;

		public StringResourceModelValidator(IStringResourceProvider stringResourceProvider, ModelValidator innerValidator, ModelMetadata metadata, ControllerContext controllerContext)
			: base(metadata, controllerContext)
		{
			StringResourceProvider = stringResourceProvider;
			InnerValidator = innerValidator;
		}

		public override IEnumerable<ModelValidationResult> Validate(object container)
		{
			var results = InnerValidator.Validate(container);
			foreach(var result in results)
				result.Message = StringResourceProvider.GetString(result.Message);

			return results;
		}

		public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
		{
			var rules = InnerValidator.GetClientValidationRules();
			foreach(var rule in rules)
				rule.ErrorMessage = StringResourceProvider.GetString(rule.ErrorMessage);

			return rules;
		}
	}
}
