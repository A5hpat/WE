// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls.Listing
{
	public partial class DefaultActionRow : UserControl
	{
		protected override void OnPreRender(EventArgs e)
		{
			DataBind();

			base.OnPreRender(e);
		}

		protected void LocaleSelector_DataBound(object sender, EventArgs e)
		{
			var container = (ActionRowContainer)NamingContainer;
			((DropDownList)sender).SelectFirstByValue(container.DisplayLocale);
		}
	}
}
