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
	public partial class News : AspDotNetStorefront.Admin.AdminPageBase
	{
		public const string PublishNewsCommand = "news:publish";
		public const string UnpublishNewsCommand = "news:unpublish";
		public const string DeleteNewsCommand = "news:delete";

		protected void DispatchCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == PublishNewsCommand)
				PublishNewsCommandHandler(e, true);
			else if(e.CommandName == UnpublishNewsCommand)
				PublishNewsCommandHandler(e, false);
			else if(e.CommandName == DeleteNewsCommand)
				DeleteNewsCommandHandler(e);
		}

		void PublishNewsCommandHandler(CommandEventArgs e, bool published)
		{
			int newsId;
			if(!Int32.TryParse((string)e.CommandArgument, out newsId))
				return;

			if(SetNewsFlags(newsId, published: published))
				ctrlAlertMessage.PushAlertMessage(
					String.Format(
						"News {0}",
						published
							? "published"
							: "unpublished"),
					AlertMessage.AlertType.Success);
		}

		void DeleteNewsCommandHandler(CommandEventArgs e)
		{
			int newsId;
			if(!Int32.TryParse((string)e.CommandArgument, out newsId))
				return;

			if(SetNewsFlags(newsId, deleted: true))
				ctrlAlertMessage.PushAlertMessage("News deleted", AlertMessage.AlertType.Success);
		}

		bool SetNewsFlags(int newsId, bool? published = null, bool? deleted = null)
		{
			try
			{
				DB.ExecuteSQL(
					@"update dbo.News 
					set Published = coalesce(@published, Published), Deleted = coalesce(@deleted, Deleted) 
					where NewsId = @newsId",
					new[]
					{
						new SqlParameter("published", (object)published ?? DBNull.Value),
						new SqlParameter("deleted", (object)deleted ?? DBNull.Value),
						new SqlParameter("newsId", newsId),
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
