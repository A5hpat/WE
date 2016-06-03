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

namespace AspDotNetStorefront.Entry.Component
{
	public class ValidationMessageEntryComponent : IEntryComponenet<ValidationMessageContext>
	{
		public IHtmlString Build<TModel, TValue>(
			HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			ValidationMessageContext context)
		{
			var htmlAttributes = new Dictionary<string, object>();

			htmlAttributes["class"] = string.Join(
				separator: " ",
				values: new[]
					{
						"form-validation-message",
						context.AdditionalCssClasses,
					}
					.Where(s => !string.IsNullOrEmpty(s)));

			if(context.HtmlAttributes != null)
				foreach(var attribute in context.HtmlAttributes)
					htmlAttributes[attribute.Key] = attribute.Value;

			return htmlHelper.ValidationMessageFor(
				expression: expression,
				validationMessage: context.Message,
				htmlAttributes: htmlAttributes);
		}
	}

	public class ValidationMessageContext
	{
		public readonly string Message;
		public readonly string AdditionalCssClasses;
		public readonly IDictionary<string, object> HtmlAttributes;

		public ValidationMessageContext(string message = null, string additionalCssClasses = null, IDictionary<string, object> htmlAttributes = null)
		{
			Message = message;
			AdditionalCssClasses = additionalCssClasses;
			HtmlAttributes = htmlAttributes;
		}
	}
}
