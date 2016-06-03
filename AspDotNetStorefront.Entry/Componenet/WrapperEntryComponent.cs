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
	public class WrapperEntryComponent : IEntryComponenet<WrapperContext>
	{
		public IHtmlString Build<TModel, TValue>(
			HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TValue>> expression,
			WrapperContext context)
		{
			var tagBuilder = new TagBuilder(context.Tag);
			if(!string.IsNullOrEmpty(context.Class))
				tagBuilder.AddCssClass(context.Class);
			tagBuilder.InnerHtml = context.Contents.ToString();

			return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
		}
	}

	public class WrapperContext
	{
		public readonly string Tag;
		public readonly string Class;
		public readonly IHtmlString Contents;

		public WrapperContext(string tag, string @class, IHtmlString contents)
		{
			Tag = tag;
			Class = @class;
			Contents = contents;
		}
	}
}
