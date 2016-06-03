// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontBuySafe;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;
using AssetServer;

namespace AspDotNetStorefrontAdmin
{
	public partial class Wizard : AspDotNetStorefront.Admin.AdminPageBase
	{
		IDictionary<string, string[]> _PaymentOptionsByCountry = new Dictionary<string, string[]>();

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			if(!IsPostBack)
			{
				if(!ThisCustomer.IsAdminSuperUser)
				{
					ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.wizard.InsufficientPermission", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Warning);
					divMain.Visible = false;
				}
				else
				{
					divMain.Visible = true;
					loadData();
				}

				EncryptWebConfigRow.Visible = (AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted);
				MachineKeyRow.Visible = (AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted);
			}
			Page.Form.DefaultButton = btnSubmitBottom.UniqueID;
		}

		protected void ddlCountries_SelectedIndexChanged(Object sender, EventArgs e)
		{
			SetPaymentOptionVisibility(ddlCountries.SelectedValue);
			BuildGatewayList();
		}

		protected void repGateways_DataBinding(object sender, RepeaterItemEventArgs e)
		{
			if(e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
				return;

			var rb = e.Item.FindControl("rbGateway") as RadioButton;
			var btnConfigureGateway = e.Item.FindControl("btnConfigureGateway") as LinkButton;
			var imgPayPal = e.Item.FindControl("imgPayPal") as Image;

			var item = e.Item.DataItem as GatewayData;

			var trGateway = e.Item.FindControl("trGateway");
			if(trGateway != null)
				trGateway.Visible = IsPaymentOptionAvailable(item.DisplayName, ddlCountries.SelectedValue);

			if(item.DisplayName.Contains("PayPal"))
				imgPayPal.Visible = true;

			if(item.DisplayName.ToUpper().Contains("PAYFLOW"))
				imgPayPal.ImageUrl = "images/PayPal_OnBoarding_PayFlow.png";

			if(AppLogic.AppConfig("PaymentGateway", 0, false).EqualsIgnoreCase("PayFlowPro"))
			{
				var payFlowProProduct = AppConfigManager.GetAppConfig("PayFlowPro.Product");
				rb.Checked = item.DisplayName == payFlowProProduct.ConfigValue;
			}
			else
				rb.Checked = AppLogic.AppConfig("PaymentGateway", 0, false).EqualsIgnoreCase(item.GatewayIdentifier);

			if(item.IsInstalled)
			{
				var gp = GatewayLoader.GetProcessor(item.GatewayIdentifier);
				if(gp != null)
				{
					var atom = gp.GetConfigurationAtom();
					rb.Enabled = atom == null || atom.IsConfigured(0) || atom.IsConfigured(AppLogic.StoreID());
				}
			}
			else
			{
				rb.Enabled = false;
				btnConfigureGateway.Visible = false;
			}

			if(item.GatewayIdentifier != null && item.GatewayIdentifier.EqualsIgnoreCase("manual"))
				btnConfigureGateway.Visible = false;
		}

		void SetGatewayRBEnabled()
		{
			foreach(RepeaterItem e in repGateways.Items)
			{
				var rb = e.FindControl("rbGateway") as RadioButton;
				var btnConfigureGateway = e.FindControl("btnConfigureGateway") as LinkButton;
				var hfGatewayIdentifier = e.FindControl("hfGatewayIdentifier") as HiddenField;

				try
				{
					var gp = GatewayLoader.GetProcessor(hfGatewayIdentifier.Value);

					var atom = gp.GetConfigurationAtom();
					rb.Enabled = atom == null || atom.IsConfigured(0) || atom.IsConfigured(AppLogic.StoreID());

				}
				catch // the gateway doesn't exist.
				{
					rb.Enabled = false;
					btnConfigureGateway.Visible = false;
				}
			}
		}

		protected override void OnInit(EventArgs e)
		{
			_PaymentOptionsByCountry.Add("PayPal Express Checkout", new string[] { "US", "CA", "AR", "AM", "AW", "AU", "AT", "BS", "BE", "BZ", "BM", "BO", "BR", "BG", "KY", "CL", "CN", "CO", "CR", "HR", "CY", "CZ", "DK", "DO", "EC", "FI", "FR", "DE", "GI", "GR", "GT", "HK", "HU", "IN", "ID", "IE", "IL", "IT", "JM", "JP", "JO", "KZ", "KW", "MY", "MX", "NL", "NZ", "NO", "PE", "PH", "PL", "PT", "QA", "RO", "RU", "SA", "SG", "SI", "ZA", "ES", "SE", "CH", "TW", "TH", "TR", "UA", "AE", "GB", "UY", "VE" });
			_PaymentOptionsByCountry.Add("PayPal Payments Advanced", new string[] { "US" });
			_PaymentOptionsByCountry.Add("PayPal Payments Standard", new string[] { "US", "CA", "AR", "AM", "AW", "AU", "AT", "BS", "BE", "BZ", "BM", "BO", "BR", "BG", "KY", "CL", "CN", "CO", "CR", "HR", "CY", "CZ", "DK", "DO", "EC", "FI", "FR", "DE", "GI", "GR", "GT", "HK", "HU", "IN", "ID", "IE", "IL", "IT", "JM", "JP", "JO", "KZ", "KW", "MY", "MX", "NL", "NZ", "NO", "PE", "PH", "PL", "PT", "QA", "RO", "RU", "SA", "SG", "SI", "ZA", "ES", "SE", "CH", "TW", "TH", "TR", "UA", "AE", "GB", "UY", "VE" });
			_PaymentOptionsByCountry.Add("PayPal PayFlow Link Only", new string[] { "US", "CA" });
			_PaymentOptionsByCountry.Add("PayPal PayFlow Pro", new string[] { "US", "CA", "AU", "NZ" });
			_PaymentOptionsByCountry.Add("PayPal Website Payments Pro", new string[] { "US", "UK", "CA" });

			if(!IsPostBack)
			{
				using(var connection = new System.Data.SqlClient.SqlConnection(DB.GetDBConn()))
				{
					connection.Open();
					using(var reader = DB.GetRS("select TwoLetterISOCode, Name from Country Order By DisplayOrder", connection))
					{
						ddlCountries.DataSource = reader;
						ddlCountries.DataValueField = "TwoLetterISOCode";
						ddlCountries.DataTextField = "Name";
						ddlCountries.DataBind();
					}
					ddlCountries.SelectedIndex = 0;
				}

				BuildGatewayList();
			}

			base.OnInit(e);
		}

		protected void repGateways_ItemCommand(object sender, RepeaterCommandEventArgs e)
		{
			if(!e.CommandName.EqualsIgnoreCase("ShowConfiguration"))
				return;

			var gp = GatewayLoader.GetProcessor(e.CommandArgument as string);
			if(gp == null)
				return;

			var hfGatewayIdentifier = e.Item.FindControl("hfGatewayIdentifier") as HiddenField;
			if(hfGatewayIdentifier != null && hfGatewayIdentifier.Value == "PayFlowPro")
			{
				var hfGatewayProductIdentifier = e.Item.FindControl("hfGatewayProductIdentifier") as HiddenField;
				LaunchGatewayConfiguration(gp, string.Format("Gateway.{0}.ConfigAtom.xml", hfGatewayProductIdentifier.Value.Replace(" ", "").Replace(" ", "")));
			}
			else
				LaunchGatewayConfiguration(gp);
		}

		void LaunchGatewayConfiguration(GatewayProcessor gateway)
		{
			LaunchGatewayConfiguration(gateway, string.Empty);
		}

		void LaunchGatewayConfiguration(GatewayProcessor gateway, string resourceName)
		{
			GatewayConfigurationAtom.AtomConfigurationDataSource = gateway.GetConfigurationAtom(resourceName);
			GatewayConfigurationAtom.DataBind();
			GatewayConfigurationAtom.Show();
		}

		protected void ShowModalAtomByXMLFile_Click(object sender, EventArgs e)
		{
			var lb = sender as LinkButton;
			FileConfigurationAtom.SetConfigurationFile(lb.CommandArgument);
			FileConfigurationAtom.Show();
		}

		protected void GatewayConfigurationAtom_Saved(object sender, EventArgs e)
		{
			SetGatewayRBEnabled();
		}

		protected void loadData()
		{
			var BadSSL = CommonLogic.QueryStringBool("BadSSL");
			if(BadSSL)
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.wizard.BadSSL", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Error);


			var PM = AppLogic.AppConfig("PaymentMethods", 0, false).ToUpperInvariant();

			BuildPaymentMethodList(PM);

			if(AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted)
			{
				var webconfig = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
				var appsettings = (AppSettingsSection)webconfig.GetSection("appSettings");
				var connectionStrings = (ConnectionStringsSection)webconfig.GetSection("connectionStrings");

				rblEncrypt.Items.FindByValue((appsettings.SectionInformation.IsProtected && connectionStrings.SectionInformation.IsProtected).ToString().ToLowerInvariant()).Selected = true;

				var mkeysec = (MachineKeySection)webconfig.GetSection("system.web/machineKey");
				if(mkeysec.ValidationKey.Equals("autogenerate", StringComparison.InvariantCultureIgnoreCase))
				{
					rblStaticMachineKey.Items.FindByValue("false").Selected = true;
					ltStaticMachineKey.Text = AppLogic.GetString("admin.wizard.SetStaticMachineKey", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				}
				else
				{
					rblStaticMachineKey.Items.FindByValue("false").Selected = true;
					ltStaticMachineKey.Text = AppLogic.GetString("admin.wizard.ChangeStaticMachineKey", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				}

			}

			switch(BuySafeController.GetBuySafeState())
			{
				case BuySafeState.NotEnabledFreeTrialAvailable:
					break;
				case BuySafeState.EnabledFullUserAfterFreeTrial:
				case BuySafeState.EnabledOnFreeTrial:
					pnlBuySafeActive.Visible = true;
					pnlBuySafeInactive.Visible = false;
					litBuySafeActiveMsg.Text = "buySAFE is enabled";
					break;
				case BuySafeState.EnabledFreeTrialExpired:
				case BuySafeState.NotEnabledFreeTrialExpired:
					pnlBuySafeActive.Visible = true;
					pnlBuySafeInactive.Visible = false;
					litBuySafeActiveMsg.Text = "<span style='line-height:normal;'>Trial expired. Please contact buySAFE to enable your account.<br />Call 1.888.926.6333 | Email: <a href='mailto:customersupport@buysafe.com'>customersupport@buysafe.com</a></span>";
					break;
				case BuySafeState.Error:
					pnlBuySafeActive.Visible = true;
					pnlBuySafeInactive.Visible = false;
					litBuySafeActiveMsg.Text = "<span style='line-height:normal;'>Please contact buySAFE to enable your account.<br />Call 1.888.926.6333 | Email: <a href='mailto:customersupport@buysafe.com'>customersupport@buysafe.com</a></span>";
					break;
			}
		}

		public void BuildPaymentMethodList(string payMethod)
		{
			var paymentMethods = new List<string>();
			var paymentMethodsCommaSeparated = payMethod.Split(',');
			foreach(var paymentMethod in paymentMethodsCommaSeparated)
				paymentMethods.Add(paymentMethod.ToLowerInvariant().Trim());

			cbxCreditCard.Checked = paymentMethods.Contains("Credit Card".ToLowerInvariant());
			cbxAmazonPayments.Checked = paymentMethods.Contains("AmazonPayments".ToLowerInvariant());
			cbxPayPalExpress.Checked = paymentMethods.Contains("PayPalExpress".ToLowerInvariant());
			cbxRequestQuote.Checked = paymentMethods.Contains("Request Quote".ToLowerInvariant());
			cbxPurchaseOrder.Checked = paymentMethods.Contains("Purchase Order".ToLowerInvariant());
			cbxCheckByMail.Checked = paymentMethods.Contains("Check By Mail".ToLowerInvariant());
			cbxCOD.Checked = paymentMethods.Contains("C.O.D.".ToLowerInvariant());
			cbxMicroPay.Checked = paymentMethods.Contains("MICROPAY".ToLowerInvariant()) || ("MICROPAY".Equals(AppLogic.ro_PMMicropay, StringComparison.InvariantCultureIgnoreCase) && AppLogic.MicropayIsEnabled());
		}

		void SetPaymentOptionVisibility(string currentCountry)
		{
			if(string.IsNullOrEmpty(currentCountry))
				return;

			trPayPalExpress.Visible = IsPaymentOptionAvailable("PayPal Express Checkout", currentCountry);
		}

		Boolean IsPaymentOptionAvailable(string paymentOption, string currentCountry)
		{
			if(string.IsNullOrEmpty(paymentOption))
				return true;

			if(string.IsNullOrEmpty(currentCountry))
				return true;

			if(!_PaymentOptionsByCountry.ContainsKey(paymentOption))
				return true;

			return _PaymentOptionsByCountry[paymentOption].Contains(currentCountry);
		}

		public void BuildGatewayList()
		{
			var ds = new List<GatewayData>();
			var downloadLink = "<br /><a href='{1}' onclick='showGatewayDirections('{2}');'>{0}</a>";
			if(repGateways.DataSource == null)
			{
				var serverAssets = AssetServer.AssetServerAsset.GetAssetServerAssets();
				var availibleGateways = GatewayLoader.GetAvailableGatewayNames();

				foreach(var s in availibleGateways)
				{
					var GWActual = GatewayLoader.GetProcessor(s);
					var gd = new GatewayData();
					gd.DisplayName = GWActual.DisplayName(ThisCustomer.LocaleSetting);
					gd.AdministratorSetupPrompt = GWActual.AdministratorSetupPrompt;
					if(serverAssets.ContainsKey(GWActual.TypeName))
					{
						if(serverAssets[GWActual.TypeName].Count == 0)
							return;

						var dllVersion = new AssetVersion(GWActual.Version);
						var availibleVersion = new AssetVersion(serverAssets[GWActual.TypeName][0].Version);

						if(availibleVersion.CompareTo(dllVersion) > 0)
						{
							gd.AdministratorSetupPrompt += "<b>Download Update</b>";
							foreach(var asa in serverAssets[GWActual.TypeName])
								gd.AdministratorSetupPrompt += string.Format(downloadLink, asa.Title + " (" + asa.Version + ")", asa.Link, CommonLogic.IIF(string.IsNullOrEmpty(asa.DownloadInstructions), string.Empty, HttpContext.Current.Server.HtmlEncode(asa.DownloadInstructions)));
						}

						serverAssets.Remove(GWActual.TypeName);
					}
					gd.IsInstalled = true;
					gd.GatewayIdentifier = s;
					ds.Add(gd);
				}

				foreach(var sa in serverAssets)
				{
					if(sa.Value.Count == 0)
						break;

					var gd = new GatewayData();
					gd.DisplayName = sa.Value[0].Title;
					gd.IsInstalled = false;

					var setupPrompt = new StringBuilder();
					setupPrompt.Append("<b>Download</b>");

					foreach(var asa in sa.Value)
						setupPrompt.AppendFormat(downloadLink, asa.Title, asa.Link, CommonLogic.IIF(string.IsNullOrEmpty(asa.DownloadInstructions), string.Empty, HttpContext.Current.Server.HtmlEncode(asa.DownloadInstructions)));

					gd.AdministratorSetupPrompt = setupPrompt.ToString();
					ds.Add(gd);
				}

				ds.Add(CreateGatewayData("PayPal Payflow Link", "PayFlowPro", "(also enables PayPal Express Checkout) - See <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=paypalpayflowlink' target='_blank'>Manual</a>."));
				ds.Add(CreateGatewayData("PayPal Payments Advanced", "PayFlowPro", "(also enables PayPal Express Checkout) - See <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=paypalpaymentsadvanced' target='_blank'>Manual</a>."));

				ds = ds.Where(gd => IsPaymentOptionAvailable(gd.DisplayName, ddlCountries.SelectedValue)).ToList();

				ds = SortGatewayList(ds);

				repGateways.DataSource = ds;
				repGateways.DataBind();
			}
		}

		GatewayData CreateGatewayData(string displayName, string gatewayIdentifier, string administratorSetupPrompt)
		{
			var gatewayData = new GatewayData();
			gatewayData.DisplayName = displayName;
			gatewayData.AdministratorSetupPrompt = administratorSetupPrompt;
			gatewayData.IsInstalled = true;
			gatewayData.GatewayIdentifier = gatewayIdentifier;

			return gatewayData;
		}

		List<GatewayData> SortGatewayList(List<GatewayData> ds)
		{
			return ds.OrderByDescending(d => d.IsInstalled)
				.ThenByDescending(d => d.DisplayName.StartsWith("Manual"))
				.ThenByDescending(d => d.DisplayName.StartsWith("PayPal Payments Advanced"))
				.ThenByDescending(d => d.DisplayName.StartsWith("PayPal Payments Pro"))
				.ThenByDescending(d => d.DisplayName.StartsWith("PayPal Payflow Link"))
				.ThenByDescending(d => d.DisplayName.StartsWith("PayPal Payflow Pro"))
				.ThenByDescending(d => d.DisplayName.Contains("Authorize.net"))
				.ThenBy(d => d.DisplayName).ToList();
		}

		string GetExternalDownloads(List<AssetServerAsset> ListOfAssets, string AboveVersion, Boolean IsUpdate)
		{
			var ret = new StringBuilder();
			if(IsUpdate)
				ret.Append("<b>Update:</b> ");
			else
				ret.Append("<b>Download:</b> ");

			var Seperator = " | ";
			foreach(var asset in ListOfAssets)
			{
				var thisVersion = new AssetVersion(asset.Version);
				var comparedVersion = new AssetVersion(AboveVersion);

				if(thisVersion.CompareTo(comparedVersion) > 0)
					ret.AppendFormat("<a href='{0}' target='_blank'>{1} {2}</a>{3}", asset.Link, asset.Title, asset.Version, Seperator);
			}

			var retS = ret.ToString();
			if(retS.Length > Seperator.Length && retS.EndsWith(Seperator))
				retS = retS.Substring(0, retS.Length - Seperator.Length);

			return retS;
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			Page.Validate();
			if(!Page.IsValid)
				return;

			var BadSSL = false;

			// save the config settings:
			AtomStoreZip.Save();
			AtomLiveServer.Save();

			if(AtomStoreUseSSL.GetValue(AppLogic.StoreID()).ToBool() || AtomStoreUseSSL.GetValue(0).ToBool())
			{
				BadSSL = true;
				var WorkerWindowInSSL = string.Empty;
				var urlsToTry = new List<string>();
				urlsToTry.Add(AppLogic.GetStoreHTTPLocation(false).Replace("http://", "https://") + "empty.htm");
				urlsToTry.Add(AppLogic.GetStoreHTTPLocation(false).Replace("http://", "https://").Replace("https://", "https://www.") + "empty.htm");

				foreach(var urlToTry in urlsToTry)
				{
					if(BadSSL)
					{
						WorkerWindowInSSL = CommonLogic.AspHTTP(urlToTry, 10);

						if(!string.IsNullOrEmpty(WorkerWindowInSSL) && WorkerWindowInSSL.IndexOf("Worker") != -1)
						{
							AtomStoreUseSSL.Save();
							BadSSL = false;
							break;
						}
					}
				}
			}
			else
			{
				AtomStoreUseSSL.Save();
			}

			AtomLiveServer.Save();
			AtomStoreCurrency.Save();
			AtomStoreCurrencyNumeric.Save();
			AtomStoreName.Save();
			AtomStoreLiveTransactions.Save();

			var checkedPaymentMethods = GetCheckedPaymentMethods();
            AppConfigManager.SetAppConfigValue("PaymentMethods", checkedPaymentMethods);

			if(AppConfigManager.AppConfigExists("PaymentGateway"))
            {
				var newGateway = getSelectedGateway();

				if(!string.IsNullOrEmpty(newGateway))
					AppConfigManager.SetAppConfigValue("PaymentGateway", newGateway);

				if(newGateway == "PayFlowPro")
				{
					var newGatewayProduct = getSelectedGatewayProduct();
					AppConfigManager.SetAppConfigValue("PayFlowPro.Product", newGatewayProduct);

					// If PPA Gateway is selected, then set the PPA Method
					if(newGatewayProduct == "PayPal Payments Advanced")
					{
						if(!checkedPaymentMethods.Contains("PayPalPaymentsAdvanced"))
						{
							var currentPaymentMethods = AppConfigManager.GetAppConfigValue("PaymentMethods");
							AppConfigManager.SetAppConfigValue("PaymentMethods", string.Format("{0}, PayPalPaymentsAdvanced", currentPaymentMethods));
						}
					}

					// if any PayFlow gateway is selected, select PayPalExpress
					if(!checkedPaymentMethods.Contains("PayPalExpress"))
					{
						var currentPaymentMethods = AppConfigManager.GetAppConfigValue("PaymentMethods");
						AppConfigManager.SetAppConfigValue("PaymentMethods", string.Format("{0}, PayPalExpress", currentPaymentMethods));
						cbxPayPalExpress.Checked = true;
					}
				}
			}

			string BuySafeMessage = string.Empty;

			if(rblBuySafeEnabled.SelectedIndex == 1)
			{
				var bss = BuySafeController.BuySafeOneClickSignup();
				if(!bss.Sucessful)
				{
					BuySafeMessage = "buySAFE could not be enabled.{0}";
					var buysafeResponse = string.IsNullOrEmpty(bss.ErrorMessage) ? "" : " Error message: " + bss.ErrorMessage;
					ctrlAlertMessage.PushAlertMessage(string.Format(BuySafeMessage, buysafeResponse), AlertMessage.AlertType.Error);
				}
			}

			var errors = new StringBuilder();
			var webMgr = new WebConfigManager();
			if(AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted)
			{
				if(webMgr.ProtectWebConfig != rblEncrypt.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase) || rblStaticMachineKey.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase))
				{
					webMgr.ProtectWebConfig = rblEncrypt.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);

					if(rblStaticMachineKey.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase))
					{
						webMgr.SetMachineKey = true;
						webMgr.ValidationKeyGenMethod = WebConfigManager.KeyGenerationMethod.Auto;
						webMgr.DecryptKeyGenMethod = WebConfigManager.KeyGenerationMethod.Auto;
					}

					var saveWebConfigExceptions = webMgr.Commit();

					var webMgrNew = new WebConfigManager();

					if(saveWebConfigExceptions.Count > 0 && (webMgr.ProtectWebConfig != webMgrNew.ProtectWebConfig || rblStaticMachineKey.SelectedValue.EqualsIgnoreCase("true")))
					{
						if(webMgr.ProtectWebConfig != webMgrNew.ProtectWebConfig)
							errors.Append("Your web config encryption could not be changed due to the following error(s): <br />");

						if(rblStaticMachineKey.SelectedValue.EqualsIgnoreCase("true"))
							errors.Append("Could not set static machine key due to the following error(s): <br />");

						foreach(var ex in saveWebConfigExceptions)
							errors.Append(ex.Message + "<br />");
					}
				}
			}

			if(BadSSL)
				errors.AppendFormat("{0}<br />", AppLogic.GetString("admin.wizard.BadSSL", ThisCustomer.LocaleSetting));

			if(!webMgr.WebConfigRequiresReload)
				Response.Redirect("wizard.aspx");

			var errorMessage = errors.ToString();
			if(string.IsNullOrEmpty(errorMessage))
				ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.wizard.success", ThisCustomer.LocaleSetting), AlertMessage.AlertType.Success);
			else
				ctrlAlertMessage.PushAlertMessage(errorMessage, AlertMessage.AlertType.Error);

			loadData();
		}

		string GetCheckedPaymentMethods()
		{
			var temp = "";
			if(cbxCreditCard.Checked)
				temp += ", Credit Card";
			if(cbxAmazonPayments.Checked)
				temp += ", AmazonPayments";
			if(cbxPayPalExpress.Checked)
				temp += ", PayPalExpress";
			if(cbxRequestQuote.Checked)
				temp += ", Request Quote";
			if(cbxPurchaseOrder.Checked)
				temp += ", Purchase Order";
			if(cbxCheckByMail.Checked)
				temp += ", Check By Mail";
			if(cbxCOD.Checked)
				temp += ", C.O.D.";
			if(cbxMicroPay.Checked)
				temp += ", MICROPAY";

			if(temp.Length == 0)
				return string.Empty;

			return temp.Substring(1);
		}

		string getSelectedGateway()
		{
			foreach(RepeaterItem ri in repGateways.Items)
			{
				var grb = ri.FindControl("rbGateway") as GroupRadioButton;
				var hfGatewayIdentifier = ri.FindControl("hfGatewayIdentifier") as HiddenField;
				if(grb.Checked)
					return hfGatewayIdentifier.Value;
			}
			return null;
		}

		string getSelectedGatewayProduct()
		{
			foreach(RepeaterItem ri in repGateways.Items)
			{
				var grb = ri.FindControl("rbGateway") as GroupRadioButton;
				var hfGatewayProductIdentifier = ri.FindControl("hfGatewayProductIdentifier") as HiddenField;
				if(grb.Checked)
					return hfGatewayProductIdentifier.Value;
			}

			return null;
		}
	}

	class GatewayData
	{
		public string DisplayName { get; set; }
		public string AdministratorSetupPrompt { get; set; }
		public Boolean IsInstalled { get; set; }
		public string GatewayIdentifier { get; set; }
		public GatewayData() { }
	}
}
