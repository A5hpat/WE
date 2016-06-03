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
	public partial class LinkGroupRatings : System.Web.UI.UserControl
	{
		public string SelectedLink { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			var links = new Dictionary<string, string>();
			links.Add("ratings.aspx", AppLogic.GetString("admin.menu.ratings", Localization.GetDefaultLocale()));
			links.Add("badword.aspx", AppLogic.GetString("admin.menu.badword", Localization.GetDefaultLocale()));
			LinkGroupLinks.Links = links;

			if(SelectedLink != null)
				LinkGroupLinks.SelectedLink = SelectedLink;
		}
	}
}
