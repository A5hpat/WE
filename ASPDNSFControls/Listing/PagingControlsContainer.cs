// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls.Listing
{
	public class PagingControlsContainer : WebControl, INamingContainer
	{
		protected override HtmlTextWriterTag TagKey
		{ get { return HtmlTextWriterTag.Div; } }

		public PagerContext PagerContext
		{ get; private set; }

		public PageContext PageContext
		{ get; private set; }

		public void UpdatePaging(PagerContext pagerContext, PageContext pageContext)
		{
			PagerContext = pagerContext;
			PageContext = pageContext;
			DataBindChildren();

			Visible = PageContext != null && PageContext.PageCount > 1;
		}
	}
}
