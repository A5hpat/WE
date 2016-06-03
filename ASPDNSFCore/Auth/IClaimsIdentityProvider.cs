// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Security.Claims;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Auth
{
	public interface IClaimsIdentityProvider
	{
		ClaimsIdentity CreateClaimsIdentity(string name, string identifier, string authenticationType, string identityProvider = null);

		ClaimsIdentity CreateClaimsIdentity(Customer customer, string authenticationType = null);

		void AddRole(string roleName, ClaimsIdentity identity);
	}
}
