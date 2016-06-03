// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Models.Converter;
using AspDotNetStorefront.Validation.AddressValidator;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront.Controllers.Classes
{
	public class AddressControllerHelper
	{
		readonly IAddressValidationProviderFactory AddressValidationProviderFactory;
		readonly AddressViewModelConverter AddressViewModelConverter;

		public AddressControllerHelper(
			IAddressValidationProviderFactory addressValidationProviderFactory,
			AddressViewModelConverter addressViewModelConverter)
		{
			AddressValidationProviderFactory = addressValidationProviderFactory;
			AddressViewModelConverter = addressViewModelConverter;
		}

		public IEnumerable<AddressViewModel> GetCustomerAddresses(Customer customer)
		{
			var addressCollection = new Addresses();
			addressCollection.LoadCustomer(customer.CustomerID);

			return addressCollection
				.Cast<Address>()
				.Select(address => AddressViewModelConverter.ConvertToAddressViewModel(address, customer));
		}

		public AddressViewModel GetCustomerAddress(int? addressId, Customer customer)
		{
			var address = new Address();

			if(addressId.HasValue)
			{
				address.LoadFromDB(addressId.Value);
				if(address.CustomerID != customer.CustomerID)
					throw new HttpException(403, "Forbidden");
			}

			return AddressViewModelConverter.ConvertToAddressViewModel(address, customer);
		}

		public Uri GetRedirectUrl(UrlHelper urlHelper)
		{
			Uri returnUrl = new Uri(urlHelper.Content("~/account"), UriKind.Relative);

			if(!string.IsNullOrEmpty(CommonLogic.QueryStringCanBeDangerousContent("returnURL")))
				Uri.TryCreate(urlHelper.Content(string.Format("~/{0}", CommonLogic.QueryStringCanBeDangerousContent("returnURL"))), UriKind.Relative, out returnUrl);

			return returnUrl;
		}

		public int UpdateAddress(AddressViewModel address, Customer customer)
		{
			var adnsfAddress = AddressViewModelConverter.ConvertToAddress(address, customer);

			if(address.Id.HasValue)
			{
				if(!Customer.OwnsThisAddress(customer.CustomerID, (int)address.Id))
					throw new HttpException(403, "Forbidden");

				adnsfAddress.UpdateDB();
			}
			else
				adnsfAddress.InsertDB();

			return adnsfAddress.AddressID;
		}

		public void DeleteAddress(int addressId, Customer customer)
		{
			var verificationAddress = new Address();
			verificationAddress.LoadFromDB(addressId);
			if(verificationAddress.CustomerID != customer.CustomerID)
				throw new HttpException(403, "Forbidden");

			Address.DeleteFromDB(addressId, customer.CustomerID);
		}

		public bool CanBePrimaryShippingAddress(AddressViewModel address)
		{
			if(AppLogic.AppConfigBool("DisallowShippingToPOBoxes"))
				return new POBoxAddressVerifier()
					.IsValid(address.Address1);

			return true;
		}

		public void UpdatePrimaryBilling(AddressViewModel address, Customer customer)
		{
			var query = new StringBuilder();
			query.Append("UPDATE Customer SET BillingAddressID = @addressId WHERE CustomerID = @customerId;");
			query.Append("UPDATE ShoppingCart SET BillingAddressID = @addressId WHERE CustomerID = @customerId AND CartType IN (0,1);");

			using(var connection = DB.dbConn())
			{
				connection.Open();
				DB.ExecuteSQL(
					sql: query.ToString(),
					parameters: new[]
						{
							new SqlParameter("addressId", address.Id),
							new SqlParameter("customerId", customer.CustomerID)
						},
					connection: connection);
			}
		}

		public void UpdatePrimaryShipping(AddressViewModel address, Customer customer)
		{
			var query = new StringBuilder();
			query.Append("UPDATE Customer SET ShippingAddressID = @addressId WHERE CustomerID = @customerId;");
			query.Append("UPDATE ShoppingCart SET ShippingAddressID = @addressId WHERE CustomerID = @customerId AND CartType IN (0,1);");

			using(var connection = DB.dbConn())
			{
				connection.Open();
				DB.ExecuteSQL(
					sql: query.ToString(),
					parameters: new[]
						{
							new SqlParameter("addressId", address.Id),
							new SqlParameter("customerId", customer.CustomerID)
						},
					connection: connection);
			}
		}

		public void UpdatePrimaryBillingEqualsShipping(Customer customer)
		{
			DB.ExecuteSQL(
				sql: "update Customer set BillingEqualsShipping = case when BillingAddressID = ShippingAddressID then 1 else 0 end where CustomerId = @customerId;",
				parameters: new SqlParameter[]
				{
					new SqlParameter("@customerId", customer.CustomerID)
				});
		}
	}
}
