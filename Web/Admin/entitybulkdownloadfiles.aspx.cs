// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Text;
using System.Globalization;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefrontAdmin
{
	public partial class entityBulkDownloadFiles : AspDotNetStorefront.Admin.AdminPageBase
	{
		int EntityID;
		String EntityName;
		EntitySpecs m_EntitySpecs;
		EntityHelper Helper;
		
		Locale SelectedLocale;
		readonly LocaleSource LocaleSource;

		public entityBulkDownloadFiles()
		{
			LocaleSource = new LocaleSource();
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			EntityID = CommonLogic.QueryStringUSInt("EntityID");
			if(EntityID < 1)
				EntityID = CommonLogic.FormNativeInt("EntityID");

			EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
			if(String.IsNullOrEmpty(EntityName))
				EntityName = CommonLogic.FormCanBeDangerousContent("EntityName");

			m_EntitySpecs = EntityDefinitions.LookupSpecs(EntityName);
			Helper = new EntityHelper(m_EntitySpecs, 0);

			if(EntityID == 0 || EntityName.Length == 0)
			{
				ltBody.Text = AppLogic.GetString("admin.common.InvalidParameters", SkinID, LocaleSetting);
				return;
			}

			if(CommonLogic.FormCanBeDangerousContent("IsSubmit").Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
			{
				var products = new ProductCollection(m_EntitySpecs.m_EntityName, EntityID);
				products.PageSize = 0;
				products.PageNum = 1;
				products.PublishedOnly = false;
				products.ReturnAllVariants = true;

				using(var dsProducts = products.LoadFromDB())
				{
					var NumProducts = products.NumProducts;
					foreach(DataRow row in dsProducts.Tables[0].Rows)
					{
						if(DB.RowFieldBool(row, "IsDownload"))
						{
							var ThisProductID = DB.RowFieldInt(row, "ProductID");
							var ThisVariantID = DB.RowFieldInt(row, "VariantID");
							var sql = new StringBuilder(1024);
							sql.Append("update productvariant set ");

							var DLoc = CommonLogic.FormCanBeDangerousContent("DownloadLocation_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString());
							if(DLoc.StartsWith("/"))
								DLoc = DLoc.Substring(1, DLoc.Length - 1); // remove leading / char!

							sql.Append("DownloadLocation=" + DB.SQuote(DLoc));
							sql.Append(" where VariantID=" + ThisVariantID.ToString());
							DB.ExecuteSQL(sql.ToString());
						}
					}
				}
				AlertMessage.PushAlertMessage("Download Files Saved", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}

			SelectedLocale = LocaleSource.GetDefaultLocale();

			LoadBody(SelectedLocale.Name);
		}

		protected void LoadBody(string locale)
		{
			ltBody.Text = ("<input type=\"hidden\" name=\"EntityName\" value=\"" + EntityName + "\">\n");
			ltBody.Text += ("<input type=\"hidden\" name=\"EntityID\" value=\"" + EntityID + "\">\n");

			var products = new ProductCollection(m_EntitySpecs.m_EntityName, EntityID);
			products.PageSize = 0;
			products.PageNum = 1;
			products.PublishedOnly = false;
			products.ReturnAllVariants = true;

			var mappingCount = DB.GetSqlN("select count(*) as N from Product" + this.m_EntitySpecs.m_EntityName + " where " + m_EntitySpecs.m_EntityName + "Id = " + this.EntityID.ToString());

			var dsProducts = new DataSet();
			if(mappingCount > 0)
				dsProducts = products.LoadFromDB();

			var NumProducts = products.NumProducts;
			if(NumProducts > 1000)
			{
				MainBody.Visible = false;
				AlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.ImportExcession", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}
			else if(NumProducts > 0)
			{
				ltBody.Text += ("<script type=\"text/javascript\">\n");
				ltBody.Text += ("function Form_Validator(theForm)\n");
				ltBody.Text += ("{\n");
				ltBody.Text += ("submitonce(theForm);\n");
				ltBody.Text += ("return (true);\n");
				ltBody.Text += ("}\n");
				ltBody.Text += ("</script>\n");

				ltBody.Text += ("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
				ltBody.Text += ("<table class=\"table\">\n");
				ltBody.Text += ("<tr class=\"table-header\">\n");
				ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.ProductID", SkinID, LocaleSetting) + "</b></td>\n");
				ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.VariantID", SkinID, LocaleSetting) + "</b></td>\n");
				ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.ProductName", SkinID, LocaleSetting) + "</b></td>\n");
				ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.VariantName", SkinID, LocaleSetting) + "</b></td>\n");
				ltBody.Text += ("<td><b>" + AppLogic.GetString("admin.common.DownloadFile", SkinID, LocaleSetting) + "</b></td>\n");
				ltBody.Text += ("</tr>\n");

				var LastProductID = 0;
				var rowcount = dsProducts.Tables[0].Rows.Count;

				for(var i = 0; i < rowcount; i++)
				{
					var row = dsProducts.Tables[0].Rows[i];

					if(DB.RowFieldBool(row, "IsDownload"))
					{
						var ThisProductID = DB.RowFieldInt(row, "ProductID");
						var ThisVariantID = DB.RowFieldInt(row, "VariantID");

						if(i % 2 == 0)
							ltBody.Text += ("<tr class=\"table-row2\">\n");
						else
							ltBody.Text += ("<tr class=\"table-alternatingrow2\">\n");

						ltBody.Text += ("<td>");
						ltBody.Text += (ThisProductID.ToString());
						ltBody.Text += ("</td>");
						ltBody.Text += ("<td>");
						ltBody.Text += (ThisVariantID.ToString());
						ltBody.Text += ("</td>");
						ltBody.Text += ("<td>");

						bool showlinks = false;
						if(showlinks)
							ltBody.Text += ("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("product.aspx") + "?productid=" + ThisProductID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");

						ltBody.Text += (DB.RowFieldByLocale(row, "Name", locale));

						if(showlinks)
							ltBody.Text += ("</a>");

						ltBody.Text += ("</td>\n");
						ltBody.Text += ("<td>");

						if(showlinks)
							ltBody.Text += ("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("variant.aspx") + "?productid=" + ThisProductID.ToString() + "&variantid=" + ThisVariantID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");

						ltBody.Text += (DB.RowFieldByLocale(row, "VariantName", locale));

						if(showlinks)
							ltBody.Text += ("</a>");

						ltBody.Text += ("</td>\n");
						ltBody.Text += ("<td>");
						ltBody.Text += ("<input maxLength=\"1000\" class=\"singleNormal\" onkeypress=\"javascript:return WebForm_FireDefaultButton(event, 'btnsubmit')\" name=\"DownloadLocation_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" value=\"" + DB.RowField(row, "DownloadLocation") + "\">\n");
						ltBody.Text += ("</td>\n");
						ltBody.Text += ("</tr>\n");

						LastProductID = ThisProductID;
					}
				}

				if(LastProductID == 0)
				{
					MainBody.Visible = false;
					AlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.NoDownloadProductsFound", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				}

				ltBody.Text += ("</table>\n");
			}
			else
			{
				MainBody.Visible = false;
				AlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.NoProductsFound", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}

			dsProducts.Dispose();
			products.Dispose();
		}
	}
}
