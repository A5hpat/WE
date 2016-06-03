// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
	public class ZipTaxRate
    {
        #region Private Variables
        private int m_Ziptaxid;
        private string m_Zipcode;
        private int m_Taxclassid;
        private decimal m_Taxrate;
        private int m_CountryID;

        #endregion

        #region Constructors

        public ZipTaxRate(int ZipTaxID, string ZipCode, int TaxClassID, decimal TaxRate, int CountryID)
        {
            m_Ziptaxid = ZipTaxID;
            m_Zipcode = ZipCode;
            m_Taxclassid = TaxClassID;
            m_Taxrate = TaxRate;
            m_CountryID = CountryID;
        }

        #endregion

        #region Public Properties

        public int ZipTaxID
        {
            get { return m_Ziptaxid; }
        }

        public string ZipCode
        {
            get { return m_Zipcode; }
        }

        public int TaxClassID
        {
            get { return m_Taxclassid; }
        }

        public decimal TaxRate
        {
            get { return m_Taxrate; }
        }

        public int CountryID
        {
            get { return m_CountryID; }
        }
        
        #endregion
    }

    public class ZipTaxRates : IEnumerable
    {
        public SortedList m_ZipTaxRates;

        public ZipTaxRates()
        {
            m_ZipTaxRates = new SortedList();

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("aspdnsf_SelectZipTaxRatesAll", con))
                {
                    while (rs.Read())
                    {
                        m_ZipTaxRates.Add(DB.RSFieldInt(rs, "ZipTaxID"), new ZipTaxRate(DB.RSFieldInt(rs, "ZipTaxID"), DB.RSField(rs, "ZipCode"), DB.RSFieldInt(rs, "TaxClassID"), DB.RSFieldDecimal(rs, "TaxRate"), DB.RSFieldInt(rs, "CountryID")));
                    }
                }
            }
        }

        public decimal GetTaxRate(string ZipCode, int TaxClassID, int CountryID)
        {
            if (ZipCode == string.Empty)
            {
                return 0.0M;
            }
            for (int i = 0; i < m_ZipTaxRates.Count; i++)
            {
                ZipTaxRate ztr = (ZipTaxRate)m_ZipTaxRates.GetByIndex(i);
                
                if (ztr.ZipCode.Equals(ZipCode, StringComparison.InvariantCultureIgnoreCase) && ztr.TaxClassID == TaxClassID && ztr.CountryID == CountryID)
                {
                    return ztr.TaxRate;
                }
            }
            return 0.0M;
        }

        public void RemoveAll(string ZipCode, int CountryID)
        {
            try
            {

                List<ZipTaxRate> itemsToBeRemoved = new List<ZipTaxRate>();
                foreach (int ziptaxid in this.m_ZipTaxRates.Keys)
                {
                    ZipTaxRate taxRate = this.m_ZipTaxRates[ziptaxid] as ZipTaxRate;
                    if (taxRate.ZipCode.Equals(ZipCode, StringComparison.InvariantCultureIgnoreCase) && taxRate.CountryID.Equals(CountryID))
                    {
                        itemsToBeRemoved.Add(taxRate);
                    }
                }

                foreach (ZipTaxRate taxRate in itemsToBeRemoved)
                {
                    this.m_ZipTaxRates.Remove(taxRate.ZipTaxID);
                }
            }
            catch { }
        }

        public IEnumerator GetEnumerator()
        {
            return new ZipTaxRatesEnumerator(this);
        }
    }

    public class ZipTaxRatesEnumerator : IEnumerator
    {
        private int position = -1;
        private ZipTaxRates m_ziptaxrates;

        public ZipTaxRatesEnumerator(ZipTaxRates ziptaxratescol)
        {
            this.m_ziptaxrates = ziptaxratescol;
        }

        public bool MoveNext()
        {
            if (position < m_ziptaxrates.m_ZipTaxRates.Count - 1)
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
                return m_ziptaxrates.m_ZipTaxRates[position];
            }
        }
    }
}
