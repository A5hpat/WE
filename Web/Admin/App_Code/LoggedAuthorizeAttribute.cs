// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public class LoggedAuthorizeAttribute : Attribute
	{
		public string Users
		{ get; set; }

		public string Roles
		{ get; set; }

		public bool LogSuccess
		{ get; set; }

		public bool LogFailed
		{ get; set; }
	}
}
