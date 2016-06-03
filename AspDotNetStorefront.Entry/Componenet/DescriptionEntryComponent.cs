// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace AspDotNetStorefront.Entry.Component
{
	public class DescriptionEntryComponent : IEntryComponenet<DescriptionContext>
	{
		public IHtmlString Build<TModel, TValue>(
			HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			DescriptionContext context)
		{
			var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
			var descriptionValue = context.DescriptionOverride ?? metadata.Description;

			if(string.IsNullOrEmpty(descriptionValue))
				return MvcHtmlString.Empty;

			var tagBuilder = new TagBuilder("div");
			if(!string.IsNullOrEmpty(context.AdditionalCssClasses))
				tagBuilder.AddCssClass(context.AdditionalCssClasses);
			tagBuilder.AddCssClass("form-description");
			tagBuilder.MergeAttributes(context.HtmlAttributes);
			tagBuilder.InnerHtml = descriptionValue;

			var tag = tagBuilder.ToString(TagRenderMode.Normal);

			return new MvcHtmlString(tag);
		}
	}

	public class DescriptionContext
	{
		public readonly string DescriptionOverride;
		public readonly string AdditionalCssClasses;
		public readonly IDictionary<string, object> HtmlAttributes;

		public DescriptionContext(string descriptionOverride = null, string additionalCssClasses = null, IDictionary<string, object> htmlAttributes = null)
		{
			DescriptionOverride = descriptionOverride;
			AdditionalCssClasses = additionalCssClasses;
			HtmlAttributes = htmlAttributes;
		}
	}
}
