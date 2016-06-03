// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Classes
{
	public class AccountSettings
	{
		public readonly bool StoreCreditCards;
		public readonly bool VatEnabled;
		public readonly bool RequireCaptchaOnLogin;
		public readonly bool RequireCaptchaOnCreateAccount;
		public readonly bool SecurityCodeCaseSensitive;
		public readonly bool DynamicRelatedProductsEnabled;
		public readonly bool UseStrongPasswords;
		public readonly bool ShowWishlistButtons;
		public readonly bool UseGatewayInternalBilling;
		public readonly bool ReorderEnabled;
		public readonly bool ShowCustomerServiceNotesInReceipts;
		public readonly int StoreId;
		public readonly int MaxBadLogins;
		public readonly int BadLoginLockTimeout;
		public readonly int NumberOfPreviouslyUsedPasswords;
		public readonly double AdminPasswordChangeDays;
		public readonly string MailFromAddress;
		public readonly string MailToAddress;
		public readonly string XmlPackageLostPassword;
		public readonly string StoreName;
		public readonly string PasswordValidatorExpression;
		public readonly string StrongPasswordValidatorExpression;

		public AccountSettings()
		{
			StoreCreditCards = AppLogic.AppConfigBool("StoreCCInDB");
			VatEnabled = AppLogic.AppConfigBool("VAT.Enabled");
			RequireCaptchaOnLogin = AppLogic.AppConfigBool("SecurityCodeRequiredOnStoreLogin");
			RequireCaptchaOnCreateAccount = AppLogic.AppConfigBool("SecurityCodeRequiredOnCreateAccount");
			SecurityCodeCaseSensitive = AppLogic.AppConfigBool("Captcha.CaseSensitive");
			DynamicRelatedProductsEnabled = AppLogic.AppConfigBool("DynamicRelatedProducts.Enabled");
			UseStrongPasswords = AppLogic.AppConfigBool("UseStrongPwd");
			UseGatewayInternalBilling = AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling");
			ReorderEnabled = AppLogic.AppConfigBool("Reorder.Enabled");
			ShowCustomerServiceNotesInReceipts = AppLogic.AppConfigBool("ShowCustomerServiceNotesInReceipts");
			StoreId = AppLogic.StoreID();
			MaxBadLogins = AppLogic.AppConfigNativeInt("MaxBadLogins");
			BadLoginLockTimeout = AppLogic.AppConfigUSInt("BadLoginLockTimeOut");
			NumberOfPreviouslyUsedPasswords = AppLogic.AppConfigUSInt("NumPreviouslyUsedPwds");
			AdminPasswordChangeDays = AppLogic.AppConfigUSDouble("AdminPwdChangeDays");
			MailFromAddress = AppLogic.AppConfig("MailMe_FromAddress");
			MailToAddress = AppLogic.AppConfig("MailMe_ToAddress");
			XmlPackageLostPassword = AppLogic.AppConfig("XmlPackage.LostPassword");
			StoreName = AppLogic.AppConfig("StoreName");
			PasswordValidatorExpression = AppLogic.AppConfig("PasswordValidator");
			StrongPasswordValidatorExpression = AppLogic.AppConfig("StrongPasswordValidator");
			ShowWishlistButtons = AppLogic.AppConfigBool("ShowWishButtons");
		}
	}
}
