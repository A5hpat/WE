// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace AspDotNetStorefrontAdmin
{
	public partial class Stores : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			scMain.ThisCustomer = ThisCustomer;
		}
	}
}
