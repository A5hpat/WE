// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class _EventHandler : AspDotNetStorefront.Admin.AdminPageBase
	{
		protected string SelectSQL = "select EventID, EventName, CalloutURL, XmlPackage, Active, Debug from EventHandler ";

		protected void Page_Load(object sender, EventArgs e)
		{
			txtAddName.Attributes.Add("placeholder", "admin.eventhandler.EventHandlerName".StringResource());
			txtAddURL.Attributes.Add("placeholder", "admin.eventhandler.CalloutUrl".StringResource());

			if(IsPostBack)
				return;
			
			LoadXMLPackages();

			ViewState["Sort"] = "EventName";
			ViewState["SortOrder"] = "ASC";
			ViewState["SQLString"] = SelectSQL;

			BuildGridData();
		}

		protected void Grid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
		{
			btnInsert.Enabled = true;
		
			if(Localization.ParseBoolean(ViewState["IsInsert"].ToString()))
			{
				GridViewRow row = Grid.Rows[e.RowIndex];
				if(row != null)
				{
					var id = Localization.ParseNativeInt(row.Cells[0].Text);
					DeleteRow(id);
				}
			}

			ViewState["IsInsert"] = false;

			Grid.EditIndex = -1;
			BuildGridData();
		}

		protected void Grid_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if(e.Row.RowType == DataControlRowType.DataRow)
			{
				var deleteButton = (LinkButton)e.Row.FindControl("Delete");
				deleteButton.Attributes.Add("onClick", "javascript: return confirm('" + AppLogic.GetString("admin.common.ConfirmDeletion", SkinID, LocaleSetting) + "')");
			}

			if((e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
			{
				var myrow = (DataRowView)e.Row.DataItem;
				var ddXmlPackage = (DropDownList)e.Row.FindControl("ddEditXMLPackage");
				ddXmlPackage.Items.Clear();
				var myNode = new ListItem();
				myNode.Value = AppLogic.GetString("admin.eventhandler.SelectPackage", SkinID, LocaleSetting);
				ddXmlPackage.Items.Add(myNode);

				var Location = CommonLogic.SafeMapPath("~/XMLPackages");
				var dir = new System.IO.DirectoryInfo(Location);
				foreach(var f in dir.GetFiles("event.*"))
				{
					var myNode1 = new ListItem();
					myNode1.Value = f.Name.ToUpperInvariant();
					ddXmlPackage.Items.Add(myNode1);
				}

				if(ddXmlPackage.Items.Count > 1)
				{
					ddXmlPackage.SelectedValue = myrow["XMLPackage"].ToString().ToUpperInvariant();
				}
				try
				{
					if(ViewState["FirstTimeEdit"].ToString() == "0")
					{
						var txt = (TextBox)e.Row.FindControl("txtEventName");
						txt.Visible = false;
						var lt = (Literal)e.Row.FindControl("ltEventName");
						lt.Visible = true;
					}
					else
					{
						var txt = (TextBox)e.Row.FindControl("txtEventName");
						txt.Visible = true;
						var lt = (Literal)e.Row.FindControl("ltEventName");
						lt.Visible = false;
					}
				}
				catch
				{
					var txt = (TextBox)e.Row.FindControl("txtEventName");
					txt.Visible = false;
					var lt = (Literal)e.Row.FindControl("ltEventName");
					lt.Visible = true;
				}
			}
		}

		protected void Grid_PageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			ViewState["IsInsert"] = false;
			Grid.PageIndex = e.NewPageIndex;
			Grid.EditIndex = -1;
			BuildGridData();
		}

		protected void Grid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if(e.CommandName == "DeleteItem")
			{
				ViewState["IsInsert"] = false;
				Grid.EditIndex = -1;
				var id = Localization.ParseNativeInt(e.CommandArgument.ToString());
				DeleteRow(id);
			}
		}

		protected void Grid_RowUpdating(object sender, GridViewUpdateEventArgs e)
		{
			btnInsert.Enabled = true;

			ViewState["IsInsert"] = false;
			var row = Grid.Rows[e.RowIndex];

			if(row != null)
			{
				var eventname = (TextBox)row.FindControl("txtEventName");
				var xmlpackage = (TextBox)row.FindControl("txtXMLPackage");
				var callouturl = (TextBox)row.FindControl("txtCalloutURL");
				var ddXMLPackage = (DropDownList)row.FindControl("ddEditXMLPackage");
				var Active = (CheckBox)row.FindControl("cbkActive");
				var Debug = (CheckBox)row.FindControl("cbkDebug");

				string XMLPackagename = ddXMLPackage.SelectedValue;
				if(ddXMLPackage.SelectedIndex == 0)
				{
					XMLPackagename = xmlpackage.Text;
				}

				var eventHandlerUpdateError = String.Empty;
				var a = AppLogic.eventHandler(eventname.Text);
				if(a == null)
				{
					ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.eventhanlder.NonExistingEventHandler", SkinID, LocaleSetting), AlertMessage.AlertType.Error);
				}
				else
				{
					eventHandlerUpdateError = a.Update(eventname.Text, callouturl.Text, XMLPackagename.ToUpperInvariant(), Active.Checked, Debug.Checked);

					if(!String.IsNullOrEmpty(eventHandlerUpdateError))
					{
						ctlAlertMessage.PushAlertMessage("Update Failed", AlertMessage.AlertType.Error);
						return;
					}
				}

				try
				{
					LoadXMLPackages();
					Grid.EditIndex = -1;
				}
				catch(Exception ex)
				{
					ctlAlertMessage.PushAlertMessage("XMLPackage load Failed" + ex.Message, AlertMessage.AlertType.Error);
				}

				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.common.ItemUpdated", SkinID, LocaleSetting), AlertMessage.AlertType.Success);
				Grid.EditIndex = -1;//stops grid staying in edit mode
				BuildGridData(); //stops edited row from being blank after edit #hack
			}
		}

		protected void Grid_RowEditing(object sender, GridViewEditEventArgs e)
		{
			btnInsert.Enabled = false;
			ViewState["IsInsert"] = false;
			Grid.EditIndex = e.NewEditIndex;
			BuildGridData();
		}

		protected void ddAddXmlPackage_SelectedIndexChanged(object sender, EventArgs e)
		{
			ViewState["IsInsert"] = false;
			Grid.EditIndex = -1;
			Grid.PageIndex = 0;
		}

		protected void btnInsert_Click(object sender, EventArgs e)
		{
			ViewState["IsInsert"] = false;
			Grid.EditIndex = -1;

			if(string.IsNullOrWhiteSpace(txtAddName.Text))
			{
				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.eventhandler.EnterEventHandler", SkinID, LocaleSetting), AlertMessage.AlertType.Error);
				return;
			}
			
			var eventHandlerName = txtAddName.Text.Trim();
			var existingEventHandler = AppLogic.eventHandler(eventHandlerName);
			if(existingEventHandler != null && !string.IsNullOrWhiteSpace(existingEventHandler.EventName))
			{
				ctlAlertMessage.PushAlertMessage(AppLogic.GetString("admin.eventhandler.DuplicateEventHandler", SkinID, LocaleSetting), AlertMessage.AlertType.Error);
				return;
			}

			try
			{
				AppLogic.EventHandlerTable.Add(
					EventName:eventHandlerName, 
					CalloutURL:txtAddURL.Text.Trim(), 
					XmlPackage: ddAddXmlPackage.SelectedValue, 
					Active: true, 
					Debug: false);
				
				ViewState["Sort"] = "EventID";
				ViewState["SortOrder"] = "DESC";
				ViewState["FirstTimeEdit"] = "0";

				BuildGridData();

				ctlAlertMessage.PushAlertMessage("Item added.", AlertMessage.AlertType.Success);
				ViewState["IsInsert"] = true;
			}
			catch(Exception ex)
			{
				throw new Exception(String.Format(AppLogic.GetString("admin.common.ErrUpdateDB", SkinID, LocaleSetting), ex));
			}
		}
		
		protected void BuildGridData()
		{
			var sql = string.Format("{0} order by {1} {2}",
				ViewState["SQLString"],
				ViewState["Sort"],
				ViewState["SortOrder"]);

			using(var dbconn = DB.dbConn())
			{
				dbconn.Open();
				using(var rs = DB.GetRS(sql, dbconn))
				{
					using(var dt = new DataTable())
					{
						dt.Load(rs);
						Grid.DataSource = dt;
						Grid.DataBind();
					}
				}
			}
		}

		protected void DeleteRow(int id)
		{
			try
			{
				DB.ExecuteSQL(string.Format("DELETE FROM EventHandler WHERE EventID={0}", id));

				LoadXMLPackages();

				BuildGridData();
				ctlAlertMessage.PushAlertMessage("Item Deleted", AlertMessage.AlertType.Success);
			}
			catch(Exception ex)
			{
				throw new Exception(String.Format(AppLogic.GetString("admin.common.ErrUpdateDB", SkinID, LocaleSetting), ex));
			}
		}
		
		void LoadXMLPackages()
		{
			var Location = CommonLogic.SafeMapPath("~/XMLPackages");
			var dir = new System.IO.DirectoryInfo(Location);
			foreach(System.IO.FileInfo f in dir.GetFiles("event.*"))
			{
				var myNode = new ListItem();
				myNode.Value = f.Name.ToUpperInvariant();
				ddAddXmlPackage.Items.Add(myNode);
			}
		}
	}
}
