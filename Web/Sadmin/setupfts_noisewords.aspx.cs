// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class Admin_setupFTS_NoiseWords : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected string selectSQL = "select * from NoiseWords";
		protected Customer cust;
		private int m_SkinID = 1;

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			cust = Context.GetCustomer();

			if(!IsPostBack)
			{
				ViewState["Sort"] = "word";
				ViewState["SortOrder"] = "ASC";
				ViewState["SQLString"] = selectSQL;

				BuildGridData();
			}
		}

		protected void BuildGridData()
		{
			selectSQL += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

			try
			{
				using(DataTable dt = new DataTable())
				{
					dt.Columns.Add("EditWord");

					using(SqlConnection con = DB.dbConn())
					{
						con.Open();
						using(IDataReader rs = DB.GetRS(selectSQL, con))
						{
							dt.Load(rs);
						}
					}

					if(dt.Rows.Count > 0)
					{
						foreach(DataRow dr in dt.Rows)
						{
							dr["EditWord"] = dr["word"];
						}
					}

					gMain.DataSource = dt;
					gMain.DataBind();
				}

			}
			catch(Exception ex)
			{
				ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
			}
		}

		protected void gMain_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
		{
			gMain.EditIndex = -1;
			BuildGridData();
		}

		protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			try
			{
				if(e.Row.RowType == DataControlRowType.DataRow)
				{
					LinkButton btnDelete = (LinkButton)e.Row.FindControl("Delete");
					btnDelete.Attributes.Add("onclick", "javascript: return confirm('" + AppLogic.GetString("setupFTS.aspx.32", m_SkinID, cust.LocaleSetting) + "')");
				}
			}
			catch(Exception ex)
			{
				ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
			}
		}
		protected void gMain_Sorting(object sender, GridViewSortEventArgs e)
		{
			ViewState["IsInsert"] = false;
			gMain.EditIndex = -1;
			ViewState["Sort"] = e.SortExpression.ToString();
			ViewState["SortOrder"] = (ViewState["SortOrder"].ToString() == "ASC" ? "DESC" : "ASC");
			BuildGridData();
		}

		protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == "DeleteItem")
			{
				gMain.EditIndex = -1;
				int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());

				StringBuilder sql = new StringBuilder();
				sql.Append("delete from NoiseWords where ID=" + iden);
				try
				{
					DB.ExecuteSQL(sql.ToString());
					BuildGridData();
				}
				catch(Exception ex)
				{
					ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
				}
			}
		}

		protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
		{

			gMain.PageIndex = e.NewPageIndex;
			gMain.EditIndex = -1;
			BuildGridData();

		}

		protected void gMain_RowUpdating(object sender, GridViewUpdateEventArgs e)
		{
			GridViewRow row = gMain.Rows[e.RowIndex];

			try
			{
				if(row != null)
				{
					string newWord = ((TextBox)row.FindControl("txtNewNoiseWord")).Text;
					int newWordID = Convert.ToInt32((((Label)row.FindControl("lblNewNoiseWordID")).Text.ToString()));

					int count = 0;

					using(SqlConnection conn = DB.dbConn())
					{
						conn.Open();
						using(IDataReader reader = DB.GetRS("select count(*) from NoiseWords where word = " + DB.SQuote(newWord), conn))
						{
							if(reader.Read())
							{
								count = reader.GetInt32(0);
							}
						}
					}

					if(count > 0)
					{
						gMain.EditIndex = -1;
						BuildGridData();
						throw new Exception("setupFTS.aspx.30".StringResource());
					}
					else
					{
						StringBuilder sql2 = new StringBuilder();

						sql2.Append("update NoiseWords set word =" + DB.SQuote(newWord));
						sql2.Append(" where ID=" + newWordID);
						DB.ExecuteSQL(sql2.ToString());
						gMain.EditIndex = -1;
						BuildGridData();
					}
				}
			}
			catch(Exception ex)
			{
				ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
			}
		}

		protected void gMain_RowEditing(object sender, GridViewEditEventArgs e)
		{

			gMain.EditIndex = e.NewEditIndex;
			BuildGridData();

		}
		protected void btn_AddNewNoiseWord_Click(object sender, EventArgs e)
		{
			try
			{
				int count = 0;
				StringBuilder sql = new StringBuilder();

				using(SqlConnection conn = DB.dbConn())
				{
					conn.Open();
					using(IDataReader reader = DB.GetRS("select count(*) from NoiseWords where word = " + DB.SQuote(txtNewNoiseWord.Text.Trim()), conn))
					{
						if(reader.Read())
						{
							count = reader.GetInt32(0);
						}
					}
				}

				if(count > 0)
				{
					txtNewNoiseWord.Text = String.Empty;
					throw new Exception("setupFTS.aspx.31".StringResource());
				}
				else
				{
					sql.Append("insert NoiseWords values(");
					sql.Append(DB.SQuote(txtNewNoiseWord.Text.Trim()));
					sql.Append(")");
					DB.ExecuteSQL(sql.ToString());
					txtNewNoiseWord.Text = String.Empty;
					BuildGridData();
				}
			}
			catch(Exception ex)
			{
				ctlAlertMessage.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
			}
		}
	}
}
