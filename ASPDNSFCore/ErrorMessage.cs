// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
    public class ErrorMessage
    {
        private int _MessageId = 0;
        private String _Message;

        public int MessageId
        {
            get { return _MessageId; }
        }
        public String Message
        {
            get { return _Message; }
        }


        public ErrorMessage(int Id)
        {
            if (!LoadFromDb("MessageId = " + Id))
            {
                _MessageId = 0;
                _Message = String.Empty;
            }
        }

        public ErrorMessage(String Message)
        {
            if (!LoadFromDb("Message = " + DB.SQuote(Message)))
                this.CreateMessage(Message);
        }

        private void CreateMessage(string Message)
        {
            Guid uid = Guid.NewGuid();
            String sql = String.Format("INSERT INTO [ErrorMessage]([Message],[MessageGuid])VALUES({0},{1})", DB.SQuote(Message), DB.SQuote(uid.ToString()));
            DB.ExecuteSQL(sql);
            if (!LoadFromDb("MessageGuid = " + DB.SQuote(uid.ToString())))
                throw new ArgumentException("There was a problem creating this error message.");
        }

        private Boolean LoadFromDb(string WhereClause)
        {
            using (var conn = DB.dbConn())
            {
                conn.Open();
                using (var rs = DB.GetRS("select MessageId, Message, MessageGuid from ErrorMessage where " + WhereClause, conn))
                {
                    if (rs.Read())
                    {
                        this._Message = DB.RSField(rs, "Message");
                        this._MessageId = DB.RSFieldInt(rs, "MessageId");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
