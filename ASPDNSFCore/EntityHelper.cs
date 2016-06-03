// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Xsl;

namespace AspDotNetStorefrontCore
{
	public class EntitySpecs
    {
        public String m_EntityXsl;
        public String m_EntityName;
        public String m_EntityNamePlural; // because english grammar is unpredictable!
        public bool m_HasParentChildRelationship;
        public bool m_HasDisplayOrder;
        public bool m_HasAddress;
        public String m_ObjectName;
        public String m_ObjectNamePlural; // because english grammar is unpredictable!
        public bool m_EntityObjectMappingIs1to1; // specify true for 1:1 Object:Entity mappings (where the Object table is assumed to have an EntityID column therefore)
        public bool m_HasIconPic;
        public bool m_HasMediumPic;
        public bool m_HasLargePic;

        public EntitySpecs(String XslName, String EntityName, String EntityNamePlural, bool HasParentChildRelationship, bool HasDisplayOrder, bool HasAddress, String ObjectName, String ObjectNamePlural, bool EntityObjectMappingIs1to1, bool HasIconPic, bool HasMediumPic, bool HasLargePic)
        {
            m_EntityXsl = XslName;
            m_EntityName = EntityName;
            m_EntityNamePlural = EntityNamePlural;
            m_HasParentChildRelationship = HasParentChildRelationship;
            m_HasDisplayOrder = HasDisplayOrder;
            m_HasAddress = HasAddress;
            m_ObjectName = ObjectName;
            m_ObjectNamePlural = ObjectNamePlural;
            m_EntityObjectMappingIs1to1 = EntityObjectMappingIs1to1;
            m_HasIconPic = HasIconPic;
            m_HasMediumPic = HasMediumPic;
            m_HasLargePic = HasLargePic;
        }
    }

    public class EntityDefinitions
    {
        static public readonly EntitySpecs readonly_CategoryEntitySpecs = new EntitySpecs("EntityMgr", "Category", "Categories", true, true, false, "Product", "Products", false, true, true, true);
        static public readonly EntitySpecs readonly_SectionEntitySpecs = new EntitySpecs("EntityMgr", "Section", "Sections", true, true, false, "Product", "Products", false, true, true, true);
        static public readonly EntitySpecs readonly_ManufacturerEntitySpecs = new EntitySpecs("EntityMgr", "Manufacturer", "Manufacturers", true, true, true, "Product", "Products", true, true, true, true);
        static public readonly EntitySpecs readonly_DistributorEntitySpecs = new EntitySpecs("EntityMgr", "Distributor", "Distributors", true, true, true, "Product", "Products", true, true, true, true);
        static public readonly EntitySpecs readonly_GenreEntitySpecs = new EntitySpecs("EntityMgr", "Genre", "Genres", true, true, false, "Product", "Products", false, true, true, true);
        static public readonly EntitySpecs readonly_VectorEntitySpecs = new EntitySpecs("EntityMgr", "Vector", "Vectors", true, true, false, "Product", "Products", false, true, true, true);
        static public readonly EntitySpecs readonly_AffiliateEntitySpecs = new EntitySpecs("EntityMgr", "Affiliate", "Affiliates", true, true, true, "Product", "Products", false, false, false, false);
        static public readonly EntitySpecs readonly_CustomerLevelEntitySpecs = new EntitySpecs("EntityMgr", "CustomerLevel", "CustomerLevels", false, true, false, "Product", "Products", false, false, false, false);
        static public readonly EntitySpecs readonly_LibraryEntitySpecs = new EntitySpecs("EntityMgr", "Library", "Libraries", true, true, false, "Document", "Documents", false, true, true, true);

        static public EntitySpecs LookupSpecs(String EntityName)
        {
            switch (EntityName.ToUpperInvariant())
            {
                case "CATEGORY":
                    return readonly_CategoryEntitySpecs;
                case "SECTION":
                case "DEPARTMENT":
                    return readonly_SectionEntitySpecs;
                case "MANUFACTURER":
                    return readonly_ManufacturerEntitySpecs;
                case "DISTRIBUTOR":
                    return readonly_DistributorEntitySpecs;
                case "GENRE":
                    return readonly_GenreEntitySpecs;
                case "VECTOR":
                    return readonly_VectorEntitySpecs;
                case "AFFILIATE":
                    return readonly_AffiliateEntitySpecs;
                case "CUSTOMERLEVEL":
                    return readonly_CustomerLevelEntitySpecs;
                case "LIBRARY":
                    return readonly_LibraryEntitySpecs;
            }
            return null;
        }
    }

    /// <summary>
    /// Summary description for  EntityHelper is a common set of routines that support
    /// a multi level table Parent-child table structure. Note that NOT ALL ROUTINES are semantically valid with all entity types!
    /// </summary>
    public class EntityHelper
    {
		readonly UrlHelper Url;

        private EntitySpecs m_EntitySpecs;
        private String m_IDColumnName;
        private int m_CacheMinutes;
        private bool m_OnlyPublishedEntitiesAndObjects;
        private int m_StoreID;

        // this is public, use with CARE! Make sure you know how this object works before using it!
        public HierarchicalTableMgr m_TblMgr;

        public EntityHelper(EntitySpecs eSpecs, int StoreID) : this(AppLogic.CacheDurationMinutes(), eSpecs, !AppLogic.IsAdminSite, StoreID) { }
        public EntityHelper(int CacheMinutes, EntitySpecs eSpecs, int StoreID) : this(CacheMinutes, eSpecs, !AppLogic.IsAdminSite, StoreID) { }

        public EntityHelper(int CacheMinutes, EntitySpecs eSpecs, bool PublishedOnly, int StoreID)
        {
			Url = DependencyResolver.Current.GetService<UrlHelper>();

			m_EntitySpecs = eSpecs;
            m_IDColumnName = m_EntitySpecs.m_EntityName + "ID";
            m_StoreID = StoreID;
            if (AppLogic.CachingOn)
            {
                m_CacheMinutes = CacheMinutes;
            }
            else
            {
                m_CacheMinutes = 0;
            }
            m_OnlyPublishedEntitiesAndObjects = PublishedOnly;
           
            m_TblMgr = new HierarchicalTableMgr(m_EntitySpecs.m_EntityName, "Entity", "EntityID", "EntityGUID", "Name", m_EntitySpecs.m_EntityXsl, m_CacheMinutes, 0, m_OnlyPublishedEntitiesAndObjects, m_StoreID);
        }

        public EntitySpecs GetEntitySpecs
        {
            get
            {
                return m_EntitySpecs;
            }
        }

        public String GetEntityBreadcrumb6(int EntityID, String LocaleSetting)
        {
            String CacheName = String.Format("GetEntityBreadcrumb6_{0}_{1}_{2}_{3}", m_EntitySpecs.m_EntityName, EntityID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }
            String tmpS = String.Empty;
            String URL = String.Empty;
            XmlNode n = m_TblMgr.SetContext(EntityID);
            if (EntityID != 0 && n != null)
            {
                n = m_TblMgr.MoveParent(n);
                while (n != null)
                {
                    if (AppLogic.IsAdminSite)
                    {
                        URL = String.Format("entity.aspx?EntityName={0}&EntityID={1}", m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n).ToString());
                    }
                    else
                    {
                        URL = Url.BuildEntityLink(m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n), m_TblMgr.CurrentField(n, "SEName"));
                    }
                    n = m_TblMgr.MoveParent(n);
                }
                n = m_TblMgr.SetContext(EntityID);
                if (AppLogic.IsAdminSite)
                {
                    URL = String.Format("entity.aspx?EntityName={0}&EntityID={1}", m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n).ToString());
                }
                else
                {
                    URL = Url.BuildEntityLink(m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n), m_TblMgr.CurrentField(n, "SEName"));
                }
                tmpS += String.Format("<a href=\"{0}\">{1} ({2})</a>", URL, m_TblMgr.CurrentName(n, LocaleSetting), m_TblMgr.CurrentID(n));
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS;
        }

        public String GetEntityBreadcrumb(int EntityID, String LocaleSetting)
        {
            String CacheName = String.Format("GetEntityBreadcrumb_{0}_{1}_{2}_{3}", m_EntitySpecs.m_EntityName, EntityID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }
            String tmpS = String.Empty;
            String URL = String.Empty;
            XmlNode n = m_TblMgr.SetContext(EntityID);
            if (EntityID != 0 && n != null)
            {
                n = m_TblMgr.MoveParent(n);
                while (n != null)
                {
                    if (AppLogic.IsAdminSite)
                    {
						URL = String.Format("entity.aspx?entityname={0}&entityid={1}", m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n));
                    }
                    else
                    {
                        URL = Url.BuildEntityLink(m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n), m_TblMgr.CurrentField(n, "SEName"));
                    }
                    tmpS = String.Format("<a href=\"{0}\">{1}</a> &rarr; ", URL, m_TblMgr.CurrentName(n, LocaleSetting)) + tmpS;
                    n = m_TblMgr.MoveParent(n);
                }
                n = m_TblMgr.SetContext(EntityID);
                if (AppLogic.IsAdminSite)
                {
					URL = String.Format("entity.aspx?entityname={0}&entityid={1}", m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n));
                }
                else
                {
					URL = Url.BuildEntityLink(m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n), m_TblMgr.CurrentField(n, "SEName"));
                }
                tmpS += String.Format("<a href=\"{0}\">{1}</a>", URL, m_TblMgr.CurrentName(n, LocaleSetting));
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS;
        }

        public ArrayList GetEntityArrayList(int ForParentEntityID, String Prefix, int FilterEntityID, String LocaleSetting, bool AllowCaching)
        {
            ArrayList al;
            String CacheName = String.Format("GetEntityArrayList_{0}_{1}_{2}_{3}_{4}_{5}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), Prefix, FilterEntityID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn && AllowCaching)
            {
                al = (ArrayList)HttpContext.Current.Cache.Get(CacheName);
                if (al != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return al;
                }
            }

            al = new ArrayList();

            StringWriter tmpS = new StringWriter();
            String XslFile = "EntityArrayListXML";
            XslCompiledTransform xForm;
            string XslFilePath = CommonLogic.SafeMapPath(string.Format("{0}/EntityHelper/{1}.xslt", AppLogic.AdminDir(), XslFile));
            xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
            if (xForm == null)
            {
                xForm = new XslCompiledTransform(false);
                xForm.Load(XslFilePath);
                HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
            }
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
            xslArgs.AddParam("filterID", "", FilterEntityID);
            xslArgs.AddParam("custlocale", "", LocaleSetting);
            xslArgs.AddParam("deflocale", "", Localization.GetDefaultLocale());
            xslArgs.AddParam("adminsite", "", AppLogic.IsAdminSite);
            xForm.Transform(m_TblMgr.XmlDoc, xslArgs, tmpS);

            XmlDocument returnedXML = new XmlDocument();
            returnedXML.LoadXml(tmpS.ToString());

            XmlNodeList entityNodes = returnedXML.SelectNodes("/Entities/Entity");

            foreach (XmlNode n in entityNodes)
            {
                try
                {
                    XmlNode idNode = n.SelectNodes("EntityId")[0];
                    XmlNode nameNode = n.SelectNodes("EntityName")[0];
                    int entityId;
                    if (int.TryParse(idNode.InnerText, out entityId) && !string.IsNullOrEmpty(nameNode.InnerText))
                    {
                        ListItemClass li = new ListItemClass();
                        li.Value = entityId;
                        li.Item = Security.HtmlEncode(nameNode.InnerText);
                        al.Add(li);
                    }
                }
                catch (Exception)
                {
                }
            }

            if (AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
            {
                try // don't let logging crash the site
                {
                    StreamWriter sw = File.CreateText(CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}_{3}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "~/", ""), XslFile, m_EntitySpecs.m_EntityName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store"))));
                    sw.WriteLine(XmlCommon.PrettyPrintXml(tmpS.ToString()));
                    sw.Close();
                }
                catch { }
            }

            if (AppLogic.CachingOn && AllowCaching)
            {
                HttpContext.Current.Cache.Insert(CacheName, al, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return al;
        }

        public int GetParentEntity(int EntityID)
        {
            int tmp = 0;
            if (EntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(EntityID);
                if (n != null)
                {
                    n = m_TblMgr.MoveParent(n);
                    if (n != null)
                    {
                        tmp = m_TblMgr.CurrentID(n);
                    }
                }
            }
            return tmp;
        }

        public int GetRootEntity(int RootOrSubEntityID)
        {
            int tmp = 0;
            if (RootOrSubEntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(RootOrSubEntityID);
                while (n != null && !m_TblMgr.IsRootLevel(n))
                {
                    n = m_TblMgr.MoveParent(n);
                }
                tmp = m_TblMgr.CurrentID(n);
            }
            return tmp;
        }

        public String GetEntityName(int EntityID, String LocaleSetting)
        {
            String tmp = String.Empty;
            if (EntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(EntityID);
                if (n != null)
                {
                    tmp = m_TblMgr.CurrentFieldByLocale(n, "Name", LocaleSetting);
                }
            }
            return tmp;
        }

        public string GetObjectEntities(int ObjectID, bool ForObjectBrowser)
        {
			var sql = "select " + m_IDColumnName + " from " + m_EntitySpecs.m_ObjectName + "" + m_EntitySpecs.m_EntityName + "  with (NOLOCK)  where " + m_EntitySpecs.m_ObjectName + "ID=" + ObjectID.ToString() + CommonLogic.IIF(ForObjectBrowser, " and " + m_IDColumnName + " in (select " + m_IDColumnName + " from " + m_EntitySpecs.m_EntityName + "  with (NOLOCK)  where Deleted=0 " + CommonLogic.IIF(m_OnlyPublishedEntitiesAndObjects, "and Published=1", "") + " and ShowIn" + m_EntitySpecs.m_ObjectName + "Browser<>0)", "");
			var tmpS = new StringBuilder(1000);

            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        if (tmpS.Length != 0)
                        {
                            tmpS.Append(",");
                        }
                        tmpS.Append(DB.RSFieldInt(rs, m_IDColumnName).ToString());
                    }
                }
            }
            return tmpS.ToString();
        }

        public static ArrayList GetProductEntityList(int ProductID, string EntityName)
        {
			var al = new ArrayList();
            if (EntityName.Length == 0)
            {
                EntityName = EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName;
            }

			var sql = "SELECT EntityID FROM dbo.ProductEntity WHERE EntityType = " + DB.SQuote(EntityName) + " and ProductID=" + ProductID + " ORDER BY DisplayOrder";

            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        al.Add(DB.RSFieldInt(rs, "EntityID"));
                    }
                }
            }
            return al;
        }

        public static int GetProductsFirstEntity(int ProductID, string EntityName)
        {
			var tmp = 0;
            if (EntityName.Length == 0)
            {
                EntityName = EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName;
            }

			var sql = string.Format("select top 1 a.EntityID from productentity a with (nolock) inner join (select distinct a.entityid, a.EntityType from productentity a with (nolock) left join EntityStore b with (nolock) on a.EntityID = b.EntityID where ({0} = 0 or b.StoreID = {1})) b " +
                "on a.EntityID = b.EntityID and a.EntityType=b.EntityType where ProductID = {2} and a.EntityType = {3} ORDER BY DisplayOrder", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), ProductID, DB.SQuote(EntityName));

            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS(sql, dbconn))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "EntityID");
                    }
                }
            }
            return tmp;
        }
    }
}
