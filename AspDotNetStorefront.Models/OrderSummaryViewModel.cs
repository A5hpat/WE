// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;

namespace AspDotNetStorefront.Models
{
	public class OrderSummaryViewModel
	{
		public string SubTotal { get; set; }

		[Display(Name = "shoppingcart.cs.200")]
		public string DiscountTotal { get; set; }

		[Display(Name = "shoppingcart.aspx.12")]
		public string ShippingTotal { get; set; }

		public string ShippingVatCaption { get; set; }

		[Display(Name = "shoppingcart.aspx.14")]
		public string TaxTotal { get; set; }

		public bool HasGiftCardDiscountTotal { get; set; }

		[Display(Name = "checkout.giftcard.label")]
		public string GiftCardDiscountTotal { get; set; }

		[Display(Name = "shoppingcart.cs.61")]
		public string Total { get; set; }

		public bool HasDiscount { get; set; }

		public bool ShowVatLabels { get; set; }

		public bool ShowTax { get; set; }
	}
}
