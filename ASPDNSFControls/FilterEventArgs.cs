// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace AspDotNetStorefrontControls
{
	public class FilterEventArgs : EventArgs
	{
		public FilterEventArgs(FilterType type, string value, int page)
		{
			this.Type = type;
			this.Filter = value;
			this.Page = page;
		}

		public FilterType Type { get; set; }

		public string Filter { get; set; }

		public int Page { get; set; }
	}
}
