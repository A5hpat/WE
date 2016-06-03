// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Admin
{
	/// <summary>
	/// Base class for all admin site pages
	/// </summary>
	public class AdminPromoUsage
    {
        #region Private Variables
        public string Code { get; set; }
        public string ShippingDiscount { get; set; }
        public string LineItemDiscount { get; set; }
        public string OrderDiscount { get; set; }
        public string TotalDiscount { get; set; }
        public bool GiftWithPurchase { get; set; }
        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public AdminPromoUsage()
        { }

        #endregion
    }
}
