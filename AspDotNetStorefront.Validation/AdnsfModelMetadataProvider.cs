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
using AspDotNetStorefront.Validation.DataAttribute;
using AspDotNetStorefront.StringResource;

namespace AspDotNetStorefront.Validation
{
	public class AdnsfModelMetadataProvider : DataAnnotationsModelMetadataProvider
	{
		public const string DisplayWidthKey = "DisplayWidth";
		public const string HtmlAutocompleteKey = "HtmlAutocomplete";
		public const string StringLengthMinimumKey = "StringLength.Minimum";
		public const string StringLengthMaximumKey = "StringLength.Maximum";
		public const string WhatsThisLabelKey = "WhatsThis.Label";
		public const string WhatsThisContentKey = "WhatsThis.Content";
		public const string WhatsThisTitleKey = "WhatsThis.Title";

		readonly IStringResourceProviderFactory StringResourceProviderFactory;

		public AdnsfModelMetadataProvider(IStringResourceProviderFactory stringResourceProviderFactory)
		{
			StringResourceProviderFactory = stringResourceProviderFactory;
		}

		protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
		{
			var metadata = base.CreateMetadata(
				attributes: attributes,
				containerType: containerType,
				modelAccessor: modelAccessor,
				modelType: modelType,
				propertyName: propertyName);

			var modelInstance = GetModelInstance(modelAccessor);

			var stringResourceProvider = StringResourceProviderFactory.Create();

			var displayAttributes = attributes.OfType<DisplayAttribute>();
			foreach(var attribute in displayAttributes)
			{
				metadata.DisplayName = stringResourceProvider.GetString(attribute.Name);
				metadata.Description = stringResourceProvider.GetString(attribute.Description);
				metadata.ShortDisplayName = stringResourceProvider.GetString(attribute.ShortName);
				metadata.Watermark = stringResourceProvider.GetString(attribute.Prompt);
			}

			var autocompleteAttributes = attributes.OfType<HtmlAutocompleteTypeAttribute>();
			foreach(var attribute in autocompleteAttributes)
				metadata.AdditionalValues[HtmlAutocompleteKey] = attribute.GetAutocompleteAttributeValue();

			var whatsThisAttributes = attributes.OfType<WhatsThisAttribute>();
			foreach(var attribute in whatsThisAttributes)
			{
				metadata.AdditionalValues[WhatsThisContentKey] = stringResourceProvider.GetString(attribute.Content);
				metadata.AdditionalValues[WhatsThisLabelKey] = stringResourceProvider.GetString(attribute.Label);
				metadata.AdditionalValues[WhatsThisTitleKey] = stringResourceProvider.GetString(attribute.Title);
			}

			var stringLengthAttributes = attributes.OfType<StringLengthAttribute>();
			foreach(var attribute in stringLengthAttributes)
			{
				metadata.AdditionalValues[StringLengthMinimumKey] = attribute.MinimumLength;
				metadata.AdditionalValues[StringLengthMaximumKey] = attribute.MaximumLength;
			}

			var displayWidthAttributes = attributes.OfType<DisplayWidthAttribute>();
			foreach(var attribute in displayWidthAttributes)
				metadata.AdditionalValues[DisplayWidthKey] = attribute.Size;
			
			var dynamicRequiredAttributes = attributes.OfType<IDynamicRequiredAttribute>();
			foreach(var attribute in dynamicRequiredAttributes)
				metadata.IsRequired = attribute.IsRequired(modelInstance);

			var dynamicDisplayAttributes = attributes.OfType<DynamicDisplayAttribute>();
			foreach(var attribute in dynamicDisplayAttributes)
				metadata.Description = stringResourceProvider.GetString(attribute.GetDescription(modelInstance));

			return metadata;
		}

		object GetModelInstance(Func<object> modelAccessor)
		{
			if(modelAccessor == null)
				return null;

			var target = modelAccessor
				.Target;

			var field = target
				.GetType()
				.GetField("container");

			if(field == null)
				return null;

			return field.GetValue(target);
		}
	}
}
