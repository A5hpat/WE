// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontAdmin;
using AspDotNetStorefrontControls;

public partial class Admin_AvalaraConnectionTest : AspDotNetStorefront.Admin.AdminPageBase
{
	protected void Page_Load(object sender, EventArgs e)
	{
		btnCloseTop.DataBind();

		if(!Page.IsPostBack)
		{
			try
			{
				var success = false;
				var reason = string.Empty;

				if(!AppLogic.AppConfigBool("AvalaraTax.Enabled"))
					ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.avalara.NotEnabled"), AlertMessage.AlertType.Warning);
				else
				{
					var avaTax = new AvaTax();
					success = avaTax.TestAddin(out reason);

					if(success)
						ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.avalara.TestSuccess"), AlertMessage.AlertType.Success);
					else if(string.IsNullOrEmpty(reason))
						ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.avalara.TestFailureGeneric"), AlertMessage.AlertType.Error);
					else
						ctlAlertMessage.PushAlertMessage(string.Format(AppLogic.GetString("admin.avalara.TestFailureError"), reason), AlertMessage.AlertType.Error);
				}
			}
			catch(Exception exception)
			{
				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.avalara.TestFailureException") + exception.ToString().Replace("\n", "<br />"), AlertMessage.AlertType.Error);
			}
		}
	}
}
