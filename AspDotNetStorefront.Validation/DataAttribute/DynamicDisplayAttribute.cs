// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Validation.DataAttribute
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public abstract class DynamicDisplayAttribute : Attribute
	{
		public abstract string GetDescription(object model);
	}

	public class StrongPasswordDisplayAttribute : DynamicDisplayAttribute
	{
		readonly string WeakDescription;
		readonly string StrongDescription;
		/// <summary>
		/// This is used so our validator knows which property of the model to pull the customer email out of. With the email we can decide if the customer requires strong or weak password validation messaging.
		/// </summary>
		readonly string EmailPropertyName;

		public StrongPasswordDisplayAttribute(string weakDescription, string strongDescription, string emailPropertyName = null)
		{
			WeakDescription = weakDescription;
			StrongDescription = strongDescription;
			EmailPropertyName = emailPropertyName;
		}

		public override string GetDescription(object model)
		{
			if(AppLogic.AppConfigBool("UseStrongPwd"))
				return StrongDescription;

			var customer = HttpContext.Current.GetCustomer();
			// Sometimes we need to use a target customer, not the current customer, to determine which password strings to show
			if(EmailPropertyName != null && model != null)
			{
				var emailPropertyInfo = model.GetType().GetProperty(EmailPropertyName);
				if(emailPropertyInfo != null)
				{
					var emailValue = emailPropertyInfo.GetValue(model);
					var email = (emailValue ?? string.Empty).ToString();

					customer = new Customer(email);
				}
			}

			return customer.IsAdminUser
				? StrongDescription
				: WeakDescription;
		}
	}
}
