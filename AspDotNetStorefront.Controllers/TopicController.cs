// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class TopicController : Controller
	{
		NoticeProvider NoticeProvider;

		public TopicController(NoticeProvider noticeProvider)
		{
			NoticeProvider = noticeProvider;
		}

		[PageTypeFilter(PageTypes.Topic)]
		public ActionResult Detail(string name, bool? disableTemplate = null)
		{
			if(string.IsNullOrEmpty(name))
				throw new HttpException(404, null);

			var topic = LoadTopic(name);
			if(topic == null)
				throw new HttpException(404, null);

			if(topic.RequiresDisclaimer && string.IsNullOrEmpty(CommonLogic.CookieCanBeDangerousContent("SiteDisclaimerAccepted", true)))
				return RedirectToAction(ActionNames.Index, ControllerNames.SiteDisclaimer, new { returnUrl = Url.BuildTopicLink(name) });

			var customer = HttpContext.GetCustomer();
			var enteredPassword = customer.ThisCustomerSession.Session(string.Format("Topic{0}", name));

			if(!string.IsNullOrEmpty(topic.Password)
				&& (string.IsNullOrEmpty(enteredPassword)
					|| Security.UnmungeString(enteredPassword) != topic.Password))
				return View(ViewNames.Password, new TopicPasswordViewModel { Name = name });

			var viewName = (disableTemplate ?? false)
				? "DetailNoTemplate"
				: "Detail";

			return View(viewName, BuildViewModel(topic));
		}

		[HttpPost]
		[RequireCustomerRecordFilter]
		public ActionResult Detail(TopicPasswordViewModel model)
		{
			if(!ModelState.IsValid)
				return View(ViewNames.Password, new TopicPasswordViewModel { Name = model.Name });

			var customer = HttpContext.GetCustomer();

			var topic = LoadTopic(model.Name);
			if(topic == null)
				throw new HttpException(404, null);

			if(model.EnteredPassword == topic.Password)
			{
				customer.ThisCustomerSession["Topic" + model.Name] = Security.MungeString(model.EnteredPassword);
				return RedirectToAction(ActionNames.Detail);
			}
			else
			{
				NoticeProvider.PushNotice("Password is incorrect", NoticeType.Failure);

				return View(ViewNames.Password, model);
			}
		}

		TopicViewModel BuildViewModel(Topic topic)
		{
			return new TopicViewModel
			{
				Name = topic.TopicName,
				MetaTitle = topic.SETitle,
				MetaDescription = topic.SEDescription,
				MetaKeywords = topic.SEKeywords,
				PageTitle = topic.SectionTitle,
				PageContent = topic.Contents,
				ReadFromLocation = topic.FromDB
					? "database"
					: "file"
			};
		}

		Topic LoadTopic(string name)
		{
			var customer = ControllerContext.HttpContext.GetCustomer();
			var topic = new Topic(name, customer.LocaleSetting, customer.SkinID, new Parser(), AppLogic.StoreID());

			// Ensure that the topic actually exists
			if(topic == null || (topic.FromDB == false && topic.Contents.Length == 0))
				return null;

			return topic;
		}
	}
}
