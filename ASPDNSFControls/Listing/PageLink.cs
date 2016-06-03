// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------

namespace AspDotNetStorefrontControls.Listing
{
	public class PageLink
	{
		public string Display
		{ get { return _Display; } }

		public long ItemIndex
		{ get { return _ItemIndex; } }

		public bool Enabled
		{ get { return _Enabled; } }

		public bool Current
		{ get { return _Current; } }

		readonly string _Display;
		readonly long _ItemIndex;
		readonly bool _Enabled;
		readonly bool _Current;

		public PageLink(string display, long itemIndex, bool enabled = false, bool current = false)
		{
			_Display = display;
			_ItemIndex = itemIndex;
			_Enabled = enabled;
			_Current = current;
		}
	}
}
