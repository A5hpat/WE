// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	[LoggedAuthorize(Roles = "SuperAdmin", LogSuccess = true, LogFailed = true)]
	public partial class securitylog : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Server.ScriptTimeout = 5000000;
		}

		protected string DecryptValue(string value)
		{
			var decryptedValue = Security.UnmungeString(value);
			if(decryptedValue.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
				return value;

			return decryptedValue;
		}

		protected void btnExport_Click(object sender, EventArgs e)
		{
			var exportContents = new StringBuilder();
			exportContents.AppendLine("Action, Description, Date, CustomerID, Email");

			// Can't use the grid as a datasource as it's paged, so query the DB with the same filters the grid is currently displaying
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				var whereClause = FilteredListing.GetFilterWhereClause();

				command.CommandText = string.Format(
					@"select SecurityAction Action, Description, ActionDate Date, UpdatedBy CustomerID, c.EMail 
					from SecurityLog with (NOLOCK) 
					left outer join Customer c with (NOLOCK) on SecurityLog.UpdatedBy = c.CustomerID 
					where {0}", whereClause.Sql);

				command.Parameters.AddRange(whereClause.Parameters.ToArray());

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						var shippedOn = DB.RSFieldDateTime(reader, "Date");

						exportContents.AppendFormat(@"""{0}"",""{1}"",{2},{3},{4}{5}",
							DecryptValue(reader.Field("Action")).Replace("\"", "\"\""),
							DecryptValue(reader.Field("Description").Replace("\"", "\"\"")),
							shippedOn == DateTime.MinValue
								? string.Empty
								: shippedOn.ToString(),
							reader.FieldInt("CustomerID"),
							reader.Field("Email"),
							Environment.NewLine);
					}
			}

			// Send the CSV
			Response.Clear();
			Response.AddHeader("content-disposition", "attachment; filename=SecurityLogExport.csv");
			Response.BufferOutput = false;
			Response.ContentType = "text/csv";
			Response.Output.Write(exportContents);
			Response.Flush();
			Response.End();
		}
	}
}
