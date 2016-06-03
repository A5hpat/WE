// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace AspDotNetStorefront.Models
{
	public class KitUploadPostModel
	{
		public int ProductId
		{ get; set; }

		public int GroupId
		{ get; set; }

		public int ItemId
		{ get; set; }

		[Required(ErrorMessage = "KitProduct.UploadImageRequired")]
		[Display(Name = "KitProduct.ChooseFile")]
		public HttpPostedFileBase FileUpload
		{ get; set; }

		public string TemporaryFileStub
		{ get; set; }
	}

	public class KitUploadViewModel : KitUploadPostModel
	{
		public KitItemViewModel Item
		{ get; set; }

		public bool UploadSuccessful
		{ get; set; }

		public string ImageUrl
		{ get; set; }
	}
}
