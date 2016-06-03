// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class Topics : AspDotNetStorefront.Admin.AdminPageBase
	{
		public const string DeleteTopicCommand = "topic:delete";
		public const string UndeleteTopicCommand = "topic:undelete";

		readonly Dictionary<string, CommandEventHandler> CommandHandlerMappings;
		
		readonly UrlHelper Url;
		readonly SkinProvider SkinProvider;

		public Topics()
		{
			Url = DependencyResolver.Current.GetService<UrlHelper>();

			// Use a dictionary to map the string command names to handler methods
			CommandHandlerMappings = new Dictionary<string, CommandEventHandler>
			{
				{ DeleteTopicCommand, (o, e) => DeleteTopicCommandHandler(o, e, deleted: true, successStringResource: "admin.topicgrid.Deleted") },
				{ UndeleteTopicCommand, (o, e) => DeleteTopicCommandHandler(o, e, deleted: false, successStringResource: "admin.topicgrid.UnDeleted") },
			};
			SkinProvider = new SkinProvider();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			BindFileGrid();
		}

		protected void DispatchGridCommand(object sender, GridViewCommandEventArgs e)
		{
			// Dispath known command names out of the dictionary
			if(CommandHandlerMappings.ContainsKey(e.CommandName))
				CommandHandlerMappings[e.CommandName].Invoke(sender, e);
		}

		void DeleteTopicCommandHandler(object sender, CommandEventArgs e, bool deleted, string successStringResource)
		{
			int topicId;

			if(!Int32.TryParse((string)e.CommandArgument, out topicId))
				return;

			if(SetDeletedFlag(topicId, deleted))
			{
				AlertMessageDisplay.PushAlertMessage(
					BuildAlertMessage(topicId, successStringResource),
					AlertMessage.AlertType.Success);

				FilteredListing.Rebind();
			}
		}

		string BuildAlertMessage(int topicId, string stringResourceName)
		{
			var locale = AppLogic.GetCurrentCustomer().LocaleSetting;
			return String.Format(
				"{0} {1} {2}",
				AppLogic.GetString("admin.common.Topic", locale),
				topicId,
				AppLogic.GetString(stringResourceName, locale));
		}

		#region Topic Data Access Methods

		bool SetDeletedFlag(int topicId, bool deleted)
		{
			try
			{
				DB.ExecuteSQL(
					"update dbo.Topic set Deleted = @deleted where TopicID = @topicId",
					new[]
					{
						new SqlParameter("deleted", deleted),
						new SqlParameter("topicId", topicId),
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

		#region FileTopics

		protected class FileBasedTopic
		{
			public string Name { get; set; }
			public string Link { get; set; }
			public string Location { get; set; }
		}

		protected void grdFileTopics_OnPageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			grdFileTopics.PageIndex = e.NewPageIndex;
			BindFileGrid();
		}

		private void BindFileGrid()
		{
			var appPath = HttpContext.Current.Request.PhysicalApplicationPath;
			var htmlExtensions = new[] { ".html", ".htm" };

			// Skin specific first
			var skinFileTopics = SkinProvider
				.GetSkins()
				.Select(skin => new
					{
						SkinId = skin.Id,
						TopicPath = Path.Combine(appPath, string.Format("Skins\\{0}\\Topics", skin.Name)),
						DisplayName = string.IsNullOrEmpty(skin.DisplayName)
							? skin.Id.ToString()
							: skin.DisplayName
					})
				.Where(o => Directory.Exists(o.TopicPath))
				.SelectMany(o => Directory
					.GetFiles(o.TopicPath)
					.Where(path => htmlExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
					.Select(path => Path.GetFileNameWithoutExtension(path))
					.OrderBy(filename => filename)
					.Select(filename => new FileBasedTopic
						{
							Name = filename,
							Location = string.Format("Skin: {0}", o.DisplayName),
							Link = Url.BuildTopicLink(
									name: XmlCommon.GetLocaleEntry(filename, ThisCustomer.LocaleSetting, true),
									additionalRouteValues: new Dictionary<string, object>
									{
										{ "skinId",  o.SkinId}
									})
						}));

			// Root folder next
			var rootFileTopics = Enumerable.Empty<FileBasedTopic>();
			var rootPath = Path.Combine(appPath, string.Format("Topics\\"));
			if(Directory.Exists(rootPath))
				rootFileTopics = Directory
					.GetFiles(rootPath)
					.Where(path => htmlExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
					.Select(path => Path.GetFileNameWithoutExtension(path))
					.OrderBy(filename => filename)
					.Select(filename => new FileBasedTopic
						{
							Name = filename,
							Location = "Root",
							Link = Url.BuildTopicLink(XmlCommon.GetLocaleEntry(filename, ThisCustomer.LocaleSetting, true))
						});

			grdFileTopics.DataSource = skinFileTopics
				.Concat(rootFileTopics)
				.ToArray();
			grdFileTopics.DataBind();
		}

		#endregion
	}
}
