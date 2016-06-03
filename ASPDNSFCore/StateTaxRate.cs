// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
	public class StateTaxRate
    {

        #region Private Variables
        private int m_Stateid;
        private int m_Taxclassid;
        private decimal m_Taxrate;

        #endregion

        #region Constructors

        public StateTaxRate(int StateID, int TaxClassID,decimal TaxRate)
        {
            m_Stateid = StateID;
            m_Taxclassid = TaxClassID;
            m_Taxrate = TaxRate;
        }

        #endregion

        #region Public Properties

        public int StateID
        {
            get { return m_Stateid; }
        }

        public int TaxClassID
        {
            get { return m_Taxclassid; }
        }
		
        public decimal TaxRate
        {
            get { return m_Taxrate; }
        }
		
        #endregion
    }

    public class StateTaxRates : IEnumerable
    {
        public SortedList m_StateTaxRates;

        public StateTaxRates()
        {
            m_StateTaxRates = new SortedList();

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("aspdnsf_getStateTaxRate", con))
                {
                    while (rs.Read())
                    {
                        m_StateTaxRates.Add(DB.RSFieldInt(rs, "StateTaxID"), new StateTaxRate(DB.RSFieldInt(rs, "StateID"), DB.RSFieldInt(rs, "TaxClassID"), DB.RSFieldDecimal(rs, "TaxRate")));
                    }
                }
            }
        }

        public decimal GetTaxRate(int StateID, int TaxClassID)
        {
            if (StateID == 0)
            {
                return 0.0M;
            }

            for (int i = 0; i < m_StateTaxRates.Count; i++)
            {
                StateTaxRate str = (StateTaxRate)m_StateTaxRates.GetByIndex(i);
                if (str.StateID == StateID && str.TaxClassID == TaxClassID)
                {
                    return str.TaxRate;
                }
            }
            return 0.0M;
        }

        /// <summary>
        /// Deletes the AppConfig record and removes the item from the collection
        /// </summary>
        public void Remove(int statetaxid)
        {
            try
            {
                DB.ExecuteSQL("delete dbo.StateTaxRate where StateTaxID = " + statetaxid.ToString());
                m_StateTaxRates.Remove(statetaxid);
            }
            catch { }
        }

        public IEnumerator GetEnumerator()
        {
            return new StateTaxRatesEnumerator(this);
        }
    }

    public class StateTaxRatesEnumerator : IEnumerator
    {
        private int position = -1;
        private StateTaxRates m_statetaxrates;

        public StateTaxRatesEnumerator(StateTaxRates statetaxratescol)
        {
            this.m_statetaxrates = statetaxratescol;
        }

        public bool MoveNext()
        {
            if (position < m_statetaxrates.m_StateTaxRates.Count - 1)
            {
                position++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            position = -1;
        }

        public object Current
        {
            get
            {
                return m_statetaxrates.m_StateTaxRates[position];
            }
        }
    }
}
