// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class TopicEditor : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected int? TopicId
		{
			get
			{
				int topicId;
				if(Int32.TryParse(Request.QueryString["topicId"], out topicId))
					return topicId;

				return null;
			}
		}

		bool UseHtmlEditor
		{ get { return !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite"); } }

		bool TopicFilteringEnabled
		{ get { return AppLogic.GlobalConfigBool("AllowTopicFiltering"); } }

		readonly TopicManager TopicManager;

		public TopicEditor()
		{
			TopicManager = new TopicManager();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if(!IsPostBack)
			{
				Page.DataBind();
				LoadTopic();

				Title
					= HeaderText.Text
					= TopicId == null
						? "admin.topic.addnew".StringResource()
						: "admin.topic.editing".StringResource();
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DataBind();
		}

		protected void btnSave_Click(object sender, EventArgs e)
		{
			SaveTopic();
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			if(SaveTopic())
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		protected void btnDelete_Click(object sender, EventArgs e)
		{
			if(TopicId == null)
				return;

			TopicManager.SetDeleteFlag(TopicId.Value, true);
			AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.topic.deleted", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);

			Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		protected void btnCopyToStore_Click(object sender, EventArgs e)
		{
			var targetStoreId = Convert.ToInt32(ddCopyToStore.SelectedValue);
			if(targetStoreId <= 0 || TopicId == null)
				return;

			var clonedTopicId = TopicManager.CloneTopic(TopicId.Value, targetStoreId);
			PopulateCopyToStoreDropdown(txtTopicName.Text);

			Response.Redirect(String.Format("topic.aspx?topicId={0}", clonedTopicId));
		}

		protected void LocaleSelector_SelectedLocaleChanged(Object sender, EventArgs e)
		{
			LoadTopic();
		}

		void LoadTopic()
		{
			var locale = LocaleSelector
				.GetSelectedLocale()
				.Name;

			if(TopicId == null)
			{
				//creating Topic
				txtTopicName.Text
					= txtTopicTitle.Text
					= ltSEKeywords.Text
					= ltSETitle.Text
					= ltSEDescription.Text
					= txtPassword.Text
					= txtDspOrdr.Text
					= String.Empty;

				radDescription.Content = String.Empty;
				txtDescriptionNoHtmlEditor.Text = String.Empty;

				radDescription.Visible = UseHtmlEditor;
				txtDescriptionNoHtmlEditor.Visible = !UseHtmlEditor;

				rbDisclaimer.SelectedIndex
					= rbPublish.SelectedIndex
					= 0;

				chkIsFrequent.Checked = true;

				storeMapperWrapper.Visible = TopicFilteringEnabled && Store.StoreCount > 1;
			}
			else
			{
				using(var connection = new SqlConnection(DB.GetDBConn()))
				{
					connection.Open();

					var sql = "select * from Topic with(nolock) where TopicID = @topicId";

					using(var reader = DB.GetRS(sql, new[] { new SqlParameter("topicId", (object)TopicId) }, connection))
					{
						if(!reader.Read())
						{
							AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.common.UnableToRetrieveData", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Error);
							return;
						}

						txtTopicName.Text = DB.RSFieldByLocale(reader, "Name", locale);
						txtTopicTitle.Text = DB.RSFieldByLocale(reader, "Title", locale);

						if(UseHtmlEditor)
							radDescription.Content = DB.RSFieldByLocale(reader, "Description", locale);
						else
						{
							txtDescriptionNoHtmlEditor.Visible = true;
							txtDescriptionNoHtmlEditor.Text = DB.RSFieldByLocale(reader, "Description", locale);
							radDescription.Visible = false;
						}

						ltSETitle.Text = DB.RSFieldByLocale(reader, "SETitle", locale);
						ltSEKeywords.Text = DB.RSFieldByLocale(reader, "SEKeywords", locale);
						ltSEDescription.Text = DB.RSFieldByLocale(reader, "SEDescription", locale);

						txtPassword.Text = DB.RSField(reader, "Password");

						txtDspOrdr.Text = DB.RSFieldInt(reader, "DisplayOrder").ToString();

						rbDisclaimer.SelectedIndex = DB.RSFieldBool(reader, "RequiresDisclaimer")
							? 1
							: 0;

						rbPublish.SelectedIndex = DB.RSFieldBool(reader, "ShowInSiteMap")
							? 1
							: 0;

						PopulateCopyToStoreDropdown(DB.RSFieldByLocale(reader, "Name", locale));

						chkPublished.Checked = DB.RSFieldBool(reader, "Published");
						chkIsFrequent.Checked = DB.RSFieldBool(reader, "IsFrequent");

						TopicStoreMapper.SelectedStoreID = DB.RSFieldInt(reader, "StoreId");
					}
				}
			}
		}

		bool SaveTopic()
		{
			var locale = LocaleSelector
				.GetSelectedLocale()
				.Name;

			Page.Validate();
			if(!Page.IsValid)
				return false;

			var selectedStoreId = GetSelectedStoreId();
			var existingTopicId = FindTopicId(txtTopicName.Text, selectedStoreId);

			if(existingTopicId.HasValue && existingTopicId != TopicId)
			{
				AlertMessageDisplay.PushAlertMessage(String.Format("A topic with this name already exists in Store ID {0}", selectedStoreId), AlertMessage.AlertType.Error);
				return false;
			}

			var name = txtTopicName.Text;
			var title = AppLogic.FormLocaleXml("Title", txtTopicTitle.Text, locale, "topic", Convert.ToInt32(TopicId));
			var descriptionText = UseHtmlEditor
				? radDescription.Content
				: txtDescriptionNoHtmlEditor.Text;
			var description = AppLogic.FormLocaleXml("Description", descriptionText, locale, "topic", Convert.ToInt32(TopicId));
			var seKeywords = AppLogic.FormLocaleXml("SEKeywords", ltSEKeywords.Text, locale, "topic", Convert.ToInt32(TopicId));
			var seDescription = AppLogic.FormLocaleXml("SEDescription", ltSEDescription.Text, locale, "topic", Convert.ToInt32(TopicId));
			var seTitle = AppLogic.FormLocaleXml("SETitle", ltSETitle.Text, locale, "topic", Convert.ToInt32(TopicId));
			var displayOrder = Localization.ParseUSInt(txtDspOrdr.Text);
			var published = chkPublished.Checked;
			var isFrequent = chkIsFrequent.Checked;
			var password = txtPassword.Text;
			var requiresDisclaimer = rbDisclaimer.SelectedValue == "1";
			var showInSiteMap = rbPublish.SelectedValue == "1";
			var storeId = GetSelectedStoreId();

			try
			{
				if(TopicId == null)
				{
					var newTopicId = TopicManager.CreateTopic(
						name: name,
						displayOrder: displayOrder,
						published: published,
						title: title,
						isFrequent: isFrequent,
						description: description,
						password: password,
						requiresDisclaimer: requiresDisclaimer,
						showInSiteMap: showInSiteMap,
						seTitle: seTitle,
						seKeywords: seKeywords,
						seDescription: seDescription,
						storeId: storeId);

					AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.topic.addedstatus", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);

					Response.Redirect(String.Format("topic.aspx?topicId={0}", newTopicId));
					return true;
				}
				else
				{
					TopicManager.UpdateTopic(
						topicId: TopicId.Value,
						name: name,
						displayOrder: displayOrder,
						published: published,
						title: title,
						isFrequent: isFrequent,
						description: description,
						password: password,
						requiresDisclaimer: requiresDisclaimer,
						showInSiteMap: showInSiteMap,
						seTitle: seTitle,
						seKeywords: seKeywords,
						seDescription: seDescription,
						storeId: storeId);

					AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.topic.updatedstatus", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);

					LoadTopic();
					return true;
				}
			}
			catch(System.Threading.ThreadAbortException)
			{
				throw;
			}
			catch(Exception ex)
			{
				AlertMessageDisplay.PushAlertMessage(ex.Message, AlertMessage.AlertType.Error);
				return false;
			}
		}

		void PopulateCopyToStoreDropdown(string topicName)
		{
			ddCopyToStore.Items.Clear();
			var stores = Store.GetStoreList();

			if(TopicFilteringEnabled)
			{
				var unassignedStores = TopicManager.UnassignedStoreIds(topicName);
				var storeItems = stores
					.Where(s => unassignedStores.Contains(s.StoreID))
					.Select(s => new ListItem(s.Name, s.StoreID.ToString()))
					.ToArray();

				ddCopyToStore.AddItems(storeItems);
			}

			trCopyToStore.Visible = TopicFilteringEnabled && stores.Count > 1 && ddCopyToStore.Items.Count > 0;
		}

		int? FindTopicId(string topicName, int storeId)
		{
			var sql = "select TopicID from Topic with (nolock) where Deleted = 0 and StoreID = @storeId and Name = @topicName";
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand(sql, connection))
			{
				command.Parameters.AddRange(new[]
					{
						new SqlParameter("storeId", (object)storeId),
						new SqlParameter("topicName", topicName),
					});

				connection.Open();
				using(var reader = command.ExecuteReader())
					if(reader.Read())
						return reader.FieldInt("TopicID");
					else
						return null;
			}
		}

		int GetSelectedStoreId()
		{
			return Store.StoreCount <= 1
				? 0
				: TopicStoreMapper.SelectedStoreID;
		}
	}

	public class TopicManager
	{
		public IEnumerable<int> UnassignedStoreIds(string topicName)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(IDataReader rs = DB.GetRS(
					"select StoreId from Store where StoreId not in (select StoreId from Topic where Name = @topicName)",
					connection,
					new SqlParameter("topicName", topicName)))
				{
					while(rs.Read())
						yield return DB.RSFieldInt(rs, "StoreId");

					rs.Close();
				}

				connection.Close();
			}
		}

		public int CreateTopic(string name, int displayOrder, bool published, string title, bool isFrequent, string description, string password, bool requiresDisclaimer, bool showInSiteMap, string seTitle, string seKeywords, string seDescription, int storeId)
		{
			var parameters = BuildQueryParameters(
				name: name,
				displayOrder: displayOrder,
				published: published,
				title: title,
				isFrequent: isFrequent,
				description: description,
				password: password,
				requiresDisclaimer: requiresDisclaimer,
				showInSiteMap: showInSiteMap,
				seTitle: seTitle,
				seKeywords: seKeywords,
				seDescription: seDescription,
				storeId: storeId);

			var sql = @"insert into Topic(TopicGUID, Name, DisplayOrder, Published, Title, IsFrequent, Description, Password, HTMLOk, RequiresDisclaimer, ShowInSiteMap, SEKeywords, SEDescription, SETitle, StoreID) 
						values(newid(), @name, @displayOrder, @published, @title, @isFrequent, @description, @password, 1, @requiresDisclaimer, @showInSiteMap, @seKeywords, @seDescription, @seTitle, @storeId)

						select convert(int, scope_identity())";

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand(sql, connection))
			{
				command.Parameters.AddRange(parameters.ToArray());
				connection.Open();

				return (int)command.ExecuteScalar();
			}
		}

		public void UpdateTopic(int topicId, string name = null, int? displayOrder = null, bool? published = null, string title = null, bool? isFrequent = null, string description = null, string password = null, bool? requiresDisclaimer = null, bool? showInSiteMap = null, string seTitle = null, string seKeywords = null, string seDescription = null, int? storeId = null)
		{
			var parameters = BuildQueryParameters(
					name: name,
					displayOrder: displayOrder,
					published: published,
					title: title,
					isFrequent: isFrequent,
					description: description,
					password: password,
					requiresDisclaimer: requiresDisclaimer,
					showInSiteMap: showInSiteMap,
					seTitle: seTitle,
					seKeywords: seKeywords,
					seDescription: seDescription,
					storeId: storeId)
				.Concat(new[] 
					{ 
						new SqlParameter("topicId", (object)topicId) 
					});

			var sql = @"update
							Topic 
						set 
							Published = isnull(@published, Published),
							Name = isnull(@name, Name),
							DisplayOrder = isnull(@displayOrder, DisplayOrder),
							Title = isnull(@title, Title),
							IsFrequent = isnull(@isFrequent, IsFrequent),
							Description = isnull(@description, Description),
							Password = isnull(@password, Password),
							HTMLOk = 1,
							RequiresDisclaimer = isnull(@requiresDisclaimer, RequiresDisclaimer),
							ShowInSiteMap = isnull(@showInSiteMap, ShowInSiteMap),
							SEKeywords = isnull(@seKeywords, SEKeywords),
							SEDescription = isnull(@seDescription, SEDescription),
							SETitle = isnull(@seTitle, SETitle),
							StoreID = isnull(@StoreId, StoreID)
						where 
							TopicID = @topicId";

			DB.ExecuteSQL(sql, parameters.ToArray());
		}

		IEnumerable<SqlParameter> BuildQueryParameters(string name = null, int? displayOrder = null, bool? published = null, string title = null, bool? isFrequent = null, string description = null, string password = null, bool? requiresDisclaimer = null, bool? showInSiteMap = null, string seTitle = null, string seKeywords = null, string seDescription = null, int? storeId = null)
		{
			return new[]
				{
					new SqlParameter("@name", (object)name),
					new SqlParameter("@displayOrder", (object)displayOrder),
					new SqlParameter("@published", (object)published),
					new SqlParameter("@title", (object)title),
					new SqlParameter("@isFrequent", (object)isFrequent),
					new SqlParameter("@description", (object)description),
					new SqlParameter("@password", (object)password),
					new SqlParameter("@requiresDisclaimer", (object)requiresDisclaimer),
					new SqlParameter("@showInSiteMap", (object)showInSiteMap),
					new SqlParameter("@seKeywords", (object)seKeywords),
					new SqlParameter("@seDescription", (object)seDescription),
					new SqlParameter("@seTitle", (object)seTitle),
					new SqlParameter("@storeId", (object)storeId),
				};
		}

		public void SetDeleteFlag(int topicId, bool deleted)
		{
			DB.ExecuteSQL(
				"update Topic set Deleted = @deleted where TopicID = @topicId",
				new[] 
						{ 
							new SqlParameter("topicId", (object)topicId),
							new SqlParameter("deleted", (object)deleted),
						});
		}

		public int? CloneTopic(int topicId, int? targetStoreId = null)
		{
			string sql = @"
					insert into Topic(
						Name,
						DisplayOrder,
						Title,
						Description,
						Password,
						HTMLOk,
						RequiresDisclaimer,
						ShowInSiteMap,
						SEKeywords,
						SEDescription,
						SETitle,
						StoreID)
					select
						Name,
						DisplayOrder,
						Title,
						Description,
						Password,
						HTMLOk,
						RequiresDisclaimer,
						ShowInSiteMap,
						SEKeywords,
						SEDescription,
						SETitle,
						isnull(@storeId, StoreID)
					from
						Topic with(nolock)
					where
						deleted = 0
						and topicid = @topicId;

					select convert(int, scope_identity())";

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand(sql, connection))
			{
				command.Parameters.AddRange(new[]
						{
							new SqlParameter("topicId", (object)topicId),
							new SqlParameter("storeId", (object)targetStoreId),
						});

				connection.Open();

				var newId = command.ExecuteScalar();
				if(newId is DBNull)
					return null;
				else
					return (int)newId;
			}
		}
	}
}
