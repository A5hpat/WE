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
	public partial class ShippingMethods : AspDotNetStorefront.Admin.AdminPageBase
	{

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
		}

		protected void ShippingMethodGrid_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
				((LinkButton)e.Row.FindControl("Delete")).Attributes.Add("onclick", "javascript: return confirm('Are you sure you want to delete this shipping method?')");
		}

		protected void ShippingMethodGrid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "DeleteItem")
			{
				DeleteShippingMethod(Localization.ParseNativeInt(e.CommandArgument.ToString()));
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
		}

		protected void UpdateItems()
		{
			try
			{
				foreach(GridViewRow row in ShippingMethodGrid.Rows)
				{
					var shippingMethodId = (Literal)row.FindControl("ShippingMethodId");
					if(shippingMethodId == null)
						return;

					var displayOrderBox = (TextBox)row.FindControl("DisplayOrder");
					if(displayOrderBox == null)
						return;

					int displayOrder = 1;

					if(!int.TryParse(displayOrderBox.Text, out displayOrder))
					{
						AlertMessage.PushAlertMessage("Please make sure you've entered a valid display order.", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
						return;
					}

					var sqlParams = new[] {
					new SqlParameter("@DisplayOrder", displayOrder),
					new SqlParameter("@ShippingMethodId", shippingMethodId.Text) };

					DB.ExecuteSQL("update ShippingMethod set DisplayOrder = @DisplayOrder where ShippingMethodID = @ShippingMethodId", sqlParams);
				}

				AlertMessage.PushAlertMessage("Your values were successfully saved.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
			catch(Exception exception)
			{
				AlertMessage.PushAlertMessage(exception.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}
		}

		protected void DeleteShippingMethod(int shippingMethodId)
		{
			var sqlParams = new[] {
				new SqlParameter("@ShippingMethodId", shippingMethodId),
			};

			DB.ExecuteSQL(@"delete from ShippingByTotal where ShippingMethodID = @ShippingMethodId
							delete from ShippingByWeight where ShippingMethodID = @ShippingMethodId
							delete from ShippingWeightByZone where ShippingMethodID = @ShippingMethodId
							delete from ShippingTotalByZone where ShippingMethodID = @ShippingMethodId
							delete from ShippingMethod where ShippingMethodID = @ShippingMethodId
							delete from ShippingMethodToStateMap where ShippingMethodID = @ShippingMethodId
							delete from ShippingMethodToCountryMap where ShippingMethodID = @ShippingMethodId
							delete from ShippingMethodToZoneMap where ShippingMethodID = @ShippingMethodId
							delete from ShippingMethodStore where ShippingMethodID = @ShippingMethodId
							update shoppingcart set ShippingMethodID=0, ShippingMethod=NULL where ShippingMethodID = @ShippingMethodId", sqlParams);
		}
	}
}
