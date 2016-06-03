// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class adjustcharge : AspDotNetStorefront.Admin.AdminPageBase
	{
		int orderNumber;
		protected void Page_Load(object sender, System.EventArgs e)
		{
			orderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
			if(!ltAdjustOrderTitle.Text.Contains(orderNumber.ToString()))
				ltAdjustOrderTitle.Text += orderNumber;

			if(!IsPostBack)
				ShowForm(orderNumber);
		}

		protected override void OnPreRender(EventArgs e)
		{
			DataBind();
			base.OnPreRender(e);
		}

		private bool UpdateOrder(int orderNumber)
		{
			Decimal newOrderTotal;
			if(Decimal.TryParse(txtNewOrderTotal.Text, out newOrderTotal))
			{
				string serviceNotes = DB.SQuote(txtCustomerServiceNotes.Text);
				if(newOrderTotal != 0.0M)
				{
					DB.ExecuteSQL(String.Format("update orders set CustomerServiceNotes={0}, OrderTotal={1}, EditedOn={2} where OrderNumber={3}", 
						serviceNotes, 
						Localization.CurrencyStringForDBWithoutExchangeRate(newOrderTotal),
						DB.DateQuote(DateTime.Now),
						orderNumber));
				}
				return true;
			}
			return false;
		}

		private void ShowForm(int orderNumber)
		{
			Decimal orderTotal = 0.0M;
			String customerServiceNotes = String.Empty;

			using(var dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(var rs = DB.GetRS(String.Format("select OrderTotal, CustomerServiceNotes from Orders with (NOLOCK) where OrderNumber={0}", orderNumber), dbconn))
				{
					if(rs.Read())
					{
						orderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
						customerServiceNotes = DB.RSField(rs, "CustomerServiceNotes");
					}
					else 
						AlertMessageDisplay.PushAlertMessage("admin.refund.InvalidOrderNumber".StringResource() + ": " + orderNumber, AlertMessage.AlertType.Info);

				}
			}
			txtNewOrderTotal.Text = Localization.CurrencyStringForGatewayWithoutExchangeRate(orderTotal);
			txtCustomerServiceNotes.Text = customerServiceNotes;
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			if(!Order.OrderExists(orderNumber))
			{
				AlertMessageDisplay.PushAlertMessage("admin.refund.InvalidOrderNumber".StringResource() + ": " + orderNumber, AlertMessage.AlertType.Error);
				return;
			}			

			if(UpdateOrder(orderNumber))
				AlertMessageDisplay.PushAlertMessage("admin.common.Updated".StringResource(), AlertMessage.AlertType.Success);
			else
				AlertMessageDisplay.PushAlertMessage("admin.common.UpdateFailed".StringResource(), AlertMessage.AlertType.Error);
		}
	}
}
