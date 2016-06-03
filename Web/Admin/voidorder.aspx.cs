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
	public partial class voidorder : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			StringBuilder writer = new StringBuilder();
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			Customer ThisCustomer = Context.GetCustomer();
			writer.Append("<div align=\"left\">");

			if(!ThisCustomer.IsAdminUser) // safety check
			{
				writer.Append("<b><font color=red>PERMISSION DENIED</b></font>");
			}
			else
			{
				int ONX = CommonLogic.QueryStringUSInt("OrderNumber");
				Order ord = new Order(ONX, ThisCustomer.LocaleSetting);
				bool isForceVoid = CommonLogic.QueryStringBool("ForceVoid");

				if(isForceVoid)
				{
					writer.Append("<b>FORCE VOID ORDER: " + ONX.ToString() + "</b><br/><br/>");
					if(CommonLogic.FormCanBeDangerousContent("IsSubmit") == "true")
					{
						String Status = Gateway.OrderManagement_DoVoid(ord, true);
						writer.Append("Force Voided Status: " + Status);
						if(Status == AppLogic.ro_OK)
						{
							writer.Append("<script type=\"text/javascript\">\n");
							writer.Append("opener.window.location = opener.window.location.href;");
							writer.Append("</script>\n");
						}
						writer.Append("<p align=\"center\"><a href=\"javascript:self.close();\">Close</a></p>");
					}
					else
					{
						writer.Append("<form method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("voidorder.aspx") + "?ordernumber=" + ONX.ToString() + "&ForceVoid=1" + "&confirm=yes\" id=\"RefundOrderForm\" name=\"RefundOrderForm\">");
						writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">");
						writer.Append("<p align=\"center\">Are you sure you want to force void this order?<br/><br/></p>");
						writer.AppendFormat("<p align=\"center\">{0}<br/><br/></p>", AppLogic.GetString("admin.orderframe.ForceVoidWarning"));
                        writer.Append("<p align=\"center\"><input type=\"submit\" class=\"btn btn-primary btn-sm\" name=\"submit\" value=\"&nbsp;&nbsp;Yes&nbsp;&nbsp;\">");
						writer.Append("<img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/spacer.gif") + "\" width=\"100\" height=\"1\">");
						writer.Append("<input type=\"button\" class=\"btn btn-default btn-sm\" name=\"cancel\" value=\"&nbsp;&nbsp;No&nbsp;&nbsp;\" onClick=\"javascript:self.close();\">");
						writer.Append("</p>");
						writer.Append("</form>");
					}
				}
				else
				{
					writer.Append("<b>VOID ORDER: " + ONX.ToString() + "</b><br/><br/>");
					if(CommonLogic.FormCanBeDangerousContent("IsSubmit") == "true")
					{

						String Status = Gateway.OrderManagement_DoVoid(ord);
						writer.Append("Void Status: " + Status);
						if(Status == AppLogic.ro_OK)
						{
							writer.Append("<script type=\"text/javascript\">\n");
							writer.Append("opener.window.location = opener.window.location.href;");
							writer.Append("</script>\n");
						}
						writer.Append("<p align=\"center\"><a href=\"javascript:self.close();\">Close</a></p>");
					}
					else
					{
						writer.Append("<form method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("voidorder.aspx") + "?ordernumber=" + ONX.ToString() + "&confirm=yes\" id=\"RefundOrderForm\" name=\"RefundOrderForm\">");
						writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">");
						writer.Append("<p align=\"center\">Are you sure you want to void this order?<br/><br/></p>");
						writer.Append("<p align=\"center\"><input type=\"submit\" class=\"btn btn-primary btn-sm\" name=\"submit\" value=\"&nbsp;&nbsp;Yes&nbsp;&nbsp;\">");
						writer.Append("<img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/spacer.gif") + "\" width=\"100\" height=\"1\">");
						writer.Append("<input type=\"button\" class=\"btn btn-default btn-sm\" name=\"cancel\" value=\"&nbsp;&nbsp;No&nbsp;&nbsp;\" onClick=\"javascript:self.close();\">");
						writer.Append("</p>");
						writer.Append("</form>");
					}
				}

			}
			writer.Append("</div>");
			ltContent.Text = writer.ToString();
		}
	}
}
