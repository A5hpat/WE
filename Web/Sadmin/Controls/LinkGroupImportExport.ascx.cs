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
	public partial class LinkGroupImportExport : System.Web.UI.UserControl
	{
		public string SelectedLink { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			var links = new Dictionary<string, string>();
			links.Add("importproductsfromexcel.aspx", AppLogic.GetString("admin.menu.productloadfromexcel", Localization.GetDefaultLocale()));
			links.Add("importproductsfromxml.aspx", AppLogic.GetString("admin.menu.productloadfromxml", Localization.GetDefaultLocale()));
			links.Add("exportproductpricing.aspx", AppLogic.GetString("admin.menu.priceexport", Localization.GetDefaultLocale()));
			links.Add("importproductpricing.aspx", AppLogic.GetString("admin.menu.priceimport", Localization.GetDefaultLocale()));
			LinkGroupLinks.Links = links;
		}

		protected override void OnPreRender(EventArgs e)
		{
			DataBind();
			base.OnPreRender(e);
		}

	}
}
