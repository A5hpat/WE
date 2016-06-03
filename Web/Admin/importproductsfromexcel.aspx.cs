// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Text;
using System.Web;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for importProductsFromXML
	/// </summary>
	public partial class importProductsFromExcel : AspDotNetStorefront.Admin.AdminPageBase
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
			Server.ScriptTimeout = 1000000;

			StringBuilder importRestrictionMessage = new StringBuilder(300);

			if (AppLogic.MaxProductsExceeded())
			{
				btnUpload.Enabled = false;
				importRestrictionMessage.Append("<font class=\"text-danger\">" + AppLogic.GetString("admin.common.ImportExcession", SkinID, LocaleSetting) + " ");
				importRestrictionMessage.Append(AppLogic.GetString("admin.common.NeedUpgrade", SkinID, LocaleSetting) + "</font>");
				AlertMessage.PushAlertMessage(importRestrictionMessage.ToString(), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}

			btnUpload.Enabled = true;

			if (!IsPostBack)
			{
				divReview.Visible = false;
			}
			Page.Form.DefaultButton = btnUpload.UniqueID;
		}

		protected void btnUpload_Click(object sender, EventArgs e)
		{
			String XlsName = "Import_" + Localization.ToNativeDateTimeString(System.DateTime.Now).Replace(" ", "").Replace("/", "").Replace(":", "").Replace(".", "");
			// handle file upload:
			try
			{
				String Image1 = String.Empty;
				HttpPostedFile Image1File = fuFile.PostedFile;
				String ExcelFile = CommonLogic.SafeMapPath("~/images" + "/" + XlsName + ".xls");
				if (Image1File.ContentLength != 0)
				{
					Image1File.SaveAs(ExcelFile);
					Import.ProcessExcelImportFile(ExcelFile);
					ltResults.Text = String.Format(AppLogic.GetString("admin.common.ViewImportLog", SkinID, LocaleSetting), "<a href=\"../images/import.htm\" target=\"_blank\">", "</a>");
					AlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.FileUploadedPleaseReviewBelow", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
					divReview.Visible = true;
				}
				else
				{
					AlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.NoDatatoImport", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				}

			}
			catch (Exception ex)
			{
				divReview.Visible = false;
				AlertMessage.PushAlertMessage(CommonLogic.GetExceptionDetail(ex, "<br/>"), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);

			}
		}

		protected void btnAccept_Click(object sender, EventArgs e)
		{
			DB.ExecuteLongTimeSQL("aspdnsf_ClearAllImportFlags", 1000);
			AlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.ImportAcceptedUC", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);

			divReview.Visible = false;
		}

		protected void btnUndo_Click(object sender, EventArgs e)
		{
			DB.ExecuteLongTimeSQL("aspdnsf_UndoImport", 1000);
			AlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.ImportHasBeenUndone", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);

			divReview.Visible = false;
		}
	}
}
