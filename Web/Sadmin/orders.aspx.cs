// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class Orders : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
		}

		protected string BuildOrderItems(int orderNumber)
		{
			var order = new Order(orderNumber, LocaleSetting);

			// Create lines for the first two items
			var firstTwoItems = order.CartItems
				.Take(2)
				.Select(item => String.Format(
					"<div class=\"order-item\">{0}</div>",
					AppLogic.MakeProperObjectName(
						item.ProductName,
						item.VariantName,
						LocaleSetting)));

			// If there are more than two items, show a link to the order details
			var moreLink = order.CartItems
				.Skip(2)
				.Select(item => String.Format(
					"<div class=\"order-item\"><a href=\"order.aspx?ordernumber={0}\">{1}</a></div>",
					orderNumber,
					AppLogic.GetString("admin.orderdetails.More", LocaleSetting)));

			// Combine them together
			var displayedItems = firstTwoItems
				.Concat(moreLink)
				.Take(3);

			return String.Join(Environment.NewLine, displayedItems);
		}

		protected void btnPrint_Click(object sender, EventArgs e)
		{
			// Get the list of order numbers from the grid
			var orderIdsToPrint = grdOrders
				.DataKeys
				.Cast<System.Web.UI.WebControls.DataKey>()
				.Where(dataKey => dataKey != null)
				.Where(dataKey => dataKey.Value != null)
				.Select(dataKey => dataKey.Value);

			if(!orderIdsToPrint.Any())
				return;

			// Pop up a window
			var popupUrl = String.Format("printreceipts.aspx?ordernumbers={0}", String.Join(",", orderIdsToPrint));
			var popupScript = String.Format("window.open('{0}', 'popup_window', 'height=600,width=800,top=0,left=0,status=yes,toolbar=yes,menubar=yes,scrollbars=yes,location=yes');", popupUrl);
			ClientScript.RegisterStartupScript(this.GetType(), "printReceiptsPopup", popupScript, true);
		}
	}
}
