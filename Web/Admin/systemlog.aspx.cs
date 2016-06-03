// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class SystemLog : AspDotNetStorefront.Admin.AdminPageBase
	{
		int? SysLogId
		{
			get
			{
				int sysLogId;
				if(!Int32.TryParse(Request.QueryString["logId"], out sysLogId))
					return null;

				return sysLogId;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if(SysLogId == null)
				Response.Redirect("systemlogs.aspx");

			if(!Page.IsPostBack)
				LoadEntry();
		}

		private void LoadEntry()
		{
			var sql = "SELECT * FROM aspdnsf_SysLog WITH (NOLOCK) WHERE SysLogID = @sysLogId";
			var sqlParams = new[] { new SqlParameter("sysLogId", (object)SysLogId) };

			using(var connection = DB.dbConn())
			{
				connection.Open();
				using(var reader = DB.GetRS(sql, sqlParams, connection))
				{
					if(reader.Read())
					{
						litLogId.Text = DB.RSFieldInt(reader, "SysLogId").ToString();
						litLogCreatedOn.Text = DB.RSFieldDateTime(reader, "CreatedOn").ToString();
						litLogMessage.Text = DB.RSField(reader, "Message");
						litLogDetails.Text = Server.HtmlDecode(DB.RSField(reader, "Details")).Replace("\r\n", "<br />");	//Try to make the errors a bit more readable
					}
				}
			}
		}
	}
}
