// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------

namespace AspDotNetStorefrontControls.Listing
{
	public class PagerContext
	{
		public int[] PageSizes
		{ get { return _PageSizes; } }

		public int PageSize
		{ get { return _PageSize; } }

		public int DisplayedPageSelectorCount
		{ get { return _DisplayedPageSelectorCount; } }

		readonly int[] _PageSizes;
		readonly int _PageSize; 
		readonly int _DisplayedPageSelectorCount;

		public PagerContext(int[] pageSizes, int pageSize, int displayedPageSelectorCount)
		{
			_PageSizes = pageSizes;
			_PageSize = pageSize;
			_DisplayedPageSelectorCount = displayedPageSelectorCount;
		}
	}
}
