// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Models
{
	public class TwoCheckoutViewModel
	{
		public string LiveServerUrl { get; set; }

		public string ReturnUrl { get; set; }

		public string Login { get; set; }

		public string Total { get; set; }

		public string InvoiceNumber { get; set; }

		public string Email { get; set; }

		public Address BillingAddress { get; set; }

		public bool UseLiveTransactions { get; set; }
	}
}
