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
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class EntityProductBulkDisplayOrder : AspDotNetStorefront.Admin.AdminPageBase
	{
		int EntityId;
		String EntityName;
		EntitySpecs EntitySpecs;
		EntityHelper Helper;
		protected int EntityCount = 0;

		Locale SelectedLocale;
		readonly LocaleSource LocaleSource;

		public EntityProductBulkDisplayOrder()
		{
			LocaleSource = new LocaleSource();
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			EntityId = CommonLogic.QueryStringUSInt("EntityID");
			EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
			EntitySpecs = EntityDefinitions.LookupSpecs(EntityName);
			Helper = new EntityHelper(EntitySpecs, 0);
			
			if(EntityId == 0 || EntityName.Length == 0)
			{
				RenderContainer.Text = "Invalid Parameters";
				return;
			}

			SelectedLocale = LocaleSource.GetDefaultLocale();

			Render(SelectedLocale.Name);
			DataBind();
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(EntityId != 0)
				DB.ExecuteSQL(String.Format("delete from {0}{1} where {2}ID={3}", EntitySpecs.m_ObjectName, EntitySpecs.m_EntityName, EntitySpecs.m_EntityName, EntityId.ToString()));

			for(var i = 0; i <= Request.Form.Count - 1; i++)
			{
				if(Request.Form.Keys[i].IndexOf("DisplayOrder_") != -1)
				{
					var keys = Request.Form.Keys[i].Split('_');
					var entityId = Localization.ParseUSInt(keys[1]);
					var displayOrder = 1;
					try
					{
						displayOrder = Localization.ParseUSInt(Request.Form[Request.Form.Keys[i]]);
					}
					catch { }
					DB.ExecuteSQL(String.Format("insert into {0}{1}({2}ID,{3}ID,DisplayOrder) values({4},{5},{6})", EntitySpecs.m_ObjectName, EntitySpecs.m_EntityName, EntitySpecs.m_EntityName, EntitySpecs.m_ObjectName, EntityId.ToString(), entityId.ToString(), displayOrder.ToString()));
				}
			}

			AlertMessage.PushAlertMessage("Display order updated", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			Render(SelectedLocale.Name);
		}

		void Render(string locale)
		{
			var body = new StringBuilder(4096);

			var sql = "select ~.*,DisplayOrder from ~   with (NOLOCK)  left outer join ~^  with (NOLOCK)  on ~.~id=~^.~id where ~^.^id=" + EntityId.ToString() + " and deleted=0 ";
			sql += " and ~.~ID in (select distinct ~id from ~^   with (NOLOCK)  where ^id=" + EntityId.ToString() + ")";
			sql += " order by DisplayOrder,Name";

			sql = sql.Replace("^", EntitySpecs.m_EntityName).Replace("~", EntitySpecs.m_ObjectName);

			body.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
			body.Append("  <table class=\"table\">\n");
			body.Append("    <tr class=\"table-header\">\n");
			body.Append("      <td><b>ID</b></td>\n");
			body.Append("      <td><b>" + EntitySpecs.m_ObjectName + "</b></td>\n");
			body.Append("      <td align=\"center\"><b>Display Order</b></td>\n");
			body.Append("    </tr>\n");

			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(var rs = DB.GetRS(sql, conn))
				{
					while(rs.Read())
					{
						var ThisID = DB.RSFieldInt(rs, EntitySpecs.m_ObjectName + "ID");
						if(EntityCount % 2 == 0)
							body.Append("    <tr class=\"table-row2\">\n");
						else
							body.Append("    <tr class=\"table-alternatingrow2\">\n");

						body.Append("      <td >" + ThisID.ToString() + "</td>\n");
						body.Append("      <td >");

						var Image1URL = AppLogic.LookupImage(EntitySpecs.m_ObjectName, ThisID, "icon", 1, locale);

						var showlinks = EntitySpecs.m_ObjectName != "Product";
						if(showlinks)
							body.Append("<a target=\"entityBody\" href=\"EntityEdit" + EntitySpecs.m_ObjectName + "s.aspx?productid=" + ThisID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityId.ToString() + "\">");

						body.Append("<img src=\"" + Image1URL + "\" height=\"25\" border=\"0\" align=\"absmiddle\">");

						if(showlinks)
							body.Append("</a>\n");

						body.Append("&nbsp;");

						if(showlinks)
							body.Append("<a target=\"entityBody\" href=\"EntityEdit" + EntitySpecs.m_ObjectName + "s.aspx?productid=" + ThisID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityId.ToString() + "\">");

						body.Append(DB.RSFieldByLocale(rs, "Name", locale));

						if(showlinks)
							body.Append("</a>");

						body.Append("</td>\n");
						body.Append("      <td align=\"center\"><input size=2 class=\"default\" type=\"text\" name=\"DisplayOrder_" + ThisID.ToString() + "\" value=\"" + CommonLogic.IIF(DB.RSFieldInt(rs, "DisplayOrder") == 0, "1", (DB.RSFieldInt(rs, "DisplayOrder")).ToString()) + "\"></td>\n");
						body.Append("    </tr>\n");
						EntityCount++;
					}
				}
			}

			body.Append("  </table>\n");

			if(EntityCount == 0)
				AlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.NoProductsFound", 1, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			else
				RenderContainer.Text = body.ToString();
		}
	}
}
