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
	public partial class ShippingMethodCountries : AspDotNetStorefront.Admin.AdminPageBase
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

			if(DB.GetSqlN("select count(*) as N from Country with (NOLOCK)") == 0)
			{
				AlertMessage.PushAlertMessage("No Countries are defined!", AspDotNetStorefrontControls.AlertMessage.AlertType.Warning);
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
			DB.ExecuteSQL("delete from ShippingMethodToCountryMap where ShippingMethodID=" + ShippingMethodID.ToString());
			foreach(String s in CommonLogic.FormCanBeDangerousContent("CountryList").Split(','))
				if(s.Trim().Length != 0)
					DB.ExecuteSQL("insert ShippingMethodToCountryMap(ShippingMethodID,CountryID) values(" + ShippingMethodID.ToString() + "," + s + ")");

			AlertMessage.PushAlertMessage("Items Saved.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			RenderDataTable();
		}

		protected void btnAllowAll_Click(object sender, EventArgs e)
		{
			DB.ExecuteSQL("delete from ShippingMethodToCountryMap where ShippingMethodID=" + ShippingMethodID.ToString());
			DB.ExecuteSQL("insert into ShippingMethodToCountryMap(ShippingMethodID,CountryID) select " + ShippingMethodID.ToString() + ",CountryID from Country");

			AlertMessage.PushAlertMessage("Items Saved.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			RenderDataTable();
		}

		protected void btnClearAll_Click(object sender, EventArgs e)
		{
			DB.ExecuteSQL("delete from ShippingMethodToCountryMap where ShippingMethodID=" + ShippingMethodID.ToString());

			AlertMessage.PushAlertMessage("Items Saved.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			RenderDataTable();
		}

		void RenderDataTable()
		{
			StringBuilder writer = new StringBuilder();
			writer.Append("<table class=\"table\">");

			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				using(IDataReader reader = DB.GetRS("select Country.CountryID,Country.Name,ShippingMethodToCountryMap.ShippingMethodID from Country  with (NOLOCK)  left outer join ShippingMethodToCountryMap  with (NOLOCK)  on Country.CountryID=ShippingMethodToCountryMap.CountryID and ShippingMethodToCountryMap.ShippingMethodID=" + ShippingMethodID.ToString() + " order by displayorder,name", connection))
				{
					while(reader.Read())
					{
						bool AllowedForThisCountry = DB.RSFieldInt(reader, "ShippingMethodID") != 0;

						writer.Append("<tr>");
						writer.Append("<td>");
						writer.Append(DB.RSField(reader, "Name"));
						writer.Append("</td>");
						writer.Append("<td>");
						writer.Append("<input type=\"checkbox\" name=\"CountryList\" value=\"" + DB.RSFieldInt(reader, "CountryID").ToString() + "\" " + CommonLogic.IIF(AllowedForThisCountry, " checked ", "") + ">");
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
