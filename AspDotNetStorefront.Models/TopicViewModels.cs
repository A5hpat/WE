// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;

namespace AspDotNetStorefront.Models
{
	public class TopicViewModel
	{
		public string Name { get; set; }
		public string MetaTitle { get; set; }
		public string MetaDescription { get; set; }
		public string MetaKeywords { get; set; }
		public string PageTitle { get; set; }
		public string PageContent { get; set; }
		public string ReadFromLocation { get; set; }
	}

	public class TopicPasswordViewModel
	{
		public string Name { get; set; }
		[Display(Name = "driver.aspx.2")]
		[Required(ErrorMessage = "driver.aspx.1")]
		public string EnteredPassword { get; set; }
	}
}
