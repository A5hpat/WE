// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class BulkShipping : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
		}

		protected void btnImport_Click(object sender, EventArgs e)
		{
			var importFile = fuShippingImport.PostedFile;
			if(importFile == null || !importFile.FileName.EndsWith(".csv"))
			{
				AlertMessage.PushAlertMessage("Please select a CSV file for import.", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return;
			}

			//Save as a temp file
			var fullFilePath = CommonLogic.SafeMapPath("~/images") + "\\ShippingExport_" + importFile.FileName.ToLowerInvariant().Substring(importFile.FileName.ToLowerInvariant().LastIndexOf('.'));

			importFile.SaveAs(fullFilePath);

			ProcessImport(fullFilePath);

			//Clean up the temp file
			File.Delete(fullFilePath);
		}

		protected void btnExport_Click(object sender, EventArgs e)
		{
			var exportContents = new StringBuilder();

			exportContents.AppendLine(MakeHeaderRow());

			//Can't use the grid as a datasource as it's paged, so query the DB with the same filters the grid is currently displaying
			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();

				var whereClause = FilteredListing.GetFilterWhereClause();
				var sql = string.Format(
					@"SELECT OrderNumber, ShippedOn, ShippedVIA, ShippingTrackingNumber,
						ShippingMethod, ShippingFirstName, ShippingLastName, ShippingAddress1,
						ShippingAddress2, ShippingSuite, ShippingCity, ShippingState, ShippingZip,
						ShippingCountry, ShippingPhone, Phone, Email
					FROM Orders WHERE {0}", whereClause.Sql);

				using(var command = new SqlCommand(sql, conn))
				{
					command.Parameters.AddRange(whereClause.Parameters.ToArray());

					using(IDataReader rs = command.ExecuteReader())
					{
						while(rs.Read())
						{
							exportContents.Append(DB.RSFieldInt(rs, "OrderNumber").ToString());
							exportContents.Append(",");

							var shippedOn = DB.RSFieldDateTime(rs, "ShippedOn");
							exportContents.Append(shippedOn == DateTime.MinValue ? string.Empty : shippedOn.ToString());
							exportContents.Append(",");

							exportContents.Append(DB.RSField(rs, "ShippedVIA"));
							exportContents.Append(",");

							exportContents.Append(DB.RSField(rs, "ShippingTrackingNumber"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "ShippingMethod"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "ShippingFirstName"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "ShippingLastName"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "ShippingAddress1"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "ShippingAddress2"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "ShippingSuite"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "ShippingCity"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "ShippingState"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "ShippingZip"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "ShippingCountry"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "ShippingPhone"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "Phone"));
							exportContents.Append(",");
							exportContents.Append(DB.RSField(rs, "Email"));
							exportContents.Append("\r\n");
						}
					}
				}
			}

			//Save a copy in images
			var filepath = CommonLogic.SafeMapPath("~/images") + "\\";
			var filename = "ShippingExport_" + Localization.ToNativeDateTimeString(DateTime.Now).Replace(" ", "_").Replace(",", "_").Replace("/", "_").Replace(":", "_").Replace(".", "_") + ".csv";
			var fullFilePath = filepath + filename;
			File.WriteAllText(fullFilePath, exportContents.ToString());

			// Send the CSV
			Response.Clear();
			Response.ClearHeaders();
			Response.ClearContent();
			Response.AddHeader("content-disposition", "attachment; filename=ShippingExport.csv");
			Response.BufferOutput = false;
			Response.ContentType = "text/csv";
			Response.TransmitFile(fullFilePath);
			Response.Flush();
			Response.End();
		}

		string MakeHeaderRow()
		{
			return string.Join(
				",",
				"OrderNumber",
				"ShippedOn",
				"Carrier",
				"TrackingNumber",
				"ShipMethod",
				"FirstName",
				"LastName",
				"ShippingAddress1",
				"ShippingAddress2",
				"ShippingSuite",
				"ShippingCity",
				"ShippingState",
				"ShippingZip",
				"ShippingCountry",
				"ShippingPhone",
				"CustomerPhone",
				"Email");
		}

		string UnQuoteValue(string quotedText)
		{
			return quotedText.TrimStart('\'').TrimEnd('\'');
		}

		void ProcessImport(string fullFilePath)
		{
			using(var reader = File.OpenText(fullFilePath))
			{
				var importTable = CsvParser.Parse(reader, true);

				foreach(DataRow row in importTable.Rows)
				{
					try
					{
						int orderNumber = 0;
						int parsedOrderNumber = 0;
						DateTime shippedOn = DateTime.Now;   //If no date is provided or we don't understand the format, use the import time
						DateTime parsedShippedOn;
						string carrier;
						string trackingNumber;
						bool sendEmail = AppLogic.AppConfigBool("BulkImportSendsShipmentNotifications");

						if(int.TryParse(UnQuoteValue(row["OrderNumber"].ToString()), out parsedOrderNumber))
						{
							orderNumber = parsedOrderNumber;

							if(DateTime.TryParse(UnQuoteValue(row["ShippedOn"].ToString()), out parsedShippedOn))
								shippedOn = parsedShippedOn;

							carrier = UnQuoteValue(row["Carrier"].ToString());
							trackingNumber = UnQuoteValue(row["TrackingNumber"].ToString());

							var order = new Order(orderNumber);
							var orderCustomer = new Customer(order.CustomerID);
							Order.MarkOrderAsShipped(orderNumber, carrier, trackingNumber, shippedOn, false, !sendEmail);
						}
					}
					catch(Exception ex)
					{
						AlertMessage.PushAlertMessage(ex.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
					}
				}
			}
			AlertMessage.PushAlertMessage("Import successful!", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
		}
	}
}
