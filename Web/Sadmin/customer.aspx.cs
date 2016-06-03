// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class _customer : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected const string SetAdminLevelCommand = "customer:setAdminLevel";

		protected int? CustomerId;
		bool VatTEnabled;

		protected override void OnInit(EventArgs e)
		{
			VatTEnabled = AppLogic.AppConfigBool("VAT.Enabled");
			CustomerId = CommonLogic.QueryStringNativeInt("customerid") == 0
				? (int?)null
				: CommonLogic.QueryStringNativeInt("customerid");

			base.OnInit(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if(!IsPostBack)
			{
				pnlCustomerEdit.Visible = true;

				Title
					= HeaderText.Text
					= CustomerId == null
						? "admin.customer.CreateNew".StringResource()
						: "admin.customer.EditCustomer".StringResource();
			}

			// Prevent showing empty form if customer is marked deleted
			if(CustomerId.HasValue && CustomerIsDeleted(CustomerId))
			{
				pnlCustomerEdit.Visible = false;
				pnlAddressEdit.Visible = false;

				AlertMessageDisplay.PushAlertMessage(
					String.Format("admin.customer.CustomerIsMarkedAsDeleted".StringResource(), ReturnUrlTracker.GetReturnUrl()),
					AlertMessage.AlertType.Warning);

				return;
			}

			if(!IsPostBack)
				InitializeContent();
		}

		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
			if(args is CommandEventArgs)
			{
				var commandEventArgs = (CommandEventArgs)args;
				switch(commandEventArgs.CommandName)
				{
					case SetAdminLevelCommand:
						DB.ExecuteSQL(
							"update dbo.Customer set IsAdmin = @adminLevel where CustomerID = @customerId",
							new SqlParameter("customerId", (object)CustomerId.Value),
							new SqlParameter("adminLevel", commandEventArgs.CommandArgument));

						var customer = new Customer(CustomerId.Value);

						Security.LogEvent(
							"Customer admin level change success",
							string.Format(
								"{0}({1}) was changed to a {2} user", 
								customer.EMail, 
								customer.CustomerID, 
								customer.IsAdminSuperUser 
									? "SuperAdmin"
									: customer.IsAdminUser 
										? "Admin"
										: "Standard"),
							0,
							ThisCustomer.CustomerID,
							ThisCustomer.CurrentSessionID);

						InitializeContent();
						return true;
				}
			}

			return base.OnBubbleEvent(source, args);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DataBind();
		}

		protected void btnSaveAndClose_Click(object sender, EventArgs e)
		{
			var result = SaveOrCreateCustomer();
			if(!result.Item1)
				return;

			Response.Redirect(ReturnUrlTracker.GetReturnUrl());
		}

		protected void btnCreateOrder_Click(object sender, EventArgs e)
		{
			Response.Redirect(string.Format("impersonationhandler.axd?customerId={0}", CustomerId));
		}

		/// <summary>
		/// Submits the entire form to add a new or update an existing customer
		/// </summary>
		protected void btnSave_Click(object sender, EventArgs e)
		{
			var result = SaveOrCreateCustomer();
			if(!result.Item1)
				return;

			// If we just added a customer, redirect to the page with the customer ID set
			if(CustomerId == null)
				Response.Redirect(AppLogic.AdminLinkUrl(String.Format("customer.aspx?customerid={0}", result.Item2)));
		}

		/// <summary>
		/// Adds or removes a ban on the target customer's last IP address
		/// </summary>
		protected void btnBanIP_Click(object sender, EventArgs e)
		{
			if(CustomerId == null)
				return;

			var customer = new Customer(CustomerId.Value);

			if(String.IsNullOrEmpty(customer.LastIPAddress))
				return;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				var ipIsBlocked = DB.GetSqlN(
					"exec [dbo].[aspdnsf_getIPIsRestricted] @ipAddress",
					connection,
					new SqlParameter("ipAddress", customer.LastIPAddress));

				if(ipIsBlocked > 0)
				{
					//Remove the IP address ban
					DB.ExecuteSQL(
						"exec [dbo].[aspdnsf_delRestrictedIP] @ipAddress",
						connection,
						new[] { new SqlParameter("ipAddress", customer.LastIPAddress) });

					AlertMessageDisplay.PushAlertMessage("admin.customer.IPAddressUnbanned".StringResource(), AlertMessage.AlertType.Success);
					btnBanIP.Text = AppLogic.GetString("admin.customer.BanIP", ThisCustomer.LocaleSetting);
				}
				else
				{
					//Ban the IP address
					DB.ExecuteSQL(
						"exec [dbo].[aspdnsf_insRestrictedIP] @ipAddress",
						connection,
						new[] { new SqlParameter("ipAddress", customer.LastIPAddress) });

					AlertMessageDisplay.PushAlertMessage("admin.customer.IPAddressBanned".StringResource(), AlertMessage.AlertType.Success);
					btnBanIP.Text = AppLogic.GetString("admin.customer.UnBanIP", ThisCustomer.LocaleSetting);
				}
			}
		}

		/// <summary>
		/// Erases all failed transactions linked to the target customer
		/// </summary>
		protected void btnClearFailedTransactions_OnClick(object sender, EventArgs e)
		{
			if(CustomerId == null)
				return;

			var customer = new Customer(CustomerId.Value);

			Customer.ClearFailedTransactions(CustomerId.Value);
			ltlFailedTransactions.Text = (customer.FailedTransactionCount <= 0 ? "0" : customer.FailedTransactionCount.ToString());
			AlertMessageDisplay.PushAlertMessage("admin.customer.FailedTransactionsCleared".StringResource(), AlertMessage.AlertType.Success);
		}

		/// <summary>
		/// Clears the target customer's session data
		/// </summary>
		protected void btnClearSession_OnClick(object sender, EventArgs e)
		{
			if(CustomerId == null)
				return;

			CustomerSession.StaticClear(CustomerId.Value);
			AlertMessageDisplay.PushAlertMessage("admin.customer.CustomerSessionCleared".StringResource(), AlertMessage.AlertType.Success);
		}

		/// <summary>
		/// Manually resets the target customer's password to the value in the text box
		/// </summary>
		protected void btnManualPassword_OnClick(object sender, EventArgs e)
		{
			if(CustomerId == null)
				return;

			var customer = new Customer(CustomerId.Value);
			var password = new Password(txtManualPassword.Text.Trim());

			//Log the event
			Security.LogEvent(
				AppLogic.GetString("admin.cst_account_process.event.AdminResetCustomerPassword.Success", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),
				string.Format("Admin reset password for {0}({1})", customer.EMail, customer.CustomerID),
				customer.CustomerID,
				ThisCustomer.CustomerID,
				ThisCustomer.CurrentSessionID);

			//Try and email the new password to the target customer
			try
			{
				AppLogic.SendMail(
					String.Format(
						"{0} - {1}",
						AppLogic.AppConfig("StoreName"),
						AppLogic.GetString("cst_account_process.aspx.1", customer.SkinID, customer.LocaleSetting)),
					AppLogic.RunXmlPackage(
						"notification.lostpassword.xml.config",
						null,
						customer,
						customer.SkinID,
						String.Empty,
						String.Format("thiscustomerid={0}&newpwd={1}", customer.CustomerID, password.ClearPassword),
						false,
						false),
					true,
					AppLogic.AppConfig("MailMe_FromAddress"),
					AppLogic.AppConfig("MailMe_FromName"),
					customer.EMail,
					customer.FullName(),
					String.Empty,
					String.Empty,
					AppLogic.MailServer());

				AlertMessageDisplay.PushAlertMessage("admin.customer.passwordResetSuccessful".StringResource(), AlertMessage.AlertType.Success);
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Alert);
				AlertMessageDisplay.PushAlertMessage("admin.customer.passwordResetEmailFailed".StringResource(), AlertMessage.AlertType.Danger);
			}

			customer.UpdateCustomer(
				saltedAndHashedPassword: password.SaltedPassword,
				saltKey: password.Salt,
				lockedUntil: DateTime.Now.AddMinutes(-1),
				badLogin: -1,
				passwordChangeRequired: true);

			txtManualPassword.Text = String.Empty;
		}

		/// <summary>
		/// Resets the target customer's password to a random value and emails them
		/// </summary>
		protected void btnRandomPassword_OnClick(object sender, EventArgs e)
		{
			if(CustomerId == null)
				return;

			var customer = new Customer(CustomerId.Value);
			var password = new Password(AspDotNetStorefrontEncrypt.Encrypt.CreateRandomStrongPassword(8));

			try
			{
				//Send the new password email
				AppLogic.SendMail(
					AppLogic.AppConfig("StoreName") + " - " + AppLogic.GetString("cst_account_process.aspx.1", customer.SkinID, customer.LocaleSetting),
					AppLogic.RunXmlPackage("notification.lostpassword.xml.config", null, customer, customer.SkinID, "", "thiscustomerid=" + customer.CustomerID.ToString() + "&newpwd=" + password.ClearPassword, false, false),
					true,
					AppLogic.AppConfig("MailMe_FromAddress"),
					AppLogic.AppConfig("MailMe_FromName"),
					customer.EMail,
					customer.FullName(),
					"",
					"",
					AppLogic.MailServer());

				Security.LogEvent(
					AppLogic.GetString("admin.cst_account_process.event.AdminResetCustomerPassword.Success", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),
					"",
					customer.CustomerID,
					ThisCustomer.CustomerID,
					ThisCustomer.CurrentSessionID);

				customer.UpdateCustomer(
					saltedAndHashedPassword: password.SaltedPassword,
					saltKey: password.Salt,
					lockedUntil: DateTime.Now.AddMinutes(-1),
					badLogin: -1,
					passwordChangeRequired: true);

				AlertMessageDisplay.PushAlertMessage("admin.customer.passwordResetSuccessful".StringResource(), AlertMessage.AlertType.Success);
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Alert);
				AlertMessageDisplay.PushAlertMessage("admin.customer.passwordResetError".StringResource(), AlertMessage.AlertType.Danger);
			}
		}

		/// <summary>
		/// If editing an existing customer, pre-populates all fields with the existing values for the target customer
		/// </summary>
		public void LoadCustomer(Customer customer)
		{
			ssOne.SelectedStoreID = customer.StoreID;
			ltlCustomerID.Text = customer.CustomerID.ToString();
			ltlCreatedOn.Text = Localization.ToThreadCultureShortDateString(customer.CreatedOn);
			ltlIPAddress.Text = customer.LastIPAddress;
			ltlFailedTransactions.Text = Math.Max(customer.FailedTransactionCount, 0).ToString();

			ltlIsRegistered.Text = customer.IsRegistered
				? AppLogic.GetString("admin.common.yes", ThisCustomer.LocaleSetting)
				: AppLogic.GetString("admin.common.no", ThisCustomer.LocaleSetting);

			txtFirstName.Text = customer.FirstName;
			txtLastName.Text = customer.LastName;
			txtEmail.Text = customer.EMail;

			txtPhone.Text = customer.Phone;
			txtMicroPay.Text = Localization.ParseNativeCurrency(customer.MicroPayBalance.ToString()).ToString();
			txtNotes.Text = customer.Notes;

			chkAccountLocked.Checked = customer.LockedUntil > DateTime.Now;
			chkOver13.Checked = customer.IsOver13;
			chkCanViewCC.Checked = customer.IsAdminUser && customer.AdminCanViewCC;
			chkOkToEmail.Checked = customer.OKToEMail;

			if(customer.AffiliateID != 0)
				ddlCustomerAffiliate.SelectFirstByValue(customer.AffiliateID.ToString());

			if(!string.IsNullOrEmpty(customer.LocaleSetting))
				ddlCustomerLocaleSetting.SelectFirstByValue(Localization.GetCustomerLocaleSettingID(customer.CustomerID));

			if(customer.CustomerLevelID != 0)
				ddlCustomerLevel.SelectFirstByValue(customer.CustomerLevelID.ToString());

			//See if the IP Is Blocked
			if(!string.IsNullOrEmpty(customer.LastIPAddress))
			{
				var ipIsBlocked = DB.GetSqlN(
					"exec [dbo].[aspdnsf_getIPIsRestricted] @ipAddress",
					new[] { new SqlParameter("ipAddress", customer.LastIPAddress) });

				if(ipIsBlocked > 0)
				{
					btnBanIP.Text = AppLogic.GetString("admin.customer.UnBanIP", ThisCustomer.LocaleSetting);
					cbeBlockIP.ConfirmText = AppLogic.GetString("admin.customer.ConfirmUnBanIP", ThisCustomer.LocaleSetting);
				}
				else
				{
					btnBanIP.Text = AppLogic.GetString("admin.customer.BanIP", ThisCustomer.LocaleSetting);
					cbeBlockIP.ConfirmText = AppLogic.GetString("admin.customer.ConfirmBanIP", ThisCustomer.LocaleSetting);
				}
			}
			else
			{
				btnBanIP.Visible = false;
			}
		}

		/// <summary>
		/// Creates a new customer record form the information supplied in the form
		/// </summary>
		Tuple<bool, int?> CreateNewCustomer()
		{
			if(!Customer.NewEmailPassesDuplicationRules(txtEmail.Text.Trim(), 0))
			{
				AlertMessageDisplay.PushAlertMessage("admin.customer.EmailInUse".StringResource(), AlertMessage.AlertType.Danger);
				return new Tuple<bool, int?>(false, null);
			}

			var clearPassword = txtCreatePassword.Text.Trim();
			var password = new Password(clearPassword);
			var vatSetting = AppLogic.AppConfigUSInt("VAT.DefaultSetting");

			//Create the new customer record and get back the customer ID to instantiate a customer object
			var newCustomerID = Customer.CreateCustomerRecord(
				email: txtEmail.Text.Trim(),
				password: clearPassword,
				skinId: null,
				affiliateId: Convert.ToInt32(ddlCustomerAffiliate.SelectedValue) == 0
					? (int?)null
					: Convert.ToInt32(ddlCustomerAffiliate.SelectedValue),
				referrer: null,
				isAdmin: false,
				lastIpAddress: null,
				localeSetting: ddlCustomerLocaleSetting.SelectedValue != "-1"
					? ddlCustomerLocaleSetting.SelectedItem.Value
					: String.Empty,
				over13Checked: chkOver13.Checked,
				currencySetting: null,
				vatSetting: vatSetting,
				vatRegistrationId: null,
				customerLevelId: Convert.ToInt32(ddlCustomerLevel.SelectedValue));

			//Reinitialize the content with the new customer
			var customer = new Customer(newCustomerID);
			var vatRegistrationId = GetVatRegId(customer);

			customer.UpdateCustomer(
				saltedAndHashedPassword: password.SaltedPassword,
				saltKey: password.Salt,
				firstName: txtFirstName.Text.Trim(),
				lastName: txtLastName.Text.Trim(),
				notes: txtNotes.Text.Trim(),
				phone: txtPhone.Text.Trim(),
				okToEmail: chkOkToEmail.Checked,
				microPayBalance: !String.IsNullOrEmpty(txtMicroPay.Text.Trim())
					? Localization.ParseNativeDecimal(txtMicroPay.Text)
					: Decimal.Zero,
				vatRegistrationId: vatRegistrationId,
				isRegistered: true,
				lockedUntil: DateTime.Now.AddMinutes(-1),
				badLogin: -1,
				passwordChangeRequired: true,
				storeId: ssOne.SelectedStoreID);

			Security.LogEvent(
				AppLogic.GetString("admin.cst_account_process.event.AdminResetCustomerPassword.Success", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),
				"",
				customer.CustomerID,
				ThisCustomer.CustomerID,
				ThisCustomer.CurrentSessionID);

			//Send the customer an email with their password
			var emailSent = false;
			try
			{
				AppLogic.SendMail(
					AppLogic.AppConfig("StoreName") + " - " + AppLogic.GetString("cst_account_process.aspx.1", customer.SkinID, customer.LocaleSetting),
					AppLogic.RunXmlPackage("notification.lostpassword.xml.config", null, customer, customer.SkinID, "", "thiscustomerid=" + customer.CustomerID.ToString() + "&newpwd=" + password.ClearPassword, false, false),
					true,
					AppLogic.AppConfig("MailMe_FromAddress"),
					AppLogic.AppConfig("MailMe_FromName"),
					customer.EMail,
					customer.FullName(),
					"",
					"",
					AppLogic.MailServer());
				emailSent = true;
			}
			catch(Exception ex)
			{
				SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Alert);
			}

			// call InitializeContent before DisplayMessage to keep the message from being hidden
			InitializeContent();

			if(emailSent)
			{
				AlertMessageDisplay.PushAlertMessage("admin.customer.CustomerCreated".StringResource(), AlertMessage.AlertType.Success);
			}
			else
			{
				AlertMessageDisplay.PushAlertMessage("admin.customer.CustomerCreatedEmailFailed".StringResource(), AlertMessage.AlertType.Danger);
			}

			return new Tuple<bool, int?>(true, newCustomerID);
		}

		/// <summary>
		/// Initializes page content, such as populating dropdown lists and setting default values
		/// All of your logic that you normally would expect to fire on page load should go here
		/// Since this is called often times from an AJAX-enabled page, any number of things
		/// could have changed since page load
		/// </summary>
		void InitializeContent()
		{
			Page.Form.DefaultButton = btnSave.UniqueID;

			pnlVATRegID.Visible = VatTEnabled;
			txtVATRegID.Visible = VatTEnabled;

			ddlCustomerLocaleSetting.Items.Clear();
			ddlCustomerAffiliate.Items.Clear();
			ddlCustomerLevel.Items.Clear();

			btnCreateOrder.Visible = CustomerId.HasValue;

			//Security
			chkCanViewCC.Visible = ThisCustomer.IsAdminSuperUser;
			txtManualPassword.Text = String.Empty;
			txtCreatePassword.Text = String.Empty;

			if(AppLogic.AppConfigBool("UseStrongPwd") || (CustomerId.HasValue && new Customer(CustomerId.Value).IsAdminUser))
			{ 
				regexValTxtCreatePassword.ValidationExpression
					= regexValManualPassword.ValidationExpression
					= AppLogic.AppConfig("StrongPasswordValidator");

				regexValTxtCreatePassword.ErrorMessage 
					= regexValManualPassword.ErrorMessage
					= AppLogic.GetString("account.strongPassword", ThisCustomer.LocaleSetting);
			}
			else
			{
				regexValTxtCreatePassword.ValidationExpression
					= regexValManualPassword.ValidationExpression
					= AppLogic.AppConfig("PasswordValidator");

				regexValTxtCreatePassword.ErrorMessage
					= regexValManualPassword.ErrorMessage 
					= AppLogic.GetString("account.aspx.19", ThisCustomer.LocaleSetting); 
            }

			var noneSelectionText = AppLogic.GetString("admin.common.DDNone", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

			//Populate the affiliate dropdown
			ddlCustomerAffiliate.Items.Add(new ListItem(noneSelectionText, "0"));

			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(var rs = DB.GetRS("exec [dbo].[aspdnsf_getAffiliateList]", conn))
				{
					while(rs.Read())
					{
						var affiliateName = DB.RSField(rs, "Name");
						var affiliateID = DB.RSFieldInt(rs, "AffiliateID").ToString();

						ddlCustomerAffiliate.Items.Add(new ListItem(affiliateName, affiliateID));
					}
				}
			}
			//End Populating Affiliate DropDown

			//Populate the Locale Dropdown
			ddlCustomerLocaleSetting.Items.Add(new ListItem(noneSelectionText, "-1"));
			ddlCustomerLocaleSetting
				.AddItems(Localization
					.GetLocales()
					.AsEnumerable()
					.Select(localeRow => new ListItem(
						DB.RowField(localeRow, "Description"), 
						DB.RowField(localeRow, "Name"))));

			if(CustomerId != null)
				ddlCustomerLocaleSetting.SelectFirstByValue(Localization.GetCustomerLocaleSettingID(CustomerId.Value));

			//Populate the Customer Level Dropdown
			ddlCustomerLevel.Items.Add(new ListItem(noneSelectionText, "0"));

			pnlAssignedToStore.Visible = AppLogic.GlobalConfigBool("AllowCustomerFiltering");

			using(var conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();

				using(var rs = DB.GetRS("exec [dbo].[aspdnsf_getCustomerLevels]", conn))
				{
					while(rs.Read())
					{
						var customerLevelID = DB.RSFieldInt(rs, "CustomerLevelID").ToString();
						var customerLevelName = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Name"), ThisCustomer.LocaleSetting, true);

						ddlCustomerLevel.Items.Add(new ListItem(customerLevelName, customerLevelID));
					}
				}
			}
			//End Populating the Customer Level Dropdown

			if(CustomerId.HasValue)
			{
				var customer = new Customer(CustomerId.Value);
				//Editing an existing customer, make sure all controls are unhidden
				pnlClearSession.Visible = true;
				pnlCreatedOn.Visible = true;
				pnlCustomerID.Visible = true;
				pnlBanIP.Visible = true;
				pnlFailedTransactions.Visible = true;
				pnlIsRegistered.Visible = true;
				pnlCreateCustomerPassword.Visible = false;

				btnRandomPassword.Visible = true;
				btnClearFailedTransactions.Visible = true;
				btnClearSession.Visible = true;
				btnManualPassword.Visible = true;
				btnBanIP.Visible = true;

				ltlResetPasswordLabel.Text = AppLogic.GetString("admin.customer.ResetPassword", ThisCustomer.LocaleSetting);

				pnlAddressEdit.Visible = true;

				//Setup the billing and shipping address...
				pnlCanViewCC.Visible = ThisCustomer.IsAdminSuperUser && customer.IsAdminUser;

				Address bAddress = new Address();
				Address sAddress = new Address();

				int bAddressID = customer.PrimaryBillingAddressID;
				int sAddressID = customer.PrimaryShippingAddressID;

				if(bAddressID == 0 && sAddressID == 0)
				{
					//See if the customer has any addresses
					Addresses addressList = new Addresses();
					addressList.LoadCustomer(customer.CustomerID);

					if(addressList.Count > 0)
					{
						bAddressID = addressList[0].AddressID;
						sAddressID = addressList[0].AddressID;

						customer.PrimaryBillingAddressID = bAddressID;
						customer.PrimaryShippingAddressID = sAddressID;

						bAddress = addressList[0];
						sAddress = addressList[0];
					}
				}

				if(bAddressID != 0)
				{
					bAddress = customer.PrimaryBillingAddress;

					ltlBillingName.Text = String.Format("{0} {1}", bAddress.FirstName, bAddress.LastName);
					ltlBillingCompany.Text = bAddress.Company;
					ltlBillingAddress1.Text = bAddress.Address1;
					ltlBillingAddress2.Text = bAddress.Address2;
					ltlBillingSuite.Text = bAddress.Suite;
					ltlBillingCityStateZip.Text = String.Format("{0}, {1} {2}", bAddress.City, bAddress.State, bAddress.Zip);
					ltlBillingCountry.Text = bAddress.Country;
					ltlBillingPhone.Text = bAddress.Phone;
					ltlBillingEmail.Text = bAddress.EMail;

					//Customer should always have a billing address, but may not have a shipping address
					if(sAddressID != 0)
						sAddress = customer.PrimaryShippingAddress;
					else
						sAddress = bAddress;		//No primary shipping address set.  Use billing.

					ltlShippingName.Text = String.Format("{0} {1}", sAddress.FirstName, sAddress.LastName);
					ltlShippingCompany.Text = sAddress.Company;
					ltlShippingAddress1.Text = sAddress.Address1;
					ltlShippingAddress2.Text = sAddress.Address2;
					ltlShippingSuite.Text = sAddress.Suite;
					ltlShippingCityStateZip.Text = String.Format("{0}, {1} {2}", sAddress.City, sAddress.State, sAddress.Zip);
					ltlShippingCountry.Text = sAddress.Country;
					ltlShippingPhone.Text = sAddress.Phone;
					ltlShippingEmail.Text = sAddress.EMail;

					pnlBillingAddress.Visible = true;
					pnlShippingAddress.Visible = true;
				}
				else
				{
					//Hide the address panels.  This customer has no addresses yet
					pnlBillingAddress.Visible = false;
					pnlShippingAddress.Visible = false;
				}

				pnlCustomerTools.Visible = true;

				if(ThisCustomer.IsAdminSuperUser)
					pnlAdminSettings.Visible = true;

				if(customer.IsAdminSuperUser)
					ltIsAdmin.Text = "(Currently SuperAdmin)";
				else if(customer.IsAdminUser)
					ltIsAdmin.Text = "(Currently Admin)";
				else
					ltIsAdmin.Text = "(Currently Standard User)";

				//Load the customer
				LoadCustomer(customer);
			}
			else
			{
				//Adding a new customer, so hide unneeded controls
				pnlClearSession.Visible = false;
				pnlCreatedOn.Visible = false;
				pnlCustomerID.Visible = false;
				pnlBanIP.Visible = false;
				pnlFailedTransactions.Visible = false;
				pnlIsRegistered.Visible = false;

				btnRandomPassword.Visible = false;
				btnClearFailedTransactions.Visible = false;
				btnClearSession.Visible = false;
				btnBanIP.Visible = false;
				btnManualPassword.Visible = false;

				pnlAddressEdit.Visible = false;

				pnlAdminSettings.Visible = false;
				pnlCustomerTools.Visible = false;
				pnlCreateCustomerPassword.Visible = true;

				// reset the checkboxes to defaults
				chkOver13.Checked = false;
				chkOkToEmail.Checked = false;

				// reset the text to defaults
				txtFirstName.Text = String.Empty;
				txtLastName.Text = String.Empty;
				txtEmail.Text = String.Empty;
				txtNotes.Text = String.Empty;
				txtPhone.Text = String.Empty;
				txtMicroPay.Text = Localization.ParseNativeCurrency("0").ToString();
				txtCreatePassword.Text = String.Empty;

				// reset the dropdowns to defaults
				ddlCustomerAffiliate.ClearSelection();
				ddlCustomerLevel.ClearSelection();
				ddlCustomerLocaleSetting.ClearSelection();

				ltlResetPasswordLabel.Text = AppLogic.GetString("admin.customer.Password", ThisCustomer.LocaleSetting);

				ddlCustomerLocaleSetting.SelectFirstByValue(Localization.GetDefaultLocale(), StringComparer.OrdinalIgnoreCase);
			}
		}

		Tuple<bool, int?> SaveOrCreateCustomer()
		{
			if(!Page.IsValid)
			{
				AlertMessageDisplay.PushAlertMessage("Please correct the validation issues below", AlertMessage.AlertType.Error);
				return new Tuple<bool, int?>(false, null);
			}

			if(CustomerId != null)
				return UpdateCustomer();
			else
				return CreateNewCustomer();
		}

		/// <summary>
		/// Updates an existing customer with the information supplied in the form
		/// </summary>
		Tuple<bool, int?> UpdateCustomer()
		{
			var customer = new Customer(CustomerId.Value);

			if (!customer.AdminCanViewCC && chkCanViewCC.Checked)
			{
				Security.LogEvent(
					securityAction: "Can View Credit Card Updated",
					description: "A customer's setting 'can view credit card number' has been enabled.",
					customerUpdated: customer.CustomerID,
					updatedBy: ThisCustomer.CustomerID,
					customerSessionId: ThisCustomer.CurrentSessionID);
			}

			customer.UpdateCustomer(
				storeId: ssOne.SelectedStoreID,
				customerLevelId: Convert.ToInt32(ddlCustomerLevel.SelectedValue),
				email: txtEmail.Text.Trim(),
				firstName: txtFirstName.Text.Trim(),
				lastName: txtLastName.Text.Trim(),
				notes: txtNotes.Text.Trim(),
				phone: txtPhone.Text.Trim(),
				okToEmail: chkOkToEmail.Checked,
				localeSetting: ddlCustomerLocaleSetting.SelectedValue != "-1"
					? ddlCustomerLocaleSetting.SelectedItem.Value
					: string.Empty,
				microPayBalance: !string.IsNullOrEmpty(txtMicroPay.Text.Trim())
					? Localization.ParseNativeDecimal(txtMicroPay.Text)
					: 0,
				over13Checked: chkOver13.Checked,
				vatRegistrationId: GetVatRegId(customer),
				lockedUntil: chkAccountLocked.Checked
					? DateTime.MaxValue
					: DateTime.Now.AddMinutes(-1),
				adminCanViewCreditCard: chkCanViewCC.Checked,
				badLogin: -1);

			//Have to handle these separately as passing a null to the aspdnsf_updCustomer sproc results in no change.
			DB.ExecuteSQL(string.Format("UPDATE Customer SET AffiliateID = {0} WHERE CustomerID = {1}",
				Convert.ToInt32(ddlCustomerAffiliate.SelectedValue) == 0
					? "null"
					: ddlCustomerAffiliate.SelectedValue,
				CustomerId.Value
				));

			AlertMessageDisplay.PushAlertMessage("admin.customer.CustomerUpdated".StringResource(), AlertMessage.AlertType.Success);

			return new Tuple<bool, int?>(true, null);
		}

		bool CustomerIsDeleted(int? customerId)
		{
			if(customerId == null)
				return false;

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();

				using(var reader = DB.GetRS("select Deleted from Customer with(nolock) where CustomerID = @customerId", new[] { new SqlParameter("@customerId", (object)customerId) }, con))
					if(reader.Read())
						return DB.RSFieldBool(reader, "Deleted");
					else
						return true;
			}
		}

		string GetVatRegId(Customer customer)
		{
			if(!VatTEnabled || String.IsNullOrWhiteSpace(txtVATRegID.Text))
				return null;
			
			if(AppLogic.VATRegistrationIDIsValid(customer, txtVATRegID.Text))
				return txtVATRegID.Text;
			else
				AlertMessageDisplay.PushAlertMessage("account.aspx.91".StringResource(), AlertMessage.AlertType.Danger); //Registration ID didn't pass validation

			return null;
		}
	}
}
