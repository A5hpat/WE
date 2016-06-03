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
	public class DropDownListEntryComponent : IEntryComponenet<DropDownListContext>
	{
		public IHtmlString Build<TModel, TValue>(
			HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			DropDownListContext context)
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
			if(metadata.AdditionalValues.ContainsKey(AdnsfModelMetadataProvider.HtmlAutocompleteKey))
				htmlAttributes["autocomplete"] = metadata.AdditionalValues[AdnsfModelMetadataProvider.HtmlAutocompleteKey];

			if(metadata.IsRequired)
				htmlAttributes["required"] = "required";

			htmlAttributes["id"] = context.Id;

			if(context.HtmlAttributes != null)
				foreach(var attribute in context.HtmlAttributes)
					htmlAttributes[attribute.Key] = attribute.Value;

			return htmlHelper.DropDownListFor(
				expression: expression,
				selectList: context.Values,
				optionLabel: context.Default,
				htmlAttributes: htmlAttributes);
		}
	}

	public class DropDownListContext
	{
		public readonly IEnumerable<SelectListItem> Values;
		public readonly string Default;
		public readonly string AdditionalCssClasses;
		public readonly string Id;
		public readonly IDictionary<string, object> HtmlAttributes;

		public DropDownListContext(IEnumerable<SelectListItem> values, string @default = null, string additionalCssClasses = null, string id = null, IDictionary<string, object> htmlAttributes = null)
		{
			Values = values;
			Default = @default;
			AdditionalCssClasses = additionalCssClasses;
			Id = id;
			HtmlAttributes = htmlAttributes;
		}
	}
}
