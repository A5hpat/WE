// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class importstringresourcefile2 : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected string ShowLocaleSetting;
		IEnumerable<string> ImportFiles;

		readonly StringResourceImporter StringResourceImporter;

		public importstringresourcefile2()
		{
			StringResourceImporter = new StringResourceImporter();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			ShowLocaleSetting = Localization.CheckLocaleSettingForProperCase(CommonLogic.QueryStringCanBeDangerousContent("ShowLocaleSetting"));
			var isMasterStringResource = CommonLogic.QueryStringBool("master");

			if(isMasterStringResource)
			{
				if(!StringResourceManager.CheckStringResourceExcelFileExists(ShowLocaleSetting))
				{
					AlertMessageControl.PushAlertMessage(AppLogic.GetString("admin.StringResources.NoFileFound", SkinID, ShowLocaleSetting), AlertMessage.AlertType.Error);
					return;
				}

				ImportFiles = StringResourceManager.GetStringResourceFilesForLocale(ShowLocaleSetting);
			}
			else
			{
				ImportFiles = new[] { CommonLogic.SafeMapPath("~/images/" + CommonLogic.QueryStringCanBeDangerousContent("SpreadsheetName")) };
			}

			if(!IsPostBack)
			{
				CancelLink.NavigateUrl = String.Format("{0}?filterlocale={1}", AppLogic.AdminLinkUrl("stringresources.aspx"), ShowLocaleSetting);

				if(isMasterStringResource)
				{
					btnProcessFile.Text = AppLogic.GetString("admin.importstringresourcefile2.BeginReload", SkinID, LocaleSetting);
					ltProcessing.Text = String.Format(AppLogic.GetString("admin.importstringresourcefile2.ReloadLocale", SkinID, LocaleSetting), ShowLocaleSetting);
				}
				else
				{
					btnProcessFile.Text = AppLogic.GetString("admin.importstringresourcefile2.BeginImport", SkinID, LocaleSetting);
					ltProcessing.Text = String.Format(AppLogic.GetString("admin.importstringresourcefile2.ProcessingFile", SkinID, LocaleSetting), ImportFiles.First());
				}

				// Preview the data
				try
				{
					var validationResult = StringResourceImporter.Validate(ShowLocaleSetting, ImportFiles, (StringResourceImporter.ImportOption)CommonLogic.QueryStringUSInt("option"));
					DataReportGrid.DataSource = validationResult;
				}
				catch(Exception exception)
				{
					while(exception.InnerException != null)
						exception = exception.InnerException;

					AlertMessageControl.PushAlertMessage(exception.Message, AlertMessage.AlertType.Error);
					ActionsPanel.Visible = false;
				}
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			Page.DataBind();
		}

		protected void btnProcessFile_Click(object sender, EventArgs e)
		{
			ActionsPanel.Visible = false;

			try
			{
				var importResult = StringResourceImporter.Import(ShowLocaleSetting, ImportFiles, (StringResourceImporter.ImportOption)CommonLogic.QueryStringUSInt("option"));
				DataReportGrid.DataSource = importResult;

				StringResourceManager.LoadAllStrings(false);
				AlertMessageControl.PushAlertMessage(AppLogic.GetString("admin.importstringresourcefile2.Done", SkinID, LocaleSetting), AlertMessage.AlertType.Success);
			}
			catch(Exception exception)
			{
				AlertMessageControl.PushAlertMessage(String.Format(AppLogic.GetString("admin.importstringresourcefile2.ErrorProcessingStrings", SkinID, LocaleSetting), exception), AlertMessage.AlertType.Error);
			}
		}

		protected void DataReportGrid_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if(e.Row.RowType != DataControlRowType.DataRow)
				return;

			e.Row.CssClass = ((DataRowView)e.Row.DataItem)
				.Row
				.Field<string>("Status")
				.StartsWith(AppLogic.ro_OK)
					? "success"
					: String.Empty;
		}

		protected void DataReportGrid_DataBinding(object sender, EventArgs e)
		{
			// Hide StoreID column if there is only a single store
			DataReportGrid
				.Columns
				.OfType<DataControlField>()
				.Last()
				.Visible = AspDotNetStorefrontCore.Store.IsMultiStore;
		}
	}
}
