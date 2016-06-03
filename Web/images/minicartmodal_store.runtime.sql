/************************************  SQL Statement and parameters for query ShoppingCart  ************************************/

declare @customerid Int
declare @StoreID Int

set @customerid = 0
set @StoreID = 1


exec dbo.nalpac_CartItems 0, @customerid, 0, 0, @storeid

/************************************  SQL Statement and parameters for query ShoppingCartItems  ************************************/

declare @customerid Int
declare @StoreID Int

set @customerid = 0
set @StoreID = 1


exec dbo.nalpac_CartCountItems 0, @customerid, 0, 0, @storeid

/************************************  SQL Statement and parameters for query ShoppingCart  ************************************/

declare @customerid Int
declare @StoreID Int

set @customerid = 0
set @StoreID = 1


exec dbo.nalpac_CartSubtotal 0, @customerid, 0, 0, @storeid

