// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using System.Text;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class entityBulkInventory : AspDotNetStorefront.Admin.AdminPageBase
	{
		int EntityId;
		String EntityName;
		EntitySpecs EntitySpecs;
		EntityHelper Helper;
		new int SkinID = 1;
		Locale SelectedLocale;
		readonly LocaleSource LocaleSource;

		public entityBulkInventory()
		{
			LocaleSource = new LocaleSource();
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			EntityId = CommonLogic.QueryStringUSInt("EntityID"); ;
			EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
			EntitySpecs = EntityDefinitions.LookupSpecs(EntityName);
			Helper = new EntityHelper(EntitySpecs, 0);

			if (EntityId == 0 || EntityName.Length == 0)
			{
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.InvalidParameters", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				MainBody.Visible = false;
				return;
			}

			SelectedLocale = LocaleSource.GetDefaultLocale();

			LoadBody(SelectedLocale.Name);
		}

		protected void LoadBody(string locale)
		{
			var tmpS = new StringBuilder(4096);

			var mappingCount = DB.GetSqlN("select count(*) as N from Product" + this.EntitySpecs.m_EntityName + " where " + EntitySpecs.m_EntityName + "Id = " + this.EntityId.ToString());

			var products = new ProductCollection(EntityName, EntityId);
			products.PageSize = 0;
			products.PageNum = 1;
			products.PublishedOnly = false;
			products.ReturnAllVariants = true;

			var dsProducts = new DataSet();
			if (mappingCount > 0)
				dsProducts = products.LoadFromDB();

			int NumProducts = products.NumProducts;
			if (NumProducts > 1000)
			{
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.ImportExcession", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				MainBody.Visible = false;
			}
			else if (NumProducts > 0)
			{
				tmpS.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
				tmpS.Append("<table class=\"table\">\n");
				tmpS.Append("<tr class=\"table-header\">\n");
				tmpS.Append("<td><b>ProductID</b></td>\n");
				tmpS.Append("<td><b>VariantID</b></td>\n");
				tmpS.Append("<td><b>Product Name</b></td>\n");
				tmpS.Append("<td><b>Variant Name</b></td>\n");
				tmpS.Append("<td><b>Inventory</b></td>\n");
				tmpS.Append("</tr>\n");

				int rowcount = dsProducts.Tables[0].Rows.Count;

				for (var i = 0; i < rowcount; i++)
				{
					var row = dsProducts.Tables[0].Rows[i];

					var ThisProductID = DB.RowFieldInt(row, "ProductID");
					var ThisVariantID = DB.RowFieldInt(row, "VariantID");

					if (i % 2 == 0)
					{
						tmpS.Append("<tr>\n");
					}
					else
					{
						tmpS.Append("<tr>\n");
					}
					tmpS.Append("<td>");
					tmpS.Append(ThisProductID.ToString());
					tmpS.Append("</td>");
					tmpS.Append("<td>");
					tmpS.Append(ThisVariantID.ToString());
					tmpS.Append("</td>");
					tmpS.Append("<td>");
					
					bool showlinks = false;
					if (showlinks)
						tmpS.Append("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("product.aspx") + "?productid=" + ThisProductID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityId.ToString() + "\">");

					tmpS.Append(DB.RowFieldByLocale(row, "Name", locale));
					
					if (showlinks)
						tmpS.Append("</a>");
					
					tmpS.Append("</td>\n");
					tmpS.Append("<td>");
					
					if (showlinks)
						tmpS.Append("<a target=\"entityBody\" href=\"" + AppLogic.AdminLinkUrl("variant.aspx") + "?productid=" + ThisProductID.ToString() + "&variantid=" + ThisVariantID.ToString() + "&entityname=" + EntityName + "&entityid=" + EntityId.ToString() + "\">");

					tmpS.Append(DB.RowFieldByLocale(row, "VariantName", locale));
					
					if (showlinks)
						tmpS.Append("</a>");
					
					tmpS.Append("</td>\n");
					tmpS.Append("<td>");
					
					var inventoryTable = AppLogic.GetInventoryTable(ThisProductID, ThisVariantID, true, SkinID, false, true);
					tmpS.Append(inventoryTable);
					tmpS.Append("</td>\n");
					tmpS.Append("</tr>\n");

				}
				tmpS.Append("</table>\n");
			}
			else
			{
				MainBody.Visible = false;
				AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.NoProductsFound", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}

			dsProducts.Dispose();
			products.Dispose();

			ltBody.Text = tmpS.ToString();
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			for (var i = 0; i <= Request.Form.Count - 1; i++)
			{
				var FieldName = Request.Form.Keys[i];
				if (FieldName.IndexOf("|") != -1 && ((FieldName.StartsWith("simple", StringComparison.InvariantCultureIgnoreCase) || FieldName.StartsWith("sizecolor", StringComparison.InvariantCultureIgnoreCase))))
				{
					var KeyVal = CommonLogic.FormCanBeDangerousContent(FieldName);
					var FieldNameSplit = FieldName.Split('|');
					var InventoryType = FieldNameSplit[0].ToLower(CultureInfo.InvariantCulture);
					var TheProductID = Localization.ParseUSInt(FieldNameSplit[1]);
					var TheVariantID = Localization.ParseUSInt(FieldNameSplit[2]);
					var Size = FieldNameSplit[3];
					var Color = FieldNameSplit[4];
					var inputVal = CommonLogic.FormUSInt(FieldName);
					
					if (InventoryType == "simple")
						DB.ExecuteSQL("update ProductVariant set Inventory=" + inputVal.ToString() + " where VariantID=" + TheVariantID.ToString());
					else
					{
						String sql = "select count(*) as N from Inventory  with (NOLOCK)  where VariantID=" + TheVariantID.ToString() + " and lower([size])=" + DB.SQuote(AppLogic.CleanSizeColorOption(Size).ToLowerInvariant()) + " and lower(color)=" + DB.SQuote(AppLogic.CleanSizeColorOption(Color).ToLowerInvariant());
						if (DB.GetSqlN(sql) == 0)
						{
							sql = "insert into Inventory(InventoryGUID,VariantID,[Size],Color,Quan) values(" + DB.SQuote(DB.GetNewGUID()) + "," + TheVariantID.ToString() + "," + DB.SQuote(AppLogic.CleanSizeColorOption(Size)) + "," + DB.SQuote(AppLogic.CleanSizeColorOption(Color)) + "," + inputVal.ToString() + ")";
							DB.ExecuteSQL(sql);
						}
						else
						{
							sql = "update Inventory set Quan=" + inputVal.ToString() + " where VariantID=" + TheVariantID.ToString() + " and lower([size])=" + DB.SQuote(AppLogic.CleanSizeColorOption(Size).ToLowerInvariant()) + " and lower(color)=" + DB.SQuote(AppLogic.CleanSizeColorOption(Color).ToLowerInvariant());
							DB.ExecuteSQL(sql);
						}
					}
				}
			}

			DB.ExecuteSQL("Update Inventory set Quan=0 where Quan<0"); // safety check
			DB.ExecuteSQL("Update ProductVariant set Inventory=0 where Inventory<0"); // safety check

			AlertMessageDisplay.PushAlertMessage("Inventory was updated", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);

			LoadBody(SelectedLocale.Name);
		}
	}
}
