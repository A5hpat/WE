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
using System.Web.Mvc.Html;

namespace AspDotNetStorefront.Entry.Component
{
	public class CheckboxEntryComponent : IEntryComponenet<CheckboxContext>
	{
		public IHtmlString Build<TModel, TValue>(
			HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			CheckboxContext context)
		{
			var htmlAttributes = new Dictionary<string, string>();
			
			if(context.HtmlAttributes != null)
				foreach(var attribute in context.HtmlAttributes)
					htmlAttributes[attribute.Key] = attribute.Value.ToString();
			
			return htmlHelper.EditorFor(
				expression: expression,
				additionalViewData: new { htmlAttributes });
		}
	}

	public class CheckboxContext
	{
		public readonly string AdditionalCssClasses;
		public readonly IDictionary<string, object> HtmlAttributes;

		public CheckboxContext(string additionalCssClasses = null, IDictionary<string, object> htmlAttributes = null)
		{
			AdditionalCssClasses = additionalCssClasses;
			HtmlAttributes = htmlAttributes;
		}
	}
}
