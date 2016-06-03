// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace AspDotNetStorefront.Entry.Component
{
	public class CaptchaImageEntryComponent : IEntryComponenet<CaptchaImageContext>
	{
		public IHtmlString Build<TModel, TValue>(
			HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			CaptchaImageContext context)
		{
			var tagBuilder = new TagBuilder("img")
			{
				Attributes =
				{
					{ "src", context.ImageUrl }
				}
			};

			return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing));
		}
	}

	public class CaptchaImageContext
	{
		public readonly string ImageUrl;

		public CaptchaImageContext(string imageUrl)
		{
			ImageUrl = imageUrl;
		}
	}
}
