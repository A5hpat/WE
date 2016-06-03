// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace AspDotNetStorefront.Models
{
	public class OrderOptionViewModel
	{
		public IEnumerable<OrderOptionItemViewModel> Options { get; set; }
	}

	public class OrderOptionItemViewModel
	{
		public Guid OrderOptionGuid { get; set; }

		public int Id { get; set; }

		public int TaxClassId { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public string Cost { get; set; }

		public string ImageUrl { get; set; }

		public bool Checked { get; set; }

		public bool HasImage { get; set; }
	}
}
