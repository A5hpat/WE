// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;

namespace AspDotNetStorefrontControls.Listing
{
	class Pager
	{
		public PageContext CreatePageContext(PagerContext pagerContext, long firstItemIndex, long lastItemIndex, long itemCount, long pageIndex, long pageCount)
		{
			return new PageContext(
				firstItemIndex: firstItemIndex,
				lastItemIndex: lastItemIndex,
				itemCount: itemCount,
				pageIndex: pageIndex,
				pageCount: pageCount,
				pageLinks: GeneratePageLinks(pagerContext, firstItemIndex, lastItemIndex, itemCount, pageIndex).ToArray());
		}

		public IEnumerable<PageLink> GeneratePageLinks(PagerContext pagerContext, long firstItemIndex, long lastItemIndex, long itemCount, long pageIndex)
		{
			yield return new PageLink(
				display: "&laquo;",
				itemIndex: 0,
				enabled: firstItemIndex > 0);

			yield return new PageLink(
				display: "&lsaquo;",
				itemIndex: firstItemIndex / pagerContext.PageSize * pagerContext.PageSize - pagerContext.PageSize,
				enabled: firstItemIndex > 0);

			var halfPageSelectors = pagerContext.DisplayedPageSelectorCount / 2;

			var directPageLinks = Enumerable
				.Range(-pagerContext.DisplayedPageSelectorCount, pagerContext.DisplayedPageSelectorCount * 2)		// Take the range of page selectors negative and positive
				.Select(index => index * pagerContext.PageSize + firstItemIndex)									// Map to item indices relative to the current page
				.Where(itemIndex => itemIndex >= 0)																	// Keep item indicies within allowed range
				.Where(itemIndex => itemIndex < itemCount)															// Keep item indicies within allowed range
				.Select(itemIndex => new
				{
					itemIndex,
					pageIndex = itemIndex / pagerContext.PageSize,													// Generate a page number from the item number
				})
				.Where(page => page.pageIndex >= pageIndex - halfPageSelectors)										// Ensure that if there a full amount of page 
				.Where(page => page.pageIndex <= pageIndex + halfPageSelectors || pageIndex < halfPageSelectors)	// selectors are always shown, even at the ends
				.Take(pagerContext.DisplayedPageSelectorCount);

			foreach(var page in directPageLinks)
				yield return new PageLink(
					display: (page.pageIndex + 1).ToString(),
					itemIndex: page.itemIndex,
					enabled: page.pageIndex != pageIndex,
					current: page.pageIndex == pageIndex);

			yield return new PageLink(
				display: "&rsaquo;",
				itemIndex: firstItemIndex / pagerContext.PageSize * pagerContext.PageSize + pagerContext.PageSize,
				enabled: lastItemIndex < itemCount - 1);

			yield return new PageLink(
				display: "&raquo;",
				itemIndex: itemCount / pagerContext.PageSize * pagerContext.PageSize,
				enabled: lastItemIndex < itemCount - 1);
		}
	}
}
