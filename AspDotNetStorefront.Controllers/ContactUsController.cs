// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.Mvc;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class ContactUsController : Controller
	{
		readonly CaptchaStorageService CaptchaStorageService;
		readonly NoticeProvider NoticeProvider;

		public ContactUsController(CaptchaStorageService captchaStorageService, NoticeProvider noticeProvider)
		{
			CaptchaStorageService = captchaStorageService;
			NoticeProvider = noticeProvider;
		}

		[HttpGet]
		[ImportModelStateFromTempData]
		public ActionResult Index()
		{
			ViewBag.MetaTitle = string.Format(
				"{0} - {1}",
				AppLogic.AppConfig("StoreName"),
                "Contact Us");

			var model = new ContactUsRenderModel(
				pageHeader: "Use the form below to send us a message",
				useCaptcha: AppLogic.AppConfigBool("ContactUs.UseCaptcha"));

			return View(model);
		}

		[HttpPost]
		[ExportModelStateToTempData]
		public ActionResult Index(ContactUsViewModel model)
		{
			if(!IsCaptchaValid(model.CaptchaCode))
			{
				CaptchaStorageService.ClearSecurityCode(HttpContext);
				ModelState.AddModelError("CaptchaCode", "The letters you entered did not match, please try again.");
			}

			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index);

			AppLogic.SendMail(subject: model.Subject,
				body: GetContactTopic(model),
				useHtml: true,
				fromAddress: AppLogic.AppConfig("GotOrderEMailFrom"),
				fromName: AppLogic.AppConfig("GotOrderEMailFromName"),
				toAddress: AppLogic.AppConfig("GotOrderEMailTo"),
				toName: AppLogic.AppConfig("GotOrderEMailTo"),
				bccAddresses: string.Empty,
				server: AppLogic.MailServer());

			// Clear the captcha so additional requests use a different security code.
			CaptchaStorageService.ClearSecurityCode(HttpContext);

			return RedirectToAction(ActionNames.Detail, ControllerNames.Topic, new { name = "ContactUsSuccessful" });
		}

		bool IsCaptchaValid(string enteredCaptchaCode)
		{
			if(!AppLogic.AppConfigBool("ContactUs.UseCaptcha"))
				return true;

			var securityCode = CaptchaStorageService.RetrieveSecurityCode(ControllerContext.HttpContext, string.Concat(ControllerNames.ContactUs, ActionNames.Index));
			if(string.IsNullOrEmpty(securityCode))
				return false;

			var comparisonType = AppLogic.AppConfigBool("Captcha.CaseSensitive")
				? StringComparison.Ordinal
				: StringComparison.OrdinalIgnoreCase;

			return securityCode.Equals(enteredCaptchaCode, comparisonType);
		}

		string GetContactTopic(ContactUsViewModel model)
		{
			return new Topic("ContactEmail")
				.ContentsRAW
				.Replace("%NAME%", model.From)
				.Replace("%EMAIL%", model.Email)
				.Replace("%PHONE%", model.Phone)
				.Replace("%SUBJECT%", model.Subject)
				.Replace("%MESSAGE%", model.Message);
		}
	}
}
