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
	public partial class editextendedprices : AspDotNetStorefront.Admin.AdminPageBase
	{
		int VariantID;
		int ProductID;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			VariantID = CommonLogic.QueryStringUSInt("VariantID");
			ProductID = CommonLogic.QueryStringUSInt("ProductID");
			DisplayBreadcrumbText(VariantID, ProductID);
		}

		protected void DisplayBreadcrumbText(int vID, int pID)
		{
			lnkVariant.NavigateUrl = "variants.aspx?variantid=" + VariantID + "&productid=" + ProductID;
			lnkVariant.Text = "Variant " + AppLogic.GetVariantName(VariantID, Localization.GetDefaultLocale());
			lnkVariant.Text += " (Product " + AppLogic.GetProductName(ProductID, Localization.GetDefaultLocale()) + ")";
		}

		protected void btnUpdate_Click(object sender, EventArgs e)
		{
			if(UpdateItems())
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.Updated", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
			else
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.UpdateFailed", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Error);
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(UpdateItems())
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.Updated", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
			else
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.UpdateFailed", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Error);

			Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		protected bool UpdateItems()
		{
			if(Grid.Rows.Count == 0 || VariantID == 0)
				return false;

			// Delete before insert of first row
			DB.ExecuteSQL(
				"delete from extendedprice where VariantID = @variantId",
				new[] { new SqlParameter("variantId", (object)VariantID) });

			var updateSql = @"
				insert into ExtendedPrice(ExtendedPriceGUID, VariantID, CustomerLevelID, Price) 
				values(newid(), @variantID, @customerLevelId, @extendedPrice)";

			foreach(GridViewRow row in Grid.Rows)
			{
				var customerLevelId = (int)Grid.DataKeys[row.DataItemIndex].Values[0];
				var txtExtendedPrice = (TextBox)row.FindControl("txtExtendedPrice");

				decimal extendedPrice;
				if(txtExtendedPrice == null || !Decimal.TryParse(txtExtendedPrice.Text, out extendedPrice))
					continue;

				DB.ExecuteSQL(
					updateSql,
					new[]
						{
							new SqlParameter("variantId", (object)VariantID),
							new SqlParameter("customerLevelId", (object)customerLevelId),
							new SqlParameter("extendedPrice", (object)extendedPrice),
						});
			}
			return true;
		}
	}
}
