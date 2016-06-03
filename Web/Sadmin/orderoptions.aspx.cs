// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class OrderOptions : AspDotNetStorefront.Admin.AdminPageBase
	{
		public const string DeleteOrderOptionCommand = "orderoption:delete";
		readonly string Locale;

		public OrderOptions()
		{
			Locale = AppLogic.GetCurrentCustomer().LocaleSetting;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
		}

		protected void DispatchGridCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName != DeleteOrderOptionCommand)
				return;

			int orderOptionId;
			if(!Int32.TryParse((string)e.CommandArgument, out orderOptionId))
				return;

			var result = DeletedOrderOption(orderOptionId);
			if (result)
				FilteredListing.Rebind();

			ctlAlertMessage.PushAlertMessage(
				message: String.Format("{0} {1} {2}",
					AppLogic.GetString("admin.common.OrderOption", Locale),
					orderOptionId,
					AppLogic.GetString(result ? "admin.common.HasBeenDeleted" : "admin.common.CouldNotBeDeleted", Locale)),
				type: AlertMessage.AlertType.Success);
		}

		bool DeletedOrderOption(int orderOptionId)
		{
			try
			{
				DB.ExecuteSQL("delete from OrderOption where OrderOptionId = @orderOptionId",
					new[]
					{
						new SqlParameter("orderOptionId", orderOptionId),
					});

				return true;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
				return false;
			}
		}
	}
}
