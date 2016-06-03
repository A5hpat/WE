// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for producttypes.
	/// </summary>
	public partial class ProductTypes : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if(e.Row.RowType == DataControlRowType.DataRow)
				((LinkButton)e.Row.FindControl("Delete")).Attributes.Add("onclick", "javascript: return confirm('Confirm Delete?')");
		}

		protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == "DeleteItem")
			{
				try
				{
					DeleteProductType(Localization.ParseNativeInt(e.CommandArgument.ToString()));
					ctlAlertMessage.PushAlertMessage("Item Deleted", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
				}
				catch (Exception ex)
				{
					ctlAlertMessage.PushAlertMessage(ex.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				}
				FilteredListing.Rebind();
			}
		}

		protected void btnUpdate_Click(object sender, EventArgs e)
		{
			Page.Validate("DisplayOrder");
			if(!Page.IsValid)
			{
				ctlAlertMessage.PushAlertMessage("Make sure you've specified a display order", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return;
			}

			UpdateItems();
			ctlAlertMessage.PushAlertMessage("Your values were successfully saved.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
		}

		protected void DeleteProductType(int typeId)
		{
			DB.ExecuteSQL("DELETE FROM ProductType WHERE ProductTypeID=" + typeId.ToString());
		}

		protected void UpdateItems()
		{
			foreach(GridViewRow row in gMain.Rows)
			{
				var typeId = (Literal)row.FindControl("ProductTypeID");
				if(typeId == null)
					return;

				var displayOrder = (TextBox)row.FindControl("DisplayOrder");
				if(displayOrder == null)
					return;

				DB.ExecuteSQL(String.Format("UPDATE ProductType SET DisplayOrder = {0} WHERE ProductTypeID = {1}", displayOrder.Text, typeId.Text));
			}
		}
	}
}
