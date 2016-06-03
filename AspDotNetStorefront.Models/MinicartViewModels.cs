// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AspDotNetStorefront.Validation.DataAttribute;

namespace AspDotNetStorefront.Models
{
	public class MinicartViewModel
	{
		public IEnumerable<MinicartItemViewModel> CartItems { get; set; }
		public int MaximumCartItemsToDisplay { get; set; }
		public bool UseMaximumCartItemBehavor { get; set; }
		public string Total { get; set; }
		public string SubTotal { get; set; }
		public string Discount { get; set; }
		public int ItemCount { get; set; }
		public int OtherMiniCount { get; set; }
		public bool AllowQuantityUpdate { get; set; }
		public bool MinicartEnabled { get; set; }
		public bool MiniwishEnabled { get; set; }
	}

	public class MinicartItemViewModel
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public int VariantId { get; set; }
		public int RecurringVariantId { get; set; }
		public string ProductName { get; set; }
		public string VariantName { get; set; }
		public string ChosenColor { get; set; }
		public string ChosenColorSkuModifier { get; set; }
		public string ChosenSize { get; set; }
		public string ChosenSizeSkuModifier { get; set; }
		public string ProductSku { get; set; }
		public string SubTotalDisplay { get; set; }
		public string VatDisplay { get; set; }
		public string Notes { get; set; }
		public string ProductUrl { get; set; }
		public string ProductImageUrl { get; set; }
		public string ProductImageAlternateText { get; set; }
		[MinValue(0, ErrorMessage = "minicart.quantity.invalid")]
		[Required(ErrorMessage = "minicart.quantity.invalid")]
		public int Quantity { get; set; }
		public bool ShowImage { get; set; }
		public bool ShowSku { get; set; }
		public string TextOptionLabel { get; set; }
		public string TextOption { get; set; }
		public string LineItemPromotionText { get; set; }
		public SelectList RecurringIntervalOptions { get; set; }
		public bool LinkToProduct { get; set; }
        public bool IsAKit { get; set; }
		public List<KitCartItemViewModel> KitDetails { get; set; }
		public bool AllowQuantityUpdate { get; set; }
		public bool ShowEditLink { get; set; }
		public string EditUrl { get; set; }
		public SelectList RestrictedQuantities { get; set; }
	}
}
