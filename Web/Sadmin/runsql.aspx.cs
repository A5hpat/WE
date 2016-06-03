// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	[LoggedAuthorize(Roles = "SuperAdmin", LogFailed = true)]
	public partial class runsql : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Page.Form.DefaultButton = btnSubmit.UniqueID;
			Page.Form.DefaultFocus = txtQuery.ClientID;
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(txtQuery.Text))
			{
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.runsql.NoInput"), AlertMessage.AlertType.Error);
				return;
			}

			try
			{
				DB.ExecuteLongTimeSQL(txtQuery.Text, 1000);

				Security.LogEvent(
					"Run SQL Success",
					txtQuery.Text,
					0,
					ThisCustomer.CustomerID,
					ThisCustomer.CurrentSessionID);

				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.runsql.Ok"), AlertMessage.AlertType.Success);
			}
			catch(Exception exception)
			{
				ctrlAlertMessage.PushAlertMessage(CommonLogic.GetExceptionDetail(exception, "<br/>"), AlertMessage.AlertType.Error);
			}
		}
	}
}
