// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Text.RegularExpressions;

namespace AspDotNetStorefrontCore.Validation
{
	public class EmailAddressValidator
	{
		public const string ValidationRegularExpression = @"^.+@.+\..+$";

		public bool IsValidEmailAddress(string email)
		{
			return string.IsNullOrEmpty(email) 
				? false 
				: Regex.IsMatch(email, ValidationRegularExpression, RegexOptions.IgnoreCase);
		}
	}
}
