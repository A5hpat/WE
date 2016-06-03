// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections;

namespace AspDotNetStorefrontCore
{
	public class CountryTaxRate
    {
        #region Private Variables
        private int m_Countryid;
        private int m_Taxclassid;
        private decimal m_Taxrate;
        #endregion

        #region Constructors

        public CountryTaxRate(int CountryID, int TaxClassID, decimal TaxRate)
        {
            m_Countryid = CountryID;
            m_Taxclassid = TaxClassID;
            m_Taxrate = TaxRate;
        }

        #endregion

        #region Public Properties

        public int CountryID
        {
            get { return m_Countryid; }
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

    public class CountryTaxRates : IEnumerable
    {
        public SortedList m_CountryTaxRates;

        public CountryTaxRates()
        {
            m_CountryTaxRates = new SortedList();
            using (var conn = DB.dbConn())
            {
                conn.Open();
                using (var rs = DB.GetRS("aspdnsf_getCountryTaxRate", conn))
                {
                    while (rs.Read())
                    {
                        m_CountryTaxRates.Add(DB.RSFieldInt(rs, "CountryTaxID"), new CountryTaxRate(DB.RSFieldInt(rs, "CountryID"), DB.RSFieldInt(rs, "TaxClassID"), DB.RSFieldDecimal(rs, "TaxRate")));
                    }
                }
            }
        }

        public decimal GetTaxRate(int CountryID, int TaxClassID)
        {
            if (CountryID == 0)
            {
                return System.Decimal.Zero;
            }
            for (int i = 0; i < m_CountryTaxRates.Count; i++)
            {
                CountryTaxRate ctr = (CountryTaxRate)m_CountryTaxRates.GetByIndex(i);
                if (ctr.CountryID == CountryID && ctr.TaxClassID == TaxClassID)
                {
                    return ctr.TaxRate;
                }
            }
            return System.Decimal.Zero;
        }

        /// <summary>
        /// Deletes the AppConfig record and removes the item from the collection
        /// </summary>
        public void Remove(int countrytaxid)
        {
            try
            {
                DB.ExecuteSQL("delete dbo.CountryTaxRate where CountryTaxRateID = " + countrytaxid.ToString());
                m_CountryTaxRates.Remove(countrytaxid);
            }
            catch { }
        }

        public IEnumerator GetEnumerator()
        {
            return new CountryTaxRatesEnumerator(this);
        }
    }

    public class CountryTaxRatesEnumerator : IEnumerator
    {
        private int position = -1;
        private CountryTaxRates m_countrytaxrates;

        public CountryTaxRatesEnumerator(CountryTaxRates countrytaxratescol)
        {
            this.m_countrytaxrates = countrytaxratescol;
        }

        public bool MoveNext()
        {
            if (position < m_countrytaxrates.m_CountryTaxRates.Count - 1)
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
                return m_countrytaxrates.m_CountryTaxRates[position];
            }
        }
    }
}
