// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class CreateOrder : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void btnStartImpersonation_Click(object sender, EventArgs e)
		{
			var button = sender as Button;
			var customerId = 0;

			if(!int.TryParse(button.CommandArgument, out customerId))
			{
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.createorder.error"), AlertMessage.AlertType.Error);
				return;
			}

			Response.Redirect(string.Format("impersonationhandler.axd?customerId={0}", customerId));
		}
	}
}
