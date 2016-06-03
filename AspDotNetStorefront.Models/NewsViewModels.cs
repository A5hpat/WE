// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace AspDotNetStorefront.Models
{
	public class NewsIndexRenderModel
	{
		public readonly IEnumerable<NewsDetailRenderModel> Articles;
		public readonly bool ShowArticleBody;
		public readonly string Teaser;

		public NewsIndexRenderModel(IEnumerable<NewsDetailRenderModel> articles, bool showArticleBody, string teaser)
		{
			Articles = articles;
			ShowArticleBody = showArticleBody;
			Teaser = teaser;
		}
	}

	public class NewsListRenderModel
	{
		public readonly IEnumerable<NewsDetailRenderModel> Articles;

		public NewsListRenderModel(IEnumerable<NewsDetailRenderModel> articles)
		{
			Articles = articles;
		}
	}

	public class NewsDetailRenderModel
	{
		public readonly int Id;
		public readonly string Headline;
		public readonly string Copy;
		public readonly string Date;

		public NewsDetailRenderModel(int id, string headline, string copy, string date)
		{
			Id = id;
			Headline = headline;
			Copy = copy;
			Date = date;
		}
	}
}
