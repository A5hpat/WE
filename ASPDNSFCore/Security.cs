// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using AspDotNetStorefrontEncrypt;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for Security: localization of password storage and comparision functions.
	/// </summary>
	public class Security
    {
		public const string KeyEncryptionKeyName = "KeyEncryptionKey";
		public const string TertiaryEncryptionKeyName = "TertiaryEncryptionKey";

		public struct SecurityParams
        {
            public String EncryptKey;
            public int KeySize;
            public int EncryptIterations;
            public String InitializationVector;
            public String HashAlgorithm;
            public SecurityParams(String p_EncryptKey, int p_KeySize, int p_EncryptIterations, String p_InitializationVector, String p_HashAlgorithm)
            {
                EncryptKey = p_EncryptKey;
                KeySize = p_KeySize;
                EncryptIterations = p_EncryptIterations;
                InitializationVector = p_InitializationVector;
                HashAlgorithm = p_HashAlgorithm;
            }
        }

        public static readonly int ro_SaltKeyIsInvalid = 0;
        public static readonly String ro_PasswordDefaultTextForAnon = String.Empty;
        public static readonly String ro_DecryptFailedPrefix = "Error.";

        public enum CryptTypeEnum
        {
            V1 = 0,
            V2 = 1
        }

        /// <summary>
        /// Generates a random key of the specified length
        /// </summary>
        /// <param name="length">length of the key (in bytes) to generate</param>
        /// <returns></returns>
        public static string CreateRandomKey(int length)
        {
            //We'll convert this to hex, so the length must be double
            StringBuilder key = new StringBuilder(length * 2);
            byte[] ba = new byte[length];

            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();

            rand.GetBytes(ba);

            foreach (byte b in ba)
            {
                key.Append(String.Format("{0:X2}", b));
            }

            return key.ToString();
        }


        /// <summary>
        /// Creates a random key with a random length (in bytes)
        /// Since this is converted to Hexidecimal, the key length (in characters) will be double the specified values
        /// </summary>
        /// <param name="minLength">Minimum length of the key to generate.</param>
        /// <param name="maxLength">Maximum length of the key to generate.</param>
        /// <returns></returns>
        public static string CreateRandomKey(int minLength, int maxLength)
        {
            int minimum = minLength;
            int maximum = maxLength;

            if (minimum > maximum)
            {
                //Someone goofed up.  Swap the numbers.
                minimum = maxLength;
                maximum = minLength;
            }

            return CreateRandomKey(new Random().Next(minimum, maximum));
        }

        public static string ConvertToHex(string input)
        {
            StringBuilder converted = new StringBuilder(input.Length * 2);
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] ba = enc.GetBytes(input);

            foreach (byte b in ba)
            {
                converted.Append(String.Format("{0:X2}", b));
            }
            
            return converted.ToString();
        }

        public static String GetMD5Hash(String s)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            Byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(s));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

		public static String GetEncryptParam(String ParamName)
		{
			var param = string.Empty;
			if(ParamName == "EncryptKey")
			{
				// Pull the encryption key from appsettings.
				param = CommonLogic.Application(ParamName);

				// The EncryptKey AppSetting will either be plain text from a merchant manually setting the value
				//  or it will be encrypted from setting it via the Change Encrypt Key in admin. If it was set through
				//  the admin, a KEK and a TEK will have been generated. Bypass decryption if neither are set.
				var keyEncryptionKeyConfig = GlobalConfig.GetGlobalConfig(KeyEncryptionKeyName);
				var tertiaryEncryptionKey = ConfigurationManager.AppSettings[TertiaryEncryptionKeyName];

				if(keyEncryptionKeyConfig != null
					&& !string.IsNullOrWhiteSpace(keyEncryptionKeyConfig.ConfigValue)
					&& !string.IsNullOrWhiteSpace(tertiaryEncryptionKey))
				{
					var encryptIterations = AppLogic.AppConfigNativeInt("EncryptIterations");
					var hashAlgorithm = AppLogic.AppConfig("HashAlgorithm");
					var initializationVector = AppLogic.AppConfig("InitializationVector");
					var keySize = AppLogic.AppConfigNativeInt("KeySize");

					// We found the KEK and the TEK, decrypt the KEK using the TEK.
					var decryptedKEK = Security.UnmungeString(keyEncryptionKeyConfig.ConfigValue, string.Empty, new Security.SecurityParams
						{
							EncryptIterations = encryptIterations,
							EncryptKey = tertiaryEncryptionKey,
							HashAlgorithm = hashAlgorithm,
							InitializationVector = initializationVector,
							KeySize = keySize
						});

					// Decrypt the Encrypt Key using the decrypted KEK.
					param = Security.UnmungeString(param, string.Empty, new Security.SecurityParams
						{
							EncryptIterations = encryptIterations,
							EncryptKey = decryptedKEK,
							HashAlgorithm = hashAlgorithm,
							InitializationVector = initializationVector,
							KeySize = keySize
						});
				}
			}
			else
            {
                param = AppLogic.AppConfig(ParamName);
            }

            // now do validation!
            if (ParamName == "EncryptKey")
            {
                if (param.Length == 0 || param == "WIZARD" ||
                    param == AppLogic.ro_TBD)
                {
                    throw new ArgumentException("You must enter your EncryptKey in the /AppSettings.config file!!! Review documentation for instructions.");
                }
            }

            if (ParamName == "EncryptIterations")
            {
                if (param.Length == 0 && !CommonLogic.IsInteger(param) && Convert.ToInt32(param) >= 1 &&
                    Convert.ToInt32(param) <= 4)
                {
                    throw new ArgumentException("The EncryptIterations parameter must be an integer value between 1 and 4.");
                }
            }

            if (ParamName == "InitializationVector")
            {
                if (param.Length == 0 || param == AppLogic.ro_TBD ||
                    param.Length != 16)
                {
                    throw new ArgumentException("You MUST set your InitializationVector in the AppConfig manager in the admin site! it MUST be exactly 16 characters/digits long. This is required for security reasons.");
                }
            }

            if (ParamName == "KeySize")
            {
                if (param.Length == 0 || param == "0" ||
                    (param != "128" && param != "192" && param != "256"))
                {
                    throw new ArgumentException("You MUST set your KeySize value in the AppConfig manager in the admin site to an allowed valid value! This is required for security reasons.");
                }
            }

            if (ParamName == "HashAlgorithm")
            {
                if (param.Length == 0 ||
                    (param != "MD5" && param != "SHA1"))
                {
                    throw new ArgumentException("You MUST set your HashAlgorithm in the AppConfig manager in the admin site to an allowed valid value! This is required for security reasons.");
                }
            }

            return param;
        }

        public static void SetEncryptParam(String ParamName, String ParamValue)
        {
            if (ParamName == "EncryptKey")
            {
                // do not use "~", seems to be a read-only accessor. Have to use the ApplicationPath to get a read-write copy.
                // Also requires that the user for the worker process (NETWORK USER, ASPNET etc..) be granted read/write to the web.config file
                System.Configuration.Configuration config =
                    WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
                AppSettingsSection appsettings = (AppSettingsSection) config.GetSection( "appSettings" );
                appsettings.Settings[ ParamName ].Value = ParamValue;
                appsettings.SectionInformation.ForceSave = true;
                config.Save( ConfigurationSaveMode.Full );

            }
            else
            {
                try
                {
                    AppConfigManager.SetAppConfigValue(ParamName, ParamValue);
                }
                catch
                {
                    throw new ArgumentException("You do not have a required Security AppConfig [" + ParamName + "] parameter in your database. Please run the latest database upgrade script, and restart your website!!");
                }
            }
        }
		
        public static SecurityParams GetSecurityParams()
        {
            SecurityParams p;
            p.EncryptKey = GetEncryptParam("EncryptKey");
            p.HashAlgorithm = GetEncryptParam("HashAlgorithm");
            p.InitializationVector = GetEncryptParam("InitializationVector");
            p.KeySize = Int32.Parse(GetEncryptParam("KeySize"));
            p.EncryptIterations = Int32.Parse(GetEncryptParam("EncryptIterations"));
            if (p.EncryptIterations == 0)
            {
                p.EncryptIterations = 1;
            }
            return p;
        }

        public static void UpgradeEncryption()
        {
			// address table info:
			var SaltField = AppLogic.AppConfig("AddressCCSaltField");
            if (SaltField.Length == 0)
            {
                throw new ArgumentException("You MUST set AppConfig:AddressCCSaltField to a valid defined value. Please see the description of that AppConfig! This is required for security reasons.");
            }

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("select " + SaltField + " ,CardNumber from Address where Crypt=" + ((int)CryptTypeEnum.V1).ToString() + " and CardNumber IS NOT NULL and convert(nvarchar(100),CardNumber)<>''", con))
                {
                    while (rs.Read())
                    {
                        String CN = DB.RSField(rs, "CardNumber").Trim();
                        if (CN.Length != 0 &&
                            CN != AppLogic.ro_CCNotStoredString)
                        {
                            CN = UnmungeStringOld(CN);
                            if (CN.StartsWith(ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                CN = DB.RSField(rs, "CardNumber");
                            }
                            if (CN.Trim().Length != 0)
                            {
                                DB.ExecuteSQL("update Address set Crypt=" + ((int)CryptTypeEnum.V2).ToString() + ", CardNumber=" + DB.SQuote(MungeString(CN, rs[SaltField].ToString())) + " where AddressID=" + DB.RSFieldInt(rs, "AddressID").ToString());
                            }
                        }
                    }
                }
            }

            DB.ExecuteSQL("update Address set Crypt=" + ((int)CryptTypeEnum.V2).ToString() + " where Crypt < " + ((int)CryptTypeEnum.V2).ToString());

            // orders table info:
            SaltField = AppLogic.AppConfig("OrdersCCSaltField");
            if (SaltField.Length == 0)
            {
                throw new ArgumentException("You MUST set AppConfig:OrdersCCSaltField to a valid defined value. Please see the description of that AppConfig! This is required for security reasons.");
            }

            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("select OrderNumber," + SaltField + " as SaltField,CardNumber from Orders where Crypt=" + ((int)CryptTypeEnum.V1).ToString() + " and CardNumber IS NOT NULL and convert(nvarchar(100),CardNumber)<>''", con))
                {
                    while (rs.Read())
                    {
						var CN = DB.RSField(rs, "CardNumber").Trim();
                        if (CN.Length != 0 &&
                            CN != AppLogic.ro_CCNotStoredString)
                        {
                            CN = UnmungeStringOld(CN);
                            if (CN.StartsWith(ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                CN = DB.RSField(rs, "CardNumber");
                            }
                            if (CN.Trim().Length != 0)
                            {
                                DB.ExecuteSQL("update Orders set Crypt=" + ((int)CryptTypeEnum.V2).ToString() + ", CardNumber=" + DB.SQuote(MungeString(CN, rs["SaltField"].ToString())) + " where OrderNumber=" + DB.RSFieldInt(rs, "OrderNumber").ToString());
                            }
                        }
                    }
                }
            }

            DB.ExecuteSQL("update Orders set Crypt=" + ((int)CryptTypeEnum.V2).ToString() + " where Crypt < " + ((int)CryptTypeEnum.V2).ToString());
        }

        /// <summary>
        /// Provides a method for scrubbing string values for anything that appears to look like a card number.
        /// The string that is returned will replace any matches with "OMMITTED"
        /// </summary>
        /// <param name="s">The string to scrub</param>
        /// <returns></returns>
        public static string ScrubCCNumbers(String unclean)
        {
            String clean = String.Empty;
            String matchPattern = @"\b(?:\d[ -]*[A-z]*\W*){13,16}\b";
            Regex rgx = new Regex(matchPattern, RegexOptions.IgnoreCase);

            clean = rgx.Replace(unclean, "OMMITTED", unclean.Length);

            // Clean up any numbers that may remain in memory
            unclean = MungeString(unclean);
            unclean = String.Empty;


            return clean;
        }

        public static String MungeString(String s)
        {
            return MungeString(s, String.Empty, GetSecurityParams());
        }

        public static String MungeString(String s, SecurityParams p)
        {
            return MungeString(s, String.Empty, p);
        }

        public static String MungeString(String s, String SaltKey)
        {
            return MungeString(s, SaltKey, GetSecurityParams());
        }

        public static string MungeString(string value, string saltKey, SecurityParams securityParams)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var encrypt = new Encrypt(
				passPhrase: securityParams.EncryptKey,
				initVector: securityParams.InitializationVector, 
				minSaltLen: 4, 
				maxSaltLen: 4, 
				keySize: securityParams.KeySize, 
				hashAlgorithm: securityParams.HashAlgorithm, 
				saltValue: saltKey,
				passwordIterations: securityParams.EncryptIterations);

            return encrypt.EncryptData(value);
        }

        public static String UnmungeString(String s)
        {
            return UnmungeString(s, String.Empty, GetSecurityParams());
        }

        public static String UnmungeString(String s, String SaltKey)
        {
            return UnmungeString(s, SaltKey, GetSecurityParams());
        }

        public static String UnmungeString(String s, String SaltKey, SecurityParams p)
        {
            if (s.Length == 0)
            {
                return s;
            }
            try
            {
                Encrypt e = new Encrypt(p.EncryptKey, p.InitializationVector, 4, 4, p.KeySize, p.HashAlgorithm, SaltKey, p.EncryptIterations);
                String tmpS = e.DecryptData(s);
                return tmpS;
            }
            catch
            {
                //return "Error: Decrypt Failed";
                // to make sure when comparing the StartsWith
                return ro_DecryptFailedPrefix + " Decrypt Failed";
            }
        }

        public static void ConvertAllPasswords()
        {
            // customer table:     
            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("select CustomerID,Password from Customer  with (NOLOCK)  where SaltKey in (-1,-2)", con))
                {
                    while (rs.Read())
                    {
                        if (DB.RSField(rs, "Password").Length != 0 ||
                            DB.RSField(rs, "Password") != ro_PasswordDefaultTextForAnon)
                        {
							var PWD = UnmungeStringOld(DB.RSField(rs, "Password"));
                            if (PWD.StartsWith(ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // must have been in clear text:
                                PWD = DB.RSField(rs, "Password");
                            }
							var Salt = Encrypt.CreateRandomSalt();
							var p = new Password(PWD, Salt);
                            DB.ExecuteSQL("update Customer set Password=" + DB.SQuote(p.SaltedPassword) + ", SaltKey=" + Salt.ToString() + " where CustomerID=" + DB.RSFieldInt(rs, "CustomerID").ToString());
                        }
                    }
                }
            }

            // Affiliate Table:
            using (var con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (var rs = DB.GetRS("select AffiliateID,Password,SaltKey from Affiliate  with (NOLOCK)  where SaltKey in (-1,-2)", con))
                {
                    while (rs.Read())
                    {
                        if (DB.RSField(rs, "Password").Length != 0 ||
                            DB.RSField(rs, "Password") != ro_PasswordDefaultTextForAnon)
                        {
							var PWD = UnmungeStringOld(DB.RSField(rs, "Password"));
                            if (PWD.StartsWith(ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                PWD = DB.RSField(rs, "Password");
                            }
							var Salt = Encrypt.CreateRandomSalt();
							var p = new Password(PWD, Salt); // PWD in this call is still in clear text really
                            DB.ExecuteSQL("update Affiliate set Password=" + DB.SQuote(p.SaltedPassword) + ", SaltKey=" + Salt.ToString() + " where AffiliateID=" + DB.RSFieldInt(rs, "AffiliateID").ToString());
                        }
                    }
                }
            }
        }

		/// <summary>
		/// Writes an event to the encrypted system security log.
		/// This method is used for PA-DSS compliance.
		/// </summary>
		/// <param name="securityAction">Brief description of the security event</param>
		/// <param name="description">Full description of the security event</param>
		/// <param name="customerUpdated">The customer ID which the event targeted (eg. password change event)</param>
		/// <param name="updatedBy">The ID of the initiating customer or administrator</param>
		/// <param name="customerSessionId">The session ID of the initiating customer or administrator</param>
		/// <param name="securityParameters">Security parameter object allowing the developer to control how strings are encrypted</param>
		public static void LogEvent(string securityAction, string description, int customerUpdated, int updatedBy, int customerSessionId, SecurityParams? securityParameters = null)
		{
			var effectiveSecurityParameters = securityParameters ?? GetSecurityParams();

			using(var connection = new SqlConnection(DB.GetDBConn()))
			using(var command = connection.CreateCommand())
			{
				connection.Open();

				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "dbo.aspdnsf_SecurityLogInsert";
				
				command.Parameters.AddWithValue("@SecurityAction", MungeString(securityAction, effectiveSecurityParameters));
				command.Parameters.AddWithValue("@Description", MungeString(description, effectiveSecurityParameters));
				command.Parameters.AddWithValue("@CustomerUpdated", customerUpdated);
				command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
				command.Parameters.AddWithValue("@CustomerSessionID", customerSessionId);
				command.Parameters.Add(new SqlParameter("@LogID", SqlDbType.Int)
				{
					Direction = ParameterDirection.Output,
				});

				try
				{
					command.ExecuteNonQuery();
				}
				catch(Exception exception)
				{
					SysLog.LogException(exception, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
				}
			}
		}

        // helper routine just for compatibility with WSI.cs class from v7.1 in the future:
        public static String HtmlEncode(String s)
        {
            return HttpContext.Current.Server.HtmlEncode(s);
        }

        public static String UrlEncode(String s)
        {
            return HttpContext.Current.Server.UrlEncode(s);
        }

        #region OLDROUTINES

        // ---------------------------------------------------------------------------------------------------
        // OLD routines now below
        // do not use anymore
        // routines above offer increased security characteristics per PABP
        // ---------------------------------------------------------------------------------------------------

        public static String UnmungeStringOld(String s)
        {
            SecurityParams p = GetSecurityParams();
            String tmpS = new EncryptOld().DecryptData(p.EncryptKey, s);
            return tmpS;
        }

		#endregion

		/// <summary>
		/// Performs an audit and returns a list of audit issues
		/// </summary>
		public static IEnumerable<SecurityAuditItem> GetAuditIssues(HttpRequestBase request = null)
		{
			// 1. Ensure that SSL is working on the admin site.  An issue with LiveServer can cause SSL not to function.
			if(!CommonLogic.IsSecureConnection())
				yield return new SecurityAuditItem()
				{
					Message = "Your Admin site is not currently using SSL.  This can allow data being sent to and from the admin site to be read in transit.  <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=ssl' target='_blank'>Click Here</a> to learn how to enable SSL.",
					ItemType = SecurityAuditItemType.Security
				};

			// 2. Check for path element containing /admin/. We do not allow Admin sites to be located at the default /admin/ path. Too easy to guess.
			if(request != null && request.Path.IndexOf("/admin/", StringComparison.InvariantCultureIgnoreCase) != -1)
				yield return new SecurityAuditItem()
				{
					Message = "The URL to your Admin site contains '/admin/'. This does not follow our <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=securitybestpractices' target='_blank'>security best practices</a>. Please immediately rename the Admin folder, and restart the website.",
					ItemType = SecurityAuditItemType.Security
				};

			// 3. Remove or change admin@aspdotnetstorefront.com. Cannot use the default credentials long-term.
			if(new Customer("admin@aspdotnetstorefront.com").EMail == "admin@aspdotnetstorefront.com")
				yield return new SecurityAuditItem()
				{
					Message = "The default user 'admin@aspdotnetstorefront.com' is a registered user on this website. <a href='customers.aspx?filter.0.0=admin%40aspdotnetstorefront.com'>Click Here</a> to edit this user.",
					ItemType = SecurityAuditItemType.Security
				};

			// 4. Check MailMe_Server AppConfig Setting. Cannot Allow blank MailMe_Server AppConfig.
			var mailServerConfig = AppLogic.AppConfig("MailMe_Server");
			if(string.IsNullOrWhiteSpace(mailServerConfig)
				|| mailServerConfig.Equals(AppLogic.ro_TBD, StringComparison.InvariantCultureIgnoreCase)
				|| mailServerConfig.Equals("MAIL.YOURDOMAIN.COM", StringComparison.InvariantCultureIgnoreCase))
				yield return new SecurityAuditItem()
				{
					Message = "The 'MailMe_Server' Setting is not properly configured. Click <a href=\"mailingtest.aspx\">here</a> to configure mail settings.",
					ItemType = SecurityAuditItemType.Security
				};

			// 5. Check for admin\assetmanager folder. Should be deleted. 
			if(Directory.Exists(CommonLogic.SafeMapPath("assetmanager")))
				yield return new SecurityAuditItem()
				{
					Message = "The obsolete folder 'assetmanager' exists in your site's Admin folder, and can present a security risk. Please delete this folder and its contents.",
					ItemType = SecurityAuditItemType.Security
				};

			// 6. Check for match between path and AdminDir. Verify that AdminDir is set correctly.
			if(request != null && request.Path.IndexOf(string.Format("/{0}/", AppLogic.AppConfig("AdminDir")), StringComparison.InvariantCultureIgnoreCase) == -1)
				yield return new SecurityAuditItem()
				{
					Message = "The URL to your Admin site does not match the 'AdminDir' Setting, which can lead to disclosure of your Admin URL. Please update the 'AdminDir' Setting, using the 'Configuration -> Settings' menu.",
					ItemType = SecurityAuditItemType.Security
				};

			if(AppLogic.TrustLevel == AspNetHostingPermissionLevel.Unrestricted || AppLogic.TrustLevel == AspNetHostingPermissionLevel.High)
			{
				var webConfig = WebConfigurationManager.OpenWebConfiguration("~");

				// 7. Check for debug=true in web.config. Should be false on a live site.
				var compilation = (CompilationSection)webConfig.GetSection("system.web/compilation");
				if(compilation.Debug == true)
					yield return new SecurityAuditItem()
					{
						Message = "The 'compilation' element in web.config is set to 'debug=true'. Please set it to 'debug=false' to improve site performance.",
						ItemType = SecurityAuditItemType.Security
					};

				// 8. Check encryption on web.config. Must be encrypted as the last step before going Live.
				var appSettings = webConfig.GetSection("appSettings");
				if(!appSettings.SectionInformation.IsProtected)
					yield return new SecurityAuditItem()
					{
						Message = "The web.config file is not encrypted. This does not follow our <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=securitybestpractices' target='_blank'>security best practices</a>. Please use the <a href='wizard.aspx'>Site Setup Wizard</a> to encrypt the web.config file before going Live.",
						ItemType = SecurityAuditItemType.Security
					};

				// 9. Check write permissions on web.config. Must have write-permission to encrypt, then have read-only permission after encryption.
				if(FileIsWriteable(CommonLogic.SafeMapPath("~/web.config")))
					yield return new SecurityAuditItem()
					{
						Message = "The file permission on web.config allows for write-access. This does not follow our <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=securitybestpractices' target=\"_blank\">security best practices</a>. Please set the web.config file as read-only before going Live.",
						ItemType = SecurityAuditItemType.Security
					};

				// 10. Check non-write permissions on root. Cannot allow root folder to have write permission. 
				if(FolderIsWriteable(CommonLogic.SafeMapPath("~/")))
					yield return new SecurityAuditItem()
					{
						Message = "The root directory of your website allows write-access. Please remove write-access before going Live. <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=golive' target='_blank'>Read more</a>",
						ItemType = SecurityAuditItemType.Security
					};

				// 11. Check for customErrors Mode=Off in web.config. Should be RemoteOnly or On on a Live site.
				var customErrors = (CustomErrorsSection)webConfig.GetSection("system.web/customErrors");
				if(customErrors.Mode == CustomErrorsMode.Off)
					yield return new SecurityAuditItem()
					{
						Message = "The 'customErrors' element in web.config is set to 'off'. Please set it to 'on' before going Live. <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=golive' target=\"_blank\">Read more</a>",
						ItemType = SecurityAuditItemType.Security
					};
			}

			// 12. DotFeed is installed but not enabled. 
			if(AppConfigManager.AppConfigExists("DotFeed.AccessKey") && string.IsNullOrEmpty(AppLogic.AppConfig("DotFeed.AccessKey")))
				yield return new SecurityAuditItem()
				{
					Message = "DotFeed is installed but not enabled, <a href=\"https://manage.dotfeed.com\" target=\"_blank\">click here</a> to configure DotFeed",

                    ItemType = SecurityAuditItemType.Configuration
				};

			// 13. Site is using the default Search Engine Meta Title, Description, and Keywords tags.
			if(AppLogic.AppConfig("SE_MetaTitle").ContainsIgnoreCase("Enter your site title here")
				|| AppLogic.AppConfig("SE_MetaDescription").ContainsIgnoreCase("enter your site description here")
				|| AppLogic.AppConfig("SE_MetaKeywords").ContainsIgnoreCase("enter your site keywords here"))
				yield return new SecurityAuditItem()
				{
					Message = "Your site is using the default Search Engine Meta Title & Description tags, <a href=\"wizard.aspx#trSEO\">click here</a> to update.",
					ItemType = SecurityAuditItemType.Configuration
				};

			// 14. Time to change the encrypt key
			var nextKeyChangeDate = DateTime.MinValue;
			if(AppLogic.AppConfigBool("StoreCCInDB")
				&& DateTime.TryParse(AppLogic.AppConfig("NextKeyChange"), out nextKeyChangeDate))
			{
				if(nextKeyChangeDate < DateTime.Now)
					yield return new SecurityAuditItem()
					{
						Message = "Time To Change Your Encrypt Key!",
						ItemType = SecurityAuditItemType.Security
					};
			}

        }

		static bool FileIsWriteable(string path)
		{
			if(!File.Exists(path))
				return false;

			var file = new FileInfo(path);
			if(file.IsReadOnly)
				return false;

			return AccessControlAllowsRightForPrincipal(
				rules: file
					.GetAccessControl()
					.GetAccessRules(true, true, typeof(SecurityIdentifier)),
				right: FileSystemRights.Write,
				principal: WindowsIdentity.GetCurrent());
		}

		static bool FolderIsWriteable(string path)
		{
			if(!Directory.Exists(path))
				return false;

			var directory = new DirectoryInfo(path);

			return AccessControlAllowsRightForPrincipal(
				rules: directory
					.GetAccessControl()
					.GetAccessRules(true, true, typeof(SecurityIdentifier)), 
				right: FileSystemRights.Write, 
				principal: WindowsIdentity.GetCurrent());
		}

		static bool AccessControlAllowsRightForPrincipal(System.Security.AccessControl.AuthorizationRuleCollection rules, FileSystemRights right, WindowsIdentity principal)
		{
			return rules
				.Cast<FileSystemAccessRule>()
				.Where(rule => principal.User.Equals(rule.IdentityReference))			// Find the rules for the given principal
				.Where(rule => rule.FileSystemRights.HasFlag(right))					// Find the rules for the given right
				.Where(rule => !AccessControlType.Deny.Equals(rule.AccessControlType))	// If the right is explicitly denied, we don't check the rule futher
				.Where(rule => AccessControlType.Allow.Equals(rule.AccessControlType))	// If the right is explicitly allowed, then we mark it as allowed
				.Any();																	// If the right is neither allowed or denied, we treat is as denied
		}
	}

	public class SecurityAuditItem
	{
		public string Message;
		public SecurityAuditItemType ItemType;
	}

	public enum SecurityAuditItemType
	{
		Security,
		Configuration
	}

    public class Password
    {
        private String m_ClearPassword = String.Empty;
        private int m_Salt = 0;
        private String m_SaltedPassword = String.Empty;

        public static readonly int ro_RandomPasswordLength = 8;
        public static readonly int ro_RandomStrongPasswordLength = 8;

        public Password(String ClearPassword, int Salt)
        {
            m_ClearPassword = ClearPassword;
            m_Salt = Salt;
            m_SaltedPassword = Encrypt.ComputeSaltedHash(m_Salt, m_ClearPassword);
        }

        public Password(String ClearPassword)
        {
            m_ClearPassword = ClearPassword;
            m_Salt = Encrypt.CreateRandomSalt();
            m_SaltedPassword = Encrypt.ComputeSaltedHash(m_Salt, m_ClearPassword);
        }

        public String ClearPassword
        {
            get { return m_ClearPassword; }
        }

        public String SaltedPassword
        {
            get { return m_SaltedPassword; }
        }

        public int Salt
        {
            get { return m_Salt; }
        }
    }

    // creates a random password with random salt, and hashes it!
    // this is NOT guaranteed to be a strong password!!
    // if you need a Strong Password use RandomStrongPassword class!!
    public class RandomPassword : Password
    {
        public RandomPassword() : base(Encrypt.CreateRandomPassword(ro_RandomPasswordLength, CommonLogic.IIF(AppLogic.AppConfig("NewPwdAllowedChars").Length == 0, @"abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ23456789~!@#$%&*()_-={}[]\\|;:\,./?", AppLogic.AppConfig("NewPwdAllowedChars"))), Encrypt.CreateRandomSalt())
        {
        }
    }

    public class RandomStrongPassword : Password
    {
        public RandomStrongPassword() : base(Encrypt.CreateRandomStrongPassword(ro_RandomStrongPasswordLength), Encrypt.CreateRandomSalt())
        {
        }
    }
}
