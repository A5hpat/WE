// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class NewsController : Controller
	{
		[HttpGet]
		[PageTypeFilter(PageTypes.News)]
		public ActionResult Index()
		{
			var newsTeaser = AppLogic.AppConfig("NewsTeaser");
			var numberOfNewsArticlesToShow = AppLogic.AppConfigNativeInt("NumberOfNewsArticlesToShow");

			var model = new NewsIndexRenderModel(
				articles: GetNewsArticles(articleId: null)
					.Take(numberOfNewsArticlesToShow)
					.ToArray(),
				showArticleBody: AppLogic.AppConfigBool("ShowFullNewsArticle"),
				teaser: newsTeaser);

			return View(model);
		}

		[HttpGet]
		public ActionResult Detail(int id)
		{
			var article = GetNewsArticles(id)
				.FirstOrDefault();

			if(article == null)
				return RedirectToAction(ActionNames.Index);

			return View(article);
		}

		[HttpGet]
		public ActionResult List(int numberToDisplay = 3)
		{
			var articles = GetNewsArticles(articleId: null)
					.Take(numberToDisplay)
					.ToArray();

			if(!articles.Any())
				return Content(string.Empty);

			var model = new NewsListRenderModel(
				articles: articles);

			return PartialView(ViewNames.NewsListPartial, model);
		}

		IEnumerable<NewsDetailRenderModel> GetNewsArticles(int? articleId)
		{
			var customer = HttpContext.GetCustomer();

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				command.CommandText = @"
					select News.NewsId, News.Headline, News.NewsCopy, News.CreatedOn
					from News with(nolock)
					where 
						News.ExpiresOn > getdate()
						and News.Deleted = 0
						and News.Published = 1
						and (
							@articleId is null 
							or News.NewsId = @articleId)
						and (
							@allowNewsFiltering = 0
							or exists (select * from NewsStore where NewsStore.NewsID = News.NewsID and NewsStore.StoreID = @storeId))
					order by News.CreatedOn desc";
				command.Parameters.AddWithValue("allowNewsFiltering", AppLogic.GlobalConfigBool("AllowNewsFiltering"));
				command.Parameters.AddWithValue("storeId", AppLogic.StoreID());
				command.Parameters.AddWithValue("articleId", (object)articleId ?? DBNull.Value);

				connection.Open();
				using(var reader = command.ExecuteReader())
					while(reader.Read())
						yield return new NewsDetailRenderModel(
							id: reader.FieldInt("NewsId"),
							headline: reader.FieldByLocale("Headline", customer.LocaleSetting),
							copy: reader.FieldByLocale("NewsCopy", customer.LocaleSetting),
							date: Localization.ToThreadCultureShortDateString(reader.FieldDateTime("CreatedOn")));
			}
		}
	}
}
