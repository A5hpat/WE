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
	/// <summary>
	/// Summary description for search.
	/// </summary>
	public partial class search : AspDotNetStorefront.Admin.AdminPageBase
	{

		protected void Page_Load(Object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			StringBuilder contents = new StringBuilder();

			EntityHelper CategoryHelper = AppLogic.LookupHelper(null, "Category");
			EntityHelper SectionHelper = AppLogic.LookupHelper(null, "Section");
			EntityHelper ManufacturerHelper = AppLogic.LookupHelper(null, "Manufacturer");

			String searchTerm = CommonLogic.QueryStringCanBeDangerousContent("SearchTerm").Trim();
			if (searchTerm.Length != 0)
			{
				String stlike = "%" + searchTerm + "%";
				String stquoted = DB.SQuote(stlike);


				// MATCHING CATEGORIES:
				bool anyFound = false;

				contents.Append("<div class=\"white-ui-box\">\n");
				contents.Append("<a class=\"btn btn-default btn-sm pull-right\" href=\"entities.aspx?entityname=category\">" + AppLogic.GetString("admin.menu.Categories", SkinID, LocaleSetting) + "</a>\n");
				contents.Append("<h3>" + AppLogic.GetString("AppConfig.CategoryPromptPlural", SkinID, LocaleSetting) + " Matching: '" + searchTerm + "'</h3>\n");
				contents.Append("<table class=\"table\">\n");

				using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using (IDataReader rs = DB.GetRS(String.Format("select CategoryID from Category with (NOLOCK)  where Category.name like {0} order by DisplayOrder,Name", stquoted), con))
					{
						while (rs.Read())
						{
							contents.Append("<tr><td>" + CategoryHelper.GetEntityBreadcrumb(DB.RSFieldInt(rs, "CategoryID"), LocaleSetting) + "</td></tr>");
							anyFound = true;
						}
					}
				}

				if (!anyFound)
				{
					contents.Append("<tr><td>No matches found</td></tr>\n");
				}
				contents.Append("</table>\n");
				contents.Append("</div>\n");

				// MATCHING SECTIONS:
				anyFound = false;

				contents.Append("<div class=\"white-ui-box\">\n");
				contents.Append("<a class=\"btn btn-default btn-sm pull-right\" href=\"entities.aspx?entityname=section\">" + AppLogic.GetString("admin.menu.Sections", SkinID, LocaleSetting) + "</a>\n");
				contents.Append("<h3>" + AppLogic.GetString("AppConfig.SectionPromptPlural", SkinID, LocaleSetting) + " Matching: '" + searchTerm + "'</h3>\n");
				contents.Append("<table class=\"table\">\n");

				using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(IDataReader rs = DB.GetRS(String.Format("select SectionID from [Section]  with (NOLOCK)  where Name like {0} order by DisplayOrder,Name", stquoted), con))
					{
						while (rs.Read())
						{
							contents.Append("<tr><td>" + SectionHelper.GetEntityBreadcrumb(DB.RSFieldInt(rs, "SectionID"), LocaleSetting) + "</td></tr>");
							anyFound = true;
						}
					}
				}

				if (!anyFound)
				{
					contents.Append("<tr><td>No matches found</td></tr>\n");
				}
				contents.Append("</table>\n");
				contents.Append("</div>\n");

				// MATCHING MANUFACTURERS:
				anyFound = false;

				contents.Append("<div class=\"white-ui-box\">\n");
				contents.Append("<a class=\"btn btn-default btn-sm pull-right\" href=\"entities.aspx?entityname=manufacturer\">" + AppLogic.GetString("admin.menu.Manufacturers", SkinID, LocaleSetting) + "</a>\n");
				contents.Append("<h3>" + AppLogic.GetString("AppConfig.ManufacturerPromptPlural", SkinID, LocaleSetting) + " Matching: '" + searchTerm + "'</h3>\n");
				contents.Append("<table class=\"table\">\n");

				using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using(IDataReader rs = DB.GetRS(String.Format("select Name, ManufacturerID from Manufacturer  with (NOLOCK)  where Name like {0} order by DisplayOrder,Name", stquoted), con))
					{
						while (rs.Read())
						{
							contents.Append("<tr><td><a href=\"entity.aspx?entityname=manufacturer&entityid=" + DB.RSFieldInt(rs, "ManufacturerID").ToString() + "\">" + CommonLogic.HighlightTerm(DB.RSFieldByLocale(rs, "Name", LocaleSetting), searchTerm) + "</a></td></tr>\n");
							anyFound = true;
						}
					}
				}

				if (!anyFound)
				{
					contents.Append("<tr><td>No matches found</td></tr>\n");
				}
				contents.Append("</table>\n");
				contents.Append("</div>\n");
				litEntityResults.Text = contents.ToString();

				contents.Clear();
				// MATCHING PRODUCTS:
				ProductCollection products = new ProductCollection();
				products.PageSize = 0;
				products.PageNum = 1;
				products.SearchMatch = searchTerm;
				products.SearchDescriptionAndSummaryFields = false;
				products.PublishedOnly = false;
				DataSet dsProducts = products.LoadFromDB();
				int NumProducts = products.NumProducts;

				contents.Append("<div class=\"white-ui-box\">\n");
				contents.Append("<a class=\"btn btn-default btn-sm pull-right\" href=\"products.aspx\">" + AppLogic.GetString("admin.menu.ProductMgr", SkinID, LocaleSetting) + "</a>\n");
				contents.Append("<h3>" + AppLogic.GetString("admin.common.products", SkinID, LocaleSetting) + " Matching: '" + searchTerm + "'</h3>\n");
				contents.Append("<table class=\"table\">\n");
				if (NumProducts > 0)
				{
					contents.Append("    <tr>\n");
					contents.Append("      <th>" + AppLogic.GetString("search.aspx.6", SkinID, LocaleSetting) + "</th>\n");
					contents.Append("      <th>" + AppLogic.GetString("search.aspx.7", SkinID, LocaleSetting) + "</th>\n");
					contents.Append("      <th>" + AppLogic.GetString("AppConfig.CategoryPromptSingular", SkinID, LocaleSetting) + "</th>\n");
					contents.Append("      <th>" + AppLogic.GetString("search.aspx.8", SkinID, LocaleSetting) + "</th>\n");
					contents.Append("    </tr>\n");
					foreach (DataRow row in dsProducts.Tables[0].Rows)
					{
						String url = String.Format("product.aspx?productid={0}&entityname=category", DB.RowFieldInt(row, "ProductID"));
						contents.Append("<tr>");
						contents.Append("<td>");
						contents.Append("<a href=\"" + url + "\">" + AppLogic.MakeProperObjectName(DB.RowFieldByLocale(row, "Name", LocaleSetting), DB.RowFieldByLocale(row, "VariantName", LocaleSetting), LocaleSetting) + "</a>");
						// QuickEdit
						contents.Append("</td>");
						contents.Append("<td>" + CommonLogic.HighlightTerm(AppLogic.MakeProperProductSKU(DB.RowField(row, "SKU"), DB.RowField(row, "SKUSuffix"), "", ""), searchTerm) + "</td>");
						String Cats = CategoryHelper.GetObjectEntities(DB.RowFieldInt(row, "ProductID"), false);
						if (Cats.Length != 0)
						{
							String[] CatIDs = Cats.Split(',');
							contents.Append("<td>");
							bool firstCat = true;
							foreach (String s in CatIDs)
							{
								if (!firstCat)
								{
									contents.Append(", ");
								}
								contents.Append("<a href=\"entity.aspx?entityname=category&entityid=" + s + "\">" + CategoryHelper.GetEntityName(Localization.ParseUSInt(s), LocaleSetting).Trim() + "</a>");
								firstCat = false;
							}
							contents.Append("</td>\n");
						}
						else
						{
							contents.Append("<td>");
							contents.Append("&nbsp;");
							contents.Append("</td>\n");
						}
						contents.Append("<td><a href=\"entity.aspx?entityname=manufacturer&entityid=" + DB.RowFieldInt(row, "ManufacturerID").ToString() + "\">" + CommonLogic.HighlightTerm(ManufacturerHelper.GetEntityName(DB.RowFieldInt(row, "ManufacturerID"), LocaleSetting), searchTerm) + "</a></td>");
						contents.Append("</tr>\n");
						anyFound = true;
					}
				}
				else
				{
					contents.Append("<tr><td colspan=\"4\">No matches found</td></tr>\n");

				}
				contents.Append("</table>\n");
				contents.Append("</div>\n");
				products.Dispose();
				dsProducts.Dispose();

				litProductResults.Text = contents.ToString();
			}
		}

	}
}
