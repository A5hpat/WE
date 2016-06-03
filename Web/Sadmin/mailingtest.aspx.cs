// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class MailingTest : AspDotNetStorefront.Admin.AdminPageBase
	{
		readonly XmlPackageManager XmlPackageManager;

		public MailingTest()
		{
			XmlPackageManager = new XmlPackageManager();
		}

		protected void Page_Load(Object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			InitializeEditor();
		}

		protected void ssOne_SelectedIndexChanged(Object sender, EventArgs e)
		{
			LoadContent(ssOne.SelectedStoreID);
		}

		protected void btnUpdateAppConfigs_Click(Object sender, EventArgs e)
		{
			UpdateAppConfigs(ssOne.SelectedStoreID);
			ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.mailingtest.ConfigSavedSuccessfully", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
		}

		protected void btnSendTestReceipt_Click(Object sender, EventArgs e)
		{
			UpdateAppConfigs(ssOne.SelectedStoreID);

			if(!AppLogic.AppConfigBool("SendOrderEMailToCustomer"))
			{
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("mailingtest.aspx.8", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return;
			}

			try
			{
				SendTestReceiptEmail(ssOne.SelectedStoreID);
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("mailingtest.aspx.1", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
			catch(Exception exception)
			{
				ctrlAlertMessage.PushAlertMessage(GenerateExceptionMessage(exception, ssOne.SelectedStoreID), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}
		}

		protected void btnSendNewOrderNotification_Click(Object sender, EventArgs e)
		{
			UpdateAppConfigs(ssOne.SelectedStoreID);

			if(AppLogic.AppConfigBool("TurnOffStoreAdminEMailNotifications"))
			{
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("mailingtest.aspx.5", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return;
			}

			try
			{
				SendTestNewOrderNotification(ssOne.SelectedStoreID);
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("mailingtest.aspx.2", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
			catch(Exception exception)
			{
				ctrlAlertMessage.PushAlertMessage(GenerateExceptionMessage(exception, ssOne.SelectedStoreID), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}
		}

		protected void btnSendTestShipped_Click(Object sender, EventArgs e)
		{
			UpdateAppConfigs(ssOne.SelectedStoreID);

			if(!AppLogic.AppConfigBool("SendShippedEMailToCustomer"))
			{
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("mailingtest.aspx.11", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return;
			}

			try
			{
				SendTestShippedEmail(ssOne.SelectedStoreID);
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("mailingtest.aspx.10", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
			catch(Exception exception)
			{
				ctrlAlertMessage.PushAlertMessage(GenerateExceptionMessage(exception, ssOne.SelectedStoreID), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}
		}

		protected void btnSendAll_Click(Object sender, EventArgs e)
		{
			btnSendTestReceipt_Click(sender, e);
			btnSendNewOrderNotification_Click(sender, e);
			btnSendTestShipped_Click(sender, e);
		}

		void InitializeEditor()
		{
			ssOne.Visible = ssOne.StoreCount > 1;

			if(!ThisCustomer.IsAdminSuperUser)
			{
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.InsufficientPermissions", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				btnSendAll.Enabled = btnSendAllTop.Enabled = false;
				btnSendNewOrderNotification.Enabled = btnSendNewOrderNotificationTop.Enabled = false;
				btnSendTestReceipt.Enabled = btnSendTestReceiptTop.Enabled = false;
				btnSendTestShipped.Enabled = btnSendTestShippedTop.Enabled = false;
			}

			if(IsPostBack)
				return;

			if(ThisCustomer.IsAdminSuperUser)
				LoadContent(AppLogic.StoreID());
		}

		void LoadContent(Int32 storeId)
		{
			txtMailMe_Server.Text = AppLogic.GetAppConfigRouted("MailMe_Server", storeId).ConfigValue;
			txtMailServerPort.Text = AppLogic.GetAppConfigRouted("MailMe_Port", storeId).ConfigValue;
			txtMailServerUser.Text = AppLogic.GetAppConfigRouted("MailMe_User", storeId).ConfigValue;
			txtMailServerPwd.Text = AppLogic.GetAppConfigRouted("MailMe_Pwd", storeId).ConfigValue;
			rblMailServerSSL.SelectedValue = AppLogic.GetAppConfigRouted("MailMe_UseSSL", storeId).ConfigValue;

			txtReceiptFrom.Text = AppLogic.GetAppConfigRouted("ReceiptEMailFrom", storeId).ConfigValue;
			txtReceiptFromName.Text = AppLogic.GetAppConfigRouted("ReceiptEMailFromName", storeId).ConfigValue;
			txtOrderNotificationFrom.Text = AppLogic.GetAppConfigRouted("GotOrderEMailFrom", storeId).ConfigValue;
			txtOrderNotificationFromName.Text = AppLogic.GetAppConfigRouted("GotOrderEMailFromName", storeId).ConfigValue;

			txtOrderNotificationTo.Text = AppLogic.GetAppConfigRouted("GotOrderEMailTo", storeId).ConfigValue;
			txtOrderNotificationToName.Text = AppLogic.GetAppConfigRouted("MailMe_ToName", storeId).ConfigValue;

			rblSendReceipts.SelectedValue = AppLogic.GetAppConfigRouted("SendOrderEMailToCustomer", storeId).ConfigValue;
			rblSendShippedNotifications.SelectedValue = AppLogic.GetAppConfigRouted("SendShippedEMailToCustomer", storeId).ConfigValue;
			rblSendOrderNotifications.SelectedValue = AppLogic.GetAppConfigRouted("TurnOffStoreAdminEMailNotifications", storeId).ConfigValue;

			var orderReceiptXmlPackage = AppLogic.GetAppConfigRouted("XmlPackage.OrderReceipt", storeId).ConfigValue;
			ddXmlPackageReceipt.SelectFirstByValue(orderReceiptXmlPackage);

			var orderNotificationXmlPackage = AppLogic.GetAppConfigRouted("XmlPackage.NewOrderAdminNotification", storeId).ConfigValue;
			ddXmlPackageOrderNotifications.SelectFirstByValue(orderNotificationXmlPackage);

			var orderShippedXmlPackage = AppLogic.GetAppConfigRouted("XmlPackage.OrderShipped", storeId).ConfigValue;
			ddXmlPackageShipped.SelectFirstByValue(orderShippedXmlPackage);
		}

		void UpdateAppConfigs(Int32 storeId)
		{
			UpdateAppConfig(storeId, "MailMe_Server", txtMailMe_Server.Text);
			UpdateAppConfig(storeId, "MailMe_Port", txtMailServerPort.Text);
			UpdateAppConfig(storeId, "MailMe_User", txtMailServerUser.Text);
			UpdateAppConfig(storeId, "MailMe_Pwd", txtMailServerPwd.Text);
			UpdateAppConfig(storeId, "MailMe_UseSSL", rblMailServerSSL.SelectedValue);

			UpdateAppConfig(storeId, "MailMe_FromAddress", txtReceiptFrom.Text);
			UpdateAppConfig(storeId, "MailMe_FromName", txtReceiptFromName.Text);
			UpdateAppConfig(storeId, "MailMe_ToAddress", txtOrderNotificationTo.Text);
			UpdateAppConfig(storeId, "MailMe_ToName", txtOrderNotificationToName.Text);

			UpdateAppConfig(storeId, "GotOrderEMailTo", txtOrderNotificationTo.Text);
			UpdateAppConfig(storeId, "GotOrderEMailFrom", txtOrderNotificationFrom.Text);
			UpdateAppConfig(storeId, "GotOrderEMailFromName", txtOrderNotificationFromName.Text);

			UpdateAppConfig(storeId, "ReceiptEMailFrom", txtReceiptFrom.Text);
			UpdateAppConfig(storeId, "ReceiptEMailFromName", txtReceiptFromName.Text);

			UpdateAppConfig(storeId, "SendOrderEMailToCustomer", rblSendReceipts.SelectedValue);
			UpdateAppConfig(storeId, "SendShippedEMailToCustomer", rblSendShippedNotifications.SelectedValue);
			UpdateAppConfig(storeId, "TurnOffStoreAdminEMailNotifications", rblSendOrderNotifications.SelectedValue);

			UpdateAppConfig(storeId, "XmlPackage.NewOrderAdminNotification", (ddXmlPackageOrderNotifications.SelectedValue != "0") ? ddXmlPackageOrderNotifications.SelectedValue.ToLowerInvariant() : "notification.adminneworder.xml.config");
			UpdateAppConfig(storeId, "XmlPackage.OrderReceipt", (ddXmlPackageReceipt.SelectedValue != "0") ? ddXmlPackageReceipt.SelectedValue.ToLowerInvariant() : "notification.receipt.xml.config");
			UpdateAppConfig(storeId, "XmlPackage.OrderShipped", (ddXmlPackageShipped.SelectedValue != "0") ? ddXmlPackageShipped.SelectedValue.ToLowerInvariant() : "notification.shipped.xml.config");
		}

		void UpdateAppConfig(Int32 storeId, String name, String value)
		{
			if(!AppConfigManager.AppConfigExists(name, storeId))
				AppConfigManager.AddAppConfig(name, string.Empty, value, "string", null, "EMAIL", false, storeId);
			else
				AppConfigManager.SetAppConfigValue(name, value, storeId);
		}

		void SendTestReceiptEmail(Int32 storeId)
		{
			var xmlPackageName = AppLogic.AppConfig("XmlPackage.OrderReceipt");
			var xmlPackage = new XmlPackage(
				packageName: xmlPackageName,
				skinId: SkinID,
				additionalRuntimeParms: "ordernumber=999999");

			var subject = String.Format(AppLogic.GetString("common.cs.2", SkinID, LocaleSetting), AppLogic.AppConfig("StoreName", ssOne.SelectedStoreID, true));
			var body = xmlPackage.TransformString();

			AppLogic.SendMail(subject: subject, body: body + AppLogic.AppConfig("MailFooter"), useHtml: true, fromAddress: AppLogic.AppConfig("ReceiptEMailFrom", storeId, true), fromName: AppLogic.AppConfig("ReceiptEMailFromName", storeId, true), toAddress: ThisCustomer.EMail, toName: String.Empty, bccAddresses: String.Empty, server: AppLogic.MailServer());
		}

		void SendTestShippedEmail(Int32 storeId)
		{
			var xmlPackageName = AppLogic.AppConfig("XmlPackage.OrderShipped");
			var xmlPackage = new XmlPackage(
				packageName: xmlPackageName,
				skinId: SkinID,
				additionalRuntimeParms: "ordernumber=999999");

			var subject = String.Format(AppLogic.GetString("common.cs.2", SkinID, LocaleSetting), AppLogic.AppConfig("StoreName", storeId, true));
			var body = xmlPackage.TransformString();

			AppLogic.SendMail(subject: subject, body: body + AppLogic.AppConfig("MailFooter"), useHtml: true, fromAddress: AppLogic.AppConfig("ReceiptEMailFrom", storeId, true), fromName: AppLogic.AppConfig("ReceiptEMailFromName", storeId, true), toAddress: ThisCustomer.EMail, toName: String.Empty, bccAddresses: String.Empty, server: AppLogic.MailServer());
		}

		void SendTestNewOrderNotification(Int32 storeId)
		{
			var xmlPackageName = AppLogic.AppConfig("XmlPackage.NewOrderAdminNotification");
			var xmlPackage = new XmlPackage(
				packageName: xmlPackageName,
				skinId: SkinID,
				additionalRuntimeParms: "ordernumber=999999");

			var subject = String.Format(AppLogic.GetString("common.cs.5", SkinID, LocaleSetting), AppLogic.AppConfig("StoreName", storeId, true));
			var body = xmlPackage.TransformString();

			var SendToList = AppLogic.AppConfig("GotOrderEMailTo").ToString().Replace(",", ";");
			if(SendToList.IndexOf(';') != -1)
				foreach(String s in SendToList.Split(';'))
					AppLogic.SendMail(subject: subject, body: body + AppLogic.AppConfig("MailFooter", storeId, true), useHtml: true, fromAddress: AppLogic.AppConfig("GotOrderEMailFrom", storeId, true), fromName: AppLogic.AppConfig("GotOrderEMailFromName", storeId, true), toAddress: s.Trim(), toName: s.Trim(), bccAddresses: String.Empty, server: AppLogic.MailServer());
			else
				AppLogic.SendMail(subject: subject, body: body + AppLogic.AppConfig("MailFooter", storeId, true), useHtml: true, fromAddress: AppLogic.AppConfig("GotOrderEMailFrom", storeId, true), fromName: AppLogic.AppConfig("GotOrderEMailFromName", storeId, true), toAddress: SendToList, toName: SendToList, bccAddresses: String.Empty, server: AppLogic.MailServer());
		}

		String GenerateExceptionMessage(Exception exception, Int32 storeId)
		{
			var MailMe_PwdLen = AppLogic.AppConfig("MailMe_Pwd", storeId, true).ToString().Length;
			var MailMe_UserLen = AppLogic.AppConfig("MailMe_User", storeId, true).ToString().Length;

			var retVal = String.Empty;

			if(exception.Message.ToString().IndexOf("AUTHENTICATION", StringComparison.InvariantCultureIgnoreCase) != -1 || exception.Message.ToString().IndexOf("OBJECT REFERENCE", StringComparison.InvariantCultureIgnoreCase) != -1 || exception.Message.ToString().IndexOf("NO SUCH USER HERE", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				if(MailMe_UserLen == 0 && MailMe_PwdLen == 0)
					retVal = AppLogic.GetString("mailingtest.aspx.3", SkinID, LocaleSetting) + "<br/>&nbsp;·" + AppLogic.GetString("mailingtest.aspx.7", SkinID, LocaleSetting) + "<br/>&nbsp;·" + AppLogic.GetString("mailingtest.aspx.6", SkinID, LocaleSetting);
				else if(MailMe_UserLen == 0)
					retVal = AppLogic.GetString("mailingtest.aspx.3", SkinID, LocaleSetting) + "<br/>&nbsp;·" + AppLogic.GetString("mailingtest.aspx.7", SkinID, LocaleSetting);
				else if(MailMe_PwdLen == 0)
					retVal = AppLogic.GetString("mailingtest.aspx.3", SkinID, LocaleSetting) + "<br/>&nbsp;·" + AppLogic.GetString("mailingtest.aspx.6", SkinID, LocaleSetting);
				else
					retVal = AppLogic.GetString("mailingtest.aspx.3", SkinID, LocaleSetting) + "<br/>&nbsp;·" + AppLogic.GetString("mailingtest.aspx.9", SkinID, LocaleSetting);

				if(retVal.Length != 0)
					return retVal;
			}

			return AppLogic.GetString("mailingtest.aspx.3", SkinID, LocaleSetting) + "<br/>&nbsp;·" + exception.Message.ToString();
		}
	}
}
