// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
	public class GiftCardManager
	{
		public GiftCard LoadByShoppingCartRecordId(int shoppingCartRecordId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "select top 1 GiftCardID from GiftCard where ShoppingCartRecId = @shoppingCartRecId";
				command.Parameters.AddWithValue("shoppingCartRecId", shoppingCartRecordId);

				connection.Open();

				var giftCardId = command.ExecuteScalar();
				if(giftCardId == null || giftCardId is DBNull)
					return null;

				return new GiftCard((int)giftCardId);
			}
		}
	}
}
