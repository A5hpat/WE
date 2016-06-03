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

namespace AspDotNetStorefrontAdmin
{
	public partial class Products : AspDotNetStorefront.Admin.AdminPageBase
	{
		public const string DeleteProductCommand = "product:delete";
		public const string UndeleteProductCommand = "product:undelete";
		public const string PublishProductCommand = "product:publish";
		public const string UnpublishProductCommand = "product:unpublish";
		public const string CloneProductCommand = "product:clone";

		readonly Dictionary<string, CommandEventHandler> CommandHandlerMappings;

		public Products()
		{
			// Use a dictionary to map the string command names to handler methods
			CommandHandlerMappings = new Dictionary<string, CommandEventHandler>
			{
				{ DeleteProductCommand, (o, e) => DeleteProductCommandHandler(o, e, deleted: true, successStringResource: "admin.productgrid.Deleted") },
				{ UndeleteProductCommand, (o, e) => DeleteProductCommandHandler(o, e, deleted: false, successStringResource: "admin.productgrid.UnDeleted") },
				{ PublishProductCommand, (o, e) => PublishProductCommandHandler(o, e, published: true, successStringResource: "admin.productgrid.Published") },
				{ UnpublishProductCommand, (o, e) => PublishProductCommandHandler(o, e, published: false, successStringResource: "admin.productgrid.UnPublished") },
				{ CloneProductCommand, CloneProductCommandHandler },
			};
		}

		protected void DispatchGridCommand(object sender, GridViewCommandEventArgs e)
		{
			// Dispatch known command names out of the dictionary
			if(CommandHandlerMappings.ContainsKey(e.CommandName))
				CommandHandlerMappings[e.CommandName].Invoke(sender, e);
		}

		void DeleteProductCommandHandler(object sender, CommandEventArgs e, bool deleted, string successStringResource)
		{
			int productId;

			if(!Int32.TryParse((string)e.CommandArgument, out productId))
				return;

			if(SetDeletedFlag(productId, deleted))
			{
				AlertMessageDisplay.PushAlertMessage(
					BuildProductAlertMessage(productId, successStringResource),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}

		void PublishProductCommandHandler(object sender, CommandEventArgs e, bool published, string successStringResource)
		{
			int productId;
			if(!Int32.TryParse((string)e.CommandArgument, out productId))
				return;

			if(SetPublishedFlag(productId, published))
			{
				AlertMessageDisplay.PushAlertMessage(
					BuildProductAlertMessage(productId, successStringResource),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}

		void CloneProductCommandHandler(object sender, CommandEventArgs e)
		{
			int productId;
			if(!Int32.TryParse((string)e.CommandArgument, out productId))
				return;

			if(CloneProduct(productId))
			{
				AlertMessageDisplay.PushAlertMessage(
					BuildProductAlertMessage(productId, "admin.productgrid.ProductCloned"),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
			else
			{
				AlertMessageDisplay.PushAlertMessage(
					BuildProductAlertMessage(productId, "admin.productgrid.ProductNotCloned"),
					AlertMessage.AlertType.Error);
			}
		}

		string BuildProductAlertMessage(int productId, string stringResourceName)
		{
			var locale = AppLogic.GetCurrentCustomer().LocaleSetting;
			return String.Format(
				"{0} {1} {2}",
				AppLogic.GetString("admin.common.Product", locale),
				productId,
				AppLogic.GetString(stringResourceName, locale));
		}

		#region Product Data Access Methods

		bool SetDeletedFlag(int productId, bool deleted)
		{
			try
			{
				DB.ExecuteSQL(
					"update dbo.Product set Deleted = @deleted where ProductID = @productId",
					new[]
					{
						new SqlParameter("deleted", deleted),
						new SqlParameter("productId", productId),
					});

				return true;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
				return false;
			}
		}

		bool SetPublishedFlag(int productId, bool published)
		{
			try
			{
				DB.ExecuteSQL(
					"update dbo.Product set Published = @published where ProductID = @productId",
					new[]
					{
						new SqlParameter("published", published),
						new SqlParameter("productId", productId),
					});

				return true;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
				return false;
			}
		}

		bool CloneProduct(int productId)
		{
			if(productId == 0)
				return false;

			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(var reader = DB.GetRS("aspdnsf_CloneProduct @productId", new[] { new SqlParameter("productId", productId) }, connection))
					return reader.Read();
			}
		}

		#endregion
	}
}
