// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for recurringimport.
	/// </summary>
	public partial class recurringimport : AspDotNetStorefront.Admin.AdminPageBase
	{
		String RecurringGateway;
		DateTime LastImportDate = System.DateTime.MinValue;

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			if(!AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling"))
			{
				DisablePageDisplay(AppLogic.GetString("admin.recurringimport.NotSupported", LocaleSetting));
				return;
			}
			else if(!AppLogic.ThereAreRecurringGatewayAutoBillOrders())	//This checks the same AppConfig again, but tells us if Gateway Internal Billing is on and there's just nothing to do
			{
				DisablePageDisplay(AppLogic.GetString("admin.recurringimport.NoRecurringOrders", LocaleSetting));
				return;
			}

			LastImportDate = Localization.ParseDBDateTime(AppLogic.AppConfig("Recurring.GatewayLastImportedDate"));
			RecurringGateway = AppLogic.ActivePaymentGatewayCleaned();

			if(!IsPostBack)
			{
				SetupPageDisplay();
			}
		}

		private void DisablePageDisplay(string message)
		{
			pnlMain.Visible = btnGetGatewayStatus.Enabled = false;
			divUnsupportedWarning.Visible = true;
			litUnsupportedWarning.Text = message;
		}

		private void SetupPageDisplay()
		{
			if(LastImportDate > System.DateTime.MinValue)
			{
				LastRunPanel.Visible = true;
				lblLastRun.Text = String.Format(AppLogic.GetString("admin.recurringimport.LastImport", LocaleSetting), Localization.ToThreadCultureShortDateString(LastImportDate));
			}

			btnGetGatewayStatus.Text = String.Format(AppLogic.GetString("admin.recurringimport.GetAutoBillStatusFile", LocaleSetting),
				LastImportDate > System.DateTime.MinValue
				? AppLogic.GetString("admin.recurringimport.Next", LocaleSetting)
				: AppLogic.GetString("admin.recurringimport.Todays", SkinID, LocaleSetting),
				RecurringGateway);

			if(LastImportDate.AddDays(1) >= DateTime.Today)
			{
				txtInputFile.Text = AppLogic.GetString("admin.recurringimport.NothingToProcess", SkinID, LocaleSetting);
				btnGetGatewayStatus.Enabled = false;
			}

			GatewayProcessor GWActual = GatewayLoader.GetProcessor(RecurringGateway);
			if(GWActual != null && GWActual.RecurringSupportType() == RecurringSupportType.Normal)
			{
				btnGetGatewayStatus.Visible = true;
				pnlMain.Visible = true;
			}
			else if(GWActual != null && GWActual.RecurringSupportType() == RecurringSupportType.Extended)
			{
				btnGetGatewayStatus.Visible = false;
				btnProcessFile.Visible = true;
				pnlMain.Visible = true;
				PastePromptLabel.Text = PastePromptLabel.Text + "<br />" + AppLogic.GetString("admin.recurringimport.RawTextContents", SkinID, LocaleSetting);
			}
			else
			{
				DisablePageDisplay(AppLogic.GetString("admin.recurringimport.NotSupported", LocaleSetting));
				return;
			}
		}

		protected void btnGetGatewayStatus_Click(object sender, EventArgs e)
		{
			try
			{
				txtResults.Text = "";
				btnGetGatewayStatus.Enabled = false;
				RecurringOrderMgr rmgr = new RecurringOrderMgr();
				btnProcessFile.Visible = true;
				btnProcessFile.Enabled = true;
				String sResults = String.Empty;
				String Status = rmgr.GetAutoBillStatusFile(RecurringGateway, out sResults);
				if(Status == AppLogic.ro_OK)
				{
					txtInputFile.Text = sResults;
				}
				else
				{
					txtInputFile.Text = Status;
				}
			}
			catch (Exception exception)
			{
				SysLog.LogException(exception, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
			}
		}

		protected void btnProcessFile_Click(object sender, EventArgs e)
		{
			txtResults.Visible = true;

			if(RecurringGateway == Gateway.ro_GWPAYFLOWPRO)
			{
				btnProcessFile.Enabled = false;
			}

			DateTime dtRun = LastImportDate;
			if(dtRun == System.DateTime.MinValue)
			{
				dtRun = DateTime.Today.AddDays(-1); // Defaults to yesterday
			}

			if(dtRun >= DateTime.Today &&
				(RecurringGateway == Gateway.ro_GWPAYFLOWPRO))
			{
				txtInputFile.Text = AppLogic.GetString("admin.recurringimport.NothingToProcess", SkinID, LocaleSetting);
				btnGetGatewayStatus.Enabled = false;
				return;
			}

			if(txtInputFile.Text.Length == 0)
			{
				txtResults.Text = AppLogic.GetString("admin.recurringimport.NothingToProcessForget", SkinID, LocaleSetting);
			}
			else
			{
				RecurringOrderMgr rmgr = new RecurringOrderMgr();
				String sResults = String.Empty;
				String Status = rmgr.ProcessAutoBillStatusFile(RecurringGateway, txtInputFile.Text, out sResults);
				if(Status == AppLogic.ro_OK)
				{
					txtResults.Text = sResults;
				}
				else
				{
					txtResults.Text = Status;
				}
			}

			btnGetGatewayStatus.Enabled = true;
			AppConfigManager.SetAppConfigValue("Recurring.GatewayLastImportedDate", Localization.ToDBDateTimeString(dtRun));
			LastRunPanel.Visible = true;
			lblLastRun.Text = String.Format(AppLogic.GetString("admin.recurringimport.LastImport", SkinID, LocaleSetting), Localization.ToThreadCultureShortDateString(dtRun));
			LastImportDate = dtRun;
		}
	}
}
