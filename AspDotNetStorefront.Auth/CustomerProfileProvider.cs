// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Profile;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Auth
{
	/// <summary>
	/// Aspdnsf custom Profile Provider storage system, whereby customer-specific preferences can be stored in SQL Server, rather than in cookies.
	/// </summary>
	public class CustomerProfileProvider : ProfileProvider
	{
		public override string ApplicationName
		{ get; set; }

		/// <summary>
		/// Initialize the provider.
		/// </summary>
		/// <param name="name">Name of the provider.</param>
		/// <param name="config">Configuration settings.</param>
		/// <remarks></remarks>
		public override void Initialize(string name, NameValueCollection config)
		{
			if(config == null)
				throw new ArgumentNullException("config");

			if(String.IsNullOrEmpty(name))
				name = "CustomerProfileProvider";

			if(String.IsNullOrEmpty(config["applicationName"]))
				ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
			else
				ApplicationName = config["applicationName"];

			base.Initialize(name, config);
		}

		/// <summary>
		/// Gets the profile property values.
		/// </summary>
		/// <param name="context">Profile context.</param>
		/// <param name="settingsProperties">The profile settings properties.</param>
		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection settingsProperties)
		{
			var username = (string)context["UserName"];
			var settingsValues = new SettingsPropertyValueCollection();

			// let's just do 1 read to the database and match up the relevant fields
			var pairs = GetPropertyValuePairs(username);

			foreach(SettingsProperty property in settingsProperties)
			{
				var propertyValue = new SettingsPropertyValue(property);

				if(pairs.ContainsKey(propertyValue.Name))
				{
					propertyValue.PropertyValue = pairs[propertyValue.Name];
					// just read from the db, assume default value
					propertyValue.IsDirty = false;
				}

				settingsValues.Add(propertyValue);
			}

			return settingsValues;
		}


		/// <summary>
		/// Sets Profile the property values.
		/// </summary>
		/// <param name="context">Profile context.</param>
		/// <param name="settingsPropertyValues">The settings profile property values.</param>
		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection settingsPropertyValues)
		{
			var username = (string)context["UserName"];
			if(string.IsNullOrEmpty(username))
				return;

			var isAuthenticated = (bool)context["IsAuthenticated"];
			var userID = GetUniqueID(username, isAuthenticated);

			foreach(SettingsPropertyValue propertyValue in settingsPropertyValues)
				if(propertyValue.IsDirty)
					SetProperty(username, userID, isAuthenticated, propertyValue.Name, (string)propertyValue.PropertyValue);
		}

		/// <summary>
		/// Deletes profiles that have been inactive since the specified date.
		/// </summary>
		/// <param name="authenticationOption">Current authentication option setting.</param>
		/// <param name="userInactiveSinceDate">Inactivity date for deletion.</param>
		/// <returns>Number of records deleted.</returns>
		public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			try
			{
				DB.ExecuteSQL(
					"Delete from Profile where UpdatedOn < @userInactiveSinceDate",
					new[] {
						new SqlParameter("userInactiveSinceDate", userInactiveSinceDate)
					});

				return 1;
			}
			catch
			{
				return 0;
			}
		}

		/// <summary>
		/// *Note Currently not supported -- Delete profiles for an array of user names.
		/// </summary>
		public override int DeleteProfiles(string[] userNames)
		{
			return 0;
		}

		/// <summary>
		/// *Note Currently not supported -- Delete profiles based upon the user names in the collection of profiles.
		/// </summary>
		public override int DeleteProfiles(ProfileInfoCollection profiles)
		{
			var userNames = new string[profiles.Count];
			return DeleteProfiles(userNames);
		}

		/// <summary>
		/// *Note Currently not supported -- Get a collection of profiles based upon a user name matching string and inactivity date.
		/// </summary>
		public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string userNameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			return GetProfileInfo(
				authenticationOption,
				userNameToMatch,
				userInactiveSinceDate,
				pageIndex,
				pageSize,
				out totalRecords);
		}

		/// <summary>
		/// *Note Currently not supported -- Get a collection of profiles based upon a user name matching string.
		/// </summary>       
		public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string userNameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			return GetProfileInfo(
				authenticationOption,
				userNameToMatch,
				null,
				pageIndex,
				pageSize,
				out totalRecords);
		}

		/// <summary>
		/// *Note Currently not supported -- Get a collection of profiles based upon an inactivity date.
		/// </summary>       
		public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			return GetProfileInfo(
				authenticationOption,
				null,
				userInactiveSinceDate,
				pageIndex,
				pageSize,
				out totalRecords);
		}

		/// <summary>
		/// *Note Currently not supported -- Get a collection of profiles.
		/// </summary>      
		public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			return GetProfileInfo(
				authenticationOption,
				null,
				null,
				pageIndex,
				pageSize,
				out totalRecords);
		}

		/// <summary>
		/// *Note Currently not supported -- Get the number of inactive profiles based upon an inactivity date.
		/// </summary>    
		public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			int inactiveProfiles;
			GetProfileInfo(authenticationOption, null, userInactiveSinceDate, 0, 0, out inactiveProfiles);

			return inactiveProfiles;
		}

		/// <summary>
		/// Sets Profile property value.
		/// </summary>
		/// <param name="username">Authentication user name(customer Guid).</param>
		/// <param name="uniqueId">Customer Id.</param>
		/// <param name="isAuthenticated">if set to <c>false</c> [is Anonymous user].</param>
		/// <param name="propertyName">The profile property name.</param>
		/// <param name="propertyValue">The profile property value.</param>
		void SetProperty(string username, int uniqueId, Boolean isAuthenticated, string propertyName, string propertyValue)
		{
			if(string.IsNullOrEmpty(username))
				return;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "dbo.aspdnsf_SetProfileProperties";

				command.Parameters.AddRange(new[] {
						new SqlParameter("@storeid", AppLogic.StoreID()),
						new SqlParameter("@customerid", uniqueId),
						new SqlParameter("@CustomerGUID", new Guid(username)),
						new SqlParameter("@isAuthenticated", isAuthenticated),
						new SqlParameter("@PropertyNames", propertyName),
						new SqlParameter("@PropertyValuesString", propertyValue),
					});

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Query profile information for the current user.
		/// </summary>
		/// <param name="username">GUID of the current user.</param>
		/// <returns>Profile information of the current user.</returns>
		Dictionary<string, object> GetPropertyValuePairs(string username)
		{
			var propertyValues = new Dictionary<string, object>();

			if(string.IsNullOrEmpty(username))
				return propertyValues;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "select PropertyName, PropertyValueString from dbo.profile with(nolock) where CustomerGUID = @username and StoreID = @storeId";
				command.Parameters.AddRange(new[] {
						new SqlParameter("username", username),
						new SqlParameter("storeId", AppLogic.StoreID()),
					});

				connection.Open();

				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						var propertyName = DB.RSField(reader, "PropertyName");
						var propertyValue = reader["PropertyValueString"];

						if(propertyValues.ContainsKey(propertyName) == false)
							propertyValues.Add(propertyName, propertyValue);
					}
			}

			return propertyValues;
		}

		/// <summary>
		/// Gets the profile property value.
		/// </summary>
		/// <param name="username">Authentication user name(customer Guid).</param>
		/// <param name="propertyName">The profile property name</param>
		/// <returns>profile property value</returns>
		public string GetProperty(string username, string propertyName)
		{
			if(string.IsNullOrEmpty(username))
				return string.Empty;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "select PropertyValueString from dbo.profile with(nolock) where CustomerGUID = @username and PropertyName = @propertyName";
				command.Parameters.AddRange(new[] {
						new SqlParameter("userName", username),
						new SqlParameter("propertyName", propertyName),
					});

				connection.Open();

				using(var reader = command.ExecuteReader())
					if(reader.Read())
						return DB.RSField(reader, "PropertyValueString");
			}

			return String.Empty;
		}

		/// <summary>
		/// Gets the customer unique ID.
		/// </summary>
		/// <param name="username">Authentication user name(customer Guid).</param>
		/// <param name="isAuthenticated">if set to <c>false</c> [is Anonymous user].</param>
		/// <returns>Customer ID</returns>
		int GetUniqueID(string username, bool isAuthenticated)
		{
			if(string.IsNullOrEmpty(username))
				return 0;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandText = "select CustomerID from dbo.Customer where CustomerGUID = @username";
				command.Parameters.AddRange(new[] {
						new SqlParameter("userName", username),
					});

				connection.Open();

				using(var reader = command.ExecuteReader())
					if(reader.Read())
						return DB.RSFieldInt(reader, "CustomerID");
			}

			return 0;
		}

		/// <summary>
		/// *note method not supported on our custom profile provider, return empty profile
		/// </summary>
		ProfileInfoCollection GetProfileInfo(ProfileAuthenticationOption authenticationOption, string usernameToMatch, object userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			var profiles = new ProfileInfoCollection();
			profiles.Add(new ProfileInfo(
				username: "",
				isAnonymous: false,
				lastActivityDate: DateTime.MinValue,
				lastUpdatedDate: DateTime.MinValue,
				size: 0));

			totalRecords = 0;

			return profiles;
		}
	}
}
