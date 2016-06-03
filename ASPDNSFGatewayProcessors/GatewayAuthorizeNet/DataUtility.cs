// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
using GatewayAuthorizeNet.AuthorizeNetApi;

namespace GatewayAuthorizeNet
{
	public class DataUtility
	{
		public static Int64 GetProfileId(int customerId)
		{
			return DB.GetSqlNLong(
				@"select top 1 CIM_ProfileId as N
					from customer
					where CustomerID = @customerId",
				new SqlParameter("@customerId", customerId));
		}

		public static void SaveProfileId(int customerId, Int64 profileId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(var command = new SqlCommand(
					@"update customer set CIM_ProfileId = @profileId
						where CustomerID = @customerId", connection))
				{
					command.Parameters.AddRange(new[]
						{
							new SqlParameter("@profileId", profileId),
							new SqlParameter("@customerId", customerId),
						});

					command.ExecuteNonQuery();
				}
			}
		}

		public static AddressPaymentProfileMap GetPaymentProfile(int customerId, Int64 paymentProfileId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(var command = new SqlCommand(
					@"select top 1 AuthorizeNetProfileId, CardType, ExpirationMonth, ExpirationYear, AddressId
						from CIM_AddressPaymentProfileMap
						where CustomerId = @customerId
							and AuthorizeNetProfileID = @paymentProfileId", connection))
				{
					command.Parameters.AddRange(new[]
						{
							new SqlParameter("@customerId", customerId),
							new SqlParameter("@paymentProfileId", paymentProfileId),
						});

					using(var reader = command.ExecuteReader())
					{
						if(!reader.Read())
							return null;

						return new AddressPaymentProfileMap(
							reader.GetInt64(reader.GetOrdinal("AuthorizeNetProfileId")),
							reader.GetString(reader.GetOrdinal("CardType")),
							reader.GetString(reader.GetOrdinal("ExpirationMonth")),
							reader.GetString(reader.GetOrdinal("ExpirationYear")),
							reader.GetInt32(reader.GetOrdinal("AddressId")));
					}
				}
			}
		}

		public static void DeletePaymentProfile(int customerId, Int64 paymentProfileId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(var command = new SqlCommand(
					@"delete from CIM_AddressPaymentProfileMap
						where CustomerId = @customerId
							and AuthorizeNetProfileId = @authorizeNetProfileId", connection))
				{
					command.Parameters.AddRange(new[]
						{
							new SqlParameter("@customerId", customerId),
							new SqlParameter("@authorizeNetProfileId", paymentProfileId),
						});

					command.ExecuteNonQuery();
				}
			}
		}

		public static void SavePaymentProfile(int customerid, int addressId, Int64 paymentProfileId, string expirationMonth, string expirationYear, string cardType)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(var command = new SqlCommand(
					@"if exists(select * from CIM_AddressPaymentProfileMap where CustomerId = @customerId and AuthorizeNetProfileId = @authorizeNetProfileId) 
						update CIM_AddressPaymentProfileMap
							set AddressId = @addressId, ExpirationMonth = @expirationMonth, ExpirationYear = @expirationYear
							where CustomerId = @customerId
								and AuthorizeNetProfileId = @authorizeNetProfileId
					else
						insert into CIM_AddressPaymentProfileMap 
							(CustomerId, AuthorizeNetProfileId, CardType, AddressId, ExpirationMonth, ExpirationYear)
							values (@customerId, @authorizeNetProfileId, @cardType, @addressId, @expirationMonth, @expirationYear)", connection))
				{
					command.Parameters.AddRange(new[]
						{
							new SqlParameter("@customerId", customerid),
							new SqlParameter("@authorizeNetProfileId", paymentProfileId),
							new SqlParameter("@cardType", cardType),
							new SqlParameter("@addressId", addressId),
							new SqlParameter("@expirationMonth", expirationMonth),
							new SqlParameter("@expirationYear", expirationYear),
						});

					command.ExecuteNonQuery();
				}
			}
		}

		public static void SetPrimaryPaymentProfile(int customerId, Int64 paymentProfileId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(var command = new SqlCommand(
					@"update CIM_AddressPaymentProfileMap
						set [Primary] = case when AuthorizeNetProfileId = @authorizeNetProfileId then 1 else 0 end
						where CustomerId = @customerId", connection))
				{
					command.Parameters.AddRange(new[]
						{
							new SqlParameter("@customerId", customerId),
							new SqlParameter("@authorizeNetProfileId", paymentProfileId),
						});

					command.ExecuteNonQuery();
				}
			}
		}

		public static PaymentProfileWrapper GetPaymentProfileWrapper(int customerId, string email, Int64 paymentProfileId)
		{
			var profileID = GetProfileId(customerId);
			var profileMgr = new ProfileManager(customerId, email, profileID);
			var paymentProfile = profileMgr.GetPaymentProfile(paymentProfileId);
			var creditCardMasked = (CreditCardMaskedType)paymentProfile.payment.Item;
			var cimPaymentProfile = GetPaymentProfile(customerId, paymentProfileId);

			if(creditCardMasked != null && cimPaymentProfile != null)
			{
				return new PaymentProfileWrapper()
					{
						CreditCardNumberMasked = creditCardMasked.cardNumber.Replace("XXXX", "****"),
						CardType = cimPaymentProfile.CardType,
						ExpirationMonth = cimPaymentProfile.ExpirationMonth,
						ExpirationYear = cimPaymentProfile.ExpirationYear,
						CustomerId = customerId,
						ProfileId = profileID
					};
			}
			return null;
		}

		public static IEnumerable<PaymentProfileWrapper> GetPaymentProfiles(int customerId, string email)
		{
			var profileId = GetProfileId(customerId);

			if(profileId <= 0)
				yield break;

			var profileMgr = new ProfileManager(customerId, email, profileId);

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(var command = new SqlCommand(
					@"select AuthorizeNetProfileId, CardType, ExpirationMonth, ExpirationYear
						from CIM_AddressPaymentProfileMap
						where CustomerId = @customerId", connection))
				{
					command.Parameters.AddRange(new[]
						{
							new SqlParameter("@customerId", customerId),
						});

					using(var reader = command.ExecuteReader())
					{
						var authorizeNetProfileIdColumn = reader.GetOrdinal("AuthorizeNetProfileId");
						var cardTypeColumn = reader.GetOrdinal("CardType");
						var expirationMonthColumn = reader.GetOrdinal("ExpirationMonth");
						var expirationYearColumn = reader.GetOrdinal("ExpirationYear");

						while(reader.Read())
						{
							var authorizeNetProfileId = reader.GetInt64(authorizeNetProfileIdColumn);
							var cardType = reader.GetString(cardTypeColumn);
							var expirationMonth = reader.GetString(expirationMonthColumn);
							var expirationYear = reader.GetString(expirationYearColumn);

							var cimProfile = profileMgr.GetPaymentProfile(authorizeNetProfileId);
							if(cimProfile == null)
								continue;

							var maskedCard = (CreditCardMaskedType)cimProfile.payment.Item;

							yield return new PaymentProfileWrapper()
								{
									CreditCardNumberMasked = string.Format("**** **** **** {0}", maskedCard.cardNumber.Substring(4)),
									CardType = cardType,
									ExpirationMonth = expirationMonth,
									ExpirationYear = expirationYear,
									CustomerId = customerId,
									ProfileId = authorizeNetProfileId,
								};
						}
					}
				}
			}
		}

		public static CustomerAddressType GetCustomerAddressFromAddress(int addressId)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(var command = new SqlCommand(
					@"select top 1 FirstName, LastName, Company, Address1, City, State, Zip, Country, Phone
						from Address where AddressID = @addressId", connection))
				{
					command.Parameters.AddRange(new[]
						{
							new SqlParameter("@addressId", addressId),
						});

					using(var reader = command.ExecuteReader())
					{
						if(!reader.Read())
							return null;

						return new CustomerAddressType()
							{
								firstName = reader.GetString(reader.GetOrdinal("FirstName")),
								lastName = reader.GetString(reader.GetOrdinal("LastName")),
								company = reader.GetString(reader.GetOrdinal("Company")),
								address = reader.GetString(reader.GetOrdinal("Address1")),
								city = reader.GetString(reader.GetOrdinal("City")),
								state = reader.GetString(reader.GetOrdinal("State")),
								zip = reader.GetString(reader.GetOrdinal("Zip")),
								country = reader.GetString(reader.GetOrdinal("Country")),
								phoneNumber = reader.GetString(reader.GetOrdinal("Phone")),
							};
					}
				}
			}
		}
	}
}
