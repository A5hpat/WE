// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Text;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for creditcards
	/// </summary>
	public partial class creditcards : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected string selectSQL = "select * from CreditCardType";

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			if(!IsPostBack)
			{
				ViewState["Sort"] = "CardType";
				ViewState["SortOrder"] = "ASC";
				ViewState["SQLString"] = selectSQL;

				ShowAddPanel(false);
			}
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
			if(e.Row.RowType == DataControlRowType.DataRow)
			{
				LinkButton deleteButton = (LinkButton)e.Row.FindControl("Delete");
				deleteButton.Attributes.Add("onClick", "javascript: return confirm('Confirm Delete?')");
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
				deleteRow(iden);
			}
		}
		protected void deleteRow(int iden)
		{
			StringBuilder sql = new StringBuilder(2500);
			sql.Append("delete from creditcardtype where CardTypeID=" + iden.ToString());
			try
			{
				DB.ExecuteSQL(sql.ToString());
				buildGridData();
				ctrlAlertMessage.PushAlertMessage("Item Deleted", AlertMessage.AlertType.Success);
			}
			catch(Exception ex)
			{
				throw new Exception(String.Format(AppLogic.GetString("admin.common.ErrUpdateDB", SkinID, LocaleSetting), ex.ToString()));
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
				string name = ((TextBox)row.FindControl("txtName")).Text;

				StringBuilder sql = new StringBuilder(2500);

				sql.Append("update CreditCardType set ");
				sql.Append("CardType=" + DB.SQuote(name));
				sql.Append(" where CardTypeID=" + iden);

				try
				{
					DB.ExecuteSQL(sql.ToString());
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.ItemUpdated", SkinID, LocaleSetting), AlertMessage.AlertType.Success);
					gMain.EditIndex = -1;
					ViewState["SQLString"] = selectSQL;
					buildGridData();
				}
				catch(Exception ex)
				{
					throw new Exception(String.Format(AppLogic.GetString("admin.creditcards.CouldntUpdateDatabase", SkinID, LocaleSetting), sql.ToString(), ex.ToString()));
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

			txtName.Text = "";
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(SaveCreditCardType())
			{
				gMain.EditIndex = -1;
				ShowAddPanel(false);
			}
			else
			{
				ShowAddPanel(true);
			}
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if (SaveCreditCardType())
			{
				gMain.EditIndex = -1;
				ShowAddPanel(false);
			}
			else
			{
				ShowAddPanel(true);
			}
		}

		private bool SaveCreditCardType()
		{
			bool saved = true;

			StringBuilder sql = new StringBuilder();

			string name = txtName.Text.Trim();

			// ok to add them:
			String NewGUID = DB.GetNewGUID();
			sql.Append("insert into CreditCardType(CardTypeGUID,CardType) values(");
			sql.Append(DB.SQuote(NewGUID) + ",");
			sql.Append(DB.SQuote(name));
			sql.Append(")");

			try
			{
				DB.ExecuteSQL(sql.ToString());
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.creditcards.CreditCardAdded", SkinID, LocaleSetting), AlertMessage.AlertType.Success);
			}
			catch (Exception ex)
			{
				ctrlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
				saved = false;
			}

			return saved;
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
