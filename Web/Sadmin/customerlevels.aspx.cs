// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for customerlevels.
    /// </summary>
    public partial class CustomerLevels : AspDotNetStorefront.Admin.AdminPageBase
    {
		public const string DeleteCustomerLevelCommand = "customerlevel:delete";
		public const string UndeleteCustomerLevelCommand = "customerlevel:undelete";
		public const string RemoveCustomerLevelCommand = "customerlevel:remove";

		readonly Dictionary<string, CommandEventHandler> CommandHandlerMappings;

		public CustomerLevels()
		{
			// Use a dictionary to map the string command names to handler methods
			CommandHandlerMappings = new Dictionary<string, CommandEventHandler>
			{
				{ DeleteCustomerLevelCommand, (o, e) => DeleteCustomerLevelCommandHandler(o, e, deleted: true, successStringResource: "admin.common.ItemDeleted") },
				{ UndeleteCustomerLevelCommand, (o, e) => DeleteCustomerLevelCommandHandler(o, e, deleted: false, successStringResource: "admin.CustomerLevelgrid.UnDeleted") },
				{ RemoveCustomerLevelCommand, (o, e) => RemoveCustomerLevelCommandHandler(o, e) },
			};
		}

		protected void DispatchGridCommand(object sender, GridViewCommandEventArgs e)
		{
			// Dispatch known command names out of the dictionary
			if(CommandHandlerMappings.ContainsKey(e.CommandName))
				CommandHandlerMappings[e.CommandName].Invoke(sender, e);
		}

		void DeleteCustomerLevelCommandHandler(object sender, CommandEventArgs e, bool deleted, string successStringResource)
		{
			int customerLevelId;

			if(!Int32.TryParse((string)e.CommandArgument, out customerLevelId))
				return;

			if(SetDeletedFlag(customerLevelId, deleted))
			{
				AlertMessageDisplay.PushAlertMessage(
					BuildAlertMessage(customerLevelId, successStringResource),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}


		void RemoveCustomerLevelCommandHandler(object sender, CommandEventArgs e)
		{
			int customerLevelId;

			if(!Int32.TryParse((string)e.CommandArgument, out customerLevelId))
				return;

			if(RemoveCustomerLevel(customerLevelId))
			{
				AlertMessageDisplay.PushAlertMessage("admin.Common.ItemDeleted".StringResource(), AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}

		string BuildAlertMessage(int id, string stringResourceName)
		{
			var locale = AppLogic.GetCurrentCustomer().LocaleSetting;
			return String.Format(
				"{0} {1} {2}",
				AppLogic.GetString("admin.common.CustomerLevel", locale),
				id,
				AppLogic.GetString(stringResourceName, locale));
		}

		#region CustomerLevel Data Access Methods

		bool SetDeletedFlag(int customerLevelId, bool deleted)
		{
			try
			{
				DB.ExecuteSQL(
					"update dbo.CustomerLevel set Deleted = @deleted where CustomerLevelID = @customerLevelId",
					new[]
					{
						new SqlParameter("deleted", deleted),
						new SqlParameter("customerLevelId", customerLevelId),
					});

				return true;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
				return false;
			}
		}

		bool RemoveCustomerLevel(int customerLevelId)
		{
			try
			{
				DB.ExecuteSQL(
					"delete from dbo.ExtendedPrice where CustomerLevelID = @CustomerLevelID",
					new[]
					{
						new SqlParameter("CustomerLevelID", customerLevelId)
					});
					DB.ExecuteSQL(
					"update dbo.Customer set CustomerLevelID=0 where CustomerLevelID = @CustomerLevelID",
					new[]
					{
						new SqlParameter("CustomerLevelID", customerLevelId)
					});
					DB.ExecuteSQL(
					"delete from dbo.CustomerLevel where CustomerLevelID = @CustomerLevelID",
					new[]
					{
						new SqlParameter("CustomerLevelID", customerLevelId)
					});

				return true;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
				return false;
			}
		}
		#endregion

    }
}
