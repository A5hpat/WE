// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using System.Globalization;

namespace AspDotNetStorefrontAdmin
{
	public partial class localesettings : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected string selectSQL = "select * from LocaleSetting  with (NOLOCK) ";
		protected Customer cust;

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			cust = Context.GetCustomer();

			if(!IsPostBack)
			{
				deleteXX();

				ViewState["Sort"] = "displayorder,description";
				ViewState["SortOrder"] = "";
				ViewState["SQLString"] = selectSQL;

				ShowAddPanel(false);
				btnInsert.Visible = true;
			}
			this.Title = AppLogic.GetString("admin.title.localesetting", ThisCustomer.LocaleSetting);
			if(Currency.NumPublishedCurrencies() < 1)
				ctrlAlertMessage.PushAlertMessage("Notice: No published currencies detected.", AlertMessage.AlertType.Info);
		}

		protected void buildGridData()
		{
			var sql = ViewState["SQLString"].ToString();
			sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var rs = DB.GetRS(sql, dbconn))
				{
					using(var dt = new DataTable())
					{
						dt.Load(rs);
						gMain.DataSource = dt;
						gMain.DataBind();
					}
				}
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
			if((e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
			{
				DataRowView myrow = (DataRowView)e.Row.DataItem;
				int cID = Localization.ParseNativeInt(myrow["DefaultCurrencyID"].ToString());
				DropDownList dd = (DropDownList)e.Row.FindControl("ddCurrency");
				ArrayList list = Currency.getCurrencyList();
				foreach(ListItemClass li in list)
				{
					dd.Items.Add(new ListItem(li.Item, li.Value.ToString()));
					if(li.Value == cID)
					{
						dd.Items.FindByValue(cID.ToString()).Selected = true;
					}
				}
			}

			if(e.Row.RowType == DataControlRowType.DataRow)
			{
				LinkButton deleteButton = (LinkButton)e.Row.FindControl("Delete");
				deleteButton.Attributes.Add("onClick", "javascript: return confirm('Confirm Delete?')");

				if((e.Row.RowState == DataControlRowState.Normal) || (e.Row.RowState == DataControlRowState.Alternate))
				{					
					//load Currency
					DataRowView myrow = (DataRowView)e.Row.DataItem;
					int cID = Localization.ParseNativeInt(myrow["DefaultCurrencyID"].ToString());
					e.Row.Cells[3].Text = Currency.GetCurrencyCode(cID) + " (" + Currency.GetName(cID) + ")";
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
			if(e.CommandName == "DeleteItem")
			{
				gMain.EditIndex = -1;
				int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());
				deleteRowPerm(iden);
			}
		}
		protected void deleteRowPerm(int iden)
		{
			try
			{
				DB.ExecuteSQL("delete from LocaleSetting where LocaleSettingid=" + iden);
				AppLogic.UpdateNumLocaleSettingsInstalled();
				buildGridData();
				ctrlAlertMessage.PushAlertMessage("Item Deleted", AlertMessage.AlertType.Success);
				btnInsert.Visible = true;
			}
			catch(Exception ex)
			{
				ctrlAlertMessage.PushAlertMessage("Couldn't delete from database: " + ex.ToString(), AlertMessage.AlertType.Danger);
			}
		}
		protected void deleteXX()
		{
			try
			{
				DB.ExecuteSQL("delete from LocaleSetting where [Name] LIKE 'xx-XX%'");
				AppLogic.UpdateNumLocaleSettingsInstalled();
			}
			catch(Exception ex)
			{
				ctrlAlertMessage.PushAlertMessage("admin.common.ErrDeleteDB".StringResource() + " " + ex.ToString(), AlertMessage.AlertType.Danger);
			}
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

			if(row != null)
			{
				string iden = row.Cells[0].Text.ToString();
				string name = CommonLogic.Left(((TextBox)row.FindControl("txtName")).Text, 10).Replace(" ", "-");
				string description = CommonLogic.Left(((TextBox)row.FindControl("txtDescription")).Text, 100);
				string cID = ((DropDownList)row.FindControl("ddCurrency")).SelectedValue;
				string order = ((TextBox)row.FindControl("txtOrder")).Text;

				// see if this LocaleSetting already exists:
				int N = DB.GetSqlN("select count(name) as N from LocaleSetting   with (NOLOCK)  where LocaleSettingID<>" + iden + " and Name=" + DB.SQuote(name));
				if(N != 0)
				{
					ctrlAlertMessage.PushAlertMessage("admin.localesettings.ExistingLocale".StringResource(), AlertMessage.AlertType.Danger);
					return;
				}

				StringBuilder sql = new StringBuilder(2500);

				// ok to update:
				sql.Append("update LocaleSetting set ");
				sql.Append("Name=" + DB.SQuote(Localization.CheckLocaleSettingForProperCase(name)) + ",");
				sql.Append("Description=" + DB.SQuote(description) + ",");
				sql.Append("DefaultCurrencyID=" + cID + ",");
				sql.Append("DisplayOrder=" + order);
				sql.Append(" where LocaleSettingID=" + iden);

				ctrlAlertMessage.PushAlertMessage("admin.common.ItemUpdated".StringResource(), AlertMessage.AlertType.Success);

				try
				{
					DB.ExecuteSQL(sql.ToString());

					deleteXX();

					gMain.EditIndex = -1;
					ViewState["SQLString"] = selectSQL;
					buildGridData();
				}
				catch(Exception ex)
				{
					ctrlAlertMessage.PushAlertMessage("Couldn't save changes. Please make sure all fields are filled in. Check system log for details.", AlertMessage.AlertType.Danger);
					SysLog.LogMessage("Locale Setting Error", String.Format("admin.creditcards.CouldntUpdateDatabase".StringResource(), sql.ToString(), ex.ToString()), MessageTypeEnum.DatabaseException,  MessageSeverityEnum.Error);
				}
			}
		}
		protected void gMain_RowEditing(object sender, GridViewEditEventArgs e)
		{
			gMain.EditIndex = e.NewEditIndex;

			buildGridData();
		}

		protected void btnInsert_Click(object sender, EventArgs e)
		{
			ShowAddPanel(true);

			txtDescription.Text = "";
			txtName.Text = "";
			txtOrder.Text = "1";

			ddCurrency.Items.Clear();
			ArrayList list = Currency.getCurrencyList();
			ddCurrency.Items.Add(new ListItem("- Select -", "0"));

			foreach(ListItemClass li in list)
			{
				ddCurrency.Items.Add(new ListItem(li.Item, li.Value.ToString()));
			}
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			StringBuilder sql = new StringBuilder();

			string name = txtName.Text.Trim();
			string description = txtDescription.Text.Trim();
			string currency = ddCurrency.SelectedValue;
			string displayorder = txtOrder.Text.Trim();

			//Locale reality check
			if(!ValidateDisplayLocale(name))
				return;

			// ok to add them:
			String NewGUID = DB.GetNewGUID();
			sql.Append("insert into LocaleSetting(LocaleSettingGUID,Name,Description,DefaultCurrencyID,DisplayOrder) values(");
			sql.Append(DB.SQuote(NewGUID) + ",");
			sql.Append(DB.SQuote(name) + ",");
			sql.Append(DB.SQuote(description) + ",");
			sql.Append(currency + ",");
			sql.Append(DB.SQuote(displayorder));
			sql.Append(")");

			try
			{
				DB.ExecuteSQL(sql.ToString());
				ctrlAlertMessage.PushAlertMessage("admin.localesettings.LocaleAdded".StringResource(), AlertMessage.AlertType.Success);

				gMain.EditIndex = -1;

				ShowAddPanel(false);
			}
			catch
			{
				ctrlAlertMessage.PushAlertMessage("admin.localesettings.LocaleExists".StringResource(), AlertMessage.AlertType.Danger);
				ShowAddPanel(true);
			}
		}

		protected void btnCancel_Click(object sender, EventArgs e)
		{
			ShowAddPanel(false);
		}

		private bool ValidateDisplayLocale(string displayLocale)
		{
			try
			{
				CultureInfo validCulture = new CultureInfo(displayLocale);
				return true;
			}
			catch
			{
				ctrlAlertMessage.PushAlertMessage(String.Format("Invalid Display Locale: {0}", displayLocale), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return false;
			}
		}

		protected void ShowAddPanel(bool showAdd)
		{
			if(showAdd)
			{
				pnlAdd.Visible = true;
				pnlGrid.Visible = false;
			}
			else
			{
				pnlAdd.Visible = false;
				pnlGrid.Visible = true;

				buildGridData();
			}
		}
	}
}
