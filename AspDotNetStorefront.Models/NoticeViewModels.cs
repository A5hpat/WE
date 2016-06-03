// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Models
{
	public class NoticeViewModel
	{
		public readonly IEnumerable<Notice> Notices;

		public NoticeViewModel(IEnumerable<Notice> notices)
		{
			Notices = notices;
		}
	}
}
