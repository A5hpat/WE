// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for MasterPageBase
	/// </summary>
	public class MasterPageBase : MasterPage
    {
        protected ContentPlaceHolder PageContent;
        protected Literal SectionTitle;
        protected ScriptManager scrptMgr;

        protected Customer ThisCustomer
        {
            get { return Customer.Current; }
        }

        /// <summary>
        /// Gets or sets the SectionTitle text
        /// </summary>
        public string SectionTitleText
        {
            get
            {
                if (SectionTitle != null)
                {
                    return SectionTitle.Text;
                }

                return string.Empty;
            }
            set
            {
                if (SectionTitle != null)
                {
                    SectionTitle.Text = value;
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {           
            if ((Page as SkinBase).RequireScriptManager)
            {               
                // provide hookup for individual pages
                (Page as SkinBase).RegisterScriptAndServices(scrptMgr);                
            }
			Page.ClientScript.RegisterClientScriptInclude(Page.GetType(), "core", ResolveClientUrl("Scripts/core.js"));

            base.OnInit(e);
        }
        
        /// <summary>
        /// Overrides the OnPreRender method
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }
    }
}
