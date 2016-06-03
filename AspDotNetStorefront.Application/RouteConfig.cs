// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Routing.Constraints;
using System.Web.Routing;
using AspDotNetStorefront.Routing;
using AspDotNetStorefront.Routing.Constraints;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Application
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.LowercaseUrls = true;

            var legacyRouteRecognitionEnabled = new LegacyRouteRecognitionEnabled();
            var legacyRouteGenerationEnabled = new LegacyRouteGenerationEnabled();
            var legacy301RedirectHandler = new Legacy301RedirectRouteHandler();
            RegisterLegacyRoutes(routes, legacyRouteRecognitionEnabled, legacyRouteGenerationEnabled, legacy301RedirectHandler);

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.asmx/{*pathInfo}");

            routes.MapRoute(
               name: "CategoryDefault",
               url: "Category/{categoryid}/{sename}",
               defaults: new { controller = "Category", action = "Index", categoryid = "0", sename = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "ProductBySku",
               url: "Product/{sku}",
               defaults: new { controller = "Product", action = "Detailbysku", sku = "0" }
            );

            var seNameOnlyRoutesEnabled = new SeNameOnlyRoutesEnabled();
            RegisterModernSeNameOnlyRoutes(routes, seNameOnlyRoutesEnabled);

            routes.MapMvcAttributeRoutes();
            RegisterAdminRoutes(routes);
            RegisterModernRoutes(routes);


        }

        static void RegisterModernRoutes(RouteCollection routes)
        {
            //routes.MapRoute(
            //	name: RouteNames.ModernEntity,
            //	url: "{entityType}/{id}/{searchEngineName}",
            //	defaults: new RouteValueDictionary {
            //		{ RouteDataKeys.Controller, ControllerNames.Entity },
            //		{ RouteDataKeys.Action, ActionNames.Detail },
            //		{ RouteDataKeys.SearchEngineName, UrlParameter.Optional }},
            //	constraints: new RouteValueDictionary {
            //		{ RouteDataKeys.EntityType, new IsEntityType() },
            //		{ RouteDataKeys.Id, new IntRouteConstraint() }});
            //    });

            routes.MapRoute(
                name: RouteNames.ModernEntityList,
                url: "{entityType}",
                defaults: new RouteValueDictionary {
                    { RouteDataKeys.Controller, ControllerNames.Entity },
                    { RouteDataKeys.Action, ActionNames.Index }},
                constraints: new RouteValueDictionary {
                    { RouteDataKeys.EntityType, new IsEntityType() }});

            routes.MapRoute(
                name: RouteNames.ModernProduct,
                url: "product/{id}/{searchEngineName}",
                defaults: new RouteValueDictionary {
                    { RouteDataKeys.Controller, ControllerNames.Product },
                    { RouteDataKeys.Action, ActionNames.Detail },
                    { RouteDataKeys.SearchEngineName, UrlParameter.Optional }},
                constraints: new RouteValueDictionary {
                    { RouteDataKeys.Id, new IntRouteConstraint() }});

            routes.MapRoute(
                name: RouteNames.ModernNews,
                url: "news/{id}",
                defaults: new RouteValueDictionary {
                    { RouteDataKeys.Controller, ControllerNames.News },
                    { RouteDataKeys.Action, ActionNames.Detail }},
                constraints: new RouteValueDictionary {
                    { RouteDataKeys.Id, new IntRouteConstraint() }});

            // Note that the web.config has a <location> tag for "topic". If you change or add routes
            // for topics, you will need to update the web.config to match.
            routes.MapRoute(
                name: RouteNames.ModernTopic,
                url: "topic/{name}",
                defaults: new RouteValueDictionary {
                    { RouteDataKeys.Controller, ControllerNames.Topic },
                    { RouteDataKeys.Action, ActionNames.Detail }});

            // Note that the web.config has a <location> tag for "xmlpackage". If you change or add routes
            // for topics, you will need to update the web.config to match.
            routes.MapRoute(
                name: RouteNames.ModernXmlPackage,
                url: "xmlpackage/{name}",
                defaults: new RouteValueDictionary {
                    { RouteDataKeys.Controller, ControllerNames.XmlPackage },
                    { RouteDataKeys.Action, ActionNames.Detail }});

            routes.MapRoute(
                name: RouteNames.Default,
                url: "{controller}/{action}/{id}",
                defaults: new RouteValueDictionary {
                    { RouteDataKeys.Controller, ControllerNames.Home },
                    { RouteDataKeys.Action, ActionNames.Index },
                    { RouteDataKeys.Id, UrlParameter.Optional }});
        }

        private static void RegisterAdminRoutes(RouteCollection routes)
        {
            // Allow direct XML package invocations in admin
            routes.MapRoute(
                name: RouteNames.AdminXmlPackage,
                url: "{adminDirectory}/x-{name}.aspx",
                defaults: new RouteValueDictionary {
                    { RouteDataKeys.Controller, ControllerNames.XmlPackage },
                    { RouteDataKeys.Action, ActionNames.Detail },
                    { RouteDataKeys.DisableTemplate, true }},
                constraints: new RouteValueDictionary {
                    { RouteDataKeys.AdminDirectory, new IsAdminDirectory() } });

            // Allow admin requests for files that don't exist to go through to admin. This is for 
            // registered HTTP handlers and such. Requests for files in admin that actually exist 
            // are passed through by default.
            routes.Ignore(
                url: "{adminDirectory}/{*pathinfo}",
                constraints: new
                {
                    adminDirectory = new IsAdminDirectory()
                });
        }

        static void RegisterModernSeNameOnlyRoutes(RouteCollection routes, IRouteConstraint seNameOnlyRoutesEnabled)
        {
            routes.Add(
                name: RouteNames.ModernEntitySeNameOnly,
                item: new AdnsfRoute(
                    url: "{entityType}/{searchEngineName}",
                    routeHandler: new MvcRouteHandler(),
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail }},
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations.Remove(RouteDataKeys.Id) },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.SeNameOnlyRoutesEnabled, seNameOnlyRoutesEnabled },
                        { RouteDataKeys.EntityType, new IsEntityType() },
                        { RouteDataKeys.SearchEngineName, new CompoundRouteConstraint(new List<IRouteConstraint> {
                                new IsEntitySeName(RouteDataKeys.EntityType, RouteDataKeys.Id),
                                new NotConstraint(new IntRouteConstraint()) } )} }));

            routes.Add(
                name: RouteNames.ModernProductSeNameOnly,
                item: new AdnsfRoute(
                    url: "product/{searchEngineName}",
                    routeHandler: new MvcRouteHandler(),
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Product },
                        { RouteDataKeys.Action, ActionNames.Detail }},
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations.Remove(RouteDataKeys.Id) },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.SeNameOnlyRoutesEnabled, seNameOnlyRoutesEnabled },
                        { RouteDataKeys.SearchEngineName, new CompoundRouteConstraint(new List<IRouteConstraint> {
                                new IsProductSeName(RouteDataKeys.Id),
                                new NotConstraint(new IntRouteConstraint()) } )} }));
        }

        static void RegisterLegacyRoutes(RouteCollection routes, IRouteConstraint legacyRouteRecognitionEnabled, IRouteConstraint legacyRouteGenerationEnabled, IRouteHandler legacy301RedirectHandler)
        {
            routes.Add(
                name: RouteNames.Watermark,
                item: new AdnsfRoute(
                    url: "watermark.axd",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Watermark },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    queryStringRouteDataValues: new[] {
                        RouteDataKeys.LegacyImageUrl,
                        RouteDataKeys.LegacyImageSize },
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations
                            .Copy(fromKey: RouteDataKeys.LegacyImageSize, toKey: RouteDataKeys.ImageSize)
                            .ForIncomingRequests(),
                        RouteDataValueTransformations
                            .Copy(fromKey: RouteDataKeys.LegacyImageUrl, toKey: RouteDataKeys.ImageUrl)
                            .ForIncomingRequests(),
                        RouteDataValueTransformations
                            .Copy(fromKey: RouteDataKeys.ImageSize, toKey: RouteDataKeys.LegacyImageSize)
                            .ForUrlGeneration(),
                        RouteDataValueTransformations
                            .Copy(fromKey: RouteDataKeys.ImageUrl, toKey: RouteDataKeys.LegacyImageUrl)
                            .ForUrlGeneration(),
                        RouteDataValueTransformations
                            .Remove(RouteDataKeys.ImageSize)
                            .ForUrlGeneration(),
                        RouteDataValueTransformations
                            .Remove(RouteDataKeys.ImageUrl)
                            .ForUrlGeneration()
                    },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.ContactUs,
                item: new AdnsfRoute(
                    url: "t-contact.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.ContactUs },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.TopicDetailNoTemplate,
                item: new AdnsfRoute(
                    url: "t2-{name}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Topic },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.DisableTemplate, true }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled },
                        { RouteDataKeys.DisableTemplate, "^[Tt]rue$" }}));

            routes.Add(
                name: RouteNames.TopicDetail,
                item: new AdnsfRoute(
                    url: "t-{name}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Topic },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.DisableTemplate, false }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.XmlPackageDetailNoTemplate,
                item: new AdnsfRoute(
                    url: "x-{name}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.XmlPackage },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.DisableTemplate, true }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled },
                        { RouteDataKeys.DisableTemplate, "^[Tt]rue$" }}));

            routes.Add(
                name: RouteNames.XmlPackageDetail,
                item: new AdnsfRoute(
                    url: "e-{name}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.XmlPackage },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.DisableTemplate, false }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.Product,
                item: new AdnsfRoute(
                    url: "p-{id}-{searchEngineName}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Product },
                        { RouteDataKeys.Action, ActionNames.Detail }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.ProductRewritten,
                item: new AdnsfRoute(
                    url: "p-{id}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Product },
                        { RouteDataKeys.Action, ActionNames.Detail }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.ShowProduct,
                item: new AdnsfRoute(
                    url: "showproduct.aspx",
                    routeHandler: legacy301RedirectHandler,
                    queryStringRouteDataValues: new[] {
                        "ProductID",
                        "SEName" },
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Product },
                        { RouteDataKeys.Action, ActionNames.Detail }},
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations.Copy("ProductID", RouteDataKeys.Id),
                        RouteDataValueTransformations.Copy("SEName", RouteDataKeys.SearchEngineName) },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.Manufacturers,
                item: new AdnsfRoute(
                    url: "manufacturers.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Index },
                        { RouteDataKeys.EntityType, EntityTypes.Manufacturer }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.Manufacturer,
                item: new AdnsfRoute(
                    url: "m-{id}-{searchEngineName}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Manufacturer }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.ManufacturerRewritten,
                item: new AdnsfRoute(
                    url: "m-{id}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Manufacturer }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.ShowManufacturer,
                item: new AdnsfRoute(
                    url: "showmanufacturer.aspx",
                    routeHandler: legacy301RedirectHandler,
                    queryStringRouteDataValues: new[] {
                        "ManufacturerID",
                        "SEName" },
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Manufacturer }},
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations.Copy("ManufacturerID", RouteDataKeys.Id),
                        RouteDataValueTransformations.Copy("SEName", RouteDataKeys.SearchEngineName) },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.Categories,
                item: new AdnsfRoute(
                    url: "categories.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Index },
                        { RouteDataKeys.EntityType, EntityTypes.Category }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.Category,
                item: new AdnsfRoute(
                    url: "c-{id}-{searchEngineName}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Category }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.CategoryRewritten,
                item: new AdnsfRoute(
                    url: "c-{id}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Category }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.ShowCategory,
                item: new AdnsfRoute(
                    url: "showcategory.aspx",
                    routeHandler: legacy301RedirectHandler,
                    queryStringRouteDataValues: new[] {
                        "CategoryID",
                        "SEName" },
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Category }},
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations.Copy("CategoryID", RouteDataKeys.Id),
                        RouteDataValueTransformations.Copy("SEName", RouteDataKeys.SearchEngineName) },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.Sections,
                item: new AdnsfRoute(
                    url: "sections.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Index },
                        { RouteDataKeys.EntityType, EntityTypes.Section }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.Section,
                item: new AdnsfRoute(
                    url: "s-{id}-{searchEngineName}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Section }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.SectionRewritten,
                item: new AdnsfRoute(
                    url: "s-{id}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Section }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.ShowSection,
                item: new AdnsfRoute(
                    url: "showsection.aspx",
                    routeHandler: legacy301RedirectHandler,
                    queryStringRouteDataValues: new[] {
                        "SectionID",
                        "SEName" },
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Section }},
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations.Copy("SectionID", RouteDataKeys.Id),
                        RouteDataValueTransformations.Copy("SEName", RouteDataKeys.SearchEngineName) },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.Distributors,
                item: new AdnsfRoute(
                    url: "distributors.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Action, ActionNames.Index },
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.EntityType, EntityTypes.Distributor }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.Distributor,
                item: new AdnsfRoute(
                    url: "d-{id}-{searchEngineName}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Distributor }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.DistributorRewritten,
                item: new AdnsfRoute(
                    url: "d-{id}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Distributor }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.ShowDistributor,
                item: new AdnsfRoute(
                    url: "showdistributor.aspx",
                    routeHandler: legacy301RedirectHandler,
                    queryStringRouteDataValues: new[] {
                        "DistributorID",
                        "SEName" },
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Distributor }},
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations.Copy("DistributorID", RouteDataKeys.Id),
                        RouteDataValueTransformations.Copy("SEName", RouteDataKeys.SearchEngineName) },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.Genres,
                item: new AdnsfRoute(
                    url: "genres.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Index },
                        { RouteDataKeys.EntityType, EntityTypes.Genre }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.Genre,
                item: new AdnsfRoute(
                    url: "g-{id}-{searchEngineName}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Genre }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.GenreRewritten,
                item: new AdnsfRoute(
                    url: "g-{id}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Genre }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.ShowGenre,
                item: new AdnsfRoute(
                    url: "showgenre.aspx",
                    routeHandler: legacy301RedirectHandler,
                    queryStringRouteDataValues: new[] {
                        "GenreID",
                        "SEName" },
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Genre }},
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations.Copy("GenreID", RouteDataKeys.Id),
                        RouteDataValueTransformations.Copy("SEName", RouteDataKeys.SearchEngineName) },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.Vectors,
                item: new AdnsfRoute(
                    url: "vectors.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Index },
                        { RouteDataKeys.EntityType, EntityTypes.Vector }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.Vector,
                item: new AdnsfRoute(
                    url: "v-{id}-{searchEngineName}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Vector }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.VectorRewritten,
                item: new AdnsfRoute(
                    url: "v-{id}.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Vector }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.ShowVector,
                item: new AdnsfRoute(
                    url: "showvector.aspx",
                    routeHandler: legacy301RedirectHandler,
                    queryStringRouteDataValues: new[] {
                        "VectorID",
                        "SEName" },
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Entity },
                        { RouteDataKeys.Action, ActionNames.Detail },
                        { RouteDataKeys.EntityType, EntityTypes.Vector }},
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations.Copy("VectorID", RouteDataKeys.Id),
                        RouteDataValueTransformations.Copy("SEName", RouteDataKeys.SearchEngineName) },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.Search,
                item: new AdnsfRoute(
                    url: "search.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Search },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.AdvancedSearch,
                item: new AdnsfRoute(
                    url: "searchadv.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Search },
                        { RouteDataKeys.Action, ActionNames.AdvancedSearch }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.NewsArticle,
                item: new AdnsfRoute(
                    url: "news.aspx",
                    routeHandler: legacy301RedirectHandler,
                    queryStringRouteDataValues: new[] {
                        RouteDataKeys.ShowArticle },
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.News },
                        { RouteDataKeys.Action, ActionNames.Detail } },
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations
                            .Copy(fromKey: RouteDataKeys.ShowArticle, toKey: RouteDataKeys.Id)
                            .ForIncomingRequests(),
                        RouteDataValueTransformations
                            .Copy(fromKey: RouteDataKeys.Id, toKey: RouteDataKeys.ShowArticle)
                            .ForUrlGeneration() },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled },
                        { RouteDataKeys.Id, new IntRouteConstraint() } }));

            routes.Add(
                name: RouteNames.News,
                item: new AdnsfRoute(
                    url: "news.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.News },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled },
                        { RouteDataKeys.Id, new OrConstraint(
                            new AbsentContsraint(),
                            new EmptyConstraint()) },
                        { RouteDataKeys.ShowArticle, new OrConstraint(
                            new AbsentContsraint(),
                            new EmptyConstraint()) } }));

            routes.Add(
                name: RouteNames.RssFeed,
                item: new AdnsfRoute(
                    url: "rssfeed.aspx",
                    routeHandler: legacy301RedirectHandler,
                    queryStringRouteDataValues: new[] {
                        RouteDataKeys.Channel },
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Rss },
                        { RouteDataKeys.Action, ActionNames.Index } },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.PageNotFound,
                item: new AdnsfRoute(
                    url: "pagenotfound.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Error },
                        { RouteDataKeys.Action, ActionNames.NotFound }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.InvalidRequest,
                item: new AdnsfRoute(
                    url: "invalidrequest.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Error },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.HomePage,
                item: new AdnsfRoute(
                    url: "default.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Home },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.Account,
                item: new AdnsfRoute(
                    url: "account.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Account },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.CreateAccount,
                item: new AdnsfRoute(
                    url: "createaccount.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Account },
                        { RouteDataKeys.Action, ActionNames.Create }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.BestSellers,
                item: new AdnsfRoute(
                    url: "bestsellers.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.BestSellers },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.RecentAdditions,
                item: new AdnsfRoute(
                    url: "recentadditions.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.RecentAdditions },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.SiteMap,
                item: new AdnsfRoute(
                    url: "sitemap.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.SiteMap },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.SiteMapFeed,
                item: new AdnsfRoute(
                    url: "googleindex.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.SiteMapFeed },
                        { RouteDataKeys.Action, ActionNames.Index } },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.SiteMapFeedEntity,
                item: new AdnsfRoute(
                    url: "googleentity.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.SiteMapFeed },
                        { RouteDataKeys.Action, ActionNames.Entity }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.SiteMapFeedTopics,
                item: new AdnsfRoute(
                    url: "googletopics.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.SiteMapFeed },
                        { RouteDataKeys.Action, ActionNames.Topics }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.SignIn,
                item: new AdnsfRoute(
                    url: "signin.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Account },
                        { RouteDataKeys.Action, ActionNames.SignIn }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.AdminSignIn,
                item: new AdnsfRoute(
                    url: "admin/signin.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Account },
                        { RouteDataKeys.Action, ActionNames.SignIn }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.SignOut,
                item: new AdnsfRoute(
                    url: "signout.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Account },
                        { RouteDataKeys.Action, ActionNames.SignOut }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.RequestCatalog,
                item: new AdnsfRoute(
                    url: "requestcatalog.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.RequestCatalog },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.SetLocale,
                item: new AdnsfRoute(
                    url: "setlocale.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Customer },
                        { RouteDataKeys.Action, ActionNames.SetLocale }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.SetCurrency,
                item: new AdnsfRoute(
                    url: "setcurrency.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Customer },
                        { RouteDataKeys.Action, ActionNames.SetCurrency }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.SetVatSetting,
                item: new AdnsfRoute(
                    url: "setvatsetting.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Customer },
                        { RouteDataKeys.Action, ActionNames.SetVatSetting }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.AddToCart,
                item: new AdnsfRoute(
                    url: "addtocart.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.ShoppingCart },
                        { RouteDataKeys.Action, ActionNames.AddToCart }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.BulkAddToCart,
                item: new AdnsfRoute(
                    url: "tableorder_process.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.ShoppingCart },
                        { RouteDataKeys.Action, ActionNames.BulkAddToCart }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.ShoppingCart,
                item: new AdnsfRoute(
                    url: "shoppingcart.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Checkout },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.PayPalNotification,
                item: new AdnsfRoute(
                    url: "paypalnotification.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.PayPalNotifications },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.AuthorizeNetSilentPost,
                item: new AdnsfRoute(
                    url: "authnetpost.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.RecurringOrder },
                        { RouteDataKeys.Action, ActionNames.AuthorizeNetSilentPost }},
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled }}));

            routes.Add(
                name: RouteNames.RateProduct,
                item: new AdnsfRoute(
                    url: "rateit.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Rating },
                        { RouteDataKeys.Action, ActionNames.RateProduct }},
                    queryStringRouteDataValues: new[] {
                        "ProductID" },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.RateComment,
                item: new AdnsfRoute(
                    url: "ratecomment.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Rating },
                        { RouteDataKeys.Action, ActionNames.RateComment }},
                    queryStringRouteDataValues: new[] {
                        "ProductID",
                        "VotingCustomerID",
                        "Vote",
                        "RatingCustomerId" },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.Receipt,
                item: new AdnsfRoute(
                    url: "receipt.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Receipt },
                        { RouteDataKeys.Action, ActionNames.Index }},
                    queryStringRouteDataValues: new[] {
                        "orderNumber" },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.EmailProduct,
                item: new AdnsfRoute(
                    url: "emailproduct.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.Product },
                        { RouteDataKeys.Action, ActionNames.EmailProduct }},
                    queryStringRouteDataValues: new[] {
                        "ProductID" },
                    routeDataValueTransforms: new[] {
                        RouteDataValueTransformations.Copy("ProductID", RouteDataKeys.Id) },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));

            routes.Add(
                name: RouteNames.PayPalEmbeddedCheckoutOk,
                item: new AdnsfRoute(
                    url: "paypalembeddedcheckoutok.aspx",
                    routeHandler: legacy301RedirectHandler,
                    defaults: new RouteValueDictionary {
                        { RouteDataKeys.Controller, ControllerNames.PayPalPaymentsAdvanced },
                        { RouteDataKeys.Action, ActionNames.Ok }},
                    queryStringRouteDataValues: new[] {
                        "AVSZIP",
                        "POSTFPSMSG",
                        "BILLTOEMAIL",
                        "TYPE",
                        "ACCT",
                        "PROCCVV2",
                        "PPREF",
                        "CVV2MATCH",
                        "LASTNAME",
                        "PNREF",
                        "TENDER",
                        "EMAILTOSHIP",
                        "EMAIL",
                        "METHOD",
                        "AMT",
                        "SHIPTOCOUNTRY",
                        "CORRELATIONID",
                        "TRANSTIME",
                        "BILLTOCOUNTRY",
                        "AUTHCODE",
                        "EXPDATE",
                        "IAVS",
                        "RESPMSG",
                        "COUNTRY",
                        "TAX",
                        "CARDTYPE",
                        "AVSDATA",
                        "PROCAVS",
                        "SECURETOKEN",
                        "PREFPSMSG",
                        "SECURETOKENID",
                        "AVSADDR",
                        "USER1",
                        "COUNTRYTOSHIP",
                        "RESULT",
                        "TRXTYPE" },
                    constraints: new RouteValueDictionary {
                        { RouteDataKeys.LegacyRoutesEnabled, legacyRouteRecognitionEnabled },
                        { RouteDataKeys.LegacyUrlGenerationEnabled, legacyRouteGenerationEnabled } }));
        }
    }
}
