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
	public partial class quantitydiscounts : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
		}

		protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if(e.Row.RowType != DataControlRowType.DataRow)
				return;

			//Delete button
			LinkButton lnkDelete = (LinkButton)e.Row.FindControl("lnkDelete");
			lnkDelete.Attributes.Add("onClick", "javascript: return confirm('Are you sure you want to delete this quantity discount table?')");
		}

		protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == "DeleteItem")
			{
				gMain.EditIndex = -1;
				int quantityDiscountId = Localization.ParseNativeInt(e.CommandArgument.ToString());

				if(DeleteAllTraceOfThisQuantityDiscount(quantityDiscountId))
					AlertMessage.PushAlertMessage("Delete Successful", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
				else
					AlertMessage.PushAlertMessage("Delete Failed. Check the system log.", AspDotNetStorefrontControls.AlertMessage.AlertType.Danger);
			}
		}

		protected bool DeleteAllTraceOfThisQuantityDiscount(int qtyDiscountId)
		{
			try
			{
				//update entity tables
				DB.ExecuteSQL("update dbo.category set QuantityDiscountID=0 where QuantityDiscountID=" + qtyDiscountId);
				DB.ExecuteSQL("update dbo.section set QuantityDiscountID=0 where QuantityDiscountID=" + qtyDiscountId);
				DB.ExecuteSQL("update dbo.genre set QuantityDiscountID=0 where QuantityDiscountID=" + qtyDiscountId);
				DB.ExecuteSQL("update dbo.manufacturer set QuantityDiscountID=0 where QuantityDiscountID=" + qtyDiscountId);
				DB.ExecuteSQL("update dbo.library set QuantityDiscountID=0 where QuantityDiscountID=" + qtyDiscountId);
				DB.ExecuteSQL("update dbo.distributor set QuantityDiscountID=0 where QuantityDiscountID=" + qtyDiscountId);
				DB.ExecuteSQL("update dbo.vector set QuantityDiscountID=0 where QuantityDiscountID=" + qtyDiscountId);

				//update product table
				DB.ExecuteSQL("update dbo.product set QuantityDiscountID=0 where QuantityDiscountID=" + qtyDiscountId);

				//delete from quantitydiscount tables
				DB.ExecuteSQL("delete from QuantityDiscountTable where QuantityDiscountID=" + qtyDiscountId);
				DB.ExecuteSQL("delete from QuantityDiscount where QuantityDiscountID=" + qtyDiscountId);

				return true;
			}
			catch(Exception ex)
			{
				SysLog.LogMessage("Delete of QuantityDiscount Failed", ex.Message, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Alert);
				return false;
			}
		}
	}
}
