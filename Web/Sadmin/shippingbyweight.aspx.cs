// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class ShippingByWeight : AspDotNetStorefront.Admin.AdminPageBase
	{
		public int SelectedStoreId;
		bool FilterShipping = AppLogic.GlobalConfigBool("AllowShippingFiltering");

		protected void Page_Load(object sender, EventArgs e)
		{
			var queryStringStoreId = Request.QueryStringNativeInt("StoreId");
			SelectedStoreId = queryStringStoreId > 0 ? queryStringStoreId : Store.GetDefaultStore().StoreID;

			if (!IsPostBack)
			{
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

			Response.Redirect(String.Format("shippingbyweight.aspx?StoreId={0}", selectedStoreId));
		}

		void BindShippingCalculationTable()
		{
			BindShippingCalculationTable(false);
		}

		void BindShippingCalculationTable(bool addInsertRow)
		{
			//clear the columns to prevent duplicates
			ShippingGrid.Columns.Clear();

			//We're going to assemble the datasource that we need by putting it together manually here.
			using (DataTable gridData = new DataTable())
			{
				//We'll need shipping method shipping charge amounts to work with in building the data source
				using (DataTable methodAmountsData = new DataTable())
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
							methodAmountsData.Load(rs);
					}

					if (methodAmountsData.Rows.Count == 0)
					{
						AlertMessage.PushAlertMessage(String.Format("You do not have any shipping methods setup for the selected store. Please <a href=\"{0}\">click here</a> to set them up.", AppLogic.AdminLinkUrl("shippingmethods.aspx")), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
						ShippingRatePanel.Visible = false;
						return;
					}

					using (DataTable shippingRangeData = new DataTable())
					{
						using (SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
						{
							//populate the shipping range data
							var sqlShipping = @"SELECT DISTINCT sw.RowGuid, sw.LowValue, sw.HighValue
									FROM ShippingByWeight sw WITH (NOLOCK)
									INNER JOIN ShippingMethod sm WITH (NOLOCK) ON sm.ShippingMethodid = sw.ShippingMethodId AND (@FilterByStore = 0 or @StoreId = sw.StoreID)
									ORDER BY LowValue";

							var shippingRangeParams = new[]
							{
								new SqlParameter("@StoreId", SelectedStoreId),
								new SqlParameter("@FilterByStore", FilterShipping),
							};

							sqlConnection.Open();
							using (IDataReader rs = DB.GetRS(sqlShipping, shippingRangeParams, sqlConnection))
								shippingRangeData.Load(rs);
						}

						//Add the data columns we'll need on our table and add grid columns to match
						gridData.Columns.Add(new DataColumn("RowGuid", typeof(string)));

						gridData.Columns.Add(new DataColumn("LowValue", typeof(string)));
						BoundField boundField = new BoundField();
						boundField.DataField = "LowValue";
						boundField.HeaderText = "Low";
						boundField.ControlStyle.CssClass = "text-xs";
						ShippingGrid.Columns.Add(boundField);

						gridData.Columns.Add(new DataColumn("HighValue", typeof(string)));
						boundField = new BoundField();
						boundField.DataField = "HighValue";
						boundField.HeaderText = "High";
						boundField.ControlStyle.CssClass = "text-xs";
						ShippingGrid.Columns.Add(boundField);

						//Add shipping method columns to our grid data
						foreach (DataRow methodAmountsRow in methodAmountsData.Rows)
						{
							var columnName = String.Format("MethodAmount_{0}", DB.RowField(methodAmountsRow, "ShippingMethodID"));
							gridData.Columns.Add(new DataColumn(columnName, typeof(string)));
							//add a column to the gridview to hold the data
							boundField = new BoundField();
							boundField.DataField = columnName;
							boundField.HeaderText = DB.RowFieldByLocale(methodAmountsRow, "Name", LocaleSetting);
							boundField.ControlStyle.CssClass = "text-xs";
							ShippingGrid.Columns.Add(boundField);
						}

						//now that our columns are setup add rows to our table
						foreach (DataRow rangeRow in shippingRangeData.Rows)
						{
							var newRow = gridData.NewRow();
							//add the range data
							newRow["RowGuid"] = rangeRow["RowGuid"];
							newRow["LowValue"] = rangeRow["LowValue"];
							newRow["HighValue"] = rangeRow["HighValue"];
							//add shipping method amounts to our grid data
							foreach (DataRow methodAmountsRow in methodAmountsData.Rows)
							{
								var shippingMethodId = DB.RowFieldInt(methodAmountsRow, "ShippingMethodID");
								var shippingRangeGuid = DB.RowFieldGUID(rangeRow, "RowGUID");
								var amount = Shipping.GetShipByWeightCharge(shippingMethodId, shippingRangeGuid);
								var localizedAmount = Localization.CurrencyStringForDBWithoutExchangeRate(amount);

								var colName = String.Format("MethodAmount_{0}", shippingMethodId);
								newRow[colName] = localizedAmount;
							}

							gridData.Rows.Add(newRow);
						}

						//if we're inserting, add an empty row to the end of the table
						if (addInsertRow)
						{
							var newRow = gridData.NewRow();
							//add the range data
							newRow["RowGuid"] = 0;
							newRow["LowValue"] = 0;
							newRow["HighValue"] = 0;
							//add shipping method columns to our insert row
							foreach (DataRow methodAmountsRow in methodAmountsData.Rows)
							{
								var shippingMethodId = DB.RowFieldInt(methodAmountsRow, "ShippingMethodID");
								var amount = 0;
								var localizedAmount = Localization.CurrencyStringForDBWithoutExchangeRate(amount);

								var colName = String.Format("MethodAmount_{0}", shippingMethodId);
								newRow[colName] = localizedAmount;
							}
							gridData.Rows.Add(newRow);
							//if we're inserting than we'll want to make the insert row editable
							ShippingGrid.EditIndex = gridData.Rows.Count - 1;
						}


						//add the delete button column
						ButtonField deleteField = new ButtonField();
						deleteField.ButtonType = ButtonType.Link;
						deleteField.Text = "<i class=\"fa fa-times\"></i> Delete";
						deleteField.CommandName = "Delete";
						deleteField.ControlStyle.CssClass = "delete-link";
						deleteField.ItemStyle.Width = 94;
						ShippingGrid.Columns.Add(deleteField);

						//add the edit button column
						CommandField commandField = new CommandField();
						commandField.ButtonType = ButtonType.Link;
						commandField.ShowEditButton = true;
						commandField.ShowDeleteButton = false;
						commandField.ShowCancelButton = true;
						commandField.ControlStyle.CssClass = "edit-link";
						commandField.EditText = "<i class=\"fa fa-share\"></i> Edit";
						commandField.CancelText = "<i class=\"fa fa-reply\"></i> Cancel";
						commandField.UpdateText = "<i class=\"fa fa-floppy-o\"></i> Save";
						commandField.ItemStyle.Width = 84;
						ShippingGrid.Columns.Add(commandField);

						ShippingGrid.DataSource = gridData;
						ShippingGrid.DataBind();

					}
				}
			}

			btnInsert.Visible = !addInsertRow;	//Hide the 'add new row' button while editing/inserting to avoid confusion and lost data
		}


		protected void btnInsert_Click(object sender, EventArgs e)
		{
			BindShippingCalculationTable(true);
		}

		protected void ShippingGrid_RowEditing(object sender, GridViewEditEventArgs e)
		{
			ShippingGrid.EditIndex = e.NewEditIndex;
			BindShippingCalculationTable();

			btnInsert.Visible = false;
		}

		protected void ShippingGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
		{
			GridViewRow row = ShippingGrid.Rows[e.RowIndex];

			//get the guid of the shipping row
			var guid = e.Keys[0].ToString();

			if (TryUpdateShippingRow(row, guid))
			{
				AlertMessage.PushAlertMessage("Your row was saved successfully", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
				//Reset the edit index.
				ShippingGrid.EditIndex = -1;
			}

			//Bind data to the GridView control.
			BindShippingCalculationTable();
		}

		bool TryUpdateShippingRow(GridViewRow row, string guid)
		{
			var newGuid = DB.GetNewGUID();
			//loop through the colums and update each shipping method with the appropriate value
			var indexCounter = 0;
			foreach (var column in ShippingGrid.Columns)
			{
				//skip past the low and high value columns and make sure we've got a bound field
				if (indexCounter > 1 && column.GetType() == typeof(BoundField))
				{
					var field = (BoundField)column;
					if (field.DataField.Contains("MethodAmount_"))
					{
						var methodId = field.DataField.Replace("MethodAmount_", String.Empty);
						decimal lowValue = 0;
						if (!decimal.TryParse(((TextBox)(row.Cells[0].Controls[0])).Text, out lowValue))
						{
							AlertMessage.PushAlertMessage("Your low value is not in the correct format.", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
							return false;
						}
						decimal highValue = 0;
						if (!decimal.TryParse(((TextBox)(row.Cells[1].Controls[0])).Text, out highValue))
						{
							AlertMessage.PushAlertMessage("Your high value is not in the correct format.", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
							return false;
						}
						decimal amount = 0;
						if (!decimal.TryParse(((TextBox)(row.Cells[indexCounter].Controls[0])).Text, out amount))
						{
							AlertMessage.PushAlertMessage("The amount you entered for one of your shipping methods is not in the correct format.", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
							return false;
						}
						if(lowValue >= highValue)
						{
							AlertMessage.PushAlertMessage("Please enter a valid range for your low and high values", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
							return false;
						}
						//passed validation. delete the old row and create a new one.
						if (guid != "0")
						{
							DB.ExecuteSQL("delete from ShippingByWeight where RowGUID = @Guid ", new[] { new SqlParameter("Guid", guid) });
						}

						var parameters = new[] {
							new SqlParameter("@ShippingGuid", newGuid),
							new SqlParameter("@LowAmount", lowValue),
							new SqlParameter("@HighAmount", highValue),
							new SqlParameter("@ShippingMethodId", methodId),
							new SqlParameter("@ShippingCharge", amount),
							new SqlParameter("@StoreId", SelectedStoreId),
						};

						var insertSql = @"insert into ShippingByWeight(RowGUID, LowValue, HighValue, ShippingMethodID, ShippingCharge, StoreID) 
										values(@ShippingGuid, @LowAmount, @HighAmount, @ShippingMethodId, @ShippingCharge, @StoreId)";

						DB.ExecuteSQL(insertSql, parameters);
					}
				}
				indexCounter++;
			}
			return true;
		}

		protected void ShippingGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			var guid = e.Keys[0];

			//cancel edit if this is a new insert row
			if (guid.ToString() == "0")
			{
				ShippingGrid.EditIndex = -1;
				BindShippingCalculationTable();
				return;
			}

			var parameters = new SqlParameter[] {
				new SqlParameter { Value= guid, ParameterName = "ShippingGuid" }
			};

			DB.ExecuteSQL("delete from ShippingByWeight where RowGUID = @ShippingGuid", parameters);
			BindShippingCalculationTable();
			AlertMessage.PushAlertMessage("The row was deleted successfully", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
		}

		protected void ShippingGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
		{
			ShippingGrid.EditIndex = -1;
			BindShippingCalculationTable();
		}

		protected void ShippingGrid_RowCreated(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.Header && ShippingGrid.Columns.Count > 4)
			{
				//Create a top level header row to help label the values with more context
				var topHeaderRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Insert);
				var orderCell = new TableHeaderCell();
				orderCell.Text = String.Format("Order Weight (in {0})", Localization.WeightUnits());
				orderCell.ColumnSpan = 2;
				topHeaderRow.Cells.Add(orderCell);

				var chargeByCell = new TableHeaderCell();
				chargeByCell.Text = "Shipping Charge By Weight";
				chargeByCell.ColumnSpan = ShippingGrid.Columns.Count - 4;
				topHeaderRow.Cells.Add(chargeByCell);

				//Add an empty header cell to span the command field and the delete field
				var emptyCell = new TableHeaderCell();
				emptyCell.ColumnSpan = 2;
				topHeaderRow.Cells.Add(emptyCell);

				ShippingGrid.Controls[0].Controls.AddAt(0, topHeaderRow);

			}
		}
	}

}
