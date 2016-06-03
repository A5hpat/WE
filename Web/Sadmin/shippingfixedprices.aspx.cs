// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontAdmin
{
	public partial class ShippingFixedPrices : AspDotNetStorefront.Admin.AdminPageBase
	{
		public int SelectedStoreId;
		bool FilterShipping = AppLogic.GlobalConfigBool("AllowShippingFiltering");

		protected void Page_Load(object sender, EventArgs e)
		{
			var queryStringStoreId = Request.QueryStringNativeInt("StoreId");
			SelectedStoreId = queryStringStoreId > 0 ? queryStringStoreId : Store.GetDefaultStore().StoreID;

			if (!IsPostBack)
			{
				if(FilterShipping)
					BindStores();

				BindShippingCalculationTable();
				StoreSelectorPanel.Visible = FilterShipping;
			}
		}

		void BindStores()
		{
			List<Store> storeList = Store.GetStoreList();
			StoreSelector.DataSource = storeList;
			StoreSelector.DataTextField = "Name";
			StoreSelector.DataValueField = "StoreID";
			StoreSelector.DataBind();

			StoreSelector.SelectedValue = SelectedStoreId.ToString();
		}

		protected void StoreSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedStoreId = 1;
			if (!int.TryParse(StoreSelector.SelectedValue, out selectedStoreId))
				return;

			Response.Redirect(String.Format("shippingfixedprices.aspx?StoreId={0}", selectedStoreId));
		}

		void BindShippingCalculationTable()
		{
			//We'll need shipping method shipping charge amounts to work with in building the data source
			using (DataTable gridData = new DataTable())
			{
				//Populate shipping methods data
				using (SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
				{
					sqlConnection.Open();

					string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @StoreId, @IsRTShipping = 0, @OnlyMapped = @FilterByStore";
					var getShippingMethodMappingParams = new[]
						{
							new SqlParameter("@StoreId", SelectedStoreId),
							new SqlParameter("@FilterByStore", FilterShipping),
						};

					using (IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
						gridData.Load(rs);
				}

				if (gridData.Rows.Count == 0)
				{
					AlertMessage.PushAlertMessage(String.Format("You do not have any shipping methods setup for the selected store. Please <a href=\"{0}\">click here</a> to set them up.", AppLogic.AdminLinkUrl("shippingmethods.aspx")), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
					ShippingRatePanel.Visible = false;
					return;
				}

				ShippingGrid.DataSource = gridData;
				ShippingGrid.DataBind();

			}
		}

		protected void Save_Click(object sender, EventArgs e)
		{
			foreach(GridViewRow row in ShippingGrid.Rows)
			{
				var rateBox = row.FindControl<TextBox>("Rate");
				if(rateBox == null)
					return;

				decimal rate = 0;
				if(!decimal.TryParse(rateBox.Text, out rate))
				{
					AlertMessage.PushAlertMessage("You have specified an invalid value for one of your shipping methods", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
					return;
				}

				var shippingMethodIdField = row.FindControl<HiddenField>("ShippingMethodID");
				if(shippingMethodIdField == null)
					return;

				var sql = @"Update ShippingMethod set FixedRate = @Rate where ShippingMethodID = @ShippingMethodId";
				var parameters = new [] {
					new SqlParameter("@Rate", String.IsNullOrEmpty(rateBox.Text) ? null : Localization.CurrencyStringForDBWithoutExchangeRate(rate)),
					new SqlParameter("@ShippingMethodId", shippingMethodIdField.Value)
				};
				DB.ExecuteSQL(sql, parameters);

			}
			AlertMessage.PushAlertMessage("Your rates were saved successfully.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			BindShippingCalculationTable();
		}
}

}
