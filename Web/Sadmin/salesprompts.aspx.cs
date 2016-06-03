// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class salesprompts : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected const string DeleteCommand = "salesPrompt:delete";

		protected void RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == DeleteCommand)
			{
				var salesPromptId = Localization.ParseNativeInt((string)e.CommandArgument);

				try
				{
					DB.ExecuteSQL(
						"update SalesPrompt set Deleted = 1 where SalesPromptID = @id",
						new[] { new SqlParameter("@id", salesPromptId) });

					ctrlAlertMessage.PushAlertMessage("Sales prompt deleted", AlertMessage.AlertType.Success);
				}
				catch(Exception exception)
				{
					ctrlAlertMessage.PushAlertMessage("Unable to delete sales prompt:<br />" + exception.ToString(), AlertMessage.AlertType.Error);
				}

				FilteredListing.Rebind();
			}
		}
	}
}
