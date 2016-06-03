// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontAdmin
{

	public partial class taxClass : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected string selectSQL = "select * from TaxClass";
		protected Customer cust;

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			cust = Context.GetCustomer();

			if (!IsPostBack)
			{
				loadScript(false);
				ViewState["Sort"] = "Name";
				ViewState["SortOrder"] = "ASC";
				ViewState["SQLString"] = selectSQL;

				ShowAddPanel(false);
			}
		}

		protected void loadScript(bool load)
		{
			if (load)
			{
				if (AppLogic.NumLocaleSettingsInstalled() > 1)
				{
					ltScript.Text += "<script type='text/javascript' src='Scripts/tabs.js'></script>";
				}
			}
			else
			{
				ltScript.Text = "";
			}
		}

		protected void buildGridData()
		{
			string sql = ViewState["SQLString"].ToString();
			sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

			using (DataTable dt = new DataTable())
			{
				using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
				{
					conn.Open();
					using (IDataReader rs = DB.GetRS(sql, conn))
					{
						dt.Load(rs);

						if (dt.Rows.Count > 0)
						{
							dt.Columns.Add("EditName");
							foreach (DataRow dr in dt.Rows)
							{
								dr["EditName"] = AppLogic.GetLocaleEntryFields(dr[2].ToString(), "Name", false, true, true, "Please enter the " + AppLogic.GetString("AppConfig.CategoryPromptSingular", 0, cust.LocaleSetting).ToLowerInvariant() + " name", 100, 25, 0, 0, false);
							}
						}
					}
				}

				gMain.DataSource = dt;
				gMain.DataBind();
			}
		}

		protected void gMain_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
		{
			ViewState["SQLString"] = selectSQL;

			gMain.EditIndex = -1;
			buildGridData();
		}
		protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				LinkButton lnkDelete = (LinkButton)e.Row.FindControl("lnkDelete");
				lnkDelete.Attributes.Add("onClick", "javascript: return confirm('Confirm Delete? This will delete all TaxRates for this Tax Class also!')");

				//Localization
				if ((e.Row.RowState == DataControlRowState.Normal) || (e.Row.RowState == DataControlRowState.Alternate))
				{
					e.Row.Cells[1].Text = XmlCommon.GetLocaleEntry(((Literal)e.Row.FindControl("ltName")).Text, cust.LocaleSetting, false);
				}
			}
		}
		protected void gMain_Sorting(object sender, GridViewSortEventArgs e)
		{
			gMain.EditIndex = -1;
			ViewState["Sort"] = e.SortExpression.ToString();
			ViewState["SortOrder"] = (ViewState["SortOrder"].ToString() == "ASC" ? "DESC" : "ASC");
			buildGridData();
		}
		protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "DeleteItem")
			{
				gMain.EditIndex = -1;
				int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());
				deleteRow(iden);
			}
		}

		protected void deleteRow(int iden)
		{
			DB.ExecuteSQL("DELETE FROM StateTaxRate WHERE TaxClassID=" + iden);
			DB.ExecuteSQL("DELETE FROM CountryTaxRate WHERE TaxClassID=" + iden);
			DB.ExecuteSQL("DELETE FROM ZipTaxRate WHERE TaxClassID=" + iden);
			DB.ExecuteSQL("DELETE FROM TaxClass where TaxClassID=" + iden.ToString());

			buildGridData();
			
			AlertMessage.PushAlertMessage("Item Deleted", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
		}

		protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			gMain.PageIndex = e.NewPageIndex;
			gMain.EditIndex = -1;
			buildGridData();
		}
		protected void gMain_RowUpdating(object sender, GridViewUpdateEventArgs e)
		{
			GridViewRow row = gMain.Rows[e.RowIndex];

			if (row != null)
			{
				string iden = row.Cells[0].Text.ToString();
				string name = AppLogic.FormLocaleXml("Name");

				// see if this appconfig already exists:
				String countSql = "SELECT COUNT(Name) AS N FROM TaxCLASS WITH (NOLOCK) WHERE TaxClassID<>@CountTaxClassID AND LOWER(Name)=@CountName";
				SqlParameter[] countSpa = { new SqlParameter("@CountTaxClassID", iden),
                                              new SqlParameter("@CountName", name) };

				int N = DB.GetSqlN(countSql, countSpa);
				if (N != 0)
				{
					AlertMessage.PushAlertMessage("There is already another tax class with that name.", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
					return;
				}

				String updateSql = "UPDATE TaxClass SET Name=@UpdateName, TaxCode=@TaxCode, DisplayOrder=@DisplayOrder WHERE TaxClassID=@UpdateTaxClassID";

				SqlParameter[] updateSpa = { new SqlParameter("@UpdateName", name),
                                         new SqlParameter("@TaxCode", ((TextBox)row.FindControl("txtTaxCode")).Text),
                                         new SqlParameter("@DisplayOrder", ((TextBox)row.FindControl("txtDisplayOrder")).Text),
                                         new SqlParameter("@UpdateTaxClassID", iden) };

				try
				{
					DB.ExecuteSQL(updateSql, updateSpa);
					AlertMessage.PushAlertMessage("Item updated", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);

					gMain.EditIndex = -1;
					ViewState["SQLString"] = selectSQL;
					buildGridData();
				}
				catch (Exception ex)
				{
					throw new Exception("Couldn't update database: " + updateSql.ToString() + ex.ToString());
				}
			}
		}
		protected void gMain_RowEditing(object sender, GridViewEditEventArgs e)
		{
			gMain.EditIndex = e.NewEditIndex;

			loadScript(true);

			buildGridData();
		}
		protected void btnInsert_Click(object sender, EventArgs e)
		{
			gMain.EditIndex = -1;
			ShowAddPanel(true);

			txtTaxCode.Text = "";
			ltTaxClass.Text = AppLogic.GetLocaleEntryFields("", "Name", false, true, true, "Please enter the Tax Class", 100, 25, 0, 0, false);
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			String sql = "INSERT INTO TaxClass(TaxClassGUID, Name, TaxCode, DisplayOrder) VALUES(@Guid, @Name, @TaxCode, @DisplayOrder)";

			if (Page.IsValid)
			{
				if (ValidInput())
				{
					// ok to add them:
					string tax = txtTaxCode.Text.Trim();
					string displayOrder = txtDisplayOrder.Text.Trim();

					SqlParameter[] spa = { new SqlParameter("@Guid", DB.GetNewGUID()),
                                             new SqlParameter("@Name", AppLogic.FormLocaleXml("Name")),
                                             new SqlParameter("@TaxCode", txtTaxCode.Text.Trim()),
                                             new SqlParameter("@DisplayOrder", txtDisplayOrder.Text.Trim()) };

					try
					{
						DB.ExecuteSQL(sql, spa);
						AlertMessage.PushAlertMessage(AppLogic.GetString("admin.taxclass.added", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);

						ShowAddPanel(false);
					}
					catch
					{
						AlertMessage.PushAlertMessage(AppLogic.GetString("admin.taxclass.adderror", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);

						ShowAddPanel(true);
					}
				}
				else
				{
					AlertMessage.PushAlertMessage(AppLogic.GetString("admin.taxclass.requirederror", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);

				}
			}
		}

		protected void btnCancel_Click(object sender, EventArgs e)
		{
			ShowAddPanel(false);
		}

		protected bool ValidInput()
		{
			if (AppLogic.FormLocaleXml("Name").Equals("<ml></ml>"))
			{
				return false;
			}

			return true;
		}

		protected void ShowAddPanel(bool showAdd)
		{
			if (showAdd)
			{
				loadScript(true);
				pnlAdd.Visible = true;
				pnlGrid.Visible = false;
			}
			else
			{
				loadScript(false);
				pnlAdd.Visible = false;
				pnlGrid.Visible = true;

				buildGridData();
			}
		}
	}
}
