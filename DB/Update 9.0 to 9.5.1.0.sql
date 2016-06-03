-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit our homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT.
-- ------------------------------------------------------------------------------------------

-- ------------------------------------------------------------------------------------------
-- Database Upgrade Script:
-- AspDotNetStorefront Version 9.0 to Latest, Microsoft SQL Server 2005 Or higher
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
		AND ConfigValue NOT IN ('','authorizenet','cybersource','eprocessingnetwork','firstpay','manual','moneris','payflowpro','paypal','paypalpro','qbmerchantservices','sagepayments','sagepayuk','skipjack','twocheckout','usaepay')

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

/*making sure the isfeatured column exists on the product table*/
IF NOT EXISTS (SELECT * FROM   sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[Product]')  AND name = 'IsFeatured')
begin
	PRINT 'Adding IsFeatured Column'
	ALTER TABLE [dbo].[Product] ADD IsFeatured tinyint NOT NULL CONSTRAINT DF_Product_IsFeatured DEFAULT((0))
end
go


PRINT 'Updating GlobalConfig Table...'
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('GlobalConfig') AND name = 'IsMultiStore')
    ALTER TABLE dbo.GlobalConfig ADD IsMultiStore [bit] NOT NULL CONSTRAINT DF_GlobalConfig_IsMultiStore DEFAULT((1))
ELSE
	declare @xtype int
	select @xtype = system_type_id from sys.types where name = 'int'
	if exists (select * from syscolumns where id = object_id('GlobalConfig') and name = 'IsMultiStore' and xtype = @xtype)
		begin
		ALTER TABLE dbo.GlobalConfig DROP CONSTRAINT DF_GlobalConfig_IsMultiStore
		alter table dbo.GlobalConfig alter column IsMultiStore bit
		ALTER TABLE dbo.GlobalConfig ADD CONSTRAINT DF_GlobalConfig_IsMultiStore DEFAULT ((1)) FOR IsMultiStore
		end
GO

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[ProductEntity]') AND type = 'V')
        DROP VIEW [dbo].[ProductEntity]
    GO

create view [dbo].[ProductEntity]

AS
select 'category' EntityType, ProductID, CategoryID EntityID, DisplayOrder, CreatedOn From dbo.productcategory with (nolock)
union all
select 'section', ProductID, SectionID EntityID, DisplayOrder, CreatedOn From dbo.productsection with (nolock)
union all
select 'manufacturer', ProductID, ManufacturerID EntityID, DisplayOrder, CreatedOn From dbo.productmanufacturer with (nolock)
union all
select 'distributor', ProductID, DistributorID EntityID, DisplayOrder, CreatedOn From dbo.productdistributor with (nolock)
union all
select 'affiliate', ProductID, AffiliateID EntityID, DisplayOrder, CreatedOn From dbo.productaffiliate with (nolock)
union all
select 'locale', ProductID, LocaleSettingID EntityID, DisplayOrder, CreatedOn From dbo.productlocalesetting with (nolock)
union all
select 'customerlevel', ProductID, CustomerLevelID EntityID, DisplayOrder, CreatedOn From dbo.ProductCustomerLevel with (nolock)
union all
select 'genre', ProductID, GenreID EntityID, DisplayOrder, CreatedOn From dbo.productgenre with (nolock)
union all
select 'vector', ProductID, VectorID EntityID, DisplayOrder, CreatedOn From dbo.productvector with (nolock)
GO

/* ************************************************************** */
/* STORED PROCEDURE UPDATES										  */
/* ************************************************************** */
PRINT 'Updating Stored Procedures...'


IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetStoreShippingMethodMapping') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetStoreShippingMethodMapping]
GO

create procedure [dbo].[aspdnsf_GetStoreShippingMethodMapping](
	@StoreId int,
	@IsRTShipping int = 0,
	@OnlyMapped int = 0,
	@ExcludeNameLike NVARCHAR(2000) = NULL
)
AS
BEGIN

	SET NOCOUNT ON;

	select	@StoreId as StoreId,
			dbo.GetStoreMap(@storeid, 'ShippingMethod', sm.ShippingMethodId) as Mapped,
			sm.*
	from ShippingMethod sm WITH (NOLOCK)
	WHERE	sm.IsRTShipping = @IsRTShipping AND
			(@OnlyMapped = 0 or (dbo.GetStoreMap(@storeid, 'ShippingMethod', sm.ShippingMethodId) = 1)) AND
			(@ExcludeNameLike IS NULL OR (sm.[Name] NOT LIKE '%' + @ExcludeNameLike + '%'))
	order by sm.DisplayOrder ASC

END


GO

/* ************************************************************** */
/* DATA UPDATES													  */
/* ************************************************************** */

-- Update store version
PRINT 'Updating Store Version...'
UPDATE [dbo].[AppConfig] SET [ConfigValue] = '9.0.1.2' WHERE [Name] = 'StoreVersion'

-- Add the topic required for the new contact page
IF EXISTS (SELECT * FROM Topic WHERE Name='ContactEmail')
BEGIN
PRINT 'ContactEmail topic exists already'
END
ELSE
BEGIN
PRINT 'Adding ContactEmail topic'
INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('ContactEmail', 1, 0, 'ContactEmail', '<table style="width: 400px;"><tbody><tr><td align="right" style="width: 150px;">Customer Name: </td><td align="left">%NAME%</td></tr><tr><td align="right" style="width: 150px;">Customer Email: </td><td align="left">%EMAIL%</td></tr><tr><td align="right" style="width: 150px;">Customer Phone:</td><td align="left">%PHONE%</td></tr><tr><td colspan="2"> </td></tr><tr><td colspan="2"><b>%SUBJECT%</b></td></tr><tr><td colspan="2" style="padding-top: 3px;">%MESSAGE%</td></tr></tbody></table>')
END

-- Create a default record in the stores table if one does not exist
PRINT 'Creating Default Store Record...'
IF (SELECT COUNT(*) FROM dbo.Store) = 0
BEGIN
	INSERT INTO dbo.Store(	StoreGUID,
							ProductionURI,
							StagingURI,
							DevelopmentURI,
							[Name],
							Summary,
							Description,
							Published,
							Deleted,
							SkinID,
							IsDefault,
							CreatedOn)
			Values(	newid(),
					'www.mystore.com',
					'staging.mystore.com',
					'localhost',
					'Default Store',
					'',
					'',
					1,
					0,
					1,
					1,
					getdate())
END

GO

-- Remove invalid mobile device useragent
PRINT 'Updating Mobile Devices...'
DELETE [dbo].[MobileDevice] WHERE UserAgent=N'tosh'
DELETE [dbo].[MobileDevice] WHERE UserAgent=N'wc3'
DELETE [dbo].[MobileDevice] WHERE UserAgent=N'oper'
GO

PRINT 'Updating Global Configs...'
-- Create globalconfig parameter for switching masterpages by locale
IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'AllowTemplateSwitchingByLocale') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) VALUES('AllowTemplateSwitchingByLocale', 'DISPLAY', 'Indicator of whether the site should attempt to load different masterpage skins based on the current locale of the browsing customer.  This should only be enabled if you have multiple locales and have created different masterpages for each of your locales (eg. template.en-us.master, template.en-gb.master, etc...).  Enabling this when you do not have multiple locales or when you have not create multiple masterpages that vary by locale can hinder the performance of your site.', 'false', 'boolean', 'false')
END
GO

PRINT 'Updating Topics Table to work with Duplicate Topic Names and Delete Name Constraint'
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('Topic') AND name = 'StoreID')
    ALTER TABLE dbo.Topic ADD StoreID [int] NOT NULL CONSTRAINT DF_Topic_StoreID DEFAULT((0))
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Topic]') AND name = N'UIX_Topic_Name')
	DROP INDEX [UIX_Topic_Name] ON [dbo].[Topic] WITH ( ONLINE = OFF )
GO

-- Update AllowCustomerDuplicateEmailAddresses globalconfig parameter
UPDATE GlobalConfig SET [IsMultiStore] = 0 WHERE Name='AllowCustomerDuplicateEmailAddresses'
GO

-- Populate the ProductStore table
-- Insert deleted products as well in case they are undeleted
PRINT 'Populating Stores Table...'
INSERT INTO ProductStore (ProductID) SELECT ProductID FROM Product WHERE ProductID NOT IN (SELECT ProductID FROM ProductStore)
GO

--Update Croatia so that it works for real time shipping
UPDATE [dbo].Country set name = 'Croatia' where name = 'Croatia (local Name: Hrvatska)'
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.API.AcceleratedBoardingEmailAddress') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.API.AcceleratedBoardingEmailAddress','GATEWAY','If you enter your email address PayPal will allow you to start taking orders without yet having an account. You must register for an account to collect funds captured within 30 days. After registering you should enter your API creds into the appropriate app configs.','');
END
GO

UPDATE [dbo].AppConfig SET configvalue = 'http://www.sitemaps.org/schemas/sitemap/0.9' WHERE name = 'GoogleSiteMap.Xmlns' and configvalue = 'http://www.google.com/schemas/sitemap/1.0'
GO

UPDATE [dbo].AppConfig SET configvalue = 'https://test.sagepay.com/simulator/VSPServerGateway.asp?service=VendorAbortTx' WHERE name = 'SagePayUKURL.Simulator.Abort' and configvalue = 'https://ukvpstest.protx.com/VSPSimulator/VSPServerGateway.asp?service=VendorAbortTx'
UPDATE [dbo].AppConfig SET configvalue = 'https://test.sagepay.com/simulator/VSPDirectCallback.asp' WHERE name = 'SagePayUKURL.Simulator.Callback' and configvalue = 'https://ukvpstest.protx.com/VSPSimulator/VSPDirectCallback.asp'
UPDATE [dbo].AppConfig SET configvalue = 'https://test.sagepay.com/simulator/VSPDirectGateway.asp' WHERE name = 'SagePayUKURL.Simulator.Purchase' and configvalue = 'https://ukvpstest.protx.com/VSPSimulator/VSPDirectGateway.asp'
UPDATE [dbo].AppConfig SET configvalue = 'https://test.sagepay.com/simulator/VSPServerGateway.asp?service=VendorRefundTx' WHERE name = 'SagePayUKURL.Simulator.Refund' and configvalue = 'https://ukvpstest.protx.com/VSPSimulator/VSPServerGateway.asp?service=VendorRefundTx'
UPDATE [dbo].AppConfig SET configvalue = 'https://test.sagepay.com/simulator/VSPServerGateway.asp?service=VendorReleaseTx' WHERE name ='SagePayUKURL.Simulator.Release' and configvalue = 'https://ukvpstest.protx.com/VSPSimulator/VSPServerGateway.asp?service=VendorReleaseTx'
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.API.AcceleratedBoardingEmailAddress') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.API.AcceleratedBoardingEmailAddress','GATEWAY','If you enter your email address PayPal will allow you to start taking orders without yet having an account. You must register for an account to collect funds captured within 30 days. After registering you should enter your API creds into the appropriate app configs.','');
END

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_CreateFeed') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
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
set @FeedID = @@identity
END

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_UpdFeed') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_UpdFeed]
GO
create proc [dbo].[aspdnsf_UpdFeed]
    @FeedID int,
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
    @ExtensionData nvarchar(max)

AS
SET NOCOUNT ON


IF isnull(@XmlPackage, '') = '' BEGIN
    RAISERROR('XmlPAckage is required', 16, 1)
    RETURN
END

IF @CanAutoFTP > 1
    SET @CanAutoFTP = 1

UPDATE dbo.Feed
SET
    StoreID = COALESCE(@StoreID, StoreID),
    Name = COALESCE(@Name, Name),
    DisplayOrder = COALESCE(@DisplayOrder, DisplayOrder),
    XmlPackage = COALESCE(@XmlPackage, XmlPackage),
    CanAutoFTP = COALESCE(@CanAutoFTP, CanAutoFTP),
    FTPUsername = COALESCE(@FTPUsername, FTPUsername),
    FTPPassword = COALESCE(@FTPPassword, FTPPassword),
    FTPServer = COALESCE(@FTPServer, FTPServer),
    FTPPort = COALESCE(@FTPPort, FTPPort),
    FTPFilename = COALESCE(@FTPFilename, FTPFilename),
    ExtensionData = COALESCE(@ExtensionData, ExtensionData)
WHERE FeedID = @FeedID





GO

/* ************************************************************** */
/* Admin Service Pack  											  */
/* ************************************************************** */
/****** Object:  StoredProcedure [dbo].[aspdnsf_GetMappedObjects]    Script Date: 11/17/2010 13:44:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[aspdnsf_GetMappedObjects](
	@StoreId int,
	@EntityType nvarchar(30),
	@AlphaFilter nvarchar(30) = null,
	@SearchFilter nvarchar(30) = null,
	@pageSize int = null,
	@page int = null,
	@IsLegacyCacheMechanism bit = 1.
)
as
begin

	-- In an effort to elliminate the insanly slow load time of the store entity caching mechanism, the default returns of this stored procedure returns bunk data.
	-- In admin, the entity object mapping controls will switch this off to work correctly.
	if (@IsLegacyCacheMechanism = 1)
		begin

			select	0 as TotalCount, 0 as PageSize, 0 as CurrentPage, 0 as TotalPages, 0 as StartIndex, 0 as EndIndex
			select	0 as StoreID, 0, '' as EntityType, 0 as [ID], '' as [Name], 0 as Mapped

		end
	else
		begin

		declare @count int
		declare @allPages int
		declare @start int
		declare @end int

		-- flag to determine if we should do paging
		declare @doPage bit
		set @doPage = case when @pageSize is null and @page is null then 0 else 1 end

		-- execute query to fetch the count of all availalble data
		-- which we will use later on to get the paging information
		select @count = count(*)
		from
		(
			select	o.EntityType,
					o.[Id],
					o.[Name]
			from ObjectView o
			where	o.EntityType = COALESCE(@EntityType, o.EntityType) and
					(@AlphaFilter IS NULL OR (o.[Name] like @AlphaFilter + '%')) and
					(@SearchFilter IS NULL OR (o.[Name] like '%' + @SearchFilter + '%'))
		) ov

		-- sanity check
		if(@count <= @pageSize) set @pageSize = @count

		-- determine start and end index
		set @start = ((@page - 1) * @pageSize) + 1
		set @end = (@start + @pageSize) - 1
		if(@end > @count) set @end = @count

		-- compute the total number of pages
		if(@count > 0 )
		begin
			set @allPages = @count / @pageSize

			declare @rem int
			set @rem = @count % @pageSize -- residue
			if(@rem > 0) set @allPages = @allPages + 1
		end
		else set @allPages = 0

		-- paging information
		select	@count as TotalCount,
				@pageSize as PageSize,
				@page as CurrentPage,
				@allPages as TotalPages,
				@start as StartIndex,
				@end as [EndIndex]

		-- actual paged result set
		select	@StoreId as StoreID,
				ROW_NUMBER,
				ov.EntityType,
				ov.[ID],
				ov.[Name],
				dbo.GetStoreMap(@StoreId, ov.EntityType, ov.ID) as Mapped
		from
		(
			select	ROW_NUMBER() over(partition by o.EntityType order by id) as [Row_Number],
					o.EntityType,
					o.[Id],
					o.[Name]
			from ObjectView o
			where	o.EntityType = COALESCE(@EntityType, o.EntityType) and
					(@AlphaFilter IS NULL OR (o.[Name] like @AlphaFilter + '%')) and
					(@SearchFilter IS NULL OR (o.[Name] like '%' + @SearchFilter + '%'))
		) ov
		where @doPage = 0 or (ROW_NUMBER BETWEEN @start AND @end)
	end
end
go

ALTER PROCEDURE [dbo].[aspdnsf_updGridProductVariant]
(
	@variantID int,
	@name nvarchar(255) = NULL,
	@description nvarchar(max),
	@skuSuffix nvarchar(50) = NULL,
	@Price money = NULL,
	@SalePrice money = NULL,
	@Inventory int = NULL,
	@deleted tinyint = 0,
	@published tinyint = 1
)
AS
BEGIN
SET NOCOUNT ON

UPDATE [ProductVariant] SET
	[Name] = COALESCE(@name, [Name]),
	[Description] = COALESCE(@description, [Description]),
	[SKUSuffix] = COALESCE(@skuSuffix, [skuSuffix]),
	[Price] = COALESCE(@Price, [Price]),
	[SalePrice] = COALESCE(@SalePrice, [SalePrice]),
	[Inventory] = COALESCE(@Inventory,[Inventory]),
	[Deleted] = @deleted,
	[Published] = @published
WHERE [VariantID] = @VariantID

END


GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'DefaultWidth_micro') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultWidth_micro','IMAGERESIZE','Default width of an micro image if no width attribute is specified in the other size-configs (i.e. width:50;).  This value should NOT be left blank.','50')
END

update AppConfig set ValueType = 'string' where Name = 'ShippingMethodIDIfFreeShippingIsOn'
GO

ALTER proc [dbo].[aspdnsf_CloneProduct]
    @productID int,
    @userid int = 0,
    @cloneinventory int = 1

AS
BEGIN

SET NOCOUNT ON

DECLARE @tmpKitTbl TABLE(KitGroupID int not null)
DECLARE @tmpPVariantTbl TABLE(VariantID int not null)
DECLARE @newproductID int
DECLARE @err int, @newkitgroupid int

SET @newproductID = -1

BEGIN TRAN
    INSERT [dbo].product (ProductGUID, Name, Summary, Description, SEKeywords, SEDescription, MiscText, FroogleDescription, SETitle, SEAltText, SizeOptionPrompt, ColorOptionPrompt, TextOptionPrompt, ProductTypeID, TaxClassID, SKU, ManufacturerPartNumber, SalesPromptID, IsFeatured, XmlPackage, ColWidth, Published, RequiresRegistration, Looks, Notes, QuantityDiscountID, RelatedProducts, UpsellProducts, UpsellProductDiscountPercentage, RelatedDocuments, TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor, IsAKit, ShowInProductBrowser, IsAPack, PackSize, ShowBuyButton, RequiresProducts, HidePriceUntilCart, IsCalltoOrder, ExcludeFromPriceFeeds, RequiresTextOption, TextOptionMaxLength, SEName, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
    SELECT newid(), Name + ' - CLONED', Summary, Description, SEKeywords, SEDescription, MiscText, FroogleDescription, SETitle, SEAltText, SizeOptionPrompt, ColorOptionPrompt, TextOptionPrompt, ProductTypeID, TaxClassID, SKU, ManufacturerPartNumber, SalesPromptID, IsFeatured, XmlPackage, ColWidth, 0, RequiresRegistration, 0, Notes, QuantityDiscountID, RelatedProducts, UpsellProducts, UpsellProductDiscountPercentage, RelatedDocuments, TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor, IsAKit, ShowInProductBrowser, IsAPack, PackSize, ShowBuyButton, RequiresProducts, HidePriceUntilCart, IsCalltoOrder, ExcludeFromPriceFeeds, RequiresTextOption, TextOptionMaxLength, SEName, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, getdate()
    FROM dbo.product
    WHERE productid = @productID

    SELECT @newproductID = @@identity, @err = @@error

    IF @err <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -1
    END

    IF @cloneinventory = 1 BEGIN
        DECLARE @PrdVariantID int, @newvariantID int
        INSERT @tmpPVariantTbl SELECT VariantID FROM dbo.productvariant  WHERE productid = @productID
        SELECT top 1 @PrdVariantID = VariantID FROM @tmpPVariantTbl
        WHILE @@rowcount <> 0 BEGIN

            INSERT [dbo].productvariant (VariantGUID, ProductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
            SELECT newid(), @newproductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, getdate()
            FROM dbo.productvariant
            WHERE VariantID = @PrdVariantID

            SELECT @newvariantID = @@identity, @err = @@error

            IF @err <> 0 BEGIN
                raiserror('Product not cloned', 1, 16)
                rollback tran
                return -2
            END


            INSERT [dbo].Inventory (InventoryGUID, VariantID, Color, Size, Quan, CreatedOn)
            SELECT newid(), @newvariantID, Color, Size, Quan, getdate()
            FROM dbo.Inventory
            WHERE VariantID = @PrdVariantID

            IF @@error <> 0 BEGIN
                raiserror('Product not cloned', 1, 16)
                rollback tran
                return -14
            END

            DELETE @tmpPVariantTbl where VariantID = @PrdVariantID
            SELECT top 1 @PrdVariantID = VariantID from @tmpPVariantTbl
        END
    END
    ELSE BEGIN

        INSERT [dbo].productvariant (VariantGUID, ProductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
        SELECT newid(), @newproductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, getdate()
        FROM dbo.productvariant
        WHERE productid = @productID

        IF @@error <> 0 BEGIN
            raiserror('Product not cloned', 1, 16)
            rollback tran
            return -2
        END

    END


    DECLARE @kitgrpid int
    INSERT @tmpKitTbl select KitGroupID FROM kitgroup  where productid = @productID
    SELECT top 1 @kitgrpid = KitGroupID FROM @tmpKitTbl
    WHILE @@rowcount <> 0 BEGIN
        INSERT [dbo].kitgroup (KitGroupGUID, Name, Description, ProductID, DisplayOrder, KitGroupTypeID, IsRequired, ExtensionData, CreatedOn)
        SELECT newid(), Name, Description, @newproductID, DisplayOrder, KitGroupTypeID, IsRequired, ExtensionData, getdate()
        FROM dbo.kitgroup
        WHERE KitGroupID = @kitgrpid

        SELECT @newkitgroupid = @@identity, @err = @@error

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

update appconfig set description = 'If you do not want to allow, or cannot allow (e.g. using FEDEX) shipping to PO Boxes, set this flag to true, and the store will TRY to prevent users from entering PO Box shipping addresses. It is NOT 100% guaranteed. Users enter all types of values.' where name = 'DisallowShippingToPOBoxes'
/* ************************************************************** */
/* Version 9.1       											  */
/* ************************************************************** */
--new default store id on app configs of 0
if not exists(select * from [dbo].[AppConfig] where storeid = 0)
BEGIN
update [dbo].[AppConfig] set StoreID = 0 where StoreID = (select StoreID from [dbo].[Store] where IsDefault = 1)
END

--new app configs will now be added with default storeid 0
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
ALTER TABLE [dbo].[AppConfig]
	DROP CONSTRAINT DF_AppConfig_StoreID
GO
ALTER TABLE [dbo].[AppConfig] ADD CONSTRAINT
	DF_AppConfig_StoreID DEFAULT ((0)) FOR StoreID
GO

--these changes still need to be merged into the create script
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - This app config has been left for backwards compatability, but, as of SP1, is no longer used by AspDotNetStorefront. Default Skin Id is now set per store under Configuration -> Store Maintenance -> Domains under store settings.' where [Name] = 'DefaultSkinID'
update [dbo].[AppConfig] set [Description] = 'The category ID that is considered to be ''Featured Products''. Products mapped to this category get a Featured status. This category ID provides an additional way for consumers to find the ''specials''. The ''is featured'' category Name typically has a Name like ''Specials'', ''On Sale Now'', ''Blowout Specials'', etc...' where [Name] = 'IsFeaturedCategoryID'

if not exists(select * from [dbo].[AppConfig] where [Name] = 'DefaultLocale')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden])
	VALUES (N'DefaultLocale', N'The default locale. If empty this will default to the value in the web.config. Note that changes to this app config will not take full effect until the site is restarted.', N'string', NULL, N'SETUP', 1, 0)
END

if not exists(select * from [dbo].[AppConfig] where [Name] = 'Paging.ShowAllPageLinks')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue])
	VALUES (N'Paging.ShowAllPageLinks', N'If true all pages will be shown in paging links.', N'boolean', NULL, N'SITEDISPLAY', 0, 0, 'false')
END


if not exists(select * from [dbo].[AppConfig] where [Name] = 'Paging.PagesForward')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue])
	VALUES (N'Paging.PagesForward', N'The number of forward pages to show if Paging.ShowAllPageLinks is set to false.', N'integer', NULL, N'SITEDISPLAY', 0, 0, '3')
END


if not exists(select * from [dbo].[AppConfig] where [Name] = 'Paging.PagesBackward')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue])
	VALUES (N'Paging.PagesBackward', N'The number of backwards pages to show if Paging.ShowAllPageLinks is set to false.', N'integer', NULL, N'SITEDISPLAY', 0, 0, '3')
END


UPDATE [dbo].[AppConfig] set [ConfigValue] = N'MONERIS' WHERE [CONFIGVALUE] like N'ESELECTPLUS'
UPDATE [dbo].[AppConfig] set [ConfigValue] = N'PAYPAL' WHERE [CONFIGVALUE] like N'PAYPALPRO'
UPDATE [dbo].[AppConfig] set [ConfigValue] = N'SAGEPAYUK' WHERE [CONFIGVALUE] like N'PROTX'
UPDATE [dbo].[AppConfig] set [ConfigValue] = N'QBMERCHANTSERVICES' WHERE [CONFIGVALUE] like N'QUICKBOOKS'

UPDATE [dbo].[AppConfig] set [ConfigValue] = N'https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor/' WHERE [Name] = N'CYBERSOURCE.PITURL'

update [dbo].[Country] set PostalCodeRegex = N'^[0-9]{4,4}(-[0-9]{3,3}){0,1}$', PostalCodeExample = N'#### or ####-###' where Name = N'Portugal' and (PostalCodeRegex = N'' or PostalCodeRegex is null)

if not exists(select * from [dbo].[AppConfig] where [Name] = 'RTSHIPPING.FedEx.Key')
	INSERT INTO [dbo].[AppConfig] ([SuperOnly],[Name],[GroupName],[Description],[ConfigValue])
	values(0,'RTSHIPPING.FedEx.Key', 'RTSHIPPING','FedEx account key given to you from FedEx.', '')
go

if not exists(select * from [dbo].[AppConfig] where [Name] = 'RTSHIPPING.FedEx.Password')
	INSERT INTO [dbo].[AppConfig] ([SuperOnly],[Name],[GroupName],[Description],[ConfigValue])
 	values(0,'RTSHIPPING.FedEx.Password', 'RTSHIPPING','FedEx password given to you from FedEx. This is givent to you when you generate you key', '')
go

if exists(select * from [dbo].[AppConfig] where [Name] = 'RTSHIPPING.FedEx.Server')
	Delete [dbo].[AppConfig] where [Name] = 'RTSHIPPING.FedEx.Server'
 go
INSERT INTO [dbo].[AppConfig] ([SuperOnly],[Name],[GroupName],[Description],[ConfigValue])
values(0,'RTShipping.FedEx.Server','RTSHIPPING','Your FedEx Server Assigned by FedEX. The Server URl is CaSE SeNSitIVe!!! Your URL may DIfFeR frOm THis One!','https://gateway.fedex.com:443/web-services');
go


if not exists (select * From syscolumns where id = object_id('Orders') and name = 'ReceiptHtml') begin
	ALTER TABLE [dbo].[Orders] ADD ReceiptHtml nvarchar(max)
end
go

 IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getOrder]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getOrder]
    GO

create proc dbo.aspdnsf_getOrder
    @ordernumber int

AS
SET NOCOUNT ON

SELECT
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
    o.eCheckBankABACode,
    o.eCheckBankAccountNumber,
    o.eCheckBankAccountType,
    o.eCheckBankName,
    o.eCheckBankAccountName,
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
    os.ShippingAddressID,
    os.IsAKit,
    os.IsAPack
FROM Orders o with (nolock)
    left join orders_ShoppingCart os with (nolock) ON os.OrderNumber = o.OrderNumber
WHERE o.OrderNumber = @ordernumber
ORDER BY os.ShippingAddressID


GO





delete from [dbo].[appconfig] where [Name] = 'RTShipping.Fedex.ShipDate'
delete from [dbo].[appconfig] where [Name] = 'RTShipping.Fedex.CarrierCodes'



/* ************************************************************** */
/* Version 9.2       											  */
/* ************************************************************** */

update [dbo].[AppConfig] set configvalue = '9.2.0.0' where name = 'StoreVersion'
GO

--buysafe global configs
delete from AppConfig where Name like 'BuySafe.%'

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.Enabled') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore])
	VALUES('BuySafe.Enabled', 'BUYSAFE', N'To enable BuySafe set this to true. If this is false, BuySafe will be disabled.', 'false', 'boolean', 'false')
END
GO


IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.EndPoint') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore])
	VALUES('BuySafe.EndPoint', 'BUYSAFE', N'The buySAFE API endpoint.', 'https://api.buysafe.com/BuySafeWS/RegistrationAPI.dll', 'string', 'false')
END
GO

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.UserName') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore])
	VALUES('BuySafe.UserName', 'BUYSAFE', N'Your BuySafe user name', '', 'string', 'false')
END
GO

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.Hash') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore])
	VALUES('BuySafe.Hash', 'BUYSAFE', N'The buySAFE Hash value is the unique identifier for your buySAFE account. Please contact buySAFE if you have any questions.', '', 'string', 'false')
END
GO

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.GMS') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore])
	VALUES('BuySafe.GMS', 'BUYSAFE', N'Your BuySafe GMS', '1000.00', 'decimal', 'false')
END
GO

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.TrialStartDate') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore])
	VALUES('BuySafe.TrialStartDate', 'BUYSAFE', N'The date on which your 30 BuySafe trial started', '', 'string', 'false')
END
GO

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.RollOverJSLocation') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore])
	VALUES('BuySafe.RollOverJSLocation', 'BUYSAFE', N'Do not change this value.', 'https://seal.buysafe.com/private/rollover/rollover.js', 'string', 'false')
END
GO

if not exists(select * from [dbo].[AppConfig] where [Name] = 'BuySafe.DisableAddoToCartKicker')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue])
	VALUES (N'BuySafe.DisableAddoToCartKicker', N'If this is set to true the buySafe kicker will not be shown on product pages.', N'boolean', NULL, N'BUYSAFE', 1, 0, 'false')
END



if not exists(select * from [dbo].[AppConfig] where [Name] = 'BuySafe.KickerType')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue])
	VALUES (N'BuySafe.KickerType', N'The value of the request to buysafe that defines the type of kicker to show. Alternate types can be found here: http://www.buysafe.com/web/general/kickerpreview.aspx.', N'string', NULL, N'BUYSAFE', 1, 0, 'Kicker Guaranteed Ribbon 200x90')
END

GO

ALTER proc [dbo].[aspdnsf_updCustomer]
    @CustomerID int,
    @CustomerLevelID int = null,
    @Email nvarchar(100) = null,
    @Password nvarchar(250) = null,
    @SaltKey int = null,
    @DateOfBirth datetime = null,
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
    @CODCompanyCheckAllowed tinyint = null,
    @CODNet30Allowed tinyint = null,
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
    @StoreID	int = null


AS
SET NOCOUNT ON

DECLARE @OldPwd nvarchar(100), @IsAdminCust tinyint, @OldSaltKey int

SELECT @OldPwd = Password, @IsAdminCust = IsAdmin, @OldSaltKey = Saltkey FROM dbo.Customer WHERE CustomerID = @CustomerID


UPDATE dbo.Customer
SET
    CustomerLevelID = COALESCE(@CustomerLevelID, CustomerLevelID),
    RegisterDate = COALESCE(@RegisterDate, RegisterDate),
    Email = COALESCE(@Email, Email),
    Password = COALESCE(@Password, Password),
    SaltKey = COALESCE(@SaltKey, SaltKey),
    DateOfBirth = COALESCE(@DateOfBirth, DateOfBirth),
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
    CODCompanyCheckAllowed = COALESCE(@CODCompanyCheckAllowed, CODCompanyCheckAllowed),
    CODNet30Allowed = COALESCE(@CODNet30Allowed, CODNet30Allowed),
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
    RequestedPaymentMethod = COALESCE(@RequestedPaymentMethod, RequestedPaymentMethod),
    StoreID = COALESCE(@StoreID, StoreID)
WHERE CustomerID = @CustomerID

IF @IsAdminCust > 0 and @OldPwd <> @Password
    INSERT dbo.PasswordLog (CustomerID, OldPwd, SaltKey, ChangeDt)
    VALUES (@CustomerID, @OldPwd, @OldSaltKey, getdate())
GO

if not exists(select * from [dbo].[AppConfig] where [Name] = 'DisablePasswordAutocomplete')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue])
	VALUES (N'DisablePasswordAutocomplete', N'If this is true the attribute autocomplete="false" will be added to password text boxes. This will stop browsers from prepopulating login credentials.', N'boolean', NULL, N'SECURITY', 1, 0, 'false')
END
GO



if not exists (select * From sysobjects where id = object_id('ErrorMessage') and type = 'u')
BEGIN
    CREATE TABLE [dbo].[ErrorMessage](
		[MessageId] [int] IDENTITY(1,1) NOT NULL,
		[Message] [nvarchar](max) NOT NULL,
		[MessageGuid] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_ErrorMessage] PRIMARY KEY CLUSTERED
	(
		[MessageId] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'RecentAdditionsShowPics'

/* ************************************************************** */
/* Version 9.1.0.2   											  */
/* ************************************************************** */

update [dbo].AppConfig set ConfigValue = 'https://www.paypal.com/en_US/i/bnr/bnr_paymentsBy_150x40.gif' where Name = 'PayPal.Express.ButtonImageURL'

--SecureNetV411 App Configs
if not exists (select * from AppConfig where Name = 'SecureNetV4.LiveURL')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'SecureNetV4.LiveURL', 'Endpoint address for live SecureNet transactions', 'http://gateway.securenet.com/api/Gateway.svc/soap', 'string', '', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'SecureNetV4.TestURL')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'SecureNetV4.TestURL', 'Endpoint address for test SecureNet transactions', 'http://certify.securenet.com/API/Gateway.svc/soap', 'string', '', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'SecureNetV4.DataAPI.LiveURL')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'SecureNetV4.DataAPI.LiveURL', 'Endpoint address for live SecureNet data transactions', 'http://gateway.securenet.com/api/data/service.svc/soap', 'string', '', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'SecureNetV4.DataAPI.TestURL')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'SecureNetV4.DataAPI.TestURL', 'Endpoint address for test SecureNet data transactions', 'http://certify.securenet.com/api/data/service.svc/soap', 'string', '', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'SecureNetV4.UseTestMode')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'SecureNetV4.UseTestMode', 'If enabled the SecureNetV4 gateway will use soft test mode. Note that this is different than hitting the test endpoing; almost no validation will be performed.', 'false', 'boolean', '', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'SecureNetV4.VaultEnabled')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'SecureNetV4.VaultEnabled', 'If enabled users will be able to store credit cards via the SecureNet Vault service. This feature is unavailable with Smart One-Page-Checkout (SmartOPC).', 'false', 'boolean', '', 'GATEWAY', 1, 0, getdate())

-- Checkout Type
if not exists (select * from AppConfig where Name = 'Checkout.Type')
begin
	declare @checkoutTypeValue nvarchar(100)

	set @checkoutTypeValue = case
		when exists (select * from AppConfig where Name like 'Vortx.OnePageCheckout.%') and exists (select * from AppConfig where Name = 'Checkout.UseOnePageCheckout' and (ConfigValue like 'true' or ConfigValue like '1' or ConfigValue like 'yes')) then 'SmartOPC'
		when exists (select * from AppCOnfig where Name = 'Checkout.UseOnePageCheckout' and (ConfigValue like 'true' or ConfigValue like '1' or ConfigValue like 'yes')) then 'BasicOPC'
		else 'Standard'
	end

	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Checkout.Type', 'The Checkout Type.  Valid Values are Standard, SmartOPC, BasicOPC, or Other.', @checkoutTypeValue, 'enum', 'Standard,SmartOPC,BasicOPC,Other', 'CHECKOUT', 1, 0, getdate())
end
else
begin
	update AppConfig set
		Description = 'The Checkout Type.  Valid Values are Standard, SmartOPC, BasicOPC, or Other.',
		AllowableValues = 'Standard,SmartOPC,BasicOPC,Other'
	where
		Name = 'Checkout.Type'
		and Description like 'The Checkout Type.  Valid Values are Standard,SmartOPC,DeprecatedOPC,or Other.'
		and AllowableValues = 'Standard,SmartOPC,DeprecatedOPC,Other'

	update AppConfig set
		ConfigValue = 'BasicOPC'
	where
		Name = 'Checkout.Type'
		and ConfigValue = 'DeprecatedOPC'
end
go

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'Anonymous.AllowAlreadyRegisteredEmail') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) VALUES('Anonymous.AllowAlreadyRegisteredEmail', 'DISPLAY', 'If true anonymous users will be able to checkout with email addresses that are already in use. If AllowCustomerDuplicateEMailAddresses is true this has no effect.', 'false', 'boolean', 'false')
END
GO

if not exists (select * from AppConfig where Name = 'QuantityDiscount.PercentDecimalPlaces')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'QuantityDiscount.PercentDecimalPlaces', 'The number of decimal places to show on percent quantity discounts', '0', 'integer', '', 'DISPLAY', 1, 0, getdate())
GO
if not exists (select * from AppConfig where Name = 'VAT.VATCheckServiceURL')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'VAT.VATCheckServiceURL', 'The endpoint for the VATCheck service.', 'http://ec.europa.eu/taxation_customs/vies/services/checkVatService', 'string', '', 'VAT', 1, 0, getdate())
GO
if not exists (select * from AppConfig where Name = 'ContactUs.UseCaptcha')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'ContactUs.UseCaptcha', 'Whether or not the contact us control protects against scripts with a captcha.', 'true', 'boolean', '', 'SECURITY', 1, 0, getdate())
GO
if not exists (select * from AppConfig where Name = 'PaymentExpress.Url')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'PaymentExpress.Url', 'The PaymentExpress endpoint.', 'https://www.paymentexpress.com/pxpost.aspx', 'string', '', 'GATEWAY', 1, 0, getdate())
GO

update appconfig set [description] = 'If you have multiple distributors, set this true to allow real time shipping rates to be calculated for the products based on the address of the distributor that its assigned to.' where name = 'RTShipping.MultiDistributorCalculation' and convert(nvarchar(max),[description]) = '*BETA* - If you have multiple distributors, set this true to allow real time shipping rates to be calculated for the products based on the address of the distributor that its assigned to.  Currently only available for UPS.'
GO

--==== AVALARA ====--
PRINT 'Creating MultiShipOrder_Shipment table for Avalara to work with Multiship functionality ...'
if not exists (select * from sysobjects where ID = object_id('MultiShipOrder_Shipment'))
	CREATE TABLE [dbo].[MultiShipOrder_Shipment](
		[MultiShipOrder_ShipmentId] [int] IDENTITY(1,1) NOT NULL,
		[MultiShipOrder_ShipmentGUID] [uniqueidentifier] NOT NULL,
		[OrderNumber] [int] NOT NULL,
		[DestinationAddress] [xml] NOT NULL,
		[ShippingAmount] [money] NOT NULL,
		[ShippingMethodId] [int] NOT NULL,
		[ShippingAddressId] [int] NOT NULL,
		[BillingAddressId] [int] NOT NULL
	) ON [PRIMARY]
GO

if not exists (select * from AppConfig where name like 'AvalaraTax.Enabled')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.Enabled', 'Set to true to use Avalara for tax calculations.', 'False', 'boolean', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.Account')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.Account', 'The account provided to you by Avalara.', '', 'string', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.License')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.License', 'The license provided to you by Avalara.', '', 'string', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.ServiceUrl')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.ServiceUrl', 'The service URL provided to you by Avalara.', '', 'string', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.CompanyCode')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.CompanyCode', 'The company code provided to you by Avalara.', '', 'string', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.TaxAddress')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.TaxAddress', 'Set to "Billing" to use the customer''s billing address for tax calculations. Set to "Shipping" to use the customer''s shipping address for tax calculations. This should not need to be changed.', 'Shipping', 'enum', 'Shipping,Billing', 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.TaxRefunds')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.TaxRefunds', 'Set to true to charge tax on order refunds. This should not need to be changed.', 'False', 'boolean', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.DetailLevel')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.DetailLevel', 'The detail level used with Avalara Tax. Valid values are "summary", "line", "document", "diagnostic", and "tax" This should not need to be changed.', 'Tax', 'enum', 'Summary,Line,Document,Diagnostic,Tax', 'AVALARATAX', 0, 0, getdate())
go

if not exists (select * from AppConfig where name like 'RTShipping.FedEx.SmartPost.IndiciaWeights')
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue])
VALUES (N'RTShipping.FedEx.SmartPost.IndiciaWeights', N'The Indicia weight breaks for Smart Post Shipments.  Format: IndiciaWeightRangeLow-IndiciaWeightRangeHigh:IndiciaType', N'string', NULL, N'RTSHIPPING', 1, 0, '0-0.99:PRESORTED_STANDARD,1-69.99:PARCEL_SELECT')

if not exists (select * from AppConfig where name like 'RTShipping.FedEx.SmartPost.AncillaryEndorsementType')
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue])
VALUES (N'RTShipping.FedEx.SmartPost.AncillaryEndorsementType', N'The Ancillary Endorsement Type for Smart Post Shipments.  Optional. 	Valid Values: "ADDRESS CORRECTION", "CARRIER LEAVE IF NO RESPONSE", "CHANGE SERVICE", "FORWARDING SERVICE", "RETURN SERVICE"', N'string', NULL, N'RTSHIPPING', 1, 0, '')

if not exists (select * from AppConfig where name like 'RTShipping.FedEx.SmartPost.HubId')
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue])
VALUES (N'RTShipping.FedEx.SmartPost.HubId', N'The HubId for your Smart Post Shipments.  See your FedEx account manager.', N'string', NULL, N'RTSHIPPING', 1, 0, '5531')

if not exists (select * from AppConfig where name like 'RTShipping.FedEx.SmartPost.Enabled')
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue])
VALUES (N'RTShipping.FedEx.SmartPost.Enabled', N'Enables the FedEx Smart Post service.  See your FedEx account manager for more information.', N'boolean', NULL, N'RTSHIPPING', 1, 0, 'false')
GO

--== Vortx OPC ==--
update AppConfig
set [Description] = 'This AppConfig has been deprecated. Use Checkout.Type instead.'
where Name = 'Checkout.UseOnePageCheckout'

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.ShowEmailPreferencesOnCheckout')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Vortx.OnePageCheckout.ShowEmailPreferencesOnCheckout', 'Set to true to allow customers to select email opt in/out on one page checkout.', 'true', 'boolean', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.UseZipcodeService')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Vortx.OnePageCheckout.UseZipcodeService', 'Set to true to use automatic city/state lookup service based on zip-code.', 'true', 'boolean', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.ShowCreateAccount')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Vortx.OnePageCheckout.ShowCreateAccount', 'True to show the create account panel on SmartOPC, false to hide.', 'true', 'boolean', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.CustomerServiceTopic')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Vortx.OnePageCheckout.CustomerServiceTopic', 'The name of the topic used for Customer Service panel on SmartOPC.', 'OPC.CustomerServicePanel', 'string', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.LicenseKey')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Vortx.OnePageCheckout.LicenseKey', 'Enter your LicenseKey for the Smart OnePageCheckout.  Contact customer service if you do not have a LicenseKey.', '', 'string', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.AddressLocale')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Vortx.OnePageCheckout.AddressLocale', 'Address Locale', 'US', 'string', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.OPCStyleSheetName')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Vortx.OnePageCheckout.OPCStyleSheetName', 'Stylesheet used for SmartOPC.', 'onepagecheckout.css', 'string', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.ZipCodeService.Timeout')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Vortx.OnePageCheckout.ZipCodeService.Timeout', 'Do Not Change', '10', 'string', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.ScriptManagerId')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Vortx.OnePageCheckout.ScriptManagerId', 'Do Not Change', 'scrptMgr', 'string', null, 'CHECKOUT', 1, 0, getdate())
end

IF (NOT EXISTS (SELECT TopicID FROM Topic WHERE Name='OPC.CustomerServicePanel'))
	INSERT INTO Topic(Name,Title,Description,SkinID,ShowInSiteMap)
	VALUES('OPC.CustomerServicePanel','OPC.CustomerServicePanel',
	'<div class="opc-container-header customer-service-header">
	<span class="opc-container-inner">
	Customer Service</span></div>
<div class="opc-container-body customer-service-body">
	<div class="page-links opc-container-inner customer-service-links">
		<a href="t-shipping.aspx" target="_blank">Shipping Info</a>
		<a href="t-returns.aspx" target="_blank">Return Policy</a>
		<a href="t-security.aspx" target="_blank">Security Policy</a>
		<a href="t-contact.aspx" target="_blank">Contact Us</a>
	</div>
	<div class="opc-container-inner customer-service-phone">
		Call Us At:<br />
		1.800.555.1234
	</div>
</div>', 0, 0);
GO

-- Add CheckoutByAmazon as a payment method option
if not exists (select * from AppConfig where Name = 'PaymentMethods' and AllowableValues like '%CheckoutByAmazon%')
	update AppConfig
	set AllowableValues = AllowableValues + ', CheckoutByAmazon'
	where Name = 'PaymentMethods'
GO

if not exists (select * from AppConfig where Name = 'Signin.SkinMaster')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Signin.SkinMaster', 'This config determines what skinid will be applied to a user after they sign in. Session: the skinid will be set based on the current session. Default: the skin id will be set to the site default. ', 'Default', 'enum', 'Session, Default', 'DISPLAY', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'QuantityDiscount.CombineQuantityByProduct')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'QuantityDiscount.CombineQuantityByProduct', 'If this is false then quantity discounts will be calculated per line item. If this is true then quantity discounts should be applied based on agregated quantities of product IDs.', 'false', 'boolean', '', 'DISPLAY', 1, 0, getdate())
end

--== Authorize.NET CIM ==--
if not exists (select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'Customer' and COLUMN_NAME = 'CIM_ProfileId')
	alter table Customer add CIM_ProfileId bigint

if not exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'CIM_AddressPaymentProfileMap')
	create table [dbo].[CIM_AddressPaymentProfileMap](
		[CIMId] int identity(1,1) not null primary key clustered,
		[AddressId] int not null foreign key([AddressId]) references [dbo].[Address] ([AddressID]),
		[CustomerId] int not null foreign key([CustomerId]) references [dbo].[Customer] ([CustomerID]),
		[AuthorizeNetProfileId] bigint not null,
		[ExpirationMonth] nvarchar(10) not null,
		[ExpirationYear] nvarchar(10) not null,
		[CardType] nvarchar(50) not null,
		[Primary] bit not null default (0)
	)
go

if not exists(select * from AppConfig Where Name='AUTHORIZENET_Cim_LiveServiceURL')
	insert into [AppConfig] (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) values(newid(), 0, 'AUTHORIZENET_Cim_LiveServiceURL', 'CIM live Service URL. Do Not Change.', 'https://api.authorize.net/soap/v1/Service.asmx', 'string', null, 'ADMIN', 0, 0, getdate());

if not exists(select * from AppConfig Where Name='AUTHORIZENET_Cim_SandboxServiceURL')
	insert into [AppConfig] (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) values(newid(), 0, 'AUTHORIZENET_Cim_SandboxServiceURL', 'CIM sandbox Service URL. Do Not Change.', 'https://apitest.authorize.net/soap/v1/Service.asmx', 'string', null, 'ADMIN', 0, 0, getdate());

if not exists(select * from AppConfig Where Name='AUTHORIZENET_Cim_UseSandbox')
	insert into [AppConfig] (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) values(newid(), 0, 'AUTHORIZENET_Cim_UseSandbox', 'Set to true to use CIM in sandbox mode.', 'false', 'boolean', null, 'ADMIN', 0, 0, getdate());

if not exists(select * from AppConfig Where Name='AUTHORIZENET_Cim_Enabled')
	insert into [AppConfig] (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) values(newid(), 0, 'AUTHORIZENET_Cim_Enabled', 'Set to true to enable CIM.', 'true', 'boolean', null, 'ADMIN', 0, 0, getdate());

if not exists (select * from AppConfig where Name = 'Bongo.Extend.Script')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Bongo.Extend.Script', 'The location of your Bongo Extend script', 'https://s3bongous.s3.amazonaws.com/extend/loader/0deeb03b99e174cd366c6db87f6be71e.js', 'string', null, 'SHIPPING', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Bongo.Extend.Enabled')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Bongo.Extend.Enabled', 'Set to true to enable Bongo Extend', 'false', 'boolean', null, 'SHIPPING', 1, 0, getdate())
end

--Start 9.2.x.x updates--
 IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[ObjectView]') AND type = 'V')
        DROP VIEW [dbo].ObjectView
    GO

CREATE VIEW [dbo].[ObjectView]
AS
SELECT	EM.EntityID AS ID,
		EM.EntityType AS EntityType,
		EM.[Name],
		EM.Description
FROM EntityMaster AS EM WITH (NOLOCK)

UNION ALL

SELECT	tp.TopicID AS ID,
		'Topic' AS EntityType,
		tp.[Name],
		tp.Description
FROM Topic AS tp WITH (NOLOCK)

UNION ALL

SELECT	nw.NewsID AS ID,
		'News' AS EntityType,
		nw.Headline AS [Name],
		'' AS Description
FROM News AS nw WITH(NOLOCK)

UNION ALL

SELECT	p.ProductID AS ID,
		'Product' AS EntityType,
		p.[Name],
		p.Description
FROM Product AS p WITH(NOLOCK)

UNION ALL

SELECT	cp.CouponID AS ID,
		'Coupon' AS EntityType,
		cp.[CouponCode] AS [Name],
		cp.Description
FROM Coupon AS cp WITH(NOLOCK)

UNION ALL

SELECT	oo.OrderOptionID AS ID,
		'OrderOption' AS EntityType,
		oo.[Name],
		oo.Description
FROM OrderOption oo WITH(NOLOCK)

UNION ALL

SELECT	gc.GiftCardID AS ID,
		'GiftCard' AS EntityType,
		gc.SerialNumber AS [Name],
		'' AS Description
FROM GiftCard AS gc WITH(NOLOCK)

UNION ALL

SELECT	sm.ShippingMethodID AS ID,
		'ShippingMethod' AS EntityType,
		sm.[Name] AS [Name],
		'' AS Description
FROM ShippingMethod AS sm WITH(NOLOCK)

GO

---------------- 9.3.0.0 -------------------------------------
-- Add PayPalPayments as a payment method option
if not exists (select * from AppConfig where Name = 'PaymentMethods' and AllowableValues like '%PayPal Payments Advanced%')
	update AppConfig
	set AllowableValues = AllowableValues + ',PayPal Payments Advanced'
	where Name = 'PaymentMethods'
GO

if not exists (select * From sysobjects where id = object_id('OrderTransaction') and type = 'u')
BEGIN
	CREATE TABLE dbo.OrderTransaction
		(
		OrderTransactionID int NOT NULL IDENTITY (1, 1),
		OrderNumber int NOT NULL,
		TransactionType nvarchar(100) NOT NULL,
		TransactionCommand nvarchar(MAX) NULL,
		TransactionResult nvarchar(MAX) NULL,
		PNREF nvarchar(400) NULL,
		Code nvarchar(400) NULL,
		PaymentMethod nvarchar(100) NULL,
		PaymentGateway nvarchar(100) NULL,
		Amount money NULL
		)
	ALTER TABLE dbo.OrderTransaction ADD CONSTRAINT
		PK_OrderTransaction PRIMARY KEY CLUSTERED
		(
		OrderTransactionID
		) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

/*********** 1st Pay Gateway *************/

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.AdminTransactionEmail.Enable')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.AdminTransactionEmail.Enable'
		,'This will enable/disable the 1stPay gateway to send out an email to the store admin for each transaction.'
		,'GATEWAY'
		,'false'
		,'boolean'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.CustomerTransactionEmail.Enable')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.CustomerTransactionEmail.Enable'
		,'This will enable/disable the 1stPay gateway to send out an email to the customer when placing an order.'
		,'GATEWAY'
		,'false'
		,'boolean'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.Level2.Enable')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.Level2.Enable'
		,'This will enable/disable the 1stPay gateway level 2 fields to be passed up on transactions. This must be enabled on the 1stPay account.'
		,'GATEWAY'
		,'false'
		,'boolean'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.Cim.Enable')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.Cim.Enable'
		,'This will enable/disable the 1stPay gateway Cim feature. This must be enabled on the 1stPay account.'
		,'GATEWAY'
		,'false'
		,'boolean'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.TransactionCenterId')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.TransactionCenterId'
		,'This is your unique Transaction Center number, NOT your 16 digit Merchant ID.'
		,'GATEWAY'
		,''
		,'integer'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.ProcessorId')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.ProcessorId'
		,'Processor Id it can be retrieved from the gateway options section in the 1stPay Transaction Center.'
		,'GATEWAY'
		,''
		,'string'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.GatewayId')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.GatewayId'
		,'This is your Alpha-numeric passphrase assigned by 1stPayGateway, it can be retrieved from the gateway options section in the 1stPay Transaction Center.'
		,'GATEWAY'
		,''
		,'string'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.EmailHeader')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.EmailHeader'
		,'This is an optional header that can be added to the emails which can be configured to send out for each transaction.  This should be short and plain text (like your company name and phone number).'
		,'GATEWAY'
		,''
		,'string'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.EmailFooter')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.EmailFooter'
		,'This is an optional footer that can be added to the emails which can be configured to send out for each transaction.  This should be short and plain text (like your company name and phone number).'
		,'GATEWAY'
		,''
		,'string'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.PaymentModuleURL')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.PaymentModuleURL'
		,'This is the payment module url for the 1stPay gateway. This should not be changed'
		,'GATEWAY'
		,'https://secure.1stpaygateway.net/secure/gateway/pm.aspx'
		,'string'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.XmlURL')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.XmlURL'
		,'This is the xml url for the 1stPay gateway. This should not be changed'
		,'GATEWAY'
		,'https://secure.1stpaygateway.net/secure/gateway/xmlgateway.aspx'
		,'string'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.TestProccessor')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.TestProccessor'
		,'This is the test processor for the 1stPay gateway. This should not be changed'
		,'GATEWAY'
		,'sandbox'
		,'string'
		, null
	);

/*********** End 1st Pay Gateway *************/

/**************TFS 529*****************/
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_ProductSequence]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_ProductSequence]
GO

create proc [dbo].[aspdnsf_ProductSequence]
    @positioning varchar(10), -- allowed values: first, next, previous, last
    @ProductID int,
    @EntityName varchar(20),
    @EntityID int,
    @ProductTypeID int = null,
    @IncludeKits tinyint = 1 ,
    @IncludePacks tinyint = 1 ,
    @SortByLooks tinyint = 0,
    @CustomerLevelID int = null,
    @affiliateID     int = null,
    @StoreID	int = null,
    @FilterProductsByStore tinyint = 0,
    @FilterOutOfStockProducts tinyint = 0

AS
BEGIN
    SET NOCOUNT ON


    DECLARE @id int, @row int
    DECLARE @affiliatecount int
    CREATE TABLE #sequence (row int identity not null, productid int not null)

    DECLARE @FilterProductsByAffiliate tinyint, @FilterProductsByCustomerLevel tinyint, @HideProductsWithLessThanThisInventoryLevel int, @CustomerLevelFilteringIsAscending bit

    SET @FilterProductsByCustomerLevel = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @FilterProductsByAffiliate = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @HideProductsWithLessThanThisInventoryLevel = (SELECT TOP 1 case ConfigValue when -1 then 0 else ConfigValue end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)

    SET @CustomerLevelFilteringIsAscending  = 0
    SET @CustomerLevelFilteringIsAscending = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterByCustomerLevelIsAscending' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)

    select @affiliatecount    = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductAffiliate') and si.indid < 2 and type = 'u'

    IF @positioning not in ('first', 'next', 'previous', 'last')
        SET @positioning = 'first'

    insert #sequence (productid)
    select pe.productid
    from dbo.ProductEntity             pe  with (nolock)
        join [dbo].Product             p   with (nolock) on p.ProductID = pe.ProductID and pe.EntityType = @EntityName and pe.EntityID = @EntityID
        left join ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID
        left join ProductAffiliate     pa  with (nolock) on p.ProductID = pa.ProductID
		left join ProductVariant pv		   with (nolock) on p.ProductID = pv.ProductID  and pv.IsDefault = 1
        left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i  on pv.VariantID = i.VariantID
    where p.ProductTypeID = coalesce(nullif(@ProductTypeID, 0), p.ProductTypeID) and p.Deleted = 0 and p.Published = 1 and p.IsAKit <= @IncludeKits and p.IsAPack <= @IncludePacks
          and (case
                when @FilterProductsByCustomerLevel = 0 then 1
                when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID or pcl.CustomerLevelID is null then 1
                when @CustomerLevelID=0 and pcl.CustomerLevelID is null then 1
                when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1
                else 0
               end  = 1
              )
          and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliatecount = 0 or @FilterProductsByAffiliate = 0)
          and ((case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end >= @HideProductsWithLessThanThisInventoryLevel) OR @FilterOutOfStockProducts = 0)
		  and (p.ProductID IN (SELECT ProductID FROM ProductStore WHERE StoreID=@StoreID) OR @FilterProductsByStore = 0)
order by pe.DisplayOrder, p.Name



    SELECT @row = row FROM #sequence WHERE ProductID = @ProductID

    IF @positioning = 'next' BEGIN
        SELECT top 1 @id = ProductID
        FROM #sequence
        WHERE row > @row
        ORDER BY row

        IF @id is null
            SET @positioning = 'first'
    END


    IF @positioning = 'previous' BEGIN
        SELECT top 1 @id = ProductID
        FROM #sequence
        WHERE row < @row
        ORDER BY row desc

        IF @id is null
            SET @positioning = 'last'
    END


    IF @positioning = 'first'
        SELECT top 1 @id = ProductID
        FROM #sequence
        ORDER BY row

    IF @positioning = 'last'
        SELECT top 1 @id = ProductID
        FROM #sequence
        ORDER BY row desc

    SELECT ProductID, SEName FROM dbo.Product with (nolock) WHERE ProductID = @id

END

GO
/**************End TFS 529*****************/

/************** TFS 597 - PayPal Icon url update *****************/

If Exists(Select * From dbo.AppConfig Where name = 'PayPal.Express.ButtonImageURL' And ConfigValue = 'https://www.paypal.com/en_US/i/bnr/bnr_paymentsBy_150x40.gif')
Begin
	Update AppConfig
	Set ConfigValue = 'https://www.paypalobjects.com/en_US/i/btn/btn_xpressCheckout.gif'
	Where Name = 'PayPal.Express.ButtonImageURL';
End

If Not Exists(Select * From dbo.AppConfig Where name = 'PayPal.Express.ButtonImageURL')
Begin

	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'PayPal.Express.ButtonImageURL'
		,'URL for Express Checkout Button Image'
		,'GATEWAY'
		,'https://www.paypalobjects.com/en_US/i/btn/btn_xpressCheckout.gif'
		,'string'
		, null
	);

End

If Exists(Select * From dbo.AppConfig Where name = 'PayPal.PaymentIcon' And ConfigValue = 'https://www.paypal.com/en_US/i/logo/PayPal_mark_50x34.gif')
Begin
	Update AppConfig
	Set ConfigValue = 'https://www.paypalobjects.com/en_US/i/logo/PayPal_mark_50x34.gif'
	Where Name = 'PayPal.PaymentIcon';
End

If Not Exists(Select * From dbo.AppConfig Where name = 'PayPal.PaymentIcon')
Begin

	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'PayPal.PaymentIcon'
		,'Image URL for Paypal payment method icon.'
		,'GATEWAY'
		,'https://www.paypalobjects.com/en_US/i/logo/PayPal_mark_50x34.gif'
		,'string'
		, null
	);

End

/************** TFS 600 *********************/

	IF NOT EXISTS (select * from [dbo].AppConfig where Name = 'PayFlowPro.Product')
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayFlowPro.Product','GATEWAY','If using PayPal PayFlow PRO merchant gateway, this is the specific product that uses the PayFlowPro gateway.','');
	GO

/************** END TFS 600 *********************/

/**************TFS 234*****************/
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetVariantsPaged]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetVariantsPaged]
GO

CREATE PROCEDURE [dbo].[aspdnsf_GetVariantsPaged]
(
	@pageSize int,
	@startIndex int,
	@EntityFilterType int = 0,
	@EntityFilterID int = 0
)
AS
BEGIN
SET NOCOUNT ON

DECLARE @Filter TABLE (productID int)

IF (@EntityFilterID <> 0 AND @EntityFilterType <> 0) BEGIN
	IF @EntityFilterType = 1
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductCategory WHERE CategoryID = @EntityFilterID
	IF @EntityFilterType = 2
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductManufacturer WHERE ManufacturerID = @EntityFilterID
	IF @EntityFilterType = 3
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductSection WHERE SectionID = @EntityFilterID
	IF @EntityFilterType = 4
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductDistributor WHERE DistributorID = @EntityFilterID
END;

WITH OrderedVariants AS
(
	SELECT	pv.variantID,
			pv.IsDefault,
			pv.Name,
			pv.Description,
			pv.ProductID,
			pv.SkuSuffix,
			pv.Price,
			pv.SalePrice,
			pv.Inventory,
			pv.Published,
			pv.Deleted,
			p.TrackInventoryBySizeAndColor,
			p.Name as ProductName,
			ROW_NUMBER() OVER(ORDER BY pv.variantID) as RowNum
			FROM ProductVariant pv WITH (NOLOCK)
			JOIN Product p on p.ProductID = pv.ProductID
			WHERE pv.Deleted = 0
				AND ((@EntityFilterType = 0 OR @EntityFilterID = 0) OR
					pv.ProductID in (SELECT ProductID from @Filter))
)
SELECT	TOP (@pageSize) variantID,
		IsDefault,
		Name,
		Description,
		ProductID,
		SkuSuffix,
		Price,
		SalePrice,
		Inventory,
		Published,
		Deleted,
		TrackInventoryBySizeAndColor,
		ProductName
FROM OrderedVariants
WHERE RowNum > @startIndex

END
GO
/**************END TFS 234*****************/

/************* TFS XXX ********************/
update AppConfig set [Description] = 'Set to true to use Avalara for tax calculations.<br /><a  href="AvalaraConnectionTest.aspx" class="lightboxLink">Click here to test your AvaTax connection</a><br /><a href="https://admin-avatax.avalara.net/" target="_blank">Click here for your AvaTax Admin Console</a>' where Name = 'AvalaraTax.Enabled' and  [Description] like 'Set to true to use Avalara for tax calculations.'
update AppConfig set [Description] = 'Unused' where Name = 'AvalaraTax.DetailLevel'

/************* END TFS XXX ****************/

/************ TFS 626 *********************/

IF NOT EXISTS (SELECT * FROM Topic WHERE Name='SubscriptionToken.Subscribe')
Begin
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description)
	values('SubscriptionToken.Subscribe'
			, 1
			, 0
			, 'SubscriptionToken.Subscribe'
			, ''
			);
End
Go

/********************* End TFS 626 **********************/

/************* TFS 549 & TFS 796 ********************/
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_ProductInfo]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_ProductInfo]
GO

CREATE proc [dbo].[aspdnsf_ProductInfo]
    @ProductID          int,
    @CustomerLevelID    int,
    @DefaultVariantOnly tinyint,
    @InvFilter          int = 0,
    @AffiliateID        int = null,
    @PublishedOnly      tinyint = 1,
    @IsAdmin			tinyint = 0,
    @StoreID			int = 0
AS BEGIN
	SET NOCOUNT ON
	DECLARE
		@CustLevelExists int,
		@AffiliateExists int,
		@FilterProductsByAffiliate tinyint,
		@FilterProductsByCustomerLevel tinyint,
		@CustomerLevelFilteringIsAscending tinyint,
		@CustomerLevelCount int,
		@AffiliateCount int,
		@MinProductCustomerLevel int,
		@HideProductsWithLessThanThisInventoryLevel int

	SELECT @FilterProductsByCustomerLevel		= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterProductsByCustomerLevel'		AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
	SELECT @CustomerLevelFilteringIsAscending	= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterByCustomerLevelIsAscending'	AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
	SELECT @FilterProductsByAffiliate			= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterProductsByAffiliate'			AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
	SELECT @HideProductsWithLessThanThisInventoryLevel	= CONVERT(INT, ConfigValue) FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'HideProductsWithLessThanThisInventoryLevel' AND ISNUMERIC(ConfigValue) = 1 AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
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
			p.*,
			pv.VariantID, pv.name VariantName, pv.Price, pv.Description VariantDescription, isnull(pv.SalePrice, 0) SalePrice, isnull(SkuSuffix, '') SkuSuffix, pv.Dimensions, pv.Weight, isnull(pv.Points, 0) Points, pv.Inventory, pv.ImageFilenameOverride VariantImageFilenameOverride,  pv.isdefault, pv.CustomerEntersPrice, isnull(pv.colors, '') Colors, isnull(pv.sizes, '') Sizes,
			sp.name SalesPromptName,
			case when pcl.productid is null then 0 else isnull(e.Price, 0) end ExtendedPrice
		FROM dbo.Product p with (nolock)
			join dbo.productvariant            pv  with (NOLOCK) on p.ProductID = pv.ProductID
			join dbo.SalesPrompt               sp  with (NOLOCK) on p.SalesPromptID = sp.SalesPromptID
			left join dbo.ExtendedPrice        e   with (NOLOCK) on pv.VariantID=e.VariantID and e.CustomerLevelID = @CustomerLevelID
			left join dbo.ProductCustomerLevel pcl with (NOLOCK) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID
			left join (select variantid, sum(quan) inventory from inventory group by variantid) i on pv.variantid = i.variantid
		WHERE p.ProductID = @ProductID
			and p.Deleted = 0
			and pv.Deleted = 0
			and p.Published >= @PublishedOnly
			and pv.Published >= @PublishedOnly
			and pv.IsDefault >= @DefaultVariantOnly
			and (case p.TrackInventoryBySizeAndColor when 1 then isnull(i.inventory, 0) else pv.inventory end >= @InvFilter or @InvFilter = 0)
		ORDER BY pv.DisplayOrder, pv.name
END
GO
/************* END TFS 549 ****************/

/************* TFS 552 **************************/
/*    This goes in the 8012-90 upgrade script   */
/************************************************/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE parent_obj = OBJECT_ID(N'[ZipTaxRate]') AND type = 'D' And (Name Like 'DF__ZipTaxRat__Count__%' OR Name = 'DF_ZipTaxRate_CountryID'))
BEGIN
	DECLARE @constraintName nvarchar(max)
	SELECT @constraintName = Name FROM dbo.sysobjects WHERE parent_obj = OBJECT_ID(N'[ZipTaxRate]') AND type = 'D' And (Name Like 'DF__ZipTaxRat__Count__%' OR Name = 'DF_ZipTaxRate_CountryID')
	EXEC('ALTER TABLE [dbo].[ZipTaxRate] DROP CONSTRAINT ' + @constraintName)
END

DECLARE @countryID int
DECLARE @SQL nvarchar(max)
SELECT @countryID = CountryID FROM Country WHERE Name='United States'
SET @sql = 'ALTER TABLE [dbo].[ZipTaxRate] ADD CONSTRAINT [DF_ZipTaxRate_CountryID] DEFAULT ' + CAST(@countryID as nvarchar) + ' FOR CountryID'
EXEC(@sql)
GO
/************* END TFS 552 ****************/

/************* TFS 180 ****************/
If Not Exists(Select * From dbo.AppConfig Where name = 'SendLowStockWarnings')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'SendLowStockWarnings'
		,'If true, store admins will be sent an email when purchases take product inventory levels below the value specified in the SendLowStockWarningsThreshold AppConfig.'
		,'OUTOFSTOCK'
		,'false'
		,'boolean'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = 'SendLowStockWarningsThreshold')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'SendLowStockWarningsThreshold'
		,'This sets the threshold at which to start notifying store admins that a product is running low on stock, if SendLowStockWarnings is enabled.'
		,'OUTOFSTOCK'
		,'1'
		,'string'
		, null
	);

If Not Exists(Select * From dbo.AppConfig Where name = 'ShowAdminLowStockAudit')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'ShowAdminLowStockAudit'
		,'If this is set to true, a table of products with inventory levels lower than SendLowStockWarningsThreshold will be displayed on the admin home page.  NOTE: This may slow down the load time of the admin home page on very large sites.'
		,'OUTOFSTOCK'
		,'false'
		,'boolean'
		, null
	);
/************* END TFS 180 ****************/

/************* TFS 578 - SecureNetV4 App Configs ****************/

/***********************************************/
/* These configs need to be in the create      */
/* script as well as the update.  The original */
/* live urls are incorrect and need to be      */
/* replaced with the https versions.           */
/*                                             */
/***********************************************/

if not exists (select * from AppConfig where Name = 'SecureNetV4.LiveURL')
Begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
		values (newid()
					, 0
					, 'SecureNetV4.LiveURL'
					, 'Endpoint address for live SecureNet transactions'
					, 'https://gateway.securenet.com/api/Gateway.svc/soap'
					, 'string'
					, ''
					, 'GATEWAY'
					, 1
					, 0
					, getdate()
				);
End
Else if exists (select * from AppConfig where Name = 'SecureNetV4.LiveURL' And ConfigValue = 'http://gateway.securenet.com/api/Gateway.svc/soap')
Begin
	Update AppConfig
	Set ConfigValue = 'https://gateway.securenet.com/api/Gateway.svc/soap'
	Where Name = 'SecureNetV4.LiveURL'
	And ConfigValue = 'http://gateway.securenet.com/api/Gateway.svc/soap';
End

if not exists (select * from AppConfig where Name = 'SecureNetV4.DataAPI.LiveURL')
Begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid()
				, 0
				, 'SecureNetV4.DataAPI.LiveURL'
				, 'Endpoint address for live SecureNet data transactions'
				, 'https://gateway.securenet.com/api/data/service.svc/soap'
				, 'string'
				, ''
				, 'GATEWAY'
				, 1
				, 0
				, getdate()
			);
End
Else if exists (select * from AppConfig where Name = 'SecureNetV4.DataAPI.LiveURL' And ConfigValue = 'http://gateway.securenet.com/api/data/service.svc/soap')
Begin
	Update AppConfig
	Set ConfigValue = 'https://gateway.securenet.com/api/data/service.svc/soap'
	Where Name = 'SecureNetV4.DataAPI.LiveURL'
	And ConfigValue = 'http://gateway.securenet.com/api/data/service.svc/soap';
End
Go

/************* END TFS 578 - SecureNetV4 App Configs ****************/

/************* TFS 615 ****************/
/* I've already added this to the     */
/* create script, with it set to      */
/* true.  Upgrades we'll set it to    */
/* false instead.                     */
/**************************************/
if not exists (select * from AppConfig where Name = 'NameImagesBySEName')
Begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
		values (newid()
					, 0
					, 'NameImagesBySEName'
					, 'If true, product images will be named by the product SEName.  If false, product ID is used.'
					, 'false'
					, 'boolean'
					, ''
					, 'GENERAL'
					, 0
					, 0
					, getdate()
				);
End
/************* END TFS 615 ****************/

/************* TFS 665: Promotions ****************/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Promotions]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[Promotions](
			[Id] [int] IDENTITY(1,1) NOT NULL,
			[PromotionGuid] [uniqueidentifier] NOT NULL CONSTRAINT DF_Promotion_PromotionGUID DEFAULT(newid()),
			[Name] [varchar](100) NOT NULL,
			[Description] [varchar](max) NOT NULL,
			[UsageText] varchar(max) null,
			EmailText varchar(max) null,
			[Code] [varchar](50) NOT NULL CONSTRAINT [DF_Promotions_Code]  DEFAULT (''),
			[Priority] [numeric](18, 0) NOT NULL,
			[Active] [bit] NOT NULL,
			[AutoAssigned] [bit] NOT NULL,
			CallToAction NVARCHAR(MAX) NULL,
			[PromotionRuleData] [xml] NULL,
			[PromotionDiscountData] [xml] NULL,
		 CONSTRAINT [PK_Promotions] PRIMARY KEY CLUSTERED
		(
			[Id] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
		) ON [PRIMARY]
	END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' and TABLE_NAME = 'Promotions' AND COLUMN_NAME = 'CallToAction')
BEGIN
   ALTER TABLE dbo.Promotions ADD CallToAction NVARCHAR(MAX) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PromotionUsage]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[PromotionUsage](
			[Id] [int] IDENTITY(1,1) NOT NULL,
			[PromotionId] [int] NOT NULL,
			[CustomerId] [int] NOT NULL,
			[OrderId] [int] NULL,
			[DateApplied] [datetime] NULL,
			ShippingDiscountAmount MONEY NULL,
			LineItemDiscountAmount MONEY NULL,
			OrderDiscountAmount MONEY NULL,
			[DiscountAmount] [money] NULL,
			[Complete] [bit] NOT NULL CONSTRAINT [DF_PromotionUsage_Complete]  DEFAULT ((0)),
			Constraint FK_PromotionUsage_PromotionId Foreign Key (PromotionId) References Promotions(Id),
		 CONSTRAINT [PK_PromotionUsage] PRIMARY KEY CLUSTERED
		(
			[Id] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
		) ON [PRIMARY]
	END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PromotionLineItem]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [dbo].[PromotionLineItem](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[PromotionUsageId][int] Not Null,
		[shoppingCartRecordId] [int] NOT NULL,
		[productId] [int] NOT NULL,
		[variantId] [int] NOT NULL,
		[sku] [nvarchar](150) NOT NULL,
		[quantity] [int] NOT NULL,
		[cartPrice] [money] NOT NULL,
		[subTotal] [money] NOT NULL,
		[isAGift] [bit] NOT NULL,
		[discountAmount] [money] NOT NULL,
		Constraint FK_PromotionLineItem_PromotionUsageId Foreign Key (PromotionUsageId) References PromotionUsage(Id) On Delete Cascade,
	 CONSTRAINT [PK_[PromotionLineItem] PRIMARY KEY CLUSTERED
	(
		[id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PromotionStore]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [dbo].[PromotionStore]
	(
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[PromotionID] [INT] NOT NULL,
		[StoreID] [INT] NOT NULL,
		[CreatedOn] [datetime] NOT NULL CONSTRAINT DF_PromotionStore_CreatedOn DEFAULT(getdate())
		, Constraint FK_PromotionStore_PromotionId Foreign Key (PromotionId) References Promotions(Id) On Delete Cascade
		, Constraint FK_PromotionStore_StoreID Foreign Key (StoreId) References Store(StoreId) On Delete Cascade
		, Constraint AK_PromotionStore Unique(PromotionId, StoreId),
		CONSTRAINT [PK_PromotionStore] Primary Key (Id)
	)
	END
GO

If Not Exists (select * from information_schema.columns where table_name = 'Orders_ShoppingCart' and column_name = 'IsGift')
Begin
	Alter Table Orders_ShoppingCart Add IsGift bit NOT NULL Constraint DF_Orders_ShoppingCart_IsGift Default(0);
End
GO

If Not Exists (select * from information_schema.columns where table_name = 'ShoppingCart' and column_name = 'IsGift')
Begin
	Alter Table ShoppingCart Add IsGift bit NOT NULL Constraint DF_ShoppingCart_IsGift Default(0);
End
GO

ALTER VIEW StoreMappingView
	AS
	SELECT ID, StoreID, EntityID, EntityType FROM EntityStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, ProductID EntityID, 'Product' EntityType FROM ProductStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, NewsID EntityID, 'News' EntityType FROM NewsStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, TopicID EntityID, 'Topic' EntityType FROM TopicStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, CouponID EntityID, 'Coupon' EntityType FROM CouponStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, PromotionID EntityID, 'Promotion' EntityType FROM PromotionStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, OrderOptionID EntityID, 'OrderOption' EntityType FROM OrderOptionStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, GiftCardID EntityID, 'GiftCard' EntityType FROM GiftCardStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, ShippingMethodID EntityID, 'ShippingMethod' EntityType FROM ShippingMethodStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, AffiliateID EntityID, 'Affiliate' EntityType FROM AffiliateStore WITH (NOLOCK)
GO

ALTER VIEW MappedObjects
	AS
	SELECT
		EM.EntityID AS ID,
		EM.EntityType AS EntityType,
		ParentEntityID AS ParentID,
		EM.EntityGUID AS GUID, EM.[Name] ,
		ES.StoreID AS StoreID
	FROM EntityMaster AS EM WITH (NOLOCK) LEFT JOIN EntityStore AS ES WITH (NOLOCK) ON ES.EntityID = EM.EntityID AND ES.EntityType = EM.EntityType
	UNION ALL
	SELECT TP.TopicID AS ID, 'Topic' AS EntityType,0 AS ParentID, TP.TopicGUID AS GUID, TP.[Name], TS.StoreID AS StoreID
	FROM Topic AS TP WITH (NOLOCK) LEFT JOIN StoreMappingView AS TS WITH (NOLOCK)
	ON TS.EntityID = TP.TopicID AND TS.EntityType='Topic'
	UNION ALL
	SELECT NW.NewsID AS ID,'News' AS EntityType,0 AS ParentID, NW.NewsGUID AS GUID, NW.Headline AS [Name], NS.StoreID AS StoreID
	FROM News AS NW LEFT JOIN StoreMappingView AS NS WITH (NOLOCK)
	ON NS.EntityID = NW.NewsID AND NS.EntityType='News'
	UNION ALL
	SELECT PR.ProductID AS ID, 'Product' AS EntityType,0 AS ParentID, PR.ProductGUID AS GUID, PR.[Name], PS.StoreID AS StoreID
	FROM Product AS PR LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
	ON PR.ProductID = PS.EntityID AND PS.EntityType='Product'
	UNION ALL
	SELECT CP.CouponID AS ID, 'Coupon' AS EntityType,0 AS ParentID, CP.CouponGUID AS GUID, CP.[CouponCode] AS [Name], PS.StoreID AS StoreID
	FROM Coupon AS CP LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
	ON CP.CouponID = PS.EntityID AND PS.EntityType='Coupon'
	UNION ALL
	SELECT Promo.Id AS ID, 'Promotion' AS EntityType,0 AS ParentID, Promo.PromotionGUID AS GUID, Promo.[Code] AS [Name], PS.StoreID AS StoreID
	FROM Promotions AS Promo LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
	ON Promo.Id = PS.EntityID AND PS.EntityType='Promotion'
	UNION ALL
	SELECT OO.OrderOptionID AS ID, 'OrderOption' AS EntityType,0 AS ParentID, OO.OrderOptionGUID AS GUID, OO.[Name], PS.StoreID AS StoreID
	FROM OrderOption AS OO LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
	ON OO.OrderOptionID = PS.EntityID AND PS.EntityType = 'OrderOption'
	UNION ALL
	SELECT GC.GiftCardID AS ID, 'GiftCard' AS EntityType,0 AS ParentID, GC.GiftCardGUID AS GUID, GC.SerialNumber AS [Name], PS.StoreID AS StoreID
	FROM GiftCard AS GC LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
	ON GC.GiftCardID = PS.EntityID AND PS.EntityType = 'GiftCard'
GO

ALTER PROC [dbo].[aspdnsf_CloneStoreMappings]
	@FromStoreID INT,
	@ToStoreID int
	AS
	BEGIN

		INSERT INTO EntityStore (StoreID, EntityID, EntityType)
		SELECT @ToStoreID AS [StoreID], EntityID, EntityType FROM EntityStore WHERE StoreID = @FromStoreID

		INSERT INTO AffiliateStore (StoreID, AffiliateID)
		SELECT @ToStoreID AS [StoreID], AffiliateID FROM AffiliateStore WHERE StoreID = @FromStoreID

		INSERT INTO NewsStore (StoreID, NewsID)
		SELECT @ToStoreID AS [StoreID], NewsID FROM NewsStore WHERE StoreID = @FromStoreID

		INSERT INTO ProductStore (StoreID, ProductID)
		SELECT @ToStoreID AS [StoreID], ProductID FROM ProductStore WHERE StoreID = @FromStoreID

		INSERT INTO TopicStore (StoreID, TopicID)
		SELECT @ToStoreID AS [StoreID], TopicID FROM TopicStore WHERE StoreID = @FromStoreID

		INSERT INTO GiftCardStore (StoreID, GiftCardID)
		SELECT @ToStoreID AS [StoreID], GiftCardID FROM GiftCardStore WHERE StoreID = @FromStoreID

		INSERT INTO CouponStore (StoreID, CouponID)
		SELECT @ToStoreID AS [StoreID], CouponID FROM CouponStore WHERE StoreID = @FromStoreID

		INSERT INTO PromotionStore (StoreID, PromotionID)
		SELECT @ToStoreID AS [StoreID], PromotionID FROM PromotionStore WHERE StoreID = @FromStoreID

		INSERT INTO OrderOptionStore(StoreID, OrderOptionID)
		SELECT @ToStoreID AS [StoreID], OrderOptionID FROM OrderOptionStore WHERE StoreID = @FromStoreID

		INSERT INTO ShippingMethodStore(StoreID, ShippingMethodID)
		SELECT @ToStoreID AS [StoreID], ShippingMethodID FROM ShippingMethodStore WHERE StoreID = @FromStoreID

		-- only create additional configs/string resources for non-default stores
		declare @isDefault tinyint
		select @isDefault = IsDefault from Store WHERE StoreID = @FromStoreID
		if(@isDefault <> 1)
		begin
		   INSERT INTO StringResource(StringResourceGUID, StoreID, [Name], LocaleSetting, ConfigValue)
			SELECT newid(), @ToStoreID, [Name], LocaleSetting, ConfigValue FROM StringResource WHERE StoreID = @FromStoreID

			INSERT INTO AppConfig(AppConfigGUID, StoreID, [Name], Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden)
			SELECT newid(), @ToStoreID, [Name], Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden FROM AppConfig WHERE StoreID = @FromStoreID
		end
	END
GO

ALTER PROC [dbo].[aspdnsf_NukeStoreMappings]
	@StoreID INT
	AS
	BEGIN
		DELETE FROM EntityStore WHERE StoreID = @StoreID
		DELETE FROM AffiliateStore WHERE StoreID = @StoreID
		DELETE FROM NewsStore WHERE StoreID = @StoreID
		DELETE FROM ProductStore WHERE StoreID = @StoreID
		DELETE FROM TopicStore WHERE StoreID = @StoreID
		DELETE FROM GiftCardStore WHERE StoreID = @StoreID
		DELETE FROM CouponStore WHERE StoreID = @StoreID
		DELETE FROM PromotionStore WHERE StoreID = @StoreID
		DELETE FROM OrderOptionStore WHERE StoreID = @StoreID
		DELETE FROM ShippingMethodStore WHERE StoreID = @StoreID

		-- only create additional configs/string resources for non-default stores
		declare @isDefault tinyint
		select @isDefault = IsDefault from Store WHERE StoreID = @StoreID
		if(@isDefault <> 1)
		begin
			DELETE FROM StringResource WHERE StoreID = @StoreID
			DELETE FROM AppConfig WHERE StoreID = @StoreID
		end
	END
GO

ALTER PROC NukeStore
	@StoreID INT,
	@NukeNews BIT = 0,
	@NukeAffiliates BIT =0,
	@NukeTopics BIT = 0,
	@NukeProducts BIT = 0,
	@NukeCoupons BIT = 0,
	@NukePromotions BIT = 0,
	@NukeOrderOptions BIT = 0,
	@NukeGiftCards BIT = 0,
	@NukeCategories BIT = 0,
	@NukeSections BIT = 0,
	@NukeManufacturers BIT = 0,
	@NukeDistributors BIT = 0,
	@NukeGenres BIT = 0,
	@NukeVectors BIT = 0
	AS
	CREATE TABLE #tmpEntities(
	GUID UniqueIdentifier
	)
	INSERT INTO #tmpEntities (GUID) (
	SELECT GUID FROM MappedObjects WHERE StoreID = @StoreID AND GUID NOT IN(
	SELECT GUID FROM MappedObjects WHERE StoreID <> @StoreID)
	)

	IF (@NukeNews = 1)			DELETE FROM News WHERE NewsGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeAffiliates = 1)	DELETE FROM Affiliate WHERE AffiliateGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeTopics = 1)		DELETE FROM Topic WHERE TopicGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeProducts = 1)		DELETE FROM Product WHERE ProductGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeCoupons = 1)		DELETE FROM Coupon WHERE CouponGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukePromotions = 1)	DELETE FROM Promotions WHERE PromotionGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeOrderOptions = 1)	DELETE FROM OrderOption WHERE OrderOptionGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeGiftCards = 1)		DELETE FROM GiftCard WHERE GiftCardGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeCategories = 1)	DELETE FROM Category WHERE CategoryGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeSections = 1)		DELETE FROM Section WHERE SectionGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeManufacturers = 1)	DELETE FROM Manufacturer WHERE ManufacturerGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeDistributors = 1)	DELETE FROM Distributor WHERE DistributorGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeGenres = 1)		DELETE FROM Genre WHERE GenreGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeVectors = 1)		DELETE FROM Vector WHERE VectorGUID IN (SELECT [GUID] FROM #tmpEntities)

	DELETE FROM EntityStore WHERE StoreID = @StoreID
	DELETE FROM AffiliateStore WHERE StoreID = @StoreID
	DELETE FROM NewsStore WHERE StoreID = @StoreID
	DELETE FROM ProductStore WHERE StoreID = @StoreID
	DELETE FROM TopicStore WHERE StoreID = @StoreID
	DELETE FROM GiftCardStore WHERE StoreID = @StoreID
	DELETE FROM CouponStore WHERE StoreID = @StoreID
	DELETE FROM OrderOptionStore WHERE StoreID = @StoreID
	DELETE FROM Store WHERE StoreID = @StoreID

	DROP TABLE #tmpEntities
GO

alter procedure [dbo].[aspdnsf_SaveMap]
	@StoreID INT,
	@EntityID INT,
	@EntityType NVARCHAR(50),
	@Map BIT
	AS
	BEGIN
		-- Add Mapping Information
		if(@map = 1)
		begin

			IF @EntityType='Product'
			begin
				IF NOT EXISTS (SELECT * FROM ProductStore WHERE StoreID = @StoreID AND ProductID = @EntityID)
				begin
					INSERT INTO ProductStore(StoreID, ProductID) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType in ('Category', 'Manufacturer', 'Section')
			begin
				IF NOT EXISTS (SELECT * FROM EntityStore WHERE StoreID = @StoreID AND EntityId = @EntityID and EntityType = @EntityType)
				begin
					INSERT INTO EntityStore(StoreID, EntityType, EntityId) VALUES (@StoreID, @EntityType, @EntityID)
				end
			end
			else IF @EntityType='ShippingMethod'
			begin
				IF NOT EXISTS (SELECT * FROM ShippingMethodStore WHERE StoreID = @StoreID AND ShippingMethodId = @EntityID)
				begin
					INSERT INTO ShippingMethodStore(StoreID, ShippingMethodId) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType = 'Topic'
			begin
				IF NOT EXISTS(SELECT * FROM TopicStore WHERE @StoreID = StoreID AND TopicID = @EntityID)
				begin
					INSERT INTO TopicStore (StoreID, TopicID) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType = 'News'
			begin
				IF NOT EXISTS (SELECT * FROM NewsStore WHERE StoreID = @StoreID AND NewsID = @EntityID)
				begin
					INSERT INTO NewsStore(StoreID, NewsID) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType='OrderOption'
			begin
				IF NOT EXISTS (SELECT * FROM OrderOptionStore WHERE StoreID = @StoreID AND OrderOptionID = @EntityID)
				begin
					INSERT INTO OrderOptionStore(StoreID, OrderOptionID) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType='GiftCard'
			begin
				IF NOT EXISTS (SELECT * FROM GiftCardStore WHERE StoreID = @StoreID AND GiftCardId = @EntityID)
				begin
					INSERT INTO GiftCardStore(StoreID, GiftCardId) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType='Affiliate'
			begin
				IF NOT EXISTS (SELECT * FROM AffiliateStore WHERE StoreID = @StoreID AND AffiliateID = @EntityID)
				begin
					INSERT INTO AffiliateStore(StoreID, AffiliateID) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType='Coupon'
			begin
				IF NOT EXISTS (SELECT * FROM CouponStore WHERE StoreID = @StoreID AND CouponID = @EntityID)
				begin
					INSERT INTO CouponStore(StoreID, CouponID) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType='Promotion'
			begin
				IF NOT EXISTS (SELECT * FROM PromotionStore WHERE StoreID = @StoreID AND PromotionId = @EntityID)
				begin
					INSERT INTO PromotionStore(StoreID, PromotionId) VALUES (@StoreID, @EntityID)
				end
			end
		end
		-- Remove Mapping Information if any
		else if (@map = 0)
		begin

			IF @EntityType='Product'
			begin
				DELETE FROM ProductStore WHERE ProductID = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType in ('Category', 'Manufacturer', 'Section')
			begin
				DELETE FROM EntityStore WHERE EntityId = @EntityID AND StoreID = @StoreID and EntityType = @EntityType
			end
			else IF @EntityType = 'ShippingMethod'
			begin
				DELETE FROM ShippingMethodStore WHERE ShippingMethodID = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType = 'Topic'
			begin
				DELETE FROM TopicStore WHERE TopicID = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType = 'News'
			begin
				DELETE FROM NewsStore WHERE NewsID = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType='OrderOption'
			begin
				DELETE FROM OrderOptionStore WHERE OrderOptionID = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType='GiftCard'
			begin
				DELETE FROM GiftCardStore WHERE GiftCardId = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType='Affiliate'
			begin
				DELETE FROM AffiliateStore WHERE AffiliateId = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType='Coupon'
			begin
				DELETE FROM CouponStore WHERE CouponID = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType='Promotion'
			begin
				DELETE FROM PromotionStore WHERE PromotionID = @EntityID AND StoreID = @StoreID
			end
		end
	END
GO

ALTER VIEW ObjectView
	AS
	SELECT	EM.EntityID AS ID,
			EM.EntityType AS EntityType,
			EM.[Name],
			EM.Description
	FROM EntityMaster AS EM WITH (NOLOCK)

	UNION ALL

	SELECT	tp.TopicID AS ID,
			'Topic' AS EntityType,
			tp.[Name],
			tp.Description
	FROM Topic AS tp WITH (NOLOCK)

	UNION ALL

	SELECT	nw.NewsID AS ID,
			'News' AS EntityType,
			nw.Headline AS [Name],
			'' AS Description
	FROM News AS nw WITH(NOLOCK)

	UNION ALL

	SELECT	p.ProductID AS ID,
			'Product' AS EntityType,
			p.[Name],
			p.Description
	FROM Product AS p WITH(NOLOCK)

	UNION ALL

	SELECT	cp.CouponID AS ID,
			'Coupon' AS EntityType,
			cp.[CouponCode] AS [Name],
			cp.Description
	FROM Coupon AS cp WITH(NOLOCK)

	UNION ALL

	SELECT	p.Id AS ID,
			'Promotion' AS EntityType,
			p.[Code] AS [Name],
			p.Description
	FROM Promotions AS p WITH(NOLOCK)

	UNION ALL

	SELECT	oo.OrderOptionID AS ID,
			'OrderOption' AS EntityType,
			oo.[Name],
			oo.Description
	FROM OrderOption oo WITH(NOLOCK)

	UNION ALL

	SELECT	gc.GiftCardID AS ID,
			'GiftCard' AS EntityType,
			gc.SerialNumber AS [Name],
			'' AS Description
	FROM GiftCard AS gc WITH(NOLOCK)

	UNION ALL

	SELECT	sm.ShippingMethodID AS ID,
			'ShippingMethod' AS EntityType,
			sm.[Name] AS [Name],
			'' AS Description
	FROM ShippingMethod AS sm WITH(NOLOCK)
GO

/************* End Promotions ****************/

/************* TFS 690: Mobile ********************/
IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[MobileLocaleMapping]')AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
    CREATE TABLE [dbo].[MobileLocaleMapping](
        [DesktopLocale] [nvarchar](50) NOT NULL,
        [MobileLocale] [nvarchar](50) NOT NULL,
		[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_MobileLocaleMapping_CreatedOn] DEFAULT (getdate())
    ) ON [PRIMARY]
END

If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.SkinId') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.SkinId' ,'For Developers Use Only - This value contains the skin id for your mobile skin. In standard mobile setups this AppConfig should be set to "2".','MOBILE','2','integer', null);
DECLARE @mobilelocale nvarchar(5)
SELECT @mobilelocale = ISNULL(configvalue, 'en-US') from appconfig where name = 'DefaultLocale' order by storeid desc
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.DefaultLocaleSetting') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.DefaultLocaleSetting' ,'The locale setting to use for the mobile platform.','MOBILE', @mobilelocale,'string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.IncludeEmailLinks') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.IncludeEmailLinks' ,'If this AppConfig is set to "true" the store will show a link to email the store administrator on every page. These emails will be sent to the email address listed in the built in AppConfig "GotOrderEMailFrom".','MOBILE','false','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.IsEnabled') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.IsEnabled' ,'Set this app config to false to turn off the mobile platform.','MOBILE','true','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.IncludePhoneLinks') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.IncludePhoneLinks' ,'If this is set to "true" the store will show a link to call your store''s phone number on every page. This phone number can be changed via the AppConfig "Mobile.ContactPhoneNumber".','MOBILE','false','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ContactPhoneNumber') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ContactPhoneNumber' ,'For Developers Use Only - This AppConfig allows the user to call directly from their mobile device. If this feature is enabled (via the "Mobile.IncludePhoneLinks" AppConfig) the call link will dial the phone number contained in this AppConfig.','MOBILE','1.800.555.1234','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.PageExceptions') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.PageExceptions' ,'For Developers Use Only - Pages listed in this appconfig will be excluded from the mobile platform.','MOBILE','','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.Entity.PageSize') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.Entity.PageSize' ,'This AppConfig sets the number of products to list on product listing pages.','MOBILE','3','integer', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.Entity.ImageWidth') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.Entity.ImageWidth' ,'For Developers Use Only - This AppConfig defines the width of product image widths on entity pages. Note that a large value in this field might break the display of this field. This value does not necessarily need to match the width of your product images; they will be resized with CSS.','MOBILE','80','integer', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ProductSlider.ImageWidth') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ProductSlider.ImageWidth' ,'For Developers Use Only - This AppConfig defines the widths of the images in the product slider. This value should space images appropriately on mobile devices when paired with the value in the "Mobile.ProductSlider.Width" AppConfig. The product slider is used for featured products, recently-viewed products, related products, and upsell products.','MOBILE','60','integer', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ProductSlider.MaxProducts') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ProductSlider.MaxProducts' ,'This AppConfig sets the maximum number of products to show in a product slider. The fewer products allowed in a product slider, the faster pages with product sliders will load. This number of products should be a evenly divisible by the value in ''Mobile.ProductSlider.Width ''.','MOBILE','15','integer', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ProductSlider.Width') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ProductSlider.Width' ,'For Developers Use Only - This AppConfig defines the number of products each pane in the product slider2 should display.','MOBILE','3','integer', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ShowAlternateCheckouts') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ShowAlternateCheckouts' ,'If true than altenative checkout methods such as paypal express will be shown on the shopping cart page.','MOBILE','true','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.DefaultXmlPackageEntity') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.DefaultXmlPackageEntity' ,'For Developers Use Only - This AppConfig defines the xml package that the Mobile Commerce Plug-in uses by default for entity pages. The default value for this AppConfig is "mobile.entity.default.xml.config". The associated xml package is included in the Mobile Skin.','MOBILE','mobile.entity.default.xml.config','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.DefaultXmlPackageProduct') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.DefaultXmlPackageProduct' ,'For Developers Use Only - This AppConfig defines the xml package that the Mobile Commerce Plug-in uses by default for product pages. The default value for this AppConfig is "mobile.entity.default.xml.config". The associated xml package is included in the Mobile Skin.','MOBILE','mobile.product.default.xml.config','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.UserAgentList') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.UserAgentList' ,'For Developers Use Only','MOBILE','up.browser, up.link, mmp, symbian, smartphone, midp, wap, phone, windows ce, pda, mobile, mini, palm, webos, android','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ShortUserAgentList') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ShortUserAgentList' ,'For Developers Use Only','MOBILE','w3c ,acs-,alav,alca,amoi,audi,avan,benq,bird,blac,blaz,brew,cell,cldc,cmd-,dang,doco,eric,hipt,inno,ipaq,java,jigs,kddi,keji,leno,lg-c,lg-d,lg-g,lge-,maui,maxo,midp,mits,mmef,mobi,mot-,moto,mwbp,nec-,newt,noki,palm,pana,pant,phil,play,port,prox,qwap,sage,sams,sany,sch-,sec-,send,seri,sgh-,shar,sie-,siem,smal,smar,sony,sph-,symb,t-mo,teli,tim-,tosh,tsm-,upg1,upsi,vk-v,voda,wap-,wapa,wapi,wapp,wapr,webc,winw,winw,xda,xda-','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.AllowAddressChangeOnCheckoutShipping') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.AllowAddressChangeOnCheckoutShipping' ,'Enables a dropdown box on the shipping selection page that allows the user to switch addresses.','MOBILE','true','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.AllowMultiShipOnCheckout') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.AllowMultiShipOnCheckout' ,'Not supported within the Mobile Commerce Plug-in - do not touch!','MOBILE','false','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ShowMobileOniPad') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ShowMobileOniPad' ,'If this is set to true the Mobile Platform will be showed to iPad users.','MOBILE','false','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ThemeId') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ThemeId' ,'The jQuery Mobile theme you would like to use. a, b, c, d, or e.','MOBILE','c','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.Action.ThemeId') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.Action.ThemeId' ,'The jQuery Mobile theme you would like to use for call-to-action buttons. a, b, c, d, or e.','MOBILE','e','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.Header.ThemeId') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.Header.ThemeId' ,'The jQuery Mobile theme you would like to use for the site header. a, b, c, d, or e.','MOBILE','b','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.PayPal.Express.ButtonImageURL') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.PayPal.Express.ButtonImageURL' ,'URL for Express Checkout Button Image used on Mobile','MOBILE','https://www.paypalobjects.com/en_US/i/btn/btn_xpressCheckout.gif','string', null);

IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Mobile.GlobalHeader') INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Mobile.GlobalHeader', 1, 0, 'Mobile.GlobalHeader', '');
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Mobile.GlobalFooter') INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Mobile.GlobalFooter', 1, 0, 'Mobile.GlobalFooter', '(!XmlPackage name="mobile.footer"!)');
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Mobile.9HomeTopIntro') INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Mobile.9HomeTopIntro', 1, 0, 'Mobile.9HomeTopIntro', '(!XmlPackage name="mobile.entitylist" entityname="section"!)<ul data-role="listview"><li class="group" data-role="list-divider"></li><li><a href="e-mobile.entitylist.aspx?entityname=category" class="fullwidth">Shop (!AppConfig name="StoreName"!)</a></li></ul>');
/************* END TFS 690: Mobile ****************/

/************* TFS 693: AvaTax Commits ****************/
if not exists (select * from AppConfig where Name = 'AvalaraTax.CommitTaxes') insert into AppConfig(Name, Description, GroupName, ConfigValue, ValueType, AllowableValues) values ('AvalaraTax.CommitTaxes', 'Set to true if AspDotNetStorefront should commit the tax document for orders. Set to false if order taxes are committed in an external system.', 'AVALARATAX', 'true', 'boolean', null);
if not exists (select * from AppConfig where Name = 'AvalaraTax.CommitRefunds') insert into AppConfig(Name, Description, GroupName, ConfigValue, ValueType, AllowableValues) values ('AvalaraTax.CommitRefunds', 'Set to true if AspDotNetStorefront should commit the tax document for refunds. Set to false if refund taxes are committed in an external system.', 'AVALARATAX', 'true', 'boolean', null);
/************* TFS 693: AvaTax Commits ****************/

/************* TFS 751: Gift Promotions ****************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[aspdnsf_AddItemToCart]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[aspdnsf_AddItemToCart]
GO

CREATE proc [dbo].[aspdnsf_AddItemToCart]
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
	DECLARE @RestrictedQy varchar(8000), @CurrentCartQty int, @InventoryFullSKU nvarchar(50), @InventoryWeightDelta money, @AllowEmptySkuAddToCart varchar(10), @ShoppingCartrecid int, @IsAKit tinyint, @IsAPack tinyint
	DECLARE @levelprice money, @LevelDiscountPercent money, @LevelDiscountsApplyToExtendedPrices tinyint, @CustEntersPrice tinyint

	SELECT @CustEntersPrice = CustomerEntersPrice, @RestrictedQy = RestrictedQuantities FROM dbo.ProductVariant with (nolock) WHERE VariantID = @VariantID

	SELECT	@IsAKit = IsAKit, @IsAPack = IsAPack FROM dbo.Product with (nolock) WHERE ProductID = @ProductID

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
	IF @CurrentCartQty is not null and @IsAKit = 0 and @IsAPack = 0 and @CustEntersPrice = 0  BEGIN
		UPDATE dbo.ShoppingCart
		SET Quantity = case @RQty when -1 then Quantity + @Quantity else isnull(@RQty, 0) end ,
			RequiresCount = RequiresCount + @RequiresCount
		WHERE ShippingAddressID = @ShippingAddressID and ProductID = @ProductID and VariantID = @VariantID and ChosenColor = @ChosenColor and ChosenSize = @ChosenSize and TextOption like @TextOption and CustomerID = @CustomerID and CartType = @CartType

		SET @NewShoppingCartRecID = 0
		RETURN
	END


	SELECT @AllowEmptySkuAddToCart = ConfigValue FROM dbo.AppConfig with (nolock) WHERE [name]= 'AllowEmptySkuAddToCart'


	--Insert item into ShoppingCart
	INSERT dbo.ShoppingCart(CartType,ShoppingCartRecGUID,CustomerID,ShippingAddressID,BillingAddressID,ProductID,VariantID,ProductSKU,ProductPrice,CustomerEntersPrice,ProductWeight,ProductDimensions,Quantity,RequiresCount,ChosenColor,ChosenColorSKUModifier,ChosenSize,ChosenSizeSKUModifier,TextOption,IsTaxable,IsShipSeparately,IsDownload,DownloadLocation,FreeShipping,DistributorID,RecurringInterval,RecurringIntervalType, IsSystem, IsAKit, IsAPack, TaxClassID, IsKit2, StoreID, IsGift)
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
			 when p.IsAPack = 1 and p.PackSize = 0 then dbo.PackPriceDelta(@CustomerID, @CustomerLevelID, @ProductID, 0)+@ColorAndSizePriceDelta
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
		p.IsAPack,
		p.TaxClassID,
		@IsKit2,
		@StoreID,
		@IsAGift
	FROM dbo.Product p with (NOLOCK)
		join dbo.ProductVariant pv with (NOLOCK) on p.productid = pv.productid
		left join dbo.Inventory i with (NOLOCK) on pv.VariantID = i.VariantID and i.size = @CleanSizeOption and i.color = @CleanColorOption
		left join dbo.ProductDistributor pd with (NOLOCK) on p.ProductID = pd.ProductID
	WHERE p.ProductID = @ProductID
		and pv.VariantID = @VariantID
		and (@AllowEmptySkuAddToCart = 'true' or rtrim(case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then isnull(p.sku, '') + isnull(pv.skusuffix, '') + isnull(@ChosenColorSKUModifier, '') + isnull(@ChosenSizeSKUModifier, '') else i.VendorFullSKU end) <> '')

	SET @ShoppingCartrecid = @@IDENTITY

	--Update KitCart Table if necessary
	IF (@IsAKit = 1 AND @IsKit2 = 0) BEGIN
		UPDATE KitCart SET ShoppingCartRecID = @ShoppingCartrecid WHERE ProductID = @ProductID and VariantID = @VariantID and ShoppingCartRecID = 0 and CustomerID = @CustomerID
	END

	SET @NewShoppingCartRecID = @ShoppingCartrecid
GO

if not exists (select * from AppConfig where Name = 'Debug.DisplayOrderSummaryDiagnostics')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)
		Values ('Debug.DisplayOrderSummaryDiagnostics' ,'Displays diagnostic subtotals in the order summary.','DEBUG', 'false','boolean', null);
GO

/************* TFS 751: Gift Promotions ****************/

/************* TFS 780                  ****************/
update [dbo].AppConfig set [Description] = '[DEPRECATED] if true, internationalcheckout.com button will be visible on your shopping cart page. You must sign up separately for an account from International Checkout if you want to use this feature.'
	where Name like 'InternationalCheckout.Enabled'

update [dbo].AppConfig set [Description] = '[DEPRECATED] if true and InternationalCheckout is enabled, enter your InternationalCheckout.com assigned storeid here (e.g. store123)'
	where Name like 'InternationalCheckout.StoreID'

update [dbo].AppConfig set [Description] = '[DEPRECATED] if true and InternationalCheckout is enabled, and the customer''s address is outside the U.S., their only checkout button will be the InternationalCheckout button. See AppConfig:InternationalCheckout.Enabled appconfig also.'
	where Name like 'InternationalCheckout.ForceForInternationalCustomers'

update [dbo].AppConfig set [Description] = '[DEPRECATED] if true and InternationalCheckout is enabled, you can set this to true to see the form that is submitted to InternationalCheckout before the submission. This can help during debug/development mode'
	where Name like 'InternationalCheckout.TestMode'
/************* TFS 780                  ****************/

/************* TFS 742: Promotions and Avalara ****************/
update AppConfig set Description = 'This AppConfig is no longer used.' where Name = 'AvalaraTax.TaxAddress'
/************* TFS 742: Promotions and Avalara ****************/

/************* TFS 821: Order Shipments ****************/
if not exists (select * From sysobjects where id = object_id('OrderShipment') and type = 'u')
	create table dbo.OrderShipment (
		OrderShipmentID int not null primary key identity(1, 1),
		OrderNumber int not null,
		AddressID int not null,
		ShippingTotal money not null
	)
/************* TFS 821: Order Shipments ****************/

if not exists (select * from AppConfig where Name = 'Debug.CouponMigrated')
	begin

	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)
		Values ('Debug.CouponMigrated' ,'Flag to determine if coupons have been migrated over to promotions.','DEBUG', 'true','boolean', null);

	print ('Migrating coupons...');
	declare @CouponId int
	declare @CouponGuid uniqueidentifier
	declare @CouponCode nvarchar(50)
	declare @CouponDescription nvarchar(max)
	declare @CouponStartDate datetime
	declare @CouponExpirationDate datetime
	declare @CouponExpireAfterUseByAnyCustomer int
	declare @CouponExpireAfterUseByEachCustomer int
	declare @CouponExpireAfterNUses int
	declare @CouponRequiresMinimumOrderAmount money
	declare @CouponValidForCustomers varchar(max)
	declare @CouponValidForProducts varchar(max)
	declare @CouponValidForManufacturers varchar(max)
	declare @CouponValidForCategories varchar(max)
	declare @CouponValidForSections varchar(max)
	declare @CouponDiscountPercent money
	declare @CouponDiscountAmount money
	declare @CouponType int
	declare @CouponDiscountIncludesFreeShipping bit
	declare @CouponActive tinyint

	declare CouponCursor cursor for
		select CouponId
			, CouponGuid
			, CouponCode
			, isnull(Description, '')
			, StartDate
			, ExpirationDate
			, DiscountPercent
			, DiscountAmount
			, CouponType
			, DiscountIncludesFreeShipping
			, ExpiresOnFirstUseByAnyCustomer
			, ExpiresAfterOneUsageByEachCustomer
			, ExpiresAfterNUses
			, isnull(RequiresMinimumOrderAmount, 0)
			, isnull(ValidForCustomers, '')
			, isnull(ValidForProducts, '')
			, isnull(ValidForManufacturers, '')
			, isnull(ValidForCategories, '')
			, isnull(ValidForSections, '')
			, case Deleted when 0 then 1 else 0 end
		from Coupon

	open CouponCursor

	fetch next from CouponCursor into
		@CouponId
		, @CouponGuid
		, @CouponCode
		, @CouponDescription
		, @CouponStartDate
		, @CouponExpirationDate
		, @CouponDiscountPercent
		, @CouponDiscountAmount
		, @CouponType
		, @CouponDiscountIncludesFreeShipping
		, @CouponExpireAfterUseByAnyCustomer
		, @CouponExpireAfterUseByEachCustomer
		, @CouponExpireAfterNUses
		, @CouponRequiresMinimumOrderAmount
		, @CouponValidForCustomers
		, @CouponValidForProducts
		, @CouponValidForManufacturers
		, @CouponValidForCategories
		, @CouponValidForSections
		, @CouponActive

	while (@@fetch_status <> -1)
	begin

		-- Promotions
		declare @CouponRuleData xml
		declare @CouponDiscountData xml
		declare @PromotionId int
		declare @PromotionDiscountBaseType nvarchar(50);

		Set @PromotionDiscountBaseType = Case When @CouponType = 0 Then 'OrderPromotionDiscount' Else 'OrderItemPromotionDiscount' End;

		-- Build expiration per use xml
		declare @RuleExpirationUseXml varchar(max)
		set @RuleExpirationUseXml = ''

		if (@CouponExpireAfterUseByEachCustomer = 1)
			begin
			set @RuleExpirationUseXml = '<PromotionRuleBase xsi:type="ExpirationNumberOfUsesPerCustomerPromotionRule"><NumberOfUsesAllowed>1</NumberOfUsesAllowed></PromotionRuleBase>'
			end
		else if (@CouponExpireAfterUseByEachCustomer = 0 and @CouponExpireAfterUseByAnyCustomer = 1)
			begin
			set @CouponExpireAfterNUses = 1
			set @RuleExpirationUseXml = '<PromotionRuleBase xsi:type="ExpirationNumberOfUsesPromotionRule"><NumberOfUsesAllowed>' + convert(varchar, @CouponExpireAfterNUses) + '</NumberOfUsesAllowed></PromotionRuleBase>'
			end
		else if (@CouponExpireAfterUseByEachCustomer = 0 and @CouponExpireAfterUseByAnyCustomer = 0 and @CouponExpireAfterNUses > 0)
			begin
			set @CouponExpireAfterNUses = @CouponExpireAfterNUses
			set @RuleExpirationUseXml = '<PromotionRuleBase xsi:type="ExpirationNumberOfUsesPromotionRule"><NumberOfUsesAllowed>' + convert(varchar, @CouponExpireAfterNUses) + '</NumberOfUsesAllowed></PromotionRuleBase>'
			end

		-- Build minimum cart amount rule xml
		declare @RuleMinimumCartAmountXml varchar(max)
		set @RuleMinimumCartAmountXml = ''

		if (@CouponRequiresMinimumOrderAmount > 0)
			begin
			set @RuleMinimumCartAmountXml = '<PromotionRuleBase xsi:type="MinimumCartAmountPromotionRule"><CartAmount>' + convert(varchar, @CouponRequiresMinimumOrderAmount) + '</CartAmount></PromotionRuleBase>'
			end

		-- Build email address rule xml
  		declare @RuleEmailAddressRequiredXml varchar(max)
		set @RuleEmailAddressRequiredXml = ''

		if (len(@CouponValidForCustomers) > 0)
			begin
			declare @CustomerEmailString varchar(max)
			set @CustomerEmailString = ''
			select @CustomerEmailString = @CustomerEmailString + '<string>' + email + '</string>' from Customer where CustomerId in (select * from dbo.Split(@CouponValidForCustomers, ','))
			set @RuleEmailAddressRequiredXml = '<PromotionRuleBase xsi:type="EmailAddressPromotionRule"><EmailAddresses>' + @CustomerEmailString + '</EmailAddresses></PromotionRuleBase>'
			end

		-- Build valid for product xml
		declare @RuleValidForProductXml varchar(max)
		set @RuleValidForProductXml = ''
		if (len(@CouponValidForProducts) > 0)
			begin
				declare @ProductIdString varchar(max)
				set @ProductIdString = ''
				select @ProductIdString = @ProductIdString + '<int>' + Items + '</int>' from dbo.Split(@CouponValidForProducts, ',')
				set @RuleValidForProductXml = '<PromotionRuleBase xsi:type="ProductIdPromotionRule"><ProductIds>' + @ProductIdString + '</ProductIds><RequireQuantity>false</RequireQuantity><Quantity>1</Quantity><AndTogether>false</AndTogether></PromotionRuleBase>'
			end

		-- Build valid for category xml
		declare @RuleValidForCategoryXml varchar(max)
		set @RuleValidForCategoryXml = ''
		if (len(@CouponValidForCategories) > 0)
			begin
				declare @CategoryIdString varchar(max)
				set @CategoryIdString = ''
				select @CategoryIdString = @CategoryIdString + '<int>' + Items + '</int>' from dbo.Split(@CouponValidForCategories, ',')
				set @RuleValidForCategoryXml = '<PromotionRuleBase xsi:type="CategoryPromotionRule"><CategoryIds>' + @CategoryIdString + '</CategoryIds></PromotionRuleBase>'
			end

		-- Build valid for Manufacturers xml
		declare @RuleValidForManufacturersXml varchar(max)
		set @RuleValidForManufacturersXml = ''
		if (len(@CouponValidForManufacturers) > 0)
			begin
				declare @ManufacturerIdString varchar(max)
				set @ManufacturerIdString = ''
				select @ManufacturerIdString = @ManufacturerIdString + '<int>' + Items + '</int>' from dbo.Split(@CouponValidForManufacturers, ',')
				set @RuleValidForManufacturersXml = '<PromotionRuleBase xsi:type="ManufacturerPromotionRule"><ManufacturerIds>' + @ManufacturerIdString + '</ManufacturerIds></PromotionRuleBase>'
			end

		-- Build valid for Manufacturers xml
		declare @RuleValidForSectionsXml varchar(max)
		set @RuleValidForSectionsXml = ''
		if (len(@CouponValidForSections) > 0)
			begin
				declare @SectionIdString varchar(max)
				set @SectionIdString = ''
				select @SectionIdString = @SectionIdString + '<int>' + Items + '</int>' from dbo.Split(@CouponValidForSections, ',')
				set @RuleValidForSectionsXml = '<PromotionRuleBase xsi:type="SectionPromotionRule"><SectionIds>' + @SectionIdString + '</SectionIds></PromotionRuleBase>'
			end

		-- Build rule data xml
		set @CouponRuleData = cast('<ArrayOfPromotionRuleBase xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
		  <PromotionRuleBase xsi:type="StartDatePromotionRule">
			<StartDate>' + convert(varchar, @CouponStartDate, 126) + '</StartDate>
		  </PromotionRuleBase>
		  <PromotionRuleBase xsi:type="ExpirationDatePromotionRule">
			<ExpirationDate>' + convert(varchar, @CouponExpirationDate, 126) + '</ExpirationDate>
		  </PromotionRuleBase>
		  ' + isnull(@RuleExpirationUseXml, '') + '
		  ' + isnull(@RuleMinimumCartAmountXml, '') + '
		  ' + isnull(@RuleEmailAddressRequiredXml, '') + '
		  ' + isnull(@RuleValidForProductXml, '') + '
		  ' + isnull(@RuleValidForCategoryXml, '') + '
		  ' + isnull(@RuleValidForManufacturersXml, '') + '
		  ' + isnull(@RuleValidForSectionsXml, '') + '
		</ArrayOfPromotionRuleBase>' as xml)

		declare @DiscountIncludeFreeShippingXml varchar(max)
		set @DiscountIncludeFreeShippingXml = ''
		if (@CouponDiscountIncludesFreeShipping > 0)
			begin
				set @DiscountIncludeFreeShippingXml = '<PromotionDiscountBase xsi:type="ShippingPromotionDiscount"><DiscountType>Percentage</DiscountType><DiscountAmount>1</DiscountAmount></PromotionDiscountBase>'
			end

		if @CouponDiscountPercent > 0
			set @CouponDiscountData = cast('<ArrayOfPromotionDiscountBase xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
			  <PromotionDiscountBase xsi:type="' + @PromotionDiscountBaseType + '">
				<DiscountType>Percentage</DiscountType>
				<DiscountAmount>' + convert(varchar, @CouponDiscountPercent/100) + '</DiscountAmount>
			  </PromotionDiscountBase>
			  ' + isnull(@DiscountIncludeFreeShippingXml, '') + '
			</ArrayOfPromotionDiscountBase>' as xml)
		else
			set @CouponDiscountData = cast('<ArrayOfPromotionDiscountBase xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
			  <PromotionDiscountBase xsi:type="' + @PromotionDiscountBaseType + '">
				<DiscountType>Fixed</DiscountType>
				<DiscountAmount>' + convert(varchar, @CouponDiscountAmount) + '</DiscountAmount>
			  </PromotionDiscountBase>
			  ' + isnull(@DiscountIncludeFreeShippingXml, '') + '
			</ArrayOfPromotionDiscountBase>' as xml)

		insert into Promotions (PromotionGuid, Name, [Description], UsageText, EmailText, Code, [Priority], Active, AutoAssigned, PromotionRuleData, PromotionDiscountData)
			values (@CouponGuid, @CouponCode, @CouponDescription, @CouponDescription, @CouponDescription, @CouponCode, 1, @CouponActive, 0, @CouponRuleData, @CouponDiscountData)
		set @PromotionId = @@identity

		-- Promotion Usage
		insert into PromotionUsage (PromotionId, CustomerId, OrderId, DateApplied, DiscountAmount, Complete)
			select @PromotionId, CustomerId, 0, CreatedOn, 0, 1 from CouponUsage where CouponCode = @CouponCode

		-- Promotion store
		insert into PromotionStore (PromotionId, StoreId, CreatedOn)
			select @PromotionId, StoreId, createdOn from CouponStore where CouponId = @CouponId

		-- Expire the old coupon record
		if (@CouponExpirationDate > getdate())
			update Coupon set ExpirationDate = getdate() where CouponId = @CouponId

		fetch next from CouponCursor into
			@CouponId
			, @CouponGuid
			, @CouponCode
			, @CouponDescription
			, @CouponStartDate
			, @CouponExpirationDate
			, @CouponDiscountPercent
			, @CouponDiscountAmount
			, @CouponType
			, @CouponDiscountIncludesFreeShipping
			, @CouponExpireAfterUseByAnyCustomer
			, @CouponExpireAfterUseByEachCustomer
			, @CouponExpireAfterNUses
			, @CouponRequiresMinimumOrderAmount
			, @CouponValidForCustomers
			, @CouponValidForProducts
			, @CouponValidForManufacturers
			, @CouponValidForCategories
			, @CouponValidForSections
			, @CouponActive
	end

	close CouponCursor
	deallocate CouponCursor
	end
GO

UPDATE dbo.[AppConfig] SET ConfigValue = 'PayPal' WHERE [Name] = 'PayFlowPro.PARTNER';
GO


---------------- 9.3.1.0 -------------------------------------
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_getAddressesByCustomer]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_getAddressesByCustomer]
GO

CREATE PROCEDURE [dbo].[aspdnsf_getAddressesByCustomer]
(
	@CustomerID int
)
AS
BEGIN
SET NOCOUNT ON
	SELECT	[AddressID],
			[CustomerID],
			[NickName],
			[FirstName],
			[LastName],
			[Company],
			[Address1],
			[Address2],
			[Suite],
			[City],
			[State],
			[Zip],
			[Country],
			[ResidenceType],
			[Phone],
			[Email]
	FROM [Address]
	WHERE [CustomerID] = @CustomerID
		AND [Deleted] = 0
		AND Address1 NOT LIKE '%Hidden By Amazon%'
END
GO

--New USPS Tracking URL--
UPDATE dbo.[AppConfig] SET ConfigValue = 'https://tools.usps.com/go/TrackConfirmAction_input?origTrackNum={0}' WHERE [Name] = 'ShippingTrackingURL.USPS' AND ConfigValue = 'http://trkcnfrm1.smi.usps.com/PTSInternetWeb/InterLabelInquiry.do?origTrackNum={0}';
GO

--Make topics publishable--
PRINT 'Updating Topic Table...'
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('Topic') AND name = 'Published')
    ALTER TABLE dbo.Topic ADD Published [bit] NOT NULL CONSTRAINT DF_Topic_Published DEFAULT((1))

/*********************Google Trusted Stores Integration ********************/
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreProductSearchID')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreProductSearchID', 'Account ID from Product Search. This value should match the account ID you use to submit your product data feed you submit to Google Product Search. (If you have an MCA account, use the subaccount ID associated with that product feed.)', '', 'string', null, 'Google Trusted Store', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreCountry')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreCountry', 'Account country from Product Search. This value should match the account country you use to submit your product data feed to Google Product Search.    The value of the country parameter should be a <a target="blank" style="text-decoration: underline;" href="http://www.iso.org/iso/country_codes/iso_3166_code_lists.htm">two-letter ISO 3166 country code</a>.', 'US', 'string', null, 'Google Trusted Store', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreLanguage')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreLanguage', 'Account language from Product Search. This value should match the account language you use to submit your product data feed to Google Product Search.    The value of the language parameter should be a <a target="blank" style="text-decoration: underline;" href="http://www.loc.gov/standards/iso639-2/php/English_list.php">two-letter ISO 639-1 language code</a>.', 'en', 'string', null, 'Google Trusted Store', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreID')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreID', 'This is your Google Trusted Store account ID, which can be gotten <a target="blank" style="text-decoration: underline;" href="http://www.google.com/trustedstores/sell/setupcode">here</a>.  Look for the code on this line:    gts.push(["id", "xxxxx"]);', '', 'string', null, 'Google Trusted Store', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreEnabled')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreEnabled', 'If true, the Google Trusted Store javascript will be added to your orderconfirmation.aspx page.', 'false', 'boolean', null, 'Google Trusted Store', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreShippingLeadTime')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreShippingLeadTime', 'Average estimated number of days before a new order ships (whole numbers only).  Be as accurate as possible without shorting this value - this factors into your store''s trust rating.', '', 'integer', null, 'Google Trusted Store', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreDeliveryLeadTime')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreDeliveryLeadTime', 'The estimated average number of days before an order is delivered (whole numbers only).  Be as accurate as possible without shorting this value - this factors into your store''s trust rating.', '', 'integer', null, 'GOOGLE TRUSTED STORE', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreBadgePosition')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreBadgePosition', 'Where to place the Google Trusted Store badge on your site - bottom left, or bottom right.', 'BOTTOM_RIGHT', 'enum', 'BOTTOM_RIGHT,BOTTOM_LEFT', 'GOOGLE TRUSTED STORE', 0, 0, getdate());
/*********************Google Trusted Stores Integration ********************/

/*********************UpdatedOn and CreatedOn Columns & DB Triggers************************/
--Add the UpdatedOn column to tables that don't have it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Address' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Address ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Address_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Affiliate' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Affiliate ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Affiliate_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateActivity' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE AffiliateActivity ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateActivity_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateCommissions' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE AffiliateCommissions ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateCommissions_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE AffiliateStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AppConfig' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE AppConfig ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_AppConfig_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'aspdnsf_SysLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE aspdnsf_SysLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_aspdnsf_SysLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AuditLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE AuditLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_AuditLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BadWord' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE BadWord ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_BadWord_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Category' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Category ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Category_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CIM_AddressPaymentProfileMap' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CIM_AddressPaymentProfileMap ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CIM_AddressPaymentProfileMap_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Country' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Country ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Country_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CountryTaxRate' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CountryTaxRate ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CountryTaxRate_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Coupon' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Coupon ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Coupon_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CouponStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CouponStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CouponStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CreditCardType' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CreditCardType ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CreditCardType_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Currency' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Currency ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Currency_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Customer' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Customer ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Customer_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerLevel' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CustomerLevel ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerLevel_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerSession' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CustomerSession ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerSession_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomReport' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CustomReport ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CustomReport_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Distributor' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Distributor ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Distributor_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Document' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Document ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Document_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentType' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE DocumentType ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentType_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EntityStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE EntityStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_EntityStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ErrorLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorMessage' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ErrorMessage ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorMessage_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EventHandler' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE EventHandler ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_EventHandler_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExtendedPrice' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ExtendedPrice ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ExtendedPrice_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FailedTransaction' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE FailedTransaction ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_FailedTransaction_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Feed' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Feed ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Feed_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Genre' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Genre ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Genre_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCard' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE GiftCard ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCard_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE GiftCardStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardUsage' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE GiftCardUsage ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardUsage_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GlobalConfig' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE GlobalConfig ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_GlobalConfig_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Inventory' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Inventory ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Inventory_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitCart' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE KitCart ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_KitCart_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroup' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE KitGroup ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroup_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroupType' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE KitGroupType ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroupType_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitItem' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE KitItem ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_KitItem_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Library' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Library ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Library_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LocaleSetting' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE LocaleSetting ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_LocaleSetting_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Manufacturer' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Manufacturer ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Manufacturer_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileDevice' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE MobileDevice ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_MobileDevice_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileLocaleMapping' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE MobileLocaleMapping ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_MobileLocaleMapping_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MultiShipOrder_Shipment' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE MultiShipOrder_Shipment ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_MultiShipOrder_Shipment_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'News' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE News ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_News_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NewsStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE NewsStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_NewsStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderNumbers' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE OrderNumbers ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_OrderNumbers_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOption' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE OrderOption ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOption_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOptionStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE OrderOptionStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOptionStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Orders ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_KitCart' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Orders_KitCart ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_KitCart_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Orders_ShoppingCart ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_ShoppingCart_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderShipment' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE OrderShipment ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_OrderShipment_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderTransaction' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE OrderTransaction ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_OrderTransaction_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PasswordLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PasswordLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PasswordLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Product ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Product_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductAffiliate' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductAffiliate ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductAffiliate_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCategory' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductCategory ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCategory_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCustomerLevel' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductCustomerLevel ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCustomerLevel_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductDistributor' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductDistributor ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductDistributor_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductGenre' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductGenre ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductGenre_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductLocaleSetting' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductLocaleSetting ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductLocaleSetting_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductManufacturer' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductManufacturer ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductManufacturer_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductSection' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductSection ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductSection_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductType' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductType ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductType_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVariant' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductVariant ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVariant_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVector' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductVector ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVector_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductView' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductView ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductView_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Profile' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Profile ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Profile_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionLineItem' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PromotionLineItem ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionLineItem_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Promotions' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Promotions ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Promotions_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PromotionStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionUsage' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PromotionUsage ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionUsage_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscount' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE QuantityDiscount ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscount_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscountTable' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE QuantityDiscountTable ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscountTable_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rating' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Rating ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Rating_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RatingCommentHelpfulness' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE RatingCommentHelpfulness ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_RatingCommentHelpfulness_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RestrictedIP' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE RestrictedIP ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_RestrictedIP_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SalesPrompt' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE SalesPrompt ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_SalesPrompt_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SearchLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE SearchLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_SearchLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Section' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Section ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Section_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SecurityLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE SecurityLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_SecurityLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByProduct' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingByProduct ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByProduct_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotal' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingByTotal ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotal_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotalByPercent' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingByTotalByPercent ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotalByPercent_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByWeight' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingByWeight ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByWeight_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculation' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingCalculation ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculation_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculationStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingCalculationStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculationStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingImportExport' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingImportExport ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingImportExport_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethod' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingMethod ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethod_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingMethodStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToCountryMap' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingMethodToCountryMap ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToCountryMap_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToStateMap' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingMethodToStateMap ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToStateMap_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToZoneMap' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingMethodToZoneMap ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToZoneMap_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingTotalByZone' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingTotalByZone ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingTotalByZone_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingWeightByZone' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingWeightByZone ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingWeightByZone_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingZone' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingZone ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingZone_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShoppingCart' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShoppingCart ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShoppingCart_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'State' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE State ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_State_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StateTaxRate' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE StateTaxRate ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_StateTaxRate_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Store' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Store ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Store_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StringResource' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE StringResource ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_StringResource_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TaxClass' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE TaxClass ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_TaxClass_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Topic' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Topic ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Topic_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicMapping' AND COLUMN_NAME = 'UpdatedOn')
    IF (OBJECT_ID('[dbo].[TopicMapping]', 'u') IS NOT NULL)
    EXEC('
	ALTER TABLE TopicMapping ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_TopicMapping_UpdatedOn DEFAULT(getdate())')
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE TopicStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_TopicStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vector' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Vector ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Vector_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ZipTaxRate' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ZipTaxRate ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ZipTaxRate_UpdatedOn DEFAULT(getdate())
	GO
--Add the CreatedOn column to tables that don't have it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Address' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Address ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Address_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Affiliate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Affiliate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Affiliate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateActivity' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateActivity ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateActivity_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateCommissions' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateCommissions ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateCommissions_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AppConfig' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AppConfig ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AppConfig_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'aspdnsf_SysLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE aspdnsf_SysLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_aspdnsf_SysLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AuditLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AuditLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AuditLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BadWord' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE BadWord ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_BadWord_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Category' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Category ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Category_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CIM_AddressPaymentProfileMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CIM_AddressPaymentProfileMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CIM_AddressPaymentProfileMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Country' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Country ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Country_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CountryTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CountryTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CountryTaxRate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Coupon' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Coupon ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Coupon_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CouponStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CouponStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CouponStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CreditCardType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CreditCardType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CreditCardType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Currency' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Currency ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Currency_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Customer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Customer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Customer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerLevel' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomerLevel ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerLevel_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerSession' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomerSession ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerSession_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomReport' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomReport ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomReport_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Distributor' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Distributor ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Distributor_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Document' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Document ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Document_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE DocumentType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EntityStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE EntityStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_EntityStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ErrorLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorMessage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ErrorMessage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorMessage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EventHandler' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE EventHandler ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_EventHandler_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExtendedPrice' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ExtendedPrice ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ExtendedPrice_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FailedTransaction' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE FailedTransaction ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_FailedTransaction_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Feed' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Feed ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Feed_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Genre' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Genre ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Genre_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCard' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCard ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCard_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCardStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardUsage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCardUsage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardUsage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GlobalConfig' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GlobalConfig ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GlobalConfig_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Inventory' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Inventory ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Inventory_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroup' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitGroup ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroup_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroupType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitGroupType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroupType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitItem' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitItem ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitItem_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Library' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Library ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Library_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LocaleSetting' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LocaleSetting ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LocaleSetting_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Manufacturer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Manufacturer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Manufacturer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileDevice' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MobileDevice ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MobileDevice_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileLocaleMapping' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MobileLocaleMapping ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MobileLocaleMapping_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MultiShipOrder_Shipment' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MultiShipOrder_Shipment ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MultiShipOrder_Shipment_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'News' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE News ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_News_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NewsStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE NewsStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_NewsStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderNumbers' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderNumbers ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderNumbers_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOption' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderOption ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOption_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOptionStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderOptionStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOptionStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_KitCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders_KitCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_KitCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders_ShoppingCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_ShoppingCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderShipment' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderShipment ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderShipment_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderTransaction' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderTransaction ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderTransaction_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PasswordLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PasswordLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PasswordLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Product ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Product_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductAffiliate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductAffiliate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductAffiliate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCategory' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductCategory ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCategory_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCustomerLevel' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductCustomerLevel ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCustomerLevel_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductDistributor' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductDistributor ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductDistributor_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductGenre' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductGenre ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductGenre_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductLocaleSetting' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductLocaleSetting ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductLocaleSetting_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductManufacturer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductManufacturer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductManufacturer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductSection' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductSection ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductSection_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVariant' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductVariant ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVariant_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVector' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductVector ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVector_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductView' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductView ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductView_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Profile' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Profile ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Profile_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionLineItem' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionLineItem ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionLineItem_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Promotions' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Promotions ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Promotions_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionUsage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionUsage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionUsage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscount' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE QuantityDiscount ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscount_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscountTable' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE QuantityDiscountTable ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscountTable_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rating' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Rating ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Rating_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RatingCommentHelpfulness' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE RatingCommentHelpfulness ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_RatingCommentHelpfulness_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RestrictedIP' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE RestrictedIP ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_RestrictedIP_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SalesPrompt' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SalesPrompt ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SalesPrompt_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SearchLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SearchLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SearchLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Section' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Section ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Section_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SecurityLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SecurityLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SecurityLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByProduct' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByProduct ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByProduct_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotal' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByTotal ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotal_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotalByPercent' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByTotalByPercent ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotalByPercent_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByWeight' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByWeight ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByWeight_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculation' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingCalculation ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculation_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculationStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingCalculationStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculationStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingImportExport' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingImportExport ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingImportExport_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethod' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethod ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethod_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToCountryMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToCountryMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToCountryMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToStateMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToStateMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToStateMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToZoneMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToZoneMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToZoneMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingTotalByZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingTotalByZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingTotalByZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingWeightByZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingWeightByZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingWeightByZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShoppingCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShoppingCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShoppingCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'State' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE State ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_State_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StateTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE StateTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_StateTaxRate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Store' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Store ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Store_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StringResource' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE StringResource ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_StringResource_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TaxClass' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE TaxClass ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TaxClass_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Topic' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Topic ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Topic_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicMapping' AND COLUMN_NAME = 'CreatedOn')
    IF (OBJECT_ID('[dbo].[TopicMapping]', 'u') IS NOT NULL)
    EXEC('ALTER TABLE TopicMapping ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TopicMapping_CreatedOn DEFAULT(getdate())')
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE TopicStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TopicStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vector' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Vector ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Vector_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ZipTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ZipTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ZipTaxRate_CreatedOn DEFAULT(getdate())
	GO

--Add the CreatedOn column to tables that don't have it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Address' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Address ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Address_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Affiliate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Affiliate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Affiliate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateActivity' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateActivity ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateActivity_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateCommissions' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateCommissions ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateCommissions_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AppConfig' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AppConfig ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AppConfig_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'aspdnsf_SysLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE aspdnsf_SysLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_aspdnsf_SysLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AuditLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AuditLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AuditLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BadWord' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE BadWord ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_BadWord_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Category' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Category ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Category_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CIM_AddressPaymentProfileMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CIM_AddressPaymentProfileMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CIM_AddressPaymentProfileMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Country' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Country ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Country_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CountryTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CountryTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CountryTaxRate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Coupon' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Coupon ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Coupon_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CouponStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CouponStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CouponStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CreditCardType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CreditCardType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CreditCardType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Currency' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Currency ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Currency_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Customer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Customer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Customer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerLevel' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomerLevel ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerLevel_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerSession' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomerSession ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerSession_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomReport' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomReport ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomReport_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Distributor' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Distributor ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Distributor_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Document' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Document ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Document_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE DocumentType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EntityStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE EntityStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_EntityStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ErrorLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorMessage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ErrorMessage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorMessage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EventHandler' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE EventHandler ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_EventHandler_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExtendedPrice' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ExtendedPrice ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ExtendedPrice_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FailedTransaction' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE FailedTransaction ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_FailedTransaction_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Feed' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Feed ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Feed_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Genre' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Genre ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Genre_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCard' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCard ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCard_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCardStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardUsage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCardUsage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardUsage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GlobalConfig' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GlobalConfig ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GlobalConfig_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Inventory' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Inventory ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Inventory_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroup' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitGroup ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroup_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroupType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitGroupType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroupType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitItem' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitItem ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitItem_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Library' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Library ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Library_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LocaleSetting' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LocaleSetting ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LocaleSetting_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Manufacturer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Manufacturer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Manufacturer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileDevice' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MobileDevice ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MobileDevice_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileLocaleMapping' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MobileLocaleMapping ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MobileLocaleMapping_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MultiShipOrder_Shipment' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MultiShipOrder_Shipment ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MultiShipOrder_Shipment_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'News' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE News ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_News_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NewsStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE NewsStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_NewsStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderNumbers' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderNumbers ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderNumbers_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOption' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderOption ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOption_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOptionStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderOptionStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOptionStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_KitCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders_KitCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_KitCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders_ShoppingCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_ShoppingCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderShipment' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderShipment ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderShipment_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderTransaction' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderTransaction ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderTransaction_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PasswordLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PasswordLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PasswordLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Product ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Product_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductAffiliate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductAffiliate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductAffiliate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCategory' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductCategory ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCategory_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCustomerLevel' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductCustomerLevel ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCustomerLevel_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductDistributor' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductDistributor ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductDistributor_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductGenre' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductGenre ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductGenre_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductLocaleSetting' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductLocaleSetting ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductLocaleSetting_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductManufacturer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductManufacturer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductManufacturer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductSection' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductSection ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductSection_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVariant' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductVariant ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVariant_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVector' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductVector ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVector_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductView' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductView ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductView_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Profile' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Profile ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Profile_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionLineItem' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionLineItem ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionLineItem_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Promotions' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Promotions ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Promotions_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionUsage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionUsage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionUsage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscount' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE QuantityDiscount ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscount_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscountTable' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE QuantityDiscountTable ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscountTable_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rating' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Rating ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Rating_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RatingCommentHelpfulness' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE RatingCommentHelpfulness ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_RatingCommentHelpfulness_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RestrictedIP' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE RestrictedIP ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_RestrictedIP_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SalesPrompt' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SalesPrompt ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SalesPrompt_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SearchLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SearchLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SearchLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Section' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Section ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Section_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SecurityLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SecurityLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SecurityLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByProduct' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByProduct ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByProduct_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotal' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByTotal ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotal_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotalByPercent' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByTotalByPercent ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotalByPercent_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByWeight' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByWeight ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByWeight_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculation' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingCalculation ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculation_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculationStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingCalculationStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculationStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingImportExport' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingImportExport ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingImportExport_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethod' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethod ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethod_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToCountryMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToCountryMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToCountryMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToStateMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToStateMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToStateMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToZoneMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToZoneMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToZoneMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingTotalByZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingTotalByZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingTotalByZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingWeightByZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingWeightByZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingWeightByZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShoppingCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShoppingCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShoppingCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'State' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE State ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_State_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StateTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE StateTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_StateTaxRate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Store' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Store ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Store_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StringResource' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE StringResource ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_StringResource_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TaxClass' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE TaxClass ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TaxClass_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Topic' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Topic ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Topic_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicMapping' AND COLUMN_NAME = 'CreatedOn')
    IF (OBJECT_ID('[dbo].[TopicMapping]', 'u') IS NOT NULL)
	   EXEC('ALTER TABLE TopicMapping ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TopicMapping_CreatedOn DEFAULT(getdate())')
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE TopicStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TopicStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vector' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Vector ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Vector_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ZipTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ZipTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ZipTaxRate_CreatedOn DEFAULT(getdate())
	GO

--Add UpdatedOn triggers to all tables
--Add the UpdatedOn triggers
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Address_Updated'))
	DROP TRIGGER [dbo].[Address_Updated]
GO

CREATE TRIGGER [dbo].[Address_Updated]
	ON [Address]
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Address_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			[Address]
		SET
			[Address].UpdatedOn = current_timestamp
		FROM
			[Address] [a]
			   INNER JOIN INSERTED i
			   ON [a].AddressID = i.AddressID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Affiliate_Updated'))
	DROP TRIGGER [dbo].[Affiliate_Updated]
GO

CREATE TRIGGER [dbo].[Affiliate_Updated]
	ON Affiliate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Affiliate_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Affiliate
		SET
			Affiliate.UpdatedOn = current_timestamp
		FROM
			Affiliate [a]
			   INNER JOIN INSERTED i
			   ON [a].AffiliateID = i.AffiliateID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'AffiliateActivity_Updated'))
	DROP TRIGGER [dbo].[AffiliateActivity_Updated]
GO

CREATE TRIGGER [dbo].[AffiliateActivity_Updated]
	ON AffiliateActivity
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('AffiliateActivity_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			AffiliateActivity
		SET
			AffiliateActivity.UpdatedOn = current_timestamp
		FROM
			AffiliateActivity [a]
			   INNER JOIN INSERTED i
			   ON [a].AffiliateActivityID = i.AffiliateActivityID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'AffiliateCommissions_Updated'))
	DROP TRIGGER [dbo].[AffiliateCommissions_Updated]
GO

CREATE TRIGGER [dbo].[AffiliateCommissions_Updated]
	ON AffiliateCommissions
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('AffiliateCommissions_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			AffiliateCommissions
		SET
			AffiliateCommissions.UpdatedOn = current_timestamp
		FROM
			AffiliateCommissions [a]
			   INNER JOIN INSERTED i
			   ON [a].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'AffiliateStore_Updated'))
	DROP TRIGGER [dbo].[AffiliateStore_Updated]
GO

CREATE TRIGGER [dbo].[AffiliateStore_Updated]
	ON AffiliateStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('AffiliateStore_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			AffiliateStore
		SET
			AffiliateStore.UpdatedOn = current_timestamp
		FROM
			AffiliateStore [a]
			   INNER JOIN INSERTED i
			   ON [a].ID = i.ID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'AppConfig_Updated'))
	DROP TRIGGER [dbo].[AppConfig_Updated]
GO

CREATE TRIGGER [dbo].[AppConfig_Updated]
	ON AppConfig
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('AppConfig_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			AppConfig
		SET
			AppConfig.UpdatedOn = current_timestamp
		FROM
			AppConfig [a]
			   INNER JOIN INSERTED i
			   ON [a].AppConfigID = i.AppConfigID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'aspdnsf_Syslog_Updated'))
	DROP TRIGGER [dbo].[aspdnsf_Syslog_Updated]
GO

CREATE TRIGGER [dbo].[aspdnsf_Syslog_Updated]
	ON aspdnsf_Syslog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('aspdnsf_Syslog_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			aspdnsf_Syslog
		SET
			aspdnsf_Syslog.UpdatedOn = current_timestamp
		FROM
			aspdnsf_Syslog [a]
			   INNER JOIN INSERTED i
			   ON [a].SyslogID = i.SyslogID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'AuditLog_Updated'))
	DROP TRIGGER [dbo].[AuditLog_Updated]
GO

CREATE TRIGGER [dbo].[AuditLog_Updated]
	ON AuditLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('AuditLog_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			AuditLog
		SET
			AuditLog.UpdatedOn = current_timestamp
		FROM
			AuditLog [a]
			   INNER JOIN INSERTED i
			   ON [a].AuditLogID = i.AuditLogID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'BadWord_Updated'))
	DROP TRIGGER [dbo].[BadWord_Updated]
GO

CREATE TRIGGER [dbo].[BadWord_Updated]
	ON BadWord
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('BadWord_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			BadWord
		SET
			BadWord.UpdatedOn = current_timestamp
		FROM
			BadWord [a]
			   INNER JOIN INSERTED i
			   ON [a].BadWordID = i.BadWordID
			   AND [a].LocaleSetting = i.LocaleSetting
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Category_Updated'))
	DROP TRIGGER [dbo].[Category_Updated]
GO

CREATE TRIGGER [dbo].[Category_Updated]
	ON Category
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Category_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Category
		SET
			Category.UpdatedOn = current_timestamp
		FROM
			Category [c]
			   INNER JOIN INSERTED i
			   ON [c].CategoryID = i.CategoryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CIM_AddressPaymentProfileMap_Updated'))
	DROP TRIGGER [dbo].[CIM_AddressPaymentProfileMap_Updated]
GO

CREATE TRIGGER [dbo].[CIM_AddressPaymentProfileMap_Updated]
	ON CIM_AddressPaymentProfileMap
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('CIM_AddressPaymentProfileMap_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			CIM_AddressPaymentProfileMap
		SET
			CIM_AddressPaymentProfileMap.UpdatedOn = current_timestamp
		FROM
			CIM_AddressPaymentProfileMap [c]
			   INNER JOIN INSERTED i
			   ON [c].CIMID = i.CIMID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Country_Updated'))
	DROP TRIGGER [dbo].[Country_Updated]
GO

CREATE TRIGGER [dbo].[Country_Updated]
	ON Country
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Country_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Country
		SET
			Country.UpdatedOn = current_timestamp
		FROM
			Country [c]
			   INNER JOIN INSERTED i
			   ON [c].CountryID = i.CountryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CountryTaxRate_Updated'))
	DROP TRIGGER [dbo].[CountryTaxRate_Updated]
GO

CREATE TRIGGER [dbo].[CountryTaxRate_Updated]
	ON CountryTaxRate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('CountryTaxRate_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			CountryTaxRate
		SET
			CountryTaxRate.UpdatedOn = current_timestamp
		FROM
			CountryTaxRate [c]
			   INNER JOIN INSERTED i
			   ON [c].CountryTaxID = i.CountryTaxID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Coupon_Updated'))
	DROP TRIGGER [dbo].[Coupon_Updated]
GO

CREATE TRIGGER [dbo].[Coupon_Updated]
	ON Coupon
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Coupon_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Coupon
		SET
			Coupon.UpdatedOn = current_timestamp
		FROM
			Coupon [c]
			   INNER JOIN INSERTED i
			   ON [c].CouponID = i.CouponID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CouponStore_Updated'))
	DROP TRIGGER [dbo].[CouponStore_Updated]
GO

CREATE TRIGGER [dbo].[CouponStore_Updated]
	ON CouponStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('CouponStore_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			CouponStore
		SET
			CouponStore.UpdatedOn = current_timestamp
		FROM
			CouponStore [c]
			   INNER JOIN INSERTED i
			   ON [c].CouponID = i.CouponID
			   AND [c].StoreId = i.StoreId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CreditCardType_Updated'))
	DROP TRIGGER [dbo].[CreditCardType_Updated]
GO

CREATE TRIGGER [dbo].[CreditCardType_Updated]
	ON CreditCardType
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('CreditCardType_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			CreditCardType
		SET
			CreditCardType.UpdatedOn = current_timestamp
		FROM
			CreditCardType [c]
			   INNER JOIN INSERTED i
			   ON [c].CardTypeID = i.CardTypeID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Currency_Updated'))
	DROP TRIGGER [dbo].[Currency_Updated]
GO

CREATE TRIGGER [dbo].[Currency_Updated]
	ON Currency
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Currency_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Currency
		SET
			Currency.UpdatedOn = current_timestamp
		FROM
			Currency [c]
			   INNER JOIN INSERTED i
			   ON [c].CurrencyID = i.CurrencyID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Customer_Updated'))
	DROP TRIGGER [dbo].[Customer_Updated]
GO

CREATE TRIGGER [dbo].[Customer_Updated]
	ON Customer
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Customer_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Customer
		SET
			Customer.UpdatedOn = current_timestamp
		FROM
			Customer [c]
			   INNER JOIN INSERTED i
			   ON [c].CustomerID = i.CustomerID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CustomerLevel_Updated'))
	DROP TRIGGER [dbo].[CustomerLevel_Updated]
GO

CREATE TRIGGER [dbo].[CustomerLevel_Updated]
	ON CustomerLevel
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('CustomerLevel_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			CustomerLevel
		SET
			CustomerLevel.UpdatedOn = current_timestamp
		FROM
			CustomerLevel [cl]
			   INNER JOIN INSERTED i
			   ON [cl].CustomerLevelID = i.CustomerLevelID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CustomerSession_Updated'))
	DROP TRIGGER [dbo].[CustomerSession_Updated]
GO

CREATE TRIGGER [dbo].[CustomerSession_Updated]
	ON CustomerSession
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('CustomerSession_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			CustomerSession
		SET
			CustomerSession.UpdatedOn = current_timestamp
		FROM
			CustomerSession [cl]
			   INNER JOIN INSERTED i
			   ON [cl].CustomerSessionID = i.CustomerSessionID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CustomReport_Updated'))
	DROP TRIGGER [dbo].[CustomReport_Updated]
GO

CREATE TRIGGER [dbo].[CustomReport_Updated]
	ON CustomReport
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('CustomReport_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			CustomReport
		SET
			CustomReport.UpdatedOn = current_timestamp
		FROM
			CustomReport [cl]
			   INNER JOIN INSERTED i
			   ON [cl].CustomReportID = i.CustomReportID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Document_Updated'))
	DROP TRIGGER [dbo].[Document_Updated]
GO

CREATE TRIGGER [dbo].[Document_Updated]
	ON Document
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Document_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Document
		SET
			Document.UpdatedOn = current_timestamp
		FROM
			Document [d]
			   INNER JOIN INSERTED i
			   ON [d].DocumentID = i.DocumentID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'DocumentType_Updated'))
	DROP TRIGGER [dbo].[DocumentType_Updated]
GO

CREATE TRIGGER [dbo].[DocumentType_Updated]
	ON DocumentType
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('DocumentType_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			DocumentType
		SET
			DocumentType.UpdatedOn = current_timestamp
		FROM
			DocumentType [d]
			   INNER JOIN INSERTED i
			   ON [d].DocumentTypeID = i.DocumentTypeID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'EntityStore_Updated'))
	DROP TRIGGER [dbo].[EntityStore_Updated]
GO

CREATE TRIGGER [dbo].[EntityStore_Updated]
	ON EntityStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('EntityStore_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			EntityStore
		SET
			EntityStore.UpdatedOn = current_timestamp
		FROM
			EntityStore [es]
			   INNER JOIN INSERTED i
			   ON [es].EntityID = i.EntityID
			   AND [es].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ErrorLog_Updated'))
	DROP TRIGGER [dbo].[ErrorLog_Updated]
GO

CREATE TRIGGER [dbo].[ErrorLog_Updated]
	ON ErrorLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ErrorLog_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ErrorLog
		SET
			ErrorLog.UpdatedOn = current_timestamp
		FROM
			ErrorLog [e]
			   INNER JOIN INSERTED i
			   ON [e].logid = i.logid
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ErrorMessage_Updated'))
	DROP TRIGGER [dbo].[ErrorMessage_Updated]
GO

CREATE TRIGGER [dbo].[ErrorMessage_Updated]
	ON ErrorMessage
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ErrorMessage_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ErrorMessage
		SET
			ErrorMessage.UpdatedOn = current_timestamp
		FROM
			ErrorMessage [e]
			   INNER JOIN INSERTED i
			   ON [e].MessageId = i.MessageId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'EventHandler_Updated'))
	DROP TRIGGER [dbo].[EventHandler_Updated]
GO

CREATE TRIGGER [dbo].[EventHandler_Updated]
	ON EventHandler
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('EventHandler_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			EventHandler
		SET
			EventHandler.UpdatedOn = current_timestamp
		FROM
			EventHandler [e]
			   INNER JOIN INSERTED i
			   ON [e].EventId = i.EventId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ExtendedPrice_Updated'))
	DROP TRIGGER [dbo].[ExtendedPrice_Updated]
GO

CREATE TRIGGER [dbo].[ExtendedPrice_Updated]
	ON ExtendedPrice
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ExtendedPrice_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ExtendedPrice
		SET
			ExtendedPrice.UpdatedOn = current_timestamp
		FROM
			ExtendedPrice [e]
			   INNER JOIN INSERTED i
			   ON [e].ExtendedPriceId = i.ExtendedPriceId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'FailedTransaction_Updated'))
	DROP TRIGGER [dbo].[FailedTransaction_Updated]
GO

CREATE TRIGGER [dbo].[FailedTransaction_Updated]
	ON FailedTransaction
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('FailedTransaction_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			FailedTransaction
		SET
			FailedTransaction.UpdatedOn = current_timestamp
		FROM
			FailedTransaction [f]
			   INNER JOIN INSERTED i
			   ON [f].DBRecNo = i.DBRecNo
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Feed_Updated'))
	DROP TRIGGER [dbo].[Feed_Updated]
GO

CREATE TRIGGER [dbo].[Feed_Updated]
	ON Feed
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Feed_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Feed
		SET
			Feed.UpdatedOn = current_timestamp
		FROM
			Feed [f]
			   INNER JOIN INSERTED i
			   ON [f].FeedID = i.FeedID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Genre_Updated'))
	DROP TRIGGER [dbo].[Genre_Updated]
GO

CREATE TRIGGER [dbo].[Genre_Updated]
	ON Genre
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Genre_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Genre
		SET
			Genre.UpdatedOn = current_timestamp
		FROM
			Genre [g]
			   INNER JOIN INSERTED i
			   ON [g].GenreID = i.GenreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'GiftCard_Updated'))
	DROP TRIGGER [dbo].[GiftCard_Updated]
GO

CREATE TRIGGER [dbo].[GiftCard_Updated]
	ON GiftCard
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('GiftCard_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			GiftCard
		SET
			GiftCard.UpdatedOn = current_timestamp
		FROM
			GiftCard [g]
			   INNER JOIN INSERTED i
			   ON [g].GiftCardID = i.GiftCardID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'GiftCardStore_Updated'))
	DROP TRIGGER [dbo].[GiftCardStore_Updated]
GO

CREATE TRIGGER [dbo].[GiftCardStore_Updated]
	ON GiftCardStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('GiftCardStore_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			GiftCardStore
		SET
			GiftCardStore.UpdatedOn = current_timestamp
		FROM
			GiftCardStore [g]
			   INNER JOIN INSERTED i
			   ON [g].GiftCardID = i.GiftCardID
			   AND [g].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'GiftCardUsage_Updated'))
	DROP TRIGGER [dbo].[GiftCardUsage_Updated]
GO

CREATE TRIGGER [dbo].[GiftCardUsage_Updated]
	ON GiftCardUsage
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('GiftCardUsage_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			GiftCardUsage
		SET
			GiftCardUsage.UpdatedOn = current_timestamp
		FROM
			GiftCardUsage [g]
			   INNER JOIN INSERTED i
			   ON [g].GiftCardUsageID = i.GiftCardUsageID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'GlobalConfig_Updated'))
	DROP TRIGGER [dbo].[GlobalConfig_Updated]
GO

CREATE TRIGGER [dbo].[GlobalConfig_Updated]
	ON GlobalConfig
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('GlobalConfig_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			GlobalConfig
		SET
			GlobalConfig.UpdatedOn = current_timestamp
		FROM
			GlobalConfig [g]
			   INNER JOIN INSERTED i
			   ON [g].GlobalConfigID = i.GlobalConfigID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Inventory_Updated'))
	DROP TRIGGER [dbo].[Inventory_Updated]
GO

CREATE TRIGGER [dbo].[Inventory_Updated]
	ON Inventory
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Inventory_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Inventory
		SET
			Inventory.UpdatedOn = current_timestamp
		FROM
			Inventory [inv]
			   INNER JOIN INSERTED i
			   ON [inv].InventoryID = i.InventoryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'KitCart_Updated'))
	DROP TRIGGER [dbo].[KitCart_Updated]
GO

CREATE TRIGGER [dbo].[KitCart_Updated]
	ON KitCart
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('KitCart_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			KitCart
		SET
			KitCart.UpdatedOn = current_timestamp
		FROM
			KitCart [k]
			   INNER JOIN INSERTED i
			   ON [k].KitCartRecID = i.KitCartRecID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'KitGroup_Updated'))
	DROP TRIGGER [dbo].[KitGroup_Updated]
GO

CREATE TRIGGER [dbo].[KitGroup_Updated]
	ON KitGroup
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('KitGroup_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			KitGroup
		SET
			KitGroup.UpdatedOn = current_timestamp
		FROM
			KitGroup [k]
			   INNER JOIN INSERTED i
			   ON [k].KitGroupID = i.KitGroupID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'KitGroupType_Updated'))
	DROP TRIGGER [dbo].[KitGroupType_Updated]
GO

CREATE TRIGGER [dbo].[KitGroupType_Updated]
	ON KitGroupType
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('KitGroupType_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			KitGroupType
		SET
			KitGroupType.UpdatedOn = current_timestamp
		FROM
			KitGroupType [k]
			   INNER JOIN INSERTED i
			   ON [k].KitGroupTypeID = i.KitGroupTypeID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'KitItem_Updated'))
	DROP TRIGGER [dbo].[KitItem_Updated]
GO

CREATE TRIGGER [dbo].[KitItem_Updated]
	ON KitItem
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('KitItem_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			KitItem
		SET
			KitItem.UpdatedOn = current_timestamp
		FROM
			KitItem [k]
			   INNER JOIN INSERTED i
			   ON [k].KitItemID = i.KitItemID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Library_Updated'))
	DROP TRIGGER [dbo].[Library_Updated]
GO

CREATE TRIGGER [dbo].[Library_Updated]
	ON Library
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Library_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Library
		SET
			Library.UpdatedOn = current_timestamp
		FROM
			Library [l]
			   INNER JOIN INSERTED i
			   ON [l].LibraryID = i.LibraryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'LocaleSetting_Updated'))
	DROP TRIGGER [dbo].[LocaleSetting_Updated]
GO

CREATE TRIGGER [dbo].[LocaleSetting_Updated]
	ON LocaleSetting
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('LocaleSetting_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			LocaleSetting
		SET
			LocaleSetting.UpdatedOn = current_timestamp
		FROM
			LocaleSetting [l]
			   INNER JOIN INSERTED i
			   ON [l].LocaleSettingID = i.LocaleSettingID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Manufacturer_Updated'))
	DROP TRIGGER [dbo].[Manufacturer_Updated]
GO

CREATE TRIGGER [dbo].[Manufacturer_Updated]
	ON Manufacturer
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Manufacturer_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Manufacturer
		SET
			Manufacturer.UpdatedOn = current_timestamp
		FROM
			Manufacturer [m]
			   INNER JOIN INSERTED i
			   ON [m].ManufacturerID = i.ManufacturerID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'MobileDevice_Updated'))
	DROP TRIGGER [dbo].[MobileDevice_Updated]
GO

CREATE TRIGGER [dbo].[MobileDevice_Updated]
	ON MobileDevice
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('MobileDevice_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			MobileDevice
		SET
			MobileDevice.UpdatedOn = current_timestamp
		FROM
			MobileDevice [m]
			   INNER JOIN INSERTED i
			   ON [m].MobileDeviceID = i.MobileDeviceID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'MobileLocaleMapping_Updated'))
	DROP TRIGGER [dbo].[MobileLocaleMapping_Updated]
GO

CREATE TRIGGER [dbo].[MobileLocaleMapping_Updated]
	ON MobileLocaleMapping
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('MobileLocaleMapping_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			MobileLocaleMapping
		SET
			MobileLocaleMapping.UpdatedOn = current_timestamp
		FROM
			MobileLocaleMapping [m]
			   INNER JOIN INSERTED i
			   ON [m].DesktopLocale = i.DesktopLocale
			   AND [m].MobileLocale = i.MobileLocale
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'MultiShipOrder_Shipment_Updated'))
	DROP TRIGGER [dbo].[MultiShipOrder_Shipment_Updated]
GO

CREATE TRIGGER [dbo].[MultiShipOrder_Shipment_Updated]
	ON MultiShipOrder_Shipment
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('MultiShipOrder_Shipment_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			MultiShipOrder_Shipment
		SET
			MultiShipOrder_Shipment.UpdatedOn = current_timestamp
		FROM
			MultiShipOrder_Shipment [m]
			   INNER JOIN INSERTED i
			   ON [m].MultiShipOrder_ShipmentGUID = i.MultiShipOrder_ShipmentGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'News_Updated'))
	DROP TRIGGER [dbo].[News_Updated]
GO

CREATE TRIGGER [dbo].[News_Updated]
	ON News
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('News_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			News
		SET
			News.UpdatedOn = current_timestamp
		FROM
			News [n]
			   INNER JOIN INSERTED i
			   ON [n].NewsID = i.NewsID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'NewsStore_Updated'))
	DROP TRIGGER [dbo].[NewsStore_Updated]
GO

CREATE TRIGGER [dbo].[NewsStore_Updated]
	ON NewsStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('NewsStore_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			NewsStore
		SET
			NewsStore.UpdatedOn = current_timestamp
		FROM
			NewsStore [n]
			   INNER JOIN INSERTED i
			   ON [n].NewsID = i.NewsID
			   AND [n].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderNumber_Updated'))
	DROP TRIGGER [dbo].[OrderNumber_Updated]
GO

CREATE TRIGGER [dbo].[OrderNumber_Updated]
	ON OrderNumbers
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('OrderNumber_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			OrderNumbers
		SET
			OrderNumbers.UpdatedOn = current_timestamp
		FROM
			OrderNumbers [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderNumber = i.OrderNumber
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderOption_Updated'))
	DROP TRIGGER [dbo].[OrderOption_Updated]
GO

CREATE TRIGGER [dbo].[OrderOption_Updated]
	ON OrderOption
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('OrderOption_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			OrderOption
		SET
			OrderOption.UpdatedOn = current_timestamp
		FROM
			OrderOption [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderOptionID = i.OrderOptionID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderOptionStore_Updated'))
	DROP TRIGGER [dbo].[OrderOptionStore_Updated]
GO

CREATE TRIGGER [dbo].[OrderOptionStore_Updated]
	ON OrderOptionStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('OrderOptionStore_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			OrderOptionStore
		SET
			OrderOptionStore.UpdatedOn = current_timestamp
		FROM
			OrderOptionStore [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderOptionID = i.OrderOptionID
			   AND [o].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Orders_Updated'))
	DROP TRIGGER [dbo].[Orders_Updated]
GO

CREATE TRIGGER [dbo].[Orders_Updated]
	ON Orders
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Orders_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Orders
		SET
			Orders.UpdatedOn = current_timestamp
		FROM
			Orders [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderNumber = i.OrderNumber
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Orders_KitCart_Updated'))
	DROP TRIGGER [dbo].[Orders_KitCart_Updated]
GO

CREATE TRIGGER [dbo].[Orders_KitCart_Updated]
	ON Orders_KitCart
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Orders_KitCart_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Orders_KitCart
		SET
			Orders_KitCart.UpdatedOn = current_timestamp
		FROM
			Orders_KitCart [o]
			   INNER JOIN INSERTED i
			   ON [o].KitCartRecID = i.KitCartRecID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Orders_ShoppingCart_Updated'))
	DROP TRIGGER [dbo].[Orders_ShoppingCart_Updated]
GO

CREATE TRIGGER [dbo].[Orders_ShoppingCart_Updated]
	ON Orders_ShoppingCart
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Orders_ShoppingCart_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Orders_ShoppingCart
		SET
			Orders_ShoppingCart.UpdatedOn = current_timestamp
		FROM
			Orders_ShoppingCart [o]
			   INNER JOIN INSERTED i
			   ON [o].ShoppingCartRecID = i.ShoppingCartRecID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderShipment_Updated'))
	DROP TRIGGER [dbo].[OrderShipment_Updated]
GO

CREATE TRIGGER [dbo].[OrderShipment_Updated]
	ON OrderShipment
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('OrderShipment_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			OrderShipment
		SET
			OrderShipment.UpdatedOn = current_timestamp
		FROM
			OrderShipment [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderShipmentID = i.OrderShipmentID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderTransaction_Updated'))
	DROP TRIGGER [dbo].[OrderTransaction_Updated]
GO

CREATE TRIGGER [dbo].[OrderTransaction_Updated]
	ON OrderTransaction
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('OrderTransaction_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			OrderTransaction
		SET
			OrderTransaction.UpdatedOn = current_timestamp
		FROM
			OrderTransaction [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderTransactionID = i.OrderTransactionID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PasswordLog_Updated'))
	DROP TRIGGER [dbo].[PasswordLog_Updated]
GO

CREATE TRIGGER [dbo].[PasswordLog_Updated]
	ON PasswordLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('PasswordLog_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			PasswordLog
		SET
			PasswordLog.UpdatedOn = current_timestamp
		FROM
			PasswordLog [p]
			   INNER JOIN INSERTED i
			   ON [p].CustomerID = i.CustomerID
			   AND [p].ChangeDt = i.ChangeDt
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Product_Updated'))
	DROP TRIGGER [dbo].[Product_Updated]
GO

CREATE TRIGGER [dbo].[Product_Updated]
	ON Product
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Product_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Product
		SET
			Product.UpdatedOn = current_timestamp
		FROM
			Product [p]
			   INNER JOIN INSERTED i
			   ON [p].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductAffiliate_Updated'))
	DROP TRIGGER [dbo].[ProductAffiliate_Updated]
GO

CREATE TRIGGER [dbo].[ProductAffiliate_Updated]
	ON ProductAffiliate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductAffiliate_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductAffiliate
		SET
			ProductAffiliate.UpdatedOn = current_timestamp
		FROM
			ProductAffiliate [p]
			   INNER JOIN INSERTED i
			   ON [p].ProductID = i.ProductID
			   AND [p].AffiliateID = i.AffiliateID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductVariant_Updated'))
	DROP TRIGGER [dbo].[ProductVariant_Updated]
GO

CREATE TRIGGER [dbo].[ProductVariant_Updated]
	ON ProductVariant
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductVariant_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductVariant
		SET
			ProductVariant.UpdatedOn = current_timestamp
		FROM
			ProductVariant [pv]
			   INNER JOIN INSERTED i
			   ON [pv].VariantID = i.VariantID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductCategory_Updated'))
	DROP TRIGGER [dbo].[ProductCategory_Updated]
GO

CREATE TRIGGER [dbo].[ProductCategory_Updated]
	ON ProductCategory
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductCategory_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductCategory
		SET
			ProductCategory.UpdatedOn = current_timestamp
		FROM
			ProductCategory [pc]
			   INNER JOIN INSERTED i
			   ON [pc].CategoryID = i.CategoryID
			   AND [pc].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductCustomerLevel_Updated'))
	DROP TRIGGER [dbo].[ProductCustomerLevel_Updated]
GO

CREATE TRIGGER [dbo].[ProductCustomerLevel_Updated]
	ON ProductCustomerLevel
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductCustomerLevel_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductCustomerLevel
		SET
			ProductCustomerLevel.UpdatedOn = current_timestamp
		FROM
			ProductCustomerLevel [pcl]
			   INNER JOIN INSERTED i
			   ON [pcl].CustomerLevelID = i.CustomerLevelID
			   AND [pcl].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductDistributor_Updated'))
	DROP TRIGGER [dbo].[ProductDistributor_Updated]
GO

CREATE TRIGGER [dbo].[ProductDistributor_Updated]
	ON ProductDistributor
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductDistributor_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductDistributor
		SET
			ProductDistributor.UpdatedOn = current_timestamp
		FROM
			ProductDistributor [pd]
			   INNER JOIN INSERTED i
			   ON [pd].DistributorID = i.DistributorID
			   AND [pd].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductGenre_Updated'))
	DROP TRIGGER [dbo].[ProductGenre_Updated]
GO

CREATE TRIGGER [dbo].[ProductGenre_Updated]
	ON ProductGenre
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductGenre_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductGenre
		SET
			ProductGenre.UpdatedOn = current_timestamp
		FROM
			ProductGenre [pg]
			   INNER JOIN INSERTED i
			   ON [pg].GenreID = i.GenreID
			   AND [pg].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductLocaleSetting_Updated'))
	DROP TRIGGER [dbo].[ProductLocaleSetting_Updated]
GO

CREATE TRIGGER [dbo].[ProductLocaleSetting_Updated]
	ON ProductLocaleSetting
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductLocaleSetting_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductLocaleSetting
		SET
			ProductLocaleSetting.UpdatedOn = current_timestamp
		FROM
			ProductLocaleSetting [pl]
			   INNER JOIN INSERTED i
			   ON [pl].LocaleSettingID = i.LocaleSettingID
			   AND [pl].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductManufacturer_Updated'))
	DROP TRIGGER [dbo].[ProductManufacturer_Updated]
GO

CREATE TRIGGER [dbo].[ProductManufacturer_Updated]
	ON ProductManufacturer
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductManufacturer_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductManufacturer
		SET
			ProductManufacturer.UpdatedOn = current_timestamp
		FROM
			ProductManufacturer [pm]
			   INNER JOIN INSERTED i
			   ON [pm].ManufacturerID = i.ManufacturerID
			   AND [pm].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductSection_Updated'))
	DROP TRIGGER [dbo].[ProductSection_Updated]
GO

CREATE TRIGGER [dbo].[ProductSection_Updated]
	ON ProductSection
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductSection_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductSection
		SET
			ProductSection.UpdatedOn = current_timestamp
		FROM
			ProductSection [ps]
			   INNER JOIN INSERTED i
			   ON [ps].SectionID = i.SectionID
			   AND [ps].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductStore_Updated'))
	DROP TRIGGER [dbo].[ProductStore_Updated]
GO

CREATE TRIGGER [dbo].[ProductStore_Updated]
	ON ProductStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductStore_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductStore
		SET
			ProductStore.UpdatedOn = current_timestamp
		FROM
			ProductStore [ps]
			   INNER JOIN INSERTED i
			   ON [ps].ID = i.ID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductType_Updated'))
	DROP TRIGGER [dbo].[ProductType_Updated]
GO

CREATE TRIGGER [dbo].[ProductType_Updated]
	ON ProductType
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductType_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductType
		SET
			ProductType.UpdatedOn = current_timestamp
		FROM
			ProductType [p]
			   INNER JOIN INSERTED i
			   ON [p].ProductTypeID = i.ProductTypeID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductVariant_Updated'))
	DROP TRIGGER [dbo].[ProductVariant_Updated]
GO

CREATE TRIGGER [dbo].[ProductVariant_Updated]
	ON ProductVariant
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductVariant_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductVariant
		SET
			ProductVariant.UpdatedOn = current_timestamp
		FROM
			ProductVariant [p]
			   INNER JOIN INSERTED i
			   ON [p].VariantID = i.VariantID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductVector_Updated'))
	DROP TRIGGER [dbo].[ProductVector_Updated]
GO

CREATE TRIGGER [dbo].[ProductVector_Updated]
	ON ProductVector
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductVector_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductVector
		SET
			ProductVector.UpdatedOn = current_timestamp
		FROM
			ProductVector [pv]
			   INNER JOIN INSERTED i
			   ON [pv].VectorID = i.VectorID
			   AND [pv].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductView_Updated'))
	DROP TRIGGER [dbo].[ProductView_Updated]
GO

CREATE TRIGGER [dbo].[ProductView_Updated]
	ON ProductView
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ProductView_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ProductView
		SET
			ProductView.UpdatedOn = current_timestamp
		FROM
			ProductView [p]
			   INNER JOIN INSERTED i
			   ON [p].ViewID = i.ViewID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PromotionLineItem_Updated'))
	DROP TRIGGER [dbo].[PromotionLineItem_Updated]
GO

CREATE TRIGGER [dbo].[PromotionLineItem_Updated]
	ON PromotionLineItem
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('PromotionLineItem_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			PromotionLineItem
		SET
			PromotionLineItem.UpdatedOn = current_timestamp
		FROM
			PromotionLineItem [p]
			   INNER JOIN INSERTED i
			   ON [p].id = i.id
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Promotions_Updated'))
	DROP TRIGGER [dbo].[Promotions_Updated]
GO

CREATE TRIGGER [dbo].[Promotions_Updated]
	ON Promotions
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Promotions_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Promotions
		SET
			Promotions.UpdatedOn = current_timestamp
		FROM
			Promotions [p]
			   INNER JOIN INSERTED i
			   ON [p].Id = i.Id
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PromotionStore_Updated'))
	DROP TRIGGER [dbo].[PromotionStore_Updated]
GO

CREATE TRIGGER [dbo].[PromotionStore_Updated]
	ON PromotionStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('PromotionStore_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			PromotionStore
		SET
			PromotionStore.UpdatedOn = current_timestamp
		FROM
			PromotionStore [p]
			   INNER JOIN INSERTED i
			   ON [p].ID = i.ID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PromotionUsage_Updated'))
	DROP TRIGGER [dbo].[PromotionUsage_Updated]
GO

CREATE TRIGGER [dbo].[PromotionUsage_Updated]
	ON PromotionUsage
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('PromotionUsage_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			PromotionUsage
		SET
			PromotionUsage.UpdatedOn = current_timestamp
		FROM
			PromotionUsage [p]
			   INNER JOIN INSERTED i
			   ON [p].Id = i.Id
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'QuantityDiscount_Updated'))
	DROP TRIGGER [dbo].[QuantityDiscount_Updated]
GO

CREATE TRIGGER [dbo].[QuantityDiscount_Updated]
	ON QuantityDiscount
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('QuantityDiscount_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			QuantityDiscount
		SET
			QuantityDiscount.UpdatedOn = current_timestamp
		FROM
			QuantityDiscount [q]
			   INNER JOIN INSERTED i
			   ON [q].QuantityDiscountID = i.QuantityDiscountID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'QuantityDiscountTable_Updated'))
	DROP TRIGGER [dbo].[QuantityDiscountTable_Updated]
GO

CREATE TRIGGER [dbo].[QuantityDiscountTable_Updated]
	ON QuantityDiscountTable
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('QuantityDiscountTable_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			QuantityDiscountTable
		SET
			QuantityDiscountTable.UpdatedOn = current_timestamp
		FROM
			QuantityDiscountTable [q]
			   INNER JOIN INSERTED i
			   ON [q].QuantityDiscountTableID = i.QuantityDiscountTableID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Rating_Updated'))
	DROP TRIGGER [dbo].[Rating_Updated]
GO

CREATE TRIGGER [dbo].[Rating_Updated]
	ON Rating
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Rating_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Rating
		SET
			Rating.UpdatedOn = current_timestamp
		FROM
			Rating [r]
			   INNER JOIN INSERTED i
			   ON [r].RatingID = i.RatingID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'RatingCommentHelpfulness_Updated'))
	DROP TRIGGER [dbo].[RatingCommentHelpfulness_Updated]
GO

CREATE TRIGGER [dbo].[RatingCommentHelpfulness_Updated]
	ON RatingCommentHelpfulness
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('RatingCommentHelpfulness_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			RatingCommentHelpfulness
		SET
			RatingCommentHelpfulness.UpdatedOn = current_timestamp
		FROM
			RatingCommentHelpfulness [r]
			   INNER JOIN INSERTED i
			   ON [r].StoreID = i.StoreID
			   AND [r].ProductID = i.ProductID
			   AND [r].RatingCustomerID = i.RatingCustomerID
			   AND [r].VotingCustomerID = i.VotingCustomerID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'RestrictedIP_Updated'))
	DROP TRIGGER [dbo].[RestrictedIP_Updated]
GO

CREATE TRIGGER [dbo].[RestrictedIP_Updated]
	ON RestrictedIP
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('RestrictedIP_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			RestrictedIP
		SET
			RestrictedIP.UpdatedOn = current_timestamp
		FROM
			RestrictedIP [r]
			   INNER JOIN INSERTED i
			   ON [r].DBRecNo = i.DBRecNo
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'SalesPrompt_Updated'))
	DROP TRIGGER [dbo].[SalesPrompt_Updated]
GO

CREATE TRIGGER [dbo].[SalesPrompt_Updated]
	ON SalesPrompt
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('SalesPrompt_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			SalesPrompt
		SET
			SalesPrompt.UpdatedOn = current_timestamp
		FROM
			SalesPrompt [s]
			   INNER JOIN INSERTED i
			   ON [s].SalesPromptID = i.SalesPromptID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'SearchLog_Updated'))
	DROP TRIGGER [dbo].[SearchLog_Updated]
GO

CREATE TRIGGER [dbo].[SearchLog_Updated]
	ON SearchLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('SearchLog_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			SearchLog
		SET
			SearchLog.UpdatedOn = current_timestamp
		FROM
			SearchLog [s]
			   INNER JOIN INSERTED i
			   ON [s].SearchTerm = i.SearchTerm
			   AND [s].CustomerID = i.CustomerID
			   AND [s].CreatedOn = i.CreatedOn
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Section_Updated'))
	DROP TRIGGER [dbo].[Section_Updated]
GO

CREATE TRIGGER [dbo].[Section_Updated]
	ON Section
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Section_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Section
		SET
			Section.UpdatedOn = current_timestamp
		FROM
			Section [s]
			   INNER JOIN INSERTED i
			   ON [s].SectionID = i.SectionID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'SecurityLog_Updated'))
	DROP TRIGGER [dbo].[SecurityLog_Updated]
GO

CREATE TRIGGER [dbo].[SecurityLog_Updated]
	ON SecurityLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('SecurityLog_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			SecurityLog
		SET
			SecurityLog.UpdatedOn = current_timestamp
		FROM
			SecurityLog [s]
			   INNER JOIN INSERTED i
			   ON [s].logid = i.logid
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingByProduct_Updated'))
	DROP TRIGGER [dbo].[ShippingByProduct_Updated]
GO

CREATE TRIGGER [dbo].[ShippingByProduct_Updated]
	ON ShippingByProduct
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingByProduct_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingByProduct
		SET
			ShippingByProduct.UpdatedOn = current_timestamp
		FROM
			ShippingByProduct [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingByProductID = i.ShippingByProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingByTotal_Updated'))
	DROP TRIGGER [dbo].[ShippingByTotal_Updated]
GO

CREATE TRIGGER [dbo].[ShippingByTotal_Updated]
	ON ShippingByTotal
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingByTotal_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingByTotal
		SET
			ShippingByTotal.UpdatedOn = current_timestamp
		FROM
			ShippingByTotal [s]
			   INNER JOIN INSERTED i
			   ON [s].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingByTotalByPercent_Updated'))
	DROP TRIGGER [dbo].[ShippingByTotalByPercent_Updated]
GO

CREATE TRIGGER [dbo].[ShippingByTotalByPercent_Updated]
	ON ShippingByTotalByPercent
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingByTotalByPercent_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingByTotalByPercent
		SET
			ShippingByTotalByPercent.UpdatedOn = current_timestamp
		FROM
			ShippingByTotalByPercent [s]
			   INNER JOIN INSERTED i
			   ON [s].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingByWeight_Updated'))
	DROP TRIGGER [dbo].[ShippingByWeight_Updated]
GO

CREATE TRIGGER [dbo].[ShippingByWeight_Updated]
	ON ShippingByWeight
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingByWeight_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingByWeight
		SET
			ShippingByWeight.UpdatedOn = current_timestamp
		FROM
			ShippingByWeight [s]
			   INNER JOIN INSERTED i
			   ON [s].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingCalculation_Updated'))
	DROP TRIGGER [dbo].[ShippingCalculation_Updated]
GO

CREATE TRIGGER [dbo].[ShippingCalculation_Updated]
	ON ShippingCalculation
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingCalculation_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingCalculation
		SET
			ShippingCalculation.UpdatedOn = current_timestamp
		FROM
			ShippingCalculation [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingCalculationID = i.ShippingCalculationID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingCalculationStore_Updated'))
	DROP TRIGGER [dbo].[ShippingCalculationStore_Updated]
GO

CREATE TRIGGER [dbo].[ShippingCalculationStore_Updated]
	ON ShippingCalculationStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingCalculationStore_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingCalculationStore
		SET
			ShippingCalculationStore.UpdatedOn = current_timestamp
		FROM
			ShippingCalculationStore [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingCalculationID = i.ShippingCalculationID
			   AND [s].StoreId = i.StoreId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingImportExport_Updated'))
	DROP TRIGGER [dbo].[ShippingImportExport_Updated]
GO

CREATE TRIGGER [dbo].[ShippingImportExport_Updated]
	ON ShippingImportExport
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingImportExport_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingImportExport
		SET
			ShippingImportExport.UpdatedOn = current_timestamp
		FROM
			ShippingImportExport [s]
			   INNER JOIN INSERTED i
			   ON [s].OrderNumber = i.OrderNumber
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethod_Updated'))
	DROP TRIGGER [dbo].[ShippingMethod_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethod_Updated]
	ON ShippingMethod
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingMethod_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingMethod
		SET
			ShippingMethod.UpdatedOn = current_timestamp
		FROM
			ShippingMethod [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethodStore_Updated'))
	DROP TRIGGER [dbo].[ShippingMethodStore_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethodStore_Updated]
	ON ShippingMethodStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingMethodStore_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingMethodStore
		SET
			ShippingMethodStore.UpdatedOn = current_timestamp
		FROM
			ShippingMethodStore [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
			   AND [s].StoreId = i.StoreId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethodToCountryMap_Updated'))
	DROP TRIGGER [dbo].[ShippingMethodToCountryMap_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethodToCountryMap_Updated]
	ON ShippingMethodToCountryMap
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingMethodToCountryMap_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingMethodToCountryMap
		SET
			ShippingMethodToCountryMap.UpdatedOn = current_timestamp
		FROM
			ShippingMethodToCountryMap [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
			   AND [s].CountryID = i.CountryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethodToStateMap_Updated'))
	DROP TRIGGER [dbo].[ShippingMethodToStateMap_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethodToStateMap_Updated]
	ON ShippingMethodToStateMap
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingMethodToStateMap_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingMethodToStateMap
		SET
			ShippingMethodToStateMap.UpdatedOn = current_timestamp
		FROM
			ShippingMethodToStateMap [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
			   AND [s].StateID = i.StateID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethodToZoneMap_Updated'))
	DROP TRIGGER [dbo].[ShippingMethodToZoneMap_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethodToZoneMap_Updated]
	ON ShippingMethodToZoneMap
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingMethodToZoneMap_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingMethodToZoneMap
		SET
			ShippingMethodToZoneMap.UpdatedOn = current_timestamp
		FROM
			ShippingMethodToZoneMap [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
			   AND [s].ShippingZoneID = i.ShippingZoneID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethodToZoneMap_Updated'))
	DROP TRIGGER [dbo].[ShippingMethodToZoneMap_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethodToZoneMap_Updated]
	ON ShippingMethodToZoneMap
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingMethodToZoneMap_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingMethodToZoneMap
		SET
			ShippingMethodToZoneMap.UpdatedOn = current_timestamp
		FROM
			ShippingMethodToZoneMap [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
			   AND [s].ShippingZoneID = i.ShippingZoneID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingTotalByZone_Updated'))
	DROP TRIGGER [dbo].[ShippingTotalByZone_Updated]
GO

CREATE TRIGGER [dbo].[ShippingTotalByZone_Updated]
	ON ShippingTotalByZone
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingTotalByZone_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingTotalByZone
		SET
			ShippingTotalByZone.UpdatedOn = current_timestamp
		FROM
			ShippingTotalByZone [s]
			   INNER JOIN INSERTED i
			   ON [s].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingWeightByZone_Updated'))
	DROP TRIGGER [dbo].[ShippingWeightByZone_Updated]
GO

CREATE TRIGGER [dbo].[ShippingWeightByZone_Updated]
	ON ShippingWeightByZone
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingWeightByZone_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingWeightByZone
		SET
			ShippingWeightByZone.UpdatedOn = current_timestamp
		FROM
			ShippingWeightByZone [s]
			   INNER JOIN INSERTED i
			   ON [s].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingZone_Updated'))
	DROP TRIGGER [dbo].[ShippingZone_Updated]
GO

CREATE TRIGGER [dbo].[ShippingZone_Updated]
	ON ShippingZone
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShippingZone_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShippingZone
		SET
			ShippingZone.UpdatedOn = current_timestamp
		FROM
			ShippingZone [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingZoneID = i.ShippingZoneID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShoppingCart_Updated'))
	DROP TRIGGER [dbo].[ShoppingCart_Updated]
GO

CREATE TRIGGER [dbo].[ShoppingCart_Updated]
	ON ShoppingCart
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ShoppingCart_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ShoppingCart
		SET
			ShoppingCart.UpdatedOn = current_timestamp
		FROM
			ShoppingCart [s]
			   INNER JOIN INSERTED i
			   ON [s].ShoppingCartRecID = i.ShoppingCartRecID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'State_Updated'))
	DROP TRIGGER [dbo].[State_Updated]
GO

CREATE TRIGGER [dbo].[State_Updated]
	ON [State]
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('State_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			[State]
		SET
			[State].UpdatedOn = current_timestamp
		FROM
			[State] [s]
			   INNER JOIN INSERTED i
			   ON [s].StateID = i.StateID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'StateTaxRate_Updated'))
	DROP TRIGGER [dbo].[StateTaxRate_Updated]
GO

CREATE TRIGGER [dbo].[StateTaxRate_Updated]
	ON StateTaxRate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('StateTaxRate_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			StateTaxRate
		SET
			StateTaxRate.UpdatedOn = current_timestamp
		FROM
			StateTaxRate [s]
			   INNER JOIN INSERTED i
			   ON [s].StateTaxID = i.StateTaxID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'StateTaxRate_Updated'))
	DROP TRIGGER [dbo].[StateTaxRate_Updated]
GO

CREATE TRIGGER [dbo].[StateTaxRate_Updated]
	ON StateTaxRate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('StateTaxRate_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			StateTaxRate
		SET
			StateTaxRate.UpdatedOn = current_timestamp
		FROM
			StateTaxRate [s]
			   INNER JOIN INSERTED i
			   ON [s].StateTaxID = i.StateTaxID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Store_Updated'))
	DROP TRIGGER [dbo].[Store_Updated]
GO

CREATE TRIGGER [dbo].[Store_Updated]
	ON Store
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Store_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Store
		SET
			Store.UpdatedOn = current_timestamp
		FROM
			Store [s]
			   INNER JOIN INSERTED i
			   ON [s].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'StringResource_Updated'))
	DROP TRIGGER [dbo].[StringResource_Updated]
GO

CREATE TRIGGER [dbo].[StringResource_Updated]
	ON StringResource
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('StringResource_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			StringResource
		SET
			StringResource.UpdatedOn = current_timestamp
		FROM
			StringResource [s]
			   INNER JOIN INSERTED i
			   ON [s].StringResourceID = i.StringResourceID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'TaxClass_Updated'))
	DROP TRIGGER [dbo].[TaxClass_Updated]
GO

CREATE TRIGGER [dbo].[TaxClass_Updated]
	ON TaxClass
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('TaxClass_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			TaxClass
		SET
			TaxClass.UpdatedOn = current_timestamp
		FROM
			TaxClass [s]
			   INNER JOIN INSERTED i
			   ON [s].TaxClassID = i.TaxClassID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Topic_Updated'))
	DROP TRIGGER [dbo].[Topic_Updated]
GO

CREATE TRIGGER [dbo].[Topic_Updated]
	ON Topic
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Topic_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Topic
		SET
			Topic.UpdatedOn = current_timestamp
		FROM
			Topic [s]
			   INNER JOIN INSERTED i
			   ON [s].TopicID = i.TopicID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'TopicMapping_Updated'))
	DROP TRIGGER [dbo].[TopicMapping_Updated]
GO
IF (object_id('[dbo].[TopicMapping]', 'u') IS NOT NULL)
EXEC(
'CREATE TRIGGER [dbo].[TopicMapping_Updated]
	ON TopicMapping
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID(''TopicMapping_Updated'')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			TopicMapping
		SET
			TopicMapping.UpdatedOn = current_timestamp
		FROM
			TopicMapping [s]
			   INNER JOIN INSERTED i
			   ON [s].TopicID = i.TopicID
			   AND [s].ParentTopicID = i.ParentTopicID
	END')
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'TopicStore_Updated'))
	DROP TRIGGER [dbo].[TopicStore_Updated]
GO

CREATE TRIGGER [dbo].[TopicStore_Updated]
	ON TopicStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('TopicStore_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			TopicStore
		SET
			TopicStore.UpdatedOn = current_timestamp
		FROM
			TopicStore [s]
			   INNER JOIN INSERTED i
			   ON [s].TopicID = i.TopicID
			   AND [s].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Vector_Updated'))
	DROP TRIGGER [dbo].[Vector_Updated]
GO

CREATE TRIGGER [dbo].[Vector_Updated]
	ON Vector
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('Vector_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			Vector
		SET
			Vector.UpdatedOn = current_timestamp
		FROM
			Vector [v]
			   INNER JOIN INSERTED i
			   ON [v].VectorID = i.VectorID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ZipTaxRate_Updated'))
	DROP TRIGGER [dbo].[ZipTaxRate_Updated]
GO

CREATE TRIGGER [dbo].[ZipTaxRate_Updated]
	ON ZipTaxRate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;

		IF trigger_nestlevel(object_ID('ZipTaxRate_Updated')) > 1 RETURN;

		SET NOCOUNT ON

		UPDATE
			ZipTaxRate
		SET
			ZipTaxRate.UpdatedOn = current_timestamp
		FROM
			ZipTaxRate [v]
			   INNER JOIN INSERTED i
			   ON [v].ZipTaxID = i.ZipTaxID
	END
GO


/*********************End UpdatedOn and CreatedOn Columns & DB Triggers************************/

UPDATE AppConfig SET Description = 'DEPRECATED - Support for this AppConfig has been removed.' WHERE Name = 'ApplyShippingHandlingExtraFeeToFreeShipping'

-- Update store version
PRINT 'Updating Store Version...'
UPDATE [dbo].[AppConfig] SET [ConfigValue] = '9.3.1.0' WHERE [Name] = 'StoreVersion'

/********************* Update PayPal Sandbox URL's **************************/
update AppConfig set ConfigValue = 'https://api-3t.sandbox.paypal.com/2.0/' where Name = 'PayPal.API.TestURL' and ConfigValue = 'https://api.sandbox.paypal.com/2.0/'
update AppConfig set Description = 'DEPRECATED - Support for this AppConfig has been removed.' where Name = 'PayPal.API.LiveAAURL'
update AppConfig set Description = 'DEPRECATED - Support for this AppConfig has been removed.' where Name = 'PayPal.API.TestAAURL'


/*********** Begin 9.4.0.0 Changes ***********************/

GO
--TFS 1311
ALTER proc [dbo].[aspdnsf_getOrder]
    @ordernumber int

AS
SET NOCOUNT ON
SELECT
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
    o.eCheckBankABACode,
    o.eCheckBankAccountNumber,
    o.eCheckBankAccountType,
    o.eCheckBankName,
    o.eCheckBankAccountName,
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
    os.IsAKit,
    os.IsAPack
FROM Orders o with (nolock)
    left join orders_ShoppingCart os with (nolock) ON os.OrderNumber = o.OrderNumber
WHERE o.OrderNumber = @ordernumber
ORDER BY os.ShippingAddressID

GO

--insert topics for skin
if not exists(select name from topic where name = 'Template.Logo')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.Logo',
	'Template.Logo',
	1,
	0,
	'
	<a id="logo" class="logo" href="default.aspx" title="YourCompany.com" >
        <img src="App_Themes/Skin_(!SKINID!)/images/logo.gif" alt="YourCompany.com"/>
    </a>
	'
);

if not exists(select name from topic where name = 'Template.Header')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.Header',
	'Template.Header',
	1,
	0,
	'
	(!Topic Name="template.logo"!)
	<div class="user-links" id="user-links">
		(!USERNAME!)
		<a href="(!SIGNINOUT_LINK!)">(!SIGNINOUT_TEXT!)</a>
		<a href="account.aspx" class="account">Your Account</a>
		<a href="shoppingcart.aspx" class="cart">Shopping Cart ((!NUM_CART_ITEMS!))</a>
	</div>
	<div class="phone">1.800.555.1234</div>
	'
);

if not exists(select name from topic where name = 'Template.TopNavigation')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.TopNavigation',
	'Template.TopNavigation',
	1,
	0,
	'
	<ul class="top-nav-list">
	    <li><a href="default.aspx">Home</a></li>
	    <li><a href="t-about.aspx">About Us</a></li>
	    <li><a href="t-service.aspx">Customer Service</a></li>
	    <li><a href="t-faq.aspx">FAQs</a></li>
	    <li><a href="t-contact.aspx">Contact Us</a></li>
    </ul>
	'
);

if not exists(select name from topic where name = 'Template.VerticalNavigation')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.VerticalNavigation',
	'Template.VerticalNavigation',
	1,
	0,
	'
	<div class="nav-wrapper">
    	<h6 class="bar">
    		(!stringresource name="appconfig.categorypromptplural"!)
    	</h6>
    	(!xmlpackage name="entitymenu.xml.config" entitytype="category" !)
    </div>
    <div class="nav-wrapper">
    	<h6 class="bar">
    		(!stringresource name="appconfig.sectionpromptplural"!)
    	</h6>
    	(!xmlpackage name="entitymenu.xml.config" entitytype="section" !)
    </div>
    <div class="nav-wrapper">
    	<h6 class="bar">
    		(!stringresource name="appconfig.manufacturerpromptplural"!)
    	</h6>
    	(!xmlpackage name="entitymenu.xml.config" entitytype="manufacturer" !)
    </div>
	'
);
if not exists(select name from topic where name = 'Template.Footer')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.Footer',
	'Template.Footer',
	1,
	0,
	'
	<ul class="footer-list">
	    <li><a href="default.aspx">Home</a></li>
	    <li><a href="t-about.aspx">About Us</a></li>
	    <li><a href="t-contact.aspx">Contact Us</a></li>
	    <li><a href="sitemap2.aspx">Site Map</a></li>
	    <li><a href="t-service.aspx">Customer Service</a></li>
	    <li><a href="wishlist.aspx">Wishlist</a></li>
	    <li><a href="t-security.aspx">Security</a></li>
	    <li><a href="t-privacy.aspx">Privacy Policy</a></li>
	    <li>(!XmlPackage Name="mobilelink"!)</li>
    </ul>
	'
);

if not exists(select name from topic where name = 'Template.SubFooter')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.SubFooter',
	'Template.SubFooter',
	1,
	0,
	'
	&copy; YourCompany.com 2013. All Rights Reserved. Powered by <a href="http://www.aspdotnetstorefront.com" target="_blank">AspDotNetStorefront</a>
	'
);

/* 9.4 added for Download Products */
if not exists (select * from AppConfig where Name = 'Download.ShowRelatedProducts')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(0,'Download.ShowRelatedProducts','DOWNLOAD','If True, the product downloads page will display related items.','true', 'boolean');

if not exists (select * from AppConfig where Name = 'Download.ReleaseOnAction')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType, AllowableValues) values(0,'Download.ReleaseOnAction','DOWNLOAD','Valid configurations are (MANUAL, CAPTURE, AUTO).  Manual will require admin to release on the order page. CAPTURE will release on payment capture status.  AUTO will release the download without any requirements.','Manual', 'enum', 'Manual,Capture,Auto');

if not exists (select * from AppConfig where Name = 'Download.StreamFile')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(0,'Download.StreamFile','DOWNLOAD','If true (recommended), the file will be streamed and delivered on a button click instead of providing a URL to the file location.','true', 'boolean');

if not exists (select * from AppConfig where Name = 'Download.CopyFileForEachOrder')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(0,'Download.CopyFileForEachOrder','DOWNLOAD','If true (not recommended), the software will create a separate copy of each file that is purchased.  This configuration is ignored if you are using files on another server for your downloads.','false', 'boolean');

if not exists (select * from AppConfig where Name = 'Download.AllowMobileAccess')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(0,'Download.AllowMobileAccess','DOWNLOAD','If true, authenticated customers on mobile devices will be able to access the downloads page.','true', 'boolean');

if not exists (select * from Topic where Name = 'Download.Information')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Download.Information', 1, 0, 'Download.Information', 'Note to <a href="http://www.aspdotnetstorefront.com/">AspDotNetStorefront</a> Administrators:<br/><br/>You can edit this placeholder text by editing the "Download.Information" topic within the Admin Console.')

if not exists (select * from Topic where Name = 'Download.EmailHeader')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Download.EmailHeader', 1, 0, 'Download.EmailHeader', '')

if not exists (select * from Topic where Name = 'Download.EmailFooter')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Download.EmailFooter', 1, 0, 'Download.EmailFooter', '')

if not exists (select * from Topic where Name = 'Download.MobilePageContent')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Download.MobilePageContent', 1, 0, 'Download.MobilePageContent', 'Please visit the Downloads page on a non-mobile device to access your downloads.')

if exists (select * from Topic where Name = 'DownloadFooter' and datalength(Description) = 0)
	delete from Topic where name like 'DownloadFooter'

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' and COLUMN_NAME = 'DownloadStatus')
	ALTER TABLE Orders_ShoppingCart ADD DownloadStatus INT
	GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' and COLUMN_NAME = 'DownloadValidDays')
	ALTER TABLE Orders_ShoppingCart ADD DownloadValidDays INT
	GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' and COLUMN_NAME = 'DownloadCategory')
	ALTER TABLE Orders_ShoppingCart ADD DownloadCategory nvarchar(max)
	GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' and COLUMN_NAME = 'DownloadReleasedOn')
	ALTER TABLE Orders_ShoppingCart ADD DownloadReleasedOn DATETIME
	GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVariant' and COLUMN_NAME = 'DownloadValidDays')
	ALTER TABLE ProductVariant ADD DownloadValidDays INT
	GO

-- Add AppConfig for the new shoppingcart variant selector
If Not Exists(Select * From dbo.AppConfig Where name = 'AllowRecurringFrequencyChangeInCart')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)
	Values ('AllowRecurringFrequencyChangeInCart' ,'If true, customers will be able to change between recurring variants on the shopping cart page.','CHECKOUT','true','boolean', null);

-- Make gateway recurring billing on by default
PRINT 'Updating Recurring.UseGatewayInternalBilling...'
UPDATE [dbo].[AppConfig] SET [ConfigValue] = 'true' WHERE [Name] = 'Recurring.UseGatewayInternalBilling'

-- Add the new recurring informational topic
IF EXISTS (SELECT * FROM Topic WHERE Name='recurringpayments')
	BEGIN
		PRINT 'Recurringpayments topic exists already'
	END
ELSE
	BEGIN
		PRINT 'Adding recurringpayments topic'
		INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap, Published, Title, Description) values('recurringpayments', 1, 0, 1, 'recurringpayments', 'Add the information about your recurring products that you want to appear on this page by editing the "recurringpayments" topic within the Admin Console.')
	END

/* 9.4 Convert CardNumber to NVARCHAR(300) for [aspdnsf_PABPEraseCCInfo] performance */
ALTER TABLE Orders ALTER COLUMN CardNumber NVARCHAR(300) null
UPDATE Orders SET CardNumber=CardNumber

ALTER TABLE [Address] ALTER COLUMN CardNumber NVARCHAR(300) null
UPDATE [Address] SET CardNumber=CardNumber

/* 9.4 Remove unused AppConfigs */
DELETE FROM AppConfig WHERE Name = 'Vortx.OnePageCheckout.ModelFactoryAssembly'
DELETE FROM AppConfig WHERE Name = 'Vortx.OnePageCheckout.ModelFactoryType'
DELETE FROM AppConfig WHERE Name = 'Vortx.OnePageCheckout.ConfigurationFactoryAssembly'
DELETE FROM AppConfig WHERE Name = 'Vortx.OnePageCheckout.ConfigurationFactoryType'
DELETE FROM AppConfig WHERE Name = 'Vortx.OnePageCheckout.ZipCodeService.Yahoo.LicenseId'

/* 9.4 Create AppConfigs for SmartOPC changes */
if not exists(select * from AppConfig Where Name='Vortx.OnePageCheckout.DefaultShippingMethodId')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'Vortx.OnePageCheckout.DefaultShippingMethodId', 'If defined, the Smart One Page Checkout will automatically select this shipping method after the customer enters their shipping address information.  Set to blank to disable.', '', 'string', null, 'CHECKOUT', 0, 0, getdate());

if not exists(select * from AppConfig Where Name='Vortx.OnePageCheckout.AllowAlternativePaymentBillingAddressEdit')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'Vortx.OnePageCheckout.AllowAlternativePaymentBillingAddressEdit', 'If true, other payment methods besides credit card payment will allow collection of separate billing address on smart one page checkout.', 'false', 'boolean', null, 'CHECKOUT', 0, 0, getdate());

-- PayPal Integrated Express Checkout

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.IntegratedCheckout.LiveURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Express.IntegratedCheckout.LiveURL','GATEWAY','PayPal Express Integrated Checkout Live Site URL. Do not change unless you know what you are doing.','https://www.paypal.com/webapps/xo/webflow/sparta/xoflow');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.IntegratedCheckout.SandboxURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Express.IntegratedCheckout.SandboxURL','GATEWAY','PayPal Express Integrated Checkout Sandbox Site URL. Do not change unless you know what you are doing.','https://www.sandbox.paypal.com/webapps/xo/webflow/sparta/xoflow');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.UseIntegratedCheckout') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Express.UseIntegratedCheckout','GATEWAY','Use the PayPal Express integrated checkout.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Express.UseIntegratedCheckout'
END
GO

-- PayPal Bill Me Later Button
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.BillMeLaterButtonURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Hidden,Name,GroupName,Description,ConfigValue) values(1,1,'PayPal.Express.BillMeLaterButtonURL','GATEWAY','URL for Bill Me Later Button.','//www.paypalobjects.com/webstatic/en_US/btn/btn_bml_SM.png');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.ShowBillMeLaterButton') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'PayPal.Express.ShowBillMeLaterButton','GATEWAY','Show the Bill Me Later button on the shoppingcart page.','true', 'boolean');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.BillMeLaterMarketingMessage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Hidden,Name,GroupName,Description,ConfigValue) values(1,1,'PayPal.Express.BillMeLaterMarketingMessage','GATEWAY','Link & URL for Bill Me later Marketing Message to display beneath the Bill Me Later Button.','<a href="//www.securecheckout.billmelater.com/paycapture-content/fetch?hash=AU826TU8&content=/bmlweb/ppwpsiw.html" target="_blank"><img src="//www.paypalobjects.com/webstatic/en_US/btn/btn_bml_text.png" alt="Get 6 months to pay on $99+"/></a>');
END
GO


-- PayPal Banner Ads (Bill Me Later)
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.TermsAndConditionsAgreement') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.TermsAndConditionsAgreement','GATEWAY','I agree to the <a href="#" target="_blank">terms and conditions</a> for PayPal Banners.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Ads.TermsAndConditionsAgreement'
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.PublisherId') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.PublisherId','GATEWAY','Your PayPal Publisher Id from the PayPal Media Network (PMN)','');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnProductPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.ShowOnProductPage','GATEWAY','Show the bill me later ads on your product page.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Ads.ShowOnProductPage'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ProductPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.ProductPageDimensions','GATEWAY','PayPal ad dimensions for the product page.','120x90');
UPDATE dbo.[AppConfig] SET ValueType = 'enum' WHERE [Name] = 'PayPal.Ads.ProductPageDimensions'
UPDATE dbo.[AppConfig] SET AllowableValues = '120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200' WHERE [Name] = 'PayPal.Ads.ProductPageDimensions'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnHomePage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.ShowOnHomePage','GATEWAY','Show the bill me later ads on your home page.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Ads.ShowOnHomePage'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.HomePageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.HomePageDimensions','GATEWAY','PayPal ad dimensions for the home page.','120x90');
UPDATE dbo.[AppConfig] SET ValueType = 'enum' WHERE [Name] = 'PayPal.Ads.HomePageDimensions'
UPDATE dbo.[AppConfig] SET AllowableValues = '120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200' WHERE [Name] = 'PayPal.Ads.HomePageDimensions'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnCartPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.ShowOnCartPage','GATEWAY','Show the bill me later ads on your Shopping Cart page.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Ads.ShowOnCartPage'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.CartPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.CartPageDimensions','GATEWAY','PayPal ad dimensions for the product page.','120x90');
UPDATE dbo.[AppConfig] SET ValueType = 'enum' WHERE [Name] = 'PayPal.Ads.CartPageDimensions'
UPDATE dbo.[AppConfig] SET AllowableValues = '120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200' WHERE [Name] = 'PayPal.Ads.CartPageDimensions'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnEntityPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.ShowOnEntityPage','GATEWAY','Show the bill me later ads on your entity pages.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Ads.ShowOnEntityPage'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.EntityPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.EntityPageDimensions','GATEWAY','PayPal ad dimensions for the entity pages.','120x90');
UPDATE dbo.[AppConfig] SET ValueType = 'enum' WHERE [Name] = 'PayPal.Ads.EntityPageDimensions'
UPDATE dbo.[AppConfig] SET AllowableValues = '120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200' WHERE [Name] = 'PayPal.Ads.EntityPageDimensions'
END
GO


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'Topic' and COLUMN_NAME = 'IsFrequent')
	ALTER TABLE Topic ADD IsFrequent BIT DEFAULT 1 WITH VALUES
	GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Promotions.ApplyDiscountsBeforePromoApplied') BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'Promotions.ApplyDiscountsBeforePromoApplied','SETUP','If this is set to TRUE, promotions will be applied after quantity discounts.','true','boolean');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'DefaultHeight_micro') BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(0,'DefaultHeight_micro','IMAGERESIZE','Default height of an micro image if no height attribute is specified in the other size-configs (i.e. width:50;). This value should NOT be left blank.','50','NULL');
END
GO

if not exists (select * from AppConfig where Name = 'Account.ShowBirthDateField')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Account.ShowBirthDateField', 'This will show the Birth Date field while creating customer account.', '', 'boolean', 'null', 'DISPLAY', 0, 0, getdate())
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderNumber_Updated'))
	DROP TRIGGER [dbo].[OrderNumber_Updated]
GO

print 'Adding ShowDistributorNotificationPriceInfo appconfig'
if not exists (select * from AppConfig where Name = 'ShowDistributorNotificationPriceInfo')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'ShowDistributorNotificationPriceInfo', 'Show prices and order total information on the distributor notification emails.', 'false', 'boolean', 'null', 'MISC', 0, 0, getdate())
GO

print 'Increasing the max length of the appconfig value column'
if not exists(select CHARACTER_MAXIMUM_LENGTH  from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'configvalue' and TABLE_NAME = 'appconfig' and CHARACTER_MAXIMUM_LENGTH = -1)
begin
	ALTER TABLE appconfig
	ALTER COLUMN configvalue nvarchar(max)
end

print 'Removing the basic OPC checkout type and related appconfigs'

update AppConfig set Description = 'The Checkout Type.  Valid Values are Standard, SmartOPC, or Other.',
		AllowableValues = 'Standard,SmartOPC,Other'
	where Name = 'Checkout.Type'

update AppConfig set  ConfigValue = 'Standard'
	where  Name = 'Checkout.Type'
	and (ConfigValue = 'BasicOPC' or ConfigValue = 'DeprecatedOPC')

delete from appconfig where name = 'Checkout.UseOnePageCheckout'
delete from appconfig where name = 'Checkout.UseOnePageCheckout.UseFinalReviewOrderPage'


--Add GTIN for 9.4
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('Inventory') AND name = 'GTIN')
BEGIN
	ALTER TABLE [dbo].[Inventory] ADD GTIN nvarchar(14) null
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('ProductVariant') AND name = 'GTIN')
BEGIN
	ALTER TABLE [dbo].[ProductVariant] ADD GTIN nvarchar(14) null
END

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('ShoppingCart') AND name = 'GTIN')
BEGIN
	ALTER TABLE [dbo].[ShoppingCart] ADD GTIN nvarchar(14) null
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('Orders_ShoppingCart') AND name = 'GTIN')
BEGIN
	ALTER TABLE [dbo].[Orders_ShoppingCart] ADD GTIN nvarchar(14) null
END
GO

ALTER proc dbo.aspdnsf_AddItemToCart
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
	DECLARE @RestrictedQy varchar(8000), @CurrentCartQty int, @InventoryFullSKU nvarchar(50), @InventoryWeightDelta money, @AllowEmptySkuAddToCart varchar(10), @ShoppingCartrecid int, @IsAKit tinyint
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


	SELECT @AllowEmptySkuAddToCart = ConfigValue FROM dbo.AppConfig with (nolock) WHERE [name]= 'AllowEmptySkuAddToCart'


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
		and (@AllowEmptySkuAddToCart = 'true' or rtrim(case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then isnull(p.sku, '') + isnull(pv.skusuffix, '') + isnull(@ChosenColorSKUModifier, '') + isnull(@ChosenSizeSKUModifier, '') else i.VendorFullSKU end) <> '')

	SET @ShoppingCartrecid = @@IDENTITY

	--Update KitCart Table if necessary
	IF (@IsAKit = 1 AND @IsKit2 = 0) BEGIN
		UPDATE KitCart SET ShoppingCartRecID = @ShoppingCartrecid WHERE ProductID = @ProductID and VariantID = @VariantID and ShoppingCartRecID = 0 and CustomerID = @CustomerID
	END

	SET @NewShoppingCartRecID = @ShoppingCartrecid
GO

print 'Updating appconfig stored procedures'
IF EXISTS (select * from dbo.sysobjects where id = object_id('aspdnsf_insAppconfig') and OBJECTPROPERTY(id, 'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insAppconfig]
GO
Create proc [dbo].[aspdnsf_insAppconfig]
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

    set @AppConfigID = @@identity
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id('aspdnsf_updAppconfig') and OBJECTPROPERTY(id, 'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_updAppconfig]
GO
Create proc [dbo].[aspdnsf_updAppconfig]
    @AppConfigID int,
    @Description nvarchar(max) = null,
    @ConfigValue nvarchar(max) = null,
    @GroupName nvarchar(100) = null,
    @SuperOnly tinyint = null,
    @StoreID int = null,
    @ValueType nvarchar(100) = null,
    @AllowableValues nvarchar(max) = null

AS
SET NOCOUNT ON


    UPDATE dbo.Appconfig
    SET
        Description = COALESCE(@Description, Description),
        ConfigValue = COALESCE(@ConfigValue, ConfigValue),
        GroupName = COALESCE(@GroupName, GroupName),
        SuperOnly = COALESCE(@SuperOnly, SuperOnly),
        StoreID =  COALESCE(@StoreID, StoreID),
	ValueType =  COALESCE(@ValueType, ValueType),
	AllowableValues = COALESCE(@AllowableValues, AllowableValues)
    WHERE AppConfigID = @AppConfigID
GO

print 'Installing and updating the maxmind appconfig parameters'
if not exists (select * from AppConfig where Name = 'MaxMind.ServiceType')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'MaxMind.ServiceType', 'This can be set to either "standard" or "premium". By default, we use the highest level of service available for your account. If you have both the premium and standard minFraud service, you can choose to use the standard service to save on costs.', 'premium', 'enum', 'standard,premium', 'GATEWAY', 0, 0, getdate())
GO
if not exists (select * from AppConfig where Name = 'MaxMind.ExplanationLink')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'MaxMind.ExplanationLink', 'The URL where admins can find more information about the maxmind riskScore. Used on the order management screen.', 'http://www.maxmind.com/en/riskscore', 'string', null, 'GATEWAY', 0, 0, getdate())
GO
UPDATE dbo.Appconfig set ConfigValue = 'http://minfraud.maxmind.com/app/minfraud_soap' where name = 'MaxMind.SOAPURL'
GO
UPDATE dbo.Appconfig
	set Description = 'Threshold over which to fail orders. 0.10 is lowest risk. 100.0 is highest risk. By default, this setting (of 100.0) will NOT FAIL any order. You will have to set this threshold to your own liking for your own business. Every store will probably use different thresholds due to the nature of their business. Consult MaxMind.com for documentation.'
where name = 'MaxMind.FailScoreThreshold'
GO
UPDATE dbo.Appconfig
	set  Description = 'Threshold over which to force delayed downloads on orders. 0.10 is lowest risk. 100.0 is highest risk. See AppConfig:MaxMind.ScoreThreshold also.'
where name = 'MaxMind.DelayDownloadThreshold'
GO
UPDATE dbo.Appconfig
	set Description = 'Threshold over which to force delayed dropship notifications on orders, regardless of the setting of AppConfig:DelayedDropShipNotifications is set to. 0.10 is lowest risk. 100.0 is highest risk. See AppConfig:MaxMind.ScoreThreshold also.'
where name = 'MaxMind.DelayDropShipThreshold'
GO
UPDATE dbo.Appconfig
	set Description = 'If true, the MaxMind fraud prevention score will be checked before a credit card is sent to the gateway. If the returned FraudScore exceeds AppLogic.MaxMind.FailScoreThreshold, the order will be failed. See MaxMind.com for more documentation. This feature uses MaxMind''s minFraud service version 1.3'
where name = 'MaxMind.Enabled'
GO

print 'Installing and updating the USAePay appconfig parameters'
if not exists (select * from AppConfig where Name = 'USAePay.EndpointLive')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'USAePay.EndpointLive','GATEWAY','WSDL Endpoint for USAePay SOAP API Live Transactions. Do not change.','https://www.usaepay.com/soap/gate/2E58E844', 'string');

if not exists (select * from AppConfig where Name = 'USAePay.EndpointSandbox')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'USAePay.EndpointSandbox','GATEWAY','WSDL Endpoint for USAePay SOAP API Sandbox Transactions. Do not change.','https://sandbox.usaepay.com/soap/gate/2E58E844', 'string');
GO

UPDATE dbo.Appconfig
	set Description = 'For Developers Use Only - Non-mobile pages entered in this AppConfig will be displayed in the mobile skin when a user is viewing the mobile site.(example: manufacturers.aspx,sitemap.aspx)'
where name = 'Mobile.PageExceptions'
GO

if not exists (select * from AppConfig where Name = 'RequireEmailConfirmation')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'RequireEmailConfirmation','SETUP','If true, customers will be prompted to re-enter their email addresses for confirmation when registering or updating accounts.','false', 'boolean');
GO

if not exists (select * from AppConfig where Name = 'ShowInStorePickupInShippingEstimator')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'ShowInStorePickupInShippingEstimator','SHIPPING','If true, customers will see the ''In Store Pickup'' option (if it is enabled) in the shipping estimator on the shopping cart page.','false', 'boolean');
GO

if not exists (select * from AppConfig where Name = 'SearchDescriptionsByDefault')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'SearchDescriptionsByDefault','SETUP','If true, searches on your site will include the description and summary fields by default. NOTE: This will put additional strain on your site''s resources and may not be advisable in some shared hosting environments.','true', 'boolean');
GO

print 'Installing Google Remarketing and Dynamic Remarketing configuration elements'
if not exists (select * from AppConfig where Name = 'Google.Remarketing.Enabled')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Google.Remarketing.Enabled', 'Enable Google Remarketing script on your site. This puts the contents of the Script.Google.Remarketing topic on every page of your site. You must make sure that the script.bodyclose xmlpackage is included in your template.', 'false', 'boolean', null, 'MISC', 0, 0, getdate())
GO

if not exists (select * from AppConfig where Name = 'Google.DynamicRemarketing.Enabled')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Google.DynamicRemarketing.Enabled', 'Enable Google Dynamic Remarketing script on your site. Google Remarketing must also be installed.', 'false', 'boolean', null, 'MISC', 0, 0, getdate())
GO

if not exists (select * from AppConfig where Name = 'Google.DynamicRemarketing.ProductIdentifierFormat')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Google.DynamicRemarketing.ProductIdentifierFormat', 'This string allows you to specify the format of your google product identifiers. Valid tokens are {ProductID},{VariantID}, and {FullSKU}. These tokens are case sensitive.', '{ProductID}-{VariantID}--', 'string', null, 'MISC', 0, 0, getdate())
GO

if not exists(select name from topic where name = 'Script.Google.Remarketing')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Script.Google.Remarketing',
	'Script.Google.Remarketing',
	1,
	0,
	''
);
GO

if not exists (select * from AppConfig where Name = 'ProductPageOutOfStockRedirect')
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'ProductPageOutOfStockRedirect','GATEWAY','Will provide a 404 for the product URL if a product is hidden due to inventory.  Should only be set to true if you are not concerned about search engine page rankings.','false', 'boolean');
GO

print 'Updating google analytics'
declare @UseDeprecatedTokens varchar(10)
select @UseDeprecatedTokens = configvalue from AppConfig where Name = 'Google.EcomOrderTrackingEnabled' and storeid = 0

if not exists (select * from AppConfig where Name = 'Google.DeprecatedEcomTokens.Enabled')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Google.DeprecatedEcomTokens.Enabled'
	, 'The GOOGLE_ECOM_TRACKING_ASYNCH and GOOGLE_ECOM_TRACKING_V2 tokens have been deprecated and will be removed in a future release. You can set the value of this appconfig to true to continue using the old tokens, but we recommend updating to the new method. Please see the manual for details.'
	, @UseDeprecatedTokens, 'boolean', null, 'MISC', 0, 0, getdate())
end
GO

UPDATE dbo.AppConfig
	set Description = 'Determines whether the google ecommerce tracking code is fired on the order confirmation.  If this AppConfig is disabled, Analytics will still function, however order details will not be sent to Google.'
where name = 'Google.EcomOrderTrackingEnabled'
GO

print 'Add Avalara AppConfig'
if not exists (select * from AppConfig where name like 'AvalaraTax.PreventOrderIfAddressValidationFails')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.PreventOrderIfAddressValidationFails', 'If true, Avalara address validation errors will prevent checkout.', 'False', 'boolean', null, 'AVALARATAX', 0, 0, getdate())
GO

print 'Updating menu appconfigs'
if not exists(select name from appconfig where name = 'MaxMenuSize')
	begin
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue)
		values(0,'MaxMenuSize','SITEDISPLAY','The maximum number of items you want to allow in a top menu (e.g. manufacturers). If a menu is longer than this, it will display a "more" link. 0 will disable the limit altogether.','0');
	end
else
	begin
		--leave the value alone in case it is already set.
		update appconfig set description = 'The maximum number of items you want to allow in a top menu (e.g. manufacturers). If a menu is longer than this, it will display a "more" link. 0 will disable the limit altogether.' where name = 'MaxMenuSize'
	end
Go

if not exists(select name from appconfig where name = 'MaxMenuLevel')
	begin
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue)
		values(0,'MaxMenuLevel','SITEDISPLAY','The maximum number of menu levels to render for dynamic menu. A value of 0 will turn this limit off altogether.','0');
	end
else
	begin
		--okay to update because the appconfig is unused until now
		update appconfig set description = 'The maximum number of menu levels to render for dynamic menu. A value of 0 will turn this limit off altogether.', configvalue = '0' where name = 'MaxMenuLevel'
	end
Go


print 'Updating PayPal Appconfig descriptions'
update appconfig set description = 'Set to true to use PayPal Instant Notification to capture payments' where name = 'PayPal.UseInstantNotification'
update appconfig set description = 'PayPal assigned API merchant e-mail address for your account. Consult PayPal documentation for more information. This is almost ALWAYS left blank!' where name = 'PayPal.API.MerchantEMailAddress'
update appconfig set description = 'PayPal assigned API username. Consult PayPal documentation for more information. You get this from the PayPal site.' where name = 'PayPal.API.Username'
update appconfig set description = 'PayPal assigned API password. Consult PayPal documentation for more information. You get this from the PayPal site.' where name = 'PayPal.API.Password'
update appconfig set description = 'PayPal assigned API signature. Consult PayPal documentation for more information. You get this from the PayPal site.' where name = 'PayPal.API.Signature'
update appconfig set description = 'This shows the PayPal Express checkout button on your shopping cart page. You will also need to configure your PayPal API credentials.' where name = 'PayPal.Express.ShowOnCartPage'
update appconfig set description = 'This enables customers to checkout using PayPal Express without being a registered customer (i.e. anonymous customer). When you set this to true you also need to set the AllowCustomerDuplicateEMailAddresses AppConfig parameter to true.' where name = 'PayPal.Express.AllowAnonCheckout'

/*********** End 9.4.0.0 Changes ***********************/



/*********** Begin 9.4.1.0 Changes *********************/

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'GiftCards.Enabled') BEGIN
	INSERT [dbo].[AppConfig] ([SuperOnly], [Name], [GroupName], [Description], [ValueType] ,[ConfigValue])
		values(1,'GiftCards.Enabled','SITEDISPLAY','Enables GiftCards to be used in the shopping cart', 'boolean', 'true');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Promotions.Enabled') BEGIN
	INSERT [dbo].[AppConfig] ([SuperOnly], [Name], [GroupName], [Description], [ValueType] ,[ConfigValue])
		values(1,'Promotions.Enabled','SITEDISPLAY','Enables Promotions to be used in the shopping cart', 'boolean', 'true');
END
GO

-- Disable promotions if they were disabled before
IF EXISTS (SELECT * FROM [dbo].[AppConfig] WHERE [Name] = 'DisallowCoupons') BEGIN
	DECLARE @PromosEnabled varchar(max);
	SELECT @PromosEnabled = ConfigValue FROM [dbo].[AppConfig] WHERE [Name] = 'DisallowCoupons';
	IF @PromosEnabled = 'true' BEGIN
		UPDATE [dbo].[AppConfig] SET [ConfigValue] = 'false' WHERE [Name] = 'Promotions.Enabled';
	END
END
GO

UPDATE [dbo].[AppConfig] SET [Description] = '*DEPRECATED* - Use AppConfigs GiftCards.Enabled or Promotions.Enabled to selectively enable the features DisallowCoupons was previously used for.'
	WHERE [Name] = 'DisallowCoupons';
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.AVSRequireConfirmedAddress') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Express.AVSRequireConfirmedAddress','GATEWAY','Require Confirmed Address. This is used to qualify for Seller Protection.  If set to true, shoppers who do not have an AVS Confirmed shipping address set in their PayPal account will not be able to check out with PayPal Express.','false', 'boolean');
END
GO

-- PayPal Banner Ads (Bill Me Later)
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.BannerEnrollmentService.SandboxURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.BannerEnrollmentService.SandboxURL','GATEWAY','Sandbox URL for Banner Enrollment Service.','https://api.financing-mint.paypal.com/finapi/v1/publishers/');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.BannerEnrollmentService.LiveURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.BannerEnrollmentService.LiveURL','GATEWAY','Live URL for Banner Enrollment Service.','https://api.financing.paypal.com/finapi/v1/publishers/');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.TermsAndConditionsAgreement') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Ads.TermsAndConditionsAgreement','GATEWAY','I agree to the <a href="https://financing.paypal.com/ppfinportal/content/operatingAgmt" target="_blank">terms and conditions</a> for PayPal Banners.','false','boolean');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.PublisherId') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.PublisherId','GATEWAY','Your PayPal Publisher Id from the PayPal Media Network (PMN)','');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnProductPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Ads.ShowOnProductPage','GATEWAY','Show the bill me later ads on your product page.','false','boolean');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ProductPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType,AllowableValues) values(1,'PayPal.Ads.ProductPageDimensions','GATEWAY','PayPal ad dimensions for the product page.','120x90','enum',
	'120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnHomePage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Ads.ShowOnHomePage','GATEWAY','Show the bill me later ads on your home page.','false','boolean');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.HomePageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType,AllowableValues) values(1,'PayPal.Ads.HomePageDimensions','GATEWAY','PayPal ad dimensions for the home page.','120x90','enum',
	'120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnCartPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Ads.ShowOnCartPage','GATEWAY','Show the bill me later ads on your Shopping Cart page.','false','boolean');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.CartPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType,AllowableValues) values(1,'PayPal.Ads.CartPageDimensions','GATEWAY','PayPal ad dimensions for the product page.','120x90','enum',
	'120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnEntityPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Ads.ShowOnEntityPage','GATEWAY','Show the bill me later ads on your entity pages.','false','boolean');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.EntityPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType,AllowableValues) values(1,'PayPal.Ads.EntityPageDimensions','GATEWAY','PayPal ad dimensions for the entity pages.','120x90','enum',
	'120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200');
END
GO

GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Vortx.OnePageCheckout.AllowRTShipping.NoMethodSelected') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType)
values(0,'Vortx.OnePageCheckout.AllowRTShipping.NoMethodSelected','CHECKOUT','This allows $0 shipping. Allows customer checkout when real time shipping returns zero methods, such as when order weight exceeds UPS, USPS, or FEDEX weight limits.','false','boolean');
END
GO


--Update the mobile user agent list to remove tosh so desktop Macs stop triggering the mobile skin
UPDATE [dbo].AppConfig SET ConfigValue = REPLACE(ConfigValue, ',tosh,', ',') WHERE Name = 'Mobile.ShortUserAgentList'
GO

--Remove Google Checkout as a payment option
DELETE FROM [dbo].AppConfig WHERE Name IN ('GoogleCheckout.ShowOnCartPage','GoogleCheckout.UseSandbox','GoogleCheckout.DefaultShippingMarkup','GoogleCheckout.MerchantId','GoogleCheckout.MerchantKey','GoogleCheckout.SandboxMerchantId','GoogleCheckout.SandboxMerchantKey','GoogleCheckout.LogMessages','GoogleCheckout.LogFileName','GoogleCheckout.BaseUrl','GoogleCheckout.DiagnosticsOnly','GoogleCheckout.DefaultTaxRate','GoogleCheckout.UseTaxTables','GoogleCheckout.ShippingIsTaxed','GoogleCheckout.SandBoxCheckoutButton','GoogleCheckout.LiveCheckoutButton','GoogleCheckout.SandBoxCheckoutURL','GoogleCheckout.AllowAnonCheckout','GoogleCheckout.DefaultDomesticShipToCity','GoogleCheckout.DefaultDomesticShipToState','GoogleCheckout.DefaultDomesticShipToZip','GoogleCheckout.DefaultDomesticShipToCountry','GoogleCheckout.DefaultInternationalShipToCity','GoogleCheckout.DefaultInternationalShipToState','GoogleCheckout.DefaultInternationalShipToZip','GoogleCheckout.DefaultInternationalShipToCountry','GoogleCheckout.CarrierCalculatedShippingEnabled','GoogleCheckout.CarrierCalculatedPackage','GoogleCheckout.CarrierCalculatedShippingOptions','GoogleCheckout.CarrierCalculatedDefaultPrice','GoogleCheckout.CarrierCalculatedFreeOption','GoogleCheckout.ConversionURL','GoogleCheckout.ConversionParameters','GoogleCheckout.SendStoreReceipt','GoogleCheckout.AuthenticateCallback','Mobile.GoogleCheckout.LiveCheckoutButton')
GO


IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'XmlPackage.SearchPage')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue)
	values(0,'XmlPackage.SearchPage','XMLPACKAGE','The XmlPackage used to display search results on the search page. "page.search.xml.config" is the default value.','page.search.xml.config');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'XmlPackage.SearchAdvPage')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue)
	values(0,'XmlPackage.SearchAdvPage','XMLPACKAGE','The XmlPackage used to display search results on the advanced search page. "page.searchadv.xml.config" is the default value.','page.searchadv.xml.config');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Search_NumberOfColumns')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue)
	values(0,'Search_NumberOfColumns','SITEDISPLAY','The number of columns on the search page grid. 4 is the default value.','4');
END
GO





update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'HidePicsInTableCondensed'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'ShowFullNameInTableExpanded'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'ShowDescriptionInTableCondensed'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'ShowDimensionsInTableCondensed'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'ShowWeightInTableCondensed'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'RelatedProductsFormat'

/*********** End 9.4.1.0 Changes ***********************/



/*********** Begin 9.4.2.0 Changes *********************/

--Update a confusing AppConfig description
if exists (select * from AppConfig where Name = 'QuantityDiscount.CombineQuantityByProduct')
begin
	UPDATE AppConfig SET Description = 'If this is false then quantity discounts will be calculated per line item. If true, all line items with the same product ID will be factored into quantity discount calculations, even if they have different variant, size, or color options.'
	WHERE Name = 'QuantityDiscount.CombineQuantityByProduct'
end
go

-- New MaxCartItemsBeforeCheckout appconfig
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'MaxCartItemsBeforeCheckout')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue)
	values(0,'MaxCartItemsBeforeCheckout','GATEWAY','Maximum # of line items a user may have in their cart before they can checkout.  Quantities do not matter, this looks at the number of separate items in the cart.','integer','300');
END
GO

-- New appconfig for re-ordering
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Reorder.Enabled')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue)
	values(0,'Reorder.Enabled','MISC','If enabled, customers will see a grid of previous orders on the account.aspx page, each with a link to place the same order again.','boolean','true');
END
GO

-- New appconfig for google trusted stores
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'GoogleTrustedStoreProductIdentifierFormat')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue)
	values(0,'GoogleTrustedStoreProductIdentifierFormat','Google Trusted Store','This string allows you to specify the format of your google product identifiers. Valid tokens are {ProductID}, {VariantID}, and {FullSKU}. These tokens are case sensitive.','string','{ProductID}-{VariantID}--');
END
GO

-- New appconfig for google trusted stores
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'GoogleTrustedStoreProductIdentifierEnabled')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue)
	values(0,'GoogleTrustedStoreProductIdentifierEnabled','Google Trusted Store','This will add your product identifier to the google trusted stores script on product pages. Make sure to setup the appconfig titled GoogleTrustedStoreProductIdentifierFormat when you enable this feature.','boolean','false');
END
GO

-- New appconfig for google analytics
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Google.AnalyticsDisplayAdvertising.Enabled')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue)
	values(0,'Google.AnalyticsDisplayAdvertising.Enabled','MISC','This will enable Google display advertising features within your Google Analytics tag. You may need to update your privacy policy if you turn this feature on. See Google''s display advertising policy requirements for more inforamtion.','boolean','false');
END
GO

--deprecate microstyle appconfig
if exists(select name from appconfig where name = 'MicroStyle')
begin
	update appconfig set description = 'DEPRECATED - Attributes used for MultiMakesMicros. The cols colspacing, and rowspacing attributes are used to determine how many images can appear in each row and how much space (in pixels) is between each image while the width and height determine the resized micro height.'
	where name = 'MicroStyle'
end
go

if exists(select * from appconfig where name = 'MultiMakesMicros')
begin
	update appconfig set description = 'If true this will create micro images resized by the width and height specified in DefaultWidth_micro and DefaultHeight_micro  and will save them in the images/product/micro folder whenever you are uploading multiple images in the medium multi-image manager.  If a product has multi-images and UseImagesForMultiNav is true then images will be shown instead of the number icons.'
	where name = 'MultiMakesMicros'
end
go

-- add display name to the shipping methods table
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('ShippingMethod') AND name = 'DisplayName')
    ALTER TABLE dbo.ShippingMethod ADD DisplayName nvarchar(400) NULL
GO

-- MicroPay cleanup
update [dbo].[AppConfig] set [Description] = 'If this is true and MicroPay is enabled as a payment method, the user''s current Micropay Balance will be shown at the top of the shopping cart page.' where [Name] = 'Micropay.ShowTotalOnTopOfCartPage'
update [dbo].[AppConfig] set [Description] = 'If this is true and MicroPay is enabled as a payment method, the "add $5 to your micropay" Product line item will NOT appear on the shopping cart page. This is helpful if the store administrator is controlling their micropay balance using some other means.' where [Name] = 'Micropay.HideOnCartPage'
DELETE FROM [dbo].[AppConfig] WHERE [Name] = 'MicroPay.Enabled'

-- Local pickup AppConfig description clarification
update [dbo].[AppConfig] set [Description] = 'State restrictions for the store-pickup option if the restriction type is state.  This should be a comma-separated list of the 2-character state abbreviations found on the Taxes->Edit State/Provinces page.' where [Name] = 'RTShipping.LocalPickupRestrictionStates'
update [dbo].[AppConfig] set [Description] = 'Zip Code restrictions for the store-pickup option if the restriction type is zip.  This should be a comma-separated list of 5-digit zip codes.' where [Name] = 'RTShipping.LocalPickupRestrictionZips'
update [dbo].[AppConfig] set [Description] = 'Zone restrictions for the store-pickup option if the restriction type is zone.  This should be a list of zone IDs from the Shipping->Shipping Zones Page.' where [Name] = 'RTShipping.LocalPickupRestrictionZones'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.aspdnsf_ProductInfo') AND type IN ( N'P', N'PC' ) )
	begin
	 DROP PROCEDURE dbo.aspdnsf_ProductInfo;
	end
GO

-- Added info for Schema.org attributes
CREATE proc [dbo].[aspdnsf_ProductInfo]
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

		SELECT @FilterProductsByCustomerLevel		= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterProductsByCustomerLevel'		AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
		SELECT @CustomerLevelFilteringIsAscending	= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterByCustomerLevelIsAscending'	AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
		SELECT @FilterProductsByAffiliate			= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterProductsByAffiliate'			AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
		SELECT @HideProductsWithLessThanThisInventoryLevel	= CONVERT(INT, ConfigValue) FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'HideProductsWithLessThanThisInventoryLevel' AND ISNUMERIC(ConfigValue) = 1 AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
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

-- New appconfig for dimension units
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Localization.DimensionUnits')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue)
	values(0,'Localization.DimensionUnits','SETUP','Enter the prompt you want to use for Dimensions (e.g. inches (IN) or centimeters (CM))','string','IN');
END
GO

-- Create the CBA AppConfigs
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.CbaAccessKey')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.CbaAccessKey','GATEWAY','Checkout By Amazon Access Key','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.CbaSecretKey')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.CbaSecretKey','GATEWAY','Checkout By Amazon Secret Key','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.MwsAccessKey')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.MwsAccessKey','GATEWAY','Amazon Marketplace Access Key','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.MwsSecretKey')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.MwsSecretKey','GATEWAY','Amazon Marketplace Secret Key','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.MerchantId')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.MerchantId','GATEWAY','Checkout By Amazon Merchant Id','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.Marketplace')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.Marketplace','GATEWAY','Checkout By Amazon Marketplace','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.WidgetUrl')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.WidgetUrl','GATEWAY','The url used to render the widget scripts','string','https://static-na.payments-amazon.com/cba/js/us/PaymentWidgets.js',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.WidgetSandboxUrl')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.WidgetSandboxUrl','GATEWAY','The sandbox url used to render the widget scripts','string','https://static-na.payments-amazon.com/cba/js/us/sandbox/PaymentWidgets.js',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.CBAServiceUrl')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.CBAServiceUrl','GATEWAY','The url used to call the cba service','string','https://payments.amazon.com/cba/api/purchasecontract/',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.CBAServiceSandboxUrl')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.CBAServiceSandboxUrl','GATEWAY','The sandbox url used to call the cba service','string','https://payments-sandbox.amazon.com/cba/api/purchasecontract/',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.MerchantServiceUrl')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.MerchantServiceUrl','GATEWAY','The url used to call the merchant service','string','https://mws.amazonservices.com',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.MerchantServiceSandboxUrl')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.MerchantServiceSandboxUrl','GATEWAY','The sandbox url used to call the merchant service','string','https://mws.amazonservices.com',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.UseSandbox')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'CheckoutByAmazon.UseSandbox','GATEWAY','Puts the Checkout By Amazon services in sandbox mode','boolean','true',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.OrderFulfillmentType')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,AllowableValues,StoreId)
	values(0,'CheckoutByAmazon.OrderFulfillmentType','GATEWAY','Checkout By Amazon Order Fulfillment Type. Instant: Marks the order as shipped immediatly after getting the ready to ship notification. MarkedAsShipped - Notifies Amazon that the order has shipped when an admin marks the order as shipped. Never - Admins must manually adjust ship status at Amazon.','enum','Never','Instant,Never,MarkedAsShipped',0);
END
GO

-- New appconfig for displaying watermarks on icon sized product images
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Watermark.Icons.Enabled')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'Watermark.Icons.Enabled','MISC','When this is set to true and watermarks are enabled, icon sized product images will also contain watermarks.','boolean','true',0);
END
GO

-- Related products changes
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'RelatedProducts.NumberDisplayed')
BEGIN
	UPDATE AppConfig SET Name = 'RelatedProducts.NumberDisplayed' WHERE Name = 'DynamicRelatedProducts.NumberDisplayed'
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'RelatedProducts.NumberDisplayed')
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId)
	values(0,'RelatedProducts.NumberDisplayed','DISPLAY','Controls the maximum number of related products that will be displayed at the bottom of the Product pages.','integer','4',0);
END
GO

/*********** Begin 9.5.0.0 Changes *********************/
--Layout cleanup
DELETE FROM AppConfig WHERE Name = 'Layouts.Enabled'

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[Layout]') and type = 'u')
DROP TABLE [dbo].[Layout]
GO
IF EXISTS (select * From sysobjects where id = object_id('[dbo].[LayoutField]') and type = 'u')
DROP TABLE [dbo].[LayoutField]
GO
IF EXISTS (select * From sysobjects where id = object_id('[dbo].[LayoutFieldAttribute]') and type = 'u')
DROP TABLE [dbo].[LayoutFieldAttribute]
GO
IF EXISTS (select * From sysobjects where id = object_id('[dbo].[LayoutMap]') and type = 'u')
DROP TABLE [dbo].[LayoutMap]
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_insLayout') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insLayout]
GO
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_insLayoutField') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insLayoutField]
GO
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_insLayoutFieldAttribute') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insLayoutFieldAttribute]
GO
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_insLayoutMap') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insLayoutMap]
GO
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_updLayout') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_updLayout]
GO
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_updLayoutFieldAttribute') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_updLayoutFieldAttribute]
GO

--Subscription cleanup
DELETE FROM AppConfig WHERE Name = 'SubscriptionExpiredGracePeriod'
DELETE FROM AppConfig WHERE Name = 'Suscription.ExpiredMessageWhenViewingTopic'
DELETE FROM AppConfig WHERE Name = 'SubscriptionExtensionOccursFromOrderDated'

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetProductsEntity') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetProductsEntity]
GO

create proc [dbo].[aspdnsf_GetProductsEntity]
    @categoryID      int = 0,
    @sectionID       int = 0,
    @manufacturerID  int = 0,
    @distributorID   int = 0,
    @genreID   int = 0,
    @vectorID   int = 0,
    @affiliateID     int = 0,
    @ProductTypeID   int = 1,
    @ViewType        bit = 1, -- 0 = all variants, 1 = one variant
    @StatsFirst      tinyint = 1,
    @searchstr       nvarchar(4000) = '',
    @extSearch       tinyint = 1,
    @publishedonly   tinyint = 0,
    @OnSaleOnly      tinyint = 0,
    @SearchIndex     varchar(2) = '',
    @SortOrder       varchar(4) = 'ASC', -- ASC or DESC
    @SortBy          varchar(50) = 'Name' -- name to sort by

AS
BEGIN

    SET NOCOUNT ON

    DECLARE @rcount int

    DECLARE @custlevelcount int, @sectioncount int, @affiliatecount int, @categorycount int, @distributorcount int, @genrecount int,  @vectorcount int, @manufacturercount int

    DECLARE @FilterProductsByAffiliate tinyint
    SELECT @FilterProductsByAffiliate = case ConfigValue when 'true' then 1 else 0 end FROM AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate'

    select @categorycount     = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('productcategory') and si.indid < 2 and type = 'u'
    select @sectioncount      = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('productsection') and si.indid < 2 and type = 'u'
    select @affiliatecount    = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductAffiliate') and si.indid < 2 and type = 'u'
    select @distributorcount  = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductDistributor') and si.indid < 2 and type = 'u'
    select @genrecount  = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductGenre') and si.indid < 2 and type = 'u'
    select @vectorcount  = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductVector') and si.indid < 2 and type = 'u'
    select @manufacturercount = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductManufacturer') and si.indid < 2 and type = 'u'


    SET @searchstr = '%' + @searchstr + '%'
    SET @SearchIndex = @SearchIndex + '%'


    SET @rcount = @@rowcount
    IF @StatsFirst = 1
        SELECT cast(ceiling(@rcount*1.0/1) as int) pages, @rcount ProductCount

    DECLARE @sql nvarchar(4000)
    SET @sql = '
    SELECT
        p.ProductID,
        p.Name,
        pv.VariantID,
        pv.Name AS VariantName,
        p.ProductGUID,
        p.Summary,
        p.Description,
        p.ProductTypeID,
        p.TaxClassID,
        p.SKU,
        p.ManufacturerPartNumber,
        p.XmlPackage,
        p.Published,
        p.Looks,
        p.Notes,
        p.IsAKit,
        p.ShowInProductBrowser,
        p.IsAPack,
        p.PackSize,
        p.IsSystem,
        p.Deleted,
        p.CreatedOn,
        p.ImageFileNameOverride,
        pv.VariantGUID,
        pv.Description AS VariantDescription,
        pv.SKUSuffix,
        pv.ManufacturerPartNumber AS VariantManufacturerPartNumber,
        pv.Price,
        pv.CustomerEntersPrice,
        isnull(pv.SalePrice, 0) SalePrice,
        cast(isnull(pv.Weight,0) as decimal(10,1)) Weight,
        pv.MSRP,
        pv.Cost,
        case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end Inventory,
        pv.DisplayOrder as VariantDisplayOrder,
        pv.Notes AS VariantNotes,
        pv.IsTaxable,
        pv.IsShipSeparately,
        pv.IsDownload,
        pv.DownloadLocation,
        pv.Published AS VariantPublished,
        pv.RestrictedQuantities,
        pv.MinimumQuantity,
        pv.Deleted AS VariantDeleted,
        pv.CreatedOn AS VariantCreatedOn,
        d.Name AS DistributorName,
        x.Name AS GenreName,
        x2.Name AS SHowName,
        d.DistributorID,
        x.GenreID,
        x2.VectorID,
        m.ManufacturerID,
        m.Name AS ManufacturerName'

    DECLARE @sql1 nvarchar(4000)
    SET @sql1 = '
    FROM Product p with (NOLOCK)
        join
        (
        SELECT distinct p.productid, pv.VariantID
            FROM
                product p with (nolock)
                left join ProductVariant pv             with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= '

    SET @sql1 = @sql1 +CONVERT(nvarchar,@ViewType)+'
                left join productcategory pc       with (nolock) on p.ProductID = pc.ProductID
                left join productsection ps        with (nolock) on p.ProductID = ps.ProductID
                left join ProductManufacturer pm   with (nolock) on p.ProductID = pm.ProductID
                left join ProductDistributor pd    with (nolock) on p.ProductID = pd.ProductID
                left join ProductGenre px    with (nolock) on p.ProductID = px.ProductID
                left join ProductVector px2    with (nolock) on p.ProductID = px2.ProductID
                left join ProductAffiliate pa      with (nolock) on p.ProductID = pa.ProductID
            WHERE
                  (pc.categoryid = '


    DECLARE @sql2 nvarchar(4000)

    SET @sql2 = ' ' + CONVERT(nvarchar,@categoryID) + ') or (ps.sectionid = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@sectionID) + ') or (pa.AffiliateID = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@affiliateID) + ') or (pm.manufacturerid = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@manufacturerID) + ') or (pd.DistributorID = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@distributorID) + ') or (px.genreID = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@genreID) + ') or (px2.vectorID = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@vectorID) + ')'

        DECLARE @sql3 nvarchar(4000)
        SET @sql3 = '
          and case when isnull(pv.saleprice,0) = 0 then 0 else 1 end >= '+CONVERT(nvarchar,@OnSaleOnly)+'
          and p.published >= '+CONVERT(nvarchar,@publishedonly)+'
          and pv.published >= '+CONVERT(nvarchar,@publishedonly)+'
          and p.Deleted = 0
          and pv.Deleted = 0
        )                              pf on p.ProductID = pf.ProductID
        left join ProductVariant      pv  with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= '

    SET @sql3 = @sql3 + CONVERT(nvarchar,@ViewType)+'
        left join ProductManufacturer pm  with (NOLOCK) on p.ProductID = pm.ProductID
        left join Manufacturer         m  with (NOLOCK) on pm.ManufacturerID = m.ManufacturerID
        left join ProductDistributor  pd  with (NOLOCK) on p.ProductID = pd.ProductID
        left join ProductGenre  px  with (NOLOCK) on p.ProductID = px.ProductID
        left join ProductVector  px2  with (NOLOCK) on p.ProductID = px2.ProductID
        left join Distributor          d  with (NOLOCK) on pd.DistributorID = d.DistributorID
        left join Genre          x  with (NOLOCK) on px.GenreID = x.GenreID
        left join Vector          x2  with (NOLOCK) on px2.VectorID = x2.VectorID
        left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i  on pv.VariantID = i.VariantID
    WHERE
        (p.ProductTypeID = '+CONVERT(nvarchar,@ProductTypeID)+ ' or  '+CONVERT(nvarchar,@ProductTypeID)+ ' = 0) and
        (
          p.Name LIKE '''+ @searchstr + '''
          or convert(nvarchar(20),p.productid) LIKE '''+ @searchstr + '''
          or pv.name LIKE '''+ @searchstr + '''
          or p.sku LIKE '''+ @searchstr + '''
          or p.manufacturerpartnumber LIKE '''+ @searchstr + '''
          or pv.manufacturerpartnumber LIKE '''+ @searchstr + '''
          or ('+CONVERT(nvarchar,@extSearch)+' = 1 AND p.Description LIKE '''+ @searchstr + ''')
          or ('+CONVERT(nvarchar,@extSearch)+' = 1 AND p.Summary LIKE '''+ @searchstr + ''')
        )
        and
        p.Name LIKE '''+ @SearchIndex + '''
    ORDER BY '

DECLARE @sql4 nvarchar(4000)

    IF @SortBy = 'ProductID'
        SET @sql4 = 'P.ProductID'
    ELSE IF @SortBy = 'SKU'
        SET @sql4 = 'SKU'
    ELSE IF @SortBy = 'ManufacturerPartNumber'
        SET @sql4 = 'P.ManufacturerPartNumber'
    ELSE IF @SortBy = 'Inventory'
        SET @sql4 = 'Inventory'
    ELSE
        SET @sql4 = 'P.[Name]'


    IF @SortOrder = 'DESC'
        SET @sql4 = @sql4 + ' DESC'
    ELSE
        SET @sql4 = @sql4 + ' ASC'

    SET @sql4 = @sql4 + ', pv.DisplayOrder'

    EXECUTE(@sql+' '+@sql1+' '+@sql2+' '+@sql3+' '+@sql4)

    IF @StatsFirst <> 1
        SELECT cast(ceiling(@rcount*1.0/1) as int) pages, @rcount ProductCount

END
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_CreateMissingVariants') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_CreateMissingVariants]
GO

create proc [dbo].[aspdnsf_CreateMissingVariants]
AS
BEGIN
SET NOCOUNT ON
INSERT [dbo].ProductVariant (VariantGUID, IsDefault, Name, ProductID, Price, SalePrice, Inventory,
                           DisplayOrder, IsTaxable, IsShipSeparately, IsDownload, FreeShipping,
                           Published, Wholesale, IsSecureAttachment, IsRecurring, RecurringInterval,
                           RecurringIntervalType, SEName, IsImport, Deleted, CreatedOn, CustomerEntersPrice)
SELECT newid(), 1, '', p.ProductID, 0, 0, 100000000,
       1, 0, 0, 0, 0,
       1, 0, 0, 0, 0,
       0, '', 0, 0, getdate(), 0
FROM dbo.Product p with (nolock)
    left join [dbo].ProductVariant pv with (nolock) on p.ProductID = pv.ProductID WHERE pv.ProductID is null
END
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_CreateDefaultVariant') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_CreateDefaultVariant]
GO

create proc [dbo].[aspdnsf_CreateDefaultVariant]
AS
SET NOCOUNT ON
INSERT [dbo].ProductVariant (VariantGUID, IsDefault, Name, ProductID, Price, SalePrice, Inventory,
                       DisplayOrder, IsTaxable, IsShipSeparately, IsDownload, FreeShipping,
                       Published, Wholesale, IsSecureAttachment, IsRecurring, RecurringInterval,
                       RecurringIntervalType, SEName, IsImport, Deleted, CreatedOn, CustomerEntersPrice)
SELECT newid(), 1, '', p.ProductID, 0, 0, 100000000,
       1, 0, 0, 0, 0,
       1, 0, 0, 0, 0,
       0, '', 0, 0, getdate(), 0
FROM dbo.Product p with (nolock)
    left join dbo.ProductVariant pv with (nolock) on p.ProductID = pv.ProductID
WHERE pv.ProductID is null
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Customer]') AND name = 'SubscriptionExpiresOn')
	BEGIN
		ALTER TABLE [dbo].[Customer] DROP COLUMN SubscriptionExpiresOn
	END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[ShoppingCart]') AND name = 'SubscriptionIntervalType')
	BEGIN
		ALTER TABLE [dbo].[ShoppingCart] DROP CONSTRAINT DF_ShoppingCart_SubscriptionIntervalType
		ALTER TABLE [dbo].[ShoppingCart] DROP COLUMN SubscriptionIntervalType
	END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[ShoppingCart]') AND name = 'SubscriptionInterval')
	BEGIN
		ALTER TABLE [dbo].[ShoppingCart] DROP COLUMN SubscriptionInterval
	END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Orders_ShoppingCart]') AND name = 'SubscriptionIntervalType')
	BEGIN
		ALTER TABLE [dbo].[Orders_ShoppingCart] DROP CONSTRAINT DF_Orders_ShoppingCart_SubscriptionIntervalType
		ALTER TABLE [dbo].[Orders_ShoppingCart] DROP COLUMN SubscriptionIntervalType
	END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Orders_ShoppingCart]') AND name = 'SubscriptionInterval')
	BEGIN
		ALTER TABLE [dbo].[Orders_ShoppingCart] DROP COLUMN SubscriptionInterval
	END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[ProductVariant]') AND name = 'SubscriptionIntervalType')
	BEGIN
		ALTER TABLE [dbo].[ProductVariant] DROP CONSTRAINT DF_ProductVariant_SubscriptionIntervalType
		ALTER TABLE [dbo].[ProductVariant] DROP COLUMN SubscriptionIntervalType
	END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[ProductVariant]') AND name = 'SubscriptionInterval')
	BEGIN
		ALTER TABLE [dbo].[ProductVariant] DROP COLUMN SubscriptionInterval
	END
GO

IF EXISTS (select * from sys.objects where name = 'DF_Topic_RequiresSubscription' AND parent_object_id = OBJECT_ID(N'[dbo].Topic'))
	BEGIN
		ALTER TABLE [dbo].[Topic] DROP CONSTRAINT DF_Topic_RequiresSubscription
	END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].Topic') AND name = 'RequiresSubscription')
	BEGIN
		ALTER TABLE [dbo].[Topic] DROP COLUMN RequiresSubscription
	END
GO

--Display AppConfig Cleanup
DELETE FROM AppConfig WHERE Name = 'Admin.EntityFrameMenuWidth'
DELETE FROM AppConfig WHERE Name = 'Admin.ShowSecurityFeed'
DELETE FROM AppConfig WHERE Name = 'AdminAlert.BackgroundColor'
DELETE FROM AppConfig WHERE Name = 'AdminAlert.FontColor'
DELETE FROM AppConfig WHERE Name = 'AlsoBoughtProductsFormat'
DELETE FROM AppConfig WHERE Name = 'BoxFrameStyle'
DELETE FROM AppConfig WHERE Name = 'ContentsBGColorDefault'
DELETE FROM AppConfig WHERE Name = 'DefaultSkinId'
DELETE FROM AppConfig WHERE Name = 'GraphicsColorDefault'
DELETE FROM AppConfig WHERE Name = 'GreyCellColor'
DELETE FROM AppConfig WHERE Name = 'HeaderBGColor'
DELETE FROM AppConfig WHERE Name = 'LightCellColor'
DELETE FROM AppConfig WHERE Name = 'MaxSlides'
DELETE FROM AppConfig WHERE Name = 'MediumCellColor'
DELETE FROM AppConfig WHERE Name = 'OnSaleForTextColor'
DELETE FROM AppConfig WHERE Name = 'PageBGColorDefault'
DELETE FROM AppConfig WHERE Name = 'ProductBrowserHeight'
DELETE FROM AppConfig WHERE Name = 'ProductBrowserHoverColor'
DELETE FROM AppConfig WHERE Name = 'ResizeSlideWindow'
DELETE FROM AppConfig WHERE Name = 'SE_MetaNoScript'
DELETE FROM AppConfig WHERE Name = 'ShoppingCartItemNotesTextareaCols'
DELETE FROM AppConfig WHERE Name = 'ShoppingCartItemNotesTextareaRows'
DELETE FROM AppConfig WHERE Name = 'ShowEditButtonInCartForPackProducts'
DELETE FROM AppConfig WHERE Name = 'ShowKitPics'
DELETE FROM AppConfig WHERE Name = 'ShowManufacturerTree'
DELETE FROM AppConfig WHERE Name = 'SlideShowInterval'
DELETE FROM AppConfig WHERE Name = 'Tree.CustomerServiceXml'
DELETE FROM AppConfig WHERE Name = 'Tree.ShowCategories'
DELETE FROM AppConfig WHERE Name = 'Tree.ShowCustomerService'
DELETE FROM AppConfig WHERE Name = 'Tree.ShowGenres'
DELETE FROM AppConfig WHERE Name = 'Tree.ShowLibraries'
DELETE FROM AppConfig WHERE Name = 'Tree.ShowManufacturers'
DELETE FROM AppConfig WHERE Name = 'Tree.ShowSections'
DELETE FROM AppConfig WHERE Name = 'Tree.ShowVectors'
DELETE FROM AppConfig WHERE Name = 'UseNameForSectionDescriptionName'
DELETE FROM AppConfig WHERE Name = 'UseParserOnObjectDescriptions'
DELETE FROM AppConfig WHERE Name = 'UseSKUForProductDescriptionName'
DELETE FROM AppConfig WHERE Name = 'Admin.ShowSponsorFeed'
DELETE FROM AppConfig WHERE Name = 'Admin.ShowNewsFeed'

--General AppConfig cleanup
DELETE FROM AppConfig WHERE Name = 'ActiveQuantityDiscountTable'
DELETE FROM AppConfig WHERE Name = 'Admin_SpecsInlineByDefault'
DELETE FROM AppConfig WHERE Name = 'AgeWishListDays'
DELETE FROM AppConfig WHERE Name = 'AutoVariantFillColumnDelimiter'
DELETE FROM AppConfig WHERE Name = 'CardinalCommerce.Centinel.Version.Authenticate'
DELETE FROM AppConfig WHERE Name = 'CardinalCommerce.Centinel.Version.Lookup'
DELETE FROM AppConfig WHERE Name = 'DelayedDownloads'
DELETE FROM AppConfig WHERE Name = 'DisallowCoupons'
DELETE FROM AppConfig WHERE Name = 'DumpDownloadInfo'
DELETE FROM AppConfig WHERE Name = 'EntitySelectLists.Enabled'
DELETE FROM AppConfig WHERE Name = 'Google.DeprecatedEcomTokens.Enabled'
DELETE FROM AppConfig WHERE Name = 'Innova.XHTML'
DELETE FROM AppConfig WHERE Name = 'InternationalCheckout.ForceForInternationalCustomers'
DELETE FROM AppConfig WHERE Name = 'IPAddress.RefuseRestrictedIPsFromSite'
DELETE FROM AppConfig WHERE Name = 'MailCheckReminder'
DELETE FROM AppConfig WHERE Name = 'MultiColorMakesSwatchAndMap'
DELETE FROM AppConfig WHERE Name = 'OrderSummaryReportFields'
DELETE FROM AppConfig WHERE Name = 'PaymentExpress.Password'
DELETE FROM AppConfig WHERE Name = 'PaymentExpress.Url'
DELETE FROM AppConfig WHERE Name = 'PaymentExpress.Username'
DELETE FROM AppConfig WHERE Name = 'PayPal.Ads.BannerEnrollmentService.LiveURL'
DELETE FROM AppConfig WHERE Name = 'PayPal.Ads.BannerEnrollmentService.SandboxURL'
DELETE FROM AppConfig WHERE Name = 'PPayPal.API.RefundVersion'
DELETE FROM AppConfig WHERE Name = 'ProductImg_swatch'
DELETE FROM AppConfig WHERE Name = 'ReceiptHideStoreVersion'
DELETE FROM AppConfig WHERE Name = 'Recurring.DefaultRecurringShippingMethod'
DELETE FROM AppConfig WHERE Name = 'Recurring.DefaultRecurringShippingMethodID'
DELETE FROM AppConfig WHERE Name = 'SearchAdv_ShowGenre'
DELETE FROM AppConfig WHERE Name = 'Search_ShowGenresInResults'
DELETE FROM AppConfig WHERE Name = 'SectionImg_medium'
DELETE FROM AppConfig WHERE Name = 'ShippingCostWhenNoZoneMatch'
DELETE FROM AppConfig WHERE Name = 'ShippingMethodIDIfItemShippingCostsAreOn'
DELETE FROM AppConfig WHERE Name = 'SwatchStyleAuto'
DELETE FROM AppConfig WHERE Name = 'UseColorSwatchIcons'
DELETE FROM AppConfig WHERE Name = 'VAT.HideTaxInOrderSummary'
DELETE FROM AppConfig WHERE Name = 'FilterDocumentsByAffiliate'
DELETE FROM AppConfig WHERE Name = 'FilterDocumentsByCustomerLevel'
DELETE FROM AppConfig WHERE Name = 'EventLoggingEnabled'
DELETE FROM AppConfig WHERE Name = 'Admin_ProductPageSize'
DELETE FROM AppConfig WHERE Name = 'Admin_TextAreaHeight'
DELETE FROM AppConfig WHERE Name = 'Admin_TextAreaHeightSmall'
DELETE FROM AppConfig WHERE Name = 'Admin_TextAreaWidth'

--Deprecated gateway cleanup
DELETE FROM AppConfig WHERE Name = 'CardiaServices.Live.MerchantToken'
DELETE FROM AppConfig WHERE Name = 'CardiaServices.Live.UserToken'
DELETE FROM AppConfig WHERE Name = 'CardiaServices.SOAPURL'
DELETE FROM AppConfig WHERE Name = 'CardiaServices.Test.MerchantToken'
DELETE FROM AppConfig WHERE Name = 'CardiaServices.Test.UserToken'
DELETE FROM AppConfig WHERE Name = 'CENTRALPAYMENTS_AssociateName'
DELETE FROM AppConfig WHERE Name = 'CENTRALPAYMENTS_AssociatePassword'
DELETE FROM AppConfig WHERE Name = 'EFSNET_LIVE_SERVER'
DELETE FROM AppConfig WHERE Name = 'EFSNET_LIVE_STOREID'
DELETE FROM AppConfig WHERE Name = 'EFSNET_LIVE_STOREKEY'
DELETE FROM AppConfig WHERE Name = 'EFSNET_TEST_SERVER'
DELETE FROM AppConfig WHERE Name = 'EFSNET_TEST_STOREID'
DELETE FROM AppConfig WHERE Name = 'EFSNET_TEST_STOREKEY'
DELETE FROM AppConfig WHERE Name = 'EFSNET_VERIFY_ADDRESSES'
DELETE FROM AppConfig WHERE Name = 'eWay.Live.CustomerID'
DELETE FROM AppConfig WHERE Name = 'eWay.Live.URL'
DELETE FROM AppConfig WHERE Name = 'eWay.Test.CustomerID'
DELETE FROM AppConfig WHERE Name = 'eWay.Test.URL'
DELETE FROM AppConfig WHERE Name = 'HSBC.CcpaClientID'
DELETE FROM AppConfig WHERE Name = 'HSBC.CcpaURL'
DELETE FROM AppConfig WHERE Name = 'HSBC.ClientAlias'
DELETE FROM AppConfig WHERE Name = 'HSBC.ClientID'
DELETE FROM AppConfig WHERE Name = 'HSBC.ClientName'
DELETE FROM AppConfig WHERE Name = 'HSBC.ClientPassword'
DELETE FROM AppConfig WHERE Name = 'HSBC.DocVersion'
DELETE FROM AppConfig WHERE Name = 'HSBC.Live.Server'
DELETE FROM AppConfig WHERE Name = 'HSBC.Mode.Live'
DELETE FROM AppConfig WHERE Name = 'HSBC.Mode.Test'
DELETE FROM AppConfig WHERE Name = 'HSBC.Pipeline'
DELETE FROM AppConfig WHERE Name = 'HSBC.Test.Server'
DELETE FROM AppConfig WHERE Name = 'IATS.AgentCode'
DELETE FROM AppConfig WHERE Name = 'IATS.Password'
DELETE FROM AppConfig WHERE Name = 'IATS.URL'
DELETE FROM AppConfig WHERE Name = 'IDEPOSIT_LIVE_CLERKID'
DELETE FROM AppConfig WHERE Name = 'IDEPOSIT_LIVE_PASSWORD'
DELETE FROM AppConfig WHERE Name = 'IDEPOSIT_LIVE_URL'
DELETE FROM AppConfig WHERE Name = 'IDEPOSIT_LIVE_USERNAME'
DELETE FROM AppConfig WHERE Name = 'IDEPOSIT_TEST_CLERKID'
DELETE FROM AppConfig WHERE Name = 'IDEPOSIT_TEST_PASSWORD'
DELETE FROM AppConfig WHERE Name = 'IDEPOSIT_TEST_URL'
DELETE FROM AppConfig WHERE Name = 'IDEPOSIT_TEST_USERNAME'
DELETE FROM AppConfig WHERE Name = 'ITransact.Password'
DELETE FROM AppConfig WHERE Name = 'ITransact.Sale_Server'
DELETE FROM AppConfig WHERE Name = 'ITransact.Test_FirstName'
DELETE FROM AppConfig WHERE Name = 'ITransact.Vendor_ID'
DELETE FROM AppConfig WHERE Name = 'ITransact.Verify_Addresses'
DELETE FROM AppConfig WHERE Name = 'ITransact.VoidRefund_Server'
DELETE FROM AppConfig WHERE Name = 'JETPAY_LIVE_SERVER'
DELETE FROM AppConfig WHERE Name = 'JETPAY_MERCHANTID'
DELETE FROM AppConfig WHERE Name = 'JETPAY_TEST_SERVER'
DELETE FROM AppConfig WHERE Name = 'LINKPOINT_CONFIGFILE'
DELETE FROM AppConfig WHERE Name = 'LINKPOINT_KEYFILE'
DELETE FROM AppConfig WHERE Name = 'LINKPOINT_LIVE_SERVER'
DELETE FROM AppConfig WHERE Name = 'LINKPOINT_PORT'
DELETE FROM AppConfig WHERE Name = 'LINKPOINT_TEST_SERVER'
DELETE FROM AppConfig WHERE Name = 'LINKPOINT_Verify_Addresses'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.Acquirer'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.Acquirer.AIB.AcceptedCardTypes'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.Acquirer.EMS.AcceptedCardTypes'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.Acquirer.EPX.AcceptedCardTypes'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.ChannelID'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.LiveServer'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.LiveServer.3DSecure'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.QuickCheckout.HideLogin'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.QuickCheckout.PaymentIcon'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.SenderID'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.TestServer'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.TestServer.3DSecure'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.UserLogin'
DELETE FROM AppConfig WHERE Name = 'Moneybookers.UserPassword'
DELETE FROM AppConfig WHERE Name = 'NETAXEPT.Error.Setup'
DELETE FROM AppConfig WHERE Name = 'NETAXEPT.Live_Server'
DELETE FROM AppConfig WHERE Name = 'NETAXEPT.Live_UI'
DELETE FROM AppConfig WHERE Name = 'NETAXEPT.MerchantSettings.RedirectUrl'
DELETE FROM AppConfig WHERE Name = 'NETAXEPT.Merchant_Id'
DELETE FROM AppConfig WHERE Name = 'NETAXEPT.Merchant_Token'
DELETE FROM AppConfig WHERE Name = 'NETAXEPT.Test_Server'
DELETE FROM AppConfig WHERE Name = 'NETAXEPT.Test_UI'
DELETE FROM AppConfig WHERE Name = 'NetBilling.Account_ID'
DELETE FROM AppConfig WHERE Name = 'NetBilling.Live_Server'
DELETE FROM AppConfig WHERE Name = 'NetBilling.Pay_Type'
DELETE FROM AppConfig WHERE Name = 'NetBilling.Site_Tag'
DELETE FROM AppConfig WHERE Name = 'NetBilling.Test_Server'
DELETE FROM AppConfig WHERE Name = 'NetBilling.Verify_Addresses'
DELETE FROM AppConfig WHERE Name = 'Netgiro.HostName'
DELETE FROM AppConfig WHERE Name = 'Netgiro.HostPort'
DELETE FROM AppConfig WHERE Name = 'Netgiro.MerchantCertificatePassword'
DELETE FROM AppConfig WHERE Name = 'Netgiro.MerchantID'
DELETE FROM AppConfig WHERE Name = 'Netgiro.NetgiroCertificatePassword'
DELETE FROM AppConfig WHERE Name = 'Netgiro.URL'
DELETE FROM AppConfig WHERE Name = 'Ogone.LivePostURL'
DELETE FROM AppConfig WHERE Name = 'Ogone.LiveServer'
DELETE FROM AppConfig WHERE Name = 'Ogone.LiveServerOrder'
DELETE FROM AppConfig WHERE Name = 'Ogone.PSPID'
DELETE FROM AppConfig WHERE Name = 'Ogone.PSWD'
DELETE FROM AppConfig WHERE Name = 'Ogone.SHASignature'
DELETE FROM AppConfig WHERE Name = 'Ogone.TestPostURL'
DELETE FROM AppConfig WHERE Name = 'Ogone.TestServer'
DELETE FROM AppConfig WHERE Name = 'Ogone.TestServerOrder'
DELETE FROM AppConfig WHERE Name = 'Ogone.Use3TierMode'
DELETE FROM AppConfig WHERE Name = 'Ogone.USERID'
DELETE FROM AppConfig WHERE Name = 'PayFuse.Alias'
DELETE FROM AppConfig WHERE Name = 'PayFuse.Live_Server'
DELETE FROM AppConfig WHERE Name = 'PayFuse.Password'
DELETE FROM AppConfig WHERE Name = 'PayFuse.Test_Server'
DELETE FROM AppConfig WHERE Name = 'PayFuse.UserID'
DELETE FROM AppConfig WHERE Name = 'PAYJUNCTION_HTTP_VERSION'
DELETE FROM AppConfig WHERE Name = 'PAYJUNCTION_LIVE_SERVER'
DELETE FROM AppConfig WHERE Name = 'PAYJUNCTION_LOGON'
DELETE FROM AppConfig WHERE Name = 'PAYJUNCTION_PASSWORD'
DELETE FROM AppConfig WHERE Name = 'PAYJUNCTION_SECURITY_DESCRIPTOR'
DELETE FROM AppConfig WHERE Name = 'PAYJUNCTION_TEST_SERVER'
DELETE FROM AppConfig WHERE Name = 'Paymentech.UseVerifiedByVisa'
DELETE FROM AppConfig WHERE Name = 'PAYMENTECH_BIN'
DELETE FROM AppConfig WHERE Name = 'PAYMENTECH_LIVE_SERVER'
DELETE FROM AppConfig WHERE Name = 'PAYMENTECH_MERCHANT_ID'
DELETE FROM AppConfig WHERE Name = 'PAYMENTECH_MERCHANT_TERMINAL_ID'
DELETE FROM AppConfig WHERE Name = 'PAYMENTECH_MERCHANT_TZCODE'
DELETE FROM AppConfig WHERE Name = 'PAYMENTECH_TEST_SERVER'
DELETE FROM AppConfig WHERE Name = 'PAYMENTECH_Verify_Addresses'
DELETE FROM AppConfig WHERE Name = 'PaymentExpress.Password'
DELETE FROM AppConfig WHERE Name = 'PaymentExpress.Url'
DELETE FROM AppConfig WHERE Name = 'PaymentExpress.Username'
DELETE FROM AppConfig WHERE Name = 'PinnaclePayments.Password'
DELETE FROM AppConfig WHERE Name = 'PinnaclePayments.SOAPURL'
DELETE FROM AppConfig WHERE Name = 'PinnaclePayments.UserName'
DELETE FROM AppConfig WHERE Name = 'PlugNPay_Password'
DELETE FROM AppConfig WHERE Name = 'PlugNPay_URL'
DELETE FROM AppConfig WHERE Name = 'PlugNPay_Username'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_LIVE_SERVER'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_TEST_SERVER'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_Verify_Addresses'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_X_DELIM_CHAR'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_X_DELIM_DATA'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_X_EMAIL'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_X_EMAIL_CUSTOMER'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_X_ENCAP_CHAR'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_X_Login'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_X_METHOD'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_X_RECURRING_BILLING'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_X_RELAY_RESPONSE'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_X_Tran_Key'
DELETE FROM AppConfig WHERE Name = 'QUICKCOMMERCE_X_VERSION'
DELETE FROM AppConfig WHERE Name = 'SecureNet.LiveURL'
DELETE FROM AppConfig WHERE Name = 'SecureNet.TestURL'
DELETE FROM AppConfig WHERE Name = 'TRANSACTIONCENTRAL_MerchantID'
DELETE FROM AppConfig WHERE Name = 'TRANSACTIONCENTRAL_RegKey'
DELETE FROM AppConfig WHERE Name = 'TRANSACTIONCENTRAL_LIVE_SERVER'
DELETE FROM AppConfig WHERE Name = 'TRANSACTIONCENTRAL_TEST_SERVER'
DELETE FROM AppConfig WHERE Name = 'TRANSACTIONCENTRAL_VOID_SERVER'
DELETE FROM AppConfig WHERE Name = 'VIAKLIX_LIVE_SERVER'
DELETE FROM AppConfig WHERE Name = 'VIAKLIX_SSL_MERCHANT_ID'
DELETE FROM AppConfig WHERE Name = 'VIAKLIX_SSL_PIN'
DELETE FROM AppConfig WHERE Name = 'VIAKLIX_SSL_USER_ID'
DELETE FROM AppConfig WHERE Name = 'VIAKLIX_TEST_SERVER'
DELETE FROM AppConfig WHERE Name = 'WorldPay_FixContact'
DELETE FROM AppConfig WHERE Name = 'WorldPay_HideContact'
DELETE FROM AppConfig WHERE Name = 'WorldPay_InstallationID'
DELETE FROM AppConfig WHERE Name = 'WorldPay_LanguageLocale'
DELETE FROM AppConfig WHERE Name = 'WorldPay_Live_Server'
DELETE FROM AppConfig WHERE Name = 'WorldPay_OnCancelAutoRedirectToCart'
DELETE FROM AppConfig WHERE Name = 'WorldPay_RequireAVSMatch'
DELETE FROM AppConfig WHERE Name = 'WorldPay_ReturnURL'
DELETE FROM AppConfig WHERE Name = 'WorldPay_TestMode'
DELETE FROM AppConfig WHERE Name = 'WorldPay_TestModeCode'
DELETE FROM AppConfig WHERE Name = 'YOURPAY_CONFIGFILE'
DELETE FROM AppConfig WHERE Name = 'YOURPAY_KEYFILE'
DELETE FROM AppConfig WHERE Name = 'YOURPAY_LIVE_SERVER'
DELETE FROM AppConfig WHERE Name = 'YOURPAY_PORT'
DELETE FROM AppConfig WHERE Name = 'YOURPAY_TEST_SERVER'
DELETE FROM AppConfig WHERE Name = 'YOURPAY_Verify_Addresses'
DELETE FROM AppConfig WHERE Name LIKE '%PayLeap%'
DELETE FROM AppConfig WHERE Name LIKE '%Verisign%'

--Inventory Control appconfig cleanup
DELETE FROM AppConfig WHERE Name = 'KitInventory.ShowStockHint'
DELETE FROM AppConfig WHERE Name = 'KitInventory.DisableItemSelection'
DELETE FROM AppConfig WHERE Name = 'KitInventory.HideOutOfStock'

--GlobalConfig cleanup
DELETE FROM GlobalConfig WHERE Name = 'SSLRededirectURL'
DELETE FROM GlobalConfig WHERE Name = 'LicenseKey'
DELETE FROM GlobalConfig WHERE Name = 'UseSharedSSL'
DELETE FROM GlobalConfig WHERE Name = 'AllowRatingFiltering'
DELETE FROM GlobalConfig WHERE Name = 'DefaultRedirectURL'

--Shipwire cleanup
DELETE FROM AppConfig WHERE Name = 'Shipwire.Username'
DELETE FROM AppConfig WHERE Name = 'Shipwire.Password'

--Verisign Cleanup
UPDATE [dbo].AppConfig SET Description = 'if true, the gateway''s internal billing will be used instead of our own build order billing mechanism when processing recurring orders. This is ONLY allowed to be true if you are using the Authorize.net or PayFlow PRO gateways!! If using those gateways, setting this flag to true allows you to not have to store credit cards in your db. See manual for further instructions on how to process the recurring order reports using each gateway.' WHERE Name = 'Recurring.UseGatewayInternalBilling'
    GO

--Convert any old Verisign orders to PayFlowPro.  This should only affect sites upgrading from a very old version straight to this one.
UPDATE [dbo].AppConfig SET ConfigValue = 'PAYFLOWPRO' WHERE Name = 'PaymentGateway' AND ConfigValue = 'VERISIGN'
    GO
UPDATE [dbo].Orders SET PaymentGateway = 'PAYFLOWPRO' WHERE PaymentGateway = 'VERISIGN'
	GO

--Poll cleanup
DELETE FROM AppConfig WHERE Name = 'Polls.Enabled'
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[Poll]') and type = 'u')
DROP TABLE [dbo].[Poll]

GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[PollAnswer]') and type = 'u')
DROP TABLE [dbo].[PollAnswer]

GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[PollCategory]') and type = 'u')
DROP TABLE [dbo].[PollCategory]

GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[PollSection]') and type = 'u')
DROP TABLE [dbo].[PollSection]

GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[PollSortOrder]') and type = 'u')
DROP TABLE [dbo].[PollSortOrder]

GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[PollStore]') and type = 'u')
DROP TABLE [dbo].[PollStore]

GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[PollVotingRecord]') and type = 'u')
DROP TABLE [dbo].[PollVotingRecord]

GO


--Order Line Item Notes Cleanup
DELETE FROM AppConfig WHERE Name = 'AllowShoppingCartItemNotes'

GO

--Remove mailing manager
IF EXISTS (select * from dbo.sysobjects where id = object_id('aspdnsf_insMailingMgrLog') and OBJECTPROPERTY(id, 'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insMailingMgrLog]
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id('aspdnsf_getMailingList') and OBJECTPROPERTY(id, 'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_getMailingList]
GO

--Remove newsletters
DELETE FROM AppConfig WHERE Name = 'Newsletter.CaptchaErrorDisplayLength'
DELETE FROM AppConfig WHERE Name = 'Newsletter.GetFirstAndLast'
DELETE FROM AppConfig WHERE Name = 'Newsletter.OptInLevel'
DELETE FROM AppConfig WHERE Name = 'Newsletter.OptOutLevel'
DELETE FROM AppConfig WHERE Name = 'Newsletter.UseCaptcha'

UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'NewsletterEmail'
UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'NewsletterOptInEmail'
UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'Newsletter.OptInOutBadRequest'
UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'Newsletter.ConfirmOptOut'
UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'Newsletter.ConfirmOptIn'
UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'Newsletter.AddressErrorBlock'
UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'Newsletter.CaptchaErrorBlock'
UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'Newsletter.SubscribeSuccessful'
UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'Newsletter.SubscribeConfirm'
UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'Newsletter.UnsubscribeSuccessful'

--Remove Cardinal MyeCheck
UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'CardinalMyECheckPageHeader'
UPDATE Topic SET Published = 0, IsFrequent = 0 WHERE Name = 'CardinalMyECheckExplanation'
DELETE FROM AppConfig WHERE Name = 'CardinalCommerce.Centinel.MyECheckMarkAsCaptured'
UPDATE AppConfig SET ConfigValue = REPLACE(ConfigValue, 'CardinalMyECheck', '') WHERE Name = 'PaymentMethods'
UPDATE AppConfig SET AllowableValues = REPLACE(AllowableValues, 'CardinalMyECheck,', '') WHERE Name = 'PaymentMethods'
UPDATE AppConfig SET AllowableValues = REPLACE(AllowableValues, 'CardinalMyECheck ,', '') WHERE Name = 'PaymentMethods'

--Remove recently viewed products
DELETE FROM AppConfig WHERE Name = 'RecentlyViewedProducts.Enabled'
DELETE FROM AppConfig WHERE Name = 'RecentlyViewedProducts.NumberDisplayed'
DELETE FROM AppConfig WHERE Name = 'RecentlyViewedProducts.ProductsFormat'
DELETE FROM AppConfig WHERE Name = 'RecentlyViewedProductsGridColWidth'

IF EXISTS (select * from dbo.sysobjects where id = object_id('aspdnsf_GetRecentlyViewedProducts') and OBJECTPROPERTY(id, 'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetRecentlyViewedProducts]
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_CloneProduct') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_CloneProduct]
GO

CREATE proc [dbo].[aspdnsf_CloneProduct]
    @productID int,
    @userid int = 0,
    @cloneinventory int = 1

AS
BEGIN

SET NOCOUNT ON

DECLARE @tmpKitTbl TABLE(KitGroupID int not null)
DECLARE @tmpPVariantTbl TABLE(VariantID int not null)
DECLARE @newproductID int
DECLARE @err int, @newkitgroupid int

SET @newproductID = -1

BEGIN TRAN
    INSERT [dbo].product (ProductGUID, Name, Summary, Description, SEKeywords, SEDescription, MiscText, FroogleDescription, SETitle, SEAltText, SizeOptionPrompt, ColorOptionPrompt, TextOptionPrompt, ProductTypeID, TaxClassID, SKU, ManufacturerPartNumber, SalesPromptID, XmlPackage, ColWidth, Published, RequiresRegistration, Looks, Notes, QuantityDiscountID, RelatedProducts, UpsellProducts, UpsellProductDiscountPercentage, RelatedDocuments, TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor, IsAKit, ShowInProductBrowser, IsAPack, PackSize, ShowBuyButton, RequiresProducts, HidePriceUntilCart, IsCalltoOrder, ExcludeFromPriceFeeds, RequiresTextOption, TextOptionMaxLength, SEName, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
    SELECT newid(), Name + ' - CLONED', Summary, Description, SEKeywords, SEDescription, MiscText, FroogleDescription, SETitle, SEAltText, SizeOptionPrompt, ColorOptionPrompt, TextOptionPrompt, ProductTypeID, TaxClassID, SKU, ManufacturerPartNumber, SalesPromptID, XmlPackage, ColWidth, 0, RequiresRegistration, 0, Notes, QuantityDiscountID, RelatedProducts, UpsellProducts, UpsellProductDiscountPercentage, RelatedDocuments, TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor, IsAKit, ShowInProductBrowser, IsAPack, PackSize, ShowBuyButton, RequiresProducts, HidePriceUntilCart, IsCalltoOrder, ExcludeFromPriceFeeds, RequiresTextOption, TextOptionMaxLength, SEName, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, getdate()
    FROM dbo.product
    WHERE productid = @productID

    SELECT @newproductID = @@identity, @err = @@error

    IF @err <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -1
    END

    IF @cloneinventory = 1 BEGIN
        DECLARE @PrdVariantID int, @newvariantID int
        INSERT @tmpPVariantTbl SELECT VariantID FROM dbo.productvariant  WHERE productid = @productID
        SELECT top 1 @PrdVariantID = VariantID FROM @tmpPVariantTbl
        WHILE @@rowcount <> 0 BEGIN

            INSERT [dbo].productvariant (VariantGUID, ProductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
            SELECT newid(), @newproductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, getdate()
            FROM dbo.productvariant
            WHERE VariantID = @PrdVariantID

            SELECT @newvariantID = @@identity, @err = @@error

            IF @err <> 0 BEGIN
                raiserror('Product not cloned', 1, 16)
                rollback tran
                return -2
            END


            INSERT [dbo].Inventory (InventoryGUID, VariantID, Color, Size, Quan, CreatedOn)
            SELECT newid(), @newvariantID, Color, Size, Quan, getdate()
            FROM dbo.Inventory
            WHERE VariantID = @PrdVariantID

            IF @@error <> 0 BEGIN
                raiserror('Product not cloned', 1, 16)
                rollback tran
                return -14
            END

            DELETE @tmpPVariantTbl where VariantID = @PrdVariantID
            SELECT top 1 @PrdVariantID = VariantID from @tmpPVariantTbl
        END
    END
    ELSE BEGIN

        INSERT [dbo].productvariant (VariantGUID, ProductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
        SELECT newid(), @newproductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, getdate()
        FROM dbo.productvariant
        WHERE productid = @productID

        IF @@error <> 0 BEGIN
            raiserror('Product not cloned', 1, 16)
            rollback tran
            return -2
        END

    END


    DECLARE @kitgrpid int
    INSERT @tmpKitTbl select KitGroupID FROM kitgroup  where productid = @productID
    SELECT top 1 @kitgrpid = KitGroupID FROM @tmpKitTbl
    WHILE @@rowcount <> 0 BEGIN
        INSERT [dbo].kitgroup (KitGroupGUID, Name, Description, ProductID, DisplayOrder, KitGroupTypeID, IsRequired, ExtensionData, CreatedOn)
        SELECT newid(), Name, Description, @newproductID, DisplayOrder, KitGroupTypeID, IsRequired, ExtensionData, getdate()
        FROM dbo.kitgroup
        WHERE KitGroupID = @kitgrpid

        SELECT @newkitgroupid = @@identity, @err = @@error

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

ALTER VIEW EntityMaster

AS
    SELECT 'category' EntityType, Entity.CategoryID EntityID, Entity.CategoryGUID EntityGuid, Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentCategoryID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Category Entity with (NOLOCK)
      left join (SELECT pc.CategoryID, COUNT(pc.ProductID) AS NumProducts
                 FROM  dbo.ProductCategory pc with (nolock)
                     join [dbo].Product p with (nolock) on pc.ProductID = p.ProductID  and p.deleted=0 and p.published=1
                 GROUP BY pc.CategoryID
                ) a on Entity.CategoryID = a.CategoryID
    WHERE Published = 1 and Deleted=0

    UNION ALL

    SELECT 'affiliate' EntityType, Entity.AffiliateID EntityID,Entity.AffiliateGUID EntityGuid, Name,4 as ColWidth,'' as Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentAffiliateID ParentEntityID,DisplayOrder,0 as SortByLooks,'' as XmlPackage,Published,isnull(NumProducts, 0) NumObjects, 0 PageSize, 0 QuantityDiscountID, '' Summary, SkinID, TemplateName
    FROM dbo.Affiliate Entity with (NOLOCK)
      left join (SELECT pa.AffiliateID, COUNT(pa.ProductID) AS NumProducts
                 FROM dbo.ProductAffiliate pa with (nolock) join [dbo].Product p with (nolock) on pa.ProductID = p.ProductID and p.deleted=0 and p.published=1
                 GROUP BY pa.AffiliateID
                ) a on Entity.AffiliateID = a.AffiliateID
    WHERE Published = 1 and Deleted=0

    UNION ALL

    SELECT 'section' EntityType, Entity.SectionID EntityID,Entity.SectionGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentSectionID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Section Entity with (NOLOCK)
        left join (SELECT ps.SectionID, COUNT(ps.ProductID) AS NumProducts
                   FROM dbo.ProductSection ps with (nolock) join [dbo].Product p with (nolock) on ps.ProductID = p.ProductID and p.deleted=0 and p.published=1
                   GROUP BY ps.SectionID
                  ) a on Entity.SectionID = a.SectionID
    WHERE Published = 1 and Deleted=0

    UNION ALL

    SELECT 'manufacturer' EntityType, Entity.ManufacturerID EntityID,Entity.ManufacturerGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentManufacturerID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Manufacturer Entity with (NOLOCK)
    left join (SELECT pm.ManufacturerID, COUNT(pm.ProductID) AS NumProducts
               FROM dbo.ProductManufacturer pm with (nolock) join [dbo].Product p with (nolock) on pm.ProductID = p.ProductID and p.deleted=0 and p.published=1
               GROUP BY pm.ManufacturerID
              ) a on Entity.ManufacturerID = a.ManufacturerID
    WHERE Published = 1 and Deleted=0

    UNION ALL

    SELECT 'library' EntityType, Entity.LibraryID EntityID,Entity.LibraryGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentLibraryID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published, 0 NumObjects, PageSize, 0 QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Library Entity with (NOLOCK)
    WHERE Published = 1 and Deleted=0

    UNION ALL

    SELECT 'distributor' EntityType, Entity.DistributorID EntityID,Entity.DistributorGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentDistributorID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Distributor Entity with (NOLOCK)
        left join (SELECT pd.DistributorID, COUNT(pd.ProductID) AS NumProducts
                   FROM dbo.ProductDistributor pd with (nolock) join [dbo].Product p with (nolock) on pd.ProductID = p.ProductID and p.deleted=0 and p.published=1
                   GROUP BY pd.DistributorID
                  ) a on Entity.DistributorID = a.DistributorID
    WHERE Published = 1 and Deleted=0

    UNION ALL

    SELECT 'genre' EntityType, Entity.GenreID EntityID,Entity.GenreGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentGenreID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Genre Entity with (NOLOCK)
        left join (SELECT px.GenreID, COUNT(px.ProductID) AS NumProducts
                   FROM dbo.ProductGenre px with (nolock) join [dbo].Product p with (nolock) on px.ProductID = p.ProductID and p.deleted=0 and p.published=1
                   GROUP BY px.GenreID
                  ) a on Entity.GenreID = a.GenreID
    WHERE Published = 1 and Deleted=0

    UNION ALL

    SELECT 'vector' EntityType, Entity.VectorID EntityID,Entity.VectorGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentVectorID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Vector Entity with (NOLOCK)
        left join (SELECT px2.VectorID, COUNT(px2.ProductID) AS NumProducts
                   FROM dbo.ProductVector px2 with (nolock) join [dbo].Product p with (nolock) on px2.ProductID = p.ProductID and p.deleted=0 and p.published=1
                   GROUP BY px2.VectorID
                  ) a on Entity.VectorID = a.VectorID
    WHERE Published = 1 and Deleted=0

    UNION ALL

    SELECT 'customerLevel' EntityType, Entity.CustomerLevelID EntityID,Entity.CustomerLevelGUID EntityGuid,Name, 4 ColWidth, '' Description,SEName, '' SEKeywords, '' SEDescription, '' SETitle,'' SEAltText,ParentCustomerLevelID ParentEntityID,DisplayOrder,0 SortByLooks, '' XmlPackage, 1 Published,isnull(NumProducts, 0) NumObjects, 20 PageSize, 0 QuantityDiscountID, '' Summary, SkinID, TemplateName
    FROM dbo.CustomerLevel Entity with (NOLOCK)
      left join (SELECT pc.CustomerLevelID, COUNT(pc.ProductID) AS NumProducts
                 FROM  dbo.ProductCustomerLevel pc with (nolock)
                     join [dbo].Product p with (nolock) on pc.ProductID = p.ProductID  and p.deleted=0 and p.published=1
                 GROUP BY pc.CustomerLevelID
                ) a on Entity.CustomerLevelID = a.CustomerLevelID
    WHERE Deleted=0

GO

--Convert ntext to nvarchar(max) fields
PRINT '****************************************************************'
PRINT 'Changing data types.  Some errors may occur in this section.  Please contact Support if any errors are reported.'
PRINT '****************************************************************'
ALTER TABLE [dbo].[Address] ALTER COLUMN [CardIssueNumber] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Address] ALTER COLUMN [CardStartDate] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Address] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Address] ALTER COLUMN [eCheckBankABACode] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Address] ALTER COLUMN [eCheckBankAccountName] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Address] ALTER COLUMN [eCheckBankAccountNumber] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Address] ALTER COLUMN [eCheckBankAccountType] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Address] ALTER COLUMN [eCheckBankName] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[AffiliateActivity] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[AffiliateActivity] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[AffiliateCommissions] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Affiliate] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Affiliate] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Affiliate] ALTER COLUMN [SEAltText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Affiliate] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Affiliate] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Affiliate] ALTER COLUMN [SETitle] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Affiliate] ALTER COLUMN [URL] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Affiliate] ALTER COLUMN [WebSiteDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[AppConfig] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Category] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Category] ALTER COLUMN [DisplayPrefix] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Category] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Category] ALTER COLUMN [ImageFilenameOverride] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Category] ALTER COLUMN [RelatedDocuments] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Category] ALTER COLUMN [SEAltText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Category] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Category] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Category] ALTER COLUMN [SETitle] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Category] ALTER COLUMN [Summary] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Country] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Coupon] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Coupon] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Coupon] ALTER COLUMN [ValidForCategories] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Coupon] ALTER COLUMN [ValidForCustomers] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Coupon] ALTER COLUMN [ValidForManufacturers] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Coupon] ALTER COLUMN [ValidForProducts] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Coupon] ALTER COLUMN [ValidForSections] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Currency] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[CustomReport] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[CustomReport] ALTER COLUMN [SQLCommand] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[CustomerLevel] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[CustomerSession] ALTER COLUMN [SessionValue] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Customer] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Customer] ALTER COLUMN [FinalizationData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Customer] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Customer] ALTER COLUMN [OrderNotes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Customer] ALTER COLUMN [OrderOptions] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Customer] ALTER COLUMN [RTShipRequest] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Customer] ALTER COLUMN [RTShipResponse] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Customer] ALTER COLUMN [RecurringShippingMethod] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Customer] ALTER COLUMN [Referrer] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Distributor] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Distributor] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Distributor] ALTER COLUMN [ImageFilenameOverride] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Distributor] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Distributor] ALTER COLUMN [SEAltText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Distributor] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Distributor] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Distributor] ALTER COLUMN [SETitle] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Distributor] ALTER COLUMN [Summary] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [ImageFilenameOverride] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [MiscText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [RelatedCategories] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [RelatedManufacturers] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [RelatedProducts] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [RelatedSections] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [SEAltText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [SETitle] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Document] ALTER COLUMN [Summary] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ErrorLog] ALTER COLUMN [ErrorMsg] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ExtendedPrice] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[FailedTransaction] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[FailedTransaction] ALTER COLUMN [MaxMindDetails] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[FailedTransaction] ALTER COLUMN [TransactionCommand] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[FailedTransaction] ALTER COLUMN [TransactionResult] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Feed] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Genre] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Genre] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Genre] ALTER COLUMN [ImageFilenameOverride] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Genre] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Genre] ALTER COLUMN [SEAltText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Genre] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Genre] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Genre] ALTER COLUMN [SETitle] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Genre] ALTER COLUMN [Summary] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[GiftCardUsage] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[GiftCard] ALTER COLUMN [EMailMessage] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[GiftCard] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[GiftCard] ALTER COLUMN [ValidForCategories] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[GiftCard] ALTER COLUMN [ValidForCustomers] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[GiftCard] ALTER COLUMN [ValidForManufacturers] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[GiftCard] ALTER COLUMN [ValidForProducts] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[GiftCard] ALTER COLUMN [ValidForSections] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[GlobalConfig] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Inventory] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[KitCart] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[KitCart] ALTER COLUMN [TextOption] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[KitGroup] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[KitGroup] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[KitGroup] ALTER COLUMN [Summary] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[KitItem] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[KitItem] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [DisplayPrefix] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [ImageFilenameOverride] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [RelatedCategories] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [RelatedManufacturers] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [RelatedProducts] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [RelatedSections] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [SEAltText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [SETitle] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Library] ALTER COLUMN [Summary] NVARCHAR(MAX)  NULL;
GO
--MailingMgrLog table is no longer used, but is not being dropped from the schema so people don't lose mailing history data.  New DBs will not have this, so skip it.
IF EXISTS (select * From sysobjects where id = object_id('[dbo].[MailingMgrLog]') and type = 'u')
BEGIN
	ALTER TABLE [dbo].[MailingMgrLog] ALTER COLUMN [Body] NVARCHAR(MAX)  NULL;
END
GO
ALTER TABLE [dbo].[Manufacturer] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Manufacturer] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Manufacturer] ALTER COLUMN [ImageFilenameOverride] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Manufacturer] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Manufacturer] ALTER COLUMN [RelatedDocuments] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Manufacturer] ALTER COLUMN [SEAltText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Manufacturer] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Manufacturer] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Manufacturer] ALTER COLUMN [SETitle] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Manufacturer] ALTER COLUMN [Summary] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[News] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[News] ALTER COLUMN [NewsCopy] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[OrderOption] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[OrderOption] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [AuthorizationResult] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CaptureTXCommand] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CaptureTXResult] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CardIssueNumber] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CardStartDate] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CardinalAuthenticateResult] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CardinalGatewayParms] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CardinalLookupResult] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CarrierReportedRate] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CarrierReportedWeight] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CouponDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [CustomerServiceNotes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [FinalizationData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [MaxMindDetails] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [OrderNotes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [OrderOptions] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [RTShipRequest] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [RTShipResponse] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [ReceiptHtml] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [Referrer] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [RefundReason] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [RefundTXCommand] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [RefundTXResult] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [ShippingMethod] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [TrackingURL] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [TransactionCommand] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [VATRegistrationID] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [VoidTXCommand] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [VoidTXResult] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [eCheckBankABACode] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [eCheckBankAccountName] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [eCheckBankAccountNumber] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [eCheckBankAccountType] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders] ALTER COLUMN [eCheckBankName] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_KitCart] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_KitCart] ALTER COLUMN [TextOption] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [ColorOptionPrompt] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [CustomerEntersPricePrompt] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [DownloadCategory] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [DownloadLocation] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [OrderedProductName] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [OrderedProductVariantName] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [ShippingDetail] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [ShippingMethod] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [SizeOptionPrompt] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [TextOptionPrompt] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [TextOption] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [ColorSKUModifiers] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [Colors] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [CustomerEntersPricePrompt] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [DownloadLocation] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [ExtensionData2] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [ExtensionData3] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [ExtensionData4] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [ExtensionData5] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [FroogleDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [ImageFilenameOverride] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [SEAltText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [SizeSKUModifiers] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ProductVariant] ALTER COLUMN [Sizes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [ColorOptionPrompt] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [ExtensionData2] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [ExtensionData3] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [ExtensionData4] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [ExtensionData5] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [FroogleDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [ImageFilenameOverride] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [MiscText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [RelatedDocuments] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [RelatedProducts] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [RequiresProducts] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [SEAltText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [SETitle] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [SizeOptionPrompt] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [Summary] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [SwatchImageMap] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [TextOptionPrompt] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Product] ALTER COLUMN [UpsellProducts] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[QuantityDiscount] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Rating] ALTER COLUMN [Comments] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[SalesPrompt] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Section] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Section] ALTER COLUMN [DisplayPrefix] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Section] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Section] ALTER COLUMN [ImageFilenameOverride] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Section] ALTER COLUMN [RelatedDocuments] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Section] ALTER COLUMN [SEAltText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Section] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Section] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Section] ALTER COLUMN [SETitle] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Section] ALTER COLUMN [Summary] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[SecurityLog] ALTER COLUMN [Description] NVARCHAR(MAX) NOT NULL;
GO
ALTER TABLE [dbo].[ShippingMethod] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ShippingMethod] ALTER COLUMN [ShipRushTemplate] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ShippingZone] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ShippingZone] ALTER COLUMN [ZipCodes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ShoppingCart] ALTER COLUMN [DownloadLocation] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ShoppingCart] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ShoppingCart] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ShoppingCart] ALTER COLUMN [ShippingMethod] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[ShoppingCart] ALTER COLUMN [TextOption] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[State] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Store] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Store] ALTER COLUMN [Summary] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Topic] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Topic] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Topic] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Topic] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Topic] ALTER COLUMN [SETitle] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Topic] ALTER COLUMN [Title] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Vector] ALTER COLUMN [Description] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Vector] ALTER COLUMN [ExtensionData] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Vector] ALTER COLUMN [ImageFilenameOverride] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Vector] ALTER COLUMN [Notes] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Vector] ALTER COLUMN [SEAltText] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Vector] ALTER COLUMN [SEDescription] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Vector] ALTER COLUMN [SEKeywords] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Vector] ALTER COLUMN [SETitle] NVARCHAR(MAX)  NULL;
GO
ALTER TABLE [dbo].[Vector] ALTER COLUMN [Summary] NVARCHAR(MAX)  NULL;
GO

--Handle special NTEXT to NVARCHAR(MAX) cases that we can't alter due to constraints
IF EXISTS (SELECT *
			FROM sys.columns AS col
				INNER JOIN sys.types AS types ON col.user_type_id=types.user_type_id
				INNER JOIN sys.tables tables ON tables.OBJECT_ID = col.OBJECT_ID
				INNER JOIN sys.schemas s ON s.schema_id = tables.schema_id
			WHERE types.Name = 'ntext'
				AND s.Name = 'dbo'
				AND tables.Name = 'Orders'
				AND col.Name = 'RecurringSubscriptionCommand')
	BEGIN
		ALTER TABLE [dbo].[Orders] ADD RecurringSubscriptionCommand_New NVARCHAR(MAX)
		EXEC('UPDATE [dbo].[Orders] SET RecurringSubscriptionCommand_New = RecurringSubscriptionCommand')
		ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF_Orders_RecurringSubscriptionCommand]
		ALTER TABLE [dbo].[Orders] DROP COLUMN RecurringSubscriptionCommand
		EXEC sp_rename 'Orders.RecurringSubscriptionCommand_New', 'RecurringSubscriptionCommand', 'COLUMN'
		ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [DF_Orders_RecurringSubscriptionCommand]  DEFAULT ('') FOR [RecurringSubscriptionCommand]
	END
	GO

IF EXISTS (SELECT *
			FROM sys.columns AS col
				INNER JOIN sys.types AS types ON col.user_type_id=types.user_type_id
				INNER JOIN sys.tables tables ON tables.OBJECT_ID = col.OBJECT_ID
				INNER JOIN sys.schemas s ON s.schema_id = tables.schema_id
			WHERE types.Name = 'ntext'
				AND s.Name = 'dbo'
				AND tables.Name = 'Orders'
				AND col.Name = 'RecurringSubscriptionResult')
	BEGIN
		ALTER TABLE [dbo].[Orders] ADD RecurringSubscriptionResult_New NVARCHAR(MAX)
		EXEC('UPDATE [dbo].[Orders] SET RecurringSubscriptionResult_New = RecurringSubscriptionResult')
		ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF_Orders_RecurringSubscriptionResult]
		ALTER TABLE [dbo].[Orders] DROP COLUMN RecurringSubscriptionResult
		EXEC sp_rename 'Orders.RecurringSubscriptionResult_New', 'RecurringSubscriptionResult', 'COLUMN'
		ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [DF_Orders_RecurringSubscriptionResult]  DEFAULT ('') FOR [RecurringSubscriptionResult]
	END
	GO

	IF EXISTS (SELECT *
			FROM sys.columns AS col
				INNER JOIN sys.types AS types ON col.user_type_id=types.user_type_id
				INNER JOIN sys.tables tables ON tables.OBJECT_ID = col.OBJECT_ID
				INNER JOIN sys.schemas s ON s.schema_id = tables.schema_id
			WHERE types.Name = 'ntext'
				AND s.Name = 'dbo'
				AND tables.Name = 'Orders'
				AND col.Name = 'BuySafeCommand')
	BEGIN
		ALTER TABLE [dbo].[Orders] ADD BuySafeCommand_New NVARCHAR(MAX)
		EXEC('UPDATE [dbo].[Orders] SET BuySafeCommand_New = BuySafeCommand')
		ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF_Orders_BuySafeCommand]
		ALTER TABLE [dbo].[Orders] DROP COLUMN BuySafeCommand
		EXEC sp_rename 'Orders.BuySafeCommand_New', 'BuySafeCommand', 'COLUMN'
		ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [DF_Orders_BuySafeCommand]  DEFAULT ('') FOR [BuySafeCommand]
	END
	GO

IF EXISTS (SELECT *
			FROM sys.columns AS col
				INNER JOIN sys.types AS types ON col.user_type_id=types.user_type_id
				INNER JOIN sys.tables tables ON tables.OBJECT_ID = col.OBJECT_ID
				INNER JOIN sys.schemas s ON s.schema_id = tables.schema_id
			WHERE types.Name = 'ntext'
				AND s.Name = 'dbo'
				AND tables.Name = 'Orders'
				AND col.Name = 'BuySafeResult')
	BEGIN
		ALTER TABLE [dbo].[Orders] ADD BuySafeResult_New NVARCHAR(MAX)
		EXEC('UPDATE [dbo].[Orders] SET BuySafeResult_New = BuySafeResult')
		ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF_Orders_BuySafeResult]
		ALTER TABLE [dbo].[Orders] DROP COLUMN BuySafeResult
		EXEC sp_rename 'Orders.BuySafeResult_New', 'BuySafeResult', 'COLUMN'
		ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [DF_Orders_BuySafeResult]  DEFAULT ('') FOR [BuySafeResult]
	END
	GO
PRINT '****************************************************************'
PRINT 'Done changing data types.'
PRINT '****************************************************************'

GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.aspdnsf_GetUpsellProducts') AND type IN ( N'P', N'PC' ) )
	begin
	 DROP PROCEDURE dbo.aspdnsf_GetUpsellProducts;
	end
GO

-- Added info for Schema.org attributes
CREATE PROCEDURE [dbo].[aspdnsf_GetUpsellProducts]
	@productID			INT,
	@customerlevelID	INT,
	@invFilter			INT,
	@storeID			INT = 1,
	@filterProduct		BIT = 0


AS
BEGIN
	SET NOCOUNT ON;

   DECLARE @UpsellProducts VARCHAR(8000),
		   @UpsellProductDiscPct MONEY

    SELECT @UpsellProducts = REPLACE(CAST(UpsellProducts AS VARCHAR(8000)), ' ', '')
	     , @UpsellProductDiscPct = UpsellProductDiscountPercentage
	  FROM dbo.product WITH (NOLOCK) WHERE productid = @productid

	SELECT 1-(@UpsellProductDiscPct/100) UpsellDiscMultiplier
		 , p.*
		 , pv.VariantID
		 , pv.Price
		 , ISNULL(pv.SalePrice, 0) SalePrice
		 , ISNULL(pv.SkuSuffix, '') AS SkuSuffix
		 , ISNULL(pv.ManufacturerPartNumber, '') AS VariantManufacturerPartNumber
		 , ISNULL(pv.Dimensions, '') AS Dimensions
		 , pv.Weight
		 , ISNULL(pv.GTIN, '') AS GTIN
		 , pv.Condition
		 , ISNULL(pv.Points, 0) Points
		 , sp.Name SalesPromptName
		 , ISNULL(ep.price, 0) ExtendedPrice
		 , ProductManufacturer.ManufacturerID AS ProductManufacturerId
		 , Manufacturer.Name AS ProductManufacturerName
		 , Manufacturer.SEName AS ProductManufacturerSEName
      FROM dbo.product p WITH (NOLOCK)
      JOIN dbo.split(@UpsellProducts, ',') up ON p.productid = CAST(up.items AS INT)
 LEFT JOIN dbo.SalesPrompt sp WITH (NOLOCK) ON sp.SalesPromptID = p.SalesPromptID
      JOIN dbo.productvariant pv WITH (NOLOCK) ON pv.productid = CAST(up.items AS INT) AND pv.isdefault = 1 AND pv.Published = 1 AND pv.Deleted = 0
 LEFT JOIN dbo.ExtendedPrice ep WITH (NOLOCK) ON ep.VariantID = pv.VariantID AND ep.CustomerLevelID = @CustomerLevelID
      JOIN (SELECT p.ProductID
              FROM dbo.product p WITH (NOLOCK)
              JOIN dbo.split(@UpsellProducts, ',') rp ON p.productid = CAST(rp.items AS INT)
              JOIN (SELECT ProductID, SUM(Inventory) Inventory
			          FROM dbo.productvariant WITH (NOLOCK)
				  GROUP BY ProductID) pv ON p.ProductID = pv.ProductID
         LEFT JOIN (SELECT ProductID
				         , SUM(quan) inventory
				      FROM dbo.inventory i1 WITH (NOLOCK)
				      JOIN dbo.productvariant pv1 WITH (NOLOCK) ON pv1.variantid = i1.variantid
				      JOIN dbo.split(@UpsellProducts, ',') rp1 ON pv1.productid = CAST(rp1.items AS INT)
			      GROUP BY pv1.productid) i ON i.productid = p.productid
                WHERE CASE p.TrackInventoryBySizeAndColor WHEN 1 THEN ISNULL(i.inventory, 0) ELSE pv.inventory END >= @InvFilter
                   ) tp ON p.productid = tp.productid
		INNER JOIN (SELECT DISTINCT a.ProductID
		              FROM Product a WITH (NOLOCK)
				 LEFT JOIN ProductStore b WITH (NOLOCK) ON a.ProductID = b.ProductID WHERE (@filterProduct = 0 OR StoreID = @storeID)
				   ) ps ON p.ProductID = ps.ProductID
LEFT JOIN dbo.ProductManufacturer WITH (NOLOCK) ON tp.ProductID = ProductManufacturer.ProductID
LEFT JOIN dbo.Manufacturer WITH (NOLOCK) ON ProductManufacturer.ManufacturerID = Manufacturer.ManufacturerID
	WHERE p.Published = 1
	  AND p.Deleted = 0
	  AND p.IsCallToOrder = 0
	  AND pv.CustomerEntersPrice = 0
	  AND p.IsAKit = 0
	  AND (pv.Sizes = '' OR pv.Sizes IS NULL)
	  AND (pv.Colors = '' OR pv.Colors IS NULL)
	  AND p.productid != @productid
END
GO

--Gift Registry Cleanup
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_AddItemToCart') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_AddItemToCart
GO

CREATE proc [dbo].[aspdnsf_AddItemToCart]
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
	DECLARE @RestrictedQy varchar(8000), @CurrentCartQty int, @InventoryFullSKU nvarchar(50), @InventoryWeightDelta money, @AllowEmptySkuAddToCart varchar(10), @ShoppingCartrecid int, @IsAKit tinyint
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


	SELECT @AllowEmptySkuAddToCart = ConfigValue FROM dbo.AppConfig with (nolock) WHERE [name]= 'AllowEmptySkuAddToCart'


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
		and (@AllowEmptySkuAddToCart = 'true' or rtrim(case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then isnull(p.sku, '') + isnull(pv.skusuffix, '') + isnull(@ChosenColorSKUModifier, '') + isnull(@ChosenSizeSKUModifier, '') else i.VendorFullSKU end) <> '')

	SET @ShoppingCartrecid = @@IDENTITY

	--Update KitCart Table if necessary
	IF (@IsAKit = 1 AND @IsKit2 = 0) BEGIN
		UPDATE KitCart SET ShoppingCartRecID = @ShoppingCartrecid WHERE ProductID = @ProductID and VariantID = @VariantID and ShoppingCartRecID = 0 and CustomerID = @CustomerID
	END

	SET @NewShoppingCartRecID = @ShoppingCartrecid

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_updCustomerByEmail') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_updCustomerByEmail
GO

CREATE proc [dbo].[aspdnsf_updCustomerByEmail]
    @Email nvarchar(100),
    @CustomerLevelID int = null,
    @Password nvarchar(250) = null,
    @SaltKey int = null,
    @DateOfBirth datetime = null,
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
    @CODCompanyCheckAllowed tinyint = null,
    @CODNet30Allowed tinyint = null,
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
    DateOfBirth = COALESCE(@DateOfBirth, DateOfBirth),
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
    CODCompanyCheckAllowed = COALESCE(@CODCompanyCheckAllowed, CODCompanyCheckAllowed),
    CODNet30Allowed = COALESCE(@CODNet30Allowed, CODNet30Allowed),
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

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_updCustomer') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_updCustomer
GO

CREATE proc [dbo].[aspdnsf_updCustomer]
    @CustomerID int,
    @CustomerLevelID int = null,
    @Email nvarchar(100) = null,
    @Password nvarchar(250) = null,
    @SaltKey int = null,
    @DateOfBirth datetime = null,
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
    @CODCompanyCheckAllowed tinyint = null,
    @CODNet30Allowed tinyint = null,
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
    @StoreID	int = null


AS
SET NOCOUNT ON

DECLARE @OldPwd nvarchar(100), @IsAdminCust tinyint, @OldSaltKey int

SELECT @OldPwd = Password, @IsAdminCust = IsAdmin, @OldSaltKey = Saltkey FROM dbo.Customer WHERE CustomerID = @CustomerID


UPDATE dbo.Customer
SET
    CustomerLevelID = COALESCE(@CustomerLevelID, CustomerLevelID),
    RegisterDate = COALESCE(@RegisterDate, RegisterDate),
    Email = COALESCE(@Email, Email),
    Password = COALESCE(@Password, Password),
    SaltKey = COALESCE(@SaltKey, SaltKey),
    DateOfBirth = COALESCE(@DateOfBirth, DateOfBirth),
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
    CODCompanyCheckAllowed = COALESCE(@CODCompanyCheckAllowed, CODCompanyCheckAllowed),
    CODNet30Allowed = COALESCE(@CODNet30Allowed, CODNet30Allowed),
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
    RequestedPaymentMethod = COALESCE(@RequestedPaymentMethod, RequestedPaymentMethod),
    StoreID = COALESCE(@StoreID, StoreID)
WHERE CustomerID = @CustomerID

IF @IsAdminCust > 0 and @OldPwd <> @Password
    INSERT dbo.PasswordLog (CustomerID, OldPwd, SaltKey, ChangeDt)
    VALUES (@CustomerID, @OldPwd, @OldSaltKey, getdate())

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_getOrder') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_getOrder
GO

CREATE proc [dbo].[aspdnsf_getOrder]
    @ordernumber int

AS
SET NOCOUNT ON
SELECT
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
    o.eCheckBankABACode,
    o.eCheckBankAccountNumber,
    o.eCheckBankAccountType,
    o.eCheckBankName,
    o.eCheckBankAccountName,
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
    os.IsAKit,
    os.IsAPack
FROM Orders o with (nolock)
    left join orders_ShoppingCart os with (nolock) ON os.OrderNumber = o.OrderNumber
WHERE o.OrderNumber = @ordernumber
ORDER BY os.ShippingAddressID

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_NukeStoreCustomer') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_NukeStoreCustomer
GO

CREATE procedure [dbo].[aspdnsf_NukeStoreCustomer]
		@StoreID int,
		@IncludeAdmins BIT = 0
	as
	begin
		set nocount on;

		delete pu
		from promotionusage pu
		inner join Customer c on pu.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete okc
		from orders_kitcart okc
		inner join Customer c on okc.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete osc
		from orders_ShoppingCart osc
		inner join Customer c on osc.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete o
		from orders o
		inner join Customer c on o.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete sc
		from ShoppingCart sc
		inner join Customer c on sc.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete ft
		from failedtransaction ft
		inner join Customer c on ft.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete kc
		from kitcart kc
		inner join Customer c on kc.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete rch
		from ratingcommenthelpfulness rch
		inner join Customer c on rch.RatingCustomerID = c.CustomerID or rch.VotingCustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete r
		from rating r
		inner join Customer c on r.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete a
		from Address a
		inner join Customer c on a.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete cs
		from CustomerSession cs
		inner join Customer c on cs.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete c
		from Customer c
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

	end

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_insCustomer') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_insCustomer
GO

CREATE proc [dbo].[aspdnsf_insCustomer]
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


insert dbo.Customer(CustomerGUID, CustomerLevelID, RegisterDate, Email, Password, DateOfBirth, Gender, FirstName, LastName, Notes, SkinID, Phone, AffiliateID, Referrer, CouponCode, OkToEmail, IsAdmin, BillingEqualsShipping, LastIPAddress, OrderNotes, RTShipRequest, RTShipResponse, OrderOptions, LocaleSetting, MicroPayBalance, RecurringShippingMethodID, RecurringShippingMethod, BillingAddressID, ShippingAddressID, CODCompanyCheckAllowed, CODNet30Allowed, ExtensionData, FinalizationData, Deleted, CreatedOn, Over13Checked, CurrencySetting, VATSetting, VATRegistrationID, StoreCCInDB, IsRegistered, LockedUntil, AdminCanViewCC, PwdChanged, BadLoginCount, LastBadLogin, Active, PwdChangeRequired, SaltKey)
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
    0,
    0,
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

DELETE FROM AppConfig WHERE Name = 'AllowGiftRegistryQuantities'
DELETE FROM AppConfig WHERE Name = 'DecrementGiftRegistryOnOrder'
DELETE FROM AppConfig WHERE Name = 'AddToCart.AddToGiftRegistryButton'
UPDATE AppConfig SET Description = 'If true, the user can add a shipping address for each item in their cart.' WHERE Name = 'AllowMultipleShippingAddressPerOrder'

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CustomerGiftRegistrySearches]') AND name = N'UIX_CustomerGiftRegistrySearches_CustomerID_GIftRegistryGUID')
	DROP INDEX [UIX_CustomerGiftRegistrySearches_CustomerID_GIftRegistryGUID] ON [dbo].[CustomerGiftRegistrySearches] WITH ( ONLINE = OFF )
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'IX_Customer_GiftRegistryGUID')
	DROP INDEX [IX_Customer_GiftRegistryGUID] ON [dbo].[Customer] WITH ( ONLINE = OFF )
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = N'IX_Customer_GiftRegistryNickName')
	DROP INDEX [IX_Customer_GiftRegistryNickName] ON [dbo].[Customer] WITH ( ONLINE = OFF )
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'[dbo].[CustomerGiftRegistrySearches_Updated]'))
	DROP TRIGGER [dbo].[CustomerGiftRegistrySearches_Updated]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[CustomerGiftRegistrySearches]') and type = 'u')
DROP TABLE [dbo].[CustomerGiftRegistrySearches]

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Customer]') AND name = 'GiftRegistryGUID')
	BEGIN
		ALTER TABLE [dbo].[Customer] DROP CONSTRAINT DF_Customer_GiftRegistryGUID
		ALTER TABLE [dbo].[Customer] DROP COLUMN GiftRegistryGUID
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Customer]') AND name = 'GiftRegistryIsAnonymous')
	BEGIN
		ALTER TABLE [dbo].[Customer] DROP CONSTRAINT DF_Customer_GiftRegistryIsAnonymous
		ALTER TABLE [dbo].[Customer] DROP COLUMN GiftRegistryIsAnonymous
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Customer]') AND name = 'GiftRegistryAllowSearchByOthers')
	BEGIN
		ALTER TABLE [dbo].[Customer] DROP CONSTRAINT DF_Customer_GiftRegistryAllowSearchByOthers
		ALTER TABLE [dbo].[Customer] DROP COLUMN GiftRegistryAllowSearchByOthers
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Customer]') AND name = 'GiftRegistryNickName')
	BEGIN
		ALTER TABLE [dbo].[Customer] DROP COLUMN GiftRegistryNickName
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Customer]') AND name = 'GiftRegistryHideShippingAddresses')
	BEGIN
		ALTER TABLE [dbo].[Customer] DROP CONSTRAINT DF_Customer_GiftRegistryHideShippingAddresses
		ALTER TABLE [dbo].[Customer] DROP COLUMN GiftRegistryHideShippingAddresses
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[ShoppingCart]') AND name = 'GiftRegistryForCustomerID')
	BEGIN
		ALTER TABLE [dbo].[ShoppingCart] DROP CONSTRAINT DF_ShoppingCart_GiftRegistryForCustomerID
		ALTER TABLE [dbo].[ShoppingCart] DROP COLUMN GiftRegistryForCustomerID
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Orders_ShoppingCart]') AND name = 'GiftRegistryForCustomerID')
	BEGIN
		ALTER TABLE [dbo].[Orders_ShoppingCart] DROP COLUMN GiftRegistryForCustomerID
	END

GO

--Customers Who Also Bought Cleanup

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetCustomersAlsoBoughtProducts') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_GetCustomersAlsoBoughtProducts
GO
DELETE FROM AppConfig WHERE Name = 'AlsoBoughtNumberToDisplay'
DELETE FROM AppConfig WHERE Name = 'AlsoBoughtProductsGridColWidth'
UPDATE dbo.[AppConfig] SET Description = 'For Developers Use Only - This AppConfig defines the widths of the images in the product slider. This value should space images appropriately on mobile devices when paired with the value in the "Mobile.ProductSlider.Width" AppConfig. The product slider is used for featured products, recently-viewed products, related products, and upsell products.' WHERE [Name] = 'Mobile.ProductSlider.ImageWidth'

GO

-- Add Google Tag Manager
if not exists (select * from AppConfig where Name = 'Google.TagManager.Enabled')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'Google.TagManager.Enabled', 'Enable Google Tag Manager script on your site. This puts the contents of the Script.Google.TagManager topic on every page of your site. You must make sure that the script.bodyopen xmlpackage is included in your template.', 'false', 'boolean', null, 'MISC', 0, 0, getdate())
GO

if not exists(select name from topic where name = 'Script.Google.TagManager' or name like '%>Script.Google.TagManager<%')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Script.Google.TagManager',
	'Script.Google.TagManager',
	1,
	0,
	''
);

-- remove old sitemap appconfigs
DELETE FROM AppConfig WHERE Name = 'SiteMap.ShowDocuments'
DELETE FROM AppConfig WHERE Name = 'SiteMap.ShowLibraries'

-- add new sitemap appconfig
if not exists (select * from AppConfig where Name = 'XmlPackage.SiteMapPage')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'XmlPackage.SiteMapPage', 'The XmlPackage used to render out the site map found at sitemap.aspx', '', 'string', null, 'SITEDISPLAY', 0, 0, getdate())
GO

--Promotions update
UPDATE AppConfig SET Name = 'Promotions.ExcludeStates' WHERE Name = 'AspDotNetStorefront.Promotions.excludestates'
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Promotions.ExcludeStates') BEGIN
	INSERT [dbo].[AppConfig] ([SuperOnly], [Name], [GroupName], [Description], [ValueType] ,[ConfigValue])
		values(1,'Promotions.ExcludeStates','CHECKOUT','Comma-separated list of 2-digit state codes to exclude from shipping promotions.', 'string', '');
END
GO

IF EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Promotions.ExcludeStates') BEGIN
	UPDATE [dbo].[AppConfig] SET [ValueType] = 'string' WHERE [Name] = 'Promotions.ExcludeStates'
END
GO

IF EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Promotions.ExcludeStates' AND [ConfigValue] = 'true') BEGIN
	UPDATE [dbo].[AppConfig] SET [ConfigValue] = '' WHERE [Name] = 'Promotions.ExcludeStates'
END
GO

-- Available Start Date Cleanup
IF EXISTS (SELECT * FROM sys.objects where name = 'DF_Product_AvailableStartDate' AND parent_object_id = OBJECT_ID(N'[dbo].[Product]'))
	BEGIN
		ALTER TABLE [dbo].[Product] DROP CONSTRAINT DF_Product_AvailableStartDate
	END
GO
IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Product]') AND name = 'AvailableStartDate')
	BEGIN
		ALTER TABLE [dbo].[Product] DROP COLUMN AvailableStartDate
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Product]') AND name = 'AvailableStopDate')
	BEGIN
		ALTER TABLE [dbo].[Product] DROP COLUMN AvailableStopDate
	END

GO

-- SENoScript Cleanup

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Affiliate]') AND name = 'SENoScript')
	BEGIN
		ALTER TABLE [dbo].[Affiliate] DROP COLUMN SENoScript
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Library]') AND name = 'SENoScript')
	BEGIN
		ALTER TABLE [dbo].[Library] DROP COLUMN SENoScript
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Topic]') AND name = 'SENoScript')
	BEGIN
		ALTER TABLE [dbo].[Topic] DROP COLUMN SENoScript
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Document]') AND name = 'SENoScript')
	BEGIN
		ALTER TABLE [dbo].[Document] DROP COLUMN SENoScript
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Category]') AND name = 'SENoScript')
	BEGIN
		ALTER TABLE [dbo].[Category] DROP COLUMN SENoScript
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Distributor]') AND name = 'SENoScript')
	BEGIN
		ALTER TABLE [dbo].[Distributor] DROP COLUMN SENoScript
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Genre]') AND name = 'SENoScript')
	BEGIN
		ALTER TABLE [dbo].[Genre] DROP COLUMN SENoScript
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Manufacturer]') AND name = 'SENoScript')
	BEGIN
		ALTER TABLE [dbo].[Manufacturer] DROP COLUMN SENoScript
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Section]') AND name = 'SENoScript')
	BEGIN
		ALTER TABLE [dbo].[Section] DROP COLUMN SENoScript
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Vector]') AND name = 'SENoScript')
	BEGIN
		ALTER TABLE [dbo].[Vector] DROP COLUMN SENoScript
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Product]') AND name = 'SENoScript')
	BEGIN
		ALTER TABLE [dbo].[Product] DROP COLUMN SENoScript
	END

GO

-- Product Editor Cleanup

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Product]') AND name = 'PageSize')
	BEGIN
		ALTER TABLE [dbo].[Product] DROP CONSTRAINT DF_Product_PageSize
		ALTER TABLE [dbo].[Product] DROP COLUMN PageSize
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Product]') AND name = 'PageBGColor')
	BEGIN
		ALTER TABLE [dbo].[Product] DROP COLUMN PageBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Product]') AND name = 'ContentsBGColor')
	BEGIN
		ALTER TABLE [dbo].[Product] DROP COLUMN ContentsBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Product]') AND name = 'GraphicsColor')
	BEGIN
		ALTER TABLE [dbo].[Product] DROP COLUMN GraphicsColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[ProductVariant]') AND name = 'PageBGColor')
	BEGIN
		ALTER TABLE [dbo].[ProductVariant] DROP COLUMN PageBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[ProductVariant]') AND name = 'ContentsBGColor')
	BEGIN
		ALTER TABLE [dbo].[ProductVariant] DROP COLUMN ContentsBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[ProductVariant]') AND name = 'GraphicsColor')
	BEGIN
		ALTER TABLE [dbo].[ProductVariant] DROP COLUMN GraphicsColor
	END

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_CloneVariant') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_CloneVariant
GO

CREATE proc [dbo].[aspdnsf_CloneVariant]
    @VariantID int,
    @userid int = 0

AS
BEGIN
    DECLARE @newvariantid int
    SET @newvariantid = 0

    INSERT [dbo].productvariant (VariantGUID, ProductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
    SELECT newid(), ProductID, 0, '(Cloned) ' + Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, 0, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ImageFilenameOverride, IsImport, Deleted, getdate()
    FROM dbo.productvariant
    WHERE VariantID = @VariantID

    SELECT @newvariantid = @@IDENTITY

    IF @@error <> 0 BEGIN
        raiserror('Variant not cloned', 1, 16)
        SELECT 0 VariantID
        RETURN
    END
    ELSE BEGIN
        INSERT [dbo].ExtendedPrice (ExtendedPriceGUID, VariantID, CustomerLevelID, Price, ExtensionData, CreatedOn)
        SELECT newid(), @newvariantid, CustomerLevelID, Price, ExtensionData, getdate()
        FROM dbo.ExtendedPrice
        WHERE VariantID = @VariantID

        INSERT [dbo].Inventory (InventoryGUID, VariantID, Color, Size, Quan, CreatedOn)
        SELECT newid(), @newvariantid, Color, Size, Quan, @userid
        FROM dbo.Inventory
        WHERE VariantID = @VariantID

        SELECT @newvariantid VariantID
    END
END

GO

-- Entity Editor Cleanup

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Library]') AND name = 'ContentsBGColor')
	BEGIN
		ALTER TABLE [dbo].[Library] DROP COLUMN ContentsBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Library]') AND name = 'PageBGColor')
	BEGIN
		ALTER TABLE [dbo].[Library] DROP COLUMN PageBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Library]') AND name = 'GraphicsColor')
	BEGIN
		ALTER TABLE [dbo].[Library] DROP COLUMN GraphicsColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Document]') AND name = 'ContentsBGColor')
	BEGIN
		ALTER TABLE [dbo].[Document] DROP COLUMN ContentsBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Document]') AND name = 'PageBGColor')
	BEGIN
		ALTER TABLE [dbo].[Document] DROP COLUMN PageBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Document]') AND name = 'GraphicsColor')
	BEGIN
		ALTER TABLE [dbo].[Document] DROP COLUMN GraphicsColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Category]') AND name = 'ContentsBGColor')
	BEGIN
		ALTER TABLE [dbo].[Category] DROP COLUMN ContentsBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Category]') AND name = 'PageBGColor')
	BEGIN
		ALTER TABLE [dbo].[Category] DROP COLUMN PageBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Category]') AND name = 'GraphicsColor')
	BEGIN
		ALTER TABLE [dbo].[Category] DROP COLUMN GraphicsColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Distributor]') AND name = 'ContentsBGColor')
	BEGIN
		ALTER TABLE [dbo].[Distributor] DROP COLUMN ContentsBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Distributor]') AND name = 'PageBGColor')
	BEGIN
		ALTER TABLE [dbo].[Distributor] DROP COLUMN PageBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Distributor]') AND name = 'GraphicsColor')
	BEGIN
		ALTER TABLE [dbo].[Distributor] DROP COLUMN GraphicsColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Genre]') AND name = 'ContentsBGColor')
	BEGIN
		ALTER TABLE [dbo].[Genre] DROP COLUMN ContentsBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Genre]') AND name = 'PageBGColor')
	BEGIN
		ALTER TABLE [dbo].[Genre] DROP COLUMN PageBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Genre]') AND name = 'GraphicsColor')
	BEGIN
		ALTER TABLE [dbo].[Genre] DROP COLUMN GraphicsColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Manufacturer]') AND name = 'ContentsBGColor')
	BEGIN
		ALTER TABLE [dbo].[Manufacturer] DROP COLUMN ContentsBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Manufacturer]') AND name = 'PageBGColor')
	BEGIN
		ALTER TABLE [dbo].[Manufacturer] DROP COLUMN PageBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Manufacturer]') AND name = 'GraphicsColor')
	BEGIN
		ALTER TABLE [dbo].[Manufacturer] DROP COLUMN GraphicsColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Section]') AND name = 'ContentsBGColor')
	BEGIN
		ALTER TABLE [dbo].[Section] DROP COLUMN ContentsBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Section]') AND name = 'PageBGColor')
	BEGIN
		ALTER TABLE [dbo].[Section] DROP COLUMN PageBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Section]') AND name = 'GraphicsColor')
	BEGIN
		ALTER TABLE [dbo].[Section] DROP COLUMN GraphicsColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Vector]') AND name = 'ContentsBGColor')
	BEGIN
		ALTER TABLE [dbo].[Vector] DROP COLUMN ContentsBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Vector]') AND name = 'PageBGColor')
	BEGIN
		ALTER TABLE [dbo].[Vector] DROP COLUMN PageBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Vector]') AND name = 'GraphicsColor')
	BEGIN
		ALTER TABLE [dbo].[Vector] DROP COLUMN GraphicsColor
	END

GO

-- Topic Editor cleanup

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Topic]') AND name = 'ContentsBGColor')
	BEGIN
		ALTER TABLE [dbo].[Topic] DROP COLUMN ContentsBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Topic]') AND name = 'PageBGColor')
	BEGIN
		ALTER TABLE [dbo].[Topic] DROP COLUMN PageBGColor
	END

GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Topic]') AND name = 'GraphicsColor')
	BEGIN
		ALTER TABLE [dbo].[Topic] DROP COLUMN GraphicsColor
	END

GO

-- Add the appconfig required for the new maintenance.master template
IF (NOT EXISTS (SELECT Name FROM AppConfig WHERE Name='DisplayMaintenancePage.Enable'))
	   INSERT INTO AppConfig (Name, ConfigValue, Description, GroupName, ValueType)
	   VALUES('DisplayMaintenancePage.Enable', 'false', 'Display maintenance.master page to non-Admin users. Administrators can still access the site.' , 'SITEDISPLAY', 'boolean')
GO

-- Add the topic required for the new maintenance.master template
IF EXISTS (SELECT * FROM Topic WHERE Name='Template.MaintenancePage')
	BEGIN
	PRINT 'Template.MaintenancePage topic exists already'
END
ELSE
	BEGIN
	PRINT 'Adding Template.MaintenancePage topic'
		INSERT INTO Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) VALUES
		(
			'Template.MaintenancePage',
			1,
			0,
			'Down for Maintenance', '
			<h1>Site is temporarily unavailable.</h1>
			<p>We are currently performing scheduled maintenance. Site will be back soon.</p>
			<p>We apologize for any inconvenience.</p>
			<p><a href="default.aspx">Click here to refresh the page</a></p>'
		)
END
GO

--Schema cleanup
IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[AffiliateActivity]') AND name = 'AffiliateActivityReasonID')
	BEGIN
		ALTER TABLE [dbo].[AffiliateActivity] DROP COLUMN AffiliateActivityReasonID
	END
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[ClickTrack]') and type = 'u')
	DROP TABLE [dbo].[ClickTrack]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[CustomCart]') and type = 'u')
	DROP TABLE [dbo].[CustomCart]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[DocumentAffiliate]') and type = 'u')
	DROP TABLE [dbo].[DocumentAffiliate]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[DocumentCustomerLevel]') and type = 'u')
	DROP TABLE [dbo].[DocumentCustomerLevel]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[DocumentLibrary]') and type = 'u')
	DROP TABLE [dbo].[DocumentLibrary]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[FAQ]') and type = 'u')
	DROP TABLE [dbo].[FAQ]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[Gallery]') and type = 'u')
	DROP TABLE [dbo].[Gallery]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[LOG_CustomerEvent]') and type = 'u')
	DROP TABLE [dbo].[LOG_CustomerEvent]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[LOG_Event]') and type = 'u')
	DROP TABLE [dbo].[LOG_Event]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[PageType]') and type = 'u')
	DROP TABLE [dbo].[PageType]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[Partner]') and type = 'u')
	DROP TABLE [dbo].[Partner]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[SkinPreview]') and type = 'u')
	DROP TABLE [dbo].[SkinPreview]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[SQLLog]') and type = 'u')
	DROP TABLE [dbo].[SQLLog]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[Staff]') and type = 'u')
	DROP TABLE [dbo].[Staff]
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GenerateCreatesForAllIndexes') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GenerateCreatesForAllIndexes]
GO

CREATE PROC [dbo].[aspdnsf_GenerateCreatesForAllIndexes]

AS
BEGIN

			SELECT TABLE_NAME = OBJECT_NAME(i.id)
				 , INDEX_NAME = i.name
				 , COLUMN_LIST = dbo.GetIndexColumns(OBJECT_NAME(i.id), i.id, i.indid)
				 , IS_CLUSTERED = INDEXPROPERTY(i.id, i.name, 'IsClustered')
				 , IS_UNIQUE = INDEXPROPERTY(i.id, i.name, 'IsUnique')
              INTO #AllIndexes
              FROM dbo.sysindexes i
        INNER JOIN dbo.sysfilegroups g
                ON i.groupid = g.groupid
             WHERE (i.indid BETWEEN 1 AND 254)
                -- leave out AUTO_STATISTICS:
               AND (i.Status & 64)=0
                -- leave out system tables:
               AND OBJECTPROPERTY(i.id, 'IsMsShipped') = 0
		    SELECT CASE WHEN T.TABLE_NAME IS NULL THEN
                             'CREATE '
                             + CASE IS_UNIQUE WHEN 1 THEN 'UNIQUE ' ELSE '' END
                             + CASE IS_CLUSTERED WHEN 1 THEN 'CLUSTERED ' ELSE '' END
                             + 'INDEX [' + INDEX_NAME + '] ON [' + v.TABLE_NAME + ']'
                             + '(' + COLUMN_LIST + ');'
                         END
                        FROM #AllIndexes v
             LEFT OUTER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS T
                          ON T.CONSTRAINT_NAME = v.INDEX_NAME
                         AND T.TABLE_NAME = v.TABLE_NAME
                       WHERE INDEX_Name LIKE 'IX_%'
					      OR INDEX_NAME LIKE 'UIX_%'
						  OR INDEX_NAME LIKE 'CIX_%'
                    ORDER BY v.TABLE_NAME, IS_CLUSTERED DESC
END

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GenerateUpdatesForAllIndexes') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GenerateUpdatesForAllIndexes]
GO

CREATE PROC [dbo].[aspdnsf_GenerateUpdatesForAllIndexes]

AS
BEGIN

			SELECT TABLE_NAME = OBJECT_NAME(i.id)
				 , INDEX_NAME = i.name
				 , COLUMN_LIST = dbo.GetIndexColumns(OBJECT_NAME(i.id), i.id, i.indid)
				 , IS_CLUSTERED = INDEXPROPERTY(i.id, i.name, 'IsClustered')
				 , IS_UNIQUE = INDEXPROPERTY(i.id, i.name, 'IsUnique')
              INTO #AllIndexes
              FROM dbo.sysindexes i
        INNER JOIN dbo.sysfilegroups g
                ON i.groupid = g.groupid
             WHERE (i.indid BETWEEN 1 AND 254)
                -- leave out AUTO_STATISTICS:
               AND (i.Status & 64)=0
                -- leave out system tables:
               AND OBJECTPROPERTY(i.id, 'IsMsShipped') = 0
		    SELECT CASE WHEN T.TABLE_NAME IS NULL THEN
							 'IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N''[dbo].[' + v.TABLE_NAME + ']'') AND name = N''' + INDEX_NAME + ''') CREATE '
                             + CASE IS_UNIQUE WHEN 1 THEN 'UNIQUE ' ELSE '' END
                             + CASE IS_CLUSTERED WHEN 1 THEN 'CLUSTERED ' ELSE '' END
                             + 'INDEX [' + INDEX_NAME + '] ON [' + v.TABLE_NAME + ']'
                             + '(' + COLUMN_LIST + ');'
                         END
                        FROM #AllIndexes v
             LEFT OUTER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS T
                          ON T.CONSTRAINT_NAME = v.INDEX_NAME
                         AND T.TABLE_NAME = v.TABLE_NAME
                       WHERE INDEX_Name LIKE 'IX_%'
					      OR INDEX_NAME LIKE 'UIX_%'
						  OR INDEX_NAME LIKE 'CIX_%'
                    ORDER BY v.TABLE_NAME, IS_CLUSTERED DESC
END

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_CreateIndexes') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_CreateIndexes]
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
CREATE INDEX [IX_Product_IsAPack] ON [Product](IsAPack);
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

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_UpdateIndexes') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_UpdateIndexes]
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
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND name = N'IX_Product_IsAPack') CREATE INDEX [IX_Product_IsAPack] ON [Product](IsAPack);
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

--New sprocs for the splash page graphs
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_SalesForChart') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_SalesForChart]
GO

CREATE PROCEDURE [dbo].[aspdnsf_SalesForChart]
    @StartDate DATETIME,
    @EndDate DATETIME,
    @NumIntervals INT
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @Totals TABLE (EndDate DATETIME, Sales MONEY)
	DECLARE @SecondInterval INT
	DECLARE @ThisIntervalStart DATETIME
	DECLARE @ThisIntervalStop DATETIME
	DECLARE @CurrentInterval INT
	DECLARE @ThisIntervalValue MONEY

	SET @ThisIntervalStop = @EndDate
	SET @CurrentInterval = 0
	SET @SecondInterval = DATEDIFF(second,@StartDate,@EndDate) / @NumIntervals
	SET @ThisIntervalStart = DATEADD(second,-@SecondInterval,@EndDate)

	WHILE @CurrentInterval < @NumIntervals
	BEGIN
		SELECT @ThisIntervalValue = SUM(OrderTotal)
		FROM Orders
		WHERE Orders.OrderDate > @ThisIntervalStart
			AND Orders.OrderDate < @ThisIntervalStop
			AND Orders.TransactionState = 'CAPTURED'
			AND Orders.Deleted = 0

		INSERT INTO @Totals VALUES(@ThisIntervalStop, ISNULL(@ThisIntervalValue,0))

		SET @CurrentInterval = @CurrentInterval + 1
		SET @ThisIntervalStop = DATEADD(second,-@SecondInterval,@ThisIntervalStop)
		SET @ThisIntervalStart = DATEADD(second,-@SecondInterval,@ThisIntervalStop)
	END

SELECT * FROM @Totals ORDER BY EndDate
END
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_TopProducts') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_TopProducts]
GO

CREATE PROCEDURE [dbo].[aspdnsf_TopProducts]
    @StartDate DATETIME,
    @EndDate DATETIME,
	@CountOfProducts INT
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @Totals TABLE (ProductName NVARCHAR(MAX), Sales MONEY, ProductID INT)

	INSERT INTO @Totals (Sales, ProductName, ProductID)
	SELECT TOP (@CountOfProducts) SUM(os.OrderedProductPrice), os.OrderedProductName, os.ProductID
		FROM Orders_ShoppingCart os
			LEFT JOIN Orders o ON os.OrderNumber = o.OrderNumber
		WHERE o.TransactionState = 'CAPTURED'
			AND o.OrderDate > @StartDate
			AND o.OrderDate < @EndDate
			AND os.IsSystem = 0
			AND o.Deleted = 0
		GROUP BY os.ProductID, os.OrderedProductName

	SELECT * FROM @Totals ORDER BY Sales DESC
END
GO

--Spec cleanup
IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Document]') AND name = 'SpecTitle')
	BEGIN
		ALTER TABLE [dbo].[Document] DROP COLUMN SpecTitle
	END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'DF_Product_SpecTitle') AND parent_object_id = OBJECT_ID(N'[dbo].[Product]'))
	BEGIN
		ALTER TABLE [dbo].[Product] DROP CONSTRAINT DF_Product_SpecTitle
	END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Product]') AND name = 'SpecTitle')
	BEGIN
		ALTER TABLE [dbo].Product DROP COLUMN SpecTitle
	END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Product]') AND name = 'SpecCall')
	BEGIN
		ALTER TABLE [dbo].Product DROP COLUMN SpecCall
	END
GO


IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Product]') AND name = 'SpecsInline')
	BEGIN
		ALTER TABLE [dbo].Product DROP CONSTRAINT DF_Product_SpecsInline
		ALTER TABLE [dbo].Product DROP COLUMN SpecsInline
	END
GO

/*********** Begin AlphaFilter Removal ********************/
PRINT 'AlphaFilter Removal BEGIN'
PRINT 'AlphaFilter Removal - Update [aspdnsf_GetMappedObjects]'
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetMappedObjects]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetMappedObjects]
GO

CREATE procedure [dbo].[aspdnsf_GetMappedObjects](
	@StoreId int,
	@EntityType nvarchar(30),
	@SearchFilter nvarchar(30) = null,
	@pageSize int = null,
	@page int = null,
	@IsLegacyCacheMechanism bit = 1.
)
as
begin

	-- In an effort to elliminate the insanly slow load time of the store entity caching mechanism, the default returns of this stored procedure returns bunk data.
	-- In admin, the entity object mapping controls will switch this off to work correctly.
	if (@IsLegacyCacheMechanism = 1)
		begin

			select	0 as TotalCount, 0 as PageSize, 0 as CurrentPage, 0 as TotalPages, 0 as StartIndex, 0 as EndIndex
			select	0 as StoreID, 0, '' as EntityType, 0 as [ID], '' as [Name], 0 as Mapped

		end
	else
		begin

		declare @count int
		declare @allPages int
		declare @start int
		declare @end int

		-- flag to determine if we should do paging
		declare @doPage bit
		set @doPage = case when @pageSize is null and @page is null then 0 else 1 end

		-- execute query to fetch the count of all availalble data
		-- which we will use later on to get the paging information
		select @count = count(*)
		from
		(
			select	o.EntityType,
					o.[Id],
					o.[Name]
			from ObjectView o
			where	o.EntityType = COALESCE(@EntityType, o.EntityType) and
					(@SearchFilter IS NULL OR (o.[Name] like '%' + @SearchFilter + '%'))
		) ov

		-- sanity check
		if(@count <= @pageSize) set @pageSize = @count

		-- determine start and end index
		set @start = ((@page - 1) * @pageSize) + 1
		set @end = (@start + @pageSize) - 1
		if(@end > @count) set @end = @count

		-- compute the total number of pages
		if(@count > 0 )
		begin
			set @allPages = @count / @pageSize

			declare @rem int
			set @rem = @count % @pageSize -- residue
			if(@rem > 0) set @allPages = @allPages + 1
		end
		else set @allPages = 0

		-- paging information
		select	@count as TotalCount,
				@pageSize as PageSize,
				@page as CurrentPage,
				@allPages as TotalPages,
				@start as StartIndex,
				@end as [EndIndex]

		-- actual paged result set
		select	@StoreId as StoreID,
				ROW_NUMBER,
				ov.EntityType,
				ov.[ID],
				ov.[Name],
				dbo.GetStoreMap(@StoreId, ov.EntityType, ov.ID) as Mapped
		from
		(
			select	ROW_NUMBER() over(partition by o.EntityType order by id) as [Row_Number],
					o.EntityType,
					o.[Id],
					o.[Name]
			from ObjectView o
			where	o.EntityType = COALESCE(@EntityType, o.EntityType) and
					(@SearchFilter IS NULL OR (o.[Name] like '%' + @SearchFilter + '%'))
		) ov
		where @doPage = 0 or (ROW_NUMBER BETWEEN @start AND @end)
	end
end

GO
PRINT 'AlphaFilter Removal - Update [aspdnsf_GetObjects]'
GO
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetObjects]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetObjects]
GO

CREATE procedure [dbo].[aspdnsf_GetObjects](
	@EntityType nvarchar(30),
	@SearchFilter nvarchar(30) = null,
	@pageSize int = null,
	@page int = null
)
as
begin

	declare @count int
	declare @allPages int
	declare @start int
	declare @end int

	-- flag to determine if we should do paging
	declare @doPage bit
	set @doPage = case when @pageSize is null and @page is null then 0 else 1 end

	-- execute query to fetch the count of all availalble data
	-- which we will use later on to get the paging information
	select @count = count(*)
	from
	(
		select	o.EntityType,
				o.[Id],
				o.[Name]
		from ObjectView o
		where	o.EntityType = COALESCE(@EntityType, o.EntityType) and
				(@SearchFilter IS NULL OR (o.[Name] like '%' + @SearchFilter + '%'))
	) ov

	-- sanity check
	if(@count <= @pageSize) set @pageSize = @count

	-- determine start and end index
	set @start = ((@page - 1) * @pageSize) + 1
	set @end = (@start + @pageSize) - 1
	if(@end > @count) set @end = @count

	-- compute the total number of pages
	if(@count > 0 )
	begin
		set @allPages = @count / @pageSize

		declare @rem int
		set @rem = @count % @pageSize -- residue
		if(@rem > 0) set @allPages = @allPages + 1
	end
	else set @allPages = 0


	-- paging information
	select	@count as TotalCount,
			@pageSize as PageSize,
			@page as CurrentPage,
			@allPages as TotalPages,
			@start as StartIndex,
			@end as [EndIndex]

	-- actual paged result set
	select	ROW_NUMBER,
			ov.EntityType,
			ov.[ID],
			ov.[Name]
	from
	(
		select	ROW_NUMBER() over(partition by o.EntityType order by id) as [Row_Number],
				o.EntityType,
				o.[Id],
				o.[Name],
				o.Description
		from ObjectView o
		where	o.EntityType = COALESCE(@EntityType, o.EntityType) and
				(@SearchFilter IS NULL OR (o.[Name] like '%' + @SearchFilter + '%'))
	) ov
	where @doPage = 0 or (ROW_NUMBER BETWEEN @start AND @end)

end

GO
PRINT 'AlphaFilter Removal - Update [aspdnsf_GetProductWithVariants]'
GO
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetProductWithVariants]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetProductWithVariants]
GO


CREATE procedure [dbo].[aspdnsf_GetProductWithVariants](
	@SearchFilter nvarchar(30) = null,
	@pageSize int = null,
	@page int = null
)
as
begin

	declare @count int
	declare @allPages int
	declare @start int
	declare @end int

	-- flag to determine if we should do paging
	declare @doPage bit
	set @doPage = case when @pageSize is null and @page is null then 0 else 1 end

	-- execute query to fetch the count of all availalble data
	-- which we will use later on to get the paging information
	select @count = count(*)
	from
	(
		select	p.ProductId,
				p.[Name],
				p.Description,
				p.Published,
				p.Deleted
		from Product p with (nolock)
		where	(@SearchFilter IS NULL OR (p.[Name] like '%' + @SearchFilter + '%'))
	) p

	-- sanity check
	if(@count <= @pageSize) set @pageSize = @count

	-- determine start and end index
	set @start = ((@page - 1) * @pageSize) + 1
	set @end = (@start + @pageSize) - 1
	if(@end > @count) set @end = @count

	-- compute the total number of pages
	if(@count > 0 )
	begin
		set @allPages = @count / @pageSize

		declare @rem int
		set @rem = @count % @pageSize -- residue
		if(@rem > 0) set @allPages = @allPages + 1
	end
	else set @allPages = 0


	-- paging information
	select	@count as TotalCount,
			@pageSize as PageSize,
			@page as CurrentPage,
			@allPages as TotalPages,
			@start as StartIndex,
			@end as [EndIndex]

	-- actual paged result set
	select	p.ProductId,
			p.[Name],
			p.Description,
			p.Published,
			p.Deleted
	from
	(
		select	ROW_NUMBER() over(order by p.ProductId) as [Row_Number],
				p.ProductId,
				p.[Name],
				p.Description,
				p.Published,
				p.Deleted
		from Product p with (nolock)
		where	(@SearchFilter IS NULL OR (p.[Name] like '%' + @SearchFilter + '%'))
	) p
	where @doPage = 0 or (ROW_NUMBER BETWEEN @start AND @end)

	-- next result set would be the variants that are tied to the previous products result set
	select	pv.VariantId,
			pv.ProductId,
			pv.[Name],
			pv.Description,
			pv.Published,
			pv.Deleted,
			pv.IsDefault,
			pv.Inventory,
			pv.Price,
			pv.SalePrice,
			pv.Weight
	from ProductVariant pv with (nolock)
	inner join
	(
		select	p.ProductId
		from
		(
			select	ROW_NUMBER() over(order by p.ProductId) as [Row_Number],
					p.ProductId,
					p.[Name],
					p.Description,
					p.Published,
					p.Deleted
			from Product p with (nolock)
			where	(@SearchFilter IS NULL OR (p.[Name] like '%' + @SearchFilter + '%'))
		) p
		where @doPage = 0 or (ROW_NUMBER BETWEEN @start AND @end)
	) p on p.ProductId = pv.ProductId

end
GO
PRINT 'AlphaFilter Removal END'
/*********** END AlphaFilter Removal ********************/
GO

/*Adding a sproc for recursive entity data*/

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_SingleEntityTree]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_SingleEntityTree]
GO
CREATE PROCEDURE [dbo].[aspdnsf_SingleEntityTree]
	@EntityID int,
    @EntityType varchar(50)
AS
BEGIN
    IF @EntityType = 'Category' BEGIN
		;WITH EntityTree (ParentEntityID, EntityID, Name, ParentName, Level)
		AS
		(
		-- Anchor member definition
			SELECT e.ParentCategoryID as ParentEntityID, e.CategoryID as EntityID, Name, convert(nvarchar(max), '') as ParentName, 0 AS Level
			FROM dbo.Category e
			WHERE CategoryID = @EntityID
			UNION ALL
		-- Recursive member definition
			SELECT e.ParentCategoryID as ParentEntityID, e.CategoryID as EntityID, e.Name, convert(nvarchar(max), p.Name) as ParentName, Level + 1
			FROM dbo.Category e
			INNER JOIN EntityTree p
				ON e.CategoryID = p.ParentEntityID
		)
		-- Statement that executes the CTE
		SELECT ParentEntityID, EntityID, Name, ParentName, Level
		FROM EntityTree
		Order By Level Desc
	END

	IF @EntityType = 'Affiliate' BEGIN
		;WITH EntityTree (ParentEntityID, EntityID, Name, ParentName, Level)
		AS
		(
		-- Anchor member definition
			SELECT e.ParentAffiliateID as ParentEntityID, e.AffiliateID as EntityID, Name, convert(nvarchar(max), '') as ParentName, 0 AS Level
			FROM dbo.Affiliate e
			WHERE AffiliateID = @EntityID
			UNION ALL
		-- Recursive member definition
			SELECT e.ParentAffiliateID as ParentEntityID, e.AffiliateID as EntityID, e.Name, convert(nvarchar(max), p.Name) as ParentName, Level + 1
			FROM dbo.Affiliate e
			INNER JOIN EntityTree p
				ON e.AffiliateID = p.ParentEntityID
		)
		-- Statement that executes the CTE
		SELECT ParentEntityID, EntityID, Name, ParentName, Level
		FROM EntityTree
		Order By Level Desc
	END

	IF @EntityType = 'Section' BEGIN
		;WITH EntityTree (ParentEntityID, EntityID, Name, ParentName, Level)
		AS
		(
		-- Anchor member definition
			SELECT e.ParentSectionID as ParentEntityID, e.SectionID as EntityID, Name, convert(nvarchar(max), '') as ParentName, 0 AS Level
			FROM dbo.Section e
			WHERE SectionID = @EntityID
			UNION ALL
		-- Recursive member definition
			SELECT e.ParentSectionID as ParentEntityID, e.SectionID as EntityID, e.Name, convert(nvarchar(max), p.Name) as ParentName, Level + 1
			FROM dbo.Section e
			INNER JOIN EntityTree p
				ON e.SectionID = p.ParentEntityID
		)
		-- Statement that executes the CTE
		SELECT ParentEntityID, EntityID, Name, ParentName, Level
		FROM EntityTree
		Order By Level Desc
	END

	IF @EntityType = 'Manufacturer' BEGIN
		;WITH EntityTree (ParentEntityID, EntityID, Name, ParentName, Level)
		AS
		(
		-- Anchor member definition
			SELECT e.ParentManufacturerID as ParentEntityID, e.ManufacturerID as EntityID, Name, convert(nvarchar(max), '') as ParentName, 0 AS Level
			FROM dbo.Manufacturer e
			WHERE ManufacturerID = @EntityID
			UNION ALL
		-- Recursive member definition
			SELECT e.ParentManufacturerID as ParentEntityID, e.ManufacturerID as EntityID, e.Name, convert(nvarchar(max), p.Name) as ParentName, Level + 1
			FROM dbo.Manufacturer e
			INNER JOIN EntityTree p
				ON e.ManufacturerID = p.ParentEntityID
		)
		-- Statement that executes the CTE
		SELECT ParentEntityID, EntityID, Name, ParentName, Level
		FROM EntityTree
		Order By Level Desc
	END

	IF @EntityType = 'Distributor' BEGIN
		;WITH EntityTree (ParentEntityID, EntityID, Name, ParentName, Level)
		AS
		(
		-- Anchor member definition
			SELECT e.ParentDistributorID as ParentEntityID, e.DistributorID as EntityID, Name, convert(nvarchar(max), '') as ParentName, 0 AS Level
			FROM dbo.Distributor e
			WHERE DistributorID = @EntityID
			UNION ALL
		-- Recursive member definition
			SELECT e.ParentDistributorID as ParentEntityID, e.DistributorID as EntityID, e.Name, convert(nvarchar(max), p.Name) as ParentName, Level + 1
			FROM dbo.Distributor e
			INNER JOIN EntityTree p
				ON e.DistributorID = p.ParentEntityID
		)
		-- Statement that executes the CTE
		SELECT ParentEntityID, EntityID, Name, ParentName, Level
		FROM EntityTree
		Order By Level Desc
	END

	IF @EntityType = 'Customerlevel' BEGIN
		;WITH EntityTree (ParentEntityID, EntityID, Name, ParentName, Level)
		AS
		(
		-- Anchor member definition
			SELECT e.ParentCustomerlevelID as ParentEntityID, e.CustomerlevelID as EntityID, Name, convert(nvarchar(max), '') as ParentName, 0 AS Level
			FROM dbo.Customerlevel e
			WHERE CustomerlevelID = @EntityID
			UNION ALL
		-- Recursive member definition
			SELECT e.ParentCustomerlevelID as ParentEntityID, e.CustomerlevelID as EntityID, e.Name, convert(nvarchar(max), p.Name) as ParentName, Level + 1
			FROM dbo.Customerlevel e
			INNER JOIN EntityTree p
				ON e.CustomerlevelID = p.ParentEntityID
		)
		-- Statement that executes the CTE
		SELECT ParentEntityID, EntityID, Name, ParentName, Level
		FROM EntityTree
		Order By Level Desc
	END

END
GO

/*********** ML Data Parsing Functions ********************/
if exists (select * from dbo.sysobjects where id = object_id('dbo.ParseMlLocales') and objectproperty(id, 'IsTableFunction') = 1)
	drop function dbo.ParseMlLocales
go

create function dbo.ParseMlLocales(@value nvarchar(max))
	returns @locales TABLE (
		Locale nvarchar(max),
		Value nvarchar(max)
	)
as begin

	if(left(@value, 4) = '<ml>') begin
		declare @localeXml xml = cast(@value as xml)

		insert into @locales
		select
			T.locale.value('(@name)[1]', 'nvarchar(max)'),
			T.locale.value('(text())[1]', 'nvarchar(max)')
		from
			@localeXml.nodes('ml/locale') AS T(locale);
	end
	else begin
		insert into @locales
		values (null, @value)
	end

	return
end
go

if exists (select * from dbo.sysobjects where id = object_id('dbo.GetMlValue') and objectproperty(id, 'IsScalarFunction') = 1)
	drop function dbo.GetMlValue
go

create function dbo.GetMlValue(@value nvarchar(max), @locale nvarchar(max))
	returns nvarchar(max)
as begin

	declare @localeValue nvarchar(max)

	select @localeValue = Value
	from dbo.ParseMlLocales(@value)
	where Locale = @locale or Locale is null

	return @localeValue
end
go

if exists (select * from dbo.sysobjects where id = object_id('dbo.SetMlValue') and objectproperty(id, 'IsScalarFunction') = 1)
	drop function dbo.SetMlValue
go

create function dbo.SetMlValue(@originalValue nvarchar(max), @newValue nvarchar(max), @locale nvarchar(max))
	returns nvarchar(max)
as begin

	if(left(@originalValue, 4) = '<ml>') begin
		declare @localeXml xml = cast(@originalValue as xml)

		set @localeXml.modify('delete /ml/locale[@name = sql:variable("@locale")]')
		set @localeXml.modify('insert <locale name="{sql:variable(''@locale'')}">{sql:variable("@newValue")}</locale> into (/ml)[1]')

		return cast(@localeXml as nvarchar(max))
	end
	else begin
		return @newValue
	end

	return @newValue
end
go

/*********** END ML Data Parsing Functions ********************/

/*********** BEGIN TopicMapping Removal ******************************/
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'TopicMapping_Updated'))
DROP TRIGGER [dbo].[TopicMapping_Updated]

GO

GO
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_SaveTopicMap]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_SaveTopicMap]
GO

IF EXISTS (select * From sysobjects where id = object_id('[dbo].[TopicMapping]') and type = 'u')
DROP TABLE [dbo].[TopicMapping]

GO
/*********** END TopicMapping Removal ********************************/

/*********** BEGIN Affiliate Cleanup ******************************/
IF EXISTS (select * From sysobjects where id = object_id('[dbo].[AffiliateActivityReason]') and type = 'u')
DROP TABLE [dbo].[AffiliateActivityReason]
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Affiliate]') AND name = 'TrackingOnly')
	BEGIN
		ALTER TABLE [dbo].[Affiliate] DROP CONSTRAINT DF_Affiliate_TrackingOnly
		ALTER TABLE [dbo].[Affiliate] DROP COLUMN TrackingOnly
	END
GO
IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Affiliate]') AND name = 'DateOfBirth')
	BEGIN
		ALTER TABLE [dbo].[Affiliate] DROP COLUMN DateOfBirth
	END
GO
IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Affiliate]') AND name = 'Gender')
	BEGIN
		ALTER TABLE [dbo].[Affiliate] DROP COLUMN Gender
	END
GO

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

set @AffiliateID = @@identity

insert into AffiliateStore (AffiliateID, StoreID, CreatedOn) values (@AffiliateID, @StoreID, GETDATE())
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_getAffiliate]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_getAffiliate]
GO
CREATE PROC dbo.aspdnsf_getAffiliate
    @AffiliateID int = null

AS
SET NOCOUNT ON

SELECT a.AffiliateID, AffiliateGUID, EMail, Password, Notes, IsOnline, FirstName, LastName, Name, Company, Address1, Address2, Suite, City, State, Zip, Country, Phone, WebSiteName,
WebSiteDescription, URL, DefaultSkinID, ParentAffiliateID, DisplayOrder, ExtensionData, SEName, SETitle, SEAltText, SEKeywords, SEDescription, Published, Wholesale, Deleted, a.CreatedOn, SaltKey, StoreID
FROM dbo.Affiliate a with (nolock) left join AffiliateStore b with (nolock) on a.AffiliateID = b.AffiliateID
WHERE a.AffiliateID = COALESCE(@AffiliateID, a.AffiliateID)
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_updAffiliate]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_updAffiliate]
GO
create proc dbo.aspdnsf_updAffiliate
    @AffiliateID int,
    @EMail nvarchar(100) = null,
    @Password nvarchar(250) = null,
    @Notes nvarchar(max) = null,
    @IsOnline tinyint = null,
    @FirstName nvarchar(100) = null,
    @LastName nvarchar(100) = null,
    @Name nvarchar(100) = null,
    @Company nvarchar(100) = null,
    @Address1 nvarchar(100) = null,
    @Address2 nvarchar(100) = null,
    @Suite nvarchar(50) = null,
    @City nvarchar(100) = null,
    @State nvarchar(100) = null,
    @Zip nvarchar(10) = null,
    @Country nvarchar(100) = null,
    @Phone nvarchar(25) = null,
    @WebSiteName nvarchar(100) = null,
    @WebSiteDescription nvarchar(max) = null,
    @URL nvarchar(max) = null,
    @DefaultSkinID int = null,
    @ParentAffiliateID int = null,
    @DisplayOrder int = null,
    @ExtensionData nvarchar(max) = null,
    @SEName nvarchar(100) = null,
    @SETitle nvarchar(max) = null,
    @SEAltText nvarchar(max) = null,
    @SEKeywords nvarchar(max) = null,
    @SEDescription nvarchar(max) = null,
    @Published tinyint = null,
    @Wholesale tinyint = null,
    @Deleted tinyint = null,
    @SaltKey int = null

AS
SET NOCOUNT ON

UPDATE dbo.Affiliate
SET
    EMail = COALESCE(@EMail, EMail),
    Password = COALESCE(@Password, Password),
    Notes = COALESCE(@Notes, Notes),
    IsOnline = COALESCE(@IsOnline, IsOnline),
    FirstName = COALESCE(@FirstName, FirstName),
    LastName = COALESCE(@LastName, LastName),
    Name = COALESCE(@Name, Name),
    Company = COALESCE(@Company, Company),
    Address1 = COALESCE(@Address1, Address1),
    Address2 = COALESCE(@Address2, Address2),
    Suite = COALESCE(@Suite, Suite),
    City = COALESCE(@City, City),
    State = COALESCE(@State, State),
    Zip = COALESCE(@Zip, Zip),
    Country = COALESCE(@Country, Country),
    Phone = COALESCE(@Phone, Phone),
    WebSiteName = COALESCE(@WebSiteName, WebSiteName),
    WebSiteDescription = COALESCE(@WebSiteDescription, WebSiteDescription),
    URL = COALESCE(@URL, URL),
    DefaultSkinID = COALESCE(@DefaultSkinID, DefaultSkinID),
    ParentAffiliateID = COALESCE(@ParentAffiliateID, ParentAffiliateID),
    DisplayOrder = COALESCE(@DisplayOrder, DisplayOrder),
    ExtensionData = COALESCE(@ExtensionData, ExtensionData),
    SEName = COALESCE(@SEName, SEName),
    SETitle = COALESCE(@SETitle, SETitle),
    SEAltText = COALESCE(@SEAltText, SEAltText),
    SEKeywords = COALESCE(@SEKeywords, SEKeywords),
    SEDescription = COALESCE(@SEDescription, SEDescription),
    Published = COALESCE(@Published, Published),
    Wholesale = COALESCE(@Wholesale, Wholesale),
    Deleted = COALESCE(@Deleted, Deleted),
    SaltKey = COALESCE(@SaltKey, SaltKey)
WHERE AffiliateID = @AffiliateID
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_getAffiliateByEmail]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_getAffiliateByEmail]
GO
CREATE PROC dbo.aspdnsf_getAffiliateByEmail
    @AffiliateEmail nvarchar(100)

AS
SET NOCOUNT ON

SELECT a.AffiliateID, AffiliateGUID, EMail, Password, Notes, IsOnline, FirstName, LastName, Name, Company, Address1, Address2, Suite, City, State, Zip, Country, Phone, WebSiteName, WebSiteDescription, URL,
DefaultSkinID, ParentAffiliateID, DisplayOrder, ExtensionData, SEName, SETitle, SEAltText, SEKeywords, SEDescription, Published, Wholesale, Deleted, a.CreatedOn, SaltKey, StoreID
FROM dbo.Affiliate a with (nolock) left join AffiliateStore b with (nolock) on a.AffiliateID = b.AffiliateID
WHERE EMail = @AffiliateEmail
GO

/*********** END Affiliate Cleanup ********************************/

--PaymentMethods AppConfig update
UPDATE dbo.[AppConfig] SET Description = 'Choose the payment methods you want to allow on the store.  Multiple values can be selected.' WHERE [Name] = 'PaymentMethods'
UPDATE dbo.[AppConfig] SET ValueType = 'multiselect', AllowableValues = 'Credit Card,PayPalExpress,PayPal,Request Quote,Purchase Order,Check By Mail,C.O.D.,C.O.D. (Money Order),C.O.D. (Company Check),C.O.D. (Net 30),ECheck,MicroPay,CheckoutByAmazon,PayPal Payments Advanced' WHERE [Name] = 'PaymentMethods'

GO

/*********** Create entities view ******************************/
IF EXISTS(select * FROM sys.views where name = 'Entities')
begin
	drop view [dbo].[Entities]
end
go
CREATE VIEW [dbo].[Entities]

AS
    SELECT 'category' EntityType, Entity.CategoryID EntityID, Entity.CategoryGUID EntityGuid, Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentCategoryID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,Deleted, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Category Entity with (NOLOCK)

    UNION ALL

    SELECT 'affiliate' EntityType, Entity.AffiliateID EntityID,Entity.AffiliateGUID EntityGuid, Name,4 as ColWidth,'' as Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentAffiliateID ParentEntityID,DisplayOrder,0 as SortByLooks,'' as XmlPackage,Published,Deleted, 0 PageSize, 0 QuantityDiscountID, '' Summary, SkinID, TemplateName
    FROM dbo.Affiliate Entity with (NOLOCK)

    UNION ALL

    SELECT 'section' EntityType, Entity.SectionID EntityID,Entity.SectionGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentSectionID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,Deleted, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Section Entity with (NOLOCK)

    UNION ALL

    SELECT 'manufacturer' EntityType, Entity.ManufacturerID EntityID,Entity.ManufacturerGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentManufacturerID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,Deleted, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Manufacturer Entity with (NOLOCK)

    UNION ALL

    SELECT 'library' EntityType, Entity.LibraryID EntityID,Entity.LibraryGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentLibraryID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,Deleted, PageSize, 0 QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Library Entity with (NOLOCK)

    UNION ALL

    SELECT 'distributor' EntityType, Entity.DistributorID EntityID,Entity.DistributorGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentDistributorID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,Deleted, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Distributor Entity with (NOLOCK)

    UNION ALL

    SELECT 'genre' EntityType, Entity.GenreID EntityID,Entity.GenreGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentGenreID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,Deleted, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Genre Entity with (NOLOCK)

    UNION ALL

    SELECT 'vector' EntityType, Entity.VectorID EntityID,Entity.VectorGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SEAltText,ParentVectorID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,Deleted, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
    FROM dbo.Vector Entity with (NOLOCK)

    UNION ALL

    SELECT 'customerLevel' EntityType, Entity.CustomerLevelID EntityID,Entity.CustomerLevelGUID EntityGuid,Name, 4 ColWidth, '' Description,SEName, '' SEKeywords, '' SEDescription, '' SETitle,'' SEAltText,ParentCustomerLevelID ParentEntityID,DisplayOrder,0 SortByLooks, '' XmlPackage, 1 Published,Deleted, 20 PageSize, 0 QuantityDiscountID, '' Summary, SkinID, TemplateName
    FROM dbo.CustomerLevel Entity with (NOLOCK)

GO

/*********** end create entities view ******************************/

/*********** begin create get entity path function ******************************/
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetEntityPath]') and type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
  drop function dbo.GetEntityPath
go

create function dbo.GetEntityPath (
	@entityId int,
	@entityType nvarchar(50),
	@separator nvarchar(max),
	@selectedLocale nvarchar(10),
	@defaultLocale nvarchar(10)
)
returns nvarchar(max)
as begin
	declare @result nvarchar(max),
			@selectedLocaleId int,
			@defaultLocaleId int,
			@maxRecursion int = 99

	select @selectedLocaleId = LocaleSettingID from LocaleSetting where Name = @selectedLocale
	select @defaultLocaleId = LocaleSettingID from LocaleSetting where Name = @defaultLocale

	-- Build a list of entity ID's
	;with EntityTree(EntityId, ParentEntityId, [Level]) as (
		select	-- Anchor
			EntityID,
			ParentEntityID,
			0 AS [Level]
		from
			dbo.Entities
		where
			EntityType = @EntityType
			and EntityID = @EntityID
		union all
		select	-- Recursion
			parent.EntityID,
			parent.ParentEntityId,
			child.[Level] + 1 [Level]
		from dbo.Entities parent
			inner join EntityTree child on
				parent.EntityID = child.ParentEntityID
				and parent.EntityType = @entityType
		where
			child.[Level] <= @maxRecursion)
	-- Form the entity ID's into a string of localized path names
	select
		@result =
			isnull(@result, '')
			+ case when @result is null then '' else @separator end
			+ coalesce(SelectedLocalization.LocalizedName, DefaultLocalization.LocalizedName, UnspecifiedLocalization.LocalizedName)
	from
		EntityTree
		left join (
			select ObjectId, LocalizedName
			from dbo.LocalizedObjectName
			where ObjectType = @entityType and LocaleId = @selectedLocaleId
			) as SelectedLocalization on EntityTree.EntityID = SelectedLocalization.ObjectId
		left join (
			select ObjectId, LocalizedName
			from dbo.LocalizedObjectName
			where ObjectType = @entityType and LocaleId = @defaultLocaleId
			) as DefaultLocalization on EntityTree.EntityID = DefaultLocalization.ObjectId
		left join (
			select ObjectId, LocalizedName
			from dbo.LocalizedObjectName
			where ObjectType = @entityType and LocaleId is null
			) as UnspecifiedLocalization on EntityTree.EntityID = UnspecifiedLocalization.ObjectId
	where [Level] <= @maxRecursion
	order by [Level] desc

	return @Result
end
go
/*********** end create get entity path function ******************************/

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_MakeStoreDefault') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_MakeStoreDefault
GO

create procedure [dbo].[aspdnsf_MakeStoreDefault]
	@StoreID int
as
begin
	set nocount on;

	update Store set IsDefault = Case when(StoreId = @StoreId) then 1 else 0 end

end

GO

/*********** begin update map sproc to handle all entities ******************************/
DROP PROCEDURE [dbo].[aspdnsf_Map];
GO

CREATE PROC [dbo].[aspdnsf_Map]
	@CreateMap BIT = null,
	@RemoveMap BIT = null,
	@Mapped BIT = null,
	@StoreID INT,
	@EntityID INT,
	@EntityType VARCHAR(100)

AS

IF @EntityType = 'Affiliate'
BEGIN
	INSERT INTO AffiliateStore (StoreID, AffiliateID)
	SELECT @StoreID, @EntityID
	WHERE
		COALESCE(@CreateMap, @Mapped, 0) = 1
		AND NOT EXISTS(SELECT * FROM AffiliateStore WHERE @StoreID = StoreID AND AffiliateID = @EntityID)

	DELETE FROM AffiliateStore
	WHERE
		COALESCE(@RemoveMap, ~@Mapped, 0) = 1
		AND AffiliateID = @EntityID
		AND StoreID = @StoreID
END
ELSE IF @EntityType = 'GiftCard'
BEGIN
	INSERT INTO GiftCardStore (StoreID, GiftCardID)
	SELECT @StoreID, @EntityID
	WHERE
		COALESCE(@CreateMap, @Mapped, 0) = 1
		AND NOT EXISTS(SELECT * FROM GiftCardStore WHERE @StoreID = StoreID AND GiftCardID = @EntityID)

	DELETE FROM GiftCardStore
	WHERE
		COALESCE(@RemoveMap, ~@Mapped, 0) = 1
		AND GiftCardID = @EntityID
		AND StoreID = @StoreID
END
ELSE IF @EntityType = 'News'
BEGIN
	INSERT INTO NewsStore(StoreID, NewsID)
	SELECT @StoreID, @EntityID
	WHERE
		COALESCE(@CreateMap, @Mapped, 0) = 1
		AND NOT EXISTS (SELECT * FROM NewsStore WHERE StoreID = @StoreID AND NewsID = @EntityID)

	DELETE FROM NewsStore
	WHERE
		COALESCE(@RemoveMap, ~@Mapped, 0) = 1
		AND NewsID = @EntityID
		AND StoreID = @StoreID
END
ELSE IF @EntityType='OrderOption'
BEGIN
	INSERT INTO OrderOptionStore(StoreID, OrderOptionID)
	SELECT @StoreID, @EntityID
	WHERE
		COALESCE(@CreateMap, @Mapped, 0) = 1
		AND NOT EXISTS (SELECT * FROM OrderOptionStore WHERE StoreID = @StoreID AND OrderOptionID = @EntityID)

	DELETE FROM OrderOptionStore
	WHERE
		COALESCE(@RemoveMap, ~@Mapped, 0) = 1
		AND OrderOptionID = @EntityID
		AND StoreID = @StoreID
END
ELSE IF @EntityType='Product'
BEGIN
	INSERT INTO ProductStore(StoreID, ProductID)
	SELECT @StoreID, @EntityID
	WHERE
		COALESCE(@CreateMap, @Mapped, 0) = 1
		AND NOT EXISTS (SELECT * FROM ProductStore WHERE StoreID = @StoreID AND ProductID = @EntityID)

	DELETE FROM ProductStore
	WHERE
		COALESCE(@RemoveMap, ~@Mapped, 0) = 1
		AND ProductID = @EntityID
		AND StoreID = @StoreID
END
ELSE IF @EntityType = 'Promotion'
BEGIN
	INSERT INTO PromotionStore (StoreID, PromotionID)
	SELECT @StoreID, @EntityID
	WHERE
		COALESCE(@CreateMap, @Mapped, 0) = 1
		AND NOT EXISTS(SELECT * FROM PromotionStore WHERE @StoreID = StoreID AND PromotionID = @EntityID)

	DELETE FROM PromotionStore
	WHERE
		COALESCE(@RemoveMap, ~@Mapped, 0) = 1
		AND PromotionID = @EntityID
		AND StoreID = @StoreID
END
ELSE IF @EntityType = 'ShippingMethod'
BEGIN
	INSERT INTO ShippingMethodStore (StoreId, ShippingMethodID)
	SELECT @StoreID, @EntityID
	WHERE
		COALESCE(@CreateMap, @Mapped, 0) = 1
		AND NOT EXISTS(SELECT * FROM ShippingMethodStore WHERE @StoreID = StoreId AND ShippingMethodID = @EntityID)

	DELETE FROM ShippingMethodStore
	WHERE
		COALESCE(@RemoveMap, ~@Mapped, 0) = 1
		AND ShippingMethodID = @EntityID
		AND StoreId = @StoreID
END
ELSE IF @EntityType = 'Topic'
BEGIN
	INSERT INTO TopicStore (StoreID, TopicID)
	SELECT @StoreID, @EntityID
	WHERE
		COALESCE(@CreateMap, @Mapped, 0) = 1
		AND NOT EXISTS(SELECT * FROM TopicStore WHERE @StoreID = StoreID AND TopicID = @EntityID)

	DELETE FROM TopicStore
	WHERE
		COALESCE(@RemoveMap, ~@Mapped, 0) = 1
		AND TopicID = @EntityID
		AND StoreID = @StoreID
END
ELSE
BEGIN
	INSERT INTO EntityStore (StoreID, EntityID, EntityType)
	SELECT @StoreID, @EntityID, @EntityType
	WHERE
		COALESCE(@CreateMap, @Mapped, 0) = 1
		AND NOT EXISTS(SELECT * FROM EntityStore WHERE StoreID = @StoreID AND EntityID = @EntityID  AND EntityType = @EntityType)

	DELETE FROM EntityStore
	WHERE
		COALESCE(@RemoveMap, ~@Mapped, 0) = 1
		AND StoreID = @StoreID
		AND EntityID = @EntityID
		AND EntityType = @EntityType
END
GO
/*********** end update map sproc to handle all entities ******************************/

-- remove friendly errors appconfig
delete from appconfig where name = 'System.ShowFriendlyErrors'

-- Add the appconfig required by the dotfeed admin cloud connector
IF (NOT EXISTS (SELECT Name FROM AppConfig WHERE Name='DotFeed.Connect.ApiUri'))
	INSERT INTO AppConfig (Name, ConfigValue, Description, GroupName, ValueType, SuperOnly, Hidden)
	VALUES('DotFeed.Connect.ApiUri', 'https://api.dotfeed.com:7000/adnsfconnect/', 'Uri to the dotfeed admin cloud connector api.' , 'DOTFEED', 'String', 1, 1)
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetAffiliatePath]')  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION dbo.GetAffiliatePath
GO
CREATE FUNCTION [dbo].[GetAffiliatePath]
(
	@AffiliateID int,
	@Locale nvarchar(max),
	@Separator nvarchar(max)
)
RETURNS nvarchar(max)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result nvarchar(max);

	WITH AffiliateTree (ParentAffiliateID, S, Level)
	AS
	(
	-- Anchor member definition
		SELECT e.ParentAffiliateID , cast(dbo.GetMlValue(Name, @Locale) as nvarchar(max)) as S, 0 AS Level
		FROM dbo.Affiliate e
		WHERE AffiliateID = @AffiliateID
		UNION ALL
	-- Recursive member definition
		SELECT e.ParentAffiliateID, cast(dbo.GetMlValue(Name, @Locale) + @Separator + S as nvarchar(max)), Level + 1
		FROM dbo.Affiliate e
		INNER JOIN AffiliateTree p ON e.AffiliateID = p.ParentAffiliateID
	)
	-- Statement that executes the CTE
	SELECT top 1 @Result = S
	FROM AffiliateTree
	Order By Level Desc

	-- Return the result of the function
	RETURN @Result

END

GO

/* begin featured products */
if not exists(select * from [dbo].[AppConfig] where [Name] = 'FeaturedProducts.NumberOfItems')
BEGIN
	declare @numberofitems varchar(max)
	set @numberofitems = '4'
	IF EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'NumHomePageSpecials')
	BEGIN
		 SELECT @numberofitems = configvalue FROM AppConfig WHERE Name = 'NumHomePageSpecials'
	END
	INSERT INTO [dbo].[AppConfig] ([Name], [ConfigValue], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden])
	VALUES (N'FeaturedProducts.NumberOfItems', @numberofitems, N'Enter the number of featured items that you want to display on your home page. Set this number to 0 in order to disable the featured items on the home page.', N'integer', NULL, N'SITEDISPLAY', 0, 0)
END
go

if not exists(select * from [dbo].[AppConfig] where [Name] = 'FeaturedProducts.NumberOfColumns')
BEGIN
	declare @numberofcolumns varchar(max)
	set @numberofcolumns = '4'
	IF EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'FeaturedProductsNumberOfColumns')
	BEGIN
		 SELECT @numberofcolumns = configvalue FROM AppConfig WHERE Name = 'FeaturedProductsNumberOfColumns'
	END

	INSERT INTO [dbo].[AppConfig] ([Name], [ConfigValue], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden])
		VALUES (N'FeaturedProducts.NumberOfColumns', @numberofcolumns, N'The number of columns on the home page featured items. 4 is the default value.', N'integer', NULL, N'SITEDISPLAY', 0, 0)
END
go

if not exists(select * from [dbo].[AppConfig] where [Name] = 'FeaturedProducts.ShowPrice')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [ConfigValue], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden])
	VALUES (N'FeaturedProducts.ShowPrice', 'true', N'Whether or not to show the price on the home page featured products.', N'boolean', NULL, N'SITEDISPLAY', 0, 0)
END
go

if not exists(select * from [dbo].[AppConfig] where [Name] = 'FeaturedProducts.ShowAddToCartForm')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [ConfigValue], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden])
	VALUES (N'FeaturedProducts.ShowAddToCartForm', 'true', N'Whether or not to show the add to cart form for featured products on the home page. Only simple products will be able to add to the cart. Complex products will show a ''Details'' button.', N'boolean', NULL, N'SITEDISPLAY', 0, 0)
END
go

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.aspdnsf_GetFeaturedProducts') AND type IN ( N'P', N'PC' ) )
	begin
	 DROP PROCEDURE dbo.aspdnsf_GetFeaturedProducts;
	end
GO

CREATE PROCEDURE [dbo].[aspdnsf_GetFeaturedProducts](
	@NumHomePageFeaturedProducts	INT,
	@CustomerLevelID				INT,
	@InventoryFilter				INT = 0,
	@StoreID						INT = 1,
	@FilterProduct					BIT = 0
)

AS
BEGIN
	SET NOCOUNT ON

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
		case when pcl.productid is null then 0 else isnull(e.Price, 0) end ExtendedPrice
	from Product p with (NOLOCK)
	inner join ProductVariant pv with (NOLOCK) on pv.ProductID = p.ProductID
	join dbo.SalesPrompt sp with (NOLOCK) on p.SalesPromptID = sp.SalesPromptID
	left join dbo.ExtendedPrice e  with (NOLOCK) on pv.VariantID=e.VariantID and e.CustomerLevelID = @CustomerLevelID
	left join dbo.ProductCustomerLevel pcl with (NOLOCK) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID
	left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i  on pv.VariantID = i.VariantID
	left join ProductStore ps with (nolock) on ps.ProductID = p.ProductID
	where p.IsFeatured=1
	and p.Deleted=0
	and p.Published = 1
	and pv.IsDefault = 1
	and (
			(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end >= @InventoryFilter)
			or @InventoryFilter = -1
		)
	and (@FilterProduct = 0 or ps.StoreID = @StoreID)
	order by newid()
END

GO

-- Set IsFeatured on products that are in the featured category
declare @IsFeaturedCategoryID varchar(10)
select @IsFeaturedCategoryID = ConfigValue from appconfig where name = 'IsFeaturedCategoryID'

declare @FeaturedProductsNumberOfColumns varchar(10)
select @FeaturedProductsNumberOfColumns = ConfigValue from appconfig where name = 'FeaturedProductsNumberOfColumns'

if @FeaturedProductsNumberOfColumns > 0 and @IsFeaturedCategoryID > 0
begin
	update product set isfeatured = 1 where productid in (
		select productid from productcategory where categoryid = @IsFeaturedCategoryID
	)
end


--Featured Products Cleanup
DELETE FROM AppConfig WHERE Name = 'FeaturedProductsNumberOfColumns'
DELETE FROM AppConfig WHERE Name = 'IsFeaturedCategoryID'
DELETE FROM AppConfig WHERE Name = 'NumHomePageSpecials'
DELETE FROM AppConfig WHERE Name = 'FeaturedProducts.CategoryID'

IF EXISTS (SELECT * FROM syscolumns WHERE id = object_id('[dbo].[Document]') AND name = 'IsFeatured')
	BEGIN
		ALTER TABLE [dbo].[Document] DROP CONSTRAINT DF_Document_IsFeatured
		ALTER TABLE [dbo].[Document] DROP COLUMN IsFeatured
	END

GO

/* end featured products */

/* ==== Schema-based Localized Object Names ==== */

PRINT 'Creating LocalizedObjectName schema'

if not exists (select * from sys.tables where Name = 'LocalizedObjectName')
begin
	---- Locatization table ----
	create table dbo.LocalizedObjectName
	(
		Id int not null identity (1,1),
		LocaleId int null,
		ObjectId int not null,
		ObjectType nvarchar(50),
		LocalizedName nvarchar(400)
	);
	create unique clustered index [IX_LocalizedObjectName_Cluster]
		on [dbo].[LocalizedObjectName]([ObjectType] asc, [ObjectId] asc, [LocaleId] asc);

	create nonclustered index [IX_LocalizedObjectName_LocaleId_ObjectType]
		on dbo.LocalizedObjectName(LocaleId, ObjectType)
		include(ObjectId, LocalizedName)
end
go

---- Product triggers ----
if exists (select * from sys.triggers where Name = 'UpdateProductLocalizedObjectName_Changes')
	drop trigger dbo.UpdateProductLocalizedObjectName_Changes
go

create trigger dbo.UpdateProductLocalizedObjectName_Changes
on dbo.Product after insert, update as
	if(update(Name)) begin
		set nocount on

		-- Create new entries for new locale/product combinations
		insert into LocalizedObjectName (LocaleId, ObjectId, ObjectType, LocalizedName)
		select
			LocaleSetting.LocaleSettingID,
			inserted.ProductID,
			'product',
			ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
		where
			(
				(ML.Locale is not null and LocaleSetting.LocaleSettingID is not null)
				or (ML.Locale is null and LocaleSetting.LocaleSettingID is null)
			)
			and not exists (
				select 1
				from LocalizedObjectName
				where
					(
						LocaleId = LocaleSetting.LocaleSettingID
						or (LocaleId is null and LocaleSetting.LocaleSettingID is null)
					)
					and ObjectId = inserted.ProductID
					and ObjectType = 'product')

		-- Update existing locale/product combinations
		update LocalizedObjectName
		set LocalizedName = ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
			left join LocalizedObjectName on
				LocalizedObjectName.ObjectId = inserted.ProductID
				and LocalizedObjectName.ObjectType = 'product'
				and (
					LocalizedObjectName.LocaleId = LocaleSetting.LocaleSettingID
					or (LocalizedObjectName.LocaleId is null and LocaleSetting.LocaleSettingID is null)
				)

		---- Remove any locale/product combinations that no longer exist
		delete LocalizedObjectName
		from inserted
			inner join LocalizedObjectName on LocalizedObjectName.ObjectId = inserted.ProductID
		where
			LocalizedObjectName.ObjectType = 'product'
			and not exists (
				select 1
				from Product
					cross apply dbo.ParseMlLocales(Product.Name) as ML
					left join LocaleSetting on LocaleSetting.Name = ML.Locale
				where
					Product.ProductID = LocalizedObjectName.ObjectId
					and (
						LocaleSetting.LocaleSettingID = LocalizedObjectName.LocaleId
						or (LocaleSetting.LocaleSettingID is null and LocalizedObjectName.LocaleId is null)
					)
			)
	end
go

if exists (select * from sys.triggers where Name = 'UpdateProductLocalizedObjectName_Deletes')
	drop trigger dbo.UpdateProductLocalizedObjectName_Deletes
go

create trigger dbo.UpdateProductLocalizedObjectName_Deletes
on dbo.Product after delete as begin
	set nocount on

	-- When a product is deleted, remove all its localized names
	delete LocalizedObjectName
	from LocalizedObjectName
		inner join deleted on LocalizedObjectName.ObjectId = deleted.ProductID and LocalizedObjectName.ObjectType = 'product'
end
go

---- ProductVariant triggers ----
if exists (select * from sys.triggers where Name = 'UpdateProductVariantLocalizedObjectName_Changes')
	drop trigger dbo.UpdateProductVariantLocalizedObjectName_Changes
go

create trigger dbo.UpdateProductVariantLocalizedObjectName_Changes
on dbo.ProductVariant after insert, update as
	if(update(Name)) begin
		set nocount on

		-- Create new entries for new locale/productvariant combinations
		insert into LocalizedObjectName (LocaleId, ObjectId, ObjectType, LocalizedName)
		select
			LocaleSetting.LocaleSettingID,
			inserted.VariantID,
			'productvariant',
			ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
		where
			(
				(ML.Locale is not null and LocaleSetting.LocaleSettingID is not null)
				or (ML.Locale is null and LocaleSetting.LocaleSettingID is null)
			)
			and not exists (
				select 1
				from LocalizedObjectName
				where
					(
						LocaleId = LocaleSetting.LocaleSettingID
						or (LocaleId is null and LocaleSetting.LocaleSettingID is null)
					)
					and ObjectId = inserted.VariantID
					and ObjectType = 'productvariant')

		-- Update existing locale/productvariant combinations
		update LocalizedObjectName
		set LocalizedName = ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
			left join LocalizedObjectName on
				LocalizedObjectName.ObjectId = inserted.VariantID
				and LocalizedObjectName.ObjectType = 'productvariant'
				and (
					LocalizedObjectName.LocaleId = LocaleSetting.LocaleSettingID
					or (LocalizedObjectName.LocaleId is null and LocaleSetting.LocaleSettingID is null)
				)

		---- Remove any locale/productvariant combinations that no longer exist
		delete LocalizedObjectName
		from inserted
			inner join LocalizedObjectName on LocalizedObjectName.ObjectId = inserted.VariantID
		where
			LocalizedObjectName.ObjectType = 'productvariant'
			and not exists (
				select 1
				from ProductVariant
					cross apply dbo.ParseMlLocales(ProductVariant.Name) as ML
					left join LocaleSetting on LocaleSetting.Name = ML.Locale
				where
					ProductVariant.VariantID = LocalizedObjectName.ObjectId
					and (
						LocaleSetting.LocaleSettingID = LocalizedObjectName.LocaleId
						or (LocaleSetting.LocaleSettingID is null and LocalizedObjectName.LocaleId is null)
					)
			)
	end
go

if exists (select * from sys.triggers where Name = 'UpdateProductVariantLocalizedObjectName_Deletes')
	drop trigger dbo.UpdateProductVariantLocalizedObjectName_Deletes
go

create trigger dbo.UpdateProductVariantLocalizedObjectName_Deletes
on dbo.ProductVariant after delete as begin
	set nocount on

	-- When a product is deleted, remove all its localized names
	delete LocalizedObjectName
	from LocalizedObjectName
		inner join deleted on LocalizedObjectName.ObjectId = deleted.VariantID and LocalizedObjectName.ObjectType = 'productvariant'
end
go

---- Category triggers ----
if exists (select * from sys.triggers where Name = 'UpdateCategoryLocalizedObjectName_Changes')
	drop trigger dbo.UpdateCategoryLocalizedObjectName_Changes
go

create trigger dbo.UpdateCategoryLocalizedObjectName_Changes
on dbo.Category after insert, update as
	if(update(Name)) begin
		set nocount on

		-- Create new entries for new locale/category combinations
		insert into LocalizedObjectName (LocaleId, ObjectId, ObjectType, LocalizedName)
		select
			LocaleSetting.LocaleSettingID,
			inserted.CategoryID,
			'category',
			ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
		where
			(
				(ML.Locale is not null and LocaleSetting.LocaleSettingID is not null)
				or (ML.Locale is null and LocaleSetting.LocaleSettingID is null)
			)
			and not exists (
				select 1
				from LocalizedObjectName
				where
					(
						LocaleId = LocaleSetting.LocaleSettingID
						or (LocaleId is null and LocaleSetting.LocaleSettingID is null)
					)
					and ObjectId = inserted.CategoryID
					and ObjectType = 'category')

		-- Update existing locale/category combinations
		update LocalizedObjectName
		set LocalizedName = ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
			left join LocalizedObjectName on
				LocalizedObjectName.ObjectId = inserted.CategoryID
				and LocalizedObjectName.ObjectType = 'category'
				and (
					LocalizedObjectName.LocaleId = LocaleSetting.LocaleSettingID
					or (LocalizedObjectName.LocaleId is null and LocaleSetting.LocaleSettingID is null)
				)

		---- Remove any locale/category combinations that no longer exist
		delete LocalizedObjectName
		from inserted
			inner join LocalizedObjectName on LocalizedObjectName.ObjectId = inserted.CategoryID
		where
			LocalizedObjectName.ObjectType = 'category'
			and not exists (
				select 1
				from Category
					cross apply dbo.ParseMlLocales(Category.Name) as ML
					left join LocaleSetting on LocaleSetting.Name = ML.Locale
				where
					Category.CategoryID = LocalizedObjectName.ObjectId
					and (
						LocaleSetting.LocaleSettingID = LocalizedObjectName.LocaleId
						or (LocaleSetting.LocaleSettingID is null and LocalizedObjectName.LocaleId is null)
					)
			)
	end
go

if exists (select * from sys.triggers where Name = 'UpdateCategoryLocalizedObjectName_Deletes')
	drop trigger dbo.UpdateCategoryLocalizedObjectName_Deletes
go

create trigger dbo.UpdateCategoryLocalizedObjectName_Deletes
on dbo.Category after delete as begin
	set nocount on

	-- When an entity is deleted, remove all its localized names
	delete LocalizedObjectName
	from LocalizedObjectName
		inner join deleted on LocalizedObjectName.ObjectId = deleted.CategoryID and LocalizedObjectName.ObjectType = 'category'
end
go

---- Manufacturer triggers ----
if exists (select * from sys.triggers where Name = 'UpdateManufacturerLocalizedObjectName_Changes')
	drop trigger dbo.UpdateManufacturerLocalizedObjectName_Changes
go

create trigger dbo.UpdateManufacturerLocalizedObjectName_Changes
on dbo.Manufacturer after insert, update as
	if(update(Name)) begin
		set nocount on

		-- Create new entries for new locale/manufacturer combinations
		insert into LocalizedObjectName (LocaleId, ObjectId, ObjectType, LocalizedName)
		select
			LocaleSetting.LocaleSettingID,
			inserted.ManufacturerID,
			'manufacturer',
			ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
		where
			(
				(ML.Locale is not null and LocaleSetting.LocaleSettingID is not null)
				or (ML.Locale is null and LocaleSetting.LocaleSettingID is null)
			)
			and not exists (
				select 1
				from LocalizedObjectName
				where
					(
						LocaleId = LocaleSetting.LocaleSettingID
						or (LocaleId is null and LocaleSetting.LocaleSettingID is null)
					)
					and ObjectId = inserted.ManufacturerID
					and ObjectType = 'manufacturer')

		-- Update existing locale/manufacturer combinations
		update LocalizedObjectName
		set LocalizedName = ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
			left join LocalizedObjectName on
				LocalizedObjectName.ObjectId = inserted.ManufacturerID
				and LocalizedObjectName.ObjectType = 'manufacturer'
				and (
					LocalizedObjectName.LocaleId = LocaleSetting.LocaleSettingID
					or (LocalizedObjectName.LocaleId is null and LocaleSetting.LocaleSettingID is null)
				)

		---- Remove any locale/manufacturer combinations that no longer exist
		delete LocalizedObjectName
		from inserted
			inner join LocalizedObjectName on LocalizedObjectName.ObjectId = inserted.ManufacturerID
		where
			LocalizedObjectName.ObjectType = 'manufacturer'
			and not exists (
				select 1
				from Manufacturer
					cross apply dbo.ParseMlLocales(Manufacturer.Name) as ML
					left join LocaleSetting on LocaleSetting.Name = ML.Locale
				where
					Manufacturer.ManufacturerID = LocalizedObjectName.ObjectId
					and (
						LocaleSetting.LocaleSettingID = LocalizedObjectName.LocaleId
						or (LocaleSetting.LocaleSettingID is null and LocalizedObjectName.LocaleId is null)
					)
			)
	end
go

if exists (select * from sys.triggers where Name = 'UpdateManufacturerLocalizedObjectName_Deletes')
	drop trigger dbo.UpdateManufacturerLocalizedObjectName_Deletes
go

create trigger dbo.UpdateManufacturerLocalizedObjectName_Deletes
on dbo.Manufacturer after delete as begin
	set nocount on

	-- When an entity is deleted, remove all its localized names
	delete LocalizedObjectName
	from LocalizedObjectName
		inner join deleted on LocalizedObjectName.ObjectId = deleted.ManufacturerID and LocalizedObjectName.ObjectType = 'manufacturer'
end
go

---- Distributor triggers ----
if exists (select * from sys.triggers where Name = 'UpdateDistributorLocalizedObjectName_Changes')
	drop trigger dbo.UpdateDistributorLocalizedObjectName_Changes
go

create trigger dbo.UpdateDistributorLocalizedObjectName_Changes
on dbo.Distributor after insert, update as
	if(update(Name)) begin
		set nocount on

		-- Create new entries for new locale/distributor combinations
		insert into LocalizedObjectName (LocaleId, ObjectId, ObjectType, LocalizedName)
		select
			LocaleSetting.LocaleSettingID,
			inserted.DistributorID,
			'distributor',
			ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
		where
			(
				(ML.Locale is not null and LocaleSetting.LocaleSettingID is not null)
				or (ML.Locale is null and LocaleSetting.LocaleSettingID is null)
			)
			and not exists (
				select 1
				from LocalizedObjectName
				where
					(
						LocaleId = LocaleSetting.LocaleSettingID
						or (LocaleId is null and LocaleSetting.LocaleSettingID is null)
					)
					and ObjectId = inserted.DistributorID
					and ObjectType = 'distributor')

		-- Update existing locale/distributor combinations
		update LocalizedObjectName
		set LocalizedName = ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
			left join LocalizedObjectName on
				LocalizedObjectName.ObjectId = inserted.DistributorID
				and LocalizedObjectName.ObjectType = 'distributor'
				and (
					LocalizedObjectName.LocaleId = LocaleSetting.LocaleSettingID
					or (LocalizedObjectName.LocaleId is null and LocaleSetting.LocaleSettingID is null)
				)

		---- Remove any locale/distributor combinations that no longer exist
		delete LocalizedObjectName
		from inserted
			inner join LocalizedObjectName on LocalizedObjectName.ObjectId = inserted.DistributorID
		where
			LocalizedObjectName.ObjectType = 'distributor'
			and not exists (
				select 1
				from Distributor
					cross apply dbo.ParseMlLocales(Distributor.Name) as ML
					left join LocaleSetting on LocaleSetting.Name = ML.Locale
				where
					Distributor.DistributorID = LocalizedObjectName.ObjectId
					and (
						LocaleSetting.LocaleSettingID = LocalizedObjectName.LocaleId
						or (LocaleSetting.LocaleSettingID is null and LocalizedObjectName.LocaleId is null)
					)
			)
	end
go

if exists (select * from sys.triggers where Name = 'UpdateDistributorLocalizedObjectName_Deletes')
	drop trigger dbo.UpdateDistributorLocalizedObjectName_Deletes
go

create trigger dbo.UpdateDistributorLocalizedObjectName_Deletes
on dbo.Distributor after delete as begin
	set nocount on

	-- When an entity is deleted, remove all its localized names
	delete LocalizedObjectName
	from LocalizedObjectName
		inner join deleted on LocalizedObjectName.ObjectId = deleted.DistributorID and LocalizedObjectName.ObjectType = 'distributor'
end
go

---- Section triggers ----
if exists (select * from sys.triggers where Name = 'UpdateSectionLocalizedObjectName_Changes')
	drop trigger dbo.UpdateSectionLocalizedObjectName_Changes
go

create trigger dbo.UpdateSectionLocalizedObjectName_Changes
on dbo.Section after insert, update as
	if(update(Name)) begin
		set nocount on

		-- Create new entries for new locale/section combinations
		insert into LocalizedObjectName (LocaleId, ObjectId, ObjectType, LocalizedName)
		select
			LocaleSetting.LocaleSettingID,
			inserted.SectionID,
			'section',
			ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
		where
			(
				(ML.Locale is not null and LocaleSetting.LocaleSettingID is not null)
				or (ML.Locale is null and LocaleSetting.LocaleSettingID is null)
			)
			and not exists (
				select 1
				from LocalizedObjectName
				where
					(
						LocaleId = LocaleSetting.LocaleSettingID
						or (LocaleId is null and LocaleSetting.LocaleSettingID is null)
					)
					and ObjectId = inserted.SectionID
					and ObjectType = 'section')

		-- Update existing locale/section combinations
		update LocalizedObjectName
		set LocalizedName = ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
			left join LocalizedObjectName on
				LocalizedObjectName.ObjectId = inserted.SectionID
				and LocalizedObjectName.ObjectType = 'section'
				and (
					LocalizedObjectName.LocaleId = LocaleSetting.LocaleSettingID
					or (LocalizedObjectName.LocaleId is null and LocaleSetting.LocaleSettingID is null)
				)

		---- Remove any locale/section combinations that no longer exist
		delete LocalizedObjectName
		from inserted
			inner join LocalizedObjectName on LocalizedObjectName.ObjectId = inserted.SectionID
		where
			LocalizedObjectName.ObjectType = 'section'
			and not exists (
				select 1
				from Section
					cross apply dbo.ParseMlLocales(Section.Name) as ML
					left join LocaleSetting on LocaleSetting.Name = ML.Locale
				where
					Section.SectionID = LocalizedObjectName.ObjectId
					and (
						LocaleSetting.LocaleSettingID = LocalizedObjectName.LocaleId
						or (LocaleSetting.LocaleSettingID is null and LocalizedObjectName.LocaleId is null)
					)
			)
	end
go

if exists (select * from sys.triggers where Name = 'UpdateSectionLocalizedObjectName_Deletes')
	drop trigger dbo.UpdateSectionLocalizedObjectName_Deletes
go

create trigger dbo.UpdateSectionLocalizedObjectName_Deletes
on dbo.Section after delete as begin
	set nocount on

	-- When an entity is deleted, remove all its localized names
	delete LocalizedObjectName
	from LocalizedObjectName
		inner join deleted on LocalizedObjectName.ObjectId = deleted.SectionID and LocalizedObjectName.ObjectType = 'section'
end
go

---- Genre triggers ----
if exists (select * from sys.triggers where Name = 'UpdateGenreLocalizedObjectName_Changes')
	drop trigger dbo.UpdateGenreLocalizedObjectName_Changes
go

create trigger dbo.UpdateGenreLocalizedObjectName_Changes
on dbo.Genre after insert, update as
	if(update(Name)) begin
		set nocount on

		-- Create new entries for new locale/genre combinations
		insert into LocalizedObjectName (LocaleId, ObjectId, ObjectType, LocalizedName)
		select
			LocaleSetting.LocaleSettingID,
			inserted.GenreID,
			'genre',
			ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
		where
			(
				(ML.Locale is not null and LocaleSetting.LocaleSettingID is not null)
				or (ML.Locale is null and LocaleSetting.LocaleSettingID is null)
			)
			and not exists (
				select 1
				from LocalizedObjectName
				where
					(
						LocaleId = LocaleSetting.LocaleSettingID
						or (LocaleId is null and LocaleSetting.LocaleSettingID is null)
					)
					and ObjectId = inserted.GenreID
					and ObjectType = 'genre')

		-- Update existing locale/genre combinations
		update LocalizedObjectName
		set LocalizedName = ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
			left join LocalizedObjectName on
				LocalizedObjectName.ObjectId = inserted.GenreID
				and LocalizedObjectName.ObjectType = 'genre'
				and (
					LocalizedObjectName.LocaleId = LocaleSetting.LocaleSettingID
					or (LocalizedObjectName.LocaleId is null and LocaleSetting.LocaleSettingID is null)
				)

		---- Remove any locale/genre combinations that no longer exist
		delete LocalizedObjectName
		from inserted
			inner join LocalizedObjectName on LocalizedObjectName.ObjectId = inserted.GenreID
		where
			LocalizedObjectName.ObjectType = 'genre'
			and not exists (
				select 1
				from Genre
					cross apply dbo.ParseMlLocales(Genre.Name) as ML
					left join LocaleSetting on LocaleSetting.Name = ML.Locale
				where
					Genre.GenreID = LocalizedObjectName.ObjectId
					and (
						LocaleSetting.LocaleSettingID = LocalizedObjectName.LocaleId
						or (LocaleSetting.LocaleSettingID is null and LocalizedObjectName.LocaleId is null)
					)
			)
	end
go

if exists (select * from sys.triggers where Name = 'UpdateGenreLocalizedObjectName_Deletes')
	drop trigger dbo.UpdateGenreLocalizedObjectName_Deletes
go

create trigger dbo.UpdateGenreLocalizedObjectName_Deletes
on dbo.Genre after delete as begin
	set nocount on

	-- When an entity is deleted, remove all its localized names
	delete LocalizedObjectName
	from LocalizedObjectName
		inner join deleted on LocalizedObjectName.ObjectId = deleted.GenreID and LocalizedObjectName.ObjectType = 'genre'
end
go

---- Vector triggers ----
if exists (select * from sys.triggers where Name = 'UpdateVectorLocalizedObjectName_Changes')
	drop trigger dbo.UpdateVectorLocalizedObjectName_Changes
go

create trigger dbo.UpdateVectorLocalizedObjectName_Changes
on dbo.Vector after insert, update as
	if(update(Name)) begin
		set nocount on

		-- Create new entries for new locale/vector combinations
		insert into LocalizedObjectName (LocaleId, ObjectId, ObjectType, LocalizedName)
		select
			LocaleSetting.LocaleSettingID,
			inserted.VectorID,
			'vector',
			ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
		where
			(
				(ML.Locale is not null and LocaleSetting.LocaleSettingID is not null)
				or (ML.Locale is null and LocaleSetting.LocaleSettingID is null)
			)
			and not exists (
				select 1
				from LocalizedObjectName
				where
					(
						LocaleId = LocaleSetting.LocaleSettingID
						or (LocaleId is null and LocaleSetting.LocaleSettingID is null)
					)
					and ObjectId = inserted.VectorID
					and ObjectType = 'vector')

		-- Update existing locale/vector combinations
		update LocalizedObjectName
		set LocalizedName = ML.Value
		from inserted
			cross apply dbo.ParseMlLocales(inserted.Name) as ML
			left join LocaleSetting on LocaleSetting.Name = ML.Locale
			left join LocalizedObjectName on
				LocalizedObjectName.ObjectId = inserted.VectorID
				and LocalizedObjectName.ObjectType = 'vector'
				and (
					LocalizedObjectName.LocaleId = LocaleSetting.LocaleSettingID
					or (LocalizedObjectName.LocaleId is null and LocaleSetting.LocaleSettingID is null)
				)

		---- Remove any locale/vector combinations that no longer exist
		delete LocalizedObjectName
		from inserted
			inner join LocalizedObjectName on LocalizedObjectName.ObjectId = inserted.VectorID
		where
			LocalizedObjectName.ObjectType = 'vector'
			and not exists (
				select 1
				from Vector
					cross apply dbo.ParseMlLocales(Vector.Name) as ML
					left join LocaleSetting on LocaleSetting.Name = ML.Locale
				where
					Vector.VectorID = LocalizedObjectName.ObjectId
					and (
						LocaleSetting.LocaleSettingID = LocalizedObjectName.LocaleId
						or (LocaleSetting.LocaleSettingID is null and LocalizedObjectName.LocaleId is null)
					)
			)
	end
go

if exists (select * from sys.triggers where Name = 'UpdateVectorLocalizedObjectName_Deletes')
	drop trigger dbo.UpdateVectorLocalizedObjectName_Deletes
go

create trigger dbo.UpdateVectorLocalizedObjectName_Deletes
on dbo.Vector after delete as begin
	set nocount on

	-- When an entity is deleted, remove all its localized names
	delete LocalizedObjectName
	from LocalizedObjectName
		inner join deleted on LocalizedObjectName.ObjectId = deleted.VectorID and LocalizedObjectName.ObjectType = 'vector'
end
go

-- Use the triggers to populate the localized names table
print 'Populating LocalizedObjectName table from existing objects'
declare @localeUpdateChunkSize int = 1000,
		@localeUpdateIndex int,
		@localeUpdateCount int

set nocount on

select @localeUpdateCount = count(*), @localeUpdateIndex = 1 from Product
while @localeUpdateIndex  < @localeUpdateCount
begin
	print '    Updating Products ' + convert(nvarchar(max), @localeUpdateIndex) + ' to ' + convert(nvarchar(max), @localeUpdateIndex + @localeUpdateChunkSize - 1) + ' of ' + convert(nvarchar(max), @localeUpdateCount)
	;with CountedRows as (
		select 
			ProductId Id,
			row_number() over (order by ProductID) Ordinal
		from Product )
	update Product
	set Name = Name
	from CountedRows
		inner join Product on CountedRows.Id = Product.ProductID
	where CountedRows.Ordinal between @localeUpdateIndex and (@localeUpdateIndex + @localeUpdateChunkSize - 1)

	set @localeUpdateIndex = @localeUpdateIndex + @localeUpdateChunkSize
end

select @localeUpdateCount = count(*), @localeUpdateIndex = 1 from ProductVariant
while @localeUpdateIndex  < @localeUpdateCount
begin
	print '    Updating Variants ' + convert(nvarchar(max), @localeUpdateIndex) + ' to ' + convert(nvarchar(max), @localeUpdateIndex + @localeUpdateChunkSize - 1) + ' of ' + convert(nvarchar(max), @localeUpdateCount)
	;with CountedRows as (
		select 
			VariantID Id,
			row_number() over (order by VariantID) Ordinal
		from ProductVariant )
	update ProductVariant
	set Name = Name
	from CountedRows
		inner join ProductVariant on CountedRows.Id = ProductVariant.VariantID
	where CountedRows.Ordinal between @localeUpdateIndex and (@localeUpdateIndex + @localeUpdateChunkSize - 1)

	set @localeUpdateIndex = @localeUpdateIndex + @localeUpdateChunkSize
end

select @localeUpdateCount = count(*), @localeUpdateIndex = 1 from Category
while @localeUpdateIndex  < @localeUpdateCount
begin
	print '    Updating Categories ' + convert(nvarchar(max), @localeUpdateIndex) + ' to ' + convert(nvarchar(max), @localeUpdateIndex + @localeUpdateChunkSize - 1) + ' of ' + convert(nvarchar(max), @localeUpdateCount)
	;with CountedRows as (
		select 
			CategoryId Id,
			row_number() over (order by CategoryID) Ordinal
		from Category )
	update Category
	set Name = Name
	from CountedRows
		inner join Category on CountedRows.Id = Category.CategoryID
	where CountedRows.Ordinal between @localeUpdateIndex and (@localeUpdateIndex + @localeUpdateChunkSize - 1)

	set @localeUpdateIndex = @localeUpdateIndex + @localeUpdateChunkSize
end

select @localeUpdateCount = count(*), @localeUpdateIndex = 1 from Manufacturer
while @localeUpdateIndex  < @localeUpdateCount
begin
	print '    Updating Manufacturers ' + convert(nvarchar(max), @localeUpdateIndex) + ' to ' + convert(nvarchar(max), @localeUpdateIndex + @localeUpdateChunkSize - 1) + ' of ' + convert(nvarchar(max), @localeUpdateCount)
	;with CountedRows as (
		select 
			ManufacturerId Id,
			row_number() over (order by ManufacturerID) Ordinal
		from Manufacturer )
	update Manufacturer
	set Name = Name
	from CountedRows
		inner join Manufacturer on CountedRows.Id = Manufacturer.ManufacturerID
	where CountedRows.Ordinal between @localeUpdateIndex and (@localeUpdateIndex + @localeUpdateChunkSize - 1)

	set @localeUpdateIndex = @localeUpdateIndex + @localeUpdateChunkSize
end

select @localeUpdateCount = count(*), @localeUpdateIndex = 1 from Distributor
while @localeUpdateIndex  < @localeUpdateCount
begin
	print '    Updating Distributors ' + convert(nvarchar(max), @localeUpdateIndex) + ' to ' + convert(nvarchar(max), @localeUpdateIndex + @localeUpdateChunkSize - 1) + ' of ' + convert(nvarchar(max), @localeUpdateCount)
	;with CountedRows as (
		select 
			DistributorId Id,
			row_number() over (order by DistributorID) Ordinal
		from Distributor )
	update Distributor
	set Name = Name
	from CountedRows
		inner join Distributor on CountedRows.Id = Distributor.DistributorID
	where CountedRows.Ordinal between @localeUpdateIndex and (@localeUpdateIndex + @localeUpdateChunkSize - 1)

	set @localeUpdateIndex = @localeUpdateIndex + @localeUpdateChunkSize
end

select @localeUpdateCount = count(*), @localeUpdateIndex = 1 from Section
while @localeUpdateIndex  < @localeUpdateCount
begin
	print '    Updating Sections ' + convert(nvarchar(max), @localeUpdateIndex) + ' to ' + convert(nvarchar(max), @localeUpdateIndex + @localeUpdateChunkSize - 1) + ' of ' + convert(nvarchar(max), @localeUpdateCount)
	;with CountedRows as (
		select 
			SectionId Id,
			row_number() over (order by SectionID) Ordinal
		from Section )
	update Section
	set Name = Name
	from CountedRows
		inner join Section on CountedRows.Id = Section.SectionID
	where CountedRows.Ordinal between @localeUpdateIndex and (@localeUpdateIndex + @localeUpdateChunkSize - 1)

	set @localeUpdateIndex = @localeUpdateIndex + @localeUpdateChunkSize
end

select @localeUpdateCount = count(*), @localeUpdateIndex = 1 from Genre
while @localeUpdateIndex  < @localeUpdateCount
begin
	print '    Updating Genres ' + convert(nvarchar(max), @localeUpdateIndex) + ' to ' + convert(nvarchar(max), @localeUpdateIndex + @localeUpdateChunkSize - 1) + ' of ' + convert(nvarchar(max), @localeUpdateCount)
	;with CountedRows as (
		select 
			GenreId Id,
			row_number() over (order by GenreID) Ordinal
		from Genre )
	update Genre
	set Name = Name
	from CountedRows
		inner join Genre on CountedRows.Id = Genre.GenreID
	where CountedRows.Ordinal between @localeUpdateIndex and (@localeUpdateIndex + @localeUpdateChunkSize - 1)

	set @localeUpdateIndex = @localeUpdateIndex + @localeUpdateChunkSize
end

select @localeUpdateCount = count(*), @localeUpdateIndex = 1 from Vector
while @localeUpdateIndex  < @localeUpdateCount
begin
	print '    Updating Vectors ' + convert(nvarchar(max), @localeUpdateIndex) + ' to ' + convert(nvarchar(max), @localeUpdateIndex + @localeUpdateChunkSize - 1) + ' of ' + convert(nvarchar(max), @localeUpdateCount)
	;with CountedRows as (
		select 
			VectorId Id,
			row_number() over (order by VectorID) Ordinal
		from Vector )
	update Vector
	set Name = Name
	from CountedRows
		inner join Vector on CountedRows.Id = Vector.VectorID
	where CountedRows.Ordinal between @localeUpdateIndex and (@localeUpdateIndex + @localeUpdateChunkSize - 1)

	set @localeUpdateIndex = @localeUpdateIndex + @localeUpdateChunkSize
end

print 'LocalizedObjectName creation and population complete'
set nocount off

go

/* ==== End Schema-based Localized Object Names ==== */

/*Inventory Control*/
IF NOT EXISTS(SELECT * FROM [dbo].[AppConfig] WHERE [Name] = 'KitInventory.ShowOutOfStockMessage')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [ConfigValue], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden])
	VALUES (N'KitInventory.ShowOutOfStockMessage', N'true', N'If true, will display a message to the user when a kit item is out of stock.', N'boolean', NULL, N'OUTOFSTOCK', 0, 0)
END
GO

IF NOT EXISTS(SELECT * FROM [dbo].[AppConfig] WHERE [Name] = 'KitInventory.AllowSaleOfOutOfStock')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [ConfigValue], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden])
	VALUES (N'KitInventory.AllowSaleOfOutOfStock', N'false', N'If true, will allow a user to purchase a kit item even when out of stock.', N'boolean', NULL, N'OUTOFSTOCK', 0, 0)
END
GO

IF EXISTS(SELECT * FROM [dbo].[AppConfig] WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel')
BEGIN
	UPDATE AppConfig SET Description = 'This can hide products from search results, category pages, upsell products, related products, and other listing pages.  Set to -1 to disable the filter and include all products, regardless of inventory levels.  Other values will filter products whose inventory is LESS THAN the amount specified. (Example: to hide all products with 0 inventory, set this to 1.) For products with multiple variants, or products whose inventory is tracked by size and color, the sum total of their inventory will be used to filter the product from the listings.'
	WHERE Name = 'HideProductsWithLessThanThisInventoryLevel'
END
GO

IF EXISTS(SELECT * FROM [dbo].[AppConfig] WHERE [Name] = 'ProductPageOutOfStockRedirect')
BEGIN
	UPDATE AppConfig SET Description = 'If enabled, instead of showing an "Out of Stock" message, a product page will redirect to a 404 "page not found" error page.  Upon seeing this, search engines will remove the product page from their records, which will negatively affect search engine rankings. Enable this only for sites selling one-off or limited run products. Out of stock status is determined by comparing the "Out of Stock Threshold" value to the product''s inventory level.'
	WHERE Name = 'ProductPageOutOfStockRedirect'
END
GO

/*End Inventory Control*/


/* Remove never-used appconfig Mobile.TopicsShowImages */

IF EXISTS(SELECT * FROM [dbo].[AppConfig] WHERE [Name] = 'Mobile.TopicsShowImages')
BEGIN
	DELETE FROM AppConfig
	WHERE [Name] = 'Mobile.TopicsShowImages'
END
GO

update appconfig set description = 'If true than altenative checkout methods such as paypal express will be shown on the shopping cart page.' where name = 'Mobile.ShowAlternateCheckouts'
GO

/* ======== Begin DotFeed Connector AppConfig ======== */

if (not exists (select Name from AppConfig where Name='DotFeed.AccessKey'))
	insert into AppConfig (Name, ConfigValue, Description, GroupName, ValueType, SuperOnly, Hidden)
	values('DotFeed.AccessKey', '', 'The key you provide to DotFeed to allow it to access your site data.', 'DOTFEED', 'String', 0, 0)
go

/* ======== End DotFeed Connector AppConfig ======== */

/* ======== Begin AppConfig Description Update ======== */

UPDATE AppConfig SET Description = 'This is the inventory level of a new Product by default, unless the user inputs something different when they add a Product.' WHERE Name = 'Admin_DefaultInventory'
UPDATE AppConfig SET Description = 'The default Product type ID applied to a new product. Users may override this value.' WHERE Name = 'Admin_DefaultProductTypeID'
UPDATE AppConfig SET Description = 'The default tax classification for a product. Users may create additional tax classes and override the default.' WHERE Name = 'Admin_DefaultTaxClassID'
UPDATE AppConfig SET Description = 'The default sales prompt applied to a new product. Users may override this value.' WHERE Name = 'Admin_DefaultSalesPromptID'
UPDATE AppConfig SET Description = 'If TRUE, the SQL used to generate admin reports will be displayed.' WHERE Name = 'Admin_ShowReportSQL'
UPDATE AppConfig SET Description = 'The subfolder on which you have the administrative site.' WHERE Name = 'AdminDir'
UPDATE AppConfig SET Description = 'Authorize.net live site URL. Do not change this without Authorize.net support.' WHERE Name = 'AUTHORIZENET_LIVE_SERVER'
UPDATE AppConfig SET Description = 'Authorize.net test site URL. Do not change this without Authorize.net support.' WHERE Name = 'AUTHORIZENET_TEST_SERVER'
UPDATE AppConfig SET Description = 'Set to true if you want the Authorize.net gateway to validate the customer billing address against the credit card. Note that this may decrease fraud, but may also cause valid transactions fail.' WHERE Name = 'AUTHORIZENET_Verify_Addresses'
UPDATE AppConfig SET Description = 'The number of products listed on the best sellers page' WHERE Name = 'BestSellersN'
UPDATE AppConfig SET Description = 'If TRUE, product images will display on the best sellers page' WHERE Name = 'BestSellersShowPics'
UPDATE AppConfig SET Description = 'If the CacheMenus setting is set to TRUE, this is the duration (in MINUTES) between cache updates. This should not be set to a value much lower than 60, or the cache will not have time to build up enough to be of any use.' WHERE Name = 'CacheDurationMinutes'
UPDATE AppConfig SET Description = 'If TRUE, site menus and many, many other navigational and dataset elements on the store and admin panel are cached for performance reasons. If cached, these update every CacheDurationMinutes, so changes made on the admin site may not take effect until the cache expires. To force the store to reload cache, restart the site through IIS or by "touching" the web.config. In production, CacheMenus should almost always be set to TRUE for performance reasons.' WHERE Name = 'CacheMenus'
UPDATE AppConfig SET Description = 'Set TRUE to engage the Cardinal Commerce Centinel EasyConnect pre-payment fraud prevention system. Please refer to the cardinalcommerce.com web site for details and to enroll. You must have an account with Cardinal Commerce before enabling this setting.' WHERE Name = 'CardinalCommerce.Centinel.Enabled'
UPDATE AppConfig SET Description = 'If TRUE, AspDotNetStorefront will call Cardinal live servers; if FALSE, AspDotNetStorefront will call Cardinal''s test servers for use with site testing and development.' WHERE Name = 'CardinalCommerce.Centinel.IsLive'
UPDATE AppConfig SET Description = 'Your Cardinal Centinal Assigned Merchant ID. 100 is a test id' WHERE Name = 'CardinalCommerce.Centinel.MerchantID'
UPDATE AppConfig SET Description = 'Your Cardinal Centinal Assigned Processor ID. 800 is a test id' WHERE Name = 'CardinalCommerce.Centinel.ProcessorID'
UPDATE AppConfig SET Description = 'Cardinal-related setting. Change this value only under direction of Cardinal or AspDotNetStorefront support. ' WHERE Name = 'CardinalCommerce.Centinel.MsgType.Lookup'
UPDATE AppConfig SET Description = 'Cardinal-related setting. Change this value only under direction of Cardinal or AspDotNetStorefront support. ' WHERE Name = 'CardinalCommerce.Centinel.MsgType.Authenticate'
UPDATE AppConfig SET Description = 'URL for Cardinal Commerce test/development server' WHERE Name = 'CardinalCommerce.Centinel.TransactionUrl.Test'
UPDATE AppConfig SET Description = 'URL for Cardinal Commerce live/production server. Assigned to your account by Cardinal Commerce.' WHERE Name = 'CardinalCommerce.Centinel.TransactionUrl.Live'
UPDATE AppConfig SET Description = 'Timeout, in milliseconds, for Cardinal Commerce API calls' WHERE Name = 'CardinalCommerce.Centinel.MapsTimeout'
UPDATE AppConfig SET Description = 'Cardinal-related setting. Change this value only under direction of Cardinal or AspDotNetStorefront support. ' WHERE Name = 'CardinalCommerce.Centinel.TermURL'
UPDATE AppConfig SET Description = 'Number of times the Cardinal Commerce authentication request will be attempted, if necessary' WHERE Name = 'CardinalCommerce.Centinel.NumRetries'
UPDATE AppConfig SET Description = 'If TRUE, customers'' old saved carts from previous sessions will be cleared the next time the customer logs in. Works in conjunction with the PreserveActiveCartOnSignin setting.' WHERE Name = 'ClearOldCartOnSignin'
UPDATE AppConfig SET Description = 'If TRUE, the distributor e-mail will NOT be automatically sent and you will therefore need to send the order e-mail manually from within the order details page. Set FALSE to automatically e-mail orders to distributors.' WHERE Name = 'DelayedDropShipNotifications'
UPDATE AppConfig SET Description = 'If blank, each order is dispatched as it is received. Specify a dollar value, e.g. 100.00, to dispatch messages only if the order total exceeds the value you set here. You can use this if you only want to be notified on larger orders' WHERE Name = 'Dispatch_OrderThreshold'
UPDATE AppConfig SET Description = 'The cell messaging number of your phone, in the EXACT format required by your carrier to receive a message, e.g. xxxxxxxxx@mmode.com, etc. Contact your cell carrier for more instructions on how to send SMS messages to your phone via e-mail.' WHERE Name = 'Dispatch_ToPhoneNumber'
UPDATE AppConfig SET Description = 'The name of the store that appears in the SMS message. This MUST be short, e.g. fewer than 5 characters. ' WHERE Name = 'Dispatch_SiteName'
UPDATE AppConfig SET Description = 'Internal use only. Do not modify.' WHERE Name = 'Dispatch_MAX_SMS_MSG_LENGTH'
UPDATE AppConfig SET Description = 'The e-mail address *From* which new order notifications are e-mailed. For example, sallyjane@mystore.com. This is used to notify store owners of new orders.' WHERE Name = 'GotOrderEMailFrom'
UPDATE AppConfig SET Description = 'The name *From* which new order notifications are e-mailed. For example, "Sally Jane" or "Orders". This optional value could also be the same as the GotOrderEMailFrom value. This is used to notify store owners of new orders. ' WHERE Name = 'GotOrderEMailFromName'
UPDATE AppConfig SET Description = 'The e-mail address *To* which order notifications are e-mailed. Typically the e-mail address store administrators monitor. This is used to notify store owners of new orders. Separate multiple e-mail addresses with a comma or semicolon.' WHERE Name = 'GotOrderEMailTo'
UPDATE AppConfig SET Description = 'The name of the home page template, if different than template.master (hometemplate.master). Include the .master extension in what you enter here.' WHERE Name = 'HomeTemplate'
UPDATE AppConfig SET Description = 'If TRUE, AspDotNetStorefront writes out the home page template as-is, after making some search tag modifications, title modifications, etc. If TRUE, AspDotNetStorefront assumes the contents section is predefined in the template. Use this for completely hand-written, custom home pages in conjunction with the HomeTemplate setting.' WHERE Name = 'HomeTemplateAsIs'
UPDATE AppConfig SET Description = 'If TRUE, add-to-cart buttons display a warning if shoppers add quantity greater than inventory on-hand. As well, the quantity value reverts to quantity on hand, making it impossible for shoppers to add a greater number than that on hand. Setting this value to FALSE turns off these limitations.' WHERE Name = 'Inventory.LimitCartToQuantityOnHand'
UPDATE AppConfig SET Description = 'DEPRECATED - this has setting has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' WHERE Name = 'KitCategoryID'
UPDATE AppConfig SET Description = 'If TRUE, the product name shown within the shopping cart will link directly to the appropriate product page. If FALSE, product names appear only as text.' WHERE Name = 'LinkToProductPageInCart'
UPDATE AppConfig SET Description = 'The domain of the live site. This is usually just domain.com for your site (use your own domain name). If you are on a subdomain, this value should be subdomain.domain.com.' WHERE Name = 'LiveServer'
UPDATE AppConfig SET Description = 'The e-mail address from which you want your store e-mails sent. Note that customer receipt e-mails use the ReceiptEMailFrom setting.' WHERE Name = 'MailMe_FromAddress'
UPDATE AppConfig SET Description = 'The name from which your store emails sent, for example, Sally Jane. Note that customer receipt e-mails use the ReceiptEMailFromName setting.' WHERE Name = 'MailMe_FromName'
UPDATE AppConfig SET Description = 'The e-mail server login password (optional). Consult your e-mail server requirements to determine whether or not a password is required. ' WHERE Name = 'MailMe_Pwd'
UPDATE AppConfig SET Description = 'Set this value to the TCP Port that your e-mail server uses.  The vast majority of SMTP servers use standard port 25. Modify this value only if you are certain that your e-mail server uses a port other than 25 for SMTP connections.' WHERE Name = 'MailMe_Port'
UPDATE AppConfig SET Description = 'DNS of your e-mail server, for example, mail.yourdomain.com.' WHERE Name = 'MailMe_Server'
UPDATE AppConfig SET Description = 'The e-mail address to which order notifications are sent, typically usually the same as MailMe_FromAddress. For example, sales@yourdomain.com' WHERE Name = 'MailMe_ToAddress'
UPDATE AppConfig SET Description = 'The e-mail Name to which you want order notifications sent, e.g. Sales.' WHERE Name = 'MailMe_ToName'
UPDATE AppConfig SET Description = 'The e-mail server''s login userName (optional). Consult your e-mail server requirements to determine whether or not a username is required.' WHERE Name = 'MailMe_User'
UPDATE AppConfig SET Description = 'Set TRUE if your e-mail server requires an SSL connection. Note that this setting is completely unrelated to SSL for your website. Also note that e-mail servers very rarely require SSL. ' WHERE Name = 'MailMe_UseSSL'
UPDATE AppConfig SET Description = 'Minimum number of items a shopper must have in their cart before they can checkout. Sum of item quantity must be greater than the value specified here in order to check out. ' WHERE Name = 'MinCartItemsBeforeCheckout'
UPDATE AppConfig SET Description = 'Maximum number of line items a shopper must have in their cart before they can checkout. Quantities do not matter; this setting considers the number of separate items in the cart.' WHERE Name = 'MaxCartItemsBeforeCheckout'
UPDATE AppConfig SET Description = 'The minimum number of characters required in search boxes on the front-end of the site. Minimum value is 1.' WHERE Name = 'MinSearchStringLength'
UPDATE AppConfig SET Description = 'The password that the admin site passes to the store site, so store administrative personnel can view the credit card numbers of customers in receipts and order summaries. Make this a unique value for your site.' WHERE Name = 'OrderShowCCPwd'
UPDATE AppConfig SET Description = 'This value specifies which credit card payment processing gateway to use. To set your active payment gateway, choose Payment Methods from the Configuration menu.' WHERE Name = 'PaymentGateway'
UPDATE AppConfig SET Description = 'One or more payment methods available to shoppers. To set payment methods, choose Payment Methods from the Configuration menu. ' WHERE Name = 'PaymentMethods'
UPDATE AppConfig SET Description = 'If TRUE, will force PayPal & PayPal Express Checkout payments to Capture, regardless of TransactionMode. If false, these payments will honor the TransactionMode setting.' WHERE Name = 'PayPal.ForceCapture'
UPDATE AppConfig SET Description = 'Test sandbox PayPal SOAP API URL. Do not change this value without consulting PayPal support. ' WHERE Name = 'PayPal.API.TestURL'
UPDATE AppConfig SET Description = 'Live server PayPal SOAP API URL. Do not change this value without consulting PayPal support.' WHERE Name = 'PayPal.API.LiveURL'
UPDATE AppConfig SET Description = 'Enables customers to check out using PayPal Express without being a registered customer. If this setting is TRUE, then also set AllowCustomerDuplicateEMailAddresses setting TRUE.' WHERE Name = 'PayPal.Express.AllowAnonCheckout'
UPDATE AppConfig SET Description = 'URL for PayPal Credit (formerly Bill Me Later) button.' WHERE Name = 'PayPal.Express.BillMeLaterButtonURL'
UPDATE AppConfig SET Description = 'Link and URL for the PayPal Credit (formerly Bill Me Later) marketing message to display beneath the PayPal Credit button.' WHERE Name = 'PayPal.Express.BillMeLaterMarketingMessage'
UPDATE AppConfig SET Description = 'Show the PayPal Credit (formerly Bill Me Later) button on the shopping cart page.' WHERE Name = 'PayPal.Express.ShowBillMeLaterButton'
UPDATE AppConfig SET Description = 'PayPal Sandbox Site URL. Do not change this value without consulting PayPal support.' WHERE Name = 'PayPal.TestServer'
UPDATE AppConfig SET Description = 'Fully-qualified URL to the PayPal payment method image icon.' WHERE Name = 'PayPal.PaymentIcon'
UPDATE AppConfig SET Description = 'Set TRUE to use PayPal Instant Notifications to capture payments. ' WHERE Name = 'PayPal.UseInstantNotification'
UPDATE AppConfig SET Description = 'If PayPal.UseInstantNotification is TRUE, specify here the URL to the page that will handle PayPal Instant Notification messages.' WHERE Name = 'PayPal.NotificationURL'
UPDATE AppConfig SET Description = 'PayPal Express Cancel Return URL. Do not change this value without consulting PayPal support.' WHERE Name = 'PayPal.Express.CancelURL'
UPDATE AppConfig SET Description = 'PayPal Express Live Site URL. Do not change this value without consulting PayPal support.' WHERE Name = 'PayPal.Express.LiveURL'
UPDATE AppConfig SET Description = 'PayPal Express Integrated Checkout Live Site URL. Do not change this value without consulting PayPal support.' WHERE Name = 'PayPal.Express.IntegratedCheckout.LiveURL'
UPDATE AppConfig SET Description = 'PayPal Express OK Return URL. Do not change this value without consulting PayPal support.' WHERE Name = 'PayPal.Express.ReturnURL'
UPDATE AppConfig SET Description = 'PayPal Express Sandbox Site URL. Do not change this value without consulting PayPal support.' WHERE Name = 'PayPal.Express.SandboxURL'
UPDATE AppConfig SET Description = 'PayPal Express Integrated Checkout Sandbox Site URL. Do not change this value without consulting PayPal support.' WHERE Name = 'PayPal.Express.IntegratedCheckout.SandboxURL'
UPDATE AppConfig SET Description = 'If TRUE, the PayPal Express checkout button appears on the shopping cart page. You must also properly configure your PayPal API credentials.' WHERE Name = 'PayPal.Express.ShowOnCartPage'
UPDATE AppConfig SET Description = 'URL for PayPal Express Checkout Button Image.' WHERE Name = 'PayPal.Express.ButtonImageURL'
UPDATE AppConfig SET Description = 'If TRUE, images are shown on the recentadditions.aspx page' WHERE Name = 'RecentAdditionsShowPics'
UPDATE AppConfig SET Description = 'If TRUE, customers'' anonymous cart contents will be moved into their cart when logging in or registering. Works in conjunction with the ClearOldCartOnSignin setting.' WHERE Name = 'PreserveActiveCartOnSignin'
UPDATE AppConfig SET Description = 'Set to "visible" or "hidden". If visible, the ratings voting worker window appears; otherwise the worker window will be hidden. Useful for debugging purposes.' WHERE Name = 'RatingsCommentFrameVisibility'
UPDATE AppConfig SET Description = 'If TRUE, anonymous visitors may rate products; otherwise, only visitors who are logged in may rate products. ' WHERE Name = 'RatingsCanBeDoneByAnons'
UPDATE AppConfig SET Description = 'Set TRUE to display product ratings at the bottom of product pages; set FALSE to disable the feature and hide product ratings. ' WHERE Name = 'RatingsEnabled'
UPDATE AppConfig SET Description = 'The number of rating comments shown on product pages, if RatingsEnabled is TRUE.' WHERE Name = 'RatingsPageSize'
UPDATE AppConfig SET Description = 'The e-mail address from which customer receipts are e-mailed, for example orders@mystore.com.' WHERE Name = 'ReceiptEMailFrom'
UPDATE AppConfig SET Description = 'The name from which customer receipts are e-mailed. For example Sales, or Customer Service. ' WHERE Name = 'ReceiptEMailFromName'
UPDATE AppConfig SET Description = 'DEPRECATED - this setting remains for backward compatibility, but is no longer used and will be removed in a future release.' WHERE Name = 'RelatedProductsFormat'
UPDATE AppConfig SET Description = 'If TRUE, the CheckoutTermsAndConditions topic appears on the checkout page and the shopper must check a box to indicate their acceptance of the specified terms and conditions. This is usually only necessary in rare circumstances (selling dangerous goods, etc).' WHERE Name = 'RequireTermsAndConditionsAtCheckout'
UPDATE AppConfig SET Description = 'Your UPS-assigned account user name.' WHERE Name = 'RTShipping.UPS.UserName'
UPDATE AppConfig SET Description = 'Your UPS-assigned account password.' WHERE Name = 'RTShipping.UPS.Password'
UPDATE AppConfig SET Description = 'Your UPS Account License. May be referred to by the name XML Access Key. ' WHERE Name = 'RTShipping.UPS.License'
UPDATE AppConfig SET Description = 'The type of UPS pickup used. Allowed values are: UPSDailyPickup, UPSCustomerCounter, UPSOneTimePickup, UPSOnCallAir, UPSSuggestedRetailRates, UPSLetterCenter, UPSAirServiceCenter.' WHERE Name = 'RTShipping.UPS.UPSPickupType'
UPDATE AppConfig SET Description = 'The maximum weight allowed for a UPS shipment, in the RTSHipping.WeightUnits setting. If an order weight exceeds this value, then the CallForShippingPrompt setting is displayed as the shipping method, with a $0 cost.' WHERE Name = 'RTShipping.UPS.MaxWeight'
UPDATE AppConfig SET Description = 'Your USPS-assigned account user name.' WHERE Name = 'RTShipping.USPS.UserName'
UPDATE AppConfig SET Description = 'The USPS live shipping rates server. Do not change without USPS support.' WHERE Name = 'RTShipping.USPS.Server'
UPDATE AppConfig SET Description = 'The USPS test shipping rates server. Do not change without USPS support.' WHERE Name = 'RTShipping.USPS.TestServer'
UPDATE AppConfig SET Description = 'The maximum allowed weight for a USPS shipment, in the RTSHipping.WeightUnits setting. If an order weight exceeds this, then the CallForShippingPrompt setting is displayed as the shipping method, with a $0 cost.' WHERE Name = 'RTShipping.USPS.MaxWeight'
UPDATE AppConfig SET Description = 'Contains a list of available USPS Services. Valid services are: Express, Priority, Parcel, Library, Media' WHERE Name = 'RTShipping.USPS.Services'
UPDATE AppConfig SET Description = 'Your FedEx-assigned account number.' WHERE Name = 'RTShipping.FedEx.AccountNumber'
UPDATE AppConfig SET Description = 'Your FedEx-assigned meter number, provided to you after registration.' WHERE Name = 'RTShipping.FedEx.Meter'
UPDATE AppConfig SET Description = 'Enter the value for RTShipping calls. See UPS/USPS/FEDEX/DHL requirements. See also the Localization.WeightUnits setting.' WHERE Name = 'RTShipping.WeightUnits'
UPDATE AppConfig SET Description = 'If you want to mark up the rates returned by the carrier, enter the mark-up percentage here. For example, 5.0 would add 5% to each returned rate.' WHERE Name = 'RTShipping.MarkupPercent'
UPDATE AppConfig SET Description = 'If TRUE, RTShippingRequest and RTShippingResponse xml is shown on the bottom of the cart page. Used only for debugging purposes. ' WHERE Name = 'RTShipping.DumpXmlOnCartPage'
UPDATE AppConfig SET Description = 'If the order exceeds maximum shipping rate, this prompt will be shown as the shipping method with $0 cost.' WHERE Name = 'RTShipping.CallForShippingPrompt'
UPDATE AppConfig SET Description = 'The maximum allowed weight for a FedEx shipment, units of which are specified in the RTSHipping.WeightUnits setting. If an order weight exceeds this, then the CallForShippingPrompt setting is displayed as the shipping method, with a $0 cost.' WHERE Name = 'RTShipping.FedEx.MaxWeight'
UPDATE AppConfig SET Description = 'If TRUE, any errors returned from the real-time rate call will be displayed on the cart page. Very helpful for debugging real-time rate issues. See also RTShipping.DumpXmlOnCartPage for more debugging information.' WHERE Name = 'RTShipping.ShowErrors'
UPDATE AppConfig SET Description = 'Your DHL-assigned account number.' WHERE Name = 'RTShipping.DHL.AccountNumber'
UPDATE AppConfig SET Description = 'Your DHL-assigned account API ID.' WHERE Name = 'RTShipping.DHL.APISystemID'
UPDATE AppConfig SET Description = 'Your DHL-assigned API account password.' WHERE Name = 'RTShipping.DHL.APISystemPassword'
UPDATE AppConfig SET Description = 'The DHL live shipping rates server. Do not change without DHL support.' WHERE Name = 'RTShipping.DHL.Server'
UPDATE AppConfig SET Description = 'The DHL test shipping rates server. Do not change without DHL  support.' WHERE Name = 'RTShipping.DHL.TestServer'
UPDATE AppConfig SET Description = 'If you want to filter the real-time shipping methods which shoppers may select, enter a comma separated list of the EXACT text description of the shipping methods that you do NOT want to allow. e.g. U. S. FedEx Same Day, FedEx Overnight, U.S. Postal Priority, etc.' WHERE Name = 'RTShipping.ShippingMethodsToPrevent'
UPDATE AppConfig SET Description = 'Extra weight, in lbs, that will be added to each package. You can use this to account for the weight of packing materials if necessary.' WHERE Name = 'RTShipping.PackageExtraWeight'
UPDATE AppConfig SET Description = 'Used to determine if the real-time rates service contacts live or test servers' WHERE Name = 'RTShipping.UseTestRates'
UPDATE AppConfig SET Description = 'If you have multiple distributors, set this TRUE to allow real-time shipping rate calculations for products based on the distributor''s address. Currently only available for UPS.' WHERE Name = 'RTShipping.MultiDistributorCalculation'
UPDATE AppConfig SET Description = 'For items shipping CanadaPost and combined into one box, this is the default size of that box in, cm.' WHERE Name = 'RTShipping.CanadaPost.DefaultPackageSize'
UPDATE AppConfig SET Description = 'Canada Post Sell Online maximum package weight in kg.' WHERE Name = 'RTShipping.CanadaPost.MaxWeight'
UPDATE AppConfig SET Description = 'Canada Post-assigned Sell Online Merchant ID.' WHERE Name = 'RTShipping.CanadaPost.MerchantID'
UPDATE AppConfig SET Description = 'DNS of Canada Post Sell Online ratings server. Do not change without Canada Post support.' WHERE Name = 'RTShipping.CanadaPost.Server'
UPDATE AppConfig SET Description = 'TCP port of Canada Post Sell Online ratings server. Do not change without Canada Post support.' WHERE Name = 'RTShipping.CanadaPost.ServerPort'
UPDATE AppConfig SET Description = 'The maximum allowed weight for an Australia Post shipment, in kg. If an order weight exceeds this, then the CallForShippingPrompt setting is displayed as the shipping method, with a $0 cost.' WHERE Name = 'RTShipping.AusPost.MaxWeight'
UPDATE AppConfig SET Description = 'If TRUE, distributor matches appear within the advanced search results.' WHERE Name = 'Search_ShowDistributorsInResults'
UPDATE AppConfig SET Description = 'If TRUE, manufacturer matches appear within the advanced search results.' WHERE Name = 'Search_ShowManufacturersInResults'
UPDATE AppConfig SET Description = 'If TRUE, category matches appear within advanced search results.' WHERE Name = 'Search_ShowCategoriesInResults'
UPDATE AppConfig SET Description = 'If TRUE, section matches appear within advanced search results.' WHERE Name = 'Search_ShowSectionsInResults'
UPDATE AppConfig SET Description = 'If TRUE, category matches appear within advanced search results.' WHERE Name = 'Search_ShowProductsInResults'
UPDATE AppConfig SET Description = 'If TRUE, product type filter appears on the advanced search page.' WHERE Name = 'SearchAdv_ShowProductType'
UPDATE AppConfig SET Description = 'If TRUE, category filter appears on the advanced search page.' WHERE Name = 'SearchAdv_ShowCategory'
UPDATE AppConfig SET Description = 'If TRUE, section filter appears on the advanced search page.' WHERE Name = 'SearchAdv_ShowSection'
UPDATE AppConfig SET Description = 'If TRUE, manufacturer filter appears on the advanced search page.' WHERE Name = 'SearchAdv_ShowManufacturer'
UPDATE AppConfig SET Description = 'If TRUE, distributor filter appears on the advanced search page.' WHERE Name = 'SearchAdv_ShowDistributor'
UPDATE AppConfig SET Description = 'If TRUE, SKU filter appears on the advanced search page.' WHERE Name = 'SearchAdv_ShowSKU'
UPDATE AppConfig SET Description = 'If TRUE, price range filter appears on the advanced search page.' WHERE Name = 'SearchAdv_ShowPriceRange'
UPDATE AppConfig SET Description = 'A comma-separated list of shipping method IDs (one or more integers) which you want to mark as having free shipping. You can see the shipping method IDs for your site from with admin. Choose Configuration, Shipping Calculation within Admin, then click View Real-Time Shipping Methods or View Shipping Methods to view shipping methods and their IDs.' WHERE Name = 'ShippingMethodIDIfFreeShippingIsOn'
UPDATE AppConfig SET Description = 'If TRUE, add-to-cart buttons appear on product pages. If FALSE, add-to-cart pages will not appear on product pages. Useful when you are running a catalog, informational, or gallery item site).' WHERE Name = 'ShowBuyButtons'
UPDATE AppConfig SET Description = 'If TRUE, add-to-wishlist buttons appear on product pages; hidden if FALSE.' WHERE Name = 'ShowWishButtons'
UPDATE AppConfig SET Description = 'If TRUE, customer service notes appear on customers'' receipt and order history pages.' WHERE Name = 'ShowCustomerServiceNotesInReceipts'
UPDATE AppConfig SET Description = 'If TRUE, mail-this-product-to-a-friend buttons appear on product pages; hidden if FALSE.' WHERE Name = 'ShowEMailProductToFriend'
UPDATE AppConfig SET Description = 'If TRUE (not recommended), customer credit card information is encrypted and stored in the database. If FALSE, credit card information is not stored anywhere within AspDotNetStorefront. We highly recommend setting this value FALSE and not storing credit card information unless you have a specific reason to do so (e.g. recurring orders, and even then only under certain circumstances). Note that CCV codes are never stored anywhere within AspDotNetStorefront.' WHERE Name = 'StoreCCInDB'
UPDATE AppConfig SET Description = 'Internal use only. Do not modify. Used for documentation purposes. All order records are tagged with the AspDotNetStorefront version with which they were created.' WHERE Name = 'StoreVersion'
UPDATE AppConfig SET Description = 'Specifies how the site handles credit cards in real-time when an order is entered. AUTH means that the card is ONLY authorized; you will have to use the admin panel to later capture the amount, or process the card manually offline. AUTH CAPTURE means that the card is authorized AND captured in real-time. Note that ECHECKS are only AUTH CAPTURE.' WHERE Name = 'TransactionMode'
UPDATE AppConfig SET Description = 'Comma-separated list of CreditCardTypeIDs (integers) which 3-D Secure transactions can be processed for with the currently active gateway. You can see a list of credit card type IDs by choosing Configuration, Credit Card Types from within admin.' WHERE Name = '3DSecure.CreditCardTypeIDs'
UPDATE AppConfig SET Description = 'The shipping zone id to use IF shipping by weight or total & zone is specified, and the customer zip does not match any zone. This setting match the Zone ID reported for the zone definition in the admin panel.' WHERE Name = 'ZoneIdForNoMatch'
UPDATE AppConfig SET Description = 'If TRUE, the gateway''s internal billing will be used instead of the built-in billing mechanism when processing recurring orders. This is ONLY allowed to be true if you are using the Authorize.net or PayPal PayFlow PRO gateways. If using these gateways, setting this flag to TRUE means that you don''t have to store credit card information in your database for recurring orders. Refer to the AspDotNetStorefront manual for further instructions on how to process recurring order reports using each gateway.' WHERE Name = 'Recurring.UseGatewayInternalBilling'
UPDATE AppConfig SET Description = 'If TRUE, an e-mail is sent to the customer when an order is marked shipped within hte admin panel. The email notifies the customer that the order has shippped. Does not apply to recurring orders. ' WHERE Name = 'SendShippedEMailToCustomer'
UPDATE AppConfig SET Description = 'If TRUE, the order notes field on the shopping cart page does not appear.' WHERE Name = 'DisallowOrderNotes'
UPDATE AppConfig SET Description = 'Required for AIM Wells Fargo SecureSource Merchants' WHERE Name = 'eProcessingNetwork_X_EMAIL'
UPDATE AppConfig SET Description = 'Do not change this value without eProcessingNetwork support.' WHERE Name = 'eProcessingNetwork_LIVE_SERVER'
UPDATE AppConfig SET Description = 'Do not change this value without eProcessingNetwork support.' WHERE Name = 'eProcessingNetwork_TEST_SERVER'
UPDATE AppConfig SET Description = 'Set to true if you want the eProcessingNetwork gateway to validate customer billing address against the credit card. Note that this may decrease fraud, but may also cause valid transactions to fail if they have different punctuation than on their credit card.' WHERE Name = 'eProcessingNetwork_Verify_Addresses'
UPDATE AppConfig SET Description = 'The ISO 4217 Standard for the master store currency (i.e. the currency in which you do business). This defines the currency code sent to the payment gateway.' WHERE Name = 'Localization.StoreCurrency'
UPDATE AppConfig SET Description = 'The ISO 4217 Standard Numeric code for the master store currency (i.e. the currency in which you do business.) This defines the currency code sent to the payment gateway.' WHERE Name = 'Localization.StoreCurrencyNumericCode'
UPDATE AppConfig SET Description = 'Enter the prompt you want to use for weights (e.g. lbs, kg, kilos, etc.)' WHERE Name = 'Localization.WeightUnits'
UPDATE AppConfig SET Description = 'Enter the prompt you want to use for dimensions (e.g. inches (IN) or centimeters (CM))' WHERE Name = 'Localization.DimensionUnits'
UPDATE AppConfig SET Description = 'If this is set to STAY then, following after an add-to-cart action, the shopper is automatically redirected back to the originating product page. If blank (the default), shoppers are redirected to the shopping cart page.' WHERE Name = 'AddToCartAction'
UPDATE AppConfig SET Description = 'If TRUE, the mini-cart can be displayed in the left column, if your skin template allows.' WHERE Name = 'ShowMiniCart'
UPDATE AppConfig SET Description = 'If TRUE, product picture icons are shown within the mini-cart.' WHERE Name = 'ShowPicsInMiniCart'
UPDATE AppConfig SET Description = 'The default quantity filled into the add-to-cart form quantity field. If blank, 1 will be used.' WHERE Name = 'DefaultAddToCartQuantity'
UPDATE AppConfig SET Description = 'The minimum order weight, in lbs. Generally most useful when you are using real-time shipping calculations. ' WHERE Name = 'MinOrderWeight'
UPDATE AppConfig SET Description = '2Checkout-provided vendor account number. Set your Direct Return to disabled in your 2Checkout control panel also.' WHERE Name = '2CHECKOUT_VendorID'
UPDATE AppConfig SET Description = '2Checkout live server. Do not change this value without 2Checkout support.' WHERE Name = '2CHECKOUT_LIVE_SERVER'
UPDATE AppConfig SET Description = 'Frequency tag used on Google Site Map (category, section, manufacturer, etc) URL nodes. Consult Google documentation for a list of the values allowed here. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'GoogleSiteMap.EntityChangeFreq'
UPDATE AppConfig SET Description = 'Priority tag used on Google Site Map entity (category, section, manufacturer, etc) URL nodes. Consult Google documentation for a list of values allowed here. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'GoogleSiteMap.EntityPriority'
UPDATE AppConfig SET Description = 'Frequency tag used on Google Site Map product (object) URL nodes. Consult Google documentation for a list of values allowed here. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'GoogleSiteMap.ObjectChangeFreq'
UPDATE AppConfig SET Description = 'Priority tag used on Google Site Map product (object) URL nodes. Consult Google documentation for a list of values allowed here. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'GoogleSiteMap.ObjectPriority'
UPDATE AppConfig SET Description = 'Frequency tag used on Google Site Map topic URL nodes. Consult Google documentation for a list of values allowed here. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'GoogleSiteMap.TopicChangeFreq'
UPDATE AppConfig SET Description = 'Priority tag used on Google Site Map topic URL nodes. Consult Google documentation for allowed values. AspDotNetStorefront support does not have information on allowed values.' WHERE Name = 'GoogleSiteMap.TopicPriority'
UPDATE AppConfig SET Description = 'Change to match Google requirements, if necessary. This should not be changed without your full understanding. All information on supported schemas must come from Google. AspDotNetStorefront support does not provide information on Google requirements in this context.' WHERE Name = 'GoogleSiteMap.Xmlns'
UPDATE AppConfig SET Description = 'Set TRUE if you want a shopper''s cart cleared when they click the reorder button on a prior order. If FALSE, the prior order contents will simply be added to their current cart. ' WHERE Name = 'Reorder.ClearCartBeforeAddingReorderItems'
UPDATE AppConfig SET Description = 'Set TRUE if you want to watermark your product images; otherwise set FALSE.' WHERE Name = 'Watermark.Enabled'
UPDATE AppConfig SET Description = 'If you want to watermark your product images (all sizes) with text, set the watermark text here. For example, Copyright YourStore.com.' WHERE Name = 'Watermark.CopyrightText'
UPDATE AppConfig SET Description = 'If you want to watermark your icon product images with an embedded image watermark, enter the relative image URL path here, e.g. /images/mywatermark.jpg' WHERE Name = 'Watermark.CopyrightImage.Icon'
UPDATE AppConfig SET Description = 'If you want to watermark your medium product images with an embedded image watermark, enter the relative image URL path here, e.g. /images/mywatermark.jpg' WHERE Name = 'Watermark.CopyrightImage.Medium'
UPDATE AppConfig SET Description = 'If you want to watermark your large product images with an embedded image watermark, enter the relative image URL path here, e.g. /images/mywatermark.jpg' WHERE Name = 'Watermark.CopyrightImage.Large'
UPDATE AppConfig SET Description = 'Offset from bottom of product image, in percent, that the copyright notice is placed' WHERE Name = 'Watermark.OffsetFromBottomPercentage'
UPDATE AppConfig SET Description = 'Opacity of watermark image, range is 0.0 = invisible watermark, 1.0 = fully visible watermark.' WHERE Name = 'Watermark.Opacity'
UPDATE AppConfig SET Description = 'If TRUE, product links are included in the sitemap.' WHERE Name = 'SiteMap.ShowProducts'
UPDATE AppConfig SET Description = 'If TRUE, manufacturers are included in the sitemap.' WHERE Name = 'SiteMap.ShowManufacturers'
UPDATE AppConfig SET Description = 'If TRUE, categories are included in the sitemap.' WHERE Name = 'SiteMap.ShowCategories'
UPDATE AppConfig SET Description = 'If TRUE, sections are included in the sitemap.' WHERE Name = 'SiteMap.ShowSections'
UPDATE AppConfig SET Description = 'If TRUE, topics are included in the sitemap.' WHERE Name = 'SiteMap.ShowTopics'
UPDATE AppConfig SET Description = 'If TRUE, customer service items are included in the sitemap. ' WHERE Name = 'SiteMap.ShowCustomerService'
UPDATE AppConfig SET Description = 'If TRUE, admin users can edit the next ship date applicable for recurring orders.' WHERE Name = 'AllowRecurringIntervalEditing'
UPDATE AppConfig SET Description = 'If True, the store topic.cs class will replace ../images with images during HTML content retrieval (topics, descriptions, etc) from the database. This is to allow images you inserted via the built-in HTML editor with Asset Manager to resolve properly on the store site.' WHERE Name = 'ReplaceImageURLFromAssetMgr'
UPDATE AppConfig SET Description = 'If TRUE, product image icons display in the shopping cart.' WHERE Name = 'ShowPicsInCart'
UPDATE AppConfig SET Description = 'If TRUE, upsell products, for those products already in the cart, are shown beneath their current cart products as a suggestive sell feature.' WHERE Name = 'ShowUpsellProductsOnCartPage'
UPDATE AppConfig SET Description = 'If ShowUpsellProductsOnCartPage is TRUE, this sets the maximum number of products shown.' WHERE Name = 'UpsellProductsLimitNumberOnCart'
UPDATE AppConfig SET Description = 'If TRUE, the parser will be invoked on product (or other object) descriptions. Do set TRUE unless you need it, as it adds significant processing overhead. ' WHERE Name = 'UseParserOnEntityDescriptions'
UPDATE AppConfig SET Description = 'This can be set to the relative path of a page on your site (i.e. c-1-myproducts.aspx, products.htm, etc). Customers who click Continue Shopping on the shopping cart page will be redirected to the page defined by this setting instead of back to the page from which they came.' WHERE Name = 'ContinueShoppingURL'
UPDATE AppConfig SET Description = 'If TRUE, visitors must read and acknoweldge a site disclaimer topic before entering the main pages on the site. See the SiteDisclaimer topic for more information.' WHERE Name = 'SiteDisclaimerRequired'
UPDATE AppConfig SET Description = 'If the visitor does not accept the site disclaimer, this is the URL to which the visitor is sent. This must be a FULLY qualified URL target, e.g. http://mystore.com/pagename.aspx' WHERE Name = 'SiteDisclaimerNotAgreedURL'
UPDATE AppConfig SET Description = 'If the visitor accepts the site disclaimer, this is the name of the PAGE relative to your store where they are sent.' WHERE Name = 'SiteDisclaimerAgreedPage'
UPDATE AppConfig SET Description = 'Set TRUE to bypass shipping pages during checkout (e.g. if your store does not at all need to consider shipping.)' WHERE Name = 'SkipShippingOnCheckout'
UPDATE AppConfig SET Description = 'Set TRUE to show quantity discount table as pop-up link on product pages. Set FALSE to show table inline, above the product description. ' WHERE Name = 'ShowQuantityDiscountTablesInline'
UPDATE AppConfig SET Description = 'If TRUE, shoppers are forcibly logged off upon order creation.' WHERE Name = 'ForceSignoutOnOrderCompletion'
UPDATE AppConfig SET Description = 'If TRUE and Micropay is enabled as a payment method, the user''s current Micropay balance appears at the top of the shopping cart page. If FALSE, Micropay balance does not appear. ' WHERE Name = 'Micropay.ShowTotalOnTopOfCartPage'
UPDATE AppConfig SET Description = 'Set TRUE to hide, for all customers in customer level (0), all prices on the entire site, and make it impossible to add products to cart. This can be useful for wholesale sites. ' WHERE Name = 'WholesaleOnlySite'
UPDATE AppConfig SET Description = 'Applies only to zero-dollar (0.00) orders. If set TRUE, all payment collection and related entry pages are bypassed during checkout. A zero-dollar order is created. If FALSE, payment collection and related pages appear even though no money is due.' WHERE Name = 'SkipPaymentEntryOnZeroDollarCheckout'
UPDATE AppConfig SET Description = 'Set TRUE to display a warning from custom.cs.86 resource string if the customer has previously purchased this product.' WHERE Name = 'ShowPreviousPurchase'
UPDATE AppConfig SET Description = 'Set TRUE to hide the (N) quantity on kit items for kit products. ' WHERE Name = 'HideKitQuantity'
UPDATE AppConfig SET Description = 'Set TRUE to hide the base kit price on product pages.' WHERE Name = 'HideKitPrice'
UPDATE AppConfig SET Description = 'If TRUE, turing number security fields are added to the login pages, to prevent automated attacks. Turing fields are also ONLY used on the live server, not the development or staging servers, so make sure your LiveServer setting is also set to yourdomain.com.' WHERE Name = 'SecurityCodeRequiredOnStoreLogin'
UPDATE AppConfig SET Description = 'If TRUE, turing number security fields are added to the login pages, to prevent automated attacks. Turing fields are also ONLY used on the live server, not the development or staging servers, so make sure your LiveServer setting is also set to yourdomain.com.' WHERE Name = 'SecurityCodeRequiredOnAdminLogin'
UPDATE AppConfig SET Description = 'If TRUE, turing number security fields are added to the create account page (not checkout mode), to prevent automated attacks. Turing fields are also ONLY used on the live server, not the development or staging servers, so make sure your LiveServer setting is also set to yourdomain.com.' WHERE Name = 'SecurityCodeRequiredOnCreateAccount'
UPDATE AppConfig SET Description = 'If TRUE, turing number security fields are added to the create account page during checkout, to prevent automated attacks. Turing fields are also ONLY used on the live server, not the development or staging servers, so make sure your LiveServer setting is also set to yourdomain.com.' WHERE Name = 'SecurityCodeRequiredOnCreateAccountDuringCheckout'
UPDATE AppConfig SET Description = 'This can hide products from search results, category pages, upsell products, related products, and other listing pages.  Set to -1 to disable the filter and include all products, regardless of inventory levels.  Other values will filter products whose inventory is LESS THAN the amount specified. (Example: to hide all products with 0 inventory, set this to 1.) For products with multiple variants, or products whose inventory is tracked by size and color, the sum total of their inventory will be used to filter the product from the listings.' WHERE Name = 'HideProductsWithLessThanThisInventoryLevel'
UPDATE AppConfig SET Description = 'Set TRUE to filter out shipping methods which result in $0 costs to the customer; set FALSE to display all shipping methods regardless of customer cost. This setting should almost always be FALSE.' WHERE Name = 'FilterOutShippingMethodsThatHave0Cost'
UPDATE AppConfig SET Description = 'Set TRUE to auto-select the first size and/or color listed within add-to-cart pages (not recommended.) Set False to require shoppers to choose size and color items from the list (recommended).' WHERE Name = 'AutoSelectFirstSizeColorOption'
UPDATE AppConfig SET Description = 'The name of the XmlPackage used to generate new-order e-mails sent to distributors.' WHERE Name = 'XmlPackage.DefaultDistributorNotification'
UPDATE AppConfig SET Description = 'The name of the XmlPackage used to generate lost password messages sent to customers.' WHERE Name = 'XmlPackage.LostPassword'
UPDATE AppConfig SET Description = 'The name of the XmlPackage used to generate order receipts sent to customers.' WHERE Name = 'XmlPackage.OrderReceipt'
UPDATE AppConfig SET Description = 'The name of the XmlPackage used to convert an order into XML format (for administrative purposes, for export purposes, etc).' WHERE Name = 'XmlPackage.OrderAsXml'
UPDATE AppConfig SET Description = 'The name of the XmlPackage used to create the "your order has shipped" e-mail notifications sent to customers.' WHERE Name = 'XmlPackage.OrderShipped'
UPDATE AppConfig SET Description = 'The name of the XmlPackage used to generate new-order notification e-mails sent to the store administrator.' WHERE Name = 'XmlPackage.NewOrderAdminNotification'
UPDATE AppConfig SET Description = 'The name of the XmlPackage used to generate e-mail messages sent to affiliates when they first sign up.' WHERE Name = 'XmlPackage.AffiliateSignupNotification'
UPDATE AppConfig SET Description = 'The name of the XmlPackage used to generate SMS messages sent to the store administrator when new orders are generated.' WHERE Name = 'XmlPackage.NewOrderAdminSMSNotification'
UPDATE AppConfig SET Description = 'The name of the XmlPackage used to generate order-finalization instructions during checkout.' WHERE Name = 'XmlPackage.OrderFinalization'
UPDATE AppConfig SET Description = 'The name of the XmlPackage used on the order confirmation page upon successful order.' WHERE Name = 'XmlPackage.OrderConfirmationPage'
UPDATE AppConfig SET Description = 'Number of columns in related products grid-layout, if grid-layout is used. ' WHERE Name = 'RelatedProductsGridColWidth'
UPDATE AppConfig SET Description = 'Number of columns in upsell products grid-layout. ' WHERE Name = 'UpsellProductsGridColWidth'
UPDATE AppConfig SET Description = 'A comma-delimited list of the ProductTypeIDs which identify a Physical Gift Card. Physical gift card serial numbers are assigned when they are shipped to a customer.' WHERE Name = 'GiftCard.PhysicalProductTypeIDs'
UPDATE AppConfig SET Description = 'A comma-delimited list of the ProductTypeIDs which identify an e-mail gift card. E-mail gift card serial numbers are assigned automatically and e-mailed to the recipient.' WHERE Name = 'GiftCard.EmailProductTypeIDs'
UPDATE AppConfig SET Description = 'A comma-delimited list of the ProductTypeIDs which identify a certificate gift card. Certificate gift cards are printed by the customer so they may be given by the customer to the recipient. Certificate gift card serial numbers are assigned automatically.' WHERE Name = 'GiftCard.CertificateProductTypeIDs'
UPDATE AppConfig SET Description = 'If TRUE, upon creating an account, users must check a checkbox to indicate they are over 13 years old. This may be required to comply with Federal regulations.' WHERE Name = 'RequireOver13Checked'
UPDATE AppConfig SET Description = 'The fully-specified URL that you will be using to get Currency Exchange Rate Data. See also AppConfig:Localization.CurrencyFeedXmlPackage' WHERE Name = 'Localization.CurrencyFeedUrl'
UPDATE AppConfig SET Description = 'The XmlPackage used to provide the currency exchange rate data conversion. This package usually must work in conjunction with the currency exchange rate data provider to convert their rate data into our predefined XML format. See also AppConfig:Localization.CurrencyFeedUrl' WHERE Name = 'Localization.CurrencyFeedXmlPackage'
UPDATE AppConfig SET Description = 'You MUST set this value to match the BASE currency code that your currency feed is returning exchange rates relative to, e.g. USD, EUR, etc.' WHERE Name = 'Localization.CurrencyFeedBaseRateCurrencyCode'
UPDATE AppConfig SET Description = 'The time that a currency rate table is cached. The site will call the live currencyserver specified in Localization.CurrencyFeedUrl setting each time this cache period expires.' WHERE Name = 'Localization.CurrencyCacheMinutes'
UPDATE AppConfig SET Description = 'Your connection ticket assigned by QuickBooks. See QuickBooks Merchant Services and the AspDotNetStorefront manual for instructions on how to obtain your connection ticket.' WHERE Name = 'QBMERCHANTSERVICES_ConnectionTicket'
UPDATE AppConfig SET Description = 'The default page size for a category, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_CategoryPageSize'
UPDATE AppConfig SET Description = 'The default ColWidth for a product, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_ProductColWidth'
UPDATE AppConfig SET Description = 'If TRUE, you can, within the admin panel, map which shipping methods are allowed for which payment methods. This is an undocumented and unsupported feature.' WHERE Name = 'UseMappingShipToPayment'
UPDATE AppConfig SET Description = 'The name of the XmlPackage used to create the body of the e-mail sent when a shopper purchases an e-mail gift card.' WHERE Name = 'XmlPackage.EmailGiftCardNotification'
UPDATE AppConfig SET Description = 'Threshold over which to force delayed downloads on orders. 0.10 is lowest risk. 100.0 is highest risk. See also the MaxMind.ScoreThreshold setting.' WHERE Name = 'MaxMind.DelayDownloadThreshold'
UPDATE AppConfig SET Description = 'Threshold over which to force delayed dropship notifications on orders, regardless of the DelayedDropShipNotifications setting. 0.0 is lowest risk; 100.0 is highest risk. See also the MaxMind.ScoreThreshold setting.' WHERE Name = 'MaxMind.DelayDropShipThreshold'
UPDATE AppConfig SET Description = 'If TRUE, customers receive a welcome e-mail.  The content of the e-mail message is controlled by the XmlPackage specified in the XmlPackage.WelcomeEmail setting.  The e-mail subject is specfied in the StringResource named createaccount.aspx.79.' WHERE Name = 'SendWelcomeEmail'
UPDATE AppConfig SET Description = 'Set TRUE to require customers to use strong passwords. When TRUE, the regular expression stored in the CustomerPwdValidator setting is used for validation.' WHERE Name = 'UseStrongPwd'
UPDATE AppConfig SET Description = 'If TRUE, credit card forms will show the Card Start Date, and Card Issue Number fields. Used for UK/EU storefronts.' WHERE Name = 'ShowCardStartDateFields'
UPDATE AppConfig SET Description = 'If TRUE, a javascript validator will execute on any page which requires credit card entry, in order to enforce the number is valid.' WHERE Name = 'ValidateCreditCardNumbers'
UPDATE AppConfig SET Description = 'Set TRUE to disable the built-in HTML editor within admin and instead show all HTML enabled fields as simple text areas. Set FALSE to enable the built-in HTML editor.' WHERE Name = 'TurnOffHtmlEditorInAdminSite'
UPDATE AppConfig SET Description = 'The salt field to use for encrypting the credit card field in the Address table. Allowable values are AddressID or CustomerID. This setting is meaningful only if the StoreCCInDB setting is set TRUE. ' WHERE Name = 'AddressCCSaltField'
UPDATE AppConfig SET Description = 'The salt field to use for encrypting the credit cad field in the Orders table. Allowable values are OrderNumber, OrderGUID, CustomerID, CustomerGUID or Email. This setting is meaningul only if the StoreCCInDB setting is set TRUE. ' WHERE Name = 'OrdersCCSaltField'
UPDATE AppConfig SET Description = 'The encryption provider used to encrypt the web.config file. The allowed values are DataProtectionConfigurationProvider and RsaProtectedConfigurationProvider. Use RsaProtectedConfigurationProvider if you are running on a web farm. You will need to create an RSA key container before implementing the RSA encryption provider.' WHERE Name = 'Web.Config.EncryptionProvider'
UPDATE AppConfig SET Description = 'If you have to present a message to all site visitors, enter it here. You must also then have an invocation of the skin.adminalert.xml.config XmlPackage in your skin file somewhere, or the message will not be displayed. The message is only displayed if not empty (blank).' WHERE Name = 'AdminAlert.Message'
UPDATE AppConfig SET Description = 'Set TRUE to enable the store to enable certain VAT-related functionality. ' WHERE Name = 'VAT.Enabled'
UPDATE AppConfig SET Description = 'Sets the default behavior of the storefront for EU customers who need to show prices either VAT-inclusive or VAT-exclusive. Allowed values are 1 (VAT inclusive) or 2 (VAT exclusive).' WHERE Name = 'VAT.DefaultSetting'
UPDATE AppConfig SET Description = 'For EU VAT customers, this setting controls whether or not shoppers can on the site choose to view the site in VAT inclusive or VAT exclusive mode for all prices. If FALSE, shoppers will not be able to choose their preference, and the store will force your VAT.DefaultSetting value to be used.' WHERE Name = 'VAT.AllowCustomerToChooseSetting'
UPDATE AppConfig SET Description = 'This must be set to the CountryID. View the country IDs in your administrative site under Configuration, Manage Country Codes. This is used to set the default country for VAT calculations.' WHERE Name = 'VAT.CountryID'
UPDATE AppConfig SET Description = 'Turns on rounding of the VAT included price before multiplying by the quantity ordered' WHERE Name = 'VAT.RoundPerItem'
UPDATE AppConfig SET Description = 'Internal use. Must be exactly 16 characters in length.' WHERE Name = 'InitializationVector'
UPDATE AppConfig SET Description = 'Number of encryption iterations. Enter a number from 1 to 4. 1 is less secure, but faster, 4 is more secure, but slower.' WHERE Name = 'EncryptIterations'
UPDATE AppConfig SET Description = 'Encryption key size. Must be 128, 192, or 256. Lower values are faster, and less secure. Higher values slower are more secure.' WHERE Name = 'KeySize'
UPDATE AppConfig SET Description = 'Type of encryption hash algorithm used. Must be either MD5 or SHA1 (SHA1 is recommended).' WHERE Name = 'HashAlgorithm'
UPDATE AppConfig SET Description = 'Internal use. Do not modify. ' WHERE Name = 'NextKeyChange'
UPDATE AppConfig SET Description = 'Your CyberSource-assigned merchant ID (often the same as your Vendor ID). Contact CyberSource if in question. ' WHERE Name = 'CYBERSOURCE.merchantID'
UPDATE AppConfig SET Description = 'CyberSource Live URL. Do not modify without CyberSource support. ' WHERE Name = 'CYBERSOURCE.LiveURL'
UPDATE AppConfig SET Description = 'CyberSource. Do not modify without CyberSource support. ' WHERE Name = 'CYBERSOURCE.PITURL'
UPDATE AppConfig SET Description = 'Usually this is blank because Cybersource will have the country code configured on your account. If Cybersource requests that you send a Country Code for Payer Authorization processing, enter the country code here.' WHERE Name = 'CYBERSOURCE.paCountryCode'
UPDATE AppConfig SET Description = 'Usually this is blank because Cybersource will have the merchant name configured on your account. If Cybersource requests you to send a Merchant Name for Payer Authorization processing, enter that value here.' WHERE Name = 'CYBERSOURCE.paMerchantName'
UPDATE AppConfig SET Description = 'Usually this is blank because Cybersource will have the merchant URL configured on your account. If Cybersource requests you to send a Merchant URL for Payer Authorization processing, enter that value here.' WHERE Name = 'CYBERSOURCE.paMerchantURL'
UPDATE AppConfig SET Description = 'If you are going to use FedEx shipping manager to process shipments, set this value to TRUE. NOTE: This setting is not for real-time rates, rather only for processing shipments via FedEx shipping manager.' WHERE Name = 'FedexShipManager.Enabled'
UPDATE AppConfig SET Description = 'Used when created new passwords via the request new password function on the sign-in page' WHERE Name = 'NewPwdAllowedChars'
UPDATE AppConfig SET Description = 'Set TRUE to enable template file switching by entity. You can then assign different template.ascx files to each entity (category, section, etc). The template.ascx file you assign to each entity must exist in the active skin folder (for example, skins/skin_1).' WHERE Name = 'TemplateSwitching.Enabled'
UPDATE AppConfig SET Description = 'If TRUE, no new order e-mail notifications will be sent to administrators. If FALSE, store administrators will receive a "new order notification" e-mail from the storefront. This setting does not influence receipt e-mails sent to customers. ' WHERE Name = 'TurnOffStoreAdminEMailNotifications'
UPDATE AppConfig SET Description = 'If TRUE, customers may easily choose or add a new address on the checkout shipping page. If FALSE, customers must work through the address book feature to modify addresses.' WHERE Name = 'AllowAddressChangeOnCheckoutShipping'
UPDATE AppConfig SET Description = 'Leave blank if you do not want AspDotNetStorefront to verify addresses. Otherwise set to "USPS" and ensure the VerifyAddressesProvider.USPS server and userid are properly set.' WHERE Name = 'VerifyAddressesProvider'
UPDATE AppConfig SET Description = 'The default customer level (integer) to be used on new customer records.' WHERE Name = 'DefaultCustomerLevelID'
UPDATE AppConfig SET Description = 'Separator character(s) used when building up breadcrumbs on entity and product pages.' WHERE Name = 'BreadcrumbSeparator'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Live.Abort'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Live.Callback'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Live.Purchase'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Live.Refund'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Live.Release'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Simulator.Abort'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Simulator.Callback'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Simulator.Purchase'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Simulator.Refund'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Simulator.Release'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Test.Abort'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Test.Callback'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Test.Purchase'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Test.Refund'
UPDATE AppConfig SET Description = 'SagePay - do not modify.' WHERE Name = 'SagePayUKURL.Test.Release'
UPDATE AppConfig SET Description = 'Sets the length used to match zip code prefixes when matching shipping zones.' WHERE Name = 'ZipCodePrefixLength'
UPDATE AppConfig SET Description = 'Comma delimited list of carriers which have tracking numbers that can be matched. These values must match up with corresponding ShippingTrackingURL and ShippingTrackingRegEx setting variables.' WHERE Name = 'ShippingTrackingCarriers'
UPDATE AppConfig SET Description = 'PayPal PayFlow Pro Live URL. Do not change this without PayPal support.' WHERE Name = 'PayFlowPro.LiveURL'
UPDATE AppConfig SET Description = 'PayPal PayFlow Pro Test URL. Do not change this without PayPal support.' WHERE Name = 'PayFlowPro.TestURL'
UPDATE AppConfig SET Description = 'Determines whether image resizing will be used.  This can be overridden in any of the size settings through use of the attribute resize (i.e resize:false;).' WHERE Name = 'UseImageResize'
UPDATE AppConfig SET Description = 'Determines whether an uploaded large image will create the icon and medium images.  This value can be over written in each of the size-related settings through use of the attribute largecreates (i.e. largecreates:false;).' WHERE Name = 'LargeCreatesOthers'
UPDATE AppConfig SET Description = 'Determines whether an uploaded large image will create AND overwrite existing icon and medium images.  This value can be over written in each of the size-related settings through use of the attribute largeoverwrites (i.e. largeoverwrites:false;).' WHERE Name = 'LargeOverwritesOthers'
UPDATE AppConfig SET Description = 'TRUE or FALSE.  You can leave the crop attribute out of all size-related settings and cropping will be determined according to this value.  If you use the crop attribute in the other settings (i.e. crop:false;) it will take precedence over this value.  This value should NOT be left blank.' WHERE Name = 'DefaultCrop'
UPDATE AppConfig SET Description = 'left, right, or center.  The horizontal anchor point when cropping will default to this unless otherwise specified in the size-related settings (i.e croph:left).' WHERE Name = 'DefaultCropHorizontal'
UPDATE AppConfig SET Description = 'Default fill color to be used if fill attribute is left out of the other fill-related settings (i.e fill:#00FF00).' WHERE Name = 'DefaultFillColor'
UPDATE AppConfig SET Description = 'Default quality if quality attribute is not used in other image-related settings (i.e. quality:75).' WHERE Name = 'DefaultQuality'
UPDATE AppConfig SET Description = 'Default stretch value if stretch attribute is not specified in other size-size-related settings (i.e. stretch:false).  Stretch is the value that determines whether a smaller, uploaded image will stretch to fill a larger, resized destination image.' WHERE Name = 'DefaultStretch'
UPDATE AppConfig SET Description = 'If TRUE, AspDotNetStorefront creates micro images resized by the width and height specified in DefaultWidth_micro and DefaultHeight_micro and will save them in the images/product/micro folder whenever you are uploading multiple images in the medium multi-image manager.  If a product has multi-images and UseImagesForMultiNav is true then images will be shown instead of the number icons.' WHERE Name = 'MultiMakesMicros'
UPDATE AppConfig SET Description = 'If TRUE, micro images will be used instead of the number icons when multiple images exist in the multi-image manager.' WHERE Name = 'UseImagesForMultiNav'
UPDATE AppConfig SET Description = 'If TRUE and UseImagesForMultiNav is TRUE, the medium image will change on mouse roll-over the micro images instead of when the shopper clicks on them.' WHERE Name = 'UseRolloverForMultiNav'
UPDATE AppConfig SET Description = 'Product pages enable shoppers to switch among product views on your site. If this setting is TRUE, shoppers choose among icon images. If FALSE, shoppers choose from a numbered list.' WHERE Name = 'MultiImage.UseProductIconPics'
UPDATE AppConfig SET Description = 'Hours to offset the reporting date from midnight. If you want the report to run through 6:00 AM then set this value to 6. Pertains to gateway / recurring orders feature.' WHERE Name = 'Recurring.GatewayImportOffsetHours'
UPDATE AppConfig SET Description = 'If TRUE, customers and admin users may edit orders, assuming those orders meet state critieria for being editable' WHERE Name = 'OrderEditingEnabled'
UPDATE AppConfig SET Description = 'If TRUE, an edit button appears in the shopping cart next to kit products, allowing customers to edit/change such items in the cart.' WHERE Name = 'ShowEditButtonInCartForKitProducts'
UPDATE AppConfig SET Description = 'If TRUE, an edit button appears in the shopping cart next to regular (non kit or pack) products, enabling shoppers to edit/change such items in the cart. ' WHERE Name = 'ShowEditButtonInCartForRegularProducts'
UPDATE AppConfig SET Description = 'Your Cardinal Centinel Assigned Transaction Password.' WHERE Name = 'CardinalCommerce.Centinel.TransactionPwd'
UPDATE AppConfig SET Description = 'The Google tracking account to be used with the Google analytics tracking javascript code (ga.js).  This does not apply to the urchin tracking javascript code (urchin.js).' WHERE Name = 'Google.AnalyticsAccount'
UPDATE AppConfig SET Description = 'Set TRUE to enable the Google e-commerce tracking code upon order confirmation. If disabled (FALSE) Google Analytics will still function, but order details will not be sent to Google.' WHERE Name = 'Google.EcomOrderTrackingEnabled'
UPDATE AppConfig SET Description = 'Set TRUE to enable logging certain actions related to recurring orders. (If enabled, you can view the log from the customer history page.)' WHERE Name = 'AuditLog.Enabled'
UPDATE AppConfig SET Description = 'If TRUE, enables HTML caching of your category, section (department) and manufacturer pages. You can use this setting in most cases, as long as you are not performing real-time inventory updates (up to the minute) or other specific entity page product filtering.' WHERE Name = 'CacheEntityPageHTML'
UPDATE AppConfig SET Description = 'If TRUE, customers must be logged in to add to the wishlist.' WHERE Name = 'DisallowAnonCustomerToCreateWishlist'
UPDATE AppConfig SET Description = 'If Show404SuggestionLinks is set to true, suggestion of links will be enabled and visible whenever the site redirects to 404.aspx' WHERE Name = 'Show404SuggestionLinks'
UPDATE AppConfig SET Description = 'The maximum number of suggested links that will appear on the 404 page. ' WHERE Name = '404.NumberOfSuggestedLinks'
UPDATE AppConfig SET Description = 'This will show "Out of Stock" or "In stock" messages depending on the inventory of the product. If the inventory is less than OutOfStockThreshold value, it will display the "Out of Stock" message, otherwise it will display the "In stock" message. To configure this setting, choose Configuration, Inventory Control from within the admin panel.' WHERE Name = 'DisplayOutOfStockProducts'
UPDATE AppConfig SET Description = 'This will show "Out of Stock" or "In stock" messages on product pages only. To enable this you must first set DisplayOutOfStockProducts to TRUE. To configure this setting, choose Configuration, Inventory Control from within the admin panel. ' WHERE Name = 'DisplayOutOfStockOnProductPages'
UPDATE AppConfig SET Description = 'This will show "Out of Stock" or "In Stock" messages only on entity pages. To enable this feature you must first set DisplayOutOfStockProducts to TRUE. To configure this setting, choose Configuration, Inventory Control from within the admin panel. ' WHERE Name = 'DisplayOutOfStockOnEntityPages'
UPDATE AppConfig SET Description = 'Set this OutOfStockThreshold setting to a value below which an item is considered out-of-stock. This setting is relevant only when the DisplayOutOfStockProducts setting is TRUE.' WHERE Name = 'OutOfStockThreshold'
UPDATE AppConfig SET Description = 'If TRUE, products related to the currently-displayed product are displayed near the bottom of the product page. FALSE disables this feature. ' WHERE Name = 'DynamicRelatedProducts.Enabled'
UPDATE AppConfig SET Description = 'The maximum number of related products displayed at the bottom of product pages.' WHERE Name = 'RelatedProducts.NumberDisplayed'
UPDATE AppConfig SET Description = 'Set TRUE to display a store-pickup option when using real-time shipping rates.' WHERE Name = 'RTShipping.AllowLocalPickup'
UPDATE AppConfig SET Description = 'State restrictions for the store-pickup option if the restriction type = State. This should be a comma-separated list of the 2-character state abbreviations found in Configuration, Manage State/Provinces within the admin panel.' WHERE Name = 'RTShipping.LocalPickupRestrictionStates'
UPDATE AppConfig SET Description = 'Zone restrictions for the store-pickup option if the restriction type is zone. View a list of zone IDs by choosing Configuration, Shipping Calculation within the admin panel. Click View Shipping Zones near the bottom of the page.' WHERE Name = 'RTShipping.LocalPickupRestrictionZones'
UPDATE AppConfig SET Description = 'Regex value for the allowed characters in the image.  See the AspDotNetStorefront manual for additional regex values.' WHERE Name = 'Captcha.AllowedCharactersRegex'
UPDATE AppConfig SET Description = 'Maximum ASCII value used in Captcha random character generation.' WHERE Name = 'Captcha.MaxAsciiValue'
UPDATE AppConfig SET Description = 'The maximum number of menu levels to render for dynamic menu. A value of 0 will turn off this limit.' WHERE Name = 'MaxMenuLevel'
UPDATE AppConfig SET Description = 'The number of times to try the backup gateway before failing the order.  Setting this value too high could cause the site to timeout for the user, or have an inordinate delay if the payment gateway does not respond.  Recommended setting = 1 or 2.' WHERE Name = 'PaymentGateway.BackupRetries'
UPDATE AppConfig SET Description = 'The image location for the add-to-cart button. This must be located in the /skins/skin_#/images folder. Specify just the file name, such as addtocart.gif.' WHERE Name = 'AddToCart.AddToCartButton'
UPDATE AppConfig SET Description = 'The image location for the add to wishlist button. This must be located in the /skins/skin_#/images folder. Specify just the filename, such as addwishlist.gif.' WHERE Name = 'AddToCart.AddToWishButton'
UPDATE AppConfig SET Description = 'The default page size for a section, when it is first added to the database. After adding, you can edit it the admin panel.' WHERE Name = 'Default_SectionPageSize'
UPDATE AppConfig SET Description = 'The default page size for a library, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_LibraryPageSize'
UPDATE AppConfig SET Description = 'The default page size for a manufacturer, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_ManufacturerPageSize'
UPDATE AppConfig SET Description = 'The default page size for a distributor, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_DistributorPageSize'
UPDATE AppConfig SET Description = 'The default page size for a product page, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_ProductPageSize'
UPDATE AppConfig SET Description = 'The default page size for a document, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_DocumentPageSize'
UPDATE AppConfig SET Description = 'The default ColWidth for a category, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_CategoryColWidth'
UPDATE AppConfig SET Description = 'The default ColWidth for a section, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_SectionColWidth'
UPDATE AppConfig SET Description = 'The default ColWidth for a library, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_LibraryColWidth'
UPDATE AppConfig SET Description = 'The default ColWidth for a manufacturer, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_ManufacturerColWidth'
UPDATE AppConfig SET Description = 'The default ColWidth for a distributor, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_DistributorColWidth'
UPDATE AppConfig SET Description = 'The default ColWidth for a document, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_DocumentColWidth'
UPDATE AppConfig SET Description = 'The default page size for a Genre, when it is first added to the database. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_GenrePageSize'
UPDATE AppConfig SET Description = 'The default ColWidth for a Genre, when it is first added to the db. After adding, you can edit it in the admin panel.' WHERE Name = 'Default_GenreColWidth'
UPDATE AppConfig SET Description = 'Default width of an icon image if no width attribute is specified in the other size-related settings (i.e. width:50).  This value should NOT be left blank.' WHERE Name = 'DefaultWidth_icon'
UPDATE AppConfig SET Description = 'Default height of an icon image if no width attribute is specified in the other size-related settings (i.e. height:50;).  This value should NOT be left blank.' WHERE Name = 'DefaultHeight_icon'
UPDATE AppConfig SET Description = 'Default width of a medium image if no width attribute is specified in the other size-related settings (i.e. width:50;).  This value should NOT be left blank.' WHERE Name = 'DefaultWidth_medium'
UPDATE AppConfig SET Description = 'Default height of a medium image if no width attribute is specified in the other size-related settings (i.e. width:50;).  This value should NOT be left blank.' WHERE Name = 'DefaultHeight_medium'
UPDATE AppConfig SET Description = 'Default width of a large image if no width attribute is specified in the other size-related settings (i.e. width:50;).  This value should NOT be left blank.' WHERE Name = 'DefaultWidth_large'
UPDATE AppConfig SET Description = 'Default height of a large image if no width attribute is specified in the other size-related settings (i.e. height:50).  This value should NOT be left blank.' WHERE Name = 'DefaultHeight_large'
UPDATE AppConfig SET Description = 'Sets the specifications for the product icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'ProductImg_icon'
UPDATE AppConfig SET Description = 'Sets the specifications for the product large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'ProductImg_large'
UPDATE AppConfig SET Description = 'Sets the specifications for the product swatch image if uploaded manually.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'ProductImg_swatch'
UPDATE AppConfig SET Description = 'Sets the specifications for the variant icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'VariantImg_icon'
UPDATE AppConfig SET Description = 'Sets the specifications for the variant medium image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'VariantImg_medium'
UPDATE AppConfig SET Description = 'Sets the specifications for the variant large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'VariantImg_large'
UPDATE AppConfig SET Description = 'Sets the specifications for the category icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'CategoryImg_icon'
UPDATE AppConfig SET Description = 'Sets the specifications for the category medium image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'CategoryImg_medium'
UPDATE AppConfig SET Description = 'Sets the specifications for the category large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'CategoryImg_large'
UPDATE AppConfig SET Description = 'Sets the specifications for the manufacturer icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'ManufacturerImg_icon'
UPDATE AppConfig SET Description = 'Sets the specifications for the manufacturer medium image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'ManufacturerImg_medium'
UPDATE AppConfig SET Description = 'Sets the specifications for the manufacturer large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'ManufacturerImg_large'
UPDATE AppConfig SET Description = 'Sets the specifications for the department icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'SectionImg_icon'
UPDATE AppConfig SET Description = 'Sets the specifications for the department medium image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'SectionImg_medium'
UPDATE AppConfig SET Description = 'Sets the specifications for the department large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'SectionImg_large'
UPDATE AppConfig SET Description = 'Sets the specifications for the distributor icon image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'DistributorImg_icon'
UPDATE AppConfig SET Description = 'Sets the specifications for the distributor medium image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), and croph(left,right, or center).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'DistributorImg_medium'
UPDATE AppConfig SET Description = 'Sets the specifications for the distributor large image.  Valid attributes are width(int), height(int), quality(int), stretch(true or false), fill(color string with #), crop(true or false), cropv(top,bottom, or middle), croph(left,right, or center), largecreates(true or false), and largeoverwrites(true or false).  All attributes are optional and if not specified will use the values from the default-settings.' WHERE Name = 'DistributorImg_large'
UPDATE AppConfig SET Description = 'If TRUE, the product downloads page will display related products.' WHERE Name = 'Download.ShowRelatedProducts'
UPDATE AppConfig SET Description = 'Valid settings include MANUAL, CAPTURE, and AUTO. Manual = requires admin users to manually release the download product. CAPTURE = download product is released upon payment capture status. AUTO = releases the download product without any requirements.' WHERE Name = 'Download.ReleaseOnAction'
UPDATE AppConfig SET Description = 'If TRUE (recommended), the download file will be streamed and delivered on button click instead of providing a URL to the file location.' WHERE Name = 'Download.StreamFile'
UPDATE AppConfig SET Description = 'If TRUE (not recommended), the software will create a separate copy of each file that is purchased. This configuration is ignored if you are using files on another server for your downloads.' WHERE Name = 'Download.CopyFileForEachOrder'
UPDATE AppConfig SET Description = 'If you enter your e-mail address here, PayPal will allow you to start taking orders without yet having a PayPal account. You must then create a PayPal account within 30 days in order to retrieve your money from PayPal. After registering you should enter your API credentials into the appropriate settings.' WHERE Name = 'PayPal.API.AcceleratedBoardingEmailAddress'
UPDATE AppConfig SET Description = 'Default width of a micro image if no width attribute is specified in the other size-related settings (i.e. width:50).  This value should NOT be left blank.' WHERE Name = 'DefaultWidth_micro'
UPDATE AppConfig SET Description = 'The default locale. If empty this will default to the value in the web.config. Note that changes to this setting do not take full effect until the site is restarted.' WHERE Name = 'DefaultLocale'
UPDATE AppConfig SET Description = 'If TRUE, the buySafe "kicker" will not be shown on product pages.' WHERE Name = 'BuySafe.DisableAddoToCartKicker'
UPDATE AppConfig SET Description = 'The value of the request to BuySafe that defines the type of kicker to show. Alternate types can be found here: http://www.buysafe.com/web/general/kickerpreview.aspx.' WHERE Name = 'BuySafe.KickerType'
UPDATE AppConfig SET Description = 'The type of checkout pages to use on the front-end.  Valid Values are Standard (for multi-page checkout), SmartOPC (for Smart One-Page Checkout), or Other.' WHERE Name = 'Checkout.Type'
UPDATE AppConfig SET Description = 'Number of decimal places to show on percent quantity discounts.' WHERE Name = 'QuantityDiscount.PercentDecimalPlaces'
UPDATE AppConfig SET Description = 'The fully-qualified URL endpoint for the VATCheck service.' WHERE Name = 'VAT.VATCheckServiceURL'
UPDATE AppConfig SET Description = 'Set TRUE to use Avalara for tax calculations.<br /><a  href="AvalaraConnectionTest.aspx" class="lightboxLink">Click here to test your AvaTax connection</a><br /><a href="https://admin-avatax.avalara.net/" target="_blank">Click here for your AvaTax Admin Console</a>.' WHERE Name = 'AvalaraTax.Enabled'
UPDATE AppConfig SET Description = 'The account Avalara provided to you' WHERE Name = 'AvalaraTax.Account'
UPDATE AppConfig SET Description = 'The license Avalara provided to you' WHERE Name = 'AvalaraTax.License'
UPDATE AppConfig SET Description = 'The service URL Avalara provided to you' WHERE Name = 'AvalaraTax.ServiceUrl'
UPDATE AppConfig SET Description = 'The company code Avalara provided to you' WHERE Name = 'AvalaraTax.CompanyCode'
UPDATE AppConfig SET Description = 'Set TRUE to charge tax on order refunds' WHERE Name = 'AvalaraTax.TaxRefunds'
UPDATE AppConfig SET Description = 'Set TRUE to allow customers to select email opt in/out on Smart One-Page Checkout.' WHERE Name = 'Vortx.OnePageCheckout.ShowEmailPreferencesOnCheckout'
UPDATE AppConfig SET Description = 'Set to TRUE to enable Smart One-Page Checkout to use automatic city/state lookup service based on zip-code.' WHERE Name = 'Vortx.OnePageCheckout.UseZipcodeService'
UPDATE AppConfig SET Description = 'Set TRUE to show the create-account panel on Smart One-Page Checkout. Set FALSE to hide.' WHERE Name = 'Vortx.OnePageCheckout.ShowCreateAccount'
UPDATE AppConfig SET Description = 'The topic name used for Customer Service panel on Smart One-Page Checkout.' WHERE Name = 'Vortx.OnePageCheckout.CustomerServiceTopic'
UPDATE AppConfig SET Description = 'Enter your LicenseKey for Smart One-Page Checkout.  Contact customer service if you do not have a LicenseKey.' WHERE Name = 'Vortx.OnePageCheckout.LicenseKey'
UPDATE AppConfig SET Description = 'Address Locale used within Smart One-Page Checkout.' WHERE Name = 'Vortx.OnePageCheckout.AddressLocale'
UPDATE AppConfig SET Description = 'Name of the CSS styles heet Smart One-Page Checkout uses.' WHERE Name = 'Vortx.OnePageCheckout.OPCStyleSheetName'
UPDATE AppConfig SET Description = 'Internal use only. Do not modify.' WHERE Name = 'Vortx.OnePageCheckout.ZipCodeService.Timeout'
UPDATE AppConfig SET Description = 'Internal use only. Do not modify.' WHERE Name = 'Vortx.OnePageCheckout.ScriptManagerId'
UPDATE AppConfig SET Description = 'Determines which SkinID is applied after the shopper signs in. Session = SkinID set based on current session. Default = SkinID set to the site''s default SkinID.' WHERE Name = 'Signin.SkinMaster'
UPDATE AppConfig SET Description = 'This setting controls how quantity discounts are calculated at checkout. If TRUE, all line items with the same product ID are factored into the quantity discount calculation, even if they have different variant, size, or color options. If FALSE, quantity discounts are calculated per line item. ' WHERE Name = 'QuantityDiscount.CombineQuantityByProduct'
UPDATE AppConfig SET Description = 'Authorize.net CIM live Service URL. Do Not Change.' WHERE Name = 'AUTHORIZENET_Cim_LiveServiceURL'
UPDATE AppConfig SET Description = 'Authorize.net CIM sandbox Service URL. Do Not Change.' WHERE Name = 'AUTHORIZENET_Cim_SandboxServiceURL'
UPDATE AppConfig SET Description = 'Set TRUE to use Authorize.net CIM in sandbox mode.' WHERE Name = 'AUTHORIZENET_Cim_UseSandbox'
UPDATE AppConfig SET Description = 'Set TRUE to enable Authorize.net CIM' WHERE Name = 'AUTHORIZENET_Cim_Enabled'
UPDATE AppConfig SET Description = 'For developer use only. This value contains the skin id for your mobile skin. In standard mobile setups this setting should be set to 2.' WHERE Name = 'Mobile.SkinId'
UPDATE AppConfig SET Description = 'If this setting is set TRUE, the store displays on every page a link to e-mail the store administrator. These e-mails are sent to the e-mail address listed in the GotOrderEMailFrom setting.' WHERE Name = 'Mobile.IncludeEmailLinks'
UPDATE AppConfig SET Description = 'Enables or disables the built-in mobile platform. TRUE to enable; FALSE to disable.' WHERE Name = 'Mobile.IsEnabled'
UPDATE AppConfig SET Description = 'If this setting is TRUE, the store displays on every page a link to call your store''s phone number. The phone number can be changed with the Mobile.ContactPhoneNumber setting.' WHERE Name = 'Mobile.IncludePhoneLinks'
UPDATE AppConfig SET Description = 'For developer use only. This settings enables the visitor to call directly from their mobile device. If this feature is enabled (via the Mobile.IncludePhoneLinks setting) the call link will dial the phone number contained in this setting.' WHERE Name = 'Mobile.ContactPhoneNumber'
UPDATE AppConfig SET Description = 'For developer use only. Non-mobile pages entered in this setting will be displayed in the mobile skin when visitor views the mobile site. For example: manufacturers.aspx,sitemap.aspx.' WHERE Name = 'Mobile.PageExceptions'
UPDATE AppConfig SET Description = 'The number of products listed on product listing pages within the mobile platform. ' WHERE Name = 'Mobile.Entity.PageSize'
UPDATE AppConfig SET Description = 'For developer use only. This setting defines product image widths on entity pages. Note that a large value in this field may break the display of this field. This value does not necessarily need to match your product image widths; images will be resized with CSS.' WHERE Name = 'Mobile.Entity.ImageWidth'
UPDATE AppConfig SET Description = 'For developer use only. This setting defines the product images within the product slider. This value should space images appropriately on mobile devices when paired with the value in the Mobile.ProductSlider.Width setting. The product slider is used for featured products, recently-viewed products, related products, and upsell products.' WHERE Name = 'Mobile.ProductSlider.ImageWidth'
UPDATE AppConfig SET Description = 'This setting sets the maximum number of products shown in a product slider. The fewer products allowed in a product slider, the faster pages with product sliders will load. This number of products should be an evenly-divisible by the value in the Mobile.ProductSlider.Width setting.' WHERE Name = 'Mobile.ProductSlider.MaxProducts'
UPDATE AppConfig SET Description = 'For developer use only. This setting defines how many products each pane in the product slider2 appear.' WHERE Name = 'Mobile.ProductSlider.Width'
UPDATE AppConfig SET Description = 'If TRUE, altenative checkout methods (e.g. PayPal Express) appear on the shopping cart page.' WHERE Name = 'Mobile.ShowAlternateCheckouts'
UPDATE AppConfig SET Description = 'For developer use only. This setting defines the default XmlPackage the mobile commerce platform uses for entity pages. The default value for this setting is mobile.entity.default.xml.config. The associated XmlPackage is included in the mobile skin.' WHERE Name = 'Mobile.DefaultXmlPackageEntity'
UPDATE AppConfig SET Description = 'For developer use only. This setting defines the default XmlPackage the mobile commerce platform uses for product pages. The default value for this setting is mobile.entity.default.xml.config. The associated XmlPackage is included in the mobile skin.' WHERE Name = 'Mobile.DefaultXmlPackageProduct'
UPDATE AppConfig SET Description = 'For developer use only.' WHERE Name = 'Mobile.UserAgentList'
UPDATE AppConfig SET Description = 'For developer use only.' WHERE Name = 'Mobile.ShortUserAgentList'
UPDATE AppConfig SET Description = 'Internal use. Do not modify. ' WHERE Name = 'Mobile.AllowMultiShipOnCheckout'
UPDATE AppConfig SET Description = 'Set TRUE to show the mobile commerce skin on iPads. Not recommended. ' WHERE Name = 'Mobile.ShowMobileOniPad'
UPDATE AppConfig SET Description = 'URL for PayPal Express Checkout Button Image used on Mobile.' WHERE Name = 'Mobile.PayPal.Express.ButtonImageURL'
UPDATE AppConfig SET Description = 'Set TRUE if AspDotNetStorefront should commit the tax document for orders. Set to FALSE if order taxes are committed in an external system.' WHERE Name = 'AvalaraTax.CommitTaxes'
UPDATE AppConfig SET Description = 'Set TRUE if AspDotNetStorefront should commit the tax document for refunds. Set to FALSE if refund taxes are committed in an external system.' WHERE Name = 'AvalaraTax.CommitRefunds'
UPDATE AppConfig SET Description = 'If TRUE, store admins are sent an e-mail when purchases take product inventory levels below the value specified in the SendLowStockWarningsThreshold setting. ' WHERE Name = 'SendLowStockWarnings'
UPDATE AppConfig SET Description = 'Sets the inventory threshold at which store administrators are notified that a product is running low on stock. Applies only in SendLowStockWarnings is set TRUE.' WHERE Name = 'SendLowStockWarningsThreshold'
UPDATE AppConfig SET Description = 'If TRUE, a table of products with inventory levels lower than SendLowStockWarningsThreshold is displayed on admin dashboard. NOTE: This may slow down the load time of the admin dashboard on sites with very large product sets.' WHERE Name = 'ShowAdminLowStockAudit'
UPDATE AppConfig SET Description = 'If TRUE, displays diagnostic subtotals in the order summary.' WHERE Name = 'Debug.DisplayOrderSummaryDiagnostics'
UPDATE AppConfig SET Description = 'Your Google Product Search Account ID. This value should match the account ID you use to submit your product data feed you submit to Google Product Search. (If you have an MCA account, use the subaccount ID associated with that product feed.)' WHERE Name = 'GoogleTrustedStoreProductSearchID'
UPDATE AppConfig SET Description = 'Account country from Google Product Search. This value should match the account country you use to submit your product data feed to Google Product Search. The value of the country parameter should be a <a target="blank" style="text-decoration: underline;" href="http://www.iso.org/iso/country_codes/iso_3166_code_lists.htm">two-letter ISO 3166 country code</a>.' WHERE Name = 'GoogleTrustedStoreCountry'
UPDATE AppConfig SET Description = 'Account language from Google Product Search. This value should match the account language you use to submit your product data feed to Google Product Search. The value of the language parameter should be a <a target="blank" style="text-decoration: underline;" href="http://www.loc.gov/standards/iso639-2/php/English_list.php">two-letter ISO 639-1 language code</a>.' WHERE Name = 'GoogleTrustedStoreLanguage'
UPDATE AppConfig SET Description = 'This is your Google Trusted Store account ID, which can be retrieved <a target="blank" style="text-decoration: underline;" href="http://www.google.com/trustedstores/sell/setupcode">here</a>.  Look for the code on this line:    gts.push(["id", "xxxxx"]);' WHERE Name = 'GoogleTrustedStoreID'
UPDATE AppConfig SET Description = 'If TRUE, the Google Trusted Store javascript will be added to your orderconfirmation.aspx page.' WHERE Name = 'GoogleTrustedStoreEnabled'
UPDATE AppConfig SET Description = 'The estimated average number of days before a new order ships (whole numbers only).  Be as accurate as possible without shorting this value - this factors into your store''s trust rating.' WHERE Name = 'GoogleTrustedStoreShippingLeadTime'
UPDATE AppConfig SET Description = 'If TRUE, promotions will be applied *after* quantity discounts.' WHERE Name = 'Promotions.ApplyDiscountsBeforePromoApplied'
UPDATE AppConfig SET Description = 'Default height of a micro image if no height attribute is specified in the other size-related settings (i.e. height:50). This value should NOT be left blank.' WHERE Name = 'DefaultHeight_micro'
UPDATE AppConfig SET Description = 'If TRUE, the birth date field appears while creating a customer (contact) account; otherwise the field does not appear. ' WHERE Name = 'Account.ShowBirthDateField'
UPDATE AppConfig SET Description = 'Show PayPal Credit (formerly Bill Me Later) advertisements on product pages.' WHERE Name = 'PayPal.Ads.ShowOnProductPage'
UPDATE AppConfig SET Description = 'PayPal ad dimensions for product pages.' WHERE Name = 'PayPal.Ads.ProductPageDimensions'
UPDATE AppConfig SET Description = 'Show PayPal Credit (formerly Bill Me Later) advertisements on your home page.' WHERE Name = 'PayPal.Ads.ShowOnHomePage'
UPDATE AppConfig SET Description = 'PayPal Credit (formerly Bill Me Later) ad dimensions for the home page.' WHERE Name = 'PayPal.Ads.HomePageDimensions'
UPDATE AppConfig SET Description = 'Show PayPal Credit (formerly Bill Me Later) ads on the Shopping Cart page.' WHERE Name = 'PayPal.Ads.ShowOnCartPage'
UPDATE AppConfig SET Description = 'PayPal Credit (formerly Bill Me Later) ad dimensions for the shopping cart page.' WHERE Name = 'PayPal.Ads.CartPageDimensions'
UPDATE AppConfig SET Description = 'Show Credit (formerly Bill Me Later) ads on entity pages.' WHERE Name = 'PayPal.Ads.ShowOnEntityPage'
UPDATE AppConfig SET Description = 'PayPal Credit (formerly Bill Me Later) ad dimensions for entity pages.' WHERE Name = 'PayPal.Ads.EntityPageDimensions'
UPDATE AppConfig SET Description = 'If defined, Smart One-Page Checkout automatically selects this shipping method after the customer enters their shipping address information. Set blank to disable.' WHERE Name = 'Vortx.OnePageCheckout.DefaultShippingMethodId'
UPDATE AppConfig SET Description = 'If TRUE, payment methods other than credit card will allow shoppers to enter a separate billing address on Smart One-Page Checkout.' WHERE Name = 'Vortx.OnePageCheckout.AllowAlternativePaymentBillingAddressEdit'
UPDATE AppConfig SET Description = 'If TRUE, customers are prompted to re-enter their e-mail address as confirmation when registering or updating accounts.' WHERE Name = 'RequireEmailConfirmation'
UPDATE AppConfig SET Description = 'If TRUE, search results will include the description and summary fields by default. NOTE: This will put additional strain on your site''s resources and may not be advisable in some shared hosting environments.' WHERE Name = 'SearchDescriptionsByDefault'
UPDATE AppConfig SET Description = 'Set TRUE to enable the Google Remarketing script on your site. This enables the contents of the Script.Google.Remarketing topic on every page of your site. You must make sure that the script.bodyclose XmlPackage is included in your template.' WHERE Name = 'Google.Remarketing.Enabled'
UPDATE AppConfig SET Description = 'If TRUE, Avalara address validation errors will prevent checkout' WHERE Name = 'AvalaraTax.PreventOrderIfAddressValidationFails'
UPDATE AppConfig SET Description = 'If TRUE, customers will be able to switch among recurring variants on the shopping cart page.' WHERE Name = 'AllowRecurringFrequencyChangeInCart'
UPDATE AppConfig SET Description = 'A comma-separated list of 2-digit state codes to exclude from shipping promotions.' WHERE Name = 'Promotions.ExcludeStates'
UPDATE AppConfig SET Description = 'This allows $0 shipping. Allows customer checkout when real-time shipping returns zero methods, such as when order weight exceeds UPS, USPS, or FEDEX weight limits.' WHERE Name = 'Vortx.OnePageCheckout.AllowRTShipping.NoMethodSelected'
UPDATE AppConfig SET Description = 'This string allows you to specify the format of your Google product identifiers. Valid tokens are {ProductID}, {VariantID}, and {FullSKU}. These tokens are case sensitive.' WHERE Name = 'GoogleTrustedStoreProductIdentifierFormat'
UPDATE AppConfig SET Description = 'Set TRUE to add your product identifier to the Google Trusted Stores script on product pages. Make sure to set up the GoogleTrustedStoreProductIdentifierFormat setting when you enable this feature. FALSE disables this feature. ' WHERE Name = 'GoogleTrustedStoreProductIdentifierEnabled'
UPDATE AppConfig SET Description = 'Set TRUE to enable Google display advertising features within your Google Analytics tag. You may need to update your privacy policy if you turn on this feature. See Google''s display advertising policy requirements for more inforamtion.' WHERE Name = 'Google.AnalyticsDisplayAdvertising.Enabled'
UPDATE AppConfig SET Description = 'Checkout By Amazon Access Key' WHERE Name = 'CheckoutByAmazon.CbaAccessKey'
UPDATE AppConfig SET Description = 'Checkout By Amazon Secret Key' WHERE Name = 'CheckoutByAmazon.CbaSecretKey'
UPDATE AppConfig SET Description = 'Amazon Marketplace Access Key' WHERE Name = 'CheckoutByAmazon.MwsAccessKey'
UPDATE AppConfig SET Description = 'Amazon Marketplace Secret Key' WHERE Name = 'CheckoutByAmazon.MwsSecretKey'
UPDATE AppConfig SET Description = 'Checkout By Amazon Merchant Id' WHERE Name = 'CheckoutByAmazon.MerchantId'
UPDATE AppConfig SET Description = 'Checkout By Amazon Marketplace' WHERE Name = 'CheckoutByAmazon.Marketplace'
UPDATE AppConfig SET Description = 'The URL used to render Checkout by Amazon widget scripts' WHERE Name = 'CheckoutByAmazon.WidgetUrl'
UPDATE AppConfig SET Description = 'The sandbox URL used to render Checkout By Amazon widget scripts' WHERE Name = 'CheckoutByAmazon.WidgetSandboxUrl'
UPDATE AppConfig SET Description = 'The URL used to call the Checkout By Amazon service' WHERE Name = 'CheckoutByAmazon.CBAServiceUrl'
UPDATE AppConfig SET Description = 'The sandbox URL used to call the Checkout By Amazon service' WHERE Name = 'CheckoutByAmazon.CBAServiceSandboxUrl'
UPDATE AppConfig SET Description = 'The URL used to call the Checkout By Amazon merchant service' WHERE Name = 'CheckoutByAmazon.MerchantServiceUrl'
UPDATE AppConfig SET Description = 'The sandbox URL used to call the Checkout By Amazon merchant service' WHERE Name = 'CheckoutByAmazon.MerchantServiceSandboxUrl'
UPDATE AppConfig SET Description = 'Set TRUE to put Checkout By Amazon services in sandbox mode' WHERE Name = 'CheckoutByAmazon.UseSandbox'
UPDATE AppConfig SET Description = 'Checkout By Amazon Order Fulfillment Type. Instant: Marks the order as shipped immediately after getting the ready to ship notification. MarkedAsShipped - Notifies Amazon that the order has shipped when an admin marks the order as shipped. Never - Admins must manually adjust ship status at Amazon.' WHERE Name = 'CheckoutByAmazon.OrderFulfillmentType'
UPDATE AppConfig SET Description = 'If TRUE and watermarks are enabled, icon-sized product images will also contain watermarks.' WHERE Name = 'Watermark.Icons.Enabled'
UPDATE AppConfig SET Description = 'Set TRUE to enable the Google Tag Manager script on your site. This enables the contents of the Script.Google.TagManager topic on every page of your site. You must make sure that the script.bodyopen XmlPackage is included in your template.' WHERE Name = 'Google.TagManager.Enabled'
UPDATE AppConfig SET Description = 'The XmlPackage used to generate the site map found at sitemap.aspx' WHERE Name = 'XmlPackage.SiteMapPage'
UPDATE AppConfig SET Description = 'Set TRUE to display maintenance.master page to non-Admin users. Administrators may still access the site.' WHERE Name = 'DisplayMaintenancePage.Enable'
UPDATE AppConfig SET Description = 'Internal use. Do not modify. ' WHERE Name = 'DotFeed.Connect.ApiUri'
UPDATE AppConfig SET Description = 'The number of featured items that appear on the home page. Set this number to 0 in order to disable featured items functionality.' WHERE Name = 'FeaturedProducts.NumberOfItems'
UPDATE AppConfig SET Description = 'Whether or not to show the add-to-cart form for featured products on the home page. Only simple products will be able to add to the cart. Complex products display a "Details" button.' WHERE Name = 'FeaturedProducts.ShowAddToCartForm'
UPDATE AppConfig SET Description = 'If TRUE, AspDotNetStorefront displays a message to the shopper when a kit item is out of stock.' WHERE Name = 'KitInventory.ShowOutOfStockMessage'
UPDATE AppConfig SET Description = 'If TRUE, AspDotNetStorefront allows a user to purchase a kit item even when the kit item is out of stock.' WHERE Name = 'KitInventory.AllowSaleOfOutOfStock'
UPDATE AppConfig SET Description = 'The key you provide to allow DotFeed to access your site data. Can be any combination of text and numbers.' WHERE Name = 'DotFeed.AccessKey'
UPDATE AppConfig SET Description = 'If TRUE, https links will be used for shoppingcart pages, account pages, receipt pages, etc. Only set this value TRUE AFTER you have your Secure Certificate (SSL cert) installed on your live server. SSL also is ONLY invoked on the live server, not the development or staging servers, so make sure your LiveServer setting is also set to yourdomain.com.' WHERE Name = 'UseSSL'
UPDATE AppConfig SET Description = 'Determines whether or not errors are logged for troubleshooting later.' WHERE Name = 'System.LoggingEnabled'
UPDATE AppConfig SET Description = 'If true, then inventory in stock table will be shown for the Product. This only works for Products using advanced inventory management.' WHERE Name = 'ShowInventoryTable'

GO
/* ======== End AppConfig Description Update ======== */

ALTER PROCEDURE [dbo].[aspdnsf_delProductRating]
	@RatingID	int
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @storeId int
	DECLARE @productId int
	DECLARE @ratingCustomerId int

	SELECT @storeId = StoreID, @productId = ProductId, @ratingCustomerId = CustomerId FROM Rating WHERE RatingId = @ratingId
	DELETE FROM RatingCommentHelpfulness WHERE StoreId = @storeId and ProductID = @productId and RatingCustomerID = @ratingCustomerId
	DELETE FROM Rating WHERE RatingID = @ratingId
END
GO

update AppConfig set GroupName = 'ENCRYPTION' where Name = 'AddressCCSaltField'
update AppConfig set GroupName = 'ENCRYPTION' where Name = 'OrdersCCSaltField'
update AppConfig set GroupName = 'GOOGLETRUSTEDSTORE' where GroupName = 'Google Trusted Store'

update AppConfig set GroupName = 'TAX' where GroupName = 'TAXES'
update AppConfig set GroupName = 'TAX' where GroupName = 'VAT'
update AppConfig set GroupName = 'TAX' where GroupName = 'AVALARATAX'

update Topic set Description = replace(description, 'sitemap.aspx', 'sitemap2.aspx')

GO

/* ======== Shipping tracking URLs Update ======== */
if (not exists (select Name from AppConfig where Name='Shipping.Tracking.Fedex'))
	insert into AppConfig (Name, ConfigValue, Description, GroupName, ValueType, SuperOnly, Hidden)
	values('Shipping.Tracking.Fedex', 'http://www.fedex.com/Tracking?action=track&tracknumbers=', 'URL for the Fedex tracking site.', 'SHIPPING', 'String', 0, 0)

if (not exists (select Name from AppConfig where Name='Shipping.Tracking.Ups'))
	insert into AppConfig (Name, ConfigValue, Description, GroupName, ValueType, SuperOnly, Hidden)
	values('Shipping.Tracking.Ups', 'http://wwwapps.ups.com/WebTracking/track?track=yes&trackNums=', 'URL for the UPS tracking site', 'SHIPPING', 'String', 0, 0)

if (not exists (select Name from AppConfig where Name='Shipping.Tracking.Usps'))
	insert into AppConfig (Name, ConfigValue, Description, GroupName, ValueType, SuperOnly, Hidden)
	values('Shipping.Tracking.Usps', 'https://tools.usps.com/go/TrackConfirmAction_input?qtc_tLabels1=', 'URL for the USPS tracking site', 'SHIPPING', 'String', 0, 0)
/* ======== End Shipping tracking URLs Update ======== */
GO

if exists(select * from sys.objects where name = 'aspdnsf_CustomerConsistencyCheck')
	drop proc dbo.aspdnsf_CustomerConsistencyCheck
GO

/* ======== BEGIN Multiple Shipping Addresses Fix ======== */
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_ReloadCart') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_ReloadCart
GO

CREATE proc [dbo].[aspdnsf_ReloadCart]
    @CartXML nvarchar(max)

AS
BEGIN
    SET NOCOUNT ON

    DECLARE @tmpShoppingCart TABLE (                                                                             [CustomerID] [int] NOT NULL , [ProductSKU] [nvarchar] (100) NULL , [ProductPrice] [money] NULL , [ProductWeight] [money] NULL , [ProductID] [int] NOT NULL , [VariantID] [int] NOT NULL , [Quantity] [int] NOT NULL , [ChosenColor] [nvarchar] (100) NULL , [ChosenColorSKUModifier] [nvarchar] (50) NULL , [ChosenSize] [nvarchar] (100) NULL , [ChosenSizeSKUModifier] [nvarchar] (50) NULL , [IsTaxable] [tinyint] NOT NULL , [IsShipSeparately] [tinyint] NOT NULL , [IsDownload] [tinyint] NOT NULL , [DownloadLocation] [nvarchar](max) NULL , [CreatedOn] [datetime] NOT NULL , [ProductDimensions] [nvarchar] (100) NULL , [CartType] [int] NOT NULL , [IsSecureAttachment] [tinyint] NOT NULL , [TextOption] [nvarchar](max) NULL , [NextRecurringShipDate] [datetime] NULL , [RecurringIndex] [int] NOT NULL , [OriginalRecurringOrderNumber] [int] NULL , [RecurringSubscriptionID] [nvarchar](100) NOT NULL, [BillingAddressID] [int] NULL , [ShippingAddressID] [int] NULL , [ShippingMethodID] [int] NULL , [ShippingMethod] [nvarchar] (max) NULL , [DistributorID] [int] NULL , [Notes] [nvarchar] (max) NULL , [IsUpsell] [tinyint] NOT NULL , [RecurringInterval] [int] NOT NULL , [RecurringIntervalType] [int] NOT NULL , [ExtensionData] [nvarchar] (max) NULL, [FreeShipping] [tinyint] NOT NULL, [CustomerEntersPrice] [tinyint] NOT NULL, [IsAKit] [tinyint] NULL, [IsAPack] [tinyint] NULL)
    DECLARE @tmpShoppingCart2 TABLE (oldCartID int not null,  [ShoppingCartRecGUID] [uniqueidentifier] NOT NULL, [CustomerID] [int] NOT NULL , [ProductSKU] [nvarchar] (100) NULL , [ProductPrice] [money] NULL , [ProductWeight] [money] NULL , [ProductID] [int] NOT NULL , [VariantID] [int] NOT NULL , [Quantity] [int] NOT NULL , [ChosenColor] [nvarchar] (100) NULL , [ChosenColorSKUModifier] [nvarchar] (50) NULL , [ChosenSize] [nvarchar] (100) NULL , [ChosenSizeSKUModifier] [nvarchar] (50) NULL , [IsTaxable] [tinyint] NOT NULL , [IsShipSeparately] [tinyint] NOT NULL , [IsDownload] [tinyint] NOT NULL , [DownloadLocation] [nvarchar](max) NULL , [CreatedOn] [datetime] NOT NULL , [ProductDimensions] [nvarchar] (100) NULL , [CartType] [int] NOT NULL , [IsSecureAttachment] [tinyint] NOT NULL , [TextOption] [nvarchar](max) NULL , [NextRecurringShipDate] [datetime] NULL , [RecurringIndex] [int] NOT NULL , [OriginalRecurringOrderNumber] [int] NULL , [RecurringSubscriptionID] [nvarchar](100) NOT NULL, [BillingAddressID] [int] NULL , [ShippingAddressID] [int] NULL , [ShippingMethodID] [int] NULL , [ShippingMethod] [nvarchar] (max) NULL , [DistributorID] [int] NULL , [Notes] [nvarchar] (max) NULL , [IsUpsell] [tinyint] NOT NULL , [RecurringInterval] [int] NOT NULL , [RecurringIntervalType] [int] NOT NULL , [ExtensionData] [nvarchar] (max) NULL, [FreeShipping] [tinyint] NOT NULL, [CustomerEntersPrice] [tinyint] NOT NULL, [IsAKit] [tinyint] NULL, [IsAPack] [tinyint] NULL)
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
    SELECT CustomerID, ProductSKU, ProductPrice, ProductWeight, ProductID,VariantID, c.qty quantity, ChosenColor, ChosenColorSKUModifier, ChosenSize,ChosenSizeSKUModifier, IsTaxable, IsShipSeparately, IsDownload,DownloadLocation, CreatedOn, ProductDimensions, CartType,IsSecureAttachment, TextOption, NextRecurringShipDate, RecurringIndex,OriginalRecurringOrderNumber, RecurringSubscriptionID, BillingAddressID,c.addressid ShippingAddressID, ShippingMethodID, ShippingMethod,DistributorID, Notes,IsUpsell, RecurringInterval,RecurringIntervalType, ExtensionData, s.FreeShipping, s.CustomerEntersPrice, s.IsAKit, s.IsAPack
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
    INSERT @tmpShoppingCart2 (oldCartID, ShoppingCartRecGUID, CustomerID,ProductSKU, ProductPrice, ProductWeight, ProductID, VariantID, Quantity,ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier,IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, CreatedOn,ProductDimensions, CartType, IsSecureAttachment, TextOption,NextRecurringShipDate, RecurringIndex, OriginalRecurringOrderNumber,RecurringSubscriptionID, BillingAddressID, ShippingAddressID,ShippingMethodID, ShippingMethod, DistributorID, Notes, IsUpsell, RecurringInterval, RecurringIntervalType, ExtensionData, FreeShipping, CustomerEntersPrice, IsAKit, IsAPack)
    SELECT c.cartid, newid(), s.CustomerID, s.ProductSKU, s.ProductPrice,s.ProductWeight, s.ProductID, s.VariantID, a.qty, s.ChosenColor,s.ChosenColorSKUModifier, s.ChosenSize, s.ChosenSizeSKUModifier,s.IsTaxable, s.IsShipSeparately, s.IsDownload, s.DownloadLocation,s.CreatedOn, s.ProductDimensions, s.CartType, s.IsSecureAttachment,s.TextOption, s.NextRecurringShipDate, s.RecurringIndex,s.OriginalRecurringOrderNumber, s.RecurringSubscriptionID,s.BillingAddressID, a.shippingaddressid, s.ShippingMethodID,s.ShippingMethod, s.DistributorID, '', s.IsUpsell, s.RecurringInterval, s.RecurringIntervalType, s.ExtensionData, s.FreeShipping, s.CustomerEntersPrice, s.IsAKit, s.IsAPack
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
        INSERT [dbo].ShoppingCart (ShoppingCartRecGUID, CustomerID,ProductSKU, ProductPrice, ProductWeight, ProductID, VariantID, Quantity,ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier,IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, CreatedOn,ProductDimensions, CartType, IsSecureAttachment, TextOption,NextRecurringShipDate, RecurringIndex, OriginalRecurringOrderNumber,RecurringSubscriptionID, BillingAddressID, ShippingAddressID,ShippingMethodID, ShippingMethod, DistributorID, Notes, IsUpsell, RecurringInterval, RecurringIntervalType, ExtensionData, FreeShipping, CustomerEntersPrice, IsAKit, IsAPack)
        SELECT ShoppingCartRecGUID, CustomerID, ProductSKU, ProductPrice,ProductWeight, ProductID, VariantID, Quantity, ChosenColor,ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, IsTaxable,IsShipSeparately, IsDownload, DownloadLocation, CreatedOn,ProductDimensions, CartType, IsSecureAttachment, TextOption,NextRecurringShipDate, RecurringIndex, OriginalRecurringOrderNumber,RecurringSubscriptionID, BillingAddressID, ShippingAddressID,ShippingMethodID, ShippingMethod, DistributorID, Notes, IsUpsell, RecurringInterval, RecurringIntervalType, ExtensionData, FreeShipping, CustomerEntersPrice, IsAKit, IsAPack
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
/* ======== BEGIN Multiple Shipping Addresses Fix ======== */
GO

alter proc dbo.aspdnsf_WSIUpdateMappings
    @xml nvarchar(max)

AS
SET NOCOUNT ON

    create table #tmp (id int not null, displayorder int)
    DECLARE @pid varchar(50), @pem varchar(50), @xpath varchar(8000), @counter int, @cmd varchar(8000)
    DECLARE @hdoc int, @retcode int, @AutoCleanup bit

    EXEC @retcode = sp_xml_preparedocument @hdoc OUTPUT, @xml

    SELECT @AutoCleanup = case AutoCleanup when 'true' then 1 else 0 end
    FROM OPENXML(@hdoc, '/Mappings', 0) WITH (AutoCleanup varchar(5))

    set @counter = 1
    select @xpath = '/Mappings/Product[' + convert(varchar(10), @counter) + ']'

    SELECT top 1 @pid = id, @pem = EntityName
    FROM OPENXML(@hdoc, @xpath, 0) WITH (id varchar(10), EntityName varchar(50))

    while @@rowcount > 0 begin
        if @pem in ('category', 'section', 'manufacturer', 'distributor', 'affiliate', 'vector', 'genre') and isnumeric(@pid) = 1 begin
            select @xpath = @xpath + '/Entity'

            truncate table #tmp
            insert #tmp
            SELECT *
            FROM OPENXML(@hdoc, @xpath, 0) WITH (id int, displayorder int)

            -- Update display order for existing mappings
            set @cmd = 'update dbo.product' + @pem + ' set displayorder = isnull(t.displayorder, pe.displayorder) from dbo.product' + @pem + ' pe with (nolock) join #tmp t on pe.Productid = ' + @pid + ' and pe.' + @pem + 'id = t.id'
            exec (@cmd)

            -- Insert new mappings
            set @cmd = 'insert dbo.product' + @pem + '(ProductID, ' + @pem + 'id, displayorder, createdon) select ' + @pid + ', id, displayorder, getdate() from #tmp where not exists (select * from dbo.product' + @pem + ' with (nolock) where ProductID = ' + @pid + ' and ' + @pem + 'id = #tmp.id)'
            exec (@cmd)

            -- if auto clenaup then remove mapping that are not in the imput xml document
            if @AutoCleanup = 1 begin
                set @cmd = 'delete dbo.product' + @pem + ' from dbo.product' + @pem + ' pe with (nolock) left join #tmp t on pe.productid = ' + @pid + ' and pe.categoryid = t.id where t.id is null'
                exec (@cmd)
            end

            set @counter = @counter + 1
            select @xpath = '/Mappings/Product[' + convert(varchar(10), @counter) + ']'

            SELECT top 1 @pid = id, @pem = EntityName
            FROM OPENXML(@hdoc, @xpath, 0) WITH (id varchar(10), EntityName varchar(50))
        end
    end

    exec sp_xml_removedocument @hdoc

    drop table #tmp
GO

/* ======== SecureNet SOPC Support ======== */

update AppConfig
set 
	Description = 'If enabled users will be able to store credit cards via the SecureNet Vault service. This feature is unavailable with SOPC.'
where 
	Name = 'SecureNetV4.VaultEnabled'

/* ======== End SecureNet SOPC Support ======== */

/*********** End 9.5.0.0 Changes *********************/

/*********** Start 9.5.1.0 Changes *********************/
--New session timeout handling
if not exists(select * from AppConfig Where Name='SessionTimeoutLandingPage')
	insert into [AppConfig] (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values(newid(), 0, 'SessionTimeoutLandingPage', 'When customers'' sessions end due to idleness, they will be sent to this page on your site.', 'default.aspx', 'string', null, 'SECURITY', 1, 0, getdate());

if not exists(select * from AppConfig Where Name='AdminSessionTimeoutInMinutes')
	insert into [AppConfig] (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values(newid(), 0, 'AdminSessionTimeoutInMinutes', 'Admin session data timeout value. Default is 15 minutes', '15', 'integer', null, 'SECURITY', 1, 0, getdate());
	
if not exists(select * from AppConfig Where Name='SessionTimeoutWarning.Enabled')
	insert into [AppConfig] (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values(newid(), 0, 'SessionTimeoutWarning.Enabled', 'If true, customers will get a warning before their sessions time out due to inactivity.', 'true', 'boolean', null, 'SECURITY', 1, 0, getdate());

if not exists(select * from Topic Where Name='SessionExpired')
INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('SessionExpired', 1, 0, 'SessionExpired', '<div class="session-warning-top-line">
																														We want to keep you safe!
																														</div>
																														<div class="session-warning-middle-line">
																														Just in case you walked away and left your screen turned on, we have expired this session.
																														</div>
																														<div class="session-warning-bottom-line">
																														Just click OK to continue.
																														</div>')
																														
if not exists(select * from Topic Where Name='SessionExpiring')
INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('SessionExpiring', 1, 0, 'SessionExpiring', '<div class="session-warning-top-line">
																														We want to keep you safe!
																														</div>
																														<div class="session-warning-middle-line">
																														Due to inactivity, your session will soon expire.
																														</div>
																														<div class="session-warning-bottom-line">
																														Just click OK to continue.
																														</div>')

--Customer & session lookup sprocs
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetCustomerByGUID') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_GetCustomerByGUID
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
            c.CustomerID, c.CustomerGUID, c.CustomerLevelID, c.RegisterDate, c.Email, c.Password, c.SaltKey, c.DateOfBirth, c.Gender,
            c.FirstName, c.LastName, c.Notes, c.SkinID, c.Phone, c.AffiliateID, c.Referrer, c.CouponCode, c.OkToEmail,
            IsAdmin&1 IsAdmin, sign(IsAdmin&2) IsSuperAdmin, c.BillingEqualsShipping, c.LastIPAddress,
            c.OrderNotes, c.RTShipRequest, c.RTShipResponse, c.OrderOptions, c.LocaleSetting,
            c.MicroPayBalance, c.RecurringShippingMethodID, c.RecurringShippingMethod, c.BillingAddressID, c.ShippingAddressID,
            c.CODCompanyCheckAllowed, c.CODNet30Allowed, c.ExtensionData,
            c.FinalizationData, c.Deleted, c.CreatedOn, c.Over13Checked, c.CurrencySetting,
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

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetCustomerByID') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_GetCustomerByID
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
            c.CustomerID, c.CustomerGUID, c.CustomerLevelID, c.RegisterDate, c.Email, c.Password, c.SaltKey, c.DateOfBirth, c.Gender,
            c.FirstName, c.LastName, c.Notes, c.SkinID, c.Phone, c.AffiliateID, c.Referrer, c.CouponCode, c.OkToEmail,
            IsAdmin&1 IsAdmin, sign(IsAdmin&2) IsSuperAdmin, c.BillingEqualsShipping, c.LastIPAddress,
            c.OrderNotes, c.RTShipRequest, c.RTShipResponse, c.OrderOptions, c.LocaleSetting,
            c.MicroPayBalance, c.RecurringShippingMethodID, c.RecurringShippingMethod, c.BillingAddressID, c.ShippingAddressID,
            c.CODCompanyCheckAllowed, c.CODNet30Allowed, c.ExtensionData,
            c.FinalizationData, c.Deleted, c.CreatedOn, c.Over13Checked, c.CurrencySetting,
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

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetCustomerByEmail') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_GetCustomerByEmail
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
            c.CustomerID, c.CustomerGUID, c.CustomerLevelID, c.RegisterDate, c.Email, c.Password, c.SaltKey, c.DateOfBirth, c.Gender,
            c.FirstName, c.LastName, c.Notes, c.SkinID, c.Phone, c.AffiliateID, c.Referrer, c.CouponCode, c.OkToEmail,
            IsAdmin&1 IsAdmin, sign(IsAdmin&2) IsSuperAdmin, c.BillingEqualsShipping, c.LastIPAddress,
            c.OrderNotes, c.RTShipRequest, c.RTShipResponse, c.OrderOptions, c.LocaleSetting,
            c.MicroPayBalance, c.RecurringShippingMethodID, c.RecurringShippingMethod, c.BillingAddressID, c.ShippingAddressID,
            c.CODCompanyCheckAllowed, c.CODNet30Allowed, c.ExtensionData,
            c.FinalizationData, c.Deleted, c.CreatedOn, c.Over13Checked, c.CurrencySetting,
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

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_SessionGetByID') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_SessionGetByID]
GO
create proc [dbo].[aspdnsf_SessionGetByID]
    @CustomerSessionID int

AS
SET NOCOUNT ON

SELECT CustomerSessionID, CustomerSessionGUID, CustomerID, SessionName, SessionValue, CreatedOn, ExpiresOn, ipaddr, LastActivity, LoggedOut
FROM dbo.Customersession
WHERE CustomerSessionID = @CustomerSessionID
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_SessionGetByCustomerID') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_SessionGetByCustomerID]
GO
create proc [dbo].[aspdnsf_SessionGetByCustomerID]
    @CustomerID int

AS
SET NOCOUNT ON

DECLARE @CustomerSessionID int

select @CustomerSessionID = max(CustomerSessionID)
from dbo.Customersession with (nolock)
WHERE CustomerID = @CustomerID

SELECT cs.CustomerSessionID, cs.CustomerSessionGUID, cs.CustomerID, cs.SessionName, cs.SessionValue, cs.CreatedOn, cs.ExpiresOn, cs.ipaddr, cs.LastActivity, cs.LoggedOut
FROM dbo.Customersession cs with (nolock)
WHERE CustomerSessionID = @CustomerSessionID
GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_SessionGetByGUID') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_SessionGetByGUID]
GO

--get_ShoppingCart tuning
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetShoppingCart') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_GetShoppingCart
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
        ShoppingCart.IsAPack,
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
        Product.ImageFileNameOverride,
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

--getProducts tuning
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetProducts') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetProducts]
GO

CREATE proc [dbo].[aspdnsf_GetProducts]
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
        p.ImageFileNameOverride,
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

--Promo query tuning
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetAvailablePromos') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetAvailablePromos]
GO

CREATE proc [dbo].[aspdnsf_GetAvailablePromos]
    @productIdList	nvarchar(max),
	@StoreID		int = 0,
	@CustomerID		int = 0,
	@CustomerLevel	int = 0

AS
BEGIN
	declare @productIds table (ProductId int not null)
	insert into @productIds select distinct * from dbo.Split(@productIdList, ',')

	declare @FilterPromotions tinyint
	SET @FilterPromotions = (SELECT case ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM GlobalConfig WHERE Name='AllowCouponFiltering')

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
				on p.Id = pt.PromotionID and p.Active = 1 and p.AutoAssigned = 1,
		@productIds ids
			left join ProductCategory pc
				on pc.ProductId = ids.ProductId
			left join ProductSection ps
				on ps.ProductId = ids.ProductId
			left join ProductManufacturer pm
				on pm.ProductId = ids.ProductId
	where
		(d.ExpDate IS NULL OR CONVERT(date, d.ExpDate) > getDate())
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
GO

--aspdnsf_EntityMgr tuning
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_EntityMgr') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_EntityMgr]
GO

create proc [dbo].[aspdnsf_EntityMgr]
    @EntityName varchar(100),
    @PublishedOnly tinyint

AS
BEGIN
    SET NOCOUNT ON
    IF @EntityName = 'Category' BEGIN
        SELECT Entity.CategoryID EntityID,
			XmlPackage,
			Name,
			ColWidth,
			ParentCategoryID ParentEntityID,
			[Description],
			DisplayOrder,
			Published,
			SEName,
			PageSize,
			SETitle,
			SEAltText,
			SEDescription,
			SEKeywords,
			TemplateName
        FROM dbo.Category Entity with (NOLOCK)
        WHERE Published >= @PublishedOnly and Deleted=0
    END

    IF @EntityName = 'Affiliate' BEGIN
        SELECT Entity.AffiliateID EntityID, 
			'' as XmlPackage,
			Name,
			4 as ColWidth,
			ParentAffiliateID ParentEntityID,
			'' as [Description],
			DisplayOrder,
			Published,
			SEName,
			0 as PageSize,
			SETitle,
			SEAltText,
			SEDescription,
			SEKeywords,
			TemplateName
        FROM dbo.Affiliate Entity with (NOLOCK)
        WHERE Published >= @PublishedOnly and Deleted=0
    END

    IF @EntityName = 'Section' BEGIN
        SELECT Entity.SectionID EntityID,
			XmlPackage,
			Name,
			ColWidth,
			ParentSectionID ParentEntityID,
			[Description],
			DisplayOrder,
			Published,
			SEName,
			PageSize,
			SETitle,
			SEAltText,
			SEDescription,
			SEKeywords,
			TemplateName
        FROM dbo.Section Entity with (NOLOCK)
        WHERE Published >= @PublishedOnly and Deleted=0
    END

    IF @EntityName = 'Manufacturer' BEGIN
        SELECT Entity.ManufacturerID EntityID,
			XmlPackage,
			Name,
			ColWidth,
			ParentManufacturerID as ParentEntityID,
			[Description],
			DisplayOrder,
			Published,
			SEName,
			PageSize,
			SETitle,
			SEAltText,
			SEDescription,
			SEKeywords,
			TemplateName
        FROM dbo.Manufacturer Entity with (NOLOCK)
        WHERE Published >= @PublishedOnly and Deleted=0
    END

    IF @EntityName = 'Library' BEGIN
        SELECT Entity.LibraryID EntityID,
			XmlPackage,
			Name,
			ColWidth,
			ParentLibraryID as ParentEntityID,
			[Description],
			DisplayOrder,
			Published,
			SEName,
			PageSize,
			SETitle,
			SEAltText,
			SEDescription,
			SEKeywords,
			TemplateName
		FROM dbo.Library Entity with (NOLOCK)
        WHERE Published >= @PublishedOnly and Deleted=0
    END

    IF @EntityName = 'Distributor' BEGIN
        SELECT Entity.DistributorID EntityID,
			XmlPackage,
			Name,
			ColWidth,
			ParentDistributorID as ParentEntityID,
			[Description],
			DisplayOrder,
			Published,
			SEName,
			PageSize,
			SETitle,
			SEAltText,
			SEDescription,
			SEKeywords,
			TemplateName
		FROM dbo.Distributor Entity with (NOLOCK)
        WHERE Published >= @PublishedOnly and Deleted=0
    END

    IF @EntityName = 'Genre' BEGIN
        SELECT Entity.GenreID EntityID,
			XmlPackage,
			Name,
			ColWidth,
			ParentGenreID as ParentEntityID,
			[Description],
			DisplayOrder,
			Published,
			SEName,
			PageSize,
			SETitle,
			SEAltText,
			SEDescription,
			SEKeywords,
			TemplateName
		FROM dbo.Genre Entity with (NOLOCK)
        WHERE Published >= @PublishedOnly and Deleted=0
    END

    IF @EntityName = 'Vector' BEGIN
        SELECT Entity.VectorID EntityID,
			XmlPackage,
			Name,
			ColWidth,
			ParentVectorID as ParentEntityID,
			[Description],
			DisplayOrder,
			Published,
			SEName,
			PageSize,
			SETitle,
			SEAltText,
			SEDescription,
			SEKeywords,
			TemplateName
		FROM dbo.Vector Entity with (NOLOCK)
        WHERE Published >= @PublishedOnly and Deleted=0
    END

    IF @EntityName = 'Customerlevel' BEGIN
        SELECT Entity.CustomerLevelID EntityID,
			'' as XmlPackage,
			Name, 
			4 as ColWidth,
			ParentCustomerLevelID ParentEntityID,
			'' as [Description],
			DisplayOrder,
			1 as Published,
			SEName,
			20 as PageSize,
			'' as SETitle,
			'' as SEAltText,
			'' as SEDescription,
			'' as SEKeywords,
			TemplateName
        FROM dbo.CustomerLevel Entity with (NOLOCK)
        WHERE Deleted=0
    END
END
GO

--aspdnsf_SessionUpdate tuning
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_SessionUpdate') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_SessionUpdate]
GO

CREATE proc [dbo].[aspdnsf_SessionUpdate]
    @CustomerSessionID int,
    @SessionName nvarchar(100),
    @SessionValue nvarchar(max),
    @ExpiresOn datetime,
    @LoggedOut datetime

AS
SET NOCOUNT ON

UPDATE dbo.Customersession
SET
    SessionName = COALESCE(@SessionName, SessionName),
    SessionValue = COALESCE(@SessionValue, SessionValue),
    ExpiresOn = COALESCE(@ExpiresOn, ExpiresOn),
    LastActivity = getdate(),
    LoggedOut = COALESCE(@LoggedOut, LoggedOut)
WHERE CustomerSessionID = @CustomerSessionID
GO

--Make SearchLog opt-in
if not exists(select * from [dbo].[AppConfig] where [Name] = 'Search_LogSearches')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [ConfigValue], [GroupName], [SuperOnly], [Hidden])
	VALUES ('Search_LogSearches', 'If true, customer searches will be logged in the SearchLog table.', 'boolean', NULL, 'false', 'SEARCH', 0, 0)
END

--Related products fixing
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetCustomersRelatedProducts') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetCustomersRelatedProducts]
GO

CREATE PROCEDURE [dbo].[aspdnsf_GetCustomersRelatedProducts]
	@CustomerViewID		NVARCHAR(50),
	@ProductID			INT,
	@CustomerLevelID	INT,
	@InvFilter			INT,
	@affiliateID		INT,
	@storeID			INT = 1,
	@filterProduct		BIT = 0

AS
SET NOCOUNT ON

DECLARE
	   @custlevelcount INT,
	   @CustomerLevelFilteringIsAscending BIT = 0,
	   @FilterProductsByCustomerLevel BIT,
	   @relatedprods VARCHAR(8000),
	   @DynamicProductsEnabled VARCHAR(10),
	   @ProductsDisplayed INT,
	   @FilterProductsByAffiliate BIT,
	   @affiliatecount INT,
	   @AffiliateExists INT

--Configs
SET @FilterProductsByCustomerLevel = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
SET @CustomerLevelFilteringIsAscending = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterByCustomerLevelIsAscending' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
SET @FilterProductsByAffiliate = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
SET @DynamicProductsEnabled = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'DynamicRelatedProducts.Enabled' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
SET @ProductsDisplayed = (SELECT TOP 1 CAST(ConfigValue AS INT) FROM dbo.AppConfig with (nolock) WHERE [Name] = 'RelatedProducts.NumberDisplayed' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)

--Counts
SELECT @custlevelcount = COUNT(*) FROM ProductCustomerLevel
SELECT @affiliatecount = COUNT(*) FROM dbo.ProductAffiliate WITH (NOLOCK) WHERE ProductID = @ProductID
SELECT @AffiliateExists = CASE WHEN AffiliateID = @affiliateID THEN 1 ELSE 0 END FROM dbo.ProductAffiliate WITH (NOLOCK) WHERE ProductID = @ProductID

--Temp table for fixed related products
select @relatedprods = replace(cast(relatedproducts as varchar(8000)), ' ', '') from dbo.product with (NOLOCK) where productid = @productid
DECLARE @RelatedProductsTable table (ProductId int not null)
insert into @RelatedProductsTable select distinct * from dbo.Split(@relatedprods, ',')

IF(@DynamicProductsEnabled = 1 and @ProductsDisplayed > 0)
BEGIN
	SELECT TOP (@ProductsDisplayed)
	           tp.ProductID
			 , tp.ProductGUID
			 , tp.ImageFilenameOverride
			 , tp.SKU
			 , ISNULL(PRODUCTVARIANT.SkuSuffix, '') AS SkuSuffix
			 , ISNULL(PRODUCTVARIANT.ManufacturerPartNumber, '') AS VariantManufacturerPartNumber
			 , ISNULL(tp.ManufacturerPartNumber, '') AS ManufacturerPartNumber
		     , ISNULL(PRODUCTVARIANT.Dimensions, '') AS Dimensions
			 , PRODUCTVARIANT.Weight
			 , ISNULL(PRODUCTVARIANT.GTIN, '') AS GTIN
			 , PRODUCTVARIANT.VariantID
			 , PRODUCTVARIANT.Condition
			 , tp.SEAltText
			 , tp.Name
			 , tp.Description
			 , ProductManufacturer.ManufacturerID AS ProductManufacturerId
			 , Manufacturer.Name AS ProductManufacturerName
			 , Manufacturer.SEName AS ProductManufacturerSEName
		FROM Product tp WITH (NOLOCK)
		JOIN (SELECT p.ProductID
				   , p.ProductGUID
				   , p.ImageFilenameOverride
				   , p.SKU
				   , p.SEAltText
				   , p.Name
				   , p.Description
				FROM dbo.product p WITH (NOLOCK)
				JOIN @RelatedProductsTable rp ON p.productid = rp.ProductId
		   LEFT JOIN dbo.productcustomerlevel pcl WITH (NOLOCK) ON p.productid = pcl.productid and @FilterProductsByCustomerLevel = 1
				JOIN (SELECT p.ProductID
						FROM dbo.product p WITH (NOLOCK)
						JOIN @RelatedProductsTable rp on p.productid = rp.ProductId
						JOIN (SELECT ProductID
								   , SUM(Inventory) Inventory
								FROM dbo.productvariant WITH (NOLOCK) GROUP BY ProductID) pv ON p.ProductID = pv.ProductID
						   LEFT JOIN (SELECT pv1.ProductID
										   , SUM(quan) inventory
										FROM dbo.inventory i1 WITH (NOLOCK)
										JOIN dbo.productvariant pv1 WITH (NOLOCK) ON pv1.variantid = i1.variantid
										JOIN @RelatedProductsTable rp1 ON pv1.productid = rp1.ProductID GROUP BY pv1.productid) i ON i.productid = p.productid
								  WHERE CASE p.TrackInventoryBySizeAndColor WHEN 1 THEN ISNULL(i.inventory, 0) ELSE pv.inventory END >= @InvFilter
					 ) tp on p.productid = tp.productid
			   WHERE published = 1
				 AND deleted = 0
				 AND p.productid != @productid
				 AND CASE
					 WHEN @FilterProductsByCustomerLevel = 0 THEN 1
					 WHEN @CustomerLevelFilteringIsAscending = 1 AND pcl.CustomerLevelID <= @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1
					 WHEN @CustomerLevelID=0 AND pcl.CustomerLevelID IS NULL THEN 1
					 WHEN @CustomerLevelID IS NULL OR @custlevelcount = 0 THEN 1
					 WHEN pcl.CustomerLevelID = @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1
					 ELSE 0
					 END = 1
	UNION ALL
	   SELECT pr.ProductID
			, pr.ProductGUID
			, pr.ImageFilenameOverride
			, pr.SKU
			, pr.SEAltText
			, pr.Name
			, pr.Description
		 FROM Product pr WITH (NOLOCK)
		WHERE pr.ProductID IN (
		SELECT TOP 100 PERCENT p.ProductID
		  FROM Product p WITH (NOLOCK)
		  JOIN (SELECT ProductID
				  FROM ProductView WITH (NOLOCK) WHERE CustomerViewID
					IN (SELECT CustomerViewID
						  FROM ProductView WITH (NOLOCK)
						 WHERE ProductID = @ProductID
						   AND CustomerViewID <> @CustomerViewID
					   )
				   AND ProductID <> @ProductID
				   AND ProductID NOT
					IN (SELECT p.ProductID
						  FROM product p WITH (NOLOCK)
						  JOIN @RelatedProductsTable rp ON p.productid = rp.ProductId
					  GROUP BY p.ProductID
					   )
				) a ON p.ProductID = a.ProductID
	LEFT JOIN dbo.productcustomerlevel pcl WITH (NOLOCK) ON p.productid = pcl.productid and @FilterProductsByCustomerLevel = 1
	LEFT JOIN dbo.ProductAffiliate pa WITH (NOLOCK) ON p.ProductID = pa.ProductID
		WHERE Published = 1 AND Deleted = 0
		 AND CASE
			 WHEN @FilterProductsByCustomerLevel = 0 THEN 1
			 WHEN @CustomerLevelFilteringIsAscending = 1 AND pcl.CustomerLevelID <= @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1
			 WHEN @CustomerLevelID=0 AND pcl.CustomerLevelID IS NULL THEN 1
			 WHEN @CustomerLevelID IS NULL OR @custlevelcount = 0 THEN 1
			 WHEN pcl.CustomerLevelID = @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1
			 ELSE 0
			  END = 1
		AND (pa.AffiliateID = @affiliateID OR pa.AffiliateID IS NULL OR @affiliatecount = 0 OR @FilterProductsByAffiliate = 0)
	GROUP BY p.ProductID
	ORDER BY COUNT(*) DESC
		)
	  )prd ON tp.ProductID = prd.ProductID
	 LEFT JOIN dbo.ProductManufacturer WITH (NOLOCK) ON tp.ProductID = ProductManufacturer.ProductID
	 LEFT JOIN dbo.Manufacturer WITH (NOLOCK) ON ProductManufacturer.ManufacturerID = Manufacturer.ManufacturerID
	      JOIN PRODUCTVARIANT WITH (NOLOCK) ON PRODUCTVARIANT.productid = CAST(tp.ProductID AS INT) AND PRODUCTVARIANT.isdefault = 1 AND PRODUCTVARIANT.Published = 1 AND PRODUCTVARIANT.Deleted = 0
	INNER JOIN (SELECT DISTINCT a.ProductID
				  FROM Product a WITH (NOLOCK)
			 LEFT JOIN ProductStore b WITH (NOLOCK) ON a.ProductID = b.ProductID
				 WHERE (@filterProduct = 0 OR StoreID = @storeID)) ps ON tp.ProductID = ps.ProductID
END

IF(@DynamicProductsEnabled = 0 and @ProductsDisplayed > 0)
BEGIN
	select TOP (@ProductsDisplayed)
	           p.ProductID
			 , p.ProductGUID
			 , p.ImageFilenameOverride
			 , p.SKU
			 , ISNULL(PRODUCTVARIANT.SkuSuffix, '') AS SkuSuffix
			 , ISNULL(PRODUCTVARIANT.ManufacturerPartNumber, '') AS VariantManufacturerPartNumber
			 , ISNULL(p.ManufacturerPartNumber, '') AS ManufacturerPartNumber
		     , ISNULL(PRODUCTVARIANT.Dimensions, '') AS Dimensions
			 , PRODUCTVARIANT.Weight
			 , ISNULL(PRODUCTVARIANT.GTIN, '') AS GTIN
			 , PRODUCTVARIANT.VariantID
			 , PRODUCTVARIANT.Condition
			 , p.SEAltText
			 , p.Name
			 , p.Description
			 , ProductManufacturer.ManufacturerID AS ProductManufacturerId
			 , Manufacturer.Name AS ProductManufacturerName
			 , Manufacturer.SEName AS ProductManufacturerSEName
		  FROM dbo.product p WITH (NOLOCK)
	 LEFT JOIN dbo.ProductManufacturer WITH (NOLOCK) ON p.ProductID = ProductManufacturer.ProductID
	 LEFT JOIN dbo.Manufacturer WITH (NOLOCK) ON ProductManufacturer.ManufacturerID = Manufacturer.ManufacturerID
		  JOIN @RelatedProductsTable rp ON p.productid = rp.ProductId
		  JOIN PRODUCTVARIANT WITH (NOLOCK) ON PRODUCTVARIANT.productid = rp.ProductId AND PRODUCTVARIANT.isdefault = 1 AND PRODUCTVARIANT.Published = 1 AND PRODUCTVARIANT.Deleted = 0
	 LEFT JOIN dbo.productcustomerlevel pcl WITH (NOLOCK) ON p.productid = pcl.productid AND @FilterProductsByCustomerLevel = 1
		  JOIN (SELECT p.ProductID
				  FROM dbo.product p WITH (NOLOCK)
				  JOIN @RelatedProductsTable rp on p.productid = rp.ProductId
				  JOIN (SELECT ProductID
							 , SUM(Inventory) Inventory
						  FROM dbo.productvariant WITH (NOLOCK)
					  GROUP BY ProductID) pv ON p.ProductID = pv.ProductID
					 LEFT JOIN (SELECT pv1.ProductID
									 , SUM(quan) inventory
								  FROM dbo.inventory i1 WITH (NOLOCK)
								  JOIN dbo.productvariant pv1 WITH (NOLOCK) ON pv1.variantid = i1.variantid
								  JOIN @RelatedProductsTable rp1 ON pv1.productid = rp1.ProductId
							  GROUP BY pv1.productid) i ON i.productid = p.productid
								 WHERE CASE p.TrackInventoryBySizeAndColor WHEN 1 THEN ISNULL(i.inventory, 0) ELSE pv.inventory END >= @InvFilter
								) tp ON p.productid = tp.productid
					INNER JOIN (SELECT DISTINCT a.ProductID
								  FROM Product a WITH (NOLOCK)
							 LEFT JOIN ProductStore b WITH (NOLOCK) ON a.ProductID = b.ProductID
								 WHERE (@filterProduct = 0 OR StoreID = @storeID)
								) ps ON p.ProductID = ps.ProductID
						 WHERE p.published = 1 and p.deleted = 0 and p.productid != @productid
						   AND CASE
							   WHEN @FilterProductsByCustomerLevel = 0 THEN 1
							   WHEN @CustomerLevelFilteringIsAscending = 1 AND pcl.CustomerLevelID <= @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1
							   WHEN @CustomerLevelID=0 AND pcl.CustomerLevelID IS NULL THEN 1
							   WHEN @CustomerLevelID IS NULL OR @custlevelcount = 0 THEN 1
							   WHEN pcl.CustomerLevelID = @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1
							   else 0
								end = 1
END
GO

--Add statistics
IF NOT EXISTS (SELECT * FROM sys.stats where Name = '_OR_Aspdnsf_Product_I_P_D_P')
    CREATE STATISTICS [_OR_Aspdnsf_Product_I_P_D_P] ON [dbo].[Product]([IsFeatured], [Published], [Deleted], [ProductID])
GO
IF NOT EXISTS (SELECT * FROM sys.stats where Name = '_OR_Aspdnsf_Profile_C_S_P')
	CREATE STATISTICS [_OR_Aspdnsf_Profile_C_S_P] ON [dbo].[Profile]([CustomerGUID], [StoreID], [PropertyName])
GO
IF NOT EXISTS (SELECT * FROM sys.stats where Name = '_OR_Aspdnsf_ProductVariant_V_P_D')
    CREATE STATISTICS [_OR_Aspdnsf_ProductVariant_V_P_D] ON [dbo].[ProductVariant]([VariantID], [Published], [Deleted])
GO
IF NOT EXISTS (SELECT * FROM sys.stats where Name = '_OR_Aspdnsf_ShoppingCart_S_C_Q')
    CREATE STATISTICS [_OR_Aspdnsf_ShoppingCart_S_C_Q] ON [dbo].[ShoppingCart]([CustomerID], [Quantity])
GO
IF NOT EXISTS (SELECT * FROM sys.stats where Name = '_OR_Aspdnsf_ShoppingCart_I_C_S')
    CREATE STATISTICS [_OR_Aspdnsf_ShoppingCart_I_C_S] ON [dbo].[ShoppingCart]([IsKit2], [CustomerID], [ShoppingCartRecID])
GO
IF NOT EXISTS (SELECT * FROM sys.stats where Name = '_OR_Aspdnsf_ShoppingCart_S_P_V_C_C')
    CREATE STATISTICS [_OR_Aspdnsf_ShoppingCart_S_P_V_C_C] ON [dbo].[ShoppingCart]([ShoppingCartRecID], [ProductID], [VariantID], [CustomerID], [CartType])
GO

--Expand monthly maintenance to update stats
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_MonthlyMaintenance') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_MonthlyMaintenance
GO

create proc [dbo].[aspdnsf_MonthlyMaintenance]
--	BACKUP YOUR DB BEFORE USING THIS SCRIPT!
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
	@cleanOrphanedLocalizedNames tinyint = 0
as
begin
	set nocount on

	-- Clear out failed transactions older than 2 months:
	delete from FailedTransaction where OrderDate < dateadd(mm,-2,getdate());

	-- Clear out old tx info, not longer needed:
	update orders set TransactionCommand=NULL, AuthorizationResult=NULL, VoidTXCommand=NULL, VoidTXResult=NULL, CaptureTXCommand=NULL, CaptureTXResult=NULL, RefundTXCommand=NULL, RefundTXResult=NULL where orderdate < dateadd(mm,-2,getdate());

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
			CardExpirationYear=NULL,
			eCheckBankABACode=NULL,
			eCheckBankAccountNumber=NULL
		where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)

	-- erase credit cards from all orders older than N days:
	if @eraseCCFromOrdersOlderThan <> 0
		update [dbo].orders set CardNumber=NULL, eCheckBankABACode=NULL,eCheckBankAccountNumber=NULL
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
	if @defragIndexes = 1
	begin
		create table #SHOWCONTIG (
			tblname varchar (255),
			ObjectId int,
			IndexName varchar (255),
			IndexId int,
			Lvl int,
			CountPages int,
			CountRows int,
			MinRecSize int,
			MaxRecSize int,
			AvgRecSize int,
			ForRecCount int,
			Extents int,
			ExtentSwitches int,
			AvgFreeBytes int,
			AvgPageDensity int,
			ScanDensity decimal,
			BestCount int,
			ActualCount int,
			LogicalFrag decimal,
			ExtentFrag decimal)

		select [name] tblname into #tmp from dbo.sysobjects with (nolock) where type = 'u' order by Name
		
		declare @cmd varchar(max)
		declare @tblname varchar(255), @indexname varchar(255)
		select top 1 @tblname = tblname from #tmp
		while @@rowcount > 0 begin
			set @cmd = 'DBCC SHOWCONTIG (''' + @tblname + ''') with tableresults, ALL_INDEXES'
			insert #SHOWCONTIG
			exec (@cmd)
			delete #tmp where tblname = @tblname
			select top 1 @tblname = tblname from #tmp
		end

		delete #SHOWCONTIG where LogicalFrag < 5 or Extents = 1 or IndexId in (0, 255)

		select top 1 @tblname = tblname, @indexname = IndexName from #SHOWCONTIG order by IndexId
		while @@rowcount > 0 begin
			set @cmd = 'DBCC DBREINDEX (''' + @tblname + ''', ''' + @indexname + ''', 90)  '
			exec (@cmd)
			delete #SHOWCONTIG where tblname = @tblname
			select top 1 @tblname = tblname, @indexname = IndexName from #SHOWCONTIG order by IndexId
		end;
	end

	--Update statistics, including ones that updating indexes misses
	if @updateStats = 1
	begin
		exec sp_updatestats @resample = 'resample'
	end
end
go

--Tuning for aspdnsf_ProductSequence
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_ProductSequence') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].aspdnsf_ProductSequence
GO

create proc [dbo].[aspdnsf_ProductSequence]
    @positioning varchar(10), -- allowed values: first, next, previous, last
    @ProductID int,
    @EntityName varchar(20),
    @EntityID int,
    @ProductTypeID int = null,
    @IncludeKits tinyint = 1 ,
    @SortByLooks tinyint = 0,
    @CustomerLevelID int = null,
    @affiliateID     int = null,
    @StoreID	int = null,
    @FilterProductsByStore tinyint = 0,
    @FilterOutOfStockProducts tinyint = 0

AS
BEGIN
    SET NOCOUNT ON

    DECLARE @id int, @row int
    DECLARE @affiliatecount int
    CREATE TABLE #sequence (row int identity not null, productid int not null)

    DECLARE @FilterProductsByAffiliate tinyint, @FilterProductsByCustomerLevel tinyint, @HideProductsWithLessThanThisInventoryLevel int, @CustomerLevelFilteringIsAscending bit

    SET @FilterProductsByCustomerLevel = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @FilterProductsByAffiliate = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @HideProductsWithLessThanThisInventoryLevel = (SELECT TOP 1 case ConfigValue when -1 then 0 else ConfigValue end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)

    SET @CustomerLevelFilteringIsAscending  = 0
    SET @CustomerLevelFilteringIsAscending = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterByCustomerLevelIsAscending' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)

    select @affiliatecount = COUNT(*) FROM ProductAffiliate

    IF @positioning not in ('first', 'next', 'previous', 'last')
        SET @positioning = 'first'

    insert #sequence (productid)
    select pe.productid
    from dbo.ProductEntity             pe  with (nolock)
        join [dbo].Product             p   with (nolock) on p.ProductID = pe.ProductID and pe.EntityType = @EntityName and pe.EntityID = @EntityID
        left join ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID
        left join ProductAffiliate     pa  with (nolock) on p.ProductID = pa.ProductID
		left join ProductVariant pv		   with (nolock) on p.ProductID = pv.ProductID  and pv.IsDefault = 1
        left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i  on pv.VariantID = i.VariantID
    where p.ProductTypeID = coalesce(nullif(@ProductTypeID, 0), p.ProductTypeID) and p.Deleted = 0 and p.Published = 1 and p.IsAKit <= @IncludeKits
          and (case
                when @FilterProductsByCustomerLevel = 0 then 1
                when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID or pcl.CustomerLevelID is null then 1
                when @CustomerLevelID=0 and pcl.CustomerLevelID is null then 1
                when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1
                else 0
               end  = 1
              )
          and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliatecount = 0 or @FilterProductsByAffiliate = 0)
          and ((case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end >= @HideProductsWithLessThanThisInventoryLevel) OR @FilterOutOfStockProducts = 0)
		  and (p.ProductID IN (SELECT ProductID FROM ProductStore WHERE StoreID=@StoreID) OR @FilterProductsByStore = 0)
	order by pe.DisplayOrder, p.Name

    SELECT @row = row FROM #sequence WHERE ProductID = @ProductID

    IF @positioning = 'next' BEGIN
        SELECT top 1 @id = ProductID
        FROM #sequence
        WHERE row > @row
        ORDER BY row

        IF @id is null
            SET @positioning = 'first'
    END

    IF @positioning = 'previous' BEGIN
        SELECT top 1 @id = ProductID
        FROM #sequence
        WHERE row < @row
        ORDER BY row desc

        IF @id is null
            SET @positioning = 'last'
    END

    IF @positioning = 'first'
        SELECT top 1 @id = ProductID
        FROM #sequence
        ORDER BY row

    IF @positioning = 'last'
        SELECT top 1 @id = ProductID
        FROM #sequence
        ORDER BY row desc

    SELECT ProductID, SEName FROM dbo.Product with (nolock) WHERE ProductID = @id
END
GO

update appconfig set description = 'If Yes, the merchant gateway IS called when processing an order. If No, then the merchant gateway code is bypassed, and an OK status is returned. No is acceptable for development/testing. Almost always must be yes for a ''live'' store site.' where name = 'UseLiveTransactions'
update appconfig set description = 'If Yes, https links will be used for shoppingcart pages, account pages, receipt pages, etc. Only set this value Yes AFTER you have your Secure Certificate (SSL cert) installed on your live server. SSL also is ONLY invoked on the live server, not the development or staging servers, so make sure your LiveServer setting is also set to yourdomain.com.' where name = 'UseSSL'
update appconfig set description = 'If Yes, the MaxMind fraud prevention score will be checked before a credit card is sent to the gateway. If the returned FraudScore exceeds AppLogic.MaxMind.FailScoreThreshold, the order will be failed. See MaxMind.com for more documentation. This feature uses MaxMind''s minFraud service version 1.3' where name = 'MaxMind.Enabled'
update appconfig set description = 'Enables customers to check out using PayPal Express without being a registered customer. If this setting is Yes, then also set AllowCustomerDuplicateEMailAddresses setting to Yes.' where name = 'PayPal.Express.AllowAnonCheckout'
update appconfig set description = 'Set to Yes to put Checkout By Amazon services in sandbox mode.' where name = 'CheckoutByAmazon.UseSandbox'
update appconfig set description = 'If Yes, will force PayPal & PayPal Express Checkout payments to Capture, regardless of TransactionMode. If No, these payments will honor the TransactionMode setting.' where name = 'PayPal.ForceCapture'
update appconfig set description = 'If Yes, the PayPal Express checkout button appears on the shopping cart page. You must also properly configure your PayPal API credentials.' where name = 'PayPal.Express.ShowOnCartPage'
update appconfig set description = 'To require the customer to check out only with a Confirmed PayPal Shipping Address set to Yes, to allow any address, set to No. It is recommended that this be set to Yes for Seller Protection.' where name = 'PayPal.RequireConfirmedAddress'
update appconfig set description = 'Set to Yes to use PayPal Instant Notifications to capture payments.' where name = 'PayPal.UseInstantNotification'
update appconfig set description = 'If Yes (not recommended), customer credit card information is encrypted and stored in the database. If No, credit card information is not stored anywhere within AspDotNetStorefront. We highly recommend setting this value to No and not storing credit card information unless you have a specific reason to do so (e.g. recurring orders, and even then only under certain circumstances). Note that CCV codes are never stored anywhere within AspDotNetStorefront.' where name = 'StoreCCInDB'
update appconfig set description = 'If Yes, a javascript validator will execute on any page which requires credit card entry, in order to enforce the number is valid.' where name = 'ValidateCreditCardNumbers'
update appconfig set description = 'If this is set to Yes and MicroPay is enabled as a payment method, the "add $5 to your micropay" Product line item will NOT appear on the shopping cart page. This is helpful if the store administrator is controlling their micropay balance using some other means.' where name = 'Micropay.HideOnCartPage'
update appconfig set description = 'If Yes and Micropay is enabled as a payment method, the user''s current Micropay balance appears at the top of the shopping cart page. If No, Micropay balance does not appear.' where name = 'Micropay.ShowTotalOnTopOfCartPage'
update appconfig set description = 'Set to Yes to enable Authorize.net CIM' where name = 'AUTHORIZENET_Cim_Enabled'
update appconfig set description = 'Set to Yes to use Authorize.net CIM in sandbox mode.' where name = 'AUTHORIZENET_Cim_UseSandbox'
update appconfig set description = 'Set to Yes if you want the Authorize.net gateway to validate the customer billing address against the credit card. Note that this may decrease fraud, but may also cause valid transactions fail.' where name = 'AUTHORIZENET_Verify_Addresses'
update appconfig set description = 'Set to Yes if you want the eProcessingNetwork gateway to validate customer billing address against the credit card. Note that this may decrease fraud, but may also cause valid transactions to fail if they have different punctuation than on their credit card.' where name = 'eProcessingNetwork_Verify_Addresses'
update appconfig set description = 'Set to Yes to use the Simulator URLs. This overrides both the Live and Test URLs.' where name = 'SagePayUK.UseSimulator'

/*********** End 9.5.1.0 Changes *********************/

PRINT CHAR(10)
PRINT '*****Finalizing database upgrade*****'
-- Update database indexes
PRINT 'Updating database indexes...'
EXEC [dbo].[aspdnsf_UpdateIndexes]
-- Update store version
PRINT 'Updating Store Version...'
UPDATE [dbo].[AppConfig] SET [ConfigValue] = '9.5.1.0' WHERE [Name] = 'StoreVersion'
print '*****Database Upgrade Completed*****'

SET NOEXEC OFF
GO
