// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for recurringorder.
	/// </summary>
	public partial class recurringorder : AspDotNetStorefront.Admin.AdminPageBase
	{
		int OriginalRecurringOrderNumber = 0;
		Customer OrderCustomer;

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			btnCloseTop.DataBind();
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			OriginalRecurringOrderNumber = CommonLogic.QueryStringUSInt("originalorderid");

			if(OriginalRecurringOrderNumber == 0)
				Response.Redirect("recurringorders.aspx");

			OrderCustomer = new Customer(GetRecurringCartCustomerId());

			if(OrderCustomer.IsAdminUser && !ThisCustomer.IsAdminSuperUser)
			{
				divContent.Visible = false;
				AlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.SecurityException", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}

			if(!Page.IsPostBack)
			{
				dpNextShipDate.Culture = Thread.CurrentThread.CurrentUICulture;
				dpNextShipDate.MinDate = DateTime.Today;

				var recurringOrder = new Order(OriginalRecurringOrderNumber);
				int OriginalOrderStoreID = Order.GetOrderStoreID(OriginalRecurringOrderNumber);

				var recurringCart = new ShoppingCart(SkinID, OrderCustomer, CartTypeEnum.RecurringCart, OriginalRecurringOrderNumber, false, OriginalOrderStoreID);
				var recurringItem = recurringCart.CartItems
					.FirstOrDefault(ci => ci.OriginalRecurringOrderNumber == OriginalRecurringOrderNumber); //Need a recurring item from this order for some info later

				if(recurringItem == null)
				{
					AlertMessage.PushAlertMessage(string.Format("admin.recurringorder.OrderCouldNotBeFound".StringResource(), OriginalRecurringOrderNumber), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
					Response.Redirect("recurringorders.aspx");
				}

				PopulateIntervalDropdown();
				grdProducts.DataSource = recurringCart.CartItems;
				grdProducts.DataBind();

				litOrderNumber.Text = OriginalRecurringOrderNumber.ToString();
				litRecurringIndex.Text = recurringItem.RecurringIndex.ToString();
				litOriginalDate.Text = Localization.ToThreadCultureShortDateString(recurringOrder.OrderDate);
				
				//prevent exception if selected date is assigned a date < MinDate
				if(recurringItem.NextRecurringShipDate < dpNextShipDate.MinDate)
					dpNextShipDate.MinDate = recurringItem.NextRecurringShipDate;

				dpNextShipDate.SelectedDate = recurringItem.NextRecurringShipDate;
				txtInterval.Text = recurringItem.RecurringInterval.ToString();
				ddIntervalType.SelectedValue = ((int)recurringItem.RecurringIntervalType).ToString();

				addressFrame.Src = String.Format("editaddressrecurring.aspx?addressid={0}&originalrecurringordernumber={1}", OrderCustomer.PrimaryBillingAddressID, OriginalRecurringOrderNumber);

				bool isPPECorder = (recurringOrder.PaymentMethod == AppLogic.ro_PMPayPalExpress);
				btnProcess.Visible = !isPPECorder;
				divAddressInfo.Visible = !isPPECorder;

				//RecurringSubscriptionID means order already processed on the gateway so we can't change it
				bool isGatewayAutoBillOrder = !String.IsNullOrWhiteSpace(recurringItem.RecurringSubscriptionID);
				if(isGatewayAutoBillOrder || !AppLogic.AppConfigBool("AllowRecurringIntervalEditing"))
				{
					lnkRetry.Visible = true;
					lnkRetry.PostBackUrl = String.Format("customer_history.aspx?customerid={0}&retrypaymentid={1}", OrderCustomer.CustomerID, OriginalRecurringOrderNumber);
					lnkRestart.Visible = true;
					lnkRestart.PostBackUrl = String.Format("customer_history.aspx?customerid={0}&restartid={1}", OrderCustomer.CustomerID, OriginalRecurringOrderNumber);

					//We can't modify the ship date, interval, or interval type in admin for GatewayInternalBilling.
					//It must be done on the gateway itself. Ex: for payflow pro go to manager.paypal.com.
					txtInterval.Enabled = false;
					ddIntervalType.Enabled = false;

					//disable all the RadDatePicker properties
					dpNextShipDate.EnableTyping
						= dpNextShipDate.DatePopupButton.Enabled
						= ddIntervalType.Enabled
						= false;

					pnlDateEditNotice.Visible = true;
					lblNoDateEditingExplanation.Text = "admin.recurringorder.OrderShipmentDateNotEditable".StringResource();
					btnUpdate.Enabled = false;
				}
			}
		}

		private int GetRecurringCartCustomerId()
		{
			string custSql = "SELECT CustomerID AS N FROM ShoppingCart WITH (NOLOCK) WHERE CartType = @CartType AND OriginalRecurringOrderNumber = @OriginalRecurringOrderNumber";
			List<SqlParameter> custParams = new List<SqlParameter> { (new SqlParameter("@CartType", (int)CartTypeEnum.RecurringCart)),
																		(new SqlParameter("@OriginalRecurringOrderNumber", OriginalRecurringOrderNumber)) };

			int customerId = DB.GetSqlN(custSql, custParams.ToArray());

			return customerId;
		}

		private void PopulateIntervalDropdown()
		{
			if(!AppLogic.UseSpecialRecurringIntervals())
			{
				ddIntervalType.Items.Add(new ListItem(DateIntervalTypeEnum.Day.ToString(), ((int)DateIntervalTypeEnum.Day).ToString()));
				ddIntervalType.Items.Add(new ListItem(DateIntervalTypeEnum.Week.ToString(), ((int)DateIntervalTypeEnum.Week).ToString()));
				ddIntervalType.Items.Add(new ListItem(DateIntervalTypeEnum.Month.ToString(), ((int)DateIntervalTypeEnum.Month).ToString()));
				ddIntervalType.Items.Add(new ListItem(DateIntervalTypeEnum.Year.ToString(), ((int)DateIntervalTypeEnum.Year).ToString()));
			}
			else
			{
				ddIntervalType.Items.Add(new ListItem(DateIntervalTypeEnum.NumberOfDays.ToString(), ((int)DateIntervalTypeEnum.NumberOfDays).ToString()));
				ddIntervalType.Items.Add(new ListItem(DateIntervalTypeEnum.Weekly.ToString(), ((int)DateIntervalTypeEnum.Weekly).ToString()));
				ddIntervalType.Items.Add(new ListItem(DateIntervalTypeEnum.BiWeekly.ToString(), ((int)DateIntervalTypeEnum.BiWeekly).ToString()));
				ddIntervalType.Items.Add(new ListItem(DateIntervalTypeEnum.Monthly.ToString(), ((int)DateIntervalTypeEnum.Monthly).ToString()));
				ddIntervalType.Items.Add(new ListItem(DateIntervalTypeEnum.Quarterly.ToString(), ((int)DateIntervalTypeEnum.Quarterly).ToString()));
				ddIntervalType.Items.Add(new ListItem(DateIntervalTypeEnum.SemiYearly.ToString(), ((int)DateIntervalTypeEnum.SemiYearly).ToString()));
				ddIntervalType.Items.Add(new ListItem(DateIntervalTypeEnum.Yearly.ToString(), ((int)DateIntervalTypeEnum.Yearly).ToString()));
			}
		}

		protected void btnStopBilling_Click(Object sender, EventArgs e)
		{
			try
			{
				var originalOrder = new Order(OriginalRecurringOrderNumber);
				var recurringOrderManager = new RecurringOrderMgr();

				var result = string.Empty;

				if(originalOrder.PaymentMethod == AppLogic.ro_PMPayPalExpress 
					&& PayPalController.GetAppropriateExpressType() == ExpressAPIType.PayPalExpress)
					result = recurringOrderManager.CancelPPECRecurringOrder(originalOrder.OrderNumber, false);
				else
					result = recurringOrderManager.CancelRecurringOrder(originalOrder.OrderNumber);

				if(result == AppLogic.ro_OK)
					AlertMessage.PushAlertMessage("admin.recurringorder.OrderCancelSuccess".StringResource(), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
				else
					AlertMessage.PushAlertMessage(result, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}
			catch(Exception ex)
			{
				AlertMessage.PushAlertMessage(ex.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}
		}

		protected void btnProcess_Click(Object sender, EventArgs e)
		{
			if(OriginalRecurringOrderNumber != 0)
			{
				RecurringOrderMgr orderMgr = new RecurringOrderMgr();
				string message = orderMgr.ProcessRecurringOrder(OriginalRecurringOrderNumber);

				AlertMessage.PushAlertMessage(message, (message == AppLogic.ro_OK)
					? AspDotNetStorefrontControls.AlertMessage.AlertType.Success
					: AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}
		}

		protected void btnUpdate_Click(Object sender, EventArgs e)
		{
			if(dpNextShipDate.SelectedDate == null || dpNextShipDate.SelectedDate < DateTime.Today)
			{
				AlertMessage.PushAlertMessage(AppLogic.GetString("recurringorder.invalidshipdate", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
				return;
			}

			DateTime newShipDate = (DateTime)dpNextShipDate.SelectedDate;

			int newInterval = int.Parse(txtInterval.Text.Trim());
			int newIntervalType = int.Parse(ddIntervalType.SelectedValue);

			try
			{
				if(newShipDate != null && newShipDate != System.DateTime.MinValue)
				{
					DB.ExecuteSQL(String.Format("update shoppingcart set NextRecurringShipDate={0} where originalrecurringordernumber={1}",
						DB.DateQuote(Localization.ToDBShortDateString(newShipDate)), OriginalRecurringOrderNumber));
				}

				if(newInterval != 0)
				{
					DB.ExecuteSQL(String.Format("update shoppingcart set RecurringInterval={0} where originalrecurringordernumber={1}",
						newInterval, OriginalRecurringOrderNumber));
				}

				if(newIntervalType != 0)
				{
					DB.ExecuteSQL(String.Format("update shoppingcart set RecurringIntervalType={0} where originalrecurringordernumber={1}",
						newIntervalType, OriginalRecurringOrderNumber));
				}

				AlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.ItemUpdated", ThisCustomer.LocaleSetting), AspDotNetStorefrontControls.AlertMessage.AlertType.Success);
			}
			catch(Exception ex)
			{
				AlertMessage.PushAlertMessage(ex.Message, AspDotNetStorefrontControls.AlertMessage.AlertType.Error);
			}
		}
	}
}
