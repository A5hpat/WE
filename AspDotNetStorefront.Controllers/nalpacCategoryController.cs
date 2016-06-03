// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Filters;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;
using AspDotNetStorefront.Routing;

namespace AspDotNetStorefront.Controllers
{
	public class nalpacCategoryController : Controller
	{
		[EntityPageTypeFilter]
		public ActionResult Detail(int id, string entityType, string searchEngineName, int? productTypeFilterId)
		{
			var normalizedEntityType = AppLogic.ro_SupportedEntities
				.Intersect(new[] { entityType }, StringComparer.OrdinalIgnoreCase)
				.FirstOrDefault();

			if(normalizedEntityType == null)
				throw new HttpException(404, string.Format("Unsupported entity type: {0}", entityType));

			var entity = new Entity(id, normalizedEntityType);
			var customer = ControllerContext.HttpContext.GetCustomer();

			//Make sure we've got a valid entity
			if(entity == null 
				|| entity.ID == 0
				|| entity.Published == false
				|| entity.Deleted == true)
				throw new HttpException(404, null);

			//Make sure that this entity is mapped to this store
			var store = new CachelessStore();
			store.StoreID = AppLogic.StoreID();
			var storeMapping = store.GetMapping(entity.EntityType, entity.ID);
			if(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true && !storeMapping.IsMapped)
				throw new HttpException(404, null);

			//301 Redirect to the correct search engine name in the url if it is wrong
			if(!StringComparer.OrdinalIgnoreCase.Equals(searchEngineName, entity.SEName))
				return RedirectPermanent(Url.BuildEntityLink(normalizedEntityType, id, entity.SEName));

			//Set last seen values on the profile
			HttpContext.Profile.SetPropertyValue("LastViewedEntityName", entity.EntityType);
			HttpContext.Profile.SetPropertyValue("LastViewedEntityInstanceID", entity.ID.ToString());
			HttpContext.Profile.SetPropertyValue("LastViewedEntityInstanceName", XmlCommon.GetLocaleEntry(entity.Name, customer.LocaleSetting, true));

			//Build up the runtime parameters for the xmlpackage
			var runtimeParameters = string.Format("EntityName={0}&EntityID={1}&ProductTypeFilterID={2}", 
				entity.EntityType, 
				entity.ID,
				productTypeFilterId ?? 0);

			var entityTypeSpecificRuntimeParamName = "CatID";
			if(entity.EntityType.Equals("manufacturer", StringComparison.InvariantCultureIgnoreCase))
				entityTypeSpecificRuntimeParamName = "ManID";
			else if(entity.EntityType.Equals("section", StringComparison.InvariantCultureIgnoreCase))
				entityTypeSpecificRuntimeParamName = "SecID";
			else if(entity.EntityType.Equals("distributor", StringComparison.InvariantCultureIgnoreCase))
				entityTypeSpecificRuntimeParamName = "DistID";
			else if(entity.EntityType.Equals("genre", StringComparison.InvariantCultureIgnoreCase))
				entityTypeSpecificRuntimeParamName = "GenreID";
			else if(entity.EntityType.Equals("vector", StringComparison.InvariantCultureIgnoreCase))
				entityTypeSpecificRuntimeParamName = "VectorID";
			else if(entity.EntityType.Equals("section", StringComparison.InvariantCultureIgnoreCase))
				entityTypeSpecificRuntimeParamName = "DistID";

			runtimeParameters += string.Format("&{0}={1}", entityTypeSpecificRuntimeParamName, entity.ID);

			//Get a default xmlpackage if we don't have one specified in the database
			var xmlPackageName = string.IsNullOrEmpty(entity.XmlPackage)
				? AppLogic.ro_DefaultEntityXmlPackage
				: entity.XmlPackage;

			//Setup Meta tags
			var metaTitle = XmlCommon.GetLocaleEntry(entity.SETitle, customer.LocaleSetting, true);
			if(string.IsNullOrEmpty(metaTitle))
				metaTitle = Security.HtmlEncode(string.Format("{0} - {1}", AppLogic.AppConfig("StoreName"), entity.LocaleName));

			var metaDescription = XmlCommon.GetLocaleEntry(entity.SEDescription, customer.LocaleSetting, true);
			if(string.IsNullOrEmpty(metaDescription))
				metaDescription = Security.HtmlEncode(entity.LocaleName);

			var metaKeywords = XmlCommon.GetLocaleEntry(entity.SEKeywords, customer.LocaleSetting, true);
			if(string.IsNullOrEmpty(metaKeywords))
				metaKeywords = Security.HtmlEncode(entity.LocaleName);

			//Setup the breadcrumb
			var pageTitle = Breadcrumb.GetEntityBreadcrumb(entity.ID, entity.LocaleName, entity.EntityType, customer);

			//Get the page content from the xmlpackage
			var pageContent = string.Empty;
			var xmlPackage = new XmlPackage(
				packageName: xmlPackageName,
				customer: customer,
				additionalRuntimeParms: runtimeParameters,
				htmlHelper: ControllerContext.GetHtmlHelper());

			var parser = new Parser();
			pageContent = AppLogic.RunXmlPackage(xmlPackage, parser, customer, customer.SkinID, true, true);
			//override the meta tags from the xmlpackage
			if(xmlPackage.SETitle != string.Empty)
				metaTitle = xmlPackage.SETitle;
			if(xmlPackage.SEDescription != string.Empty)
				metaDescription = xmlPackage.SEDescription;
			if(xmlPackage.SEKeywords != string.Empty)
				metaKeywords = xmlPackage.SEKeywords;
			if(xmlPackage.SectionTitle != string.Empty)
				pageTitle = xmlPackage.SectionTitle;

			var payPalAd = new PayPalAd(PayPalAd.TargetPage.Entity);

			//Build the view model
			var entityViewModel = new EntityViewModel
			{
				Name = XmlCommon.GetLocaleEntry(entity.Name, customer.LocaleSetting, true),
				MetaTitle = metaTitle,
				MetaDescription = metaDescription,
				MetaKeywords = metaKeywords,
				PageTitle = pageTitle,
				PageContent = pageContent,
				PayPalAd = payPalAd.Show ? payPalAd.ImageScript : string.Empty,
				XmlPackageName = xmlPackageName
			};

			AppLogic.eventHandler("ViewEntityPage").CallEvent("&ViewEntityPage=true");


			//Override the layout
			var layoutName = string.Empty;
			if(AppLogic.AppConfigBool("TemplateSwitching.Enabled"))
				layoutName = AppLogic.GetCurrentEntityTemplateName(entity.EntityType, entity.ID);

			if(!string.IsNullOrEmpty(layoutName))
				return View(ActionNames.Detail, layoutName, entityViewModel);
			else
				return View(entityViewModel);
		}

		public ActionResult Index(string entityType)
		{
			var normalizedEntityType = AppLogic.ro_SupportedEntities
				.Intersect(new[] { entityType }, StringComparer.OrdinalIgnoreCase)
				.FirstOrDefault();

			if(normalizedEntityType == null)
				throw new HttpException(404, string.Format("Unsupported entity type: {0}", entityType));

			var customer = ControllerContext.HttpContext.GetCustomer();
			var runtimeParameters = string.Format("entity={0}", normalizedEntityType);
			var xmlpackage = new XmlPackage("entitygridpage.xml.config", customer, customer.SkinID, string.Empty, runtimeParameters, string.Empty, true);
			var packageOutput = AppLogic.RunXmlPackage(xmlpackage, null, customer, customer.SkinID, true, false);

			var pageTitle = AppLogic.GetString(string.Format("AppConfig.{0}PromptPlural", normalizedEntityType),
					customer.SkinID,
					customer.LocaleSetting);

			if(!string.IsNullOrEmpty(xmlpackage.SectionTitle))
				pageTitle = xmlpackage.SectionTitle;

			var simplePageViewModel = new SimplePageViewModel
			{
				MetaTitle = xmlpackage.SETitle,
				MetaDescription = xmlpackage.SEDescription,
				MetaKeywords = xmlpackage.SEKeywords,
				PageTitle = pageTitle,
				PageContent = packageOutput,
			};
			return View(ViewNames.SimplePage, simplePageViewModel);
		}
	}
}
