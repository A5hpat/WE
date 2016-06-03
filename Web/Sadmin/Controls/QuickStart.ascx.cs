// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
	public partial class QuickStart : System.Web.UI.UserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if(!string.IsNullOrEmpty(CommonLogic.QueryStringCanBeDangerousContent("showquickstart")))
				AppConfigManager.SetAppConfigValue("ShowQuickStart", "true", AppLogic.StoreID());

			Visible = AppLogic.AppConfigBool("ShowQuickStart");
		}

		protected void HideQuickStart(Object sender, CommandEventArgs e)
		{
			AppConfigManager.SetAppConfigValue("ShowQuickStart", "false", AppLogic.StoreID());
			Visible = false;
		}
	}
}
