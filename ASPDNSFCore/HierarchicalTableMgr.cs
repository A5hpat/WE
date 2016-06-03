// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Xml;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for HierarchicalTableMgr.
	/// Remember, manages tables with parent-child relationships as nested nodes, and each sibling level is ordered by DisplayOrder asc. DisplayOrder MAY NOT be unique!
	/// Requires table to meet certain requirements:
	/// Must have "ID" integer column, e.g. "CategoryID" (can be named differently)
	/// Must have "Name" field column, e.g. "Name" (can be named differently)
	/// Must have a ParentID integer column, e.g. "ParentCategoryID" (can be named differently)
	/// Must have a Published tinyint column, with value of 0 meaning non-published (and not included in the hierarchy)
	/// Must have a Deleted tinyint column. Values of deleted=0 mean they are not included in the hierarchy
	/// Parent Child relationships should be in the form Parent:Child of 1:N (e.g. this is not a N:N mgr!)
	/// Root level records should have ParentID = 0 (NOT NULL!!) This could be extended to work with NULL ID as the root level indicator, but it's not done currently
	/// The table can have as many other fields and types as you want.
	/// It is anticipated that the Name field is <ml>...</ml> encoded for Locales, but that is not required.
	/// </summary>
	public class HierarchicalTableMgr
	{
		private String m_FinalXml = String.Empty;
		private XmlDocument m_XmlDoc;
		private bool m_FromCache = false;
		
		private String m_NodeName = String.Empty;
        private String m_IDColumnName = String.Empty;
        private String m_GUIDColumnName = String.Empty;
        private String m_NameColumnName = "Name";
		private String m_XmlPackageName = "HierarchicalTableMgr";
		private bool m_OnlyPublishedEntitiesAndObjects;

		public HierarchicalTableMgr(String TableName, String NodeName, String IDColumnName, String GUIDColumnName, String NameColumnName, String XmlPackageName, int CacheMinutes, int SetInitialContextToNodeID, bool OnlyPublishedEntitiesAndObjects, int StoreID)
		{
			m_NodeName = NodeName;
            m_IDColumnName = IDColumnName;
            m_GUIDColumnName = GUIDColumnName;
            m_NameColumnName = NameColumnName;
			m_XmlPackageName = XmlPackageName;
			m_OnlyPublishedEntitiesAndObjects = OnlyPublishedEntitiesAndObjects;

			if(m_XmlDoc == null)
			{
                String RTParams = "EntityName=" + TableName + "&PublishedOnly=" + CommonLogic.IIF(m_OnlyPublishedEntitiesAndObjects, "1", "0");
                if (StoreID > 0)
                {
                    RTParams += "&FilterByStore=true&CurrentStoreID=" + StoreID.ToString();
                }
                var package = new XmlPackage(
                    packageName: m_XmlPackageName,
                    additionalRuntimeParms: RTParams);

                m_FinalXml = package.TransformString();
                m_XmlDoc = new XmlDocument();
                if (m_FinalXml.Length != 0)
                {
                    using (StringReader sr = new StringReader(m_FinalXml))
                    {
                        using (XmlReader xr = XmlReader.Create(sr))
                        {
                            m_XmlDoc.Load(xr);
                        }
                    }
                }
            }
		}

		public String FinalXml
		{
			get 
			{
				return m_FinalXml;
			}
		}

		public XmlDocument XmlDoc
		{
			get 
			{
				return m_XmlDoc;
			}
		}

		public XmlNode ResetToRootNode()
		{
			XmlNode n = m_XmlDoc.SelectSingleNode("/root");
			return n;
		}

		// if ToNodeID = 0 or ToNodeID doesn't exist, the current context will NOT be changed and null is returned
		public XmlNode SetContext(int ToNodeID)
		{
			XmlNode n;
			if(ToNodeID == 0)
			{
				return null;
			}
			else
			{
				// TBD need to handle <ml>...</ml> markups here too!
				String NodeSpec = String.Format(@"//{0}[./{1}={2}]",m_NodeName,m_IDColumnName,ToNodeID.ToString());
				n = m_XmlDoc.SelectSingleNode(NodeSpec);
			}
			return n;
		}

        // returns the id of the currently active node
        public int CurrentID(XmlNode CurrentContext)
        {
            if (CurrentContext == null)
            {
                return 0;
            }
            return XmlCommon.XmlFieldUSInt(CurrentContext, m_IDColumnName);
        }

        // returns the GUID of the currently active node
        public String CurrentGUID(XmlNode CurrentContext)
        {
            if (CurrentContext == null)
            {
                return String.Empty;
            }
            return XmlCommon.XmlField(CurrentContext, m_GUIDColumnName);
        }

        // returns the name (locale specific) of the currently active node
		public String CurrentName(XmlNode CurrentContext, String LocaleSetting)
		{
            if (CurrentContext == null)
            {
                return String.Empty;
            } 
            return XmlCommon.XmlFieldByLocale(CurrentContext, m_NameColumnName, LocaleSetting);
		}

		// returns the <FieldName> element value of the currently active node
		public String CurrentField(XmlNode CurrentContext, String FieldName)
		{
            if (CurrentContext == null)
            {
                return String.Empty;
            } 
            return XmlCommon.XmlField(CurrentContext, FieldName);
		}

		// returns the <FieldName> element value of the currently active node
		public String CurrentFieldByLocale(XmlNode CurrentContext, String FieldName, String LocaleSetting)
		{
            if (CurrentContext == null)
            {
                return String.Empty;
            } 
            return XmlCommon.XmlFieldByLocale(CurrentContext, FieldName, LocaleSetting);
		}

		// returns true if the currently active node is at the root level
		public bool IsRootLevel(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return true;
            }
            return (CurrentContext.ParentNode.Name == "root");
		}

		// returns true if the currently active node has any children nodes
		public bool HasChildren(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return false;
            } 
            XmlNode n = CurrentContext.SelectSingleNode(m_NodeName);
			return (n != null);
		}

		// returns xml node list of all categories at this same level
		public XmlNodeList SiblingList(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return null;
            } 
            return CurrentContext.ParentNode.SelectNodes("./" + m_NodeName);
		}

		// changes current context to the parent node of the currently active node
		public XmlNode MoveParent(XmlNode CurrentContext)
		{
			if(IsRootLevel(CurrentContext))
			{
				return null;
			}
			return CurrentContext.ParentNode;
		}

		// changes current context to the first child node of the currently active node
		public XmlNode MoveFirstChild(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return null;
            } 
            XmlNodeList children = CurrentContext.SelectNodes("./" + m_NodeName);
			if (children.Count == 0)
			{
				return null;
			}
			XmlNode n = children[0];
			return n;
		}

		// changes current context to the first sibling node on the same level as the currently active node
		public XmlNode MoveFirstSibling(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return null;
            } 
            XmlNode n = SiblingList(CurrentContext)[0];
			return n;
		}

		// changes current context to the next sibling node on the same level as the currently active node
		public XmlNode MoveNextSibling(XmlNode CurrentContext, bool Circular)
		{
            if (CurrentContext == null)
            {
                return null;
            } 
            XmlNode next = CurrentContext.NextSibling;
			if(Circular && next == null)
			{
				next = MoveFirstSibling(CurrentContext);
			}
			return next;
		}
	}
}
