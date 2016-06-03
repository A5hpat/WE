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
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

	public partial class versioninfo : AspDotNetStorefront.Admin.AdminPageBase
	{

		#region PRIVATE VARIABLES

		private Customer cust;
		private int m_SkinID;
		private bool excludeMISCFolders = false;

		#endregion

		#region PAGE EVENTS

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			cust = Context.GetCustomer();
			m_SkinID = cust.SkinID;
			this.Title = "Version Info";
			excludeMISCFolders = CommonLogic.QueryStringBool("excludeImages");
			InitPatchInformation();
			loadSystemInformation();
			litFileInfo.Text = loadFileInformation();
			litAssemblyInfo.Text = loadAssemblyInformation();
			litConfigInfo.Text = loadConfigInformation();
		}

		#endregion

		#region RENDER REPORT METHODS

		private void InitPatchInformation()
		{
			//Load version xmlpackages
			ArrayList xmlPackages = LocalReadAdminXmlPackages("versioninfo");
			StringBuilder sb = new StringBuilder();
			bool VersionFileFound = false;
			foreach(String s in xmlPackages)
			{
				VersionFileFound = true;
				var package = new XmlPackage(
					packageName: s,
					skinId: SkinID);
				sb.Append(package.TransformString());
			}
			if(VersionFileFound)
			{
				litPatchInfo.Text = sb.ToString();
				pnlPatchInfo.Visible = true;
			}
		}

		static public ArrayList LocalReadAdminXmlPackages(String PackageFilePrefix) //This method is duplicated in SP1 Applogic as "ReadAdminXmlPackages". We moved it here to provide the same functionality in the Admin Service Pack
		{
			// create an array to hold the list of files
			ArrayList fArray = new ArrayList();

			// now check common area:
			String SFP = CommonLogic.SafeMapPath(string.Format("{0}/XmlPackages/bogus.htm", AppLogic.AdminDir())).Replace("bogus.htm", String.Empty);
			DirectoryInfo dirInfo = new DirectoryInfo(SFP);
			FileSystemInfo[] myDir = dirInfo.GetFiles(CommonLogic.IIF(PackageFilePrefix.Length != 0, PackageFilePrefix + ".*.xml.config", "*.xml.config"));
			for(int i = 0; i < myDir.Length; i++)
			{
				fArray.Add(myDir[i].ToString().ToLowerInvariant());
			}

			if(fArray.Count != 0)
			{
				// sort the files alphabetically
				fArray.Sort(0, fArray.Count, null);
			}
			return fArray;
		}

		protected void loadSystemInformation()
		{
			ltOnLiveServer.Text = AppLogic.OnLiveServer().ToString();
			ltServerPortSecure.Text = CommonLogic.IsSecureConnection().ToString();
			ltStoreVersion.Text = CommonLogic.GetVersion();
			ltWebConfigLocaleSetting.Text = Localization.GetDefaultLocale();
			ltSQLLocaleSetting.Text = Localization.GetSqlServerLocale();
			ltCustomerLocaleSetting.Text = ThisCustomer.LocaleSetting;
			ltPaymentGateway.Text = AppLogic.ActivePaymentGatewayRAW();
			ltUseLiveTransactions.Text = (AppLogic.AppConfigBool("UseLiveTransactions") ? AppLogic.GetString("admin.splash.aspx.20", m_SkinID, ThisCustomer.LocaleSetting) : AppLogic.GetString("admin.splash.aspx.21", m_SkinID, ThisCustomer.LocaleSetting));
			ltTransactionMode.Text = AppLogic.AppConfig("TransactionMode").ToString();
			ltPaymentMethods.Text = AppLogic.AppConfig("PaymentMethods").ToString();
			ltTrustLevel.Text = AppLogic.TrustLevel.ToString(); CardinalEnabled.Text = AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled").ToString();
			ltMicroPayEnabled.Text = AppLogic.MicropayIsEnabled().ToString();
			StoreCC.Text = AppLogic.StoreCCInDB().ToString();
			GatewayRecurringBilling.Text = AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling").ToString();
			ltUseSSL.Text = AppLogic.UseSSL().ToString();
			PrimaryCurrency.Text = Localization.GetPrimaryCurrency();
			ltDateTime.Text = Localization.ToNativeDateTimeString(System.DateTime.Now);
			ltExecutionMode.Text = CommonLogic.IIF(IntPtr.Size == 8, "64 Bit", "32 Bit");
		}

		private string loadAssemblyInformation()
		{
			StringBuilder ret = new StringBuilder();
			Assembly asm;
			ret.AppendLine("<table class=\"table\" style=\"table-layout:fixed;-ms-word-wrap:break-word;word-wrap:break-word;\">");
			ret.AppendLine("<tr style=\"text-align:left;\"><th>Info</th></tr>");
			string[] fileEntries = Directory.GetFiles(CommonLogic.SafeMapPath(@"~\bin\"), "*.dll");
			foreach(String dllname in fileEntries)
			{
				try
				{
					asm = Assembly.LoadFrom(dllname);
					printAssemblyInformation(asm, ret);
				}
				catch(Exception e)
				{
					SysLog.LogMessage(dllname + " failed to load.", e.ToString(), MessageTypeEnum.Informational, MessageSeverityEnum.Message);
				}
			}
			ret.AppendLine("</table>");
			return ret.ToString();
		}

		private string loadFileInformation()
		{
			StringBuilder ret = new StringBuilder();
			String SFP = CommonLogic.SafeMapPath("~/");
			DirectoryInfo dirInfo = new DirectoryInfo(SFP);
			ret.AppendLine("<table class=\"table\" style=\"table-layout:fixed;-ms-word-wrap:break-word;word-wrap:break-word;\">");
			ret.AppendLine("<tr style=\"text-align:left;\"><th>File</th><th>Modified</th><th>Checksum</th></tr>");
			PrintDirectory(dirInfo, 1, ret);
			ret.AppendLine("</table>");
			return ret.ToString();
		}

		private string loadConfigInformation()
		{
			StringBuilder ret = new StringBuilder();
			ret.AppendLine("<table class=\"table\" style=\"table-layout:fixed;-ms-word-wrap:break-word;word-wrap:break-word;\">");
			ret.AppendLine("<tr style=\"text-align:left;\"><th>Name</th><th>StoreID</th><th>ConfigValue</th><th>IsGlobal</th><th>ValueType</th><th>SuperOnly</th><th>Hidden</th></tr>");
			string rowFormat = "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td></tr>\n";
			using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(IDataReader rsst = DB.GetRS(@"select Name, 0 as StoreID, ConfigValue, 'True' as IsGlobal, ValueType, SuperOnly, Hidden from globalconfig
                                                     UNION
                                                     select  Name, StoreID, ConfigValue, 'False' as IsGlobal, ValueType, SuperOnly, Hidden from appconfig"
												   , conn))
				{
					while(rsst.Read())
					{
						ret.AppendFormat(rowFormat,
							  DB.RSField(rsst, "Name")
							, DB.RSFieldInt(rsst, "StoreID")
							, DB.RSField(rsst, "ConfigValue")
							, DB.RSField(rsst, "IsGlobal")
							, DB.RSField(rsst, "ValueType")
							, DB.RSFieldBool(rsst, "SuperOnly")
							, DB.RSFieldBool(rsst, "Hidden")
						);
					}
				}
			}
			ret.AppendLine("</table>");
			return ret.ToString();
		}

		#endregion

		#region PRINT METHODS

		private void printAssemblyInformation(Assembly a, StringBuilder appendTo)
		{
			appendTo.AppendLine("<tr>");
			appendTo.AppendLine("<td>" + a.FullName + "</td>");
			appendTo.AppendLine("</tr>");
		}

		private bool NotExcludedDirectory(string dirName)
		{
			if(
				dirName.ToLowerInvariant() == "images"
				|| dirName.ToLowerInvariant() == "radspell"
				|| dirName.ToLowerInvariant() == "_sgbak"
				|| dirName.ToLowerInvariant() == "aspnet_client"
				|| dirName.ToLowerInvariant() == "descriptions"
				|| dirName.ToLowerInvariant() == "download"
				|| dirName.ToLowerInvariant() == "radcontrols"
				|| dirName.ToLowerInvariant() == "orderdownloads"
				)
			{
				return false;
			}
			return true;
		}

		private string PrintDirectory(DirectoryInfo dirInfo, int depth, StringBuilder appendTo)
		{
			if(NotExcludedDirectory(dirInfo.Name) || !excludeMISCFolders)
			{

				DirectoryInfo[] subDirectories = dirInfo.GetDirectories();
				FileInfo[] files = dirInfo.GetFiles();
				appendTo.AppendLine("<tr class=\"dirInfo\">");
				appendTo.AppendLine("<td colspan=\"3\">");
				appendTo.AppendLine("<b class=\"directoryName\">" + PrintIndent(depth) + dirInfo.Name + "</b>");
				appendTo.AppendLine("</td>");
				appendTo.AppendLine("</tr>");
				foreach(DirectoryInfo sd in subDirectories)
				{
					PrintDirectory(sd, depth + 1, appendTo);
				}
				foreach(FileInfo fi in files)
				{
					PrintFile(fi, depth + 1, appendTo);
				}
			}
			return "";
		}

		private void PrintFile(FileInfo fi, int depth, StringBuilder appendTo)
		{
			appendTo.AppendLine("<tr class=\"fileInfo\">");
			appendTo.AppendLine("<td>");
			appendTo.AppendLine("<span class=\"fileName\">" + PrintIndent(depth) + fi.Name + "</span>");
			appendTo.AppendLine("</td>");
			appendTo.AppendLine("<td>");
			appendTo.AppendLine(fi.LastWriteTime.ToShortTimeString() + " " + fi.LastWriteTime.ToShortDateString());
			appendTo.AppendLine("</td>");
			appendTo.AppendLine("<td>");
			appendTo.AppendLine(GetMD5Checksum(fi.FullName));
			appendTo.AppendLine("</td>");
			appendTo.AppendLine("</tr>");
		}

		private string PrintIndent(int depth)
		{
			StringBuilder indent = new StringBuilder();
			for(int i = 0; i < depth; i++)
			{
				indent.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
			}
			return indent.ToString();
		}

		private string GetMD5Checksum(string fileName)
		{
			try
			{
				using(FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
				{
					MD5 md5 = new MD5CryptoServiceProvider();
					byte[] retVal = md5.ComputeHash(file);
					file.Close();

					StringBuilder sb = new StringBuilder();
					for(int i = 0; i < retVal.Length; i++)
					{
						sb.Append(retVal[i].ToString("x2"));
					}
					return sb.ToString();
				}
			}
			catch(Exception)
			{
				return "Could not calculate.";
			}

		}

		#endregion

		#region BUTTON EVENTS
		
		protected void btnExport_Click(object sender, EventArgs e)
		{
			StringBuilder export = new StringBuilder();
			export.AppendLine("<html>");
			export.AppendLine("<head>");
			//Adding in-page css because this creates a file that is stored on the user's disk and cannot find the style sheet.
			export.AppendLine(GetCSS());
			export.AppendLine("<title>" + AppLogic.GetStoreHTTPLocation(false) + " (" + AppLogic.AppConfig("StoreName") + ") Version Report</title>");
			export.AppendLine("</head>");
			export.AppendLine("<body>");
			using(StringWriter sw = new StringWriter())
			{
				using(HtmlTextWriter w = new HtmlTextWriter(sw))
				{
					divReports.RenderControl(w);
					export.AppendLine(sw.GetStringBuilder().ToString());
				}
			}
			export.AppendLine("</body>");
			export.AppendLine("</html>");

			Response.Clear();
			Response.ClearHeaders();
			Response.ClearContent();
			Response.AddHeader("content-disposition", "attachment; filename=VersionInfo.htm");
			Response.BufferOutput = false;
			//Send Report
			Response.ContentType = "text/HTML";
			Response.Write(export.ToString());
			Response.Flush();
			Response.End();
		}

		#endregion

		#region HELPER METHODS

		/// <summary>
		/// Adds table styling to saved page
		/// </summary>
		/// <returns></returns>
		protected string GetCSS()
		{
			StringBuilder strCss = new StringBuilder();
			strCss.AppendLine("<style>");
			strCss.AppendLine("table {");
			strCss.AppendLine("max-width: 100%;");
			strCss.AppendLine("background-color: transparent;");
			strCss.AppendLine("border-collapse: collapse;");
			strCss.AppendLine("border-spacing: 0;");
			strCss.AppendLine("font-size: 12px;");
			strCss.AppendLine("}");
			strCss.AppendLine("th {");
			strCss.AppendLine("text-align: left;");
			strCss.AppendLine("font-weight: bold;");
			strCss.AppendLine("}");
			strCss.AppendLine(".table {");
			strCss.AppendLine("width: 100%;");
			strCss.AppendLine("margin-bottom: 20px;");
			strCss.AppendLine("}");
			strCss.AppendLine(".table > thead > tr > th,");
			strCss.AppendLine(".table > tbody > tr > th,");
			strCss.AppendLine(".table > tfoot > tr > th,");
			strCss.AppendLine(".table > thead > tr > td,");
			strCss.AppendLine(".table > tbody > tr > td,");
			strCss.AppendLine(".table > tfoot > tr > td {");
			strCss.AppendLine("padding: 8px;");
			strCss.AppendLine("line-height: 1.428571429;");
			strCss.AppendLine("vertical-align: top;");
			strCss.AppendLine("border-top: 1px solid #dddddd;");
			strCss.AppendLine("}");
			strCss.AppendLine(".table > tbody > tr > th {");
			strCss.AppendLine("vertical-align: bottom;");
			strCss.AppendLine("border-bottom: 2px solid #dddddd;");
			strCss.AppendLine("}");
			strCss.AppendLine(".table > caption + thead > tr:first-child > th,");
			strCss.AppendLine(".table > colgroup + thead > tr:first-child > th,");
			strCss.AppendLine(".table > tbody:first-child > tr:first-child > th,");
			strCss.AppendLine(".table > caption + thead > tr:first-child > td,");
			strCss.AppendLine(".table > colgroup + thead > tr:first-child > td,");
			strCss.AppendLine(".table > thead:first-child > tr:first-child > td {");
			strCss.AppendLine("border-top: 0;");
			strCss.AppendLine("}");
			strCss.AppendLine(".table > tbody + tbody {");
			strCss.AppendLine("border-top: 2px solid #dddddd;");
			strCss.AppendLine("}");
			strCss.AppendLine(".table .table {");
			strCss.AppendLine("background-color: #ffffff;");
			strCss.AppendLine("}");
			strCss.AppendLine(".table-striped > tbody > tr:nth-child(even) > td,");
			strCss.AppendLine(".table-striped > tbody > tr:nth-child(even) > th {");
			strCss.AppendLine("background-color: #f9f9f9;");
			strCss.AppendLine("}");
			strCss.AppendLine(".table > thead > tr > .active,");
			strCss.AppendLine(".table > tbody > tr > .active,");
			strCss.AppendLine(".table > tfoot > tr > .active,");
			strCss.AppendLine(".table > thead > .active > td,");
			strCss.AppendLine(".table > tbody > .active > td,");
			strCss.AppendLine(".table > tfoot > .active > td,");
			strCss.AppendLine(".table > thead > .active > th,");
			strCss.AppendLine(".table > tbody > .active > th,");
			strCss.AppendLine(".table > tfoot > .active > th {");
			strCss.AppendLine("background-color: #f5f5f5;");
			strCss.AppendLine("}");
			strCss.AppendLine("</style>");
			return strCss.ToString();

		}		

		#endregion
	}
}
