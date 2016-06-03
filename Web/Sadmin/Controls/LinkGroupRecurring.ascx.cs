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
	public partial class LinkGroupRecurring : System.Web.UI.UserControl
	{
		public string SelectedLink { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			var links = new Dictionary<string, string>();
			links.Add("recurringorders.aspx", AppLogic.GetString("admin.menu.orderrecurringpending", Localization.GetDefaultLocale()));
			links.Add("recurringimport.aspx", AppLogic.GetString("admin.menu.OrderRecurringImport", Localization.GetDefaultLocale()));
			LinkGroupLinks.Links = links;

			if(SelectedLink != null)
				LinkGroupLinks.SelectedLink = SelectedLink;
		}
	}
}
