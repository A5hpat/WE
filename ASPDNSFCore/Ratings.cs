// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for Ratings.
	/// </summary>
	public class Ratings
	{
		static readonly SkinProvider SkinProvider;

		static Ratings()
		{
			SkinProvider = new SkinProvider();
		}

		static UrlHelper Url
		{ get { return DependencyResolver.Current.GetService<UrlHelper>(); } }

		/// <summary>
		/// Gets the product rating.
		/// </summary>
		/// <param name="CustomerID">The CustomerID.</param>
		/// <param name="ProductID">The ProductID.</param>
		/// <returns>Returns the product rating.</returns>
		static public int GetProductRating(int CustomerID, int ProductID)
		{
			if(CustomerID == 0)
				return 0;

			int uname = 0;
			using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(IDataReader rs = DB.GetRS("Select rating from Rating   with (NOLOCK)  where CustomerID=" + CustomerID.ToString() + " and ProductID=" + ProductID.ToString(), dbconn))
				{
					if(rs.Read())
					{
						uname = DB.RSFieldInt(rs, "rating");
					}
				}
			}
			return uname;
		}

		/// <summary>
		/// Determine if the string has bad words.
		/// </summary>
		/// <param name="s">The string.</param>
		/// <returns>Returns TRUE if the string has bad words otherwise FALSE.</returns>
		static public bool StringHasBadWords(String s)
		{
			if(string.IsNullOrWhiteSpace(s))
			{
				return false;
			}

			String sql = "aspdnsf_CheckFilthy " + DB.SQuote(s) + "," + DB.SQuote(Thread.CurrentThread.CurrentUICulture.Name);

			bool hasBad = false;

			using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(IDataReader rs = DB.GetRS(sql, dbconn))
				{
					rs.Read();
					int IsFilthy = DB.RSFieldInt(rs, "IsFilthy");

					if(IsFilthy == 1)
						hasBad = true;

				}

			}

			return hasBad;
		}

		/// <summary>
		/// Displays the product rating
		/// </summary>
		/// <param name="ThisCustomer">Customer object</param>
		/// <param name="ProductID">Product ID of the product rating to display</param>
		/// <param name="CategoryID">Category ID of the product rating to display</param>
		/// <param name="SectionID">Section ID of the product rating to display</param>
		/// <param name="ManufacturerID">Manufacturer ID of the product rating to display</param>
		/// <param name="SkinID">skin id of the page</param>
		/// <param name="encloseInTab">set to true if not to be displayed in a tabUI</param>
		/// <returns>returns string html to be rendered</returns>
		static public String Display(Customer ThisCustomer, int ProductID, int CategoryID, int SectionID, int ManufacturerID, int SkinID, bool encloseInTab)
		{
			string productName = AppLogic.GetProductName(ProductID, ThisCustomer.LocaleSetting);
			StringBuilder tmpS = new StringBuilder(50000);

			if(!AppLogic.IsAdminSite)
			{
				tmpS.Append("<input type=\"hidden\" name=\"ProductID\" value=\"" + ProductID.ToString() + "\">");
				tmpS.Append("<input type=\"hidden\" name=\"CategoryID\" value=\"" + CategoryID.ToString() + "\">");
				tmpS.Append("<input type=\"hidden\" name=\"SectionID\" value=\"" + SectionID.ToString() + "\">");
				tmpS.Append("<input type=\"hidden\" name=\"ManufacturerID\" value=\"" + ManufacturerID.ToString() + "\">");
				if(!encloseInTab)
				{
					tmpS.Append("<input type=\"hidden\" name=\"productTabs\" value=\"2\">");
				}
			}

			if(encloseInTab)
			{
				tmpS.Append("<div class=\"group-header rating-header\">Customer Reviews</div>");
			}

			// RATINGS BODY:
			string sql = string.Format("aspdnsf_ProductStats {0}, {1}", ProductID, AppLogic.StoreID());
			int ratingsCount = 0;
			decimal ratingsAverage = 0;

			using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(IDataReader rs = DB.GetRS(sql, dbconn))
				{
					rs.Read();
					ratingsCount = DB.RSFieldInt(rs, "NumRatings");
					ratingsAverage = DB.RSFieldDecimal(rs, "AvgRating");
				}
			}

			int[] ratingPercentages = new int[6]; // indexes 0-5, but we only use indexes 1-5

			using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
			{
				string query = string.Format("select Productid, rating, count(rating) as N from Rating with (NOLOCK) where Productid = {0} and StoreID = {1} group by Productid,rating order by rating", ProductID, AppLogic.StoreID());
				dbconn.Open();
				using(IDataReader rs = DB.GetRS(query, dbconn))
				{
					while(rs.Read())
					{
						int NN = DB.RSFieldInt(rs, "N");
						Decimal pp = ((Decimal)NN) / ratingsCount;
						int pper = (int)(pp * 100.0M);
						ratingPercentages[DB.RSFieldInt(rs, "Rating")] = pper;
					}
				}

			}

			int orderIndex = 0;
			if("OrderBy".Equals(CommonLogic.FormCanBeDangerousContent("__EVENTTARGET"), StringComparison.InvariantCultureIgnoreCase))
			{
				orderIndex = CommonLogic.FormNativeInt("OrderBy");
			}
			if(orderIndex == 0)
			{
				orderIndex = 3;
			}

			int pageSize = AppLogic.AppConfigUSInt("RatingsPageSize");
			int pageNumber = CommonLogic.QueryStringUSInt("PageNum");
			if(pageNumber == 0)
			{
				pageNumber = 1;
			}
			if(pageSize == 0)
			{
				pageSize = 10;
			}
			if(CommonLogic.QueryStringCanBeDangerousContent("show") == "all")
			{
				pageSize = 1000000;
				pageNumber = 1;
			}

			using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(SqlCommand cmd = new SqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "aspdnsf_GetProductComments";
					cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@votingcustomer", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@pagesize", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@pagenum", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@sort", SqlDbType.TinyInt));
					cmd.Parameters.Add(new SqlParameter("@storeID", SqlDbType.Int));

					cmd.Parameters["@ProductID"].Value = ProductID;
					cmd.Parameters["@votingcustomer"].Value = ThisCustomer.CustomerID;
					cmd.Parameters["@pagesize"].Value = pageSize;
					cmd.Parameters["@pagenum"].Value = pageNumber;
					cmd.Parameters["@sort"].Value = orderIndex;
					cmd.Parameters["@storeID"].Value = AppLogic.StoreID();

					SqlDataReader dr = cmd.ExecuteReader();
					dr.Read();

					int rowsCount = Convert.ToInt32(dr["totalcomments"]);
					int pagesCount = Convert.ToInt32(dr["pages"]);
					dr.NextResult();

					if(pageNumber > pagesCount && pageNumber > 1 && rowsCount == 0)
					{
						dr.Close();

						var redirectUrl = Url.BuildProductLink(
							id: ProductID,
							additionalRouteValues: new Dictionary<string, object>
							{
								{ "pagenum", pageNumber - 1 }
							});

						HttpContext.Current.Response.Redirect(redirectUrl);
					}

					int StartRow = (pageSize * (pageNumber - 1)) + 1;
					int StopRow = CommonLogic.IIF((StartRow + pageSize - 1) > rowsCount, rowsCount, StartRow + pageSize - 1);

					if(ratingsCount > 0)
					{
						tmpS.AppendFormat("<span itemprop=\"aggregateRating\" itemscope itemtype=\"{0}://schema.org/AggregateRating\">{1}", HttpContext.Current.Request.Url.Scheme, Environment.NewLine);
						tmpS.AppendFormat("<meta itemprop=\"ratingValue\" content=\"{0}\"/>{1}", ratingsAverage, Environment.NewLine);
						tmpS.AppendFormat("<meta itemprop=\"reviewCount\" content=\"{0}\"/>{1}", ratingsCount, Environment.NewLine);
						tmpS.AppendFormat("<meta itemprop=\"bestRating\" content=\"5\"/>{0}", Environment.NewLine);
						tmpS.AppendFormat("<meta itemprop=\"worstRating\" content=\"1\"/>{0}", Environment.NewLine);
						tmpS.AppendFormat("</span>{0}", Environment.NewLine);
					}

					tmpS.Append("<div class=\"page-row total-rating-row\">");
					tmpS.Append("   <div class=\"rating-stars-wrap\">");
					tmpS.Append(BuildStarImages(ratingsAverage, SkinID) + "<span class=\"ratings-average-wrap\">(" + String.Format("{0:f}", ratingsAverage) + ")</span>");
					tmpS.Append("   </div>");
					tmpS.Append("   <div class=\"rating-count-wrap\">");
					tmpS.Append("       <span># of Ratings:</span> " + ratingsCount.ToString());
					tmpS.Append("   </div>");
					tmpS.Append("</div>");

					string rateScript = "javascript:RateIt(" + ProductID.ToString() + ");";

					int productRating = Ratings.GetProductRating(ThisCustomer.CustomerID, ProductID);

					tmpS.Append("<div class=\"page-row rating-link-row\">");
					if(productRating != 0)
					{
						tmpS.Append("<div class=\"rating-link-wrap\">");
						tmpS.Append("   <span>Your Rating: " + productRating.ToString() + "</span>");
						tmpS.Append("</div>");
						if(!AppLogic.IsAdminSite)
						{
							tmpS.Append("<div class=\"rating-link-wrap\">");
							tmpS.Append("   <a class=\"btn btn-default change-rating-button\" href=\"" + rateScript + "\">Click Here</a> ");
							tmpS.Append("	<span>to change your rating</span>");
							tmpS.Append("</div>");
						}
					}
					else
					{
						if((AppLogic.AppConfigBool("RatingsCanBeDoneByAnons") || ThisCustomer.IsRegistered) && !AppLogic.IsAdminSite)
						{
							tmpS.Append("<div class=\"rating-link-wrap\">");
							tmpS.Append("   <a class=\"btn btn-default add-rating-button\" href=\"" + rateScript + "\">Click Here</a> ");
							tmpS.Append("	<span>to rate this product</span>");
							tmpS.Append("</div>");
						}
						else
						{
							tmpS.Append("<div class=\"rating-link-wrap\">");
							tmpS.Append("   <span>(Only registered customers can rate)</span>");
							tmpS.Append("</div>");
						}
					}
					tmpS.Append("</div>");

					if(rowsCount > 0)
					{
						while(dr.Read())
						{
							tmpS.AppendFormat("<div class=\"page-row rating-comment-row\" itemprop=\"review\" itemscope itemtype=\"{0}://schema.org/Review\">{1}", HttpContext.Current.Request.Url.Scheme, Environment.NewLine);
							tmpS.AppendFormat("<meta itemprop=\"datePublished\" content=\"{0}\"/>{1}", Convert.ToDateTime(dr["CreatedOn"]).ToString("yyyy-MM-dd"), Environment.NewLine);
							tmpS.AppendFormat("<meta itemprop=\"itemReviewed\" content=\"{0}\"/>{1}", productName, Environment.NewLine);
							tmpS.Append("	<div class=\"rating-author-wrap\">\n");
							tmpS.Append("		<span class=\"rating-row-number\">" + dr["rownum"].ToString() + ". </span><span class=\"rating-row-author\" itemprop=\"author\">" + HttpContext.Current.Server.HtmlEncode(CommonLogic.IIF(dr["FirstName"].ToString().Length == 0, AppLogic.GetString("ratings.cs.14", SkinID, Thread.CurrentThread.CurrentUICulture.Name), dr["FirstName"].ToString())) + "</span> <span class=\"rating-row-said\">" + AppLogic.GetString("ratings.cs.15", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + " " + Localization.ToThreadCultureShortDateString(Convert.ToDateTime(dr["CreatedOn"])) + ", " + AppLogic.GetString("ratings.cs.16", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + " </span>");
							tmpS.Append("	</div>");
							tmpS.AppendFormat("<div class=\"rating-comment-stars\" itemprop=\"reviewRating\" itemscope itemtype=\"{0}://schema.org/Rating\">{1}", HttpContext.Current.Request.Url.Scheme, Environment.NewLine);
							tmpS.AppendFormat("<meta itemprop=\"bestRating\" content=\"5\"/>{0}", Environment.NewLine);
							tmpS.AppendFormat("<meta itemprop=\"worstRating\" content=\"1\"/>{0}", Environment.NewLine);
							tmpS.AppendFormat("<meta itemprop=\"ratingValue\" content=\"{0}\"/>{1}", Convert.ToDecimal(dr["Rating"]), Environment.NewLine);
							tmpS.Append(BuildStarImages(Convert.ToDecimal(dr["Rating"]), SkinID));
							tmpS.Append("	</div>");
							tmpS.Append("	<div class=\"rating-comments\" itemprop=\"reviewBody\">\n");
							tmpS.Append(HttpContext.Current.Server.HtmlEncode(dr["Comments"].ToString()));
							tmpS.Append("	</div>\n");
							tmpS.Append("</div>\n");
							tmpS.Append("<div class=\"form rating-comment-helpfulness-wrap\">");
							tmpS.Append("	<div class=\"form-group\">");
							if(ThisCustomer.CustomerID != Convert.ToInt32(dr["CustomerID"]))
							{
								if(!AppLogic.IsAdminSite)
								{
									tmpS.Append("Was this comment helpful? ");
									tmpS.Append("<input TYPE=\"RADIO\" NAME=\"helpful_" + ProductID.ToString() + "_" + dr["CustomerID"].ToString() + "\" onClick=\"return RateComment('" + ProductID.ToString() + "','" + ThisCustomer.CustomerID + "','Yes','" + dr["CustomerID"].ToString() + "');\" " + CommonLogic.IIF(Convert.ToInt16(dr["CommentHelpFul"]) == 1, " checked ", "") + ">\n");
									tmpS.Append("<span>yes</span> \n");
									tmpS.Append("<input TYPE=\"RADIO\" NAME=\"helpful_" + ProductID.ToString() + "_" + dr["CustomerID"].ToString() + "\" onClick=\"return RateComment('" + ProductID.ToString() + "','" + ThisCustomer.CustomerID + "','No','" + dr["CustomerID"].ToString() + "');\" " + CommonLogic.IIF(Convert.ToInt16(dr["CommentHelpFul"]) == 0, " checked ", "") + ">\n");
									tmpS.Append("<span>no/span> \n");
								}
								else
								{
									tmpS.Append("Was this comment helpful? ");
									tmpS.Append("<input TYPE=\"RADIO\" NAME=\"helpful_" + ProductID.ToString() + "_" + dr["CustomerID"].ToString() + "\" " + CommonLogic.IIF(Convert.ToInt16(dr["CommentHelpFul"]) == 1, " checked ", "") + ">\n");
									tmpS.Append("<span>yes</span>\n");
									tmpS.Append("<input TYPE=\"RADIO\" NAME=\"helpful_" + ProductID.ToString() + "_" + dr["CustomerID"].ToString() + "\" " + CommonLogic.IIF(Convert.ToInt16(dr["CommentHelpFul"]) == 0, " checked ", "") + ">\n");
									tmpS.Append("<span>no</span>\n");
								}
							}
							tmpS.Append("	</div>\n");
							tmpS.Append("	<div class=\"form-text rating-helpfulness-text\">");
							tmpS.Append("			(" + dr["FoundHelpful"].ToString() + " people found  " + CommonLogic.IIF(ThisCustomer.CustomerID != Convert.ToInt32(dr["CustomerID"]), "this", "your") + " comment helpful, " + dr["FoundNotHelpful"].ToString() + " did not)");
							tmpS.Append("	</div>\n");
							tmpS.Append("</div>\n");
						}
					}
					dr.Close();

					if(rowsCount > 0)
					{
						tmpS.Append("<div class=\"page-row comments-count-wrap\">");
						tmpS.Append(String.Format("Showing comments {0}-{1} of {2}", StartRow.ToString(), StopRow.ToString(), rowsCount.ToString()));
						if(pagesCount > 1)
						{
							tmpS.Append(" (");
							if(pageNumber > 1)
							{
								var url = Url.BuildProductLink(
									id: CommonLogic.QueryStringUSInt("ProductID"),
									additionalRouteValues: new Dictionary<string, object>
									{
										{ "OrderBy", orderIndex },
										{ "pagenum", pageNumber - 1 },
									});

								tmpS.AppendFormat(
									"<a href=\"{0}\">{1} {2}</a>",
									url,
                                    "Previous",
									pageSize);
							}
							if(pageNumber > 1 && pageNumber < pagesCount)
							{
								tmpS.Append(" | ");
							}
							if(pageNumber < pagesCount)
							{
								var url = Url.BuildProductLink(
									id: CommonLogic.QueryStringUSInt("ProductID"),
									additionalRouteValues: new Dictionary<string, object>
									{
										{ "OrderBy", orderIndex },
										{ "pagenum", pageNumber + 1 },
									});

								tmpS.AppendFormat(
									"<a href=\"{0}\">{1} {2}</a>",
									url,
                                    "Next",
									pageSize);
							}
							tmpS.Append(")");
						}
						tmpS.Append("</div>\n");
						tmpS.Append("<div class=\"page-row comments-pager-wrap\">");
						if(pagesCount > 1)
						{
							var url = Url.BuildProductLink(
								id: CommonLogic.QueryStringUSInt("ProductID"),
								additionalRouteValues: new Dictionary<string, object>
								{
									{ "show", "all" },
									{ "pagenum", pageNumber + 1 },
								});

							tmpS.AppendFormat(
								"<a href=\"{0}\">{1}</a> {2}",
								url,
                                "Click Here",
                                "to see all comments");
						}
						tmpS.Append("</div>\n");
					}

					// END RATINGS BODY:

					if(!AppLogic.IsAdminSite)
					{
						var rateCommentUrl = Url.Action(
							actionName: ActionNames.RateComment,
							controllerName: ControllerNames.Rating);

						var rateProductUrl = Url.Action(
							actionName: ActionNames.Index,
							controllerName: ControllerNames.Rating);

						tmpS.Append("<div id=\"RateCommentDiv\" name=\"RateCommentDiv\" style=\"position:absolute; left:0px; top:0px; visibility:" + AppLogic.AppConfig("RatingsCommentFrameVisibility") + "; z-index:2000; \">\n");
						tmpS.Append("<iframe name=\"RateCommentFrm\" id=\"RateCommentFrm\" width=\"400\" height=\"100\" hspace=\"0\" vspace=\"0\" marginheight=\"0\" marginwidth=\"0\" frameborder=\"0\" noresize scrolling=\"yes\" src=\"" + Url.Content("~/empty.htm") + "\"></iframe>\n");
						tmpS.Append("</div>\n");
						tmpS.Append("<script type=\"text/javascript\">\n");
						tmpS.Append("function RateComment(ProductID,MyCustomerID,MyVote,RatersCustomerID)\n");
						tmpS.Append("	{\n");
						tmpS.Append("	RateCommentFrm.location = '" + rateCommentUrl + "?Productid=' + ProductID + '&VotingCustomerID=' + MyCustomerID + '&Vote=' + MyVote + '&RatingCustomerID=' + RatersCustomerID\n");
						tmpS.Append("	}\n");
						tmpS.Append("</script>\n");

						tmpS.Append("<script type=\"text/javascript\">\n");
						tmpS.Append("	function RateIt(ProductID)\n");
						tmpS.Append("	{\n");
						tmpS.Append("		window.open('" + rateProductUrl + "?Productid=' + ProductID + '&refresh=no&returnurl=" + HttpContext.Current.Server.UrlEncode(CommonLogic.PageInvocation()) + "','ASPDNSF_ML" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','height=550,width=400,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no')\n");
						tmpS.Append("	}\n");
						tmpS.Append("</script>\n");
					}
				}
			}
			return tmpS.ToString();
		}

		static public string BuildStarImages(decimal rating, int skinId)
		{
			var starTypes = new[]
			{
				rating < 0.25M
					? StarType.Empty
					: rating >= 0.25M && rating < 0.75M
						? StarType.Half
						: StarType.Full,
				rating < 1.25M
					? StarType.Empty
					: rating >= 1.25M && rating < 1.75M
						? StarType.Half
						: StarType.Full,
				rating < 2.25M
					? StarType.Empty
					: rating >= 2.25M && rating < 2.75M
						? StarType.Half
						: StarType.Full,
				rating < 3.25M
					? StarType.Empty
					: rating >= 3.25M && rating < 3.75M
						? StarType.Half
						: StarType.Full,
				rating < 4.25M
					? StarType.Empty
					: rating >= 4.25M && rating < 4.75M
						? StarType.Half
						: StarType.Full,
			};

			var skinName = SkinProvider.GetSkinNameById(skinId);

			return string.Join("",
				starTypes.Select((star, idx) => string.Format(
					"<img class='ratings-star-{0}-{1}' src='{2}' />",
					idx,
					star,
					AppLogic.LocateImageURL(string.Format("~/Skins/{0}/images/{1}", skinName, StarImage(star))))));
		}

		static string StarImage(StarType starType)
		{
			switch(starType)
			{
				default:
				case StarType.Empty:
					return "stare.gif";
				case StarType.Half:
					return "starh.gif";
				case StarType.Full:
					return "starf.gif";
			}
		}

		enum StarType
		{
			Empty,
			Half,
			Full
		}
	}
}
