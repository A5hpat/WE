// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class RatingController : Controller
	{
		readonly NoticeProvider NoticeProvider;

		public RatingController(NoticeProvider noticeProvider)
		{
			NoticeProvider = noticeProvider;
		}

		[HttpGet]
		public ActionResult Index(int productId)
		{
			if(productId == 0)
			{
				NoticeProvider.PushNotice("Could not find the product to rate", NoticeType.Info);
				return RedirectToAction(ActionNames.Index, ControllerNames.Home);   //Ended up here somehow by mistake - have to send them somewhere
			}

			return View(BuildRatingViewModel(productId, false));
		}

		[HttpPost]
		[RequireCustomerRecordFilter]
		public ActionResult RateProduct(RatingPostModel model)
		{
			if(!ModelState.IsValid)
				return RedirectToAction(ActionNames.Index, new { @productId = model.ProductId });

			var customer = HttpContext.GetCustomer();

			if(!customer.IsRegistered && !AppLogic.AppConfigBool("RatingsCanBeDoneByAnons"))
				return RedirectToAction(ActionNames.Index, new { @productId = model.ProductId });

			var sqlParams = new SqlParameter[]
				{
					new SqlParameter("@isFilthy", Ratings.StringHasBadWords(model.Comment)),
					new SqlParameter("@rating", model.Rating),
					new SqlParameter("@hasComment", !string.IsNullOrEmpty(model.Comment)),
					new SqlParameter("@comments", model.Comment ?? string.Empty),
					new SqlParameter("@productId", model.ProductId),
					new SqlParameter("@customerId", customer.CustomerID),
					new SqlParameter("@storeId", AppLogic.StoreID()),
				};

			if(model.Editing)
			{
				var editSql = @"UPDATE Rating 
								SET IsFilthy = @isFilthy, 
									Rating = @rating, 
									HasComment = @hasComment, 
									Comments = @comments 
								WHERE ProductID = @productId 
									AND CustomerID = @customerId 
									AND StoreID = @storeId";

				DB.ExecuteSQL(editSql, sqlParams);
			}
			else
			{
				var insertSql = @"INSERT INTO Rating (ProductID, IsFilthy, CustomerID, CreatedOn, Rating, HasComment, StoreID, Comments)
									VALUES(@productId, @isFilthy, @customerId, GETDATE(), @rating, @hasComment, @storeId, @comments)";

				DB.ExecuteSQL(insertSql, sqlParams);
			}

			return View(ActionNames.Index, BuildRatingViewModel(model.ProductId, true));
		}

		[HttpGet]
		public ActionResult RateComment(int productId, int votingCustomerId, string vote, int ratingCustomerId)
		{
			var votedSql = @"SELECT Helpful 
							FROM RatingCommentHelpfulness WITH (NOLOCK) 
							WHERE ProductID = @ProductId 
								AND RatingCustomerID = @RatingCustomerId 
								AND VotingCustomerID = @VotingCustomerId 
								AND StoreID = @StoreId";

			var paramsDictionary = new Dictionary<string, object>
			{
				{ "VotingCustomerId", votingCustomerId },
				{ "ProductId", productId },
				{ "RatingCustomerId", ratingCustomerId },
				{ "StoreId", AppLogic.StoreID() },
				{ "Helpful", vote.ToLowerInvariant() == "yes" ? 1 : 0 },
			}.Select(kvp => new SqlParameter(kvp.Key, kvp.Value));

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();
				using(var reader = DB.GetRS(votedSql, connection, paramsDictionary.ToArray()))
				{
					if(reader.Read())   //They've voted on this comment before and changed their mind
					{
						//Nuke the old vote
						DB.ExecuteSQL(@"DELETE FROM RatingCommentHelpfulness 
									WHERE ProductID = @Productid 
										AND RatingCustomerID = @RatingCustomerId 
										AND VotingCustomerID = @VotingCustomerId 
										AND StoreID = @StoreId",
										paramsDictionary.ToArray());

						//Adjust some totals
						if(DB.RSFieldBool(reader, "Helpful"))
							DB.ExecuteSQL(@"UPDATE Rating 
										SET FoundHelpful = FoundHelpful - 1 
										WHERE ProductID = @ProductId 
											AND CustomerID = @RatingCustomerId 
											AND StoreID = @StoreId",
											paramsDictionary.ToArray());
						else
							DB.ExecuteSQL(@"UPDATE Rating 
										SET FoundNotHelpful = FoundNotHelpful - 1 
										WHERE ProductID = @ProductId 
											AND CustomerID = @RatingCustomerId 
											AND StoreID = @StoreId",
											paramsDictionary.ToArray());
					}
				}
			}

			//Now add the new vote
			DB.ExecuteSQL(@"INSERT INTO RatingCommentHelpfulness (StoreID, ProductID, RatingCustomerID, VotingCustomerID, Helpful) 
							VALUES(@StoreId, @ProductId, @RatingCustomerId, @VotingCustomerId, @Helpful)",
							paramsDictionary.ToArray());

			if(vote.ToLowerInvariant() == "yes")
			{
				DB.ExecuteSQL(@"UPDATE Rating 
								SET FoundHelpful = FoundHelpful + 1 
								WHERE ProductID = @ProductId 
									AND CustomerID = @RatingCustomerId 
									AND StoreID = @StoreId",
								paramsDictionary.ToArray());
			}
			else
			{
				DB.ExecuteSQL(@"UPDATE Rating 
								SET FoundNotHelpful = FoundNotHelpful + 1 
								WHERE ProductID = @ProductId
									AND CustomerID = @RatingCustomerId 
									AND StoreID = @StoreId",
								paramsDictionary.ToArray());
			}

			//We don't give any feedback that this worked
			return Content(string.Empty);
		}

		RatingViewModel BuildRatingViewModel(int productId, bool submittedSuccessfully)
		{
			var customer = HttpContext.GetCustomer();
			var product = new Product(productId);
			var rating = 0;
			var comment = string.Empty;
			var editing = false;

			//Load up previous info if they've rated this before
			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				var sql = "SELECT Rating, Comments FROM Rating WHERE CustomerID = @CustomerId AND ProductID = @ProductId AND StoreID = @StoreId";

				var parameters = new[] {
					new SqlParameter("CustomerId", customer.CustomerID),
					new SqlParameter("ProductId", productId),
					new SqlParameter("StoreId", AppLogic.StoreID())
				};

				connection.Open();

				using(var reader = DB.GetRS(sql, parameters, connection))
				{
					if(reader.Read())
					{
						editing = true;
						rating = DB.RSFieldInt(reader, "Rating");
						comment = DB.RSField(reader, "Comments");
					}
				}
			}

			return new RatingViewModel(productId: productId,
				productName: product.LocaleName,
				comment: comment,
				hasBadWords: Ratings.StringHasBadWords(comment),
				editing: editing,
				submittedSuccessfully: submittedSuccessfully,
				rating: rating,
				ratingOptions: new SelectList(
					items: new List<SelectListItem>
						{
							new SelectListItem { Text = "Select Rating", Value = "0", Selected = rating == 0 },
							new SelectListItem { Text = "1-Terrible!", Value = "1", Selected = rating == 1 },
							new SelectListItem { Text = "2-Bad", Value = "2", Selected = rating == 2 },
							new SelectListItem { Text = "3-OK", Value = "3", Selected = rating == 3 },
							new SelectListItem { Text = "4-Good", Value = "4", Selected = rating == 4 },
							new SelectListItem { Text = "5-Great!", Value = "5", Selected = rating == 5 },
						},
					dataValueField: "Value",
					dataTextField: "Text"));
		}
	}
}
