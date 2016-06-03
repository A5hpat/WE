/************************************  SQL Statement and parameters for query customers  ************************************/

declare @customerid Int
declare @blah VarChar

set @customerid = 59709
set @blah = '0'


select COALESCE(FirstName, '') as FirstName , coalesce(LastName, '') as LastName, coalesce(customerid, 0) as customerid
from customer
where customerid = @customerid

