// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Models
{
	public class UserLinksViewModel
	{
		public bool UserIsRegistered { get; set; }
		public string UserFirstName { get; set; }
		public string UserLastName { get; set; }
		public string Email { get; set; }
		public bool MinicartEnabled { get; set; }
		public bool MiniwishlistEnabled { get; set; }
		public bool CheckoutInProgress { get; set; }
		public bool CartHasItems { get; set; }
	}

	public class LoginLinksViewModel
	{
		public readonly bool UserIsRegistered;

		public LoginLinksViewModel(bool userIsRegistered)
		{
			UserIsRegistered = userIsRegistered;
		}
	}
}
