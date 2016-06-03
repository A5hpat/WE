// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Caching.ObjectCaching;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefront.Routing;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
	[SecureAccessFilter(forceHttps: true)]
	public class CheckoutGiftCardSetupController : Controller
	{
		readonly ICachedShoppingCartProvider CachedShoppingCartProvider;

		public CheckoutGiftCardSetupController(ICachedShoppingCartProvider cachedShoppingCartProvider)
		{
			CachedShoppingCartProvider = cachedShoppingCartProvider;
		}

		[PageTypeFilter(PageTypes.Checkout)]
		public ActionResult GiftCardSetup()
		{
			var customer = HttpContext.GetCustomer();
			var cart = CachedShoppingCartProvider.Get(customer, CartTypeEnum.ShoppingCart, AppLogic.StoreID());

			var model = new EmailGiftCardsViewModel
			{
				EmailGiftCardsInCart = LoadEmailGiftCardsFromCart(customer, cart)
			};

			return View(model);
		}

		[HttpPost]
		public ActionResult GiftCardSetup(EmailGiftCardsViewModel model)
		{
			if(!ModelState.IsValid)
				return View(model);
			
			foreach(var emailGiftCard in model.EmailGiftCardsInCart)
				GiftCard.UpdateCard(GiftCardID: emailGiftCard.GiftCardId, 
					SerialNumber: null, 
					OrderNumber: null, 
					InitialAmount: null,
					Balance: null, 
					DisabledByAdministrator: null, 
					ExpirationDate: null, 
					EMailName: emailGiftCard.RecipientName, 
					EMailTo: emailGiftCard.RecipientEmail, 
					EMailMessage: emailGiftCard.RecipientMessage, 
					ValidForCustomers: null, 
					ValidForProducts: null, 
					ValidForManufacturers: null, 
					ValidForCategories: null, 
					ValidForSections: null, 
					ExtensionData: null);

			return RedirectToAction(ActionNames.Index, ControllerNames.Checkout);
		}
		
		List<EmailGiftCardViewModel> LoadEmailGiftCardsFromCart(Customer customer, ShoppingCart cart)
		{
			var emailGiftCards = new List<EmailGiftCardViewModel>();
			var emailGiftCardProductTypes = AppLogic.AppConfig("GiftCard.EmailProductTypeIDs").Trim().Split(',')
				.Select(int.Parse)
				.ToList();

			var emailGiftCardRecIds = string.Join(",", cart.CartItems
				.Where(ci => emailGiftCardProductTypes.Contains(ci.ProductTypeId))
				.Select(cartItem => cartItem.ShoppingCartRecordID));

			var query = @"SELECT p.Name AS ProductName, g.GiftCardID, g.InitialAmount, g.EMailName, g.EMailTo, g.EMailMessage 
                FROM GiftCard g 
                LEFT JOIN Product p ON g.ProductID = p.ProductID 
                WHERE g.ShoppingCartRecID IN ({0})";
			//can't parameterize emailGiftCardRecIds as list of ints
			query = string.Format(query, emailGiftCardRecIds);

			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(IDataReader rs = DB.GetRS(query, dbconn))
				{
					while(rs.Read())
					{
						emailGiftCards.Add(new EmailGiftCardViewModel
						{
							GiftCardId = rs.FieldInt("GiftCardID"),
							ProductName = rs.FieldByLocale("ProductName", customer.LocaleSetting),
							Amount = Localization.CurrencyStringForDisplayWithExchangeRate(rs.FieldDecimal("InitialAmount"), customer.CurrencySetting),
							RecipientName = rs.Field("EMailName"),
							RecipientEmail = rs.Field("EMailTo"),
							RecipientMessage = rs.Field("EMailMessage")
						});
					}
				}
			}

			return emailGiftCards;
		}
	}
}
