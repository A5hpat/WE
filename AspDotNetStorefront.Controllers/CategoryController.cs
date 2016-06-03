using System.Web;
using System.Web.Mvc;
using AspDotNetStorefront.Models;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Controllers
{
    public class CategoryController : Controller
    {

        public ActionResult Index(int categoryid, string searchEngineName)

        {


            var entity = new Entity(categoryid, "category");
            var customer = ControllerContext.HttpContext.GetCustomer();

            //Make sure we've got a valid entity
            if (entity == null
                || entity.ID == 0
                || entity.Published == false
                || entity.Deleted == true)
                throw new HttpException(404, null);

            //Make sure that this entity is mapped to this store
            var store = new CachelessStore();
            store.StoreID = AppLogic.StoreID();
            var storeMapping = store.GetMapping(entity.EntityType, entity.ID);
            if (AppLogic.GlobalConfigBool("AllowEntityFiltering") == true && !storeMapping.IsMapped)
                throw new HttpException(404, null);

            //Set last seen values on the profile
            HttpContext.Profile.SetPropertyValue("LastViewedEntityName", entity.EntityType);
            HttpContext.Profile.SetPropertyValue("LastViewedEntityInstanceID", entity.ID.ToString());
            HttpContext.Profile.SetPropertyValue("LastViewedEntityInstanceName", XmlCommon.GetLocaleEntry(entity.Name, customer.LocaleSetting, true));

            //Build up the runtime parameters for the xmlpackage
            var runtimeParameters = string.Format("EntityName={0}&EntityID={1}&ProductTypeFilterID=0",
                entity.EntityType,
                entity.ID);

            var entityTypeSpecificRuntimeParamName = "CatID";

            runtimeParameters += string.Format("&{0}={1}", entityTypeSpecificRuntimeParamName, entity.ID);

            //Get a default xmlpackage if we don't have one specified in the database
            var xmlPackageName = string.IsNullOrEmpty(entity.XmlPackage)
                ? "category.xml.config"
                : entity.XmlPackage;

            //Setup Meta tags
            var metaTitle = XmlCommon.GetLocaleEntry(entity.SETitle, customer.LocaleSetting, true);
            if (string.IsNullOrEmpty(metaTitle))
                metaTitle = Security.HtmlEncode(string.Format("{0} - {1}", AppLogic.AppConfig("StoreName"), entity.LocaleName));

            var metaDescription = XmlCommon.GetLocaleEntry(entity.SEDescription, customer.LocaleSetting, true);
            if (string.IsNullOrEmpty(metaDescription))
                metaDescription = Security.HtmlEncode(entity.LocaleName);

            var metaKeywords = XmlCommon.GetLocaleEntry(entity.SEKeywords, customer.LocaleSetting, true);
            if (string.IsNullOrEmpty(metaKeywords))
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
            if (xmlPackage.SETitle != string.Empty)
                metaTitle = xmlPackage.SETitle;
            if (xmlPackage.SEDescription != string.Empty)
                metaDescription = xmlPackage.SEDescription;
            if (xmlPackage.SEKeywords != string.Empty)
                metaKeywords = xmlPackage.SEKeywords;
            if (xmlPackage.SectionTitle != string.Empty)
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

            return View(entityViewModel);
        }
    }
}

