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
	public class ClaimsIdentityProvider : IClaimsIdentityProvider
	{
		public ClaimsIdentity CreateClaimsIdentity(string name, string identifier, string authenticationType, string identityProvider = null)
		{
			   var identity = new ClaimsIdentity(
				new[] {
					new Claim(ClaimTypes.Name, name),
					new Claim(ClaimTypes.NameIdentifier, identifier)
				},
				authenticationType);

			if(identityProvider != null)
				identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", identityProvider));

			return identity;
		}

		public ClaimsIdentity CreateClaimsIdentity(Customer customer, string authenticationType = null)
		{
			var identity = CreateClaimsIdentity(
				name: customer.CustomerGUID.ToString(),
				identifier: customer.CustomerGUID.ToString(),
				authenticationType: authenticationType ?? AuthValues.CookiesAuthenticationType,
				identityProvider: AuthValues.CookiesAuthenticationIdentityProvider);

			if(customer.IsAdminUser)
				AddRole("Admin", identity);

			if(customer.IsAdminSuperUser)
				AddRole("SuperAdmin", identity);

			return identity;
		}

		public void AddRole(string roleName, ClaimsIdentity identity)
		{
			var roleClaim = identity.FindFirst(c => c.Type == ClaimTypes.Role && c.Value == roleName);
			if(roleClaim != null)
				return;

			identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
		}
	}
}
