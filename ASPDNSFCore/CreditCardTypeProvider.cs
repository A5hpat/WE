// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
	public class CreditCardTypeProvider
	{
		public IEnumerable<string> GetCardTypesRequiringIssueNumber()
		{
			return new[]
			{
				"SOLO",
				"MAESTRO",
			};
		}

		public IEnumerable<string> GetAcceptedCreditCardTypes()
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "select CardType from CreditCardType with(nolock) where Accepted = 1 and CardType is not null order by CardType";
				connection.Open();

				using(var reader = command.ExecuteReader())
					while(reader.Read())
						yield return (string)reader["CardType"];
			}
		}
	}
}
