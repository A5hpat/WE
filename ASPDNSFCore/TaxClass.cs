// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
	public class TaxClass
    {
        #region Private Variables
		
        private string m_Taxcode;
        
        #endregion

        #region Constructors

        public TaxClass(int TaxClassID)
        {
            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("aspdnsf_getTaxclass " + TaxClassID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        m_Taxcode = DB.RSField(rs, "TaxCode");

                    }
                    else
                    {
                        m_Taxcode = string.Empty;
                    }
                }
            }
        }

        #endregion

        #region Public Properties

        public string TaxCode
        {
            get { return m_Taxcode; }
        }

        #endregion
    }
}
