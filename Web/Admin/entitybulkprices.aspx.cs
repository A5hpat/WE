// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Data;
using System.Text;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefrontAdmin
{
	public partial class entityBulkPrices : AspDotNetStorefront.Admin.AdminPageBase
	{
		int EntityID;
		String EntityName;
		EntitySpecs m_EntitySpecs;
		EntityHelper Helper;
		Locale SelectedLocale;
		readonly LocaleSource LocaleSource;

		public entityBulkPrices()
		{
			LocaleSource = new LocaleSource();
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			EntityID = CommonLogic.QueryStringUSInt("EntityID"); ;
			EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
			m_EntitySpecs = EntityDefinitions.LookupSpecs(EntityName);
			Helper = new EntityHelper(m_EntitySpecs, 0);

			if (EntityID == 0 || EntityName.Length == 0)
			{
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.InvalidParameters", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return;
			}

			if (CommonLogic.FormCanBeDangerousContent("IsSubmit").Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
			{
				var products = new ProductCollection(m_EntitySpecs.m_EntityName, EntityID);
				products.PageSize = 0;
				products.PageNum = 1;
				products.PublishedOnly = false;
				products.ReturnAllVariants = true;

				var dsProducts = products.LoadFromDB();
				var NumProducts = products.NumProducts;
				foreach (DataRow row in dsProducts.Tables[0].Rows)
				{
					var ThisProductID = DB.RowFieldInt(row, "ProductID");
					var ThisVariantID = DB.RowFieldInt(row, "VariantID");
					var Price = System.Decimal.Zero;
					var SalePrice = System.Decimal.Zero;
					var MSRP = System.Decimal.Zero;
					var Cost = System.Decimal.Zero;

					if (CommonLogic.FormCanBeDangerousContent("Price_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString()).Length != 0)
						Price = CommonLogic.FormUSDecimal("Price_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString());
					
					if (CommonLogic.FormCanBeDangerousContent("SalePrice_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString()).Length != 0)
						SalePrice = CommonLogic.FormUSDecimal("SalePrice_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString());
					
					if (CommonLogic.FormCanBeDangerousContent("MSRP_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString()).Length != 0)
						MSRP = CommonLogic.FormUSDecimal("MSRP_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString());
					
					if (CommonLogic.FormCanBeDangerousContent("Cost_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString()).Length != 0)
						Cost = CommonLogic.FormUSDecimal("Cost_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString());
				
					var sql = new StringBuilder(1024);
					sql.Append("update productvariant set ");
					sql.Append("Price=" + Localization.DecimalStringForDB(Price) + ",");
					sql.Append("SalePrice=" + CommonLogic.IIF(SalePrice != System.Decimal.Zero, Localization.DecimalStringForDB(SalePrice), "NULL") + ",");
					sql.Append("MSRP=" + CommonLogic.IIF(MSRP != System.Decimal.Zero, Localization.DecimalStringForDB(MSRP), "NULL") + ",");
					sql.Append("Cost=" + CommonLogic.IIF(Cost != System.Decimal.Zero, Localization.DecimalStringForDB(Cost), "NULL"));
					sql.Append(" where VariantID=" + ThisVariantID.ToString());

					DB.ExecuteSQL(sql.ToString());
				}
				dsProducts.Dispose();

				AlertMessageDisplay.PushAlertMessage("The prices have been updated.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}

			SelectedLocale = LocaleSource.GetDefaultLocale();
			LoadBody(SelectedLocale.Name);
		}

		protected void LoadBody(string locale)
		{
			var body = new StringBuilder(4096);
			var mappingCount = DB.GetSqlN("select count(*) as N from Product" + this.m_EntitySpecs.m_EntityName + " where " + m_EntitySpecs.m_EntityName + "Id = " + this.EntityID.ToString());
			var products = new ProductCollection(m_EntitySpecs.m_EntityName, EntityID);
			products.PageSize = 0;
			products.PageNum = 1;
			products.PublishedOnly = false;
			products.ReturnAllVariants = true;

			var dsProducts = new DataSet();
			if (mappingCount > 0)
				dsProducts = products.LoadFromDB();

			var NumProducts = products.NumProducts;
			if (NumProducts > 1000)
			{
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.ImportExcession", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				MainBody.Visible = false;
			}
			else if (NumProducts > 0)
			{
				body.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
				body.Append("<table class=\"table\">\n");
				body.Append("<tr class=\"table-header\">\n");
				body.Append("<td><b>" + AppLogic.GetString("admin.common.ProductID", SkinID, LocaleSetting) + "</b></td>\n");
				body.Append("<td><b>" + AppLogic.GetString("admin.common.VariantID", SkinID, LocaleSetting) + "</b></td>\n");
				body.Append("<td><b>" + AppLogic.GetString("admin.common.ProductName", SkinID, LocaleSetting) + "</b></td>\n");
				body.Append("<td><b>" + AppLogic.GetString("admin.common.VariantName", SkinID, LocaleSetting) + "</b></td>\n");
				body.Append("<td align=\"center\"><b>" + AppLogic.GetString("admin.common.Price", SkinID, LocaleSetting) + "</b><br/><small>" + AppLogic.GetString("admin.entityBulkPrices.Format", SkinID, LocaleSetting) + "</small></td>\n");
				body.Append("<td align=\"center\"><b>" + AppLogic.GetString("admin.common.SalePrice", SkinID, LocaleSetting) + "</b><br/><small>" + AppLogic.GetString("admin.entityBulkPrices.Format", SkinID, LocaleSetting) + "</small></td>\n");
				body.Append("<td align=\"center\"><b>" + AppLogic.GetString("admin.common.MSRP", SkinID, LocaleSetting) + "</b><br/><small>" + AppLogic.GetString("admin.entityBulkPrices.Format", SkinID, LocaleSetting) + "</small></td>\n");
				body.Append("<td align=\"center\"><b>" + AppLogic.GetString("admin.common.Cost", SkinID, LocaleSetting) + "</b><br/><small>" + AppLogic.GetString("admin.entityBulkPrices.Format", SkinID, LocaleSetting) + "</small></td>\n");
				body.Append("</tr>\n");
				
				var LastProductID = 0;
				var rowcount = dsProducts.Tables[0].Rows.Count;
				for (var i = 0; i < rowcount; i++)
				{
					var row = dsProducts.Tables[0].Rows[i];

					var ThisProductID = DB.RowFieldInt(row, "ProductID");
					var ThisVariantID = DB.RowFieldInt(row, "VariantID");

					if (i % 2 == 0)
						body.Append("<tr class=\"table-row2\">\n");
					else
						body.Append("<tr class=\"table-alternatingrow2\">\n");
					
					body.Append("<td align=\"left\" valign=\"middle\">");
					body.Append(ThisProductID.ToString());
					body.Append("</td>");
					body.Append("<td align=\"left\" valign=\"middle\">");
					body.Append(ThisVariantID.ToString());
					body.Append("</td>");
					body.Append("<td align=\"left\" valign=\"middle\">");

					bool showlinks = false;
					if (showlinks)
						body.Append("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("product.aspx") + "?productid=" + ThisProductID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");
					
					body.Append(DB.RowFieldByLocale(row, "Name", locale));
					if (showlinks)
						body.Append("</a>");
					
					body.Append("</td>\n");
					body.Append("<td align=\"left\" valign=\"middle\">");
					
					if (showlinks)
						body.Append("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("variant.aspx") + "?productid=" + ThisProductID.ToString() + "&variantid=" + ThisVariantID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityID.ToString() + "\">");

					body.Append(DB.RowFieldByLocale(row, "VariantName", locale));
					
					if (showlinks)
						body.Append("</a>");
					
					body.Append("</td>\n");
					body.Append("<td align=\"center\" valign=\"middle\">");
					body.Append("<input maxLength=\"10\" size=\"10\" class=\"default\" name=\"Price_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(row, "Price")) + "\">");
					body.Append("<input type=\"hidden\" name=\"Price_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "_vldt\" value=\"[req][number][blankalert=" + AppLogic.GetString("admin.common.VariantPricePrompt", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("admin.common.ValidDollarAmountPrompt", SkinID, LocaleSetting) + "]\">\n");
					body.Append("</td>");
					body.Append("<td align=\"center\" valign=\"middle\">");
					body.Append("<input maxLength=\"10\" size=\"10\" class=\"default\" name=\"SalePrice_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" value=\"" + CommonLogic.IIF(DB.RowFieldDecimal(row, "SalePrice") != System.Decimal.Zero, Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(row, "SalePrice")), "") + "\">\n");
					body.Append("<input type=\"hidden\" name=\"SalePrice_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "_vldt\" value=\"[number][invalidalert=" + AppLogic.GetString("admin.common.ValidDollarAmountPrompt", SkinID, LocaleSetting) + "]\">\n");
					body.Append("</td>");
					body.Append("<td align=\"center\" valign=\"middle\">");
					body.Append("<input maxLength=\"10\" size=\"10\" class=\"default\" name=\"MSRP_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" value=\"" + CommonLogic.IIF(DB.RowFieldDecimal(row, "MSRP") != System.Decimal.Zero, Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(row, "MSRP")), "") + "\">\n");
					body.Append("<input type=\"hidden\" name=\"MSRP_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "_vldt\" value=\"[number][invalidalert=" + AppLogic.GetString("admin.commmon.ValidDollarAmountPrompt", SkinID, LocaleSetting) + "]\">\n");
					body.Append("</td>");
					body.Append("<td align=\"center\" valign=\"middle\">");
					body.Append("<input maxLength=\"10\" size=\"10\" class=\"default\" name=\"Cost_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "\" value=\"" + CommonLogic.IIF(DB.RowFieldDecimal(row, "Cost") != System.Decimal.Zero, Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(row, "Cost")), "") + "\">\n");
					body.Append("<input type=\"hidden\" name=\"Cost_" + ThisProductID.ToString() + "_" + ThisVariantID.ToString() + "_vldt\" value=\"[number][invalidalert=" + AppLogic.GetString("admin.common.ValidDollarAmountPrompt", SkinID, LocaleSetting) + "]\">\n");
					body.Append("</td>\n");
					body.Append("</tr>\n");
					
					LastProductID = ThisProductID;
				}
				body.Append("</table>\n");
			}
			else
			{
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.NoProductsFound", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				MainBody.Visible = false;
			}

			dsProducts.Dispose();
			products.Dispose();

			ltBody.Text = body.ToString();
		}
	}
}
