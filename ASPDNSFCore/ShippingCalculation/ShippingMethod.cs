// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefrontCore.ShippingCalculation
{
	public class ShippingMethod
	{
		public int Id
		{ get; set; }

		public string Name
		{ get; set; }

		public string DisplayName
		{ get; set; }

		public decimal Freight
		{ get; set; }

		public bool IsFree
		{ get; set; }

		public bool ShippingIsFree
		{ get; set; }

		public bool IsRealTime
		{ get; set; }

		public decimal VatRate
		{ get; set; }

		public string ImageFileName
		{ get; set; }

		public string Carrier
		{ get; set; }

		public decimal FreeItemsRate
		{ get; set; }

		public ShippingMethod()
		{
			Name = string.Empty;
			DisplayName = string.Empty;
			Carrier = string.Empty;
		}

		public string GetNameForDisplay()
		{
			return !string.IsNullOrEmpty(DisplayName)
				? DisplayName
				: Name;
		}
	}
}
