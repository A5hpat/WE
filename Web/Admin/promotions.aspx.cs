// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

public partial class _PromotionGrid : AspDotNetStorefront.Admin.AdminPageBase
{
	protected void btnDelete_Click(object sender, EventArgs e)
	{
		int promotionId;
		LinkButton btn = (LinkButton)sender;
           
		if (int.TryParse(btn.CommandArgument, out promotionId))
		{
			AspDotNetStorefront.Promotions.Data.Promotion promotion = AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.Promotions.FirstOrDefault(l => l.Id == promotionId);
			if(promotion == null)
				return;

			AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.PromotionUsages.DeleteAllOnSubmit(AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.PromotionUsages.Where(pu => pu.PromotionId == promotion.Id));
			AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.Promotions.DeleteOnSubmit(promotion);
			AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.SubmitChanges();
		}
	}
}
