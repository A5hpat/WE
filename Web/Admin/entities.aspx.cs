// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using System.Linq;
using AspDotNetStorefrontControls.Listing;

namespace AspDotNetStorefrontAdmin
{
	public partial class Entities : AspDotNetStorefront.Admin.AdminPageBase
	{
		public const string DeleteEntityCommand = "entity:delete";
		public const string UndeleteEntityCommand = "entity:undelete";
		public const string PublishEntityCommand = "entity:publish";
		public const string UnpublishEntityCommand = "entity:unpublish";

		protected string EntityType = AppLogic.EntityType.Category.ToString();

		readonly Dictionary<string, CommandEventHandler> CommandHandlerMappings;

		public Entities()
		{
			// Use a dictionary to map the string command names to handler methods
			CommandHandlerMappings = new Dictionary<string, CommandEventHandler>
			{
				{ DeleteEntityCommand, (o, e) => DeleteEntityCommandHandler(o, e, deleted: true, successStringResource: "admin.common.HasBeenDeleted") },
				{ UndeleteEntityCommand, (o, e) => DeleteEntityCommandHandler(o, e, deleted: false, successStringResource: "admin.common.HasBeenRestored") },
				{ PublishEntityCommand, (o, e) => PublishEntityCommandHandler(o, e, published: true, successStringResource: "admin.common.HasBeenPublished") },
				{ UnpublishEntityCommand, (o, e) => PublishEntityCommandHandler(o, e, published: false, successStringResource: "admin.common.HasBeenUnPublished") },
			};
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			var querystringEntityType = !String.IsNullOrEmpty(CommonLogic.QueryStringCanBeDangerousContent("entityname"))
				? CommonLogic.QueryStringCanBeDangerousContent("entityname")
				: AppLogic.EntityType.Category.ToString();

			switch(querystringEntityType.ToLowerInvariant())
			{
				case "manufacturer":
					this.Title = HeaderName.Text = AppLogic.GetString("admin.menu.Manufacturers", ThisCustomer.LocaleSetting);
					EntityType = AppLogic.EntityType.Manufacturer.ToString();
					break;
				case "distributor":
					Page.Title = HeaderName.Text = AppLogic.GetString("admin.menu.Distributors", ThisCustomer.LocaleSetting);
					EntityType = AppLogic.EntityType.Distributor.ToString();
					break;
				case "department":
				case "section":
					Page.Title = HeaderName.Text = AppLogic.GetString("admin.menu.Sections", ThisCustomer.LocaleSetting);
					EntityType = AppLogic.EntityType.Section.ToString();
					break;
				case "category":
				default:
					Page.Title = HeaderName.Text = AppLogic.GetString("admin.menu.Categories", ThisCustomer.LocaleSetting);
					EntityType = AppLogic.EntityType.Category.ToString();
					break;
			}
		}

		protected void DispatchGridCommand(object sender, GridViewCommandEventArgs e)
		{
			// Dispatch known command names out of the dictionary
			if(CommandHandlerMappings.ContainsKey(e.CommandName))
				CommandHandlerMappings[e.CommandName].Invoke(sender, e);
		}

		void DeleteEntityCommandHandler(object sender, CommandEventArgs e, bool deleted, string successStringResource)
		{
			int entityId;

			if (!Int32.TryParse((string)e.CommandArgument, out entityId))
				return;

			if (SetDeletedFlag(entityId, deleted))
			{
				AlertMessageDisplay.PushAlertMessage(
					BuildEntityAlertMessage(entityId, successStringResource),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}

		void PublishEntityCommandHandler(object sender, CommandEventArgs e, bool published, string successStringResource)
		{
			int entityId;
			if (!Int32.TryParse((string)e.CommandArgument, out entityId))
				return;

			if (SetPublishedFlag(entityId, published))
			{
				AlertMessageDisplay.PushAlertMessage(
					BuildEntityAlertMessage(entityId, successStringResource),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}

		string BuildEntityAlertMessage(int entityId, string stringResourceName)
		{
			var locale = AppLogic.GetCurrentCustomer().LocaleSetting;
			return String.Format(
				"{0} {1} {2}",
                String.Format("admin.common.{0}", EntityType).StringResource(),
				entityId,
				AppLogic.GetString(stringResourceName, locale));
		}

		#region Entity Data Access Methods

		bool SetDeletedFlag(int entityId, bool deleted)
		{
			try
			{
				DB.ExecuteSQL(
					String.Format("update {0} set Deleted = @deleted where {0}ID = @entityId", EntityType),
					new[]
					{
						new SqlParameter("deleted", deleted),
						new SqlParameter("entityId", entityId),
					});

				return true;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
				return false;
			}
		}

		bool SetPublishedFlag(int entityId, bool published)
		{
			try
			{
				DB.ExecuteSQL(
					String.Format("update {0} set Published = @published where {0}ID = @entityId", EntityType),
					new[]
					{
						new SqlParameter("published", published),
						new SqlParameter("entityId", entityId),
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

		protected void Grid_DataBinding(object sender, EventArgs e)
		{
			var entityDisplayName = AppLogic.GetString(String.Format("admin.common.{0}", EntityType), LocaleSetting);
			Grid.Columns[1].HeaderText = String.Format("{0} {1}", entityDisplayName, AppLogic.GetString("admin.entity.NameSearch", LocaleSetting));
		}
}
}
