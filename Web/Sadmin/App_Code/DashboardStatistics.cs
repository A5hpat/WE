// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using AspDotNetStorefrontCore;
using Newtonsoft.Json;

namespace AspDotNetStorefront
{
	public class DashboardStatistics : IHttpHandler
	{
		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.Clear();
			context.Response.ContentType = "application/json";

			var length = CommonLogic.QueryStringCanBeDangerousContent("length");
			if(string.IsNullOrEmpty(length))
				length = "month";

			var statisticType = CommonLogic.QueryStringCanBeDangerousContent("type");
			if(statisticType.EqualsIgnoreCase("orders"))
				WriteOrderData(context.Response, length);
			else if(statisticType.EqualsIgnoreCase("products"))
				WriteProductData(context.Response, length);

			context.ApplicationInstance.CompleteRequest();
		}

		void WriteOrderData(HttpResponse response, string length)
		{
			response.Write(@"{ ""data"": [");

			var endDate = DateTime.Now;
			DateTime startDate;
			var interval = 12;
			var dateFormat = "MMM d";
			switch(length)
			{
				case "day":
					startDate = endDate.AddDays(-1);
					interval = 24;
					dateFormat = "h tt";
					break;
				case "year":
					startDate = endDate.AddYears(-1);
					interval = 12;
					dateFormat = "MMM y";
					break;
				default:
					startDate = endDate.AddMonths(-1);
					interval = 30;
					break;
			}

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				using(var command = connection.CreateCommand())
				{
					command.CommandText = "aspdnsf_SalesForChart";
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate });
					command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });
					command.Parameters.Add(new SqlParameter("@NumIntervals", SqlDbType.Int) { Value = interval });

					connection.Open();
					using(var reader = command.ExecuteReader())
					{
						var isFirst = true;

						while(reader.Read())
						{
							if(!isFirst)
								response.Write(",");
							else
								isFirst = false;

							var date = DB.RSFieldDateTime(reader, "EndDate");
							var sales = DB.RSFieldDecimal(reader, "Sales");
							response.Write(string.Format(@"[ ""{0}"" , {1}]", date.ToString(dateFormat), JsonConvert.ToString(sales)));
						}
					}
				}
			}
			response.Write("]}");

		}

		void WriteProductData(HttpResponse response, string length)
		{
			response.Write(@"{ ""data"": [");

			var endDate = DateTime.Now;
			DateTime startDate;
			switch(length)
			{
				case "day":
					startDate = endDate.AddDays(-1);
					break;
				case "year":
					startDate = endDate.AddYears(-1);
					break;
				default:
					startDate = endDate.AddMonths(-1);
					break;
			}

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				using(var command = connection.CreateCommand())
				{
					command.CommandText = "aspdnsf_TopProducts";
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate });
					command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });
					command.Parameters.Add(new SqlParameter("@CountOfProducts", SqlDbType.Int) { Value = 10 });

					connection.Open();
					using(var reader = command.ExecuteReader())
					{
						var isFirst = true;

						while(reader.Read())
						{
							if(!isFirst)
								response.Write(",");
							else
								isFirst = false;

							var productName = DB.RSFieldByLocale(reader, "ProductName", Localization.GetDefaultLocale());
							var sales = DB.RSFieldDecimal(reader, "Sales");
							var productId = DB.RSFieldInt(reader, "ProductID");
							response.Write(string.Format(@"[ ""{0}"" , {1} , {2}]", Security.HtmlEncode(productName), sales, productId));
						}
					}
				}
			}

			response.Write("]}");
		}
	}
}
