// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using AspDotNetStorefront.Validation;

namespace AspDotNetStorefront.Entry.Component
{
	public class EditorEntryComponent : IEntryComponenet<EditorContext>
	{
		public IHtmlString Build<TModel, TValue>(
			HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			EditorContext context)
		{
			var htmlAttributes = new Dictionary<string, object>();

			htmlAttributes["class"] = string.Join(
				separator: " ",
				values: new[]
					{
						"form-control",
						context.AdditionalCssClasses,
					}
					.Where(s => !string.IsNullOrEmpty(s)));

			var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
			if(!string.IsNullOrEmpty(metadata.Watermark))
				htmlAttributes["placeholder"] = metadata.Watermark;

			if(metadata.AdditionalValues.ContainsKey(AdnsfModelMetadataProvider.HtmlAutocompleteKey))
				htmlAttributes["autocomplete"] = metadata.AdditionalValues[AdnsfModelMetadataProvider.HtmlAutocompleteKey];

			if(metadata.IsRequired)
				htmlAttributes["required"] = "required";

			if(metadata.AdditionalValues.ContainsKey(AdnsfModelMetadataProvider.StringLengthMaximumKey))
				htmlAttributes["maxlength"] = metadata.AdditionalValues[AdnsfModelMetadataProvider.StringLengthMaximumKey];

			if(context.HtmlAttributes != null)
				foreach(var attribute in context.HtmlAttributes)
					htmlAttributes[attribute.Key] = attribute.Value;

			return htmlHelper.EditorFor(
				expression: expression,
				additionalViewData: new { htmlAttributes });
		}
	}

	public class EditorContext
	{
		public readonly string AdditionalCssClasses;
		public readonly IDictionary<string, object> HtmlAttributes;

		public EditorContext(string additionalCssClasses = null, IDictionary<string, object> htmlAttributes = null)
		{
			AdditionalCssClasses = additionalCssClasses;
			HtmlAttributes = htmlAttributes;
		}
	}
}
