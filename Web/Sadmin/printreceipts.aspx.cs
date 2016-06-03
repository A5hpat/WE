// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class printreceipts : AspDotNetStorefront.Admin.AdminPageBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            StringBuilder writer = new StringBuilder();
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Server.ScriptTimeout = 3000000;

			Customer ThisCustomer = Context.GetCustomer();
            String OrderNumbers = CommonLogic.QueryStringCanBeDangerousContent("OrderNumbers").Trim(',');

            if (OrderNumbers.Length <= 1)
            {
                writer.Append(AppLogic.GetString("admin.common.NothingToPrint", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                return;
            }

            String ShippingMethods = String.Empty;

            using (var conn = DB.dbConn())
            {
                conn.Open();
                using (var rs = DB.GetRS("select distinct shippingmethodid from orders with (NOLOCK) where ordernumber in (" + OrderNumbers + ")", conn))
                {
                    while (rs.Read())
                    {
                        if (ShippingMethods.Length != 0)
                        {
                            ShippingMethods += ",";
                        }
                        ShippingMethods += DB.RSFieldInt(rs, "ShippingMethodID").ToString();
                    }
                }
            }

            bool firstpass = true;
            foreach (String sx in ShippingMethods.Split(','))
            {

                int ShippingMethodID = 0;
                try
                {
                    ShippingMethodID = System.Int32.Parse(sx);
                }
                catch { }

                String TheseOrderNumbers = String.Empty;

                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var rs2 = DB.GetRS("select distinct ordernumber from orders with (NOLOCK) where OrderNumber in (" + OrderNumbers + ") and ShippingMethodID=" + ShippingMethodID.ToString(), conn))
                    {
                        while (rs2.Read())
                        {
                            if (TheseOrderNumbers.Length != 0)
                            {
                                TheseOrderNumbers += ",";
                            }
                            TheseOrderNumbers += DB.RSFieldInt(rs2, "ordernumber").ToString();
                        }
                    }
                }

                if (TheseOrderNumbers.Length > 0)
                {
                    String SM = String.Empty;
                    using (var conn = DB.dbConn())
                    {
                        conn.Open();
                        using (var rs3 = DB.GetRS("Select * from shippingmethod with (NOLOCK) where ShippingMethodID=" + ShippingMethodID.ToString(), conn))
                        {
                            if (rs3.Read())
                            {
                                SM = DB.RSFieldByLocale(rs3, "Name", Localization.GetDefaultLocale());
                            }
                            if (SM.Length == 0)
                            {
                                SM = AppLogic.ro_NotApplicable;
                            }
                        }
                    }

                    foreach (String s in TheseOrderNumbers.Split(','))
                    {
                        if (s.Length != 0)
                        {
                            if (firstpass)
                            {
                                writer.Append("<p><H1><b>" + String.Format(AppLogic.GetString("admin.printreceipts.ShippingMethod", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), SM) + "</b></H1><p>");
                                firstpass = false;
                            }
                            else
                            {
                                writer.Append("<p style=\"page-break-before: always;\"><H1><b>" + String.Format(AppLogic.GetString("admin.printreceipts.ShippingMethod", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), SM) + "</b></H1></p>");
                            }
                            int ONX = System.Int32.Parse(s);
                            Order o = new Order(ONX, ThisCustomer.LocaleSetting);
                            writer.Append(o.Receipt(ThisCustomer, false));
                            DB.ExecuteSQL("update orders set IsPrinted=1 where OrderNumber=" + ONX.ToString());
                        }
                    }
                }
            }
            ltContent.Text = writer.ToString();
        }
    }
}
