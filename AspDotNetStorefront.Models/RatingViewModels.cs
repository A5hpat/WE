// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;

namespace AspDotNetStorefront.Models
{
	public class RatingViewModel : RatingPostModel
	{
		public readonly bool HasBadWords;
		public readonly bool SubmittedSuccessfully;
		public readonly System.Web.Mvc.SelectList RatingOptions;
		
		public RatingViewModel(
			int productId, 
			string productName, 
			string comment, 
			bool hasBadWords, 
			bool editing, 
			bool submittedSuccessfully, 
			int rating, 
			System.Web.Mvc.SelectList ratingOptions)
		{
			ProductId = productId;
			ProductName = productName;
			Comment = comment;
			HasBadWords = hasBadWords;
			Editing = editing;
			SubmittedSuccessfully = submittedSuccessfully;
			Rating = rating;
			RatingOptions = ratingOptions;
		}
	}

	public class RatingPostModel
	{
		public int ProductId
		{ get; set; }

		public string ProductName
		{ get; set; }

		[Range(1, 5, ErrorMessage = "rateit.aspx.1")]
        public int Rating
		{ get; set; }

		[DataType(DataType.MultilineText)]
		[StringLength(5000, ErrorMessage = "rateit.aspx.2")]
		public string Comment
		{ get; set; }

		public bool Editing
		{ get; set; }
	}
}
