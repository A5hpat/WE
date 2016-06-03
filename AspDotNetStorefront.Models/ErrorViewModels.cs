// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace AspDotNetStorefront.Models
{
	public class NotFoundViewModel
	{
		public readonly string Title;
		public readonly string Content;
		public readonly IEnumerable<NotFoundSuggestionViewModel> Suggestions;

		public NotFoundViewModel(string title, string content, IEnumerable<NotFoundSuggestionViewModel> suggestions = null)
		{
			Title = title;
			Content = content;
			Suggestions = suggestions;
		}
	}

	public class NotFoundSuggestionViewModel
	{
		public string Name;
		public string Url;
		public string Description;

		public NotFoundSuggestionViewModel(string name, string url, string description)
		{
			Name = name;
			Url = url;
			Description = description;
		}
	}

	public class InvalidRequestViewModel
	{
		public readonly string ErrorCode;
		public readonly bool UserIsAdmin;

		public InvalidRequestViewModel(string errorCode, bool userIsAdmin)
		{
			ErrorCode = errorCode;
			UserIsAdmin = userIsAdmin;
		}
	}
}
