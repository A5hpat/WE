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
using AspDotNetStorefront.StringResource;

namespace AspDotNetStorefront.Validation
{
	/// <summary>
	/// Parses <see cref="ValidationAttribute"/> error messages as string resources and wraps all <see cref="ModelValidator"/>'s in <see cref="StringResourceModelValidator"/> so that error messages can use ADNSF string resources.
	/// </summary>
	public class StringResourceWrappedModelValidatorProvider : DataAnnotationsModelValidatorProvider
	{
		readonly IStringResourceProviderFactory StringResourceProviderFactory;

		public StringResourceWrappedModelValidatorProvider(IStringResourceProviderFactory stringResourceProviderFactory)
		{
			StringResourceProviderFactory = stringResourceProviderFactory;
		}

		protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes)
		{
			var stringResourceProvider = StringResourceProviderFactory.Create();

			var flattenedAttributes = attributes.ToArray();

			var validationAttributesWithErrorMessages = flattenedAttributes
				.OfType<ValidationAttribute>()
				.Where(validationAttribute => !string.IsNullOrEmpty(validationAttribute.ErrorMessage));

			foreach(var attribute in validationAttributesWithErrorMessages)
				attribute.ErrorMessage = stringResourceProvider.GetString(attribute.ErrorMessage);

			return base
				.GetValidators(metadata, context, flattenedAttributes)
				.Select(validator => new StringResourceModelValidator(stringResourceProvider, validator, metadata, context));
		}
	}
}
