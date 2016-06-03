// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.UI;
using AspDotNetStorefront.Filters;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Admin
{
	// Base class for all admin site pages
	public class AdminPageBase : Page
	{
		protected Customer ThisCustomer
		{ get; private set; }

		protected new int SkinID
		{ get { return 1; } }

		protected bool Editing
		{ get; set; }

		protected string LocaleSetting
		{ get; private set; }

		public AdminPageBase()
		{
			ThisCustomer = HttpContext.Current.GetCustomer();
			LocaleSetting = Localization.GetDefaultLocale();
		}

		protected override void OnPreInit(EventArgs e)
		{
			// Redirect to HTTPS if needed
			if(AppLogic.IsAdminSite
				&& AppLogic.OnLiveServer()
				&& AppLogic.UseSSL()
				&& !CommonLogic.IsSecureConnection())
			{
				var secureUrl = new UriBuilder(Request.Url)
				{
					Scheme = Uri.UriSchemeHttps,
					Port = -1,
				};

				Response.Redirect(secureUrl.ToString());
			}

			// Set content security policy headers
			new ContentSecurityPolicy()
				.Enforce(new HttpResponseWrapper(HttpContext.Current.Response));

			// Disable caching
			Response.Cache.SetCacheability(HttpCacheability.NoCache);

			// Configure the page
			var pageName = CommonLogic.GetThisPageName(false).Replace(".aspx", "");
			Page.Title = AppLogic.GetString(string.Format("admin.title.{0}", pageName));
			Page.Theme = "Admin_Default";

			base.OnPreInit(e);
		}

		protected string CreateLinkText(object value)
		{
			var linkText = (string)value;
			return !string.IsNullOrWhiteSpace(linkText) ? linkText : AppLogic.GetString("admin.nolinktext", ThisCustomer.LocaleSetting);
		}
	}
}
