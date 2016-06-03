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
using AspDotNetStorefrontControls.Listing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class Affiliates : AspDotNetStorefront.Admin.AdminPageBase
	{
		public const string DeleteAffiliateCommand = "affiliate:delete";
		public const string UndeleteAffiliateCommand = "affiliate:undelete";
		public const string PublishAffiliateCommand = "affiliate:publish";
		public const string UnpublishAffiliateCommand = "affiliate:unpublish";

		readonly Dictionary<string, CommandEventHandler> CommandHandlerMappings;

		public Affiliates()
		{
			// Use a dictionary to map the string command names to handler methods
			CommandHandlerMappings = new Dictionary<string, CommandEventHandler>
			{
				{ DeleteAffiliateCommand, (o, e) => DeleteAffiliateCommandHandler(o, e, deleted: true, successStringResource: "admin.common.HasBeenDeleted") },
				{ UndeleteAffiliateCommand, (o, e) => DeleteAffiliateCommandHandler(o, e, deleted: false, successStringResource: "admin.common.HasBeenRestored") },
				{ PublishAffiliateCommand, (o, e) => PublishAffiliateCommandHandler(o, e, published: true, successStringResource: "admin.common.HasBeenPublished") },
				{ UnpublishAffiliateCommand, (o, e) => PublishAffiliateCommandHandler(o, e, published: false, successStringResource: "admin.common.HasBeenUnPublished") },
			};
		}

		protected void DispatchGridCommand(object sender, GridViewCommandEventArgs e)
		{
			// Dispatch known command names out of the dictionary
			if(CommandHandlerMappings.ContainsKey(e.CommandName))
				CommandHandlerMappings[e.CommandName].Invoke(sender, e);
		}

		void DeleteAffiliateCommandHandler(object sender, CommandEventArgs e, bool deleted, string successStringResource)
		{
			int affiliateId;

			if(!Int32.TryParse((string)e.CommandArgument, out affiliateId))
				return;

			if(SetDeletedFlag(affiliateId, deleted))
			{
				AlertMessageDisplay.PushAlertMessage(
					BuildAffiliateAlertMessage(affiliateId, successStringResource),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}

		void PublishAffiliateCommandHandler(object sender, CommandEventArgs e, bool published, string successStringResource)
		{
			int affiliateId;
			if(!Int32.TryParse((string)e.CommandArgument, out affiliateId))
				return;

			if(SetPublishedFlag(affiliateId, published))
			{
				AlertMessageDisplay.PushAlertMessage(
					BuildAffiliateAlertMessage(affiliateId, successStringResource),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}

		string BuildAffiliateAlertMessage(int affiliateId, string stringResourceName)
		{
			var locale = AppLogic.GetCurrentCustomer().LocaleSetting;
			return String.Format(
				"{0} {1} {2}",
				AppLogic.GetString("admin.common.affiliate", LocaleSetting),
				affiliateId,
				AppLogic.GetString(stringResourceName, locale));
		}

		#region Affiliate Data Access Methods

		bool SetDeletedFlag(int affiliateId, bool deleted)
		{
			try
			{
				DB.ExecuteSQL(
					String.Format("update Affiliate set Deleted = @deleted where AffiliateID = @affiliateId"),
					new[]
					{
						new SqlParameter("deleted", deleted),
						new SqlParameter("affiliateId", affiliateId),
					});

				return true;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
				return false;
			}
		}

		bool SetPublishedFlag(int affiliateId, bool published)
		{
			try
			{
				DB.ExecuteSQL(
					String.Format("update Affiliate set Published = @published where AffiliateID = @affiliateId"),
					new[]
					{
						new SqlParameter("published", published),
						new SqlParameter("affiliateId", affiliateId),
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
