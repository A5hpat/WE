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
	public partial class LinkGroupOrders : System.Web.UI.UserControl
	{
		public string SelectedLink { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			var links = new Dictionary<string, string>();
			links.Add("orders.aspx", AppLogic.GetString("admin.menu.ordermanage", Localization.GetDefaultLocale()));
			links.Add("failedtransactions.aspx", AppLogic.GetString("admin.menu.failedtransactions", Localization.GetDefaultLocale()));
			links.Add("fraudorders.aspx", AppLogic.GetString("admin.menu.fraudorders", Localization.GetDefaultLocale()));
			LinkGroupLinks.Links = links;

			if(SelectedLink != null)
				LinkGroupLinks.SelectedLink = SelectedLink;
		}
	}
}
