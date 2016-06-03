// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class states : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected override void OnInit(EventArgs e)
		{
			FilteredListing.SqlQuery = "select {0} state.*, country.Name as Country from State state with (NOLOCK) left outer join Country country with (NOLOCK) on state.CountryID = country.CountryID where {1}";
			base.OnInit(e);
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
		}

		protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
				((LinkButton)e.Row.FindControl("Delete")).Attributes.Add("onclick", "javascript: return confirm('Confirm Delete?')");
		}

		protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "DeleteItem")
			{
				DeleteState(Localization.ParseNativeInt(e.CommandArgument.ToString()));
				AlertMessage.PushAlertMessage("Item Deleted", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
				FilteredListing.Rebind();
			}
		}

		protected void btnUpdate_Click(object sender, EventArgs e)
		{
			Page.Validate("DisplayOrder");
			if (!Page.IsValid)
			{
				AlertMessage.PushAlertMessage("Make sure you've specified a display order", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return;
			}

			UpdateItems();
			AlertMessage.PushAlertMessage("Your values were successfully saved.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
		}

		protected void UpdateItems()
		{
			foreach (GridViewRow row in gMain.Rows)
			{
				var stateId = (Literal)row.FindControl("StateID");
				if (stateId == null)
					return;

				var displayOrder = (TextBox)row.FindControl("DisplayOrder");
				if (displayOrder == null)
					return;

				DB.ExecuteSQL("update State set DisplayOrder=" + displayOrder.Text.ToString() + " where StateID=" + stateId.Text.ToString());

				var cbPublished = (CheckBox)row.FindControl("cbPublished");
				if (cbPublished == null)
					return;

				var published = cbPublished.Checked ? "1" : "0";
				DB.ExecuteSQL("update State set Published=" + published + " where StateID=" + stateId.Text.ToString());
			}
		}

		protected void DeleteState(int stateId)
		{
			AppLogic.StateTaxRatesTable.Remove(stateId);
			DB.ExecuteSQL("delete from State where StateID=" + stateId.ToString());
		}

		protected bool publishedCheck(object PublishedValue)
		{
			return PublishedValue.ToString().Equals("1");
		}
	}
}
