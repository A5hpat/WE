// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontExcelWrapper;

namespace AspDotNetStorefrontAdmin
{
	public partial class exportProductPricing : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			if(!IsPostBack)
			{
				LoadData();
				pnlDistributor.Visible = false;
				pnlDistributor.Visible = true;
			}
			Page.Form.DefaultButton = btnUpload.UniqueID;
		}

		protected void LoadData()
		{
			//clear
			ddCategory.Items.Clear();
			ddSection.Items.Clear();
			ddManufacturer.Items.Clear();
			ddDistributor.Items.Clear();

			//load Categories
			ddCategory.Items.Add(new ListItem(" - Select -", "-1"));
			EntityHelper eTemp = new EntityHelper(EntityDefinitions.readonly_CategoryEntitySpecs, 0);
			ArrayList al = eTemp.GetEntityArrayList(0, "", 0, LocaleSetting, false);
			for(int i = 0; i < al.Count; i++)
			{
				ListItemClass lic = (ListItemClass)al[i];
				string value = lic.Value.ToString();
				string name = Server.HtmlDecode(lic.Item);

				ddCategory.Items.Add(new ListItem(name, value));
			}

			//load Sections
			ddSection.Items.Add(new ListItem(" - Select -", "-1"));
			eTemp = new EntityHelper(EntityDefinitions.readonly_SectionEntitySpecs, 0);
			al = eTemp.GetEntityArrayList(0, "", 0, LocaleSetting, false);
			for(int i = 0; i < al.Count; i++)
			{
				ListItemClass lic = (ListItemClass)al[i];
				string value = lic.Value.ToString();
				string name = Server.HtmlDecode(lic.Item);

				ddSection.Items.Add(new ListItem(name, value));
			}

			//load Manufacturers
			ddManufacturer.Items.Add(new ListItem(" - Select -", "-1"));
			eTemp = new EntityHelper(EntityDefinitions.readonly_ManufacturerEntitySpecs, 0);
			al = eTemp.GetEntityArrayList(0, "", 0, LocaleSetting, false);
			for(int i = 0; i < al.Count; i++)
			{
				ListItemClass lic = (ListItemClass)al[i];
				string value = lic.Value.ToString();
				string name = Server.HtmlDecode(lic.Item);

				ddManufacturer.Items.Add(new ListItem(name, value));
			}

			//load Distributors
			ddDistributor.Items.Add(new ListItem(" - Select -", "-1"));

			using(var dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(var rsst = DB.GetRS("select * from Distributor   with (NOLOCK)  where Deleted=0 order by DisplayOrder,Name", dbconn))
				{
					while(rsst.Read())
					{
						ddDistributor.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", LocaleSetting), DB.RSFieldInt(rsst, "DistributorID").ToString()));
					}
				}
			}
		}

		protected void btnUpload_Click(object sender, EventArgs e)
		{
			string filepath = CommonLogic.SafeMapPath("~/images") + "\\";
			string filename = "priceexport_" + Localization.ToNativeDateTimeString(System.DateTime.Now).Replace(" ", "").Replace("/", "").Replace(":", "").Replace(".", "");
			string fileextension = String.Empty;

			string xml = AppLogic.ExportProductList(Localization.ParseNativeInt(ddCategory.SelectedValue), Localization.ParseNativeInt(ddSection.SelectedValue), Localization.ParseNativeInt(ddManufacturer.SelectedValue), Localization.ParseNativeInt(ddDistributor.SelectedValue), -1, -1);
			string exporttype = rblExport.SelectedValue;

			//remove old export files
			string[] oldfiles = Directory.GetFiles(filepath, "priceexport_*." + exporttype);
			foreach(string oldfile in oldfiles)
			{
				try
				{
					File.Delete(oldfile);
				}
				catch { }
			}

			XmlDocument xdoc = new XmlDocument();
			xdoc.LoadXml(xml);

			string FullFilePath = filepath + filename;
			XslCompiledTransform xsl = new XslCompiledTransform();

			Customer c = Context.GetCustomer();
			XsltArgumentList args = new XsltArgumentList();
			args.AddParam("locale", "", c.LocaleSetting);

			if(exporttype == "xls")
			{
				FullFilePath += ".xls";
				fileextension = ".xls";
				xsl.Load(CommonLogic.SafeMapPath(string.Format("{0}/XmlPackages/ProductPricingExportExcel.xslt", AppLogic.AdminDir())));
				StringWriter xsw = new StringWriter();
				xsl.Transform(xdoc, args, xsw);
				xdoc.LoadXml(xsw.ToString());
				XmlToExcel.ConvertToExcel(xdoc, FullFilePath);
			}
			else
			{
				if(exporttype == "xml")
				{
					FullFilePath += ".xml";
					fileextension = ".xml";
					xsl.Load(CommonLogic.SafeMapPath(string.Format("{0}/XmlPackages/ProductPricingExport.xslt", AppLogic.AdminDir())));
				}
				else
				{
					FullFilePath += ".csv";
					fileextension = ".csv";
					xsl.Load(CommonLogic.SafeMapPath(string.Format("{0}/XmlPackages/ProductPricingExportCSV.xslt", AppLogic.AdminDir())));
				}

				StreamWriter sw = new StreamWriter(FullFilePath);
				xsl.Transform(xdoc, args, sw);
				sw.Close();
			}

			Response.Clear();
			Response.ClearHeaders();
			Response.ClearContent();
			Response.AddHeader("content-disposition", "attachment; filename=ProductPricing" + fileextension);
			Response.BufferOutput = false;

			if(exporttype == "xml")
			{
				//Send the XML
				Response.ContentType = "text/xml";
				Response.Write(XmlCommon.PrettyPrintXml(CommonLogic.ReadFile(FullFilePath, false)));
			}
			else if(exporttype == "xls")
			{
				Response.ContentType = "application/vnd.ms-excel";
				Response.Charset = "";
				Response.TransmitFile(FullFilePath);
			}
			else
			{
				// Send the CSV
				Response.BufferOutput = false;
				Response.ContentType = "application/x-unknown";
				Response.TransmitFile(FullFilePath);
			}

			Response.Flush();
			Response.End();
		}
	}
}
