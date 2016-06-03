// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	[LoggedAuthorize(Roles = "SuperAdmin", LogFailed = true)]
	public partial class changeencryptkey : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Server.ScriptTimeout = 5000000;

			if(IsPostBack)
				return;

			if(AppLogic.TrustLevel != AspNetHostingPermissionLevel.Unrestricted && AppLogic.TrustLevel != AspNetHostingPermissionLevel.High)
			{
				ctlAlertMessage.PushAlertMessage("admin.changeEncryptKey.insufficientTrustLevel", AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				Response.Redirect(AppLogic.GetAdminDir());
			}

			StoringCC.Text = AppLogic.StoreCCInDB()
					? AppLogic.GetString("admin.common.Yes")
					: AppLogic.GetString("admin.common.No");

			RecurringProducts.Text = AppLogic.ThereAreRecurringOrdersThatNeedCCStorage()
					? AppLogic.GetString("admin.common.Yes")
					: AppLogic.GetString("admin.common.No");

			pnlUpdateEncryptKey.Visible = false;
		}

		protected void btnUpdateEncryptKey_Click(object sender, EventArgs e)
		{
			if(!string.IsNullOrEmpty(txtSecondaryEncryptKey.Text)
				&& (string.IsNullOrEmpty(txtSecondaryEncryptKeyConfirm.Text)))
			{
				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.changeencrypt.SecondaryEncryptKey.Confirm.Required"), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
                return;
			}

			var changeEncryptKeySelected = rblChangeEncryptKey.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);
			var changeMachineKeySelected = rblChangeMachineKey.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);
			var machineKeyAutoGenerate = rblMachineKeyGenType.SelectedValue.Equals("auto", StringComparison.InvariantCultureIgnoreCase);

			var primaryEncryptKey = txtPrimaryEncryptKey
				.Text
				.Trim();

			var secondaryEncryptKey = txtSecondaryEncryptKey
				.Text
				.Trim();

			var combinedEncryptKey = string.Concat(primaryEncryptKey, secondaryEncryptKey);

			var validationKey = txtValidationKey.Text.Trim();
			var decryptKey = txtDecryptKey.Text.Trim();

			if(changeEncryptKeySelected
				&& (string.IsNullOrWhiteSpace(primaryEncryptKey)
					|| combinedEncryptKey.Length < 8
					|| combinedEncryptKey.Length > 50))
			{
				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.changeencryptkey.AtLeast", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return;
			}

			if(changeMachineKeySelected)
			{
				if(!machineKeyAutoGenerate && (validationKey.Length < 32 || validationKey.Length > 64))
				{
					ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.changeencryptkey.ValidationKeyAtLeast", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
					return;
				}
				else if(!machineKeyAutoGenerate && decryptKey.Length != 24)
				{
					ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.changeencryptkey.DecryptKeyAtLeast", SkinID, LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
					return;
				}
			}

			try
			{
				var webConfigManager = new WebConfigManager();

				if(changeEncryptKeySelected)
				{
					webConfigManager.SetEncryptKey = true;
					webConfigManager.EncryptKeyGenMethod = WebConfigManager.KeyGenerationMethod.Manual;

					var encryptIterations = AppLogic.AppConfigNativeInt("EncryptIterations");
					var hashAlgorithm = AppLogic.AppConfig("HashAlgorithm");
					var initializationVector = AppLogic.AppConfig("InitializationVector");
					var keySize = AppLogic.AppConfigNativeInt("KeySize");

					var keyEncryptionKeyConfig = GlobalConfig.GetGlobalConfig(Security.KeyEncryptionKeyName);
					var tertiaryEncryptionKey = ConfigurationManager.AppSettings[Security.TertiaryEncryptionKeyName];

					var keyEncryptionKeyConfigExists = keyEncryptionKeyConfig != null && !string.IsNullOrWhiteSpace(keyEncryptionKeyConfig.ConfigValue);
					var tertiaryEncryptionKeyExists = !string.IsNullOrWhiteSpace(tertiaryEncryptionKey);

					if((keyEncryptionKeyConfigExists && !tertiaryEncryptionKeyExists)
						|| (!keyEncryptionKeyConfigExists && tertiaryEncryptionKeyExists))
						throw new ArgumentException("Both the key encryption key and tertiary encryption key were expected to be found but one or the other is missing. At this point you cannot recover encrypted data until you restore the original values. You can recover from this error by clearing both encryption keys but you will not be able to recover encrypted data.");

					if(string.IsNullOrWhiteSpace(keyEncryptionKeyConfig.ConfigValue))
					{
						// No KEK has been created, make one along with a TEK.
						tertiaryEncryptionKey = Security.CreateRandomKey(64);
						webConfigManager.TertiaryEncryptionKey = tertiaryEncryptionKey;

						var keyEncryptionKey = Security.CreateRandomKey(64);
						webConfigManager.KeyEncryptionKey = keyEncryptionKey;

						// Encrypt the KEK using the TEK and store it in the database.
						var encryptedKEK = Security.MungeString(
							value: keyEncryptionKey,
							saltKey: string.Empty,
							securityParams: new Security.SecurityParams
								{
									EncryptKey = tertiaryEncryptionKey,
									EncryptIterations = encryptIterations,
									HashAlgorithm = hashAlgorithm,
									InitializationVector = initializationVector,
									KeySize = keySize
							});
						keyEncryptionKeyConfig.ConfigValue = encryptedKEK;
						keyEncryptionKeyConfig.Save();
					}
					else
					{
						// A KEK has already been generated, pull the TEK, decrypt the KEK with the TEK and prep
						//  the WebConfigManager with the decrypted keys to encrypt the EncryptKey.
						webConfigManager.TertiaryEncryptionKey = ConfigurationManager.AppSettings[Security.TertiaryEncryptionKeyName];
						webConfigManager.KeyEncryptionKey = Security.UnmungeString(
							s: keyEncryptionKeyConfig.ConfigValue,
							SaltKey: string.Empty,
							p: new Security.SecurityParams
								{
									EncryptKey = webConfigManager.TertiaryEncryptionKey,
									EncryptIterations = encryptIterations,
									HashAlgorithm = hashAlgorithm,
									InitializationVector = initializationVector,
									KeySize = keySize
							}); ;
					}

					// If the merchant has specified their encrypt key, set the WebConfigManager's EncryptKey
					//  property to the plain text value to re-encrypt already encrypted data. The EncryptKey
					//  will be later encrypted in the AppSettings file. If we're auto-generating an EncryptKey
					//  then just pass an empty string.
					if(webConfigManager.EncryptKeyGenMethod == WebConfigManager.KeyGenerationMethod.Manual)
						webConfigManager.EncryptKey = combinedEncryptKey;
					else
						webConfigManager.EncryptKey = string.Empty;
				}

				if(changeMachineKeySelected)
				{
					webConfigManager.SetMachineKey = true;
					webConfigManager.ValidationKeyGenMethod = (WebConfigManager.KeyGenerationMethod)Enum.Parse(
						enumType: typeof(WebConfigManager.KeyGenerationMethod),
						value: rblMachineKeyGenType.SelectedValue,
						ignoreCase: true);

					webConfigManager.DecryptKeyGenMethod = webConfigManager.ValidationKeyGenMethod;

					if(webConfigManager.ValidationKeyGenMethod == WebConfigManager.KeyGenerationMethod.Manual)
					{
						webConfigManager.ValidationKey = validationKey;
						webConfigManager.DecryptKey = decryptKey;
					}
				}

				var commitExceptions = webConfigManager.Commit();
				if(commitExceptions.Any())
				{
					var errorMessage = commitExceptions
						.Aggregate(
							new StringBuilder("Your web.config could not be saved for the following reasons:<br />"),
							(message, exception) => message.AppendFormat("{0}<br />", exception.Message))
						.ToString();

					ctlAlertMessage.PushAlertMessage(errorMessage, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				}
				else
				{
					ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.changeencryptkey.Done", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);

					Response.Redirect("changeencryptkey.aspx");
				}
			}
			catch(ThreadAbortException)
			{
				throw;
			}
			catch(Exception exception)
			{
				ctlAlertMessage.PushAlertMessage(CommonLogic.GetExceptionDetail(exception, "<br/>"), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}
		}

		protected void rblMachineKeyGenType_OnSelectedIndexChanged(object sender, EventArgs e)
		{
			pnlMachineKey.Visible = !rblMachineKeyGenType.SelectedValue.Equals("auto", StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Determines if any of the ChangeEncryptKey Options are shown
		/// </summary>
		protected void rblChangeEncryptKey_OnSelectedIndexChanged(object sender, EventArgs e)
		{
			var changeEncryptKeyEnabled = rblChangeEncryptKey.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);
			var changeMachineKeyEnabled = rblChangeMachineKey.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);

			pnlChangeEncryptKeyMaster.Visible = changeEncryptKeyEnabled;
			pnlUpdateEncryptKey.Visible = changeEncryptKeyEnabled || changeMachineKeyEnabled;
		}

		/// <summary>
		/// Determines if any of the Change/Set machine key options are displayed
		/// </summary>
		protected void rblChangeMachineKey_OnSelectedIndexChanged(object sender, EventArgs e)
		{
			var changeEncryptKeyEnabled = rblChangeEncryptKey.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);
			var changeMachineKeyEnabled = rblChangeMachineKey.SelectedValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);

			pnlChangeSetMachineKey.Visible = changeMachineKeyEnabled;
			rblMachineKeyGenType.SelectedValue = "auto";
			pnlUpdateEncryptKey.Visible = changeMachineKeyEnabled || changeEncryptKeyEnabled;
		}
	}
}
