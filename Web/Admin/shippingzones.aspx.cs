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
	public partial class ShippingZones : AspDotNetStorefront.Admin.AdminPageBase
	{
		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
		}

		protected void ShippingZoneGrid_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if(e.Row.RowType == DataControlRowType.DataRow)
			{
				LinkButton Delete = (LinkButton)e.Row.FindControl("Delete");
				Delete.Attributes.Add("onClick", "javascript: return confirm('Are you sure you want to delete this shipping zone?')");
			}
		}

		/// <summary>
		/// Row Command
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ShippingZoneGrid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == "DeleteItem")
			{
				if(DeleteZone(Localization.ParseNativeInt(e.CommandArgument.ToString())))			
				{
					FilteredListing.Rebind();
				}				
			}
		}

		/// <summary>
		/// Delete zone
		/// </summary>
		/// <param name="zoneId"></param>
		protected bool DeleteZone(int zoneId)
		{
			bool IsDeleted = false;
			if(zoneId > 0)
			{
				try
				{
					// delete the Zone:
					DB.ExecuteSQL(String.Format("DELETE FROM ShippingWeightByZone WHERE ShippingZoneID={0}", zoneId));
					DB.ExecuteSQL(String.Format("DELETE FROM ShippingTotalByZone WHERE ShippingZoneID={0}", zoneId));
					DB.ExecuteSQL(String.Format("DELETE FROM ShippingZone WHERE ShippingZoneID={0}", zoneId));
					ctlAlertMessage.PushAlertMessage("Zone Deleted", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
					IsDeleted =  true;
				}
				catch
				{
					ctlAlertMessage.PushAlertMessage("Zone Did Not Delete, Please Try Again", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
				}				
			}
			else
			{
				ctlAlertMessage.PushAlertMessage("Zone Did Not Delete, Please Try Again", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
			return IsDeleted;
		}
	}
}
