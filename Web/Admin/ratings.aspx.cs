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
	/// Summary description for ratings.
	/// </summary>
	public partial class ratings : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
		}

		protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == "DeleteItem")
			{
				gMain.EditIndex = -1;
				int ratingId = Localization.ParseNativeInt(e.CommandArgument.ToString());
				DeleteRating(ratingId);
			}
		}
		
		protected void gMain_RowDataBound(Object sender, GridViewRowEventArgs e)
		{
			if(e.Row.RowType == DataControlRowType.DataRow)
			{
				LinkButton lnkDelete = (LinkButton)e.Row.FindControl("lnkDelete");
				lnkDelete.Attributes.Add("onClick", "return confirm('" + AppLogic.GetString("admin.manageratings.confirmdelete", ThisCustomer.LocaleSetting) + "')");
			}
		}

		private void DeleteRating(int ratingId)
		{
			DB.ExecuteSQL(String.Format("aspdnsf_delProductRating {0}", ratingId));
		}
	}
}
