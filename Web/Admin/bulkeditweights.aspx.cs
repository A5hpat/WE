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
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

public partial class _bulkeditweights : AspDotNetStorefront.Admin.AdminPageBase
{
	protected void Save(object sender, EventArgs e)
	{
		try
		{
			var items = repeatMap.Items.Cast<RepeaterItem>().Where(item => item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem);
			foreach(var item in items)
			{
				var variantId = int.Parse(((Label)item.FindControl("lblVariantId")).Text);
				var where = FilteredListing.GetFilterWhereClause();

				string dimensions = BuildDimensions(item);

				var isShipSeparately = ((CheckBox)item.FindControl("chkIsShipSeparately")).Checked;

				decimal? weight = null;
				decimal result;
				if(decimal.TryParse(((TextBox)item.FindControl("txtWeight")).Text, out result) && result > 0)
					weight = result;

				using(var connection = new SqlConnection(DB.GetDBConn()))
				{
					if(weight != null)
						SaveWeight(weight.Value, variantId, connection);

					if(dimensions != null)
						SaveDimensions(dimensions, variantId, connection);

					SaveShipSeparately(isShipSeparately, variantId, connection);
				}
			}

			AlertMessage.PushAlertMessage(AppLogic.GetString("admin.bulkeditweights.weightsupdated", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
		}
		catch(Exception ex)
		{
			AlertMessage.PushAlertMessage(ex.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
		}
	}

	private void ProcessImport(string fullFilePath)
	{
		var valuesWereSkipped = false;

		using(TextReader tr = File.OpenText(fullFilePath))
		{
			var importTable = CsvParser.Parse(tr, true);

			foreach(DataRow row in importTable.Rows)
			{
				try
				{
					var variantId = 0;
					var weight = 0.0M;
					var width = 0.0M;
					var height = 0.0M;
					var depth = 0.0M;
					var isShipSeparately = false;

					int.TryParse(UnQuoteValue(row["VariantId"].ToString()), out variantId);
					decimal.TryParse(UnQuoteValue(row["Weight"].ToString()), out weight);
					decimal.TryParse(UnQuoteValue(row["Width"].ToString()), out width);
					decimal.TryParse(UnQuoteValue(row["Height"].ToString()), out height);
					decimal.TryParse(UnQuoteValue(row["Depth"].ToString()), out depth);
					bool.TryParse(UnQuoteValue(row["IsShipSeparately"].ToString()), out isShipSeparately);

					if(variantId != 0)
					{
						using(var connection = new SqlConnection(DB.GetDBConn()))
						{
							if(weight > 0)
								SaveWeight(weight, variantId, connection);
							else
								valuesWereSkipped = true;

							if(width > 0 && height > 0 && depth > 0)
								SaveDimensions(String.Format("{0}x{1}x{2}", width, height, depth), variantId, connection);
							else
								valuesWereSkipped = true;

							SaveShipSeparately(isShipSeparately, variantId, connection);
						}
					}
				}
				catch(Exception ex)
				{
					AlertMessage.PushAlertMessage(ex.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				}
			}
		}
		AlertMessage.PushAlertMessage("Import successful!", AspDotNetStorefrontControls.AlertMessage.AlertType.Success);

		if(valuesWereSkipped)
			AlertMessage.PushAlertMessage("Some values were skipped.  Please review your import file and verify that all values are populated with positive numbers.", AspDotNetStorefrontControls.AlertMessage.AlertType.Warning);
	}

	protected void btnImport_Click(object sender, EventArgs e)
	{
		HttpPostedFile importFile = fuWeightImport.PostedFile;

		if(importFile == null || !importFile.FileName.EndsWith(".csv"))
		{
			AlertMessage.PushAlertMessage("Please select a CSV file for import.", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			return;
		}

		//Save as a temp file
		string fullFilePath = String.Format("{0}\\WeightImport_temp.csv",
			CommonLogic.SafeMapPath("~/images"));

		try
		{
			importFile.SaveAs(fullFilePath);

			ProcessImport(fullFilePath);
		}
		finally
		{
			//Clean up the temp file
			if(File.Exists(fullFilePath))
				File.Delete(fullFilePath);
		}
	}

	protected void btnExport_Click(object sender, EventArgs e)
	{
		var exportContents = new StringBuilder("ProductID,ProductName,VariantID,VariantName,Weight,Width,Height,Depth,IsShipSeparately\r\n");
		
		//In this case we don't want to honor the grid's filters - export everything
		using(var conn = new SqlConnection(DB.GetDBConn()))
		{
			conn.Open();

			var sql = @"SELECT p.ProductID, 
							p.Name AS ProductName,
							v.VariantID,
							v.Name AS VariantName, 
							v.Weight, 
							v.Dimensions, 
							v.IsShipSeparately 
						FROM ProductVariant v inner join Product p on p.ProductID = v.ProductID 
						WHERE p.Deleted = 0 AND v.Deleted = 0";

			using(var command = new SqlCommand(sql, conn))
			{
				using(var rs = command.ExecuteReader())
				{
					while(rs.Read())
					{
						var dimensions = DB.RSField(rs, "Dimensions");

						exportContents.AppendFormat("{0},\"{1}\",{2},\"{3}\",{4},{5},{6},{7},{8}\r\n",
							DB.RSFieldInt(rs, "ProductID"),
							DB.RSFieldByLocale(rs, "ProductName", LocaleSetting).Replace("\"", "\"\""),
							DB.RSFieldInt(rs, "VariantID"),
							DB.RSFieldByLocale(rs, "VariantName", LocaleSetting).Replace("\"", "\"\""),
							DB.RSFieldDecimal(rs, "Weight"),
							AppLogic.RetrieveProductDimension(dimensions, "width"),
							AppLogic.RetrieveProductDimension(dimensions, "height"),
							AppLogic.RetrieveProductDimension(dimensions, "depth"),
							DB.RSFieldBool(rs, "IsShipSeparately"));
					}
				}
			}
		}

		// Send the CSV
		Response.ClearHeaders();
		Response.ClearContent();
		Response.AddHeader("content-disposition", "attachment; filename=WeightExport.csv");
		Response.BufferOutput = false;
		Response.ContentType = "text/csv";
		Response.Write(exportContents);
		Response.Flush();
		Response.End();
	}

	String BuildDimensions(RepeaterItem item)
	{
		decimal width = 0;
		decimal.TryParse(((TextBox)item.FindControl("txtWidth")).Text, out width);

		decimal height = 0;
		decimal.TryParse(((TextBox)item.FindControl("txtHeight")).Text, out height);

		decimal depth = 0;
		decimal.TryParse(((TextBox)item.FindControl("txtDepth")).Text, out depth);

		if(width <= 0 || height <= 0 || depth <= 0)
			return null;

		return String.Format("{0}x{1}x{2}", width, height, depth);
	}

	String UnQuoteValue(string quotedText)
	{
		return quotedText.Trim('\'').Trim('"');
	}

	void SaveWeight(decimal weight, int variantId, SqlConnection connection)
	{
		var command = new SqlCommand(@"update ProductVariant set Weight = @Weight where VariantID = @VariantID", connection);

		command.Parameters.Add(new SqlParameter("VariantID", variantId));
		command.Parameters.Add(new SqlParameter("@Weight", weight));

		DB.ExecuteSQL(command);
	}

	void SaveDimensions(string dimensions, int variantId, SqlConnection connection)
	{
		var command = new SqlCommand(@"update ProductVariant set Dimensions = @Dimensions where VariantID = @VariantID", connection);

		command.Parameters.Add(new SqlParameter("VariantID", variantId));
		command.Parameters.Add(new SqlParameter("@Dimensions", dimensions));

		DB.ExecuteSQL(command);
	}

	void SaveShipSeparately(bool isShipSeparately, int variantId, SqlConnection connection)
	{
		var command = new SqlCommand(@"update ProductVariant set IsShipSeparately = @IsShipSeparately where VariantID = @VariantID", connection);

		command.Parameters.Add(new SqlParameter("VariantID", variantId));
		command.Parameters.Add(new SqlParameter("@IsShipSeparately", isShipSeparately));

		DB.ExecuteSQL(command);
	}
}
