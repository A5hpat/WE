// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Xml;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

	public partial class stringresourcerpt : AspDotNetStorefront.Admin.AdminPageBase
	{
		private string ShowLocaleSetting = Localization.CheckLocaleSettingForProperCase(CommonLogic.QueryStringCanBeDangerousContent("ShowLocaleSetting"));
		String ReportType = "missing";

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			ReportType = CommonLogic.QueryStringCanBeDangerousContent("ReportType").Trim();

			if(ReportType.Equals("missing", StringComparison.InvariantCultureIgnoreCase) == false && ReportType.Equals("modified", StringComparison.InvariantCultureIgnoreCase) == false)
			{
				ReportType = "missing";
			}

			if(ReportType == "missing")
			{
				Page.Header.Title = string.Format("{0} - {1}", AppLogic.GetString("admin.stringreport.ResourceReport", ThisCustomer.LocaleSetting), AppLogic.GetString("admin.stringreport.missing", ThisCustomer.LocaleSetting));
			}
			else
			{
				Page.Header.Title = string.Format("{0} - {1}", AppLogic.GetString("admin.stringreport.ResourceReport", ThisCustomer.LocaleSetting), AppLogic.GetString("admin.stringreport.modified", ThisCustomer.LocaleSetting));
			}

			if(!IsPostBack)
			{
				if(ReportType == "missing")
				{
					ltLocale.Text = string.Format("{0} {1} {2} <a href=\"{3}?filterlocale={4}\">{5}</a>", AppLogic.GetString("admin.stringreport.LocaleOneFirst", ThisCustomer.LocaleSetting), ShowLocaleSetting, AppLogic.GetString("admin.stringreport.LocaleOneSecond", ThisCustomer.LocaleSetting), AppLogic.AdminLinkUrl("stringresources.aspx"), ShowLocaleSetting, AppLogic.GetString("admin.stringreport.BackText", ThisCustomer.LocaleSetting));
					ReportLabel.Text = AppLogic.GetString("admin.stringreport.MissingStrings", ThisCustomer.LocaleSetting);
				}
				else
				{
					ltLocale.Text = string.Format("{0} {1} {2} <a href=\"{3}?filterlocale={4}\">{5}</a>", AppLogic.GetString("admin.stringreport.LocaleTwoFirst", ThisCustomer.LocaleSetting), ShowLocaleSetting, AppLogic.GetString("admin.stringreport.LocaleTwoSecond", ThisCustomer.LocaleSetting), AppLogic.AdminLinkUrl("stringresources.aspx"), ShowLocaleSetting, AppLogic.GetString("admin.stringreport.BackText", ThisCustomer.LocaleSetting));
					ReportLabel.Text = AppLogic.GetString("admin.stringreport.ModifiedStrings", ThisCustomer.LocaleSetting);
				}
				loadData();
			}
			Page.Form.DefaultButton = btnSubmit.UniqueID;
		}

		protected void loadData()
		{
			bool hasRecordsFound = false;
			string query = string.Empty;

			if(ReportType == "missing")
			{
				btnSubmit.Visible = true;
				btnSubmitBottom.Visible = true;
				topButtonPanel.Visible = true;
				bottomButtonPanel.Visible = true;
				query = "select A.Name, A.ConfigValue from (select * from StringResource where LocaleSetting=" + DB.SQuote("en-US") + ") A left join (select * from StringResource where LocaleSetting=" + DB.SQuote(ShowLocaleSetting) + ") B on A.Name=B.Name WHERE B.LocaleSetting is null order by A.Name";
			}
			else
			{
				btnSubmit.Visible = false;
				btnSubmitBottom.Visible = false;
				query = "select A.ConfigValue from StringResource A where LocaleSetting=" + DB.SQuote(ShowLocaleSetting) + " and Modified=1 order by A.Name";
			}

			using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(IDataReader rs = DB.GetRS(query, con))
				{
					ltData.Text = "<table class=\"table wide-table-fix\">";
					ltData.Text += "<tr><th>" + AppLogic.GetString("admin.stringreport.ResourceName", ThisCustomer.LocaleSetting) + "</th>";
					if(ReportType == "missing")
					{
						ltData.Text += "<th>" + AppLogic.GetString("admin.stringreport.EnUs", ThisCustomer.LocaleSetting) + "</th>";
					}
					ltData.Text += "<th>" + ShowLocaleSetting + " " + AppLogic.GetString("admin.common.Value", ThisCustomer.LocaleSetting) + "</th></tr>";
					bool data = false;
					bool alt = false;

					while(rs.Read())
					{
						hasRecordsFound = true;
						ltData.Text += "<tr><td>";
						ltData.Text += DB.RSField(rs, "Name");
						ltData.Text += "</td>";
						if(ReportType == "missing")
						{
							ltData.Text += "<td>";
							ltData.Text += DB.RSField(rs, "ConfigValue");
							ltData.Text += "</td>";
							ltData.Text += "<td>";
							ltData.Text += "<input type=\"text\" class=\"text-lg\" maxlength=\"500\" id=\"" + DB.RSField(rs, "Name") + "\" name=\"" + DB.RSField(rs, "Name") + "\">";
							ltData.Text += "</td>";
						}
						else
						{
							ltData.Text += "<td>";
							ltData.Text += DB.RSField(rs, "ConfigValue");
							ltData.Text += "</td>";
						}
						ltData.Text += "</tr>";
						alt = !alt;
						data = true;
					}

					if(!data)
					{
						if(ReportType == "missing")
						{
							ltData.Text += ("<tr><td>" + AppLogic.GetString("admin.stringreport.NoneMissing", ThisCustomer.LocaleSetting) + "</td></tr>");
						}
						else
						{
							ltData.Text += ("<tr><td>" + AppLogic.GetString("admin.stringreport.NoneModified", ThisCustomer.LocaleSetting) + "</td></tr>");
						}
						btnSubmit.Visible = false;
						btnSubmitBottom.Visible = false;
						if(hasRecordsFound == false)
						{
							topButtonPanel.Visible = false;
							bottomButtonPanel.Visible = false;
						}
					}
					ltData.Text += ("</table>");
				}
			}

			btnExportExcel.Visible = hasRecordsFound;
			btnExportExcelBottom.Visible = hasRecordsFound;
			if(hasRecordsFound == true)
			{
				topButtonPanel.Visible = true;
				bottomButtonPanel.Visible = true;
			}
		}

		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			for(int i = 0; i < HttpContext.Current.Request.Form.Count; i++)
			{
				bool okField = false;
				if(HttpContext.Current.Request.Form.Keys[i].IndexOf(".", StringComparison.InvariantCultureIgnoreCase) != -1)
				{
					okField = true;
				}
				if(okField)
				{
					String FldVal = HttpContext.Current.Request.Form[HttpContext.Current.Request.Form.Keys[i]];
					FldVal = FldVal.Trim();
					if(FldVal.Length != 0)
					{
						String sql = String.Format("insert StringResource(Name,LocaleSetting,ConfigValue) values({0},{1},{2})", DB.SQuote(HttpContext.Current.Request.Form.Keys[i]), DB.SQuote(ShowLocaleSetting), DB.SQuote(FldVal));
						DB.ExecuteSQL(sql);
					}
				}
			}

			ctrlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.stringreport.StringsUpdated", SkinID, LocaleSetting), AlertMessage.AlertType.Success);
			loadData();
		}

		protected void btnExportExcel_Click(object sender, EventArgs e)
		{
			bool isReportTypeMissingStringResource = ReportType.Equals("missing", StringComparison.InvariantCultureIgnoreCase);
			string query = string.Empty;

			if(isReportTypeMissingStringResource)
			{
				query = "SELECT A.[Name], A.ConfigValue FROM (SELECT * FROM StringResource WHERE LocaleSetting=" + DB.SQuote("en-US") + ") A LEFT JOIN (SELECT * from StringResource where LocaleSetting=" + DB.SQuote(ShowLocaleSetting) + ") B on A.Name=B.Name WHERE B.LocaleSetting IS NULL ORDER BY A.NAME";
			}
			else
			{
				query = "SELECT A.[Name], A.ConfigValue FROM StringResource A WHERE LocaleSetting=" + DB.SQuote(ShowLocaleSetting) + " AND Modified=1 ORDER BY A.Name";
			}
			// reload the data.
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateNode(XmlNodeType.Element, "root", string.Empty);
			doc.AppendChild(root);

			using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(IDataReader reader = DB.GetRS(query, con))
				{
					while(reader.Read())
					{
						XmlNode stringResourceNode = doc.CreateNode(XmlNodeType.Element, "StringResource", string.Empty);
						XmlNode nameNode = doc.CreateNode(XmlNodeType.Element, "Name", string.Empty);
						XmlNode valueNode = doc.CreateNode(XmlNodeType.Element, "Value", string.Empty);

						nameNode.InnerText = DB.RSField(reader, "Name");
						valueNode.InnerText = DB.RSField(reader, "ConfigValue");

						stringResourceNode.AppendChild(nameNode);
						stringResourceNode.AppendChild(valueNode);

						root.AppendChild(stringResourceNode);
					}
				}
			}

			string filePath = CommonLogic.SafeMapPath("~/images") + "\\";
			string fileName = "StringResource_" + Localization.ToNativeDateTimeString(System.DateTime.Now).Replace(" ", "").Replace("/", "").Replace(":", "").Replace(".", "") + ".xls";

			//remove old export files
			string[] oldfiles = Directory.GetFiles(filePath, "StringResource_*.xls");
			foreach(string oldfile in oldfiles)
			{
				try
				{
					File.Delete(oldfile);
				}
				catch { }
			}

			string fileNameWithFullPath = filePath + fileName;
			AspDotNetStorefrontExcelWrapper.XmlToExcel.ConvertToExcel(doc, fileNameWithFullPath);

			string outputFileName = CommonLogic.IIF(isReportTypeMissingStringResource, "Missing", "Modified");

			Response.Clear();
			Response.AddHeader("content-disposition", string.Format("attachment; filename={0}StringResource.{1}.xls", outputFileName, ShowLocaleSetting));
			Response.BufferOutput = false;
			Response.ContentType = "application/vnd.ms-excel";
			Response.Charset = "";
			Response.TransmitFile(fileNameWithFullPath);
			Response.Flush();
			Response.End();
		}
	}
}
