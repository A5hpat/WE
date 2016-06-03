// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AspDotNetStorefrontCore
{
	public class NoticeProvider
	{
		readonly INoticeStorageProvider NoticeStorageProvider;

		public NoticeProvider(INoticeStorageProvider noticeStorageProvider)
		{
			NoticeStorageProvider = noticeStorageProvider;
		}

		public void PushNotice(Notice notice)
		{
			var notices = NoticeStorageProvider.LoadNotices();
			NoticeStorageProvider.SaveNotices(notices
				.Concat(new[] {
					notice
				}));
		}

		public void PushNotice(string message, NoticeType type)
		{
			var notices = NoticeStorageProvider.LoadNotices();
			NoticeStorageProvider.SaveNotices(notices
				.Concat(new[] { 
					new Notice(
						message: message, 
						type: type) 
				}));
		}

		public IEnumerable<Notice> GetNotices()
		{
			return NoticeStorageProvider.LoadNotices();
		}

		public void ClearNotices()
		{
			NoticeStorageProvider.SaveNotices(Enumerable.Empty<Notice>());
		}
	}

	public interface INoticeStorageProvider
	{
		IEnumerable<Notice> LoadNotices();
		void SaveNotices(IEnumerable<Notice> notices);
	}

	public class CookieNoticeStorageProvider : INoticeStorageProvider
	{
		const string CookieName = "adnsf.notices";

		readonly HttpContextBase HttpContext;

		public CookieNoticeStorageProvider(HttpContextBase httpContext)
		{
			HttpContext = httpContext;
		}

		public IEnumerable<Notice> LoadNotices()
		{
			if(HttpContext.Response.Cookies.AllKeys.Contains(CookieName))
				return JsonConvert.DeserializeObject<Notice[]>(HttpContext.Response.Cookies[CookieName].Value);
			else if(HttpContext.Request.Cookies.AllKeys.Contains(CookieName))
				return JsonConvert.DeserializeObject<Notice[]>(HttpContext.Request.Cookies[CookieName].Value);
			else
				return Enumerable.Empty<Notice>();
		}

		public void SaveNotices(IEnumerable<Notice> notices)
		{
			var cookie = new HttpCookie(
				name: CookieName,
				value: JsonConvert.SerializeObject(notices));

			if(!HttpContext.Response.Cookies.AllKeys.Contains(CookieName))
				HttpContext.Response.Cookies.Add(cookie);
			else
				HttpContext.Response.Cookies.Set(cookie);
		}
	}

	public class Notice
	{
		public readonly string Message;
		public readonly NoticeType Type;

		public Notice(string message, NoticeType type)
		{
			Message = message;
			Type = type;
		}
	}

	public enum NoticeType
	{
		Info,
		Success,
		Warning,
		Failure,
	}

	public enum AjaxNoticeType
	{
		info,
		failure,
		success
	}

	public class AjaxNotice
	{
		public readonly string Message;
		public readonly AjaxNoticeType Type;

		public AjaxNotice(string message, AjaxNoticeType type)
		{
			Message = message;
			Type = type;
		}
	}
}
