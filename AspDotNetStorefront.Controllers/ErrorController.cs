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
using System.Web.Routing;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class ErrorController : Controller
	{
		public ActionResult Index(string errorCode = null, int? statusCode = null)
		{
			Response.StatusCode = statusCode ?? 500;

			var customer = HttpContext.GetCustomer();

			var model = new InvalidRequestViewModel(
				errorCode: !string.IsNullOrWhiteSpace(errorCode)
					? errorCode
					: null,
				userIsAdmin: customer.IsAdminUser);

			return View(model);
		}

		public ActionResult NotFound()
		{
			Response.StatusCode = 404;

			var customer = ControllerContext.HttpContext.GetCustomer();
			var topic = new Topic("PageNotFound", customer.LocaleSetting, customer.SkinID, new Parser(), AppLogic.StoreID());

			var model = new NotFoundViewModel(
				title: topic.SectionTitle,
				content: topic.Contents,
				suggestions: AppLogic.AppConfigBool("Show404SuggestionLinks")
					? GenerateSuggestions(customer)
					: null);

			return View(model);
		}

		IEnumerable<NotFoundSuggestionViewModel> GenerateSuggestions(Customer customer)
		{
			var desiredPage = Request.RawUrl.TrimStart('/').ToLowerInvariant();
			var resources = new List<NotFoundResource>();
			var suggestions = new List<NotFoundSuggestionViewModel>();
			var showPageNotFoundDistance = AppLogic.AppConfigNativeDouble("404.ComparisonDistance");

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(var reader = DB.GetRS("exec aspdnsf_Get404Suggestions @storeId",
					connection,
					new SqlParameter("@storeId", AppLogic.GlobalConfigBool("AllowProductFiltering")
						? AppLogic.StoreID()
						: 0)))
				{
					while(reader.Read())
					{
						var resourceName = reader.FieldByLocale("Name", customer.LocaleSetting);
						double computedDistance = LevenshteinDistance.CalculateDistance(resourceName.ToLowerInvariant(), desiredPage);

						//Divide it by resource name length for more accuracy
						double meanDistancePerLetter = computedDistance / resourceName.Length;

						if(meanDistancePerLetter <= showPageNotFoundDistance)
							resources.Add(new NotFoundResource
							(
								name: resourceName,
								description: reader.FieldByLocale("Description", customer.LocaleSetting),
								type: reader.Field("ObjectType"),
								id: reader.FieldInt("Id"),
								distance: computedDistance
							));
					}
				}

				var sortedResources = resources
					.OrderBy(r => r.Distance)
					.Take(AppLogic.AppConfigNativeInt("404.NumberOfSuggestedLinks"));

				foreach(var resource in sortedResources)
					suggestions.Add(new NotFoundSuggestionViewModel
						(
							name: resource.Name,
							url: BuildSuggestionUrl(resource),
							description: resource.Description
						));
			}

			return suggestions;
		}

		string BuildSuggestionUrl(NotFoundResource resource)
		{
			switch(resource.Type)
			{
				case "product":
					return Url.BuildProductLink(resource.Id);
				case "topic":
					return Url.BuildTopicLink(resource.Name, false);
				default:
					return Url.Action(
						actionName: ActionNames.Detail,
						controllerName: ControllerNames.Entity,
						routeValues: new RouteValueDictionary
							{
								{ RouteDataKeys.EntityType, resource.Type }
							});
			}
		}
	}

	public class NotFoundResource
	{
		public string Name;
		public string Description;
		public string Type;
		public int Id;
		public double Distance;

		public NotFoundResource(string name, string description, string type, int id, double distance)
		{
			Name = name;
			Description = description;
			Type = type;
			Id = id;
			Distance = distance;
		}
	}

	public class LevenshteinDistance
	{
		public static int CalculateDistance(string objectName, string desiredName)
		{
			var objectNameLength = objectName.Length;
			var desiredNameLength = desiredName.Length;
			var distanceMatrix = new int[objectNameLength + 1, desiredNameLength + 1];
			var distance = 0;

			//If either is empty, the length of the other is the distance
			if(objectNameLength == 0)
				return desiredNameLength;
			if(desiredNameLength == 0)
				return objectNameLength;

			for(var i = 0; i < objectNameLength; distanceMatrix[i, 0] = ++i) ;
			for(var j = 0; j < desiredNameLength; distanceMatrix[0, j] = ++j) ;

			for(var i = 1; i <= objectNameLength; i++)
			{
				for(var j = 1; j <= desiredNameLength; j++)
				{
					distance = (desiredName.Substring(j - 1, 1) == objectName.Substring(i - 1, 1) ? 0 : 1);

					distanceMatrix[i, j] = System.Math.Min(System.Math.Min(distanceMatrix[i - 1, j] + 1, distanceMatrix[i, j - 1] + 1),
						distanceMatrix[i - 1, j - 1] + distance);
				}
			}

			//The bottom right value of the matrix is the final distnace
			return distanceMatrix[objectNameLength, desiredNameLength];
		}
	}
}
