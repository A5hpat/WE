// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls
{
	public partial class LinkGroupProducts : System.Web.UI.UserControl
	{
		public string SelectedLink { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			var links = new Dictionary<string, string>();
			links.Add("products.aspx", AppLogic.GetString("admin.menu.productmgr", Localization.GetDefaultLocale()));
			links.Add("quantitydiscounts.aspx", AppLogic.GetString("admin.menu.quantitydiscounts", Localization.GetDefaultLocale()));
			links.Add("salesprompts.aspx", AppLogic.GetString("admin.menu.salesprompts", Localization.GetDefaultLocale()));
			links.Add("orderoptions.aspx", AppLogic.GetString("admin.menu.orderoptions", Localization.GetDefaultLocale()));
			LinkGroupLinks.Links = links;

			if(SelectedLink != null)
				LinkGroupLinks.SelectedLink = SelectedLink;
		}
	}
}
