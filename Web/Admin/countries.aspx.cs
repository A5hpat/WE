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
	public partial class countries : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected const string DeleteCountryCommand = "countries:delete";

		protected void HandleCommands(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == DeleteCountryCommand)
			{
				var countryId = Localization.ParseNativeInt(e.CommandArgument.ToString());
				DeleteCountry(countryId);
			}
		}

		protected void btnUpdate_Click(object sender, EventArgs e)
		{
			Page.Validate("DisplayOrder");
			if(Page.IsValid)
			{
				UpdateItems();
				AlertMessage.PushAlertMessage("Your values were successfully saved.", AlertMessage.AlertType.Success);
			}
			else
			{
				AlertMessage.PushAlertMessage("Make sure you've specified a display order", AlertMessage.AlertType.Warning);
			}
		}

		void DeleteCountry(int countryId)
		{
			AppLogic.CountryTaxRatesTable.Remove(countryId);
			DB.ExecuteSQL(
				"delete from Country where CountryID = @countryId",
				new[] { new SqlParameter("countryId", (object)countryId) });

			AlertMessage.PushAlertMessage("Item Deleted", AlertMessage.AlertType.Success);
		}

		protected void UpdateItems()
		{
			foreach(GridViewRow row in CountryGrid.Rows)
			{
				var countryId = CountryGrid.DataKeys[row.DataItemIndex].Value;

				int? displayOrder = null;
				int parsedDisplayOrder;
				var ctlDisplayOrder = (TextBox)row.FindControl("DisplayOrder");
				if(ctlDisplayOrder != null && Int32.TryParse(ctlDisplayOrder.Text, out parsedDisplayOrder))
					displayOrder = parsedDisplayOrder;

				bool? published = null;
				var cbPublished = (CheckBox)row.FindControl("cbPublished");
				if(cbPublished != null)
					published = cbPublished.Checked;

				bool? postalCodeRequired = null;
				var cbPostalCodeRequired = (CheckBox)row.FindControl("cbPostalCodeRequired");
				if(cbPostalCodeRequired != null)
					postalCodeRequired = cbPostalCodeRequired.Checked;

				var sql = @"
					update
						Country 
					set 
						DisplayOrder = isnull(@displayOrder, DisplayOrder),
						Published = isnull(@published, Published),
						PostalCodeRequired = isnull(@postalCodeRequired, PostalCodeRequired)
					where 
						CountryID = @countryId";

				var parameters = new[]
					{
						new SqlParameter("displayOrder", (object)displayOrder),
						new SqlParameter("published", (object)published),
						new SqlParameter("postalCodeRequired", (object)postalCodeRequired),
						new SqlParameter("countryId", (object)countryId),
					};

				DB.ExecuteSQL(sql, parameters);
			}
		}
	}
}
