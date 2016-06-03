// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
namespace AspDotNetStorefront.Models
{
	public class ThreeDSecureViewModel
	{
		public string ACSUrl { get; set; }
		
		public string PaReq { get; set; }

		public string MD { get; set; }	//This is short for 'MerchantData' - SagePayUK wants it as 'MD' in the hidden form field, so calling it that.

		public string TermUrl { get; set; }
	}

	public class ThreeDSecureFrameViewModel
	{
		public string FrameUrl { get; set; }
	}
}
