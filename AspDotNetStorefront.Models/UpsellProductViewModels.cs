// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace AspDotNetStorefront.Models
{
	public class UpsellProductsViewModel
	{
		public IEnumerable<UpsellProductViewModel> UpsellProducts { get; set; }
	}

	public class UpsellProductViewModel
	{
		public int ProductId { get; set; }
		public string Name { get; set; }
		public string AltText { get; set; }
		public string ProductLink { get; set; }
		public string ImageUrl { get; set; }
		public string DisplayPrice { get; set; }
		public bool Selected { get; set; }
	}
}
