// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Linq;
using AspDotNetStorefront;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class SystemLogs : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void btnClear_Click(Object sender, EventArgs e)
		{
			SysLog.Clear();
			Response.Redirect(AppLogic.AdminLinkUrl("default.aspx"));
		}

		protected void btnExport_Click(Object sender, EventArgs e)
		{
			Response.Clear();
			Response.ContentType = "application/octet-stream";
			Response.AddHeader("Content-Disposition", "attachment; filename=SystemLog.txt");

			Response.Write("----------------------------------------------------------------------\r\n");
			Response.Write("----------------------------------------------------------------------\r\n");
			Response.Write("SYSTEM INFORMATION \r\n");
			Response.Write("----------------------------------------------------------------------\r\n");
			Response.Write("----------------------------------------------------------------------\r\n");

			Response.Write("\r\n");
			Response.Write("Version:            " + CommonLogic.GetVersion() + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("On Live Server:     " + AppLogic.OnLiveServer().ToString() + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("UseSSL:             " + AppLogic.UseSSL().ToString() + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Is Secure Page:     " + CommonLogic.IsSecureConnection().ToString() + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Changed Admin Dir:  " + CommonLogic.IIF(AppLogic.AppConfig("AdminDir").ToLowerInvariant() == "admin", "No", "Yes") + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Caching:            " + (AppLogic.AppConfigBool("CacheMenus") ? AppLogic.GetString("admin.common.OnUC", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) : AppLogic.GetString("admin.common.OffUC", AppLogic.DefaultSkinID(), Localization.GetDefaultLocale())) + "\r\n");
			Response.Write("\r\n");
			Response.Write("----------------------LOCALIZATION----------------------\r\n");
			Response.Write("\r\n");
			Response.Write("Web Config Locale:  " + Localization.GetDefaultLocale() + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("SQL Server Locale:  " + Localization.GetSqlServerLocale() + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Store Currency:     " + AppLogic.AppConfig("Localization.StoreCurrency") + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Store Currency Code:" + AppLogic.AppConfig("Localization.StoreCurrencyNumericCode") + "\r\n");
			Response.Write("\r\n");
			Response.Write("----------------------GATEWAY INFO----------------------\r\n");
			Response.Write("\r\n");
			Response.Write("Payment Gateway:    " + AppLogic.ActivePaymentGatewayRAW() + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Use Live Transactions:" + (AppLogic.AppConfigBool("UseLiveTransactions") ? AppLogic.GetString("admin.splash.aspx.20", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) : AppLogic.GetString("admin.splash.aspx.21", AppLogic.DefaultSkinID(), Localization.GetDefaultLocale())) + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Transaction Mode:   " + AppLogic.AppConfig("TransactionMode").ToString() + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Payment Methods:    " + AppLogic.AppConfig("PaymentMethods").ToString() + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Primary Currency:   " + Localization.GetPrimaryCurrency() + "\r\n");
			Response.Write("\r\n");
			Response.Write("----------------------SHIPPING INFO----------------------\r\n");
			Response.Write("\r\n");
			Response.Write("Shipping Rates Table:" + DB.GetSqlS("select Name as S from dbo.ShippingCalculation where Selected=1") + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Origin State:       " + AppLogic.AppConfig("RTShipping.OriginState") + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Origin Zip:         " + AppLogic.AppConfig("RTShipping.OriginZip") + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Origin Country:     " + AppLogic.AppConfig("RTShipping.OriginCountry") + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Free Shipping Threshold: " + AppLogic.AppConfigNativeDecimal("FreeShippingThreshold").ToString() + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Free Shipping Method:" + AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn") + "\r\n");
			Response.Write("----------------------\r\n");
			Response.Write("Allow Rate Selection:" + CommonLogic.IIF(AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"), "On", "Off") + "\r\n");
			Response.Write("\r\n\r\n");

			var whereClause = FilteredListing.GetFilterWhereClause();
			var sql = String.Format("select * from aspdnsf_SysLog where {0}", whereClause.Sql);

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = new SqlCommand(sql, connection))
			{
				connection.Open();
				command.Parameters.AddRange(whereClause.Parameters.ToArray());

				using(var reader = command.ExecuteReader())
					while(reader.Read())
					{
						Response.Write("----------------------------------------------------------------------\r\n");
						Response.Write("----------------------------------------------------------------------\r\n");
						Response.Write(AppLogic.GetString("admin.systemlog.aspx.1", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) + ": ");
						Response.Write(DB.RSFieldInt(reader, "SysLogID").ToString() + "  " + DB.RSFieldDateTime(reader, "CreatedOn").ToString() + "  \r\n");
						Response.Write("----------------------------------------------------------------------\r\n");
						Response.Write("----------------------------------------------------------------------\r\n");
						Response.Write("\r\n\r\n");
						Response.Write(DB.RSField(reader, "Message"));
						Response.Write("\r\n\r\n");
						Response.Write(DB.RSField(reader, "Details"));
						Response.Write("\r\n\r\n");
					}
			}

			Response.Flush();
			Response.Close();
		}
	}
}
