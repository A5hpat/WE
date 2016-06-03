// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Threading;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class NewsEditor : AspDotNetStorefront.Admin.AdminPageBase
	{
		readonly bool UseHtmlEditor;

		int RecordId
		{
			get { return (int?)ViewState["RecordId"] ?? CommonLogic.QueryStringNativeInt("NewsId"); }
			set { ViewState["RecordId"] = value; }
		}

		public NewsEditor()
		{
			UseHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			txtCopy.Visible = !UseHtmlEditor;
			radCopy.Visible = UseHtmlEditor;

			if(Page.IsPostBack)
				return;

			txtDate.Culture = Thread.CurrentThread.CurrentUICulture;
			LoadNewsItem();
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DataBind();
			btnClose.DataBind();
			btnCloseTop.DataBind();
		}

		protected void LocaleSelector_SelectedLocaleChanged(object sender, EventArgs e)
		{
			LoadNewsItem();
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			try
			{
				Page.Validate();
				if(!Page.IsValid)
					return;

				SaveNewsItem();
				ctlAlertMessage.PushAlertMessage("admin.orderdetails.UpdateSuccessful".StringResource(), AlertMessage.AlertType.Success);
			}
			catch(Exception exception)
			{
				ctlAlertMessage.PushAlertMessage(exception.Message, AlertMessage.AlertType.Error);
			}
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			try
			{
				Page.Validate();
				if(!Page.IsValid)
					return;

				SaveNewsItem();
				Response.Redirect(ReturnUrlTracker.GetReturnUrl());
			}
			catch(ThreadAbortException)
			{
				throw;
			}
			catch(Exception exception)
			{
				ctlAlertMessage.PushAlertMessage(exception.Message, AlertMessage.AlertType.Error);
			}
		}

		void LoadNewsItem()
		{
			if(RecordId == 0)
			{
				Title = HeaderText.Text = AppLogic.GetString("admin.editnews.AddingNews", ThisCustomer.LocaleSetting);
				txtDate.SelectedDate = DateTime.Now.AddMonths(1);
			}
			else
			{
				Title = HeaderText.Text = AppLogic.GetString("admin.editnews.EditingNews", ThisCustomer.LocaleSetting);

				using(var connection = new SqlConnection(DB.GetDBConn()))
				using(var command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText = "SELECT * FROM News WITH (NOLOCK) WHERE NewsID = @NewsID";
					command.Parameters.AddRange(new[]
						{
							new SqlParameter("@NewsID", RecordId.ToString()) 
						});

					connection.Open();
					using(var reader = command.ExecuteReader())
						if(reader.Read())
						{
							var selectedLocale = LocaleSelector.GetSelectedLocale().Name;

							litNewsId.Text = reader.FieldInt("NewsID").ToString();
							txtHeadline.Text = reader.FieldByLocale("Headline", selectedLocale);

							if(UseHtmlEditor)
								radCopy.Content = reader.FieldByLocale("NewsCopy", selectedLocale);
							else
								txtCopy.Text = reader.FieldByLocale("NewsCopy", selectedLocale);

							txtDate.SelectedDate = reader.FieldDateTime("ExpiresOn");
							cbxPublished.Checked = reader.FieldBool("Published");
						}
				}
			}

			StoresMapping.ObjectID = RecordId;
			StoresMapping.DataBind();
			divStoreMapping.Visible = StoresMapping.StoreCount > 1;
		}

		void SaveNewsItem()
		{
			var editing = RecordId != 0;
			var selectedLocale = LocaleSelector.GetSelectedLocale().Name;
			var expirationDate = txtDate.SelectedDate ?? System.DateTime.Now.AddMonths(1);
			var headline = editing
				? AppLogic.FormLocaleXml("Headline", txtHeadline.Text.Trim(), selectedLocale, "News", RecordId)
				: AppLogic.FormLocaleXml(txtHeadline.Text.Trim(), selectedLocale);

			var copyContent = UseHtmlEditor
				? radCopy.Content
				: txtCopy.Text;

			var copy = editing
				? AppLogic.FormLocaleXml("NewsCopy", copyContent, selectedLocale, "News", RecordId)
				: AppLogic.FormLocaleXml(copyContent, selectedLocale);

			var parameters = new[]
				{
					new SqlParameter("@newsId", RecordId),
					new SqlParameter("@headline", headline),
					new SqlParameter("@copy", copy),
					new SqlParameter("@expirationDate", expirationDate),
					new SqlParameter("@published", cbxPublished.Checked)
				};

			var query = editing
				? "UPDATE News SET Headline = @headline, NewsCopy = @copy, Published = @published, ExpiresOn = @expirationDate WHERE NewsID = @newsID"
				: "INSERT News (Headline, NewsCopy, Published, ExpiresOn) VALUES (@headline, @copy, @published, @expirationDate); select cast(SCOPE_IDENTITY() as int) N;";

			var identity = DB.GetSqlN(query, parameters);
			if(!editing)
				RecordId = identity;

			StoresMapping.ObjectID = RecordId;
			StoresMapping.Save();
		}
	}
}
