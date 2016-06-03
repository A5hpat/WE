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
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class quantitydiscount : AspDotNetStorefront.Admin.AdminPageBase
	{
		int DiscountId = 0;
		int DiscountType = 0;
		
		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			DiscountId = CommonLogic.QueryStringNativeInt("discountid");

			if(DiscountId != 0)
			{
				Editing = true;
			}

			if(!Page.IsPostBack)
			{
				PopulateForm(Editing);
			}

			ToggleNewRowHeader();
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			btnClose.DataBind();
			btnCloseTop.DataBind();
		}

		private void ToggleNewRowHeader()
		{
			//Show the new row header only if there aren't any existing rows
			int numRows = DB.GetSqlN(String.Format("SELECT COUNT(*) N FROM QuantityDiscountTable WHERE QuantityDiscountID = {0}", DiscountId));
			if(numRows == 0)
				tblNewRowHeader.Visible = true;
		}

		private void PopulateForm(bool editing)
		{
			if(!editing)
			{
				litHeader.Text = AppLogic.GetString("admin.editquantitydiscount.AddNewQuantityDiscount", ThisCustomer.LocaleSetting);
			}
			else
			{
				litHeader.Text = AppLogic.GetString("admin.editquantitydiscount.EditingQuantityDiscounts", ThisCustomer.LocaleSetting);
				tblNewRow.Visible = true;

				string sql = "SELECT * FROM QuantityDiscount WITH (NOLOCK) WHERE QuantityDiscountID = @QuantityDiscountID";
				List<SqlParameter> sqlParams = new List<SqlParameter> { new SqlParameter("@QuantityDiscountID", DiscountId.ToString()) };

				using(SqlConnection dbconn = DB.dbConn())
				{
					dbconn.Open();
					using(IDataReader rs = DB.GetRS(sql, sqlParams.ToArray(), dbconn))
					{
						if(rs.Read())
						{
							litDiscountId.Text = DB.RSFieldInt(rs, "QuantityDiscountID").ToString();
							txtDiscountName.Text = DB.RSField(rs, "Name");
							txtDisplayOrder.Text = DB.RSFieldInt(rs, "DisplayOrder").ToString();
							DiscountType = DB.RSFieldTinyInt(rs, "DiscountType");
							ddlDiscountType.SelectedValue = DiscountType.ToString();
						}
					}
				}

				BindGrid();
			}

		}

		private void BindGrid()
		{
			divDiscountTable.Visible = true;

			using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
			{
				string sql = String.Format("SELECT QuantityDiscountTableID, LowQuantity, HighQuantity, DiscountPercent FROM QuantityDiscountTable WHERE QuantityDiscountID = {0} ORDER BY QuantityDiscountTableID", DiscountId);

				dbconn.Open();
				using(IDataReader rs = DB.GetRS(sql, dbconn))
				{
					using(DataTable dt = new DataTable())
					{
						dt.Load(rs);
						grdDiscountRows.DataSource = dt;
						grdDiscountRows.DataBind();
					}
				}
			}
		}

		protected void grdDiscountRows_OnRowDataBound(object sender, GridViewRowEventArgs e)
		{
			if(e.Row.RowType == DataControlRowType.DataRow)
			{
				LinkButton lnkDelete = e.Row.FindControl("lnkDelete") as LinkButton;
				lnkDelete.Attributes.Add("onClick", "javascript: return confirm('Are you sure you want to delete this row?')");
			}

			if(e.Row.RowType == DataControlRowType.Header)
			{
				Label lblDiscountTypeLabel = e.Row.FindControl("lblDiscountTypeLabel") as Label;

				if(lblDiscountTypeLabel != null)
				{
					lblDiscountTypeLabel.Text = DiscountType == 0 ? "Percentage Discount" : "Fixed Discount";
				}
			}
		}

		protected void grdDiscountRows_OnPageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			grdDiscountRows.PageIndex = e.NewPageIndex;
			BindGrid();
		}

		protected void grdDiscountRows_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == "DeleteItem")
			{
				int quantityDiscountTableID = Localization.ParseNativeInt(e.CommandArgument.ToString());
				
				if (quantityDiscountTableID != 0)
				{
					try
					{
						DB.ExecuteSQL(String.Format("DELETE FROM QuantityDiscountTable WHERE QuantityDiscountTableID = {0}", quantityDiscountTableID));
						ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.Deleted", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
					}
					catch (Exception ex)
					{
						ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
					}
				}
			}
			BindGrid();
			ToggleNewRowHeader();	//The last row may have been deleted, in which case we need to show the header again
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(SaveDiscount())
				Response.Redirect(String.Format("quantitydiscount.aspx?discountid={0}", DiscountId));
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(SaveDiscount())
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		private bool SaveDiscount()
		{
			bool saved = true;
			string discountSql = String.Empty;
			string discountName = txtDiscountName.Text.Trim();
			int discountType = 0;
			int displayOrder = 1;

			int.TryParse(txtDisplayOrder.Text.Trim(), out displayOrder);
			int.TryParse(ddlDiscountType.SelectedValue, out discountType);

			List<SqlParameter> discountParams = new List<SqlParameter> { new SqlParameter("@Name", discountName),
																	new SqlParameter("@DisplayOrder", displayOrder),
																	new SqlParameter("@DiscountType", discountType) };

			if(Editing)
			{
				int discountId = int.Parse(litDiscountId.Text);

				discountParams.Add(new SqlParameter("@QuantityDiscountID", discountId));

				discountSql = "UPDATE QuantityDiscount SET Name = @Name, DisplayOrder = @DisplayOrder, DiscountType = @DiscountType WHERE QuantityDiscountID = @QuantityDiscountID";
			}
			else
			{
				discountSql = "INSERT INTO QuantityDiscount (Name, DisplayOrder, DiscountType) VALUES (@Name, @DisplayOrder, @DiscountType)";
			}

			try
			{
				DB.ExecuteSQL(discountSql, discountParams.ToArray());
				ctlAlertMessage.PushAlertMessage("admin.orderdetails.UpdateSuccessful".StringResource(), AlertMessage.AlertType.Success);
			}
			catch(Exception ex)
			{
				ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
				saved = false;
			}

			if(!Editing)
			{
				//Added a new table - get the ID so we can reload the form & save rows
				DiscountId = DB.GetSqlN("SELECT TOP 1 QuantityDiscountID N FROM QuantityDiscount ORDER BY QuantityDiscountID DESC");

				////Add an initial row for the discount table
				//string discountTableSql = "INSERT INTO QuantityDiscountTable (QuantityDiscountTableGUID, QuantityDiscountID, LowQuantity, HighQuantity, DiscountPercent, CreatedOn) VALUES (newID(), @QuantityDiscountID, 0, 0, 0, getDate())";
				//List<SqlParameter> discountTableParams = new List<SqlParameter> { new SqlParameter("@QuantityDiscountID", DiscountId) };

				//DB.ExecuteSQL(discountTableSql, discountTableParams.ToArray());
			}

			if(Editing)
			{
				//This quantity discount already exists, so it can have rows to add/save
				saved = SaveRows();
			}

			return saved;
		}

		private bool SaveRows()
		{
			bool saved = false;

			if (Page.IsValid)
			{
				string sql = string.Empty;

				foreach (GridViewRow row in grdDiscountRows.Rows)
				{
					//Updating
					if(row.RowType == DataControlRowType.DataRow)
					{
						try
						{
							List<SqlParameter> discountRowParams = new List<SqlParameter>();

							int quantityDiscountTableID = 0;
							int lowQuantity = 0;
							int highQuantity = 0;
							decimal discountPercent = 0.0M;
							sql = "UPDATE QuantityDiscountTable SET LowQuantity = @LowQuantity, HighQuantity = @HighQuantity, DiscountPercent = @DiscountPercent WHERE QuantityDiscountTableID = @QuantityDiscountTableID";

							Literal litQuantityDiscountTableID = row.FindControl("litQuantityDiscountTableID") as Literal;
							TextBox txtLowQuantity = row.FindControl("txtLowQuantity") as TextBox;
							TextBox txtHighQuantity = row.FindControl("txtHighQuantity") as TextBox;
							TextBox txtDiscountPercent = row.FindControl("txtDiscountPercent") as TextBox;

							if(litQuantityDiscountTableID != null)
								int.TryParse(litQuantityDiscountTableID.Text, out quantityDiscountTableID);

							if(txtLowQuantity != null)
								int.TryParse(txtLowQuantity.Text.Trim(), out lowQuantity);

							if(txtHighQuantity != null)
								int.TryParse(txtHighQuantity.Text.Trim(), out highQuantity);

							if(txtDiscountPercent != null)
								decimal.TryParse(txtDiscountPercent.Text.Trim(), out discountPercent);

							discountRowParams.Add(new SqlParameter("@QuantityDiscountTableID", quantityDiscountTableID));
							discountRowParams.Add(new SqlParameter("@LowQuantity", lowQuantity));
							discountRowParams.Add(new SqlParameter("@HighQuantity", highQuantity));
							discountRowParams.Add(new SqlParameter("@DiscountPercent", discountPercent));

							DB.ExecuteSQL(sql, discountRowParams.ToArray());
							saved = true;
						}
						catch(Exception ex)
						{
							ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
							saved = false;
							return saved;	//Bail early so the error doesn't get swallowed by the next update
						}
					}
				}

				//Now add
				try
				{
					List<SqlParameter> discountRowParams = new List<SqlParameter>();

					int lowQuantity = 0;
					int highQuantity = 0;
					decimal discountPercent = 0.0M;
					sql = "INSERT INTO QuantityDiscountTable (QuantityDiscountTableGUID, QuantityDiscountID, LowQuantity, HighQuantity, DiscountPercent, CreatedOn) VALUES (newID(), @QuantityDiscountID, @LowQuantity, @HighQuantity, @DiscountPercent, getDate())";

					//Don't save blank rows
					if(!String.IsNullOrEmpty(txtNewLowQuantity.Text)
						&& !String.IsNullOrEmpty(txtNewHighQuantity.Text)
						&& !String.IsNullOrEmpty(txtNewDiscountPercent.Text))
					{
						int.TryParse(txtNewLowQuantity.Text.Trim(), out lowQuantity);
						int.TryParse(txtNewHighQuantity.Text.Trim(), out highQuantity);
						decimal.TryParse(txtNewDiscountPercent.Text.Trim(), out discountPercent);

						discountRowParams.Add(new SqlParameter("@QuantityDiscountID", DiscountId));
						discountRowParams.Add(new SqlParameter("@LowQuantity", lowQuantity));
						discountRowParams.Add(new SqlParameter("@HighQuantity", highQuantity));
						discountRowParams.Add(new SqlParameter("@DiscountPercent", discountPercent));

						DB.ExecuteSQL(sql, discountRowParams.ToArray());
						saved = true;
					}
				}
				catch(Exception ex)
				{
					ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
					saved = false;
				}
			}

			return saved;
		}
	}
}
