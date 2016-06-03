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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class ShippingMethodStates : AspDotNetStorefront.Admin.AdminPageBase
	{
		int ShippingMethodID = 0;

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			ShippingMethodID = CommonLogic.QueryStringUSInt("shippingmethodid");
			if(ShippingMethodID == 0)
				Response.Redirect(AppLogic.AdminLinkUrl("shippingmethods.aspx"));

			if(IsPostBack)
				return;

			if(DB.GetSqlN("select count(*) as N from State with (NOLOCK)") == 0)
			{
				AlertMessage.PushAlertMessage("No States are defined!", AspDotNetStorefrontControls.AlertMessage.AlertType.Warning);
				return;
			}

			RenderDataTable();
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			btnClose.DataBind();
			btnCloseTop.DataBind();
		}

		protected void btnSave_Click(object sender, EventArgs e)
		{
			DB.ExecuteSQL("delete from ShippingMethodToStateMap where ShippingMethodID=" + ShippingMethodID.ToString());
			foreach(String s in CommonLogic.FormCanBeDangerousContent("StateList").Split(','))
				if(s.Trim().Length != 0)
					DB.ExecuteSQL("insert ShippingMethodToStateMap(ShippingMethodID,StateID) values(" + ShippingMethodID.ToString() + "," + s + ")");

			AlertMessage.PushAlertMessage("Items Saved.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			RenderDataTable();
		}

		protected void btnAllowAll_Click(object sender, EventArgs e)
		{
			DB.ExecuteSQL("delete from ShippingMethodToStateMap where ShippingMethodID=" + ShippingMethodID.ToString());
			DB.ExecuteSQL("insert into ShippingMethodToStateMap(ShippingMethodID,StateID) select " + ShippingMethodID.ToString() + ",StateID from State");
			AlertMessage.PushAlertMessage("Items Saved.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			RenderDataTable();
		}

		protected void btnClearAll_Click(object sender, EventArgs e)
		{
			DB.ExecuteSQL("delete from ShippingMethodToStateMap where ShippingMethodID=" + ShippingMethodID.ToString());
			AlertMessage.PushAlertMessage("Items Saved.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			RenderDataTable();
		}

		void RenderDataTable()
		{
			StringBuilder writer = new StringBuilder();
			writer.Append("");

			writer.Append("<table class=\"table\">");
			writer.Append("<tr class=\"table-alternatingrow2\"><td>State/Province/County</td><td>Country</td><td>Allowed?</td></tr>");

			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				using(IDataReader reader = DB.GetRS(
					"select s.StateID, s.Name, c.Name countryName, sm2s.ShippingMethodID " +
					"from dbo.State s with (NOLOCK) " +
					"left outer join dbo.Country c with (NOLOCK) on c.CountryID = s.CountryID " +
					"left outer join dbo.ShippingMethodToStateMap sm2s with (NOLOCK) on s.StateID = sm2s.StateID and sm2s.ShippingMethodID = " + ShippingMethodID.ToString() + " " +
					"order by c.DisplayOrder, c.Name, s.DisplayOrder, s.Name", connection))
				{
					while(reader.Read())
					{
						bool AllowedForThisState = DB.RSFieldInt(reader, "ShippingMethodID") != 0;

						writer.Append("<tr>");
						writer.Append("<td>");
						writer.Append(DB.RSField(reader, "Name"));
						writer.Append("</td>");
						writer.Append("<td>");
						writer.Append(DB.RSField(reader, "countryName"));
						writer.Append("</td>");
						writer.Append("<td>");
						writer.Append("<input type=\"checkbox\" name=\"StateList\" value=\"" + DB.RSFieldInt(reader, "StateID").ToString() + "\" " + CommonLogic.IIF(AllowedForThisState, " checked ", "") + ">");
						writer.Append("</td>");
						writer.Append("</tr>");
					}
				}
			}
			writer.Append("</table>");

			ltContent.Text = writer.ToString();
		}
	}
}
