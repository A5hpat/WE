// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class importstringresourcefile1 : AspDotNetStorefront.Admin.AdminPageBase
	{
		string ShowLocaleSetting;

		protected void Page_Load(object sender, EventArgs e)
		{
			Server.ScriptTimeout = 1000000;

			ShowLocaleSetting = CommonLogic.QueryStringCanBeDangerousContent("ShowLocaleSetting");
			
			var isMasterReload = CommonLogic.QueryStringBool("master");
			pnlReload.Visible = isMasterReload;
			pnlUpload.Visible = !isMasterReload;

			if(isMasterReload && !StringResourceManager.CheckStringResourceExcelFileExists(ShowLocaleSetting))
			{
				ctlAlertMessage.PushAlertMessage("The server does not have any string resource files for the chosen locale.", AlertMessage.AlertType.Error);
				pnlReload.Visible = false;
			}

			if(!IsPostBack)
			{
				litSelectFileInstructions.Text = String.Format(AppLogic.GetStringForDefaultLocale("admin.stringresources.SelectFile"), ShowLocaleSetting);
				lnkBack1.NavigateUrl = AppLogic.AdminLinkUrl("stringresources.aspx") + "?filterlocale=" + Localization.CheckLocaleSettingForProperCase(ShowLocaleSetting);
				lnkBack2.NavigateUrl = AppLogic.AdminLinkUrl("stringresources.aspx") + "?filterlocale=" + Localization.CheckLocaleSettingForProperCase(ShowLocaleSetting);

				if(isMasterReload)
					litStage.Text = string.Format(AppLogic.GetStringForDefaultLocale("admin.stringresources.ReloadMaster"), ShowLocaleSetting);
				else
					litStage.Text = string.Format(AppLogic.GetStringForDefaultLocale("admin.stringresources.ImportFile1"), ShowLocaleSetting);
			}
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			var option = StringResourceImporter.ImportOption.Default;
			if(chkLeaveModified.Checked)
				option = option | StringResourceImporter.ImportOption.LeaveModified;
			
			if(chkReplaceExisting.Checked)
				option = option | StringResourceImporter.ImportOption.OverWrite;

			// handle file upload:
			try
			{
				var extension = System.IO.Path.GetExtension(fuMain.FileName);
				var spreadsheetName = String.Format(
					"Strings_{0}{1}", 
					Localization.ToThreadCultureShortDateString(System.DateTime.Now).Replace(" ", "").Replace("/", "").Replace(":", "").Replace(".", ""), 
					extension);

				var targetFile = CommonLogic.SafeMapPath(String.Format("~/images/{0}", spreadsheetName));
				fuMain.SaveAs(targetFile);

				var message = String.Format(
					@"<a href=""{0}?spreadsheetname={1}&showlocalesetting={2}&option={3}"">{4}</a>",
					AppLogic.AdminLinkUrl("importstringresourcefile2.aspx"),
					spreadsheetName,
					ShowLocaleSetting,
					(int)option,
					AppLogic.GetString("admin.importstringresourcefile1.UploadSuccessful", SkinID, LocaleSetting));

				ctlAlertMessage.PushAlertMessage(message, AlertMessage.AlertType.Success);
			}
			catch(Exception exception)
			{
				ctlAlertMessage.PushAlertMessage(String.Format("admin.importstringresourcefile1.UploadError".StringResource(), exception), AlertMessage.AlertType.Error);
			}
		}

		protected void btnReload_Click(object sender, EventArgs e)
		{
			var option = StringResourceImporter.ImportOption.Default;
			if(chkReloadLeaveModified.Checked)
				option = option | StringResourceImporter.ImportOption.LeaveModified;

			if(chkReloadReplaceExisting.Checked)
				option = option | StringResourceImporter.ImportOption.OverWrite;

			var redirectUrl = String.Format(
				"{0}?master=true&showlocalesetting={1}&option={2}",
				AppLogic.AdminLinkUrl("importstringresourcefile2.aspx"),
				ShowLocaleSetting,
				(int)option);

			Response.Redirect(redirectUrl);
		}
	}
}
