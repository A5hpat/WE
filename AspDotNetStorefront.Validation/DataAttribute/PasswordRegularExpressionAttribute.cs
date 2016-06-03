// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Validation.DataAttribute
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class PasswordRegularExpressionAttribute : ValidationAttribute, IClientValidatable
	{
		readonly string NormalPasswordErrorMessage;
		readonly string StrongPasswordErrorMessage;
		/// <summary>
		/// This is used so our validator knows which property of the model to pull the customer email out of. With the email we can decide if the customer requires strong or weak password validation.
		/// </summary>
		readonly string EmailPropertyName;
		readonly bool DisableClientValidation;

		public PasswordRegularExpressionAttribute(string normalPasswordErrorMessage, string strongPasswordErrorMessage, string emailPropertyName = null, bool disableClientValidation = false)
		{
			NormalPasswordErrorMessage = normalPasswordErrorMessage;
			StrongPasswordErrorMessage = strongPasswordErrorMessage;
			EmailPropertyName = emailPropertyName;
			DisableClientValidation = disableClientValidation;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var stringValue = Convert.ToString(value);
			if(string.IsNullOrWhiteSpace(stringValue))
				return ValidationResult.Success;

			var useStrongPassword = ShouldUseStrongPassword(validationContext.ObjectInstance);

			var expression = GetExpression(useStrongPassword);
			if(string.IsNullOrWhiteSpace(expression))
				return ValidationResult.Success;

			var match = Regex.Match(stringValue, expression);
			if(!match.Success || match.Index != 0 || match.Length != stringValue.Length)
				return new ValidationResult(GetErrorMessage(useStrongPassword));

			return ValidationResult.Success;
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			if(DisableClientValidation)
				return Enumerable.Empty<ModelClientValidationRegexRule>();

			var useStrongPassword = ShouldUseStrongPassword();

			return new[] 
			{
				new ModelClientValidationRegexRule(
					GetErrorMessage(useStrongPassword), 
					GetExpression(useStrongPassword))
			};
		}

		bool ShouldUseStrongPassword(object model = null)
		{
			if(AppLogic.AppConfigBool("UseStrongPwd"))
				return true;

			// Sometimes we need to use a target customer, not the current customer, to determine which password validation to use
			if(EmailPropertyName == null || model == null)
				return Customer.Current.IsAdminUser;

			var emailPropertyInfo = model.GetType().GetProperty(EmailPropertyName);
			if(emailPropertyInfo == null)
				throw new Exception(string.Format("Model does not contain the email property named \"{0}\"", EmailPropertyName));

			var emailValue = emailPropertyInfo.GetValue(model);
			var email = (emailValue ?? string.Empty).ToString();

			return new Customer(email).IsAdminUser;
		}

		string GetExpression(bool useStrongPassword)
		{
			return useStrongPassword
				? AppLogic.AppConfig("StrongPasswordValidator")
				: AppLogic.AppConfig("PasswordValidator");
		}

		string GetErrorMessage(bool useStrongPassword)
		{
			return useStrongPassword
				? StrongPasswordErrorMessage
				: NormalPasswordErrorMessage;
		}
	}
}
