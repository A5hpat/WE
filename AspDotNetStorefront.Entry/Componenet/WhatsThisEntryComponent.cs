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
using AspDotNetStorefront.Validation;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Entry.Component
{
	public class WhatsThisEntryComponent : IEntryComponenet<WhatsThisContext>
	{
		public IHtmlString Build<TModel, TValue>(
			HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			WhatsThisContext context)
		{
			var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

			var content = context.WhatsThisOverride
				?? (metadata.AdditionalValues.ContainsKey(AdnsfModelMetadataProvider.WhatsThisContentKey)
					? metadata.AdditionalValues[AdnsfModelMetadataProvider.WhatsThisContentKey] as string
					: null);

			if(content == null)
				return MvcHtmlString.Empty;

			var labelText = context.LabelText
				?? (metadata.AdditionalValues.ContainsKey(AdnsfModelMetadataProvider.WhatsThisLabelKey)
					? metadata.AdditionalValues[AdnsfModelMetadataProvider.WhatsThisLabelKey] as string
					: null)
				?? "<i class=\"fa fa-question - circle\"></i>";

			var title = context.Title
				?? (metadata.AdditionalValues.ContainsKey(AdnsfModelMetadataProvider.WhatsThisTitleKey)
					? metadata.AdditionalValues[AdnsfModelMetadataProvider.WhatsThisTitleKey] as string
					: null);

			var popoverTagBuilder = new TagBuilder("span");
			if(!string.IsNullOrEmpty(context.AdditionalCssClasses))
				popoverTagBuilder.AddCssClass(context.AdditionalCssClasses);
			popoverTagBuilder.AddCssClass("whats-this js-whats-this");
			popoverTagBuilder.InnerHtml = labelText;
			popoverTagBuilder.MergeAttributes(
				new Dictionary<string, object>
					{
							{ "rel", "popover" },
							{ "data-placement", "auto top" },
							{ "data-original-title", title },
							{ "data-content" , content },
					},
				replaceExisting: true);
			popoverTagBuilder.MergeAttributes(context.HtmlAttributes, replaceExisting: true);

			var popoverTag = popoverTagBuilder.ToString(TagRenderMode.Normal);

			return new MvcHtmlString(popoverTag);
		}
	}

	public class WhatsThisContext
	{
		public readonly string WhatsThisOverride;
		public readonly string LabelText;
		public readonly string Title;
		public readonly string AdditionalCssClasses;
		public readonly IDictionary<string, object> HtmlAttributes;

		public WhatsThisContext(string whatsThisOverride = null, string labelText = null, string title = null, string additionalCssClasses = null, IDictionary<string, object> htmlAttributes = null)
		{
			WhatsThisOverride = whatsThisOverride;
			LabelText = labelText;
			Title = title;
			AdditionalCssClasses = additionalCssClasses;
			HtmlAttributes = htmlAttributes;
		}
	}
}
