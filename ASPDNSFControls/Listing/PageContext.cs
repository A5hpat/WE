// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace AspDotNetStorefrontControls.Listing
{
	public class PageContext
	{
		public long FirstItemIndex
		{ get { return _FirstItemIndex; } }

		public long LastItemIndex
		{ get { return _LastItemIndex; } }

		public long ItemCount
		{ get { return _ItemCount; } }

		public long PageIndex
		{ get { return _PageIndex; } }

		public long PageCount
		{ get { return _PageCount; } }

		public IEnumerable<PageLink> PageLinks
		{ get { return _PageLinks; } }

		readonly long _FirstItemIndex;
		readonly long _LastItemIndex;
		readonly long _ItemCount;
		readonly long _PageIndex;
		readonly long _PageCount;
		readonly IEnumerable<PageLink> _PageLinks;

		public PageContext(long firstItemIndex, long lastItemIndex, long itemCount, long pageIndex, long pageCount, IEnumerable<PageLink> pageLinks)
		{
			_FirstItemIndex = firstItemIndex;
			_LastItemIndex = lastItemIndex;
			_ItemCount = itemCount;
			_PageIndex = pageIndex;
			_PageCount = pageCount;
			_PageLinks = pageLinks;
		}
	}
}
