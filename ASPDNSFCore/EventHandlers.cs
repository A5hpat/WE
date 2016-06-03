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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AspDotNetStorefrontCore
{
	public class AspdnsfEventHandler
    {
        #region private variables
        private int m_EventID;
        private string m_EventName;
        private string m_CalloutURL;
        private string m_XmlPackage;
        private bool m_Active;
        private bool m_Debug;
        #endregion

        #region contructors

        public AspdnsfEventHandler(int EventID)
        {

            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS("aspdnsf_getEventHandler " + EventID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        m_EventID = DB.RSFieldInt(rs, "EventID");
                        m_EventName = DB.RSField(rs, "EventName").Trim();
                        m_CalloutURL = DB.RSField(rs, "CalloutURL").Trim();
                        m_XmlPackage = DB.RSField(rs, "XmlPackage").Trim();
                        m_Active = DB.RSFieldBool(rs, "Active");
                        m_Debug = DB.RSFieldBool(rs, "Debug");
                    }
                }
            }
        }

        public AspdnsfEventHandler(int EventID, string EventName, string CalloutURL, string XmlPackage, bool Active, bool Debug)
        {
            m_EventID = EventID;
            m_EventName = EventName;
            m_CalloutURL = CalloutURL;
            m_XmlPackage = XmlPackage;
            m_Active = Active;
            m_Debug = Debug;
        }

        #endregion

        #region static methods

        public static AspdnsfEventHandler Create(string EventName, string CalloutURL, string XmlPackage, bool Active, bool Debug)
        {
			var EventID = 0;

            if (EventName.Trim().Length == 0)
            {
                return null;
            }

            if (CalloutURL.Trim().Length == 0)
            {
                return null;
            }

            if (XmlPackage.Trim().Length == 0)
            {
                return null;
            }
			
			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_insEventHandler";

					cmd.Parameters.Add(new SqlParameter("@EventName", SqlDbType.NVarChar, 20));
					cmd.Parameters.Add(new SqlParameter("@CalloutURL", SqlDbType.VarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@XmlPackage", SqlDbType.VarChar, 100));
					cmd.Parameters.Add(new SqlParameter("@Active", SqlDbType.Bit));
					cmd.Parameters.Add(new SqlParameter("@Debug", SqlDbType.Bit));
					cmd.Parameters.Add(new SqlParameter("@EventID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

					cmd.Parameters["@EventName"].Value = EventName;
					cmd.Parameters["@CalloutURL"].Value = CalloutURL;
					cmd.Parameters["@XmlPackage"].Value = XmlPackage;
					cmd.Parameters["@Active"].Value = Active;
					cmd.Parameters["@Debug"].Value = Debug;

					try
					{
						cmd.ExecuteNonQuery();
						EventID = Int32.Parse(cmd.Parameters["@EventID"].Value.ToString());
					}
					catch(Exception ex)
					{
						SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
					}
				}
			}

            if (EventID > 0)
            {
                AspdnsfEventHandler a = new AspdnsfEventHandler(EventID);
                return a;
            }
            return null;

        }

        public static string Update(int EventID, SqlParameter[] spa)
        {
			var err = String.Empty;

			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_updEventHandler";

					var sqlparam = new SqlParameter("@EventID", SqlDbType.Int, 4);
					sqlparam.Value = EventID;
					cmd.Parameters.Add(sqlparam);
					foreach(SqlParameter sp in spa)
						cmd.Parameters.Add(sp);

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

        public static string Update(int EventID, string EventName, string CalloutURL, string XMLPackage, object Active, object Debug)
        {

			var err = string.Empty;
			using(var cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using(var cmd = new SqlCommand())
				{
					cmd.Connection = cn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "dbo.aspdnsf_updEventHandler";

					cmd.Parameters.Add(new SqlParameter("@EventID", SqlDbType.Int, 4));
					cmd.Parameters.Add(new SqlParameter("@EventName", SqlDbType.NVarChar, 20));
					cmd.Parameters.Add(new SqlParameter("@CalloutURL", SqlDbType.VarChar, 200));
					cmd.Parameters.Add(new SqlParameter("@XMLPackage", SqlDbType.VarChar, 100));
					cmd.Parameters.Add(new SqlParameter("@Active", SqlDbType.Bit));
					cmd.Parameters.Add(new SqlParameter("@Debug", SqlDbType.Bit));
					cmd.Parameters["@EventID"].Value = EventID;

					if(EventName.Trim() == null)
						cmd.Parameters["@EventName"].Value = DBNull.Value;
					else
						cmd.Parameters["@EventName"].Value = EventName;

					if(CalloutURL == null)
						cmd.Parameters["@CalloutURL"].Value = DBNull.Value;
					else
						cmd.Parameters["@CalloutURL"].Value = CalloutURL;

					if(XMLPackage == null)
						cmd.Parameters["@XmlPackage"].Value = DBNull.Value;
					else
						cmd.Parameters["@XmlPackage"].Value = XMLPackage;

					if(Active == null)
						cmd.Parameters["@Active"].Value = DBNull.Value;
					else
						cmd.Parameters["@Active"].Value = Active;

					if(Debug == null)
						cmd.Parameters["@Debug"].Value = DBNull.Value;
					else
						cmd.Parameters["@Debug"].Value = Debug;

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

        #endregion

        public string Update(SqlParameter[] spa)
        {
            return AspdnsfEventHandler.Update(this.EventID, spa);
        }

        public string Update(string EventName, string CalloutURL, string XmlPackage, object Active, object Debug)
        {
            string err = String.Empty;
            try
            {
                err = Update(this.m_EventID, EventName, CalloutURL, XmlPackage, Active,Debug);
                if (err == "")
                {
                    m_EventName = CommonLogic.IIF(EventName != null, EventName, m_EventName);
                    m_CalloutURL = CommonLogic.IIF(CalloutURL != null, CalloutURL, m_CalloutURL);
                    m_XmlPackage = CommonLogic.IIF(XmlPackage != null, XmlPackage, m_XmlPackage);
                    if (Active != null)
                    {
                        m_Active = (bool)Active;
                    }
                    if (Debug != null)
                    {
                        m_Debug = (bool)Debug;
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            return err;

        }

		# region AspDotNetStorefront Event Handlers

		public void CallEvent(string runtimeParams)
        {
			if (m_Active)
			{ 
				try
				{
					Customer customer = HttpContext.Current.GetCustomer();
					string calloutUrl = this.CalloutURL.Trim();

					if (!String.IsNullOrEmpty(calloutUrl))
					{
						Uri uri = new Uri(calloutUrl);
						string content = AppLogic.RunXmlPackage(this.m_XmlPackage, null, customer, 1, String.Empty, runtimeParams, false, false);

						Task.Run(() => PostEventAsync(uri.AbsoluteUri, content, m_Debug));
					}
				}
				catch (Exception ex)
				{
					SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
				}
			}
        }

		public void PostEventAsync(string uri, string content, bool debug)
		{
			var response = String.Empty;
			try
			{
				using(var httpClient = new HttpClient())
				{
					response = httpClient
						.PostAsync(uri, new StringContent(content))
						.ContinueWith(task => task
							.Result
							.EnsureSuccessStatusCode()
							.Content.ReadAsStringAsync())
						.Unwrap()
						.Result;
				}

				if(debug)
				{
					var detail = new StringBuilder();
					detail.AppendFormat("URI: {0} ", uri);
					detail.AppendFormat("Request: {0} ", content);
					detail.AppendFormat("Response: {0} ", response);

					SysLog.LogMessage(m_EventName, detail.ToString(), MessageTypeEnum.Informational, MessageSeverityEnum.Message);
				}
			}
			catch(Exception exception)
			{
				SysLog.LogMessage("Event Handler Exception", exception.ToString(), MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
			}
		}

		#endregion

		#region public properties

		public int EventID
        {
            get { return m_EventID; }
        }

        public string CalloutURL
        {
            get { return m_CalloutURL; }
            set
            {

                SqlParameter sp1 = new SqlParameter("@CalloutURL", SqlDbType.VarChar, 200);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_CalloutURL = value;
                }
            }
        }

        public string EventName
        {
            get { return m_EventName; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@EventName", SqlDbType.NVarChar, 20);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_EventName = value;
                }
            }
        }

        #endregion
    }
	
    public class AspdnsfEventHandlers : IEnumerable
    {
        public SortedList m_EventHandlers;

        public AspdnsfEventHandlers()
        {
            m_EventHandlers = new SortedList();

            using (var dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (var rs = DB.GetRS("aspdnsf_getEventHandler", dbconn))
                {
                    while (rs.Read())
                    {
                        m_EventHandlers.Add(DB.RSField(rs, "EventName").ToLowerInvariant(), new AspdnsfEventHandler(DB.RSFieldInt(rs, "EventID"), DB.RSField(rs, "EventName"), DB.RSField(rs, "CalloutURL"), DB.RSField(rs, "XmlPackage"), DB.RSFieldBool(rs, "Active"), DB.RSFieldBool(rs, "Debug")));
                    }
                }
            }
        }

        public AspdnsfEventHandler this[string eventname]
        {
            get
            {
                return (AspdnsfEventHandler)m_EventHandlers[eventname.ToLowerInvariant()];
            }
        }

        public AspdnsfEventHandler this[int eventid]
        {
            get
            {
                SortedList syncdSL = SortedList.Synchronized(m_EventHandlers);

                for (int i = 0; i < syncdSL.Count; i++)
                {
                    if (((AspdnsfEventHandler)syncdSL.GetByIndex(i)).EventID == eventid)
                    {
                        return (AspdnsfEventHandler)syncdSL.GetByIndex(i);
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Adds an existing EventHandlerClass object to the collection
        /// </summary>
        public void Add(AspdnsfEventHandler eventhandler)
        {
            m_EventHandlers.Add(eventhandler.EventName.ToLowerInvariant(), eventhandler);
        }

        /// <summary>
        /// Creates a new EventHandler record and adds it to the collection
        /// </summary>
        public void Add(string EventName, string CalloutURL, string XmlPackage, bool Active,bool Debug)
        {
            this.Add(AspdnsfEventHandler.Create(EventName, CalloutURL.Trim(), XmlPackage.Trim(), Active,Debug));
        }

        public IEnumerator GetEnumerator()
        {
            return new EventHandlersEnumerator(this);
        }
    }

    public class EventHandlersEnumerator : IEnumerator
    {
        private int position = -1;
        private AspdnsfEventHandlers m_eventhandlers;

        public EventHandlersEnumerator(AspdnsfEventHandlers eventhandlerscol)
        {
            this.m_eventhandlers = eventhandlerscol;
        }

        public bool MoveNext()
        {
            if (position < m_eventhandlers.m_EventHandlers.Count - 1)
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
                return m_eventhandlers.m_EventHandlers[position];
            }
        }
    }
}
