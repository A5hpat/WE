// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;

namespace AspDotNetStorefront.Models
{
	public class ProductEmailViewModel : ProductEmailPostViewModel
	{
		public readonly string ProductImage;

		public ProductEmailViewModel(string productImage)
		{
			ProductImage = productImage;
		}
	}

	public class ProductEmailPostViewModel
	{
		public int ProductId
		{ get; set; }

		[Display(Name = "emailproduct.to.name", Prompt = "emailproduct.to.prompt")]
		[Required(ErrorMessage = "emailproduct.to.required")]
		[DataType(DataType.EmailAddress, ErrorMessage = "emailproduct.to.format")]
		[StringLength(100)]
		public string To
		{ get; set; }

		[Display(Name = "emailproduct.message.name", Prompt = "emailproduct.message.prompt")]
		[DataType(DataType.MultilineText)]
		public string Message
		{ get; set; }

		[Display(Name = "emailproduct.from.name", Prompt = "emailproduct.from.prompt")]
		[Required(ErrorMessage = "emailproduct.from.required")]
		[DataType(DataType.EmailAddress, ErrorMessage = "emailproduct.from.format")]
		[StringLength(100)]
		public string From
		{ get; set; }
	}
}
