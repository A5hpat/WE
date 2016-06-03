// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Security;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Auth
{
	public sealed class CustomerMembershipProvider : MembershipProvider
	{
		public override string ApplicationName
		{ get; set; }

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			if(config == null)
				throw new ArgumentNullException("config");

			if(String.IsNullOrEmpty(name))
				name = "CustomerMembershipProvider";

			if(String.IsNullOrEmpty(config["applicationName"]))
				ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
			else
				ApplicationName = config["applicationName"];

			base.Initialize(name, config);
		}

		/// <summary>
		/// Processes a request to update the password for a membership user.
		/// </summary>
		/// <param name="username">The user to update the password for.</param>
		/// <param name="oldPassword">The current password for the specified user.</param>
		/// <param name="newPassword">The new password for the specified user.</param>
		/// <returns>
		/// true if the password was updated successfully; otherwise, false.
		/// </returns>
		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			var customerGuid = new Guid(username);
			var customer = new Customer(customerGuid, true);

			if(!ValidateUser(customer.EMail, oldPassword))
				return false;

			var oldSaltedPassword = new Password(oldPassword, customer.SaltKey);
			var newSaltedPassword = new Password(newPassword, customer.SaltKey);

			customer.UpdateCustomer(
				saltedAndHashedPassword: newSaltedPassword.SaltedPassword,
				saltKey: newSaltedPassword.Salt,
				badLogin: -1,
				passwordChangeRequired: false);

			return true;
		}

		/// <summary>
		/// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
		/// </summary>
		/// <param name="username">The name of the user to get information for.</param>
		/// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
		/// <returns>
		/// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
		/// </returns>
		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			var customer = new Customer(username, true);

			var user = new MembershipUser(
				providerName: GetType().Name,
				name: customer.Name,
				providerUserKey: null,
				email: customer.EMail,
				passwordQuestion: String.Empty,
				comment: String.Empty,
				isApproved: true,
				isLockedOut: false,
				creationDate: customer.CreatedOn,
				lastLoginDate: DateTime.Now,
				lastActivityDate: DateTime.Now,
				lastPasswordChangedDate: customer.PwdChanged,
				lastLockoutDate: customer.LockedUntil);

			return user;
		}

		/// <summary>
		/// Resets a user's password to a new, automatically generated password.
		/// </summary>
		/// <param name="username">The user to reset the password for.</param>
		/// <param name="answer">The password answer for the specified user.</param>
		/// <returns>The new password for the specified user.</returns>
		public override string ResetPassword(string username, string answer)
		{
			var customerGuid = new Guid(username);
			var customer = new Customer(customerGuid, true);
			var lockuntil = DateTime.Now.AddMinutes(-1);
			var generatedPassword = customer.IsAdminUser || customer.IsAdminSuperUser
				? (Password)new RandomStrongPassword()
				: (Password)new RandomPassword();

			var clearPassowrd = generatedPassword.ClearPassword.Replace("&", "*");

			customer.UpdateCustomer(
				saltedAndHashedPassword: generatedPassword.SaltedPassword,
				saltKey: generatedPassword.Salt,
				lockedUntil: lockuntil,
				badLogin: -1,
				passwordChangeRequired: true);

			return clearPassowrd;
		}

		/// <summary>
		/// Verifies that the specified user name and password exist in the data source.
		/// </summary>
		/// <param name="username">The name of the user to validate.</param>
		/// <param name="password">The password for the specified user.</param>
		/// <returns>
		/// true if the specified username and password are valid; otherwise, false.
		/// </returns>
		public override bool ValidateUser(string username, string password)
		{
			var customer = new Customer(username, true);
			return customer.CheckLogin(password);
		}

		#region NotSupported Routines

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			throw new NotImplementedException();
		}

		public override bool EnablePasswordReset
		{
			get { throw new NotImplementedException(); }
		}

		public override bool EnablePasswordRetrieval
		{
			get { throw new NotImplementedException(); }
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline()
		{
			throw new NotImplementedException();
		}

		public override string GetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}


		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public override string GetUserNameByEmail(string email)
		{
			throw new NotImplementedException();
		}

		public override int MaxInvalidPasswordAttempts
		{
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredPasswordLength
		{
			get { throw new NotImplementedException(); }
		}

		public override int PasswordAttemptWindow
		{
			get { throw new NotImplementedException(); }
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			get { throw new NotImplementedException(); }
		}

		public override string PasswordStrengthRegularExpression
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresQuestionAndAnswer
		{
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresUniqueEmail
		{
			get { throw new NotImplementedException(); }
		}

		public override bool UnlockUser(string userName)
		{
			throw new NotImplementedException();
		}

		public override void UpdateUser(MembershipUser user)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
