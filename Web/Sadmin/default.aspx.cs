// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class _default : AspDotNetStorefront.Admin.AdminPageBase
	{
        #region CONSTANT VARIABLES
        const string ORDER_COUNT_SQL = "SELECT COUNT(OrderNumber) FROM Orders WITH (NOLOCK) WHERE Deleted = 0";
        const string CUSTOMER_COUNT_SQL = "SELECT COUNT(DISTINCT Customer.CustomerID) FROM Customer WITH (NOLOCK) INNER JOIN Orders ON Customer.CustomerID = Orders.CustomerID WHERE Customer.Deleted = 0 AND IsRegistered = 1";
        const string CONTACT_COUNT_SQL = "SELECT COUNT(DISTINCT Customer.CustomerID) FROM Customer WITH (NOLOCK) LEFT JOIN Orders ON Customer.CustomerID = Orders.CustomerID WHERE Orders.CustomerID IS NULL AND Customer.Deleted = 0 AND IsRegistered = 1";
        const string PUBLISHED_PRODUCT_COUNT_SQL = "SELECT COUNT(ProductID) FROM Product WITH (NOLOCK) WHERE Published = 1 AND Deleted = 0";
        const string ACTIVE_PROMOTION_COUNT_SQL = "SELECT COUNT(Id) FROM Promotions WITH (NOLOCK) WHERE Active = 1";       
        #endregion

        #region VARIABLES
        string LowStockCountSql = string.Format("SELECT COUNT(CASE WHEN ISNULL(Quan, 0) > 0 THEN Quan ELSE Inventory END) [Inventory] FROM Product p, ProductVariant pv LEFT JOIN Inventory i ON pv.VariantId = i.VariantId WHERE p.Deleted=0 AND pv.Deleted=0 AND p.ProductId = pv.ProductId AND CASE WHEN ISNULL(Quan, 0) > 0 THEN Quan ELSE Inventory END < {0}", AppLogic.AppConfigNativeInt("SendLowStockWarningsThreshold"));
        #endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			if(CommonLogic.QueryStringCanBeDangerousContent("flushcache").Length != 0 || CommonLogic.QueryStringCanBeDangerousContent("resetcache").Length != 0 || CommonLogic.QueryStringCanBeDangerousContent("clearcache").Length != 0)
			{
				foreach(DictionaryEntry dEntry in HttpContext.Current.Cache)
				{
					HttpContext.Current.Cache.Remove(dEntry.Key.ToString());
				}
				AppLogic.m_RestartApp();
				Response.Redirect(AppLogic.AdminLinkUrl("default.aspx"));
			}

			divLowStock.Visible = ShowLowStockAudit();
			CustomerSession.StaticClear();
			
			CheckForMaintenanceAlert();
			CheckForSecurityAlerts();

			if(!IsPostBack)
			{
				SetCacheSwitch(AppLogic.AppConfigBool("CacheMenus"));
				loadGrids();
				currencySymbol.Value = Currency.GetDefaultCurrencySymbol();
				LoadQuickLook();
			}
		}

		void CheckForSecurityAlerts()
		{
			var auditList = Security.GetAuditIssues(new HttpRequestWrapper(Request));

			AlertMessageDisplay.PushAlertMessage(
					string.Format(AppLogic.GetString("admin.default.auditsummary"),
						auditList.Where(i => i.ItemType == SecurityAuditItemType.Security).Count(),
						auditList.Where(i => i.ItemType == SecurityAuditItemType.Configuration).Count()),
					AlertMessage.AlertType.Warning);
		}

		void CheckForMaintenanceAlert()
		{
			var nextMaintenanceDate = DateTime.MinValue;
			if(DateTime.TryParse(AppLogic.AppConfig("NextMaintenanceDate"), out nextMaintenanceDate))
			{
				if(nextMaintenanceDate < DateTime.Now)
					AlertMessageDisplay.PushAlertMessage(AppLogic.GetString("admin.monthlymaintenance.overdue"), AlertMessage.AlertType.Warning);
			}
		}
		
		protected void loadGrids()
		{
			var SummaryReportFields = "OrderNumber,OrderDate,OrderTotal,FirstName,LastName,ShippingMethod,isnull(MaxMindFraudScore, -1) MaxMindFraudScore ";
            var summarySQL = string.Format("SELECT TOP 10 {0} from Orders with (NOLOCK) ORDER BY OrderDate DESC", SummaryReportFields);

			using(var con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();

				using(var rs = DB.GetRS(summarySQL, con))
				{
					gOrders.DataSource = rs;
					gOrders.DataBind();
				}
			}
		}

		/// <summary>
		/// Populates the quick look section of the dashboard
		/// </summary>
		void LoadQuickLook()
		{
            lblCompletedOrders.Text = GetDashboardData(ORDER_COUNT_SQL).ToString();
			lblCustomerWithOrders.Text = GetDashboardData(CUSTOMER_COUNT_SQL).ToString();
			lblCustomersWithOutOrders.Text = GetDashboardData(CONTACT_COUNT_SQL).ToString();
			lblPublishedProducts.Text = GetDashboardData(PUBLISHED_PRODUCT_COUNT_SQL).ToString();
			lblActivePromotions.Text = GetDashboardData(ACTIVE_PROMOTION_COUNT_SQL).ToString();

			if(ShowLowStockAudit())
			{
				divLowStockCount.Visible = true;
                lblLowStock.Text = GetDashboardData(LowStockCountSql).ToString();
			}
			else
				divLowStockCount.Visible = false;

			lblAcceptPayPal.Text = DetermineIfPayPalIsPaymentMethod() == true ? "Yes" : "No";
			lblVersion.Text = CommonLogic.GetVersion(false);
		}

		/// <summary>
		/// Gets data for the dashboard based on passed in sql
		/// </summary>
		/// <param name="sqlStatement"></param>
		/// <returns></returns>
		int GetDashboardData(string sqlStatement)
		{
			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

                using (var command = new SqlCommand(sqlStatement, connection))
				{
					var response = command.ExecuteScalar();
					if(!(response is DBNull) && (response != null))
						return (int)response;
					else
						return 0;
				}
			}
		}

		/// <summary>
		/// Determines if paypal is used
		/// </summary>
		/// <returns></returns>
		bool DetermineIfPayPalIsPaymentMethod()
		{
			if(AppLogic.AppConfig("PaymentGateway").ToLowerInvariant().Contains("paypal"))
				return true;
			else
			{
                var paymentMethods = new List<string>();
				string[] paymentMethodsCommaSeparated = AppLogic.AppConfig("PaymentMethods", 0, false).ToUpperInvariant().Split(',');
				foreach(string paymentMethod in paymentMethodsCommaSeparated)
				{
                    if (paymentMethod.Trim().ToLowerInvariant().Contains("paypal") || paymentMethod.Trim().ToLowerInvariant().Contains("payflowpro"))
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns a boolean to indicate whether or not to show the low stock audit
		/// </summary>
		/// <returns></returns>
		bool ShowLowStockAudit()
		{
			return (AppLogic.AppConfigBool("ShowAdminLowStockAudit") && ThisCustomer.IsAdminSuperUser);
		}

		protected void SetCache(Object sender, CommandEventArgs e)
		{
            var cacheIsOn = e.CommandArgument.ToString() == "on";

			AppConfigManager.SetAppConfigValue("CacheMenus", cacheIsOn.ToString(), AppLogic.StoreID());

			SetCacheSwitch(cacheIsOn);
		}


		private void SetCacheSwitch(bool cacheIsOn)
		{
			if(cacheIsOn)
			{
				CacheToggleWrap.CssClass = "toggle-wrap on";
				CacheToggle.CommandArgument = "off";
			}
			else
			{
				CacheToggleWrap.CssClass = "toggle-wrap";
				CacheToggle.CommandArgument = "on";
			}
		}
	}
}
