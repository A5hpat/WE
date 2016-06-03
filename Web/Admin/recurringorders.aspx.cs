// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Text;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
	public partial class recurringorders : AspDotNetStorefront.Admin.AdminPageBase
	{
		// NOTE: THIS PAGE DOES NOT PROCESS GATEWAY AUTOBILL RECURRING ORDERS!
		// NOTE: USE THE RECURRINGIMPORT.ASPX PAGE FOR THOSE!!

		protected override void OnInit(EventArgs e)
		{
			FilteredListing.SqlQuery = @"
				SELECT {0} 
				ShoppingCart.OriginalRecurringOrderNumber, 
				ShoppingCart.CustomerID, 
				ShoppingCart.NextRecurringShipDate, 
				Store.Name, 
				Customer.Email 
				FROM ShoppingCart WITH (NOLOCK) 
				LEFT JOIN Store ON ShoppingCart.StoreId = Store.StoreId 
				LEFT JOIN Customer ON ShoppingCart.CustomerID = Customer.CustomerID 
				WHERE CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() +
				@" AND {1} 
				GROUP BY ShoppingCart.OriginalRecurringOrderNumber, 
				ShoppingCart.CustomerID, 
				ShoppingCart.NextRecurringShipDate, 
				Store.Name, 
				Customer.Email, 
				ShoppingCart.StoreID, 
				ShoppingCart.CustomerID";
			base.OnInit(e);
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			if(!OrdersArePendingToday())
				AlertMessage.PushAlertMessage(AppLogic.GetString("admin.recurring.NoRecurringShipmentsDueToday", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Warning);
		}

		protected bool OrdersArePendingToday()
		{
			var ordersPendingToday = DB.GetSqlN(
				"SELECT COUNT(*) AS N FROM ShoppingCart WITH (NOLOCK) WHERE CartType = @CartType AND NextRecurringShipDate < DATEADD(d, 1, GETDATE())",
				new SqlParameter("@CartType", (int)CartTypeEnum.RecurringCart));

			return ordersPendingToday > 0;
		}

		protected void btnProcessAll_Click(object sender, EventArgs e)
		{
			var output = new StringBuilder();
			var recurringOrderMgr = new RecurringOrderMgr();

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = "Select distinct(OriginalRecurringOrderNumber) from ShoppingCart where RecurringSubscriptionID='' and CartType = @cartType and NextRecurringShipDate < dateadd(d,1,getDate())";
				command.Parameters.AddWithValue("@cartType", (int)CartTypeEnum.RecurringCart);

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						output.AppendFormat(
							AppLogic.GetString("admin.recurring.ProcessingNextOccurrence"),
							reader.FieldInt("OriginalRecurringOrderNumber"));
						output.Append(recurringOrderMgr.ProcessRecurringOrder(reader.FieldInt("OriginalRecurringOrderNumber")));
						output.Append("...<br/>");
					}
			}

			AlertMessage.PushAlertMessage(
				output.ToString(),
				AspDotNetStorefrontControls.AlertMessage.AlertType.Info);
		}
	}
}
