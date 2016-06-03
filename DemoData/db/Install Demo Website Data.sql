-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit our homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT.
-- ------------------------------------------------------------------------------------------

-- ------------------------------------------------------------------------------------------
--To clear all 
-- ------------------------------------------------------------------------------------------

delete from Product
go
delete from ProductCategory
go
delete from ProductSection
go
delete from ProductVariant
go
delete from ProductManufacturer
go
delete from Category
go
delete from Section
go
delete from Manufacturer
go
delete from ProductView
go
delete from KitGroup
go
delete from KitItem
go
delete from Genre
go
delete from Vector
go
delete from ProductGenre
go
delete from ProductVector
go
delete from Distributor
go
delete from ProductDistributor
go
delete from Affiliate
go
delete from ProductAffiliate
go
delete from LocaleSetting
go
delete from Currency
go
delete from ShippingMethod
go
delete from ShippingWeightByZone
go
delete from ShippingZone
go
delete from Customer where customerid != 58639
go
delete from Address where AddressID  != 1
go
DELETE FROM ORDEROPTION
go
DELETE FROM Orders WHERE OrderNumber IN (100137,100139,100140,100141,100156)
go
DELETE FROM Orders_ShoppingCart WHERE OrderNumber IN (100137,100139,100140,100141,100156)
go
DELETE FROM OrderNumbers WHERE OrderNumber IN (100137,100139,100140,100141,100156)
GO


--_Category
set IDENTITY_INSERT [dbo].Category ON;
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(1,'Video Games','video-games',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(2,'Electronic Toys','electronic-toys',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(3,'Riding Toys','riding-toys',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(4,'Major Appliances','major-appliances',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(6,'Kids Jewelry','kids-jewelry',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(7,'Bikes','bikes',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(9,'Scooters','scooters',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(10,'Desktops','desktops',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(11,'Basic Bedding','basic-bedding',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(12,'Printers & Fax','printers-fax',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(13,'Rings','Rings',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(14,'TableTop','tabletop',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(15,'Living Room Furniture','living-room-furniture',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(16,'Womens Apparel','womens-apparel',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(17,'Vitamins','vitamins',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(18,'Cooling & Heating','cooling-heating',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(19,'Gift Cards','gift-cards',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(20,'Televisions','television',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(21,'Disc Player','disc-player',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(22,'Hard drives/Storage','storage-device',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(23,'Lighting','lighting',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(24,'Movies','movies',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(25,'Butcher Shop','butcher-shop',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(27,'Wines & Wine Accesories','wines-accesories',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(28,'Cheese','cheese',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(29,'Wine Storage & Bars','wine-storage-bars',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(31,'Body & Skin Care','body-skin-care',1,0,0,0);
INSERT [dbo].Category(CategoryID,Name,SEName,Published,AllowSectionFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(32,'MP3','music',1,0,0,0);
set IDENTITY_INSERT [dbo].Category OFF;
GO

--_Section
set IDENTITY_INSERT [dbo].Section ON;
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(1,'Electronics','electronics',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(2,'Toys','toys',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(3,'Appliances','appliances',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(4,'Jewelry & Accessories','jewelry-accessories',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(5,'Sports & Recreations','sports-recreations',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(6,'Computers','computers',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(7,'Bed & Bath','bed-bath',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(8,'Housewares','housewares',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(9,'Furnitures','furnitures',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(10,'Fashion','fashion',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(11,'Health','health',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(12,'Gifts & Tickets','gifts-tickets',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(13,'Books/cds/movies/mp3','books-cds-movies-mp3',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(14,'Foods & Wine','foods-wine',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(15,'Decors','decors',1,0,0,0);
INSERT [dbo].Section(SectionID,Name,SEName,Published,AllowCategoryFiltering,AllowManufacturerFiltering,AllowProductTypeFiltering) values(16,'Beauty','Beauty',1,0,0,0);
set IDENTITY_INSERT [dbo].Section OFF;
GO

--_Manufacturer
set IDENTITY_INSERT [dbo].Manufacturer ON;
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(1,'Sony','Sony');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(2,'Nintendo','Nintendo');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(3,'Team Up','Team-Up');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(4,'Megatech','Megatech');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(5,'Kettler','Kettler');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(6,'Caterpillar','Caterpillar');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(7,'Step2','Step2');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(8,'Microsoft','Microsoft');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(9,'Whirlpool','Whirlpool');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(10,'Razor','Razor');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(11,'Micro','Micro');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(12,'HP','HP');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(13,'Dell','Dell');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(14,'Pacific Coast','Pacific-Coast');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(15,'Joseph Abboud','Joseph-Abboud');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(16,'Lexmark','Lexmark');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(17,'Brother','Brother');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(18,'Reed & Barton','Red-Barton');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(19,'Petite','Petite');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(20,'Comfort Research','Comfort-Research');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(21,'Barbour','Barbour');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(22,'Centrum','Centrum');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(23,'Nature Made','Nature-Made');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(24,'One A Day','OneADay');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(25,'Providence','Providence');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(26,'Simcoe','Simcoe');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(27,'Designer Edge','Designer-Edge');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(28,'Lasko','Lasko');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(29,'Presto','Presto');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(30,'Bionaire','Bionaire');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(31,'Seagate','Seagate');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(32,'Wolverine','Wolverine');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(33,'WildCatch','WildCatch');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(34,'Niman Ranch','Niman-Ranch');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(35,'Wise','Wise');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(36,'Fusebox','Fusebox');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(37,'Montes Folly','Montes-Folly');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(38,'Col Solare','Col-Solare');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(39,'Poderi Luigi Einaudu Barolo','Poderi-Barolo');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(40,'FloraSprings','FloraSprings');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(41,'d’Arenberg','Arenberg');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(42,'Marcarini','Marcarini');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(43,'Giordano','Giordano');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(44,'Bodegas Caro','Bodegas-Caro');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(45,'Kirkland Signature','Kirkland-Signature');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(46,'Martin Ray','Martin-Ray');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(47,'Rogars State','Rogars-State');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(48,'Quest','Quest');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(49,'Zippity Rabit','Zippity-Rabit');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(50,'Sisley','Sisley');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(51,'Strivectin','Strivectin');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(52,'Skin Medica','Skin-Medica');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(53,'Prevage','Prevage');
INSERT [dbo].Manufacturer(ManufacturerID,Name,SEName) values(54,'Shiseido','Shiseido');
set IDENTITY_INSERT [dbo].Manufacturer OFF;
GO

--_Genre
set IDENTITY_INSERT [dbo].Genre ON;
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (1,'Sony Playstation', 1,1);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (2,'Nintendo DS', 1,2);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (3,'Remote Control Toys', 1,3);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (4,'Wagons & Push Riding Toys', 1,4);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (5,'Microsoft Xbox', 1,5);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (6,'Refrigerators', 1,6);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (7,'Microwaves', 1,7);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (8,'Kids Necklaces', 1,8);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (9,'Kids Earings', 1,9);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (10,'Kids Rings', 1,10);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (11,'Kids Bikes', 1,11);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (12,'Adult Bikes', 1,12);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (13,'Kick Scooters', 1,13);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (14,'Desktop Home', 1,14);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (15,'Comforters', 1,15);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (16,'Gemstone Rings', 1,16);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (17,'Flatware', 1,17);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (18,'Sofas', 1,18);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (19,'Bean Bags & Casual Furniture', 1,19);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (20,'Drinkware', 1,20);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (21,'Sony Bravia', 1,21);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (22,'Sony Blu-Ray Player', 1,22);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (23,'Sony DVD Player', 1,23);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (24,'Portable Storage Drives', 1,24);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (25,'Floor Lamp', 1,25);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (26,'Seafoods', 1,26);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (27,'Lamb, Poultry & Sausage', 1,27);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (28,'Red Wine', 1,28);
INSERT INTO [dbo].Genre(GenreID,Name,Published,DisplayOrder) values (29,'Rap Music', 1,29);
set IDENTITY_INSERT [dbo].Genre OFF;
GO

--_Vector
set IDENTITY_INSERT [dbo].Vector ON;
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (1,'Mountain Bike',1,1)
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (2,'Kiddie Bike',1,2)
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (3,'BMX',1,3)
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (4,'Salmon Fish ',1,4)
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (5,'Lamb Meat',1,5)
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (6,'Chicken Meat',1,6)
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (7,'LCD TV',1,7)
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (8,'Jacket',1,8)
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (9,'Coat',1,9)
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (10,'Powder Foundation',1,10)
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (11,'Night Cream',1,11)
INSERT [dbo].Vector(VectorID,Name,Published,DisplayOrder) values (12,'Eye and Lip Contour',1,12)
set IDENTITY_INSERT [dbo].Vector OFF;
GO

--_Distributor
set IDENTITY_INSERT [dbo].Distributor ON;
INSERT INTO [dbo].Distributor (DistributorID,Name,Published) VALUES (1,'Acme Distributing',1);
INSERT INTO [dbo].Distributor (DistributorID,Name,Published) VALUES (2,'Foobar Inc.',1);
set IDENTITY_INSERT [dbo].Distributor OFF;
GO

--_Affiliate
set IDENTITY_INSERT [dbo].Affiliate ON;
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(1,'Sony','Sony Computer Entertainment Inc.',1);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(2,'Nintendo','Nintendo Company Ltd',2);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(3,'Megatech','Megatech International',3);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(4,'Kettler','Kettler',4);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(5,'CAT','Caterpillar',5);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(6,'Step2','Step2 Company',6);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(7,'Microsoft','Microsoft Corporation',7);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(8,'Whirlpool','Whirlpool Corporation',8);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(9,'Razor','Razor USA LLC',9);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(10,'Micro','Micro',10);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(11,'HP','Hewlett-Packard Development Company',11);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(12,'Dell','Dell Company',12);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(13,'Joseph Abboud','JA Apparel Corporation',13);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(14,'Lexmark','Lexmark International',14);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(15,'Reed & Barton','Reed & Barton Company',15);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(16,'Barbour','J. Barbour & Sons Ltd.',16);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(17,'Centrum','Wyeth',17);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(18,'One A Day','Bayer Healthcare',18);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(19,'Nature Made','Pharmavite',19);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(20,'Lasco','Lasco Bathware',20);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(21,'Seagate','Seagate Technology',21);
INSERT [dbo].Affiliate(AffiliateID,Name,Company,DisplayOrder) values(22,'Kirkland Signature','Costco',21);
set IDENTITY_INSERT [dbo].Affiliate OFF;
go

--_Product
set IDENTITY_INSERT [dbo].Product ON;
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(1, 'Playstation 3','As DVD playback made the PlayStation 2 more than just a game machine, hefty multi-media features make the Sony PlayStation 3 an even more versatile home entertainment machine. Features such as video chat, Internet access, digital photo viewing, and digital audio and video will likely make it the central component of your media set-up. Still, it is first and foremost a game console--a powerful one at that.',1,1,'01-0001',1)	
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(2, 'PlaysStation Portable(PSP)','The PSP is primarily a game console (PSP games come in UMD, or Universal Media Disc, format), but it can also play UMD-format movies. Using a memory stick, the PSP can play music and video files, and display picture files such as photos. This portable console can also connect to the internet via a web browser (not incuded in early firmware releases) and built-in wi-fi.',1,1,'01-0002',1)	
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(3, 'Nintendo DS Lite','The Nintendo DS Lite is a high-powered handheld video game system in a sleek folding design loaded with features for a unique gaming experience. The color screens are now even brighter and the lower touchscreen provides a totally new way of playing and controlling games. Use the built-in wireless mode to share games, chat or even play multiplayer games on-line via Nintendo Wi-Fi Connection. Play impressive 3-D rendered Nintendo DS games and play all your favorite Game Boy Advance games in single player mode. Nintendo DS Lite comes with a variety of distinctive changes that set it apart from the original: it’s less than two-thirds the size of the original Nintendo DS and more than 20% lighter; its 2 bright screens can be adjusted to 4 levels to adapt to different lighting conditions and to extend battery power; the microphone sits in the center of the unit, and the LED lights are clearly visible whether the unit is open or closed; the stylus is 1 cm. longer and 1 mm. thicker than the stylus of the original, and slides into a side storage slot; the Start and Select buttons were repositioned for easier access and a removable cover keeps the Game Boy Advance cartridge slot clear from dust and debris when it’s not in use.',1,1,'01-0003',1)	
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(4, 'Team Up Nascar RC Cars','This officially licensed 1:18 scale NASCAR remote control car is designed with detailed authentic graphics and has over a 50-yard signal range. It is the perfect gift for any NASCAR fan!',1,1,'01-0004',1)	
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(5, 'Megatech MegaBotz RC Artificial Intelligence Battle Vehicles','The Megatech MegaBotz will chase one another and battle until one vehicle loses all 10 of its "life" lights and the game is over. Choose any of the play modes: you against a friend, you against the other MegaBotz set on artificial intelligence mode or both MegaBotz set on artificial intelligence mode to battle each other while you watch.',1,1,'01-0005',1)	
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(6, 'Megatech Micro Fly RC Featherweight Helicopter','The total control Micro Fly weighs a mere 2oz. Although its light, you have complete up, down, left, and right RC control with selectable forward, hover, or backward flight mode. The Micro Fly comes totally built, brightly painted, and ready to fly. With its counter-rotating main rotor blades, and weight gyroscopic fly-bar, its so stable that youll be hovering around the house and landing on the coffee table in no time.',1,1,'01-0006',1)	
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(7, 'Megatech AirStrike Electric Powered Free Flight Airplane','The AirStrike Free Flight Airplane is specifically designed for children showing an interest in aviation. With a clean design and the included easy to understand instruction booklet, the basics of aerodynamics can be quickly understood and applied. The unique break-away wing design allows the airplane to be flown again and again even after a crash landing.',1,1,'01-0007',1)	
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(8, 'Avion Remote Control Electric 5-Inch Wingspan Biplane','Megatechs Avion will convert your living room into an indoor aerodrome. Boasting a tight 3-foot turning radius, Avion is a high performance, precision controlled, highly maneuverable, ready-to-fly, easy to use, indoor aerobat.',1,1,'01-0008',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(9, 'Kettler Classic Flyer Air Tire Wagon','The Kettler Classic Flyer Air Tire Wagon features a wagon body of natural wood. Use it with or without the 4 removable wooden slats. Durable air tires for a smooth and quiet ride and ergonomically designed handle for easy pulling make every walk a pleasure.',1,1,'01-0009',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(10, 'Kettler Classic Flyer First Trike','The Kettler Classic Flyer First Trike is a natural hardwood foot to floor trike. Limited turn steering and a wide front wheelbase ensure that your child will not tip over while learning to ride this classic mobile machine.',1,1,'01-0010',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(11, 'Caterpillar CAT Digger Ride-On Toy','Wow! The Caterpillar CAT Digger Ride-On Toy makes it easy to imagine youre working right on the construction site. Real working dual action levers and a large resin digging shovel make pretending fun. The digger rotates 360° and sits on 4 rolling wheels to move forward and backwards.',1,1,'01-0011',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(12, 'Step2 Whisper Ride Buggy','Ease your childs transition out of their stroller with the new Whisper Ride Buggy. Extra-Large Silent Ride Tires, a cupholder, and the pretend steering wheel with electronic horn keep your child happy, and the under-hood storage compartment, seat belt, and removable handle make trips easier on you! (2 AAA batteries not included.)',1,1,'01-0012',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(13, 'Step2 Up and Down Roller Coaster','This award-winning roller coaster is a Step2 favorite. With non-slip steps and snap-together construction, amusement-park fun lives in your play room or back yard.',1,1,'01-0013',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(14, 'Microsoft Xbox 360','The Xbox 360 Premium System is the way to begin experiencing the ultimate in next-generation gaming. Now improved with an HDMI port, your games and video entertainment will look better than ever when connected to an HDTV. Amazing Digital entertainment and video gaming experiences, unprecedented in home console entertainment, are waiting for you -- all you have to do is turn on the controller to this incredible device and an unsurpassed level of adventure and excitement can be yours. Xbox Live Marketplace - Download the latest game demos, arcade games, television, movies, and more straight to your Xbox 360 console via any Broadband Internet service Games - Games look, feel, and Sound realistic with 480p/720p/1080i HD Output via HDMI or Component cable, 16 - 9 widescreen aspect ratio, anti-aliasing, and multi-channel Surround sound support Digital Entertainment - Play DVD movies right out of the box, play HD DVDs with the Optional HD-DVD Player (sold separately), rip music to the 360 Hard Drive, connect your Digital Camera and share your digital pictures with friends, or connect your Xbox 360 to a Windows XP or Windows Vista Media Center PC and Stream TV, Music, Movies, and Pictures to your Xbox 360 console over your network! NOTE - Some features may require optional Xbox Live GOLD service; One month of complementary Xbox Live GOLD service is included',1,1,'01-0014',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(15, 'Megatech Party Blimp RC 4-Channel Electric Ready-To-Fly Blimp','Where no party has gone before! Add excitement to any indoor event. If youre looking for that special item which will set your party apart from any other - youve just found it. Control a 3 long helium blimp thats orbiting the festivities with a personalized message. Simply fill with helium available at florists and party stores.',1,1,'01-0015',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(16, 'Megatech Radio Rodent RC Electric Mouse','Radio Rodent is on the loose! Terrorize your parents with eyes that light-up red and a tail that will flip your mouse upright. Experience wild and crazy radio control fun with this remote control mouse.',1,1,'01-0016',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(17, 'Megatech Firefly RC 2-Channel Electric Ready-To-Fly Airplane','Designed with the first time flyer in mind! The FireFly is the complete package. It is a 100% ready-to-fly airplane and it comes in its own carrying case. Fly indoors and out with stability and ease.',1,1,'01-0017',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(18, 'Whirlpool French Door Satina Refrigerator','Satina is a painted monochromatic finish that has a stainless look. # Capacity: 24.9 cu. ft. (18.3 cu. ft. refrigerator / 6.6 cu. ft. freezer). ENERGY STAR® Qualified. Contour smooth door. External ice & water dispenser. Water filter & indicator. 4 spill-proof adjustable glass shelves (1 mini & 3 slide-out). Hanging wire shelf. 2 humidity-controlled crispers. 5 Clear Door Bins (2 Gallon-Size) with Grip Pads. 2 opaque condiment holders (in door). Factory installed automatic ice maker. 2 wire freezer baskets (rack & pinion system). Blue lower freezer basket tray',1,1,'01-0018',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(19, 'Whirlpool Side by Side Stainless Steel Refrigerator','Capacity: 25.3 cu. ft. (15.4 cu. ft Refrigerator / 9.9 cu.ft. Freezer). 2008 ENERGY STAR® Qualified. Traditional styling. In-door-ice® ice dispensing system. Standard temperature management. Pur® 6-month water filtration. Water filter indicator light. Backlit slide temperature controls. Sylvania daylight® interior lighting. Adaptive defrost system (ads). Adjustable gallon door bin. Gallon door bins: 3. Spillguard™ glass shelves. Clear humidity-controlled crisper. Clear temperature-controlled meat pan. Clear snack pan. Full-width humidity-controlled crisper. 4 door shelves. 4 shelves, 1 bin freezer storage. 3 fresh food shelves. 2 spillproof, 1 fixed pan shelf shelving ',1,1,'01-0019',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(20, 'Whirlpool Bottom Mount Stainless Steel Refrigerator','Capacity: 21.9 cu. ft.(15.6 cu. ft. refrigerator / 6.3 cu. ft. freezer). ENERGY STAR® Qualified. Smooth door. Interior water dispenser. Factory installed automatic ice maker. Digital temperature control. 3 slide-out spill-proof shelves & 1 standard spill-proof shelf. 2 humidity-controlled crispers. 1 dairy compartment. 1 clear full-width framed pantry. 1 wire can rack. 4 clear door bins with 2 blue mats. 1 fixed opaque full-width gallon door bin. 2 slide-out freezer baskets. Reversible door',1,1,'01-0020',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(21, 'Whirlpool MaxWave','Maxwave™ Cooking System: The unique system releases microwave energy from multiple points inside the oven cavity. Uniform cooking ensures your food is cooked throughout, without cold centers or overdone edges. Precision and speed means you can enjoy a meal in just a few minutes.',1,1,'01-0021',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(22, 'Childs Sterling Silver Little Angel Heart 2-Tone Locket','This precious pendant is perfect for that sweet little one. Featuring a 2-tone sterling silver locket with a cherub angel on front, the whole package rests on a 13" sterling silver chain.',1,1,'01-0022',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(23, 'Gold-Filled Engraved Childrens Heart Locket','A sweet little hand-engraved heart with satin and polished finishes hangs delicately on a 13" chain.',1,1,'01-0023',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(24, 'Gold-Filled Childrens Cloisonne Cross','This sweet cloisonné cross is decorated with a pink flower and green leaves, perfect for your precious little girl.',1,1,'01-0024',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(25, 'Gold-Filled Childrens Polished Fancy Cross','Childs 14k yellow gold cross with fancy milled border and 13" chain. Perfect for those special gift giving events.',1,1,'01-0025',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(26, 'Sterling Silver Childrens Small Engraved Cross','This childrens engraved sterling silver cross on a 13" chain. Perfect for those special gift giving events.',1,1,'01-0026',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(27, 'Childs Sterling Silver Daddys Little Girl Pendant','Tell your little girl how special she is with this "Daddys Little Girl" pendant on a 13" chain.',1,1,'01-0027',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(28, 'Childs Sterling Silver Dolphin Post Earrings','These childrens sterling silver novelty dolphin post earrings have diamond cuts to add extra sparkle.',1,1,'01-0028',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(29, 'Sterling Silver Youth/Tween Ring with Crystal Spinel - Size 4','This adorable sterling silver ring with crystal spinel in elegant oval setting is perfect for a youth or "tween" ring size 4.',1,1,'01-0029',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(30, 'Childs 14K Gold 3mm White Cubic Zirconia Post Earrings','These childrens 14k yellow gold earrings have 3mm white cubic zirconias. There perfect for a younger person with push-on, screw-off clutches.',1,1,'01-0030',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(31, '16-Inch Kettler Pulse Spider Boys Bike','The Pulse Spider is one cool bike that is sure to spark fond "first bike" memories that will be treasured forever. The Spider has sporty colors and all the cool features that boys love - and best of all, parents will have peace of mind because they know the bike comes fully equipped with safety features. Removable training wheels, padded handlebar, rear coaster brake, hand brake, durable BMX-inspired frame and an enclosed chain are only the beginning to getting the kids off to a safe start. Pneumatic tires, front and rear reflectors and metal fenders make this bike a very cool ride. For a custom fit that ensures proper posture, the padded seat and handlebar are adjustable. Nobodys tougher on bikes than boys, but the Spider is dependable, built to last, and easy to ride.',1,1,'01-0031',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(32, 'Kettler Kettrike Jumbo Trike','Three ways to have fun in one trike! The Kettler Kettrike Jumbo Trike allows your child to ride on his own, get a push with the included push bar, or take a tandem bike trip with other Kettrike riders. The trike features a handbrake so your tike can stop on a dime, and a 3-in-1 auto freewheel allows your little one to pedal and brake, coast along when being pushed, or team up for tandem riding. Best of all, the Jumbo Trike features a tipping rear bucket to transport favorite items!',1,1,'01-0032',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(33, 'Kettler Pulse Girls Bike','',1,1,'01-0033',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(34, 'Kettler Classic Flyer Low Rider Trike','The Kettler Classic Flyer Low Rider Trike with stroller pushbar has a patented 3-in-1 auto freewheel, limited turn radius steering and a low center of gravity to keep your child safe while playing. Its easy to pack up and move on with the stroller push bar with removable backpack for all of the walks necessities.',1,1,'01-0034',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(35, 'Kettler Kiddio Supreme Stroller Trike','The Kettler Kiddio Supreme Stroller Trike works for kids from 1-5 years old as a stroller or a pushable ride-on toy. The supreme stroller has so many features for mom and dad, youll almost forget its built for your little one. A 5-point safety harness, height adjustable stroller push-bar, patented parental control steering lock and patented limited turn radius are excellent safety features, and adjustable headrest, UV protective canopy, and large rear storage tray are the perfect finishing touches.',1,1,'01-0035',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(36, '17-Inch Kettler Ibiza bike','The simplest bicycle to operate, the Ibiza is at the pinnacle of comfort bicycle design. The Shimano Nexus 7-speed hub does all the work, leaving you without any gear-shifts to adjust or tune. A stylish aluminum frame, Suntour suspension fork, luggage rack, fenders, suspension seat post, and deluxe saddle make the Ibiza ready to roll right out of the box.',1,1,'01-0036',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(37, '15-Inch Kettler Pulse Firenze Womens Bike - Periwinkle','The fastest bicycle to ride, the Firenze is also the most versatile Pulse bicycle, shifting effortlessly between workday commutes and fun weekend rides. A complete Shimano drive train promises miles of crisp efficient shifting. A joy to ride, the Firenzes premium features include a 45mm InSync EnRoute suspension fork with adjustable preloads, alloy platform pedals, deluxe saddle seat with elastomers, luggage rack, and resin fenders.',1,1,'01-0037',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(38, '17-Inch Kettler Pulse Tivoli Mens Bike - Metallic Gray/White','For the ultimate day trip, take the ultimate day tripper! The Kettler Tivoli has all the features a true comfort bicycle needs. The Shimano TX-70 rear derailleur has a comfort-minded design with Megarange gearing, a Mega pulley, aluminum linkage, and slant pantograph. What does that mean to you? Just relax and enjoy the journey secure in the knowledge that your Pulse bicycle has an efficient, crisp shifting Shimano drive train to deliver you to any destination.',1,1,'01-0038',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(39, 'Kettler Ibiza bike','The simplest bicycle to operate, the Ibiza is at the pinnacle of comfort bicycle design. The Shimano Nexus 7-speed hub does all the work, leaving you without any gear-shifts to adjust or tune. A stylish aluminum frame, Suntour suspension fork, luggage rack, fenders, suspension seat post and deluxe saddle make the Ibiza ready to roll right out of the box.',1,1,'01-0039',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published,XmlPackage) values(40, 'Razor Kick Scooter A3','Its everything you love about the A and A2 Razor Kick Scooters just, well, kicked up a bit. Complete with the popular springless suspension and patented rear fender brake, the A3 now rocks 4.912" wheels for increased stability and maneuverability. Thats over an inch larger than the wheels used on all previous Razor Kick Scooter models. Add that to its aircraft-grade aluminum t-tube and deck, and youve got a turn on a classic that youre definitely going to want to take. Its even easy to fold and carry with Razors patented folding system.',1,1,'01-0040',1,'product.simpleproduct.xml.config')
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(41, 'Razor Kick Scooter A2','The perfect piece for the serious scooter aficionado, the Kick Scooter A2, with a springless front wheel shock system, can really fly. Whether youre looking for a simple transport or a kickin stick for rocking your tail whip, the A2 has it. Designed with a patented folding mechanism.',1,1,'01-0041',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(42, 'Razor Kiddie Kick Scooter','Is your little kiddo ready to kick it? With a stable 3-wheel design, the Kiddie Kick helps young riders build coordination while having a blast. The original Razor scooter for first-time riders, it comes complete with an extra-wide, slip resistant polypropylene deck, as well as an adjustable handlebar and built-in spare wheel, so you can go from zero to fun in no time flat.',1,1,'01-0042',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(43, 'Micro Compact Kickboard','Three wheels = more street surfing fun than you can imagine. Building on the success of the Micros first product, the Compact Kickboard reaches new heights. Made of high quality aluminum, it features all modular construction - no welds and a sandblasted aluminum deck. It has 120mm front and rear high rebound cast urethane wheels with ABEC 5 precision bearings while the steering tube lets you ride the street with confidence.',1,1,'01-0043',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(44, 'Micro Monster Bullet Scooter','Get the thrills and adrenaline rush you crave with the Micro Monster Bullet. The Monster combines the established strengths of Micros scooters with a totally new type of drive concept - an integrated carving tool is the back wheel for freaky fun and wild times. The scooter heralds a new era with its wider stance, and 120x64mm front and 80x64mm rear urethane wheels with ABEC 5 precision bearings. It features an aluminum deck, adjustable height handlebar with foam padded grips, and patented safe folding system.',1,1,'01-0044',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(45, 'Micro Monster Kickboard','Combine a scooter with a kickboard and what do you get? The new Micro Monster Kickboard - the best of both the scooter and kickboard. Surf the street with confidence with the easily adjustable steering column. The wood and fiberglass flex deck offers added support while the wide 120mm front and 100mm rear urethane wheels with ABEC 5 precision bears ensures better road coverage and speed.',1,1,'01-0045',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(46, 'Micro Sprite Scooter','Perfect for the beginner, the Micro Sprite Scooter is lightweight and can be taken anywhere. Made of high quality aluminum, it features all modular construction - no welds. It has 120mm front and 100mm rear high rebound cast urethane wheels with ABEC 5 precision bearings while the adjustable height handlebar comes complete with removable foam padded grips. Patented safe folding mechanism makes storing your scooter a snap.',1,1,'01-0046',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(47, 'Micro White Scooter','Sleek and stylish, the Micro White Scooter is built for cruising. The specially designed deck height helps you keep your balance - something youll appreciate no matter where you are going. Made of high quality aluminum, it features all modular construction - no welds. It has 200mm front high rebound cast urethane wheels with ABEC 5 precision bearings, kickstand, and an extra long handlebar that adjusts for kids and adults. Patented safe folding mechanism makes storing your scooter a snap.',1,1,'01-0007',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published,IsAKit) values(48, 'HP Pavilion Slimline s3600t PC','Full PC functionality in one-third the size - The HP Pavilion Slimline PC, featuring Intel(R) desktop processors.',2,1,'01-00048',1,1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(49, 'Dell XPS 630','Intel® Core™ 2 Processor Q6600 at 2.4 GHz Quad Core Technology. 640GB 7200RPM SATA Hard Drive. Dell E248WFP 24" Wide LCD. NVIDIA GeForce 9800 GT. Microsoft® Windows® Vista Home Premium 64-bit. Integrated 7.1 Channel Audio. Dell USB Keyboard. Dell 2-Button Optical Mouse. Integrated 10/100/1000 Ethernet.',2,1,'01-00049',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(50, '370TC Baffle Box Down Comforters Light Weight','Restore your body and spirit with this luxuriously soft White Goose Down comforter from Pacific Coast®. Choose from Light Warmth, Year Round Warmth or Extra Warmth to suit your personal sleeping preference. 600 fill power Hyperclean® white goose down means larger, fluffier clusters that provide exceptional insulation. True baffle box design and patented Comfort Lock® no-shift border allows the down inside to fully loft and prevents shifting. A soft 370-count 100% cotton Barrier weave™ cover feels wonderful and prevents the down from sneaking out.',1,1,'01-00050',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(51, 'Joseph Abboud 1000TC Down Comforter','A distinctive blend of beauty and warmth, the Joseph Abboud designed comforters and pillows offer a level of style and luxury beyond compare. Hypo-allergenic, featuring Joseph Abboud European White Goose Down, 650 fill power, and a luxurious 1000 thread count combed cotton sateen fabric. Joseph Abboud Down is certified Freshness Assured™ having undergone a 15 step cleaning process that guarantees freshness and purity for the life of the product. ',2,1,'01-00051',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(52, 'Lexmark X7350 All-in-One Printer','Great quality straight out of the box! The Lexmark All-in-One printers are ideal for all your printing, scanning, and copying needs. With features varying from built-in screens to the Accu-feed paper handling system, or the high-resolution printing and copying, the All-in-One series of Lexmark printers is perfect for whatever your needs demand. Complete with a connection port compatible with a wide variety of digital camera memory cards or the PictBridge digital camera interface, you can print directly without the use of a computer. Ease of use and function are what this line is known for.',1,1,'01-00052',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(53, 'Lexmark X5470 5-in-1 Photo Printer','For sheer power and number of features, the 5-in-1 Photo Printer by Lexmark is the way to go. Printing, copying, scanning, faxing, editing, and transferring files into various formats are all easily accessed in this multiple-use printer. With a long list of features, its functions seem to never end. Uses a wide variety of paper for almost any printing job around and compatible with a huge list of memory sticks and USB cords. Simple computer-free ease of use for everything from copying to editing photos. This is a top-of-the-line, multi-function printer, perfect for anyone serious about their technology.',1,1,'01-00053',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(54, 'Lexmark International C500N Color Laser Printer','The easy-to-use Lexmark C500n Color Laser Printer is for the small business owner or for the home office. Features impressive print quality to help you perform your day-to-day tasks with confidence and ease. With the C500n, small businesses or the home user can have the power of color to enhance documents.',1,1,'01-00054',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(55, 'Lexmark X2500 All-in-One Printer','The Lexmark X2500 All-in-One Printer with photo features combines fast speed - up to 22ppm black, 16ppm color - versatile functionality, and brilliant photo quality in one sleek design. Complete tasks effortlessly with 1-touch copy and a 36-bit flatbed scanner. Print borderless photos and create high quality photo projects with Lexmark Imaging Studio. The versatile X2500 all-in-one is great for your home and basic office needs.',1,1,'01-00055',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(56, 'Lexmark X4550 All-in-One Wireless Printer','Experience the convenience of wireless printing with the Lexmark X4550. Get high-quality prints from virtually anywhere in the home over a secure wireless network. With the X4550 wireless all-in-one, easily print, copy, scan and share memories. Print up to 26ppm black and 18ppm color. Copy photos, black and white, or color documents with the touch of a button. Scan on the 48-bit flatbed scanner. From the proof sheet, easily select and print borderless photos from your digital memory cards. Enjoy direct photo printing from memory cards, USB drives, or with PictBridge.',1,1,'01-00056',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(57, 'Lexmark X4850 All-in-One Wireless Printer','The Lexmark X4850 All-in-One Printer offers the convenience of wireless technology combined with efficient 2-sided printing and business class speeds of up to 30ppm black and 27ppm color. Produce high resolution scan documents and easily copy with a touch of a button. Robust photo performance allows viewing and editing of photos on the 2.4" LCD and direct photo printing from memory cards, USB flash drives or from PictBridge compatible cameras. Enjoy vibrant borderless photos in popular photos sizes. The Lexmark X4850 provides maximum performance for your home.',1,1,'01-00057',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(58, 'Lexmark X5070 4-in-1 Printer','The Lexmark X5070 4-in-1 Printer provides business class speeds of up to 24ppm black and 17ppm color! Produce high quality scan documents and copy with a touch of a button! Fax or copy multiple page documents using the up to 30-page automatic document feeder (ADF). Full fax capabilities include corded handset, automatic redial, speed dial, and caller ID. Print borderless photos directly from your PictBridge capable camera. All features combined bring office functionality to the home.',1,1,'01-00058',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(59, 'Brother IntelliFax-4100e Business-Class Laser Fax','Get fast black and white laser copying at up to 15cpm with the Intellifax-4100e Business-Class Laser Fax. Multi-copy (up to 99), sort, reduction, and enlargement (50%-200%). Features group dialing (up to 6 groups), fax forwarding, paging, and remote fax retrieval. Has document memory back-up (up to 4 days), polling and delayed transmission (up to 50 timers), telephone handset, external answering machine interface, electronic cover page, and 64-shade gray scale.',1,1,'01-00059',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(60, 'Blue Sapphire & Diamond Ring 18kt White Gold','Simply stunning!  Impressive oval blue sapphire accented with round brilliant diamonds.  A ring that she will treasure forever.',1,1,'01-00060',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(61, 'Pink Spinel & Diamond Ring 18k White Gold','She will love this spinel & diamond ring.  An impressive 4.79 ct oval pink spinel surrounded by round and pentagonal diamonds.  Set in white gold for an unbelievable shine.',1,1,'01-00061',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(62, 'Oval Blue Sapphire & Diamond Ring','Blue sapphires & diamonds, the perfect combination.  Oval blue sapphires accompanied by oval diamonds.  Set in white gold for an eye-catching shine. ',1,1,'01-00062',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(63, 'Tanzanite & Diamond Ring 18kt White Gold','Simply Stunning!  Impressive cushion cut tanzanite accented with bullet diamonds.  Set in white gold for the perfect finish.',1,1,'01-00063',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(64, 'Oval Rhodolite Garnet & Diamond Ring 18kt White Gold','With gleaming white gold, this setting beautifully showcases a stunning, oval rhodolite garnet surrounded by round brilliant diamonds.  A gorgeous ring with exceptional color.',1,1,'01-00064',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(65, 'Peridot & Diamond Ring 18kt White Gold','A stunning oval peridot is accented by baguette diamonds set in white gold.  This unique ring has a detailed design that will light up the room.',1,1,'01-00065',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(66, 'Oval Orange Spessartite & Round Diamond Ring 18kt Two-Tone','From the garnet family comes this vivid orange spessartite.  The craftsmanship of this ring is just spectacular.  The center stone is surrounded by round diamonds as well as along the side of the band.  Set in 18kt two-tone gold for an unbelievable shine.',1,1,'01-00066',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(67, 'Orangy Brown Zircon & Diamond Ring 18kt White Gold','A very unique cushion cut orange zircon accompanied by round brilliant & baguette diamonds.  Zircon is an exceptionally brilliant stone, set in white gold for an unbelievable shine.',1,1,'01-00067',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(68, 'Reed & Barton Hammered Antique 52pcs Flatware Set','This versatile pattern incorporates the look and feel of the finest hand hammered metal and is the perfect complement to both casual and fine china.',1,1,'01-00068',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(69, 'Reed & Barton New Attitude 90pcs Flatware Set','The contemporary New Attitude 90-piece flatware set from Reed & Barton offers sleek, simplistic styling that will compliment most dinnerware. This large sized flatware is made of dishwasher safe stainless steel. ',1,1,'01-00069',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(70, 'Reed & Barton Uptown 60pcs Flatware Set','Graceful contemporary styling highlights this beautiful flatware pattern. The contoured handles are comfortable to hold and pleasing to the eye; a bold and sophisticated compliment to todays updated tableware styles. This durable pattern is suitable for both causal and formal dinnerware settings.',1,1,'01-00070',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(71, 'Reed & Barton Woodbury 110pcs Flatware Set','Woodbury makes an exceptional statement of true elegance and grace. This superb pattern features a subtle scroll border, accentuated by two fine points at the tip of the handle. Its graceful profile and uncomplicated design make this a must have for those who appreciate lifes finer things for the home.',1,1,'01-00071',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(72, 'Petite Microfiber Loveseat','This gorgeous loveseat from the Petite Collection boasts style, quality, and value. Plus, removable cushions and stain-resistant microfiber fabric make for easy cleaning. Pick up one today or join it with the matching chair, ottoman, and sofa (sold separately) to complete the set.',1,1,'01-00072',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(73, 'Petite Microfiber Sofa','This lovely peat sofa from the Petite Collection boasts style, quality, and value. Plus, removable cushions and stain-resistant microfiber fabric make for easy cleaning. Pick up one today or join it with the matching chair, ottoman, and loveseat (sold separately) to complete the set.',1,1,'01-00073',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(74, 'Comfort Research Kiddie Bean Bag Chair','The Kiddie Bean Bag Chair features an easy-to-clean vinyl cover and polystyrene fill with locking zippers for safety. Your kids will love these hip chairs that go great in any room. Fun, functional, and comfortable, this will be the favorite seat in the house! Available in a variety of colors.',1,1,'01-00074',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(75, 'Comfort Research Jumbo Bean Bag with Liner - Royal Blue Vinyl','Big fun for the whole family. The Jumbo Bean Bag comes with an inner liner for extra strength and ease of cleaning. When a regular bean bag just isnt enough, make it a jumbo so you can really be enveloped in comfort. Made with a vinyl cover and polystyrene fill, it has a locking zipper for your familys safety. Available in a variety of colors.',1,1,'01-00075',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(76, 'Reed & Barton Soho Spirit Sets','Reed & Barton Soho Spirits Set has classic design with simple, elegant lines. In your dining room or bar, it offers a nice way to store or serve liquor.',1,1,'01-00076',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(77, 'Reed & Barton Soho Bar Sets','The Soho bar set has a crisp, classic look that has made it one of the most popular patterns from Reed and Barton. Made of hand-blown lead crystal, its a smart accessory for your bar or dining room.',1,1,'01-00077',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(78, 'Barbour Fur Trim Womens Utility Jacket','Shell: 100% waxed cotton. Inner: 80% wool and 20% polyamide. Sleeve: 100% polyamide. Detachable fur collar 69% acrylic, 11% cotton, 10% polyester, 10% viscose. Multi-pocketed with 2 chest, 2 hand and 1inside Made in England.',1,1,'01-00078',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(79, 'Barbour Weather Worked Womens Utility Jacket','Shell: 62% cotton and 38% polyester leather look fabric. Inner: Body 100% cotton and sleeves 100% polyamideLiner: 60% polyurethane and 40% polyesterTrim: 100% cottonVelvet collar and trims. Multi pocketed with 2 chest, 2 hand and 1 inside. Waterproof breathable dropliner. Machine washable. Made in Indonesia.',1,1,'01-00079',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(80, 'Barbour Liddesdale Womens Coat','100% polyamide shell and lining. 100% polyester self-lined, diamond quilted with 100g polyester wadding. Corduroy collar. Four snap front. Zippered inside pocket. Machine wash. Made in Indonesia.',1,1,'01-00080',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(81, 'Centrum Performance','Helps Energize Your Body and Mind. Specially Formulated with Ginseng, Ginkgo, and Higher Levels of Five Essential B Vitamins',1,1,'01-00081',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(82, 'Centrum Silver MultiVitamin/MultiMineral Supplement','Specially Formulated Multivitamin/Multimineral supplement for Adults 50+',1,1,'01-00082',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(83, 'Centrum MultiVitamin/MultiMineral Supplement','Contains all nutrients with an RDI',1,1,'01-00083',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(84, 'Nature Made Vitamin B-12 1,000 mcg','Essential for a healthy nervous system, vital for red blood cell formation!',1,1,'01-00084',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(85, 'Nature Made Triple Omega','To be able to stay active, do the things you enjoy, spend quality time with your loved ones, friends and family, it is important to maintain your heart health and give your body the nutrients you need. Getting enough essential fatty acids in our diet is critical and you can do this through supplementation. Nature Made® Triple Omega delivers three essential omega fatty acids in a convenient easy to swallow softgel formulated for easy absorption. Consumption of Omega 3 fatty acids may reduce the risk of coronary heart disease and also help maintain triglyceride levels already in the normal range.',1,1,'01-00085',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(86, 'One A Day Mens 50+ Advantage','Compared to One A Day® Essential, and with nearly twice the Selenium in Centrum® Silver® for prostate health. To help maintain blood pressure levels already within the normal range',1,1,'01-00086',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(87, 'One A Day Womens 50+ Advantage','Compared to One A Day® Essential, and with more Calcium and Vitamin D than Centrum® Silver® for bone strength and breast health.',1,1,'01-00087',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(88, 'Nature Made Vitamin C Liquid Softgels','Nature Made® Vitamin C Liquid Softgels help boost a healthy immune system by harnessing the vitamins antioxidant properties to neutralize free radicals.† Vitamin C also plays a key role in collagen production and helps the body absorb iron and Vitamin E.† Nature Made® Vitamin C Liquid Softgel is formulated for easy absorption and is easy to swallow.',1,1,'01-00088',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(89, 'One A Day Mens Health Formula','With 2 times the amount of Lycopene in Centrum® or Centrum® Silver®',1,1,'01-00089',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(90, 'One A Day Womens Multivitamin/Multimineral','Promotes Bone Strength. Supports a Healthy Reproductive System.',1,1,'01-00090',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(91, 'Nature Made Vitamin E 400 IU','Vitamin E is a workhorse vitamin. With so much that it does: Promotes heart health; Water solubilized for better absorption; Powerful antioxidant†, who can argue that one doesnt need a daily dose of Vitamin E. It also helps boost your immunity and maintains red blood cells and muscle tissues such as cardiac and skeletal muscles†. Even better, take the Vitamin E made by Nature Made, specially Formulated for Easy Absorption from the number one manufacturer of Vitamin E and a brand you already know and trust. ',1,1,'01-00091',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(92, 'Nature Made Vitamin D 1000 IU','Try Nature Made Vitamin D 1000 IU. It  provides maximum strength for bone & joint protection†. We recommend that you take a Nature Made Calcium supplement with this product. Vitamin D 1000 IU is specially formulated for Easy Absorption and is made with D3, a more absorbable form of Vitamin D. Now get Vitamin D 1000 IU from Nature Made, a brand you already know and trust.',1,1,'01-00092',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(93, 'Electric Fireplace And Mantel Surround W/ Built In Wine Storage And Cooler','Experience the ultimate in electric fireplace mantel design, paired with wine cooler technology.  This beautiful oak finished mantel surround piece is designed to fit into any room decor from contemporary to traditional.  The wine and glass storage area on the left is for red wines best served at room temperature,  The wine storage cooler on the right keeps white wines chilled to your chosen temperature.',1,1,'01-00093',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(94, 'Barclay Mantel with 28" Electric Flame Firebox','This beautiful fireplace and Solid Wood & Veneer Mantel Surround in Aged Chestnut finish is the perfect addition to any home decor.  The realistic fireplace feature brings a feeling of warmth and ambiance to any room area.  Comfortable remote control to turn unit on or off.  This is great for heat output and flame control lets the unit operate with or without heat, and gives the look from dancing flames to dying embers.',1,1,'01-00094',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(95, 'Providence Chadwick Mantel with 28" Electric Flame Firebox','This beautiful fireplace and Solid Wood Mantel Surround in Tobacco finish is the perfect addition to any home decor.  The realistic fireplace feature brings a feeling of warmth and ambiance to any room area.  Comfortable remote control to turn unit on or off.  This is great for heat output and flame control lets the unit operate with or without heat, and gives the look from dancing flames to dying embers.',1,1,'01-00095',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(96, 'Simcoe Lustrous Chocolate Mocha Espresso Electric Fireplace and Mantel Surround','This beautiful fireplace and Solid Wood Mantel Surround in Tobacco finish is the perfect addition to any home decor.  The realistic fireplace feature brings a feeling of warmth and ambiance to any room area.  Comfortable remote control to turn unit on or off.  This is great for heat output and flame control lets the unit operate with or without heat, and gives the look from dancing flames to dying embers.',1,1,'01-00096',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(97, 'Wall Mounted Vent-free Gas Fireplace','This wall mounted vent-free fireplace has a stylish elliptical glass front and matching cherry finish wood surround.  Complete with a patented dual fuel technology that can use both natural or propane gas.',1,1,'01-00097',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(98, 'Bionaire Fireplace Heater','The Bionaire Fireplace Heater provides the ambiance and warmth of a fireplace, without the mess! No logs, no open fire, no propane, no fumes to deal with. Beautiful and modern, this heater fits in with your home and lifestyle.  Wall mountable or floor standing--move it if you wish. Fan powered heat efficiently brings the room to your desired temperature.',1,1,'01-00098',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(99, 'The Designers Edge 1,200 Watt Wall Mount Patio Heater','The Designers Edge commercial grade electric Heat Zone infrared heater is an excellent source for outdoor and indoor heating.  Easy to install and is far less expensive to use than traditional propane heaters.  The Lamp is a true infrared heat source so the actual heated area will not be affected by a breeze or wind is substantially less expensive to use than propane heaters and is far more convenient to use. Instant on this unit will cover a 100 square feet of coverage.  Great for use on the patio, garage, storage sheds, barns or workbench.',1,1,'01-00099',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(100, 'Lasko Cyclonic Ceramic Heater','It’s time to get rid of your oil-filled heater and experience the power and style of the Cyclonic Ceramic Heater. The intelligently engineered Cyclonic heater effectively circulates warm air throughout your entire room. Its glide-system pivot allows you direct warm air where it is needed the most. With easy-to-use digital controls, an adjustable thermostat and seven-hour timer to create your own, personalized comfort level. What more could you ask for?',1,1,'01-00100',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(101, 'Presto HeatDish Parabolic Heater Plus Footlight','The PRESTO® HeatDish® Plus Footlight parabolic electric heater uses a computer-designed parabolic reflector to focus heat, like a satellite dish concentrates TV signals, so it feels three times warmer than 1500 watt heaters, yet uses a third less energy. Because it warms you directly, you feel the heat almost instantly without first heating the entire room. ',1,1,'01-00101',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(102, 'Lasko Air Director Floor/Window Fan','Directs air where its needed for maximum comfort. For use on the floor or in a window. The pivoting, lightweight Lasko Air Director® fits neatly in front of any window screen for simple set-up. Three whisper-quiet speeds. ',1,1,'01-00102',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(103, 'Lasko Clip Stick 2-Pack','Lasko™ personal fans incorporate focused air movement, stylish design, and practical features to keep your personal space cool and comfortable. They can go where other fans simply cannot hold their own.',1,1,'01-00103',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(104, 'Lasko Outdoor Living Tower Fan','The space-saving Outdoor Living™ Fan creates the quiet, comfortable setting you need for summer relaxation. Enhanced engineering for quiet, yet effective outdoor winds on all three speeds.',1,1,'01-00104',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(105, 'Lasko Quickmount 20 High Velocity Floor','The versatile Lasko Quickmount Fan can sit on the floor, table or counter, or be mounted on a wall.  Durable steel construction with metal fan blades for maximum air movement.',1,1,'01-00105',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(106, 'McCormick & Schmicks Seafood Restaurant Gift Cards','The people at McCormick & Schmicks Seafood Restaurants are passionate about preparing only the freshest, mouthwatering seafood. So, its only fitting that they print their famous "Fresh List" menu twice daily. Select from 30 varieties prepared more than 80 different ways.',101,1,'01-00106',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(107, 'Delmonico Restaurant Two $50 Gift Certificates','The legend of Delmonico’s Restaurant is woven into both the history of New York City and the evolution of fine dining in America. Operating in the same building it was established in 1837, Delmonico’s has been recognized as the premier restaurant of culinary firsts. For years, we have served the finest meals to world-famous diners from the arts, politics and society. These gift cards now offer you the opportunity to dine in the old-world splendor that Delmonico’s continues to offer. You will find that the present day Delmonico’s still serves the finest Delmonico steak along with the many other trend-setting dishes. We are proud to invite you to experience fine dining as it has been offered at Delmonico’s for over 170 years. ',101,1,'01-00107',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(108, 'Las Vegas Restaurants Two $50 Gift Cards','The Las Vegas Restaurant Gift Card has something for everyone. Choose from many of the citys well known award-winning restaurants. This Restaurant Gift Card offers food lovers an incredible dining value.',100,1,'01-00108',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(109, 'Rosemary Restaurant Two $50 Gift Cards','At Rosemarys Restaurant you can experience a bit of home while in Las Vegas; the fast-paced, entertainment capital of the world. Whether you are a visitor or business traveler longing to escape the strip for a dose of reality or a local seeking a truly great neighborhood restaurant and bar, we welcome you with warm smiles and sincere concern for your total dining experience. At Rosemarys we combine great food, drink and service with uncommon value and dining diversity.',101,1,'01-00109',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(110, 'San Diego Restaurant Two $50 Gift Cards','The San Diego Restaurant Gift Card has something for everyone. Choose from many of the citys well known award-winning restaurants. Participating businesses span the entire county from The Gaslamp to La Jolla, to North County and South Bay. The San Diego Restaurant Gift Card offers food lovers an incredible dining value, while spotlighting the citys premier dining destinations.',100,1,'01-00110',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(111, 'Tap House Grill Two $50 Gift  Certificates','Tap House Grill is known for “giving beer the respect it deserves” with 160 beers on tap. Tap House Grill specializes in the largest collection of draft beer anywhere in the Northwest, serving everything from fine Northwest microbrews to beers from around the world. For non-beer drinkers, specialty cocktails include Tap House Grill’s signature Orangutanqueray and Yellow Submarine. The progressive urban dining menu features a broad spectrum of cuisine including steak, seafood and sushi ready to be paired with the perfect brew.  Executive Chef Erik Carlson’s signature style lends creativity to every meal at Tap House Grill.',100,1,'01-00111',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(112, '21 $10 Gift Certificates Redeemable at Selected Oregon Restaurants','Enjoy the Bounty of Oregon! We’ve gathered up 21 fabulous restaurants in Oregon that contributed recipes to the highly acclaimed, award-winning A Chef’s Bounty: Celebrating Oregon’s Cuisine cookbook. Take your sweetheart, your mom, your friends or business prospects out on the town to enjoy a fine meal at one of our participating restaurants. And, for those nights when you just want to stay at home, you can try one of the fabulous recipes in the cookbook from notable chefs around the state. Bring the taste of Oregon into your home! ',101,1,'01-00112',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(113, 'Brooks Steak House Two $50 Gift Cards','Locally owned Brook’s Steak House & Cellar is recognized as one of the top 10 steak houses in North America.  Serving USDA prime steaks, the finest Colorado lamb, and succulent lobster tails from southern Australia, Brook’s is truly a special dining experience.  For the wine connoisseur we offer a wine list that will capture the attention of the most discriminating pallet.  With over 1,500 wines from California and around the world, you are certain to find one of your favorites, or savor a new wine suggestion from our knowledgeable sommelier.',101,1,'01-00113',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(114, 'Vitamins Pack Galore','A multivitamin is a preparation intended to supplement a human diet with vitamins, dietary minerals and other nutritional elements. Such preparations are available in the form of tablets, capsules, pastilles, powders, liquids and injectable formulations. Other than injectable formulations, which are only available and administered under medical supervision, multivitamins are recognised by the Codex Alimentarius Commission (the United Nations highest authority on food standards) as a category of food.',3,1,'01-00114',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(115, 'Sony Bravia 40" 1080p LCD TV','It’s a high definition world and the best high definition resolution this brave new world offers is 1080p; that’s why Sony has more Full HD 1080p HDTVs than ever before. Full HD 1080p means our connections accept 1080p signals and the display will render that signal in Full HD 1080p. The flexibility of native 1920 x 1080 displays allows any high definition content to be viewed without the need to downconvert images to the native resolution of the display. Take full advantage of Blu-ray Disc™ players and PlayStation®3 systems that can deliver 1080p content.',1,1,'01-00114',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(116, 'Sony Bravia 37" LCD TV','The Sony KDL-37NL140 is everything you’d expect from a Sony and more. With 1366 x 768 resolution, 9,000:1 Dynamic contrast ratio and Bravia Engine 2™Technology; your home entertainment experience comes to life. ',1,1,'01-00115',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(117, 'Sony Bravia 52" 1080p MotionFlow 120Hz LCD HDTV','The Sony Bravia KDL52WL140 52” Motionflow™ 120Hz 1080p LCD HDTV with The BRAVIA® Internet Video Link DMX-NV1 module is taking high definition to an entirely new arena. The BRAVIA® Internet Video Link DMX-NV1 Module is a small device that attaches to the back of your DMex™ compatible BRAVIA® HDTV and connects to both DMex and HDMI.  It’s a high definition world and the best high definition resolution this brave new world offers is 1080p; that’s why Sony has more Full HD 1080p HDTVs than ever before.',1,1,'01-00116',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(118, 'Sony Blu-Ray Disc Player','See and hear a whole new world in high definition with Sony’s BDP-BX1. The crystalline clarity of 1080p Blu-ray Disc™ movies1 and DVD upscaling2 that can bring your DVDs to the highest possible quality, mean your favorite movies have never looked or sounded so good.',1,1,'01-00118',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(119, 'Sony DVD/VCR Combo','Sony’s SLV-D281P simplifies your home theater by combining a VCR and DVD player, all in one component! Now you can enjoy your VHS tapes and DVD library using the same player. It’s a full featured VCR allowing you to record your favorite shows, while watching a DVD. The DVD player portion of the component plays standard DVDs, CDs, and MP3s1. This combo DVD/VCR player is a must have for every home!',1,1,'01-00119',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(120, 'Sony DVD Player w/ HDMI output','The Sony DVPNS601HP combines simplicity w/ elegant design and brings your home theater experience to life. HDMI connectivity makes installation quick and simple and your DVD collection has never looked better',1,1,'01-00120',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(121, 'Seagate FreeAgent Go 500GB Portable Hard Drive','Smart. Simple. Sleek. Store and safeguard all your favorite files—photos, music, videos, documents, you name it—in a stylishly designed desktop storage solution thats as powerful as it is beautiful. ',1,1,'01-00121',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(122, 'HP 160GB Pocket Media Drive ','The HP Pocket Media Drive is a USB external hard drive that lets you take your digital life on the go with you. The small, lightweight drive comes in a sleek case about the size of a wallet and provides the high-capacity storage you need for your digital photos, video, MP3 files and TV programs. ',1,1,'01-00122',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(123, 'Wolverine 160GB Biometric Secure External Drive','Wolverine’s embedded fingerprint scanner digitizes your fingerprints and encrypts them into individual keys, which are in other words your passwords. Next time someone tries to retrieve your data from the USB hard drive, he will be stopped at the authentication stage. Wolverine Secure Drive allows you to store up to 5 different fingerprints to share the drive with 5 other users. Furthermore, unlike some competitors, the Wolverine Fingerprint drive will remain encrypted even if the internal hard drive is removed from the enclosure ensuring the security of your data! ',1,1,'01-00123',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(124, 'Wolverine ESP 100GB Multi Media Storage Device & Player','The Wolverine ESP was created for you to enjoy the freedom of multimedia portability anywhere, anytime.  Featuring a 3.6 razor sharp LCD and enormous storage capacity to carry and play all your digital photos, music and videos converged in the palm of your hand. ',1,1,'01-00124',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(125, 'Wolverine 160GB eSata External Hard Drive Professional Series','The Wolverine eSATA External Hard Drive, is a blazingly fast way to store any digital work. The External Serial ATA (eSATA) brings increased performance and features to external storage, surpassing performance and throughput limitations of existing USB or FireWire interfaces ',1,1,'01-00125',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(126, 'Wolverine 120GB FlashPac & 7in1  Card Reader ','The perfect travel companion for digital camera users. Store thousands of photos with a single Memory Card. A single press of a button and all your photos and digital data are effortlessly copied from the built in 7-in-1 Card Reader into a portable, self powered 120 gigabytes of mass storage unit. With the contents of your memory card now safely stored, your card is now ready to be reused - and you never had to go near a computer. Because the Wolverine FlashPac is battery operated you can literally save your data anywhere in the world. Its perfect for photographers, trips, sharing data during meetings, any situation involving a memory card.',1,1,'01-00126',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(127, 'Brussels Floor  Lamps ','Modern geometry for your home or office, the slender ‘pinch-waist’ black wood body attaches to a satin steel base.  Topping this contemporary look is a natural linen fabric shade that echoes the lamp’s shape, built with a three-way rotary switch. Made in China. ',1,1,'01-00127',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(128, 'Crystal Estrella Orb Lamp Collection','Crystal orbs are the centerpiece of these timeless designs. Simple and elegant, pure and classic. Add these lamps to any room and bring a true uplifting sparkle.',1,1,'01-00128',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(129, 'Elegance Floor Lamp','A lighting solution with elegance and simplicity. This beautiful table lamp has a chrome base, and unique shade that is crafted out of strings to give a soft look and texture, accented with hand-polished crystals. ',1,1,'01-00129',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(130, 'Guggenheim Lamp Pedestal Collection','The circular design is enhanced by its contemporary-style with brushed nickel, frosted glass, and a rich dark brown finish. The soft lighting option heightens the refined look of the pedestal.',1,1,'01-00130',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(131, 'Illuminated Pedestal Spot Light','These illuminated pedestals softly underlight sculptures, glassware, trophies, or flower arrangements for a dramatic visual display anywhere in your home. The circular design is enhanced by its contemporary-style with brushed nickel, frosted glass, and a rich dark brown finish. These pieces bring furniture, lighting and function together to fit your lifestyle.',1,1,'01-00131',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(132, 'Planar Lamp Collection','The Planar lamp collection is based on NOVAs original designs that were successful in the 1950s and 1960s. This collection makes use of NOVAs archives from years past and our interpretation of that era. The combination of wood and brushed metal, and a focus on function are highlighted in these unique lamps.',1,1,'01-00132',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(133, 'WALL-E Blu-Ray DVD 2-Disc','The highly acclaimed director of Finding Nemo and the creative storytellers behind Cars and Ratatouille transport you to a galaxy not so far away for a new cosmic comedy adventure about a determined robot named WALL-E. After hundreds of lonely years of doing what he was built for, the curious and lovable WALL-E discovers a new purpose in life when he meets a sleek search robot named EVE. Join them and a hilarious cast of characters on a fantastic journey across the universe. Transport yourself to a fascinating new world with Disney-Pixars latest adventure, now even more astonishing on DVD and loaded with bonus features, including the exclusive animated short film BURN-E. WALL-E is a film your family will want to enjoy over and over again.',1,1,'01-00133',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(134, 'The Dark Knight Blu-Ray DVD 2-Disc','The follow-up to Batman Begins, The Dark Knight™ reunites director Christopher Nolan and star Christian Bale, who reprises the role of Batman/Bruce Wayne in his continuing war on crime. With the help of Lt. Jim Gordon and District Attorney Harvey Dent, Batman sets out to destroy organized crime in Gotham for good. The triumvirate proves effective, but soon find themselves prey to a rising criminal mastermind known as The Joker, who thrusts Gotham into anarchy and forces Batman closer to crossing the fine line between hero and vigilante. Heath Ledger stars as archvillain The Joker, and Aaron Eckhart plays Dent. Maggie Gyllenhaal joins the cast as Rachel Dawes. Returning from Batman Begins are Gary Oldman as Gordon, Michael Caine as Alfred and Morgan Freeman as Lucius Fox.',1,1,'01-00134',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(135, 'The Chronicles of Narnia: Prince Caspian ','The magical world of C.S. Lewis beloved fantasy comes to life once again in Prince Caspian, the second installment of The Chronicles Of Narnia series. Join Peter, Susan, Edmund Lucy, the mighty and majestic Aslan, friendly new Narnian creatures and Prince Caspian as they lead the Narnians on a remarkable journey to restore peace and glory to their enchanted land. Continuing the adventure of The Lion, The Witch And The Wardrobe with more magic and a brand-new hero, Prince Caspian is a triumph of imagination, courage, love, joy and humor your whole family will want to watch again and again.',1,1,'01-00135',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(136, 'Wanted Blu-Ray DVD','Wesleys (James McAvoy) life is over - his pathetic old one, anyway, and its all because of a girl. Sizzling-hot Fox (Angelina Jolie) crashes into his life, introducing him to The Fraternity, a secret society of assassins led by the enigmatic Sloan (Morgan Freeman). Wes soon learns that his long-lost father was killed while working for The Fraternity and Wes has been selected to target the rogue member who murdered his father. But, before he can complete this assignment and determine his own destiny, Wes must first uncover the dark secrets behind The Fraternity.',1,1,'01-00136',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(137, 'I Am Legend DVD','Robert Neville is a brilliant scientist, but even he could not contain the terrible virus that was unstoppable, incurable, and man-made. Somehow immune, Neville is now the last human survivor in what is left of New York City and maybe the world. For three years, Neville has faithfully sent out daily radio messages, desperate to find any other survivors who might be out there. But he is not alone. Mutant victims of the plague -- The Infected -- lurk in the shadows... watching Nevilles every move... waiting for him to make a fatal mistake. Perhaps mankinds last, best hope, Neville is driven by only one remaining mission: to find a way to reverse the effects of the virus using his own immune blood. But he knows he is outnumbered... and quickly running out of time. ',1,1,'01-00137',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(138, 'Sweeney Todd DVD Special Edition','Johnny Depp and Tim Burton join forces again in a big-screen adaptation of Stephen Sondheims award-winning musical thriller Sweeney Todd. Depp stars in the title role as a man unjustly sent to prison who vows revenge, not only for that cruel punishment, but for the devastating consequences of what happened to his wife and daughter. When he returns to reopen his barber shop, Sweeney Todd becomes the Demon Barber of Fleet Street who "shaved the heads of gentlemen who never thereafter were heard from again." Joining Depp is Helena Bonham Carter as Mrs. Lovett, Sweeneys amorous accomplice, who creates diabolical meat pies. The cast also includes Alan Rickman, who portrays the evil Judge Turpin, who sends Sweeney to prison and Timothy Spall as the Judges wicked associate Beadle Bamford and Sacha Baron Cohen is a rival barber, the flamboyant Signor Adolfo Pirelli.',1,1,'01-00138',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(139, 'There Will Be Blood DVD','A sprawling epic of family, faith, power and oil, There Will Be Blood  is set on the incendiary frontier of California’s turn-of-the-century petroleum boom. The story chronicles the life and times of one Daniel Plainview (Daniel Day-Lewis), who transforms himself from a down-and-out silver miner raising a son on his own into a self-made oil tycoon. When Plainview gets a mysterious tip-off that there’s a little town out West where an ocean of oil is oozing out of the ground, he heads with his son, H.W. (Dillon Freasier), to take their chances in dust-worn Little Boston. In this hardscrabble town, where the main excitement centers around the holy roller church of charismatic preacher Eli Sunday (Paul Dano), Plainview and H.W. make their lucky strike. But even as the well raises all of their fortunes, nothing will remain the same as conflicts escalate and every human value – love, hope, community, belief, ambition and even the bond between father and son – is imperiled by corruption, deception and the flow of oil.',1,1,'01-00139',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(140, 'Memoirs of Geisha DVD','A Cinderella story set in a mysterious and exotic world, this stunning romantic epic shows how a house servant blossoms, against all odds, to become the most captivating geisha of her day.',1,1,'01-00140',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(141, 'The Happening DVD','In The Happening, M. Night Shyamalan serves up over-the-top, apocalyptic strangeness. The film opens onto New York Citys Central Park with a crowd of people enjoying an idyllic summer day. The carefree scene soon takes a terrifying turn, when out of nowhere, hordes of people begin to commit suicide en masse. People scramble to make sense of the pandemonium, and many believe it is a terrorist attack. It appears that some sort of deadly toxin is being released into the air. Cut to Elliot a science teacher in Philadelphia. When he learns of the attack on New York, he meets up with his wife Alma, his friend Julian, and Julians daughter, Jess. They make plans to get out of the city via train, but the train is evacuated in the middle of a small Pennsylvania town. When they learn that the mysterious toxin is spreading its way across the Northeast, they break up into groups, with Elliot, Alma, and Jess running through open farmland in search of safety. They are unsure of where to hide, or what exactly they are hiding from, until Elliot slowly forms a theory about the threat. He fights to keep Alma and Jess free from harm, and the film builds to a bizarre, unsettling climax, with Shyamalans usual surprise ending.',1,1,'01-00141',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(142, 'No Country for Old Men','Acclaimed filmmakers Joel and Ethan Coen deliver their most gripping and ambitious film yet in this sizzling and supercharged action-thriller. When a man stumbles on a bloody crime scene, a pickup truck loaded with heroin, and two million dollars in irresistible cash, his decision to take the money sets off an unstoppable chain reaction of violence. Not even West Texas law can contain it. Based on the novel by Pulitzer Prize-winning author Cormac McCarthy, and featuring an acclaimed cast led by Tommy Lee Jones, this gritty game of cat and mouse will take you to the edge of your seat and beyond - right up to its heart-stopping final moment.',1,1,'01-00142',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(143, 'Sockeye Salmon Freezer Pack 17.5 lbs ','The Wildcatch™ Sockeye Salmon Freezer Pack is the perfect way to stock up on wild Alaska Salmon. Each pack contains approximately seven to ten individually vacuum sealed whole, skin-on, boneless Sockeye fillets, for a total of 17.5 pounds. These fillets store conveniently in your home freezer.',1,1,'01-00143',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(144, 'Wild Pacific Halibut Fillets ','Wildcatch™ sources premium halibut from deep waters off the coast of Alaska. Halibut is known for its delicate, mild flavor, firm texture and high nutritional value. Each case contains 10 pounds of skinless halibut fillets ranging from 1-3 pounds each, enough for several holiday celebrations throughout the season. Skinless fletches are chef friendly and can be grilled, broiled, baked, poached or steamed. Alaska halibut has earned its well-deserved reputation as the worlds premium whitefish.',1,1,'01-00144',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(145, 'Alaska Smoked Fish Sampler ','The Alaska Smoked Sampler is the ultimate assortment of Wildcatch™ gourmet smoked salmon and black cod. Natural hardwood smoked and cold smoke nova lox are ready to thaw and eat, no further cooking needed. This delicious all natural smoked sampler is the perfect choice for the health conscious consumer.',1,1,'01-00145',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(146, 'Salmon & Halibut Wellness Pack','Our Salmon & Halibut Wellness Pack adds a healthy variety of salmon and halibut to your diet. A delicious, natural way to integrate great taste and heart-healthy Omega-3s into your lifestyle, this collection also makes the perfect gift for the healthy eater. All salmon and halibut products are certified sustainable by the Marine Stewardship Council.',1,1,'01-00146',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(147, 'Lamb Locker Pack All-Natural Selection ','Enjoy delicious, tender lamb year round with this lamb assortment from Niman Ranch. This sampler offers something for every lamb lover from delicate lamb chops to a substantial bone in leg of lamb that’s the perfect centerpiece of any large family gathering.',1,1,'01-00147',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(148, 'All Natural French Double Rack of Lamb','Niman Ranch® Lamb is a delicious choice any time of year. Niman Ranch® works with a small, select group of US ranchers to provide a year-round supply of seasonally fresh, young lamb. Sweet, mild and naturally tender, Niman Ranch® Lamb is simply the finest tasting lamb available.',1,1,'01-00148',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(149, 'All Natural Boneless Lamb Loin Roast','The Boneless Tied Lamb Loin from Niman Ranch® is perfect for special events or holiday dinners. Niman Ranch® works with a small, select group of ranchers who raise their sheep in the US on environmentally sustainable, family-run ranches to provide a year-round supply of seasonally fresh lamb.',1,1,'01-00149',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(150, 'All Natural Bacon & Sausage Sampler ','Break out the barbecue with this all-natural Niman Ranch Bacon & Sausage Sampler. Enjoy the 100% Beef Fearless Franks and Kid’s Dogs, a blend of beef and pork, and are sure summertime staples. The Andouille Sausage is spicy and smoky and the Chorizo spices up any meal from breakfast burritos to paella.',1,1,'01-00150',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(151, 'Korsher & Organic Grill Pack','Wise Organic Pastures believes a healthy animal is a tasty animal. Their chickens and cattle are raised on small family farms, by farmers who do things the old fashioned way. They feed their animals organic vegetarian feed, give them plenty of fresh air and sunshine, and raise them without antibiotics or growth hormones. Not only are these animals certified organic, they are certified kosher. This double certification is an unbeatable combination and adds an extra measure of care to their products.',1,1,'01-00151',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(152, 'Korsher, Organic & Free-Range Double Broiler Chickens','Wise Organic Pastures believes a healthy chicken is a tasty chicken. Their chickens are raised on small family farms, by farmers who do things the old fashioned way. They feed their chickens organic vegetarian feed, give them plenty of fresh air and sunshine, and raise them without antibiotics or growth hormones. Not only are these chickens certified organic, they are certified kosher. This double certification is an unbeatable combination and adds an extra measure of care to their products.',1,1,'01-00152',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(153, 'Fusebox Wine Blending Kit','As mentioned in Wine Spectators January 31, 2008 Edition, Fusebox is a wine-blending adventure that introduces the rarified art used by the worlds greatest winemakers. By taking the essential red varietals, Fusebox will allow you to blend wine yourself and understand how wine experts turn a good wine into a great one.',1,1,'01-00153',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(154, 'Montes Folly 2005','Rich and harmonious, the 2005 vintage of Montes Folly is a powerful wine. From the hills of Apalta Mountain, this 100% Syrah has layers of blueberry, coffee, fig, blackberry and chocolate that blend seamlessly with well-integrated tannins. Smooth balance and structure is followed by a strong, lingering finish. A beautiful wine. Ready to drink now, but could benefit from 3 to 5 years of aging.',1,1,'01-00154',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(155, 'Col Solare 2005','The 2005 growing season started cool, followed by 90+ degree days in July and early August. A temperature drop in late summer cooled the vineyards and allowed the grapes extra hand time to enhance flavor development. Harvest commenced as one of the earliest on record, and moderate weather conditions during harvest contributed to mature fruit with intense flavor, good sugar development, and high natural acidity across red varietals.',1,1,'01-00155',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(156, 'Poderi Luigi Einaudu Barolo 2004','The 2004 Poderi Luigi Einaudi Barolo has aromas of blackberry, tobacco, cinnamon, tar and spice accents, with undertones of fine wood. It is on the palate that this wine reveals its magnitude: soft, velvety tannins and notes of licorice and vanilla, a balance of plum and red fruits in the aftertaste. ',1,1,'01-00156',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(157, 'Flora Springs Trilogy Red Meritage 2005','Perched on the bench of the Mayacamas in the farthest northwest corner of the Rutherford appellation, the location of the Flora Springs Estate is enviable. In 1984, we set out to make the best wine possible by hand selecting the highest quality lots from our estate vineyards. Using a blend of three Bordeaux varietals – Cabernet Sauvignon, Cabernet Franc and Merlot – we created Trilogy, one of the original Meritage wines.',1,1,'01-00157',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(158, 'D’Arenberg The Dead Arm Shiraz 2005','The Dead Arm Shiraz 2005 from d’Arenberg is a stunning wine fashioned from ancient head-pruned vines. It was aged for 22 months in a mix of new and used French and American oak. An inky, purple color is accompanied by a glorious perfume of blueberry, pepper and smoke and flavors of blackberry, tar and spice. This deep, rich, full-bodied Shiraz is ready to drink now and could also benefit from 3 to 5 years of cellaring.',1,1,'01-00158',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(159, 'Marcarini Barolo "Brunate" 2004','The 2004 Marcarini Barolo “Brunate” shows aromas of plum with a hint of wet earth.  Menthol, licorice, dark cherries, new leather and spices are all found in this generous, layered wine.  Full-bodied, with sweet, ripe tannins provide a long, caressing finish.',1,1,'01-00159',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(160, 'Giordano Barolo 2004 ','At Valle Talloria d’Alba, the Giordano family has been producing classic wines since 1900. At the beginning the work was done by Ferdinando, the current chairman’s grandfather, who after some years was joined by his son Giovanni. The range and quantity of the wines were limited, of course, and the bottles produced were reserved for but few, regular customers.',1,1,'01-00160',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(161, 'Bodegas Caro "Caro" 2004','The 2004 Caro is a dark and complex wine. Polished with lots of structure, this Cabernet Sauvignon and Malbec blend has great depth and character. Upon decanting, wonderful aromas of violets, tobacco and black currants blend seamlessly with flavors of dark cherry, coffee and cedar notes. Firm tannins and an elegant finish make this a wonderful wine, worthy of cellaring for 4 to 5 years.',1,1,'01-00161',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(162, 'Kirkland Signature Columbia Valley Syrah 2005 2 Bottles','This Kirkland Signature™ Washington Syrah is dark opaque and deeply pigmented, with aromas of smoke, pepper, blackberry and vanilla. It is a dense, mouth-filling, lush black fruit-focused wine with fully integrated ripe tannins from sun-ripened grape skins, fruit and seasoned oak. From the attack through mid-palate and on to the finish, this wine has a seamless feel driven by the sweet fruit of black cherries and balanced acidity. Excellent now or consider the added nuance that two to three years of bottle age will add.',1,1,'01-00162',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(163, 'Martin Ray SLD Cabernet Sauvignon 2005','The Martin Ray Cabernet Sauvignon 2004 is produced from the renowned vineyards of the Stags Leap District in Napa Valley. The grapes grown in this region are influenced by the distinct river sediments in the soil.',1,1,'01-00163',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(164, 'Swiss Emmentaler Imported 32 lb (2-16 lb. Loaves)','This cheese gets its name from the Emme River in the beautiful country of Switzerland.  This classic Swiss cheese is made from cows milk with a full flavored creamy open-air taste and a subtle sweet, nutty, and fruity flavor.  Ivory to light yellow in color with cherry size holes regularly distributed.  An excellent cheese course choice, and perfect for sandwiches or salads. Emmentaler can also be used in fondue or grated on soups. Try with a Muscadet or fruity red wine. ',1,1,'01-00164',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(165, 'Rembrandt Gouda Imported 18 lb. Wheel','Rembrandt aged Gouda cheese is aged over one year and finishes with a nutty taste. Made in Holland using pasteurized cow milk. This cheese is great for grating over pasta, vegetables or just cubed as an appetizer. Pairs great with a dark beer or red wine.',1,1,'01-00165',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(166, 'Swiss Gruyere Imported 10 lb. (2-5 lb. Loaves)','Gruyere comes from the area of Switzerland north and east of Lake Geneva.  One of the truly great cheeses of the world, this cow’s milk cheese is moister and more highly flavored than Emmentaler.  Gruyere has a smooth creamy consistency with very few small pea size holes.  This is an excellent choice for fondue, and due to its sweetness is also delicious with pears and apples.  Serve with a mature Cabernet, Red or White Burgundy.',1,1,'01-00166',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(167, 'Danish Havarti Imported','From Denmark, this mild and creamy cow’s milk cheese is distinguished by innumerable small holes throughout.   Havarti is a classic sandwich cheese, and is also great for snacking with fruit or crackers. Serve with light fruity red wines.',1,1,'01-00167',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(168, 'Margaux Brie Imported 6.6 lb. Wheel ','French Brie has been called “les roi du fromage”---the king of cheeses.  Made from cow’s milk, imported Margaux de Brie is a 60% double cream soft-ripened cheese.  Margaux de Brie is made using authentic French methods, giving it a uniformly creamy, pleasant brie taste.  When Margaux de Brie is fully ripened, it should yield to the touch and slightly bulge at the sides.   Try with a warmed baguette or chilled fruit and English water crackers. Serve with a “big” red wine such as Bordeaux or Burgund',1,1,'01-00168',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(169, 'Jarlsberg Imported 22 lb. Wheel','Jarlsberg is one of America’s best selling imported cheeses. This Norwegian cheese is mellow, with a slightly sweet flavor and an elastic texture. It is golden yellow in color with variously sized round holes.  Use in a wide variety of foods and snacks, it’s perfect for sandwiches and with crackers.  Serve with dry white wines and fruity reds.',1,1,'01-00169',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(170, 'Chianti Wine Cabinet','Wine is central to the culture of Tuscany, having been cultivated there for some 3000 years. From the neatly aligned rows of vines that ride the steep hillsides of the Chianti region comes one of the worlds oldest and best-known wine-Chianti. Crafted of wood, stone, iron and glass, the Chianti Wine cabinet pays homage to this celebrated natural resource with generous facilities for storage and serving and decorated with beautiful scrolling ironwork.',1,1,'01-00170',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(171, 'Polar Ware 34 Quart Hammered Steel Party Tub','Serve beverages to your guests in style with this beautiful hand hammered party tub that will make a lasting impression with everyone.',1,1,'01-00171',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(172, 'Antique Black Tuscany Wine Stand','Great for displays, serving and space saving! The decorative and functional Tuscany wine stand holds glasses and wine for a well turned-out display. The top effortlessly doubles as a serving tray when needed. Practicality and elegance in one place.',1,1,'01-00172',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(173, 'Rogar Antique Bronze Estate Wine Opener','Rogars Estate Wine opener features an antique bronze finish and is a beautiful hand-finished replica of an 1890s American antique. Absolute black granite on the table stand base and handle further enriches the luxurious of the opener. Functional, as well as elegant, this wine opener can both uncork and recork wine bottles with ease.',1,1,'01-00173',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(174, 'Quest Beverage Dispencer Two Gallon Capacity','This high-grade quality beverage dispenser is perfect for catering, parties, or large gatherings and can be used for hot and cold beverages. The inner metal cylinder can be filled with water and frozen to keep beverages cold without diluting the beverage. The clear polypropylene tank is durable and easy to clean, and the industrial grade nozzle provides easy flow delivery.',1,1,'01-00174',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(175, 'Metrokane Zippity Rabbit Corkscrew Set','A wonderful gift for wine lovers. The Zippity Rabbit™  corkscrew set begins with a Metrokane sterling lever-style corkscrew that can pull a wine cork in seconds  and works on all types of wine bottles. Its handles are ergonomically shaped to fit snugly in the hands, with rubber grip pads for better leverage.',1,1,'01-00175',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(186, 'Sisley Daily Line Reducer ','Directs air where its needed for maximum comfort. For use on the floor or in a window. The pivoting, lightweight Lasko Air Director® fits neatly in front of any window screen for simple set-up. Three whisper-quiet speeds. ',1,1,'01-00186',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(187, 'Sisley Global Anti-Aging Day and Night Cream','Sisley Paris Global Anti-Age Day and Night Cream offers complete skin care for your face.  Filled with natural botanicals and vitamins such as B5 and A that will smooth, soften and moisturize your skin for a healthy, vibrant glow. ',1,1,'01-00187',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(188, 'Sisley All Day All Year Essential Day Care','Sisley All Day All Year Essential Day Care is an advanced, three-in-one daily moisturizer for the face and neck. This sophisticated moisturizer provides intense hydration for soft supple skin, and UVA, UVB, and environmental protection to prevent skin from premature aging, such as wrinkles, discoloration and loss of elasticity. Boasts natural extracts to aid in skin rejuvenation.',1,1,'01-00188',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(189, 'Sisley Ecological Compound','Sisley Ecological Compound is a rich, concentrated day/night emulsion intended to help moisturize, tone and protect the sensitive skin on your face and neck. Leaves your skin looking fresh and vibrant. This light, anti-aging formula is filled with vital oils and soothing plant extracts. ',1,1,'01-00189',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(190, 'Sisley Night Complex w/ Collagen & Woodmallow','Sisley Night Cream is a hydrating and rejuvenating night cream specially designed for dull, dry or sensitive skin. Contains natural rejuvenating nutrients, including woodmallow and collagen. Leaves skin soft, toned, and renewed. ',1,1,'01-00190',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(191, 'Strivectin SD-Cream','Theres a reason StriVectin-SD®, the stretch-mark cream turned anti-wrinkle phenomenon™, is the number-one selling prestige skin cream in the entire world (including France). StriVectin-SD helps reduce the appearance of fine lines, wrinkles and crows feet... the type of fine lines, wrinkles and crows feet that can add 10-15 years to your appearance, and which Botulinum Toxin treatments leave behind. StriVectin-SD helps give you a youthful, healthy-looking, glowing complexion faster than retinol, far superior to vitamin C, and without irritation, needles, or surgery. ',1,1,'01-00191',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(192, 'SkinMedica TNS Recovery','SkinMedica TNS Recovery Complex® with NouriCel-MD is a powerful, intense repair cream designed to revitalize your aging or sun damaged skin. NouriCel-MD is a combination of essential elements found in healthy, young skin.',1,1,'01-00192',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(193, 'Sisley Eye and Lip Contour Complex','Sisley Eye and Lip Contour Complex is a rich, concentrated night emulsion intended to help moisturize, tone and smooth the sensitive skin around your lips and eyes. Also helps reduce puffiness, fine lines and darkness so you look fresh and vibrant. This anti-aging formula is filled with vital oils and soothing plant extracts.',1,1,'01-00193',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(194, 'Strivectin Gift of Youth','This fantastic StriVectin kit contains three proven StriVectin products in one convenient set: The original StriVectin-SD®, the “stretch-mark cream turned anti-wrinkle phenomenon™,” for general face and body use (reducing the appearance of fine lines, wrinkles and crows’ feet as well as existing stretch marks); StriVectin-SD® Eye Cream (for reducing the appearance of fine lines, wrinkles and crows’ feet in the orbital eye area); and StriVectin-HS® Hydro-Thermal Deep Wrinkle Serum™, the first and only formulation to address the integrity of the Dermal-Epidermal Junction (DEJ)… the place where deep wrinkles really begin. StriVectin-HS is an intensive, heat-activated concentrate designed for direct application to deep, “problem” wrinkles and creases on the face, neck, and décolleté. Our StriVectin kit is a great value and a fantastic way to introduce yourself to the world’s best-selling prestige skin treatments.',1,1,'01-00194',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(195, 'Prevage MD Anti-Aging Treatment','PREVAGE® MD anti-aging treatment is an intense antioxidant designed to improve skin texture, tone and pigmentation, while diminishing wrinkles. This amazing product also helps to fight premature aging by shielding your skin from harmful sun exposure, pollution, and environmental conditions.',1,1,'01-00195',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(196, 'Shiseido Future Solution Eye & Lip Contour Cream','Help support the fragile, sensitive skin around the eyes and lips with Shiseido Future Solution Eye & lip Contour Crème. This rich moisturizing cream tightens and tones skin while diminishing wrinkles, fine lines and discoloration. Improves elasticity. Your skin will appear more vibrant and youthful.',1,1,'01-00196',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(197, 'Shiseido Bio Performance Super Restoring Cream','Shiseido Bio Performance Super Restoring Cream is an anti-aging facial cream that will moisturize, smooth and firm your skin.  Improves elasticity for a more youthful appearance.',1,1,'01-00197',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(198, 'Sisley Paris Eye & Lip Contour Balm 1 oz.','Sisley Eye and Lip Contour Balm is a clear gel formula intended to help protect, hydrate and smooth the sensitive skin around your lips and eyes. Apply in the morning to help reduce puffiness, fine lines and dryness so you look fresh and vibrant. This anti-aging formula is filled with active plant extracts, which may temporarily produce a slight prickling feeling.',1,1,'01-00198',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(199, 'Strivectin Deep Wrinkle Filler','is pleased to announce StriVectin®-WF Instant Deep Wrinkle Filler. It’s a fast-acting topical formulation you apply directly to those “problem areas” such as the deep creases between your eyebrows, at the corners of your eyes, around the lips and along the nasolabial folds between your nose and mouth. You simply put it on, and immediately see a visible difference in your wrinkles and creases. You can apply it as often as needed throughout the day. But that’s not all! You can also apply a small amount of StriVectin-WF to larger areas of your face where you need help with fine lines, wrinkles, and crows’ feet. As StriVectin-WF begins to work, you’ll notice an instant improvement in the appearance of deep wrinkles, fine wrinkles, any wrinkles at all! And what’s more, it works better the longer you use it. It’s as simple as that!',1,1,'01-00199',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(200, 'Kirkland Signature by Borghese Mineral Loose Powder Foundation SPF 15','New! A Naturally Beautiful Look.  One Sweep with our exclusive Kabuki brush and you’ll feel, and see, the difference.  This is foundation as it should be: light and soft to the skin.  With adjustable coverage, it helps even skin texture and provides a luminous, naturally perfect finish.  Infused with a multi-mineral complex, this formula is paraben-free, talc-free, fragrance-free, and oil-free.',1,1,'01-00200',1)
INSERT [dbo].Product(ProductID,Name,Description,ProductTypeID,SalesPromptID,SKU,Published) values(201, 'MP3 Songs','Buy your favorite music right here right now!',1,1,'01-00201',1)
set IDENTITY_INSERT [dbo].Product OFF;
GO

--_Product Affiliate
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (1,1,1);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (2,1,2);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (3,2,3);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (5,3,5);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (6,3,6);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (7,3,7);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (8,3,8);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (9,4,9);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (10,4,10);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (11,5,11);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (12,6,12);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (13,6,13);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (14,7,14);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (15,3,15);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (16,3,16);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (17,3,17);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (18,8,18);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (19,8,19);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (20,8,20);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (21,8,21);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (40,9,40);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (41,9,41);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (42,9,42);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (43,10,43);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (44,10,44);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (45,10,45);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (46,10,46);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (47,10,47);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (48,11,48);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (49,12,49);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (50,12,50);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (51,12,51);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (52,12,52);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (53,12,53);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (54,12,54);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (55,12,55);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (56,12,56);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (68,15,68);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (69,15,69);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (70,15,70);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (71,15,71);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (78,16,78);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (79,16,79);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (80,16,80);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (81,17,81);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (82,17,82);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (83,17,83);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (84,19,84);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (85,19,85);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (86,18,86);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (87,18,87);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (88,19,88);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (89,18,89);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (90,18,90);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (91,19,91);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (92,19,92);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (102,20,102);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (103,20,103);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (104,20,104);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (105,20,105);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (121,21,121);
INSERT INTO [dbo].ProductAffiliate(ProductID,AffiliateID,DisplayOrder) VALUES (200,22,200);
go

--_LocaleSetting 
set IDENTITY_INSERT [dbo].LocaleSetting ON;
INSERT [dbo].LocaleSetting(LocaleSettingID,Name,DisplayOrder,Description) values(1,'en-US' ,1,'United States');
--INSERT [dbo].LocaleSetting(LocaleSettingID,Name,DisplayOrder,Description) values(2,'es-ES' ,2,'Spain');
set IDENTITY_INSERT [dbo].LocaleSetting OFF;
GO

--_Currency
set IDENTITY_INSERT [dbo].Currency ON;
INSERT [dbo].Currency(CurrencyID,Name,CurrencyCode,ExchangeRate,Published,DisplayOrder,DisplayLocaleFormat) values(1,'US Dollar','USD',1.00,1,1,'en-US');
INSERT [dbo].Currency(CurrencyID,Name,CurrencyCode,ExchangeRate,Published,DisplayOrder,DisplayLocaleFormat) values(2,'Canadian Dollar','CAD',1,0,2,'en-CA');
INSERT [dbo].Currency(CurrencyID,Name,CurrencyCode,ExchangeRate,Published,DisplayOrder,DisplayLocaleFormat) values(3,'Spanish Peseta EUR','ESP',1,0,2,'es-ES');
INSERT [dbo].Currency(CurrencyID,Name,CurrencyCode,ExchangeRate,Published,DisplayOrder,DisplayLocaleFormat) values(4,'British Pound','GBP',1,0,2,'en-GB');
INSERT [dbo].Currency(CurrencyID,Name,CurrencyCode,ExchangeRate,Published,DisplayOrder,DisplayLocaleFormat) values(5,'Australian Dollar','AUD',NULL,0,2,'en-AU');
INSERT [dbo].Currency(CurrencyID,Name,CurrencyCode,ExchangeRate,Published,DisplayOrder,DisplayLocaleFormat) values(6,'Japanese Yen','JPY',1,0,2,'ja-JP');
set IDENTITY_INSERT [dbo].Currency OFF;
GO

set IDENTITY_INSERT [dbo].KitGroup ON;
INSERT [dbo].KitGroup(KitGroupID,Name,Description,ProductID,DisplayOrder,KitGroupTypeID,IsRequired) values(1,'Processors','The primary chip of the system that oversees all the other components of the system.',48,1,1,1);
INSERT [dbo].KitGroup(KitGroupID,Name,Description,ProductID,DisplayOrder,KitGroupTypeID,IsRequired) values(2,'Memory','Chips in the computer used for temporary storage of data.',48,2,1,1);
INSERT [dbo].KitGroup(KitGroupID,Name,Description,ProductID,DisplayOrder,KitGroupTypeID,IsRequired) values(3,'Hard drive','The main device a computer uses to permanently store and retrieve information.',48,3,1,1);
INSERT [dbo].KitGroup(KitGroupID,Name,Description,ProductID,DisplayOrder,KitGroupTypeID,IsRequired) values(4,'Graphics Card','A peripheral device that attaches to the PCI or AGP slot in your computer to enable the computer to process and deliver video. Once installed in the computer, a cable is used to attach the graphics card to a computer monitor.',48,4,1,1);
set IDENTITY_INSERT [dbo].KitGroup OFF;
GO

set IDENTITY_INSERT [dbo].KitItem ON;
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(1,1 ,'Intel(R) Pentium(R) Dual-Core processor E5200 (2.5.GHz)','',0.00,1,1);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(2,1 ,'Intel(R) Core(TM) 2 Duo processor E4700 (2.6GHz)','',29.00,0,2);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(3,1 ,'Intel(R) Core(TM) 2 Duo processor E7200 (2.5GHz, 3MB)','',75.00,0,3);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(4,1 ,'Intel(R) Core(TM) 2 Quad processor Q9300','',232.00,0,4);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(5,2 ,'2GB DDR2-800MHz dual channel SDRAM (2x1024)','',0.00,1,1);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(6,2 ,'3GB DDR2-800MHz SDRAM (1x2048,1x1024)','',48.00,0,2);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(7,2 ,'4GB DDR2-800MHz dual channel SDRAM (2x2048)','',93.00,0,3);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(8,3 ,'320GB 7200 rpm SATA 3Gb/s hard drive','',0.00,1,1);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(9,3 ,'640GB 7200 rpm SATA 3Gb/s hard drive','',47.00,0,2);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(10,3 ,'1TB 7200 rpm SATA 3Gb/s hard drive','',920.00,0,3);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(11,4 ,'Integrated Graphics (NVIDIA GeForce 7100), VGA','',0.00,1,1);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(12,4 ,'128MB NVIDIA GeForce 9300, DVI-I, VGA adapter','',30.00,0,2);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(13,4 ,'256MB NVIDIA GeForce 9300, DVI-I, VGA adapter, HDMI','',60.00,0,3);
INSERT [dbo].KitItem(KitItemID,KitGroupID,Name,Description,PriceDelta,IsDefault,DisplayOrder) values(14,4 ,'512MB NVIDIA GeForce 9500GS, DVI-I, HDMI, VGA adapter','',88.00,0,4);
set IDENTITY_INSERT [dbo].KitItem OFF;
GO

--_Product Genre
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (1,1,1);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (1,2,2);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (2,3,3);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (3,4,4);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (3,5,5);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (3,6,6);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (3,7,7);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (3,8,8);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (4,9,9);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (4,10,10);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (4,11,11);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (4,12,12);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (4,13,13);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (5,14,14);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (3,15,15);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (3,16,16);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (3,17,17);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (6,18,18);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (6,19,19);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (6,20,20);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (7,21,21);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (8,22,22);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (8,23,23);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (8,24,24);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (8,25,25);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (8,26,26);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (8,27,27);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (9,28,28);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (10,29,29);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (9,30,30);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (11,31,31);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (11,32,32);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (11,33,33);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (11,34,34);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (11,35,35);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (12,36,36);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (12,37,37);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (12,38,38);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (12,39,39);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (13,40,40);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (13,41,41);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (13,42,42);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (13,43,43);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (13,44,44);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (13,45,45);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (13,46,46);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (13,47,47);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (14,48,48);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (14,49,49);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (15,50,50);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (15,51,51);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (16,60,60);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (16,61,61);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (16,62,62);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (16,63,63);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (16,64,64);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (16,65,65);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (16,66,66);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (16,67,67);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (17,68,68);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (17,69,69);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (17,70,70);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (17,71,71);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (18,72,72);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (18,73,73);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (19,74,74);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (19,75,75);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (20,76,76);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (20,77,77);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (21,115,115);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (21,116,116);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (21,117,117);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (22,118,118);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (23,119,119);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (23,120,120);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (24,121,121);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (24,122,122);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (24,123,123);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (24,124,124);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (24,125,125);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (24,126,126);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (25,127,127);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (25,128,128);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (25,129,129);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (25,130,130);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (25,131,131);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (25,132,132);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (26,143,143);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (26,144,144);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (26,145,145);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (26,146,146);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (27,147,147);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (27,148,148);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (27,149,149);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (27,150,150);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (27,151,151);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (27,152,152);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (28,153,153);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (28,154,154);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (28,155,155);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (28,156,156);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (28,157,157);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (28,158,158);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (28,159,159);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (28,160,160);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (28,161,161);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (28,162,162);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (28,163,163);
INSERT INTO [dbo].ProductGenre(GenreID,ProductID,DisplayOrder) values (29,201,201);

--1 Video Game                 9   Scooters						17  Vitamin					25  Seafoods
--2 Remote Control Toys        10  Desktops						18  Cooling and Heating		26  Lamb						
--3 Riding Toys                11  Basic Bedding				19  Gift Cards				27  Wine
--4 Refrigerator			   12  Printers and Fax				20  Televisions				28  Cheese
--5 Microwave Oven             13  Gemstone						21  Disc Player				29  Wine Storage
--6 Kids Jewelry			   14  Tabletop						22  Storage Device			30  Magazine Subs
--7 Kids Bike				   15  Living Room Furnitures		23  Lighting				31  Body & Skin Care
--8 Adult Bikes                16  Women's Apparel				24  Movies

--_ProductCategory
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(1,1,1);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(2,1,2);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(3,1,3);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(4,2,4);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(5,2,5);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(6,2,6);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(7,2,7);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(8,2,8);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(9,3,9);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(10,3,10);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(11,3,11);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(12,3,12);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(13,3,13);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(14,1,14);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(15,2,15);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(16,2,16);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(17,2,17);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(18,4,18);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(19,4,19);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(20,4,20);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(21,4,21);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(22,6,22);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(23,6,23);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(24,6,24);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(25,6,25);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(26,6,26);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(27,6,27);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(28,6,28);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(29,6,29);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(30,6,30);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(31,7,31);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(32,7,32);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(33,7,33);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(34,7,34);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(35,7,35);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(36,7,36);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(37,7,37);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(38,7,38);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(39,7,39);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(40,9,40);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(41,9,41);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(42,9,42);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(43,9,43);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(44,9,44);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(45,9,45);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(46,9,46);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(47,9,47);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(48,10,48);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(49,10,49);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(50,11,50);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(51,11,51);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(52,12,52);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(53,12,53);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(54,12,54);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(55,12,55);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(56,12,56);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(57,12,57);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(58,12,58);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(59,12,59);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(60,13,60);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(61,13,61);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(62,13,62);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(63,13,63);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(64,13,64);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(65,13,65);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(66,13,66);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(67,13,67);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(68,14,68);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(69,14,69);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(70,14,70);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(71,14,71);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(72,15,72);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(73,15,73);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(74,15,74);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(75,15,75);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(76,14,76);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(77,14,77);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(78,16,78);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(79,16,79);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(80,16,80);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(81,17,81);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(82,17,82);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(83,17,83);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(84,17,84);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(85,17,85);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(86,17,86);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(87,17,87);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(88,17,88);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(89,17,89);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(90,17,90);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(91,17,91);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(92,17,92);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(93,18,93);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(94,18,94);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(95,18,95);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(96,18,96);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(97,18,97);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(98,18,98);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(99,18,99);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(100,18,100);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(101,18,101);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(102,18,102);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(103,18,103);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(104,18,104);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(105,18,105);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(106,19,106);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(107,19,107);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(108,19,108);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(109,19,109);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(110,19,110);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(111,19,111);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(112,19,112);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(113,19,113);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(114,17,114);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(115,20,115);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(116,20,116);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(117,20,117);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(118,21,118);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(119,21,119);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(120,21,120);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(121,22,121);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(122,22,122);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(123,22,123);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(124,22,124);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(125,22,125);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(126,22,126);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(127,23,127);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(128,23,128);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(129,23,129);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(130,23,130);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(131,23,131);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(132,23,132);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(133,24,133);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(134,24,134);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(135,24,135);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(136,24,136);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(137,24,137);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(138,24,138);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(139,24,139);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(140,24,140);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(141,24,141);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(142,24,142);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(143,25,143);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(144,25,144);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(145,25,145);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(146,25,146);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(147,25,147);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(148,25,148);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(149,25,149);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(150,25,150);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(151,25,151);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(152,25,152);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(153,27,153);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(154,27,154);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(155,27,155);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(156,27,156);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(157,27,157);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(158,27,158);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(159,27,159);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(160,27,160);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(161,27,161);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(162,27,162);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(163,27,163);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(164,28,164);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(165,28,165);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(166,28,166);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(167,28,167);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(168,28,168);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(169,28,169);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(170,29,170);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(171,29,171);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(172,29,172);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(173,29,173);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(174,29,174);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(175,29,175);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(186,31,186);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(187,31,187);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(188,31,188);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(189,31,189);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(190,31,190);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(191,31,191);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(192,31,192);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(193,31,193);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(194,31,194);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(195,31,195);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(196,31,196);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(197,31,197);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(198,31,198);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(199,31,199);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(200,31,200);
INSERT [dbo].ProductCategory(ProductID,CategoryID,DisplayOrder) values(201,32,201);

--1 Electronics					9   Furnitures 
--2 Toys						10  Fashion
--3 Major Appliances			11  Health
--4 Jewelry & Accessories		12  Gifts & Tickets
--5 Sports & Recreations		13  Books/cds/movies
--6 Computers					14  Foods & Wines
--7 Bed & Bath					15  Decors
--8 Housewares					16  Beauty

--_ProductSection
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(1,1,1);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(2,1,2);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(3,1,3);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(4,2,4);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(5,2,5);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(6,2,6);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(7,2,7);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(8,2,8);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(9,2,9);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(10,2,10);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(11,2,11);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(12,2,12);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(13,2,13);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(14,1,14);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(15,2,15);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(16,2,16);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(17,2,17);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(18,3,18);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(19,3,19);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(20,3,20);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(21,3,21);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(22,4,22);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(23,4,23);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(24,4,24);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(25,4,25);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(26,4,26);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(27,4,27);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(28,4,28);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(29,4,29);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(30,4,30);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(31,5,31);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(32,5,32);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(33,5,33);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(34,5,34);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(35,5,35);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(36,5,36);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(37,5,37);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(38,5,38);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(39,5,39);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(40,5,40);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(41,5,41);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(42,5,42);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(43,5,43);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(44,5,44);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(45,5,45);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(46,5,46);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(47,5,47);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(48,6,48);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(49,6,49);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(50,7,50);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(51,7,51);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(52,6,52);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(53,6,53);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(54,6,54);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(55,6,55);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(56,6,56);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(57,6,57);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(58,6,58);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(59,6,59);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(60,4,60);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(61,4,61);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(62,4,62);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(63,4,63);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(64,4,64);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(65,4,65);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(66,4,66);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(67,4,67);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(68,8,68);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(69,8,69);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(70,8,70);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(71,8,71);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(72,9,72);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(73,9,73);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(74,9,74);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(75,9,75);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(76,8,76);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(77,8,77);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(78,10,78);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(79,10,79);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(80,10,80);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(81,11,81);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(82,11,82);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(83,11,83);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(84,11,84);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(85,11,85);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(86,11,86);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(87,11,87);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(88,11,88);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(89,11,89);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(90,11,90);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(91,11,91);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(92,11,92);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(93,3,93);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(94,3,94);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(95,3,95);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(96,3,96);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(97,3,97);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(98,3,98);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(99,3,99);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(100,3,100);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(101,3,101);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(102,3,102);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(103,3,103);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(104,3,104);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(105,3,105);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(106,12,106);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(107,12,107);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(108,12,108);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(109,12,109);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(110,12,110);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(111,12,111);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(112,12,112);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(113,12,113);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(114,11,114);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(115,1,115);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(116,1,116);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(117,1,117);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(118,1,118);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(119,1,119);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(120,1,120);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(121,1,121);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(122,1,122);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(123,1,123);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(124,1,124);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(125,1,125);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(126,1,126);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(127,9,127);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(128,9,128);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(129,9,129);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(130,9,130);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(131,9,131);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(132,9,132);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(133,13,133);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(134,13,134);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(135,13,135);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(136,13,136);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(137,13,137);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(138,13,138);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(139,13,139);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(140,13,140);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(141,13,141);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(142,13,142);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(143,14,143);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(144,14,144);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(145,14,145);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(146,14,146);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(147,14,147);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(148,14,148);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(149,14,149);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(150,14,150);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(151,14,151);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(152,14,152);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(153,14,153);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(154,14,154);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(155,14,155);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(156,14,156);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(157,14,157);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(158,14,158);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(159,14,159);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(160,14,160);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(161,14,161);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(162,14,162);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(163,14,163);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(164,14,164);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(165,14,165);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(166,14,166);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(167,14,167);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(168,14,168);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(169,14,169);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(170,14,169);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(171,15,171);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(172,15,172);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(173,15,173);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(174,15,174);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(175,8,175);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(186,16,186);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(187,16,187);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(188,16,188);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(189,16,189);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(190,16,190);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(191,16,191);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(192,16,192);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(193,16,193);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(194,16,194);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(195,16,195);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(196,16,196);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(197,16,197);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(198,16,198);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(199,16,199);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(200,16,200);
INSERT [dbo].ProductSection(ProductID,SectionID,DisplayOrder) values(201,13,201);

--1 Sony					     --9   Whirlpool			--17  Brother				--25  Providence
--2 Nintendo					 --10  Razor				--18  Reed & Barton			--26  Simcoe
--3 Team Up                      --11  Micro				--19  Petite				--27  Designer Edge
--4 Megatech                     --12  HP					--20  Comfort Research		--28  Lasko	
--5 Kettler                      --13  Dell					--21  Barbour				--29  Presto
--6 Caterpillar                  --14  Pacific Coast		--22  Centrum				--30  Bionaire
--7 Step2						 --15  Joseph Abboud		--23  Nature Made			--31  Seagate
--8 Microsoft					 --16  Lexmark				--24  One A Day				--32  Wolverine


--33  WildCatch					 --41  D Arenberg			--48  Rogar's State			--55  Shiseido  	
--34  Niman Ranch				 --42  Marcarini			--49  Quest					
--35  Wise						 --43  Giordano				--50  Zippity
--36  Fusebox					 --44  Bodegas Caro			--51  Sisley
--37  Montes Folly				 --45  Kirkland Signature	--52  Strivectin	
--38  Col Solare				 --46  Martin Ray			--53  SkinMedica
--39  Poderi Luigi				 --47  Rembrandth			--54  Prevage
--40  Flora Springs	

--_Product Manufacturer
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(1,1,1)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(2,1,2)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(3,2,3)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(4,3,4)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(5,4,5)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(6,4,6)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(7,4,7)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(8,4,8)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(9,5,9)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(10,5,10)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(11,6,11)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(12,7,12)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(13,7,13)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(14,8,14)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(15,4,15)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(16,4,16)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(17,4,17)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(18,9,18)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(19,9,19)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(20,9,20)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(21,9,21)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(31,5,31)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(32,5,32)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(33,5,33)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(34,5,34)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(35,5,35)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(36,5,36)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(37,5,37)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(38,5,38)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(39,5,39)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(40,10,40)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(41,10,41)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(42,10,42)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(43,11,43)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(44,11,44)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(45,11,45)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(46,11,46)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(47,11,47)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(48,12,48)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(49,13,49)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(50,14,50)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(51,15,51)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(52,16,52)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(53,16,53)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(54,16,54)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(55,16,55)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(56,16,56)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(57,16,57)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(58,16,58)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(59,17,59)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(68,18,68)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(69,18,69)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(70,18,70)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(71,18,71)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(72,19,72)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(73,19,73)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(74,20,74)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(75,20,75)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(76,18,76)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(77,18,77)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(78,21,78)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(79,21,79)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(80,21,80)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(81,22,81)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(82,22,82)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(83,22,83)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(84,23,84)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(85,23,85)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(86,24,86)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(87,24,87)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(88,23,88)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(89,24,89)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(90,24,90)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(91,23,91)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(92,23,92)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(95,25,95)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(96,26,96)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(97,30,97)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(98,30,98)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(99,27,99)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(100,28,100)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(101,29,101)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(102,28,102)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(103,28,103)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(104,28,104)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(105,28,105)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(115,1,115)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(116,1,116)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(117,1,117)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(118,1,118)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(119,1,119)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(120,1,120)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(121,31,121)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(122,12,122)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(123,32,123)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(124,32,124)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(125,32,125)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(126,32,126)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(143,33,143)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(144,33,144)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(145,33,145)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(146,33,146)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(147,34,147)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(148,34,148)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(149,34,149)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(150,34,150)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(151,35,151)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(152,35,152)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(153,36,153)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(154,37,154)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(155,38,155)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(156,39,156)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(157,40,157)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(158,41,158)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(159,42,159)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(160,43,160)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(161,44,161)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(162,45,162)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(163,46,163)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(165,47,165)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(173,48,173)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(174,49,174)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(175,50,175)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(186,51,186)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(187,51,187)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(188,51,188)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(189,51,189)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(190,51,190)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(191,52,191)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(192,53,192)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(193,51,193)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(194,52,194)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(195,54,195)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(196,55,196)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(197,55,197)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(198,51,198)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(199,52,199)
INSERT [dbo].ProductManufacturer(ProductID,ManufacturerID,DisplayOrder) values(200,45,200)

--1 Mountain Bike
--2 Kiddie Bike
--3 BMX
--4 Salmon Fish 
--5 Lamb Meat
--6 Chicken Meat
--7 LCD TV
--8 Jacket
--9 Coat
--10 Powder Foundation
--11 Night Cream
--12 Eye and Lip Contour

--_ProductVector
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (36,1,1)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (37,1,2)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (38,1,3)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (39,1,4)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (32,2,5)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (33,2,6)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (34,2,7)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (35,2,8)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (31,3,9)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (143,4,10)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (144,4,11)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (145,4,12)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (146,4,13)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (152,6,14)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (147,5,15)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (148,5,16)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (149,5,17)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (115,7,18)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (116,7,19)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (117,7,20)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (78,8,21)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (79,8,22)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (80,9,23)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (200,10,24)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (187,11,25)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (190,11,26)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (193,12,27)
INSERT [dbo].ProductVector(ProductID,VectorID,DisplayOrder) values (198,12,28)


--_Product Distributor 
INSERT [dbo].ProductDistributor(DistributorID,ProductID,DisplayOrder) select 1,ProductID,ProductID from [dbo].Product group by ProductID having ProductID <= 127
INSERT [dbo].ProductDistributor(DistributorID,ProductID,DisplayOrder) select 2,ProductID,ProductID from [dbo].Product group by ProductID having ProductID > 127

--_ProductVariant
set IDENTITY_INSERT [dbo].ProductVariant ON;
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(1,1,1,'PlayStation 3 Console 80GB','The ultimate high-definition entertainment experience has arrived. With the 80GB PlayStation 3 system, you get PlayStation Network membership, built-in Wi-Fi and 80GB of hard disk drive storage for games, music, videos and photos. Also, every PS3 comes with a built-in Blu-ray player to give you pristine picture quality and the best high-definition viewing experience available. Whether its gaming, Blu-ray movies, music or online services, experience it all with the PlayStation 3. Power pack includes a cooling system, a charging station, an HDMI cable and a remote control.','-ps380gb',399.99,13.14,'17x13.5x7',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(2,1,0,'PlayStation 3 Console 40GB','The PS3 features IBMs "Cell" processor and a co-developed Nvidia graphics processor that makes the system able to perform two trillion calculations per second. That makes the PlayStation 3 40 times faster than the PS2. Along with the traditional AV and composite connections, it also boasts an HDMI (High-Definition Multimedia Interface) port, which delivers uncompressed, unconverted digital picture and sound to compatible high-definition TV and projectors. The system is capable of 128-bit pixel precision and 1080p resolution for a full HD experience. This console also provides for a sound experience by supporting Dolby Digital 5.1, DTS 5.1, as well as Linear PCM 7.1. A pre-installed 40 GB hard disc drive allows you to save games as well as download content from the internet. Unlike the other models of the PlayStation 3, the 40GB does not offer backwards compatibility.','-ps340gb',315.00,11,'10.8x12.75x3.86',5);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(3,2,1,'','','',169.00,1.51,'9.8x8.5x2',40);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(4,3,1,'','','',129.99,1.05,'7.8x4.9x1.9',25);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(5,4,1,'Team Up #8 Dale Earnhardt Jr 1:18 Scale Remote Control Car','For Dale Earnhardth Jr Fan!','-dearn',34.99,'3.20','',50);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(6,4,0,'Team Up #01 Mark Martin 1:18 Scale Remote Control Car','For Mark Martin Fan!','-mmart',34.99,'3.20','',50);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(7,4,0,'Team Up #24 Jeff Gordon 1:18 Scale Remote Control Car','For Jeff Gordon Fan!','-jgord',34.99,'3.20','',50);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(8,4,0,'Team Up #20 Tony Stewart 1:18 Scale Remote Control Car  ','For Tony Stewart Fan!','-tstew',34.99,'3.20','',50);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(9,5,1,'','','',69.99,'14','',25);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(10,6,1,'','','',69.99,'14','',25);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(11,7,1,'','','',24.99,'6.54','',45);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(12,8,1,'','','',89.99,'8.92','',15);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(13,9,1,'','','',179.99,'8.92','',20);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(14,10,1,'','','',79.99,'1.52','',25);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(15,11,1,'','','',119.99,'.92','',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(16,12,1,'','','',62.98,'.94','',15);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(17,13,1,'','','',99.99,'.95','',40);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(18,14,1,'','','',299.99,'13.04','12.5x11.5x7.3',25);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(19,15,1,'','','',79.99,'14.26','',20);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(20,16,1,'','','',24.99,'14.21','',15);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(21,17,1,'','','',59.99,'6.5','',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(22,18,1,'','','',1699.99,'.15','35x68x70',5);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(23,19,1,'','','',1499.99,'.15','35x68x49',15);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(24,20,1,'','','',1499.99,'.15','32x68x33',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(25,21,1,'Whirlpool Stainless Steel  Maxwave 10 Level Power Controls Sensor Reheat','1.7 CuFt. 1,200 Watts. Stainless Steel.','-ss1.7c1200w',319.99,39,'22x17x13',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(26,21,0,'Whirlpool White MaxWave 10 Level Power Controls Sensor Reheat','1.7 CuFt. 1,200 Watts. White.','-wht1.7c1200w',199.99,39,'22x17x13',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(27,21,0,'Whirlpool Black 10 Level Power Controls','1.1 CuFt. 1,100 Watts. Black.','-blk1.1c1100w',129.99,38,'20x15x11',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(28,22,1,'','','',69.99,'.15','.57.x.59',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(29,23,1,'','','',49.99,'.15','.37.x.4',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(30,24,1,'','','',39.99,'.15','.36.x.62',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(31,25,1,'','','',39.99,'5.98','.12.x.12',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(32,26,1,'','','',39.99,'5.98','.25.x.24',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(33,27,1,'','','',39.99,'5.98','.55.x.36',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(34,28,1,'','','',39.99,'5.98','.29.x.39',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(35,29,1,'','','',19.99,'5.98','',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Weight,Dimensions,Inventory) values(36,30,1,'','','',59.99,'5.98','.12.x.12',10);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(37,31,1,'','','',189.99,10,'5.98');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(38,32,1,'','','',211.75,10,'5.98');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(39,33,1,'16-Inch Kettler Pulse Candy Girls Bike - Turquoise/Pink','The Pulse Candy is one chic bike that is sure to spark fond "first bike" memories that will be treasured forever. The Candy sports bright colors and all the cool features that girls love - and best of all, parents will have peace of mind because they know the bike comes fully equipped with safety features. Removable training wheels, padded handlebar, rear coaster brake, easy step-through frame, and an enclosed chain are only the beginning to getting the kids off to a safe start. Pneumatic tires, front and rear reflectors and sporty fenders make this bike a very cool ride. For a custom fit that ensures proper posture, the padded seat and handlebar are adjustable. Let the quest for inspiring escapades begin. Nobodys tougher on bikes than kids, but the Candy is dependable, built to last, and easy to ride.','-vletplse',189.99,10,'5.98');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(40,33,0,'12-Inch Kettler Pulse Violet Girls Bike - Pink/Light Blue','The Pulse Violet is one stylish bike that is sure to spark fond "first bike" memories that will be treasured forever. The Violet has vibrant colors and all the cool features that little girls love - and best of all, parents will have peace of mind because they know the bike comes fully equipped with safety features. Removable training wheels, padded handlebar, rear coaster brake, durable low step-through frame and an enclosed chain are only the beginning to getting the kids off to a safe start. Pneumatic tires, front and rear reflectors, and sporty fenders make this bike a very cool ride. For a custom fit that ensures proper posture, the padded seat and handlebar are adjustable. Let the adventures begin! Nobodys tougher on bikes than kids, but the Violet is dependable, built to last, and easy to ride.','-cndyplse',149.99,10,'5.98');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(41,34,1,'','','',99.99,10,'2.98');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(42,35,1,'','','',149.99,10,'2.21');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(43,36,1,'','','',559.99,10,'2.23');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(44,37,1,'','','',459.99,10,'2.16');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(45,38,1,'','','',459.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(46,39,1,'15-Inch Kettler Ibiza bike','','-ktrib15',559.99,10,'2.55');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(47,39,0,'17-Inch Kettler Ibiza Pulse bike','','-ktrib17',559.99,10,'2.58');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(48,39,0,'19-Inch Kettler Ibiza Pulse bike','','-ktrib19',559.99,10,'2.98');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Colors,ColorSKUModifiers,Weight) values(49,40,1,'','','',50.49,10,'blue,green,red','-b,-g,-r','2.13');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(50,41,1,'','','',39.99,10,'2.13');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(51,42,1,'','','',39.99,10,'2.13');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(52,43,1,'','','',179.99,10,'2.13');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(53,44,1,'','','',169.99,10,'2.13');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(54,45,1,'','','',229.99,10,'2.13');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(55,46,1,'','','',89.99,10,'2.13');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(56,47,1,'','','',189.99,10,'2.13');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(57,48,1,'','','',504.99,15,'12.52');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(58,49,1,'','','',1699.99,15,'12.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory, Sizes, SizeSKUModifiers,Weight) values(59,50,1,'','','',79.99,15,'Twin Light Weight Comforters,Full/Queen Light Weight Comforter[99.99],King Light Weight Comforter[134.99]','-tlwc,-fqlwc,-klwc','4.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory, Sizes, SizeSKUModifiers,Weight) values(60,51,1,'','','',169.99,15,'Joseph Abboud Twin Comforter,Joseph Abboud Full/Queen Comforter[59.99],Joseph Abboud King Comforter[85.99]','-jatc,-jafqc,-jakc','4.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(61,52,1,'','','',129.99,15,'14.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(62,53,1,'','','',99.99,15,'14.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(63,54,1,'','','',319.99,15,'14.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(64,55,1,'','','',89.99,15,'14.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(65,56,1,'','','',119.99,15,'14.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(66,57,1,'','','',149.99,15,'14.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(67,58,1,'','','',89.99,15,'14.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(68,59,1,'','','',299.99,15,'14.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(69,60,1,'','','',6799.99,15,'.86');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(70,61,1,'','','',5999.99,15,'.86');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(71,62,1,'','','',4699.99,15,'.86');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(72,63,1,'','','',4499.99,15,'.86');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(73,64,1,'','','',3399.99,15,'.86');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(74,65,1,'','','',2999.99,15,'.86');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(75,66,1,'','','',2799.99,15,'.86');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(76,67,1,'','','',2499.99,15,'.86');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(77,68,1,'','','',199.99,15,'2.89');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(78,69,1,'','','',129.99,15,'2.89');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(79,70,1,'','','',139.99,15,'2.89');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(80,71,1,'','','',89.99,15,'2.89');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Dimensions,Inventory,IsRecurring,RecurringInterval,RecurringIntervalType,Weight) values(81,72,1,'','','',349.99,'54x34x31',15,1,1,2,'26.99');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Dimensions,Inventory,IsRecurring,RecurringInterval,RecurringIntervalType,Weight) values(82,73,1,'','','',439.99,'76x34x31',15,1,1,2,'26.99');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Dimensions,Inventory,IsRecurring,RecurringInterval,RecurringIntervalType,Weight) values(83,74,1,'','','',29.99,'22x22x22',15,1,1,2,'1.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Dimensions,Inventory,IsRecurring,RecurringInterval,RecurringIntervalType,Weight) values(84,75,1,'','','',64.99,'30x20x30',15,1,1,2,'2.01');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(85,76,1,'','','',139.99,15,'2.54');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(86,77,1,'','','',129.99,15,'2.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(87,78,1,'','','',99.97,15,'.91');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(88,79,1,'','','',99.97,15,'.91');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(89,80,1,'','','',49.97,15,'.91');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(90,81,1,'','','',19.99,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(91,82,1,'','','',18.59,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(92,83,1,'','','',17.69,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(93,84,1,'','','',17.39,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(94,85,1,'','','',16.79,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(95,86,1,'','','',16.59,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(96,87,1,'','','',16.59,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(97,88,1,'','','',14.99,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(98,89,1,'','','',14.39,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(99,90,1,'','','',14.39,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(100,91,1,'','','',14.29,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(101,92,1,'','','',12.99,15,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(102,93,1,'','','',999.99,15,'15.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(103,94,1,'','','',649.99,15,'15.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(104,95,1,'','','',649.99,15,'15.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(105,96,1,'','','',499.99,15,'15.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(106,97,1,'','','',329.99,15,'15.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(107,98,1,'','','',229.99,15,'15.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(108,99,1,'','','',99.99,15,'14.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(109,100,1,'','','',79.99,15,'15.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(110,101,1,'','','',67.99,15,'11.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(111,102,1,'','','',39.99,15,'15.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(112,103,1,'','','',39.99,15,'2.25');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(113,104,1,'','','',199.99,15,'5.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(114,105,1,'','','',59.99,15,'12.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory) values(115,106,1,'','','',129.90,15);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping) values(116,107,1,'','','',79.99,15,1);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping) values(117,108,1,'','','',79.99,15,1);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping) values(118,109,1,'','','',79.99,15,1);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping) values(119,110,1,'','','',79.99,15,1);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping) values(120,111,1,'','','',79.99,15,1);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping) values(121,112,1,'','','',239.95,15,1);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping) values(122,113,1,'','','',79.99,15,1);
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(123,114,1,'','','',399.99,10,'9.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(124,115,1,'','','',999.99,10,'21.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(125,116,1,'','','',999.99,10,'21.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(126,117,1,'','','',2499.99,10,'21.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(127,118,1,'','','',279.99,10,'4.45');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(128,119,1,'','','',84.99,10,'4.45');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(129,120,1,'','','',74.99,10,'4.45');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(130,121,1,'','','',199.99,10,'2.51');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(131,122,1,'','','',107.99,10,'2.51');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(132,123,1,'','','',159.99,10,'2.51');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(133,124,1,'','','',299.99,10,'2.51');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(134,125,1,'','','',159.99,10,'2.51');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(135,126,1,'','','',149.99,10,'2.51');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(136,127,1,'','','',259.99,10,'15.52');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(137,128,1,'','','',349.99,10,'15.52');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(138,129,1,'','','',299.99,10,'15.52');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(139,130,1,'','','',189.99,10,'15.52');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(140,131,1,'','','',239.99,10,'15.52');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(141,132,1,'','','',239.99,10,'15.52');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(142,133,1,'','','',26.99,10,'.90');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(143,134,1,'','','',26.99,10,'.90');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(144,135,1,'','','',26.99,10,'.90');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(145,136,1,'','','',26.99,10,'.90');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(146,137,1,'','','',19.49,10,'.90');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(147,138,1,'','','',21.69,10,'.90');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(148,139,1,'','','',19.49,10,'.90');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(149,140,1,'','','',12.99,10,'.90');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(150,141,1,'','','',18.79,10,'.90');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(151,142,1,'','','',18.99,10,'.90');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(152,143,1,'','','',159.99,10,'5.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(153,144,1,'','','',159.99,10,'5.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(154,145,1,'','','',69.99,10,'5.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(155,146,1,'','','',129.99,10,'5.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(156,147,1,'','','',349.99,10,'5.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(157,148,1,'','','',94.99,10,'5.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(158,149,1,'','','',74.99,10,'5.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(159,150,1,'','','',69.99,10,'5.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(160,151,1,'','','',59.99,10,'5.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(161,152,1,'','','',39.99,10,'5.56');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(162,153,1,'','','',89.99,10,'2.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(163,154,1,'','','',64.99,10,'2.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping,Weight) values(164,155,1,'','','',54.99,10,2,'2.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping,Weight) values(165,156,1,'','','',54.99,10,2,'2.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping,Weight) values(166,157,1,'','','',49.99,10,2,'2.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping,Weight) values(167,158,1,'','','',49.98,10,2,'2.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping,Weight) values(168,159,1,'','','',46.99,10,2,'2.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,FreeShipping,Weight) values(169,160,1,'','','',37.79,10,2,'2.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(170,161,1,'','','',34.99,10,'2.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(171,162,1,'','','',34.79,10,'2.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(172,163,1,'','','',31.99,10,'2.50');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(173,164,1,'','','',219.99,10,'7.46');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(174,165,1,'','','',159.99,10,'7.46');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(175,166,1,'','','',99.99,10,'7.46');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(195,186,1,'','','',314.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(196,187,1,'','','',289.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(197,188,1,'','','',259.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(198,189,1,'','','',169.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(199,190,1,'','','',149.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(200,191,1,'','','',134.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(201,192,1,'','','',124.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(202,193,1,'','','',119.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(203,194,1,'','','',119.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(204,195,1,'','','',99.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(205,196,1,'','','',99.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(206,197,1,'','','',89.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(207,198,1,'','','',89.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(208,199,1,'','','',69.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,Weight) values(209,200,1,'','','',26.99,10,'2.15');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(210,201,1,'In The Ayer','Music for Flo-Rida ft. Will-I-Am and Fergie','-ndayer',5.99,10,1,'OrderDownloads/InTheAyer.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(211,201,0,'Superhuman','Music from Chris Brown ft. Keri Hilson','-suphuman',5.99,10,1,'OrderDownloads/Superhuman.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(212,201,0,'St Anger','Music from Metallica','-anger',5.99,10,1,'OrderDownloads/StAnger.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(213,201,0,'Hands Down','Music from Dashboard Confessional','-hndsdwn',5.99,10,1,'OrderDownloads/HandsDown.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(214,201,0,'Vindicated','Music from Dashboard Confessional','-suphuman',5.99,10,1,'OrderDownloads/Vindicated.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(215,201,0,'Officially Missing You','Music from Tamia','-offmissu',5.99,10,1,'OrderDownloads/Officially.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(216,201,0,'Youth Of the Nation','Music from P.O.D.','-ythofnon',5.99,10,1,'OrderDownloads/Youth.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(217,201,0,'High','Music from LightHouse Family','-highlh',5.99,10,1,'OrderDownloads/High.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(218,201,0,'Between Angels & Devils','Music from Papa Roach','-bangdev',5.99,10,1,'OrderDownloads/BetweenAngelsDevils.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(219,201,0,'Ill be Missing You','Music from Puff Daddy','-pdad',5.99,10,1,'OrderDownloads/MissingU.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(220,201,0,'All Apologies','Music from Nirvana','-allapo',5.99,10,1,'OrderDownloads/AllApologies.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(221,201,0,'Young Modern Station','Music from Silverchair','-yngms',5.99,10,1,'OrderDownloads/ModernStation.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(222,201,0,'Straight Lines','Music from Silverchair','-strlnes',5.99,10,1,'OrderDownloads/StraightLines.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(223,201,0,'Insomia','Music from Silverchair','-ins',5.99,10,1,'OrderDownloads/Insomia.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(224,201,0,'All Across the World','Music from Silverchair','-accwrld',5.99,10,1,'OrderDownloads/AllAcrossDWorld.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(225,201,0,'Low','Music from Silverchair','-lowsc',5.99,10,1,'OrderDownloads/Low.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(226,201,0,'Bleeding Love','Music from Leona Lewis','-bl',5.99,10,1,'OrderDownloads/BleedingLove.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(227,201,0,'Face to Face','Music from Sevendust','-ftof',5.99,10,1,'OrderDownloads/FacetoFace.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(228,201,0,'Licking Cream','Music from Sevendust','-lcrea',5.99,10,1,'OrderDownloads/LickingCream.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(229,201,0,'Ugly','Music from Sevendust','-uglsd',5.99,10,1,'OrderDownloads/Ugly.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(230,201,0,'Everything','Music from Lifehouse','-evthlh',5.99,10,1,'OrderDownloads/Everything.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(231,201,0,'Hanging by a Moment','Music from Lifehouse','-hbamntlf',5.99,10,1,'OrderDownloads/HangingMoment.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(232,201,0,'Unknown','Music from Lifehouse','-unklh',5.99,10,1,'OrderDownloads/Unknown.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(233,201,0,'Break Me Shake Me','Music from Savage Garden','-bsmesg',5.99,10,1,'OrderDownloads/BreakShake.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(234,201,0,'Crash And Burn','Music from Savage Garden','-cbsg',5.99,10,1,'OrderDownloads/CrashBurn.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(235,201,0,'I Knew I loved you','Music from Savage Garden','-iklusg',5.99,10,1,'OrderDownloads/IKnewILovedYou.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(236,201,0,'I Want You','Music from Savage Garden','-mscsg',5.99,10,1,'OrderDownloads/IWantU.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(237,201,0,'The Animal Song','Music from Savage Garden','-asngsg',5.99,10,1,'OrderDownloads/DAnimalSong.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(238,201,0,'Truly Madly Deeply','Music from Savage Garden','-tmaddep',5.99,10,1,'OrderDownloads/TrulyMadlyDeeply.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(239,201,0,'Passenger Seat','Music from Stephen Speaks','-pseat',5.99,10,1,'OrderDownloads/PassengerSeat.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(240,201,0,'Out Of My League','Music from Stephen Speaks','-outleag',5.99,10,1,'OrderDownloads/OutOfMyLeague.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(241,201,0,'Crawling In The Dark','Music from Hoobastank','-crandrk',5.99,10,1,'OrderDownloads/CrawlingNDDark.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(242,201,0,'Out Of Control','Music from Hoobastank','-otofcnt',5.99,10,1,'OrderDownloads/OutOfControl.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(243,201,0,'Pieces','Music from Hoobastank','-pis',5.99,10,1,'OrderDownloads/Pieces.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(244,201,0,'Running Away','Music from Hoobastank','-runwy',5.99,10,1,'OrderDownloads/RunningAway.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(245,201,0,'The Reason Is You','Music from Hoobastank','-resisu',5.99,10,1,'OrderDownloads/ReasonIsYou.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(246,201,0,'With You','Music from Linkin Park','-witu',5.99,10,1,'OrderDownloads/WithYou.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(247,201,0,'Somewhere I Belong','Music from Linkin Park','-somibel',5.99,10,1,'OrderDownloads/SomewherIBelong.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(248,201,0,'In The End','Music from Linkin Park','-ndend',5.99,10,1,'OrderDownloads/InTheEnd.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(249,201,0,'Faint','Music from Linkin Park','-fnt',5.99,10,1,'OrderDownloads/Faint.mp3');
INSERT [dbo].ProductVariant(VariantID,ProductID,IsDefault,Name,Description,SKUSuffix,Price,Inventory,IsDownload,DownloadLocation) values(250,201,0,'One Step Closer','Music from Linkin Park','-onestpcls',5.99,10,1,'OrderDownloads/OneStepCloserf.mp3');
set IDENTITY_INSERT [dbo].ProductVariant OFF;
--Colors
--ColorSKUModifiers
--Sizes
--SizeSKUModifiers

-- set default packages:
update [dbo].category set XmlPackage='entity.grid.xml.config' where XmlPackage IS NULL or XmlPackage='';
go
update [dbo].section set XmlPackage='entity.grid.xml.config' where XmlPackage IS NULL or XmlPackage='';
go
update [dbo].manufacturer set XmlPackage='entity.grid.xml.config' where XmlPackage IS NULL or XmlPackage='';
go
update [dbo].distributor set XmlPackage='entity.grid.xml.config' where XmlPackage IS NULL or XmlPackage='';
go
update [dbo].product set XmlPackage='product.variantsinrightbar.xml.config' where IsAKit=0 and XmlPackage IS NULL or XmlPackage='';
go
update Product set XmlPackage='product.variantsingrid.xml.config' where ProductID=4
go
update Product set XmlPackage='product.variantsingrid.xml.config' where ProductID=21
go
update Product set XmlPackage='product.variantsingrid.xml.config' where ProductID=33
go
update Product set XmlPackage='product.variantsingrid.xml.config' where ProductID=39
go
update Product set XmlPackage='product.kitproduct.xml.config' where IsAKit=1 and XmlPackage IS NULL or XmlPackage='';
go

--Related Products
UPDATE [dbo].Product SET RelatedProducts = '1,2,3,14' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 1)
UPDATE [dbo].Product SET RelatedProducts = '4,5,6,7,8,9,10,11,12,13' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 2 or CategoryID = 3)
UPDATE [dbo].Product SET RelatedProducts = '18,19,20,21' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 6 or GenreID = 7)
UPDATE [dbo].Product SET RelatedProducts = '22,23,24,25,26,27,28,29,30' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 8 or GenreID = 9 or GenreID = 10)
UPDATE [dbo].Product SET RelatedProducts = '31,32,33,34,35' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 11)
UPDATE [dbo].Product SET RelatedProducts = '36,37,38,39' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 12)
UPDATE [dbo].Product SET RelatedProducts = '40,41,42,44,45,46,47' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 9)
UPDATE [dbo].Product SET RelatedProducts = '48,49' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 14)
UPDATE [dbo].Product SET RelatedProducts = '50,51' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 15)
UPDATE [dbo].Product SET RelatedProducts = '52,53,54,55,56,57,58,59' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 12)
UPDATE [dbo].Product SET RelatedProducts = '60,61,62,63,64,65,66,67' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 16)
UPDATE [dbo].Product SET RelatedProducts = '68,69,70,71' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 17)
UPDATE [dbo].Product SET RelatedProducts = '72,73' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 18)
UPDATE [dbo].Product SET RelatedProducts = '74,75' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 19)
UPDATE [dbo].Product SET RelatedProducts = '76,77' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 20)
UPDATE [dbo].Product SET RelatedProducts = '78,79,80' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 16)
UPDATE [dbo].Product SET RelatedProducts = '81,82,83,84,85,86,87,88,89,90,91,92' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 17) and ProductID != 114
UPDATE [dbo].Product SET RelatedProducts = '93,94,95,96,97,98,99,100,101,102,103,104,105,114' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 18)
UPDATE [dbo].Product SET RelatedProducts = '106,107,108,109,110,111,112,113' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 19)
UPDATE [dbo].Product SET RelatedProducts = '115,116,117' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 21)
UPDATE [dbo].Product SET RelatedProducts = '118,119,120' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 22 or GenreID = 23)
UPDATE [dbo].Product SET RelatedProducts = '121,122,123,124,125,126' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 24)
UPDATE [dbo].Product SET RelatedProducts = '127,128,129,130,131,132' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 25)
UPDATE [dbo].Product SET RelatedProducts = '133,134,135,136,137,138,139,140,141,142' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 24)
UPDATE [dbo].Product SET RelatedProducts = '143,142,145,146' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 26)
UPDATE [dbo].Product SET RelatedProducts = '147,148,149,150,151,152' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 27)
UPDATE [dbo].Product SET RelatedProducts = '153,154,155,156,157,158,159,160,161,162,163' WHERE ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 28)
UPDATE [dbo].Product SET RelatedProducts = '164,165,166,167,168,169' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 28)
UPDATE [dbo].Product SET RelatedProducts = '170,171,172,173,174,175' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 29)
UPDATE [dbo].Product SET RelatedProducts = '186,187,188,189,190,191,192,193,194,195,196,197,198,199,200' WHERE ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 31)

--_Upsell Products
UPDATE [dbo].Product Set UpsellProducts = '1,2,3,118,119,120' Where ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 21)
UPDATE [dbo].Product Set UpsellProducts = '52,53,54,55,56,121,122,123' where ProductID in (48,49)
UPDATE [dbo].Product Set UpsellProducts = '133,134,135,136,137,138,139,140,141,142' where ProductID in (118)
UPDATE [dbo].Product Set UpsellProducts = '137,138,139,140,141,142' where ProductID in (119,120)
UPDATE [dbo].Product Set UpsellProducts = '171,172,173,174' where ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 27)
UPDATE [dbo].Product Set UpsellProducts = '127,128,129,130,131,132' Where ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 11)
UPDATE [dbo].Product Set UpsellProducts = '48,49' where ProductID in (select ProductID from [dbo].ProductCategory where CategoryID = 12)
UPDATE [dbo].Product Set UpsellProducts = '76,77' where ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 17)
UPDATE [dbo].Product Set UpsellProducts = '68,69,70,71' where ProductID in (select ProductID from [dbo].ProductGenre where GenreID = 20)
UPDATE [dbo].Product Set UpsellProducts = '72,73,74,75' where ProductID in (93,94,95,96,97,98)

--_Shipping Zone
SET IDENTITY_INSERT [dbo].ShippingZone ON;
INSERT [dbo].ShippingZone(ShippingZoneID, Name, ZipCodes, DisplayOrder, CreatedOn)
VALUES(1, 'Zones 1 & 2', '894-897,940-941,942,943-955,956-959,960-966,987', 1, GETDATE())
SET IDENTITY_INSERT [dbo].ShippingZone OFF;
GO


SET IDENTITY_INSERT [dbo].ShippingZone ON;
INSERT [dbo].ShippingZone(ShippingZoneID, Name, ZipCodes, DisplayOrder, CreatedOn)
VALUES(2, 'Zone 3', '932-933,936-939,975-976', 2, GETDATE())
SET IDENTITY_INSERT [dbo].ShippingZone OFF;
GO


SET IDENTITY_INSERT [dbo].ShippingZone ON;
INSERT [dbo].ShippingZone(ShippingZoneID, Name, ZipCodes, DisplayOrder, CreatedOn)
VALUES(3, 'Zone 4', '832-837,840-847,854,860-864,889-893,898-931,934-935,970-974,977-986,988-989,993-994', 3, GETDATE())
SET IDENTITY_INSERT [dbo].ShippingZone OFF;
GO


SET IDENTITY_INSERT [dbo].ShippingZone ON;
INSERT [dbo].ShippingZone(ShippingZoneID, Name, ZipCodes, DisplayOrder, CreatedOn)
VALUES(4, 'Zone 5', '590-599,693,798-831,838,850-853,865-880,883,885,990-992', 4, GETDATE())
SET IDENTITY_INSERT [dbo].ShippingZone OFF;
GO


SET IDENTITY_INSERT [dbo].ShippingZone ON;
INSERT [dbo].ShippingZone(ShippingZoneID, Name, ZipCodes, DisplayOrder, CreatedOn)
VALUES(5, 'Zone 6', '510-513,515-516,562,565,567-576,580-588,664-666,668-692,730-732,734-739,746,748,763,768-769,790-797,881-882,884,999', 5, GETDATE())
SET IDENTITY_INSERT [dbo].ShippingZone OFF;
GO


SET IDENTITY_INSERT [dbo].ShippingZone ON;
INSERT [dbo].ShippingZone(ShippingZoneID, Name, ZipCodes, DisplayOrder, CreatedOn)
VALUES(6, 'Zone 7', '375,380-382,386-387,389,420,463-464,498-509,514,520-561,563-564,566,600-663,667,705-706,710-729,733,740-745,747,749-762,764-767,770-789,998', 6, GETDATE())
SET IDENTITY_INSERT [dbo].ShippingZone OFF;
GO


SET IDENTITY_INSERT [dbo].ShippingZone ON;
INSERT [dbo].ShippingZone(ShippingZoneID, Name, ZipCodes, DisplayOrder, CreatedOn)
VALUES(7, 'Zone 8', '004-374,376-379,383-385,388,390-418,421-462,465-497,700-704,707-709,967-969,995-997', 7, GETDATE())
SET IDENTITY_INSERT [dbo].ShippingZone OFF;
GO

--_Shipping Method
SET IDENTITY_INSERT [dbo].ShippingMethod ON;
INSERT [dbo].ShippingMethod(ShippingMethodID, ShippingMethodGUID, Name, IsRTShipping, DisplayOrder, CreatedOn)
VALUES (1, NEWID(), 'Express Mail - Retail', 0, 1, GETDATE());

INSERT [dbo].ShippingMethod(ShippingMethodID, ShippingMethodGUID, Name, IsRTShipping, DisplayOrder, CreatedOn)
VALUES (2, NEWID(), 'Priority Mail - Retail', 0, 2, GETDATE());

INSERT [dbo].ShippingMethod(ShippingMethodID, ShippingMethodGUID, Name, IsRTShipping, DisplayOrder, CreatedOn)
VALUES (3, NEWID(), 'Parcel Select', 0, 3, GETDATE());

INSERT [dbo].ShippingMethod(ShippingMethodID, ShippingMethodGUID, Name, IsRTShipping, DisplayOrder, CreatedOn)
VALUES (4, NEWID(), 'Parcel Select BMC and OBMC Presort', 0, 4, GETDATE());

SET IDENTITY_INSERT [dbo].ShippingMethod ON;
INSERT [dbo].ShippingMethod(ShippingMethodID, ShippingMethodGUID, Name, IsRTShipping, DisplayOrder, CreatedOn)
VALUES (5, NEWID(), 'Express Mail International', 0, 1, GETDATE())

INSERT [dbo].ShippingMethod(ShippingMethodID, ShippingMethodGUID, Name, IsRTShipping, DisplayOrder, CreatedOn)
VALUES (6, NEWID(), 'Priority Mail International', 0, 2, GETDATE())
SET IDENTITY_INSERT [dbo].ShippingZone OFF;
GO
SET IDENTITY_INSERT [dbo].ShippingMethod OFF;
GO

/* ACTIVE RATE TABLE FOR: CALCULATE SHIPPING BY ORDER WEIGHT BY ZONE: */

/*Weight
Not Over
(pounds) 0.5*/


INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(1, 0.01, 0.50, 1, 12.60, GETDATE())

INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(1, 0.51, 1.00, 1, 14.55, GETDATE())

INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(1, 1.01, 2.00, 1, 15.70, GETDATE())

INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(1, 3.01, 4.00, 1, 17.95, GETDATE())

INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(1, 4.01, 5.50, 1, 18.60, GETDATE())

INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(1, 5.01, 6.00, 1, 21.85, GETDATE())

INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(1, 6.01, 7.00, 1, 25.10, GETDATE())  

INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(1, 7.01, 8.00, 1, 26.35, GETDATE())  

INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(1, 8.01, 9.00, 1, 27.80, GETDATE())  

INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(1, 9.01, 10.00, 1, 28.60, GETDATE())

--JOSEPH ERICKSON VILLAR
DECLARE @ROWGUID uniqueidentifier

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.01 AND HighValue = 0.50)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.01 AND HighValue = 0.50)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.01, 0.50, 2, 14.65, GETDATE())	
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.01 AND HighValue = 0.50)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.01, 0.50, 3, 17.45, GETDATE())	
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.01 AND HighValue = 0.50)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.01, 0.50, 4, 18.30, GETDATE())	
	END
 
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.01 AND HighValue = 0.50)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.01, 0.50, 5, 18.60, GETDATE())	
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.01 AND HighValue = 0.50)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.01, 0.50, 6, 19.25, GETDATE())	
	END 

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.01 AND HighValue = 0.50)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.01, 0.50, 7, 19.50, GETDATE())	
	END  


/*Weight
Not Over
(pounds) 1*/

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.51 AND HighValue = 1.00)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.51 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.51, 1.00, 2, 19.00, GETDATE())	
	END  

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.51 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.51, 1.00, 3, 22.40, GETDATE())	
	END 

 IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.51 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.51, 1.00, 4, 22.65, GETDATE())	
	END 

 IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.51 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.51, 1.00, 5, 22.90, GETDATE())	
	END  

 IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.51 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.51, 1.00, 6, 23.15, GETDATE())	
	END  
 
 IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.51 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.51, 1.00, 7, 23.40, GETDATE())	
	END  
 

/*Weight
Not Over
(pounds) 2*/

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 1.01 AND HighValue = 2.00)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID,ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 1.01, 2.00, 2, 20.15, GETDATE())
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 1.01, 2.00, 3, 24.65, GETDATE())
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 1.01, 2.00, 4, 24.90, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 1.01, 2.00, 5, 25.15, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 1.01, 2.00, 6, 25.40, GETDATE())
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 1.01, 2.00, 7, 25.65, GETDATE())	
	END
	


/*Weight
Not Over
(pounds) 3*/

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 2.01 AND HighValue = 3.00)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 2.01, 3.00, 2, 21.35, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 2.01, 3.00, 3, 28.40, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 2.01, 3.00, 4, 28.65, GETDATE())
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 2.01, 3.00, 5, 28.90, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 2.01, 3.00, 6, 29.15, GETDATE())
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 2.01, 3.00, 7, 29.40, GETDATE())
	END


/*Weight
Not Over
(pounds) 4*/

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 3.01 AND HighValue = 4.00)
 
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 3.01, 4.00, 2, 22.75, GETDATE())
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 3.01, 4.00, 3, 32.10, GETDATE())
	END


IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 3.01, 4.00, 4, 32.35, GETDATE())
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 3.01, 4.00, 5, 32.60, GETDATE())
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 3.01, 4.00, 6, 32.85, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 3.01, 4.00, 7, 33.10, GETDATE())
	END


/*Weight
Not Over
(pounds) 5*/

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 4.01 AND HighValue = 5.00)
 
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 4.01, 5.00, 2, 24.35, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 4.01, 5.00, 3, 35.85, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 4.01, 5.00, 4, 36.10, GETDATE())	
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 4.01, 5.00, 5, 36.35, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 4.01, 5.00, 6, 36.60, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 4.01, 5.00, 7, 36.85, GETDATE())
	END

  
/*Weight
Not Over
(pounds) 6*/
 
SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 5.01 AND HighValue = 6.00)
      
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 5.01, 6.00, 2, 29.25, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 5.01, 6.00, 3, 39.55, GETDATE())
	END
	

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 5.01, 6.00, 4, 39.80, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 5.01, 6.00, 5, 40.05, GETDATE())
	END


IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 5.01, 6.00, 6, 40.30, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 5.01, 6.00, 6, 40.30, GETDATE())	
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 5.01, 6.00, 7, 40.55, GETDATE())
	END
	

/*Weight
Not Over
(pounds) 7*/
   
SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 6.01 AND HighValue = 7.00)
     
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 6.01, 7.00, 2, 34.15, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 6.01, 7.00, 3, 43.25, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 6.01, 7.00, 4, 43.50, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 6.01, 7.00, 5, 43.75, GETDATE())
	END


IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 6.01, 7.00, 6, 44.00, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 6.01, 7.00, 7, 44.25, GETDATE())
	END


/*Weight
Not Over
(pounds) 8*/
 
SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 7.01 AND HighValue = 8.00)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 7.01, 8.00, 2, 35.15, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID,ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 7.01, 8.00, 3, 47.00, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 7.01, 8.00, 4, 47.25, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 7.01, 8.00, 5, 47.50, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 7.01, 8.00, 5, 47.50, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 7.01, 8.00, 6, 47.75, GETDATE())
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 7.01, 8.00, 7, 48.00, GETDATE())
	END
       

/*Weight
Not Over
(pounds) 9*/
         
SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 8.01 AND HighValue = 9.00)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 8.01, 9.00, 2, 36.65, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 8.01, 9.00, 3, 50.35, GETDATE())
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 8.01, 9.00, 4, 50.95, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 8.01, 9.00, 5, 51.20, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 8.01, 9.00, 6, 51.45, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 8.01, 9.00, 7, 51.70, GETDATE())
	END



/*Weight
Not Over
(pounds) 10*/
 
         
SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 9.01 AND HighValue = 10.00)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN		
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 9.01, 10.00, 2, 38.10, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 9.01, 10.00, 3, 52.70, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 9.01, 10.00, 4, 53.55, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 9.01, 10.00, 5, 53.80, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 9.01, 10.00, 6, 54.05, GETDATE())
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 9.01, 10.00, 7, 54.30, GETDATE())
	END
	



--_Shipping Weight By Zone 3 & 4
--Parcel Select
--1 pound
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(3, 0.01, 1, 1, 2.52, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 0.01 AND HighValue = 1)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 0.01 AND HighValue = 1)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 0.01, 1, 2, 2.94, GETDATE());		
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)	
	VALUES(@ROWGUID, 3, 0.01, 1, 3, 3.29, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 0.01, 1, 4, 4.22, GETDATE());	
END


--2 pounds
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(3, 1.01, 2, 1, 2.83, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 1.01 AND HighValue = 2)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 1.01 AND HighValue = 2)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 1.01, 2, 2, 3.60, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 1.01, 2, 3, 4.29, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 1.01, 2, 4, 5.02, GETDATE());		
END

--3 pounds
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(3, 2.01, 3, 1, 3.14, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 2.01 AND HighValue = 3)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 2.01 AND HighValue = 3)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 2.01, 3, 2, 4.27, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 2.01, 3, 3, 5.24, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 2.01, 3, 4, 5.85, GETDATE());		
END

--4 pounds
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(3, 3.01, 4, 1, 3.43, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 3.01 AND HighValue = 4)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 3.01 AND HighValue = 4)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 3.01, 4, 2, 4.87, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 3.01, 4, 3, 6.01, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 3.01, 4, 4, 6.55, GETDATE());		
END

--5 pounds
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(3, 4.01, 5, 1, 3.69, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 4.01 AND HighValue = 5)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 4.01 AND HighValue = 5)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 4.01, 5, 2, 5.45, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 4.01, 5, 3, 6.58, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 4.01, 5, 4, 7.24, GETDATE());		
END

--6 pounds
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(3, 5.01, 6, 1, 3.95, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 5.01 AND HighValue = 6)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 5.01 AND HighValue = 6)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 5.01, 6, 2, 5.97, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 5.01, 6, 3, 7.04, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 5.01, 6, 4, 7.84, GETDATE());		
END

--7 pounds
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(3, 6.01, 7, 1, 4.19, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 6.01 AND HighValue = 7)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 6.01 AND HighValue = 7)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 6.01, 7, 2, 6.48, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 6.01, 7, 3, 7.49, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 6.01, 7, 4, 8.45, GETDATE());		
END

--8 pounds
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(3, 7.01, 8, 1, 4.44, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 7.01 AND HighValue = 8)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 7.01 AND HighValue = 8)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 7.01, 8, 2, 6.97, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 7.01, 8, 3, 7.89, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 7.01, 8, 4, 8.99, GETDATE());		
END

--9 pounds
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(3, 8.01, 9, 1, 4.64, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 8.01 AND HighValue = 9)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 8.01 AND HighValue = 9)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 8.01, 9, 2, 7.38, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 8.01, 9, 3, 8.26, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 8.01, 9, 4, 9.43, GETDATE());		
END

--10 pounds
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(3, 9.01, 10, 1, 4.85, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 9.01 AND HighValue = 10)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 3 AND LowValue = 9.01 AND HighValue = 10)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 9.01, 10, 2, 7.81, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 9.01, 10, 3, 9.15, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 3, 9.01, 10, 4, 9.88, GETDATE());		
END

--1 pound
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(4, 0.01, 1, 1, 4.55, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 0.01 AND HighValue = 1)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 0.01 AND HighValue = 1)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 0.01, 1, 2, 4.55, GETDATE());		
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)	
	VALUES(@ROWGUID, 4, 0.01, 1, 3, 4.55, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 0.01, 1, 4, 4.55, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 0.01, 1, 5, 4.55, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 0.01, 1, 6, 4.55, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 0.01, 1, 7, 4.55, GETDATE());		
END

--2 pounds
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(4, 1.01, 2, 1, 4.55, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 1.01 AND HighValue = 2)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 1.01 AND HighValue = 2)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 1.01, 2, 2, 4.85, GETDATE());		
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)	
	VALUES(@ROWGUID, 4, 1.01, 2, 3, 5.35, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 1.01, 2, 4, 5.94, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 1.01, 2, 5, 6.13, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 1.01, 2, 6, 6.35, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 1.01, 2, 7, 6.67, GETDATE());		
END

--3 pound
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(4, 2.01, 3, 1, 5.05, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 2.01 AND HighValue = 3)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 2.01 AND HighValue = 3)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 2.01, 3, 2, 5.70, GETDATE());		
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)	
	VALUES(@ROWGUID, 4, 2.01, 3, 3, 6.60, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 2.01, 3, 4, 6.94, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 2.01, 3, 5, 7.22, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 2.01, 3, 6, 7.52, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 2.01, 3, 7, 8.12, GETDATE());		
END

--4 pound
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(4, 3.01, 4, 1, 5.75, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 3.01 AND HighValue = 4)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 3.01 AND HighValue = 4)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 3.01, 4, 2, 6.75, GETDATE());		
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)	
	VALUES(@ROWGUID, 4, 3.01, 4, 3, 7.55, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 3.01, 4, 4, 7.88, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 3.01, 4, 5, 8.23, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 3.01, 4, 6, 8.62, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 3.01, 4, 7, 9.38, GETDATE());		
END

--5 pound
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(4, 4.01, 5, 1, 6.40, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 4.01 AND HighValue = 5)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 4.01 AND HighValue = 5)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 4.01, 5, 2, 7.70, GETDATE());		
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)	
	VALUES(@ROWGUID, 4, 4.01, 5, 3, 8.37, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 4.01, 5, 4, 8.76, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 4.01, 5, 5, 9.19, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 4.01, 5, 6, 9.67, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 4.01, 5, 7, 10.58, GETDATE());		
END

--6 pound
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(4, 5.01, 6, 1, 7.00, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 5.01 AND HighValue = 6)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 5.01 AND HighValue = 6)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 5.01, 6, 2, 8.60, GETDATE());		
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)	
	VALUES(@ROWGUID, 4, 5.01, 6, 3, 9.15, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 5.01, 6, 4, 9.61, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 5.01, 6, 5, 10.11, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 5.01, 6, 6, 10.66, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 5.01, 6, 7, 11.72, GETDATE());		
END

--7 pound
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(4, 6.01, 7, 1, 7.55, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 6.01 AND HighValue = 7)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 6.01 AND HighValue = 7)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 6.01, 7, 2, 9.34, GETDATE());		
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)	
	VALUES(@ROWGUID, 4, 6.01, 7, 3, 9.89, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 6.01, 7, 4, 10.42, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 6.01, 7, 5, 10.98, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 6.01, 7, 6, 11.60, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 6.01, 7, 7, 12.81, GETDATE());		
END

--8 pound
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(4, 7.01, 8, 1, 8.00, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 7.01 AND HighValue = 8)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 7.01 AND HighValue = 8)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 7.01, 8, 2, 9.70, GETDATE());		
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)	
	VALUES(@ROWGUID, 4, 7.01, 8, 3, 10.61, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 7.01, 8, 4, 11.19, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 7.01, 8, 5, 11.82, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 7.01, 8, 6, 12.51, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 7.01, 8, 7, 13.85, GETDATE());		
END

--9 pound
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(4, 8.01, 9, 1, 8.40, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 8.01 AND HighValue = 9)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 8.01 AND HighValue = 9)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 8.01, 9, 2, 10.06, GETDATE());		
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)	
	VALUES(@ROWGUID, 4, 8.01, 9, 3, 11.30, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 8.01, 9, 4, 11.94, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 8.01, 9, 5, 12.63, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 8.01, 9, 6, 13.39, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 8.01, 9, 7, 14.86, GETDATE());		
END

--10 pound
INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(4, 9.01, 10, 1, 8.80, GETDATE())

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 9.01 AND HighValue = 10)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 4 AND LowValue = 9.01 AND HighValue = 10)
BEGIN
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 9.01, 10, 2, 11.20, GETDATE());		
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)	
	VALUES(@ROWGUID, 4, 9.01, 10, 3, 11.96, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 9.01, 10, 4, 12.66, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 9.01, 10, 5, 13.40, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 9.01, 10, 6, 14.23, GETDATE());	
	INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
	VALUES(@ROWGUID, 4, 9.01, 10, 7, 12.83, GETDATE());		
END

/*Weight
Not Over
(pounds) 1*/

INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(2, 0.01, 1.00, 1, 4.80, GETDATE())


INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(2, 1.01, 2.00, 1, 4.80, GETDATE())


INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(2, 2.01, 3.00, 1, 5.20, GETDATE())


INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(2, 3.01, 4.00, 1, 5.80, GETDATE())


INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(2, 4.01, 5.00, 1, 6.45, GETDATE())


INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(2, 5.01, 6.00, 1, 7.05, GETDATE())


INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(2, 6.01, 7.00, 1, 7.60, GETDATE())


INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(2, 7.01, 8.00, 1, 8.05, GETDATE())



INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(2, 8.01, 9.00, 1, 8.45, GETDATE())



INSERT [dbo].ShippingWeightByZone(ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
VALUES(2, 9.01, 10.00, 1, 8.85, GETDATE())  

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 0.01 AND HighValue = 1.00)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 0.01 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 0.01, 1.00, 2, 4.80, GETDATE())	
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.01 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 1, 0.01, 1.00, 3, 4.80, GETDATE())	
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 1 AND LowValue = 0.01 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 0.01, 1.00, 4, 4.80, GETDATE())	
	END
 
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 0.01 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 0.01, 1.00, 5, 4.80, GETDATE())	
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 0.01 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 0.01, 1.00, 6, 4.80, GETDATE())	
	END 

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 0.01 AND HighValue = 1.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 0.01, 1.00, 7, 4.80, GETDATE())	
	END  

/*Weight
Not Over
(pounds) 2*/

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 1.01 AND HighValue = 2.00)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 1.01, 2.00, 2,  5.05, GETDATE())	
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 1.01, 2.00, 3,  5.60, GETDATE())	
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 1.01, 2.00, 4,  6.80, GETDATE())	
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 1.01, 2.00, 5,  7.20, GETDATE())	
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 1.01, 2.00, 6,  7.70, GETDATE())	
	END	
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 1.01 AND HighValue = 2.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 1.01, 2.00, 7,  8.25, GETDATE())	
	END	
   
/*Weight
Not Over
(pounds) 3*/
 
SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 2.01 AND HighValue = 3.00)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 2.01, 3.00, 2,  5.95, GETDATE())	
	END

 IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 2.01, 3.00, 3,  6.75, GETDATE())	
	END
	
 IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 2.01, 3.00, 4,  8.75, GETDATE())	
	END
	
 IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 2.01, 3.00, 5,  9.55, GETDATE())	
	END
	
 IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 2.01, 3.00, 6,  10.35, GETDATE())	
	END
	
 IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 2.01 AND HighValue = 3.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 2.01, 3.00, 7,  11.50, GETDATE())	
	END

/*Weight
Not Over
(pounds) 4*/		
       
SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 3.01 AND HighValue = 4.00)   

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 3.01, 4.00, 2,  6.80, GETDATE())	
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 3.01, 4.00, 3,  7.85, GETDATE())	
	END
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 3.01, 4.00, 4,  10.55, GETDATE())	
	END

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 3.01, 4.00, 5,  11.60, GETDATE())	
	END
 
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 3.01, 4.00, 6,  12.65, GETDATE())	
	END 

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 3.01 AND HighValue = 4.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 3.01, 4.00, 7,  14.25, GETDATE())	
	END 

/*Weight
Not Over
(pounds) 5*/     

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 4.01 AND HighValue = 5.00)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 4.01, 5.00, 2,  6.80, GETDATE())	
	END      

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 4.01, 5.00, 3,  8.90, GETDATE())	
	END 

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 4.01, 5.00, 4,  12.20, GETDATE())	
	END 

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 4.01, 5.00, 5,  13.45, GETDATE())	
	END 

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 4.01, 5.00, 6,  14.75, GETDATE())	
	END 
 
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 4.01 AND HighValue = 5.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 4.01, 5.00, 7,  16.80, GETDATE())	
	END  

/*Weight
Not Over
(pounds) 6*/    

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 5.01 AND HighValue = 6.00)

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 5.01, 6.00, 2, 8.65, GETDATE())	
	END   
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 5.01, 6.00, 3, 10.00, GETDATE())	
	END 

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 5.01, 6.00, 4, 13.95, GETDATE())	
	END 
  
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 5.01, 6.00, 5, 14.40, GETDATE())	
	END   

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 5.01, 6.00, 6, 16.25, GETDATE())	
	END  

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 5.01 AND HighValue = 6.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 5.01, 6.00, 7, 17.65, GETDATE())	
	END  
	
	
/*Weight
Not Over
(pounds) 7*/ 

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 6.01 AND HighValue = 7.00)
   
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 6.01, 7.00, 2, 9.40, GETDATE())	
	END   

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 6.01, 7.00, 3, 11.00, GETDATE())	
	END    

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 6.01, 7.00, 4, 15.35, GETDATE())	
	END    

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 6.01, 7.00, 5, 15.80, GETDATE())	
	END   

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 6.01, 7.00, 6, 18.05, GETDATE())	
	END  

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 6.01 AND HighValue = 7.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 6.01, 7.00, 7, 20.15, GETDATE())	
	END  
 
/*Weight
Not Over
(pounds) 8*/ 

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 7.01 AND HighValue = 8.00)
           
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 7.01, 8.00, 2, 9.75, GETDATE())	
	END   
      
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 7.01, 8.00, 3, 11.95, GETDATE())	
	END  

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 7.01, 8.00, 4, 16.40, GETDATE())	
	END  

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 7.01, 8.00, 5, 17.15, GETDATE())	
	END  
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 7.01, 8.00, 6, 19.80, GETDATE())	
	END  
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 7.01 AND HighValue = 8.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 7.01, 8.00, 7, 22.60, GETDATE())	
	END 
	
/*Weight
Not Over
(pounds) 9*/

SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 8.01 AND HighValue = 9.00)
   
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 8.01, 9.00, 2, 10.45, GETDATE())	
	END  

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 8.01, 9.00, 3, 12.75, GETDATE())	
	END  

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 8.01, 9.00, 4, 17.50, GETDATE())	
	END   
 
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 8.01, 9.00, 5, 18.55, GETDATE())	
	END 
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 8.01, 9.00, 6, 21.55, GETDATE())	
	END 

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 8.01 AND HighValue = 9.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 8.01, 9.00, 7, 25.15, GETDATE())	
	END 	
		
/*Weight
Not Over
(pounds) 10*/ 	
  
SET @ROWGUID = (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 9.01 AND HighValue = 10.00)
    
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 9.01, 10.00, 2, 11.25, GETDATE())	
	END  

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 9.01, 10.00, 3, 13.45, GETDATE())	
	END  
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 9.01, 10.00, 4, 18.65, GETDATE())	
	END  
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 9.01, 10.00, 5, 20.10, GETDATE())	
	END 
	
IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN	
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 9.01, 10.00, 6, 23.45, GETDATE())	
	END 

IF EXISTS (SELECT DISTINCT RowGUID FROM ShippingWeightByZone WHERE ShippingMethodID = 2 AND LowValue = 9.01 AND HighValue = 10.00)
	BEGIN
		INSERT [dbo].ShippingWeightByZone(RowGUID, ShippingMethodID, LowValue, HighValue, ShippingZoneID, ShippingCharge, CreatedOn)
		VALUES(@ROWGUID, 2, 9.01, 10.00, 7, 27.55, GETDATE())	
	END 
		
     

/*100 Customers*/
/*BEGIN - LETTER A*/
delete from Customer where CustomerID != 58639
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58640 AND Email = 'steve_alisauskas@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.')
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58640, 'Steve', 'Alisauskas', 'steve_alisauskas@gmail.com', 'steve$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58641 AND Email = 'arnold_alishio@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.')
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58641, 'Arnold', 'Alishio', 'arnold_alishio@yahoo.com', 'arnold$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58642 AND Email = 'ilene_alishouse@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.')
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58642, 'Ilene', 'Alishouse', 'ilene_alishouse@gmail.com', 'ilene$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58643 AND Email = 'marylou_alison@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.')
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58643, 'Mary Lou', 'Alison', 'marylou_alison@yahoo.com', 'marylou$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58644 AND Email = 'robert_alispach@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.')
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58644, 'Robert', 'Alispach', 'robert_alispach@gmail.com', 'robert$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
/*END - LETTER A*/


/*BEGIN - LETTER B*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58645 AND Email = 'guillermo_brena@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.')
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58645, 'Guillermo', 'Brena', 'guillermo_brena@yahoo.com', 'guillermo$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58646 AND Email = 'kevin_brenan@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.')
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58646, 'Kevin', 'Brenan', 'kevin_brenan@gmail.com', 'kevin$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58647 AND Email = 'glen_brenchley@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.')
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58647, 'Glen', 'Brenchley', 'glen_brenchley@yahoo.com', 'glen$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58648 AND Email = 'stefanie_brendl@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.')
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58648, 'Stefanie', 'Brendl', 'stefanie_brendl@gmail.com', 'stefanie$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58649 AND Email = 'evelyn_brenaman@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.')
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58649, 'Evelyn', 'Brenaman', 'evelyn_brenaman@yahoo.com', 'evelyn$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
/*END - LETTER B*/


/*BEGIN - LETTER C*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58650 AND Email = 'doug_clore@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.')
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58650, 'Doug', 'Clore', 'doug_clore@gmail.com', 'doug$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58651 AND Email = 'frank_crisafulli@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58651, 'Frank', 'Crisafulli', 'frank_crisafulli@yahoo.com', 'frank$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58652 AND Email = 'lidia_crisan@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58652, 'Lidia', 'Crisan', 'lidia_crisan@gmail.com', 'lidia$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58653 AND Email = 'jerry_casabella@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58653, 'Jerry', 'Casabella', 'jerry_casabella@yahoo.com', 'jerry$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58654 AND Email = 'anthony_casablanca@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58654, 'Anthony', 'Casablanca', 'anthony_casablanca@gmail.com', 'anthony$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 	
/*END - LETTER C*/


/*BEGIN - LETTER D*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58655 AND Email = 'tony_dori@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58655, 'Toni', 'Doro', 'tony_dori@yahoo.com', 'tony$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 	  	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58656 AND Email = 'lee_dorobiala@gmail.com')
	BEGIN  	
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58656, 'Lee', 'Dorobiala', 'lee_dorobiala@gmail.com', 'lee$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO  		  
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58657 AND Email = 'anton_dorokhin@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58657, 'Anton', 'Dorokhin', 'anton_dorokhin@yahoo.com', 'anton$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 	 	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58658 AND Email = 'scott_dalitzky@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58658, 'Scott', 'Dalitzky', 'scott_dalitzky@gmail.com', 'scott$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58659 AND Email = 'andre_deseck@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58659, 'Andre', 'Deseck', 'andre_deseck@yahoo.com', 'andre$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 		
/*END - LETTER D*/


/*BEGIN - LETTER E*/     	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58660 AND Email = 'kathlene_elledge@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58660, 'Kathlene', 'Elledge', 'kathlene_elledge@gmail.com', 'kathlene$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58661  AND Email = 'james_ellefsen@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58661, 'James', 'Ellefsen', 'james_ellefsen@yahoo.com', 'james$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58662 AND Email = 'cliff_ellefson@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58662, 'Cliff', 'Ellefson', 'cliff_ellefson@gmail.com', 'cliff$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58663 AND Email = 'ken_ellegard@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58663, 'Ken', 'Ellegard', 'ken_ellegard@yahoo.com', 'ken$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58664 AND Email = 'william_ellen@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58664, 'William', 'Ellen', 'william_ellen@gmail.com', 'william$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 		
/*END - LETTER E*/

/*BEGIN - LETTER F*/  
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58665 AND Email = 'sahra_farah@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58665, 'Sahra', 'Farah', 'sahra_farah@yahoo.com', 'sahra$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58666 AND Email = 'kambiz_farahmand@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58666, 'Kambiz', 'Farahmand', 'kambiz_farahmand@gmail.com', 'kambiz$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 	
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58667 AND Email = 'leyla_faras@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58667, 'Leyla', 'Faras', 'leyla_faras@yahoo.com', 'leyla$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58668 AND Email = 'aranulfo_feria@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58668, 'Aranulfo', 'Feria', 'aranulfo_feria@gmail.com', 'aranulfo$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58669 AND Email = 'stephen_fenick@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58669, 'Stephen', 'Fenick', 'stephen_fenick@yahoo.com', 'stephen$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER F*/

/*BEGIN - LETTER G*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58670 AND Email = 'lawrence_gaba@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58670, 'Lawrence', 'Gaba', 'lawrence_gaba@gmail.com', 'lawrence$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58671 AND Email = 'stephan_gabalac@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58671, 'Stephan', 'Gabalac', 'stephan_gabalac@yahoo.com', 'stephan$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58672 AND Email = 'eugene_gabalski@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58672, 'Eugene', 'Gabalski', 'eugene_gabalski@gmail.com', 'eugene$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58673 AND Email = 'glenn_gabamonte@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58673, 'Glenn', 'Gabamonte', 'glenn_gabamonte@yahoo.com', 'glenn$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58674 AND Email = 'louis_gabanyic@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58674, 'Louis', 'Gabanyic', 'louis_gabanyic@gmail.com', 'louis$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 	
/*END - LETTER G*/

/*BEGIN - LETTER H*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58675 AND Email = 'stefan_hasiak@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58675, 'Stefan', 'Hasiak', 'stefan_hasiak@yahoo.com', 'stefan$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58676 AND Email = 'emir_hasic@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58676, 'Emir', 'Hasic', 'emir_hasic@gmail.com', 'emir$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58677 AND Email = 'kimberly_halom@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58677, 'Kimberly', 'Halom', 'kimberly_halom@yahoo.com', 'kimberly$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58678 AND Email = 'tom_halowell@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58678, 'Tom', 'Halowell', 'tom_halowell@gmail.com', 'tom$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58679 AND Email = 'david_hissey@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58679, 'David', 'Hissey', 'david_hissey@yahoo.com', 'david$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER H*/


/*BEGIN - LETTER I*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58680 AND Email = 'kathy_illingworth@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58680, 'Kathy', 'Illingworth', 'kathy_illingworth@gmail.com', 'kathy$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58681 AND Email = 'guss_irani@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58681, 'Guss', 'Irani', 'guss_irani@yahoo.com', 'guss$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58682 AND Email = 'abdi_iman@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58682, 'Abdi', 'Iman', 'abdi_iman@gmail.com', 'abdi$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58683 AND Email = 'shahla_imani@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58683, 'Shahla', 'Imani', 'shahla_imani@yahoo.com', 'shahla$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58684 AND Email = 'dana_imanski@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58684, 'Dana', 'Imanski', 'dana_imanski@gmail.com', 'dana$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER I*/


/*BEGIN - LETTER J*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58685 AND Email = 'susie_jacocks@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58685, 'Susie', 'Jacocks', 'susie_jacocks@yahoo.com', 'susie$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58686 AND Email = 'leslie_jacoway@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58686, 'Leslie', 'Jacoway', 'leslie_jacoway@gmail.com', 'leslie$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58687 AND Email = 'barbara_jacquot@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58687, 'Barbara', 'Jacquot', 'barbara_jacquot@yahoo.com', 'barbara$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58688 AND Email = 'charles_jaecksch@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58688, 'Charles', 'Jaecksch', 'charles_jaecksch@gmail.com', 'charles$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58689 AND Email = 'jas_jaffray@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58689, 'Jas', 'Jaffray', 'jas_jaffray@yahoo.com', 'jas$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO  
/*END - LETTER J*/


/*BEGIN - LETTER K*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58690 AND Email = 'christian_kaas@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58690, 'Christian', 'Kaas', 'christian_kaas@gmail.com', 'christian$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58691 AND Email = 'eugene_kabat@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58691, 'Eugene', 'Kabat', 'eugene_kabat@yahoo.com', 'eugene$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58692 AND Email = 'bradford_kartman@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58692, 'Bradford', 'Kartman', 'bradford_kartman@gmail.com', 'bradford$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58693 AND Email = 'jerome_klint@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58693, 'Jerome', 'Klint', 'jerome_klint@yahoo.com', 'jerome$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58694 AND Email = 'bernadette_kray@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58694, 'Bernadette', 'Kray', 'bernadette_kray@gmail.com', 'bernadette$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER K*/


/*BEGIN - LETTER L*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58695 AND Email = 'michael_loso@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58695, 'Michael', 'Loso', 'michael_loso@yahoo.com', 'michael$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58696 AND Email = 'carrie_losten@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58696, 'Carrie', 'Losten', 'carrie_losten@gmail.com', 'carrie$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58697 AND Email = 'alana_lostaunau@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58697, 'Alana', 'Lostaunau', 'alana_lostaunau@yahoo.com', 'alana$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58698 AND Email = 'joyce_lostetter@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58698, 'Joyce', 'Lostetter', 'joyce_lostetter@gmail.com', 'joyce$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER L*/


/*BEGIN - LETTER M*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58699 AND Email = 'elma_macadamia@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58699, 'Elma', 'Macadamia', 'elma_macadamia@yahoo.com', 'elma$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58700 AND Email = 'tyrone_myree@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58700, 'Tyrone', 'Myree', 'tyrone_myree@gmail.com', 'tyrone$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58701 AND Email = 'james_macdougald@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58701, 'James', 'Macdougald', 'james_macdougald@yahoo.com', 'james$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58702 AND Email = 'mary_mccrobie@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58702, 'Mary', 'McCrobie', 'mary_mccrobie@gmail.com', 'mary$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER M*/


/*BEGIN - LETTER N*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58703 AND Email = 'sadaf_nabavian@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58703, 'Sadaf', 'Nabavian', 'sadaf_nabavian@yahoo.com', 'sadaf$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58704 AND Email = 'alan_neufer@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58704, 'Alan', 'Neufer', 'alan_neufer@gmail.com', 'alan$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58705 AND Email = 'razvan_nicolescu@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58705, 'Razvan', 'Nicolescu', 'razvan_nicolescu@yahoo.com', 'razvan$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER N*/

 	
/*BEGIN - LETTER O*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58706 AND Email = 'julieann_odo@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58706, 'Julieann', 'Odo', 'julieann_odo@gmail.com', 'julieann$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58707 AND Email = 'jodi_oatney@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58707, 'Jodi', 'Oatney', 'jodi_oatney@yahoo.com', 'jodi$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58708 AND Email = 'beverly_oatfield@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58708, 'Beverly', 'Oatfield', 'beverly_oatfield@gmail.com', 'beverly$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER O*/


/*BEGIN - LETTER P*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58709 AND Email = 'michelle_palisano@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58709, 'Michelle', 'Palisano', 'michelle_palisano@yahoo.com', 'michelle$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58710 AND Email = 'painting_parkers@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58710, 'Painting', 'Parkers', 'painting_parkers@gmail.com', 'painting$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58711 AND Email = 'marguerite_pechin@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58711, 'Marguerite', 'Pechin', 'marguerite_pechin@yahoo.com', 'marguerite$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER P*/


/*BEGIN - LETTER Q*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58712 AND Email = 'merlyn_querido@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58712, 'Merlyn', 'Querido', 'merlyn_querido@gmail.com', 'merlyn$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58713 AND Email = 'shaijh_quader@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58713, 'Shaijh', 'Quader', 'shaijh_quader@yahoo.com', 'shaijh$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58714 AND Email = 'lucerio_quintano@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58714, 'Lucerio', 'Quintano', 'lucerio_quintano@gmail.com', 'lucerio$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER Q*/

	
/*BEGIN - LETTER R*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58715 AND Email = 'lori_raffone@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58715, 'Lori', 'Raffone', 'lori_raffone@yahoo.com', 'lori$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58716 AND Email = 'tom_radden@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58716, 'Tom', 'Radden', 'tom_radden@gmail.com', 'tom$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58717 AND Email = 'david_regenbogen@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58717, 'David', 'Regenbogen', 'david_regenbogen@yahoo.com', 'davidregenbogen$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER R*/

	
/*BEGIN - LETTER S*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58718 AND Email = 'kanalieh_saberi@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58718, 'Kanalieh', 'Saberi', 'kanalieh_saberi@gmail.com', 'kanalieh$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58719 AND Email = 'clifton_sigworth@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58719, 'Clifton', 'Sigworth ', 'clifton_sigworth@yahoo.com', 'clifton$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58720 AND Email = 'cassandra_sawney@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58720, 'Cassandra', 'Sawney', 'cassandra_sawney@yahoo.com', 'cassandra$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER S*/


/*BEGIN - LETTER T*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58721 AND Email = 'kevin_taberski@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58721, 'Kevin', 'Taberski', 'kevin_taberski@gmail.com', 'kevin$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58722 AND Email = 'darius_tandon@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58722, 'Darius', 'Tandon', 'darius_tandon@yahoo.com', 'darius$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58723 AND Email = 'gina_taymon@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58723, 'Gina', 'Taymon', 'gina_taymon@gmail.com', 'gina$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER T*/


/*BEGIN - LETTER U*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58724 AND Email = 'miguel_ubaldo@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58724, 'Miguel', 'Ubaldo', 'miguel_ubaldo@yahoo.com', 'miguel$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58725 AND Email = 'ram_udani@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58725, 'Ram', 'Udani', 'ram_udani@gmail.com', 'ram$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58726 AND Email = 'danielmckeithen_ulven@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58726, 'Daniel McKeithen', 'Ulven', 'danielmckeithen_ulven@yahoo.com', 'danielmckeithen$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER U*/

		
/*BEGIN - LETTER V*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58727 AND Email = 'antonio_vanson@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58727, 'Antonio', 'Vanson', 'antonio_vanson@gmail.com', 'antonio$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58728 AND Email = 'theresa_vaulet@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58728, 'Theresa', 'Vaulet', 'theresa_vaulet@yahoo.com', 'theresa$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58729 AND Email = 'richard_vetterick@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58729, 'Richard', 'Vetterick', 'richard_vetterick@gmail.com', 'richard$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER V*/


/*BEGIN - LETTER W*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58730 AND Email = 'amanda_waley@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58730, 'Amanda', 'Waley', 'amanda_waley@yahoo.com', 'amanda$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58731 AND Email = 'dale_waser@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58731, 'Dale', 'Waser', 'dale_waser@gmail.com', 'dale$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58732 AND Email = 'sharon_weibe@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58732, 'Sharon', 'Weibe', 'sharon_weibe@yahoo.com', 'sharon$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER W*/


/*BEGIN - LETTER X*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58733 AND Email = 'bounmy_xayasith@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58733, 'Bounmy', 'Xayasith', 'bounmy_xayasith@gmail.com', 'bounmy$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58734 AND Email = 'may_zyong@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58734, 'May', 'Xyong', 'may_xyong@yahoo.com', 'may$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER X*/


/*BEGIN - LETTER Y*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58735 AND Email = 'debbie_yarwood@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58735, 'Debbie', 'Yarwood', 'debbie_yarwood@gmail.com', 'debbie$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58736 AND Email = 'albert_yozzo@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58736, 'Albert', 'Yozzo', 'albert_yozzo@yahoo.com', 'albert$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58737 AND Email = 'doug_yray@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58737, 'Doug', 'Yray', 'doug_yray@gmail.com', 'dougyray$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
/*END - LETTER Y*/


/*BEGIN - LETTER Z*/
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58738 AND Email = 'antoinette_zirker@yahoo.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58738, 'Antoinette', 'Zirker', 'antoinette_zirker@yahoo.com', 'antoinette$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO 
IF EXISTS (SELECT * FROM Customer WITH (NOLOCK) WHERE CustomerID = 58739 AND Email = 'randy_zotti@gmail.com')
	BEGIN
		PRINT ('Customer already exists because email address is already taken.') 	
	END
ELSE
	BEGIN
		SET IDENTITY_INSERT [dbo].Customer ON;
		INSERT [dbo].Customer(CustomerID, FirstName, LastName, Email, Password, SaltKey, IsAdmin, LocaleSetting, IsRegistered, Over13Checked, CreatedOn) 
		VALUES				 (58739, 'Randy', 'Zotti', 'randy_zotti@gmail.com', 'randy$888', -1, 0, 'en-US', 1, 1, GETDATE());                    
		SET IDENTITY_INSERT [dbo].Customer OFF;
	END
GO
/*END - LETTER Z*/

/*120 ADDRESS*/
/* 80% U.S. Addresses  20% International Addresses */



--1
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,     Company, Address1,       Address2,        Suite, City,			 State, Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(2,         newid(),     58640,      '',       'Steve',   'Alisauskas', '',      '140 Stump Dr', 'Belle Vernon,', '',    'Pennsylvania', 'PA',  '15012',   'United States',	1,			   '724-930-9470', 'steve_alisauskas@gmail.com', 0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--2
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,       Address2,        Suite, City,			 State, Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(3,         newid(),     58641,      '',       'Arnold',  'Alishio', '',      '1833 34 St',	  'Allegan,',	   '',    'Michigan',    'MI',  '49010',   'United States',	1,			   '269-673-1590', 'arnold_alishio@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--3
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,   Company, Address1,        Address2,       Suite, City,		State, Zip,	    Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(4,         newid(),     58642,      '',       'Ilene',  'Alishouse', '',      '15901 S 57 St', 'Papillion,',	'',    'Nebraska',  'NE',  '68133', 'United States',	1,			   '402-339-1535', 'ilene_alishouse@gmail.com',  0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--4
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName,   LastName,  Company, Address1,				Address2,      Suite, City,			 State, Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(5,         newid(),     58643,      '',       'Mary Lou',  'Alison', '',       '2432 Paulawood Dr',	'Nashville,',  '',    'Tennessee',   'TN',  '37207',   'United States',	1,			   '615-228-1545', 'marylou_alison@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--5
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(6,         newid(),     58644,      '',       'Robert',  'Alispach', '',     '1405 SW Osprey Cove', 'Port Saint Lucie,',  '',    'Florida',  'FL',   '34986', 'United States',	1,			   '772-878-6758', 'robert_alispach@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--6
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName,	 LastName,  Company, Address1,				 Address2,   Suite, City,		State,		Zip,	 Country,	        ResidenceType, Phone,	       Email,						   Deleted, CreatedOn, Crypt) 
VALUES				(7,         newid(),	 58645,      '',       'Guillermo',  'Brena',   '',      '8517 E Hawthorne St',  'Tucson,',  '',    'Arizona',	'AZ',	'85710', 'United States',		1,			   '520-760-0765', 'guillermo_brena@yahoo.com',    0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--7
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,              Address2,       Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(8,         newid(),     58646,      '',	   'Kevin',   'Brenan',  '',      '1850 Club Center Dr', 'Sacramento,',  '',    'California',  'CA',  '95835', 'United States',	1,			   '916-285-6812', 'kevin_brenan@gmail.com',     0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--8
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,    Company, Address1,				 Address2,       Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(9,         newid(),     58647,      '',       'Glen',	  'Brenchley', '',      '4925 Roosevelt Ave',    'Sacramento,',  '',    'California',  'CA',   '95820', 'United States',	1,			   '916-452-3284', 'glen_brenchley@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--9
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(10,         newid(),    58648,      '',       'Stefanie',  'Brendl', '',     '620 Hampton Ct', 'Franklin,',  '',    'Tennessee',  'TN',   '37064', 'United States',	1,			   '615-794-3565', 'stefanie_brendl@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--10
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,       Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(11,         newid(),    58649,      '',       'Evelyn',  'Brenaman', '',     '5424-30 White Tail Cir', 'Conover,',  '',    'North Carolina',  'NC',   '28613', 'United States',	1,			   '828-256-8293', 'evelyn_brenaman@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--11
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(12,         newid(),    58650,      '',       'Doug',  'Clore', '',     '6317 Torrey Pines Dr', 'North Richland Hills,',  '',    'Texas',  'TX',   '76180', 'United States',	1,			   '817-428-2508', 'doug_clore@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--12
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(13,         newid(),    58651,      '',       'Frank',  'Crisafulli', '',     '55 SW Brook St', 'Newport,',  '',    'Oregon',  'OR',   '97365', 'United States',	1,			   '541-265-8537', 'frank_crisafulli@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--13
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(14,         newid(),    58652,      '',       'Lidia',  'Crisan', '',     '8 Newton Ave', 'Saratoga Springs,',  '',    'New York',  'NY',   '12866', 'United States',	1,			   '518-306-5117', 'lidia_crisan@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--14
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(15,         newid(),    58653,      '',       'Jerry',  'Casabella', '',     '504 Shamrock Dr', 'Jacksonville,',  '',    'North Carolina',  'NC',   ' 28540', 'United States',	1,			   '910-937-2587', 'jerry_casabella@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--15
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(16,         newid(),    58654,      '',       'Anthony',  'Casablanca ', '',     '1415 Maryland Ave', 'Covington,',  '',    'Kentucky',  'KY',   ' 41011', 'United States',	1,			   '859-491-0177', 'anthony_casablanca@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--16
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(17,         newid(),    58655,      '',       'Toni',  'Doro', '',     '1315 Winding Branch Cir Dunwoody', 'Atlanta,',  '',    'Georgia',  'GA',   '30338', 'United States',	1,			   '770-393-2395', 'tony_dori@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--17
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(18,         newid(),    58656,      '',       'Lee',  'Dorobiala', '',     '12016 NW 47 St', 'Coral Springs,',  '',    'Florida',  'FL',   '33076', 'United States',	1,			   '954-575-9993', 'lee_dorobiala@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--18
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(19,         newid(),    58657,      '',       'Anton',  'Dorokhin', '',     '8551 Corona St', 'Thornton,',  '',    'Colorado',  'CO',   '80229', 'United States',	1,			   '303-430-7967', 'anton_dorokhin@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--19
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(20,         newid(),    58658,      '',       'Scott',  'Dalitzky', '',     '1272 Woodhaven Ln', 'Lodi,',  '',    'California',  'CA',   '95242', 'United States',	1,			   '209-224-8591', 'scott_dalitzky@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--20
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(21,         newid(),    58659,      '',       'Andre',  'Deseck', '',     '8132 Cholo Tr', 'Jacksonville,',  '',    'Florida',  'FL',   '32244', 'United States',	1,			   '904-527-1325', 'andre_deseck@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--21
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(22,         newid(),    58660,      '',       'Kathlene',  'Elledge', '',     '1117 10 St NW', 'Washington,',  '',    'District of Columbia',  'DC',   '20001', 'United States',	1,			   '202-842-7491', 'kathlene_elledge@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--22
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(23,         newid(),    58661,      '',       'James',  'Ellefsen', '',     '6362 Corinth Rd', 'Longmont,',  '',    'Colorado',  'CO',   '80503', 'United States',	1,			   '303-684-0081', 'james_ellefsen@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--23
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(24,         newid(),    58662,      '',       'Cliff',  'Ellefson', '',     '406 2 St', 'East Brady,',  '',    'Pennsylvania',  'PA',   '16028', 'United States',	1,			   '724-526-5270', 'cliff_ellefson@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--24
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(25,         newid(),    58663,      '',       'Ken',  'Ellegard', '',     '1211 W Cedar St', 'Stilwell,',  '',    'Oklahoma',  'OK',   '74960', 'United States',	1,			   '918-696-5970', 'ken_ellegard@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--25
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(26,         newid(),    58664,      '',       'William',  'Ellen', '',     '645 W 59 Ter', 'Kansas City,',  '',    'Missouri',  'MO',   '64113', 'United States',	1,			   '816-363-5774', 'william_ellen@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--26
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(27,         newid(),    58665,      '',       'Sahra',  'Farah', '',     '2874 Market Place Dr', 'Little Canada,',  '',    'Minnesota',  'MN',   '55117', 'United States',	1,			   '651-494-3821', 'sahra_farah@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--27
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(28,         newid(),    58666,      '',       'Kambiz',  'Farahmand', '',     '1415 Tulane Ave', 'New Orleans,',  '',    'Louisiana',  'LA',   '70112', 'United States',	1,			   '504-988-5363', 'kambiz_farahmand@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--28
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(29,         newid(),    58667,      '',       'Leyla',  'Faras', '',     '7932 Belfast St', 'New Orleans,',  '',    'Louisiana',  'LA',   '70125', 'United States',	1,			   '504-866-8958', 'leyla_faras@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--29
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(30,         newid(),    58668,      '',       'Aranulfo',  'Feria', '',     '3394 Pepperhill Rd', 'Lexington,',  '',    'Kentucky',  'KY',   '40502', 'United States',	1,			   '859-269-9696', 'aranulfo_feria@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--30
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(31,         newid(),    58669,      '',       'Stephen',  'Fenick', '',     '200 S Windsor Dr', 'Arlington Heights,',  '',    'Illinois',  'IL',   '60004', 'United States',	1,			   '847-394-9029', 'stephen_fenick@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--31
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(32,         newid(),    58670,      '',       'Lawrence',  'Gaba', '',     '1090 Woodduck St', 'Fruitland,',  '',    'Idaho',  'ID',   '83619', 'United States',	1,			   '208-452-3616', 'lawrence_gaba@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--32
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(33,         newid(),    58671,      '',       'Stephan',  'Gabalac', '',     '3100 Sweetwater Rd', 'Lawrenceville,',  '',    'Georgia',  'GA',   '30044', 'United States',	1,			   '678-245-3341', 'stephan_gabalac@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--33
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(34,         newid(),    58672,      '',       'Eugene',  'Gabalski', '',     '1850 Folsom St', 'Boulder,',  '',    'Colorado',  'CO',   '80302', 'United States',	1,			   '303-443-8866', 'eugene_gabalski@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--34
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(35,         newid(),    58673,      '',       'Glenn',  'Gabamonte', '',     '3716 Celine Ct', 'Bakersfield,',  '',    'California',  'CA',   '93309', 'United States',	1,			   '661-832-4108', 'glenn_gabamonte@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--35
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(36,         newid(),    58674,      '',       'Louis',  'Gabanyic', '',     '7856 Kristen Cir', 'Mabelvale,',  '',    'Arkansas',  'AR',   '72103', 'United States',	1,			   '501-602-2189', 'louis_gabanyic@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--36
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(37,         newid(),    58675,      '',       'Stefan',  'Hasiak', '',     '2221 W Crenshaw St', 'Kuna,',  '',    'Idaho',  'ID',   '83634', 'United States',	1,			   '208-922-9838', 'stefan_hasiak@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--37
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(38,         newid(),    58676,      '',       'Emir',  'Hasic', '',     '7436 Gary Ave', 'Miami Beach,',  '',    'Florida',  'FL',   '33141', 'United States',	1,			   '305-865-9201', 'emir_hasic@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--38
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(39,         newid(),    58677,      '',       'Kimberly',  'Halom', '',     '2422 Poe Ave', 'Clovis,',  '',    'California',  'CA',   '93611', 'United States',	1,			   '559-322-5662', 'kimberly_halom@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--39
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(40,         newid(),    58678,      '',       'Tom',  'Halowell', '',     '3145 Yellow Lantana Ln', 'Kissimmee,',  '',    'Florida',  'FL',   ' 34747', 'United States',	1,			   '407-396-0366', 'tom_halowell@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--40
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(41,         newid(),    58679,      '',       'David',  'Hissey', '',     '41780 Butterfield Stage Rd', 'Temecula,',  '',    'California',  'CA',   '92592', 'United States',	1,			   '951-695-4982', 'david_hissey@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--41
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(42,         newid(),    58680,      '',       'Kathy',  'Illingworth', '',     '165 Collins St', 'Spring City,',  '',    'Tennessee',  'TN',   '37381', 'United States',	1,			   '423-365-2196', 'kathy_illingworth@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--42
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(43,         newid(),    58681,      '',       'Guss',  'Irani', '',     '1703 Hudgins Dr', 'Greensboro,',  '',    'North Carolina',  'NC',   '27406', 'United States',	1,			   '336-285-6193', 'guss_irani@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--43
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(44,         newid(),    58682,      '',       'Abdi',  'Iman', '',     '1630 S 6 St', 'Minneapolis,',  '',    'Minnesota',  'MN',   '55454', 'United States',	1,			   '612-339-3352', 'abdi_iman@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--44
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(45,         newid(),    58683,      '',       'Shahla',  'Imani', '',     '320 S Railroad St', 'Delcambre,',  '',    'Louisiana',  'LA',   '70528', 'United States',	1,			   '337-685-7777', 'shahla_imani@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--45
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(46,         newid(),    58684,      '',       'Dana',  'Imanski', '',     '2331 W Sonoma Ct', 'Meridian,',  '',    'Idaho',  'ID',   '83642', 'United States',	1,			   '208-884-4694', 'dana_imanski@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--46
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(47,         newid(),    58685,      '',       'Susie',  'Jacocks', '',     '267 Dublin Dr SE', 'Calhoun,',  '',    'Georgia',  'GA',   '30701', 'United States',	1,			   '706-602-9189', 'susie_jacocks@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--47
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(48,         newid(),    58686,      '',       'Leslie',  'Jacoway', '',     '5661 Turkey Rd', 'Pensacola,',  '',    'Florida',  'FL',   '32526', 'United States',	1,			   '850-458-2188', 'leslie_jacoway@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--48
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(49,         newid(),    58687,      '',       'Barbara',  'Jacquot', '',     '2508 Rimrock Dr', 'Colorado Springs,',  '',    'Colorado',  'CO',   '80915', 'United States',	1,			   '719-596-9231', 'barbara_jacquot@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--49
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(50,         newid(),    58688,      '',       'Charles',  'Jaecksch', '',     '3558 Dinny St', 'Santa Clara,',  '',    'California',  'CA',   '95054', 'United States',	1,			   '', '',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--50
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(51,         newid(),    58689,      '',       'Jas',  'Jaffray', '',     '3558 Dinny St', 'Santa Clara,',  '',    'California',  'CA',   '95054', 'United States',	1,			   '408-988-0434', 'jas_jaffray@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--51
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(52,         newid(),    58690,      '',       'Christian',  'Kaas', '',     '10910 E Bellflower Dr', 'Sun Lakes,',  '',    'Arizona',  'AZ',   ' 85248', 'United States',	1,			   '480-895-3026', 'christian_kaas@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--52
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(53,         newid(),    58691,      '',       'Eugene',  'Kabat', '',     '12407 N La Paloma Ct', 'Sun City,',  '',    'Arizona',  'AZ',   '85351', 'United States',	1,			   '623-972-4242', 'eugene_kabat@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--53
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(54,         newid(),    58692,      '',       'Bradford',  'Kartman', '',     '10205 N 105 Dr', 'Sun City,',  '',    'Arizona',  'AZ',   '85351', 'United States',	1,			   '623-974-8383', 'bradford_kartman@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--54
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(55,         newid(),    58693,      '',       'Jerome',  'Klint', '',     '164 W Eric St', 'Tucson,',  '',    'Arizona',  'AZ',   '85706', 'United States',	1,			   '520-807-0490', 'jerome_klint@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--55
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(56,         newid(),    58694,      '',       'Bernadette',  'Kray', '',     '2248 E Mead Pl', 'Chandler,',  '',    'Arizona',  'AZ',   '85249', 'United States',	1,			   '480-883-7795', 'bernadette_kray@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--56
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(57,         newid(),    58695,      '',       'Michael',  'Loso', '',     '728 E Main St', 'Batesville,',  '',    'Arkansas',  'AR',   '72501', 'United States',	1,			   '870-793-0086', 'michael_loso@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--57
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(58,         newid(),    58696,      '',       'Carrie',  'Losten', '',     '3670 Irby Dr', 'Conway,',  '',    'Arkansas',  'AR',   '72034', 'United States',	1,			   '501-505-8303', 'carrie_losten@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--58
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(59,         newid(),    58697,      '',       'Alana',  'Lostaunau', '',     '2802 Goldhill Rd', 'Fairbanks,',  '',    'Alaska',  'AK',   '99709', 'United States',	1,			   '907-479-3567', 'alana_lostaunau@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--59
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(60,         newid(),    58698,      '',       'Joyce',  'Lostetter', '',     'Jumbo Subd Bx Mxy', 'McCarthy,',  '',    'Alaska',  'AK',   '99695', 'United States',	1,			   '907-334-0970', 'joyce_lostetter@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--60
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(61,         newid(),    58699,      '',       'Elma',  'Macadamia', '',     '8610 Crest Ct', 'Baton Rouge,',  '',    'Louisiana',  'LA',   '70809', 'United States',	1,			   '225-761-0035', 'elma_macadamia@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--61
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(62,         newid(),    58700,      '',       'Tyrone',  'Myree', '',     '8411 Pflumm Rd', 'Lenexa,',  '',    'Kansas',  'KS',   '66215', 'United States',	1,			   '913-825-0601', 'tyrone_myree@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--62
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(63,         newid(),    58701,      '',       'James',  'Macdougald', '',     '2331 Taliesin Dr', ',',  '',    'Illinois',  'IL',   '60506', 'United States',	1,			   '630-844-0890', 'james_macdougald@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--63
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(64,         newid(),    58702,      '',       'Mary',  'McCrobie', '',     '2371 Hoohu Rd', 'Koloa,',  '',    'Hawaii',  'HI',   '96756', 'United States',	1,			   '808-742-7341', 'mary_mccrobie@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--64
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(65,         newid(),    58703,      '',       'Sadaf',  'Nabavian', '',     '1004 Chippewa Tr', 'Holly Hill,',  '',    'Florida',  'FL',   '32117', 'United States',	1,			   '386-254-2882', 'sadaf_nabavian@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--65
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(66,         newid(),    58704,      '',       'Alan',  'Neufer', '',     '3385 Independence Ct', 'Wheat Ridge,',  '',    'Colorado',  'CO',   '80033', 'United States',	1,			   '303-238-2976', 'alan_neufer@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--66
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(67,         newid(),    58705,      '',       'Razvan',  'Nicolescu', '',     '325 Sylvan Ave', 'Mountain View,',  '',    'California',  'CA',   '94041', 'United States',	1,			   '650-938-3895', 'razvan_nicolescu@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--67
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(68,         newid(),    58706,      '',       'Julieann',  'Odo', '',     '19 Finger Cir', 'Bella Vista,',  '',    'Arkansas',  'AR',   '72715', 'United States',	1,			   '479-855-3463', 'julieann_odo@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--68
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(69,         newid(),    58707,      '',       'Jodi',  'Oatney', '',     '1427 W Beacon Ave', 'Anaheim,',  '',    'California',  'CA',   '92802', 'United States',	1,			   '714-772-9996', 'jodi_oatney@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--69
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(70,         newid(),    58708,      '',       'Beverly',  'Oatfield', '',     '64 Hewitt Ave', 'Staten Island,',  '',    'New York',  'NY',   '10301', 'United States',	1,			   '718-982-8944', 'beverly_oatfield@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--70
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(71,         newid(),    58709,      '',       'Michelle',  'Palisano', '',     '950 Ridgefield Dr', 'Carson City,',  '',    'Nevada',  'NV',   '89706', 'United States',	1,			   '775-888-6812', 'michelle_palisano@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--71
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(72,         newid(),    58710,      '',       'Painting',  'Parkers', '',     '57 Tudor Dr', 'Trenton,',  '',    'New Jersey',  'NJ',   '08690', 'United States',	1,			   '609-631-8666', 'painting_parkers@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--72
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(73,         newid(),    58711,      '',       'Marguerite',  'Pechin', '',     '118 N Norris Ave', 'Pender,',  '',    'Nebraska',  'NE',   '68047', 'United States',	1,			   '402-385-2156', 'marguerite_pechin@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--73
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(74,         newid(),    58712,      '',       'Merlyn',  'Querido', '',     '1207 E 93 1/2 St', 'Bloomington,',  '',    'Minnesota',  'MN',   '55425', 'United States',	1,			   '952-888-4216', 'merlyn_querido@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--74
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(75,         newid(),    58713,      '',       'Shaijh',  'Quader', '',     '3559 18 St', 'Wyandotte,',  '',    'Michigan',  'MI',   '48192', 'United States',	1,			   '734-282-9028', 'shaijh_quader@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--75
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(76,         newid(),    58714,      '',       'Lucerio',  'Quintano', '',     'Beech Hill Rd', 'Blue Hill,',  '',    'Maine',  'ME',   '04614', 'United States',	1,			   '207-374-5172', 'lucerio_quintano@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--76
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(77,         newid(),    58715,      '',       'Lori',  'Raffone', '',     '222 Michele Cir', 'Millersville,',  '',    'Maryland',  'MD',   '21108', 'United States',	1,			   '410-729-2577', 'lori_raffone@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--77
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(78,         newid(),    58716,      '',       'Tom',  'Radden', '',     '1200 Bourbon St', 'Thibodaux,',  '',    'Louisiana',  'LA',   '70301', 'United States',	1,			   '985-446-2476', 'tom_radden@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--78
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(79,         newid(),    58717,      '',       'David',  'Regenbogen', '',     '3329 Alford Ave', '',  '',    'Kentucky',  'KY',   '40212', 'United States',	1,			   '502-614-8026', 'david_regenbogen@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--79
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(80,         newid(),    58718,      '',       'Kanalieh',  'Saberi', '',     '7647 Winding Way', 'Fishers,',  '',    'Indiana',  'IN',   '46038', 'United States',	1,			   '317-570-9069', 'kanalieh_saberi@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--80
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(81,         newid(),    58719,      '',       'Clifton',  'Sigworth ', '',     '527 Reliance Dr', 'Evansville,',  '',    'Indiana',  'IN',   '47711', 'United States',	1,			   '812-867-7004', 'clifton_sigworth@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--81
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(82,         newid(),    58720,      '',       'Cassandra',  'Sawney', '',     '1287 Lopaka Pl', 'Kailua,',  '',    'Hawaii',  'HI',   '96734', 'United States',	1,			   '808-261-5030', 'cassandra_sawney@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--82
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(83,         newid(),    58721,      '',       'Kevin',  'Taberski', '',     '121 Park Plaza Dr', 'Daly City,',  '',    'California',  'CA',   '94015', 'United States',	1,			   '650-994-4751', 'kevin_taberski@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--83
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(84,         newid(),    58722,      '',       'Darius',  'Tandon', '',     '1167 W Lisa Ln', 'Tempe,',  '',    'Arizona',  'AZ',   '85284', 'United States',	1,			   '480-592-9908', 'darius_tandon@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--84
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(85,         newid(),    58723,      '',       'Gina',  'Taymon', '',     '964 Morris Ave', 'Green Bay,',  '',    'Wisconsin',  'WI',   ' 54304', 'United States',	1,			   '920-494-4658', 'gina_taymon@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--85
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(86,         newid(),    58724,      '',       'Miguel',  'Ubaldo', '',     '220 E Ohio Ave', 'Milwaukee,',  '',    'Wisconsin',  'WI',   '53207', 'United States',	1,			   '414-483-5508', 'miguel_ubaldo@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--86
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(87,         newid(),    58725,      '',       'Ram',  'Udani', '',     'N63 W37913 Vista Dr', 'Oconomowoc,',  '',    'Wisconsin',  'WI',   '53066', 'United States',	1,			   '262-567-3805', 'ram_udani@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--87
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(88,         newid(),    58726,      '',       'Daniel McKeithen',  'Ulven', '',     '7825 W Bender Ave', 'Milwaukee,',  '',    'Wisconsin',  'WI',   '53218', 'United States',	1,			   '414-760-0631', 'danielmckeithen_ulven@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--88
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(89,         newid(),    58727,      '',       'Antonio',  'Vanson', '',     '41143 Calla Lily St', 'Fort Mill,',  '',    'South Carolina',  'SC',   '29715', 'United States',	1,			   '803-802-2192', 'antonio_vanson@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--89
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(90,         newid(),    58728,      '',       'Theresa',  'Vaulet', '',     '3 Jenkins St', 'Bristol,',  '',    'Rhode Island',  'RI',   '02809', 'United States',	1,			   '401-254-1487', 'theresa_vaulet@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--90
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(91,         newid(),    58729,      '',       'Richard',  'Vetterick', '',     '51 Woodcross Dr', 'Columbia,',  '',    'South Carolina',  'SC',   '29212', 'United States',	1,			   '803-781-7987', 'richard_vetterick@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--91
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(92,         newid(),    58730,      '',       'Amanda',  'Waley', '',     '409 E Calhoun Crossing Ct', 'Spartanburg,',  '',    'South Carolina',  'SC',   '29307', 'United States',	1,			   '864-529-0058', 'amanda_waley@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--92
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(93,         newid(),    58731,      '',       'Dale',  'Waser', '',     '3397 Calle Calvi', 'Rincon,',  '',    'Puerto Rico',  'PR',   '00677', 'United States',	1,			   '787-823-1871', 'dale_waser@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--93
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(94,         newid(),    58732,      '',       'Sharon',  'Weibe', '',     '3791 NW Boxwood Pl', 'Corvallis,',  '',    'Oregon',  'OR',   '97330', 'United States',	1,			   '541-754-0281', 'sharon_weibe@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--94
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(95,         newid(),    58733,      '',       'Bounmy',  'Xayasith', '',     '23555 N Henney Rd', 'Arcadia,',  '',    'Oklahoma',  'OK',   '73007', 'United States',	1,			   '405-396-2214', 'bounmy_xayasith@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--95
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(96,         newid(),    58734,      '',       'May',  'Xyong', '',     '8327 S 43 West Ave', 'Tulsa,',  '',    'Oklahoma',  'OK',   '74132', 'United States',	1,			   '918-445-0414', 'may_xyong@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--96
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(97,         newid(),    58735,      '',       'Debbie',  'Yarwood', '',     '901 W Martin Luther King St', 'Muskogee,',  '',    'Oklahoma',  'OK',   '74401', 'United States',	1,			   '918-684-9916', 'debbie_yarwood@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--97
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(98,         newid(),    58736,      '',       'Albert',  'Yozzo', '',     '821 Squire Rd', 'Harrisburg,',  '',    'Pennsylvania',  'PA',   '17111', 'United States',	1,			   '717-695-6332', 'albert_yozzo@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--98
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(99,         newid(),    58737,      '',       'Doug',  'Yray', '',     '6 Earl St', 'Glouster,',  '',    'Ohio',  'OH',   '45732', 'United States',	1,			   '740-767-3051', 'doug_yray@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--99
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(100,         newid(),    58738,      '',       'Antoinette',  'Zirker', '',     '1446 Cloud Dance Ct', 'North Las Vegas,',  '',    'Nevada',  'NV',   '89031', 'United States',	1,			   '702-655-2437', 'antoinette_zirker@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--100
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(101,         newid(),    58739,      '',       'Randy',  'Zotti', '',     '2001 Hopewell St', 'Santa Fe,',  '',    'New Mexico',  'NM',   '87505', 'United States',	1,			   '505-983-1405', 'randy_zotti@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


/*NON-US ADDRESSES*/

--101
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(102,         newid(),    58640,      '',       'Arnold',  'Alishio', '',     '26 Antheon Street', 'Paleo Psychico,',  '',    'Athens',  '--',   '15452', 'Greece',	1,			   '210-672-8318', 'arnold_alishio@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--102
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(103,         newid(),    58645,      '',       'Guillermo',  'Brena', '',     'Jl. Imam Bonjol No. 29', '',  '',    'Jakarta',  '--',   '10310', 'Indonesia',	1,			   '3193-2372', 'guillermo_brena@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--103
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(104,         newid(),    58650,      '',       'Doug',  'Clore', '',     'No. 1 Changkat Kia Peng', '',  '',    'Kuala Lumpur',  '--',   '50450', 'Malaysia',	1,			   '2148-3342', 'doug_clore@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--104
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(105,         newid(),    58655,      '',       'Toni',  'Doro', '',     '1 Moonah Place Yarralumla ACT 2600', 'P.O. Box 3297,',  '',    'Manuka',  '--',   '2603', 'Australia',	1,			   '6273-3525', 'tony_dori@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--105
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(106,         newid(),    58660,      '',       'Kathlene',  'Elledge', '',     'No. 23 Xiu Shui Bei-jie', 'Jiangoumenwai,',  '',    'Beijing',  '--',   '100600', 'China',	1,			   '6532-1825', 'kathlene_elledge@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--106
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(107,         newid(),    58665,      '',       'Sahra',  'Farah', '',     '13th Floor, Textile Center Building,', '2 Kaufman Street,',  '',    'Tel-Aviv ',  '--',   '68012', 'Israel',	1,			   '5175-263', 'sahra_farah@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--107
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(108,         newid(),    58670,      '',       'Lawrence',  'Gaba', '',     '50-N Nyaya Marg', 'Chanakyapuri,',  '',    'New Delhi ',  '--',   '110021', 'India',	1,			   '2410-1120', 'lawrence_gaba@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO
 
--108
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(109,         newid(),    58675,      '',       'Stefan',  'Hasiak', '',     'Site D3 Collector Road C, Diplomatic Quarter', 'PO Box 94366,',  '',    'Riyadh',  '--',   '11693', 'Saudi Arabia',	1,			   '4825-935', 'stefan_hasiak@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--109
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(110,         newid(),    58680,      '',       'Kathy',  'Illingworth', '',     '173 Park Road', 'Johnsonville,',  '',    'Wellington',  '--',   '6004', 'New Zealand',	1,			   '4725-170', 'kathy_illingworth@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--110
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(111,         newid(),    58685,      '',       'Susie',  'Jacocks', '',     'Villa # 7 A1 Eithar Street', 'Saha 2, West Bay Area,',  '',    'Doha',  '--',   '24900', 'Qatar',	1,			   '4831-855', 'susie_jacocks@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--111
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(112,         newid(),    58690,      '',       'Christian',  'Kaas', '',     'Villa No. 2 Street 5, E-18/02, Plot No. 97 behind Al Falah Plaza,', 'Madinat Zayed ,',  '',    'Abu Dhabi',  '--',   '3215', 'United Arab Emirates',	1,			   '641-9259', 'christian_kaas@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--112
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(113,         newid(),    58695,      '',       'Michael',  'Loso', '',     '54 Nicholson Street', 'P.O. Box 2562, Brooklyn Square 0075 Muckleneuk,',  '',    'Pretoria',  '--',   '0181', 'South Africa',	1,			   '3460-145', 'michael_loso@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--113
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(114,         newid(),    58699,      '',       'Elma',  'Macadamia', '',     'No. 56 Mahatma Gandhi Caddesi', 'Gaziosmanpasa,',  '',    'Ankara',  '--',   '06700', 'Turkey',	1,			   '446-315858', 'elma_macadamia@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--114
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(115,         newid(),    58703,      '',       'Sadaf',  'Nabavian', '',     'Calle Aduana, 29', '',  '',    'Madrid',  '--',   '28070', 'Spain',	1,			   '411-0666', 'sadaf_nabavian@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--115
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(116,         newid(),    58706,      '',       'Julieann',  'Odo', '',     '4, Hameau de Boulainvilliers', '',  '',    'Paris',  '--',  '75016', 'France',	1,			   '44-14-07-05', 'julieann_odo@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--116
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(117,         newid(),    58709,      '',       'Michelle',  'Palisano', '',     'Tinkune', '',  '',    'Kathmandu',  '--',   '2640', 'Nepal',	1,			   '478-103', 'michelle_palisano@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

--117
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(118,         newid(),    58712,      '',       'Merlyn',  'Querido', '',     'Mariscal Ram?n Castilla 3075', '',  '',    'Buenos Aires',  '--',   '1425', 'Argentina',	1,			   '4807-3433', 'merlyn_querido@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--118
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(119,         newid(),    58715,      '',       'Lori',  'Raffone', '',     'Glanhofen 6', '',  '',    'Salzburg',  '--',   '5017', 'Austria',	1,			   '823-8084', 'lori_raffone@yahoo.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--119
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(120,         newid(),    58718,      '',       'Kanalieh',  'Saberi', '',     'No. 33 Road 294 Khan Chamcarmon', '',  '',    'Phnom Penh',  '--',   '2018', 'Cambodia',	1,			   '215-154', 'kanalieh_saberi@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO


--120
SET IDENTITY_INSERT [dbo].Address ON;
INSERT [dbo].Address(AddressID, AddressGUID, CustomerID, NickName, FirstName, LastName,  Company, Address1,				 Address2,             Suite, City,		  State,  Zip,	   Country,			ResidenceType, Phone,		   Email,						 Deleted, CreatedOn, Crypt) 
VALUES				(121,         newid(),    58721,      '',       'Kevin',  'Taberski', '',     'Kirchenfeldstrasse 73', '',  '',    'Bern',  '--',   '3005', 'Switzerland',	1,			   '350-7171', 'kevin_taberski@gmail.com',   0,       getdate(), 1);
SET IDENTITY_INSERT [dbo].Address OFF;
GO

/*MAP CUSTOMER A BILLING AND SHIPPING ADDRESSES*/
UPDATE Customer SET BillingAddressID = 2, ShippingAddressID = 102 WHERE CustomerID = 58640
UPDATE Customer SET BillingAddressID = 3, ShippingAddressID = 3 WHERE CustomerID = 58641
UPDATE Customer SET BillingAddressID = 4, ShippingAddressID = 4 WHERE CustomerID = 58642
UPDATE Customer SET BillingAddressID = 5, ShippingAddressID = 5 WHERE CustomerID = 58643
UPDATE Customer SET BillingAddressID = 6, ShippingAddressID = 6 WHERE CustomerID = 58644
UPDATE Customer SET BillingAddressID = 7, ShippingAddressID = 103 WHERE CustomerID = 58645
UPDATE Customer SET BillingAddressID = 8, ShippingAddressID = 8 WHERE CustomerID = 58646
UPDATE Customer SET BillingAddressID = 9, ShippingAddressID = 9 WHERE CustomerID = 58647
UPDATE Customer SET BillingAddressID = 10, ShippingAddressID = 10 WHERE CustomerID = 58648

UPDATE Customer SET BillingAddressID = 11, ShippingAddressID = 11 WHERE CustomerID = 58649
UPDATE Customer SET BillingAddressID = 12, ShippingAddressID = 104 WHERE CustomerID = 58650
UPDATE Customer SET BillingAddressID = 13, ShippingAddressID = 13 WHERE CustomerID = 58651
UPDATE Customer SET BillingAddressID = 14, ShippingAddressID = 14 WHERE CustomerID = 58652
UPDATE Customer SET BillingAddressID = 15, ShippingAddressID = 15 WHERE CustomerID = 58653

UPDATE Customer SET BillingAddressID = 16, ShippingAddressID = 16 WHERE CustomerID = 58654
UPDATE Customer SET BillingAddressID = 17, ShippingAddressID = 105 WHERE CustomerID = 58655
UPDATE Customer SET BillingAddressID = 18, ShippingAddressID = 18 WHERE CustomerID = 58656
UPDATE Customer SET BillingAddressID = 19, ShippingAddressID = 19 WHERE CustomerID = 58657
UPDATE Customer SET BillingAddressID = 20, ShippingAddressID = 20 WHERE CustomerID = 58658

UPDATE Customer SET BillingAddressID = 21, ShippingAddressID = 21 WHERE CustomerID = 58659
UPDATE Customer SET BillingAddressID = 22, ShippingAddressID = 106 WHERE CustomerID = 58660
UPDATE Customer SET BillingAddressID = 23, ShippingAddressID = 23 WHERE CustomerID = 58661
UPDATE Customer SET BillingAddressID = 24, ShippingAddressID = 24 WHERE CustomerID = 58662
UPDATE Customer SET BillingAddressID = 25, ShippingAddressID = 25 WHERE CustomerID = 58663

UPDATE Customer SET BillingAddressID = 26, ShippingAddressID = 26 WHERE CustomerID = 58664
UPDATE Customer SET BillingAddressID = 27, ShippingAddressID = 107 WHERE CustomerID = 58665
UPDATE Customer SET BillingAddressID = 28, ShippingAddressID = 28 WHERE CustomerID = 58666
UPDATE Customer SET BillingAddressID = 29, ShippingAddressID = 29 WHERE CustomerID = 58667
UPDATE Customer SET BillingAddressID = 30, ShippingAddressID = 30 WHERE CustomerID = 58668

UPDATE Customer SET BillingAddressID = 31, ShippingAddressID = 31 WHERE CustomerID = 58669
UPDATE Customer SET BillingAddressID = 32, ShippingAddressID = 108 WHERE CustomerID = 58670
UPDATE Customer SET BillingAddressID = 33, ShippingAddressID = 33 WHERE CustomerID = 58671
UPDATE Customer SET BillingAddressID = 34, ShippingAddressID = 34 WHERE CustomerID = 58672
UPDATE Customer SET BillingAddressID = 35, ShippingAddressID = 35 WHERE CustomerID = 58673

UPDATE Customer SET BillingAddressID = 36, ShippingAddressID = 36 WHERE CustomerID = 58674
UPDATE Customer SET BillingAddressID = 37, ShippingAddressID = 109 WHERE CustomerID = 58675
UPDATE Customer SET BillingAddressID = 38, ShippingAddressID = 38 WHERE CustomerID = 58676
UPDATE Customer SET BillingAddressID = 39, ShippingAddressID = 39 WHERE CustomerID = 58677
UPDATE Customer SET BillingAddressID = 40, ShippingAddressID = 40 WHERE CustomerID = 58678

UPDATE Customer SET BillingAddressID = 41, ShippingAddressID = 41 WHERE CustomerID = 58679
UPDATE Customer SET BillingAddressID = 42, ShippingAddressID = 110 WHERE CustomerID = 58680
UPDATE Customer SET BillingAddressID = 43, ShippingAddressID = 43 WHERE CustomerID = 58681
UPDATE Customer SET BillingAddressID = 44, ShippingAddressID = 44 WHERE CustomerID = 58682
UPDATE Customer SET BillingAddressID = 45, ShippingAddressID = 45 WHERE CustomerID = 58683

UPDATE Customer SET BillingAddressID = 46, ShippingAddressID = 46 WHERE CustomerID = 58684
UPDATE Customer SET BillingAddressID = 47, ShippingAddressID = 111 WHERE CustomerID = 58685
UPDATE Customer SET BillingAddressID = 48, ShippingAddressID = 48 WHERE CustomerID = 58686
UPDATE Customer SET BillingAddressID = 49, ShippingAddressID = 49 WHERE CustomerID = 58687
UPDATE Customer SET BillingAddressID = 50, ShippingAddressID = 50 WHERE CustomerID = 58688

UPDATE Customer SET BillingAddressID = 51, ShippingAddressID = 51 WHERE CustomerID = 58689
UPDATE Customer SET BillingAddressID = 52, ShippingAddressID = 112 WHERE CustomerID = 58690
UPDATE Customer SET BillingAddressID = 53, ShippingAddressID = 53 WHERE CustomerID = 58691
UPDATE Customer SET BillingAddressID = 54, ShippingAddressID = 54 WHERE CustomerID = 58692
UPDATE Customer SET BillingAddressID = 55, ShippingAddressID = 55 WHERE CustomerID = 58693

UPDATE Customer SET BillingAddressID = 56, ShippingAddressID = 56 WHERE CustomerID = 58694
UPDATE Customer SET BillingAddressID = 57, ShippingAddressID = 113 WHERE CustomerID = 58695
UPDATE Customer SET BillingAddressID = 58, ShippingAddressID = 58 WHERE CustomerID = 58696
UPDATE Customer SET BillingAddressID = 59, ShippingAddressID = 59 WHERE CustomerID = 58697
UPDATE Customer SET BillingAddressID = 60, ShippingAddressID = 60 WHERE CustomerID = 58698

UPDATE Customer SET BillingAddressID = 61, ShippingAddressID = 114 WHERE CustomerID = 58699
UPDATE Customer SET BillingAddressID = 62, ShippingAddressID = 62 WHERE CustomerID = 58700
UPDATE Customer SET BillingAddressID = 63, ShippingAddressID = 63 WHERE CustomerID = 58701
UPDATE Customer SET BillingAddressID = 64, ShippingAddressID = 64 WHERE CustomerID = 58702
UPDATE Customer SET BillingAddressID = 65, ShippingAddressID = 115 WHERE CustomerID = 58703

UPDATE Customer SET BillingAddressID = 66, ShippingAddressID = 66 WHERE CustomerID = 58704
UPDATE Customer SET BillingAddressID = 67, ShippingAddressID = 67 WHERE CustomerID = 58705
UPDATE Customer SET BillingAddressID = 68, ShippingAddressID = 116 WHERE CustomerID = 58706
UPDATE Customer SET BillingAddressID = 69, ShippingAddressID = 69 WHERE CustomerID = 58707
UPDATE Customer SET BillingAddressID = 70, ShippingAddressID = 70 WHERE CustomerID = 58708

UPDATE Customer SET BillingAddressID = 71, ShippingAddressID = 117 WHERE CustomerID = 58709
UPDATE Customer SET BillingAddressID = 72, ShippingAddressID = 72 WHERE CustomerID = 58710
UPDATE Customer SET BillingAddressID = 73, ShippingAddressID = 73 WHERE CustomerID = 58711
UPDATE Customer SET BillingAddressID = 74, ShippingAddressID = 118 WHERE CustomerID = 58712
UPDATE Customer SET BillingAddressID = 75, ShippingAddressID = 75 WHERE CustomerID = 58713

UPDATE Customer SET BillingAddressID = 76, ShippingAddressID = 76 WHERE CustomerID = 58714
UPDATE Customer SET BillingAddressID = 77, ShippingAddressID = 119 WHERE CustomerID = 58715
UPDATE Customer SET BillingAddressID = 78, ShippingAddressID = 78 WHERE CustomerID = 58716
UPDATE Customer SET BillingAddressID = 79, ShippingAddressID = 79 WHERE CustomerID = 58717
UPDATE Customer SET BillingAddressID = 80, ShippingAddressID = 120 WHERE CustomerID = 58718

UPDATE Customer SET BillingAddressID = 81, ShippingAddressID = 81 WHERE CustomerID = 58719
UPDATE Customer SET BillingAddressID = 82, ShippingAddressID = 82 WHERE CustomerID = 58720
UPDATE Customer SET BillingAddressID = 83, ShippingAddressID = 121 WHERE CustomerID = 58721
UPDATE Customer SET BillingAddressID = 84, ShippingAddressID = 84 WHERE CustomerID = 58722
UPDATE Customer SET BillingAddressID = 85, ShippingAddressID = 85 WHERE CustomerID = 58723

UPDATE Customer SET BillingAddressID = 86, ShippingAddressID = 86 WHERE CustomerID = 58724
UPDATE Customer SET BillingAddressID = 87, ShippingAddressID = 87 WHERE CustomerID = 58725
UPDATE Customer SET BillingAddressID = 88, ShippingAddressID = 88 WHERE CustomerID = 58726
UPDATE Customer SET BillingAddressID = 89, ShippingAddressID = 89 WHERE CustomerID = 58727
UPDATE Customer SET BillingAddressID = 90, ShippingAddressID = 90 WHERE CustomerID = 58728

UPDATE Customer SET BillingAddressID = 91, ShippingAddressID = 91 WHERE CustomerID = 58729
UPDATE Customer SET BillingAddressID = 92, ShippingAddressID = 92 WHERE CustomerID = 58730
UPDATE Customer SET BillingAddressID = 93, ShippingAddressID = 93 WHERE CustomerID = 58731
UPDATE Customer SET BillingAddressID = 94, ShippingAddressID = 94 WHERE CustomerID = 58732
UPDATE Customer SET BillingAddressID = 95, ShippingAddressID = 95 WHERE CustomerID = 58733

UPDATE Customer SET BillingAddressID = 96, ShippingAddressID = 96 WHERE CustomerID = 58734
UPDATE Customer SET BillingAddressID = 97, ShippingAddressID = 97 WHERE CustomerID = 58735
UPDATE Customer SET BillingAddressID = 98, ShippingAddressID = 98 WHERE CustomerID = 58736
UPDATE Customer SET BillingAddressID = 99, ShippingAddressID = 99 WHERE CustomerID = 58737
UPDATE Customer SET BillingAddressID = 100, ShippingAddressID = 100 WHERE CustomerID = 58738
UPDATE Customer SET BillingAddressID = 101, ShippingAddressID = 101 WHERE CustomerID = 58739

UPDATE Customer SET BillingEqualsShipping = 1 WHERE BillingAddressID <> ShippingAddressID 



set identity_insert ordernumbers on;
insert into OrderNumbers (ordernumber) values (100137)
insert into OrderNumbers (ordernumber) values (100139)
insert into OrderNumbers (ordernumber) values (100140)
insert into OrderNumbers (ordernumber) values (100141)
insert into OrderNumbers (ordernumber) values (100156)
set identity_insert ordernumbers off;

insert into Orders_ShoppingCart (OrderNumber,ShoppingCartRecID,CustomerID,ProductID,VariantID,Quantity,OrderedProductName,OrderedProductSKU,OrderedProductPrice,OrderedProductRegularPrice,IsTaxable,ShippingAddressID,ShippingDetail,ShippingMethodID,ShippingMethod,DistributorID,IsSystem,TaxClassID,TaxRate)
values  (100156,215,58639,97,106,1,'Wall Mounted Vent-free Gas Fireplace','01-00097',296.99,329.99,1,1,'<Detail><Address><AddressID>122</AddressID><NickName>joseph</NickName><FirstName>joseph</FirstName><LastName>joseph</LastName><Company>joseph</Company><ResidenceType>0</ResidenceType><Address1>123 joseph</Address1><Address2></Address2><Suite></Suite><City>boston</City><State>MA</State><Zip>01432</Zip><Country>United States</Country><Phone>911-11</Phone><EMail></EMail></Address><Shipping><CustomerID>58639</CustomerID><Date>0001-01-01T00:00:00</Date></Shipping></Detail>',0,'',1,NULL,1,15.00)

insert into Orders_ShoppingCart (OrderNumber,ShoppingCartRecID,CustomerID,ProductID,VariantID,Quantity,OrderedProductName,OrderedProductSKU,OrderedProductPrice,OrderedProductRegularPrice,IsTaxable,ShippingAddressID,ShippingDetail,ShippingMethodID,ShippingMethod,DistributorID,IsSystem,TaxClassID,TaxRate)
values  (100156,214,58639,97,106,1,'Wall Mounted Vent-free Gas Fireplace','01-00097',296.99,329.99,1,121,'<Detail><Address><AddressID>1</AddressID><NickName></NickName><FirstName>Admin</FirstName><LastName>User</LastName><Company></Company><ResidenceType>0</ResidenceType><Address1>123 Main St</Address1><Address2></Address2><Suite></Suite><City>New York</City><State>NY</State><Zip>10451</Zip><Country>United States</Country><Phone>123-456-7890</Phone><EMail>admin@admin.com</EMail></Address><Shipping><CustomerID>58639</CustomerID><Date>0001-01-01T00:00:00</Date></Shipping></Detail>',0,'',1,NULL,1,15.00)						

insert into Orders (OrderNumber,CustomerID,CustomerGUID,SkinID,ShippingMethodID,ShippingCalculationID,CardType,OrderSubtotal,OrderTax,OrderShippingCosts,OrderTotal,PaymentGateway,AuthorizationResult,AuthorizationPNREF,TransactionCommand,PaymentMethod,TransactionState,AVSResult,LocaleSetting,AlreadyConfirmed,CartType,AuthorizedOn,OrderWeight,CapturedOn,Last4,RTShipRequest,RTShipResponse,LastIPAddress,TransactionType,ReceiptEmailSentOn,AuthorizationCode,OkToEmail,RegisterDate,ShippingMethod,StoreVersion,VoidTXCommand,VoidTXResult,VoidedOn,BillingAddress1,BillingCity,BillingState,BillingZip,BillingPhone,ShippingAddress1,ShippingCity,ShippingCountry,BillingLastName,BillingFirstName,ShippingLastName,ShippingFirstName,Email)
values (100137,58640,'78659F0E-44E5-4A5A-BB5A-083962B5DC6C',1,4,8,'VISA',39.99,8.91,4.55,53.45,'MANUAL','MANUAL GATEWAY SAID OK','83138C97-896F-46F6-BAFC-8E94D0BAACC6','x_type=AUTH_CAPTURE&x_test_request=TRUE&x_description=AspDotNetStorefront+Order+100137&x_amount=53.45&x_card_num=****1111&x_card_code=***&x_exp_date=03/2010&x_phone=***-456-7890&x_fax=&x_customer_tax_id=&x_cust_id=58640&x_invoice_num=100137&x_email=admin%40aspdotnetstorefront.com&x_customer_ip=127.0.0.1&x_first_name=Admin&x_last_name=User&x_company=&x_address=***+Main+St&x_city=New+York&x_state=NY&x_zip=10002&x_country=United+States&x_ship_to_first_name=Admin&x_ship_to_last_name=User&x_ship_to_company=&x_ship_to_address=***+Main+St&x_ship_to_city=New+York&x_ship_to_state=NY&x_ship_to_zip=10002&x_ship_to_country=United+States&x_customer_ip=127.0.0.1','CREDITCARD','VOIDED','OK','en-US',1,0,'2008-11-14 18:15:58.807',1,'2008-11-14 18:15:58.947',1111,'<UPSRequest><AccessRequest xml:lang="en-us"><AccessLicenseNumber>FB91B6D3F6A6AFC8</AccessLicenseNumber><UserId>mikecoburn</UserId><Password>j1i1m1i1</Password></AccessRequest><RatingServiceSelectionRequest xml:lang="en-US"><Request><RequestAction>Rate</RequestAction><RequestOption>Shop</RequestOption><TransactionReference><CustomerContext>Rating and Service</CustomerContext><XpciVersion>1.0001</XpciVersion></TransactionReference></Request><PickupType><Code>01</Code></PickupType><Shipment><Shipper><Address><City>DAYTON</City><StateProvinceCode>OH</StateProvinceCode><PostalCode>45459</PostalCode><CountryCode>US</CountryCode></Address></Shipper><ShipTo><Address><City>NEW YORK</City><StateProvinceCode>WA</StateProvinceCode><PostalCode>10451</PostalCode><CountryCode>US</CountryCode><ResidentialAddressIndicator/></Address></ShipTo><ShipmentWeight><UnitOfMeasurement><Code>LBS</Code></UnitOfMeasurement><Weight>1.5</Weight></ShipmentWeight><Package><PackagingType><Code>02</Code></PackagingType><Dimensions><UnitOfMeasurement><Code>IN</Code></UnitOfMeasurement><Length>0</Length><Width>0</Width><Height>0</Height></Dimensions><Description>1</Description><PackageWeight><UnitOfMeasure><Code>LBS</Code></UnitOfMeasure><Weight>1.5</Weight></PackageWeight><OversizePackage /></Package><ShipmentServiceOptions/></Shipment></RatingServiceSelectionRequest></UPSRequest><USPSRequest><RateV2Request USERID="758PARAC7739"><Package ID="1-0"><Service>Express</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-1"><Service>Priority</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-2"><Service>Parcel</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-3"><Service>Library</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-4"><Service>Media</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package></RateV2Request></USPSRequest><FedExRequest><FDXRateAvailableServicesRequest xmlns:api="http://www.fedex.com/fsmapi" xmlns:xsi="http://www.w3.org/2001/XmlSchema-instance" xsi:noNamespaceSchemaLocation="FDXRateAvailableServicesRequest.xsd"><RequestHeader><CustomerTransactionIdentifier>RatesRequest</CustomerTransactionIdentifier><AccountNumber>183045091</AccountNumber><MeterNumber>7298194</MeterNumber><CarrierCode></CarrierCode></RequestHeader><ShipDate>2008-10-18</ShipDate><DropoffType>REGULARPICKUP</DropoffType><Packaging>YOURPACKAGING</Packaging><WeightUnits>LBS</WeightUnits><ListRate>false</ListRate><Weight>1.5</Weight><OriginAddress><StateOrProvinceCode>OH</StateOrProvinceCode><PostalCode>45459</PostalCode><CountryCode>US</CountryCode></OriginAddress><DestinationAddress><StateOrProvinceCode>WA</StateOrProvinceCode><PostalCode>10451</PostalCode><CountryCode>US</CountryCode></DestinationAddress><Payment><PayorType>SENDER</PayorType></Payment><PackageCount>1</PackageCount></FDXRateAvailableServicesRequest></FedExRequest><DHLRequest><ECommerce xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" action="Request" version="1.1"><Requestor><ID /><Password /></Requestor><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>G</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Ground Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>S</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL 2nd Day Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>N</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 3:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 12:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight><SpecialServices><SpecialService><Code>1030</Code></SpecialService></SpecialServices></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 10:30 am</TransactionTrace></Shipment></ECommerce></DHLRequest>','<UPSResponse><RatingServiceSelectionResponse><Response><TransactionReference><CustomerContext>Rating and Service</CustomerContext><XpciVersion>1.0001</XpciVersion></TransactionReference><ResponseStatusCode>0</ResponseStatusCode><ResponseStatusDescription>Failure</ResponseStatusDescription><Error><ErrorSeverity>Hard</ErrorSeverity><ErrorCode>111285</ErrorCode><ErrorDescription>The postal code 10451 is invalid for WA United States.</ErrorDescription></Error></Response></RatingServiceSelectionResponse></UPSResponse><USPSResponse>  <RateV2Response><Package ID="1-0"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Express Mail</MailService><Rate>24.65</Rate></Postage></Package><Package ID="1-1"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Priority Mail</MailService><Rate>5.60</Rate></Postage></Package><Package ID="1-2"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Machinable>FALSE</Machinable><Zone>4</Zone><Postage><MailService>Parcel Post</MailService><Rate>9.05</Rate></Postage></Package><Package ID="1-3"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Library Mail</MailService><Rate>2.45</Rate></Postage></Package><Package ID="1-4"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Media Mail</MailService><Rate>2.58</Rate></Postage></Package></RateV2Response>  </USPSResponse><FedExResponse><FDXRateAvailableServicesReply xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FDXRateAvailableServicesReply.xsd"><ReplyHeader><CustomerTransactionIdentifier>RatesRequest</CustomerTransactionIdentifier></ReplyHeader><Error><Code>61468</Code><Message>Recipient postal code does not match recipient state/province code.</Message></Error></FDXRateAvailableServicesReply></FedExResponse><DHLResponse><ECommerce xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" action="Response" version="1.1" timestamp="2008/10/17T03:21:22" transmission_reference="680E6900"><Faults><Fault><Code>1002</Code><Description><![CDATA[Required element/node is missing.]]></Description><Context><![CDATA[The required node <ID> is missing, empty, or contains a value that is not the correct data type.]]></Context></Fault></Faults><Requestor><ID/><Password/></Requestor><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>G</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Ground Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>S</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL 2nd Day Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>N</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 3:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 12:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight><SpecialServices><SpecialService><Code>1030</Code></SpecialService></SpecialServices></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 10:30 am</TransactionTrace></Shipment></ECommerce>  </DHLResponse>','127.0.0.1',1,GETDATE(),0,1,'2008-09-22 12:02:23.000','Parcel Select BMC and OBMC Presort','AspDotNetStorefront ML 8.0.1.2/8.0.1.2','x_type=VOID&x_test_request=TRUE&x_customer_ip=127.0.0.1&x_trans_id=83138C97-896F-46F6-BAFC-8E94D0BAACC6','MANUAL GATEWAY SAID OK',GETDATE(),'140 Stump Dr','Pennsylvania','PA','15012','724-930-9470','26 Antheon Street','Athens','Greece','Alisauskas','Steve','Alishio','Arnold','steve_alisauskas@gmail.com')

insert into Orders (OrderNumber,CustomerID,CustomerGUID,SkinID,ShippingMethodID,ShippingCalculationID,CardType,OrderSubtotal,OrderTax,OrderShippingCosts,OrderTotal,PaymentGateway,AuthorizationResult,AuthorizationPNREF,TransactionCommand,PaymentMethod,TransactionState,AVSResult,LocaleSetting,AlreadyConfirmed,CartType,AuthorizedOn,OrderWeight,CapturedOn,Last4,RTShipRequest,RTShipResponse,LastIPAddress,TransactionType,ReceiptEmailSentOn,AuthorizationCode,OkToEmail,RegisterDate,ShippingMethod,StoreVersion,BillingAddress1,BillingCity,BillingState,BillingZip,BillingPhone,ShippingAddress1,ShippingCity,ShippingCountry,BillingLastName,BillingFirstName,ShippingLastName,ShippingFirstName,Email,RefundTXCommand,RefundTXResult,RefundedOn,RefundReason)
values (100139,58640,'78659F0E-44E5-4A5A-BB5A-083962B5DC6C',1,4,8,'VISA',39.99,8.91,4.55,53.45,'MANUAL','MANUAL GATEWAY SAID OK','83138C97-896F-46F6-BAFC-8E94D0BAACC6','x_type=AUTH_CAPTURE&x_test_request=TRUE&x_description=AspDotNetStorefront+Order+100137&x_amount=53.45&x_card_num=****1111&x_card_code=***&x_exp_date=03/2010&x_phone=***-456-7890&x_fax=&x_customer_tax_id=&x_cust_id=58640&x_invoice_num=100137&x_email=admin%40aspdotnetstorefront.com&x_customer_ip=127.0.0.1&x_first_name=Admin&x_last_name=User&x_company=&x_address=***+Main+St&x_city=New+York&x_state=NY&x_zip=10002&x_country=United+States&x_ship_to_first_name=Admin&x_ship_to_last_name=User&x_ship_to_company=&x_ship_to_address=***+Main+St&x_ship_to_city=New+York&x_ship_to_state=NY&x_ship_to_zip=10002&x_ship_to_country=United+States&x_customer_ip=127.0.0.1','CREDITCARD','REFUNDED','OK','en-US',1,0,'2008-11-14 18:15:58.807',1,'2008-11-14 18:15:58.947',1111,'<UPSRequest><AccessRequest xml:lang="en-us"><AccessLicenseNumber>FB91B6D3F6A6AFC8</AccessLicenseNumber><UserId>mikecoburn</UserId><Password>j1i1m1i1</Password></AccessRequest><RatingServiceSelectionRequest xml:lang="en-US"><Request><RequestAction>Rate</RequestAction><RequestOption>Shop</RequestOption><TransactionReference><CustomerContext>Rating and Service</CustomerContext><XpciVersion>1.0001</XpciVersion></TransactionReference></Request><PickupType><Code>01</Code></PickupType><Shipment><Shipper><Address><City>DAYTON</City><StateProvinceCode>OH</StateProvinceCode><PostalCode>45459</PostalCode><CountryCode>US</CountryCode></Address></Shipper><ShipTo><Address><City>NEW YORK</City><StateProvinceCode>WA</StateProvinceCode><PostalCode>10451</PostalCode><CountryCode>US</CountryCode><ResidentialAddressIndicator/></Address></ShipTo><ShipmentWeight><UnitOfMeasurement><Code>LBS</Code></UnitOfMeasurement><Weight>1.5</Weight></ShipmentWeight><Package><PackagingType><Code>02</Code></PackagingType><Dimensions><UnitOfMeasurement><Code>IN</Code></UnitOfMeasurement><Length>0</Length><Width>0</Width><Height>0</Height></Dimensions><Description>1</Description><PackageWeight><UnitOfMeasure><Code>LBS</Code></UnitOfMeasure><Weight>1.5</Weight></PackageWeight><OversizePackage /></Package><ShipmentServiceOptions/></Shipment></RatingServiceSelectionRequest></UPSRequest><USPSRequest><RateV2Request USERID="758PARAC7739"><Package ID="1-0"><Service>Express</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-1"><Service>Priority</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-2"><Service>Parcel</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-3"><Service>Library</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-4"><Service>Media</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package></RateV2Request></USPSRequest><FedExRequest><FDXRateAvailableServicesRequest xmlns:api="http://www.fedex.com/fsmapi" xmlns:xsi="http://www.w3.org/2001/XmlSchema-instance" xsi:noNamespaceSchemaLocation="FDXRateAvailableServicesRequest.xsd"><RequestHeader><CustomerTransactionIdentifier>RatesRequest</CustomerTransactionIdentifier><AccountNumber>183045091</AccountNumber><MeterNumber>7298194</MeterNumber><CarrierCode></CarrierCode></RequestHeader><ShipDate>2008-10-18</ShipDate><DropoffType>REGULARPICKUP</DropoffType><Packaging>YOURPACKAGING</Packaging><WeightUnits>LBS</WeightUnits><ListRate>false</ListRate><Weight>1.5</Weight><OriginAddress><StateOrProvinceCode>OH</StateOrProvinceCode><PostalCode>45459</PostalCode><CountryCode>US</CountryCode></OriginAddress><DestinationAddress><StateOrProvinceCode>WA</StateOrProvinceCode><PostalCode>10451</PostalCode><CountryCode>US</CountryCode></DestinationAddress><Payment><PayorType>SENDER</PayorType></Payment><PackageCount>1</PackageCount></FDXRateAvailableServicesRequest></FedExRequest><DHLRequest><ECommerce xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" action="Request" version="1.1"><Requestor><ID /><Password /></Requestor><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>G</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Ground Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>S</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL 2nd Day Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>N</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 3:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 12:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight><SpecialServices><SpecialService><Code>1030</Code></SpecialService></SpecialServices></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 10:30 am</TransactionTrace></Shipment></ECommerce></DHLRequest>','<UPSResponse><RatingServiceSelectionResponse><Response><TransactionReference><CustomerContext>Rating and Service</CustomerContext><XpciVersion>1.0001</XpciVersion></TransactionReference><ResponseStatusCode>0</ResponseStatusCode><ResponseStatusDescription>Failure</ResponseStatusDescription><Error><ErrorSeverity>Hard</ErrorSeverity><ErrorCode>111285</ErrorCode><ErrorDescription>The postal code 10451 is invalid for WA United States.</ErrorDescription></Error></Response></RatingServiceSelectionResponse></UPSResponse><USPSResponse>  <RateV2Response><Package ID="1-0"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Express Mail</MailService><Rate>24.65</Rate></Postage></Package><Package ID="1-1"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Priority Mail</MailService><Rate>5.60</Rate></Postage></Package><Package ID="1-2"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Machinable>FALSE</Machinable><Zone>4</Zone><Postage><MailService>Parcel Post</MailService><Rate>9.05</Rate></Postage></Package><Package ID="1-3"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Library Mail</MailService><Rate>2.45</Rate></Postage></Package><Package ID="1-4"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Media Mail</MailService><Rate>2.58</Rate></Postage></Package></RateV2Response>  </USPSResponse><FedExResponse><FDXRateAvailableServicesReply xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FDXRateAvailableServicesReply.xsd"><ReplyHeader><CustomerTransactionIdentifier>RatesRequest</CustomerTransactionIdentifier></ReplyHeader><Error><Code>61468</Code><Message>Recipient postal code does not match recipient state/province code.</Message></Error></FDXRateAvailableServicesReply></FedExResponse><DHLResponse><ECommerce xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" action="Response" version="1.1" timestamp="2008/10/17T03:21:22" transmission_reference="680E6900"><Faults><Fault><Code>1002</Code><Description><![CDATA[Required element/node is missing.]]></Description><Context><![CDATA[The required node <ID> is missing, empty, or contains a value that is not the correct data type.]]></Context></Fault></Faults><Requestor><ID/><Password/></Requestor><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>G</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Ground Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>S</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL 2nd Day Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>N</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 3:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 12:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight><SpecialServices><SpecialService><Code>1030</Code></SpecialService></SpecialServices></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 10:30 am</TransactionTrace></Shipment></ECommerce>  </DHLResponse>','127.0.0.1',1,GETDATE(),0,1,'2008-09-22 12:02:23.000','Parcel Select BMC and OBMC Presort','AspDotNetStorefront ML 8.0.1.2/8.0.1.2','140 Stump Dr','Pennsylvania','PA','15012','724-930-9470','26 Antheon Street','Athens','Greece','Alisauskas','Steve','Alishio','Arnold','steve_alisauskas@gmail.com','x_type=CREDIT&x_test_request=TRUE&x_trans_id=3689A94A-5109-4E5F-BADF-711B83D91687&x_amount=39.99&x_customer_ip=127.0.0.1&x_card_num=1111','MANUAL GATEWAY SAID OK',getdate(),'NOTHING')

insert into Orders (OrderNumber,CustomerID,CustomerGUID,SkinID,ShippingMethodID,ShippingCalculationID,CardType,OrderSubtotal,OrderTax,OrderShippingCosts,OrderTotal,PaymentGateway,AuthorizationResult,AuthorizationPNREF,TransactionCommand,PaymentMethod,TransactionState,AVSResult,LocaleSetting,AlreadyConfirmed,CartType,AuthorizedOn,OrderWeight,CapturedOn,Last4,RTShipRequest,RTShipResponse,LastIPAddress,TransactionType,ReceiptEmailSentOn,AuthorizationCode,OkToEmail,RegisterDate,ShippingMethod,StoreVersion,BillingAddress1,BillingCity,BillingState,BillingZip,BillingPhone,ShippingAddress1,ShippingCity,ShippingCountry,BillingLastName,BillingFirstName,ShippingLastName,ShippingFirstName,Email,ShippingTrackingNumber,ShippedVIA,ShippedOn)
values (100140,58640,'78659F0E-44E5-4A5A-BB5A-083962B5DC6C',1,4,8,'VISA',39.99,8.91,4.55,53.45,'MANUAL','MANUAL GATEWAY SAID OK','83138C97-896F-46F6-BAFC-8E94D0BAACC6','x_type=AUTH_CAPTURE&x_test_request=TRUE&x_description=AspDotNetStorefront+Order+100137&x_amount=53.45&x_card_num=****1111&x_card_code=***&x_exp_date=03/2010&x_phone=***-456-7890&x_fax=&x_customer_tax_id=&x_cust_id=58640&x_invoice_num=100137&x_email=admin%40aspdotnetstorefront.com&x_customer_ip=127.0.0.1&x_first_name=Admin&x_last_name=User&x_company=&x_address=***+Main+St&x_city=New+York&x_state=NY&x_zip=10002&x_country=United+States&x_ship_to_first_name=Admin&x_ship_to_last_name=User&x_ship_to_company=&x_ship_to_address=***+Main+St&x_ship_to_city=New+York&x_ship_to_state=NY&x_ship_to_zip=10002&x_ship_to_country=United+States&x_customer_ip=127.0.0.1','CREDITCARD','CAPTURED','OK','en-US',1,0,'2008-11-14 18:15:58.807',1,'2008-11-14 18:15:58.947',1111,'<UPSRequest><AccessRequest xml:lang="en-us"><AccessLicenseNumber>FB91B6D3F6A6AFC8</AccessLicenseNumber><UserId>mikecoburn</UserId><Password>j1i1m1i1</Password></AccessRequest><RatingServiceSelectionRequest xml:lang="en-US"><Request><RequestAction>Rate</RequestAction><RequestOption>Shop</RequestOption><TransactionReference><CustomerContext>Rating and Service</CustomerContext><XpciVersion>1.0001</XpciVersion></TransactionReference></Request><PickupType><Code>01</Code></PickupType><Shipment><Shipper><Address><City>DAYTON</City><StateProvinceCode>OH</StateProvinceCode><PostalCode>45459</PostalCode><CountryCode>US</CountryCode></Address></Shipper><ShipTo><Address><City>NEW YORK</City><StateProvinceCode>WA</StateProvinceCode><PostalCode>10451</PostalCode><CountryCode>US</CountryCode><ResidentialAddressIndicator/></Address></ShipTo><ShipmentWeight><UnitOfMeasurement><Code>LBS</Code></UnitOfMeasurement><Weight>1.5</Weight></ShipmentWeight><Package><PackagingType><Code>02</Code></PackagingType><Dimensions><UnitOfMeasurement><Code>IN</Code></UnitOfMeasurement><Length>0</Length><Width>0</Width><Height>0</Height></Dimensions><Description>1</Description><PackageWeight><UnitOfMeasure><Code>LBS</Code></UnitOfMeasure><Weight>1.5</Weight></PackageWeight><OversizePackage /></Package><ShipmentServiceOptions/></Shipment></RatingServiceSelectionRequest></UPSRequest><USPSRequest><RateV2Request USERID="758PARAC7739"><Package ID="1-0"><Service>Express</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-1"><Service>Priority</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-2"><Service>Parcel</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-3"><Service>Library</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-4"><Service>Media</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package></RateV2Request></USPSRequest><FedExRequest><FDXRateAvailableServicesRequest xmlns:api="http://www.fedex.com/fsmapi" xmlns:xsi="http://www.w3.org/2001/XmlSchema-instance" xsi:noNamespaceSchemaLocation="FDXRateAvailableServicesRequest.xsd"><RequestHeader><CustomerTransactionIdentifier>RatesRequest</CustomerTransactionIdentifier><AccountNumber>183045091</AccountNumber><MeterNumber>7298194</MeterNumber><CarrierCode></CarrierCode></RequestHeader><ShipDate>2008-10-18</ShipDate><DropoffType>REGULARPICKUP</DropoffType><Packaging>YOURPACKAGING</Packaging><WeightUnits>LBS</WeightUnits><ListRate>false</ListRate><Weight>1.5</Weight><OriginAddress><StateOrProvinceCode>OH</StateOrProvinceCode><PostalCode>45459</PostalCode><CountryCode>US</CountryCode></OriginAddress><DestinationAddress><StateOrProvinceCode>WA</StateOrProvinceCode><PostalCode>10451</PostalCode><CountryCode>US</CountryCode></DestinationAddress><Payment><PayorType>SENDER</PayorType></Payment><PackageCount>1</PackageCount></FDXRateAvailableServicesRequest></FedExRequest><DHLRequest><ECommerce xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" action="Request" version="1.1"><Requestor><ID /><Password /></Requestor><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>G</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Ground Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>S</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL 2nd Day Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>N</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 3:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 12:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight><SpecialServices><SpecialService><Code>1030</Code></SpecialService></SpecialServices></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 10:30 am</TransactionTrace></Shipment></ECommerce></DHLRequest>','<UPSResponse><RatingServiceSelectionResponse><Response><TransactionReference><CustomerContext>Rating and Service</CustomerContext><XpciVersion>1.0001</XpciVersion></TransactionReference><ResponseStatusCode>0</ResponseStatusCode><ResponseStatusDescription>Failure</ResponseStatusDescription><Error><ErrorSeverity>Hard</ErrorSeverity><ErrorCode>111285</ErrorCode><ErrorDescription>The postal code 10451 is invalid for WA United States.</ErrorDescription></Error></Response></RatingServiceSelectionResponse></UPSResponse><USPSResponse>  <RateV2Response><Package ID="1-0"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Express Mail</MailService><Rate>24.65</Rate></Postage></Package><Package ID="1-1"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Priority Mail</MailService><Rate>5.60</Rate></Postage></Package><Package ID="1-2"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Machinable>FALSE</Machinable><Zone>4</Zone><Postage><MailService>Parcel Post</MailService><Rate>9.05</Rate></Postage></Package><Package ID="1-3"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Library Mail</MailService><Rate>2.45</Rate></Postage></Package><Package ID="1-4"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Media Mail</MailService><Rate>2.58</Rate></Postage></Package></RateV2Response>  </USPSResponse><FedExResponse><FDXRateAvailableServicesReply xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FDXRateAvailableServicesReply.xsd"><ReplyHeader><CustomerTransactionIdentifier>RatesRequest</CustomerTransactionIdentifier></ReplyHeader><Error><Code>61468</Code><Message>Recipient postal code does not match recipient state/province code.</Message></Error></FDXRateAvailableServicesReply></FedExResponse><DHLResponse><ECommerce xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" action="Response" version="1.1" timestamp="2008/10/17T03:21:22" transmission_reference="680E6900"><Faults><Fault><Code>1002</Code><Description><![CDATA[Required element/node is missing.]]></Description><Context><![CDATA[The required node <ID> is missing, empty, or contains a value that is not the correct data type.]]></Context></Fault></Faults><Requestor><ID/><Password/></Requestor><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>G</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Ground Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>S</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL 2nd Day Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>N</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 3:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 12:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight><SpecialServices><SpecialService><Code>1030</Code></SpecialService></SpecialServices></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 10:30 am</TransactionTrace></Shipment></ECommerce>  </DHLResponse>','127.0.0.1',1,GETDATE(),0,1,'2008-09-22 12:02:23.000','Parcel Select BMC and OBMC Presort','AspDotNetStorefront ML 8.0.1.2/8.0.1.2','140 Stump Dr','Pennsylvania','PA','15012','724-930-9470','26 Antheon Street','Athens','Greece','Alisauskas','Steve','Alishio','Arnold','steve_alisauskas@gmail.com',123456,'FEDEX Express',GETDATE())

insert into Orders (OrderNumber,CustomerID,CustomerGUID,SkinID,ShippingMethodID,ShippingCalculationID,CardType,OrderSubtotal,OrderTax,OrderShippingCosts,OrderTotal,PaymentGateway,AuthorizationResult,AuthorizationPNREF,TransactionCommand,PaymentMethod,TransactionState,AVSResult,LocaleSetting,AlreadyConfirmed,CartType,AuthorizedOn,OrderWeight,CapturedOn,Last4,RTShipRequest,RTShipResponse,LastIPAddress,TransactionType,ReceiptEmailSentOn,AuthorizationCode,OkToEmail,RegisterDate,ShippingMethod,StoreVersion,BillingAddress1,BillingCity,BillingState,BillingZip,BillingPhone,ShippingAddress1,ShippingCity,ShippingCountry,BillingLastName,BillingFirstName,ShippingLastName,ShippingFirstName,Email)
values (100141,58640,'78659F0E-44E5-4A5A-BB5A-083962B5DC6C',1,4,8,'VISA',39.99,8.91,4.55,53.45,'MANUAL','MANUAL GATEWAY SAID OK','83138C97-896F-46F6-BAFC-8E94D0BAACC6','x_type=AUTH_CAPTURE&x_test_request=TRUE&x_description=AspDotNetStorefront+Order+100137&x_amount=53.45&x_card_num=****1111&x_card_code=***&x_exp_date=03/2010&x_phone=***-456-7890&x_fax=&x_customer_tax_id=&x_cust_id=58640&x_invoice_num=100137&x_email=admin%40aspdotnetstorefront.com&x_customer_ip=127.0.0.1&x_first_name=Admin&x_last_name=User&x_company=&x_address=***+Main+St&x_city=New+York&x_state=NY&x_zip=10002&x_country=United+States&x_ship_to_first_name=Admin&x_ship_to_last_name=User&x_ship_to_company=&x_ship_to_address=***+Main+St&x_ship_to_city=New+York&x_ship_to_state=NY&x_ship_to_zip=10002&x_ship_to_country=United+States&x_customer_ip=127.0.0.1','CREDITCARD','CAPTURED','OK','en-US',1,0,'2008-11-14 18:15:58.807',1,'2008-11-14 18:15:58.947',1111,'<UPSRequest><AccessRequest xml:lang="en-us"><AccessLicenseNumber>FB91B6D3F6A6AFC8</AccessLicenseNumber><UserId>mikecoburn</UserId><Password>j1i1m1i1</Password></AccessRequest><RatingServiceSelectionRequest xml:lang="en-US"><Request><RequestAction>Rate</RequestAction><RequestOption>Shop</RequestOption><TransactionReference><CustomerContext>Rating and Service</CustomerContext><XpciVersion>1.0001</XpciVersion></TransactionReference></Request><PickupType><Code>01</Code></PickupType><Shipment><Shipper><Address><City>DAYTON</City><StateProvinceCode>OH</StateProvinceCode><PostalCode>45459</PostalCode><CountryCode>US</CountryCode></Address></Shipper><ShipTo><Address><City>NEW YORK</City><StateProvinceCode>WA</StateProvinceCode><PostalCode>10451</PostalCode><CountryCode>US</CountryCode><ResidentialAddressIndicator/></Address></ShipTo><ShipmentWeight><UnitOfMeasurement><Code>LBS</Code></UnitOfMeasurement><Weight>1.5</Weight></ShipmentWeight><Package><PackagingType><Code>02</Code></PackagingType><Dimensions><UnitOfMeasurement><Code>IN</Code></UnitOfMeasurement><Length>0</Length><Width>0</Width><Height>0</Height></Dimensions><Description>1</Description><PackageWeight><UnitOfMeasure><Code>LBS</Code></UnitOfMeasure><Weight>1.5</Weight></PackageWeight><OversizePackage /></Package><ShipmentServiceOptions/></Shipment></RatingServiceSelectionRequest></UPSRequest><USPSRequest><RateV2Request USERID="758PARAC7739"><Package ID="1-0"><Service>Express</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-1"><Service>Priority</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-2"><Service>Parcel</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-3"><Service>Library</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package><Package ID="1-4"><Service>Media</Service><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>Regular</Size><Machinable>False</Machinable></Package></RateV2Request></USPSRequest><FedExRequest><FDXRateAvailableServicesRequest xmlns:api="http://www.fedex.com/fsmapi" xmlns:xsi="http://www.w3.org/2001/XmlSchema-instance" xsi:noNamespaceSchemaLocation="FDXRateAvailableServicesRequest.xsd"><RequestHeader><CustomerTransactionIdentifier>RatesRequest</CustomerTransactionIdentifier><AccountNumber>183045091</AccountNumber><MeterNumber>7298194</MeterNumber><CarrierCode></CarrierCode></RequestHeader><ShipDate>2008-10-18</ShipDate><DropoffType>REGULARPICKUP</DropoffType><Packaging>YOURPACKAGING</Packaging><WeightUnits>LBS</WeightUnits><ListRate>false</ListRate><Weight>1.5</Weight><OriginAddress><StateOrProvinceCode>OH</StateOrProvinceCode><PostalCode>45459</PostalCode><CountryCode>US</CountryCode></OriginAddress><DestinationAddress><StateOrProvinceCode>WA</StateOrProvinceCode><PostalCode>10451</PostalCode><CountryCode>US</CountryCode></DestinationAddress><Payment><PayorType>SENDER</PayorType></Payment><PackageCount>1</PackageCount></FDXRateAvailableServicesRequest></FedExRequest><DHLRequest><ECommerce xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" action="Request" version="1.1"><Requestor><ID /><Password /></Requestor><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>G</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Ground Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>S</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL 2nd Day Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>N</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 3:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 12:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr /></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight><SpecialServices><SpecialService><Code>1030</Code></SpecialService></SpecialServices></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr /></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions /><TransactionTrace>DHL Next Day 10:30 am</TransactionTrace></Shipment></ECommerce></DHLRequest>','<UPSResponse><RatingServiceSelectionResponse><Response><TransactionReference><CustomerContext>Rating and Service</CustomerContext><XpciVersion>1.0001</XpciVersion></TransactionReference><ResponseStatusCode>0</ResponseStatusCode><ResponseStatusDescription>Failure</ResponseStatusDescription><Error><ErrorSeverity>Hard</ErrorSeverity><ErrorCode>111285</ErrorCode><ErrorDescription>The postal code 10451 is invalid for WA United States.</ErrorDescription></Error></Response></RatingServiceSelectionResponse></UPSResponse><USPSResponse>  <RateV2Response><Package ID="1-0"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Express Mail</MailService><Rate>24.65</Rate></Postage></Package><Package ID="1-1"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Priority Mail</MailService><Rate>5.60</Rate></Postage></Package><Package ID="1-2"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Machinable>FALSE</Machinable><Zone>4</Zone><Postage><MailService>Parcel Post</MailService><Rate>9.05</Rate></Postage></Package><Package ID="1-3"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Library Mail</MailService><Rate>2.45</Rate></Postage></Package><Package ID="1-4"><ZipOrigination>45459</ZipOrigination><ZipDestination>10451</ZipDestination><Pounds>1</Pounds><Ounces>8</Ounces><Size>REGULAR</Size><Zone>4</Zone><Postage><MailService>Media Mail</MailService><Rate>2.58</Rate></Postage></Package></RateV2Response>  </USPSResponse><FedExResponse><FDXRateAvailableServicesReply xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FDXRateAvailableServicesReply.xsd"><ReplyHeader><CustomerTransactionIdentifier>RatesRequest</CustomerTransactionIdentifier></ReplyHeader><Error><Code>61468</Code><Message>Recipient postal code does not match recipient state/province code.</Message></Error></FDXRateAvailableServicesReply></FedExResponse><DHLResponse><ECommerce xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" action="Response" version="1.1" timestamp="2008/10/17T03:21:22" transmission_reference="680E6900"><Faults><Fault><Code>1002</Code><Description><![CDATA[Required element/node is missing.]]></Description><Context><![CDATA[The required node <ID> is missing, empty, or contains a value that is not the correct data type.]]></Context></Fault></Faults><Requestor><ID/><Password/></Requestor><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>G</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Ground Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>S</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL 2nd Day Service</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>N</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 3:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 12:00 pm</TransactionTrace></Shipment><Shipment action="RateEstimate" version="1.0"><ShippingCredentials><ShippingKey>54233F2B2C41554143565B555D503053414A544B4258565D50</ShippingKey><AccountNbr/></ShippingCredentials><ShipmentDetail><ShipDate>2008-10-20</ShipDate><Service><Code>E</Code></Service><ShipmentType><Code>P</Code></ShipmentType><Weight>2</Weight><SpecialServices><SpecialService><Code>1030</Code></SpecialService></SpecialServices></ShipmentDetail><Billing><Party><Code>S</Code></Party><AccountNbr/></Billing><Receiver><Address><State>WA</State><Country>US</Country><PostalCode>10451</PostalCode></Address></Receiver><ShipmentProcessingInstructions/><TransactionTrace>DHL Next Day 10:30 am</TransactionTrace></Shipment></ECommerce>  </DHLResponse>','127.0.0.1',1,GETDATE(),0,1,'2008-09-22 12:02:23.000','Parcel Select BMC and OBMC Presort','AspDotNetStorefront ML 8.0.1.2/8.0.1.2','140 Stump Dr','Pennsylvania','PA','15012','724-930-9470','26 Antheon Street','Athens','Greece','Alisauskas','Steve','Alishio','Arnold','steve_alisauskas@gmail.com')

insert into orders (OrderNumber,CustomerID,CustomerGUID,SkinID,ShippedOn,LastName,FirstName,Email,BillingLastName,BillingFirstName,BillingAddress1,BillingCity,BillingState,BillingZip,BillingCountry,BillingPhone,ShippingLastName,ShippingFirstName,ShippingCompany,ShippingResidenceType,ShippingAddress1,ShippingAddress2,ShippingSuite,ShippingCity,ShippingState,ShippingZip,ShippingCountry,ShippingMethodID,ShippingMethod,ShippingPhone,ShippingCalculationID,Phone,RegisterDate,OkToEmail,Deleted,CardType,CardName,CardNumber,CardExpirationMonth,CardExpirationYear,OrderSubtotal,OrderTax,OrderTotal,PaymentGateWay,AuthorizationResult,AuthorizationPNREF,TransactionCommand,OrderDate,LastIPAddress,PaymentMethod,ReceiptEmailSentOn,TransactionState,AVSResult,OrderWeight,LocaleSetting,FinalizationData,AlreadyConfirmed,CartType,Last4,ReadyToShip,IsPrinted,AuthorizedOn,CapturedOn,TransactionType,StoreVersion)
values (100156,58639,'F40420EB-6B38-4F4C-9D57-164A7C1F2DB1',1,NULL,'User','Admin','admin@admin.com','User','Admin','123 Main St','New York','NY',10451,'United States',123-456-7890,NULL,NULL,NULL,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,NULL,NULL,8,911,'2008-11-17 09:34:01.000',1,0,'VISA','Admin User','Not Stored',04,2010,593.98,74.25,668.23,'MANUAL','MANUAL GATEWAY SAID OK','1722927B-5ABD-466D-9BED-624ED6BC788A','x_type=AUTH_CAPTURE&x_test_request=TRUE&x_description=AspDotNetStorefront+Order+100156&x_amount=668.23&x_card_num=****1111&x_card_code=***&x_exp_date=04/2010&x_phone=***-456-7890&x_fax=&x_customer_tax_id=&x_cust_id=58639&x_invoice_num=100156&x_email=admin%40aspdotnetstorefront.com&x_customer_ip=127.0.0.1&x_first_name=Admin&x_last_name=User&x_company=&x_address=***+Main+St&x_city=New+York&x_state=NY&x_zip=10451&x_country=United+States&x_customer_ip=127.0.0.1',GETDATE(),'127.0.0.1','CREDITCARD',GETDATE(),'CAPTURED','OK',1,'en-US','<root></root>',1,0,1111,0,0,GETDATE(),GETDATE(),1,'AspDotNetStorefront ML 8.0.1.2/8.0.1.2')


--_OrderOption
SET IDENTITY_INSERT [dbo].OrderOption ON;
INSERT INTO [dbo].OrderOption (OrderOptionID,Name,Description,Cost,DisplayOrder,TaxClassID) VALUES (1,'Gift Wrap','Gift Wrap your present',10.00,1,5)
SET IDENTITY_INSERT [dbo].OrderOption OFF;

--_MicroPay
-- MICROPAY System Product:
INSERT [dbo].Product(Name,Description,ProductTypeID,SalesPromptID,SKU,ManufacturerPartNumber,Published,XmlPackage) values('Add $5 to my MicroPay account','Use this product to add to your micropay balance.',1,1,'MICROPAY','MICROPAY',1,'product.simpleproduct.xml.config');
go
INSERT [dbo].ProductVariant(ProductID,Name,IsDefault) select ProductID,SKU,1 from Product where SKU='MICROPAY';
go
update [dbo].ProductVariant set Name='',Description='',SKUSuffix='',Price=5.00,Inventory=100000,Published=1,IsDownload=0,FreeShipping=1,DownloadLocation=NULL where Name='MICROPAY';
GO
update [dbo].Product set IsSystem=1 where SKU='MICROPAY';
go
