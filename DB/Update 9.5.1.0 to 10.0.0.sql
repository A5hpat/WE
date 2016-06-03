-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit our homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT.
-- ------------------------------------------------------------------------------------------

-- ------------------------------------------------------------------------------------------
-- Database Upgrade Script:
-- AspDotNetStorefront Version 9.5.1.0 to 10.0.0 Microsoft SQL Server 2008 or higher
-- ------------------------------------------------------------------------------------------

/*********** ASPDOTNETSTOREFRONT v9.0 to Latest *******************/
/*                                                                */
/*                                                                */
/* BACKUP YOUR EXISTING DATABASE BEFORE RUNNING THIS SCRIPT!!     */
/* BACKUP YOUR EXISTING DATABASE BEFORE RUNNING THIS SCRIPT!!     */
/* BACKUP YOUR EXISTING DATABASE BEFORE RUNNING THIS SCRIPT!!     */
/* BACKUP YOUR EXISTING DATABASE BEFORE RUNNING THIS SCRIPT!!     */
/*                                                                */
/*                                                                */
/* ************************************************************** */

SET NOCOUNT ON
GO

SET NOEXEC OFF
GO

DECLARE @oldGateway NVARCHAR(MAX), @gatewayType NVARCHAR(MAX), @storeId INT, @abortScript BIT
DECLARE @removedGateways TABLE (Name NVARCHAR(MAX), GatewayType NVARCHAR(MAX), StoreId INT)

INSERT INTO @removedGateways (Name, GatewayType, StoreId)
	SELECT ConfigValue, Name, StoreId 
	FROM AppConfig 
	WHERE (Name = 'PaymentGateway' OR Name = 'PaymentGatewayBackup') 
		AND ConfigValue NOT IN ('','authorizenet','braintree','cybersource','eprocessingnetwork','manual','moneris','payflowpro','paypal','qbmerchantservices','sagepayments','sagepayuk','skipjack','twocheckout','usaepay')

IF EXISTS (SELECT * FROM @removedGateways)
BEGIN

	SET @abortScript = 1

	PRINT '!!WARNING!!'
	PRINT 'Your site is configured to use a gateway that is no longer supported so the upgrade process is aborting.'
	PRINT 'The PaymentGateway and/or PaymentGatewayBackup AppConfigs will need to be set to a gateway that is still supported'
	PRINT 'or ''MANUAL'' to allow the upgrade to continue.  You will need to obtain a new gateway account and configure it before'
	PRINT 'continuing.  Supported gateways are listed in the latest manual.'
	PRINT ''
	PRINT 'Any pending transactions placed under the old gateway should be fully processed before upgrading.'
	PRINT ''
	PRINT 'Contact support at https://support.aspdotnetstorefront.com/ with any questions about this process.'
	PRINT ''

	DECLARE gateway_cursor CURSOR FOR SELECT * FROM @removedGateways
	OPEN gateway_cursor
	FETCH NEXT FROM gateway_cursor INTO @oldGateway, @gatewayType, @storeId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		Print '     Store ' 
			+ CAST(@storeId AS NVARCHAR(MAX)) 
			+ ' uses ' 
			+ @oldGateway 
			+ ' as the ' 
			+ CASE 
				WHEN @gatewayType = 'PaymentGateway' THEN 'payment gateway.'
				ELSE 'backup payment gateway.'
				END

		FETCH NEXT FROM gateway_cursor INTO @oldGateway, @gatewayType, @storeId
	END
END

IF @abortScript = 1
	SET NOEXEC ON	--Don't run the rest of the script until the gateway issue is resolved

PRINT '*****Database Upgrade Started*****'

/* ************************************************************** */
/* SCHEMA UPDATES												  */
/* ************************************************************** */

/* ======== Customer AdminCanViewCC Defaults ======== */
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'DF_Customer_AdminCanViewCC' AND type_desc='DEFAULT_CONSTRAINT')
	ALTER TABLE dbo.Customer DROP CONSTRAINT DF_Customer_AdminCanViewCC

ALTER TABLE dbo.Customer ADD CONSTRAINT DF_Customer_AdminCanViewCC
	DEFAULT (0) FOR AdminCanViewCC

UPDATE Customer SET AdminCanViewCC = 0 WHERE IsAdmin = 0

/* ======== Routing GlobalConfigs ======== */
if not exists (select * from GlobalConfig where Name = 'UrlMode')
	insert into GlobalConfig(
		[GlobalConfigGUID], 
		[Name], 
		[Description], 
		[ConfigValue], 
		[ValueType], 
		[GroupName], 
		[EnumValues], 
		[SuperOnly], 
		[Hidden], 
		[IsMultiStore])
	values (
		newid(),
		'UrlMode',
'Choose the mode that AspDotNetStorefront will use to recognize and generate URL''s. Note that no matter what mode is chosen, the site will always recognize modern URL''s.

Legacy Only: This site will only use URL''s compatible with AspDotNetStorefront 9.5.1 where possible. The site will still generate modern-style URL''s when there is no corresponding legacy page.

Modern with Legacy 301 Redirects: The site will only generate modern URL''s, but if any links come in on a legacy URL it will be redirected to the modern URL with a 301 redirect.

Modern Only: The site will only generate and recognize modern-style URL''s

If no option is chosen, the default is "Modern Only".',
		'Legacy Only',
		'enum',
		'ROUTING',
		'Legacy Only,Modern with Legacy 301 Redirects,Modern Only',
		0,
		0,
		0)

if not exists (select * from GlobalConfig where Name = 'EnableSeNameOnlyUrls')
	insert into GlobalConfig(
		[GlobalConfigGUID], 
		[Name], 
		[Description], 
		[ConfigValue], 
		[ValueType], 
		[GroupName], 
		[EnumValues], 
		[SuperOnly], 
		[Hidden], 
		[IsMultiStore])
	values (
		newid(),
		'EnableSeNameOnlyUrls',
'Set this to true to allow the site to recognize product and entity links that contain the SEName without an ID.

To use this feature, you must guarantee that every product and entity in your site has the SEName field populated in the database and that each SEName is unique among the products or entities of the same type.',
		'false',
		'boolean',
		'ROUTING',
		null,
		0,
		0,
		0)

/* ======== Authorize.Net URL updates ======== */
UPDATE AppConfig SET ConfigValue = 'https://api2.authorize.net/soap/v1/Service.asmx' WHERE Name = 'AUTHORIZENET_Cim_LiveServiceURL' AND ConfigValue = 'https://api.authorize.net/soap/v1/Service.asmx'
UPDATE AppConfig SET ConfigValue = 'https://secure2.authorize.net/gateway/transact.dll' WHERE Name = 'AUTHORIZENET_LIVE_SERVER' AND ConfigValue = 'https://secure.authorize.net/gateway/transact.dll'
UPDATE AppConfig SET ConfigValue = 'https://api2.authorize.net/xml/v1/request.api' WHERE Name = 'Recurring.AUTHORIZENET_AUTOBILL_LIVE_SERVER' AND ConfigValue = 'https://api.authorize.net/xml/v1/request.api'

/* ======== Signin Changes ======== */
UPDATE AppConfig SET Name = 'SecurityCodeRequiredDuringCheckout',
	Description = 'If TRUE, Captcha fields are added to the checkout page to prevent automated attacks. Captcha fields are ONLY used on the live server, not the development or staging servers, so make sure your LiveServer setting is also set to yourdomain.com.'
WHERE Name = 'SecurityCodeRequiredOnCreateAccountDuringCheckout'

/* ======== Session Timer ======== */
UPDATE AppConfig SET ConfigValue = '' WHERE Name = 'SessionTimeoutLandingPage' AND ConfigValue = 'default.aspx'

/* ======== C.O.D. Cleanup ======== */
--Change the allowed payment methods
UPDATE dbo.[AppConfig] SET ValueType = 'multiselect', AllowableValues = 'Credit Card,PayPalExpress,PayPal,Request Quote,Purchase Order,Check By Mail,C.O.D.,ECheck,MicroPay,CheckoutByAmazon,PayPal Payments Advanced' WHERE [Name] = 'PaymentMethods'
--Update LastPaymentMethodUsed for anyone who used one of the removed methods
UPDATE dbo.[Customer] SET RequestedPaymentMethod = 'C.O.D.' WHERE RequestedPaymentMethod LIKE '%C.O.D.%'
--Fix order history so these orders can be found by payment method
UPDATE dbo.[Orders] SET PaymentMethod = 'C.O.D.' WHERE PaymentMethod LIKE '%C.O.D.%'
--Fix the AppConfig.  This may leave some 'orphaned' commas but the software handles that OK
UPDATE dbo.[AppConfig] SET ConfigValue = REPLACE(ConfigValue, 'C.O.D. (Money Order)', '') WHERE Name = 'PaymentMethods'
UPDATE dbo.[AppConfig] SET ConfigValue = REPLACE(ConfigValue, 'C.O.D. (Company Check)', '') WHERE Name = 'PaymentMethods'
UPDATE dbo.[AppConfig] SET ConfigValue = REPLACE(ConfigValue, 'C.O.D. (Net 30)', '') WHERE Name = 'PaymentMethods'
--Customer table cleanup

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE parent_obj = OBJECT_ID(N'[Customer]') AND type = 'D' And Name = 'DF_Customer_CODCompanyCheckAllowed')
	ALTER TABLE Customer DROP CONSTRAINT DF_Customer_CODCompanyCheckAllowed

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Customer]') AND name = 'CODCompanyCheckAllowed')
	ALTER TABLE Customer DROP COLUMN CODCompanyCheckAllowed

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE parent_obj = OBJECT_ID(N'[Customer]') AND type = 'D' And Name = 'DF_Customer_CODNet30Allowed')
	ALTER TABLE Customer DROP CONSTRAINT DF_Customer_CODNet30Allowed

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Customer]') AND name = 'CODNet30Allowed')
ALTER TABLE Customer DROP COLUMN CODNet30Allowed
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insCustomer]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insCustomer]
GO
CREATE PROC [dbo].[aspdnsf_insCustomer]
    @Email nvarchar(100),
    @Password nvarchar(100),
    @SkinID int,
    @AffiliateID int,
    @Referrer nvarchar(max),
    @IsAdmin tinyint,
    @LastIPAddress varchar(40),
    @LocaleSetting nvarchar(10),
    @Over13Checked tinyint,
    @CurrencySetting nvarchar(10),
    @VATSetting int,
    @VATRegistrationID nvarchar(100),
    @CustomerLevelID int,
    @CustomerID int OUTPUT

AS
SET NOCOUNT ON

insert dbo.Customer(CustomerGUID, CustomerLevelID, RegisterDate, Email, Password, Gender, FirstName, LastName, Notes, SkinID, Phone, AffiliateID, Referrer, CouponCode, OkToEmail, IsAdmin, BillingEqualsShipping, LastIPAddress, OrderNotes, RTShipRequest, RTShipResponse, OrderOptions, LocaleSetting, MicroPayBalance, RecurringShippingMethodID, RecurringShippingMethod, BillingAddressID, ShippingAddressID, ExtensionData, FinalizationData, Deleted, CreatedOn, Over13Checked, CurrencySetting, VATSetting, VATRegistrationID, StoreCCInDB, IsRegistered, LockedUntil, AdminCanViewCC, PwdChanged, BadLoginCount, LastBadLogin, Active, PwdChangeRequired, SaltKey)
values
(
    newid(),
    @CustomerLevelID,
    getdate(),
    isnull(@Email, ''),
    isnull(@Password, ''),
    null,
    null,
    null,
    null,
    isnull(@SkinID, 1),
    null,
    @AffiliateID,
    @Referrer,
    null,
    1,
    isnull(@IsAdmin, 0),
    0,
    @LastIPAddress,
    null,
    null,
    null,
    null,
    isnull(@LocaleSetting, ('en-US')),
    0.0,
    1,
    null,
    null,
    null,
    null,
    null,
    0,
    getdate(),
    @Over13Checked,
    @CurrencySetting,
    @VATSetting,
    @VATRegistrationID,
    1,
    0,
    null,
    1,
    getdate(),
    0,
    null,
    1,
    0,
    0
)

set @CustomerID = @@identity
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetCustomerByID]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetCustomerByID]
GO
create proc [dbo].[aspdnsf_GetCustomerByID]
    @CustomerID int

AS
BEGIN
    SET NOCOUNT ON

    DECLARE @CustomerSessionID int = -1, @LastActivity datetime = '1/1/1900'

    SELECT  @CustomerSessionID  = cs.CustomerSessionID , @LastActivity = cs.LastActivity
    FROM dbo.CustomerSession cs with (nolock)
    WHERE cs.CustomerID = @CustomerID

    SELECT top 1
            c.CustomerID, c.CustomerGUID, c.CustomerLevelID, c.RegisterDate, c.Email, c.Password, c.SaltKey, c.Gender,
            c.FirstName, c.LastName, c.Notes, c.SkinID, c.Phone, c.AffiliateID, c.Referrer, c.CouponCode, c.OkToEmail,
            IsAdmin&1 IsAdmin, sign(IsAdmin&2) IsSuperAdmin, c.BillingEqualsShipping, c.LastIPAddress,
            c.OrderNotes, c.RTShipRequest, c.RTShipResponse, c.OrderOptions, c.LocaleSetting,
            c.MicroPayBalance, c.RecurringShippingMethodID, c.RecurringShippingMethod, c.BillingAddressID, c.ShippingAddressID,
            c.ExtensionData, c.FinalizationData, c.Deleted, c.CreatedOn, c.Over13Checked, c.CurrencySetting,
            case when isnull(cl.CustomerLevelID, 0) > 0 and cl.LevelHasNoTax = 1 then 2 else c.VATSetting end VATSetting,
            c.VATRegistrationID, c.StoreCCInDB, c.IsRegistered, c.LockedUntil, c.AdminCanViewCC, c.PwdChanged, c.BadLoginCount,
            c.LastBadLogin, c.Active, c.PwdChangeRequired, c.SaltKey, isnull(cl.LevelDiscountPercent, 0) LevelDiscountPercent,
            isnull(cl.LevelDiscountsApplyToExtendedPrices, 0) LevelDiscountsApplyToExtendedPrices, c.RequestedPaymentMethod,
            @CustomerSessionID CustomerSessionID,
            @LastActivity LastActivity, c.StoreID, d.Name StoreName
    FROM dbo.Customer c with (nolock) left join dbo.CustomerLevel cl with (nolock) on c.CustomerLevelID = cl.CustomerLevelID
    left join Store d with (nolock) on c.StoreID = d.StoreID
    WHERE c.Deleted=0 and c.CustomerID = @CustomerID
END
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetCustomerByGUID]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetCustomerByGUID]
GO
create proc [dbo].[aspdnsf_GetCustomerByGUID]
    @CustomerGUID uniqueidentifier

AS
BEGIN
    SET NOCOUNT ON

    DECLARE @CustomerSessionID int = -1, @LastActivity datetime = '1/1/1900'

    SELECT  @CustomerSessionID  = cs.CustomerSessionID , @LastActivity = cs.LastActivity
    FROM dbo.CustomerSession cs with (nolock)
		LEFT JOIN dbo.Customer c with (nolock) on cs.CustomerID = c.CustomerID
              WHERE c.CustomerGUID = @CustomerGUID

    SELECT top 1
            c.CustomerID, c.CustomerGUID, c.CustomerLevelID, c.RegisterDate, c.Email, c.Password, c.SaltKey, c.Gender,
            c.FirstName, c.LastName, c.Notes, c.SkinID, c.Phone, c.AffiliateID, c.Referrer, c.CouponCode, c.OkToEmail,
            IsAdmin&1 IsAdmin, sign(IsAdmin&2) IsSuperAdmin, c.BillingEqualsShipping, c.LastIPAddress,
            c.OrderNotes, c.RTShipRequest, c.RTShipResponse, c.OrderOptions, c.LocaleSetting,
            c.MicroPayBalance, c.RecurringShippingMethodID, c.RecurringShippingMethod, c.BillingAddressID, c.ShippingAddressID,
            c.ExtensionData, c.FinalizationData, c.Deleted, c.CreatedOn, c.Over13Checked, c.CurrencySetting,
            case when isnull(cl.CustomerLevelID, 0) > 0 and cl.LevelHasNoTax = 1 then 2 else c.VATSetting end VATSetting,
            c.VATRegistrationID, c.StoreCCInDB, c.IsRegistered, c.LockedUntil, c.AdminCanViewCC, c.PwdChanged, c.BadLoginCount,
            c.LastBadLogin, c.Active, c.PwdChangeRequired, c.SaltKey, isnull(cl.LevelDiscountPercent, 0) LevelDiscountPercent,
            isnull(cl.LevelDiscountsApplyToExtendedPrices, 0) LevelDiscountsApplyToExtendedPrices, c.RequestedPaymentMethod,
            @CustomerSessionID CustomerSessionID, @LastActivity LastActivity, c.StoreID, d.Name StoreName
    FROM dbo.Customer c with (nolock) left join dbo.CustomerLevel cl with (nolock) on c.CustomerLevelID = cl.CustomerLevelID
    left join Store d with (nolock) on c.StoreID = d.StoreID
    WHERE c.Deleted=0 and c.CustomerGUID = @CustomerGUID
END
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetCustomerByEmail]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetCustomerByEmail]
GO
CREATE PROC [dbo].[aspdnsf_GetCustomerByEmail]
    @Email nvarchar(100),
    @FilterCustomer bit,
    @StoreID int = 1,
    @AdminOnly bit = 0
AS
BEGIN
    SET NOCOUNT ON

    DECLARE @CustomerSessionID int = -1, @LastActivity datetime = '1/1/1900'

    SELECT  @CustomerSessionID  = cs.CustomerSessionID, @LastActivity = cs.LastActivity
    FROM dbo.CustomerSession cs with (nolock)
		LEFT JOIN dbo.Customer c with (nolock) on cs.CustomerID = c.CustomerID
    WHERE c.Email = @Email
		and (@FilterCustomer = 0 or c.StoreID = @StoreID)
		and (@AdminOnly = 0 or c.IsAdmin > 0)

    SELECT top 1
            c.CustomerID, c.CustomerGUID, c.CustomerLevelID, c.RegisterDate, c.Email, c.Password, c.SaltKey, c.Gender,
            c.FirstName, c.LastName, c.Notes, c.SkinID, c.Phone, c.AffiliateID, c.Referrer, c.CouponCode, c.OkToEmail,
            IsAdmin&1 IsAdmin, sign(IsAdmin&2) IsSuperAdmin, c.BillingEqualsShipping, c.LastIPAddress,
            c.OrderNotes, c.RTShipRequest, c.RTShipResponse, c.OrderOptions, c.LocaleSetting,
            c.MicroPayBalance, c.RecurringShippingMethodID, c.RecurringShippingMethod, c.BillingAddressID, c.ShippingAddressID,
            c.ExtensionData, c.FinalizationData, c.Deleted, c.CreatedOn, c.Over13Checked, c.CurrencySetting,
            case when isnull(cl.CustomerLevelID, 0) > 0 and cl.LevelHasNoTax = 1 then 2 else c.VATSetting end VATSetting,
            c.VATRegistrationID, c.StoreCCInDB, c.IsRegistered, c.LockedUntil, c.AdminCanViewCC, c.PwdChanged, c.BadLoginCount,
            c.LastBadLogin, c.Active, c.PwdChangeRequired, c.SaltKey, isnull(cl.LevelDiscountPercent, 0) LevelDiscountPercent,
            isnull(cl.LevelDiscountsApplyToExtendedPrices, 0) LevelDiscountsApplyToExtendedPrices, c.RequestedPaymentMethod,
            @CustomerSessionID CustomerSessionID, @LastActivity LastActivity, c.StoreID, d.Name StoreName
    FROM dbo.Customer c with (nolock) left join dbo.CustomerLevel cl with (nolock) on c.CustomerLevelID = cl.CustomerLevelID
    left join Store d with (nolock) on c.StoreID = d.StoreID
    WHERE c.Deleted=0
		and c.Email = @Email
		and ((@filtercustomer = 0 or IsAdmin > 0) or c.StoreID = @StoreID)
		and (@AdminOnly = 0 or c.IsAdmin > 0)
    ORDER BY c.IsRegistered desc, c.CreatedOn desc
END
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_updCustomerByEmail]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_updCustomerByEmail]
GO
CREATE proc [dbo].[aspdnsf_updCustomerByEmail]
    @Email nvarchar(100),
    @CustomerLevelID int = null,
    @Password nvarchar(250) = null,
    @SaltKey int = null,
    @Gender nvarchar(1) = null,
    @FirstName nvarchar(100) = null,
    @LastName nvarchar(100) = null,
    @Notes nvarchar(max) = null,
    @SkinID int = null,
    @Phone nvarchar(25) = null,
    @AffiliateID int = null,
    @Referrer nvarchar(max) = null,
    @CouponCode nvarchar(50) = null,
    @OkToEmail tinyint = null,
    @IsAdmin tinyint = null,
    @BillingEqualsShipping tinyint = null,
    @LastIPAddress varchar(40) = null,
    @OrderNotes nvarchar(max) = null,
    @RTShipRequest nvarchar(max) = null,
    @RTShipResponse nvarchar(max) = null,
    @OrderOptions nvarchar(max) = null,
    @LocaleSetting nvarchar(10) = null,
    @MicroPayBalance money = null,
    @RecurringShippingMethodID int = null,
    @RecurringShippingMethod nvarchar(100) = null,
    @BillingAddressID int = null,
    @ShippingAddressID int = null,
    @ExtensionData nvarchar(max) = null,
    @FinalizationData nvarchar(max) = null,
    @Deleted tinyint = null,
    @Over13Checked tinyint = null,
    @CurrencySetting nvarchar(10) = null,
    @VATSetting int = null,
    @VATRegistrationID nvarchar(100) = null,
    @StoreCCInDB tinyint = null,
    @IsRegistered tinyint = null,
    @LockedUntil datetime = null,
    @AdminCanViewCC tinyint = null,
    @BadLogin smallint = 0 , --only pass -1 = null, 0 = null, or 1: -1 clears the field = null, 0 does nothing = null, 1 increments the field by one
    @Active tinyint = null,
    @PwdChangeRequired tinyint = null,
    @RequestedPaymentMethod  nvarchar(100) = null

AS
SET NOCOUNT ON

DECLARE @CustomerID int, @OldPwd nvarchar(100), @IsAdminCust tinyint, @OldSaltKey int

SELECT @CustomerID = CustomerID , @OldPwd = Password, @IsAdminCust = IsAdmin, @OldSaltKey = Saltkey FROM dbo.Customer WHERE Email = @Email


UPDATE dbo.Customer
SET
    CustomerLevelID = COALESCE(@CustomerLevelID, CustomerLevelID),
    Email = COALESCE(@Email, Email),
    Password = COALESCE(@Password, Password),
    SaltKey = COALESCE(@SaltKey, SaltKey),
    Gender = COALESCE(@Gender, Gender),
    FirstName = COALESCE(@FirstName, FirstName),
    LastName = COALESCE(@LastName, LastName),
    Notes = COALESCE(@Notes, Notes),
    SkinID = COALESCE(@SkinID, SkinID),
    Phone = COALESCE(@Phone, Phone),
    AffiliateID = COALESCE(@AffiliateID, AffiliateID),
    Referrer = COALESCE(@Referrer, Referrer),
    CouponCode = COALESCE(@CouponCode, CouponCode),
    OkToEmail = COALESCE(@OkToEmail, OkToEmail),
    IsAdmin = COALESCE(@IsAdmin, IsAdmin),
    BillingEqualsShipping = COALESCE(@BillingEqualsShipping, BillingEqualsShipping),
    LastIPAddress = COALESCE(@LastIPAddress, LastIPAddress),
    OrderNotes = COALESCE(@OrderNotes, OrderNotes),
    RTShipRequest = COALESCE(@RTShipRequest, RTShipRequest),
    RTShipResponse = COALESCE(@RTShipResponse, RTShipResponse),
    OrderOptions = COALESCE(@OrderOptions, OrderOptions),
    LocaleSetting = COALESCE(@LocaleSetting, LocaleSetting),
    MicroPayBalance = COALESCE(@MicroPayBalance, MicroPayBalance),
    RecurringShippingMethodID = COALESCE(@RecurringShippingMethodID, RecurringShippingMethodID),
    RecurringShippingMethod = COALESCE(@RecurringShippingMethod, RecurringShippingMethod),
    BillingAddressID = COALESCE(@BillingAddressID, BillingAddressID),
    ShippingAddressID = COALESCE(@ShippingAddressID, ShippingAddressID),
    ExtensionData = COALESCE(@ExtensionData, ExtensionData),
    FinalizationData = COALESCE(@FinalizationData, FinalizationData),
    Deleted = COALESCE(@Deleted, Deleted),
    Over13Checked = COALESCE(@Over13Checked, Over13Checked),
    CurrencySetting = COALESCE(@CurrencySetting, CurrencySetting),
    VATSetting = COALESCE(@VATSetting, VATSetting),
    VATRegistrationID = COALESCE(@VATRegistrationID, VATRegistrationID),
    StoreCCInDB = COALESCE(@StoreCCInDB, StoreCCInDB),
    IsRegistered = COALESCE(@IsRegistered, IsRegistered),
    LockedUntil = COALESCE(@LockedUntil, LockedUntil),
    AdminCanViewCC = COALESCE(@AdminCanViewCC, AdminCanViewCC),
    PwdChanged = case when @OldPwd <> @Password and @Password is not null then getdate() else PwdChanged end,
    BadLoginCount = case @BadLogin when -1 then 0 else BadLoginCount + @BadLogin end,
    LastBadLogin = case @BadLogin when -1 then null when 1 then getdate() else LastBadLogin end,
    Active = COALESCE(@Active, Active),
    PwdChangeRequired = COALESCE(@PwdChangeRequired, PwdChangeRequired),
    RequestedPaymentMethod = COALESCE(@RequestedPaymentMethod, RequestedPaymentMethod)
WHERE Email = @Email

IF @IsAdminCust = 1 and @OldPwd <> @Password
    INSERT dbo.PasswordLog (CustomerID, OldPwd, SaltKey, ChangeDt)
    VALUES (@CustomerID, @OldPwd, @OldSaltKey, getdate())

GO

-- Begin eCheck Clean Up ---
Update Customer set RequestedPaymentMethod = null where RequestedPaymentMethod = 'ECHECK'
go

update dbo.[AppConfig] set ValueType = 'multiselect', AllowableValues = 'Credit Card,PayPalExpress,PayPal,Request Quote,Purchase Order,Check By Mail,C.O.D.,MicroPay,CheckoutByAmazon,PayPal Payments Advanced' WHERE [Name] = 'PaymentMethods'
update AppConfig SET Description = 'Specifies how the site handles credit cards in real-time when an order is entered. AUTH means that the card is ONLY authorized; you will have to use the admin panel to later capture the amount, or process the card manually offline. AUTH CAPTURE means that the card is authorized AND captured in real-time.' WHERE Name = 'TransactionMode'
go

declare @tableId varchar(max)
select @tableId = object_id from sys.tables where name = 'Address'

if exists(select * from sys.columns where object_id = @tableId and name = 'eCheckBankABACode')
	alter table address drop column eCheckBankABACode

if exists(select * from sys.columns where object_id = @tableId and name = 'eCheckBankAccountNumber')
	alter table address drop column eCheckBankAccountNumber

if exists(select * from sys.columns where object_id = @tableId and name = 'eCheckBankAccountType')
	alter table address drop column eCheckBankAccountType

if exists(select * from sys.columns where object_id = @tableId and name = 'eCheckBankName')
	alter table address drop column eCheckBankName

if exists(select * from sys.columns where object_id = @tableId and name = 'eCheckBankAccountName')
	alter table address drop column eCheckBankAccountName

select @tableId = object_id from sys.tables where name = 'Orders'

if exists(select * from sys.columns where object_id = @tableId and name = 'eCheckBankABACode')
	alter table orders drop column eCheckBankABACode

if exists(select * from sys.columns where object_id = @tableId and name = 'eCheckBankAccountNumber')
	alter table orders drop column eCheckBankAccountNumber

if exists(select * from sys.columns where object_id = @tableId and name = 'eCheckBankAccountType')
	alter table orders drop column eCheckBankAccountType

if exists(select * from sys.columns where object_id = @tableId and name = 'eCheckBankName')
	alter table orders drop column eCheckBankName

if exists(select * from sys.columns where object_id = @tableId and name = 'eCheckBankAccountName')
	alter table orders drop column eCheckBankAccountName
go

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_getOrder]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_getOrder]
GO
CREATE proc [dbo].[aspdnsf_getOrder]
	@ordernumber int
as
set nocount on
select
	o.OrderNumber,
	o.OrderGUID,
	o.ParentOrderNumber,
	o.StoreVersion,
	o.QuoteCheckout,
	o.IsNew,
	o.ShippedOn,
	o.CustomerID,
	o.CustomerGUID,
	o.Referrer,
	o.SkinID,
	o.LastName,
	o.FirstName,
	o.Email,
	o.Notes,
	o.BillingEqualsShipping,
	o.BillingLastName,
	o.BillingFirstName,
	o.BillingCompany,
	o.BillingAddress1,
	o.BillingAddress2,
	o.BillingSuite,
	o.BillingCity,
	o.BillingState,
	o.BillingZip,
	o.BillingCountry,
	o.BillingPhone,
	o.ShippingLastName,
	o.ShippingFirstName,
	o.ShippingCompany,
	o.ShippingResidenceType,
	o.ShippingAddress1,
	o.ShippingAddress2,
	o.ShippingSuite,
	o.ShippingCity,
	o.ShippingState,
	o.ShippingZip,
	o.ShippingCountry,
	o.ShippingMethodID,
	o.ShippingMethod,
	o.ShippingPhone,
	o.ShippingCalculationID,
	o.Phone,
	o.RegisterDate,
	o.AffiliateID,
	o.CouponCode,
	o.CouponType,
	o.CouponDescription,
	o.CouponDiscountAmount,
	o.CouponDiscountPercent,
	o.CouponIncludesFreeShipping,
	o.OkToEmail,
	o.Deleted,
	o.CardType,
	o.CardName,
	o.CardNumber,
	o.CardExpirationMonth,
	o.CardExpirationYear,
	o.OrderSubtotal,
	o.OrderTax,
	o.OrderShippingCosts,
	o.OrderTotal,
	o.PaymentGateway,
	o.AuthorizationCode,
	o.AuthorizationResult,
	o.AuthorizationPNREF,
	o.TransactionCommand,
	o.OrderDate,
	o.LevelID,
	o.LevelName,
	o.LevelDiscountPercent,
	o.LevelDiscountAmount,
	o.LevelHasFreeShipping,
	o.LevelAllowsQuantityDiscounts,
	o.LevelHasNoTax,
	o.LevelAllowsCoupons,
	o.LevelDiscountsApplyToExtendedPrices,
	o.LastIPAddress,
	o.PaymentMethod,
	o.OrderNotes,
	o.PONumber,
	o.DownloadEmailSentOn,
	o.ReceiptEmailSentOn,
	o.DistributorEmailSentOn,
	o.ShippingTrackingNumber,
	o.ShippedVIA,
	o.CustomerServiceNotes,
	o.RTShipRequest,
	o.RTShipResponse,
	o.TransactionState,
	o.AVSResult,
	o.CaptureTXCommand,
	o.CaptureTXResult,
	o.VoidTXCommand,
	o.VoidTXResult,
	o.RefundTXCommand,
	o.RefundTXResult,
	o.CardinalLookupResult,
	o.CardinalAuthenticateResult,
	o.CardinalGatewayParms,
	o.AffiliateCommissionRecorded,
	o.OrderOptions,
	o.OrderWeight,
	o.CarrierReportedRate,
	o.CarrierReportedWeight,
	o.LocaleSetting,
	o.FinalizationData,
	o.ExtensionData,
	o.AlreadyConfirmed,
	o.CartType,
	o.THUB_POSTED_TO_ACCOUNTING,
	o.THUB_POSTED_DATE,
	o.THUB_ACCOUNTING_REF,
	o.Last4,
	o.ReadyToShip,
	o.IsPrinted,
	o.AuthorizedOn,
	o.CapturedOn,
	o.RefundedOn,
	o.VoidedOn,
	o.EditedOn,
	o.InventoryWasReduced,
	o.MaxMindFraudScore,
	o.MaxMindDetails,
	o.CardStartDate,
	o.CardIssueNumber,
	o.TransactionType,
	o.Crypt,
	o.VATRegistrationID,
	o.FraudedOn,
	o.RefundReason,
	o.AuthorizationPNREF as TransactionID,
	o.RecurringSubscriptionID,
	o.RelatedOrderNumber,
	o.ReceiptHtml,
	os.ShoppingCartRecID,
	os.IsTaxable,
	os.IsShipSeparately,
	os.IsDownload,
	os.DownloadLocation,
	os.FreeShipping,
	os.DistributorID,
	os.ShippingDetail,
	os.TaxClassID,
	os.TaxRate,
	os.Notes,
	os.CustomerEntersPrice,
	os.ProductID,
	os.VariantID,
	os.Quantity,
	os.ChosenColor,
	os.ChosenColorSKUModifier,
	os.ChosenSize,
	os.ChosenSizeSKUModifier,
	os.TextOption,
	os.SizeOptionPrompt,
	os.ColorOptionPrompt,
	os.TextOptionPrompt,
	os.CustomerEntersPricePrompt,
	os.OrderedProductQuantityDiscountID,
	os.OrderedProductQuantityDiscountName,
	os.OrderedProductQuantityDiscountPercent,
	os.OrderedProductName,
	os.OrderedProductVariantName,
	os.OrderedProductSKU,
	os.OrderedProductManufacturerPartNumber ,
	os.OrderedProductPrice,
	os.OrderedProductWeight,
	os.OrderedProductPrice,
	os.ShippingMethodID,
	os.ShippingMethodID CartItemShippingMethodID,
	os.ShippingMethod CartItemShippingMethod,
	os.ShippingAddressID,
	os.IsAKit
from Orders o with (nolock)
	left join orders_ShoppingCart os with (nolock) ON os.OrderNumber = o.OrderNumber
where o.OrderNumber = @ordernumber
order by os.ShippingAddressID
go

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'dbo.aspdnsf_updOrders') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc dbo.aspdnsf_updOrders
GO
create proc dbo.aspdnsf_updOrders
    @OrderNumber int,
    @ParentOrderNumber int = null,
    @StoreVersion nvarchar(100) = null,
    @QuoteCheckout tinyint = null,
    @IsNew tinyint = null,
    @ShippedOn datetime = null,
    @CustomerID int = null,
    @CustomerGUID uniqueidentifier = null,
    @Referrer nvarchar(max) = null,
    @SkinID int = null,
    @LastName nvarchar(100) = null,
    @FirstName nvarchar(100) = null,
    @Email nvarchar(100) = null,
    @Notes nvarchar(max) = null,
    @BillingEqualsShipping tinyint = null,
    @BillingLastName nvarchar(100) = null,
    @BillingFirstName nvarchar(100) = null,
    @BillingCompany nvarchar(100) = null,
    @BillingAddress1 nvarchar(100) = null,
    @BillingAddress2 nvarchar(100) = null,
    @BillingSuite nvarchar(50) = null,
    @BillingCity nvarchar(100) = null,
    @BillingState nvarchar(100) = null,
    @BillingZip nvarchar(10) = null,
    @BillingCountry nvarchar(100) = null,
    @BillingPhone nvarchar(25) = null,
    @ShippingLastName nvarchar(100) = null,
    @ShippingFirstName nvarchar(100) = null,
    @ShippingCompany nvarchar(100) = null,
    @ShippingResidenceType int = null,
    @ShippingAddress1 nvarchar(100) = null,
    @ShippingAddress2 nvarchar(100) = null,
    @ShippingSuite nvarchar(50) = null,
    @ShippingCity nvarchar(100) = null,
    @ShippingState nvarchar(100) = null,
    @ShippingZip nvarchar(10) = null,
    @ShippingCountry nvarchar(100) = null,
    @ShippingMethodID int = null,
    @ShippingMethod nvarchar(max) = null,
    @ShippingPhone nvarchar(25) = null,
    @ShippingCalculationID int = null,
    @Phone nvarchar(20) = null,
    @RegisterDate datetime = null,
    @AffiliateID int = null,
    @CouponCode nvarchar(50) = null,
    @CouponType int = null,
    @CouponDescription nvarchar(max) = null,
    @CouponDiscountAmount money = null,
    @CouponDiscountPercent money = null,
    @CouponIncludesFreeShipping tinyint = null,
    @OkToEmail tinyint = null,
    @Deleted tinyint = null,
    @CardType nvarchar(20) = null,
    @CardName nvarchar(100) = null,
    @CardNumber nvarchar(max) = null,
    @CardExpirationMonth nvarchar(10) = null,
    @CardExpirationYear nvarchar(10) = null,
    @OrderSubtotal money = null,
    @OrderTax money = null,
    @OrderShippingCosts money = null,
    @OrderTotal money = null,
    @PaymentGateway nvarchar(50) = null,
    @AuthorizationCode nvarchar(100) = null,
    @AuthorizationResult nvarchar(max) = null,
    @AuthorizationPNREF nvarchar(100) = null,
    @TransactionCommand nvarchar(max) = null,
    @OrderDate datetime = null,
    @LevelID int = null,
    @LevelName nvarchar(100) = null,
    @LevelDiscountPercent money = null,
    @LevelDiscountAmount money = null,
    @LevelHasFreeShipping tinyint = null,
    @LevelAllowsQuantityDiscounts tinyint = null,
    @LevelHasNoTax tinyint = null,
    @LevelAllowsCoupons tinyint = null,
    @LevelDiscountsApplyToExtendedPrices tinyint = null,
    @LastIPAddress varchar(40) = null,
    @PaymentMethod nvarchar(100) = null,
    @OrderNotes nvarchar(max) = null,
    @PONumber nvarchar(50) = null,
    @DownloadEmailSentOn datetime = null,
    @ReceiptEmailSentOn datetime = null,
    @DistributorEmailSentOn datetime = null,
    @ShippingTrackingNumber nvarchar(100) = null,
    @ShippedVIA nvarchar(100) = null,
    @CustomerServiceNotes nvarchar(max) = null,
    @RTShipRequest nvarchar(max) = null,
    @RTShipResponse nvarchar(max) = null,
    @TransactionState nvarchar(20) = null,
    @AVSResult nvarchar(50) = null,
    @CaptureTXCommand nvarchar(max) = null,
    @CaptureTXResult nvarchar(max) = null,
    @VoidTXCommand nvarchar(max) = null,
    @VoidTXResult nvarchar(max) = null,
    @RefundTXCommand nvarchar(max) = null,
    @RefundTXResult nvarchar(max) = null,
    @CardinalLookupResult nvarchar(max) = null,
    @CardinalAuthenticateResult nvarchar(max) = null,
    @CardinalGatewayParms nvarchar(max) = null,
    @AffiliateCommissionRecorded tinyint = null,
    @OrderOptions nvarchar(max) = null,
    @OrderWeight money = null,
    @CarrierReportedRate nvarchar(max) = null,
    @CarrierReportedWeight nvarchar(max) = null,
    @LocaleSetting nvarchar(10) = null,
    @FinalizationData nvarchar(max) = null,
    @ExtensionData nvarchar(max) = null,
    @AlreadyConfirmed tinyint = null,
    @CartType int = null,
    @THUB_POSTED_TO_ACCOUNTING char(1) = null,
    @THUB_POSTED_DATE datetime = null,
    @THUB_ACCOUNTING_REF char(25) = null,
    @Last4 nvarchar(4) = null,
    @ReadyToShip tinyint = null,
    @IsPrinted tinyint = null,
    @AuthorizedOn datetime = null,
    @CapturedOn datetime = null,
    @RefundedOn datetime = null,
    @VoidedOn datetime = null,
    @InventoryWasReduced int = null,
    @MaxMindFraudScore decimal(5, 2) = null,
    @MaxMindDetails nvarchar(max) = null,
    @CardStartDate nvarchar(20) = null,
    @CardIssueNumber nvarchar(25) = null,
    @TransactionType int = null,
    @Crypt int = null,
    @VATRegistrationID nvarchar(max) = null,
    @FraudedOn tinyint = null,
    @RefundReason nvarchar(max) = null
as
set nocount on

update dbo.Orders
set
	ParentOrderNumber = COALESCE(@ParentOrderNumber, ParentOrderNumber),
	StoreVersion = COALESCE(@StoreVersion, StoreVersion),
	QuoteCheckout = COALESCE(@QuoteCheckout, QuoteCheckout),
	IsNew = COALESCE(@IsNew, IsNew),
	ShippedOn = COALESCE(@ShippedOn, ShippedOn),
	CustomerID = COALESCE(@CustomerID, CustomerID),
	CustomerGUID = COALESCE(@CustomerGUID, CustomerGUID),
	Referrer = COALESCE(@Referrer, Referrer),
	SkinID = COALESCE(@SkinID, SkinID),
	LastName = COALESCE(@LastName, LastName),
	FirstName = COALESCE(@FirstName, FirstName),
	Email = COALESCE(@Email, Email),
	Notes = COALESCE(@Notes, Notes),
	BillingEqualsShipping = COALESCE(@BillingEqualsShipping, BillingEqualsShipping),
	BillingLastName = COALESCE(@BillingLastName, BillingLastName),
	BillingFirstName = COALESCE(@BillingFirstName, BillingFirstName),
	BillingCompany = COALESCE(@BillingCompany, BillingCompany),
	BillingAddress1 = COALESCE(@BillingAddress1, BillingAddress1),
	BillingAddress2 = COALESCE(@BillingAddress2, BillingAddress2),
	BillingSuite = COALESCE(@BillingSuite, BillingSuite),
	BillingCity = COALESCE(@BillingCity, BillingCity),
	BillingState = COALESCE(@BillingState, BillingState),
	BillingZip = COALESCE(@BillingZip, BillingZip),
	BillingCountry = COALESCE(@BillingCountry, BillingCountry),
	BillingPhone = COALESCE(@BillingPhone, BillingPhone),
	ShippingLastName = COALESCE(@ShippingLastName, ShippingLastName),
	ShippingFirstName = COALESCE(@ShippingFirstName, ShippingFirstName),
	ShippingCompany = COALESCE(@ShippingCompany, ShippingCompany),
	ShippingResidenceType = COALESCE(@ShippingResidenceType, ShippingResidenceType),
	ShippingAddress1 = COALESCE(@ShippingAddress1, ShippingAddress1),
	ShippingAddress2 = COALESCE(@ShippingAddress2, ShippingAddress2),
	ShippingSuite = COALESCE(@ShippingSuite, ShippingSuite),
	ShippingCity = COALESCE(@ShippingCity, ShippingCity),
	ShippingState = COALESCE(@ShippingState, ShippingState),
	ShippingZip = COALESCE(@ShippingZip, ShippingZip),
	ShippingCountry = COALESCE(@ShippingCountry, ShippingCountry),
	ShippingMethodID = COALESCE(@ShippingMethodID, ShippingMethodID),
	ShippingMethod = COALESCE(@ShippingMethod, ShippingMethod),
	ShippingPhone = COALESCE(@ShippingPhone, ShippingPhone),
	ShippingCalculationID = COALESCE(@ShippingCalculationID, ShippingCalculationID),
	Phone = COALESCE(@Phone, Phone),
	RegisterDate = COALESCE(@RegisterDate, RegisterDate),
	AffiliateID = COALESCE(@AffiliateID, AffiliateID),
	CouponCode = COALESCE(@CouponCode, CouponCode),
	CouponType = COALESCE(@CouponType, CouponType),
	CouponDescription = COALESCE(@CouponDescription, CouponDescription),
	CouponDiscountAmount = COALESCE(@CouponDiscountAmount, CouponDiscountAmount),
	CouponDiscountPercent = COALESCE(@CouponDiscountPercent, CouponDiscountPercent),
	CouponIncludesFreeShipping = COALESCE(@CouponIncludesFreeShipping, CouponIncludesFreeShipping),
	OkToEmail = COALESCE(@OkToEmail, OkToEmail),
	Deleted = COALESCE(@Deleted, Deleted),
	CardType = COALESCE(@CardType, CardType),
	CardName = COALESCE(@CardName, CardName),
	CardNumber = COALESCE(@CardNumber, CardNumber),
	CardExpirationMonth = COALESCE(@CardExpirationMonth, CardExpirationMonth),
	CardExpirationYear = COALESCE(@CardExpirationYear, CardExpirationYear),
	OrderSubtotal = COALESCE(@OrderSubtotal, OrderSubtotal),
	OrderTax = COALESCE(@OrderTax, OrderTax),
	OrderShippingCosts = COALESCE(@OrderShippingCosts, OrderShippingCosts),
	OrderTotal = COALESCE(@OrderTotal, OrderTotal),
	PaymentGateway = COALESCE(@PaymentGateway, PaymentGateway),
	AuthorizationCode = COALESCE(@AuthorizationCode, AuthorizationCode),
	AuthorizationResult = COALESCE(@AuthorizationResult, AuthorizationResult),
	AuthorizationPNREF = COALESCE(@AuthorizationPNREF, AuthorizationPNREF),
	TransactionCommand = COALESCE(@TransactionCommand, TransactionCommand),
	OrderDate = COALESCE(@OrderDate, OrderDate),
	LevelID = COALESCE(@LevelID, LevelID),
	LevelName = COALESCE(@LevelName, LevelName),
	LevelDiscountPercent = COALESCE(@LevelDiscountPercent, LevelDiscountPercent),
	LevelDiscountAmount = COALESCE(@LevelDiscountAmount, LevelDiscountAmount),
	LevelHasFreeShipping = COALESCE(@LevelHasFreeShipping, LevelHasFreeShipping),
	LevelAllowsQuantityDiscounts = COALESCE(@LevelAllowsQuantityDiscounts, LevelAllowsQuantityDiscounts),
	LevelHasNoTax = COALESCE(@LevelHasNoTax, LevelHasNoTax),
	LevelAllowsCoupons = COALESCE(@LevelAllowsCoupons, LevelAllowsCoupons),
	LevelDiscountsApplyToExtendedPrices = COALESCE(@LevelDiscountsApplyToExtendedPrices, LevelDiscountsApplyToExtendedPrices),
	LastIPAddress = COALESCE(@LastIPAddress, LastIPAddress),
	PaymentMethod = COALESCE(@PaymentMethod, PaymentMethod),
	OrderNotes = COALESCE(@OrderNotes, OrderNotes),
	PONumber = COALESCE(@PONumber, PONumber),
	DownloadEmailSentOn = COALESCE(@DownloadEmailSentOn, DownloadEmailSentOn),
	ReceiptEmailSentOn = COALESCE(@ReceiptEmailSentOn, ReceiptEmailSentOn),
	DistributorEmailSentOn = COALESCE(@DistributorEmailSentOn, DistributorEmailSentOn),
	ShippingTrackingNumber = COALESCE(@ShippingTrackingNumber, ShippingTrackingNumber),
	ShippedVIA = COALESCE(@ShippedVIA, ShippedVIA),
	CustomerServiceNotes = COALESCE(@CustomerServiceNotes, CustomerServiceNotes),
	RTShipRequest = COALESCE(@RTShipRequest, RTShipRequest),
	RTShipResponse = COALESCE(@RTShipResponse, RTShipResponse),
	TransactionState = COALESCE(@TransactionState, TransactionState),
	AVSResult = COALESCE(@AVSResult, AVSResult),
	CaptureTXCommand = COALESCE(@CaptureTXCommand, CaptureTXCommand),
	CaptureTXResult = COALESCE(@CaptureTXResult, CaptureTXResult),
	VoidTXCommand = COALESCE(@VoidTXCommand, VoidTXCommand),
	VoidTXResult = COALESCE(@VoidTXResult, VoidTXResult),
	RefundTXCommand = COALESCE(@RefundTXCommand, RefundTXCommand),
	RefundTXResult = COALESCE(@RefundTXResult, RefundTXResult),
	CardinalLookupResult = COALESCE(@CardinalLookupResult, CardinalLookupResult),
	CardinalAuthenticateResult = COALESCE(@CardinalAuthenticateResult, CardinalAuthenticateResult),
	CardinalGatewayParms = COALESCE(@CardinalGatewayParms, CardinalGatewayParms),
	AffiliateCommissionRecorded = COALESCE(@AffiliateCommissionRecorded, AffiliateCommissionRecorded),
	OrderOptions = COALESCE(@OrderOptions, OrderOptions),
	OrderWeight = COALESCE(@OrderWeight, OrderWeight),
	CarrierReportedRate = COALESCE(@CarrierReportedRate, CarrierReportedRate),
	CarrierReportedWeight = COALESCE(@CarrierReportedWeight, CarrierReportedWeight),
	LocaleSetting = COALESCE(@LocaleSetting, LocaleSetting),
	FinalizationData = COALESCE(@FinalizationData, FinalizationData),
	ExtensionData = COALESCE(@ExtensionData, ExtensionData),
	AlreadyConfirmed = COALESCE(@AlreadyConfirmed, AlreadyConfirmed),
	CartType = COALESCE(@CartType, CartType),
	THUB_POSTED_TO_ACCOUNTING = COALESCE(@THUB_POSTED_TO_ACCOUNTING, THUB_POSTED_TO_ACCOUNTING),
	THUB_POSTED_DATE = COALESCE(@THUB_POSTED_DATE, THUB_POSTED_DATE),
	THUB_ACCOUNTING_REF = COALESCE(@THUB_ACCOUNTING_REF, THUB_ACCOUNTING_REF),
	Last4 = COALESCE(@Last4, Last4),
	ReadyToShip = COALESCE(@ReadyToShip, ReadyToShip),
	IsPrinted = COALESCE(@IsPrinted, IsPrinted),
	AuthorizedOn = COALESCE(@AuthorizedOn, AuthorizedOn),
	CapturedOn = COALESCE(@CapturedOn, CapturedOn),
	RefundedOn = COALESCE(@RefundedOn, RefundedOn),
	VoidedOn = COALESCE(@VoidedOn, VoidedOn),
	InventoryWasReduced = COALESCE(@InventoryWasReduced, InventoryWasReduced),
	MaxMindFraudScore = COALESCE(@MaxMindFraudScore, MaxMindFraudScore),
	MaxMindDetails = COALESCE(@MaxMindDetails, MaxMindDetails),
	CardStartDate = COALESCE(@CardStartDate, CardStartDate),
	CardIssueNumber = COALESCE(@CardIssueNumber, CardIssueNumber),
	TransactionType = COALESCE(@TransactionType, TransactionType),
	Crypt = COALESCE(@Crypt, Crypt),
	VATRegistrationID = COALESCE(@VATRegistrationID, VATRegistrationID),
	FraudedOn = COALESCE(@FraudedOn, FraudedOn),
	RefundReason = COALESCE(@RefundReason, RefundReason)
where OrderNumber = @OrderNumber
go

--New Sproc for stat updates
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_UpdateStatistics]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_UpdateStatistics]
GO
CREATE PROCEDURE [dbo].[aspdnsf_UpdateStatistics]
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @cmd NVARCHAR(MAX), @tableName VARCHAR(128)
	CREATE TABLE #TableNames(
		Name VARCHAR(128))
		
	INSERT INTO #TableNames
	SELECT o.name 
	FROM sys.sysobjects o
	WHERE xtype = 'U'
	ORDER BY o.name

	DECLARE statCursor CURSOR
		FOR SELECT Name FROM #TableNames
	OPEN statCursor
	FETCH NEXT FROM statCursor
		INTO @tableName
	WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @cmd = 'UPDATE STATISTICS ' + @tableName
			EXEC (@cmd)
				
		FETCH NEXT FROM statCursor
			INTO @tableName
		END
	CLOSE statCursor
	DEALLOCATE statCursor
END
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_MonthlyMaintenance]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_MonthlyMaintenance]
GO
create proc [dbo].[aspdnsf_MonthlyMaintenance]
	@purgeAnonCustomers tinyint = 1,
	@cleanShoppingCartsOlderThan smallint = 30, -- set to 0 to disable erasing
	@cleanWishListsOlderThan smallint = 30, -- set to 0 to disable erasing
	@eraseCCFromAddresses tinyint = 1, -- except those used for recurring billing items!
	@clearProductViewsOrderThan smallint = 180,
	@eraseCCFromOrdersOlderThan smallint = 30, -- set to 0 to disable erasing
	@defragIndexes tinyint = 0,
	@updateStats tinyint = 0,
	@purgeDeletedRecords tinyint = 0,-- Purges records in all tables with a deleted flag set to 1
	@removeRTShippingDataOlderThan smallint = 30, -- set to 0 to disable erasing
	@clearSearchLogOlderThan smallint = 30,-- set to 0 to disable erasing
	@cleanOrphanedLocalizedNames tinyint = 0,
	@cleanupSecurityLog tinyint = 0
	as
begin
	set nocount on

	-- Clear out failed transactions older than 2 months:
	delete from FailedTransaction where OrderDate < dateadd(mm,-2,getdate());

	-- Clear out old tx info, not longer needed:
	update orders set TransactionCommand=NULL, AuthorizationResult=NULL, VoidTXCommand=NULL, VoidTXResult=NULL, CaptureTXCommand=NULL, CaptureTXResult=NULL, RefundTXCommand=NULL, RefundTXResult=NULL where orderdate < dateadd(mm,-2,getdate());

	-- Clean up Security Log entries that are more than 1 year old
	if @cleanupSecurityLog = 1
	begin
		delete from SecurityLog where ActionDate < dateadd(year,-1,getdate());
	end

	-- Clean up data in the LocalizedObjectName table for locales that no longer exist
	if @cleanOrphanedLocalizedNames = 1
	begin
		delete from LocalizedObjectName where LocaleId not in (select LocaleSettingID from LocaleSetting);
	end

	-- clean up all carts (don't touch recurring items or wishlist items however):
	if @cleanShoppingCartsOlderThan <> 0
	begin
		delete dbo.kitcart where (CartType=0 or CartType=101) and CreatedOn < dateadd(d,-@cleanShoppingCartsOlderThan,getdate());
		delete dbo.ShoppingCart where (CartType=0 or CartType=101) and CreatedOn < dateadd(d,-@cleanShoppingCartsOlderThan,getdate());
	end

	if @cleanWishListsOlderThan <> 0
	begin
		delete dbo.kitcart where CartType=1 and CreatedOn < dateadd(d,-@cleanWishListsOlderThan,getdate());
		delete dbo.ShoppingCart where CartType=1 and CreatedOn < dateadd(d,-@cleanWishListsOlderThan,getdate());
	end

	-- purge anon customers:
	if @purgeAnonCustomers = 1
	begin
		-- clean out CIM profiles for orders that were not completed
		delete dbo.CIM_AddressPaymentProfileMap where customerid not in (select customerid from dbo.customer with (NOLOCK) where IsRegistered=1)

		delete dbo.customer where 
			IsRegistered=0 and IsAdmin = 0
			and customerid not in (select customerid from dbo.ShoppingCart with (NOLOCK))
			and customerid not in (select customerid from dbo.kitcart with (NOLOCK))
			and customerid not in (select customerid from dbo.orders with (NOLOCK))
			and customerid not in (select customerid from dbo.rating with (NOLOCK))
			and customerid not in (select ratingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK))
			and customerid not in (select votingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK))
	end

	-- clean addresses, except for those that have recurring orders
	if @eraseCCFromAddresses = 1
		update [dbo].address set 
			CardNumber=NULL,
			CardStartDate=NULL,
			CardIssueNumber=NULL,
			CardExpirationMonth=NULL,
			CardExpirationYear=NULL
		where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)

	-- erase credit cards from all orders older than N days:
	if @eraseCCFromOrdersOlderThan <> 0
		update [dbo].orders set CardNumber=NULL
		where
			OrderDate < dateadd(d,-@eraseCCFromOrdersOlderThan,getdate())

	-- erase product views both for dynamic
	if @clearProductViewsOrderThan <> 0
	begin
		delete dbo.ProductView where ViewDate < dateadd(d,-@clearProductViewsOrderThan,getdate())
	end

	-- Nuke deleted stores
	declare @storeId int
	select top 1 @storeId = StoreID from Store where Deleted = 1
	while @@rowcount > 0 begin
		exec aspdnsf_NukeStore @storeId, 0
		select top 1 @storeId = StoreID from Store where Deleted = 1
	end

	if @purgeDeletedRecords = 1 begin
		delete dbo.Address where deleted = 1
		delete dbo.Coupon where deleted = 1
		delete dbo.Customer where deleted = 1
		delete dbo.Document where deleted = 1
		delete dbo.News where deleted = 1
		delete dbo.Product where deleted = 1
		delete dbo.ProductVariant where deleted = 1 or not exists (select * from dbo.Product where productid = ProductVariant.productid)
		delete dbo.SalesPrompt where deleted = 1
		delete dbo.ShippingZone where deleted = 1
		delete dbo.Topic where deleted = 1
		delete dbo.Affiliate where deleted = 1
		delete dbo.Category where deleted = 1
		delete dbo.CustomerLevel where deleted = 1
		delete dbo.Distributor where deleted = 1
		delete dbo.Genre where deleted = 1
		delete dbo.Library where deleted = 1
		delete dbo.Manufacturer where deleted = 1
		delete dbo.Section where deleted = 1
		delete dbo.Vector where deleted = 1
		delete dbo.ProductVector where not exists (select * from dbo.product where productid = ProductVector.productid) or not exists (select * from dbo.vector where vectorid = ProductVector.vectorid)
		delete dbo.ProductAffiliate where not exists (select * from dbo.product where productid = ProductAffiliate.productid) or not exists (select * from dbo.Affiliate where Affiliateid = ProductAffiliate.Affiliateid)
		delete dbo.ProductCategory where not exists (select * from dbo.product where productid = ProductCategory.productid) or not exists (select * from dbo.Category where Categoryid = ProductCategory.Categoryid)
		delete dbo.ProductCustomerLevel where not exists (select * from dbo.product where productid = ProductCustomerLevel.productid) or not exists (select * from dbo.CustomerLevel where CustomerLevelid = ProductCustomerLevel.CustomerLevelid)
		delete dbo.ProductDistributor where not exists (select * from dbo.product where productid = ProductDistributor.productid) or not exists (select * from dbo.Distributor where Distributorid = ProductDistributor.Distributorid)
		delete dbo.ProductGenre where not exists (select * from dbo.product where productid = ProductGenre.productid) or not exists (select * from dbo.Genre where Genreid = ProductGenre.Genreid)
		delete dbo.ProductLocaleSetting where not exists (select * from dbo.product where productid = ProductLocaleSetting.productid) or not exists (select * from dbo.LocaleSetting where LocaleSettingid = ProductLocaleSetting.LocaleSettingid)
		delete dbo.ProductManufacturer where not exists (select * from dbo.product where productid = ProductManufacturer.productid) or not exists (select * from dbo.Manufacturer where Manufacturerid = ProductManufacturer.Manufacturerid)
		delete dbo.ProductSection where not exists (select * from dbo.product where productid = ProductSection.productid) or not exists (select * from dbo.Section where Sectionid = ProductSection.Sectionid)
	end

	-- Clear out all customer sessions
	truncate table CustomerSession

	-- Clean up abandon records tied to customers that no longer exist
	delete from dbo.ShoppingCart where CustomerID not in (select distinct CustomerID from Customer);
	delete from dbo.KitCart where ShoppingCartRecID not in (select distinct ShoppingCartRecID from ShoppingCart);
	delete from dbo.CustomerSession where CustomerID not in (select distinct CustomerID from Customer);
	delete from dbo.RatingCommentHelpfulness where RatingCustomerID not in (select distinct CustomerID from Customer);
	delete from dbo.RatingCommentHelpfulness where VotingCustomerID not in (select distinct CustomerID from Customer);
	delete from dbo.PromotionUsage where CustomerID not in (select distinct CustomerID from Customer);
	delete from dbo.Address where CustomerID not in (select distinct CustomerID from Customer);
	delete from dbo.Rating where CustomerID not in (select distinct CustomerID from Customer);

	-- Remove old rtshipping requests and responses
	if @removeRTShippingDataOlderThan <> 0
	begin
		update dbo.Customer set RTShipRequest = '', RTShipResponse = ''
		where CreatedOn < dateadd(d,-@removeRTShippingDataOlderThan,getdate())

		update dbo.Orders set RTShipRequest = '', RTShipResponse = ''
		where OrderDate < dateadd(d,-@removeRTShippingDataOlderThan,getdate())
	end

	-- Search log
	if @clearSearchLogOlderThan <> 0
	begin
		delete from dbo.SearchLog where CreatedOn < dateadd(d,-@clearSearchLogOlderThan,getdate())
	end

	-- Defrag indexes
	DECLARE @indexesUpdated BIT = 0
	IF @defragIndexes = 1
	BEGIN
		DECLARE @cmd NVARCHAR(MAX), @tableName VARCHAR(128), @indexName VARCHAR(128)
		CREATE TABLE #INDEXESTOUPDATE(
			TableName VARCHAR(128),
			IndexName VARCHAR(128))
		
		INSERT INTO #IndexesToUpdate
		SELECT  o.name AS TableName, i.name AS IndexName
		FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, NULL) s
			JOIN sys.sysobjects o ON s.object_id = o.id
			LEFT JOIN sys.indexes i ON s.index_id = i.index_id AND o.id = i.object_id
		WHERE s.index_id != 0
			AND s.avg_fragmentation_in_percent > 10
			AND s.page_count > 500
		ORDER BY o.name

		DECLARE indexCursor CURSOR
			FOR SELECT TableName, IndexName FROM #IndexesToUpdate
		OPEN indexCursor
		FETCH NEXT FROM indexCursor
			INTO @tableName, @indexName
		WHILE @@FETCH_STATUS = 0
			BEGIN
				SET @cmd = 'ALTER INDEX [' + @indexName + '] ON [' + @tableName  + '] REBUILD'
				EXEC (@cmd)
				
			FETCH NEXT FROM indexCursor
				INTO @tableName, @indexName
			END
		CLOSE indexCursor
		DEALLOCATE indexCursor

		-- ALTER INDEX doesn't update statistics, have to trigger it manually
		EXEC [dbo].[aspdnsf_UpdateStatistics]
		SET @indexesUpdated = 1
	END

	--Update statistics, including ones that updating indexes misses
	IF @updateStats = 1 AND @indexesUpdated = 0 --This may have already been done above
	BEGIN
		EXEC [dbo].[aspdnsf_UpdateStatistics]
	END
end
go
-- End eCheck Clean Up ---

if not exists (select * from AppConfig where Name = 'Checkout.ShowOkToEmailOnCheckout')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Checkout.ShowOkToEmailOnCheckout', 'Set to true to allow customers to select email opt in/out during checkout.', 'true', 'boolean', null, 'CHECKOUT', 1, 0, getdate())
end

-- Update PayPal Image Urls
update AppConfig set ConfigValue = 'https://www.paypalobjects.com/webstatic/en_US/i/buttons/checkout-logo-medium.png' where name = 'PayPal.Express.ButtonImageURL'
update AppConfig set ConfigValue = 'https://www.paypalobjects.com/webstatic/en_US/i/buttons/ppcredit-logo-medium.png' where name = 'PayPal.Express.BillMeLaterButtonURL'

-- Address form changes
if not exists(select * from AppConfig where Name = 'Address.CollectCompany')
begin
	insert AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	values('Address.CollectCompany','DISPLAY','If true, address forms will include a field to collect the customer''s company name.','true', 'boolean', 0);
end
go

if not exists(select * from AppConfig where Name = 'Address.CollectNickName')
begin
	insert AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	values('Address.CollectNickName','DISPLAY','If true, address forms will include a field to let customers give the address a nickname for display on address lists.','false', 'boolean', 0);
end
go

-- Maximum Number Of Cart Items To Display
if not exists (select * from AppConfig where Name = 'MaximumNumberOfCartItemsToDisplay')
	insert into AppConfig(StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden)
	values (0, 'MaximumNumberOfCartItemsToDisplay', 
	'The maximumn number of items to show in the cart. If the number of items in the cart exceeds this number the customer will be presented with a link to show all the cart items. To disable this feature, set this value to zero (the default). This is useful for sites that tend to have larger shopping carts and only want to show the first few items.', 
	'0', 'integer', '', 'CHECKOUT', 0, 0)
go

-- Add imagefilename to the shipping methods table
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('ShippingMethod') AND name = 'ImageFileName')
    ALTER TABLE dbo.ShippingMethod ADD ImageFileName nvarchar(400) NULL
GO

-- Show Shipping Icons
if not exists (select * from AppConfig where Name = 'ShowShippingIcons')
	insert into AppConfig(StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden)
	values (0, 'ShowShippingIcons', 'If true Icons will show on the checkout page', 'true', 'boolean', '', 'CHECKOUT', 0, 0)
go

update AppConfig set Name = 'RTShipping.DumpDebugXmlOnCheckout', 
	Description = 'If true, than the real-time shipping request and real-time shipping response XML will be shown on bottom of checkout page. This is for debugging why rates are or are not showing. It will only appear in the checkout process fro admins. You can send non-admins to checkout/debugrealtimeshipping to see the output as well.'
	where name = 'RTShipping.DumpXmlOnCartPage'
go
/***************************** Braintree *****************************/
if not exists (select * from AppConfig where Name = 'Braintree.MerchantId')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'Braintree.MerchantId'
		,'The Merchant ID given to you by Braintree.  This is different than your ''Merchant Account ID''.'
		,'GATEWAY'
		,''
		,'string'
		, null
	);
	
if not exists (select * from AppConfig where Name = 'Braintree.PublicKey')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'Braintree.PublicKey'
		,'The Public Key given to you by Braintree.'
		,'GATEWAY'
		,''
		,'string'
		, null
	);
	
if not exists (select * from AppConfig where Name = 'Braintree.PrivateKey')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'Braintree.PrivateKey'
		,'The Private Key given to you by Braintree.'
		,'GATEWAY'
		,''
		,'string'
		, null
	);
	
if not exists (select * from AppConfig where Name = 'Braintree.ScriptUrl')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'Braintree.ScriptUrl'
		,'URL to the Braintree API. Do not modify.'
		,'GATEWAY'
		,'https://js.braintreegateway.com/v2/braintree.js'
		,'string'
		, null
	);

if not exists (select * from AppConfig where Name = 'Braintree.3dSecureEnabled')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'Braintree.3dSecureEnabled'
		,'If true, customers checking out with credit cards through Braintree will be shown the 3dSecure form if their card is enrolled.  3dSecure must be enabled on the Braintree account, which requires help from their Support department.'
		,'GATEWAY'
		,'false'
		,'boolean'
		, null
	);
/***************************** Braintree *****************************/

/* ======== New topics ======== */
IF NOT EXISTS (SELECT * FROM Topic WHERE Name = 'RequestCatalogSuccessful')
	INSERT [dbo].Topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('RequestCatalogSuccessful',1,0,'Catalog Request Submitted','Your request has been received! Thank you for your interest in our products.');
IF NOT EXISTS (SELECT * FROM Topic WHERE Name = 'ContactUsSuccessful')
	INSERT [dbo].Topic(Name,HTMLOk,ShowInSiteMap,Title,Description) values('ContactUsSuccessful',1,0,'Contact Form Submitted','Your email has sent and we should receive it shortly.');

/* ======== Zip Code Lookup ======== */
if not exists (select * from AppConfig where Name = 'Address.UsePostalCodeLookupService')
	insert into AppConfig(StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden)
	values (
		0, 
		'Address.UsePostalCodeLookupService',
		'Set to true to enable city/state lookup service based on postal code.',
		'true',
		'boolean',
		null,
		'CHECKOUT',
		0,
		0)
go

if not exists (select * from AppConfig where Name = 'Address.PostalCodeLookupService.UspsServiceUrl')
	insert into AppConfig(StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden)
	values (
		0, 
		'Address.PostalCodeLookupService.UspsServiceUrl',
		'The service URL for USPS Web Tools.',
		'http://production.shippingapis.com/ShippingAPI.dll',
		'string',
		'',
		'CHECKOUT',
		0,
		0)
go

if not exists (select * from AppConfig where Name = 'Address.PostalCodeLookupService.UspsUserId')
	insert into AppConfig(StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden)
	values (
		0, 
		'Address.PostalCodeLookupService.UspsUserId',
		'Your user ID for USPS Web Tools. This is provided by USPS when you register at https://www.usps.com/business/web-tools-apis/welcome.htm',
		'',
		'string',
		'',
		'CHECKOUT',
		0,
		0)
go

-- Amazon Payments
if not exists (select * from AppConfig where Name = 'AmazonPayments.MerchantId')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'AmazonPayments.MerchantId'
		,'Your Pay with Amazon merchant id.'
		,'GATEWAY'
		,''
		,'string'
		,null
	);

if not exists (select * from AppConfig where Name = 'AmazonPayments.ClientId')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'AmazonPayments.ClientId'
		,'Your Pay with Amazon client id.'
		,'GATEWAY'
		,''
		,'string'
		,null
	);

if not exists (select * from AppConfig where Name = 'AmazonPayments.AccessKey')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'AmazonPayments.AccessKey'
		,'Your Pay with Amazon access key.'
		,'GATEWAY'
		,''
		,'string'
		,null
	);

if not exists (select * from AppConfig where Name = 'AmazonPayments.SecretAccessKey')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'AmazonPayments.SecretAccessKey'
		,'Your Pay with Amazon secret access key.'
		,'GATEWAY'
		,''
		,'string'
		,null
	);

if not exists (select * from AppConfig where Name = 'AmazonPayments.SellerNote')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'AmazonPayments.SellerNote'
		,'The description of the order that is displayed in emails to the buyer.'
		,'GATEWAY'
		,''
		,'string'
		,null
	);

if not exists (select * from AppConfig where Name = 'AmazonPayments.LiveServiceUrl')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'AmazonPayments.LiveServiceUrl'
		,'The endpoint used in live mode to make api calls.'
		,'GATEWAY'
		,'https://mws.amazonservices.com/OffAmazonPayments/2013-01-01/'
		,'string'
		,null
	);

if not exists (select * from AppConfig where Name = 'AmazonPayments.SandboxServiceUrl')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'AmazonPayments.SandboxServiceUrl'
		,'The endpoint used in sandbox mode to make api calls.'
		,'GATEWAY'
		,'https://mws.amazonservices.com/OffAmazonPayments_Sandbox/2013-01-01/'
		,'string'
		,null
	);

if not exists (select * from AppConfig where Name = 'AmazonPayments.SandboxScriptUrl')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'AmazonPayments.SandboxScriptUrl'
		,'The endpoint used in sandbox mode to render scripts.'
		,'GATEWAY'
		,'https://static-na.payments-amazon.com/OffAmazonPayments/us/sandbox/js/Widgets.js'
		,'string'
		,null
	);

if not exists (select * from AppConfig where Name = 'AmazonPayments.LiveScriptUrl')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'AmazonPayments.LiveScriptUrl'
		,'The endpoint used in sandbox mode to render scripts.'
		,'GATEWAY'
		,'https://static-na.payments-amazon.com/OffAmazonPayments/us/js/Widgets.js'
		,'string'
		,null
	);

if not exists (select * from AppConfig where Name = 'AmazonPayments.LiveProfileServiceUrl')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'AmazonPayments.LiveProfileServiceUrl'
		,'The endpoint used in live mode to make profile api calls.'
		,'GATEWAY'
		,'https://api.amazon.com/user/profile'
		,'string'
		,null
	);

if not exists (select * from AppConfig where Name = 'AmazonPayments.SandboxProfileServiceUrl')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'AmazonPayments.SandboxProfileServiceUrl'
		,'The endpoint used in sandbox mode to make profile api calls.'
		,'GATEWAY'
		,'https://api.sandbox.amazon.com/user/profile'
		,'string'
		,null
	);

UPDATE dbo.[AppConfig] SET AllowableValues = 'Credit Card,PayPalExpress,Request Quote,Purchase Order,Check By Mail,C.O.D.,MicroPay,PayPal Payments Advanced,AmazonPayments' WHERE [Name] = 'PaymentMethods'

update appconfig set name = 'ShowShippingEstimate', Description = 'If true, the Shipping Estimator will show in checkout' where name = 'ShowShippingAndTaxEstimate'

if not exists (select * from AppConfig where Name = 'Address.Country.Default')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'Address.Country.Default'
		,'The two letter ISO country code to use as the default value for the country dropdown when customers create a new address.'
		,'DISPLAY'
		,'US'
		,'string'
		,null
	);

if not exists (select * from AppConfig where Name = 'BotUserAgentRegEx')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'BotUserAgentRegEx'
		,'Regular Expression for identifying search engine bots by user agent. This value can be as minimal or broad as you choose - it is used to identify search engine bots so they are not logged as "real" product views.'
		,'GENERAL'
		,'bot|spider'
		,'string'
		,null
	);
	
if not exists (select * from syscolumns where id = object_id('Address') and name = 'OffsiteSource')
	alter table dbo.Address add OffsiteSource nvarchar(400) NULL
go

if not exists (select * from AppConfig where Name = 'Minicart.Enabled')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'Minicart.Enabled'
		,'Enable the minicart on your site.'
		,'DISPLAY'
		,'true'
		,'boolean'
		,null
	);

if not exists (select * from AppConfig where Name = 'Minicart.ShowImages')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'Minicart.ShowImages'
		,'If true, product picture icons are shown within the minicart.'
		,'DISPLAY'
		,'true'
		,'boolean'
		,null
	);

if not exists (select * from AppConfig where Name = 'Minicart.QuantityUpdate.Enabled')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'Minicart.QuantityUpdate.Enabled'
		,'If true, users can update the cart item quantities in the minicart.'
		,'DISPLAY'
		,'false'
		,'boolean'
		,null
	);

if not exists (select * from AppConfig where Name = 'Minicart.ShowSku')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'Minicart.ShowSku'
		,'If true, the product sku will show in the minicart.'
		,'DISPLAY'
		,'false'
		,'boolean'
		,null
	);

delete from AppConfig where name = 'ShowPicsInMiniCart'
delete from appconfig where name = 'Minicart.MaxLatestCartItemsCount'
delete from appconfig where name = 'Minicart.UseAjaxAddToCart'
delete from appconfig where name = 'MiniCartMaxIconHeight'
delete from appconfig where name = 'MiniCartMaxIconWidth'
delete from appconfig where name = 'ShowMiniCart'
delete from appconfig where name = 'Captcha.HorizontalColor'
delete from appconfig where name = 'Captcha.ImageBackColor'
delete from appconfig where name = 'Captcha.ImageForeColor'
delete from appconfig where name = 'Captcha.TextBackColor'
delete from appconfig where name = 'Captcha.TextForeColor'
delete from appconfig where name = 'Captcha.VerticalColor'
delete from appconfig where name = 'paypal.express.showoncartpage'
delete from appconfig where name = 'relatedproducts.numberdisplayed'

/* ======== Remove PayPal Standard ======== */
delete from AppConfig where Name = 'PayPal.BusinessID'
delete from AppConfig where Name = 'PayPal.UseInstantNotification'
UPDATE AppConfig SET AllowableValues = REPLACE(AllowableValues, 'PayPal,', '') WHERE Name = 'PaymentMethods'

-- Removing Checkout By Amazon
delete from AppConfig where Name = 'CheckoutByAmazon.CbaAccessKey'
delete from AppConfig where Name = 'CheckoutByAmazon.CbaSecretKey'
delete from AppConfig where Name = 'CheckoutByAmazon.MwsAccessKey'
delete from AppConfig where Name = 'CheckoutByAmazon.MwsSecretKey'
delete from AppConfig where Name = 'CheckoutByAmazon.MerchantId'
delete from AppConfig where Name = 'CheckoutByAmazon.Marketplace'
delete from AppConfig where Name = 'CheckoutByAmazon.WidgetUrl'
delete from AppConfig where Name = 'CheckoutByAmazon.WidgetSandboxUrl'
delete from AppConfig where Name = 'CheckoutByAmazon.CBAServiceUrl'
delete from AppConfig where Name = 'CheckoutByAmazon.CBAServiceSandboxUrl'
delete from AppConfig where Name = 'CheckoutByAmazon.MerchantServiceUrl'
delete from AppConfig where Name = 'CheckoutByAmazon.MerchantServiceSandboxUrl'
delete from AppConfig where Name = 'CheckoutByAmazon.UseSandbox'
delete from AppConfig where Name = 'CheckoutByAmazon.OrderFulfillmentType'

-- Clean payment methods of Checkout by Amazon
declare	@xml as xml
declare	@list varchar(max) = ''

-- Config Value
select @xml = cast(('<x>' + replace(ConfigValue,',','</x><x>') + '</x>') as xml) from AppConfig where Name = 'PaymentMethods'
select @list = @list + ltrim(rtrim(x.y.value('.','varchar(max)'))) + ',' from @xml.nodes('x') x(y) where ltrim(rtrim(y.value('.','varchar(max)'))) <> 'CheckoutByAmazon'
select @list = left(@list, len(@list)-1)
update	AppConfig set ConfigValue = @list where Name = 'PaymentMethods'

GO
--reset variables
declare	@xml as xml
declare	@list varchar(max) = ''

-- Allowable Values
select @xml = cast(('<x>' + replace(AllowableValues,',','</x><x>') + '</x>') as xml) from AppConfig where Name = 'PaymentMethods'
select @list = @list + ltrim(rtrim(x.y.value('.','varchar(max)'))) + ',' from @xml.nodes('x') x(y) where ltrim(rtrim(y.value('.','varchar(max)'))) <> 'CheckoutByAmazon'
select @list = left(@list, len(@list)-1)
update	AppConfig set AllowableValues = @list where Name = 'PaymentMethods'

-- Clean up hidden amazon addresses
delete from [Address] where Address1 = 'Hidden By Amazon'

-- End Removing Checkout By Amazon

/************************** Begin Guest Checkout **************************/
if not exists (select * from AppConfig where Name = 'GuestCheckout')
begin
	declare @defaultValue varchar(max) = 'AllowUnregisteredCustomers'

	if exists (select * from AppConfig where Name = 'Checkout.Type' and ConfigValue = 'SmartOPC')
		and exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.ShowCreateAccount' and ConfigValue = 'False')
	begin
		set @defaultValue = 'PasswordNeverRequestedAtCheckout'
	end
	else if exists (select * from AppConfig where Name = 'PasswordIsOptionalDuringCheckout' and ConfigValue = 'false') 
		or exists (select * from GlobalConfig where Name = 'AllowCustomerDuplicateEMailAddresses' and ConfigValue = 'false')
	begin
		set @defaultValue = 'Disabled'
	end	
	else if exists (select * from GlobalConfig where Name = 'Anonymous.AllowAlreadyRegisteredEmail' and ConfigValue = 'false')
	begin
		set @defaultValue = 'AllowRegisteredCustomers'
	end

	insert into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues) Values ('GuestCheckout', 'Use this setting to control guest checkout on your store. Disabled: Completely disables guest checkout and forces all shopper''s to login or create an account upon checkout. AllowUnregisteredCustomers (default): Enables guest checkout and makes collecting a shopper''s password during checkout optional. Once an email address is registered, that email can never be used for guest checkout again. AllowRegisteredCustomers: Enables guest checkout and gives the shopper the option to provide a password and login. A registered email will not stop the email address from being used in a future guest checkout, however, the email address cannot be registered again. PasswordNeverRequestedAtCheckout: Shoppers are not prompted to enter credentials or to create an account on the checkout page.','CHECKOUT',@defaultValue,'enum','Disabled,AllowUnregisteredCustomers,AllowRegisteredCustomers,PasswordNeverRequestedAtCheckout');
end

if exists(select * from GlobalConfig where Name = 'AllowCustomerFiltering' and ConfigValue = 'true')
begin
	with Accounts as (
		select max(CustomerId) as CustomerId, Email, StoreID
		from Customer
		where Email <> '' and Deleted = 0 and IsRegistered = 1
		group by Email, StoreId
		having count(*) > 1)
	update c
		set IsRegistered = 0, Password = '', SaltKey = -1
		from Customer c inner join Accounts a on c.Email = a.Email and c.StoreID = a.StoreID and c.CustomerId <> a.CustomerId
		where c.IsRegistered = 1
end
else
begin
	with Accounts as (
		select max(CustomerId) as CustomerId, Email
		from Customer
		where Email <> '' and Deleted = 0 and IsRegistered = 1
		group by Email
		having count(*) > 1)
	update c
		set IsRegistered = 0, Password = '', SaltKey = -1
		from Customer c inner join Accounts a on c.Email = a.Email and c.CustomerId <> a.CustomerId
		where c.IsRegistered = 1
	end
go

/************************** Mobile Schema Cleanup **************************/

DELETE FROM AppConfig WHERE Name LIKE 'Mobile.%'

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[aspdnsf_GetMobileEntities]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[aspdnsf_GetMobileEntities]
GO
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[aspdnsf_RecurseMobileEntities]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[aspdnsf_RecurseMobileEntities]
GO
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[aspdnsf_GetProductsForMobileEntity]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[aspdnsf_GetProductsForMobileEntity]
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id('[dbo].[MobileDevice]') and type = 'u')
	DROP TABLE [dbo].[MobileDevice]
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id('[dbo].[MobileLocaleMapping]') and type = 'u')
	DROP TABLE [dbo].[MobileLocaleMapping]

/************************** Log all previous passwords **************************/
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_updCustomer]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_updCustomer]
GO
CREATE proc [dbo].[aspdnsf_updCustomer]
	@CustomerID int,
	@CustomerLevelID int = null,
	@Email nvarchar(100) = null,
	@Password nvarchar(250) = null,
	@SaltKey int = null,
	@Gender nvarchar(1) = null,
	@FirstName nvarchar(100) = null,
	@LastName nvarchar(100) = null,
	@Notes nvarchar(max) = null,
	@SkinID int = null,
	@Phone nvarchar(25) = null,
	@AffiliateID int = null,
	@Referrer nvarchar(max) = null,
	@CouponCode nvarchar(50) = null,
	@OkToEmail tinyint = null,
	@IsAdmin tinyint = null,
	@BillingEqualsShipping tinyint = null,
	@LastIPAddress varchar(40) = null,
	@OrderNotes nvarchar(max) = null,
	@RTShipRequest nvarchar(max) = null,
	@RTShipResponse nvarchar(max) = null,
	@OrderOptions nvarchar(max) = null,
	@LocaleSetting nvarchar(10) = null,
	@MicroPayBalance money = null,
	@RecurringShippingMethodID int = null,
	@RecurringShippingMethod nvarchar(100) = null,
	@BillingAddressID int = null,
	@ShippingAddressID int = null,
	@ExtensionData nvarchar(max) = null,
	@FinalizationData nvarchar(max) = null,
	@Deleted tinyint = null,
	@Over13Checked tinyint = null,
	@CurrencySetting nvarchar(10) = null,
	@VATSetting int = null,
	@VATRegistrationID nvarchar(100) = null,
	@StoreCCInDB tinyint = null,
	@IsRegistered tinyint = null,
	@LockedUntil datetime = null,
	@AdminCanViewCC tinyint = null,
	@BadLogin smallint = 0, --only pass -1 = null, 0 = null, or 1: -1 clears the field = null, 0 does nothing = null, 1 increments the field by one
	@Active tinyint = null,
	@PwdChangeRequired tinyint = null,
	@RegisterDate datetime = null,
	@RequestedPaymentMethod  nvarchar(100) = null,
	@ClearSavedCCNumbers bit = 0,
	@StoreID	int = null
AS
SET NOCOUNT ON

DECLARE @OldPwd nvarchar(100), @OldSaltKey int
SELECT @OldPwd = Password, @OldSaltKey = SaltKey FROM dbo.Customer WHERE CustomerID = @CustomerID

UPDATE dbo.Customer
SET
	CustomerLevelID = COALESCE(@CustomerLevelID, CustomerLevelID),
	RegisterDate = COALESCE(@RegisterDate, RegisterDate),
	Email = COALESCE(@Email, Email),
	Password = COALESCE(@Password, Password),
	SaltKey = COALESCE(@SaltKey, SaltKey),
	Gender = COALESCE(@Gender, Gender),
	FirstName = COALESCE(@FirstName, FirstName),
	LastName = COALESCE(@LastName, LastName),
	Notes = COALESCE(@Notes, Notes),
	SkinID = COALESCE(@SkinID, SkinID),
	Phone = COALESCE(@Phone, Phone),
	AffiliateID = COALESCE(@AffiliateID, AffiliateID),
	Referrer = COALESCE(@Referrer, Referrer),
	CouponCode = COALESCE(@CouponCode, CouponCode),
	OkToEmail = COALESCE(@OkToEmail, OkToEmail),
	IsAdmin = COALESCE(@IsAdmin, IsAdmin),
	BillingEqualsShipping = COALESCE(@BillingEqualsShipping, BillingEqualsShipping),
	LastIPAddress = COALESCE(@LastIPAddress, LastIPAddress),
	OrderNotes = COALESCE(@OrderNotes, OrderNotes),
	RTShipRequest = COALESCE(@RTShipRequest, RTShipRequest),
	RTShipResponse = COALESCE(@RTShipResponse, RTShipResponse),
	OrderOptions = COALESCE(@OrderOptions, OrderOptions),
	LocaleSetting = COALESCE(@LocaleSetting, LocaleSetting),
	MicroPayBalance = COALESCE(@MicroPayBalance, MicroPayBalance),
	RecurringShippingMethodID = COALESCE(@RecurringShippingMethodID, RecurringShippingMethodID),
	RecurringShippingMethod = COALESCE(@RecurringShippingMethod, RecurringShippingMethod),
	BillingAddressID = COALESCE(@BillingAddressID, BillingAddressID),
	ShippingAddressID = COALESCE(@ShippingAddressID, ShippingAddressID),
	ExtensionData = COALESCE(@ExtensionData, ExtensionData),
	FinalizationData = COALESCE(@FinalizationData, FinalizationData),
	Deleted = COALESCE(@Deleted, Deleted),
	Over13Checked = COALESCE(@Over13Checked, Over13Checked),
	CurrencySetting = COALESCE(@CurrencySetting, CurrencySetting),
	VATSetting = COALESCE(@VATSetting, VATSetting),
	VATRegistrationID = COALESCE(@VATRegistrationID, VATRegistrationID),
	StoreCCInDB = COALESCE(@StoreCCInDB, StoreCCInDB),
	IsRegistered = COALESCE(@IsRegistered, IsRegistered),
	LockedUntil = COALESCE(@LockedUntil, LockedUntil),
	AdminCanViewCC = COALESCE(@AdminCanViewCC, AdminCanViewCC),
	PwdChanged = CASE 
		WHEN @OldPwd <> @Password AND @Password IS NOT NULL THEN getdate() 
		ELSE PwdChanged END,
	BadLoginCount = CASE @BadLogin 
		WHEN -1 THEN 0 
		ELSE BadLoginCount + isnull(@BadLogin, 0) END,
	LastBadLogin = CASE @BadLogin 
		WHEN -1 THEN NULL 
		WHEN 1 THEN getdate() 
		ELSE LastBadLogin END,
	Active = COALESCE(@Active, Active),
	PwdChangeRequired = COALESCE(@PwdChangeRequired, PwdChangeRequired),
	RequestedPaymentMethod = COALESCE(@RequestedPaymentMethod, RequestedPaymentMethod),
	StoreID = COALESCE(@StoreID, StoreID)
WHERE
	CustomerID = @CustomerID
	-- Only update the row if any fields will actually change
	AND (
		@CustomerLevelID is not null and (CustomerLevelID is null or CustomerLevelID != @CustomerLevelID)
		OR @RegisterDate is not null and (RegisterDate is null or RegisterDate != @RegisterDate)
		OR @Email is not null and (Email is null or Email != @Email)
		OR @Password is not null and ([Password] is null or [Password] != @Password)
		OR @SaltKey is not null and (SaltKey is null or SaltKey != @SaltKey)
		OR @Gender is not null and (Gender is null or Gender != @Gender)
		OR @FirstName is not null and (FirstName is null or FirstName != @FirstName)
		OR @LastName is not null and (LastName is null or LastName != @LastName)
		OR @Notes is not null and (Notes is null or Notes != @Notes)
		OR @SkinID is not null and (SkinID is null or SkinID != @SkinID)
		OR @Phone is not null and (Phone is null or Phone != @Phone)
		OR @AffiliateID is not null and (AffiliateID is null or AffiliateID != @AffiliateID)
		OR @Referrer is not null and (Referrer is null or Referrer != @Referrer)
		OR @CouponCode is not null and (CouponCode is null or CouponCode != @CouponCode)
		OR @OkToEmail is not null and (OkToEmail is null or OkToEmail != @OkToEmail)
		OR @IsAdmin is not null and (IsAdmin is null or IsAdmin != @IsAdmin)
		OR @BillingEqualsShipping is not null and (BillingEqualsShipping is null or BillingEqualsShipping != @BillingEqualsShipping)
		OR @LastIPAddress is not null and (LastIPAddress is null or LastIPAddress != @LastIPAddress)
		OR @OrderNotes is not null and (OrderNotes is null or OrderNotes != @OrderNotes)
		OR @RTShipRequest is not null and (RTShipRequest is null or RTShipRequest != @RTShipRequest)
		OR @RTShipResponse is not null and (RTShipResponse is null or RTShipResponse != @RTShipResponse)
		OR @OrderOptions is not null and (OrderOptions is null or OrderOptions != @OrderOptions)
		OR @LocaleSetting is not null and (LocaleSetting is null or LocaleSetting != @LocaleSetting)
		OR @MicroPayBalance is not null and (MicroPayBalance is null or MicroPayBalance != @MicroPayBalance)
		OR @RecurringShippingMethodID is not null and (RecurringShippingMethodID is null or RecurringShippingMethodID != @RecurringShippingMethodID)
		OR @RecurringShippingMethod is not null and (RecurringShippingMethod is null or RecurringShippingMethod != @RecurringShippingMethod)
		OR @BillingAddressID is not null and (BillingAddressID is null or BillingAddressID != @BillingAddressID)
		OR @ShippingAddressID is not null and (ShippingAddressID is null or ShippingAddressID != @ShippingAddressID)
		OR @ExtensionData is not null and (ExtensionData is null or ExtensionData != @ExtensionData)
		OR @FinalizationData is not null and (FinalizationData is null or FinalizationData != @FinalizationData)
		OR @Deleted is not null and (Deleted is null or Deleted != @Deleted)
		OR @Over13Checked is not null and (Over13Checked is null or Over13Checked != @Over13Checked)
		OR @CurrencySetting is not null and (CurrencySetting is null or CurrencySetting != @CurrencySetting)
		OR @VATSetting is not null and (VATSetting is null or VATSetting != @VATSetting)
		OR @VATRegistrationID is not null and (VATRegistrationID is null or VATRegistrationID != @VATRegistrationID)
		OR @StoreCCInDB is not null and (StoreCCInDB is null or StoreCCInDB != @StoreCCInDB)
		OR @IsRegistered is not null and (IsRegistered is null or IsRegistered != @IsRegistered)
		OR @LockedUntil is not null and (LockedUntil is null or LockedUntil != @LockedUntil)
		OR @AdminCanViewCC is not null and (AdminCanViewCC is null or AdminCanViewCC != @AdminCanViewCC)
		OR @Password is not null and (@Password != @OldPwd)
		OR @BadLogin != 0
		OR @PwdChangeRequired is not null and (PwdChangeRequired is null or PwdChangeRequired != @PwdChangeRequired)
		OR @RequestedPaymentMethod is not null and (RequestedPaymentMethod is null or RequestedPaymentMethod != @RequestedPaymentMethod)
		OR @StoreID is not null and (StoreID is null or StoreID != @StoreID)
	)
	
IF @OldPwd <> @Password
	INSERT dbo.PasswordLog (CustomerID, OldPwd, SaltKey, ChangeDt)
	VALUES (@CustomerID, @OldPwd, @OldSaltKey, getdate())

IF NULLIF(@ClearSavedCCNumbers, 0) = 1
BEGIN
	UPDATE Address
	SET	CardNumber = NULL,
		CardExpirationMonth = NULL,
		CardExpirationYear = NULL,
		CardStartDate = NULL,
		CardIssueNumber = NULL
	WHERE CustomerID = @CustomerID

	UPDATE Orders
	SET	CardNumber = NULL,
		CardExpirationMonth = NULL,
		CardExpirationYear = NULL,
		CardStartDate = NULL,
		CardIssueNumber = NULL
	WHERE CustomerID = @CustomerID
END
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_updCustomerByEmail]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_updCustomerByEmail]
GO
CREATE proc [dbo].[aspdnsf_updCustomerByEmail]
    @Email nvarchar(100),
    @CustomerLevelID int = null,
    @Password nvarchar(250) = null,
    @SaltKey int = null,
    @Gender nvarchar(1) = null,
    @FirstName nvarchar(100) = null,
    @LastName nvarchar(100) = null,
    @Notes nvarchar(max) = null,
    @SkinID int = null,
    @Phone nvarchar(25) = null,
    @AffiliateID int = null,
    @Referrer nvarchar(max) = null,
    @CouponCode nvarchar(50) = null,
    @OkToEmail tinyint = null,
    @IsAdmin tinyint = null,
    @BillingEqualsShipping tinyint = null,
    @LastIPAddress varchar(40) = null,
    @OrderNotes nvarchar(max) = null,
    @RTShipRequest nvarchar(max) = null,
    @RTShipResponse nvarchar(max) = null,
    @OrderOptions nvarchar(max) = null,
    @LocaleSetting nvarchar(10) = null,
    @MicroPayBalance money = null,
    @RecurringShippingMethodID int = null,
    @RecurringShippingMethod nvarchar(100) = null,
    @BillingAddressID int = null,
    @ShippingAddressID int = null,
    @ExtensionData nvarchar(max) = null,
    @FinalizationData nvarchar(max) = null,
    @Deleted tinyint = null,
    @Over13Checked tinyint = null,
    @CurrencySetting nvarchar(10) = null,
    @VATSetting int = null,
    @VATRegistrationID nvarchar(100) = null,
    @StoreCCInDB tinyint = null,
    @IsRegistered tinyint = null,
    @LockedUntil datetime = null,
    @AdminCanViewCC tinyint = null,
    @BadLogin smallint = 0 , --only pass -1 = null, 0 = null, or 1: -1 clears the field = null, 0 does nothing = null, 1 increments the field by one
    @Active tinyint = null,
    @PwdChangeRequired tinyint = null,
    @RequestedPaymentMethod  nvarchar(100) = null

AS
SET NOCOUNT ON

DECLARE @CustomerID int, @OldPwd nvarchar(100), @IsAdminCust tinyint, @OldSaltKey int

SELECT @CustomerID = CustomerID , @OldPwd = Password, @IsAdminCust = IsAdmin, @OldSaltKey = Saltkey FROM dbo.Customer WHERE Email = @Email


UPDATE dbo.Customer
SET
    CustomerLevelID = COALESCE(@CustomerLevelID, CustomerLevelID),
    Email = COALESCE(@Email, Email),
    Password = COALESCE(@Password, Password),
    SaltKey = COALESCE(@SaltKey, SaltKey),
    Gender = COALESCE(@Gender, Gender),
    FirstName = COALESCE(@FirstName, FirstName),
    LastName = COALESCE(@LastName, LastName),
    Notes = COALESCE(@Notes, Notes),
    SkinID = COALESCE(@SkinID, SkinID),
    Phone = COALESCE(@Phone, Phone),
    AffiliateID = COALESCE(@AffiliateID, AffiliateID),
    Referrer = COALESCE(@Referrer, Referrer),
    CouponCode = COALESCE(@CouponCode, CouponCode),
    OkToEmail = COALESCE(@OkToEmail, OkToEmail),
    IsAdmin = COALESCE(@IsAdmin, IsAdmin),
    BillingEqualsShipping = COALESCE(@BillingEqualsShipping, BillingEqualsShipping),
    LastIPAddress = COALESCE(@LastIPAddress, LastIPAddress),
    OrderNotes = COALESCE(@OrderNotes, OrderNotes),
    RTShipRequest = COALESCE(@RTShipRequest, RTShipRequest),
    RTShipResponse = COALESCE(@RTShipResponse, RTShipResponse),
    OrderOptions = COALESCE(@OrderOptions, OrderOptions),
    LocaleSetting = COALESCE(@LocaleSetting, LocaleSetting),
    MicroPayBalance = COALESCE(@MicroPayBalance, MicroPayBalance),
    RecurringShippingMethodID = COALESCE(@RecurringShippingMethodID, RecurringShippingMethodID),
    RecurringShippingMethod = COALESCE(@RecurringShippingMethod, RecurringShippingMethod),
    BillingAddressID = COALESCE(@BillingAddressID, BillingAddressID),
    ShippingAddressID = COALESCE(@ShippingAddressID, ShippingAddressID),
    ExtensionData = COALESCE(@ExtensionData, ExtensionData),
    FinalizationData = COALESCE(@FinalizationData, FinalizationData),
    Deleted = COALESCE(@Deleted, Deleted),
    Over13Checked = COALESCE(@Over13Checked, Over13Checked),
    CurrencySetting = COALESCE(@CurrencySetting, CurrencySetting),
    VATSetting = COALESCE(@VATSetting, VATSetting),
    VATRegistrationID = COALESCE(@VATRegistrationID, VATRegistrationID),
    StoreCCInDB = COALESCE(@StoreCCInDB, StoreCCInDB),
    IsRegistered = COALESCE(@IsRegistered, IsRegistered),
    LockedUntil = COALESCE(@LockedUntil, LockedUntil),
    AdminCanViewCC = COALESCE(@AdminCanViewCC, AdminCanViewCC),
    PwdChanged = case when @OldPwd <> @Password and @Password is not null then getdate() else PwdChanged end,
    BadLoginCount = case @BadLogin when -1 then 0 else BadLoginCount + @BadLogin end,
    LastBadLogin = case @BadLogin when -1 then null when 1 then getdate() else LastBadLogin end,
    Active = COALESCE(@Active, Active),
    PwdChangeRequired = COALESCE(@PwdChangeRequired, PwdChangeRequired),
    RequestedPaymentMethod = COALESCE(@RequestedPaymentMethod, RequestedPaymentMethod)
WHERE Email = @Email

IF @OldPwd <> @Password
    INSERT dbo.PasswordLog (CustomerID, OldPwd, SaltKey, ChangeDt)
    VALUES (@CustomerID, @OldPwd, @OldSaltKey, getdate())


GO

/*************************************************************************************/

if exists(select * from AppConfig where Name = 'SecurityCodeRequiredOnAdminLogin')
	delete AppConfig WHERE Name = 'SecurityCodeRequiredOnAdminLogin'

if exists(select * from AppConfig where Name = 'SecurityCodeRequiredDuringCheckout')
	delete AppConfig WHERE Name = 'SecurityCodeRequiredDuringCheckout'

if not exists(select * from AppConfig where Name = 'PreserveActiveAddressOnSignin')
	insert AppConfig (Name, SuperOnly, GroupName, ValueType, ConfigValue, Description)
	values('PreserveActiveAddressOnSignin', 1, 'SETUP', 'boolean', 'true', 'If true, addresses created by customers while anonymous will be moved to their new session when logging in or registering.');

if not exists(select * from AppConfig where Name = 'NumberOfNewsArticlesToShow')
	insert AppConfig (Name, SuperOnly, GroupName, ValueType, ConfigValue, Description)
	values('NumberOfNewsArticlesToShow', 1, 'GENERAL', 'int', '100', 'Maximum number of news articles to show where you have news displaying on your site.');

/************************** Object Caching AppConfigs **************************/
if not exists(select * from AppConfig where Name = 'CacheShoppingCarts')
	insert into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	( 'CacheShoppingCarts', 'Set to false to disable caching customers'' shopping carts during checkout. This should only be used for troubleshooting, as it will slow down the site. The default is true.', 'CACHING', 'True', 'boolean', '')

if not exists(select * from AppConfig where Name = 'CacheShippingMethods')
	insert into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	( 'CacheShippingMethods', 'Set to false to disable caching customers'' shipping methods during checkout. This should only be used for troubleshooting, as it will slow down the site. The default is true.', 'CACHING', 'True', 'boolean', '')

/* ======== BuySAFE AppConfig Name Fix ======== */

UPDATE AppConfig SET Name = 'BuySafe.DisableAddToCartKicker' WHERE Name = 'BuySafe.DisableAddoToCartKicker'

/************************** Fix identity issues on inserts/updates **************************/
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insAffiliate]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insAffiliate]
GO
CREATE PROC dbo.aspdnsf_insAffiliate
    @EMail nvarchar(100),
    @Password nvarchar(250),
    @Notes nvarchar(max),
    @IsOnline tinyint,
    @FirstName nvarchar(100),
    @LastName nvarchar(100),
    @Name nvarchar(100),
    @Company nvarchar(100),
    @Address1 nvarchar(100),
    @Address2 nvarchar(100),
    @Suite nvarchar(50),
    @City nvarchar(100),
    @State nvarchar(100),
    @Zip nvarchar(10),
    @Country nvarchar(100),
    @Phone nvarchar(25),
    @WebSiteName nvarchar(100),
    @WebSiteDescription nvarchar(max),
    @URL nvarchar(max),
    @DefaultSkinID int,
    @ParentAffiliateID int,
    @DisplayOrder int,
    @ExtensionData nvarchar(max),
    @SEName nvarchar(100),
    @SETitle nvarchar(max),
    @SEAltText nvarchar(max),
    @SEKeywords nvarchar(max),
    @SEDescription nvarchar(max),
    @Wholesale tinyint,
    @SaltKey int,
    @StoreID int,
    @AffiliateID int OUTPUT

AS
SET NOCOUNT ON


insert dbo.Affiliate(AffiliateGUID, EMail, Password, Notes, IsOnline, FirstName, LastName, [Name], Company, Address1, Address2, Suite, City, State, Zip, Country, Phone, WebSiteName, WebSiteDescription, URL, DefaultSkinID, ParentAffiliateID, DisplayOrder, ExtensionData, SEName, SETitle, SEAltText, SEKeywords, SEDescription, Published, Wholesale, CreatedOn, SaltKey)
values (newid(), @EMail, @Password, @Notes, @IsOnline, @FirstName, @LastName, @Name, @Company, @Address1, @Address2, @Suite, @City, @State, @Zip, @Country, @Phone, @WebSiteName, @WebSiteDescription, @URL, @DefaultSkinID, @ParentAffiliateID, @DisplayOrder, @ExtensionData, @SEName, @SETitle, @SEAltText, @SEKeywords, @SEDescription, 1, @Wholesale, getdate(), @SaltKey)

set @AffiliateID = SCOPE_IDENTITY()

insert into AffiliateStore (AffiliateID, StoreID, CreatedOn) values (@AffiliateID, @StoreID, GETDATE())

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_CreateFeed]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_CreateFeed]
GO
create proc [dbo].[aspdnsf_CreateFeed]
    @StoreID int,
    @Name nvarchar(100),
    @DisplayOrder int,
    @XmlPackage nvarchar(100),
    @CanAutoFTP tinyint,
    @FTPUsername nvarchar(100),
    @FTPPassword nvarchar(100),
    @FTPServer nvarchar(100),
    @FTPPort int,
    @FTPFilename nvarchar(100),
    @ExtensionData nvarchar(max),
    @FeedID int OUTPUT

AS
BEGIN
SET NOCOUNT ON

IF isnull(@XmlPackage, '') = '' BEGIN
    RAISERROR('XmlPAckage is required', 16, 1)
    RETURN
END

IF @CanAutoFTP > 1
    SET @CanAutoFTP = 1



INSERT dbo.Feed(FeedGUID, StoreID, Name, DisplayOrder, XmlPackage, CanAutoFTP, FTPUsername, FTPPassword, FTPServer, FTPPort, FTPFilename, ExtensionData, CreatedOn)
VALUES (newid(), @StoreID, @Name, isnull(@DisplayOrder,1), @XmlPackage, isnull(@CanAutoFTP,0), @FTPUsername, @FTPPassword, @FTPServer, @FTPPort, @FTPFilename, @ExtensionData, getdate())
set @FeedID = SCOPE_IDENTITY()
END

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insCustomer]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insCustomer]
GO
create proc [dbo].[aspdnsf_insCustomer]
    @Email nvarchar(100),
    @Password nvarchar(100),
    @SkinID int,
    @AffiliateID int,
    @Referrer nvarchar(max),
    @IsAdmin tinyint,
    @LastIPAddress varchar(40),
    @LocaleSetting nvarchar(10),
    @Over13Checked tinyint,
    @CurrencySetting nvarchar(10),
    @VATSetting int,
    @VATRegistrationID nvarchar(100),
    @CustomerLevelID int,
    @CustomerID int OUTPUT

AS
SET NOCOUNT ON

insert dbo.Customer(CustomerGUID, CustomerLevelID, RegisterDate, Email, Password, Gender, FirstName, LastName, Notes, SkinID, Phone, AffiliateID, Referrer, CouponCode, OkToEmail, IsAdmin, BillingEqualsShipping, LastIPAddress, OrderNotes, RTShipRequest, RTShipResponse, OrderOptions, LocaleSetting, MicroPayBalance, RecurringShippingMethodID, RecurringShippingMethod, BillingAddressID, ShippingAddressID, ExtensionData, FinalizationData, Deleted, CreatedOn, Over13Checked, CurrencySetting, VATSetting, VATRegistrationID, StoreCCInDB, IsRegistered, LockedUntil, AdminCanViewCC, PwdChanged, BadLoginCount, LastBadLogin, Active, PwdChangeRequired, SaltKey)
values
(
    newid(),
    @CustomerLevelID,
    getdate(),
    isnull(@Email, ''),
    isnull(@Password, ''),
    null,
    null,
    null,
    null,
    isnull(@SkinID, 1),
    null,
    @AffiliateID,
    @Referrer,
    null,
    1,
    isnull(@IsAdmin, 0),
    0,
    @LastIPAddress,
    null,
    null,
    null,
    null,
    isnull(@LocaleSetting, ('en-US')),
    0.0,
    1,
    null,
    null,
    null,
    null,
    null,
    0,
    getdate(),
    @Over13Checked,
    @CurrencySetting,
    @VATSetting,
    @VATRegistrationID,
    1,
    0,
    null,
    1,
    getdate(),
    0,
    null,
    1,
    0,
    0
)

set @CustomerID = SCOPE_IDENTITY()

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_SessionInsert]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_SessionInsert]
GO
create proc [dbo].[aspdnsf_SessionInsert]
    @CustomerID int,
    @SessionValue nvarchar(max),
    @ipaddr varchar(15),
    @CustomerSessionID int OUTPUT

AS
SET NOCOUNT ON

DECLARE @CustomerSessionGUID uniqueidentifier

set @CustomerSessionGUID = newid()
insert dbo.Customersession(CustomerID, SessionName, SessionValue, CreatedOn, ipaddr, LastActivity, CustomerSessionGUID)
values (@CustomerID, '', isnull(@SessionValue, ''), getdate(), @ipaddr, getdate(), @CustomerSessionGUID)

set @CustomerSessionID = SCOPE_IDENTITY()

DELETE dbo.Customersession WHERE CustomerID = @CustomerID and CustomersessionID <> @CustomerSessionID

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_SaveKitGroup]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_SaveKitGroup]
GO
create procedure [dbo].[aspdnsf_SaveKitGroup] (
	@KitGroupID int,
	@Name nvarchar(400),
	@Description nvarchar(max),
	@Summary nvarchar(max),
	@ProductID int,
	@DisplayOrder int,
	@KitGroupTypeID int,
	@IsRequired bit,
	@IsReadOnly bit,
	@SavedID int OUTPUT)

AS
BEGIN
	IF(EXISTS(SELECT * FROM KitGroup WITH (NOLOCK) WHERE KitGroupID = @KitGroupID))
	BEGIN
		UPDATE KitGroup SET
			[Name] = @Name,
			Description = @Description,
			Summary = @Summary,
			ProductID = @ProductID,
			DisplayOrder = @DisplayOrder,
			KitGroupTypeID = @KitGroupTypeID,
			IsRequired = @IsRequired,
			IsReadOnly = @IsReadOnly
		WHERE KitGroupID = @KitGroupID

		SET @SavedID = @KitGroupID

	END
	ELSE
	BEGIN
		INSERT INTO KitGroup(
			KitGroupGUID,
			[Name],
			Description,
			Summary,
			ProductID,
			DisplayOrder,
			KitGroupTypeID,
			IsRequired,
			IsReadOnly,
			CreatedOn)
		VALUES (
			newid(),
			@Name,
			@Description,
			@Summary,
			@ProductID,
			@DisplayOrder,
			@KitGroupTypeID,
			@IsRequired,
			@IsReadOnly,
			getdate())

		SET @SavedID = SCOPE_IDENTITY()
	END
END

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_SaveKitItem]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_SaveKitItem]
GO
create procedure [dbo].[aspdnsf_SaveKitItem](
	@KitItemID int,
	@KitGroupID int,
	@Name nvarchar(400),
	@Description nvarchar(max),
	@PriceDelta money,
	@WeightDelta money,
	@IsDefault bit,
	@DisplayOrder int,
	@InventoryVariantID int,
	@InventoryQuantityDelta int,
	@InventoryVariantColor nvarchar(100),
	@InventoryVariantSize nvarchar(100),
	@SavedID int OUTPUT
)

AS
BEGIN

	IF EXISTS(SELECT * FROM KitItem WITH(NOLOCK) WHERE KitItemId = @KitItemId )
	BEGIN
		UPDATE KitItem SET
			KitGroupID = @KitGroupID,
			[Name] = @Name,
			Description = @Description,
			PriceDelta = @PriceDelta,
			WeightDelta = @WeightDelta,
			IsDefault = @IsDefault,
			DisplayOrder = @DisplayOrder,
			InventoryVariantID = @InventoryVariantID,
			InventoryQuantityDelta = @InventoryQuantityDelta,
			InventoryVariantColor = @InventoryVariantColor,
			InventoryVariantSize = @InventoryVariantSize
		WHERE KitItemID = @KitItemID

		SET @SavedID = @KitItemId
	END
	ELSE
	BEGIN

		INSERT INTO KitItem( KitItemGUID,
			KitGroupID,
			[Name],
			Description,
			PriceDelta,
			WeightDelta,
			IsDefault,
			DisplayOrder,
			InventoryVariantID,
			InventoryQuantityDelta,
			InventoryVariantColor,
			InventoryVariantSize,
			CreatedOn)
		VALUES ( newid(),
			@KitGroupID,
			@Name,
			@Description,
			@PriceDelta,
			@WeightDelta,
			@IsDefault,
			@DisplayOrder,
			@InventoryVariantID,
			@InventoryQuantityDelta,
			@InventoryVariantColor,
			@InventoryVariantSize,
			getdate() )

		SET @SavedID = SCOPE_IDENTITY()

	END

END

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insBadWord]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insBadWord]
GO
create proc [dbo].[aspdnsf_insBadWord]
    @LocaleSetting nvarchar(10),
    @Word nvarchar(100),
    @BadWordID int OUTPUT

AS
SET NOCOUNT ON

INSERT INTO [dbo].[BadWord] (LocaleSetting, Word, CreatedOn)
VALUES(@LocaleSetting,@Word,getdate())

set @BadWordId = SCOPE_IDENTITY()

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insProductType]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insProductType]
GO

create proc [dbo].[aspdnsf_insProductType]
    @Name nvarchar(400),
    @ProductTypeID int OUTPUT

AS
SET NOCOUNT ON

if exists (select * FROM dbo.ProductType with (nolock) where [Name] = @Name) begin
 select @ProductTypeID=ProductTypeID FROM dbo.ProductType with (nolock) where [Name] = @Name
end
else begin
    insert dbo.ProductType(ProductTypeGUID, Name) values (newid(),@Name)
    set @ProductTypeID = SCOPE_IDENTITY()
end

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_CloneProduct]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop proc [dbo].[aspdnsf_CloneProduct]
GO
CREATE proc [dbo].[aspdnsf_CloneProduct]
    @productID int,
    @userid int = 0

AS
BEGIN

SET NOCOUNT ON

DECLARE @tmpKitTbl TABLE(KitGroupID int not null)
DECLARE @tmpPVariantTbl TABLE(VariantID int not null)
DECLARE @newproductID int
DECLARE @err int, @newkitgroupid int

SET @newproductID = -1

-- We need to build up a name string with all of the ML-values prefix ed with "(Cloned)".
-- To do that, we create a variable with the name of the variant, loop through all locales
-- in the name, and prepend the "(Cloned) " string to each localized name.

-- Create the @updatedName variable and initialize it with the variant's name. It may or 
-- may not contain ML data.
DECLARE @updatedName NVARCHAR(MAX) = ''
SELECT
	@updatedName = Name
FROM 
	Product
where
	ProductID = @productId

-- Create a CTE with each locale in the variant's name. If there is no ML data, this will 
-- return one null locale.
;WITH variantLocales AS (
	SELECT DISTINCT
		ml_name.Locale Locale
	FROM
		Product
		CROSS APPLY dbo.ParseMlLocales(Name) ml_name
	WHERE
		ProductID = @productId
		AND Deleted = 0
)
	-- Loop through each locale in the CTE, updating the variable's value for that locale. If
	-- a null locale name is given, it will just write out the name instead of the ML data XML.
SELECT 
	@updatedName = dbo.SetMlValue(
		@updatedName, 
		'(Cloned) ' + dbo.GetMlValue(@updatedName, variantLocales.Locale), 
		variantLocales.locale)
FROM 
	variantLocales;

-- Now we can run our normal operations with the updated name.
BEGIN TRAN
    INSERT [dbo].product (ProductGUID, Name, Summary, Description, SEKeywords, SEDescription, MiscText, FroogleDescription, SETitle, SEAltText, SizeOptionPrompt, ColorOptionPrompt, TextOptionPrompt, ProductTypeID, TaxClassID, SKU, ManufacturerPartNumber, SalesPromptID, IsFeatured, XmlPackage, ColWidth, Published, RequiresRegistration, Looks, Notes, QuantityDiscountID, RelatedProducts, UpsellProducts, UpsellProductDiscountPercentage, RelatedDocuments, TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor, IsAKit, ShowInProductBrowser, ShowBuyButton, RequiresProducts, HidePriceUntilCart, IsCalltoOrder, ExcludeFromPriceFeeds, RequiresTextOption, TextOptionMaxLength, SEName, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
    SELECT newid(), @updatedName, Summary, Description, SEKeywords, SEDescription, MiscText, FroogleDescription, SETitle, SEAltText, SizeOptionPrompt, ColorOptionPrompt, TextOptionPrompt, ProductTypeID, TaxClassID, SKU, ManufacturerPartNumber, SalesPromptID, IsFeatured, XmlPackage, ColWidth, 0, RequiresRegistration, 0, Notes, QuantityDiscountID, RelatedProducts, UpsellProducts, UpsellProductDiscountPercentage, RelatedDocuments, TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor, IsAKit, ShowInProductBrowser, ShowBuyButton, RequiresProducts, HidePriceUntilCart, IsCalltoOrder, ExcludeFromPriceFeeds, RequiresTextOption, TextOptionMaxLength, SEName, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, getdate()
    FROM dbo.product
    WHERE productid = @productID

    SELECT @newproductID = SCOPE_IDENTITY(), @err = @@error

    IF @err <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -1
    END

        DECLARE @PrdVariantID int, @newvariantID int
        INSERT @tmpPVariantTbl SELECT VariantID FROM dbo.productvariant  WHERE productid = @productID
        SELECT top 1 @PrdVariantID = VariantID FROM @tmpPVariantTbl
        WHILE @@rowcount <> 0 BEGIN

            INSERT [dbo].productvariant (VariantGUID, ProductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
            SELECT newid(), @newproductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, getdate()
            FROM dbo.productvariant
            WHERE VariantID = @PrdVariantID

            SELECT @newvariantID = SCOPE_IDENTITY(), @err = @@error

            IF @err <> 0 BEGIN
                raiserror('Product not cloned', 1, 16)
                rollback tran
                return -2
            END


            INSERT [dbo].Inventory (InventoryGUID, VariantID, Color, Size, Quan, CreatedOn)
            SELECT newid(), @newvariantID, Color, Size, Quan, getdate()
            FROM dbo.Inventory
            WHERE VariantID = @PrdVariantID

		INSERT [dbo].ExtendedPrice (ExtendedPriceGUID, VariantID, CustomerLevelID, Price, ExtensionData, CreatedOn, UpdatedOn)
		SELECT newid(), @newvariantID, CustomerLevelID, Price, ExtensionData, getdate(), getdate()
		FROM ExtendedPrice where VariantID = @PrdVariantID
            IF @@error <> 0 BEGIN
                raiserror('Product not cloned', 1, 16)
                rollback tran
                return -14
            END

            DELETE @tmpPVariantTbl where VariantID = @PrdVariantID
            SELECT top 1 @PrdVariantID = VariantID from @tmpPVariantTbl
    END





    DECLARE @kitgrpid int
    INSERT @tmpKitTbl select KitGroupID FROM kitgroup  where productid = @productID
    SELECT top 1 @kitgrpid = KitGroupID FROM @tmpKitTbl
    WHILE @@rowcount <> 0 BEGIN
        INSERT [dbo].kitgroup (KitGroupGUID, Name, Description, ProductID, DisplayOrder, KitGroupTypeID, IsRequired, ExtensionData, CreatedOn)
        SELECT newid(), Name, Description, @newproductID, DisplayOrder, KitGroupTypeID, IsRequired, ExtensionData, getdate()
        FROM dbo.kitgroup
        WHERE KitGroupID = @kitgrpid

        SELECT @newkitgroupid = SCOPE_IDENTITY(), @err = @@error

        IF @err <> 0 BEGIN
            raiserror('Product not cloned', 1, 16)
            rollback tran
            return -3
        END


        INSERT [dbo].kititem (KitItemGUID, KitGroupID, Name, Description, PriceDelta, IsDefault, DisplayOrder, TextOptionMaxLength, TextOptionWidth, TextOptionHeight, ExtensionData, InventoryVariantID, InventoryVariantColor, InventoryVariantSize, CreatedOn)
        SELECT newid(), @newkitgroupid, kititem.Name, kititem.Description, kititem.PriceDelta, kititem.IsDefault, kititem.DisplayOrder, kititem.TextOptionMaxLength, kititem.TextOptionWidth, kititem.TextOptionHeight, kititem.ExtensionData, kititem.InventoryVariantID, kititem.InventoryVariantColor, kititem.InventoryVariantSize, getdate()
        FROM dbo.kititem
        WHERE KitGroupID = @kitgrpid

        IF @@error <> 0 BEGIN
            raiserror('Product not cloned', 1, 16)
            rollback tran
            return -6
        END

        DELETE @tmpKitTbl WHERE KitGroupID = @kitgrpid
        SELECT top 1 @kitgrpid = KitGroupID FROM @tmpKitTbl
    END

    INSERT [dbo].productcategory (ProductID, CategoryID, CreatedOn)
    SELECT @newproductID, CategoryID, getdate()
    FROM dbo.productcategory
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -5
    END



    INSERT [dbo].productsection (ProductID, SectionID, CreatedOn)
    SELECT @newproductID, SectionID, getdate()
    FROM dbo.productsection
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -7
    END


    INSERT [dbo].productaffiliate (ProductID, AffiliateID, CreatedOn)
    SELECT @newproductID, AffiliateID, getdate()
    FROM dbo.productaffiliate
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -9
    END

    INSERT [dbo].productcustomerlevel (ProductID, CustomerLevelID, CreatedOn)
    SELECT @newproductID, CustomerLevelID, getdate()
    FROM dbo.productcustomerlevel
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -10
    END

    INSERT [dbo].productlocalesetting (ProductID, LocaleSettingID, CreatedOn)
    SELECT @newproductID, LocaleSettingID, getdate()
    FROM dbo.productlocalesetting
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -11
    END

    INSERT [dbo].ProductManufacturer (ManufacturerID, ProductID, DisplayOrder, CreatedOn)
    SELECT ManufacturerID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productmanufacturer
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -12
    END


    INSERT [dbo].ProductDistributor (DistributorID, ProductID, DisplayOrder, CreatedOn)
    SELECT DistributorID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productdistributor
    WHERE productid = @productID

    INSERT [dbo].ProductGenre (GenreID, ProductID, DisplayOrder, CreatedOn)
    SELECT GenreID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productgenre
    WHERE productid = @productID

    INSERT [dbo].ProductVector (VectorID, ProductID, DisplayOrder, CreatedOn)
    SELECT VectorID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productvector
    WHERE productid = @productID

    INSERT [dbo].ProductStore (ProductId, StoreId, CreatedOn)
    SELECT @newproductID, StoreId, getdate()
    FROM dbo.ProductStore
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -13
    END


    -- return one result row with new Product ID
    select @newproductID
COMMIT TRAN
END

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_CloneVariant]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop proc [dbo].[aspdnsf_CloneVariant]
GO
CREATE proc [dbo].[aspdnsf_CloneVariant]
	@variantId int
AS BEGIN
	DECLARE @newVariantId int = 0

	-- We need to build up a name string with all of the ML-values prefix ed with "(Cloned)".
	-- To do that, we create a variable with the name of the variant, loop through all locales
	-- in the name, and prepend the "(Cloned) " string to each localized name.

	-- Create the @updatedName variable and initialize it with the variant's name. It may or 
	-- may not contain ML data.
	DECLARE @updatedName NVARCHAR(MAX) = ''
	SELECT
		@updatedName = Name
	FROM 
		ProductVariant
	where
		VariantID = @variantId

	-- Create a CTE with each locale in the variant's name. If there is no ML data, this will 
	-- return one null locale.
	;WITH variantLocales AS (
		SELECT DISTINCT
			ml_name.Locale Locale
		FROM
			ProductVariant
			CROSS APPLY dbo.ParseMlLocales(Name) ml_name
		WHERE
			VariantID = @variantId
			AND Deleted = 0
	)
	-- Loop through each locale in the CTE, updating the variable's value for that locale. If
	-- a null locale name is given, it will just write out the name instead of the ML data XML.
	SELECT 
		@updatedName = dbo.SetMlValue(
			@updatedName, 
			'(Cloned) ' + dbo.GetMlValue(@updatedName, variantLocales.Locale), 
			variantLocales.locale)
	FROM 
		variantLocales

	-- Now duplicate the target variant, using the name we generated above
	INSERT dbo.ProductVariant(
		VariantGUID,
		ProductID,
		IsDefault,
		Name,
		[Description],
		SEKeywords,
		SEDescription,
		Colors,
		ColorSKUModifiers,
		Sizes,
		SizeSKUModifiers,
		FroogleDescription,
		SKUSuffix,
		ManufacturerPartNumber,
		Price,
		SalePrice,
		[Weight],
		MSRP,
		Cost,
		Points,
		Dimensions,
		Inventory,
		DisplayOrder,
		Notes,
		IsTaxable,
		IsShipSeparately,
		IsDownload,
		DownloadLocation,
		FreeShipping,
		Published,
		IsSecureAttachment,
		IsRecurring,
		RecurringInterval,
		RecurringIntervalType,
		RewardPoints,
		SEName,
		RestrictedQuantities,
		MinimumQuantity,
		ExtensionData,
		ExtensionData2,
		ExtensionData3,
		ExtensionData4,
		ExtensionData5,
		ImageFilenameOverride,
		IsImport,
		Deleted)
	SELECT
		newid(),
		ProductID,
		0,
		@updatedName,
		[Description],
		SEKeywords,
		SEDescription,
		Colors,
		ColorSKUModifiers,
		Sizes,
		SizeSKUModifiers,
		FroogleDescription,
		SKUSuffix,
		ManufacturerPartNumber,
		Price,
		SalePrice,
		[Weight],
		MSRP,
		Cost,
		Points,
		Dimensions,
		Inventory,
		DisplayOrder,
		Notes,
		IsTaxable,
		IsShipSeparately,
		IsDownload,
		DownloadLocation,
		FreeShipping,
		0,
		IsSecureAttachment,
		IsRecurring,
		RecurringInterval,
		RecurringIntervalType,
		RewardPoints,
		SEName,
		RestrictedQuantities,
		MinimumQuantity,
		ExtensionData,
		ExtensionData2,
		ExtensionData3,
		ExtensionData4,
		ExtensionData5,
		ImageFilenameOverride,
		IsImport,
		Deleted
	FROM
		dbo.ProductVariant
	WHERE
		VariantID = @variantId

	-- Save the ID of the cloned variant so we can update related tables.
	SELECT @newVariantId = SCOPE_IDENTITY()

	IF @@error <> 0 BEGIN
		raiserror('Variant not cloned', 1, 16)
		SELECT 0 VariantID
		RETURN
	END
	ELSE BEGIN
		-- Clone any extended prices
		INSERT dbo.ExtendedPrice (ExtendedPriceGUID, VariantID, CustomerLevelID, Price, ExtensionData)
		SELECT newid(), @newVariantId, CustomerLevelID, Price, ExtensionData
		FROM dbo.ExtendedPrice
		WHERE VariantID = @variantId

		-- Clone any inventory records
		INSERT dbo.Inventory (InventoryGUID, VariantID, Color, Size, Quan)
		SELECT newid(), @newVariantId, Color, Size, Quan
		FROM dbo.Inventory
		WHERE VariantID = @variantId

		SELECT @newVariantId VariantID
	END
END
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_CreateGiftCard]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_CreateGiftCard]
GO
CREATE proc [dbo].[aspdnsf_CreateGiftCard]
    @SerialNumber nvarchar(200),
    @PurchasedByCustomerID int,
    @OrderNumber int = null,
    @ShoppingCartRecID int,
    @ProductID int = null,
    @VariantID int = null,
    @InitialAmount money = null,
    @Balance money = null,
    @ExpirationDate datetime = null,
    @GiftCardTypeID int,
    @EMailName nvarchar(100) = null,
    @EMailTo nvarchar(100) = null,
    @EMailMessage nvarchar(max) = null,
    @ValidForCustomers nvarchar(max) = null,
    @ValidForProducts nvarchar(max) = null,
    @ValidForManufacturers nvarchar(max) = null,
    @ValidForCategories nvarchar(max) = null,
    @ValidForSections nvarchar(max) = null,
    @ExtensionData nvarchar(max) = null,
    @GiftCardID int OUTPUT

AS
SET NOCOUNT ON

IF @ExpirationDate is null
    set @ExpirationDate = dateadd(yy, 1, getdate())

insert dbo.GiftCard(GiftCardGUID, SerialNumber, PurchasedByCustomerID, OrderNumber, ShoppingCartRecID, ProductID, VariantID, InitialAmount, Balance, ExpirationDate, GiftCardTypeID, EMailName, EMailTo, EMailMessage, ValidForCustomers, ValidForProducts, ValidForManufacturers, ValidForCategories, ValidForSections, DisabledByAdministrator, ExtensionData, CreatedOn)
values
(
    newid(),
    @SerialNumber,
    @PurchasedByCustomerID,
    isnull(@OrderNumber, 0),
    isnull(@ShoppingCartRecID, 0),
    isnull(@ProductID, 0),
    isnull(@VariantID, 0),
    isnull(@InitialAmount,0),
    isnull(@Balance, 0),
    @ExpirationDate,
    @GiftCardTypeID,
    @EMailName,
    @EMailTo,
    @EMailMessage,
    @ValidForCustomers,
    @ValidForProducts,
    @ValidForManufacturers,
    @ValidForCategories,
    @ValidForSections,
    0,
    @ExtensionData,
    getdate()
)

set @GiftCardID = SCOPE_IDENTITY()

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insGiftCardUsage]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insGiftCardUsage]
GO
create proc [dbo].[aspdnsf_insGiftCardUsage]
    @GiftCardID int,
    @UsageTypeID int,
    @UsedByCustomerID int,
    @OrderNumber int,
    @Amount money,
    @ExtensionData nvarchar(max) = null,
    @GiftCardUsageID int OUTPUT

AS
BEGIN
    SET NOCOUNT ON
    DECLARE @err int, @TotalUsage money, @Balance money

    select @Balance = Balance from dbo.GiftCard with (nolock) WHERE GiftCardID = @GiftCardID
    IF @UsageTypeID in (2, 4) and @Balance < @Amount BEGIN
        SET @Amount = @Balance
    END

    BEGIN TRAN
        insert dbo.GiftCardUsage(GiftCardUsageGUID, GiftCardID, UsageTypeID, UsedByCustomerID, OrderNumber, Amount, ExtensionData, CreatedOn)
        values (newid(), @GiftCardID, @UsageTypeID, @UsedByCustomerID, @OrderNumber, @Amount, @ExtensionData, getdate())

        SELECT  @GiftCardUsageID = SCOPE_IDENTITY(), @err = @@ERROR
        IF @err <> 0 BEGIN
            SET @GiftCardUsageID = -2
            RAISERROR('Could not enter gift card usage transaction', 16, 1)
            ROLLBACK TRAN
            RETURN
        END

        SELECT @TotalUsage = sum(Amount*(case when UsageTypeID in (2, 4) then -1 else 1 end)) FROM dbo.GiftCardUsage with (nolock) WHERE GiftCardID = @GiftCardID
        UPDATE dbo.GiftCard
        SET Balance = InitialAmount + @TotalUsage
        WHERE GiftCardID = @GiftCardID

        IF @err <> 0 BEGIN
            SET @GiftCardUsageID = -3
            RAISERROR('Could not update gift card balance', 16, 1)
            ROLLBACK TRAN
            RETURN
        END

    COMMIT TRAN
END

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insAppconfig]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insAppconfig]
GO
create proc dbo.aspdnsf_insAppconfig
    @Name nvarchar(100),
    @Description nvarchar(max),
    @ConfigValue nvarchar(max),
    @GroupName nvarchar(100),
    @SuperOnly tinyint,
    @StoreID int,
    @ValueType nvarchar(100) = null,
    @AllowableValues nvarchar(max) = null,
    @AppConfigID int OUTPUT

AS
SET NOCOUNT ON


    INSERT dbo.Appconfig(AppConfigGUID, Name, Description, ConfigValue, GroupName, SuperOnly, ValueType, AllowableValues, StoreID,CreatedOn)
    VALUES (newid(), @Name, @Description, @ConfigValue, @GroupName, @SuperOnly, @ValueType, @AllowableValues, @StoreID, getdate())

    set @AppConfigID = SCOPE_IDENTITY()

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_SecurityLogInsert]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_SecurityLogInsert]
GO
create proc [dbo].[aspdnsf_SecurityLogInsert]
    @SecurityAction nvarchar(100),
    @Description nvarchar(max),
    @CustomerUpdated int,
    @UpdatedBy int,
    @CustomerSessionID int,
    @logid bigint OUTPUT

AS
SET NOCOUNT ON


insert dbo.SecurityLog(SecurityAction, Description, ActionDate, CustomerUpdated, UpdatedBy, CustomerSessionID)
values (@SecurityAction, @Description, getdate(), @CustomerUpdated, @UpdatedBy, @CustomerSessionID)

set @logid = SCOPE_IDENTITY()

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insEventHandler]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insEventHandler]
GO
create proc [dbo].[aspdnsf_insEventHandler]
    @EventName nvarchar(20),
    @CalloutURL varchar(200),
    @XmlPackage varchar(100),
    @Active bit,
    @Debug bit,
    @EventID int OUTPUT

AS
SET NOCOUNT ON


    if exists (select * from dbo.EventHandler with (nolock) where EventName = @EventName)
        set @EventID = -1
    else begin
        INSERT dbo.EventHandler(EventName, CalloutURL, XmlPackage, Active, Debug)
        VALUES (@EventName, @CalloutURL, @XmlPackage, @Active, @Debug)
        set @EventID = SCOPE_IDENTITY()
    end

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insTaxclass]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insTaxclass]
GO
create proc [dbo].[aspdnsf_insTaxclass]
    @Name nvarchar(400),
    @TaxCode nvarchar(400),
    @DisplayOrder int,
    @TaxClassID int OUTPUT

AS
SET NOCOUNT ON

insert dbo.Taxclass(TaxClassGUID, Name, TaxCode, DisplayOrder, CreatedOn)
values (newid(), @Name, @TaxCode, @DisplayOrder, getdate())

set @TaxClassID = SCOPE_IDENTITY()

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insCountryTaxRate]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insCountryTaxRate]
GO
create proc [dbo].[aspdnsf_insCountryTaxRate]
    @CountryID int,
    @TaxClassID int,
    @TaxRate money,
    @CountryTaxID int OUTPUT

AS
SET NOCOUNT ON


insert dbo.CountryTaxRate(CountryID, TaxClassID, TaxRate, CreatedOn)
values (@CountryID, @TaxClassID, @TaxRate, getdate())

set @CountryTaxID = SCOPE_IDENTITY()

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insStateTaxRate]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insStateTaxRate]
GO
create proc [dbo].[aspdnsf_insStateTaxRate]
    @StateID int,
    @TaxClassID int,
    @TaxRate money,
    @StateTaxID int OUTPUT

AS
SET NOCOUNT ON

insert dbo.StateTaxRate(StateID, TaxClassID, TaxRate, CreatedOn)
values (@StateID, @TaxClassID, @TaxRate, getdate())

set @StateTaxID = SCOPE_IDENTITY()

GO

/* ======== AppConfig Cleanup ======== */
delete from AppConfig where Name = 'Account.ShowBirthDateField'
delete from AppConfig where Name = 'DisablePasswordAutocomplete'
delete from AppConfig where Name = 'RedirectLiveToWWW'
delete from AppConfig where Name = 'Checkout.Type'
delete from AppConfig where Name = 'UseMappingShipToPayment'
delete from AppConfig where Name = 'ShowPreviousPurchase'
delete from AppConfig where Name LIKE '%InternationalCheckout%'
delete from AppConfig where Name = 'Micropay.ShowTotalOnTopOfCartPage'
delete from AppConfig where Name = 'Micropay.HideOnCartPage'
delete from AppConfig where Name LIKE '%SecureNet%'
delete from AppConfig where Name LIKE '%Vortx.OnePageCheckout%'
delete from AppConfig where Name = 'OrderEditingEnabled'
delete from AppConfig where name = 'PayPal.Express.AllowAnon'
delete from AppConfig where name = 'CardinalCommerce.Centinel.TermURL'
delete from AppConfig where name = 'PasswordIsOptionalDuringCheckout'
delete from AppConfig where name = 'HidePasswordFieldDuringCheckout'
DELETE FROM AppConfig WHERE Name = 'PayPal.Express.ReturnURL'
DELETE FROM AppConfig WHERE Name = 'PayPal.Express.CancelURL'
DELETE FROM AppConfig WHERE Name = 'PayPal.Express.NotificationURL'
DELETE FROM AppConfig WHERE Name = 'AllowMultipleShippingAddressPerOrder'
DELETE FROM AppConfig WHERE Name = 'MultiShipMaxItemsAllowed'
DELETE FROM AppConfig WHERE Name = 'HideProductNextPrevLinks'
DELETE FROM AppConfig WHERE Name = 'AllowEmptySkuAddToCart'
DELETE FROM AppConfig WHERE Name = 'AllowAddressChangeOnCheckoutShipping'
DELETE FROM AppConfig WHERE Name = 'AnonCheckoutReqEmail'
DELETE FROM AppConfig WHERE Name = 'AffiliateEMailAddress'
DELETE FROM AppConfig WHERE Name = 'AddToCart.AddToCartButton'
DELETE FROM AppConfig WHERE Name = 'AddToCart.AddToWishButton'
DELETE FROM AppConfig WHERE Name = 'AddToCart.UseImageButton'
DELETE FROM AppConfig WHERE Name = 'Admin_OrderStatisticIsChart'
DELETE FROM AppConfig WHERE Name = 'Admin_ShowReportSQL'
DELETE FROM AppConfig WHERE Name = 'CacheEntityPageHTML'
DELETE FROM AppConfig WHERE Name = 'HomeTemplate'
DELETE FROM AppConfig WHERE Name = 'HomeTemplateAsIs'
DELETE FROM AppConfig WHERE Name = 'KitCategoryID'
DELETE FROM AppConfig WHERE Name = 'ManufacturersLinkToOurPage'
DELETE FROM AppConfig WHERE Name = 'MaxMenuLevel'
DELETE FROM AppConfig WHERE Name = 'PayPal.API.RefundVersion'
DELETE FROM AppConfig WHERE Name = 'PayPal.Express.AllowAnonCheckout'
DELETE FROM AppConfig WHERE Name = 'PayPal.Express.BillMeLaterMarketingMessage'
DELETE FROM AppConfig WHERE Name = 'PayPal.ReturnCancelURL'
DELETE FROM AppConfig WHERE Name = 'PayPal.ReturnOKURL'
DELETE FROM AppConfig WHERE Name = 'PersistFilters'
DELETE FROM AppConfig WHERE Name = 'SearchAdv_ShowSKU'
DELETE FROM AppConfig WHERE Name = 'ShowInStorePickupInShippingEstimator'
DELETE FROM AppConfig WHERE Name = 'ShowPriceRegularPrompt'
DELETE FROM AppConfig WHERE Name = 'ShowSpecialsPics'
DELETE FROM AppConfig WHERE Name = 'XmlPackage.AffiliateSignupNotification'
DELETE FROM AppConfig WHERE Name = 'XmlPackage.OrderAsXml'
DELETE FROM AppConfig WHERE Name = 'XmlPackage.OrderFinalization'
DELETE FROM AppConfig WHERE Name = 'PayPal.API.Version'
DELETE FROM AppConfig WHERE Name = 'PayPal.PaymentIcon'
DELETE FROM AppConfig WHERE Name = 'AmazonPayments.ButtonImageURL'
DELETE FROM AppConfig WHERE Name = 'AUTHORIZENET_Verify_Addresses'
DELETE FROM AppConfig WHERE Name = 'eProcessingNetwork_Verify_Addresses'
DELETE FROM AppConfig WHERE Name = 'Default_DocumentColWidth'
DELETE FROM AppConfig WHERE Name = 'Default_DocumentPageSize'
DELETE FROM AppConfig WHERE Name = 'ProductImg_swatch'
DELETE FROM AppConfig WHERE Name = 'EntityDescrHTMLEqualsEmpty'
DELETE FROM AppConfig WHERE Name = 'SharedSSLLocation'
DELETE FROM AppConfig WHERE Name = 'UseStaticLinks'
delete from GlobalConfig where name = 'Anonymous.AllowAlreadyRegisteredEmail'
delete from GlobalConfig where name = 'AllowCustomerDuplicateEMailAddresses'

delete from appconfig where name = 'Zoomify.Active'
delete from appconfig where name = 'Zoomify.GalleryMedium'
delete from appconfig where name = 'Zoomify.Large.Height'
delete from appconfig where name = 'Zoomify.Large.Width'
delete from appconfig where name = 'Zoomify.Medium.Height'
delete from appconfig where name = 'Zoomify.Medium.Width'
delete from appconfig where name = 'Zoomify.ProductLarge'
delete from appconfig where name = 'Zoomify.ProductMedium'

delete from appconfig where name = '1stPay.AdminTransactionEmail.Enable'
delete from appconfig where name = '1stPay.CustomerTransactionEmail.Enable'
delete from appconfig where name = '1stPay.Level2.Enable'
delete from appconfig where name = '1stPay.Cim.Enable'
delete from appconfig where name = '1stPay.TransactionCenterId'
delete from appconfig where name = '1stPay.ProcessorId'
delete from appconfig where name = '1stPay.GatewayId'
delete from appconfig where name = '1stPay.EmailHeader'
delete from appconfig where name = '1stPay.EmailFooter'
delete from appconfig where name = '1stPay.PaymentModuleURL'
delete from appconfig where name = '1stPay.XmlURL'
delete from appconfig where name = '1stPay.TestProccessor'
delete from appconfig where name = 'RecentAdditionsShowPics'
delete from appconfig where name = 'OrderShowCCPwd'
delete from appconfig where name = 'DotFeed.Connect.ApiUri'

IF NOT EXISTS (SELECT * FROM AppConfig WHERE Name = 'IPAddress.RefuseRestrictedIPsFromSite')
	INSERT INTO [dbo].[AppConfig] ([StoreID], [Name], [Description], [ConfigValue], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) 
	VALUES (0, N'IPAddress.RefuseRestrictedIPsFromSite', N'If false, IPs listed in the RestrictedIPs table in the DB (from being banned, marked as fraud, etc) aren''t blocked from the site.  This should usually be left true.', N'true', N'boolean', NULL, N'SECURITY', 1, 0)

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetAvailablePromos]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetAvailablePromos]
GO
create proc [dbo].[aspdnsf_GetAvailablePromos]
    @productIdList	nvarchar(max),
	@StoreID		int = 0,
	@CustomerID		int = 0,
	@CustomerLevel	int = 0

AS
BEGIN
	declare @productIds table (ProductId int not null)
	insert into @productIds select distinct * from dbo.Split(@productIdList, ',')

	declare @FilterPromotions tinyint
	SET @FilterPromotions = (SELECT case ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM GlobalConfig WHERE Name='AllowPromotionFiltering')

	DECLARE @CustomerEmail varchar(max)
	SELECT @CustomerEmail = Email FROM Customer WHERE CustomerID = @CustomerID
				
	select
		DISTINCT ids.ProductId,
		p.CallToAction
	from
		Promotions p
			left join (SELECT PromotionRuleData.value('(/ArrayOfPromotionRuleBase/PromotionRuleBase/ExpirationDate/node())[1]', 'nvarchar(40)') AS ExpDate, Id 
				FROM Promotions) d on d.Id = p.Id
			left join PromotionStore pt
				on p.Id = pt.PromotionID, @productIds ids
			left join ProductCategory pc
				on pc.ProductId = ids.ProductId
			left join ProductSection ps
				on ps.ProductId = ids.ProductId
			left join ProductManufacturer pm
				on pm.ProductId = ids.ProductId
	where
		(d.ExpDate IS NULL OR CONVERT(date, d.ExpDate) > getDate())
		and p.Active = 1 
		and p.AutoAssigned = 1
		and isnull(p.CallToAction, '') != ''
		and (@FilterPromotions = 0 OR pt.StoreID = @StoreID)
		and (
				-- ProductIdPromotionRule
				p.PromotionRuleData.exist('/ArrayOfPromotionRuleBase/PromotionRuleBase/ProductIds[int = sql:column("ids.ProductId")]') = 1

				-- CategoryPromotionRule
				or p.PromotionRuleData.exist('/ArrayOfPromotionRuleBase/PromotionRuleBase/CategoryIds[int = sql:column("pc.CategoryId")]') = 1

				-- SectionPromotionRule
				or p.PromotionRuleData.exist('/ArrayOfPromotionRuleBase/PromotionRuleBase/SectionIds[int = sql:column("ps.SectionId")]') = 1

				-- ManufacturerPromotionRule
				or p.PromotionRuleData.exist('/ArrayOfPromotionRuleBase/PromotionRuleBase/ManufacturerIds[int = sql:column("pm.ManufacturerId")]') = 1

				-- GiftProductPromotionDiscount
				or p.PromotionDiscountData.exist('/ArrayOfPromotionDiscountBase/PromotionDiscountBase/GiftProductIds[int = sql:column("ids.ProductId")]') = 1
			)
		-- Email Address rule
		and (
				p.PromotionRuleData.exist('/ArrayOfPromotionRuleBase/PromotionRuleBase/EmailAddresses') = 0
				or p.PromotionRuleData.exist('/ArrayOfPromotionRuleBase/PromotionRuleBase/EmailAddresses[string = sql:variable("@CustomerEmail")]') = 1
			)
		-- Customer Level rule
		and (
				p.PromotionRuleData.exist('/ArrayOfPromotionRuleBase/PromotionRuleBase/CustomerLevels') = 0
				or p.PromotionRuleData.exist('/ArrayOfPromotionRuleBase/PromotionRuleBase/CustomerLevels[int = sql:variable("@CustomerLevel")]') = 1
			)
	for xml path('Promotion')
END
go
/************************** Site Map AppConfig Rename **************************/
delete from AppConfig where Name = 'GoogleSiteMap.Xmlns'
update AppConfig set Name = 'SiteMapFeed.EntityChangeFreq' where Name = 'GoogleSiteMap.EntityChangeFreq'
update AppConfig set Name = 'SiteMapFeed.EntityPriority' where Name = 'GoogleSiteMap.EntityPriority'
update AppConfig set Name = 'SiteMapFeed.ObjectChangeFreq' where Name = 'GoogleSiteMap.ObjectChangeFreq'
update AppConfig set Name = 'SiteMapFeed.ObjectPriority' where Name = 'GoogleSiteMap.ObjectPriority'
update AppConfig set Name = 'SiteMapFeed.TopicChangeFreq' where Name = 'GoogleSiteMap.TopicChangeFreq'
update AppConfig set Name = 'SiteMapFeed.TopicPriority' where Name = 'GoogleSiteMap.TopicPriority'

update AppConfig SET Description = 'Frequency tag used to build the Site Map feed (category, section, manufacturer, etc) URL nodes. Consult http://www.sitemaps.org/protocol.html documentation for a list of the values allowed here. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'SiteMapFeed.EntityChangeFreq'
update AppConfig SET Description = 'Priority tag used to build the Site Map feed entity (category, section, manufacturer, etc) URL nodes. Consult http://www.sitemaps.org/protocol.html documentation for a list of values allowed here. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'SiteMapFeed.EntityPriority'
update AppConfig SET Description = 'Frequency tag used to build the Site Map feed product (object) URL nodes. Consult http://www.sitemaps.org/protocol.html documentation for a list of values allowed here. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'SiteMapFeed.ObjectChangeFreq'
update AppConfig SET Description = 'Priority tag used to build the Site Map feed product (object) URL nodes. Consult http://www.sitemaps.org/protocol.html documentation for a list of values allowed here. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'SiteMapFeed.ObjectPriority'
update AppConfig SET Description = 'Frequency tag used to build the Site Map feed topic URL nodes. Consult http://www.sitemaps.org/protocol.html documentation for a list of values allowed here. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'SiteMapFeed.TopicChangeFreq'
update AppConfig SET Description = 'Priority tag used to build the Site Map feed topic URL nodes. Consult http://www.sitemaps.org/protocol.html documentation for allowed values. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'SiteMapFeed.TopicPriority'

/************************** AlwaysUseHTTPS AppConfig **************************/
if not exists (select * from AppConfig where Name = 'AlwaysUseHTTPS')
	insert into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues) 
	values ('AlwaysUseHTTPS', 'Set to true to always 301 redirect non-secure requests to HTTPS. Set to false to not take any special action on non-secure requests to non-secure pages. Note that this setting overrides the GoNonSecureAgain AppConfig.', 'SECURITY', 'False', 'boolean', null)

/************************** HstsHeader AppConfig **************************/
if not exists (select * from AppConfig where Name = 'HstsHeader')
	insert into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues) 
	values ('HstsHeader', 'This adds the HSTS Strict-Transport-Security HSTS header to the site as long as the AlwaysUseHTTPS appconfig is enabled. To remove the header, clear out this appconfig, to disable HSTS set the max-age to 0, to adjust the HSTS age set the HSTS max-age in seconds. For example, to tell browsers to always use https for 180 days, set this value to: max-age=15552000', 'SECURITY', '', 'string', null)

/************************** Update Country Postal Code Regexes and Samples **************************/
update Country set PostalCodeRegex = '^(?![dfioquDFIOQU])[a-zA-Z]\d(?![dfioquDFIOQU])[a-zA-Z]\s?\d(?![dfioquDFIOQU])[a-zA-Z]\d$' where TwoLetterISOCode = 'CA' and PostalCodeRegEx = '^([a-z-[dfioqu]]|[A-Z-[DFIOQU]])\d([a-z-[dfioqu]]|[A-Z-[DFIOQU]])(\s)?\d([a-z-[dfioqu]]|[A-Z-[DFIOQU]])\d$'
update Country set PostalCodeRegex = '^([Dd]([Ee][Nn][Mm][Aa][Rr])?[Kk](\s|-))?\d{3,4}$' where TwoLetterISOCode = 'DK' and PostalCodeRegEx = '^(((D|d)(K|k)(\s|-)?)|((D|d)(E|e)(N|n)(M|m)(A|a)(R|r)(K|k)(\s|-)?))?\d{3,4}$'
update Country set PostalCodeRegex = '^[0-9]{4}(-[0-9]{3})?$'where TwoLetterISOCode = 'PT' and PostalCodeRegex = N'^[0-9]{4,4}(-[0-9]{3,3}){0,1}$'
update Country set PostalCodeExample = '1200 or 1350-224' where TwoLetterISOCode = 'PT' and PostalCodeExample = N'#### or ####-###'
update Country set PostalCodeExample = '12345 or 123 45' where TwoLetterISOCode = 'SE' and PostalCodeExample = '12345, '
update Country set PostalCodeRegex = '^([A-Za-z][A-Za-z0-9]{1,3} \d[A-Za-z0-9][A-Za-z])|([Aa][Ii]-2640)$' where TwoLetterISOCode = 'GB' and PostalCodeRegEx = '^((GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}|([A-HK-Y][0-9]|[A-HK-Y][0-9]([0-9]|[ABEHMNPRV-Y]))|[0-9][A-HJKS-UW]) [0-9][ABD-HJLNP-UW-Z]{2})|((GIR 0AA)|((([A-Z-[QVX]][0-9][0-9]?)|(([A-Z-[QVX]][A-Z-[IJZ]][0-9][0-9]?)|(([A-Z-[QVX]][0-9][A-HJKSTUW])|([A-Z-[QVX]][A-Z-[IJZ]][0-9][ABEHMNPRVWXY])))) [0-9][A-Z-[CIKMOV]]{2}){2})|([A-Z]{1,2}[0-9R][0-9A-Z]? [0-9][A-Z-[CIKMOV]]{2})|([A-Z]{1,2}[0-9R][0-9A-Z]? [0-9][A-Z]{2}))$'
go
	
/******************************* Packs Cleanup *******************************/
GO

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND Name = N'IX_Product_IsAPack')
	DROP INDEX [IX_Product_IsAPack] ON [dbo].[Product] WITH ( ONLINE = OFF )
GO

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND Name = N'IX_Product_PackSize')
	DROP INDEX [IX_Product_PackSize] ON [dbo].[Product] WITH ( ONLINE = OFF )
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE parent_obj = OBJECT_ID(N'[Product]') AND type = 'D' And Name = 'DF_Product_IsAPack')
	ALTER TABLE Product DROP CONSTRAINT DF_Product_IsAPack
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE parent_obj = OBJECT_ID(N'[Product]') AND type = 'D' And Name = 'DF_Product_PackSize')
	ALTER TABLE Product DROP CONSTRAINT DF_Product_PackSize
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('[dbo].[Product]') AND Name = 'IsAPack')
	ALTER TABLE Product DROP COLUMN IsAPack
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('[dbo].[Product]') AND Name = 'PackSize')
	ALTER TABLE Product DROP COLUMN PackSize
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('[dbo].[Orders_ShoppingCart]') AND Name = 'IsAPack')
	ALTER TABLE Orders_ShoppingCart DROP COLUMN IsAPack
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('[dbo].[ShoppingCart]') AND Name = 'IsAPack')
	ALTER TABLE ShoppingCart DROP COLUMN IsAPack
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('dbo.PackPriceDelta') AND objectproperty(id, 'IsScalarFunction') = 1)
	DROP FUNCTION dbo.PackPriceDelta
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].aspdnsf_AdjustInventory') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].aspdnsf_AdjustInventory
GO

create proc [dbo].[aspdnsf_AdjustInventory]
    @ordernumber int,
    @direction smallint -- 1 = add items to inventory, -1 = remove from inventory

AS
BEGIN
    SET NOCOUNT ON

    IF @direction <> 1 and @direction <> -1 BEGIN
        RAISERROR('Invalid direction specified', 16, 1)
        RETURN
    END

    DECLARE @InventoryWasReduced int
    SELECT @InventoryWasReduced = InventoryWasReduced FROM dbo.orders with (nolock) WHERE ordernumber = @ordernumber

    IF (@direction = 1 and @InventoryWasReduced = 1) or (@direction = -1 and @InventoryWasReduced = 0) BEGIN

        BEGIN TRAN
            -- update [dbo].Products
            update dbo.Inventory
            SET Quan = Quan + (a.qty*@direction)
            FROM dbo.Inventory i
                join (select o.variantid,
                            case when o.ChosenColor is null then '' when charindex('[', o.ChosenColor)>0 then rtrim(left(o.ChosenColor, charindex('[', o.ChosenColor)-1)) else o.ChosenColor end ChosenColor,
                            case when o.ChosenSize is null then '' when charindex('[', o.ChosenSize)>0 then rtrim(left(o.ChosenSize, charindex('[', o.ChosenSize)-1)) else o.ChosenSize end ChosenSize,
                            sum(o.Quantity) qty
                      from dbo.Orders_ShoppingCart o
                          join dbo.product p on p.ProductID = o.ProductID
                          join dbo.productvariant pv on o.ProductID = pv.ProductID and o.VariantID = pv.VariantID
                      where o.ordernumber = @ordernumber and p.TrackInventoryBySizeAndColor = 1
                      group by o.variantid,
                            case when o.ChosenColor is null then '' when charindex('[', o.ChosenColor)>0 then rtrim(left(o.ChosenColor, charindex('[', o.ChosenColor)-1)) else o.ChosenColor end ,
                            case when o.ChosenSize is null then '' when charindex('[', o.ChosenSize)>0 then rtrim(left(o.ChosenSize, charindex('[', o.ChosenSize)-1)) else o.ChosenSize end
                     ) a on i.variantid = a.variantid and isnull(i.size, '') = a.ChosenSize and isnull(i.Color, '') = a.ChosenColor

            IF @@ERROR <> 0 BEGIN
                ROLLBACK TRAN
                RAISERROR('Inventory update failed', 16, 1)
                RETURN
            END


            update dbo.ProductVariant
            SET Inventory = Inventory + (a.qty*@direction)
            FROM dbo.ProductVariant pv join [dbo].Product p on pv.productid = p.productid
                join (select o.variantid, sum(o.Quantity) qty
                      from dbo.Orders_ShoppingCart o
                          join dbo.product p on p.ProductID = o.ProductID
                          join dbo.productvariant pv on o.ProductID = pv.ProductID and o.VariantID = pv.VariantID
                      where o.ordernumber = @ordernumber and p.TrackInventoryBySizeAndColor = 0
                      group by o.variantid
                     ) a on pv.variantid = a.variantid


            IF @@ERROR <> 0 BEGIN
                ROLLBACK TRAN
                RAISERROR('ProductVariant inventory update failed', 16, 1)
                RETURN
            END


            --Update Inventory of inventoryable kititems
            UPDATE dbo.Inventory
            SET Quan = Quan + (a.qty*@direction)
            FROM dbo.Inventory i
                join (select o.InventoryVariantID variantid,
                            case when o.InventoryVariantColor is null then '' when charindex('[', o.InventoryVariantColor)>0 then rtrim(left(o.InventoryVariantColor, charindex('[', o.InventoryVariantColor)-1)) else o.InventoryVariantColor end ChosenColor,
                            case when o.InventoryVariantSize is null then '' when charindex('[', o.InventoryVariantSize)>0 then rtrim(left(o.InventoryVariantSize, charindex('[', o.InventoryVariantSize)-1)) else o.InventoryVariantSize end ChosenSize,
                            sum(o.Quantity) qty
                      from dbo.Orders_KitCart o
                          join dbo.Orders_ShoppingCart sc on sc.ShoppingCartRecID = o.ShoppingCartRecID
                          join dbo.product p on p.ProductID = o.ProductID
                          join dbo.productvariant pv on o.ProductID = pv.ProductID and o.VariantID = pv.VariantID
                      where o.ordernumber = @ordernumber and p.TrackInventoryBySizeAndColor = 1
                      group by o.InventoryVariantID,
                            case when o.InventoryVariantColor is null then '' when charindex('[', o.InventoryVariantColor)>0 then rtrim(left(o.InventoryVariantColor, charindex('[', o.InventoryVariantColor)-1)) else o.InventoryVariantColor end ,
                            case when o.InventoryVariantSize is null then '' when charindex('[', o.InventoryVariantSize)>0 then rtrim(left(o.InventoryVariantSize, charindex('[', o.InventoryVariantSize)-1)) else o.InventoryVariantSize end
                     ) a on i.variantid = a.variantid and isnull(i.size, '') = a.ChosenSize and isnull(i.Color, '') = a.ChosenColor

            IF @@ERROR <> 0 BEGIN
                ROLLBACK TRAN
                RAISERROR('KitItem Inventory update failed', 16, 1)
                RETURN
            END


            update dbo.ProductVariant
            SET Inventory = Inventory + (a.qty*@direction)
            FROM dbo.ProductVariant pv join [dbo].Product p on pv.productid = p.productid
                join (select o.InventoryVariantID variantid, sum(o.Quantity*sc.Quantity) qty
                      from dbo.Orders_KitCart o
                          join dbo.Orders_ShoppingCart sc on sc.ShoppingCartRecID = o.ShoppingCartRecID
                          join dbo.product p on p.ProductID = o.ProductID
                          join dbo.productvariant pv on o.ProductID = pv.ProductID and o.VariantID = pv.VariantID
                      where o.ordernumber = @ordernumber and p.TrackInventoryBySizeAndColor = 0
                      group by o.InventoryVariantID
                     ) a on pv.variantid = a.variantid


            IF @@ERROR <> 0 BEGIN
                ROLLBACK TRAN
                RAISERROR('KitItem ProductVariant inventory update failed', 16, 1)
                RETURN
            END

            UPDATE dbo.orders SET InventoryWasReduced = case @direction when 1 then 0 when -1 then 1 else InventoryWasReduced end WHERE ordernumber = @ordernumber

        COMMIT TRAN

    END

END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].aspdnsf_CloneProduct') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].aspdnsf_CloneProduct
GO

CREATE proc [dbo].[aspdnsf_CloneProduct]
    @productID int,
    @userid int = 0

AS
BEGIN

SET NOCOUNT ON

DECLARE @tmpKitTbl TABLE(KitGroupID int not null)
DECLARE @tmpPVariantTbl TABLE(VariantID int not null)
DECLARE @newproductID int
DECLARE @err int, @newkitgroupid int

SET @newproductID = -1

BEGIN TRAN
    INSERT [dbo].product (ProductGUID, Name, Summary, Description, SEKeywords, SEDescription, MiscText, FroogleDescription, SETitle, SEAltText, SizeOptionPrompt, ColorOptionPrompt, TextOptionPrompt, ProductTypeID, TaxClassID, SKU, ManufacturerPartNumber, SalesPromptID, IsFeatured, XmlPackage, ColWidth, Published, RequiresRegistration, Looks, Notes, QuantityDiscountID, RelatedProducts, UpsellProducts, UpsellProductDiscountPercentage, RelatedDocuments, TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor, IsAKit, ShowInProductBrowser, ShowBuyButton, RequiresProducts, HidePriceUntilCart, IsCalltoOrder, ExcludeFromPriceFeeds, RequiresTextOption, TextOptionMaxLength, SEName, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
    SELECT newid(), Name + ' - CLONED', Summary, Description, SEKeywords, SEDescription, MiscText, FroogleDescription, SETitle, SEAltText, SizeOptionPrompt, ColorOptionPrompt, TextOptionPrompt, ProductTypeID, TaxClassID, SKU, ManufacturerPartNumber, SalesPromptID, IsFeatured, XmlPackage, ColWidth, 0, RequiresRegistration, 0, Notes, QuantityDiscountID, RelatedProducts, UpsellProducts, UpsellProductDiscountPercentage, RelatedDocuments, TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor, IsAKit, ShowInProductBrowser, ShowBuyButton, RequiresProducts, HidePriceUntilCart, IsCalltoOrder, ExcludeFromPriceFeeds, RequiresTextOption, TextOptionMaxLength, SEName, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, getdate()
    FROM dbo.product
    WHERE productid = @productID

    SELECT @newproductID = SCOPE_IDENTITY(), @err = @@error

    IF @err <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -1
    END

        DECLARE @PrdVariantID int, @newvariantID int
        INSERT @tmpPVariantTbl SELECT VariantID FROM dbo.productvariant  WHERE productid = @productID
        SELECT top 1 @PrdVariantID = VariantID FROM @tmpPVariantTbl
        WHILE @@rowcount <> 0 BEGIN

            INSERT [dbo].productvariant (VariantGUID, ProductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
            SELECT newid(), @newproductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, getdate()
            FROM dbo.productvariant
            WHERE VariantID = @PrdVariantID

            SELECT @newvariantID = SCOPE_IDENTITY(), @err = @@error

            IF @err <> 0 BEGIN
                raiserror('Product not cloned', 1, 16)
                rollback tran
                return -2
            END


            INSERT [dbo].Inventory (InventoryGUID, VariantID, Color, Size, Quan, CreatedOn)
            SELECT newid(), @newvariantID, Color, Size, Quan, getdate()
            FROM dbo.Inventory
            WHERE VariantID = @PrdVariantID

		INSERT [dbo].ExtendedPrice (ExtendedPriceGUID, VariantID, CustomerLevelID, Price, ExtensionData, CreatedOn, UpdatedOn)
		SELECT newid(), @newvariantID, CustomerLevelID, Price, ExtensionData, getdate(), getdate()
		FROM ExtendedPrice where VariantID = @PrdVariantID
            IF @@error <> 0 BEGIN
                raiserror('Product not cloned', 1, 16)
                rollback tran
                return -14
            END

            DELETE @tmpPVariantTbl where VariantID = @PrdVariantID
            SELECT top 1 @PrdVariantID = VariantID from @tmpPVariantTbl
    END





    DECLARE @kitgrpid int
    INSERT @tmpKitTbl select KitGroupID FROM kitgroup  where productid = @productID
    SELECT top 1 @kitgrpid = KitGroupID FROM @tmpKitTbl
    WHILE @@rowcount <> 0 BEGIN
        INSERT [dbo].kitgroup (KitGroupGUID, Name, Description, ProductID, DisplayOrder, KitGroupTypeID, IsRequired, ExtensionData, CreatedOn)
        SELECT newid(), Name, Description, @newproductID, DisplayOrder, KitGroupTypeID, IsRequired, ExtensionData, getdate()
        FROM dbo.kitgroup
        WHERE KitGroupID = @kitgrpid

        SELECT @newkitgroupid = SCOPE_IDENTITY(), @err = @@error

        IF @err <> 0 BEGIN
            raiserror('Product not cloned', 1, 16)
            rollback tran
            return -3
        END


        INSERT [dbo].kititem (KitItemGUID, KitGroupID, Name, Description, PriceDelta, IsDefault, DisplayOrder, TextOptionMaxLength, TextOptionWidth, TextOptionHeight, ExtensionData, InventoryVariantID, InventoryVariantColor, InventoryVariantSize, CreatedOn)
        SELECT newid(), @newkitgroupid, kititem.Name, kititem.Description, kititem.PriceDelta, kititem.IsDefault, kititem.DisplayOrder, kititem.TextOptionMaxLength, kititem.TextOptionWidth, kititem.TextOptionHeight, kititem.ExtensionData, kititem.InventoryVariantID, kititem.InventoryVariantColor, kititem.InventoryVariantSize, getdate()
        FROM dbo.kititem
        WHERE KitGroupID = @kitgrpid

        IF @@error <> 0 BEGIN
            raiserror('Product not cloned', 1, 16)
            rollback tran
            return -6
        END

        DELETE @tmpKitTbl WHERE KitGroupID = @kitgrpid
        SELECT top 1 @kitgrpid = KitGroupID FROM @tmpKitTbl
    END

    INSERT [dbo].productcategory (ProductID, CategoryID, CreatedOn)
    SELECT @newproductID, CategoryID, getdate()
    FROM dbo.productcategory
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -5
    END



    INSERT [dbo].productsection (ProductID, SectionID, CreatedOn)
    SELECT @newproductID, SectionID, getdate()
    FROM dbo.productsection
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -7
    END


    INSERT [dbo].productaffiliate (ProductID, AffiliateID, CreatedOn)
    SELECT @newproductID, AffiliateID, getdate()
    FROM dbo.productaffiliate
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -9
    END

    INSERT [dbo].productcustomerlevel (ProductID, CustomerLevelID, CreatedOn)
    SELECT @newproductID, CustomerLevelID, getdate()
    FROM dbo.productcustomerlevel
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -10
    END

    INSERT [dbo].productlocalesetting (ProductID, LocaleSettingID, CreatedOn)
    SELECT @newproductID, LocaleSettingID, getdate()
    FROM dbo.productlocalesetting
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -11
    END

    INSERT [dbo].ProductManufacturer (ManufacturerID, ProductID, DisplayOrder, CreatedOn)
    SELECT ManufacturerID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productmanufacturer
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -12
    END


    INSERT [dbo].ProductDistributor (DistributorID, ProductID, DisplayOrder, CreatedOn)
    SELECT DistributorID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productdistributor
    WHERE productid = @productID

    INSERT [dbo].ProductGenre (GenreID, ProductID, DisplayOrder, CreatedOn)
    SELECT GenreID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productgenre
    WHERE productid = @productID

    INSERT [dbo].ProductVector (VectorID, ProductID, DisplayOrder, CreatedOn)
    SELECT VectorID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productvector
    WHERE productid = @productID

    INSERT [dbo].ProductStore (ProductId, StoreId, CreatedOn)
    SELECT @newproductID, StoreId, getdate()
    FROM dbo.ProductStore
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -13
    END


    -- return one result row with new Product ID
    select @newproductID
COMMIT TRAN
END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].aspdnsf_CreateIndexes') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].aspdnsf_CreateIndexes
GO

CREATE PROC [dbo].[aspdnsf_CreateIndexes]

AS
BEGIN

CREATE UNIQUE INDEX [UIX_Address_AddressGUID] ON [Address](AddressGUID);
CREATE INDEX [IX_Address_CustomerID] ON [Address](CustomerID);
CREATE INDEX [IX_Address_Deleted] ON [Address](Deleted);
CREATE INDEX [IX_AffiliateActivity_AffiliateID] ON [AffiliateActivity](AffiliateID);
CREATE INDEX [IX_AffiliateActivity_AffiliateID_ActivityDate] ON [AffiliateActivity](AffiliateID, ActivityDate);
CREATE INDEX [IX_AffiliateCommissions_RowGUID] ON [AffiliateCommissions](RowGUID);
CREATE INDEX [IX_AffiliateCommissions_LowValue] ON [AffiliateCommissions](LowValue);
CREATE INDEX [IX_AffiliateCommissions_HighValue] ON [AffiliateCommissions](HighValue);
CREATE UNIQUE INDEX [UIX_AppConfig_Name_StoreID] ON [AppConfig](Name, StoreID);
CREATE INDEX [IX_AppConfig_GroupName] ON [AppConfig](GroupName);
CREATE INDEX [IX_BadWord] ON [BadWord](Word);
CREATE INDEX [IX_Category_Name] ON [Category](Name);
CREATE INDEX [IX_Category_Deleted] ON [Category](Deleted);
CREATE INDEX [IX_Category_Published] ON [Category](Published);
CREATE INDEX [IX_Category_Wholesale] ON [Category](Wholesale);
CREATE INDEX [IX_Category_CategoryGUID] ON [Category](CategoryGUID);
CREATE INDEX [IX_Category_ParentCategoryID] ON [Category](ParentCategoryID);
CREATE INDEX [IX_Category_DisplayOrder] ON [Category](DisplayOrder);
CREATE INDEX [IX_Category_Deleted_Published] ON [Category](Deleted, Published);
CREATE INDEX [IX_Category_Deleted_Wholesale] ON [Category](Deleted, Wholesale);
CREATE UNIQUE INDEX [UIX_Country_Name] ON [Country](Name);
CREATE INDEX [IX_Country_DisplayOrder_Name] ON [Country](DisplayOrder, Name);
CREATE INDEX [IX_Country_CountryGUID] ON [Country](CountryGUID);
CREATE INDEX [IX_Country_DisplayOrder] ON [Country](DisplayOrder);
CREATE UNIQUE INDEX [UIX_CountryTaxRate_CountryID_TaxClassID] ON [CountryTaxRate](CountryID, TaxClassID);
CREATE UNIQUE INDEX [UIX_Coupon_CouponGUID] ON [Coupon](CouponGUID);
CREATE UNIQUE INDEX [UIX_Coupon_CouponCode] ON [Coupon](CouponCode);
CREATE INDEX [IX_Coupon_ExpirationDate] ON [Coupon](ExpirationDate);
CREATE INDEX [IX_Coupon_Deleted] ON [Coupon](Deleted);
CREATE UNIQUE INDEX [UIX_CreditCardTypes] ON [CreditCardType](CardTypeGUID);
CREATE INDEX [IX_CreditCardType_CardType] ON [CreditCardType](CardType);
CREATE UNIQUE INDEX [UIX_Currency_CurrencyGUID] ON [Currency](CurrencyGUID);
CREATE UNIQUE INDEX [UIX_Customer_CustomerGUID] ON [Customer](CustomerGUID);
CREATE INDEX [IX_Customer_EMail] ON [Customer](Email);
CREATE INDEX [IX_Customer_Password] ON [Customer](Password);
CREATE INDEX [IX_Customer_CustomerLevelID] ON [Customer](CustomerLevelID);
CREATE INDEX [IX_Customer_IsAdmin] ON [Customer](IsAdmin);
CREATE INDEX [IX_Customer_OkToEMail] ON [Customer](OkToEmail);
CREATE INDEX [IX_Customer_Deleted] ON [Customer](Deleted);
CREATE INDEX [IX_Customer_AffiliateID] ON [Customer](AffiliateID);
CREATE INDEX [IX_Customer_CouponCode] ON [Customer](CouponCode);
CREATE INDEX [IX_Customer_CreatedOn] ON [Customer](CreatedOn);
CREATE INDEX [IX_CustomerLevel_Deleted] ON [CustomerLevel](Deleted);
CREATE INDEX [IX_CustomerLevel_Name] ON [CustomerLevel](Name);
CREATE INDEX [IX_CustomerLevel_DisplayOrder] ON [CustomerLevel](DisplayOrder);
CREATE INDEX [IX_CustomerLevel_DisplayOrder_Name] ON [CustomerLevel](DisplayOrder, Name);
CREATE INDEX [IX_CustomerSession_CustomerID] ON [CustomerSession](CustomerID);
CREATE UNIQUE INDEX [UIX_Distributor_DistributorGUID] ON [Distributor](DistributorGUID);
CREATE INDEX [IX_Distributor_DisplayOrder] ON [Distributor](DisplayOrder);
CREATE INDEX [IX_Distributor_Name] ON [Distributor](Name);
CREATE INDEX [IX_Distributor_DisplayOrder_Name] ON [Distributor](DisplayOrder, Name);
CREATE UNIQUE INDEX [UIX_Document_DocumentGUID] ON [Document](DocumentGUID);
CREATE INDEX [IX_Document_Published] ON [Document](Published);
CREATE INDEX [IX_Document_Wholesale] ON [Document](Wholesale);
CREATE INDEX [IX_Document_Deleted] ON [Document](Deleted);
CREATE INDEX [IX_Document_DocumentTypeID] ON [Document](DocumentTypeID);
CREATE INDEX [IX_Document_Published_Deleted] ON [Document](Published, Deleted);
CREATE INDEX [IX_Document_Wholesale_Deleted] ON [Document](Wholesale, Deleted);
CREATE INDEX [IX_Document_Name] ON [Document](Name);
CREATE INDEX [IX_DocumentType_DocumentTypeGUID] ON [DocumentType](DocumentTypeGUID);
CREATE INDEX [IX_DocumentType_Name] ON [DocumentType](Name);
CREATE INDEX [IX_DocumentType_DisplayOrder] ON [DocumentType](DisplayOrder);
CREATE INDEX [IX_DocumentType_DisplayOrder_Name] ON [DocumentType](DisplayOrder, Name);
CREATE UNIQUE INDEX [UIX_EventhHandler] ON [EventHandler](EventName);
CREATE UNIQUE INDEX [UIX_ExtendedPrice_2] ON [ExtendedPrice](ExtendedPriceGUID);
CREATE INDEX [IX_ExtendedPrice_VariantID_CustomerLevelID] ON [ExtendedPrice](VariantID, CustomerLevelID);
CREATE INDEX [IX_ExtendedPrice_VariantID] ON [ExtendedPrice](VariantID);
CREATE INDEX [IX_FailedTransaction_OrderDate] ON [FailedTransaction](OrderDate);
CREATE INDEX [IX_FailedTransaction_PaymentGateway] ON [FailedTransaction](PaymentGateway);
CREATE UNIQUE INDEX [UIX_Feed_FeedGUID] ON [Feed](FeedGUID);
CREATE INDEX [IX_Feed_DisplayOrder] ON [Feed](DisplayOrder);
CREATE INDEX [IX_Feed_DisplayOrder_Name] ON [Feed](DisplayOrder, Name);
CREATE UNIQUE INDEX [UIX_Genre_GenreGUID] ON [Genre](GenreGUID);
CREATE INDEX [IX_Genre_Name] ON [Genre](Name);
CREATE INDEX [IX_Genre_DisplayOrder_Name] ON [Genre](DisplayOrder, Name);
CREATE UNIQUE INDEX [UIX_GiftCard_GiftCardGUID] ON [GiftCard](GiftCardGUID);
CREATE INDEX [IX_GiftCard_SerialNumber] ON [GiftCard](SerialNumber);
CREATE INDEX [IX_GiftCard_ExpirationDate] ON [GiftCard](ExpirationDate);
CREATE INDEX [IX_GiftCard_CreatedOn] ON [GiftCard](CreatedOn);
CREATE INDEX [IX_GiftCard_PurchasedByCustomerID] ON [GiftCard](PurchasedByCustomerID);
CREATE UNIQUE INDEX [UIX_GiftCardUsage_GiftCardUsageGUID] ON [GiftCardUsage](GiftCardUsageGUID);
CREATE INDEX [IX_GiftCardUsage_GiftCardID] ON [GiftCardUsage](GiftCardID);
CREATE INDEX [IX_GiftCardUsage_UsedByCustomerID] ON [GiftCardUsage](UsedByCustomerID);
CREATE UNIQUE INDEX [UIX_Inventory_InventoryGUID] ON [Inventory](InventoryGUID);
CREATE INDEX [IX_Inventory_VariantID_Color_Size] ON [Inventory](VariantID, Color, Size);
CREATE INDEX [IX_KitCart_CreatedOn] ON [KitCart](CreatedOn);
CREATE INDEX [IX_KitCart_ShoppingCartRecID] ON [KitCart](ShoppingCartRecID);
CREATE INDEX [IX_KitCart_CustomerID_ShoppingCartRecID] ON [KitCart](CustomerID, ShoppingCartRecID);
CREATE INDEX [IX_KitCart_ProductID] ON [KitCart](ProductID);
CREATE INDEX [IX_KitCart_VariantID] ON [KitCart](VariantID);
CREATE INDEX [IX_KitCart_KitGroupID] ON [KitCart](KitGroupID);
CREATE INDEX [IX_KitCart_KitItemID] ON [KitCart](KitItemID);
CREATE UNIQUE INDEX [UIX_KitGroup_KitGroupGUID] ON [KitGroup](KitGroupGUID);
CREATE INDEX [IX_KitGroup_ProductID] ON [KitGroup](ProductID);
CREATE INDEX [IX_KitGroup_ProductID_DisplayOrder] ON [KitGroup](ProductID, DisplayOrder);
CREATE INDEX [IX_KitGroup_DisplayOrder] ON [KitGroup](DisplayOrder);
CREATE UNIQUE INDEX [UIX_KitGroupType_KitGroupTypeGUID] ON [KitGroupType](KitGroupTypeGUID);
CREATE INDEX [IX_KitGroupType_DisplayOrder] ON [KitGroupType](DisplayOrder);
CREATE INDEX [IX_KitGroupType_DisplayOrder_Name] ON [KitGroupType](DisplayOrder, Name);
CREATE INDEX [IX_KitGroupType_Name] ON [KitGroupType](Name);
CREATE UNIQUE INDEX [UIX_KitItem_KitItemGUID] ON [KitItem](KitItemGUID);
CREATE INDEX [IX_KitItem_KitGroupID] ON [KitItem](KitGroupID);
CREATE INDEX [IX_KitItem_KitGroupID_DisplayOrder] ON [KitItem](KitGroupID, DisplayOrder);
CREATE INDEX [IX_KitItem_DisplayOrder] ON [KitItem](DisplayOrder);
CREATE INDEX [IX_KitItem_DisplayOrder_Name] ON [KitItem](DisplayOrder, Name);
CREATE INDEX [IX_KitItem_Name] ON [KitItem](Name);
CREATE INDEX [IX_Library_Deleted] ON [Library](Deleted);
CREATE INDEX [IX_Library_Published] ON [Library](Published);
CREATE INDEX [IX_Library_Wholesale] ON [Library](Wholesale);
CREATE INDEX [IX_Library_LibraryGUID] ON [Library](LibraryGUID);
CREATE INDEX [IX_Library_ParentLibraryID] ON [Library](ParentLibraryID);
CREATE INDEX [IX_Library_DisplayOrder] ON [Library](DisplayOrder);
CREATE INDEX [IX_Library_Deleted_Published] ON [Library](Deleted, Published);
CREATE INDEX [IX_Library_Deleted_Wholesale] ON [Library](Deleted, Wholesale);
CREATE INDEX [IX_Library_Name] ON [Library](Name);
CREATE INDEX [IX_Library_DisplayOrder_Name] ON [Library](DisplayOrder, Name);
CREATE UNIQUE INDEX [UIX_Locale_LocaleSettingGUID] ON [LocaleSetting](LocaleSettingGUID);
CREATE UNIQUE INDEX [UIX_Locale_Name] ON [LocaleSetting](Name);
CREATE INDEX [IX_Locale_DisplayOrder_Name] ON [LocaleSetting](DisplayOrder, Name);
CREATE INDEX [IX_Locale_DisplayOrder] ON [LocaleSetting](DisplayOrder);
CREATE UNIQUE INDEX [UIX_Manufacturer_ManufacturerGUID] ON [Manufacturer](ManufacturerGUID);
CREATE INDEX [IX_Manufacturer_Deleted] ON [Manufacturer](Deleted);
CREATE INDEX [IX_Manufacturer_DisplayOrder] ON [Manufacturer](DisplayOrder);
CREATE INDEX [IX_Manufacturer_Name] ON [Manufacturer](Name);
CREATE INDEX [IX_Manufacturer_DisplayOrder_Name] ON [Manufacturer](DisplayOrder, Name);
CREATE UNIQUE INDEX [UIX_News_NewsGUID] ON [News](NewsGUID);
CREATE INDEX [IX_News_ExpiresOn] ON [News](ExpiresOn DESC);
CREATE INDEX [IX_News_Deleted] ON [News](Deleted);
CREATE INDEX [IX_News_Published] ON [News](Published);
CREATE INDEX [IX_News_Wholesale] ON [News](Wholesale);
CREATE UNIQUE INDEX [UIX_OrderNumbers_OrderNumberGUID] ON [OrderNumbers](OrderNumberGUID);
CREATE INDEX [IX_OrderNumbers_CreatedOn] ON [OrderNumbers](CreatedOn);
CREATE INDEX [IX_Orders_OrderNumber] ON [Orders](OrderNumber);
CREATE INDEX [IX_Orders_ParentOrderNumber] ON [Orders](ParentOrderNumber);
CREATE INDEX [IX_Orders_CustomerID] ON [Orders](CustomerID);
CREATE INDEX [IX_Orders_OrderNumber_CustomerID] ON [Orders](OrderNumber, CustomerID);
CREATE INDEX [IX_Orders_AffiliateID] ON [Orders](AffiliateID);
CREATE INDEX [IX_Orders_OrderDate] ON [Orders](OrderDate);
CREATE INDEX [IX_Orders_OrderGUID] ON [Orders](OrderGUID);
CREATE INDEX [IX_Orders_EMail] ON [Orders](Email);
CREATE INDEX [IX_Orders_IsNew] ON [Orders](IsNew);
CREATE INDEX [IX_Orders_CouponCode] ON [Orders](CouponCode);
CREATE INDEX [IX_Orders_TransactionState] ON [Orders](TransactionState);
CREATE CLUSTERED INDEX [IX_Orders_KitCart_OrderNumber] ON [Orders_KitCart](OrderNumber);
CREATE INDEX [IX_Orders_KitCart_ProductID_VariantID] ON [Orders_KitCart](ProductID, VariantID);
CREATE INDEX [IX_Orders_KitCart_CreatedOn] ON [Orders_KitCart](CreatedOn);
CREATE INDEX [IX_Orders_KitCart_KitCartRecID] ON [Orders_KitCart](KitCartRecID);
CREATE INDEX [IX_Orders_KitCart_CustomerID] ON [Orders_KitCart](CustomerID);
CREATE INDEX [IX_Orders_KitCart_ShoppingCartRecID] ON [Orders_KitCart](ShoppingCartRecID);
CREATE INDEX [IX_Orders_KitCart_KitGroupID] ON [Orders_KitCart](KitGroupID);
CREATE CLUSTERED INDEX [IX_Orders_ShoppingCart_OrderNumber_CustomerID] ON [Orders_ShoppingCart](OrderNumber, CustomerID);
CREATE INDEX [IX_Orders_ShoppingCart_OrderedProductSKU] ON [Orders_ShoppingCart](OrderedProductSKU);
CREATE INDEX [IX_Orders_ShoppingCart_CustomerID] ON [Orders_ShoppingCart](CustomerID);
CREATE INDEX [IX_Orders_ShoppingCart_ShoppingCartRecID] ON [Orders_ShoppingCart](ShoppingCartRecID);
CREATE INDEX [IX_Orders_ShoppingCart_ProductID] ON [Orders_ShoppingCart](ProductID);
CREATE INDEX [IX_Orders_ShoppingCart_ProductID_VariantID_ChosenColor_ChosenSize] ON [Orders_ShoppingCart](ProductID, VariantID, ChosenColor, ChosenSize);
CREATE INDEX [IX_Orders_ShoppingCart_CreatedOn] ON [Orders_ShoppingCart](CreatedOn);
CREATE CLUSTERED INDEX [CIX_PasswordLog] ON [PasswordLog](CustomerID, ChangeDt);
CREATE UNIQUE INDEX [UIX_Product_ProductGUID] ON [Product](ProductGUID);
CREATE INDEX [IX_Product_SKU] ON [Product](SKU);
CREATE INDEX [IX_Product_IsImport] ON [Product](IsImport);
CREATE INDEX [IX_Product_IsSystem] ON [Product](IsSystem);
CREATE INDEX [IX_Product_Published] ON [Product](Published);
CREATE INDEX [IX_Product_Wholesale] ON [Product](Wholesale);
CREATE INDEX [IX_Product_Deleted] ON [Product](Deleted);
CREATE INDEX [IX_Product_ProductTypeID] ON [Product](ProductTypeID);
CREATE INDEX [IX_Product_IsAKit] ON [Product](IsAKit);
CREATE INDEX [IX_Product_Name] ON [Product](Name);
CREATE INDEX [IX_Product_ManufacturerPartNumber] ON [Product](ManufacturerPartNumber);
CREATE INDEX [IX_Product_Published_Deleted] ON [Product](Published, Deleted);
CREATE INDEX [IX_Product_Wholesale_Deleted] ON [Product](Wholesale, Deleted);
CREATE INDEX [IX_Product_ProductID] ON [ProductCategory](ProductID);
CREATE NONCLUSTERED INDEX [IX_Product_Published_Deleted_IsAKit] ON [dbo].[Product] ([Published],[Deleted],[IsAKit]) INCLUDE ([ProductID],[Name],[ProductTypeID],[TrackInventoryBySizeAndColor])
CREATE INDEX [IX_Category_CategoryID] ON [ProductCategory](CategoryID);
CREATE INDEX [IX_ProductSection_SectionID_DisplayOrder] ON [ProductSection](SectionID, DisplayOrder);
CREATE INDEX [IX_ProductType_ProductTypeGUID] ON [ProductType](ProductTypeGUID);
CREATE INDEX [IX_ProductType_Name] ON [ProductType](Name);
CREATE INDEX [IX_ProductType_DisplayOrder] ON [ProductType](DisplayOrder);
CREATE INDEX [IX_ProductType_DisplayOrder_Name] ON [ProductType](DisplayOrder, Name);
CREATE UNIQUE INDEX [UIX_ProductVariant_VariantID] ON [ProductVariant](VariantGUID);
CREATE INDEX [IX_ProductVariant_ProductID] ON [ProductVariant](ProductID);
CREATE INDEX [IX_ProductVariant_SKUSuffix] ON [ProductVariant](SKUSuffix);
CREATE INDEX [IX_ProductVariant_ManufacturerPartNumber] ON [ProductVariant](ManufacturerPartNumber);
CREATE INDEX [IX_ProductVariant_Deleted] ON [ProductVariant](Deleted);
CREATE INDEX [IX_ProductVariant_Published] ON [ProductVariant](Published);
CREATE INDEX [IX_ProductVariant_Wholesale] ON [ProductVariant](Wholesale);
CREATE INDEX [IX_ProductVariant_Deleted_Published] ON [ProductVariant](Deleted, Published);
CREATE INDEX [IX_ProductVariant_Deleted_Wholesale] ON [ProductVariant](Deleted, Wholesale);
CREATE INDEX [IX_ProductVariant_IsDefault] ON [ProductVariant](IsDefault);
CREATE INDEX [IX_ProductVariant_DisplayOrder] ON [ProductVariant](DisplayOrder);
CREATE INDEX [IX_ProductVariant_Name] ON [ProductVariant](Name);
CREATE INDEX [IX_ProductVariant_DisplayOrder_Name] ON [ProductVariant](DisplayOrder, Name);
CREATE INDEX [IX_Profile_CustomerGuid] ON [Profile](CustomerGUID);
CREATE PRIMARY XML INDEX [XMLIX_Promotions_PromotionRuleData] ON [Promotions](PromotionRuleData)
CREATE XML INDEX [XMLIX_Promotions_PromotionRuleData_Path] ON [Promotions](PromotionRuleData) USING XML INDEX XMLIX_Promotions_PromotionRuleData FOR PATH
CREATE PRIMARY XML INDEX [XMLIX_Promotions_PromotionDiscountData] ON [Promotions](PromotionDiscountData)
CREATE XML INDEX [XMLIX_Promotions_PromotionDiscountData_Path] ON [Promotions](PromotionDiscountData) USING XML INDEX XMLIX_Promotions_PromotionDiscountData FOR PATH
CREATE UNIQUE INDEX [UIX_QuantityDiscount_QuantityDiscountGUID] ON [QuantityDiscount](QuantityDiscountGUID);
CREATE INDEX [IX_QuantityDiscount_DisplayOrder] ON [QuantityDiscount](DisplayOrder);
CREATE INDEX [IX_QuantityDiscount_DisplayOrder_Name] ON [QuantityDiscount](DisplayOrder, Name);
CREATE INDEX [IX_QuantityDiscount_Name] ON [QuantityDiscount](Name);
CREATE UNIQUE INDEX [UIX_QuantityDiscountTable_QuantityDiscountTableGUID] ON [QuantityDiscountTable](QuantityDiscountTableGUID);
CREATE INDEX [IX_QuantityDiscountTable_QuantityDiscountTableID] ON [QuantityDiscountTable](QuantityDiscountID);
CREATE INDEX [IX_QuantityDiscountTable_LowQuantity_HighQuantity] ON [QuantityDiscountTable](LowQuantity, HighQuantity);
CREATE INDEX [IX_Rating_FoundNotHelpful] ON [Rating](FoundNotHelpful);
CREATE INDEX [IX_Rating_CreatedOn] ON [Rating](CreatedOn);
CREATE INDEX [IX_Rating] ON [Rating](HasComment);
CREATE INDEX [IX_Rating_ProductID] ON [Rating](ProductID);
CREATE INDEX [IX_Rating_CustomerID] ON [Rating](CustomerID);
CREATE INDEX [IX_Rating_IsROTD] ON [Rating](IsROTD);
CREATE INDEX [IX_Rating_FoundHelpful] ON [Rating](FoundHelpful);
CREATE INDEX [IX_Rating_IsFilthy] ON [Rating](IsFilthy);
CREATE INDEX [IX_RatingCommentHelpfulness_StoreID] ON [RatingCommentHelpfulness](StoreID);
CREATE INDEX [IX_RatingCommentHelpfulness_VotingCustomerID] ON [RatingCommentHelpfulness](VotingCustomerID);
CREATE INDEX [IX_RatingCommentHelpfulness_ProductID] ON [RatingCommentHelpfulness](ProductID);
CREATE INDEX [IX_RatingCommentHelpfulness_RatingCustomerID] ON [RatingCommentHelpfulness](RatingCustomerID);
CREATE INDEX [IX_RatingCommentHelpfulness_Helpful] ON [RatingCommentHelpfulness](Helpful);
CREATE UNIQUE INDEX [UIX_SalesPrompt_SalesPromptGUID] ON [SalesPrompt](SalesPromptGUID);
CREATE INDEX [IX_SalesPrompt_Deleted] ON [SalesPrompt](Deleted);
CREATE INDEX [IX_SalesPrompt_Name] ON [SalesPrompt](Name);
CREATE UNIQUE INDEX [UIX_Section_SectionGUID] ON [Section](SectionGUID);
CREATE INDEX [IX_Section_ParentSectionID] ON [Section](ParentSectionID);
CREATE INDEX [IX_Section_DisplayOrder] ON [Section](DisplayOrder);
CREATE INDEX [IX_Section_Name] ON [Section](Name);
CREATE INDEX [IX_Section_DisplayOrder_Name] ON [Section](DisplayOrder, Name);
CREATE INDEX [IX_Section_Published] ON [Section](Published);
CREATE INDEX [IX_Section_Wholesale] ON [Section](Wholesale);
CREATE INDEX [IX_Section_Deleted] ON [Section](Deleted);
CREATE INDEX [IX_Section_Deleted_Published] ON [Section](Deleted, Published);
CREATE INDEX [IX_Section_Deleted_Wholesale] ON [Section](Deleted, Wholesale);
CREATE UNIQUE INDEX [UIX_ShippingByProduct_ShippingByProductGUID] ON [ShippingByProduct](ShippingByProductGUID);
CREATE INDEX [IX_ShippingByTotal_ShippingMethodID_LowValue_HighValue] ON [ShippingByTotal](ShippingMethodID, LowValue, HighValue);
CREATE INDEX [IX_ShippingByTotal_RowGUID] ON [ShippingByTotal](RowGUID);
CREATE INDEX [IX_ShippingByTotalByPercent_ShippingMethodID_LowValue_HighValue] ON [ShippingByTotalByPercent](ShippingMethodID, LowValue, HighValue);
CREATE INDEX [IX_ShippingByTotalByPercent_RowGUID] ON [ShippingByTotalByPercent](RowGUID);
CREATE INDEX [IX_ShippingByWeight_ShippingMethodID_LowValue_HighValue] ON [ShippingByWeight](ShippingMethodID, LowValue, HighValue);
CREATE INDEX [IX_ShippingByWeight_RowGUID] ON [ShippingByWeight](RowGUID);
CREATE UNIQUE INDEX [UIX_ShippingCalculation_ShippingCalculationID] ON [ShippingCalculation](ShippingCalculationGUID);
CREATE INDEX [IX_ShippingCalculation_DisplayOrder] ON [ShippingCalculation](DisplayOrder);
CREATE INDEX [IX_ShippingCalculation_Name] ON [ShippingCalculation](Name);
CREATE INDEX [IX_ShippingCalculation_DisplayOrder_Name] ON [ShippingCalculation](DisplayOrder, Name);
CREATE UNIQUE INDEX [UIX_ShippingMethod_ShippingMethodGUID] ON [ShippingMethod](ShippingMethodGUID);
CREATE INDEX [IX_ShippingMethod_IsRTShipping] ON [ShippingMethod](IsRTShipping);
CREATE INDEX [IX_ShippingMethod_DisplayOrder] ON [ShippingMethod](DisplayOrder);
CREATE INDEX [IX_ShippingTotalByZone_RowGUID] ON [ShippingTotalByZone](RowGUID);
CREATE INDEX [IX_ShippingTotalByZone_ShippingZoneID_LowValue_HighValue] ON [ShippingTotalByZone](ShippingZoneID, LowValue, HighValue);
CREATE INDEX [IX_ShippingWeightByZone_RowGUID] ON [ShippingWeightByZone](RowGUID);
CREATE INDEX [IX_ShippingWeightByZone_ShippingZoneID_LowValue_HighValue] ON [ShippingWeightByZone](ShippingZoneID, LowValue, HighValue);
CREATE UNIQUE INDEX [UIX_ShippingZone_ShippingZoneGUID] ON [ShippingZone](ShippingZoneGUID);
CREATE INDEX [IX_ShippingZone_DisplayOrder] ON [ShippingZone](DisplayOrder);
CREATE INDEX [IX_ShippingZone_Deleted] ON [ShippingZone](Deleted);
CREATE UNIQUE INDEX [UIX_ShoppingCart_ShoppingCartRecGUID] ON [ShoppingCart](ShoppingCartRecGUID);
CREATE INDEX [IX_ShoppingCart_CustomerID] ON [ShoppingCart](CustomerID);
CREATE INDEX [IX_ShoppingCart_CustomerID_CartType] ON [ShoppingCart](CustomerID, CartType);
CREATE INDEX [IX_ShoppingCart_ProductID] ON [ShoppingCart](ProductID);
CREATE INDEX [IX_ShoppingCart_VariantID] ON [ShoppingCart](VariantID);
CREATE INDEX [IX_ShoppingCart_ProductID_VariantID_ChosenColor_ChosenSize] ON [ShoppingCart](ProductID, VariantID, ChosenColor, ChosenSize);
CREATE INDEX [IX_ShoppingCart_CreatedOn] ON [ShoppingCart](CreatedOn);
CREATE INDEX [IX_ShoppingCart_CartType] ON [ShoppingCart](CartType);
CREATE INDEX [IX_ShoppingCart_CartType_RecurringSubscriptionID] ON [ShoppingCart](CartType, RecurringSubscriptionID);
CREATE INDEX [IX_ShoppingCart_NextRecurringShipDate] ON [ShoppingCart](NextRecurringShipDate);
CREATE INDEX [IX_ShoppingCart_RecurringIndex] ON [ShoppingCart](RecurringIndex);
CREATE UNIQUE INDEX [UIX_State_StateGUID] ON [State](StateGUID);
CREATE UNIQUE INDEX [UIX_State_Country_Abbrv] ON [State](CountryID, Abbreviation);
CREATE INDEX [IX_State_DisplayOrder] ON [State](DisplayOrder);
CREATE INDEX [IX_State_Name] ON [State](Name);
CREATE INDEX [IX_State_DisplayOrder_Name] ON [State](DisplayOrder, Name);
CREATE INDEX [IX_State_CountryID] ON [State](CountryID);
CREATE UNIQUE INDEX [UIX_StateTaxRate_StateID_TaxClassID] ON [StateTaxRate](StateID, TaxClassID);
CREATE UNIQUE INDEX [UIX_StringResource_Name_LocaleSetting_StoreId] ON [StringResource](Name, LocaleSetting, StoreID);
CREATE UNIQUE INDEX [UIX_Topic_TopicGUID] ON [Topic](TopicGUID);
CREATE INDEX [IX_Topic_Deleted] ON [Topic](Deleted);
CREATE INDEX [IX_Topic_ShowInSiteMap] ON [Topic](ShowInSiteMap);
CREATE UNIQUE INDEX [UIX_Vector_VectorGUID] ON [Vector](VectorGUID);
CREATE INDEX [IX_Vector_Name] ON [Vector](Name);
CREATE INDEX [IX_Vector_DisplayOrder_Name] ON [Vector](DisplayOrder, Name);
CREATE UNIQUE INDEX [UIX_ZipTaxRate_ZipCode_TaxClassID_CountryID] ON [ZipTaxRate](ZipCode, TaxClassID, CountryID);

END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].aspdnsf_EditOrder') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].aspdnsf_EditOrder
GO

create proc dbo.aspdnsf_EditOrder
    @OrderNumber int

AS
SET NOCOUNT ON

DECLARE @custid int, @custlvl int

SELECT @custid = customerid from dbo.orders with (nolock) where OrderNumber = @OrderNumber
SELECT @custlvl = CustomerLevelID FROM dbo.Customer with (nolock) WHERE customerid = @custid

DELETE dbo.shoppingcart where customerid = @custid and carttype = 0
DELETE dbo.KitCart where customerid = @custid and carttype = 0

INSERT dbo.ShoppingCart(ShoppingCartRecGUID, CustomerID, ProductSKU, ProductPrice, ProductWeight, ProductID, VariantID, Quantity, ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, DistributorID, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, CreatedOn, ProductDimensions, CartType, IsSecureAttachment, TextOption,ShippingAddressID, IsUpsell, CustomerEntersPrice, IsAKit, IsSystem, TaxClassID, TaxRate, RequiresCount)
SELECT  newid(), os.CustomerID, os.OrderedProductSKU, case when isnull(pv.saleprice, 0) = 0 then  pv.Price else pv.saleprice end, pv.Weight, os.ProductID, os.VariantID, os.Quantity, os.ChosenColor, os.ChosenColorSKUModifier, os.ChosenSize, os.ChosenSizeSKUModifier, os.DistributorID, os.IsTaxable, os.IsShipSeparately, os.IsDownload, os.DownloadLocation, os.FreeShipping, getdate(), pv.Dimensions, 0, os.IsSecureAttachment, os.TextOption,os.ShippingAddressID, 0, os.CustomerEntersPrice, os.IsAKit, os.IsSystem, os.TaxClassID, os.TaxRate, ShoppingCartRecID
FROM dbo.orders_shoppingcart os with (NOLOCK)
    join dbo.product p with (NOLOCK) on os.productid = p.productid
    join dbo.productvariant pv with (NOLOCK) on os.variantid = pv.variantid
WHERE os.OrderNumber = @OrderNumber

INSERT dbo.KitCart(CartType, CreatedOn, CustomerID, ExtensionData, InventoryVariantColor, InventoryVariantID, InventoryVariantSize, KitGroupID, KitGroupTypeID, KitItemID, ProductID, Quantity, ShoppingCartRecID, TextOption, VariantID)
SELECT 0, getdate(), kc.CustomerID, kc.ExtensionData, kc.InventoryVariantColor, kc.InventoryVariantID, kc.InventoryVariantSize, kc.KitGroupID, kc.KitGroupTypeID, kc.KitItemID, kc.ProductID, kc.Quantity, s.ShoppingCartRecID, kc.TextOption, kc.VariantID
FROM dbo.orders_KitCart kc with (NOLOCK)
    join dbo.orders_shoppingcart os with (NOLOCK) on kc.ShoppingCartRecID = os.ShoppingCartRecID
    join dbo.ShoppingCart s with (NOLOCK) on os.ShoppingCartRecID = s.RequiresCount
WHERE os.OrderNumber = @OrderNumber

UPDATE ShoppingCart SET RequiresCount = 0 WHERE customerid = @custid and carttype = 0

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].aspdnsf_GetShoppingCart') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].aspdnsf_GetShoppingCart
GO

create proc [dbo].[aspdnsf_GetShoppingCart]
    @CartType tinyint, -- ShoppingCart = 0, WishCart = 1, RecurringCart = 2
    @CustomerID int,
    @OriginalRecurringOrderNumber int,
    @OnlyLoadRecurringItemsThatAreDue tinyint,
    @StoreID int = 1

AS
BEGIN
    SET NOCOUNT ON
    declare @FilterShoppingCart bit, @FilterProduct bit
	SELECT TOP 1 @FilterShoppingCart = ConfigValue FROM GlobalConfig WHERE Name='AllowShoppingcartFiltering'
    SELECT TOP 1 @FilterProduct = ConfigValue FROM GlobalConfig WHERE Name='AllowProductFiltering'

    SELECT
        ShoppingCart.ProductSKU,
        ShoppingCart.IsUpsell,
        ShoppingCart.Notes,
        ShoppingCart.ExtensionData,
        ShoppingCart.CustomerEntersPrice,
        ShoppingCart.NextRecurringShipDate,
        ShoppingCart.RecurringIndex,
        ShoppingCart.OriginalRecurringOrderNumber,
        ShoppingCart.RecurringSubscriptionID,
        ShoppingCart.CartType,
        ShoppingCart.ProductPrice,
        ShoppingCart.ProductWeight,
        ShoppingCart.ProductDimensions,
        ShoppingCart.ShoppingCartRecID,
        ShoppingCart.ProductID,
        ShoppingCart.VariantID,
        ShoppingCart.Quantity,
        ShoppingCart.IsTaxable,
        ShoppingCart.TaxClassID,
        ShoppingCart.TaxRate,
        ShoppingCart.IsShipSeparately,
        ShoppingCart.ChosenColor,
        ShoppingCart.ChosenColorSKUModifier,
        ShoppingCart.ChosenSize,
        ShoppingCart.ChosenSizeSKUModifier,
        ShoppingCart.TextOption,
        ShoppingCart.IsDownload,
        ShoppingCart.FreeShipping,
        ShoppingCart.DistributorID,
        ShoppingCart.DownloadLocation,
        ShoppingCart.CreatedOn,
        ShoppingCart.BillingAddressID as ShoppingCartBillingAddressID,
        ShoppingCart.ShippingAddressID as ShoppingCartShippingAddressID,
        ShoppingCart.ShippingMethodID,
        ShoppingCart.ShippingMethod,
        ShoppingCart.RequiresCount,
        ShoppingCart.IsSystem,
        ShoppingCart.IsAKit,
        ShoppingCart.IsGift,
        Customer.EMail,
        Customer.OrderOptions,
        Customer.OrderNotes,
        Customer.FinalizationData,
        Customer.CouponCode,
        Customer.ShippingAddressID as
        CustomerShippingAddressID,
        Customer.BillingAddressID as CustomerBillingAddressID,
        Product.Name as ProductName,
        Product.IsSystem,
        ProductVariant.name as VariantName,
        Product.TextOptionPrompt,
        Product.SizeOptionPrompt,
        Product.ColorOptionPrompt,
        ProductVariant.CustomerEntersPricePrompt,
        Product.ProductTypeId,
        Product.TaxClassId,
        Product.ManufacturerPartNumber,
        Product.ImageFilenameOverride,
        Product.SEName,
        Product.Deleted,
        ProductVariant.Weight,
        case @CartType when 2 then ShoppingCart.RecurringInterval else productvariant.RecurringInterval end RecurringInterval,
        case @CartType when 2 then ShoppingCart.RecurringIntervalType else productvariant.RecurringIntervalType end RecurringIntervalType

    FROM dbo.Customer with (NOLOCK)
        join dbo.ShoppingCart with (NOLOCK) ON Customer.CustomerID = ShoppingCart.CustomerID
        join dbo.Product with (NOLOCK) on ShoppingCart.ProductID=Product.ProductID
        left join dbo.ProductVariant with (NOLOCK) on ShoppingCart.VariantID=ProductVariant.VariantID
        left join dbo.Address with (NOLOCK) on Customer.ShippingAddressID=Address.AddressID
		inner join (select distinct a.ProductID,a.StoreID from ShoppingCart a with (nolock) left join ProductStore b with (nolock) on a.ProductID = b.ProductID and (@FilterProduct = 0 or a.StoreID = b.StoreID)) productstore
        on ShoppingCart.ProductID = productstore.ProductID and ShoppingCart.StoreID = productstore.StoreID

    WHERE ShoppingCart.CartType = @CartType
        and Product.Deleted in (0,2)
        and ProductVariant.Deleted = 0
        and Customer.customerid = @CustomerID
        and (@OriginalRecurringOrderNumber = 0 or ShoppingCart.OriginalRecurringOrderNumber = @OriginalRecurringOrderNumber)
        and (@OnlyLoadRecurringItemsThatAreDue = 0 or (@CartType = 2 and NextRecurringShipDate < dateadd(dy, 1, getdate())))
        and (@FilterShoppingCart = 0 or ShoppingCart.StoreID = @StoreID)
END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].aspdnsf_MoveToShoppingCart') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].aspdnsf_MoveToShoppingCart
GO

create proc [dbo].[aspdnsf_MoveToShoppingCart]
    @ShoppingCartRecId int,
    @CartType int

AS
SET NOCOUNT ON

DECLARE @custid int, @pid int, @vid int, @isakit tinyint, @color nvarchar(200), @size nvarchar(200), @text nvarchar(4000), @qty int

select @custid = s.Customerid, @pid = s.ProductID, @vid = variantid, @color = s.ChosenColor, @size = s.ChosenSize, @qty = s.Quantity, @text = convert(nvarchar(4000), TextOption), @isakit = p.IsAKit
from dbo.shoppingcart s with (nolock)
    join dbo.Product p with (nolock) on s.ProductID = p.ProductID
where s.ShoppingCartRecId = @ShoppingCartRecId and s.CartType = @CartType

if @isakit = 0 begin
    if exists (select * from dbo.shoppingcart with (nolock) where CustomerID=@custid and carttype = 0 and productid = @pid and variantid = @vid and ChosenColor = @color and ChosenSize = @size and convert(nvarchar(4000), TextOption) = @text) begin
        update dbo.shoppingcart set Quantity = Quantity + @qty,CreatedOn=getdate() where CustomerID=@custid and carttype = 0 and productid = @pid and variantid = @vid and ChosenColor = @color and ChosenSize = @size and convert(nvarchar(4000), TextOption) = @text
        delete dbo.shoppingcart where ShoppingCartRecId = @ShoppingCartRecId and CartType = @CartType
    end
    else begin
        update dbo.ShoppingCart set CartType = 0,CreatedOn=getdate() where ShoppingCartRecId = @ShoppingCartRecId and CartType = @CartType
    end
end
else begin
    update dbo.ShoppingCart set CartType = 0,CreatedOn=getdate() where ShoppingCartRecId = @ShoppingCartRecId and CartType = @CartType
    update dbo.KitCart set CartType = 0,CreatedOn=getdate() where ShoppingCartRecId = @ShoppingCartRecId and CartType = @CartType
end

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].aspdnsf_ReloadCart') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].aspdnsf_ReloadCart
GO

CREATE proc [dbo].[aspdnsf_ReloadCart]
    @CartXML nvarchar(max)

AS
BEGIN
    SET NOCOUNT ON

    DECLARE @tmpShoppingCart TABLE (                                                                             [CustomerID] [int] NOT NULL , [ProductSKU] [nvarchar] (100) NULL , [ProductPrice] [money] NULL , [ProductWeight] [money] NULL , [ProductID] [int] NOT NULL , [VariantID] [int] NOT NULL , [Quantity] [int] NOT NULL , [ChosenColor] [nvarchar] (100) NULL , [ChosenColorSKUModifier] [nvarchar] (50) NULL , [ChosenSize] [nvarchar] (100) NULL , [ChosenSizeSKUModifier] [nvarchar] (50) NULL , [IsTaxable] [tinyint] NOT NULL , [IsShipSeparately] [tinyint] NOT NULL , [IsDownload] [tinyint] NOT NULL , [DownloadLocation] [nvarchar](max) NULL , [CreatedOn] [datetime] NOT NULL , [ProductDimensions] [nvarchar] (100) NULL , [CartType] [int] NOT NULL , [IsSecureAttachment] [tinyint] NOT NULL , [TextOption] [nvarchar](max) NULL , [NextRecurringShipDate] [datetime] NULL , [RecurringIndex] [int] NOT NULL , [OriginalRecurringOrderNumber] [int] NULL , [RecurringSubscriptionID] [nvarchar](100) NOT NULL, [BillingAddressID] [int] NULL , [ShippingAddressID] [int] NULL , [ShippingMethodID] [int] NULL , [ShippingMethod] [nvarchar] (max) NULL , [DistributorID] [int] NULL , [Notes] [nvarchar] (max) NULL , [IsUpsell] [tinyint] NOT NULL , [RecurringInterval] [int] NOT NULL , [RecurringIntervalType] [int] NOT NULL , [ExtensionData] [nvarchar] (max) NULL, [FreeShipping] [tinyint] NOT NULL, [CustomerEntersPrice] [tinyint] NOT NULL, [IsAKit] [tinyint] NULL)
    DECLARE @tmpShoppingCart2 TABLE (oldCartID int not null,  [ShoppingCartRecGUID] [uniqueidentifier] NOT NULL, [CustomerID] [int] NOT NULL , [ProductSKU] [nvarchar] (100) NULL , [ProductPrice] [money] NULL , [ProductWeight] [money] NULL , [ProductID] [int] NOT NULL , [VariantID] [int] NOT NULL , [Quantity] [int] NOT NULL , [ChosenColor] [nvarchar] (100) NULL , [ChosenColorSKUModifier] [nvarchar] (50) NULL , [ChosenSize] [nvarchar] (100) NULL , [ChosenSizeSKUModifier] [nvarchar] (50) NULL , [IsTaxable] [tinyint] NOT NULL , [IsShipSeparately] [tinyint] NOT NULL , [IsDownload] [tinyint] NOT NULL , [DownloadLocation] [nvarchar](max) NULL , [CreatedOn] [datetime] NOT NULL , [ProductDimensions] [nvarchar] (100) NULL , [CartType] [int] NOT NULL , [IsSecureAttachment] [tinyint] NOT NULL , [TextOption] [nvarchar](max) NULL , [NextRecurringShipDate] [datetime] NULL , [RecurringIndex] [int] NOT NULL , [OriginalRecurringOrderNumber] [int] NULL , [RecurringSubscriptionID] [nvarchar](100) NOT NULL, [BillingAddressID] [int] NULL , [ShippingAddressID] [int] NULL , [ShippingMethodID] [int] NULL , [ShippingMethod] [nvarchar] (max) NULL , [DistributorID] [int] NULL , [Notes] [nvarchar] (max) NULL , [IsUpsell] [tinyint] NOT NULL , [RecurringInterval] [int] NOT NULL , [RecurringIntervalType] [int] NOT NULL , [ExtensionData] [nvarchar] (max) NULL, [FreeShipping] [tinyint] NOT NULL, [CustomerEntersPrice] [tinyint] NOT NULL, [IsAKit] [tinyint] NULL)
    DECLARE @tmpCart TABLE (cartid int not null, addressid int not null, qty  int not null)
    DECLARE @tmp1 TABLE ( [CustomerID] [int] NOT NULL , [CartType] [int] NOT NULL , [ProductID] [int] NOT NULL , [VariantID] [int] NOT NULL , [ChosenColor] [nvarchar] (100) NULL , [ChosenSize] [nvarchar] (100) NULL ,[TextOption] [nvarchar](max) NULL , [ShippingAddressID] [int] NULL , [Qty] [int] NOT NULL )
    DECLARE @tmp2 TABLE ([cartid] [int] NOT NULL )

    DECLARE @hdoc int, @retcode int
    EXEC @retcode = sp_xml_preparedocument
                        @hdoc OUTPUT,
                        @CartXML

    INSERT @tmpCart (cartid, addressid, qty)
    SELECT cartid, addressid, count(*)
    FROM OPENXML(@hdoc, '/root/row', 0) WITH (cartid int, addressid int)
    GROUP BY cartid, addressid

    DECLARE @custid int, @carttype int

    SELECT top 1 @custid = CustomerID, @carttype = CartType
    FROM dbo.ShoppingCart s with (nolock)
            join @tmpCart c on s.ShoppingCartRecID = c.cartid


    --creates cart item/shipping address combinations
    INSERT @tmpShoppingCart
    SELECT CustomerID, ProductSKU, ProductPrice, ProductWeight, ProductID,VariantID, c.qty quantity, ChosenColor, ChosenColorSKUModifier, ChosenSize,ChosenSizeSKUModifier, IsTaxable, IsShipSeparately, IsDownload,DownloadLocation, CreatedOn, ProductDimensions, CartType,IsSecureAttachment, TextOption, NextRecurringShipDate, RecurringIndex,OriginalRecurringOrderNumber, RecurringSubscriptionID, BillingAddressID,c.addressid ShippingAddressID, ShippingMethodID, ShippingMethod,DistributorID, Notes,IsUpsell, RecurringInterval,RecurringIntervalType, ExtensionData, s.FreeShipping, s.CustomerEntersPrice, s.IsAKit
    FROM dbo.ShoppingCart s
        join @tmpCart c on s.ShoppingCartRecID = c.cartid



    -- combines like items based on the fields in the group by clause
    INSERT @tmp1
    SELECT customerid, carttype, productid, variantid, chosencolor, chosensize, convert(nvarchar(4000), textoption) textoption, shippingaddressid, sum(quantity)
    FROM @tmpShoppingCart
    GROUP BY customerid,carttype,productid,variantid,chosencolor,chosensize,convert(nvarchar(4000), textoption),shippingaddressid


    -- gets original cartID for restricting new cart items
    INSERT @tmp2
    SELECT min(ShoppingCartRecID) cartid
    FROM dbo.ShoppingCart
    WHERE customerid = @custid and carttype = @carttype
    GROUP BY customerid, carttype, productid, variantid, chosencolor, chosensize, convert(nvarchar(4000), textoption)


    -- create new cart records
    INSERT @tmpShoppingCart2 (oldCartID, ShoppingCartRecGUID, CustomerID,ProductSKU, ProductPrice, ProductWeight, ProductID, VariantID, Quantity,ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier,IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, CreatedOn,ProductDimensions, CartType, IsSecureAttachment, TextOption,NextRecurringShipDate, RecurringIndex, OriginalRecurringOrderNumber,RecurringSubscriptionID, BillingAddressID, ShippingAddressID,ShippingMethodID, ShippingMethod, DistributorID, Notes, IsUpsell, RecurringInterval, RecurringIntervalType, ExtensionData, FreeShipping, CustomerEntersPrice, IsAKit)
    SELECT c.cartid, newid(), s.CustomerID, s.ProductSKU, s.ProductPrice,s.ProductWeight, s.ProductID, s.VariantID, a.qty, s.ChosenColor,s.ChosenColorSKUModifier, s.ChosenSize, s.ChosenSizeSKUModifier,s.IsTaxable, s.IsShipSeparately, s.IsDownload, s.DownloadLocation,s.CreatedOn, s.ProductDimensions, s.CartType, s.IsSecureAttachment,s.TextOption, s.NextRecurringShipDate, s.RecurringIndex,s.OriginalRecurringOrderNumber, s.RecurringSubscriptionID,s.BillingAddressID, a.shippingaddressid, s.ShippingMethodID,s.ShippingMethod, s.DistributorID, '', s.IsUpsell, s.RecurringInterval, s.RecurringIntervalType, s.ExtensionData, s.FreeShipping, s.CustomerEntersPrice, s.IsAKit
    FROM dbo.ShoppingCart s
        join @tmp1 a
            on s.customerid = a.customerid and
               s.carttype = a.carttype and
               s.productid = a.productid and
               s.variantid = a.variantid and
               s.chosencolor = a.chosencolor and
               s.chosensize = a.chosensize and
               convert(nvarchar(4000), s.textoption)  = convert(nvarchar(4000), a.textoption)
        join @tmp2 c on s.ShoppingCartRecID = c.cartid


    BEGIN TRAN
        INSERT [dbo].ShoppingCart (ShoppingCartRecGUID, CustomerID,ProductSKU, ProductPrice, ProductWeight, ProductID, VariantID, Quantity,ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier,IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, CreatedOn,ProductDimensions, CartType, IsSecureAttachment, TextOption,NextRecurringShipDate, RecurringIndex, OriginalRecurringOrderNumber,RecurringSubscriptionID, BillingAddressID, ShippingAddressID,ShippingMethodID, ShippingMethod, DistributorID, Notes, IsUpsell, RecurringInterval, RecurringIntervalType, ExtensionData, FreeShipping, CustomerEntersPrice, IsAKit)
        SELECT ShoppingCartRecGUID, CustomerID, ProductSKU, ProductPrice,ProductWeight, ProductID, VariantID, Quantity, ChosenColor,ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, IsTaxable,IsShipSeparately, IsDownload, DownloadLocation, CreatedOn,ProductDimensions, CartType, IsSecureAttachment, TextOption,NextRecurringShipDate, RecurringIndex, OriginalRecurringOrderNumber,RecurringSubscriptionID, BillingAddressID, ShippingAddressID,ShippingMethodID, ShippingMethod, DistributorID, Notes, IsUpsell, RecurringInterval, RecurringIntervalType, ExtensionData, FreeShipping, CustomerEntersPrice, IsAKit
        FROM @tmpShoppingCart2

        IF @@Error <>0 BEGIN
            ROLLBACK TRAN
            RAISERROR('Could not add new shopping cart records', 16, 1)
            RETURN -1
        END



        INSERT [dbo].KitCart(CustomerID, ShoppingCartRecID, ProductID,VariantID, KitGroupID, KitGroupTypeID, KitItemID, TextOption, Quantity,CartType, OriginalRecurringOrderNumber, ExtensionData, InventoryVariantID, InventoryVariantColor,InventoryVariantSize, CreatedOn)
        SELECT k.CustomerID, s.ShoppingCartRecID, k.ProductID, k.VariantID,k.KitGroupID, k.KitGroupTypeID, k.KitItemID, k.TextOption, k.Quantity,k.CartType, k.OriginalRecurringOrderNumber, k.ExtensionData, k.InventoryVariantID, k.InventoryVariantColor,k.InventoryVariantSize, k.CreatedOn
        FROM dbo.KitCart k
            join @tmpShoppingCart2 c on k.ShoppingCartRecID = c.oldCartID
            join [dbo].ShoppingCart s with (nolock) on s.ShoppingCartRecGUID = c.ShoppingCartRecGUID

        IF @@Error <>0 BEGIN
            ROLLBACK TRAN
            RAISERROR('Could not add new kit cart records', 16, 1)
            RETURN -2
        END


        DELETE dbo.ShoppingCart
        FROM dbo.ShoppingCart s
            join @tmpCart c on s.ShoppingCartRecID = c.cartid

        IF @@Error <>0 BEGIN
            ROLLBACK TRAN
            RAISERROR('Could not delete old shopping cart records', 16, 1)
            RETURN -4
        END

        DELETE dbo.KitCart
        FROM dbo.KitCart s
            join @tmpCart c on s.ShoppingCartRecID = c.cartid

        IF @@Error <>0 BEGIN
            ROLLBACK TRAN
            RAISERROR('Could not delete old kit cart records', 16, 1)
            RETURN -5
        END

    COMMIT TRAN

    exec sp_xml_removedocument @hdoc
END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].aspdnsf_UpdateIndexes') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].aspdnsf_UpdateIndexes
GO

CREATE PROC [dbo].[aspdnsf_UpdateIndexes]

AS
BEGIN

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Address]') AND name = N'UIX_Address_AddressGUID') CREATE UNIQUE INDEX [UIX_Address_AddressGUID] ON [Address](AddressGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Address]') AND name = N'IX_Address_CustomerID') CREATE INDEX [IX_Address_CustomerID] ON [Address](CustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Address]') AND name = N'IX_Address_Deleted') CREATE INDEX [IX_Address_Deleted] ON [Address](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateActivity]') AND name = N'IX_AffiliateActivity_AffiliateID') CREATE INDEX [IX_AffiliateActivity_AffiliateID] ON [AffiliateActivity](AffiliateID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateActivity]') AND name = N'IX_AffiliateActivity_AffiliateID_ActivityDate') CREATE INDEX [IX_AffiliateActivity_AffiliateID_ActivityDate] ON [AffiliateActivity](AffiliateID, ActivityDate);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateCommissions]') AND name = N'IX_AffiliateCommissions_RowGUID') CREATE INDEX [IX_AffiliateCommissions_RowGUID] ON [AffiliateCommissions](RowGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateCommissions]') AND name = N'IX_AffiliateCommissions_LowValue') CREATE INDEX [IX_AffiliateCommissions_LowValue] ON [AffiliateCommissions](LowValue);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateCommissions]') AND name = N'IX_AffiliateCommissions_HighValue') CREATE INDEX [IX_AffiliateCommissions_HighValue] ON [AffiliateCommissions](HighValue);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AppConfig]') AND name = N'UIX_AppConfig_Name_StoreID') CREATE UNIQUE INDEX [UIX_AppConfig_Name_StoreID] ON [AppConfig](Name, StoreID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AppConfig]') AND name = N'IX_AppConfig_GroupName') CREATE INDEX [IX_AppConfig_GroupName] ON [AppConfig](GroupName);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BadWord]') AND name = N'IX_BadWord') CREATE INDEX [IX_BadWord] ON [BadWord](Word);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Category]') AND name = N'IX_Category_Name') CREATE INDEX [IX_Category_Name] ON [Category](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Category]') AND name = N'IX_Category_Deleted') CREATE INDEX [IX_Category_Deleted] ON [Category](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Category]') AND name = N'IX_Category_Published') CREATE INDEX [IX_Category_Published] ON [Category](Published);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Category]') AND name = N'IX_Category_Wholesale') CREATE INDEX [IX_Category_Wholesale] ON [Category](Wholesale);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Category]') AND name = N'IX_Category_CategoryGUID') CREATE INDEX [IX_Category_CategoryGUID] ON [Category](CategoryGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Category]') AND name = N'IX_Category_ParentCategoryID') CREATE INDEX [IX_Category_ParentCategoryID] ON [Category](ParentCategoryID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Category]') AND name = N'IX_Category_DisplayOrder') CREATE INDEX [IX_Category_DisplayOrder] ON [Category](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Category]') AND name = N'IX_Category_Deleted_Published') CREATE INDEX [IX_Category_Deleted_Published] ON [Category](Deleted, Published);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Category]') AND name = N'IX_Category_Deleted_Wholesale') CREATE INDEX [IX_Category_Deleted_Wholesale] ON [Category](Deleted, Wholesale);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Country]') AND name = N'UIX_Country_Name') CREATE UNIQUE INDEX [UIX_Country_Name] ON [Country](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Country]') AND name = N'IX_Country_DisplayOrder_Name') CREATE INDEX [IX_Country_DisplayOrder_Name] ON [Country](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Country]') AND name = N'IX_Country_CountryGUID') CREATE INDEX [IX_Country_CountryGUID] ON [Country](CountryGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Country]') AND name = N'IX_Country_DisplayOrder') CREATE INDEX [IX_Country_DisplayOrder] ON [Country](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CountryTaxRate]') AND name = N'UIX_CountryTaxRate_CountryID_TaxClassID') CREATE UNIQUE INDEX [UIX_CountryTaxRate_CountryID_TaxClassID] ON [CountryTaxRate](CountryID, TaxClassID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Coupon]') AND name = N'UIX_Coupon_CouponGUID') CREATE UNIQUE INDEX [UIX_Coupon_CouponGUID] ON [Coupon](CouponGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Coupon]') AND name = N'UIX_Coupon_CouponCode') CREATE UNIQUE INDEX [UIX_Coupon_CouponCode] ON [Coupon](CouponCode);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Coupon]') AND name = N'IX_Coupon_ExpirationDate') CREATE INDEX [IX_Coupon_ExpirationDate] ON [Coupon](ExpirationDate);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Coupon]') AND name = N'IX_Coupon_Deleted') CREATE INDEX [IX_Coupon_Deleted] ON [Coupon](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CreditCardType]') AND name = N'UIX_CreditCardTypes') CREATE UNIQUE INDEX [UIX_CreditCardTypes] ON [CreditCardType](CardTypeGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CreditCardType]') AND name = N'IX_CreditCardType_CardType') CREATE INDEX [IX_CreditCardType_CardType] ON [CreditCardType](CardType);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Currency]') AND name = N'UIX_Currency_CurrencyGUID') CREATE UNIQUE INDEX [UIX_Currency_CurrencyGUID] ON [Currency](CurrencyGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'UIX_Customer_CustomerGUID') CREATE UNIQUE INDEX [UIX_Customer_CustomerGUID] ON [Customer](CustomerGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'IX_Customer_EMail') CREATE INDEX [IX_Customer_EMail] ON [Customer](Email);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'IX_Customer_Password') CREATE INDEX [IX_Customer_Password] ON [Customer](Password);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'IX_Customer_CustomerLevelID') CREATE INDEX [IX_Customer_CustomerLevelID] ON [Customer](CustomerLevelID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'IX_Customer_IsAdmin') CREATE INDEX [IX_Customer_IsAdmin] ON [Customer](IsAdmin);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'IX_Customer_OkToEMail') CREATE INDEX [IX_Customer_OkToEMail] ON [Customer](OkToEmail);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'IX_Customer_Deleted') CREATE INDEX [IX_Customer_Deleted] ON [Customer](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'IX_Customer_AffiliateID') CREATE INDEX [IX_Customer_AffiliateID] ON [Customer](AffiliateID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'IX_Customer_CouponCode') CREATE INDEX [IX_Customer_CouponCode] ON [Customer](CouponCode);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'IX_Customer_CreatedOn') CREATE INDEX [IX_Customer_CreatedOn] ON [Customer](CreatedOn);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CustomerLevel]') AND name = N'IX_CustomerLevel_Deleted') CREATE INDEX [IX_CustomerLevel_Deleted] ON [CustomerLevel](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CustomerLevel]') AND name = N'IX_CustomerLevel_Name') CREATE INDEX [IX_CustomerLevel_Name] ON [CustomerLevel](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CustomerLevel]') AND name = N'IX_CustomerLevel_DisplayOrder') CREATE INDEX [IX_CustomerLevel_DisplayOrder] ON [CustomerLevel](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CustomerLevel]') AND name = N'IX_CustomerLevel_DisplayOrder_Name') CREATE INDEX [IX_CustomerLevel_DisplayOrder_Name] ON [CustomerLevel](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CustomerSession]') AND name = N'IX_CustomerSession_CustomerID') CREATE INDEX [IX_CustomerSession_CustomerID] ON [CustomerSession](CustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Distributor]') AND name = N'UIX_Distributor_DistributorGUID') CREATE UNIQUE INDEX [UIX_Distributor_DistributorGUID] ON [Distributor](DistributorGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Distributor]') AND name = N'IX_Distributor_DisplayOrder') CREATE INDEX [IX_Distributor_DisplayOrder] ON [Distributor](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Distributor]') AND name = N'IX_Distributor_Name') CREATE INDEX [IX_Distributor_Name] ON [Distributor](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Distributor]') AND name = N'IX_Distributor_DisplayOrder_Name') CREATE INDEX [IX_Distributor_DisplayOrder_Name] ON [Distributor](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Document]') AND name = N'UIX_Document_DocumentGUID') CREATE UNIQUE INDEX [UIX_Document_DocumentGUID] ON [Document](DocumentGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Document]') AND name = N'IX_Document_Published') CREATE INDEX [IX_Document_Published] ON [Document](Published);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Document]') AND name = N'IX_Document_Wholesale') CREATE INDEX [IX_Document_Wholesale] ON [Document](Wholesale);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Document]') AND name = N'IX_Document_Deleted') CREATE INDEX [IX_Document_Deleted] ON [Document](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Document]') AND name = N'IX_Document_DocumentTypeID') CREATE INDEX [IX_Document_DocumentTypeID] ON [Document](DocumentTypeID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Document]') AND name = N'IX_Document_Published_Deleted') CREATE INDEX [IX_Document_Published_Deleted] ON [Document](Published, Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Document]') AND name = N'IX_Document_Wholesale_Deleted') CREATE INDEX [IX_Document_Wholesale_Deleted] ON [Document](Wholesale, Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Document]') AND name = N'IX_Document_Name') CREATE INDEX [IX_Document_Name] ON [Document](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DocumentType]') AND name = N'IX_DocumentType_DocumentTypeGUID') CREATE INDEX [IX_DocumentType_DocumentTypeGUID] ON [DocumentType](DocumentTypeGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DocumentType]') AND name = N'IX_DocumentType_Name') CREATE INDEX [IX_DocumentType_Name] ON [DocumentType](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DocumentType]') AND name = N'IX_DocumentType_DisplayOrder') CREATE INDEX [IX_DocumentType_DisplayOrder] ON [DocumentType](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DocumentType]') AND name = N'IX_DocumentType_DisplayOrder_Name') CREATE INDEX [IX_DocumentType_DisplayOrder_Name] ON [DocumentType](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[EventHandler]') AND name = N'UIX_EventhHandler') CREATE UNIQUE INDEX [UIX_EventhHandler] ON [EventHandler](EventName);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ExtendedPrice]') AND name = N'UIX_ExtendedPrice_2') CREATE UNIQUE INDEX [UIX_ExtendedPrice_2] ON [ExtendedPrice](ExtendedPriceGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ExtendedPrice]') AND name = N'IX_ExtendedPrice_VariantID_CustomerLevelID') CREATE INDEX [IX_ExtendedPrice_VariantID_CustomerLevelID] ON [ExtendedPrice](VariantID, CustomerLevelID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ExtendedPrice]') AND name = N'IX_ExtendedPrice_VariantID') CREATE INDEX [IX_ExtendedPrice_VariantID] ON [ExtendedPrice](VariantID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[FailedTransaction]') AND name = N'IX_FailedTransaction_OrderDate') CREATE INDEX [IX_FailedTransaction_OrderDate] ON [FailedTransaction](OrderDate);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[FailedTransaction]') AND name = N'IX_FailedTransaction_PaymentGateway') CREATE INDEX [IX_FailedTransaction_PaymentGateway] ON [FailedTransaction](PaymentGateway);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Feed]') AND name = N'UIX_Feed_FeedGUID') CREATE UNIQUE INDEX [UIX_Feed_FeedGUID] ON [Feed](FeedGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Feed]') AND name = N'IX_Feed_DisplayOrder') CREATE INDEX [IX_Feed_DisplayOrder] ON [Feed](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Feed]') AND name = N'IX_Feed_DisplayOrder_Name') CREATE INDEX [IX_Feed_DisplayOrder_Name] ON [Feed](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Genre]') AND name = N'UIX_Genre_GenreGUID') CREATE UNIQUE INDEX [UIX_Genre_GenreGUID] ON [Genre](GenreGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Genre]') AND name = N'IX_Genre_Name') CREATE INDEX [IX_Genre_Name] ON [Genre](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Genre]') AND name = N'IX_Genre_DisplayOrder_Name') CREATE INDEX [IX_Genre_DisplayOrder_Name] ON [Genre](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND name = N'UIX_GiftCard_GiftCardGUID') CREATE UNIQUE INDEX [UIX_GiftCard_GiftCardGUID] ON [GiftCard](GiftCardGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND name = N'IX_GiftCard_SerialNumber') CREATE INDEX [IX_GiftCard_SerialNumber] ON [GiftCard](SerialNumber);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND name = N'IX_GiftCard_ExpirationDate') CREATE INDEX [IX_GiftCard_ExpirationDate] ON [GiftCard](ExpirationDate);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND name = N'IX_GiftCard_CreatedOn') CREATE INDEX [IX_GiftCard_CreatedOn] ON [GiftCard](CreatedOn);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GiftCard]') AND name = N'IX_GiftCard_PurchasedByCustomerID') CREATE INDEX [IX_GiftCard_PurchasedByCustomerID] ON [GiftCard](PurchasedByCustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GiftCardUsage]') AND name = N'UIX_GiftCardUsage_GiftCardUsageGUID') CREATE UNIQUE INDEX [UIX_GiftCardUsage_GiftCardUsageGUID] ON [GiftCardUsage](GiftCardUsageGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GiftCardUsage]') AND name = N'IX_GiftCardUsage_GiftCardID') CREATE INDEX [IX_GiftCardUsage_GiftCardID] ON [GiftCardUsage](GiftCardID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GiftCardUsage]') AND name = N'IX_GiftCardUsage_UsedByCustomerID') CREATE INDEX [IX_GiftCardUsage_UsedByCustomerID] ON [GiftCardUsage](UsedByCustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Inventory]') AND name = N'UIX_Inventory_InventoryGUID') CREATE UNIQUE INDEX [UIX_Inventory_InventoryGUID] ON [Inventory](InventoryGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Inventory]') AND name = N'IX_Inventory_VariantID_Color_Size') CREATE INDEX [IX_Inventory_VariantID_Color_Size] ON [Inventory](VariantID, Color, Size);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitCart]') AND name = N'IX_KitCart_CreatedOn') CREATE INDEX [IX_KitCart_CreatedOn] ON [KitCart](CreatedOn);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitCart]') AND name = N'IX_KitCart_ShoppingCartRecID') CREATE INDEX [IX_KitCart_ShoppingCartRecID] ON [KitCart](ShoppingCartRecID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitCart]') AND name = N'IX_KitCart_CustomerID_ShoppingCartRecID') CREATE INDEX [IX_KitCart_CustomerID_ShoppingCartRecID] ON [KitCart](CustomerID, ShoppingCartRecID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitCart]') AND name = N'IX_KitCart_ProductID') CREATE INDEX [IX_KitCart_ProductID] ON [KitCart](ProductID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitCart]') AND name = N'IX_KitCart_VariantID') CREATE INDEX [IX_KitCart_VariantID] ON [KitCart](VariantID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitCart]') AND name = N'IX_KitCart_KitGroupID') CREATE INDEX [IX_KitCart_KitGroupID] ON [KitCart](KitGroupID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitCart]') AND name = N'IX_KitCart_KitItemID') CREATE INDEX [IX_KitCart_KitItemID] ON [KitCart](KitItemID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitGroup]') AND name = N'UIX_KitGroup_KitGroupGUID') CREATE UNIQUE INDEX [UIX_KitGroup_KitGroupGUID] ON [KitGroup](KitGroupGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitGroup]') AND name = N'IX_KitGroup_ProductID') CREATE INDEX [IX_KitGroup_ProductID] ON [KitGroup](ProductID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitGroup]') AND name = N'IX_KitGroup_ProductID_DisplayOrder') CREATE INDEX [IX_KitGroup_ProductID_DisplayOrder] ON [KitGroup](ProductID, DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitGroup]') AND name = N'IX_KitGroup_DisplayOrder') CREATE INDEX [IX_KitGroup_DisplayOrder] ON [KitGroup](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitGroupType]') AND name = N'UIX_KitGroupType_KitGroupTypeGUID') CREATE UNIQUE INDEX [UIX_KitGroupType_KitGroupTypeGUID] ON [KitGroupType](KitGroupTypeGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitGroupType]') AND name = N'IX_KitGroupType_DisplayOrder') CREATE INDEX [IX_KitGroupType_DisplayOrder] ON [KitGroupType](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitGroupType]') AND name = N'IX_KitGroupType_DisplayOrder_Name') CREATE INDEX [IX_KitGroupType_DisplayOrder_Name] ON [KitGroupType](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitGroupType]') AND name = N'IX_KitGroupType_Name') CREATE INDEX [IX_KitGroupType_Name] ON [KitGroupType](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitItem]') AND name = N'UIX_KitItem_KitItemGUID') CREATE UNIQUE INDEX [UIX_KitItem_KitItemGUID] ON [KitItem](KitItemGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitItem]') AND name = N'IX_KitItem_KitGroupID') CREATE INDEX [IX_KitItem_KitGroupID] ON [KitItem](KitGroupID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitItem]') AND name = N'IX_KitItem_KitGroupID_DisplayOrder') CREATE INDEX [IX_KitItem_KitGroupID_DisplayOrder] ON [KitItem](KitGroupID, DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitItem]') AND name = N'IX_KitItem_DisplayOrder') CREATE INDEX [IX_KitItem_DisplayOrder] ON [KitItem](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitItem]') AND name = N'IX_KitItem_DisplayOrder_Name') CREATE INDEX [IX_KitItem_DisplayOrder_Name] ON [KitItem](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KitItem]') AND name = N'IX_KitItem_Name') CREATE INDEX [IX_KitItem_Name] ON [KitItem](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Library]') AND name = N'IX_Library_Deleted') CREATE INDEX [IX_Library_Deleted] ON [Library](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Library]') AND name = N'IX_Library_Published') CREATE INDEX [IX_Library_Published] ON [Library](Published);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Library]') AND name = N'IX_Library_Wholesale') CREATE INDEX [IX_Library_Wholesale] ON [Library](Wholesale);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Library]') AND name = N'IX_Library_LibraryGUID') CREATE INDEX [IX_Library_LibraryGUID] ON [Library](LibraryGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Library]') AND name = N'IX_Library_ParentLibraryID') CREATE INDEX [IX_Library_ParentLibraryID] ON [Library](ParentLibraryID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Library]') AND name = N'IX_Library_DisplayOrder') CREATE INDEX [IX_Library_DisplayOrder] ON [Library](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Library]') AND name = N'IX_Library_Deleted_Published') CREATE INDEX [IX_Library_Deleted_Published] ON [Library](Deleted, Published);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Library]') AND name = N'IX_Library_Deleted_Wholesale') CREATE INDEX [IX_Library_Deleted_Wholesale] ON [Library](Deleted, Wholesale);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Library]') AND name = N'IX_Library_Name') CREATE INDEX [IX_Library_Name] ON [Library](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Library]') AND name = N'IX_Library_DisplayOrder_Name') CREATE INDEX [IX_Library_DisplayOrder_Name] ON [Library](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LocaleSetting]') AND name = N'UIX_Locale_LocaleSettingGUID') CREATE UNIQUE INDEX [UIX_Locale_LocaleSettingGUID] ON [LocaleSetting](LocaleSettingGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LocaleSetting]') AND name = N'UIX_Locale_Name') CREATE UNIQUE INDEX [UIX_Locale_Name] ON [LocaleSetting](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LocaleSetting]') AND name = N'IX_Locale_DisplayOrder_Name') CREATE INDEX [IX_Locale_DisplayOrder_Name] ON [LocaleSetting](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LocaleSetting]') AND name = N'IX_Locale_DisplayOrder') CREATE INDEX [IX_Locale_DisplayOrder] ON [LocaleSetting](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Manufacturer]') AND name = N'UIX_Manufacturer_ManufacturerGUID') CREATE UNIQUE INDEX [UIX_Manufacturer_ManufacturerGUID] ON [Manufacturer](ManufacturerGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Manufacturer]') AND name = N'IX_Manufacturer_Deleted') CREATE INDEX [IX_Manufacturer_Deleted] ON [Manufacturer](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Manufacturer]') AND name = N'IX_Manufacturer_DisplayOrder') CREATE INDEX [IX_Manufacturer_DisplayOrder] ON [Manufacturer](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Manufacturer]') AND name = N'IX_Manufacturer_Name') CREATE INDEX [IX_Manufacturer_Name] ON [Manufacturer](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Manufacturer]') AND name = N'IX_Manufacturer_DisplayOrder_Name') CREATE INDEX [IX_Manufacturer_DisplayOrder_Name] ON [Manufacturer](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[News]') AND name = N'UIX_News_NewsGUID') CREATE UNIQUE INDEX [UIX_News_NewsGUID] ON [News](NewsGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[News]') AND name = N'IX_News_ExpiresOn') CREATE INDEX [IX_News_ExpiresOn] ON [News](ExpiresOn DESC);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[News]') AND name = N'IX_News_Deleted') CREATE INDEX [IX_News_Deleted] ON [News](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[News]') AND name = N'IX_News_Published') CREATE INDEX [IX_News_Published] ON [News](Published);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[News]') AND name = N'IX_News_Wholesale') CREATE INDEX [IX_News_Wholesale] ON [News](Wholesale);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OrderNumbers]') AND name = N'UIX_OrderNumbers_OrderNumberGUID') CREATE UNIQUE INDEX [UIX_OrderNumbers_OrderNumberGUID] ON [OrderNumbers](OrderNumberGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OrderNumbers]') AND name = N'IX_OrderNumbers_CreatedOn') CREATE INDEX [IX_OrderNumbers_CreatedOn] ON [OrderNumbers](CreatedOn);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = N'IX_Orders_OrderNumber') CREATE INDEX [IX_Orders_OrderNumber] ON [Orders](OrderNumber);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = N'IX_Orders_ParentOrderNumber') CREATE INDEX [IX_Orders_ParentOrderNumber] ON [Orders](ParentOrderNumber);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = N'IX_Orders_CustomerID') CREATE INDEX [IX_Orders_CustomerID] ON [Orders](CustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = N'IX_Orders_OrderNumber_CustomerID') CREATE INDEX [IX_Orders_OrderNumber_CustomerID] ON [Orders](OrderNumber, CustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = N'IX_Orders_AffiliateID') CREATE INDEX [IX_Orders_AffiliateID] ON [Orders](AffiliateID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = N'IX_Orders_OrderDate') CREATE INDEX [IX_Orders_OrderDate] ON [Orders](OrderDate);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = N'IX_Orders_OrderGUID') CREATE INDEX [IX_Orders_OrderGUID] ON [Orders](OrderGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = N'IX_Orders_EMail') CREATE INDEX [IX_Orders_EMail] ON [Orders](Email);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = N'IX_Orders_IsNew') CREATE INDEX [IX_Orders_IsNew] ON [Orders](IsNew);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = N'IX_Orders_CouponCode') CREATE INDEX [IX_Orders_CouponCode] ON [Orders](CouponCode);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = N'IX_Orders_TransactionState') CREATE INDEX [IX_Orders_TransactionState] ON [Orders](TransactionState);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_KitCart]') AND name = N'IX_Orders_KitCart_OrderNumber') CREATE CLUSTERED INDEX [IX_Orders_KitCart_OrderNumber] ON [Orders_KitCart](OrderNumber);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_KitCart]') AND name = N'IX_Orders_KitCart_ProductID_VariantID') CREATE INDEX [IX_Orders_KitCart_ProductID_VariantID] ON [Orders_KitCart](ProductID, VariantID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_KitCart]') AND name = N'IX_Orders_KitCart_CreatedOn') CREATE INDEX [IX_Orders_KitCart_CreatedOn] ON [Orders_KitCart](CreatedOn);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_KitCart]') AND name = N'IX_Orders_KitCart_KitCartRecID') CREATE INDEX [IX_Orders_KitCart_KitCartRecID] ON [Orders_KitCart](KitCartRecID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_KitCart]') AND name = N'IX_Orders_KitCart_CustomerID') CREATE INDEX [IX_Orders_KitCart_CustomerID] ON [Orders_KitCart](CustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_KitCart]') AND name = N'IX_Orders_KitCart_ShoppingCartRecID') CREATE INDEX [IX_Orders_KitCart_ShoppingCartRecID] ON [Orders_KitCart](ShoppingCartRecID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_KitCart]') AND name = N'IX_Orders_KitCart_KitGroupID') CREATE INDEX [IX_Orders_KitCart_KitGroupID] ON [Orders_KitCart](KitGroupID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_ShoppingCart]') AND name = N'IX_Orders_ShoppingCart_OrderNumber_CustomerID') CREATE CLUSTERED INDEX [IX_Orders_ShoppingCart_OrderNumber_CustomerID] ON [Orders_ShoppingCart](OrderNumber, CustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_ShoppingCart]') AND name = N'IX_Orders_ShoppingCart_OrderedProductSKU') CREATE INDEX [IX_Orders_ShoppingCart_OrderedProductSKU] ON [Orders_ShoppingCart](OrderedProductSKU);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_ShoppingCart]') AND name = N'IX_Orders_ShoppingCart_CustomerID') CREATE INDEX [IX_Orders_ShoppingCart_CustomerID] ON [Orders_ShoppingCart](CustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_ShoppingCart]') AND name = N'IX_Orders_ShoppingCart_ShoppingCartRecID') CREATE INDEX [IX_Orders_ShoppingCart_ShoppingCartRecID] ON [Orders_ShoppingCart](ShoppingCartRecID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_ShoppingCart]') AND name = N'IX_Orders_ShoppingCart_ProductID') CREATE INDEX [IX_Orders_ShoppingCart_ProductID] ON [Orders_ShoppingCart](ProductID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_ShoppingCart]') AND name = N'IX_Orders_ShoppingCart_ProductID_VariantID_ChosenColor_ChosenSize') CREATE INDEX [IX_Orders_ShoppingCart_ProductID_VariantID_ChosenColor_ChosenSize] ON [Orders_ShoppingCart](ProductID, VariantID, ChosenColor, ChosenSize);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Orders_ShoppingCart]') AND name = N'IX_Orders_ShoppingCart_CreatedOn') CREATE INDEX [IX_Orders_ShoppingCart_CreatedOn] ON [Orders_ShoppingCart](CreatedOn);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PasswordLog]') AND name = N'CIX_PasswordLog') CREATE CLUSTERED INDEX [CIX_PasswordLog] ON [PasswordLog](CustomerID, ChangeDt);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'UIX_Product_ProductGUID') CREATE UNIQUE INDEX [UIX_Product_ProductGUID] ON [Product](ProductGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_SKU') CREATE INDEX [IX_Product_SKU] ON [Product](SKU);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_IsImport') CREATE INDEX [IX_Product_IsImport] ON [Product](IsImport);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_IsSystem') CREATE INDEX [IX_Product_IsSystem] ON [Product](IsSystem);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_Published') CREATE INDEX [IX_Product_Published] ON [Product](Published);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_Wholesale') CREATE INDEX [IX_Product_Wholesale] ON [Product](Wholesale);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_Deleted') CREATE INDEX [IX_Product_Deleted] ON [Product](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_ProductTypeID') CREATE INDEX [IX_Product_ProductTypeID] ON [Product](ProductTypeID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_IsAKit') CREATE INDEX [IX_Product_IsAKit] ON [Product](IsAKit);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_Name') CREATE INDEX [IX_Product_Name] ON [Product](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_ManufacturerPartNumber') CREATE INDEX [IX_Product_ManufacturerPartNumber] ON [Product](ManufacturerPartNumber);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_Published_Deleted') CREATE INDEX [IX_Product_Published_Deleted] ON [Product](Published, Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_Wholesale_Deleted') CREATE INDEX [IX_Product_Wholesale_Deleted] ON [Product](Wholesale, Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_Published_Deleted_IsAKit') CREATE NONCLUSTERED INDEX [IX_Product_Published_Deleted_IsAKit] ON [dbo].[Product] ([Published],[Deleted],[IsAKit]) INCLUDE ([ProductID],[Name],[ProductTypeID],[TrackInventoryBySizeAndColor])
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductCategory]') AND name = N'IX_Product_ProductID') CREATE INDEX [IX_Product_ProductID] ON [ProductCategory](ProductID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductCategory]') AND name = N'IX_Category_CategoryID') CREATE INDEX [IX_Category_CategoryID] ON [ProductCategory](CategoryID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductSection]') AND name = N'IX_ProductSection_SectionID_DisplayOrder') CREATE INDEX [IX_ProductSection_SectionID_DisplayOrder] ON [ProductSection](SectionID, DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductType]') AND name = N'IX_ProductType_ProductTypeGUID') CREATE INDEX [IX_ProductType_ProductTypeGUID] ON [ProductType](ProductTypeGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductType]') AND name = N'IX_ProductType_Name') CREATE INDEX [IX_ProductType_Name] ON [ProductType](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductType]') AND name = N'IX_ProductType_DisplayOrder') CREATE INDEX [IX_ProductType_DisplayOrder] ON [ProductType](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductType]') AND name = N'IX_ProductType_DisplayOrder_Name') CREATE INDEX [IX_ProductType_DisplayOrder_Name] ON [ProductType](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'UIX_ProductVariant_VariantID') CREATE UNIQUE INDEX [UIX_ProductVariant_VariantID] ON [ProductVariant](VariantGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_ProductID') CREATE INDEX [IX_ProductVariant_ProductID] ON [ProductVariant](ProductID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_SKUSuffix') CREATE INDEX [IX_ProductVariant_SKUSuffix] ON [ProductVariant](SKUSuffix);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_ManufacturerPartNumber') CREATE INDEX [IX_ProductVariant_ManufacturerPartNumber] ON [ProductVariant](ManufacturerPartNumber);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_Deleted') CREATE INDEX [IX_ProductVariant_Deleted] ON [ProductVariant](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_Published') CREATE INDEX [IX_ProductVariant_Published] ON [ProductVariant](Published);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_Wholesale') CREATE INDEX [IX_ProductVariant_Wholesale] ON [ProductVariant](Wholesale);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_Deleted_Published') CREATE INDEX [IX_ProductVariant_Deleted_Published] ON [ProductVariant](Deleted, Published);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_Deleted_Wholesale') CREATE INDEX [IX_ProductVariant_Deleted_Wholesale] ON [ProductVariant](Deleted, Wholesale);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_IsDefault') CREATE INDEX [IX_ProductVariant_IsDefault] ON [ProductVariant](IsDefault);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_DisplayOrder') CREATE INDEX [IX_ProductVariant_DisplayOrder] ON [ProductVariant](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_Name') CREATE INDEX [IX_ProductVariant_Name] ON [ProductVariant](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProductVariant]') AND name = N'IX_ProductVariant_DisplayOrder_Name') CREATE INDEX [IX_ProductVariant_DisplayOrder_Name] ON [ProductVariant](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Profile]') AND name = N'IX_Profile_CustomerGuid') CREATE INDEX [IX_Profile_CustomerGuid] ON [Profile](CustomerGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Promotions]') AND name = N'XMLIX_Promotions_PromotionRuleData') CREATE PRIMARY XML INDEX [XMLIX_Promotions_PromotionRuleData] ON [Promotions](PromotionRuleData)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Promotions]') AND name = N'XMLIX_Promotions_PromotionRuleData_Path') CREATE XML INDEX [XMLIX_Promotions_PromotionRuleData_Path] ON [Promotions](PromotionRuleData) USING XML INDEX XMLIX_Promotions_PromotionRuleData FOR PATH
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Promotions]') AND name = N'XMLIX_Promotions_PromotionDiscountData') CREATE PRIMARY XML INDEX [XMLIX_Promotions_PromotionDiscountData] ON [Promotions](PromotionDiscountData)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Promotions]') AND name = N'XMLIX_Promotions_PromotionDiscountData_Path') CREATE XML INDEX [XMLIX_Promotions_PromotionDiscountData_Path] ON [Promotions](PromotionDiscountData) USING XML INDEX XMLIX_Promotions_PromotionDiscountData FOR PATH
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[QuantityDiscount]') AND name = N'UIX_QuantityDiscount_QuantityDiscountGUID') CREATE UNIQUE INDEX [UIX_QuantityDiscount_QuantityDiscountGUID] ON [QuantityDiscount](QuantityDiscountGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[QuantityDiscount]') AND name = N'IX_QuantityDiscount_DisplayOrder') CREATE INDEX [IX_QuantityDiscount_DisplayOrder] ON [QuantityDiscount](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[QuantityDiscount]') AND name = N'IX_QuantityDiscount_DisplayOrder_Name') CREATE INDEX [IX_QuantityDiscount_DisplayOrder_Name] ON [QuantityDiscount](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[QuantityDiscount]') AND name = N'IX_QuantityDiscount_Name') CREATE INDEX [IX_QuantityDiscount_Name] ON [QuantityDiscount](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[QuantityDiscountTable]') AND name = N'UIX_QuantityDiscountTable_QuantityDiscountTableGUID') CREATE UNIQUE INDEX [UIX_QuantityDiscountTable_QuantityDiscountTableGUID] ON [QuantityDiscountTable](QuantityDiscountTableGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[QuantityDiscountTable]') AND name = N'IX_QuantityDiscountTable_QuantityDiscountTableID') CREATE INDEX [IX_QuantityDiscountTable_QuantityDiscountTableID] ON [QuantityDiscountTable](QuantityDiscountID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[QuantityDiscountTable]') AND name = N'IX_QuantityDiscountTable_LowQuantity_HighQuantity') CREATE INDEX [IX_QuantityDiscountTable_LowQuantity_HighQuantity] ON [QuantityDiscountTable](LowQuantity, HighQuantity);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Rating]') AND name = N'IX_Rating_FoundNotHelpful') CREATE INDEX [IX_Rating_FoundNotHelpful] ON [Rating](FoundNotHelpful);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Rating]') AND name = N'IX_Rating_CreatedOn') CREATE INDEX [IX_Rating_CreatedOn] ON [Rating](CreatedOn);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Rating]') AND name = N'IX_Rating') CREATE INDEX [IX_Rating] ON [Rating](HasComment);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Rating]') AND name = N'IX_Rating_ProductID') CREATE INDEX [IX_Rating_ProductID] ON [Rating](ProductID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Rating]') AND name = N'IX_Rating_CustomerID') CREATE INDEX [IX_Rating_CustomerID] ON [Rating](CustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Rating]') AND name = N'IX_Rating_IsROTD') CREATE INDEX [IX_Rating_IsROTD] ON [Rating](IsROTD);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Rating]') AND name = N'IX_Rating_FoundHelpful') CREATE INDEX [IX_Rating_FoundHelpful] ON [Rating](FoundHelpful);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Rating]') AND name = N'IX_Rating_IsFilthy') CREATE INDEX [IX_Rating_IsFilthy] ON [Rating](IsFilthy);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RatingCommentHelpfulness]') AND name = N'IX_RatingCommentHelpfulness_StoreID') CREATE INDEX [IX_RatingCommentHelpfulness_StoreID] ON [RatingCommentHelpfulness](StoreID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RatingCommentHelpfulness]') AND name = N'IX_RatingCommentHelpfulness_VotingCustomerID') CREATE INDEX [IX_RatingCommentHelpfulness_VotingCustomerID] ON [RatingCommentHelpfulness](VotingCustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RatingCommentHelpfulness]') AND name = N'IX_RatingCommentHelpfulness_ProductID') CREATE INDEX [IX_RatingCommentHelpfulness_ProductID] ON [RatingCommentHelpfulness](ProductID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RatingCommentHelpfulness]') AND name = N'IX_RatingCommentHelpfulness_RatingCustomerID') CREATE INDEX [IX_RatingCommentHelpfulness_RatingCustomerID] ON [RatingCommentHelpfulness](RatingCustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RatingCommentHelpfulness]') AND name = N'IX_RatingCommentHelpfulness_Helpful') CREATE INDEX [IX_RatingCommentHelpfulness_Helpful] ON [RatingCommentHelpfulness](Helpful);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SalesPrompt]') AND name = N'UIX_SalesPrompt_SalesPromptGUID') CREATE UNIQUE INDEX [UIX_SalesPrompt_SalesPromptGUID] ON [SalesPrompt](SalesPromptGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SalesPrompt]') AND name = N'IX_SalesPrompt_Deleted') CREATE INDEX [IX_SalesPrompt_Deleted] ON [SalesPrompt](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SalesPrompt]') AND name = N'IX_SalesPrompt_Name') CREATE INDEX [IX_SalesPrompt_Name] ON [SalesPrompt](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'UIX_Section_SectionGUID') CREATE UNIQUE INDEX [UIX_Section_SectionGUID] ON [Section](SectionGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'IX_Section_ParentSectionID') CREATE INDEX [IX_Section_ParentSectionID] ON [Section](ParentSectionID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'IX_Section_DisplayOrder') CREATE INDEX [IX_Section_DisplayOrder] ON [Section](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'IX_Section_Name') CREATE INDEX [IX_Section_Name] ON [Section](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'IX_Section_DisplayOrder_Name') CREATE INDEX [IX_Section_DisplayOrder_Name] ON [Section](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'IX_Section_Published') CREATE INDEX [IX_Section_Published] ON [Section](Published);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'IX_Section_Wholesale') CREATE INDEX [IX_Section_Wholesale] ON [Section](Wholesale);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'IX_Section_Deleted') CREATE INDEX [IX_Section_Deleted] ON [Section](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'IX_Section_Deleted_Published') CREATE INDEX [IX_Section_Deleted_Published] ON [Section](Deleted, Published);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'IX_Section_Deleted_Wholesale') CREATE INDEX [IX_Section_Deleted_Wholesale] ON [Section](Deleted, Wholesale);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingByProduct]') AND name = N'UIX_ShippingByProduct_ShippingByProductGUID') CREATE UNIQUE INDEX [UIX_ShippingByProduct_ShippingByProductGUID] ON [ShippingByProduct](ShippingByProductGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingByTotal]') AND name = N'IX_ShippingByTotal_ShippingMethodID_LowValue_HighValue') CREATE INDEX [IX_ShippingByTotal_ShippingMethodID_LowValue_HighValue] ON [ShippingByTotal](ShippingMethodID, LowValue, HighValue);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingByTotal]') AND name = N'IX_ShippingByTotal_RowGUID') CREATE INDEX [IX_ShippingByTotal_RowGUID] ON [ShippingByTotal](RowGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingByTotalByPercent]') AND name = N'IX_ShippingByTotalByPercent_ShippingMethodID_LowValue_HighValue') CREATE INDEX [IX_ShippingByTotalByPercent_ShippingMethodID_LowValue_HighValue] ON [ShippingByTotalByPercent](ShippingMethodID, LowValue, HighValue);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingByTotalByPercent]') AND name = N'IX_ShippingByTotalByPercent_RowGUID') CREATE INDEX [IX_ShippingByTotalByPercent_RowGUID] ON [ShippingByTotalByPercent](RowGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingByWeight]') AND name = N'IX_ShippingByWeight_ShippingMethodID_LowValue_HighValue') CREATE INDEX [IX_ShippingByWeight_ShippingMethodID_LowValue_HighValue] ON [ShippingByWeight](ShippingMethodID, LowValue, HighValue);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingByWeight]') AND name = N'IX_ShippingByWeight_RowGUID') CREATE INDEX [IX_ShippingByWeight_RowGUID] ON [ShippingByWeight](RowGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingCalculation]') AND name = N'UIX_ShippingCalculation_ShippingCalculationID') CREATE UNIQUE INDEX [UIX_ShippingCalculation_ShippingCalculationID] ON [ShippingCalculation](ShippingCalculationGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingCalculation]') AND name = N'IX_ShippingCalculation_DisplayOrder') CREATE INDEX [IX_ShippingCalculation_DisplayOrder] ON [ShippingCalculation](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingCalculation]') AND name = N'IX_ShippingCalculation_Name') CREATE INDEX [IX_ShippingCalculation_Name] ON [ShippingCalculation](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingCalculation]') AND name = N'IX_ShippingCalculation_DisplayOrder_Name') CREATE INDEX [IX_ShippingCalculation_DisplayOrder_Name] ON [ShippingCalculation](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingMethod]') AND name = N'UIX_ShippingMethod_ShippingMethodGUID') CREATE UNIQUE INDEX [UIX_ShippingMethod_ShippingMethodGUID] ON [ShippingMethod](ShippingMethodGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingMethod]') AND name = N'IX_ShippingMethod_IsRTShipping') CREATE INDEX [IX_ShippingMethod_IsRTShipping] ON [ShippingMethod](IsRTShipping);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingMethod]') AND name = N'IX_ShippingMethod_DisplayOrder') CREATE INDEX [IX_ShippingMethod_DisplayOrder] ON [ShippingMethod](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingTotalByZone]') AND name = N'IX_ShippingTotalByZone_RowGUID') CREATE INDEX [IX_ShippingTotalByZone_RowGUID] ON [ShippingTotalByZone](RowGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingTotalByZone]') AND name = N'IX_ShippingTotalByZone_ShippingZoneID_LowValue_HighValue') CREATE INDEX [IX_ShippingTotalByZone_ShippingZoneID_LowValue_HighValue] ON [ShippingTotalByZone](ShippingZoneID, LowValue, HighValue);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingWeightByZone]') AND name = N'IX_ShippingWeightByZone_RowGUID') CREATE INDEX [IX_ShippingWeightByZone_RowGUID] ON [ShippingWeightByZone](RowGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingWeightByZone]') AND name = N'IX_ShippingWeightByZone_ShippingZoneID_LowValue_HighValue') CREATE INDEX [IX_ShippingWeightByZone_ShippingZoneID_LowValue_HighValue] ON [ShippingWeightByZone](ShippingZoneID, LowValue, HighValue);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingZone]') AND name = N'UIX_ShippingZone_ShippingZoneGUID') CREATE UNIQUE INDEX [UIX_ShippingZone_ShippingZoneGUID] ON [ShippingZone](ShippingZoneGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingZone]') AND name = N'IX_ShippingZone_DisplayOrder') CREATE INDEX [IX_ShippingZone_DisplayOrder] ON [ShippingZone](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShippingZone]') AND name = N'IX_ShippingZone_Deleted') CREATE INDEX [IX_ShippingZone_Deleted] ON [ShippingZone](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShoppingCart]') AND name = N'UIX_ShoppingCart_ShoppingCartRecGUID') CREATE UNIQUE INDEX [UIX_ShoppingCart_ShoppingCartRecGUID] ON [ShoppingCart](ShoppingCartRecGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShoppingCart]') AND name = N'IX_ShoppingCart_CustomerID') CREATE INDEX [IX_ShoppingCart_CustomerID] ON [ShoppingCart](CustomerID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShoppingCart]') AND name = N'IX_ShoppingCart_CustomerID_CartType') CREATE INDEX [IX_ShoppingCart_CustomerID_CartType] ON [ShoppingCart](CustomerID, CartType);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShoppingCart]') AND name = N'IX_ShoppingCart_ProductID') CREATE INDEX [IX_ShoppingCart_ProductID] ON [ShoppingCart](ProductID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShoppingCart]') AND name = N'IX_ShoppingCart_VariantID') CREATE INDEX [IX_ShoppingCart_VariantID] ON [ShoppingCart](VariantID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShoppingCart]') AND name = N'IX_ShoppingCart_ProductID_VariantID_ChosenColor_ChosenSize') CREATE INDEX [IX_ShoppingCart_ProductID_VariantID_ChosenColor_ChosenSize] ON [ShoppingCart](ProductID, VariantID, ChosenColor, ChosenSize);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShoppingCart]') AND name = N'IX_ShoppingCart_CreatedOn') CREATE INDEX [IX_ShoppingCart_CreatedOn] ON [ShoppingCart](CreatedOn);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShoppingCart]') AND name = N'IX_ShoppingCart_CartType') CREATE INDEX [IX_ShoppingCart_CartType] ON [ShoppingCart](CartType);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShoppingCart]') AND name = N'IX_ShoppingCart_CartType_RecurringSubscriptionID') CREATE INDEX [IX_ShoppingCart_CartType_RecurringSubscriptionID] ON [ShoppingCart](CartType, RecurringSubscriptionID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShoppingCart]') AND name = N'IX_ShoppingCart_NextRecurringShipDate') CREATE INDEX [IX_ShoppingCart_NextRecurringShipDate] ON [ShoppingCart](NextRecurringShipDate);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShoppingCart]') AND name = N'IX_ShoppingCart_RecurringIndex') CREATE INDEX [IX_ShoppingCart_RecurringIndex] ON [ShoppingCart](RecurringIndex);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[State]') AND name = N'UIX_State_StateGUID') CREATE UNIQUE INDEX [UIX_State_StateGUID] ON [State](StateGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[State]') AND name = N'UIX_State_Country_Abbrv') CREATE UNIQUE INDEX [UIX_State_Country_Abbrv] ON [State](CountryID, Abbreviation);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[State]') AND name = N'IX_State_DisplayOrder') CREATE INDEX [IX_State_DisplayOrder] ON [State](DisplayOrder);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[State]') AND name = N'IX_State_Name') CREATE INDEX [IX_State_Name] ON [State](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[State]') AND name = N'IX_State_DisplayOrder_Name') CREATE INDEX [IX_State_DisplayOrder_Name] ON [State](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[State]') AND name = N'IX_State_CountryID') CREATE INDEX [IX_State_CountryID] ON [State](CountryID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[StateTaxRate]') AND name = N'UIX_StateTaxRate_StateID_TaxClassID') CREATE UNIQUE INDEX [UIX_StateTaxRate_StateID_TaxClassID] ON [StateTaxRate](StateID, TaxClassID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[StringResource]') AND name = N'UIX_StringResource_Name_LocaleSetting_StoreId') CREATE UNIQUE INDEX [UIX_StringResource_Name_LocaleSetting_StoreId] ON [StringResource](Name, LocaleSetting, StoreID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Topic]') AND name = N'UIX_Topic_TopicGUID') CREATE UNIQUE INDEX [UIX_Topic_TopicGUID] ON [Topic](TopicGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Topic]') AND name = N'IX_Topic_Deleted') CREATE INDEX [IX_Topic_Deleted] ON [Topic](Deleted);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Topic]') AND name = N'IX_Topic_ShowInSiteMap') CREATE INDEX [IX_Topic_ShowInSiteMap] ON [Topic](ShowInSiteMap);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Vector]') AND name = N'UIX_Vector_VectorGUID') CREATE UNIQUE INDEX [UIX_Vector_VectorGUID] ON [Vector](VectorGUID);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Vector]') AND name = N'IX_Vector_Name') CREATE INDEX [IX_Vector_Name] ON [Vector](Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Vector]') AND name = N'IX_Vector_DisplayOrder_Name') CREATE INDEX [IX_Vector_DisplayOrder_Name] ON [Vector](DisplayOrder, Name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ZipTaxRate]') AND name = N'UIX_ZipTaxRate_ZipCode_TaxClassID_CountryID') CREATE UNIQUE INDEX [UIX_ZipTaxRate_ZipCode_TaxClassID_CountryID] ON [ZipTaxRate](ZipCode, TaxClassID, CountryID);

END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].aspdnsf_AddItemToCart') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].aspdnsf_AddItemToCart
GO

CREATE proc dbo.aspdnsf_AddItemToCart
    @CustomerID int,
    @ProductID int,
    @VariantID int,
    @Quantity int,
    @ShippingAddressID int,
    @BillingAddressID int,
    @ChosenColor nvarchar(100),
    @ChosenColorSKUModifier varchar(100),
    @ChosenSize nvarchar(100),
    @ChosenSizeSKUModifier varchar(100),
    @CleanColorOption nvarchar(100),
    @CleanSizeOption nvarchar(100),
    @ColorAndSizePriceDelta money,
    @TextOption nvarchar(max),
    @CartType int,
    @CustomerEnteredPrice money,
    @CustomerLevelID int = 0,
    @RequiresCount int = 0,
	@IsKit2 tinyint = 0,
    @NewShoppingCartRecID int OUTPUT,
    @StoreID int,
    @IsAGift bit = 0
AS
SET NOCOUNT ON
	DECLARE @RestrictedQy varchar(8000), @CurrentCartQty int, @InventoryFullSKU nvarchar(50), @InventoryWeightDelta money, @ShoppingCartrecid int, @IsAKit tinyint
	DECLARE @levelprice money, @LevelDiscountPercent money, @LevelDiscountsApplyToExtendedPrices tinyint, @CustEntersPrice tinyint

	SELECT @CustEntersPrice = CustomerEntersPrice, @RestrictedQy = RestrictedQuantities FROM dbo.ProductVariant with (nolock) WHERE VariantID = @VariantID

	SELECT	@IsAKit = IsAKit FROM dbo.Product with (nolock) WHERE ProductID = @ProductID

	-- We are always going to ignore gift records, gift item code should be able to avoid duplicate records.
	SELECT @CurrentCartQty = Quantity From dbo.shoppingcart with (nolock) where ShippingAddressID = @ShippingAddressID and ProductID = @ProductID and VariantID = @VariantID and ChosenColor = @ChosenColor and ChosenSize = @ChosenSize and TextOption like @TextOption and CustomerID = @CustomerID and CartType = @CartType and StoreID = @StoreID and (IsGift = 0 And @IsAGift = 0)

	DECLARE @RQty int
	IF isnull(rtrim(@RestrictedQy), '') = ''
		set @RQty = -1
	ELSE
		SELECT @RQty = cast(items as int) FROM dbo.Split(@RestrictedQy, ',') WHERE cast(items as int) <= isnull(@CurrentCartQty, 0) + @Quantity

	IF @CustomerLevelID = 0
		SELECT @LevelDiscountPercent = 0.0, @LevelDiscountsApplyToExtendedPrices = 0
	ELSE
		SELECT @LevelDiscountPercent = LevelDiscountPercent, @LevelDiscountsApplyToExtendedPrices = LevelDiscountsApplyToExtendedPrices FROM dbo.customerlevel with (nolock) WHERE customerlevelid = @CustomerLevelID

	-- if item already exists in the cart update it's quantity
	IF @CurrentCartQty is not null and @IsAKit = 0 and @CustEntersPrice = 0  BEGIN
		UPDATE dbo.ShoppingCart
		SET Quantity = case @RQty when -1 then Quantity + @Quantity else isnull(@RQty, 0) end ,
			RequiresCount = RequiresCount + @RequiresCount
		WHERE ShippingAddressID = @ShippingAddressID and ProductID = @ProductID and VariantID = @VariantID and ChosenColor = @ChosenColor and ChosenSize = @ChosenSize and TextOption like @TextOption and CustomerID = @CustomerID and CartType = @CartType

		SET @NewShoppingCartRecID = 0
		RETURN
	END

	--Insert item into ShoppingCart
	INSERT dbo.ShoppingCart(CartType,ShoppingCartRecGUID,CustomerID,ShippingAddressID,BillingAddressID,ProductID,VariantID,ProductSKU,ProductPrice,CustomerEntersPrice,ProductWeight,ProductDimensions,Quantity,RequiresCount,ChosenColor,ChosenColorSKUModifier,ChosenSize,ChosenSizeSKUModifier,TextOption,IsTaxable,IsShipSeparately,IsDownload,DownloadLocation,FreeShipping,DistributorID,RecurringInterval,RecurringIntervalType, IsSystem, IsAKit, TaxClassID, IsKit2, StoreID, IsGift, GTIN)
	SELECT
		@CartType,
		newid(),
		@CustomerID,
		@ShippingAddressID,
		@BillingAddressID,
		@ProductID,
		@VariantID,
		case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then isnull(p.sku, '') + isnull(pv.skusuffix, '') + isnull(@ChosenColorSKUModifier, '') + isnull(@ChosenSizeSKUModifier, '') else i.VendorFullSKU end,
		case when isnull(@CustomerEnteredPrice, 0) > 0 then @CustomerEnteredPrice
			 when p.IsAKit = 1 then dbo.GetCustomerLevelPrice(pv.VariantID, @CustomerLevelID)+((dbo.KitPriceDelta(@CustomerID, @ProductID, 0)*(100.0 - @LevelDiscountPercent))/100.0)
			 else dbo.GetCustomerLevelPrice(pv.VariantID, @CustomerLevelID)+@ColorAndSizePriceDelta
		end,
		case when @CustomerEnteredPrice is not null and @CustomerEnteredPrice > 0 then 1 else 0 end,
		pv.Weight + case when p.IsAKit = 1 then dbo.KitWeightDelta(@CustomerID, @ProductID, 0) else isnull(i.WeightDelta, 0) end,
		pv.Dimensions,
		case @RQty when -1 then @Quantity else isnull(@RQty, 0) end,
		@RequiresCount,
		@ChosenColor,
		case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then @ChosenColorSKUModifier else '' end,
		@ChosenSize,
		case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then @ChosenSizeSKUModifier else '' end,
		@TextOption,
		pv.IsTaxable,
		pv.IsShipSeparately,
		pv.IsDownload,
		pv.DownloadLocation,
		pv.FreeShipping,
		isnull(pd.DistributorID, 0),
		case pv.RecurringInterval when 0 then 1 else pv.RecurringInterval end,
		case pv.RecurringIntervalType when 0 then -5 else pv.RecurringIntervalType end,
		p.IsSystem,
		p.IsAKit,
		p.TaxClassID,
		@IsKit2,
		@StoreID,
		@IsAGift,
		case when p.TrackInventoryBySizeAndColor = 1 then i.GTIN else pv.GTIN end
	FROM dbo.Product p with (NOLOCK)
		join dbo.ProductVariant pv with (NOLOCK) on p.productid = pv.productid
		left join dbo.Inventory i with (NOLOCK) on pv.VariantID = i.VariantID and i.size = @CleanSizeOption and i.color = @CleanColorOption
		left join dbo.ProductDistributor pd with (NOLOCK) on p.ProductID = pd.ProductID
	WHERE p.ProductID = @ProductID
		and pv.VariantID = @VariantID
		
	SET @ShoppingCartrecid = SCOPE_IDENTITY()

	--Update KitCart Table if necessary
	IF (@IsAKit = 1 AND @IsKit2 = 0) BEGIN
		UPDATE KitCart SET ShoppingCartRecID = @ShoppingCartrecid WHERE ProductID = @ProductID and VariantID = @VariantID and ShoppingCartRecID = 0 and CustomerID = @CustomerID
	END

	SET @NewShoppingCartRecID = @ShoppingCartrecid
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].aspdnsf_NukeProduct') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].aspdnsf_NukeProduct
GO

create proc [dbo].[aspdnsf_NukeProduct]
    @ProductID int

AS
SET NOCOUNT ON
BEGIN


BEGIN TRAN
    DELETE dbo.ProductCategory WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductCategory could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.ProductDistributor WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductDistributor could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.ProductGenre WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductGenre could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.KitCart WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('KitCart could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.KitGroup WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('KitGroup could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.ProductManufacturer WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductManufacturer could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.ProductAffiliate WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductAffiliate could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.ProductCategory WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductCategory could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.ProductCustomerLevel WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductCustomerLevel could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.ProductLocaleSetting WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductLocaleSetting could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.ProductSection WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductSection could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.Rating WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('Rating could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.RatingCommentHelpfulness WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('RatingCommentHelpfulness could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.ShoppingCart WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ShoppingCart could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.ExtendedPrice FROM dbo.ExtendedPrice with (nolock) join [dbo].productvariant pv with (nolock) on ExtendedPrice.variantid = pv.variantid where pv.productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ExtendedPrice could not be deleted', 1, 16)
        rollback tran
        return
    end

    DELETE dbo.Inventory FROM dbo.Inventory with (nolock) join [dbo].productvariant pv with (nolock) on Inventory.variantid = pv.variantid where pv.productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('Inventory could not be deleted', 1, 16)
        rollback tran
        return
    end

    DELETE dbo.ProductVariant WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductVariant could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.Product WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('Product could not be deleted', 1, 16)
        rollback tran
        return
    END


COMMIT TRAN

END

GO


/* ======== Clean up casing issues in aspdnsf_GetProducts ======== */
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].aspdnsf_GetProducts') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].aspdnsf_GetProducts
GO

CREATE PROCEDURE [dbo].[aspdnsf_GetProducts]
    @categoryID      int = null,
    @sectionID       int = null,
    @manufacturerID  int = null,
    @distributorID   int = null,
    @genreID         int = null,
    @vectorID        int = null,
    @localeID        int = null,
    @CustomerLevelID int = null,
    @affiliateID     int = null,
    @ProductTypeID   int = null,
    @ViewType        bit = 1, -- 0 = all variants, 1 = one variant
    @sortEntity      int = 0, -- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector
    @pagenum         int = 1,
    @pagesize        int = null,
    @StatsFirst      tinyint = 1,
    @searchstr       nvarchar(4000) = null,
    @extSearch       tinyint = 0,
    @publishedonly   tinyint = 0,
    @ExcludeKits     tinyint = 0,
    @ExcludeSysProds tinyint = 0,
    @InventoryFilter int = 0,  --  will only show products with an inventory level GREATER OR EQUAL TO than the number specified in this parameter, set to -1 to disable inventory filtering
    @sortEntityName  varchar(20) = '', -- usely only when the entity id is provided, allowed values: category, section, manufacturer, distributor, genre, vector
    @localeName      varchar(20) = '',
    @OnSaleOnly      tinyint = 0,
	@storeID		 int = 1,
	@filterProduct	 bit = 0,
	@sortby			 varchar(10) = 'default',
	@since			 int = 180  -- best sellers in the last "@since" number of days
AS
BEGIN
	SET NOCOUNT ON

    DECLARE @rcount int
    DECLARE @productfiltersort table (rownum int not null identity  primary key, productid int not null, price money null, saleprice money null, displayorder int not null, VariantID int not null, VariantDisplayOrder int not null, ProductName nvarchar(400) null, VariantName nvarchar(400) null)
    DECLARE @productfilter table (rownum int not null identity  primary key, productid int not null, price money null, saleprice money null,  displayorder int not null, VariantID int not null, VariantDisplayOrder int not null, ProductName nvarchar(400) null, VariantName nvarchar(400) null)
	DECLARE @FilterProductsByAffiliate tinyint, @FilterProductsByCustomerLevel tinyint, @HideProductsWithLessThanThisInventoryLevel int
    CREATE TABLE #displayorder ([name] nvarchar (800), productid int not null primary key, displayorder int not null)
    CREATE TABLE #inventoryfilter (productid int not null, variantid int not null, InvQty int not null)
    CREATE CLUSTERED INDEX tmp_inventoryfilter ON #inventoryfilter (productid, variantid)

    DECLARE @customerLevelMappingsExist bit, @sectionMappingsExist bit, @localeMappingsExist bit, @affiliateMappingsExist bit, @categoryMappingsExist bit, @CustomerLevelFilteringIsAscending bit, @distributorMappingsExist bit, @genreMappingsExist bit, @vectorMappingsExist bit, @manufacturerMappingsExist bit, @ftsenabled tinyint = 0, @searching bit = 0

	IF @searchstr IS NOT NULL
		SET @searching = 1

	IF @searching = 1
	BEGIN
		IF ((SELECT DATABASEPROPERTYEX(db_name(db_id()),'IsFulltextEnabled')) = 1
			AND EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[KeyWordSearch]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
			AND EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetValidSearchString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT')))
		BEGIN
			SET @ftsenabled = 1
		END
	END

    SET @FilterProductsByAffiliate = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @FilterProductsByCustomerLevel = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @HideProductsWithLessThanThisInventoryLevel = (SELECT TOP 1 case ConfigValue when -1 then 0 else ConfigValue end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)

    IF @InventoryFilter <> -1 and (@HideProductsWithLessThanThisInventoryLevel > @InventoryFilter or @HideProductsWithLessThanThisInventoryLevel  = -1)
        SET @InventoryFilter  = @HideProductsWithLessThanThisInventoryLevel

    SET @categoryID      = nullif(@categoryID, 0)
    SET @sectionID       = nullif(@sectionID, 0)
    SET @manufacturerID  = nullif(@manufacturerID, 0)
    SET @distributorID   = nullif(@distributorID, 0)
    SET @genreID         = nullif(@genreID, 0)
    SET @vectorID        = nullif(@vectorID, 0)
    SET @affiliateID     = nullif(@affiliateID, 0)
    SET @ProductTypeID   = nullif(@ProductTypeID, 0)

    SET @CustomerLevelFilteringIsAscending  = 0
    SET @CustomerLevelFilteringIsAscending = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterByCustomerLevelIsAscending' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)

    IF @localeID is null and ltrim(rtrim(@localeName)) <> ''
        SELECT @localeID = LocaleSettingID FROM dbo.LocaleSetting with (nolock) WHERE Name = ltrim(rtrim(@localeName))

	SELECT @categoryMappingsExist		= CASE WHEN EXISTS (SELECT * FROM ProductCategory) THEN 1 ELSE 0 END
	SELECT @sectionMappingsExist		= CASE WHEN EXISTS (SELECT * FROM ProductSection) THEN 1 ELSE 0 END
	SELECT @localeMappingsExist			= CASE WHEN EXISTS (SELECT * FROM ProductLocaleSetting) THEN 1 ELSE 0 END
	SELECT @customerLevelMappingsExist	= CASE WHEN EXISTS (SELECT * FROM ProductCustomerLevel) THEN 1 ELSE 0 END
	SELECT @affiliateMappingsExist		= CASE WHEN EXISTS (SELECT * FROM ProductAffiliate) THEN 1 ELSE 0 END
	SELECT @distributorMappingsExist	= CASE WHEN EXISTS (SELECT * FROM ProductDistributor) THEN 1 ELSE 0 END
	SELECT @genreMappingsExist			= CASE WHEN EXISTS (SELECT * FROM ProductGenre) THEN 1 ELSE 0 END
	SELECT @vectorMappingsExist			= CASE WHEN EXISTS (SELECT * FROM ProductVector) THEN 1 ELSE 0 END
	SELECT @manufacturerMappingsExist	= CASE WHEN EXISTS (SELECT * FROM ProductManufacturer) THEN 1 ELSE 0 END
	
    -- get page size
    IF @pagesize is null or @pagesize = 0 BEGIN
        IF @categoryID is not null
            SELECT @pagesize = PageSize FROM dbo.Category with (nolock) WHERE categoryID = @categoryID
        ELSE IF @sectionID is not null
            SELECT @pagesize = PageSize FROM dbo.Section with (nolock) WHERE sectionID = @sectionID
        ELSE IF @manufacturerID is not null
            SELECT @pagesize = PageSize FROM dbo.Manufacturer with (nolock) WHERE manufacturerID = @manufacturerID
        ELSE IF @distributorID is not null
            SELECT @pagesize = PageSize FROM dbo.Distributor with (nolock) WHERE distributorID = @distributorID
        ELSE IF @genreID is not null
            SELECT @pagesize = PageSize FROM dbo.Genre with (nolock) WHERE genreID = @genreID
        ELSE IF @vectorID is not null
            SELECT @pagesize = PageSize FROM dbo.Vector with (nolock) WHERE vectorID = @vectorID
        ELSE
            SET @pagesize = (SELECT TOP 1 ConfigValue FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'Default_CategoryPageSize' AND (StoreID = @storeID OR StoreID = 0) ORDER BY StoreID DESC)
    END

    IF @pagesize is null or @pagesize = 0
        SET @pagesize = 20

    -- get sort order
    IF @filterProduct = 1 BEGIN
		IF @sortEntity = 1 or @sortEntityName = 'category' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductCategory a with (nolock) inner join (select distinct a.ProductID from ProductCategory a with (nolock)
			inner join ProductStore ps with (nolock) on a.ProductID = ps.ProductID and StoreID = @storeID) b  on a.ProductID = b.ProductID where categoryID = @categoryID
		END
		ELSE IF @sortEntity = 2 or @sortEntityName = 'section' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductSection a with (nolock) inner join (select distinct a.ProductID from ProductSection a with (nolock)
			inner join ProductStore ps with (nolock) on a.ProductID = ps.ProductID and StoreID = @storeID) b on a.ProductID = B.ProductID where sectionId = @sectionID
		END
		ELSE IF @sortEntity = 3 or @sortEntityName = 'manufacturer' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductManufacturer a with (nolock) inner join (select distinct a.ProductID from ProductManufacturer a with (nolock)
			inner join ProductStore ps with (nolock) on a.ProductID = ps.ProductID and StoreID = @storeID) b on a.ProductID = B.ProductID where ManufacturerID = @manufacturerID
		END
		ELSE IF @sortEntity = 4 or @sortEntityName = 'distributor' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductDistributor a with (nolock) inner join (select distinct a.ProductID from ProductDistributor a with (nolock)
			inner join ProductStore ps with (nolock) on a.ProductID = ps.ProductID and StoreID = @storeID) b on a.ProductID = B.ProductID where DistributorID = @distributorID
		END
		ELSE IF @sortEntity = 5 or @sortEntityName = 'genre' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductGenre a with (nolock) inner join (select distinct a.ProductID from ProductGenre a with (nolock)
			inner join ProductStore ps with (nolock) on a.ProductID = ps.ProductID and StoreID = @storeID) b on a.ProductID = B.ProductID where GenreID = @genreID
		END
		ELSE IF @sortEntity = 6 or @sortEntityName = 'vector' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductVector a with (nolock) inner join (select distinct a.ProductID from ProductVector a with (nolock)
			inner join ProductStore ps with (nolock) on a.ProductID = ps.ProductID and StoreID = @storeID) b on a.ProductID = B.ProductID where VectorID = @vectorID
		END
		ELSE BEGIN
			INSERT #displayorder select distinct [name], a.productid, 1 from dbo.Product a with (nolock) inner join (select distinct a.ProductID from Product a with (nolock)
			inner join ProductStore ps with (nolock) on a.ProductID = ps.ProductID and StoreID = @storeID) b on a.ProductID = B.ProductID ORDER BY Name
		END
	END
	ELSE BEGIN
		IF @sortEntity = 1 or @sortEntityName = 'category' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductCategory a with (nolock) where categoryID = @categoryID
		END
		ELSE IF @sortEntity = 2 or @sortEntityName = 'section' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductSection a with (nolock) where sectionId = @sectionID
		END
		ELSE IF @sortEntity = 3 or @sortEntityName = 'manufacturer' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductManufacturer a with (nolock) where ManufacturerID = @manufacturerID
		END
		ELSE IF @sortEntity = 4 or @sortEntityName = 'distributor' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductDistributor a with (nolock) where DistributorID = @distributorID
		END
		ELSE IF @sortEntity = 5 or @sortEntityName = 'genre' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductGenre a with (nolock) where GenreID = @genreID
		END
		ELSE IF @sortEntity = 6 or @sortEntityName = 'vector' BEGIN
			INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductVector a with (nolock) where VectorID = @vectorID
		END
		ELSE BEGIN
			INSERT #displayorder select distinct [name], a.productid, 1 from dbo.Product a with (nolock) ORDER BY Name
		END
	END

	IF @searching = 1
	BEGIN
		IF (@ftsenabled = 1)
		BEGIN
			IF rtrim(isnull(@searchstr, '')) <> ''
			BEGIN
				DECLARE @tmpsrch nvarchar(4000)
				SET @tmpsrch = dbo.GetValidSearchString(@searchstr)
				DELETE #displayorder from #displayorder d left join dbo.KeyWordSearch(@tmpsrch) k on d.productid = k.productid where k.productid is null
			END
		END

		SET @searchstr = '%' + rtrim(ltrim(@searchstr)) + '%'
	END

    IF @InventoryFilter <> -1 BEGIN
        IF @ViewType = 1 BEGIN
            INSERT #inventoryfilter
            SELECT p.productid, pv.VariantID, sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) invqty
            FROM product p with (NOLOCK) join #displayorder d on p.ProductID = d.ProductID
                join ProductVariant pv with (NOLOCK) on p.ProductID = pv.ProductID  and pv.IsDefault = 1
                left join Inventory i with (NOLOCK) on pv.VariantID = i.VariantID
            GROUP BY p.productid, pv.VariantID
            HAVING sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) >= @InventoryFilter
        END
        ELSE
            INSERT #inventoryfilter
            SELECT p.productid, pv.VariantID, sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) invqty
            FROM product p with (NOLOCK) join #displayorder d on p.ProductID = d.ProductID
                join ProductVariant pv with (NOLOCK) on p.ProductID = pv.ProductID
                left join Inventory i with (NOLOCK) on pv.VariantID = i.VariantID
            GROUP BY p.productid, pv.VariantID
            HAVING sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) >= @InventoryFilter


        INSERT @productfilter (productid, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName)
        SELECT distinct p.productid, do.displayorder, pv.VariantID, pv.DisplayOrder, p.Name, pv.Name
        FROM
            product p with (nolock)
            join #displayorder do on p.ProductID = do.ProductID
            left join ProductVariant pv        with (NOLOCK) ON p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType
            left join productcategory pc       with (nolock) on p.ProductID = pc.ProductID
            left join productsection ps        with (nolock) on p.ProductID = ps.ProductID
            left join ProductManufacturer pm   with (nolock) on p.ProductID = pm.ProductID
            left join ProductDistributor pd    with (nolock) on p.ProductID = pd.ProductID
            left join ProductGenre px          with (nolock) on p.ProductID = px.ProductID
            left join ProductVector px2        with (nolock) on p.ProductID = px2.ProductID
            left join ProductLocaleSetting pl  with (nolock) on p.ProductID = pl.ProductID
            left join ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID
            left join ProductAffiliate pa      with (nolock) on p.ProductID = pa.ProductID

            join #inventoryfilter i on pv.VariantID = i.VariantID
        WHERE
              (pc.categoryid = @categoryID or @categoryID is null or @categoryMappingsExist = 0)
          and (ps.sectionid = @sectionID or @sectionID is null or @sectionMappingsExist = 0)
          and (pl.LocaleSettingID = @localeID or @localeID is null or @localeMappingsExist = 0)
          and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliateMappingsExist = 0 or @FilterProductsByAffiliate = 0)
          and (pm.manufacturerid = @manufacturerID or @manufacturerID is null or @manufacturerMappingsExist = 0)
          and (pd.DistributorID = @distributorID or @distributorID is null or @distributorMappingsExist = 0)
          and (px.GenreID = @genreID or @genreID is null or @genreMappingsExist = 0)
          and (px2.VectorID = @vectorID or @vectorID is null or @vectorMappingsExist = 0)
          and p.ProductTypeID = coalesce(@ProductTypeID, p.ProductTypeID)
          and (case
                when @FilterProductsByCustomerLevel = 0 or @customerLevelMappingsExist = 0 or pcl.CustomerLevelID is null or @CustomerLevelID is null then 1
                when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID then 1
                when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1
                else 0
               end  = 1
              )
          and (@ftsenabled = 1 or
					(@searching = 0 or
						(@searching = 1 and
							(@ftsenabled = 0 and
								(patindex(@searchstr, isnull(p.name, '')) > 0
									or patindex(@searchstr, isnull(convert(nvarchar(20),p.productid), '')) > 0
									or patindex(@searchstr, isnull(pv.name, '')) > 0
									or patindex(@searchstr, isnull(p.sku , '')+isnull(pv.skusuffix , '')) > 0
									or patindex(@searchstr, isnull(p.manufacturerpartnumber, '')) > 0
									or patindex(@searchstr, isnull(pv.manufacturerpartnumber, '')) > 0
									or (patindex(@searchstr, isnull(p.Description, '')) > 0 and @extSearch = 1)
									or (patindex(@searchstr, isnull(p.Summary, '')) > 0 and @extSearch = 1)
								)
							)
						)
					)
				)
          and case when isnull(pv.saleprice,0) = 0 then 0 else 1 end >= @OnSaleOnly
          and p.published >= @publishedonly
          and pv.published >= @publishedonly
          and isnull(p.IsAKit, 0) <= 1-@ExcludeKits
          and p.IsSystem <= 1-@ExcludeSysProds
          and p.Deleted = 0
          and pv.Deleted = 0
        order by do.displayorder, p.Name, pv.DisplayOrder, pv.Name

    END
    ELSE BEGIN
        INSERT @productfilter (productid, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName)
        SELECT distinct p.productid, do.displayorder, pv.VariantID, pv.DisplayOrder, p.Name, pv.Name
        FROM
            product p with (nolock)
            join #displayorder do on p.ProductID = do.ProductID
            join ProductVariant pv             with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType
            left join productcategory pc       with (nolock) on p.ProductID = pc.ProductID
            left join productsection ps        with (nolock) on p.ProductID = ps.ProductID
            left join ProductManufacturer pm   with (nolock) on p.ProductID = pm.ProductID
            left join ProductDistributor pd    with (nolock) on p.ProductID = pd.ProductID
            left join ProductGenre px          with (nolock) on p.ProductID = px.ProductID
            left join ProductVector px2        with (nolock) on p.ProductID = px2.ProductID
            left join ProductLocaleSetting pl  with (nolock) on p.ProductID = pl.ProductID
            left join ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID
            left join ProductAffiliate pa      with (nolock) on p.ProductID = pa.ProductID
        WHERE
              (pc.categoryid = @categoryID or @categoryID is null or @categoryMappingsExist = 0)
          and (ps.sectionid = @sectionID or @sectionID is null or @sectionMappingsExist = 0)
          and (pl.LocaleSettingID = @localeID or @localeID is null or @localeMappingsExist = 0)
          and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliateMappingsExist = 0 or @FilterProductsByAffiliate = 0)
          and (pm.manufacturerid = @manufacturerID or @manufacturerID is null or @manufacturerMappingsExist = 0)
          and (pd.DistributorID = @distributorID or @distributorID is null or @distributorMappingsExist = 0)
          and (px.GenreID = @genreID or @genreID is null or @genreMappingsExist = 0)
          and (px2.VectorID = @vectorID or @vectorID is null or @vectorMappingsExist = 0)
          and p.ProductTypeID = coalesce(@ProductTypeID, p.ProductTypeID)
          and (case
                when @FilterProductsByCustomerLevel = 0 or @customerLevelMappingsExist = 0 or pcl.CustomerLevelID is null or @CustomerLevelID is null then 1
                when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID then 1
                when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1
                else 0
               end  = 1
              )
          and (@ftsenabled = 1 or
					(@searching = 0 or
						(@searching = 1 and
							(@ftsenabled = 0 and
								(patindex(@searchstr, isnull(p.name, '')) > 0
									or patindex(@searchstr, isnull(convert(nvarchar(20),p.productid), '')) > 0
									or patindex(@searchstr, isnull(pv.name, '')) > 0
									or patindex(@searchstr, isnull(p.sku , '')+isnull(pv.skusuffix , '')) > 0
									or patindex(@searchstr, isnull(p.manufacturerpartnumber, '')) > 0
									or patindex(@searchstr, isnull(pv.manufacturerpartnumber, '')) > 0
									or (patindex(@searchstr, isnull(p.Description, '')) > 0 and @extSearch = 1)
									or (patindex(@searchstr, isnull(p.Summary, '')) > 0 and @extSearch = 1)
								)
							)
						)
					)
				)
          and case when isnull(pv.saleprice,0) = 0 then 0 else 1 end >= @OnSaleOnly
          and p.published >= @publishedonly
          and pv.published >= @publishedonly
          and isnull(p.IsAKit, 0) <= 1-@ExcludeKits
          and p.IsSystem <= 1-@ExcludeSysProds
          and p.Deleted = 0
          and pv.Deleted = 0
        order by do.displayorder, p.Name, pv.DisplayOrder, pv.Name
    END

    SET @rcount = @@rowcount
    IF @StatsFirst = 1
        SELECT cast(ceiling(@rcount*1.0/@pagesize) as int) pages, @rcount ProductCount

  --Begin sorting
  	if @sortby = 'bestseller'
		begin
			insert @productfiltersort (productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName)
			select pf.productid, pf.price, pf.saleprice, pf.displayorder, pf.VariantID, pf.VariantDisplayOrder, pf.ProductName, pf.VariantName
				from @productfilter pf
				inner join (
					select ProductID, SUM(Quantity) AS NumSales
					  from dbo.Orders_ShoppingCart sc with (NOLOCK)
							join [dbo].Orders o with (NOLOCK)  on sc.OrderNumber = o.OrderNumber and o.OrderDate >= dateadd(dy, -@since, getdate())
					  group by ProductID
				) bsSort on pf.productid = bsSort.ProductID
				order by isnull(bsSort.NumSales, 0) DESC
		end
  	else --default
		begin
			insert @productfiltersort (productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName)
			select productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName
			from @productfilter order by displayorder, productName, variantDisplayOrder, variantName
		end

    -- Check filtered products for recurring variants
    Declare @ProductResults Table
    (
		ProductID int
		, VariantID int
		, HasRecurring bit
		, RowNum int
    );
    -- temp table based on filtered product result set
    Insert Into @ProductResults

    SELECT   Distinct
        p.ProductID,
		pv.VariantID,
		0,
		pf.rownum
    FROM dbo.Product p with (NOLOCK)
        left join dbo.ProductVariant       pv  with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType
        join @productfiltersort            pf                on pv.ProductID = pf.ProductID and pv.VariantID = pf.VariantID
    WHERE pf.rownum >= @pagesize*(@pagenum-1)+1 and pf.rownum <= @pagesize*(@pagenum)
    ORDER BY pf.rownum

    -- set HasRecurring
    Update pr
    Set HasRecurring = 1
    From @ProductResults pr, (
		Select prs.ProductId
		From @ProductResults prs, ProductVariant pv
		Where prs.ProductID = pv.ProductID
		And pv.IsRecurring = 1
		And pv.Deleted = 0
		And pv.Published = 1
		Group By prs.ProductId
		Having Count(*) > 0) tmp
    Where pr.ProductId = tmp.ProductId
    ---- End Recurring

    SELECT
        p.ProductID,
        p.Name,
        pv.VariantID,
        pv.Name AS VariantName,
        p.ProductGUID,
        p.Summary,
        p.Description,
        p.SEKeywords,
        p.SEDescription,
        p.MiscText,
        p.FroogleDescription,
        p.SETitle,
        p.SEAltText,
        p.SizeOptionPrompt,
        p.ColorOptionPrompt,
        p.TextOptionPrompt,
        p.ProductTypeID,
        p.TaxClassID,
        p.SKU,
        p.ManufacturerPartNumber,
        p.SalesPromptID,
        p.IsFeatured,
        p.XmlPackage,
        p.ColWidth,
        p.Published,
        p.RequiresRegistration,
        p.Looks,
        p.Notes,
        p.QuantityDiscountID,
        p.RelatedProducts,
        p.UpsellProducts,
        p.UpsellProductDiscountPercentage,
        p.RelatedDocuments,
        p.TrackInventoryBySizeAndColor,
        p.TrackInventoryBySize,
        p.TrackInventoryByColor,
        p.IsAKit,
        p.ShowBuyButton,
        p.RequiresProducts,
        p.HidePriceUntilCart,
        p.IsCalltoOrder,
        p.ExcludeFromPriceFeeds,
        p.RequiresTextOption,
        p.TextOptionMaxLength,
        p.SEName,
        p.Deleted,
        p.CreatedOn,
        p.ImageFilenameOverride,
        pv.VariantGUID,
        pv.Description AS VariantDescription,
        pv.SEKeywords AS VariantSEKeywords,
        pv.SEDescription AS VariantSEDescription,
        pv.Colors,
        pv.ColorSKUModifiers,
        pv.Sizes,
        pv.SizeSKUModifiers,
        pv.FroogleDescription AS VariantFroogleDescription,
        pv.SKUSuffix,
        pv.ManufacturerPartNumber AS VariantManufacturerPartNumber,
        pv.Price,
        pv.CustomerEntersPrice,
        pv.CustomerEntersPricePrompt,
        isnull(pv.SalePrice, 0) SalePrice,
        cast(isnull(pv.Weight,0) as decimal(10,1)) Weight,
        pv.MSRP,
        pv.Cost,
        isnull(pv.Points,0) Points,
        pv.Dimensions,
        case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end Inventory,
        pv.DisplayOrder as VariantDisplayOrder,
        pv.Notes AS VariantNotes,
        pv.IsTaxable,
        pv.IsShipSeparately,
        pv.IsDownload,
        pv.DownloadLocation,
        pv.Published AS VariantPublished,
        pv.IsSecureAttachment,
        pv.IsRecurring,
        pv.RecurringInterval,
        pv.RecurringIntervalType,
        pv.SEName AS VariantSEName,
        pv.RestrictedQuantities,
        pv.MinimumQuantity,
        pv.Deleted AS VariantDeleted,
        pv.CreatedOn AS VariantCreatedOn,
        d.Name AS DistributorName,
        d.DistributorID,
        d.SEName AS DistributorSEName,
        m.ManufacturerID,
        m.Name AS ManufacturerName,
        m.SEName AS ManufacturerSEName,
        s.Name AS SalesPromptName,
        pf.HasRecurring,
        case when pcl.productid is null then 0 else isnull(ep.Price, 0) end ExtendedPrice
    FROM dbo.Product p with (NOLOCK)
        left join dbo.ProductVariant       pv  with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType
        join @ProductResults			   pf                on pv.ProductID = pf.ProductID and pv.VariantID = pf.VariantID
        left join dbo.SalesPrompt           s  with (NOLOCK) on p.SalesPromptID = s.SalesPromptID
        left join dbo.ProductManufacturer  pm  with (NOLOCK) on p.ProductID = pm.ProductID
        left join dbo.Manufacturer          m  with (NOLOCK) on pm.ManufacturerID = m.ManufacturerID
        left join dbo.ProductDistributor   pd  with (NOLOCK) on p.ProductID = pd.ProductID
        left join dbo.Distributor           d  with (NOLOCK) on pd.DistributorID = d.DistributorID
        left join dbo.ExtendedPrice        ep  with (NOLOCK) on ep.VariantID = pv.VariantID and ep.CustomerLevelID = @CustomerLevelID
        left join dbo.ProductCustomerLevel pcl with (NOLOCK) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID
        left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i  on pv.VariantID = i.VariantID

    IF @StatsFirst <> 1
        SELECT cast(ceiling(@rcount*1.0/@pagesize) as int) pages, @rcount ProductCount
END
GO

if not exists(select * from AppConfig where Name = 'MicroPay.ShowAddToBalanceLink')
begin
	insert AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	values('MicroPay.ShowAddToBalanceLink','DISPLAY','If true, the account page will show a link to the MicroPay product so a user can add to their balance. Micropay must also be enabled.','true', 'boolean', 0);
end
go

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('dbo.FindQtyDiscountID') AND OBJECTPROPERTY(id, 'IsScalarFunction') = 1)
	DROP FUNCTION dbo.FindQtyDiscountID
GO

CREATE FUNCTION dbo.FindQtyDiscountID(@entityid INT, @entitytype VARCHAR(20), @nestedLevel INT)
RETURNS INT
AS
BEGIN
	SET @nestedLevel = @nestedLevel + 1;
	IF (@nestedLevel = 16)
		RETURN -1;

    DECLARE @quantityDiscountId INT, @parentEntityId INT;
    
	SELECT 
		@quantityDiscountId = QuantityDiscountID,
		@parentEntityId = ParentEntityID
	FROM dbo.entitymaster (NOLOCK)
	WHERE EntityId = @entityid 
		AND EntityType = @entitytype;

    IF (ISNULL(@quantityDiscountId, 0) = 0) 
		AND (ISNULL(@parentEntityId, 0) <> 0)
		BEGIN
			SELECT @quantityDiscountId = dbo.FindQtyDiscountID(@parentEntityId, @entitytype, @nestedLevel);
		END;

    RETURN ISNULL(@quantityDiscountId, 0);
END
GO

if exists (select * from dbo.sysobjects where id = object_id('dbo.GetQtyDiscountID') and objectproperty(id, 'IsScalarFunction') = 1)
	drop function dbo.GetQtyDiscountID
go

CREATE FUNCTION [dbo].[GetQtyDiscountID](@productid INT)
RETURNS INT
AS
BEGIN
    DECLARE @did INT;
    SET @did = 0;

    SELECT @did = ISNULL(QuantityDiscountID, 0) FROM dbo.Product (NOLOCK) WHERE ProductID = @productid;

    IF @did = 0
		SELECT TOP 1 
			@did = dbo.FindQtyDiscountID(pe.EntityID, pe.EntityType, 1)
		FROM dbo.ProductEntity pe (NOLOCK)
			JOIN dbo.EntityMaster em (NOLOCK) ON pe.EntityID = em.EntityID AND pe.EntityType = em.EntityType
		WHERE pe.ProductID = @productid
			AND pe.EntityType IN ('category', 'section', 'manufacturer')
			AND dbo.FindQtyDiscountID(pe.EntityID, pe.EntityType, 1) != 0
		ORDER BY 
			CASE pe.EntityType 
				WHEN 'category' THEN 1 
				WHEN 'section' THEN 2 
				WHEN 'manufacturer' THEN 3 END, 
			em.ParentEntityID, pe.DisplayOrder;

    RETURN @did;
END
GO

/* ======== Schema changes to fix shipping method filtering by store ======== */
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('[dbo].[ShippingWeightByZone]') AND Name = 'StoreID')
	ALTER TABLE [dbo].[ShippingWeightByZone] ADD StoreID INT NOT NULL DEFAULT(1)
GO

update AppConfig set ConfigValue = '' where Name = 'SiteDisclaimerAgreedPage' and ConfigValue = 'default.aspx'
go

update appconfig set description = 'When customers'' sessions end due to idleness, they will be sent to this page on your site.  A blank value means the site''s home page.' 
where name = 'SessionTimeoutLandingPage'

update appconfig set description = 'If you want to enable template file switching by entity, set this flag to true. You can then assign a custom layout file to entities (category, section, etc).' 
where name = 'TemplateSwitching.Enabled'

update appconfig set description = 'Specifies the URL to the page that handles callback notifications from PayPal. Do not change this value without consulting PayPal support.' 
where name = 'PayPal.NotificationURL'

UPDATE AppConfig SET Description = 'The image location for the add-to-cart button. This must be located in the /skins/{SkinName}/images folder. Specify just the file name, such as addtocart.gif.' 
WHERE Name = 'AddToCart.AddToCartButton'

UPDATE AppConfig SET Description = 'The image location for the add to wishlist button. This must be located in the /skins/{SkinName}/images folder. Specify just the filename, such as addwishlist.gif.' 
WHERE Name = 'AddToCart.AddToWishButton'

UPDATE AppConfig SET Description = N'If TRUE, any errors returned from the real-time rate call will be displayed on the cart page. Very helpful for debugging real-time rate issues.' WHERE Name = 'RTShipping.ShowErrors'

/* ======== Password Requirement Changes ======== */
UPDATE AppConfig SET Name = 'StrongPasswordValidator', GroupName = 'SECURITY', Description = 'A Regular Expression that is used to validate passwords.  This enforces stronger passwords than PasswordValidator does. Test your expression thoroughly before changing this.'
WHERE Name = 'CustomerPwdValidator'
UPDATE AppConfig SET Description = N'Set TRUE to require customers to use strong passwords. When TRUE, the regular expression stored in the StrongPasswordValidator setting is used for validation.' WHERE Name = 'UseStrongPwd'

IF NOT EXISTS(SELECT * FROM AppConfig WHERE Name = 'PasswordValidator')
BEGIN
	INSERT AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	VALUES('PasswordValidator','SECURITY','A Regular Expression that is used to validate passwords. Test your expression thoroughly before changing this.','^(?=.*[0-9])(?=.*[a-zA-Z]).{7,}$', 'string', 1);
END

/* ======== Update Sprocs for Store-Specific AppConfigs ======== */
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_ProductInfo]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_ProductInfo]
GO

CREATE PROCEDURE [dbo].[aspdnsf_ProductInfo]
    @ProductID          INT,
    @CustomerLevelID    INT,
    @DefaultVariantOnly TINYINT,
    @InvFilter          INT = 0,
    @AffiliateID        INT = null,
    @PublishedOnly      TINYINT = 1,
    @IsAdmin			TINYINT = 0,
    @StoreID			INT = 0
AS BEGIN
	SET NOCOUNT ON
	DECLARE
		@CustLevelExists INT,
		@AffiliateExists INT,
		@FilterProductsByAffiliate TINYINT,
		@FilterProductsByCustomerLevel TINYINT,
		@CustomerLevelFilteringIsAscending TINYINT,
		@CustomerLevelCount INT,
		@AffiliateCount INT,
		@MinProductCustomerLevel INT,
		@HideProductsWithLessThanThisInventoryLevel INT

		SET @FilterProductsByCustomerLevel = (SELECT TOP 1 CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterProductsByCustomerLevel' AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC)
		SET @CustomerLevelFilteringIsAscending= (SELECT TOP 1 CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterByCustomerLevelIsAscending'	AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC)
		SET @FilterProductsByAffiliate = (SELECT TOP 1 CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterProductsByAffiliate' AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC)
		SET @HideProductsWithLessThanThisInventoryLevel	= (SELECT TOP 1 CONVERT(INT, ConfigValue) FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'HideProductsWithLessThanThisInventoryLevel' AND ISNUMERIC(ConfigValue) = 1 AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC)
		SELECT @CustomerLevelCount = COUNT(*), @MinProductCustomerLevel = MIN(CustomerLevelID), @CustLevelExists = SUM(CASE WHEN CustomerLevelID = @CustomerLevelID THEN 1 ELSE 0 END) FROM dbo.ProductCustomerLevel WITH (NOLOCK) WHERE ProductID = @ProductID
		SELECT @AffiliateCount = COUNT(*), @AffiliateExists = SUM(CASE WHEN AffiliateID = @AffiliateID THEN 1 ELSE 0 END) FROM dbo.ProductAffiliate WITH (NOLOCK) WHERE ProductID = @ProductID

		IF (@HideProductsWithLessThanThisInventoryLevel > @InvFilter or @HideProductsWithLessThanThisInventoryLevel = -1) and @InvFilter <> 0
			SET @InvFilter = @HideProductsWithLessThanThisInventoryLevel

		IF
		(
			(
				(
					@FilterProductsByCustomerLevel = 0
					OR @CustomerLevelCount = 0
					OR (
						@CustomerLevelFilteringIsAscending = 1
						AND @MinProductCustomerLevel <= @CustomerLevelID)
					OR @CustLevelExists > 0
				)
				AND (
					@FilterProductsByAffiliate = 0
					OR @AffiliateCount = 0
					OR @AffiliateExists > 0)
			)
			OR @IsAdmin = 1
		)
		SELECT
			  p.*
			, pv.VariantID
			, pv.name VariantName
			, pv.Price
			, pv.Description VariantDescription
			, ISNULL(pv.SalePrice, 0) SalePrice
			, ISNULL(pv.SkuSuffix, '') AS SkuSuffix
			, ISNULL(pv.ManufacturerPartNumber, '') AS VariantManufacturerPartNumber
			, ISNULL(pv.Dimensions, '') AS Dimensions
			, pv.Weight
			, ISNULL(pv.GTIN, '') AS GTIN
			, pv.Condition
			, ISNULL(pv.Points, 0) Points
			, pv.Inventory
			, pv.ImageFilenameOverride VariantImageFilenameOverride
			, pv.isdefault
			, pv.CustomerEntersPrice
			, ISNULL(pv.colors, '') Colors
			, ISNULL(pv.sizes, '') Sizes
			, sp.name SalesPromptName
			, CASE WHEN pcl.productid is null THEN 0 ELSE ISNULL(e.Price, 0) END ExtendedPrice
			, PRODUCTMANUFACTURER.ManufacturerID AS ProductManufacturerId
			, MANUFACTURER.Name AS ProductManufacturerName
			, MANUFACTURER.SEName AS ProductManufacturerSEName
		 FROM dbo.Product p WITH (NOLOCK)
		 JOIN dbo.productvariant pv WITH (NOLOCK) ON p.ProductID = pv.ProductID
		 JOIN dbo.SalesPrompt sp WITH (NOLOCK) ON p.SalesPromptID = sp.SalesPromptID
	LEFT JOIN dbo.ExtendedPrice e WITH (NOLOCK) ON pv.VariantID=e.VariantID and e.CustomerLevelID = @CustomerLevelID
	LEFT JOIN dbo.ProductCustomerLevel pcl WITH (NOLOCK) ON p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID
	LEFT JOIN (SELECT variantid, SUM(quan) inventory
				 FROM inventory
			 GROUP BY variantid) i on pv.variantid = i.variantid
	LEFT JOIN dbo.PRODUCTMANUFACTURER WITH (NOLOCK) ON p.ProductID = PRODUCTMANUFACTURER.ProductID
	LEFT JOIN dbo.MANUFACTURER WITH (NOLOCK) ON PRODUCTMANUFACTURER.ManufacturerID = MANUFACTURER.ManufacturerID
	    WHERE p.ProductID = @ProductID
		  AND p.Deleted = 0
		  AND pv.Deleted = 0
		  AND p.Published >= @PublishedOnly
		  AND pv.Published >= @PublishedOnly
		  AND pv.IsDefault >= @DefaultVariantOnly
		  AND (CASE p.TrackInventoryBySizeAndColor WHEN 1 THEN ISNULL(i.inventory, 0) ELSE pv.inventory END >= @InvFilter or @InvFilter = 0)
     ORDER BY pv.DisplayOrder, pv.name
END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SessionAge]') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].[aspdnsf_SessionAge]
GO

CREATE PROCEDURE [dbo].[aspdnsf_SessionAge]
    @CustomerID int = null,
	@storeId INT = 0

AS
SET NOCOUNT ON

DECLARE @SessionTimeOut varchar(10), @intSessionTimeOut int
SET @SessionTimeOut = (SELECT TOP 1 ConfigValue FROM dbo.AppConfig with (nolock) WHERE [Name] = 'SessionTimeoutInMinutes' AND (StoreID=@storeId OR StoreID=0) ORDER BY StoreID desc)

IF ISNUMERIC(@SessionTimeOut) = 1
    set @intSessionTimeOut = convert(int, @SessionTimeOut)
ELSE
    set @intSessionTimeOut = 60

DELETE dbo.Customersession WHERE CustomerID = coalesce(@CustomerID, CustomerID) and  (LoggedOut is not null or LastActivity <= dateadd(mi, -@intSessionTimeOut, getdate()))
GO

/* ======== Unused stored procedure cleanup ======== */
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_ProductSequence]') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].[aspdnsf_ProductSequence]
GO

if not exists(select * from AppConfig where Name = 'SecurityCodeRequiredOnCheckout')
begin
	insert AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	values('SecurityCodeRequiredOnCheckout','SECURITY','If true, the checkout process will require a customer to enter a security code (captcha code) to login or create an account.','false', 'boolean', 1);
end
go

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetProductsEntity]') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].[aspdnsf_GetProductsEntity]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_GetSimpleObjectEntityList]') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].[aspdnsf_GetSimpleObjectEntityList]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_SearchProductComments]') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].[aspdnsf_SearchProductComments]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_ShowDuplicateAppConfigs]') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].[aspdnsf_ShowDuplicateAppConfigs]
GO

/* ======== Fix product import ======== */
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_ImportProductPricing_XML]') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].[aspdnsf_ImportProductPricing_XML]
GO

CREATE PROCEDURE [dbo].[aspdnsf_ImportProductPricing_XML]
    @pricing nvarchar(max)

AS
BEGIN
SET NOCOUNT ON

CREATE TABLE #tmp (ProductID int, VariantID int, KitItemID int, Name nvarchar(400), KitGroup nvarchar(800), SKU nvarchar(50), SKUSuffix nvarchar(50), ManufacturerPartNumber nvarchar(50), Cost money, MSRP money, Price money, SalePrice money, Inventory int)
DECLARE @hdoc int, @retcode int
EXEC @retcode = sp_xml_preparedocument
                    @hdoc OUTPUT,
                    @pricing

INSERT #tmp
SELECT *
FROM OPENXML(@hdoc, '/productlist/productvariant', 2)
        WITH (ProductID int, VariantID int, KitItemID int, Name nvarchar(400), KitGroup nvarchar(800), SKU nvarchar(50), SKUSuffix nvarchar(50), ManufacturerPartNumber nvarchar(50), Cost money, MSRP money, Price money, SalePrice money, Inventory int)


UPDATE dbo.ProductVariant
SET Price = t.Price,
    SalePrice = nullif(t.SalePrice,0),
    Inventory = t.Inventory,
    Cost = t.cost,
	MSRP = t.msrp
FROM dbo.ProductVariant p
    join #tmp t  on p.ProductID = t.ProductID and p.VariantID = t.VariantID
WHERE KitItemID = 0

UPDATE dbo.KitItem
SET PriceDelta = t.Price
FROM dbo.KitItem k
    join #tmp t  on k.KitItemID = t.KitItemID
WHERE t.KitItemID > 0

exec sp_xml_removedocument @hdoc
DROP TABLE #tmp
END
GO

--Content security policy
if not exists(select * from AppConfig where Name = 'ContentSecurityPolicy.Enabled')
begin
	insert AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	values('ContentSecurityPolicy.Enabled','SECURITY','If true, application responses will output the necessary headers to direct browsers to block rendering the site in an iframe.','true', 'boolean', 1);
end
go

if not exists(select * from AppConfig where Name = 'ContentSecurityPolicy.X-Frame-Options')
begin
	insert AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	values('ContentSecurityPolicy.X-Frame-Options','SECURITY','If ContentSecurityPolicy.Enabled is set to true, this value will be used for the X-Frame-Options header.  Consult your developer before changing this value.','SAMEORIGIN', 'string', 1);
end
go

if not exists(select * from AppConfig where Name = 'ContentSecurityPolicy.Content-Security-Policy')
begin
	insert AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	values('ContentSecurityPolicy.Content-Security-Policy','SECURITY','If ContentSecurityPolicy.Enabled is set to true, this value will be used for the Content-Security-Policy header.  Consult your developer before changing this value.','frame-ancestors ''self''', 'string', 1);
end
go

if not exists(select * from AppConfig where Name = 'ContentSecurityPolicy.X-Content-Security-Policy')
begin
	insert AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	values('ContentSecurityPolicy.X-Content-Security-Policy','SECURITY','If ContentSecurityPolicy.Enabled is set to true, this value will be used for the X-Content-Security-Policy header.  Consult your developer before changing this value.','frame-ancestors ''self''', 'string', 1);
end
go

UPDATE AppConfig SET Name = 'AdminPwdChangeDays'
WHERE Name = 'PasswordChangeFrequency'

if not exists(select * from AppConfig where Name = 'NumberOfShippingMethodsToDisplay')
begin
	insert AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	values('NumberOfShippingMethodsToDisplay','CHECKOUT','The number of shipping methods to display on checkout before a ''More Options'' link is shown. A value of zero will disable this feature.','0', 'integer', 0);
end
go

/* ======== Speed up PABPEraseCCInfo ======== */
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_PABPEraseCCInfo]') AND objectproperty(id, N'IsProcedure') = 1)
    DROP proc [dbo].[aspdnsf_PABPEraseCCInfo]
GO

CREATE PROCEDURE [dbo].[aspdnsf_PABPEraseCCInfo]
    @CartType int

AS
SET NOCOUNT ON
UPDATE [dbo].[Orders] SET CardNumber = NULL WHERE CardNumber IS NOT NULL

UPDATE [dbo].[Address] SET CardNumber = NULL
FROM [dbo].[Address] a LEFT JOIN (SELECT DISTINCT CustomerID FROM [dbo].[Shoppingcart] WHERE CartType = @CartType) b ON a.CustomerID = b.CustomerID
WHERE CardNumber IS NOT NULL AND b.CustomerID IS NULL
GO

--phone required
if (not exists (select Name from AppConfig where Name='AddressPhoneRequired'))
begin
	insert into AppConfig (Name, ConfigValue, Description, GroupName, ValueType) 
	values('AddressPhoneRequired', 'true', 'If true, phone number is required for customer billing and shipping addresses.' , 'SECURITY', 'boolean')
end

if (not exists (select Name from AppConfig where Name='UseLegacySENameProvider'))
begin
	insert into AppConfig (Name, ConfigValue, Description, GroupName, ValueType) 
	values('UseLegacySENameProvider', 'true', 'If true, the search engine name provider will work with ascii names only, otherwise, the new UTF-8 aware provider will be used.' , 'GENERAL', 'boolean')
end

if (NOT EXISTS (SELECT Name FROM AppConfig WHERE Name='ContinueShoppingURL'))
begin
	INSERT INTO AppConfig (Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly) 
	VALUES (
		'ContinueShoppingURL', 
		'This can be set to the relative path of a page on your site (i.e. c-1-myproducts.aspx, products.htm, etc). Customers who click Continue Shopping on the shopping cart page will be redirected to the page defined by this setting instead of back to the page from which they came.', 
		'/', 'string', NULL, 'CHECKOUT', 0)
end

if (NOT EXISTS (SELECT Name FROM AppConfig WHERE Name='ContinueShopping.Enabled'))
begin
	INSERT INTO AppConfig (Name, ConfigValue, Description, GroupName, ValueType) 
	VALUES('ContinueShopping.Enabled', 'true', 'Show a continue shopping link on the checkout page.' , 'CHECKOUT', 'boolean')
end

/* ======== 404 Suggestions ======== */
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[aspdnsf_Get404Suggestions]'))
    DROP PROC [dbo].[aspdnsf_Get404Suggestions]
GO

CREATE PROC [dbo].[aspdnsf_Get404Suggestions]
	@storeId INT = 0

AS
BEGIN
    SET NOCOUNT ON

	DECLARE @suggestionTypes VARCHAR(100) = (SELECT TOP 1 CASE ConfigValue WHEN '' THEN 'product, category, manufacturer, section, topic' ELSE ConfigValue END FROM [dbo].[AppConfig] WITH(NOLOCK) WHERE [Name] = '404.VisibleSuggestions' AND (StoreID=@storeId OR StoreID=0) ORDER BY StoreID desc)
	DECLARE @hideInventoryLevel INT = (SELECT TOP 1 ConfigValue FROM [dbo].[AppConfig] WITH(NOLOCK) WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel' AND (StoreID=@storeId OR StoreID=0) ORDER BY StoreID desc)
	
	DECLARE @filterProducts BIT = (SELECT ConfigValue FROM [dbo].[GlobalConfig] WITH(NOLOCK) WHERE [Name] = 'AllowProductFiltering')
	DECLARE @filterTopics BIT = (SELECT ConfigValue FROM [dbo].[GlobalConfig] WITH(NOLOCK) WHERE [Name] = 'AllowTopicFiltering')
	DECLARE @filterEntities BIT = (SELECT ConfigValue FROM [dbo].[GlobalConfig] WITH(NOLOCK) WHERE [Name] = 'AllowEntityFiltering')

	CREATE TABLE #UnfilteredEntities 
	(
		Id INT NOT NULL IDENTITY(1,1),
		ObjectType VARCHAR(100) NOT NULL,
		Name NVARCHAR(400) NOT NULL,
		[Description] NVARCHAR(MAX)
	)

	--Products
	IF @suggestionTypes LIKE '%product%'
	BEGIN
		INSERT INTO #UnfilteredEntities(ObjectType, Name, [Description])
		SELECT 'product', 
			p.Name, 
			p.[Description]
		FROM Product p WITH (NOLOCK)
			INNER JOIN ProductVariant pv ON p.ProductID = pv.ProductID AND pv.IsDefault = 1
			LEFT JOIN (SELECT VariantID, SUM(Quan) AS Inventory
						FROM Inventory
						GROUP BY VariantID) i on pv.VariantID = i.VariantID
			LEFT JOIN ProductStore ps ON p.ProductID = ps.ProductID AND (@filterProducts = 0 OR ps.StoreID = @storeId)
		WHERE p.Deleted = 0
			AND p.Published = 1
			AND pv.Deleted = 0
			AND pv.Published = 1
			AND (CASE p.TrackInventoryBySizeAndColor 
					WHEN 1 THEN ISNULL(i.Inventory, 0) 
					ELSE pv.Inventory 
				END >= @hideInventoryLevel or @hideInventoryLevel = -1)
	END

	--Topics
	IF @suggestionTypes LIKE '%topic%'
	BEGIN
		INSERT INTO #UnfilteredEntities(ObjectType, Name, [Description])
		SELECT 'topic', 
			t.Name, 
			t.Title
		FROM Topic t
		WHERE t.Published = 1
			AND t.ShowInSiteMap = 1
			AND (@filterTopics = 0 OR t.StoreID = @storeId)
	END

	--Categories
	IF @suggestionTypes LIKE '%category%'
	BEGIN
		INSERT INTO #UnfilteredEntities(ObjectType, Name, [Description])
			SELECT 'category', 
			c.Name,
			c.[Description]
		FROM Category c
		WHERE c.Published = 1
			AND c.Deleted = 0
			AND (@filterEntities = 0 OR (c.CategoryID IN (SELECT DISTINCT EntityID FROM EntityStore WHERE EntityType = 'category' AND StoreID = @storeId)))
	END
	
	--Manufacturers
	IF @suggestionTypes LIKE '%manufacturer%'
	BEGIN
		INSERT INTO #UnfilteredEntities(ObjectType, Name, [Description])
			SELECT 'manufacturer', 
			m.Name,
			m.[Description]
		FROM Manufacturer m
		WHERE m.Published = 1
			AND m.Deleted = 0
			AND (@filterEntities = 0 OR (m.ManufacturerID IN (SELECT DISTINCT EntityID FROM EntityStore WHERE EntityType = 'manufacturer' AND StoreID = @storeId)))
	END
	
	--Manufacturers
	IF @suggestionTypes LIKE '%section%'
	BEGIN
		INSERT INTO #UnfilteredEntities(ObjectType, Name, [Description])
			SELECT 'section', 
			s.Name,
			s.[Description]
		FROM Section s
		WHERE s.Published = 1
			AND s.Deleted = 0
			AND (@filterEntities = 0 OR (s.SectionID IN (SELECT DISTINCT EntityID FROM EntityStore WHERE EntityType = 'section' AND StoreID = @storeId)))
	END

	SELECT * FROM #UnfilteredEntities
END
GO

UPDATE AppConfig SET Description = 'Maximum number of line items a shopper can have in their cart in order to check out. Quantities do not matter; this setting considers the number of separate items in the cart.' 
WHERE Name = 'MaxCartItemsBeforeCheckout'

UPDATE AppConfig SET Description = 'Comma-separated list of CreditCardTypeIDs (integers) which 3-D Secure transactions can be processed for with the currently active gateway. You can see a list of credit card type IDs by choosing Configuration, Credit Card Types from within admin. Note that this setting has no effect if your store is using Braintree. Configure Braintree in Configuration, Site Setup Wizard to add 3-D Secure features to Braintree.' 
WHERE Name = '3DSecure.CreditCardTypeIDs'

UPDATE AppConfig SET Description = 'If this is set to STAY, following after an add-to-cart action, the shopper remains on the page and a message appears indicating the product has been added to the cart. If blank (the default), the shopper is redirected to the checkout page. Applies only if Minicart.enabled = False.' 
WHERE Name = 'AddToCartAction'

--Rename PayPal Bill Me Later to PayPal Credit
UPDATE AppConfig
SET [Description] = REPLACE([Description], '(formerly Bill Me Later)', '')
WHERE [Description] LIKE '%(formerly Bill Me Later)%'

UPDATE AppConfig
SET Name = 'PayPal.Express.PayPalCreditButtonURL'
WHERE Name = 'PayPal.Express.BillMeLaterButtonURL'

UPDATE AppConfig
SET Name = 'PayPal.Express.ShowPayPalCreditButton'
WHERE Name = 'PayPal.Express.ShowBillMeLaterButton'

UPDATE Orders
SET PaymentMethod = 'PAYPALCREDIT'
WHERE PaymentMethod = 'PAYPALBILLMELATER'

-- Upsell cart products stored procedure
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetUpsellProductsForCart]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetUpsellProductsForCart]
GO
CREATE PROCEDURE [dbo].[aspdnsf_GetUpsellProductsForCart]
	@customerId			INT,
	@customerlevelId	INT,
	@invFilter			INT,
	@storeId			INT = 1,
	@upsellProductsLimitNumberOnCart			INT = 4,
	@filterProduct		BIT = 0
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @tmp TABLE (ProductId INT, UpsellProductID INT)
    DECLARE @upsellProducts VARCHAR(8000), @pId INT
    SET @upsellProducts = ''

    SELECT TOP 1 @pId = p.ProductID, @upsellProducts = ISNULL(CAST(UpsellProducts AS VARCHAR(8000)), '')
    FROM ShoppingCart sc WITH (NOLOCK) JOIN Product p WITH (NOLOCK) ON sc.ProductID = p.ProductID 
    WHERE CustomerID = @customerId
    ORDER BY sc.ProductID 

    WHILE @@rowcount > 0 BEGIN
        INSERT @tmp SELECT @pId, CONVERT(INT, s.items) UpsellProductID FROM dbo.split(@UpsellProducts , ',') s LEFT JOIN @tmp t on t.UpsellProductID = CONVERT(INT, s.Items) WHERE t.ProductId IS NULL
                    
        SELECT TOP 1 @pId = p.ProductID, @upsellProducts = ISNULL(CAST(UpsellProducts AS VARCHAR(8000)), '')
        FROM ShoppingCart sc WITH (NOLOCK) JOIN Product p WITH (NOLOCK) ON sc.ProductID = p.ProductID 
        WHERE CustomerID = @customerId AND sc.ProductID > @pId
        ORDER BY sc.ProductID 
    END
                
    --For multi store. Delete items not included in a specific store.
    IF (@filterProduct = 1)
	BEGIN
		DELETE @tmp WHERE UpsellProductID NOT IN (SELECT ProductID FROM ProductStore WHERE StoreID = @storeId)
	END  

    SELECT TOP (@upsellProductsLimitNumberOnCart) 1-(p2.UpsellProductDiscountPercentage/100) UpsellDiscMultiplier,
            p.ProductID, p.Name, p.SEName, p.Summary, p.Description, RTRIM(pv.Name) VariantName, p.SEAltText, p.ImageFilenameOverride, p.TaxClassID,
            p.SKU, pv.VariantID, p.HidePriceUntilCart, pv.Price, ISNULL(pv.SalePrice, 0) SalePrice, ISNULL(ep.Price, 0) ExtendedPrice, pv.Points, 
            sp.Name SalesPromptName
    FROM @tmp t
        JOIN Product p WITH (NOLOCK) ON p.ProductID  = t.UpsellProductID  
        JOIN Product p2 WITH (NOLOCK) ON p2.ProductID  = t.ProductId  
        JOIN ProductVariant pv WITH (NOLOCK) ON p.ProductID = pv.productid AND pv.IsDefault = 1
        LEFT JOIN SalesPrompt sp  with (NOLOCK) ON p.SalesPromptID = sp.SalesPromptID
        LEFT JOIN ExtendedPrice ep  with (NOLOCK) ON ep.VariantID = pv.VariantID and ep.CustomerLevelID = @customerlevelId
        LEFT JOIN (SELECT VariantID, SUM(quan) quan FROM Inventory WITH (NOLOCK) GROUP BY VariantID) i ON i.VariantID = pv.VariantID
    where (p.RequiresTextOption IS NULL OR p.RequiresTextOption=0) 
        AND (ISNULL(CONVERT(NVARCHAR(4000), Sizes), '') = '' OR CONVERT(NVARCHAR(4000), Sizes) NOT LIKE '%>[^<>]%_[^<>]%<%') 
        AND (ISNULL(CONVERT(NVARCHAR(4000), Colors), '') = '' OR CONVERT(NVARCHAR(4000), Colors) NOT LIKE '%>[^<>]%_[^<>]%<%') 
        AND p.IsAKit=0 AND p.IsCallToOrder = 0 AND pv.CustomerEntersPrice = 0
		AND (pv.Sizes = '' OR pv.Sizes IS NULL) AND (pv.Colors = '' OR pv.Colors IS NULL)
        AND p.ProductID NOT IN (SELECT ProductID FROM ShoppingCart  WITH (NOLOCK) WHERE CartType=0 and CustomerID=@customerId)
        AND CASE p.TrackInventoryBySizeAndColor WHEN 1 THEN ISNULL(i.quan, 0) ELSE pv.inventory END >= @invFilter
        AND p.Published > 0
END
GO

--Remove Shiprush
IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('ShippingMethod') AND name = 'ShipRushTemplate')
	ALTER TABLE [dbo].[ShippingMethod] DROP COLUMN [ShipRushTemplate]
GO
DELETE FROM [dbo].[AppConfig] WHERE Name LIKE '%shiprush%'

--Normalize an AppConfig name
UPDATE AppConfig SET Name = 'ShowNewsOnHomePage' WHERE Name = 'DoNotShowNewsOnHomePage'
UPDATE AppConfig 
SET ConfigValue = 
	CASE
		WHEN ConfigValue = 'false' THEN 'true'
		ELSE 'false'
	END
WHERE Name = 'ShowNewsOnHomePage'

--Change AllowCouponFiltering name
UPDATE [dbo].[GlobalConfig] SET Name = 'AllowPromotionFiltering' WHERE Name = 'AllowCouponFiltering'

--Update descriptions of search appconfigs
UPDATE AppConfig SET Description = 'If TRUE, category matches appear within the search results.' 
WHERE Name = 'Search_ShowCategoriesInResults'

UPDATE AppConfig SET Description = 'If TRUE, distributor matches appear within the search results.' 
WHERE Name = 'Search_ShowDistributorsInResults'

UPDATE AppConfig SET Description = 'If TRUE, manufacturer matches appear within the search results.' 
WHERE Name = 'Search_ShowManufacturersInResults'

UPDATE AppConfig SET Description = 'If TRUE, product matches appear within the search results.' 
WHERE Name = 'Search_ShowProductsInResults'

UPDATE AppConfig SET Description = 'If TRUE, section matches appear within the search results.' 
WHERE Name = 'Search_ShowSectionsInResults'

--Update description of USPS AppConfig for Canada change
UPDATE AppConfig SET Description = 'Contains a list of available USPS Services for domestic USPS rates requests. Valid services are: Express, Priority, Parcel, Library, Media' WHERE Name = 'RTShipping.USPS.Services'

--Update AppConfig descriptions
UPDATE AppConfig SET [Description] = 'If TRUE, the software displays related products based on shopper product viewing behavior. Please see AspDotNetStorefront help for more information on Related Products.' 
WHERE Name = 'DynamicRelatedProducts.Enabled'

UPDATE AppConfig SET [Description] = 'The number of columns on the home page featured items.  Valid values are 1 - 6. 4 is the default value.' 
WHERE Name = 'FeaturedProducts.NumberOfColumns'

-- Add directory paths and ports to the store table
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Store]') AND name = 'ProductionDirectoryPath')
	ALTER TABLE Store Add ProductionDirectoryPath nvarchar(max)

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Store]') AND name = 'StagingDirectoryPath')
	ALTER TABLE Store Add StagingDirectoryPath nvarchar(max)

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Store]') AND name = 'DevelopmentDirectoryPath')
	ALTER TABLE Store Add DevelopmentDirectoryPath nvarchar(max)

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Store]') AND name = 'ProductionPort')
	ALTER TABLE Store Add ProductionPort nvarchar(10)

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Store]') AND name = 'StagingPort')
	ALTER TABLE Store Add StagingPort nvarchar(10)

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Store]') AND name = 'DevelopmentPort')
	ALTER TABLE Store Add DevelopmentPort nvarchar(10)

/* ======== Update Store cloning ======== */
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[aspdnsf_CloneStore]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
    DROP PROC [dbo].[aspdnsf_CloneStore]
GO

CREATE PROC [dbo].[aspdnsf_CloneStore]
	@StoreID INT,
	@NewStoreName nvarchar(400) = NULL,
	@NewStoreID int output
AS
BEGIN
	INSERT INTO Store (ProductionURI, ProductionDirectoryPath, ProductionPort, StagingURI, StagingDirectoryPath, StagingPort, DevelopmentURI, DevelopmentDirectoryPath, DevelopmentPort, [Name], Description, SkinID)
	SELECT ProductionURI, ProductionDirectoryPath, ProductionPort, StagingURI, StagingDirectoryPath, StagingPort, DevelopmentURI, DevelopmentDirectoryPath, DevelopmentPort, ISNULL(@NewStoreName, [Name]), Description, SkinID
	FROM Store WHERE StoreID = @StoreID

	select @NewStoreID=Max(StoreID) FROM Store

	EXEC [aspdnsf_CloneStoreMappings] @StoreID, @NewStoreID

END
GO

-- Featured products stored procedure
if exists(select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetFeaturedProducts]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[aspdnsf_GetFeaturedProducts]
go

create procedure [dbo].[aspdnsf_GetFeaturedProducts](
	@NumHomePageFeaturedProducts	int,
	@CustomerLevelID				int,
	@InventoryFilter				int = 0,
	@StoreID						int = 1,
	@FilterProduct					bit = 0 )
as begin
	set nocount on

	select top (@NumHomePageFeaturedProducts)
		p.ProductID,
		p.ImageFilenameOverride,
		p.SKU,
		p.SEName,
		p.Name,
		p.Description,
		p.TaxClassID,
		pv.VariantID,
		p.HidePriceUntilCart,
		pv.name VariantName,
		pv.Price,
		pv.Description VariantDescription,
		isnull(pv.SalePrice, 0) SalePrice,
		isnull(SkuSuffix, '') SkuSuffix,
		pv.Dimensions,
		pv.Weight,
		isnull(pv.Points, 0) Points,
		pv.Inventory,
		pv.ImageFilenameOverride VariantImageFilenameOverride,
		pv.isdefault,
		pv.CustomerEntersPrice,
		isnull(pv.colors, '') Colors,
		isnull(pv.sizes, '') Sizes,
		sp.name SalesPromptName,
		case 
			when pcl.productid is null then 0 
			else isnull(e.Price, 0) 
		end ExtendedPrice
	from
		Product p with (nolock)
		inner join ProductVariant pv with (nolock) on pv.ProductID = p.ProductID
		join dbo.SalesPrompt sp with (nolock) on p.SalesPromptID = sp.SalesPromptID
		left join dbo.ExtendedPrice e  with (nolock) on pv.VariantID=e.VariantID and e.CustomerLevelID = @CustomerLevelID
		left join dbo.ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID
		left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i  on pv.VariantID = i.VariantID
		left join ProductStore ps with (nolock) on ps.ProductID = p.ProductID and ps.StoreID = @StoreID and @FilterProduct = 1
	where 
		p.IsFeatured=1
		and p.Deleted=0
		and p.Published = 1
		and pv.IsDefault = 1
		and (
			(case p.TrackInventoryBySizeAndColor 
				when 1 then isnull(i.quan, 0) 
				else pv.inventory 
			end >= @InventoryFilter)
			or @InventoryFilter = -1 )
		and (@FilterProduct = 0 or ps.ProductID is not null)
	order by 
		newid()
end
go

-- Insert customer stored procedure
if exists(select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_insCustomer]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[aspdnsf_insCustomer]
go

create proc [dbo].[aspdnsf_insCustomer]
	@Email nvarchar(100),
	@Password nvarchar(100),
	@SkinID int,
	@AffiliateID int,
	@Referrer nvarchar(max),
	@IsAdmin tinyint,
	@LastIPAddress varchar(40),
	@LocaleSetting nvarchar(10),
	@Over13Checked tinyint,
	@CurrencySetting nvarchar(10),
	@VATSetting int,
	@VATRegistrationID nvarchar(100),
	@CustomerLevelID int,
	@StoreID int = 1,
	@CustomerID int output

as begin
	set nocount on

	insert dbo.Customer(CustomerGUID, CustomerLevelID, RegisterDate, Email, Password, Gender, FirstName, LastName, Notes, SkinID, Phone, AffiliateID, Referrer, CouponCode, OkToEmail, IsAdmin, BillingEqualsShipping, LastIPAddress, OrderNotes, RTShipRequest, RTShipResponse, OrderOptions, LocaleSetting, MicroPayBalance, RecurringShippingMethodID, RecurringShippingMethod, BillingAddressID, ShippingAddressID, ExtensionData, FinalizationData, Deleted, CreatedOn, Over13Checked, CurrencySetting, VATSetting, VATRegistrationID, StoreCCInDB, IsRegistered, LockedUntil, AdminCanViewCC, PwdChanged, BadLoginCount, LastBadLogin, Active, PwdChangeRequired, SaltKey, StoreID)
	values
	(
		newid(),
		@CustomerLevelID,
		getdate(),
		isnull(@Email, ''),
		isnull(@Password, ''),
		null,
		null,
		null,
		null,
		isnull(@SkinID, 1),
		null,
		@AffiliateID,
		@Referrer,
		null,
		1,
		isnull(@IsAdmin, 0),
		0,
		@LastIPAddress,
		null,
		null,
		null,
		null,
		isnull(@LocaleSetting, ('en-US')),
		0.0,
		1,
		null,
		null,
		null,
		null,
		null,
		0,
		getdate(),
		@Over13Checked,
		@CurrencySetting,
		@VATSetting,
		@VATRegistrationID,
		1,
		0,
		null,
		1,
		getdate(),
		0,
		null,
		1,
		0,
		0,
		@StoreID
	)

	set @CustomerID = scope_identity()
end
go

if exists(select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_ImportProductPricing_XML]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[aspdnsf_ImportProductPricing_XML]
go
create procedure [dbo].[aspdnsf_ImportProductPricing_XML]
	@document xml
as
begin
	set nocount on

	declare @productImport table
	(
		ProductId int,
		VariantId int,
		KitItemId int,
		Name nvarchar(400),
		KitGroup nvarchar(800),
		SKU nvarchar(50),
		SKUSuffix nvarchar(50),
		ManufacturerPartNumber nvarchar(50),
		Cost money,
		MSRP money,
		Price money,
		SalePrice money,
		Inventory int
	)

	insert into @productImport
		select
			T.c.value('(ProductID)[1]', 'int'),
			T.c.value('(VariantID)[1]', 'int'),
			T.c.value('(KitItemID)[1]', 'int'),
			T.c.value('(Name)[1]', 'nvarchar(max)'),
			T.c.value('(KitGroup)[1]', 'nvarchar(max)'),
			T.c.value('(SKU)[1]', 'nvarchar(max)'),
			T.c.value('(SKUSuffix)[1]', 'nvarchar(max)'),
			T.c.value('(ManufacturerPartNumber)[1]', 'nvarchar(max)'),
			T.c.value('(Cost)[1]', 'money'),
			T.c.value('(MSRP)[1]', 'money'),
			T.c.value('(Price)[1]', 'money'),
			T.c.value('(SalePrice)[1]', 'money'),
			T.c.value('(Inventory)[1]', 'int')
		from 
			@document.nodes('productlist/productvariant') T(c)	

	update dbo.ProductVariant
		set Price = productImport.Price,
			SalePrice = nullif(productImport.SalePrice,0),
			Inventory = productImport.Inventory,
			Cost = productImport.cost,
			MSRP = productImport.msrp
		from dbo.ProductVariant p
			join @productImport productImport  on p.ProductID = productImport.ProductID and p.VariantID = productImport.VariantID
		where KitItemID = 0

	update dbo.KitItem
		set PriceDelta = productImport.Price
		from dbo.KitItem k
			join @productImport productImport  on k.KitItemID = productImport.KitItemID
		where productImport.KitItemID > 0
end
go

-- Drop default order options column
IF EXISTS(SELECT * FROM sys.default_constraints WHERE OBJECT_NAME(parent_object_id) = 'OrderOption' and  name ='DF_OrderOption_DefaultIsChecked')
BEGIN
	ALTER TABLE OrderOption DROP CONSTRAINT DF_OrderOption_DefaultIsChecked
END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[OrderOption]') AND name = 'DefaultIsChecked')
BEGIN
	ALTER TABLE OrderOption DROP COLUMN DefaultIsChecked
END
GO

-- Recent additions
IF NOT EXISTS(SELECT * FROM AppConfig WHERE Name = 'RecentAdditionsNumDays')
BEGIN
	INSERT AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	VALUES('RecentAdditionsNumDays','MISC','The number of days back from the current date to pull products using the CreatedOn date for display on the recent additions page.','180', 'integer', 0);
END

IF NOT EXISTS(SELECT * FROM AppConfig WHERE Name = 'RecentAdditionsN')
BEGIN
	INSERT AppConfig (Name, GroupName, Description, ConfigValue, ValueType, SuperOnly)
	VALUES('RecentAdditionsN','MISC','The maximum number of products to display on the recent additions page.','100', 'integer', 0);
END

--Monthly Maintenance alert
IF NOT EXISTS(SELECT * FROM AppConfig WHERE Name = 'NextMaintenanceDate')
BEGIN
	INSERT AppConfig (Name, GroupName, [Description], ConfigValue, ValueType, SuperOnly)
	VALUES('NextMaintenanceDate','SYSTEM','Internal use. Do not modify.',DATEADD(dy, 30, GETDATE()), 'string', 1);
END

/* ======== New Skin Topics ======== */
if not exists (select * from [dbo].[Topic] where Name = 'HomeTopIntro')
	insert into [dbo].[Topic] ([TopicGUID], [Name], [Title], [HTMLOk], [Published], [IsFrequent], [Description]) 
	values (newid(), N'HomeTopIntro', N'Home Page Contents', 1, 1, 1, N'
		(!TOPIC Name="HomePage.HomeImage"!)
		<div class="row">
			<div class="col-md-4">
				<div class="home-image">
					<img src="(!SkinPath!)/images/home1.jpg" alt="Store Image One" class="img-responsive center-block" />
				</div>
			</div>
			<div class="col-md-4">
				<div class="home-image">
					<img src="(!SkinPath!)/images/home2.jpg" alt="Store Image Two" class="img-responsive center-block" />
				</div>
			</div>
			<div class="col-md-4">
				<div class="home-image">
					<img src="(!SkinPath!)/images/home3.jpg" alt="Store Image Three" class="img-responsive center-block" />
				</div>
			</div>
		</div>

		<div class="row">
			<div class="col-md-6">
				<p>Your AspDotNetStorefront store is a thing of beauty right out of the box, but it''s almost certain that you''re going to want to stamp your own brand identity onto the ''skin''.</p>
				<h3>Three ways to personalize your store design</h3>
				<div class="row">
					<div class="col-md-4">
						<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=adminskinning" class="thumbnail" target="_blank">
							<img alt="Use the admin wizard" src="(!SkinPath!)/images/box1.jpg" />
						</a>
					</div>
					<div class="col-md-4">
						<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=learntoskin" class="thumbnail" target="_blank">
							<img alt="Learn how to skin a store" src="(!SkinPath!)/images/box2.jpg" />
						</a>
					</div>
					<div class="col-md-4">
						<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=expertskinning" class="thumbnail" target="_blank">
							<img alt="Hire the experts to help" src="(!SkinPath!)/images/box3.jpg" />
						</a>
					</div>
				</div>
				<div class="row">
					<div class="col-md-4 text-center">
						<h4>
							<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=adminskinning" target="_blank">
								Use the provided admin wizard
							</a>
						</h4>
					</div>
					<div class="col-md-4 text-center">
						<h4>
							<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=learntoskin" target="_blank">Learn how to ''skin'' a store</a>
						</h4>
					</div>
					<div class="col-md-4 text-center">
						<h4>
							<a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=expertskinning" target="_blank">Hire the experts to help</a>
						</h4>
					</div>
				</div>
			</div>
			<div class="col-md-6">
				<div class="home-image">
					<img src="(!SkinPath!)/images/home4.jpg" alt="Store Image Four" class="img-responsive center-block" />
				</div>
			</div>
		</div>
		')

if not exists (select * from [dbo].[Topic] where Name = 'HomePage.HomeImage')
	insert into [dbo].[Topic] ([TopicGUID], [Name], [Title], [HTMLOk], [Published], [IsFrequent], [ShowInSiteMap], [Description]) 
	values (newid(), N'HomePage.HomeImage', N'The main home page image area', 1, 1, 1, 0, N'
	<div class="home-image home-main-image">
		<img src="(!SkinPath!)/images/home.jpg" alt="AspDotNetStorefront" class="img-responsive center-block" />
	</div>
	')

if not exists (select * from [dbo].[Topic] where Name = 'Template.Logo')
	insert into [dbo].[Topic] ([TopicGUID], [Name], [Title], [HTMLOk], [Published], [IsFrequent], [Description]) 
	values (newid(), N'Template.Logo', N'Template.Logo', 1, 1, 1, N'
	<a id="logo" class="logo" href="(!Url ActionName=''Index'' ControllerName=''Home''!)" title="This image size is 250px x 87px">
		<img src="(!SKINPATH!)/images/logo.jpg" alt="YourCompany.com" />
	</a>
	')

if not exists (select * from [dbo].[Topic] where Name = 'Template.Footer')
	insert into [dbo].[Topic] ([TopicGUID], [Name], [Title], [HTMLOk], [Published], [IsFrequent], [Description]) 
	values (newid(), N'Template.Footer', N'The footer section of the template', 1, 1, 1, N'
	<div class="row">
		<div class="col-sm-12 col-md-3">
			<ul class="footer-list">
				<li class="footer-heading">Location & Hours</li>
				<li>1234 Main St.</li>
				<li>Ashland, OR 97520</li>
				<li>Phone: 541-867-5309</li>
				<li>M-F 9am - 5pm</li>
				<li><a href="(!Url ActionName=''Index'' ControllerName=''ContactUs''!)">Contact Us</a></li>
			</ul>
		</div>
		<div class="col-sm-12 col-md-3">
			<ul class="footer-list">
				<li class="footer-heading">Store Policies</li>
				<li><a href="(!TopicLink name=''security''!)">Security</a></li>
				<li><a href="(!TopicLink name=''privacy''!)">Privacy Policy</a></li>
				<li><a href="(!TopicLink name=''returns''!)">Return Policy</a></li>
				<li><a href="(!TopicLink name=''service''!)">Customer Service</a></li>
			</ul>
		</div>
		<div class="col-sm-12 col-md-3">
			<ul class="footer-list">
				<li class="footer-heading">Store Information</li>
				<li><a href="(!Url ActionName=''Index'' ControllerName=''Account''!)">Account</a></li>
				<li><a href="(!Url ActionName=''Index'' ControllerName=''Account''!)#OrderHistory">Order Tracking</a></li>
				<li><a href="(!Url ActionName=''Index'' ControllerName=''SiteMap''!)">Site Map</a></li>
			</ul>
		</div>
		<div class="col-sm-12 col-md-3">
			<ul class="footer-list">
				<li>
					<div class="social-links">
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=facebook"><i class="icon fa fa-facebook"></i></a>		
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=instagram"><i class="icon fa fa-instagram"></i></a>
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=pinterest"><i class="icon fa fa-pinterest"></i></a>	
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=twitter"><i class="icon fa fa-twitter"></i></a>				
						<a target="_blank" href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000skin&type=youtube"><i class="icon fa fa-youtube"></i></a>		
					</div>
				</li>
				<li>
					<div class="seal-marker">
						<img src="(!SkinPath!)/images/seal.png" />
					</div>
				</li>
			</ul>
		</div>
	</div>
	')

-- Remove mobilelink xmlpackage token --
UPDATE topic SET description = REPLACE(description, '(!XmlPackage Name="mobilelink"!)', '')

--Update AppConfig Descriptions
Update AppConfig SET Description = 'If True, "Out-of-Stock" or "In-stock" labels appear on product listing pages (entities) such as category pages. Has no affect if DisplayOutOfStockProducts = False.' WHERE Name = 'DisplayOutOfStockOnEntityPages'
Update AppConfig SET Description = 'If True, "Out-of-Stock" or "In-stock" labels appear on product detail pages. Has no affect if DisplayOutOfStockProducts = False.' WHERE Name = 'DisplayOutOfStockOnProductPages'
Update AppConfig SET Description = 'If True, "Out-of-Stock" or "In-stock" labels appear depending on product inventory levels. If the inventory is less than the OutOfStockThreshold value, "Out-of-Stock" labels appear; otherwise, "In-stock" labels will appear.' WHERE Name = 'DisplayOutOfStockProducts'
Update AppConfig SET Description = 'Hides from all pages products whose inventory falls *below* the value you set here. For example, to hide all products with inventory of 5, set this value to 6. Set this value to -1 to show all products regardless of inventory level. ' WHERE Name = 'HideProductsWithLessThanThisInventoryLevel'
Update AppConfig SET Description = 'If True, add-to-cart buttons display a warning if shoppers add a quantity greater than inventory on-hand. Further, the quantity value reverts to quantity on hand, making it impossible for shoppers to add a greater quantity than you may have on hand. Set False to turn off these limitations, enabling shoppers to order greater quantities than you may have on hand.' WHERE Name = 'Inventory.LimitCartToQuantityOnHand'
Update AppConfig SET Description = 'If True, AspDotNetStorefront allows shoppers to purchase a kit item even when the kit item is out of stock. ' WHERE Name = 'KitInventory.AllowSaleOfOutOfStock'
Update AppConfig SET Description = 'If True, "Out-of-Stock" or "In-stock" labels appear on product kit pages.' WHERE Name = 'KitInventory.ShowOutOfStockMessage'
Update AppConfig SET Description = 'The quantity *below* which an item is considered out-of-stock. Has no affect if DisplayOutOfStockProducts is disabled.' WHERE Name = 'OutOfStockThreshold'
Update AppConfig SET Description = 'If True, instead of showing an "Out of Stock" message, "hidden" product pages will redirect to a 404 "page not found" error pages. Be aware that search engines remove from their records such 404 pages, which will negatively affect search engine rankings. Set this = True only for sites selling one-off or limited run products.' WHERE Name = 'ProductPageOutOfStockRedirect'
Update AppConfig SET Description = 'The default inventory level applied to new products. Users may modify inventory levels at any time.' WHERE Name = 'Admin_DefaultInventory'
Update AppConfig SET Description = 'The default Product type ID applied to a new products. Users may modify product type at any time.' WHERE Name = 'Admin_DefaultProductTypeID'
Update AppConfig SET Description = 'The default tax classification applied to new products. Users may create additional tax classes and override the default.' WHERE Name = 'Admin_DefaultTaxClassID'
Update AppConfig SET Description = 'The default Sales Prompt ID applied to new products. Sales prompts are optionally shown when products are "on sale." Users may choose different sales prompts at any time.' WHERE Name = 'Admin_DefaultSalesPromptID'
Update AppConfig SET Description = 'The number of days after which products are removed from customer carts. ' WHERE Name = 'AgeCartDays'
Update AppConfig SET Description = 'If TRUE, the customer can specify a shipping address which is different than their billing address. If false, then ONLY the billing address can be entered, and the shipping address will be set to match it.' WHERE Name = 'AllowShipToDifferentThanBillTo'
Update AppConfig SET Description = 'Duration (in minutes) between menu cache updates. This should not be set to a value much lower than 60, or the cache will not have time to build up enough to be of any use. Has no effect unless CacheMenus is set to TRUE.' WHERE Name = 'CacheDurationMinutes'
Update AppConfig SET Description = 'If TRUE, site menus and many, many other navigational and dataset elements on the store and admin panel are cached for performance reasons. If cached, these update every CacheDurationMinutes, so changes made on the admin site may not take effect until the cache expires. To force the store to reload cache, restart the site through IIS or by "touching" the web.config file in your root web folder. In production, CacheMenus should always be set to TRUE to improve performance.' WHERE Name = 'CacheMenus'
Update AppConfig SET Description = 'If FALSE, shoppers MUST enter their 3 or 4 digit card verification code (CVV). If TRUE, shoppers may enter the CVV, but it is not required by the javascript form validation routines.  Note that this controls on-site validation only, your payment gateway may require the CVV value regardless of this setting.' WHERE Name = 'CardExtraCodeIsOptional'
Update AppConfig SET Description = 'If TRUE, the shopper''s coupon code will be cleared in their customer record after each order. If FALSE, shopper''s coupon code will remain active in their customer record.  Generally best left set to TRUE.' WHERE Name = 'ClearCouponAfterOrdering'
Update AppConfig SET Description = 'If TRUE, shoppers'' old saved carts from previous sessions are cleared the next time the shopper logs in. Works in conjunction with the PreserveActiveCartOnSignin setting.' WHERE Name = 'ClearOldCartOnSignin'
Update AppConfig SET Description = 'If non-zero, the amount specified in this setting is added to all order totals using the C.O.D. payment method, for orders where the shipping total is already non zero. This cost is NOT added to orders where the shipping total computes to 0.00. This value should be a dollar amount, without leading $ or other currency character, e.g. 5.00' WHERE Name = 'CODHandlingExtraFee'
Update AppConfig SET Description = 'If TRUE, distributor e-mails are NOT automatically sent and you will therefore need to send the order e-mail manually from within the order details page. Set FALSE to automatically e-mail orders to distributors.' WHERE Name = 'DelayedDropShipNotifications'
Update AppConfig SET Description = 'If blank, each order is dispatched via SMS as it is received. Specify a dollar value, e.g. 100.00, to dispatch messages only if the order total exceeds the value you set here. You can use this if you only want to be notified on larger orders' WHERE Name = 'Dispatch_OrderThreshold'
Update AppConfig SET Description = 'If TRUE, news items appear on the home page. If FALSE, news items do not appear on the home page, even if news items exist. ' WHERE Name = 'ShowNewsOnHomePage'
Update AppConfig SET Description = 'If set to a dollar amount (no $ or other currency symbol), orders equal to or greater than the amount specified here have free shipping. A value of 0.00 or blank disables this feature.' WHERE Name = 'FreeShippingThreshold'
Update AppConfig SET Description = 'The name *From* which new order notifications are e-mailed. For example, "Sally Jane" or "Orders". This optional value may also be the same as the GotOrderEMailFrom value. This is used to notify store owners of new orders. ' WHERE Name = 'GotOrderEMailFromName'
Update AppConfig SET Description = 'If TRUE, add-to-cart buttons display a warning if shoppers add a quantity greater than inventory on-hand. Further, the quantity value reverts to quantity on hand, making it impossible for shoppers to add a greater quantity than you may have on hand. Set FALSE to turn off these limitations, enabling shoppers to order greater quantities than you may have on hand.' WHERE Name = 'Inventory.LimitCartToQuantityOnHand'
Update AppConfig SET Description = 'The domain of the live site. This is usually just domain.com for your site (use your own domain name). If your store runs on a subdomain, this value should be subdomain.domain.com.' WHERE Name = 'LiveServer'
Update AppConfig SET Description = 'Maximum number of line items a shopper can have in their cart in order to check out. Quantities are not relevant; this setting considers only the number of separate items in the cart.' WHERE Name = 'MaxCartItemsBeforeCheckout'
Update AppConfig SET Description = 'URI to the live PayPal API. Do not change this value without consulting PayPal support.' WHERE Name = 'PayPal.LiveServer'
Update AppConfig SET Description = 'To require shoppers to check out only with a confirmed PayPal shipping address, set this to Yes. To allow any address, set to No. Best practice recommend a Yes value.' WHERE Name = 'PayPal.RequireConfirmedAddress'
Update AppConfig SET Description = 'Use the PayPal Express Integrated checkout, which delivers the best shopper experience.' WHERE Name = 'PayPal.Express.UseIntegratedCheckout'
Update AppConfig SET Description = 'URL for PayPal Credit  button. Do not change this value without consulting PayPal support.' WHERE Name = 'PayPal.Express.PayPalCreditButtonURL'
Update AppConfig SET Description = 'Show the PayPal Credit  button on the checkout page.' WHERE Name = 'PayPal.Express.ShowPayPalCreditButton'
Update AppConfig SET Description = 'Specifies the URL to the page which handles PayPal callback notifications. Do not change this value without consulting PayPal support.' WHERE Name = 'PayPal.NotificationURL'
Update AppConfig SET Description = 'A URL for the image you want to appear at the top left of the PayPal payment page. The image has a maximum size of 750 pixels wide by 90 pixels high. PayPal recommends that you provide an image that is stored on a secure (https) server. Consult PayPal documentation for more information. ' WHERE Name = 'PayPal.Express.HeaderImage'
Update AppConfig SET Description = 'Sets the background color for the header of the PayPal payment page. Character length and limitation: Six character HTML hexadecimal color code in ASCII. Consult PayPal documentation for more information.' WHERE Name = 'PayPal.Express.HeaderBackColor'
Update AppConfig SET Description = 'Sets the border color around the header of the PayPal payment page. The border is a 2-pixel perimeter around the header space, which is 750 pixels wide by 90 pixels high. Character length and limitations: Six character HTML hexadecimal color code in ASCII. Consult PayPal documentation for more information.' WHERE Name = 'PayPal.Express.HeaderBorderColor'
Update AppConfig SET Description = 'Sets the background color for the PayPal payment page. Character length and limitation: Six character HTML hexadecimal color code in ASCII. Consult PayPal documentation for more information. ' WHERE Name = 'PayPal.Express.PayFlowColor'
Update AppConfig SET Description = 'If TRUE, instead of showing an "Out of Stock" message, "hidden" product pages will redirect to a 404 "page not found" error pages. Be aware that search engines remove from their records such 404 pages, which will negatively affect search engine rankings. Set this = TRUE only for sites selling one-off or limited run products.' WHERE Name = 'ProductPageOutOfStockRedirect'
Update AppConfig SET Description = 'If TRUE, shoppers'' anonymous cart contents will be moved into their cart when logging in or registering. Works in conjunction with the ClearOldCartOnSignin setting.' WHERE Name = 'PreserveActiveCartOnSignin'
Update AppConfig SET Description = 'The shipping carrier you will be using; may be a comma separated list of: UPS, USPS, FEDEX, DHL' WHERE Name = 'RTShipping.ActiveCarrier'
Update AppConfig SET Description = 'The shipping carrier you will be using for domestic shipments; may be a comma separated list of: UPS, USPS, FEDEX, DHL' WHERE Name = 'RTShipping.DomesticCarriers'
Update AppConfig SET Description = 'The shipping carrier you will be using for international shipments; may be a comma separated list of: UPS, USPS, FEDEX, DHL' WHERE Name = 'RTShipping.InternationalCarriers'
Update AppConfig SET Description = 'TRUE if you want the shipment insured; otherwise, FALSE.' WHERE Name = 'RTShipping.Insured'
Update AppConfig SET Description = 'If any Product variant does not have any weight specified in the Product variant, this default weight will be used (float number)' WHERE Name = 'RTShipping.DefaultItemWeight'
Update AppConfig SET Description = 'Line 1 of the physical address from which you ship.' WHERE Name = 'RTShipping.OriginAddress'
Update AppConfig SET Description = 'Line 2 of the physical address from which you ship.' WHERE Name = 'RTShipping.OriginAddress2'
Update AppConfig SET Description = 'The city from which you ship.' WHERE Name = 'RTShipping.OriginCity'
Update AppConfig SET Description = 'The state/province abbreviation from which you ship.' WHERE Name = 'RTShipping.OriginState'
Update AppConfig SET Description = 'The postal code from which you ship.' WHERE Name = 'RTShipping.OriginZip'
Update AppConfig SET Description = 'The 2-character country abbreviation from which you ship.' WHERE Name = 'RTShipping.OriginCountry'
Update AppConfig SET Description = 'The maximum weight allowed for a UPS shipment in the RTShipping.WeightUnits setting. If an order weight exceeds this value, then the CallForShippingPrompt appears as the shipping method, with a $0 price.' WHERE Name = 'RTShipping.UPS.MaxWeight'
Update AppConfig SET Description = 'The maximum weight allowed for a USPS shipment in the RTSHipping.WeightUnits setting. If an order weight exceeds this, then the CallForShippingPrompt setting is displayed as the shipping method, with a $0 price.' WHERE Name = 'RTShipping.USPS.MaxWeight'
Update AppConfig SET Description = 'If you want to mark up the rates returned by the carrier, enter the mark-up percentage here. For example, 5.0 adds 5% to each returned rate.' WHERE Name = 'RTShipping.MarkupPercent'
Update AppConfig SET Description = 'If TRUE, real-time shipping request and real-time shipping response XML will be shown on bottom of checkout page. This is used only for debugging purposes. Appears only in the checkout process for administrative users. You can send non-admins to /checkout/DebugRealtimeShipping to see the output as well.' WHERE Name = 'RTShipping.DumpDebugXmlOnCheckout'
Update AppConfig SET Description = 'If the order exceeds maximum shipping rate, the prompt specified here is shown as the shipping method with $0 price.' WHERE Name = 'RTShipping.CallForShippingPrompt'
Update AppConfig SET Description = 'The maximum allowed weight for a FedEx shipment, units of which are specified in the RTSHipping.WeightUnits setting. If an order weight exceeds the value specified here, then the CallForShippingPrompt setting is displayed as the shipping method, with a $0 price.' WHERE Name = 'RTShipping.FedEx.MaxWeight'
Update AppConfig SET Description = 'If TRUE, any errors returned from the real-time rate call will be displayed on the cart page. Very helpful for debugging real-time rate issues.' WHERE Name = 'RTShipping.ShowErrors'
Update AppConfig SET Description = 'A comma-separated list of real-time shipping methods which do not appear to the shopper. List must exactly match text description of the shipping methods, e.g. U. S. FedEx Same Day, FedEx Overnight, U.S. Postal Priority.' WHERE Name = 'RTShipping.ShippingMethodsToPrevent'
Update AppConfig SET Description = 'Used to determine if the real-time rates service contacts live servers or test servers' WHERE Name = 'RTShipping.UseTestRates'
Update AppConfig SET Description = 'For items shipping CanadaPost and combined into one box, this is the default size of that box in cm.' WHERE Name = 'RTShipping.CanadaPost.DefaultPackageSize'
Update AppConfig SET Description = 'Language code for CanadaPost. Valid values are en (English), fr (French), auto (default; selected based on customer locale).' WHERE Name = 'RTShipping.CanadaPost.Language'
Update AppConfig SET Description = 'CanadaPost Sell Online maximum package weight in kg.' WHERE Name = 'RTShipping.CanadaPost.MaxWeight'
Update AppConfig SET Description = 'CanadaPost-assigned Sell Online Merchant ID.' WHERE Name = 'RTShipping.CanadaPost.MerchantID'
Update AppConfig SET Description = 'DNS of CanadaPost Sell Online ratings server. Do not change without CanadaPost support.' WHERE Name = 'RTShipping.CanadaPost.Server'
Update AppConfig SET Description = 'TCP port of CanadaPost Sell Online ratings server. Do not change without CanadaPost support.' WHERE Name = 'RTShipping.CanadaPost.ServerPort'
Update AppConfig SET Description = 'The number of days from today to calculate the DHL Ship-On date.' WHERE Name = 'RTShipping.DHL.ShipInDays'
Update AppConfig SET Description = 'DHL billing  party. Valid codes are S (Sender, default), R (Receiver), and 3 (Third Party).' WHERE Name = 'RTShipping.DHLIntl.BillingParty'
Update AppConfig SET Description = 'DHL duty charge. Indicates whether or not the shipment is dutiable. Valid codes are Y (dutiable, default) or N (non-dutiable).' WHERE Name = 'RTShipping.DHLIntl.Dutiable'
Update AppConfig SET Description = 'DHL duty and tax charge payment type. Valid codes are S (Shipper, default), R (Receiver), or 3 (Third Party).' WHERE Name = 'RTShipping.DHLIntl.DutyPayment'
Update AppConfig SET Description = 'If DHL duty payment type is someone other than Sender or Receiver, enter the DHL account number to be billed.' WHERE Name = 'RTShipping.DHLIntl.DutyPaymentAccountNbr'
Update AppConfig SET Description = 'DHL packaging specifier. Valid codes are P (package, box or tube), or L (Letter, cannot exceed 0.5 lb)' WHERE Name = 'RTShipping.DHLIntl.Packaging'
Update AppConfig SET Description = 'DHL Service Level Code for the requested service levels. Valid codes are IE;DHL Worldwide Priority Express' WHERE Name = 'RTShipping.DHLIntl.Services'
Update AppConfig SET Description = 'For non-ShipSeparately items, the package dimensions to use, specified in cm. Australia Post only.' WHERE Name = 'RTShipping.AusPost.DefaultPackageSize'
Update AppConfig SET Description = 'List of domestic shipping classes, separated by commas. Each entry consists of a CODE;Description pair, separated by a semi-colon. Australia Post only.' WHERE Name = 'RTShipping.AusPost.DomesticServices'
Update AppConfig SET Description = 'List of Australia Post international shipping classes, separated by commas. Each entry consists of a CODE;Description pair, separated by a semi-colon.' WHERE Name = 'RTShipping.AusPost.IntlServices'
Update AppConfig SET Description = 'The maximum weight allowed for an Australia Post shipment, in kg. If an order weight exceeds this, then the CallForShippingPrompt setting is displayed as the shipping method, with a $0 price.' WHERE Name = 'RTShipping.AusPost.MaxWeight'
Update AppConfig SET Description = 'Set TRUE to sort the real-time rate list by increasing rate; otherwise FALSE;' WHERE Name = 'RTShipping.SortByRate'
Update AppConfig SET Description = 'Default page titles, if nothing else is found.' WHERE Name = 'SE_MetaTitle'
Update AppConfig SET Description = 'If a page doesn''t get a meta description from somewhere (e.g. a Product page), this is used by default.' WHERE Name = 'SE_MetaDescription'
Update AppConfig SET Description = 'If a page doesn''t get a meta keyword list from somewhere (e.g. a Product page), this is used by default.' WHERE Name = 'SE_MetaKeywords'
Update AppConfig SET Description = 'If TRUE, customer searches will be logged in the SearchLog table.' WHERE Name = 'Search_LogSearches'
Update AppConfig SET Description = 'Shopper session data timeout value, in minutes. Default is 15 minutes.' WHERE Name = 'SessionTimeoutInMinutes'
Update AppConfig SET Description = 'Admin session data timeout value, in minutes. Default is 15 minutes' WHERE Name = 'AdminSessionTimeoutInMinutes'
Update AppConfig SET Description = 'When shopper sessions end due to idleness, this page on your site appears. A blank value sends shopper to the site''s home page.' WHERE Name = 'SessionTimeoutLandingPage'
Update AppConfig SET Description = 'If TRUE, shoppers are warned before their sessions time out due to inactivity.' WHERE Name = 'SessionTimeoutWarning.Enabled'
Update AppConfig SET Description = 'The extra fee added to all shipping totals, for orders where the shipping total is already non-zero. The value specified here is NOT added to orders where the shipping total computes to 0.00. This value should be a dollar amount, without leading $ or other currency symbol, e.g. 5.00. ' WHERE Name = 'ShippingHandlingExtraFee'
Update AppConfig SET Description = 'A comma-separated list of shipping method IDs (one or more integers) which are marked as having free shipping. You can see the shipping method IDs for your site from with admin. Choose Configuration, Shipping Calculation within Admin, then click View Real-Time Shipping Methods or View Shipping Methods to view shipping methods and their IDs.' WHERE Name = 'ShippingMethodIDIfFreeShippingIsOn'
Update AppConfig SET Description = 'If TRUE, add-to-cart buttons appear on product pages. If FALSE, add-to-cart buttons do not appear on product pages. Useful when you are running a catalogue, informational, or gallery item site).' WHERE Name = 'ShowBuyButtons'
Update AppConfig SET Description = 'If TRUE, a quantity box appears on Product pages, enabling shoppers to specify a quantity of the product added to the cart.' WHERE Name = 'ShowQuantityOnProductPage'
Update AppConfig SET Description = 'If TRUE, add-to-wishlist buttons appear on product pages; otherwise FALSE.' WHERE Name = 'ShowWishButtons'
Update AppConfig SET Description = 'If TRUE, customer service notes appear on shopper receipts and on order history pages.' WHERE Name = 'ShowCustomerServiceNotesInReceipts'
Update AppConfig SET Description = 'If TRUE, email-this-product-to-a-friend buttons appear on product pages; otherwise FALSE.' WHERE Name = 'ShowEMailProductToFriend'
Update AppConfig SET Description = 'If TRUE, a product''s full name (product name + variant name) displays is always displayed in a right bar format. If FALSE (recommended) AspDotNetStorefront makes some decisions on what display is most appropriate.' WHERE Name = 'ShowFullNameInRightBar'
Update AppConfig SET Description = 'If TRUE, inventory-in-stock table appears for the product. Applies only for Products using advanced inventory management.' WHERE Name = 'ShowInventoryTable'
Update AppConfig SET Description = 'If a category has subcategories, subcategories appear in either list or grid format based on this flag, at the top of category pages.' WHERE Name = 'ShowSubcatsInGrid'
Update AppConfig SET Description = 'Specifies how the site handles credit cards in real-time when an order is entered. AUTH means that the card is ONLY authorized; you must use the admin console to later capture the amount, or process the card manually offline. AUTH CAPTURE means that the card is authorized AND captured in real-time.' WHERE Name = 'TransactionMode'
Update AppConfig SET Description = 'If Yes, AspDotNetStorefront calls the merchant gateway when processing an order. If No, AspDotNetStorefront does not call the merchant gateway and an OK status is returned. No is acceptable for development and testing purposes. Must always must be set to Yes for a "live" store site.' WHERE Name = 'UseLiveTransactions'
Update AppConfig SET Description = 'If TRUE, product image file name is taken from the Product SKU.jpg, SKU.gif, or SKU.png. If FALSE (the default), product image file names will be ProductID.JPG, ProductID.GIF or ProductID.PNG' WHERE Name = 'UseSKUForProductImageName'
Update AppConfig SET Description = 'Comma-separated list of CreditCardTypeIDs (integers) which 3-D Secure transactions are processed with the currently-active gateway. You can see a list of credit card type IDs by choosing Configuration, Credit Card Types from within admin. Note that this setting has no effect if your store is using Braintree. Configure Braintree in Configuration, Site Setup Wizard to add 3-D Secure features to Braintree.' WHERE Name = '3DSecure.CreditCardTypeIDs'
Update AppConfig SET Description = 'The shipping zone id to use if shipping by weight or total & zone is specified and the shopper postal code does not match any zone. This setting matches the Zone ID reported for the zone definition in the admin console.' WHERE Name = 'ZoneIdForNoMatch'
Update AppConfig SET Description = 'If TRUE, shoppers are emailed an order receipt when a recurring order is successfully processed (i.e. card charged OK, and new order created). If FALSE, order receipts are not e-mailed.' WHERE Name = 'Recurring.SendOrderEMailToCustomer'
Update AppConfig SET Description = 'If TRUE, shoppers are e-mailed a "your order has shipped" e-mail when a recurring order is marked as shipped. If FALSE, "your order has shipped" e-mails are not sent.' WHERE Name = 'Recurring.SendShippedEMail'
Update AppConfig SET Description = 'If TRUE, the gateway''s internal billing will be used instead of the built-in billing mechanism when processing recurring orders. This is ONLY allowed to be true if you are using the Authorize.net or PayPal PayFlow PRO gateways. If using these gateways, setting this flag to TRUE means that it is unnecessary to store credit card information in your database for recurring orders. Refer to Help for further instructions on how to process recurring order reports using each gateway.' WHERE Name = 'Recurring.UseGatewayInternalBilling'
Update AppConfig SET Description = 'If TRUE, recurrences of Auto-Ship orders will have the IsNew flag cleared automatically.' WHERE Name = 'Recurring.ClearIsNewFlag'
Update AppConfig SET Description = 'If TRUE, when a (normal, not recurring) order isCREATEd, an order receipt e-mail is sent to the customer. If FALSE, no order e-mail is sent.' WHERE Name = 'SendOrderEMailToCustomer'
Update AppConfig SET Description = 'If TRUE, an e-mail is sent to the shopper when an order is marked shipped within the admin console. The e-mail notifies the shopper that the order has shippped. Does not apply to recurring orders. ' WHERE Name = 'SendShippedEMailToCustomer'
Update AppConfig SET Description = 'If TRUE, the summary field in the Product will be used on category pages formatted in table order.' WHERE Name = 'ShowSummaryInTableOrderFormat'
Update AppConfig SET Description = 'If TRUE, customer level 0 (all customers) may use purchase orders (assuming the purchase order payment method is enabled).' WHERE Name = 'CustomerLevel0AllowsPOs'
Update AppConfig SET Description = 'If TRUE, products are filtered by the customer affiliate ID, and the Product to affiliate mappings are used in the ProductAffiliate table.' WHERE Name = 'FilterProductsByAffiliate'
Update AppConfig SET Description = 'If TRUE, the order notes field on the shopping cart page does not appear.' WHERE Name = 'DisallowOrderNotes'
Update AppConfig SET Description = 'The minimum order amount which can be ordered. Leave blank for no minimum. An example minimum is 100.00, with no $ or other currency symbol.' WHERE Name = 'CartMinOrderAmount'
Update AppConfig SET Description = 'Do not change this value. This value is required by AspDotNetStorefront. AspDotNetStorefront generates its own receipts.' WHERE Name = 'eProcessingNetwork_X_EMAIL_CUSTOMER'
Update AppConfig SET Description = 'If TRUE, products are filtered by the customer affiliate id, and the product-to-customer level mappings are used in the Productcustomerlevel table.' WHERE Name = 'FilterProductsByCustomerLevel'
Update AppConfig SET Description = 'If TRUE, and FilterProductsByCustomerLevel = TRUE, then HIGHER customer levels (numerically) can see Products from that level and all lower customer levels, e.g. Customer Level 3, see all products mapped to customer levels, 0, 1, 2, and 3. Note that Customer Level 0 is an anonymous user / regular site visitor.' WHERE Name = 'FilterByCustomerLevelIsAscending'
Update AppConfig SET Description = 'The minimum order weight, in lbs. Generally most useful when you use real-time shipping calculations. ' WHERE Name = 'MinOrderWeight'
Update AppConfig SET Description = '2Checkout-provided vendor account number. Within the 2Checkout control panel, set "Direct Return" to disabled.' WHERE Name = '2CHECKOUT_VendorID'
Update AppConfig SET Description = 'If TRUE, text files are formatted using UTF-8. If FALSE (the default), files are formatted using ASCII.' WHERE Name = 'WriteFileInUTF8'
Update AppConfig SET Description = 'If TRUE, the Google tracking topic will be included on the order confirmation page.' WHERE Name = 'IncludeGoogleTrackingCode'
Update AppConfig SET Description = 'Sets image widths within the multi-image product manager in the Admin Console. Set to 0 or blank to use built-in width. This setting does not affect images on your website.  This setting is merely a convenient way to help view the multi-image gallery in a smaller size when editing on the admin site.' WHERE Name = 'Admin.MultiGalleryImageWidth'
Update AppConfig SET Description = 'If TRUE, shoppers may resize the large image popup window.' WHERE Name = 'ResizableLargeImagePopup'
Update AppConfig SET Description = 'If enabled, shoppers see a grid of previous orders on the account page, each with a link to place the same order again.' WHERE Name = 'Reorder.Enabled'
Update AppConfig SET Description = 'If TRUE, the shopping cart is cleared when the shopper clicks the reorder button on a prior order. If FALSE, the prior order contents will simply be added to the current cart. ' WHERE Name = 'Reorder.ClearCartBeforeAddingReorderItems'
Update AppConfig SET Description = 'If TRUE, watermarks appear on product images.' WHERE Name = 'Watermark.Enabled'
Update AppConfig SET Description = 'Watermark text displayed on product images (all sizes). For example, Copyright YourStore.com.' WHERE Name = 'Watermark.CopyrightText'
Update AppConfig SET Description = 'Watermark text placement expressed as percentage from the bottom of product images. ' WHERE Name = 'Watermark.OffsetFromBottomPercentage'
Update AppConfig SET Description = 'If TRUE, admin users may edit the next ship date applicable for recurring orders.' WHERE Name = 'AllowRecurringIntervalEditing'
Update AppConfig SET Description = 'If TRUE, the parser will be invoked on product (or other object) descriptions. Do not set TRUE unless necessary, as doing so often adds significant processing overhead. ' WHERE Name = 'UseParserOnEntityDescriptions'
Update AppConfig SET Description = 'If FALSE, the cart stays secure on ALL pages following any page which goes secure (account, cart, etc). If true, the cart will attempt to go non-secure again on other pages, after it has gone secure. FALSE is the recommended value.' WHERE Name = 'GoNonSecureAgain'
Update AppConfig SET Description = 'If TRUE, Captcha fields appear on login pages.' WHERE Name = 'SecurityCodeRequiredOnStoreLogin'
Update AppConfig SET Description = 'If TRUE, Captcha fields appear on the create account page (not checkout mode).' WHERE Name = 'SecurityCodeRequiredOnCreateAccount'
Update AppConfig SET Description = 'If TRUE, XmlPackages write debugging output .xml files in the /images directory' WHERE Name = 'XmlPackage.DumpTransform'
Update AppConfig SET Description = 'If TRUE, shoppers creating accounts must check a box to indicate they are over 13 years old. This may be required to comply with Federal regulations.' WHERE Name = 'RequireOver13Checked'
Update AppConfig SET Description = 'The fully-qualified URL you use to retrieve currency exchange rate data. Refer also to the Localization.CurrencyFeedXmlPackage setting.' WHERE Name = 'Localization.CurrencyFeedUrl'
Update AppConfig SET Description = 'The XmlPackage used to provide the currency exchange rate data conversion. This package usually must work in conjunction with the currency exchange rate data provider to convert their rate data into our predefined XML format. Refer also to the Localization.CurrencyFeedUrl setting.' WHERE Name = 'Localization.CurrencyFeedXmlPackage'
Update AppConfig SET Description = 'The time, in minutes, which a currency rate table is cached. The site will call the live currencyserver specified in Localization.CurrencyFeedUrl setting each time this cache period expires.' WHERE Name = 'Localization.CurrencyCacheMinutes'
Update AppConfig SET Description = 'The application login you obtained from QuickBooks. Refer to Help for instructions.' WHERE Name = 'QBMERCHANTSERVICES_ApplicationLogin'
Update AppConfig SET Description = 'The Application ID you obtained from QuickBooks. Refer to Help for instructions.' WHERE Name = 'QBMERCHANTSERVICES_ApplicationID'
Update AppConfig SET Description = 'Do not change this value. Required by AspDotNetStorefront.' WHERE Name = 'QBMERCHANTSERVICES_ApplicationVersion'
Update AppConfig SET Description = 'Obtain this value from Quickbooks if necessary.' WHERE Name = 'QBMERCHANTSERVICES_InstallID'
Update AppConfig SET Description = 'The initial page size applied to new categories. Category page sizes may be modified within the Admin Console at any time.' WHERE Name = 'Default_CategoryPageSize'
Update AppConfig SET Description = 'The initial column width applied to new products. Product column widths may be modified within the Admin Console at any time.' WHERE Name = 'Default_ProductColWidth'
Update AppConfig SET Description = 'If TRUE, AspDotNetStorefront attempts to prevent shoppers from entering PO Box numbers in shipping addresses. Not 100% reliable.' WHERE Name = 'DisallowShippingToPOBoxes'
Update AppConfig SET Description = 'If TRUE, customers receive a welcome e-mail.  The content of the e-mail message is controlled by the XmlPackage specified in the XmlPackage.WelcomeEmail setting.  The e-mail subject is specfied in the StringResource named createaccount.aspx.79.' WHERE Name = 'SendWelcomeEmail'
Update AppConfig SET Description = 'Set TRUE to require customers to use strong passwords. When TRUE, the regular expression stored in the StrongPasswordValidator setting is used for validation.' WHERE Name = 'UseStrongPwd'
Update AppConfig SET Description = 'Enables on-page credit card number validation. ' WHERE Name = 'ValidateCreditCardNumbers'
Update AppConfig SET Description = 'Number of minutes which an account will be locked out after the MaxBadLogins threshold has been exceeded.  Set to zero to disable lockout behavior.' WHERE Name = 'BadLoginLockTimeOut'
Update AppConfig SET Description = 'The number of days between password resets for admin and superadmin users.' WHERE Name = 'AdminPwdChangeDays'
Update AppConfig SET Description = 'This must match the tax class id of your shipping class, this is sometimes necessary to set tax rates for shipping by country, state, or zip.' WHERE Name = 'ShippingTaxClassID'
Update AppConfig SET Description = 'Your CyberSource-assigned merchant ID (often the same as your Vendor ID). Contact CyberSource for more information.' WHERE Name = 'CYBERSOURCE.merchantID'
Update AppConfig SET Description = 'Should be full physical file path with ending backslash. Consult CyberSource documentation. ' WHERE Name = 'CYBERSOURCE.keysDirectory'
Update AppConfig SET Description = 'Full file name 1111111.p12 for example. Consult CyberSource documentation. ' WHERE Name = 'CYBERSOURCE.keyFilename'
Update AppConfig SET Description = 'If TRUE, "new order notification" e-mails are not sent to administrators. If FALSE, store administrators receive a "new order notification" e-mail from the storefront. This setting does not influence receipt e-mails sent to customers. ' WHERE Name = 'TurnOffStoreAdminEMailNotifications'
Update AppConfig SET Description = 'Text displayed at the top of the news page.' WHERE Name = 'NewsTeaser'
Update AppConfig SET Description = 'If TRUE, when bulk-importing shipments that are not voided, shipped notification e-mails are sent to customers of orders whose shipments were imported.' WHERE Name = 'BulkImportSendsShipmentNotifications'
Update AppConfig SET Description = 'The default customer level (integer) to be used on new customer records. Typically 0. ' WHERE Name = 'DefaultCustomerLevelID'
Update AppConfig SET Description = 'Text value passed to USAePay to describe every transaction.' WHERE Name = 'USAePay.Description'
Update AppConfig SET Description = 'Allows customers to test against their USAepay sandbox environment.' WHERE Name = 'USAePay.UseSandBox'
Update AppConfig SET Description = 'Do not change this value unless instructed by Authorize.net.' WHERE Name = 'Recurring.AUTHORIZENET_AUTOBILL_TEST_SERVER'
Update AppConfig SET Description = 'Do not change this value unless instructed by Authorize.net.' WHERE Name = 'Recurring.AUTHORIZENET_AUTOBILL_LIVE_SERVER'
Update AppConfig SET Description = 'Prevents admin users from reusing any of the specified number of previously-used passwords. PA-DSS requires 4.' WHERE Name = 'NumPreviouslyUsedPwds'
Update AppConfig SET Description = 'If TRUE, the Shipping Estimator appears on the checkout page.' WHERE Name = 'ShowShippingEstimate'
Update AppConfig SET Description = 'If TRUE, AspDotNetStorefront chooses which products to display, based on shopper orders. Search Help for Related Products for more information.' WHERE Name = 'DynamicRelatedProducts.Enabled'
Update AppConfig SET Description = 'If TRUE, Captcha entry is case-sensitive.' WHERE Name = 'Captcha.CaseSensitive'
Update AppConfig SET Description = 'If TRUE, the product downloads page displays related products.' WHERE Name = 'Download.ShowRelatedProducts'
Update AppConfig SET Description = 'The number of featured items that appear on the home page. Set this number to 0 to disable featured items functionality.' WHERE Name = 'FeaturedProducts.NumberOfItems'
Update AppConfig SET Description = 'Whether or not to show the add-to-cart form for featured products on the home page. Only simple products may be added to the cart. Complex products display a "Details" button.' WHERE Name = 'FeaturedProducts.ShowAddToCartForm'
Update AppConfig SET Description = 'If True, checkout page enables shoppers to opt out or in to email lists.' WHERE Name = 'Checkout.ShowOkToEmailOnCheckout'
Update AppConfig SET Description = 'If True, address forms include a Company Name field.' WHERE Name = 'Address.CollectCompany'
Update AppConfig SET Description = 'If True, address forms include a nickname field.' WHERE Name = 'Address.CollectNickName'
Update AppConfig SET Description = 'The maximum number of initial items to show on the checkout page. If the actual number of items in the cart exceeds this number, the shopping cart page displays a link to show all items in the cart. Set this value to 0 to disable this feature. This is useful for sites which tend to have larger shopping carts and only want to show the first few items.' WHERE Name = 'MaximumNumberOfCartItemsToDisplay'
Update AppConfig SET Description = 'If True, carrier icons appear on the checkout page.' WHERE Name = 'ShowShippingIcons'
Update AppConfig SET Description = 'If True, customers checking out with credit cards through Braintree will be shown the 3dSecure form.  3dSecure must be enabled on the Braintree account, which requires help from Braintree Support.' WHERE Name = 'Braintree.3dSecureEnabled'
Update AppConfig SET Description = 'If TRUE, product picture icons appear within the minicart.' WHERE Name = 'Minicart.ShowImages'
Update AppConfig SET Description = 'If TRUE, shoppers may modify cart item quantities within the minicart.' WHERE Name = 'Minicart.QuantityUpdate.Enabled'
Update AppConfig SET Description = 'If TRUE, the product sku appears within the minicart.' WHERE Name = 'Minicart.ShowSku'
Update AppConfig SET Description = 'If TRUE, addresses created by guest customers are moved to their new registered session when logging in or registering.' WHERE Name = 'PreserveActiveAddressOnSignin'
Update AppConfig SET Description = 'Maximum number of news articles which appear, wherever on your site news articles appear.' WHERE Name = 'NumberOfNewsArticlesToShow'
Update AppConfig SET Description = 'If TRUE (default), shopping carts are cached in memory; otherwise shopping carts are not cached. Should only be set FALSE for troubleshooting purposes; otherwise performance suffers. ' WHERE Name = 'CacheShoppingCarts'
Update AppConfig SET Description = 'If TRUE, (default) shipping methods during checkout are cached in memory. Should only be set FALSE for troubleshooting purposes; otherwise performance suffers.' WHERE Name = 'CacheShippingMethods'
Update AppConfig SET Description = 'Set to true to always 301 redirect non-secure requests to HTTPS. Set to false to not take any special action on non-secure requests to non-secure pages. Note that this setting overrides the GoNonSecureAgain setting.' WHERE Name = 'AlwaysUseHTTPS'
Update AppConfig SET Description = 'This adds the HSTS Strict-Transport-Security HSTS header to the site as long as the AlwaysUseHTTPS setting is enabled. To remove the header, set this value blank. To disable HSTS set the max-age to 0. To adjust the HSTS age set the HSTS max-age in seconds. For example, to direct browsers to always use https for 180 days, set this value to: max-age=15552000.' WHERE Name = 'HstsHeader'


--Quickstart panel
IF NOT EXISTS(SELECT * FROM AppConfig WHERE Name = 'ShowQuickStart')
BEGIN
	INSERT INTO [dbo].[AppConfig] ([StoreID], [Name], [Description], [ConfigValue], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [CreatedOn], [UpdatedOn]) VALUES (0, N'ShowQuickStart', N'If TRUE, the Getting Started panel displays on the admin home page.', N'true', N'boolean', NULL, N'ADMIN', 0, 0, GETDATE(), GETDATE());
END

IF NOT EXISTS(select * from [dbo].[GlobalConfig] where name = 'KeyEncryptionKey')
	INSERT INTO [dbo].[GlobalConfig] ([GlobalConfigGUID], [Name], [Description], [ConfigValue], [ValueType], [GroupName], [EnumValues], [SuperOnly], [Hidden], [IsMultiStore], [CreatedOn], [UpdatedOn]) VALUES (newid(), N'KeyEncryptionKey', N'This is a system level option only, do not change this value or you will no longer be able to recover encrypted data.', N'', N'string', 'ENCRYPTION', NULL, 1, 0, 0, GETDATE(), GETDATE())

/*********** End 10.0.0.0 Changes *********************/

PRINT CHAR(10)
PRINT '*****Finalizing database upgrade*****'
-- Update database indexes
PRINT 'Updating database indexes...'
EXEC [dbo].[aspdnsf_UpdateIndexes]
-- Update store version
PRINT 'Updating Store Version...'
UPDATE [dbo].[AppConfig] SET [ConfigValue] = '10.0.0' WHERE [Name] = 'StoreVersion'
print '*****Database Upgrade Completed*****'

SET NOEXEC OFF
GO	
