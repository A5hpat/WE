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
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using AspDotNetStorefront;
using AspDotNetStorefront.License.Concrete;
using AspDotNetStorefront.License.Contract;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class about : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			ltDateTime.Text = Localization.ToNativeDateTimeString(System.DateTime.Now);
			ltExecutionMode.Text = CommonLogic.IIF(IntPtr.Size == 8, "64 Bit", "32 Bit");

			if (!IsPostBack)
			{
				loadSystemInformation();
				loadLicensing();
				loadLocalizationInformation();
				loadGatewayInformation();
				loadShippingInformation();
			}

			// perform security audit on this website, and present the merchant with a list of issues needing attention before the site goes live
			if (!AppLogic.AppConfigBool("SkipSecurityAudit") && ThisCustomer.IsAdminSuperUser)
			{
				try
				{
					DisplaySecurityAudit();
				}
				catch { }
			}
			else
			{
				pnlSecurity.Visible = false;
			}
		}

		protected void loadSystemInformation()
		{
			ltStoreVersion.Text = CommonLogic.GetVersion();
			ltOnLiveServer.Text = AppLogic.OnLiveServer().ToString();
			ltUseSSL.Text = AppLogic.UseSSL().ToString();
			ltServerPortSecure.Text = CommonLogic.IsSecureConnection().ToString();
			ltAdminDirChanged.Text = CommonLogic.IIF(AppLogic.AppConfig("AdminDir").ToLowerInvariant() == "admin", "No", "Yes");
			ltCaching.Text = (AppLogic.AppConfigBool("CacheMenus") ? AppLogic.GetString("admin.common.OnUC", SkinID, LocaleSetting) : AppLogic.GetString("admin.common.OffUC", SkinID, LocaleSetting));
			ltTrustLevel.Text = AppLogic.TrustLevel.ToString();
		}

		protected void loadLicensing()
		{
			var licenseValidationResult = LicenseController
				.Current
				.GetLicenseValidationResult();

			var licenses = licenseValidationResult.Licenses;
			var permissionSet = licenseValidationResult.PermissionSet;

            var retVal = new StringBuilder();

			retVal.AppendLine("<br /><br />");

			if (licenses == null || !licenses.Any())
			{
				pnlLicenseInformation.Text = retVal.ToString();
				return;
			}

			retVal.Append("The following licenses have been found in your images directory:<br /><br />");
			foreach (ILicense license in licenses)
				retVal.Append(GetLicenseSummary(license));

			if (permissionSet == null)
			{
				pnlLicenseInformation.Text = retVal.ToString();
				return;
			}

            var domainPermissions = permissionSet.PermissionsOfType<DomainPermission>();
			if (domainPermissions.Count() > 0)
			{
                retVal.Append("<b>The following domains are authorized for this resource</b>: ");
				foreach (DomainPermission domainPermission in domainPermissions)
					foreach (String domain in domainPermission.Domains)
						retVal.AppendFormat("{0}<br />", domain);
			}

			retVal.AppendLine("<br /><br />");

            var tokenPermissions = permissionSet.PermissionsOfType<TokenPermission>();
			if (tokenPermissions.Count() > 0)
			{
				retVal.Append("The following tokens are authorized:<br />");
				foreach (TokenPermission tokenPermission in tokenPermissions)
					foreach (String token in tokenPermission.Tokens)
						retVal.AppendFormat("{0}<br />", token);
			}

			retVal.AppendLine("<br /><br />");

            var identifierPermissions = permissionSet.PermissionsOfType<IdentifierPermission>();
			if (identifierPermissions.Count() > 0)
			{
				retVal.Append("The following identifiers are authorized:<br />");
				foreach (IdentifierPermission identifierPermission in identifierPermissions)
					foreach (Guid identifier in identifierPermission.Identifiers)
						retVal.AppendFormat("{0}<br />", identifier);
			}

			retVal.AppendLine("<br /><br />");

            IEnumerable<String> notifications = licenses.SelectMany(l => l.Policies.Select(p => p.NotificationSet).SelectMany(ns => ns.Notifications))
                            .Where(n =>
                                            n.GetType() == typeof(ADNSFWebsiteNotification)

                                            &&
                                            (n.NotificationAreaTarget == NotificationAreaTarget.Private ||
                                            n.NotificationAreaTarget == NotificationAreaTarget.Both)

                                            && (n.NotificationRenderType == NotificationRenderType.RenderAlways ||
                                            n.NotificationRenderType == NotificationRenderType.RenderIfValid)

                                            && (!n.ActiveBeginDate.HasValue || DateTime.Now >= n.ActiveBeginDate)
                                            && (!n.ActiveEndDate.HasValue || DateTime.Now < n.ActiveEndDate)
                                            ).Select(n => n.Message);

			foreach (String notification in notifications)
				retVal.AppendFormat("{0}<br />", notification);

			pnlLicenseInformation.Text = retVal.ToString();
		}

		private string GetLicenseSummary(ILicense license)
		{
            var retVal = new StringBuilder();
			retVal.AppendFormat("<b>Id: {0}</b><br />", license.Identifier);
			retVal.AppendFormat("<b>Created: {0}</b><br />", license.CreatedDate);
			retVal.AppendFormat("<b>Filename: {0}</b><br /><br />", license.Filename);
			int policynumber = 1;
			retVal.Append("<table>");
			foreach (IPolicy ip in license.Policies.Where(p => p.GetType() == typeof(ADNSFPolicy)))
			{
                var aip = ip as ADNSFPolicy;
                var token = string.Empty;
                var tp = aip.PermissionSet.Permissions.FirstOrDefault(p => p.GetType() == typeof(TokenPermission)) as TokenPermission;
				if (tp != null)
					token = string.Join(" ", tp.Tokens);

                retVal.AppendFormat("<tr><td><b>Policy {0}</b></td><td><b>{1}</b></td></tr>", policynumber++, token);
                retVal.AppendFormat("<tr><td align=\"right\">Resource: </td><td>{0}</td></tr>", aip.ResourceIdentifier);

				foreach (IRule ir in ip.RuleSet.Rules.Where(r => r.GetType() == typeof(HostRule)))
				{
                    var hr = ir as HostRule;
                    retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", CommonLogic.IIF(hr.Domains.Count() > 1,  "Valid Hosts: ", "Valid Host: "), String.Join("; ", hr.Domains) + ";");
				}

				foreach (IRule ir in ip.RuleSet.Rules.Where(r => r.GetType() == typeof(LicenseRule)))
				{
                    var lr = ir as LicenseRule;
                    retVal.AppendFormat("<tr><td align=\"right\">Required License Filename: </td><td>{0}</td></tr>", lr.RequiredFileName);
				}

				foreach (IRule ir in ip.RuleSet.Rules.Where(r => r.GetType() == typeof(TimePeriodRule)))
				{
                    var tpr = ir as TimePeriodRule;
                    retVal.AppendFormat("<tr><td align=\"right\">Start Date: </td><td>{0}</td></tr>", tpr.BeginDate.ToShortDateString());
                    retVal.AppendFormat("<tr><td align=\"right\">End Date: </td><td>{0}</td></tr>", tpr.EndDate.ToShortDateString());
				}

				foreach (IRule ir in ip.RuleSet.Rules.Where(r => r.GetType() == typeof(EnvironmentRule)))
				{
                    var er = ir as EnvironmentRule;
					if (!String.IsNullOrEmpty(er.MachineName))
                        retVal.AppendFormat("<tr><td align=\"right\">Required Machine Name: </td><td>{0}</td></tr>", er.MachineName);
					if (!String.IsNullOrEmpty(er.OSVersion))
                        retVal.AppendFormat("<tr><td align=\"right\">Required OS Version: </td><td>{0}</td></tr>", er.OSVersion);
					if (!String.IsNullOrEmpty(er.PlatformVersion))
                        retVal.AppendFormat("<tr><td align=\"right\">Required Platform Version: </td><td>{0}</td></tr>", er.PlatformVersion);
					if (er.ProcessorCount > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">Required Processor Count: </td><td>{0}</td></tr>", er.ProcessorCount);
				}

				foreach (IRule ir in ip.RuleSet.Rules.Where(r => r.GetType() == typeof(ADNSFLimitsRule)))
				{
                    var lr = ir as ADNSFLimitsRule;
					if (lr.CategoryLimit > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">Category Limit: </td><td>{0}</td></tr>", lr.CategoryLimit);
					if (lr.CustomerLimit > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">Customer Limit: </td><td>{0}</td></tr>", lr.CustomerLimit);
					if (lr.OrderLimit > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">Order Limit: </td><td>{0}</td></tr>", lr.OrderLimit);
					if (lr.ProductLimit > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">Product Limit: </td><td>{0}</td></tr>", lr.ProductLimit);
					if (lr.SectionLimit > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">Section Limit: </td><td>{0}</td></tr>", lr.SectionLimit);
				}


			}
			retVal.Append("</table>");
			retVal.AppendLine("<br /><br />");
			return retVal.ToString();
		}

		protected void loadLocalizationInformation()
		{
			ltWebConfigLocaleSetting.Text = Localization.GetDefaultLocale();
			ltSQLLocaleSetting.Text = Localization.GetSqlServerLocale();
			ltCustomerLocaleSetting.Text = LocaleSetting;
			ltLocalizationCurrencyCode.Text = AppLogic.AppConfig("Localization.StoreCurrency");
			ltLocalizationCurrencyNumericCode.Text = AppLogic.AppConfig("Localization.StoreCurrencyNumericCode");
		}

		protected void loadGatewayInformation()
		{
			ltPaymentGateway.Text = AppLogic.ActivePaymentGatewayRAW();
			ltUseLiveTransactions.Text = (AppLogic.AppConfigBool("UseLiveTransactions") ? AppLogic.GetString("admin.splash.aspx.20", SkinID, LocaleSetting) : AppLogic.GetString("admin.splash.aspx.21", SkinID, LocaleSetting));
			ltTransactionMode.Text = AppLogic.AppConfig("TransactionMode").ToString();
			ltPaymentMethods.Text = AppLogic.AppConfig("PaymentMethods").ToString();
			ltPrimaryCurrency.Text = Localization.GetPrimaryCurrency();
		}

		protected void loadShippingInformation()
		{
			ltShippingCalculation.Text = DB.GetSqlS("select Name as S from dbo.ShippingCalculation where Selected=1");
			ltOriginState.Text = AppLogic.AppConfig("RTShipping.OriginState");
			ltOriginZip.Text = AppLogic.AppConfig("RTShipping.OriginZip");
			ltOriginCountry.Text = AppLogic.AppConfig("RTShipping.OriginCountry");
			ltFreeShippingThreshold.Text = AppLogic.AppConfigNativeDecimal("FreeShippingThreshold").ToString();
			ltFreeShippingMethods.Text = AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn");
            ltFreeShippingRateSelection.Text = CommonLogic.IIF(AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"),"On","Off");
		}

		/// <summary>
		/// Displays Security Audit Messages
		/// </summary>
		void DisplaySecurityAudit()
		{
			var list = Security.GetAuditIssues(new HttpRequestWrapper(Request));
			foreach(var item in list)
			{
                var row = new TableRow();
                var textCell = new TableCell();
                var cellText = new Literal();
				cellText.Text = string.Format(
					"<div class=\"row admin-row\"><div class=\"col-sm-1\"><i class=\"fa {0}\"></i></div><div class=\"col-sm-11\">{1}</div></div>",
					item.ItemType == SecurityAuditItemType.Security
						? "fa-lock"
						: "fa-cogs",
					item.Message);
			    textCell.Controls.Add(cellText);
			    textCell.CssClass = "splash_SecurityAudit";
			    row.Controls.Add(textCell);
			    tblSecurityAudit.Controls.Add(row);
		    }

			if(list.Any())
				{
				pnlSecurity.Visible = true;
				}
			else
			{
				pnlSecurity.Visible = false;
			}
		}
	}
}
