// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;

namespace AspDotNetStorefront.Models
{
	public class PurchaseOrderViewModel
	{
		[Display(Name = "checkout.purchaseordernumber.label", Prompt = "checkout.purchaseordernumber.example")]
		public string PONumber
		{ get; set; }
	}
}
