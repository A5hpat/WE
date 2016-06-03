// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class NoticeController : Controller
	{
		readonly NoticeProvider NoticeProvider;

		public NoticeController(NoticeProvider noticeProvider)
		{
			NoticeProvider = noticeProvider;
		}

		[ChildActionOnly]
		public ActionResult Index()
		{
			var notices = NoticeProvider.GetNotices();
			NoticeProvider.ClearNotices();

			return PartialView(ViewNames.IndexPartial, new NoticeViewModel(notices));
		}
	}
}
