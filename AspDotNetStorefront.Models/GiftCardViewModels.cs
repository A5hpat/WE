// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspDotNetStorefront.Models
{
	public class GiftCardViewModel
	{
		[Display(Prompt = "checkout.giftcard.example")]
		[Required(ErrorMessage = "checkout.giftcard.required")]
		public string Code { get; set; }
	}

	public class EmailGiftCardsViewModel
	{
		public IEnumerable<EmailGiftCardViewModel> EmailGiftCardsInCart { get; set; }
	}

	public class EmailGiftCardViewModel
	{
		public int GiftCardId { get; set; }

		public string ProductName { get; set; }

		public string Amount { get; set; }

		[Display(Name = "giftcard.recipientname.label", Prompt = "giftcard.recipientname.example")]
		[Required(ErrorMessage = "giftcard.recipientname.required")]
		public string RecipientName { get; set; }

		[Display(Name = "giftcard.recipientemail.label", Prompt = "giftcard.recipientemail.example")]
		[Required(ErrorMessage = "giftcard.recipientemail.required")]
		public string RecipientEmail { get; set; }
		
		[Display(Name = "giftcard.recipientmessage.label", Prompt = "giftcard.recipientmessage.example")]
		[DataType(DataType.MultilineText)]
		public string RecipientMessage { get; set; }
	}
}
