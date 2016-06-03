// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class ShippingPage : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected int SelectedStoreId;
		protected int SelectedShippingCalculationId;
		protected bool FilterShipping = AppLogic.GlobalConfigBool("AllowShippingFiltering");

		protected void Page_Load(object sender, EventArgs e)
		{
			var queryStringStoreId = Request.QueryStringNativeInt("StoreId");
			SelectedStoreId = queryStringStoreId > 0
				? queryStringStoreId
				: Store.GetDefaultStore().StoreID;

			if(IsPostBack)
				return;

			if(FilterShipping)
				BindStores();

			BindShippingCalculations(SelectedStoreId);
		}

		void BindShippingCalculations(int StoreId)
		{
			string query = string.Empty;
			query = "select sc.* from ShippingCalculation sc with(nolock) order by ShippingCalculationId";

			SelectedShippingCalculationId = EnsureStoreShippingCalculation(SelectedStoreId);

			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				using(IDataReader rs = DB.GetRS(query, connection))
				{
					var currentShippingCalculationId = (int)Shipping.GetActiveShippingCalculationID(StoreId);
					while(rs.Read())
					{
						var shippingCalculationId = rs.FieldInt("ShippingCalculationID");
						var isSelected = currentShippingCalculationId == shippingCalculationId;

						if(isSelected)
							SelectedShippingCalculationId = DB.RSFieldInt(rs, "ShippingCalculationID");

						switch(shippingCalculationId)
						{
							case (int)Shipping.ShippingCalculationEnum.CalculateShippingByWeight:
								CalculateShippingByWeightName.Text = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
								break;
							case (int)Shipping.ShippingCalculationEnum.CalculateShippingByTotal:
								CalculateShippingByTotalName.Text = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
								break;
							case (int)Shipping.ShippingCalculationEnum.UseFixedPrice:
								UseFixedPriceName.Text = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
								break;
							case (int)Shipping.ShippingCalculationEnum.AllOrdersHaveFreeShipping:
								AllOrdersHaveFreeShippingName.Text = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
								break;
							case (int)Shipping.ShippingCalculationEnum.UseFixedPercentageOfTotal:
								UseFixedPercentageOfTotalName.Text = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
								break;
							case (int)Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts:
								UseIndividualItemShippingCostsName.Text = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
								break;
							case (int)Shipping.ShippingCalculationEnum.UseRealTimeRates:
								UseRealTimeRatesName.Text = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
								break;
							case (int)Shipping.ShippingCalculationEnum.CalculateShippingByWeightAndZone:
								CalculateShippingByWeightAndZoneName.Text = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
								break;
							case (int)Shipping.ShippingCalculationEnum.CalculateShippingByTotalAndZone:
								CalculateShippingByTotalAndZoneName.Text = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
								break;
							case (int)Shipping.ShippingCalculationEnum.CalculateShippingByTotalByPercent:
								CalculateShippingByTotalByPercentName.Text = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
								break;
						}
					}
				}
			}

			Page.DataBind();
		}

		void BindStores()
		{
			var storeList = Store.GetStoreList();

			StoreSelector.DataSource = storeList;
			StoreSelector.DataTextField = "Name";
			StoreSelector.DataValueField = "StoreID";
			StoreSelector.DataBind();

			StoreSelector.SelectedValue = SelectedStoreId.ToString();
		}

		protected void StoreSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			int selectedStoreId;
			if(!int.TryParse(StoreSelector.SelectedValue, out selectedStoreId))
				return;

			Response.Redirect(string.Format("shipping.aspx?StoreId={0}", selectedStoreId));
		}

		int EnsureStoreShippingCalculation(int storeId)
		{
			// Check if we have an entry in the mapping table for this store calculation
			if(!StoreHasShippingCalculationMap(storeId))
				DB.ExecuteSQL(
					"insert into ShippingCalculationStore(StoreId, ShippingCalculationId) Values(@storeId, @shippingCalculationId)",
					new SqlParameter("@storeId", storeId),
					new SqlParameter("@shippingCalculationId", AppLogic.AppConfigUSInt("DefaultShippingCalculationID")));

			return DB.GetSqlN(
				"select ShippingCalculationId as N from ShippingCalculationStore where StoreID = @storeId",
				new SqlParameter("@storeId", storeId));
		}

		bool StoreHasShippingCalculationMap(int storeId)
		{
			return DB.GetSqlN(
				"select count(*) as N from ShippingCalculationStore where StoreID = @storeId",
				new SqlParameter("@storeId", storeId)) > 0;
		}

		void UpdateStoreCalculation(int shippingCalculationId, int storeId)
		{
			DB.ExecuteSQL("update ShippingCalculation set Selected = 0");
			DB.ExecuteSQL(
				"update ShippingCalculationStore set ShippingCalculationID = @shippingCalculationId where StoreId = @storeId",
				new SqlParameter("@shippingCalculationId", shippingCalculationId),
				new SqlParameter("@storeId", storeId));
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			var newSelectedShippingCalculationId = int.Parse(hdnSelectedShippingCalculationId.Value);
			EnsureStoreShippingCalculation(SelectedStoreId);
			UpdateStoreCalculation(newSelectedShippingCalculationId, SelectedStoreId);
			BindShippingCalculations(SelectedStoreId);

			AlertMessage.PushAlertMessage("Shipping calculation method updated.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			Response.Redirect(AppLogic.AdminLinkUrl("shipping.aspx"));
		}
	}
}
