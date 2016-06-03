// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	public class CountryController : Controller
	{
		[HttpGet]
		public ActionResult States(string countryName)
		{
			return Json(
				data: State
					.GetAllStatesForCountry(AppLogic.GetCountryID(countryName))
					.Select(state => new
					{
						name = state.Name,
						abbreviation = state.Abbreviation
					})
					.ToArray(),
				behavior: JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult PostalCodeRegexMap()
		{
			return Json(Country
				.GetAll()
				.ToDictionary(
					country => country.Name,
					country => country.PostalCodeRegex),
				behavior: JsonRequestBehavior.AllowGet);
		}
	}
}
