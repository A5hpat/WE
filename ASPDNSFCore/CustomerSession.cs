// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// This class in only intended to be access from the Customer class
    /// </summary>
    public class CustomerSession
    {
        private int m_CustomerID;
        private int m_SessionID;
        private DateTime m_LastActivity;
        private Hashtable m_SessionParms;

        /// <summary>
        /// Used only to create an empty customersession object for anonymous customer no data is saved to the database
        /// </summary>
        public CustomerSession()
        {
            m_CustomerID = 0;
            m_SessionID = 0;
            m_LastActivity = DateTime.MinValue;
            m_SessionParms = new Hashtable();
        }

        public CustomerSession(int CustomerID)
        {
            m_SessionParms = new Hashtable();
            m_CustomerID = CustomerID;
            LoadFromDB();
        }

        public CustomerSession(int sessionid, bool ParamIsNotUsedJustForOverloadSemantics)
        {
            m_SessionParms = new Hashtable();
            LoadFromDB(sessionid);
        }

        public void Clear()
        {
			StaticClear(this.m_CustomerID);
        }

        public void ClearVal(String ParamName)
        {
            ParamName = ParamName.ToLowerInvariant();
            if (m_SessionID != 0 && m_SessionParms.ContainsKey(ParamName))
            {
                m_SessionParms.Remove(ParamName);
                UpdateCustomerSession(null, null);
            }
        }

        private void LoadFromDB()
        {
            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS("aspdnsf_SessionGetByCustomerID " + m_CustomerID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        m_CustomerID = DB.RSFieldInt(rs, "CustomerID");
                        m_SessionID = DB.RSFieldInt(rs, "CustomerSessionID");
                        m_LastActivity = DB.RSFieldDateTime(rs, "LastActivity");
                        string sessionparams = DB.RSField(rs, "SessionValue");
                        DeserializeParams(sessionparams);
                    }
                    else
                    {
						var sessionid = CustomerSession.createSession(m_CustomerID, "", "", CommonLogic.CustomerIpAddress());

                        using (var dbconn2 = DB.dbConn())
                        {
                            dbconn2.Open();
                            using (var rs2 = DB.GetRS("aspdnsf_SessionGetByID " + sessionid.ToString(), dbconn2))
                            {
                                if (rs2.Read())
                                {
                                    m_CustomerID = DB.RSFieldInt(rs2, "CustomerID");
                                    m_SessionID = sessionid;
                                    m_LastActivity = DB.RSFieldDateTime(rs2, "LastActivity");
									var sessionparams = DB.RSField(rs2, "SessionValue");
                                    DeserializeParams(sessionparams);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadFromDB(int sessionid)
        {
            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS("aspdnsf_SessionGetByID " + sessionid.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        m_CustomerID = DB.RSFieldInt(rs, "CustomerID");
                        m_SessionID = sessionid;
                        m_LastActivity = DB.RSFieldDateTime(rs, "LastActivity");
						var sessionparams = DB.RSField(rs, "SessionValue");
                        DeserializeParams(sessionparams);
                    }
                    else
                    {
                        m_SessionID = 0;
                    }
                }
            }
        }

        public static int createSession(int customerid, string ParamName, string SessionValue, string ipaddr)
        {
            ParamName = ParamName.ToLowerInvariant();

            var SessionID = 0;
			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_SessionInsert";

					cmd.Parameters.Add(new SqlParameter("@CustomerID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@SessionValue", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ipaddr", SqlDbType.VarChar, 15));
					cmd.Parameters.Add(new SqlParameter("@CustomerSessionID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

					cmd.Parameters["@CustomerID"].Value = customerid;

					var sessionparams = new StringBuilder(1024);
					sessionparams.Append("<params>");
					if(ParamName != null && ParamName != "")
					{
						sessionparams.Append("<param name=\"");
						sessionparams.Append(XmlCommon.XmlEncodeAttribute(ParamName));
						sessionparams.Append("\" val=\"");
						sessionparams.Append(XmlCommon.XmlEncodeAttribute(SessionValue));
						sessionparams.Append("\"/>");
					}
					sessionparams.Append("</params>");
					cmd.Parameters["@SessionValue"].Value = sessionparams.ToString();

					if(ipaddr == null)
						cmd.Parameters["@ipaddr"].Value = DBNull.Value;
					else
						cmd.Parameters["@ipaddr"].Value = ipaddr;

					try
					{
						cmd.ExecuteNonQuery();
						SessionID = Convert.ToInt32(cmd.Parameters["@CustomerSessionID"].Value);
					}
					catch(Exception ex)
					{
						SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
					}
				}
			}
            return SessionID;
        }

        public static CustomerSession CreateCustomerSession(int customerid, string SessionName, string SessionValue, string ipaddr)
        {

            CustomerSession cs = null;
			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_SessionInsert";

					cmd.Parameters.Add(new SqlParameter("@CustomerID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@SessionValue", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ipaddr", SqlDbType.VarChar, 15));
					cmd.Parameters.Add(new SqlParameter("@CustomerSessionID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

					cmd.Parameters["@CustomerID"].Value = customerid;

					var sessionparams = new StringBuilder(1024);
					sessionparams.Append("<params>");
					if(SessionName != null && SessionName != "")
					{
						sessionparams.Append("<param name=\"");
						sessionparams.Append(XmlCommon.XmlEncodeAttribute(SessionName));
						sessionparams.Append("\" val=\"");
						sessionparams.Append(XmlCommon.XmlEncodeAttribute(SessionValue));
						sessionparams.Append("\"/>");
					}
					sessionparams.Append("</params>");
					cmd.Parameters["@SessionValue"].Value = sessionparams.ToString();

					if(ipaddr == null)
						cmd.Parameters["@ipaddr"].Value = DBNull.Value;
					else
						cmd.Parameters["@ipaddr"].Value = ipaddr;

					try
					{
						cmd.ExecuteNonQuery();
						var SessionID = Convert.ToInt32(cmd.Parameters["@CustomerSessionID"].Value);
						cs = new CustomerSession(SessionID, false);
					}
					catch(Exception ex)
					{
						SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
					}
				}
			}
            return cs;
        }

        public static void StaticClear()
        {
            DB.ExecuteSQL("aspdnsf_SessionAge", new SqlParameter("@storeId", AppLogic.StoreID()));
        }

        public static void StaticClear(int CustomerID)
        {
            if (CustomerID != 0)
            {
                DB.ExecuteSQL("delete from CustomerSession where CustomerID=" + CustomerID.ToString());
            }
        }

        public string UpdateCustomerSession(string ExpiresOn, object LoggedOut)
        {
            if (m_SessionID == 0)
                return "";

            if (LoggedOut != null)
            {
                Clear();
                return "";
            }
			
            var err = string.Empty;
			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_SessionUpdate";

					cmd.Parameters.Add(new SqlParameter("@CustomerSessionID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@SessionName", SqlDbType.NVarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@SessionValue", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ExpiresOn", SqlDbType.VarChar, 30));
					cmd.Parameters.Add(new SqlParameter("@LoggedOut", SqlDbType.DateTime, 30));


					cmd.Parameters["@CustomerSessionID"].Value = m_SessionID;
					cmd.Parameters["@SessionName"].Value = "";

					cmd.Parameters["@SessionValue"].Value = SerializeParams();


					if(ExpiresOn == null)
						cmd.Parameters["@ExpiresOn"].Value = DBNull.Value;
					else
						cmd.Parameters["@ExpiresOn"].Value = ExpiresOn;

					if(LoggedOut == null)
						cmd.Parameters["@LoggedOut"].Value = DBNull.Value;
					else
						cmd.Parameters["@LoggedOut"].Value = LoggedOut;

					try
					{
						cmd.ExecuteNonQuery();
					}
					catch(Exception ex)
					{
						err = ex.Message;
					}
				}
			}
            return err;
        }

        public void SetVal(String ParamName, String SessionValue)
        {
            SetVal(ParamName, SessionValue, DateTime.MaxValue);
        }

		public void SetVal(string paramName, string sessionValue, DateTime expiresOn)
		{
			paramName = paramName.ToLowerInvariant();

			if (m_SessionID == 0)
				return;

			if (m_SessionParms.ContainsKey(paramName))
			{
				if (string.IsNullOrEmpty(sessionValue))
				{
					m_SessionParms.Remove(paramName);
				}
				else
				{
					var sessionParam = (SessionParam)m_SessionParms[paramName];
					sessionParam.ParamValue = sessionValue;
					sessionParam.ExpireOn = expiresOn;
				}
			}
			else
			{
				if(string.IsNullOrEmpty(sessionValue))
					return;

				var sessionParam = new SessionParam(paramName, sessionValue, expiresOn);
				m_SessionParms.Add(paramName, sessionParam);
			}

			UpdateCustomerSession(null, null);
		}

		public String Session(String ParamName)
        {
            ParamName = ParamName.ToLowerInvariant();
            String tmpS = String.Empty;
            try
            {
                if (m_SessionParms.Contains(ParamName))
                {
                    SessionParam sp = (SessionParam)m_SessionParms[ParamName.ToLowerInvariant().Trim()];
                    tmpS = sp.ParamValue;
                }
                else
                {
                    tmpS = string.Empty;
                }
            }
            catch
            {
                tmpS = String.Empty;
            }
            return tmpS;
        }

        public int SessionUSInt(String paramName)
        {
            String tmpS = Session(paramName);
            return Localization.ParseUSInt(tmpS);
        }

        public string this[string ParamName]
        {
            get { return this.Session(ParamName); }
            set { this.SetVal(ParamName, value); }
        }
		
        public int SessionID
        {
            get { return m_SessionID; }
        }

        /// <summary>
        /// Converts all param keys in the m_SessionParms HashTable to an xml fragment of <param> nodes
        /// </summary>
        /// <returns></returns>
        private string SerializeParams()
        {

            StringBuilder sb = new StringBuilder("<params>", 1024);
            foreach (string s in m_SessionParms.Keys)
            {
                SessionParam sp = (SessionParam)m_SessionParms[s];
                sb.Append("<param name=\"" + XmlCommon.XmlEncodeAttribute(s) + "\" val=\"" + XmlCommon.XmlEncodeAttribute(sp.ParamValue) + "\" " + CommonLogic.IIF(sp.ExpireOn.Equals(DateTime.MaxValue), "", "expireon=\"" + sp.ExpireOn.ToString() + "\"") + " />");
            }
            sb.Append("</params>");
            return sb.ToString();
        }

        /// <summary>
        /// Accepts an XML document fragment of <param> nodes and adds them the the m_SessionParms HashTable 
        /// </summary>
        /// <param name="s"></param>
        private void DeserializeParams(string s)
        {
            XmlDocument x = new XmlDocument();
            x.LoadXml(s);
            foreach (XmlNode n in x.SelectNodes("//param"))
            {
                if (n.Attributes["name"] != null)
                {
                    DateTime expireon = DateTime.MaxValue;
                    if (n.Attributes["expireon"] != null)
                    {
                        expireon = DateTime.Parse(n.Attributes["expireon"].InnerText);
                    }
                    if (expireon > DateTime.Now)
                    {
                        SessionParam sp = new SessionParam(n.Attributes["name"].InnerText, n.Attributes["val"].InnerText, expireon);
                        m_SessionParms.Add(n.Attributes["name"].InnerText.ToLowerInvariant(), sp);
                    }
                }
            }
        }

    }

    public class SessionParam
    {
        public string ParamName;
        public string ParamValue;
        public DateTime ExpireOn;

        public SessionParam(string paramname, string paramvalue, DateTime expireon)
        {
            ParamName = paramname.ToLowerInvariant();
            ParamValue = paramvalue;
            ExpireOn = expireon;
        }
    }
}
