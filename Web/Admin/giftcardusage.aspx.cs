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
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{

	public partial class giftcardusage : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected string selectSQL = "select G.*, C.LastName, C.FirstName FROM GiftCardUsage G with (NOLOCK) left outer join Customer C with (NOLOCK) " +
			" on G.UsedByCustomerID=C.CustomerID WHERE G.GiftCardID = " + CommonLogic.QueryStringNativeInt("giftcardid");
		int giftCardId;

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			Page.Form.DefaultButton = btnUsage.UniqueID;
			Page.Form.DefaultFocus = txtUsage.ClientID;

			giftCardId = CommonLogic.QueryStringNativeInt("giftcardid");

			var giftCardIdMatchSql = string.Format("select count(*) as N from giftcard where giftcardid={0}", giftCardId);
			var giftCardIdFound = DB.GetSqlN(giftCardIdMatchSql) > 0;

			if(giftCardIdFound)
			{
				if(!IsPostBack)
				{
					ltSerialNumber.Text = DB.GetSqlS(string.Format("SELECT SerialNumber AS S FROM GiftCard WHERE GiftCardID = {0}", giftCardId));
					ViewState["Sort"] = "G.CreatedOn";
					ViewState["SortOrder"] = "DESC";
					ViewState["SQLString"] = selectSQL;

					EditLink.NavigateUrl = EditLinkBottom.NavigateUrl = String.Format("{0}?giftcardid={1}", AppLogic.AdminLinkUrl("giftcard.aspx"), giftCardId);

					buildGridData();
				}
			}
			else
			{
				AlertMessage.PushAlertMessage("admin.giftcardusage.giftcardnotfound".StringResource(), AlertMessage.AlertType.Error);
				pnlGiftCardUsageWrap.Visible = false;
				EditLink.Visible = EditLinkBottom.Visible = false;
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
			ltBalance.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(DB.GetSqlNDecimal(String.Format("SELECT Balance AS N FROM GiftCard WHERE GiftCardID={0}", giftCardId)));
		}

		protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if(e.Row.RowType == DataControlRowType.DataRow)
			{
				DataRowView myrow = (DataRowView)e.Row.DataItem;

				string amount = Localization.CurrencyStringForDisplayWithoutExchangeRate(Localization.ParseNativeCurrency(myrow["Amount"].ToString()));
				Literal ltAmount = (Literal)e.Row.FindControl("ltAmount");
				ltAmount.Text = amount;
			}
		}

		protected void gMain_Sorting(object sender, GridViewSortEventArgs e)
		{
			ViewState["IsInsert"] = false;
			gMain.EditIndex = -1;
			ViewState["Sort"] = e.SortExpression.ToString();
			ViewState["SortOrder"] = (ViewState["SortOrder"].ToString() == "ASC" ? "DESC" : "ASC");
			buildGridData();
		}

		protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			ViewState["IsInsert"] = false;
			gMain.PageIndex = e.NewPageIndex;
			gMain.EditIndex = -1;
			buildGridData();
		}

		protected void btnUsage_Click(object sender, EventArgs e)
		{
			//prevent negative numbers
			var amount = Math.Abs(Localization.ParseUSDecimal(txtUsage.Text));
			var usageTypeId = Localization.ParseNativeInt(ddUsage.SelectedValue);

			var orderNumber = 0;
			int.TryParse(txtorderNumber.Text.Trim(), out orderNumber);

			DB.ExecuteSQL(
				@"INSERT INTO GiftCardUsage(GiftCardUsageGUID, GiftCardID, UsageTypeID, UsedByCustomerID, OrderNumber, Amount) 
					VALUES(newid(), @giftCardId, @usageTypeId, @customerId, @orderNumber, @amount)",
				new[]
					{
						new SqlParameter("@giftCardId", giftCardId),
						new SqlParameter("@usageTypeId", usageTypeId),
						new SqlParameter("@customerId", ThisCustomer.CustomerID),
						new SqlParameter("@orderNumber", orderNumber.ToString()),
						new SqlParameter("@amount", Localization.CurrencyStringForDBWithoutExchangeRate(amount))
					});

			//update the gift card
			var addOrSubtractAmount = usageTypeId == (int)GiftCardUsageType.AddFunds 
				? "+" + amount 
				: "-" + amount;

			DB.ExecuteSQL(string.Format("UPDATE GiftCard SET Balance = Balance {0} WHERE GiftCardID={1}",
				addOrSubtractAmount, 
				giftCardId));

			ViewState["Sort"] = "G.CreatedOn";
			ViewState["SortOrder"] = "DESC";

			buildGridData();

			if(usageTypeId == (int)GiftCardUsageType.AddFunds)
				AlertMessage.PushAlertMessage("admin.giftcardusage.FundsAdded".StringResource(), AlertMessage.AlertType.Success);
			else
				AlertMessage.PushAlertMessage("admin.giftcardusage.FundsDecremented".StringResource(), AlertMessage.AlertType.Success);

			// reset form fields:
			txtUsage.Text = txtorderNumber.Text = String.Empty;
			ddUsage.SelectedIndex = 0;
		}

		private enum GiftCardUsageType
		{
			AddFunds = 3,
			SubtractFunds = 4
		}
	}
}
