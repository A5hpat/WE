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
	/// <summary>
	/// ZipCodes for taxes
	/// </summary>
	public partial class zipcodes : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected int RowBoundIndex = 1;

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			RowBoundIndex = 1;

			if(ViewState["SortExpression"] == null)
				ViewState["SortExpression"] = "DisplayOrder";

			if(ViewState["SortDirection"] == null)
				ViewState["SortDirection"] = "ASC";

			if(!IsPostBack)
			{
				BuildGridData();
			}
			RowBoundIndex = 1;
		}

		protected void BuildGridData()
		{
			string sql = String.Format(@"SELECT DISTINCT z.ZipCode AS ZipCode, 
												z.CountryID AS CountryID,
												c.Name AS Name,
												c.DisplayOrder
												FROM ZipTaxRate z, Country c with (NOLOCK)
												WHERE z.CountryId = c.CountryId
												ORDER BY {0} {1}", ViewState["SortExpression"], ViewState["SortDirection"]);

			using(DataTable dt = new DataTable())
			{
				using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
				{
					connection.Open();
					using(IDataReader rs = DB.GetRS(sql, connection))
					{
						dt.Load(rs);
					}
				}

				gMain.DataSource = dt;
				gMain.DataBind();
			}
		}

		protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if(e.Row.RowType == DataControlRowType.DataRow)
			{
				LinkButton ib = (LinkButton)e.Row.FindControl("Delete");
				ib.Attributes.Add("onclick", "javascript: return confirm('Confirm Delete?')");

				RowBoundIndex++;
			}
		}

		protected void gMain_Sorting(object sender, GridViewSortEventArgs e)
		{
			gMain.EditIndex = -1;
			var sortExpression = e.SortExpression.ToString();
			var sortDirection = "ASC";

			//check the sortexpression to decide whether we need to change directions
			if(sortExpression == ViewState["SortExpression"].ToString())
				sortDirection = ViewState["SortDirection"].ToString() == "ASC" ? "DESC" : "ASC";

			//now store the sort expression for use later
			ViewState["SortExpression"] = sortExpression;
			ViewState["SortDirection"] = sortDirection;

			BuildGridData();
		}
		protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == "DeleteItem")
			{
				gMain.EditIndex = -1;
				String[] keys = e.CommandArgument.ToString().Split('|');
				string ZipCode = keys[0];
				string CountryID = keys[1];
				deleteRowPerm(ZipCode, AppLogic.GetCountryID(CountryID));
			}
		}

		protected void deleteRowPerm(string ZipCode, int CountryID)
		{
			StringBuilder sql = new StringBuilder(2500);
			sql.Append("delete from ZipTaxRate where ZipCode=" + DB.SQuote(ZipCode) + " AND CountryID=" + CountryID);
			try
			{
				DB.ExecuteSQL(sql.ToString());

				// synchronize with the cached tax rates
				AppLogic.ZipTaxRatesTable.RemoveAll(ZipCode, CountryID);
				BuildGridData();
				ctrlAlertMessage.PushAlertMessage("Item Deleted", AlertMessage.AlertType.Success);
			}
			catch(Exception ex)
			{
				ctrlAlertMessage.PushAlertMessage("Couldn't Delete - " + ex.Message, AlertMessage.AlertType.Danger);
			}
		}

		protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			gMain.PageIndex = e.NewPageIndex;
			gMain.EditIndex = -1;
			BuildGridData();
		}
	}
}
