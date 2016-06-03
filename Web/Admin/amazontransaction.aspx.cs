// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefrontAdmin
{
	public partial class amazontransaction : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			StringBuilder writer = new StringBuilder();
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			Customer ThisCustomer = Context.GetCustomer();
			writer.Append("<div align=\"left\">");
			int ONX = CommonLogic.QueryStringUSInt("OrderNumber");
			Order ord = new Order(ONX, ThisCustomer.LocaleSetting);
			Customer c = new Customer(ord.CustomerID);

			if(!ThisCustomer.IsAdminUser) // safety check
			{
				writer.Append("<b><font color=red>" + AppLogic.GetString("admin.common.PermissionDeniedUC", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</b></font>");
			}
			else
			{
				writer.Append(AppLogic.GetString("admin.amazontransaction.UpdateTransaction", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + ONX.ToString() + "</b><br/><br/>");
				String Status = Gateway.OrderManagement_UpdateTransaction(ord);
				writer.Append("Status: " + Status);
				if(Status == AppLogic.ro_OK)
				{
					writer.Append("<script type=\"text/javascript\">\n");
					writer.Append("opener.window.location = opener.window.location.href;");
					writer.Append("</script>\n");
				}
			}
			writer.Append("</div>");
			writer.Append("<p align=\"center\"><a href=\"javascript:self.close();\">" + AppLogic.GetString("admin.common.Close", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a></p>");
			ltContent.Text = writer.ToString();
		}
	}
}

