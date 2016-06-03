// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using AspDotNetStorefrontCore;

public partial class Admin_adhocchargecomplete : AspDotNetStorefront.Admin.AdminPageBase
{
	protected void Page_Load(object sender, EventArgs e)
	{
		StringBuilder writer = new StringBuilder();
		Customer ThisCustomer = Context.GetCustomer();

		//This wil update and refresh the parent window                 
		writer.Append("<h1>" + AppLogic.GetString("adhoccharge.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + CommonLogic.QueryStringCanBeDangerousContent("ordernumber") + "</h1>");
		writer.Append("<p><a class=\"btn btn-primary\" href=\"javascript:self.close();\">" + AppLogic.GetString("admin.common.Close", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a></p>");
		ltContent.Text = writer.ToString();
	}
}
