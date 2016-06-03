<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin._EventHandler" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="eventhandler.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-exclamation-triangle"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.eventhandler %>" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="ctlAlertMessage" />
	<div class="white-ui-box">
		<asp:GridView ID="Grid" runat="server"
			AutoGenerateColumns="False"
			CssClass="table"
			OnRowCancelingEdit="Grid_RowCancelingEdit"
			OnRowCommand="Grid_RowCommand"
			OnRowDataBound="Grid_RowDataBound"
			OnRowUpdating="Grid_RowUpdating"
			OnRowEditing="Grid_RowEditing"
			OnPageIndexChanging="Grid_PageIndexChanging"
			CellPadding="0"
			GridLines="None"
			AllowPaging="true" 
			PagerStyle-CssClass="gridview-pager">
			<Columns>
				<asp:BoundField DataField="EventID" HeaderText="<%$Tokens:StringResource, admin.common.ID %>" ReadOnly="True" SortExpression="EventID" />
				<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.eventhandler.EventName %>" SortExpression="EventName">
					<ItemTemplate>
						<%# DataBinder.Eval(Container.DataItem, "EventName") %>
					</ItemTemplate>
					<EditItemTemplate>
						<asp:TextBox ID="txtEventName" runat="Server" CssClass="singleAuto" Text='<%# DataBinder.Eval(Container.DataItem, "EventName") %>' />
						<asp:Literal ID="ltEventName" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "EventName") %>' />
						<asp:RequiredFieldValidator ID="RequiredFieldValidator" runat="server" ControlToValidate="txtEventName"	ErrorMessage="<%$Tokens:StringResource, admin.common.ValidatorMessage %>" />
					</EditItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.eventhandler.XMLpackage %>">
					<ItemTemplate>
						<div style="white-space: normal; overflow: visible;">
							<%# DataBinder.Eval(Container.DataItem, "XmlPackage") %>
						</div>
					</ItemTemplate>
					<EditItemTemplate>
						<asp:DropDownList ID="ddEditXMLPackage" runat="server" CssClass="default" />
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Or %>" />
						<asp:TextBox ID="txtXMLPackage" runat="Server" ToolTip="Write in xmlpackage name" CssClass="default" />
					</EditItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.eventhandler.CalloutUrl %>">
					<ItemTemplate>
						<div style="white-space: normal; overflow: visible;">
							<%# DataBinder.Eval(Container.DataItem, "CalloutURL") %>
						</div>
					</ItemTemplate>
					<EditItemTemplate>
						<asp:TextBox ID="txtCalloutURL" runat="server" CssClass="singleAuto" Text='<%# DataBinder.Eval(Container.DataItem, "CalloutURL") %>'
							TextMode="SingleLine"></asp:TextBox>
					</EditItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Active %>">
					<ItemTemplate>
						<div style="white-space: normal; overflow: visible;">
							<%# DataBinder.Eval(Container.DataItem, "Active") %>
						</div>
					</ItemTemplate>
					<EditItemTemplate>
						<asp:CheckBox ID="cbkActive" runat="server" CssClass="singleAuto" Checked='<%# DataBinder.Eval(Container.DataItem, "Active") %>' />
					</EditItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Debug %>">
					<ItemTemplate>
						<div style="white-space: normal; overflow: visible;">
							<%# DataBinder.Eval(Container.DataItem, "Debug") %>
						</div>
					</ItemTemplate>
					<EditItemTemplate>
						<asp:CheckBox ID="cbkDebug" runat="server" CssClass="singleAuto" Checked='<%# DataBinder.Eval(Container.DataItem, "Debug") %>' />
					</EditItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Delete %>">
					<ItemTemplate>
						<asp:LinkButton ID="Delete" CommandName="DeleteItem" CommandArgument='<%# Eval("EventID") %>' runat="server" Text='<i class="fa fa-times"></i> Delete' CssClass="delete-link" />
					</ItemTemplate>
				</asp:TemplateField>
				<asp:CommandField HeaderText="<%$Tokens:StringResource, admin.common.Edit %>" ItemStyle-Width="10%" ButtonType="Link" ShowEditButton="True" ControlStyle-CssClass="edit-link" CancelText='<i class="fa fa-reply"></i> Cancel' EditText='<i class="fa fa-share"></i> Edit' UpdateText='<i class="fa fa-floppy-o"></i> Save' />				
			</Columns>			
			<SelectedRowStyle CssClass="grid-view-action" />
		</asp:GridView>
	</div>
	<div class="admin-row">
		<div id="divAddEventHandler" class="white-ui-box">
			<div class="white-box-header">
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.AddNew %>" />:
			</div>
			<div class="row">
				<div class="col-md-6">
					<asp:Panel ID="pnlAdd" runat="server" DefaultButton="btnInsert">
						<div class="form-group">
							<asp:TextBox ID="txtAddName" runat="server" CssClass="form-control" />
							<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtAddName" Display="dynamic" ErrorMessage="*" ValidationGroup="Add" />
						</div>
						<div class="form-group">
							<asp:DropDownList ID="ddAddXmlPackage" runat="server" CssClass="form-control" OnSelectedIndexChanged="ddAddXmlPackage_SelectedIndexChanged" />
						</div>
						<div class="form-group">
							<asp:TextBox ID="txtAddURL" runat="server" CssClass="form-control" />
							<asp:RequiredFieldValidator ID="RequiredFieldValidator" runat="server" ControlToValidate="txtAddURL" Display="dynamic" ErrorMessage="<%$Tokens:StringResource, admin.common.ValidatorMessage %>" ValidationGroup="Add" />
						</div>
						<asp:Button ID="btnInsert" runat="server" CssClass="btn btn-action btn-sm" OnClick="btnInsert_Click" Text="<%$Tokens:StringResource, admin.common.Add %>" ValidationGroup="Add" />
					</asp:Panel>
				</div>
			</div>
		</div>
	</div>
</asp:Content>