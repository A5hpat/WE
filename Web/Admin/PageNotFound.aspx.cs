// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace AspDotNetStorefront
{
	public partial class PageNotFound : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Response.StatusCode = 404;
		}
	}
}
