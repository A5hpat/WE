// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class Reports : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected Customer Customer;
		protected List<SqlParameter> Parameters = new List<SqlParameter>();
		protected bool IsMultiStore = false;

		protected void Page_Load(object sender, EventArgs e)
		{
			//Do we need to show the store selector?
			var storeList = Store.GetStoreList();
			IsMultiStore = storeList.Count > 1;

			Customer = Context.GetCustomer();

			if(!IsPostBack)
			{
				dateStart.SelectedDate = DateTime.Now.AddMonths(-6);

				dateEnd.SelectedDate = DateTime.Now;

				dateStart.Culture = Thread.CurrentThread.CurrentUICulture;
				dateEnd.Culture = Thread.CurrentThread.CurrentUICulture;
				phReportResults.Visible = false;
				pnlSummary.Visible = false;

				using(var dbc = new SqlConnection(DB.GetDBConn()))
				{
					dbc.Open();
					using(var rsd = DB.GetRS("select name from customreport with (NOLOCK)", dbc))
					{
						while(rsd.Read())
						{
							string name = DB.RSField(rsd, "name");
							ListItem li = new ListItem(name, "vBuiltIn" + name);
							ddCustomReportType.Items.Add(li);
						}
					}
				}
				BuildSummaryReport();

				PopulateAffiliateDropdown();
				PopulateCustomerLevelDropdown();
			}
		}
		
		void InitCurrentReport()
		{
			//Show the Store selector if multiple stores exist and it's not a custom report
			if(IsMultiStore && !ddReportType.SelectedValue.Contains("vBuiltIn"))
			{
				pnlStores.Visible = true;
				CheckStoreCount();
				ToggleStoreFilter();
			}

			if(ddReportType.SelectedIndex == 0)
			{
				HideFilters();
				return;
			}

			//Report Step 2 - Show all of the spec panels your report uses, if any.
			switch(ddReportType.SelectedValue.ToLowerInvariant())
			{
				case "abandonedcart":
					litReportDescription.Text = AppLogic.GetString("admin.reports.abandonedcartdescription", ThisCustomer.LocaleSetting);
					pnlDateSpecs.Visible = true;
					break;
				case "bestsellers":
					litReportDescription.Text = AppLogic.GetString("admin.reports.top10description", ThisCustomer.LocaleSetting);
					pnlDateSpecs.Visible = true;
					break;
				case "promotions":
					litReportDescription.Text = AppLogic.GetString("admin.reports.promotionusagedescription", ThisCustomer.LocaleSetting);
					pnlDateSpecs.Visible = true;
					break;
				case "referrals":
					litReportDescription.Text = AppLogic.GetString("admin.reports.referralsdescription", ThisCustomer.LocaleSetting);
					pnlDateSpecs.Visible = true;
					break;
				case "ordersbydaterange":
					litReportDescription.Text = AppLogic.GetString("admin.reports.ordersbydaterangedescription", ThisCustomer.LocaleSetting);
					pnlDateSpecs.Visible = true;
					break;
				case "ordersbyitem":
					litReportDescription.Text = AppLogic.GetString("admin.reports.ordersbyitemdescription", ThisCustomer.LocaleSetting);
					pnlDateSpecs.Visible = true;
					break;
				case "customers":
					litReportDescription.Text = AppLogic.GetString("admin.reports.customersdescription", ThisCustomer.LocaleSetting);
					pnlStores.Visible = false;
					break;
				case "affiliates":
					litReportDescription.Text = AppLogic.GetString("admin.reports.affiliatesdescription", ThisCustomer.LocaleSetting);
					pnlDateSpecs.Visible = true;
					pnlAffiliates.Visible = true;
					break;
				case "customersbyproduct":
					litReportDescription.Text = AppLogic.GetString("admin.reports.customerswhoboughtdescription", ThisCustomer.LocaleSetting);
					pnlProductId.Visible = true;
					pnlDateSpecs.Visible = true;
					break;
				case "emptyentities":
					litReportDescription.Text = AppLogic.GetString("admin.reports.emptyentitiesdescription", ThisCustomer.LocaleSetting);
					pnlEntityTypes.Visible = true;
					break;
				case "ordersbyentity":
					litReportDescription.Text = AppLogic.GetString("admin.reports.ordersbyentitydescription", ThisCustomer.LocaleSetting);
					PopulateEntityDropdowns(); // Don't build these lists unless the option is chosen, as this may be slow!
					pnlDateSpecs.Visible = true;
					pnlEntities.Visible = true;
					break;
				case "inventorylevels":
					litReportDescription.Text = AppLogic.GetString("admin.reports.inventorylevelsdescription", ThisCustomer.LocaleSetting);
					pnlQuantity.Visible = true;
					break;
				case "unmappedproducts":
					litReportDescription.Text = AppLogic.GetString("admin.reports.unmappedproductsdescription", ThisCustomer.LocaleSetting);
					pnlEntityTypes.Visible = true;
					break;
				case "productsbycustomerlevel":
					litReportDescription.Text = AppLogic.GetString("admin.reports.customerlevelproductsdescription", ThisCustomer.LocaleSetting);
					pnlCustomerLevels.Visible = true;
					break;
				case "currentrecurring":
					litReportDescription.Text = AppLogic.GetString("admin.reports.currentrecurringdescription", ThisCustomer.LocaleSetting);
					break;
				case "sename":
					litReportDescription.Text = AppLogic.GetString("admin.reports.senamedescription", ThisCustomer.LocaleSetting);
					break;
				case "emptysku":
					litReportDescription.Text = AppLogic.GetString("admin.reports.emptyskudescription", ThisCustomer.LocaleSetting);
					break;
				case "orderstats":
					litReportDescription.Text = AppLogic.GetString("admin.reports.orderstatsdescription", ThisCustomer.LocaleSetting);
					pnlDateSpecs.Visible = true;
					break;
			}

			btnReport.Visible = true;
		}

		void BuildReport()
		{
			if(ddCustomReportType.SelectedValue.Contains("vBuiltIn"))
			{
				var reportname = ddCustomReportType.SelectedValue.Replace("vBuiltIn", "");
				BuildCustomReport(reportname);
				return;
			}

			if(IsMultiStore)
				Parameters.Add(new SqlParameter("@StoreID", ssOne.SelectedIndex));
			else
				Parameters.Add(new SqlParameter("@StoreID", "0"));

			//Report Step 3 - Add your report's method to the list.
			switch(ddReportType.SelectedValue.ToLowerInvariant())
			{
				case "abandonedcart":
					BuildAbandonedCartReport();
					break;
				case "bestsellers":
					BuildBestSellersReport();
					break;
				case "promotions":
					BuildPromotionReport();
					break;
				case "referrals":
					BuildReferralsReport();
					break;
				case "ordersbydaterange":
					BuildOrdersByDateRangeReport();
					break;
				case "ordersbyitem":
					BuildOrdersByItemReport();
					break;
				case "customers":
					BuildCustomerReport();
					break;
				case "affiliates":
					BuildAffiliatesReport();
					break;
				case "customersbyproduct":
					BuildCustomersByProductReport();
					break;
				case "emptyentities":
					BuildEmptyEntitiesReport();
					break;
				case "ordersbyentity":
					BuildOrdersByEntityReport();
					break;
				case "inventorylevels":
					BuildInventoryReport();
					break;
				case "unmappedproducts":
					BuildUnmappedProductsReport();
					break;
				case "productsbycustomerlevel":
					BuildProductsByCustomerLevelReport();
					break;
				case "currentrecurring":
					BuildCurrentRecurringProductsReport();
					break;
				case "sename":
					BuildSENamesReport();
					break;
				case "emptysku":
					BuildEmptySkuReport();
					break;
				case "orderstats":
					BuildOrderStatsReport();
					break;
			}
		}

		void BuildReportFromSql(string sql)
		{
			var rTable = new HtmlTable();
			rTable.CellSpacing = 1;
			rTable.ID = "ReportResultsTable";
			rTable.Attributes["class"] = "table report-table";
			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var reader = DB.GetRS(sql, Parameters.ToArray(), dbconn))
				{
					var headerRow = new HtmlTableRow();
					for(int i = 0; i < reader.FieldCount; i++)
					{
						var header = new HtmlTableCell();
						header.InnerText = reader.GetName(i);
						headerRow.Cells.Add(header);
						headerRow.Attributes["class"] = "white-box-heading report-header";
					}
					rTable.Rows.Add(headerRow);
					while(reader.Read())
					{
						var row = new HtmlTableRow();
						for(int i = 0; i < reader.FieldCount; i++)
						{
							var cell = new HtmlTableCell();
							var cellText = XmlCommon.GetLocaleEntry(reader[i].ToString(), ThisCustomer.LocaleSetting, true);

							if(!string.IsNullOrEmpty(cellText) && reader.GetFieldType(i) == typeof(decimal))
								cellText = decimal.Parse(cellText).ToString("0.00");

							cell.InnerText = cellText;

							if(i % 2 == 0)
								cell.Attributes["class"] = "td-alt";
							row.Cells.Add(cell);
						}
						rTable.Rows.Add(row);
					}
				}
			}

			phReportResults.Controls.Add(rTable);
			phReportResults.Visible = true;
			btnSaveReport.Visible = true;
		}

		void ToggleStoreFilter()
		{
			if(ddEntity.SelectedValue == "Stores" && ddReportType.SelectedValue.ToLowerInvariant() == "unmappedproducts")
			{
				pnlStores.Visible = false;
			}
			else
			{
				pnlStores.Visible = true;
			}
		}

		void PopulateAffiliateDropdown()
		{
			using(var dbc = new SqlConnection(DB.GetDBConn()))
			{
				dbc.Open();
				using(var rsd = DB.GetRS("SELECT Name, AffiliateId FROM Affiliate WITH (NOLOCK) ORDER BY Name", dbc))
				{
					while(rsd.Read())
					{
						var name = DB.RSField(rsd, "Name");
						var Id = DB.RSFieldInt(rsd, "AffiliateId");
						var listItem = new ListItem(name, Id.ToString());

						ddAffiliates.Items.Add(listItem);
					}
				}
			}
		}

		void PopulateEntityDropdowns()
		{
			if(ddCategories.Items.Count == 0)
			{
				ddCategories.Items.Clear();
				ddCategories.Items.Add(new ListItem("-- None --", "0"));
				using(var dbc = new SqlConnection(DB.GetDBConn()))
				{
					dbc.Open();
					using(var rsd = DB.GetRS("SELECT Name, CategoryID FROM Category WITH (NOLOCK) ORDER BY Name", dbc))
					{
						while(rsd.Read())
						{
							var name = DB.RSFieldByLocale(rsd, "Name", ThisCustomer.LocaleSetting);
							var Id = DB.RSFieldInt(rsd, "CategoryID");
							var listItem = new ListItem(name, Id.ToString());

							ddCategories.Items.Add(listItem);
						}
					}
				}
			}

			if(ddManufacturers.Items.Count == 0)
			{
				ddManufacturers.Items.Clear();
				ddManufacturers.Items.Add(new ListItem("-- None --", "0"));
				using(var dbc = new SqlConnection(DB.GetDBConn()))
				{
					dbc.Open();
					using(var rsd = DB.GetRS("SELECT Name, ManufacturerID FROM Manufacturer WITH (NOLOCK) ORDER BY Name", dbc))
					{
						while(rsd.Read())
						{
							var name = DB.RSFieldByLocale(rsd, "Name", ThisCustomer.LocaleSetting);
							var Id = DB.RSFieldInt(rsd, "ManufacturerID");
							var listItem = new ListItem(name, Id.ToString());

							ddManufacturers.Items.Add(listItem);
						}
					}
				}
			}

			if(ddSections.Items.Count == 0)
			{
				ddSections.Items.Clear();
				ddSections.Items.Add(new ListItem("-- None --", "0"));
				using(var dbc = new SqlConnection(DB.GetDBConn()))
				{
					dbc.Open();
					using(var rsd = DB.GetRS("SELECT Name, SectionID FROM Section WITH (NOLOCK) ORDER BY Name", dbc))
					{
						while(rsd.Read())
						{
							var name = DB.RSFieldByLocale(rsd, "Name", ThisCustomer.LocaleSetting);
							var Id = DB.RSFieldInt(rsd, "SectionID");
							var listItem = new ListItem(name, Id.ToString());

							ddSections.Items.Add(listItem);
						}
					}
				}
			}
		}

		protected void PopulateDateParameters()
		{
			//Some checks to make sure we don't get bad dates
			dateStart.MinDate = (DateTime)SqlDateTime.MinValue;
			dateStart.MaxDate = (DateTime)SqlDateTime.MaxValue;

			dateEnd.MinDate = (DateTime)SqlDateTime.MinValue;
			dateEnd.MaxDate = (DateTime)SqlDateTime.MaxValue;

			if(dateStart.SelectedDate == null)
				dateStart.SelectedDate = DateTime.Now.AddDays(-30);
			if(dateEnd.SelectedDate == null)
				dateEnd.SelectedDate = DateTime.Now;

			var startDate = DateTime.Now;
			var endDate = DateTime.Now;

			switch(rblRange.SelectedValue)
			{
				case "0":
					{
						if(dateStart.SelectedDate > dateEnd.SelectedDate) //Flip them
						{
							endDate = (DateTime)dateStart.SelectedDate;
							dateStart.SelectedDate = dateEnd.SelectedDate;
							dateEnd.SelectedDate = endDate;
						}

						startDate = (DateTime)dateStart.SelectedDate;
						endDate = (DateTime)dateEnd.SelectedDate;

						break;
					}
				case "1":
					{
						startDate = DateTime.Today;
						endDate = startDate.AddDays(1);
						break;
					}
				case "2":
					{
						startDate = DateTime.Today.AddDays(-1);
						endDate = startDate;
						break;
					}
				case "3":
					{
						startDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek));
						endDate = startDate.AddDays(6);
						break;
					}
				case "4":
					{
						startDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek) - 7);
						endDate = startDate.AddDays(6);
						break;
					}
				case "5":
					{
						startDate = DateTime.Today.AddDays(1 - DateTime.Today.Day);
						endDate = startDate.AddMonths(1);
						break;
					}
				case "6":
					{
						startDate = DateTime.Today.AddMonths(-1);
						startDate = startDate.AddDays(1 - startDate.Day);
						endDate = startDate.AddMonths(1);
						break;
					}
				case "7":
					{
						startDate = DateTime.Today.AddMonths(1 - DateTime.Today.Month);
						startDate = startDate.AddDays(1 - startDate.Day);
						endDate = startDate.AddYears(1);
						break;
					}
				case "8":
					{
						startDate = DateTime.Today.AddYears(-1);
						startDate = startDate.AddMonths(1 - startDate.Month);
						startDate = startDate.AddDays(1 - startDate.Day);
						endDate = startDate.AddYears(1);
						break;
					}
			}

			if(startDate >= SqlDateTime.MinValue.Value && startDate <= SqlDateTime.MaxValue.Value)
				Parameters.Add(new SqlParameter("@StartDate", Localization.ToDBShortDateString(startDate)));

			if(endDate >= SqlDateTime.MinValue.Value && endDate == SqlDateTime.MaxValue.Value)
			{
				Parameters.Add(new SqlParameter("@EndDate", Localization.ToDBShortDateString(endDate)));
			}
			else if(endDate >= SqlDateTime.MinValue.Value && endDate < SqlDateTime.MaxValue.Value)
			{
				Parameters.Add(new SqlParameter("@EndDate", Localization.ToDBShortDateString(endDate.AddDays(1))));
			}
		}

		void PopulateCustomerLevelDropdown()
		{
			using(var dbc = new SqlConnection(DB.GetDBConn()))
			{
				dbc.Open();
				using(var rsd = DB.GetRS("Select CustomerLevelID, Name FROM CustomerLevel WITH (NOLOCK) ORDER BY Name", dbc))
				{
					while(rsd.Read())
					{
						var name = DB.RSFieldByLocale(rsd, "Name", ThisCustomer.LocaleSetting);
						var Id = DB.RSFieldInt(rsd, "CustomerLevelID");
						var listItem = new ListItem(name, Id.ToString());

						ddCustomerLevel.Items.Add(listItem);
					}
				}
			}
		}

		void HideFilters()
		{
			//Input Parameter Step 2 - add your panel to the global hide logic (for report switching). 
			pnlEntityTypes.Visible = false;
			pnlAffiliates.Visible = false;
			pnlDateSpecs.Visible = false;
			pnlSummary.Visible = false;
			btnSaveReport.Visible = false;
			pnlStores.Visible = false;
			pnlEntities.Visible = false;
			pnlQuantity.Visible = false;
			pnlProductId.Visible = false;
			pnlCustomerLevels.Visible = false;
			litReportDescription.Text = String.Empty;
		}
	
		protected void ddReportType_SelectedIndexChanged(object sender, EventArgs e)
		{
			ddCustomReportType.SelectedIndex = 0;
			btnCustomReport.Visible = false;
			btnReport.Visible = ddReportType.SelectedIndex != 0;

			HideFilters();
			InitCurrentReport();
		}

		protected void ddEntity_SelectedIndexChanged(object sender, EventArgs e)
		{
			ToggleStoreFilter();
		}

		protected void ddCustomReportType_Clicked(object sender, EventArgs e)
		{
			ddReportType.SelectedIndex = 0;
			btnReport.Visible = false;

			HideFilters();
			btnCustomReport.Visible = ddCustomReportType.SelectedIndex != 0;
		}

		protected void BtnReport_Click(object sender, EventArgs e)
		{
			BuildReport();
		}

		protected void BtnSaveReport_click(object sender, EventArgs e)
		{
			BuildReport();

			try
			{
				var tableStream = new MemoryStream();
				var tableWriter = new StreamWriter(tableStream, System.Text.Encoding.UTF8);

				using(var htmlWriter = new HtmlTextWriter(tableWriter))
				{
					tableWriter.Write("<root>");
					phReportResults.RenderControl(htmlWriter);
					tableWriter.Write("</root>");
				}
				tableWriter.Flush();

				var filepath = CommonLogic.SafeMapPath("~/images") + "\\";
				var filename = "ReportExport_" + Localization.ToNativeDateTimeString(System.DateTime.Now).Replace(" ", "_").Replace(",", "_").Replace("/", "_").Replace(":", "_").Replace(".", "_");
				filename += ".csv";

				tableStream.Position = 0;
				var reader = new StreamReader(tableStream, System.Text.Encoding.UTF8);

				var xdoc = new XmlDocument();
				xdoc.Load(reader);

				var FullFilePath = filepath + filename;
				var xsl = new XslCompiledTransform();

				xsl.Load(CommonLogic.SafeMapPath(string.Format("{0}/XmlPackages/ReportExportCSV.xslt", AppLogic.AdminDir())));

				using(var sw = new StreamWriter(FullFilePath, false, System.Text.Encoding.UTF8))
					xsl.Transform(xdoc, null, sw);

				Response.Clear();
				Response.ClearHeaders();
				Response.ClearContent();
				Response.AddHeader("content-disposition", "attachment; filename=" + filename);
				Response.BufferOutput = false;

				// Send the CSV
				Response.BufferOutput = false;
				Response.ContentType = "text/csv";
				Response.TransmitFile(FullFilePath);
				Response.Flush();
				Response.End();
			}
			catch(ThreadAbortException)
			{
				throw;
			}
			catch(Exception exception)
			{
				ctrlAlertMessage.PushAlertMessage(exception.Message, AlertMessage.AlertType.Error);
			}
		}

		void BuildCustomReport(string reportname)
		{
			Parameters.Add(new SqlParameter("@ReportName", reportname));

			using(var dbc = new SqlConnection(DB.GetDBConn()))
			{
				dbc.Open();
				using(var rsd = DB.GetRS("select sqlcommand from customreport with (NOLOCK) where name = @ReportName", Parameters.ToArray(), dbc))
				{
					Parameters.Clear();
					if(rsd.Read())
						BuildReportFromSql(DB.RSField(rsd, "sqlcommand"));
				}
			}
		}

		//Report Step 4 - Build your report function(s). 
		void CheckStoreCount()
		{
			if(!IsMultiStore || ddReportType.SelectedValue.ToLowerInvariant() != "unmappedproducts")
			{
				ddEntity.Items.Remove(AppLogic.GetString("admin.reports.stores", "en-US"));
			}
			else
			{
				if(!ddEntity.Items.Contains(new ListItem("Stores")))
				{
					ddEntity.Items.Add(new ListItem(AppLogic.GetString("admin.reports.stores", "en-US"), AppLogic.GetString("admin.reports.stores", "en-US")));
				}
			}
		}

		void BuildAbandonedCartReport()
		{
			var sql = "Select c.Email "
									+ ",c.FirstName "
									+ ",c.LastName "
									+ ",c.Phone "
									+ ",Case When c.OkToEmail = 0 Then 'No' Else 'Yes' End [OkToEmail] "
									+ ",sc.[CustomerID] "
									+ ",sc.[ProductSKU] "
									+ ",sc.[ProductPrice] "
									+ ",sc.[ProductID] "
									+ ",p.Name [ProductName] "
									+ ",sc.[VariantID] "
									+ ",pv.Name [VariantName] "
									+ ",sc.[Quantity] "
									+ ",sc.[CreatedOn] [CartCreatedOn] "
									+ ",sc.[TextOption] "
									+ ",sc.[BillingAddressID] "
									+ ",sc.[ShippingAddressID] "
									+ ",sc.[Notes] "
									+ ",sc.[CustomerEntersPrice] "
									+ ",ba.FirstName [BillingFirstName] "
									+ ",ba.LastName [BillingLastName] "
									+ ",ba.Company [BillingCompany] "
									+ ",ba.Address1 [BillingAddress1] "
									+ ",ba.Address2 [BillingAddress2] "
									+ ",ba.Suite [BillingSuite] "
									+ ",ba.City [BillingCity] "
									+ ",ba.[State] [BillingState] "
									+ ",ba.[Zip] [BillingZip] "
									+ ",ba.Phone [BillingPhone] "
									+ ",ba.Email [BillingEmail] "
									+ ",sa.FirstName [ShippingFirstName] "
									+ ",sa.LastName [ShippingLastName] "
									+ ",sa.Company [ShippingCompany] "
									+ ",sa.Address1 [ShippingAddress1] "
									+ ",sa.Address2 [ShippingAddress2] "
									+ ",sa.Suite [ShippingSuite] "
									+ ",sa.City [ShippingCity] "
									+ ",sa.[State] [ShippingState] "
									+ ",sa.[Zip] [ShippingZip] "
									+ ",sa.Phone [ShippingPhone] "
									+ ",sa.Email [ShippingEmail] "
							+ "From ShoppingCart sc "
							+ "Inner Join [Customer] c On c.CustomerID = sc.CustomerID "
							+ "Inner Join Product p On p.ProductId = sc.ProductID "
							+ "Inner Join ProductVariant pv On pv.VariantId = sc.VariantId "
							+ "Left Join [Address] ba On ba.AddressID = sc.BillingAddressID "
							+ "Left Join [Address] sa On sa.AddressID = sc.ShippingAddressID "
							+ "Where sc.CartType = 0 "
							+ "and sc.CreatedOn >= @StartDate and sc.CreatedOn <= @EndDate "
							+ "and (sc.StoreID = @StoreID OR 0 = @StoreID) "
							+ "Order By sc.CreatedOn Desc, c.Email, c.CustomerID";

			PopulateDateParameters();
			BuildReportFromSql(sql);
		}

		void BuildInventoryReport()
		{
			Parameters.Add(new SqlParameter("@InventoryLevel", txtQuantity.Text.Trim()));

			var sql = "SELECT p.ProductID, p.Name AS 'Product Name', "
					+ "pv.Name AS 'Variant Name', "
					+ "CASE p.TrackInventoryBySizeAndColor "
						+ "WHEN 1 THEN i.Size "
						+ "ELSE '-' "
					+ "END AS Size, "
					+ "CASE p.TrackInventoryBySizeAndColor "
						+ "WHEN 1 THEN i.Color "
						+ "ELSE '-' "
					+ "END AS Color, "
					+ "CASE p.TrackInventoryBySizeAndColor "
						+ "WHEN 1 THEN ISNULL(i.Quan, 0) "
						+ "ELSE pv.Inventory "
					+ "END AS 'On Hand', "
					+ "CASE p.Published "
						+ "WHEN 1 THEN 'Yes' "
						+ "ELSE 'No' "
					+ "END AS 'Published?' "
					+ "FROM Product p "
					+ "LEFT JOIN ProductVariant pv ON p.ProductID = pv.ProductID "
					+ "LEFT JOIN Inventory i ON pv.VariantID = i.VariantID "
					+ "LEFT JOIN ProductStore ps ON p.ProductID = ps.ProductID "
					+ "WHERE (ISNULL(i.Quan, @InventoryLevel - 1) < @InventoryLevel) AND pv.Inventory < @InventoryLevel "
					+ "AND (ps.StoreID = @StoreId OR 0 = @StoreId)";

			BuildReportFromSql(sql);
		}

		void BuildCustomerReport()
		{
			var sql = "SELECT c.Email, "
							+ "c.FirstName, "
							+ "c.LastName, "
							+ "CASE c.OkToEmail "
							   + " WHEN 1 THEN 'Yes' "
								+ "ELSE 'No' "
							+ "END AS 'Ok to Email?', "
							+ "CASE c.IsRegistered "
								+ "WHEN 1 THEN 'Registered' "
								+ "ELSE 'Anonymous' "
							+ "END AS 'Registered?', "
							+ "c.CreatedOn AS 'First Visit', "
							+ "c.StoreId, "
							+ "a.FirstName AS 'Shipping First Name', "
							+ "a.LastName AS 'Shipping Last Name', "
							+ "a.Address1, "
							+ "a.Address2, "
							+ "a.Suite, "
							+ "a.City, "
							+ "a.State, "
							+ "a.Zip, "
							+ "a.Country, "
							+ "c.Phone "
						+ "FROM Customer c "
							+ "LEFT JOIN Address a ON c.ShippingAddressId = a.AddressId ";

			sql += " ORDER BY c.CreatedOn DESC";

			BuildReportFromSql(sql);
		}

		void BuildPromotionReport()
		{
			var sql = "SELECT p.Name, SUM(CAST(pu.Complete AS Integer)) AS 'Times Used', "
				+ "-SUM(pu.ShippingDiscountAmount) AS 'Shipping Discounts Given', "
				+ "-SUM(pu.LineItemDiscountAmount) AS 'Line Item Discounts Given', "
				+ "-SUM(pu.OrderDiscountAmount) AS 'Order Level Discounts Given', "
				+ "CASE p.Active WHEN 1 THEN 'Yes' ELSE 'No' END AS 'Still Active?' "
				+ "FROM Promotions p LEFT JOIN PromotionUsage pu ON P.Id = pu.PromotionId "
				+ "LEFT JOIN Orders o ON o.OrderNumber = pu.OrderId "
				+ "WHERE pu.Complete IS NOT NULL "
					+ "AND pu.Complete > 0 "
					+ "AND pu.DateApplied >= @StartDate "
					+ "AND pu.DateApplied <= @EndDate "
					+ "AND (o.StoreId = @StoreId OR 0 = @StoreId) "
				+ "GROUP BY p.Name, p.Active";

			PopulateDateParameters();
			BuildReportFromSql(sql);
		}

		void BuildUnmappedProductsReport()
		{
			var sql = string.Empty;
			switch(ddEntity.SelectedValue)
			{
				case "Stores":
					{
						sql = "SELECT p.Name, p.ProductID, "
								+ "CASE p.Published WHEN 1 THEN 'Yes' ELSE 'No' END AS 'Published?' "
								+ "FROM Product p "
								+ "WHERE NOT EXISTS "
								+ "(SELECT ProductID FROM ProductStore ps WHERE p.ProductID = ps.ProductID) AND p.IsSystem != 1";

						break;
					}
				default:
					{
						sql = "SELECT p.Name, p.ProductID, CASE p.Published WHEN 1 THEN 'Yes' ELSE 'No' END AS 'Published?' "
					+ "FROM Product p LEFT JOIN ProductStore ps ON p.ProductID = ps.ProductID "
					+ "WHERE p.ProductID NOT IN ";

						switch(ddEntity.SelectedValue)
						{
							case "Category":
								{
									sql += "(SELECT DISTINCT ProductID FROM ProductCategory) ";
									break;
								}
							case "Section":
								{
									sql += "(SELECT DISTINCT ProductID FROM ProductSection) ";
									break;
								}
							case "Manufacturer":
								{
									sql += "(SELECT DISTINCT ProductID FROM ProductManufacturer) ";
									break;
								}
						}
						sql += "AND p.IsSystem != 1 "
								+ "AND (ps.StoreId = @StoreId OR 0 = @StoreId)";
						break;
					}
			}
			BuildReportFromSql(sql);
		}

		void BuildEmptyEntitiesReport()
		{
			var sql = String.Empty;

			switch(ddEntity.SelectedValue)
			{
				case "Category":
					{
						sql = "SELECT Name, CategoryID FROM Category WHERE CategoryID NOT IN (SELECT DISTINCT CategoryID FROM ProductCategory)";
						break;
					}
				case "Manufacturer":
					{
						sql = "SELECT Name, ManufacturerID FROM Manufacturer WHERE ManufacturerID NOT IN (SELECT DISTINCT ManufacturerID FROM ProductManufacturer)";
						break;
					}
				case "Section":
					{
						sql = "SELECT Name, SectionID FROM Section WHERE SectionID NOT IN (SELECT DISTINCT SectionID FROM ProductSection)";
						break;
					}
			}

			BuildReportFromSql(sql);
		}

		void BuildAffiliatesReport()
		{
			Parameters.Add(new SqlParameter("@AffiliateID", ddAffiliates.SelectedValue));

			var sql = "SELECT o.OrderNumber, o.OrderDate, o.OrderTotal "
				+ "FROM Orders o LEFT JOIN Affiliate a ON o.AffiliateID = a.AffiliateID "
				+ "WHERE a.AffiliateID = @AffiliateID "
				+ "AND o.OrderDate >= @StartDate "
				+ "AND o.OrderDate <= @EndDate "
				+ "AND (o.StoreId = @StoreId OR 0 = @StoreId)";

			PopulateDateParameters();
			BuildReportFromSql(sql);
		}

		void BuildReferralsReport()
		{
			var sql = "SELECT OrderNumber, Referrer, OrderDate FROM Orders "
				+ "WHERE Referrer IS NOT Null "
				+ "AND CAST(Referrer AS nvarchar(max)) != '' "
				+ "AND OrderDate >= @StartDate "
				+ "AND OrderDate <= @EndDate "
				+ "AND (StoreID = @StoreId OR 0 = @StoreId)";

			PopulateDateParameters();
			BuildReportFromSql(sql);
		}

		void BuildOrdersByEntityReport()
		{
			string entityClause;
			if(ddCategories.SelectedValue != "0")
			{
				Parameters.Add(new SqlParameter("@CategoryID", ddCategories.SelectedValue));
				entityClause = "LEFT JOIN ProductCategory pc ON p.ProductID = pc.ProductId WHERE pc.CategoryID=@CategoryID ";
			}
			else if(ddManufacturers.SelectedValue != "0")
			{
				Parameters.Add(new SqlParameter("@ManufacturerID", ddManufacturers.SelectedValue));
				entityClause = "LEFT JOIN ProductManufacturer pf ON p.ProductID = pf.ProductId WHERE pf.ManufacturerID=@ManufacturerID ";
			}
			else if(ddSections.SelectedValue != "0")
			{
				Parameters.Add(new SqlParameter("@SectionID", ddSections.SelectedValue));
				entityClause = "LEFT JOIN ProductSection ps ON p.ProductID = ps.ProductId WHERE ps.SectionID=@SectionID ";
			}
			else
			{
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.reports.entityerror", Customer.LocaleSetting), AlertMessage.AlertType.Error);
				return;
			}

			var sql = "SELECT os.OrderedProductName AS 'Product Name', "
				+ "os.OrderedProductVariantName AS 'Variant Name', "
				+ "o.OrderNumber, "
				+ "os.Quantity, "
				+ "o.OrderDate AS 'Order Date' "
				+ "FROM Orders o "
				+ "LEFT JOIN Orders_ShoppingCart os ON o.OrderNumber = os.OrderNumber "
				+ "LEFT JOIN Product p ON os.ProductId = p.ProductID "
				+ entityClause
				+ "AND o.OrderDate >= @StartDate AND o.OrderDate <= @EndDate "
				+ "AND (o.StoreId = @StoreId OR 0 = @StoreId) "
				+ "ORDER BY os.ProductID";

			PopulateDateParameters();
			BuildReportFromSql(sql);
		}

		void BuildBestSellersReport()
		{
			var sql = "SELECT TOP 10 b.Name AS 'Product Name', "
				+ "b.VariantName AS 'Variant Name', "
				+ "b.ChosenSize AS 'Size', "
				+ "b.ChosenColor AS 'Color', "
				+ "b.NumSales AS 'Number of Sales', "
				+ "b.Dollars FROM (SELECT p.Name, "
					+ "pv.Name AS 'VariantName', "
					+ "s.ChosenSize, "
					+ "s.ChosenColor, "
					+ "s.NumSales, "
					+ "s.Dollars "
				+ "FROM "
					+ "(SELECT ProductID, "
					+ "VariantID, "
					+ "ChosenSize, "
					+ "ChosenColor, "
					+ "SUM(Quantity) AS NumSales, "
					+ "SUM(OrderedProductPrice) AS Dollars "
				+ " FROM Orders_ShoppingCart sc "
					+ "JOIN Orders o ON sc.OrderNumber = o.OrderNumber "
					+ "AND o.OrderDate >= @StartDate "
					+ "AND o.OrderDate <= @EndDate "
					+ "AND (o.StoreId = @StoreId or 0 = @StoreId) "
					+ "GROUP BY ProductID, VariantID, ChosenSize, ChosenColor) s "
				+ "JOIN Product p ON s.ProductID = p.ProductID "
				+ "JOIN Orders_ShoppingCart os ON p.ProductID = os.ProductID "
				+ "JOIN ProductVariant pv ON p.ProductID = pv.ProductID "
				+ "GROUP BY p.Name, pv.Name, s.ChosenColor, s.ChosenSize, s.NumSales, s.Dollars) b "
				+ "ORDER BY Dollars DESC";

			PopulateDateParameters();
			BuildReportFromSql(sql);
		}

		void BuildCustomersByProductReport()
		{
			Parameters.Add(new SqlParameter("@ProductID", txtProductId.Text.Trim()));

			var sql = "SELECT c.Email, c.FirstName AS 'First Name', c.LastName AS 'Last Name', SUM(os.Quantity) AS 'Total Purchased' "
				+ "FROM Customer c LEFT JOIN Orders o ON c.CustomerID = o.CustomerID "
					+ "LEFT JOIN Orders_ShoppingCart os ON os.OrderNumber = o.OrderNumber "
				+ "WHERE os.ProductID = @ProductId "
					+ "AND o.OrderDate >= @StartDate "
					+ "AND o.OrderDate <= @EndDate "
					+ "AND (o.StoreID = @StoreId OR 0 = @StoreId) "
				+ "GROUP BY c.Email, c.FirstName, c.LastName";

			PopulateDateParameters();
			BuildReportFromSql(sql);
		}

		void BuildCurrentRecurringProductsReport()
		{
			Parameters.Add(new SqlParameter("@CartType", ((int)CartTypeEnum.RecurringCart).ToString()));

			var sql = "SELECT c.email AS 'Customer Email', p.Name AS 'Product Name', pv.Name AS 'Variant', sc.Quantity AS 'QTY', CONVERT(varchar, sc.NextRecurringShipDate, 101) AS 'Next Shipping Date', sc.RecurringIndex AS '# of Past Shipments', "
				+ "sc.OriginalRecurringOrderNumber AS 'Original Order Number' "
					+ "FROM ShoppingCart sc "
						+ "LEFT JOIN ProductVariant pv ON sc.VariantID = pv.VariantID "
						+ "LEFT JOIN Product p ON sc.ProductID = p.ProductID "
						+ "LEFT JOIN Customer c ON sc.CustomerID = c.CustomerID "
					+ "WHERE CartType=@CartType "
					+ "ORDER BY sc.NextRecurringShipDate";

			BuildReportFromSql(sql);
		}

		void BuildProductsByCustomerLevelReport()
		{
			Parameters.Add(new SqlParameter("@CustomerLevelID", ddCustomerLevel.SelectedValue));
			var sql = "SELECT DISTINCT p.ProductID, p.Name, p.SKU, CASE p.Published WHEN 1 THEN 'Yes' ELSE 'No' END AS 'Published?', "
				+ "CASE WHEN @StoreId = 0 THEN 'N/A' ELSE CAST(ps.StoreID AS VARCHAR(10)) END AS 'Store ID' "
				+ "FROM Product p "
					+ "LEFT JOIN ProductCustomerLevel pcl ON p.ProductID = pcl.ProductID "
					+ "LEFT JOIN CustomerLevel cl ON pcl.CustomerLevelID = cl.CustomerLevelID "
					+ "LEFT JOIN ProductStore ps ON p.ProductID = ps.ProductID "
				+ "WHERE cl.CustomerLevelID = @CustomerLevelID "
					+ "AND (ps.StoreID = @StoreId OR 0 = @StoreId)";

			BuildReportFromSql(sql);
		}

		void BuildOrdersByDateRangeReport()
		{
			var sql = "SELECT DISTINCT o.OrderNumber,  "
							+ "o.OrderDate,  "
							+ "o.OrderSubTotal, "
							+ "o.OrderTax,  "
							+ "o.OrderShippingCosts,  "
							+ "o.OrderTotal, "
							+ "SUM(os.Quantity) AS '# Items', "
							+ "o.BillingFirstName + ' ' + o.BillingLastName AS Name, "
							+ "o.PaymentMethod, "
							+ "o.TransactionState, "
							+ "s.Name AS 'Store' "
						+ "FROM Orders o "
							+ "LEFT JOIN Orders_ShoppingCart os ON o.OrderNumber = os.OrderNumber "
							+ "LEFT JOIN Store s ON o.StoreID = s.StoreID "
						+ "WHERE o.OrderDate >= @StartDate "
							+ "AND o.OrderDate <= @EndDate "
							+ "AND (o.StoreID = @StoreId OR 0 = @StoreId) "
						+ "GROUP BY o.OrderNumber, o.OrderDate, o.OrderSubTotal, o.OrderTax, "
							+ "o.OrderShippingCosts, o.BillingFirstName + ' ' + o.BillingLastName, "
							+ "o.OrderTotal, o.PaymentMethod, o.TransactionState, s.Name "
						+ "ORDER BY o.OrderNumber ASC";

			PopulateDateParameters();
			BuildReportFromSql(sql);
		}

		void BuildOrdersByItemReport()
		{
			var sql = "SELECT ProductID, "
							+ "CAST(OrderedProductName AS NVARCHAR(MAX)) AS 'Name',  "
							+ "CAST(OrderedProductVariantName AS NVARCHAR(MAX)) AS 'Variant Name',  "
							+ "CAST(OrderedProductSKU AS NVARCHAR(MAX)) AS 'SKU',  "
							+ "SUM(Quantity) AS '# Sold',  "
							+ "SUM(OrderedProductPrice) AS 'Total Value',  "
							+ "s.Name AS 'Store'  "
						+ "FROM Orders_ShoppingCart os  "
							+ "INNER JOIN Orders o ON os.OrderNumber = o.OrderNumber  "
							+ "INNER JOIN Store s ON o.StoreID = s.StoreID "
						+ "WHERE os.CreatedOn >= @StartDate  "
							+ "AND os.CreatedOn <= @EndDate  "
							+ "AND (o.StoreID = @StoreID OR 0 = @StoreID)  "
						+ "GROUP BY ProductID, "
							+ "CAST(OrderedProductName AS NVARCHAR(MAX)),  "
							+ "s.Name,  "
							+ "CAST(OrderedProductVariantName AS NVARCHAR(MAX)),  "
							+ "CAST(OrderedProductSKU AS NVARCHAR(MAX)),  "
							+ "Quantity,  "
							+ "OrderedProductPrice";

			PopulateDateParameters();
			BuildReportFromSql(sql);
		}

		void BuildSENamesReport()
		{
			var sql = @"SELECT	-- The 'final' columns that will be returned
						[Entity type], 
						[Problem Type], 
						[SEName],
						[Affected Entity IDs]
					FROM
						(
							SELECT [Entity type],	-- Intermediate columns from the UNION.  Some Rows from this select won't appear
								[Problem Type], 
								[SEName],
								[Affected Entity IDs],
								[Rows]
							FROM (
									SELECT 'Product' AS 'Entity Type', 
										CASE
											WHEN p1.SEName IS NOT NULL THEN 'Duplicated SEName (' + CAST(COUNT(p1.SEName) AS VARCHAR) + ')'
											ELSE 'Empty SEName'
										END AS 'Problem Type',
										ISNULL(p1.SEName, '-- N/A --') AS 'SEName',
										STUFF((SELECT ', ' + CAST(p2.ProductId AS VARCHAR)
												FROM Product p2
												WHERE ISNULL(p2.SEName, 'EMPTY') = ISNULL(p1.SEName, 'EMPTY')
												ORDER BY p2.ProductId
												FOR XML PATH(N'')), 1, 2, N'') AS 'Affected Entity IDs',
										COUNT(*) AS 'Rows'
									FROM Product p1
									GROUP BY p1.SEName
								) AS Products
								UNION
								(
									SELECT 'Variant' AS 'Entity Type', 
										CASE
											WHEN v1.SEName IS NOT NULL THEN 'Duplicated SEName (' + CAST(COUNT(v1.SEName) AS VARCHAR) + ')'
											ELSE 'Empty SEName'
										END AS 'Problem Type',
										ISNULL(v1.SEName, '-- N/A --') AS 'SEName',
										STUFF((SELECT ', ' + CAST(v2.VariantID AS VARCHAR)
												FROM ProductVariant v2
												WHERE ISNULL(v2.SEName, 'EMPTY') = ISNULL(v1.SEName, 'EMPTY')
												ORDER BY v2.VariantID
												FOR XML PATH(N'')), 1, 2, N'') AS 'Affected Entity IDs',
										COUNT(*) AS 'Rows'
									FROM ProductVariant v1
									GROUP BY v1.SEName
								)
								UNION
								(
									SELECT 'Category' AS 'Entity Type', 
										CASE
											WHEN c1.SEName IS NOT NULL THEN 'Duplicated SEName (' + CAST(COUNT(c1.SEName) AS VARCHAR) + ')'
											ELSE 'Empty SEName'
										END AS 'Problem Type',
										ISNULL(c1.SEName, '-- N/A --') AS 'SEName',
										STUFF((SELECT ', ' + CAST(c2.CategoryID AS VARCHAR)
												FROM Category c2
												WHERE ISNULL(c2.SEName, 'EMPTY') = ISNULL(c1.SEName, 'EMPTY')
												ORDER BY c2.CategoryID
												FOR XML PATH(N'')), 1, 2, N'') AS 'Affected Entity IDs',
										COUNT(*) AS 'Rows'
									FROM Category c1
									GROUP BY c1.SEName
								)
								UNION
								(
									SELECT 'Manufacturer' AS 'Entity Type', 
										CASE
											WHEN m1.SEName IS NOT NULL THEN 'Duplicated SEName (' + CAST(COUNT(m1.SEName) AS VARCHAR) + ')'
											ELSE 'Empty SEName'
										END AS 'Problem Type',
										ISNULL(m1.SEName, '-- N/A --') AS 'SEName',
										STUFF((SELECT ', ' + CAST(m2.ManufacturerID AS VARCHAR)
												FROM Manufacturer m2
												WHERE ISNULL(m2.SEName, 'EMPTY') = ISNULL(m1.SEName, 'EMPTY')
												ORDER BY m2.ManufacturerID
												FOR XML PATH(N'')), 1, 2, N'') AS 'Affected Entity IDs',
										COUNT(*) AS 'Rows'
									FROM Manufacturer m1
									GROUP BY m1.SEName
								)
								UNION
								(
									SELECT 'Department' AS 'Entity Type', 
										CASE
											WHEN s1.SEName IS NOT NULL THEN 'Duplicated SEName (' + CAST(COUNT(s1.SEName) AS VARCHAR) + ')'
											ELSE 'Empty SEName'
										END AS 'Problem Type',
										ISNULL(s1.SEName, '-- N/A --') AS 'SEName',
										STUFF((SELECT ', ' + CAST(s2.SectionID AS VARCHAR)
												FROM Section s2
												WHERE ISNULL(s2.SEName, 'EMPTY') = ISNULL(s1.SEName, 'EMPTY')
												ORDER BY s2.SectionID
												FOR XML PATH(N'')), 1, 2, N'') AS 'Affected Entity IDs',
										COUNT(*) AS 'Rows'
									FROM Section s1
									GROUP BY s1.SEName
								)
								UNION
								(
									SELECT 'Vector' AS 'Entity Type', 
										CASE
											WHEN v1.SEName IS NOT NULL THEN 'Duplicated SEName (' + CAST(COUNT(v1.SEName) AS VARCHAR) + ')'
											ELSE 'Empty SEName'
										END AS 'Problem Type',
										ISNULL(v1.SEName, '-- N/A --') AS 'SEName',
										STUFF((SELECT ', ' + CAST(v2.VectorID AS VARCHAR)
												FROM Vector v2
												WHERE ISNULL(v2.SEName, 'EMPTY') = ISNULL(v1.SEName, 'EMPTY')
												ORDER BY v2.VectorID
												FOR XML PATH(N'')), 1, 2, N'') AS 'Affected Entity IDs',
										COUNT(*) AS 'Rows'
									FROM Vector v1
									GROUP BY v1.SEName
								)
								UNION
								(
									SELECT 'Genre' AS 'Entity Type', 
										CASE
											WHEN g1.SEName IS NOT NULL THEN 'Duplicated SEName (' + CAST(COUNT(g1.SEName) AS VARCHAR) + ')'
											ELSE 'Empty SEName'
										END AS 'Problem Type',
										ISNULL(g1.SEName, '-- N/A --') AS 'SEName',
										STUFF((SELECT ', ' + CAST(g2.GenreID AS VARCHAR)
												FROM Genre g2
												WHERE ISNULL(g2.SEName, 'EMPTY') = ISNULL(g1.SEName, 'EMPTY')
												ORDER BY g2.GenreID
												FOR XML PATH(N'')), 1, 2, N'') AS 'Affected Entity IDs',
										COUNT(*) AS 'Rows'
									FROM Genre g1
									GROUP BY g1.SEName
								)
								UNION
								(
									SELECT 'Library' AS 'Entity Type', 
										CASE
											WHEN l1.SEName IS NOT NULL THEN 'Duplicated SEName (' + CAST(COUNT(l1.SEName) AS VARCHAR) + ')'
											ELSE 'Empty SEName'
										END AS 'Problem Type',
										ISNULL(l1.SEName, '-- N/A --') AS 'SEName',
										STUFF((SELECT ', ' + CAST(l2.LibraryID AS VARCHAR)
												FROM Library l2
												WHERE ISNULL(l2.SEName, 'EMPTY') = ISNULL(l1.SEName, 'EMPTY')
												ORDER BY l2.LibraryID
												FOR XML PATH(N'')), 1, 2, N'') AS 'Affected Entity IDs',
										COUNT(*) AS 'Rows'
									FROM Library l1
									GROUP BY l1.SEName
								)
						) AS ReportData
					WHERE ReportData.[Rows] > 1
						OR ReportData.[Problem Type] = 'Empty SEName'	-- We only care about dupes and empties";

			BuildReportFromSql(sql);
		}

		void BuildEmptySkuReport()
		{
			var sql = @"SELECT p.ProductID, p.Name, pv.VariantID, pv.Name 
						FROM Product p 
							JOIN ProductVariant pv ON p.ProductID = pv.ProductID 
							LEFT JOIN Inventory i ON pv.VariantID = i.VariantID 
							LEFT JOIN ProductStore ps ON p.ProductID = ps.ProductID 
						WHERE(ISNULL(p.Sku, '') + ISNULL(pv.SkuSuffix, '') + ISNULL(i.VendorFullSKU, '')) = ''
							AND (ps.StoreID = @StoreId OR 0 = @StoreId)";

			BuildReportFromSql(sql);
		}

		void BuildOrderStatsReport()
		{
			var sql = @"SELECT TransactionState,
							SUM(OrderSubtotal) AS 'Subtotals',
							SUM(OrderTax) AS 'Taxes',
							SUM(OrderShippingCosts) AS 'Shipping Costs',
							SUM(OrderTotal) AS 'Totals',
							COUNT(OrderNumber) AS 'Number of Orders',
							ROUND(AVG(OrderTotal), 2) AS 'Average Order Amount'
						FROM Orders
						WHERE OrderDate >= @StartDate
							AND OrderDate <= @EndDate
							AND (@StoreID = 0 OR @StoreID = StoreID)
						GROUP BY TransactionState;";

			PopulateDateParameters();
			BuildReportFromSql(sql);
		}

		void BuildSummaryReport()
		{
			using(var dbConn = new SqlConnection(DB.GetDBConn()))
			{
				dbConn.Open();

				litRegisteredCustomers.Text = DB.GetSqlN("SELECT COUNT(*) AS N FROM Customer WHERE IsRegistered=1", dbConn).ToString();
				litAnonymousCustomers.Text = DB.GetSqlN("SELECT COUNT(*) AS N FROM Customer WHERE IsRegistered=0", dbConn).ToString();
				litNumberOrders.Text = DB.GetSqlN("SELECT COUNT(*) AS N FROM Orders", dbConn).ToString();
				litOrderTotals.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(Math.Round(DB.GetSqlNDecimal("SELECT SUM(OrderTotal) AS N FROM Orders", dbConn), 2));
				litOrderSubtotals.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(Math.Round(DB.GetSqlNDecimal("SELECT SUM(OrderSubTotal) AS N FROM Orders", dbConn), 2));
				litOrderShipping.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(Math.Round(DB.GetSqlNDecimal("SELECT SUM(OrderShippingCosts) AS N FROM Orders", dbConn), 2));
				litOrderTax.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(Math.Round(DB.GetSqlNDecimal("SELECT SUM(OrderTax) AS N FROM Orders", dbConn), 2));
				litAverageOrder.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(Math.Round(DB.GetSqlNDecimal("SELECT SUM(OrderTotal)/COUNT(*) AS N FROM Orders", dbConn), 2));

				phReportResults.Visible = false;
				pnlSummary.Visible = true;
				btnSaveReport.Visible = false;
			}
		}
	}
}
