// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

	public partial class giftcards : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
		}

		protected void grdGiftCards_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "ItemAction")
			{
				string temp = e.CommandArgument.ToString();
				int action = Localization.ParseNativeInt(temp.Substring(0, 1));
				int giftcardid = Localization.ParseNativeInt(temp.Substring(temp.IndexOf("|") + 1));
				updateRow(giftcardid, action);
			}
		}

		protected void grdGiftCards_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if(e.Row.RowType == DataControlRowType.DataRow)
			{
				DataRowView myrow = (DataRowView)e.Row.DataItem;

				LinkButton action = (LinkButton)e.Row.FindControl("lnkAction");
				if(myrow["DisabledByAdministrator"].ToString() == "1")
				{
					action.CommandArgument = "0|" + myrow["GiftCardID"].ToString();
					action.Attributes.Add("onClick", "javascript: return confirm('" + AppLogic.GetString("admin.giftcard.Enable", SkinID, LocaleSetting) + "')");
					action.Text = AppLogic.GetString("admin.common.CapsEnable", SkinID, LocaleSetting);
				}
				else
				{
					action.CommandArgument = "1|" + myrow["GiftCardID"].ToString();
					action.Attributes.Add("onClick", "javascript: return confirm('" + AppLogic.GetString("admin.giftcard.Disable", SkinID, LocaleSetting) + "')");
					action.Text = AppLogic.GetString("admin.common.CapsDisable", SkinID, LocaleSetting);
				}
			}
		}

		protected void updateRow(int giftcardid, int action)
		{
			string message = string.Empty;
			try
			{
				DB.ExecuteSQL("UPDATE GiftCard SET DisabledByAdministrator=" + action + " WHERE GiftCardID=" + giftcardid.ToString());
				if (action == 1)
				{
					message = AppLogic.GetString("admin.giftcard.Disabled", SkinID, LocaleSetting);
				}
				else
				{
					message = AppLogic.GetString("admin.giftcard.Enabled", SkinID, LocaleSetting);
				}
				AlertMessage.PushAlertMessage(message, AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
			catch (Exception ex)
			{
				throw new Exception(AppLogic.GetString("admin.giftcard.CannotUpdate", SkinID, LocaleSetting) + " " + ex.ToString());
			}
		}
	}
}
