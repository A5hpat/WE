// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace AspDotNetStorefrontCore
{
	public class KitComposition : IEnumerable<KitCartItem>
    {
        public KitComposition(int cartId)
        {
            this.CartID = cartId;
        }

        public int CartID;
        public List<KitCartItem> Compositions = new List<KitCartItem>();

        public bool Matches(KitComposition other)
        {
            if (this.Compositions.Count != other.Compositions.Count) return false;

            bool matchesAll = false;

            foreach (KitCartItem kitItem in Compositions)
            {
                matchesAll = other.Compositions.Find(kitItem.Match) != null;

                if (!matchesAll)
                {
                    return false;
                }
            }

            return true;
        }

        public static KitComposition FromCart(Customer thisCustomer, CartTypeEnum cartType, int cartId)
        {
			var composition = new KitComposition(cartId);

            using (var dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
				var query = string.Format(
                                @"SELECT 
                                    kc.ShoppingCartRecID, 
                                    kc.ProductID, 
                                    kc.VariantID, 
                                    kc.KitGroupID, 
                                    kc.KitItemID,
                                    ki.Name,
                                    kc.TextOption,
                                    kc.Quantity
                                    FROM KitCart kc WITH (NOLOCK)
                                    INNER JOIN ShoppingCart sc WITH (NOLOCK) ON kc.ShoppingCartRecID = sc.ShoppingCartRecID 
                                    INNER JOIN KitItem ki ON ki.KitItemID = kc.KitItemID
                                    WHERE kc.CustomerID = {0} AND sc.CartType = {1}  AND sc.ShoppingCartRecID = {2}", 
                                    thisCustomer.CustomerID, (int)cartType, cartId);
                using (var reader = DB.GetRS(query,dbconn))
                {
                    while (reader.Read())
                    {
						var kit = new KitCartItem();
                        kit.CustomerID = thisCustomer.CustomerID;
                        kit.ProductID = DB.RSFieldInt(reader, "ProductID");
                        kit.VariantID = DB.RSFieldInt(reader, "VariantID");
                        kit.KitGroupID = DB.RSFieldInt(reader, "KitGroupID");
                        kit.KitItemID = DB.RSFieldInt(reader, "KitItemID");
                        kit.Name = DB.RSFieldByLocale(reader, "Name", thisCustomer.LocaleSetting);
                        kit.TextOption = DB.RSField(reader, "TextOption");
                        kit.Quantity = DB.RSFieldInt(reader, "Quantity");

                        composition.Compositions.Add(kit);
                    }
                }

            }
      
            return composition;
        }

        public static KitComposition FromOrder(Customer thisCustomer, int orderNumber, int cartId)
        {
			var composition = new KitComposition(cartId);

			var query = string.Format("SELECT okc.ProductID, okc.VariantID, okc.KitGroupID, okc.KitItemID, okc.TextOption, okc.Quantity FROM Orders_KitCart okc INNER JOIN Orders_ShoppingCart osc ON osc.ShoppingCartRecID = okc.ShoppingCartRecID INNER JOIN Orders o ON osc.OrderNumber = o.OrderNumber WHERE o.CustomerID = {0} AND o.OrderNumber = {1} AND osc.ShoppingCartRecID = {2}", thisCustomer.CustomerID, orderNumber, cartId);

            using (var dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (var reader = DB.GetRS(query,dbconn))
                {
                    while (reader.Read())
                    {
						var kit = new KitCartItem();
                        kit.CustomerID = thisCustomer.CustomerID;
                        kit.ProductID = DB.RSFieldInt(reader, "ProductID");
                        kit.VariantID = DB.RSFieldInt(reader, "VariantID");
                        kit.KitGroupID = DB.RSFieldInt(reader, "KitGroupID");
                        kit.KitItemID = DB.RSFieldInt(reader, "KitItemID");
                        kit.TextOption = DB.RSField(reader, "TextOption");
                        kit.Quantity = DB.RSFieldInt(reader, "Quantity");

                        composition.Compositions.Add(kit);
                    }
                }
            
            }
            
            return composition;
        }

        #region IEnumerable<KitCartItem> Members

        public IEnumerator<KitCartItem> GetEnumerator()
        {
            return Compositions.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public class KitCartItem
    {
        public int ProductID;
        public int VariantID;
        public int KitGroupID;
        public int KitItemID;
        public int CustomerID;
        public string TextOption = string.Empty;
        public HttpPostedFile ImageFile = null;
        public int Quantity = 1;

        public bool ContentIsImage
        {
            get 
            {                
                // check the extension
                return ValidImageExtension(TextOption) && File.Exists(HttpContext.Current.Request.MapPath(TextOption));
            }
        }

        private static string[] validImageExtensions = { ".jpg", ".gif", ".png" };

        private bool ValidImageExtension(string file)
        {
            try
            {
                string fileExt = Path.GetExtension(file);
                foreach (string ext in validImageExtensions)
                {
                    if (fileExt.Equals(ext))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }
            

            return false;
        }

        public string Name = string.Empty;        
        public bool Match(KitCartItem other)
        {
            return this.ProductID.Equals(other.ProductID) &&
                    this.VariantID.Equals(other.VariantID) &&
                    this.KitGroupID.Equals(other.KitGroupID) &&
                    this.KitItemID.Equals(other.KitItemID) &&
                    this.CustomerID.Equals(other.CustomerID) &&
                    this.TextOption == other.TextOption;
        }
    }
}
