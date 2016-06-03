// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Classes
{
	public class CreditCardSelectListBuilder
	{
		const string SelectListValueField = "Value";
		const string SelectListDataField = "Text";

		public SelectList BuildExpirationMonthSelectList()
		{
			return new SelectList(
				items: Enumerable.Range(1, 12)
					.Select(monthIndex => new SelectListItem
					{
						Text = monthIndex.ToString(),
						Value = monthIndex.ToString()
					}),
				dataValueField: SelectListValueField,
				dataTextField: SelectListDataField);
		}

		public SelectList BuildExpirationYearSelectList()
		{
			return new SelectList(
				items: Enumerable.Range(0, 10)
					.Select(yearIndex =>
					{
						var year = (DateTime.Now.Year + yearIndex).ToString();
						return new SelectListItem
						{
							Text = year,
							Value = year
						};
					}),
				dataValueField: SelectListValueField,
				dataTextField: SelectListDataField);
		}

		public SelectList BuildCreditCardTypeSelectList()
		{
			return new SelectList(
				items: GetCreditCardTypes()
					.Select(cardType => new SelectListItem
					{
						Text = cardType,
						Value = cardType
					}),
				dataValueField: SelectListValueField,
				dataTextField: SelectListDataField);
		}

		IEnumerable<string> GetCreditCardTypes()
		{
			var query = @"
				select CardType
				from CreditCardType with (nolock)
				where Accepted = 1
				order by CardType";

			using(var connection = DB.dbConn())
			{
				connection.Open();
				using(var reader = DB.GetRS(query, connection))
					while(reader.Read())
						yield return DB.RSField(reader, "CardType");
			}
		}
	}
}
