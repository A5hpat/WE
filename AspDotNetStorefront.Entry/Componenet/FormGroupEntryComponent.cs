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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Entry.Component
{
	public class FormGroupEntryComponent : IEntryComponenet<FormGroupContext>
	{
		public IHtmlString Build<TModel, TValue>(
			HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			FormGroupContext context)
		{
			var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
			var sizeCssClass = GetSizeClass(context.DisplayWidth);

			var tagBuilder = new TagBuilder("div");

			tagBuilder.AddCssClass("form-group has-feedback");
			if(!string.IsNullOrEmpty(sizeCssClass))
				tagBuilder.AddCssClass(sizeCssClass);
			if(!string.IsNullOrEmpty(context.AdditionalCssClasses))
				tagBuilder.AddCssClass(context.AdditionalCssClasses);

			tagBuilder.MergeAttributes(context.HtmlAttributes);
			tagBuilder.InnerHtml = context.Contents.ToString();

			return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
		}

		string GetSizeClass(EntrySize displayWidth)
		{
			if(displayWidth == EntrySize.ExtraSmall)
				return "form-group-xsmall";

			if(displayWidth == EntrySize.Small)
				return "form-group-small";

			if(displayWidth == EntrySize.Medium)
				return "form-group-medium";

			if(displayWidth == EntrySize.Large)
				return "form-group-large";

			return "form-group-full";
		}
	}

	public class FormGroupContext
	{
		public readonly IHtmlString Contents;
		public readonly string AdditionalCssClasses;
		public readonly IDictionary<string, object> HtmlAttributes;
		public readonly EntrySize DisplayWidth;

		public FormGroupContext(IHtmlString contents, string additionalCssClasses = null, EntrySize displayWidth = EntrySize.Full, IDictionary<string, object> htmlAttributes = null)
		{
			Contents = contents;
			AdditionalCssClasses = additionalCssClasses;
			DisplayWidth = displayWidth;
			HtmlAttributes = htmlAttributes;
		}
	}
}
