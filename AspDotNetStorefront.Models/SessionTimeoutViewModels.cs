// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace AspDotNetStorefront.Models
{
	public class AddTimerViewModel
	{
		public readonly bool Enabled;
		public readonly TimeSpan SessionTimeout;
		public readonly string RefreshUrl;

		public AddTimerViewModel(bool enabled, TimeSpan sessionTimeout, string refreshUrl)
		{
			Enabled = enabled;
			SessionTimeout = sessionTimeout;
			RefreshUrl = refreshUrl;
		}
	}
}
