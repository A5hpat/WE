// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AspDotNetStorefront.Models
{
	public class ShippingEstimateViewModel
	{
		[Display(Name = "checkoutshipping.AddressControl.Country")]
		[Required(ErrorMessage = "shippingestimator.country.required")]
		public string Country { get; set; }
		[Display(Name = "checkoutshipping.AddressControl.City")]
		public string City { get; set; }
		[Display(Name = "checkoutshipping.AddressControl.State")]
		public string State { get; set; }
		[Display(Name = "checkoutshipping.AddressControl.PostalCode")]
		public string PostalCode { get; set; }
		public IEnumerable<SelectListItem> Countries { get; set; }
		public IEnumerable<SelectListItem> States { get; set; }
		public bool ShowNoRates { get; set; }
		public bool ShippingCalculationRequiresCityAndState { get; set; }
	}
}
