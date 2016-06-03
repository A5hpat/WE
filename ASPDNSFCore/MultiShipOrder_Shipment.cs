// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace AspDotNetStorefrontCore
{
	public class MultiShipOrder_Shipment
    {
        #region fields and properties
        private int _MultiShipOrder_ShipmentId = 0;
        private int _OrderNumber = 0;
        private string _DestinationAddress = string.Empty;
        private decimal _ShippingAmount = 0M;
        private int _ShippingMethodId = 0;
        private int _ShippingAddressId = 0;
        private int _BillingAddressId = 0;

        public int ShippingAddressId
        {
            set { _ShippingAddressId = value; }
        }
        public int BillingAddressId
        {
            set { _BillingAddressId = value; }
        }
        public int ShippingMethodId
        {
            set { _ShippingMethodId = value; }
        }
        public int OrderNumber
        {
            set { _OrderNumber = value; }
        }
        public string DestinationAddress
        {
            set { _DestinationAddress = value; }
        }
        public decimal ShippingAmount
        {
            set { _ShippingAmount = value; }
        }
        #endregion

        public void Save()
        {
            if (_MultiShipOrder_ShipmentId > 0)
                UpdateDB();
            else
                InsertDB();
        }

        /// <summary>
        /// Adds Shipment details from a Multiple Shipment Order 
        /// </summary>
        private void InsertDB()
        {
			var MultiShipOrder_ShipmentGUID = CommonLogic.GetNewGUID();
			var sql = string.Format("insert into MultiShipOrder_Shipment(MultiShipOrder_ShipmentGUID,OrderNumber,DestinationAddress,ShippingAmount,ShippingMethodId,ShippingAddressId,BillingAddressId) values({0},{1},{2},{3},{4},{5},{6})", DB.SQuote(MultiShipOrder_ShipmentGUID), _OrderNumber, DB.SQuote(_DestinationAddress), _ShippingAmount, _ShippingMethodId,_ShippingAddressId,_BillingAddressId);
            DB.ExecuteSQL(sql);

            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS(String.Format("select MultiShipOrder_ShipmentId from MultiShipOrder_Shipment with (NOLOCK) where MultiShipOrder_ShipmentGUID={0}", DB.SQuote(MultiShipOrder_ShipmentGUID)), dbconn))
                {
                    if (rs.Read())
                    {
                        _MultiShipOrder_ShipmentId = DB.RSFieldInt(rs, "MultiShipOrder_ShipmentId");
                    }
                }
            }
        }

		/// <summary>
		/// Updates Shipment details from a Multiple Shipment Order 
		/// </summary>
		private void UpdateDB()
        {
            string sql = String.Format("update Address set OrderNumber={1},DestinationAddress={2},ShippingAmount={3},ShippingMethodId={4},ShippingAddressId={5},BillingAddressId={6} where MultiShipOrder_Shipmentid={0}", _MultiShipOrder_ShipmentId, _OrderNumber, DB.SQuote(_DestinationAddress), _ShippingAmount, _ShippingMethodId,_ShippingAddressId,_BillingAddressId);

            DB.ExecuteSQL(sql);
        }
    }
}
