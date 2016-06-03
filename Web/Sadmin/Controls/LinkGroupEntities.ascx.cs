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
	public partial class LinkGroupEntities : System.Web.UI.UserControl
	{
		public string SelectedLink { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			var links = new Dictionary<string, string>();
			links.Add("entities.aspx?entityname=category", AppLogic.GetString("admin.menu.categories", Localization.GetDefaultLocale()));
			links.Add("entities.aspx?entityname=section", AppLogic.GetString("admin.menu.sections", Localization.GetDefaultLocale()));
			links.Add("entities.aspx?entityname=manufacturer", AppLogic.GetString("admin.menu.manufacturers", Localization.GetDefaultLocale()));
			links.Add("entities.aspx?entityname=distributor", AppLogic.GetString("admin.menu.distributors", Localization.GetDefaultLocale()));
			links.Add("producttypes.aspx", AppLogic.GetString("admin.menu.producttypes", Localization.GetDefaultLocale()));
			LinkGroupLinks.Links = links;
		}

		protected override void OnPreRender(EventArgs e)
		{
			DataBind();
			base.OnPreRender(e);
		}

	}
}
