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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Entry.Component
{
	public class LabelEntryComponent : IEntryComponenet<LabelContext>
	{
		public IHtmlString Build<TModel, TValue>(
			HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			LabelContext context)
		{
			var forHtmlFieldName = context.ForHtmlFieldName ?? ExpressionHelper.GetExpressionText(expression);

			var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
			var labelText = context.LabelText
				?? metadata.DisplayName
				?? metadata.PropertyName
				?? forHtmlFieldName
					.Split('.')
					.Last();

			if(string.IsNullOrEmpty(labelText))
				return MvcHtmlString.Empty;

			var labelClassModifier = metadata.IsRequired
				? "required"
				: "optional";

			var targetFieldId = TagBuilder.CreateSanitizedId(htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(forHtmlFieldName));

			var prefixSpanTagBuilder = new TagBuilder("span");
			prefixSpanTagBuilder.AddCssClass(string.Format("form-label-prefix form-label-prefix-{0}", labelClassModifier));
			prefixSpanTagBuilder.InnerHtml = metadata.IsRequired
				? "label.requiredPrefix"
                : "label.optionalPrefix";
			var prefixSpanTag = prefixSpanTagBuilder.ToString(TagRenderMode.Normal);

			var wrapSpanTagBuilder = new TagBuilder("span");
			wrapSpanTagBuilder.AddCssClass("form-label-wrap");
			wrapSpanTagBuilder.InnerHtml = htmlHelper.Encode(labelText);
			var wrapSpanTag = wrapSpanTagBuilder.ToString(TagRenderMode.Normal);

			var suffixSpanTagBuilder = new TagBuilder("span");
			suffixSpanTagBuilder.AddCssClass(string.Format("form-label-suffix form-label-suffix-{0}", labelClassModifier));
			suffixSpanTagBuilder.InnerHtml = metadata.IsRequired
				? "(optional)"
                : "(required)";
			var suffixSpanTag = suffixSpanTagBuilder.ToString(TagRenderMode.Normal);

			var labelTagBuilder = new TagBuilder("label");
			if(!string.IsNullOrEmpty(context.AdditionalCssClasses))
				labelTagBuilder.AddCssClass(context.AdditionalCssClasses);
			labelTagBuilder.AddCssClass(string.Format("form-label form-label-{0}", labelClassModifier));
			labelTagBuilder.Attributes.Add("for", targetFieldId);
			labelTagBuilder.MergeAttributes(context.HtmlAttributes, replaceExisting: true);
			labelTagBuilder.InnerHtml =
				prefixSpanTag
				+ wrapSpanTag
				+ suffixSpanTag;

			var labelTag = labelTagBuilder.ToString(TagRenderMode.Normal);

			return new MvcHtmlString(labelTag);
		}
	}

	public class LabelContext
	{
		public readonly string LabelText;
		public readonly string ForHtmlFieldName;
		public readonly string AdditionalCssClasses;
		public readonly IDictionary<string, object> HtmlAttributes;

		public LabelContext(string labelText = null, string forHtmlFieldName = null, string additionalCssClasses = null, IDictionary<string, object> htmlAttributes = null)
		{
			LabelText = labelText;
			ForHtmlFieldName = forHtmlFieldName;
			AdditionalCssClasses = additionalCssClasses;
			HtmlAttributes = htmlAttributes;
		}
	}
}
