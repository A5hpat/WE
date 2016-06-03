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
	public partial class entityBulkShipping : AspDotNetStorefront.Admin.AdminPageBase
	{
		int EntityId;
		string EntityName;
		EntitySpecs EntitySpecs;
		EntityHelper Helper;
		Locale SelectedLocale;
		readonly LocaleSource LocaleSource;

		public entityBulkShipping()
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
				ltBody.Text = AppLogic.GetString("admin.common.InvalidParameters", SkinID, LocaleSetting);
				return;
			}

			if(CommonLogic.FormCanBeDangerousContent("IsSubmit").Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
			{
				for(var i = 0; i <= Request.Form.Count - 1; i++)
				{
					var fieldName = Request.Form.Keys[i];
					if(fieldName.StartsWith("shippingcost", StringComparison.InvariantCultureIgnoreCase) && !fieldName.EndsWith("_vldt", StringComparison.InvariantCultureIgnoreCase))
					{
						var fieldNameSplit = fieldName.Split('_');
						var variantId = Localization.ParseUSInt(fieldNameSplit[1]);
						var shippingMethodId = Localization.ParseUSInt(fieldNameSplit[2]);
						var shippingCost = CommonLogic.FormUSDecimal(fieldName);
						DB.ExecuteSQL("delete from ShippingByProduct where VariantID=" + variantId.ToString() + " and ShippingMethodID=" + shippingMethodId.ToString());
						DB.ExecuteSQL("insert ShippingByProduct(VariantID,ShippingMethodID,ShippingCost) values(" + variantId.ToString() + "," + shippingMethodId.ToString() + "," + Localization.CurrencyStringForDBWithoutExchangeRate(shippingCost) + ")");
					}
				}

				AlertMessageDisplay.PushAlertMessage("The shipping costs have been saved.", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}

			SelectedLocale = LocaleSource.GetDefaultLocale();

			LoadBody(SelectedLocale.Name);
		}

		void LoadBody(string locale)
		{
			var body = new StringBuilder(4096);

			using(var products = new ProductCollection(EntitySpecs.m_EntityName, EntityId))
			{
				var mappingCount = DB.GetSqlN("select count(*) as N from Product" + this.EntitySpecs.m_EntityName + " where " + EntitySpecs.m_EntityName + "Id = " + this.EntityId.ToString());

				products.PageSize = 0;
				products.PageNum = 1;
				products.PublishedOnly = false;
				products.ReturnAllVariants = true;

				var dsProducts = new DataSet();
				if(mappingCount > 0)
					dsProducts = products.LoadFromDB();

				var numberOfProducts = products.NumProducts;
				if(numberOfProducts > 1000)
				{
					AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.ImportExcession", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
					MainBody.Visible = false;
				}
				else if(numberOfProducts > 0)
				{
					using(var dtShippingMethods = new DataTable())
					{
						using(var connection = new SqlConnection(DB.GetDBConn()))
						{
							connection.Open();
							using(var rs = DB.GetRS("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 order by DisplayOrder", connection))
							{
								dtShippingMethods.Load(rs);
							}
						}

						body.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
						body.Append("<table class=\"table\">\n");
						body.Append("<tr class=\"table-header\">\n");
						body.Append("<td><b>" + AppLogic.GetString("admin.common.ProductID", SkinID, LocaleSetting) + "</b></td>\n");
						body.Append("<td><b>" + AppLogic.GetString("admin.common.VariantID", SkinID, LocaleSetting) + "</b></td>\n");
						body.Append("<td><b>" + AppLogic.GetString("admin.common.ProductName", SkinID, LocaleSetting) + "</b></td>\n");
						body.Append("<td><b>" + AppLogic.GetString("admin.common.VariantName", SkinID, LocaleSetting) + "</b></td>\n");
						
						foreach(DataRow row in dtShippingMethods.Rows)
							body.Append("<td><b>" + DB.RowFieldByLocale(row, "Name", locale) + " Cost</b><br/>" + AppLogic.GetString("admin.entityBulkPrices.Format", SkinID, LocaleSetting) + "</td>\n");
						
						body.Append("</tr>\n");

						for(var i = 0; i < dsProducts.Tables[0].Rows.Count; i++)
						{
							var row = dsProducts.Tables[0].Rows[i];
							var productId = DB.RowFieldInt(row, "ProductID");
							var variantId = DB.RowFieldInt(row, "VariantID");

							body.Append("<tr>\n");
							body.Append("<td>");
							body.Append(productId.ToString());
							body.Append("</td>");
							body.Append("<td>");
							body.Append(variantId.ToString());
							body.Append("</td>");
							body.Append("<td>");
							body.Append("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("product.aspx") + "?productid=" + productId.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityId.ToString() + "\">");
							body.Append(DB.RowFieldByLocale(row, "Name", locale));
							body.Append("</a>");
							body.Append("</td>\n");
							body.Append("<td>");
							body.Append("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("variant.aspx") + "?productid=" + productId.ToString() + "&variantid=" + variantId.ToString() + "\">");
							body.Append(DB.RowFieldByLocale(row, "VariantName", locale));
							body.Append("</a>");
							body.Append("</td>\n");

							foreach(DataRow row2 in dtShippingMethods.Rows)
							{
								body.Append("<td>");
								body.Append("<input class=\"default\" maxLength=\"10\" size=\"10\" name=\"ShippingCost_" + variantId.ToString() + "_" + DB.RowFieldInt(row2, "ShippingMethodID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetVariantShippingCost(variantId, DB.RowFieldInt(row2, "ShippingMethodID"))) + "\">\n");
								body.Append("<input type=\"hidden\" name=\"ShippingCost_" + variantId.ToString() + "_" + DB.RowFieldInt(row2, "ShippingMethodID") + "_vldt\" value=\"[number][invalidalert=" + AppLogic.GetString("admin.common.ValidDollarAmountPrompt", SkinID, LocaleSetting) + "]\">\n");
								body.Append("</td>\n");
							}

							body.Append("</tr>\n");
						}

						body.Append("</table>\n");
					}
				}
				else
				{
					AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.NoProductsFound", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
					MainBody.Visible = false;
				}

				ltBody.Text = body.ToString();
			}
		}
	}
}
