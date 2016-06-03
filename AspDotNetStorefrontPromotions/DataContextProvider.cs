// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Configuration;

namespace AspDotNetStorefront.Promotions.Data
{
	public static class DataContextProvider
	{
		#region Properties

        public static EntityContextDataContext Current
		{
			get
			{
				if (ContextStorageController.Current == null)
					ContextStorageController.Current = new HttpContextStorage();

				if (ContextStorageController.Current["PromotionsDataContext"] == null)
					ContextStorageController.Current["PromotionsDataContext"] = new EntityContextDataContext(ConfigurationManager.ConnectionStrings["DBConn"].ConnectionString);

                return (EntityContextDataContext)ContextStorageController.Current["PromotionsDataContext"];
			}
		}

		#endregion
	}
}
