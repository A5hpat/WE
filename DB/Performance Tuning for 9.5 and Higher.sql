-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit our homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT.
-- ------------------------------------------------------------------------------------------

/******** AspDotNetStorefront Performance Tuning Script ***********/
/*                                                                */
/*  This script will provide a performance boost to some sites    */
/*  running AspDotNetStorefront version 9.5.0.0 or higher.        */
/*															      */
/*  It must be run AFTER the upgrade to 9.5+ has been performed.  */
/*                                                                */
/*  This script can run for a LONG time, and can cause the site   */
/*  to slow down while it is running.  Work with your developer   */
/*  and host to schedule the best time to run it.                 */
/*                                                                */
/* ************************************************************** */

--Setting the column equal to itself will copy any data small enough to fit into the row instead of LOB now that 
--these columns are no longer NTEXTs, which increases performance
UPDATE [dbo].[Address] SET [CardIssueNumber] = [CardIssueNumber];
GO
UPDATE [dbo].[Address] SET [CardStartDate] = [CardStartDate];
GO
UPDATE [dbo].[Address] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[AffiliateActivity] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[AffiliateActivity] SET [Notes] = [Notes];
GO
UPDATE [dbo].[AffiliateCommissions] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Affiliate] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Affiliate] SET [Notes] = [Notes];
GO
UPDATE [dbo].[Affiliate] SET [SEAltText] = [SEAltText];
GO
UPDATE [dbo].[Affiliate] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[Affiliate] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[Affiliate] SET [SETitle] = [SETitle];
GO
UPDATE [dbo].[Affiliate] SET [URL] = [URL];
GO
UPDATE [dbo].[Affiliate] SET [WebSiteDescription] = [WebSiteDescription];
GO
UPDATE [dbo].[AppConfig] SET [Description] = [Description];
GO
UPDATE [dbo].[Category] SET [Description] = [Description];
GO
UPDATE [dbo].[Category] SET [DisplayPrefix] = [DisplayPrefix];
GO
UPDATE [dbo].[Category] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Category] SET [ImageFilenameOverride] = [ImageFilenameOverride];
GO
UPDATE [dbo].[Category] SET [RelatedDocuments] = [RelatedDocuments];
GO
UPDATE [dbo].[Category] SET [SEAltText] = [SEAltText];
GO
UPDATE [dbo].[Category] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[Category] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[Category] SET [SETitle] = [SETitle];
GO
UPDATE [dbo].[Category] SET [Summary] = [Summary];
GO
UPDATE [dbo].[Country] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Coupon] SET [Description] = [Description];
GO
UPDATE [dbo].[Coupon] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Coupon] SET [ValidForCategories] = [ValidForCategories];
GO
UPDATE [dbo].[Coupon] SET [ValidForCustomers] = [ValidForCustomers];
GO
UPDATE [dbo].[Coupon] SET [ValidForManufacturers] = [ValidForManufacturers];
GO
UPDATE [dbo].[Coupon] SET [ValidForProducts] = [ValidForProducts];
GO
UPDATE [dbo].[Coupon] SET [ValidForSections] = [ValidForSections];
GO
UPDATE [dbo].[Currency] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[CustomReport] SET [Description] = [Description];
GO
UPDATE [dbo].[CustomReport] SET [SQLCommand] = [SQLCommand];
GO
UPDATE [dbo].[CustomerLevel] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[CustomerSession] SET [SessionValue] = [SessionValue];
GO
UPDATE [dbo].[Customer] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Customer] SET [FinalizationData] = [FinalizationData];
GO
UPDATE [dbo].[Customer] SET [Notes] = [Notes];
GO
UPDATE [dbo].[Customer] SET [OrderNotes] = [OrderNotes];
GO
UPDATE [dbo].[Customer] SET [OrderOptions] = [OrderOptions];
GO
UPDATE [dbo].[Customer] SET [RTShipRequest] = [RTShipRequest];
GO
UPDATE [dbo].[Customer] SET [RTShipResponse] = [RTShipResponse];
GO
UPDATE [dbo].[Customer] SET [RecurringShippingMethod] = [RecurringShippingMethod];
GO
UPDATE [dbo].[Customer] SET [Referrer] = [Referrer];
GO
UPDATE [dbo].[Distributor] SET [Description] = [Description];
GO
UPDATE [dbo].[Distributor] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Distributor] SET [ImageFilenameOverride] = [ImageFilenameOverride];
GO
UPDATE [dbo].[Distributor] SET [Notes] = [Notes];
GO
UPDATE [dbo].[Distributor] SET [SEAltText] = [SEAltText];
GO
UPDATE [dbo].[Distributor] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[Distributor] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[Distributor] SET [SETitle] = [SETitle];
GO
UPDATE [dbo].[Distributor] SET [Summary] = [Summary];
GO
UPDATE [dbo].[Document] SET [Description] = [Description];
GO
UPDATE [dbo].[Document] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Document] SET [ImageFilenameOverride] = [ImageFilenameOverride];
GO
UPDATE [dbo].[Document] SET [MiscText] = [MiscText];
GO
UPDATE [dbo].[Document] SET [Notes] = [Notes];
GO
UPDATE [dbo].[Document] SET [RelatedCategories] = [RelatedCategories];
GO
UPDATE [dbo].[Document] SET [RelatedManufacturers] = [RelatedManufacturers];
GO
UPDATE [dbo].[Document] SET [RelatedProducts] = [RelatedProducts];
GO
UPDATE [dbo].[Document] SET [RelatedSections] = [RelatedSections];
GO
UPDATE [dbo].[Document] SET [SEAltText] = [SEAltText];
GO
UPDATE [dbo].[Document] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[Document] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[Document] SET [SETitle] = [SETitle];
GO
UPDATE [dbo].[Document] SET [Summary] = [Summary];
GO
UPDATE [dbo].[ErrorLog] SET [ErrorMsg] = [ErrorMsg];
GO
UPDATE [dbo].[ExtendedPrice] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[FailedTransaction] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[FailedTransaction] SET [MaxMindDetails] = [MaxMindDetails];
GO
UPDATE [dbo].[FailedTransaction] SET [TransactionCommand] = [TransactionCommand];
GO
UPDATE [dbo].[FailedTransaction] SET [TransactionResult] = [TransactionResult];
GO
UPDATE [dbo].[Feed] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Genre] SET [Description] = [Description];
GO
UPDATE [dbo].[Genre] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Genre] SET [ImageFilenameOverride] = [ImageFilenameOverride];
GO
UPDATE [dbo].[Genre] SET [Notes] = [Notes];
GO
UPDATE [dbo].[Genre] SET [SEAltText] = [SEAltText];
GO
UPDATE [dbo].[Genre] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[Genre] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[Genre] SET [SETitle] = [SETitle];
GO
UPDATE [dbo].[Genre] SET [Summary] = [Summary];
GO
UPDATE [dbo].[GiftCardUsage] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[GiftCard] SET [EMailMessage] = [EMailMessage];
GO
UPDATE [dbo].[GiftCard] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[GiftCard] SET [ValidForCategories] = [ValidForCategories];
GO
UPDATE [dbo].[GiftCard] SET [ValidForCustomers] = [ValidForCustomers];
GO
UPDATE [dbo].[GiftCard] SET [ValidForManufacturers] = [ValidForManufacturers];
GO
UPDATE [dbo].[GiftCard] SET [ValidForProducts] = [ValidForProducts];
GO
UPDATE [dbo].[GiftCard] SET [ValidForSections] = [ValidForSections];
GO
UPDATE [dbo].[GlobalConfig] SET [Description] = [Description];
GO
UPDATE [dbo].[Inventory] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[KitCart] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[KitCart] SET [TextOption] = [TextOption];
GO
UPDATE [dbo].[KitGroup] SET [Description] = [Description];
GO
UPDATE [dbo].[KitGroup] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[KitGroup] SET [Summary] = [Summary];
GO
UPDATE [dbo].[KitItem] SET [Description] = [Description];
GO
UPDATE [dbo].[KitItem] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Library] SET [Description] = [Description];
GO
UPDATE [dbo].[Library] SET [DisplayPrefix] = [DisplayPrefix];
GO
UPDATE [dbo].[Library] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Library] SET [ImageFilenameOverride] = [ImageFilenameOverride];
GO
UPDATE [dbo].[Library] SET [RelatedCategories] = [RelatedCategories];
GO
UPDATE [dbo].[Library] SET [RelatedManufacturers] = [RelatedManufacturers];
GO
UPDATE [dbo].[Library] SET [RelatedProducts] = [RelatedProducts];
GO
UPDATE [dbo].[Library] SET [RelatedSections] = [RelatedSections];
GO
UPDATE [dbo].[Library] SET [SEAltText] = [SEAltText];
GO
UPDATE [dbo].[Library] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[Library] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[Library] SET [SETitle] = [SETitle];
GO
UPDATE [dbo].[Library] SET [Summary] = [Summary];
GO
--MailingMgrLog table is no longer used, but is not being dropped from the schema so people don't lose mailing history data.  New DBs will not have this, so skip it.
IF EXISTS (select * From sysobjects where id = object_id('[dbo].[MailingMgrLog]') and type = 'u')
BEGIN
	UPDATE [dbo].[MailingMgrLog] SET [Body] = [Body];
END
GO
UPDATE [dbo].[Manufacturer] SET [Description] = [Description];
GO
UPDATE [dbo].[Manufacturer] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Manufacturer] SET [ImageFilenameOverride] = [ImageFilenameOverride];
GO
UPDATE [dbo].[Manufacturer] SET [Notes] = [Notes];
GO
UPDATE [dbo].[Manufacturer] SET [RelatedDocuments] = [RelatedDocuments];
GO
UPDATE [dbo].[Manufacturer] SET [SEAltText] = [SEAltText];
GO
UPDATE [dbo].[Manufacturer] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[Manufacturer] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[Manufacturer] SET [SETitle] = [SETitle];
GO
UPDATE [dbo].[Manufacturer] SET [Summary] = [Summary];
GO
UPDATE [dbo].[News] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[News] SET [NewsCopy] = [NewsCopy];
GO
UPDATE [dbo].[OrderOption] SET [Description] = [Description];
GO
UPDATE [dbo].[OrderOption] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Orders] SET [AuthorizationResult] = [AuthorizationResult];
GO
UPDATE [dbo].[Orders] SET [CaptureTXCommand] = [CaptureTXCommand];
GO
UPDATE [dbo].[Orders] SET [CaptureTXResult] = [CaptureTXResult];
GO
UPDATE [dbo].[Orders] SET [CardIssueNumber] = [CardIssueNumber];
GO
UPDATE [dbo].[Orders] SET [CardStartDate] = [CardStartDate];
GO
UPDATE [dbo].[Orders] SET [CardinalAuthenticateResult] = [CardinalAuthenticateResult];
GO
UPDATE [dbo].[Orders] SET [CardinalGatewayParms] = [CardinalGatewayParms];
GO
UPDATE [dbo].[Orders] SET [CardinalLookupResult] = [CardinalLookupResult];
GO
UPDATE [dbo].[Orders] SET [CarrierReportedRate] = [CarrierReportedRate];
GO
UPDATE [dbo].[Orders] SET [CarrierReportedWeight] = [CarrierReportedWeight];
GO
UPDATE [dbo].[Orders] SET [CouponDescription] = [CouponDescription];
GO
UPDATE [dbo].[Orders] SET [CustomerServiceNotes] = [CustomerServiceNotes];
GO
UPDATE [dbo].[Orders] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Orders] SET [FinalizationData] = [FinalizationData];
GO
UPDATE [dbo].[Orders] SET [MaxMindDetails] = [MaxMindDetails];
GO
UPDATE [dbo].[Orders] SET [Notes] = [Notes];
GO
UPDATE [dbo].[Orders] SET [OrderNotes] = [OrderNotes];
GO
UPDATE [dbo].[Orders] SET [OrderOptions] = [OrderOptions];
GO
UPDATE [dbo].[Orders] SET [RTShipRequest] = [RTShipRequest];
GO
UPDATE [dbo].[Orders] SET [RTShipResponse] = [RTShipResponse];
GO
UPDATE [dbo].[Orders] SET [ReceiptHtml] = [ReceiptHtml];
GO
UPDATE [dbo].[Orders] SET [Referrer] = [Referrer];
GO
UPDATE [dbo].[Orders] SET [RefundReason] = [RefundReason];
GO
UPDATE [dbo].[Orders] SET [RefundTXCommand] = [RefundTXCommand];
GO
UPDATE [dbo].[Orders] SET [RefundTXResult] = [RefundTXResult];
GO
UPDATE [dbo].[Orders] SET [ShippingMethod] = [ShippingMethod];
GO
UPDATE [dbo].[Orders] SET [TrackingURL] = [TrackingURL];
GO
UPDATE [dbo].[Orders] SET [TransactionCommand] = [TransactionCommand];
GO
UPDATE [dbo].[Orders] SET [VATRegistrationID] = [VATRegistrationID];
GO
UPDATE [dbo].[Orders] SET [VoidTXCommand] = [VoidTXCommand];
GO
UPDATE [dbo].[Orders] SET [VoidTXResult] = [VoidTXResult];
GO
UPDATE [dbo].[Orders_KitCart] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Orders_KitCart] SET [TextOption] = [TextOption];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [ColorOptionPrompt] = [ColorOptionPrompt];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [CustomerEntersPricePrompt] = [CustomerEntersPricePrompt];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [DownloadCategory] = [DownloadCategory];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [DownloadLocation] = [DownloadLocation];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [Notes] = [Notes];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [OrderedProductName] = [OrderedProductName];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [OrderedProductVariantName] = [OrderedProductVariantName];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [ShippingDetail] = [ShippingDetail];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [ShippingMethod] = [ShippingMethod];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [SizeOptionPrompt] = [SizeOptionPrompt];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [TextOptionPrompt] = [TextOptionPrompt];
GO
UPDATE [dbo].[Orders_ShoppingCart] SET [TextOption] = [TextOption];
GO
UPDATE [dbo].[ProductVariant] SET [ColorSKUModifiers] = [ColorSKUModifiers];
GO
UPDATE [dbo].[ProductVariant] SET [Colors] = [Colors];
GO
UPDATE [dbo].[ProductVariant] SET [CustomerEntersPricePrompt] = [CustomerEntersPricePrompt];
GO
UPDATE [dbo].[ProductVariant] SET [Description] = [Description];
GO
UPDATE [dbo].[ProductVariant] SET [DownloadLocation] = [DownloadLocation];
GO
UPDATE [dbo].[ProductVariant] SET [ExtensionData2] = [ExtensionData2];
GO
UPDATE [dbo].[ProductVariant] SET [ExtensionData3] = [ExtensionData3];
GO
UPDATE [dbo].[ProductVariant] SET [ExtensionData4] = [ExtensionData4];
GO
UPDATE [dbo].[ProductVariant] SET [ExtensionData5] = [ExtensionData5];
GO
UPDATE [dbo].[ProductVariant] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[ProductVariant] SET [FroogleDescription] = [FroogleDescription];
GO
UPDATE [dbo].[ProductVariant] SET [ImageFilenameOverride] = [ImageFilenameOverride];
GO
UPDATE [dbo].[ProductVariant] SET [Notes] = [Notes];
GO
UPDATE [dbo].[ProductVariant] SET [SEAltText] = [SEAltText];
GO
UPDATE [dbo].[ProductVariant] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[ProductVariant] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[ProductVariant] SET [SizeSKUModifiers] = [SizeSKUModifiers];
GO
UPDATE [dbo].[ProductVariant] SET [Sizes] = [Sizes];
GO
UPDATE [dbo].[Product] SET [ColorOptionPrompt] = [ColorOptionPrompt];
GO
UPDATE [dbo].[Product] SET [Description] = [Description];
GO
UPDATE [dbo].[Product] SET [ExtensionData2] = [ExtensionData2];
GO
UPDATE [dbo].[Product] SET [ExtensionData3] = [ExtensionData3];
GO
UPDATE [dbo].[Product] SET [ExtensionData4] = [ExtensionData4];
GO
UPDATE [dbo].[Product] SET [ExtensionData5] = [ExtensionData5];
GO
UPDATE [dbo].[Product] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Product] SET [FroogleDescription] = [FroogleDescription];
GO
UPDATE [dbo].[Product] SET [ImageFilenameOverride] = [ImageFilenameOverride];
GO
UPDATE [dbo].[Product] SET [MiscText] = [MiscText];
GO
UPDATE [dbo].[Product] SET [Notes] = [Notes];
GO
UPDATE [dbo].[Product] SET [RelatedDocuments] = [RelatedDocuments];
GO
UPDATE [dbo].[Product] SET [RelatedProducts] = [RelatedProducts];
GO
UPDATE [dbo].[Product] SET [RequiresProducts] = [RequiresProducts];
GO
UPDATE [dbo].[Product] SET [SEAltText] = [SEAltText];
GO
UPDATE [dbo].[Product] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[Product] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[Product] SET [SETitle] = [SETitle];
GO
UPDATE [dbo].[Product] SET [SizeOptionPrompt] = [SizeOptionPrompt];
GO
UPDATE [dbo].[Product] SET [Summary] = [Summary];
GO
UPDATE [dbo].[Product] SET [SwatchImageMap] = [SwatchImageMap];
GO
UPDATE [dbo].[Product] SET [TextOptionPrompt] = [TextOptionPrompt];
GO
UPDATE [dbo].[Product] SET [UpsellProducts] = [UpsellProducts];
GO
UPDATE [dbo].[QuantityDiscount] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Rating] SET [Comments] = [Comments];
GO
UPDATE [dbo].[SalesPrompt] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Section] SET [Description] = [Description];
GO
UPDATE [dbo].[Section] SET [DisplayPrefix] = [DisplayPrefix];
GO
UPDATE [dbo].[Section] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Section] SET [ImageFilenameOverride] = [ImageFilenameOverride];
GO
UPDATE [dbo].[Section] SET [RelatedDocuments] = [RelatedDocuments];
GO
UPDATE [dbo].[Section] SET [SEAltText] = [SEAltText];
GO
UPDATE [dbo].[Section] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[Section] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[Section] SET [SETitle] = [SETitle];
GO
UPDATE [dbo].[Section] SET [Summary] = [Summary];
GO
UPDATE [dbo].[SecurityLog] SET [Description] = [Description];
GO
UPDATE [dbo].[ShippingMethod] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[ShippingZone] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[ShippingZone] SET [ZipCodes] = [ZipCodes];
GO
UPDATE [dbo].[ShoppingCart] SET [DownloadLocation] = [DownloadLocation];
GO
UPDATE [dbo].[ShoppingCart] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[ShoppingCart] SET [Notes] = [Notes];
GO
UPDATE [dbo].[ShoppingCart] SET [ShippingMethod] = [ShippingMethod];
GO
UPDATE [dbo].[ShoppingCart] SET [TextOption] = [TextOption];
GO
UPDATE [dbo].[State] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Store] SET [Description] = [Description];
GO
UPDATE [dbo].[Store] SET [Summary] = [Summary];
GO
UPDATE [dbo].[Topic] SET [Description] = [Description];
GO
UPDATE [dbo].[Topic] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Topic] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[Topic] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[Topic] SET [SETitle] = [SETitle];
GO
UPDATE [dbo].[Topic] SET [Title] = [Title];
GO
UPDATE [dbo].[Vector] SET [Description] = [Description];
GO
UPDATE [dbo].[Vector] SET [ExtensionData] = [ExtensionData];
GO
UPDATE [dbo].[Vector] SET [ImageFilenameOverride] = [ImageFilenameOverride];
GO
UPDATE [dbo].[Vector] SET [Notes] = [Notes];
GO
UPDATE [dbo].[Vector] SET [SEAltText] = [SEAltText];
GO
UPDATE [dbo].[Vector] SET [SEDescription] = [SEDescription];
GO
UPDATE [dbo].[Vector] SET [SEKeywords] = [SEKeywords];
GO
UPDATE [dbo].[Vector] SET [SETitle] = [SETitle];
GO
UPDATE [dbo].[Vector] SET [Summary] = [Summary];
GO

--Refresh all the views that looked at ntext columns
EXECUTE sp_refreshview '[dbo].[ObjectView ]';
EXECUTE sp_refreshview '[dbo].[MappedObjects]';
EXECUTE sp_refreshview '[dbo].[StoreMappingView]';
EXECUTE sp_refreshview '[dbo].[EntityMaster]';
EXECUTE sp_refreshview '[dbo].[ProductEntity]';
EXECUTE sp_refreshview '[dbo].[ShippingMethodStoreSummaryView]';
GO
