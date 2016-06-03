// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace AspDotNetStorefrontCore
{
    #region enums

    /// <summary>
    /// Used by the database log provider to convey the severity of the message
    /// 0 = Unknown     Default value.  Developers should never explicitly use this value.
    /// 1 = Message     Informational message, such as application start completion
    /// 2 = Alert       Medium severity message, such as a potential misconfiguration
    /// 3 = Error       An unhandled exception
    /// </summary>
    public enum MessageSeverityEnum : int
    {
        Unknown = 0,
        Message = 1,
        Alert = 2,
        Error = 3
    }

    /// <summary>
    /// Used by the database log provider to convey the type of message being logged
    /// 0 = Unknown             Default value.  Developers should never explicitly use this.
    /// 1 = Informational       An informational message or alert
    /// 2 = GeneralException    A general (generic) unhandled exception
    /// 3 = DatabaseException   An exception reading from or writing to the database
    /// 4 = XMLPackageException An exception that occurred within an XMLPackage ExtensionFunction
    /// </summary>
    public enum MessageTypeEnum : int
    {
        Unknown = 0,
        Informational = 1,
        GeneralException = 2,
        DatabaseException = 3,
        XmlPackageException = 4
    }

    #endregion

    /// <summary>
    /// Class for logging informational alerts and exceptions to the database
    /// This class is NOT for use logging PA-DSS related information, such as admin login attempts
    /// </summary>
    public class SysLog
    {
        #region private variables
		
        private int m_maxEntries = AppLogic.AppConfigNativeInt("System.MaxLogEntries");
        private int m_maxDays = AppLogic.AppConfigNativeInt("System.MaxLogDays");
        private string m_message = String.Empty;
        private string m_details = String.Empty;
        private MessageTypeEnum m_messageType = MessageTypeEnum.Unknown;
        private MessageSeverityEnum m_messageSeverity = MessageSeverityEnum.Unknown;
		
        #endregion

        #region non-static methods

		/// <summary>
		/// Ages and prunes the log based on the System.MaxLogEntries and System.MaxLogDays AppConfigs
		/// </summary>
		private void Age()
		{
			var currentErrors = DB.GetSqlN("select count(*) as N from aspdnsf_SysLog");

			var earliestDayToKeep = DateTime.Now.AddDays((MaxLogDays * -1));
			DB.ExecuteSQL(string.Format("delete from aspdnsf_syslog where createdon < {0}", DB.SQuote(Localization.DateStringForDB(earliestDayToKeep))));

			//Make space for new entries
			var entriesToDelete = currentErrors - MaxLogEntries;
			if(entriesToDelete > 0)
				DB.ExecuteSQL(string.Format("delete from aspdnsf_syslog where SysLogID in (select top {0} SysLogID from aspdnsf_syslog order by createdon)", entriesToDelete + 5));
		}

		/// <summary>
		/// Commits the log entry to the database
		/// </summary>
		private void Commit()
		{
			try 
			{
				this.Age();

				DB.ExecuteStoredProcInt("aspdnsf_insSysLog", new []
				{
					DB.CreateSQLParameter("@message", SqlDbType.Text, this.Message.Length, this.Message, ParameterDirection.Input),
					DB.CreateSQLParameter("@details", SqlDbType.Text, this.Details.Length, this.Details, ParameterDirection.Input),
					DB.CreateSQLParameter("@type", SqlDbType.NVarChar, 100, this.MessageType.ToString(), ParameterDirection.Input),
					DB.CreateSQLParameter("@severity", SqlDbType.NVarChar, 100, this.MessageSeverity.ToString(), ParameterDirection.Input)
				});
			}
			catch // Never let logging crash the site!
			{
				return;
			}

		}

        #endregion
        
        #region static methods
        /// <summary>
        /// Deletes the entire contents of the event log
        /// </summary>
        public static void Clear()
        {
            DB.ExecuteSQL("DELETE from aspdnsf_SysLog");
        }

        /// <summary>
        /// Takes an exception and, if logging is enabled, logs it to the database
        /// </summary>
        /// <param name="ex">The exception object</param>
        /// <param name="messageType">The type of message being logged</param>
        /// <param name="messageSeverity">The severity of the message being logged</param>
        public static void LogException(Exception ex, MessageTypeEnum messageType, MessageSeverityEnum messageSeverity)
        {
            try
            {
                String details = FormatExceptionForLog(ex);

                LogMessage(ex.Message, details, messageType, messageSeverity);
            }
            catch // Never let logging crash the site!
            {
                return;
            }

        }

		/// <summary>
		/// Takes a message or alert, and if logging is enabled, logs it to the database
		/// For logging exceptions, SysLog.LogException should be used to capture the details of the exception
		/// </summary>
		/// <param name="message">A general description of the message being logged</param>
		/// <param name="details">Detailed information about the message</param>
		/// <param name="messageType">The type of message being logged</param>
		/// <param name="messageSeverity">The severity of the message being logged</param>
		public static void LogMessage(string message, string details, MessageTypeEnum messageType, MessageSeverityEnum messageSeverity)
		{
			try
			{
				if (!AppLogic.AppConfigBool("System.LoggingEnabled"))
					return;

				if(!CommonLogic.StringInCommaDelimitedStringList("database", AppLogic.AppConfig("System.LoggingLocation")))
					return;
				
				new SysLog
				{
					Message = System.Web.HttpUtility.HtmlEncode(Security.ScrubCCNumbers(message)),
					Details = System.Web.HttpUtility.HtmlEncode(Security.ScrubCCNumbers(details)),
					MessageType = messageType,
					MessageSeverity = messageSeverity
				}.Commit();
			}
			catch // Never let logging crash the site!
			{
				return;
			}
		}

        /// <summary>
        /// Formats and extracts details from an exception for log entry
        /// </summary>
        /// <param name="ex">The exception to format</param>
        /// <returns></returns>
        public static string FormatExceptionForLog(Exception ex)
        {
            try  // Never let logging crash the site!
            {
                StringBuilder details = new StringBuilder();

                try // In case returning the page name fails, still log the rest of the data
                {
                    details.Append("Page URL:" + Security.ScrubCCNumbers(CommonLogic.GetThisPageName(true)) + "\r\n");
                }
                catch { }

                if (ex.InnerException != null)
                {
                    details.Append("Source:" + ex.InnerException.Source + "\r\n");
                    details.Append("Message:" + ex.InnerException.Message + "\r\n");
                    details.Append("Stack Trace: " + "\r\n" + ex.InnerException.StackTrace + "\r\n");
                }
                else
                {
                    details.Append("Source: " + ex.Source + "\r\n");
                    details.Append("Message: " + ex.Message + "\r\n");
                    details.Append("Stack Trace: " + "\r\n" + ex.StackTrace + "\r\n");
                }
                return details.ToString();
            }
            catch
            {
                return String.Empty;
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Returns the max number of log entries to store.  This property is read-only.
        /// </summary>
        public int MaxLogEntries
        {
            get { return m_maxEntries; }
        }

        /// <summary>
        /// Returns the max number of days to store log entries.  This property is read-only.
        /// </summary>
        public int MaxLogDays
        {
            get { return m_maxDays; }
        }

        /// <summary>
        /// Gets or sets the message for this log entry.
        /// </summary>
        public string Message
        {
            get { return m_message; }
            set { m_message = value; }
        }

        /// <summary>
        /// Gets or sets the details for this log entry
        /// </summary>
        public string Details
        {
            get {return m_details;}
            set { m_details = value; }
        }

        /// <summary>
        /// Gets or sets the message type for this log entry.
        /// </summary>
        public MessageTypeEnum MessageType
        {
            get { return m_messageType; }
            set { m_messageType = value; }
        }

        /// <summary>
        /// Returns the severity of this log entry.
        /// </summary>
        public MessageSeverityEnum MessageSeverity
        {
            get { return m_messageSeverity; }
            set { m_messageSeverity = value; }
        }

        #endregion
    }
}
