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
	public class PromotionsViewModel
	{
		[Display(Prompt = "checkout.promo.example")]
		[Required(ErrorMessage = "checkout.promo.required")]
		public string EnteredCode { get; set; }
		public readonly IEnumerable<PromotionViewModel> Promotions;

        public PromotionsViewModel()
        { }

		public PromotionsViewModel(string enteredPromoCode, IEnumerable<PromotionViewModel> promotions)
		{
			EnteredCode = enteredPromoCode;
			Promotions = promotions;
		}
	}

	public class PromotionViewModel
	{
		public readonly string Code;
		public readonly string Description;

		public PromotionViewModel(string code, string description)
		{
			Code = code;
			Description = description;
		}
	}
}
