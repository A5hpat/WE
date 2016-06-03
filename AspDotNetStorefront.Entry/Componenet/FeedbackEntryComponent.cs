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
	public class FeedbackEntryComponent : IEntryComponenet<FeedbackContext>
	{
		public IHtmlString Build<TModel, TValue>(
			HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			FeedbackContext context)
		{
			var tagBuilder = new TagBuilder("i");
			if(!string.IsNullOrEmpty(context.AdditionalCssClasses))
				tagBuilder.AddCssClass(context.AdditionalCssClasses);
			tagBuilder.AddCssClass("fa form-control-feedback");
			tagBuilder.MergeAttributes(context.HtmlAttributes);

			return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
		}
	}

	public class FeedbackContext
	{
		public readonly string AdditionalCssClasses;
		public readonly IDictionary<string, object> HtmlAttributes;

		public FeedbackContext(string additionalCssClasses = null, IDictionary<string, object> htmlAttributes = null)
		{
			AdditionalCssClasses = additionalCssClasses;
			HtmlAttributes = htmlAttributes;
		}
	}
}
