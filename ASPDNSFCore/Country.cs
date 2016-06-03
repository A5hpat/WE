// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
	public class Country
	{
		public readonly int ID;
		public readonly Guid CountryGuid;
		public readonly string Name;
		public readonly string TwoLetterIsoCode;
		public readonly string ThreeLetterIsoCode;
		public readonly string NumericIsoCode;
		public readonly bool Published;
		public readonly int DisplayOrder;
		public readonly string ExtensionData;
		public readonly bool PostalCodeRequired;
		public readonly string PostalCodeRegex;
		public readonly string PostalCodeExample;
		public readonly DateTime CreatedOn;
		public readonly DateTime UpdatedOn;

		public Country(
			int id,
			Guid countryGuid,
			string name,
			string twoLetterIsoCode,
			string threeLetterIsoCode,
			string numericIsoCode,
			bool published,
			int displayOrder,
			string extensionData,
			bool postalCodeRequired,
			string postalCodeRegex,
			string postalCodeExample,
			DateTime createdOn,
			DateTime updatedOn)
		{
			ID = id;
			CountryGuid = countryGuid;
			Name = name;
			TwoLetterIsoCode = twoLetterIsoCode;
			ThreeLetterIsoCode = threeLetterIsoCode;
			NumericIsoCode = numericIsoCode;
			Published = published;
			DisplayOrder = displayOrder;
			ExtensionData = extensionData;
			PostalCodeRequired = postalCodeRequired;
			PostalCodeRegex = postalCodeRegex;
			PostalCodeExample = postalCodeExample;
			CreatedOn = createdOn;
			UpdatedOn = updatedOn;
		}

		public static List<Country> GetAll()
		{
			var countries = new List<Country>();

			using(var connection = DB.dbConn())
			using(var command = connection.CreateCommand())
			{
				command.CommandText = @"
					select 
						CountryID, 
						CountryGUID,
						Name,
						TwoLetterISOCode,
						ThreeLetterISOCode,
						NumericISOCode,
						Published,
						DisplayOrder,
						ExtensionData,
						PostalCodeRequired,
						PostalCodeRegex,
						PostalCodeExample,
						CreatedOn,
						UpdatedOn
					from Country
					where Published = 1 
					order by DisplayOrder, Name";

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
						countries.Add(new Country(
							id: reader.FieldInt("CountryID"),
							countryGuid: reader.FieldGuid("CountryGUID"),
							name: reader.Field("Name"),
							twoLetterIsoCode: reader.Field("TwoLetterISOCode"),
							threeLetterIsoCode: reader.Field("ThreeLetterISOCode"),
							numericIsoCode: reader.Field("NumericISOCode"),
							published: reader.FieldBool("Published"),
							displayOrder: reader.FieldInt("DisplayOrder"),
							extensionData: reader.Field("ExtensionData"),
							postalCodeRequired: reader.FieldBool("PostalCodeRequired"),
							postalCodeRegex: reader.Field("PostalCodeRegex"),
							postalCodeExample: reader.Field("PostalCodeExample"),
							createdOn: reader.FieldDateTime("CreatedOn"),
							updatedOn: reader.FieldDateTime("UpdatedOn")));
			}

			return countries;
		}
	}

	public class State
	{
		public readonly string Abbreviation;
		public readonly string Name;

		public State(string name, string abbreviation)
		{
			Name = name;
			Abbreviation = abbreviation;
		}

		public static IEnumerable<State> GetAllStatesForCountry(int countryId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = "select Abbreviation, Name from State where Published = 1 and CountryID = @countryId order by displayOrder, Name";
				command.Parameters.AddWithValue("@countryId", countryId);

				connection.Open();
				using(var reader = command.ExecuteReader())
				{
					if(!reader.Read())
					{
						yield return new State(
							name: "Other (Non U.S.)",
							abbreviation: "--");
						yield break;
					}

					do
					{
						yield return new State(
							name: DB.RSField(reader, "Name"),
							abbreviation: DB.RSField(reader, "Abbreviation"));
					} while(reader.Read());
				}
			}
		}
	}
}
