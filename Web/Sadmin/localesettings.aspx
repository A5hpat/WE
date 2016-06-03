<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.localesettings" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="localesettings.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-language"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.locales.ManageLocales %>" />
	</h1>
	
	<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />

	<div id="container">
		<asp:Panel runat="server" ID="pnlGrid">
			<div class="item-action-bar">
				<a class="btn btn-default" href="currencies.aspx">Manage Currencies</a>
				<asp:Button runat="server" ID="btnInsert" CssClass="btn btn-action" Text="<%$Tokens:StringResource, admin.locales.CreateLocale %>" OnClick="btnInsert_Click" /><br />
			</div>
			<div class="white-ui-box">
				<asp:GridView ID="gMain" runat="server"
					AutoGenerateColumns="False"
					CssClass="table"
					OnRowCancelingEdit="gMain_RowCancelingEdit"
					OnRowCommand="gMain_RowCommand"
					OnRowDataBound="gMain_RowDataBound"
					OnRowUpdating="gMain_RowUpdating"
					OnRowEditing="gMain_RowEditing"
					OnPageIndexChanging="gMain_PageIndexChanging"
					CellPadding="0"
					GridLines="None"
					AllowPaging="true">
					<Columns>
						<asp:BoundField DataField="LocaleSettingID" HeaderText="ID" ReadOnly="True" SortExpression="LocaleSettingID"></asp:BoundField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Name %>" SortExpression="Name">
							<ItemTemplate>
								<asp:Literal runat="server" ID="ltName" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:Literal>
							</ItemTemplate>
							<EditItemTemplate>
								<asp:TextBox MaxLength="10" CssClass="singleNormal" runat="server" ID="txtName" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:TextBox>
								ex: en-US
								<asp:RequiredFieldValidator runat="server" ErrorMessage="<%$Tokens:StringResource, admin.common.ValidatorMessage %>" ControlToValidate="txtName" />
							</EditItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Description %>" SortExpression="Description">
							<ItemTemplate>
								<asp:Literal runat="server" ID="ltDescription" Text='<%# DataBinder.Eval(Container.DataItem, "Description") %>'></asp:Literal>
							</ItemTemplate>
							<EditItemTemplate>
								<asp:TextBox CssClass="singleNormal" runat="server" ID="txtDescription" Text='<%# DataBinder.Eval(Container.DataItem, "Description") %>'></asp:TextBox>
								<asp:RequiredFieldValidator runat="server" ErrorMessage="<%$Tokens:StringResource, admin.common.ValidatorMessage %>" ControlToValidate="txtDescription" />
							</EditItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.localesettings.DefaultCurrency %>" SortExpression="DefaultCurrencyID">
							<ItemTemplate>
								<%# DataBinder.Eval(Container.DataItem, "DefaultCurrencyID") %>
							</ItemTemplate>
							<EditItemTemplate>
								<asp:DropDownList runat="server" ID="ddCurrency" ToolTip="Published Currencies"></asp:DropDownList>
							</EditItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Prompts">
							<ItemTemplate>
								<a href='stringresources.aspx?filterlocale=<%# DataBinder.Eval(Container.DataItem, "Name") %>'>
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.localesettings.EditUpload %>" /></a>
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.localesettings.DisplayOrder %>" SortExpression="DisplayOrder">
							<ItemTemplate>
								<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>
							</ItemTemplate>
							<EditItemTemplate>
								<asp:TextBox CssClass="singleShortest" runat="server" ID="txtOrder" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>'></asp:TextBox>
							</EditItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Delete %>">
							<ItemTemplate>
								<asp:LinkButton ID="Delete" CommandName="DeleteItem" CommandArgument='<%# Eval("LocaleSettingID") %>' runat="server" Text='<i class="fa fa-times"></i> Delete' CssClass="delete-link" />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:CommandField HeaderText="<%$Tokens:StringResource, admin.common.Edit %>" ItemStyle-Width="10%" ButtonType="Link" ShowEditButton="True" ControlStyle-CssClass="edit-link" CancelText='<i class="fa fa-reply"></i> Cancel' EditText='<i class="fa fa-share"></i> Edit' UpdateText='<i class="fa fa-floppy-o"></i> Save' />
					</Columns>
					<SelectedRowStyle CssClass="grid-view-action" />
				</asp:GridView>
			</div>
		</asp:Panel>
		<asp:Panel ID="pnlAdd" runat="Server" DefaultButton="btnSubmit">
			<div class="white-ui-box">
				<div class="row">
					<div class="col-sm-6">
						<div class="form-group">
							<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Name %>" />:
							<asp:TextBox ID="txtName" runat="server" CssClass="form-control" ValidationGroup="gAdd" />
							<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.common.FillinName %>" CssClass="text-danger" ControlToValidate="txtName" ValidationGroup="gAdd" Display="dynamic" SetFocusOnError="true" runat="server" />
							<asp:RegularExpressionValidator ErrorMessage="Validate Name" ControlToValidate="txtName" CssClass="text-danger" ValidationGroup="gAdd" Display="dynamic" SetFocusOnError="true" runat="server" ValidationExpression="^[a-z][a-z]-[A-Z][A-Z]$" />
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.localesettings.Sample %>" />
						</div>
					</div>
				</div>
				<div class="row">
					<div class="col-sm-6">
						<div class="form-group">
							<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Description %>" />:
							<asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="multiline" ValidationGroup="gAdd" />
							<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.common.FillinDescription %>" CssClass="text-danger" ControlToValidate="txtDescription" ValidationGroup="gAdd" Display="dynamic" SetFocusOnError="true" runat="server" />
						</div>
					</div>
				</div>
				<div class="row">
					<div class="col-sm-6">
						<div class="form-group">
							<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.localesettings.DefaultCurrency %>" />:
							<asp:DropDownList CssClass="form-control" ID="ddCurrency" runat="server" ValidationGroup="gAdd" />
							<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.localesettings.SelectCurrency %>" CssClass="text-danger" InitialValue="0" ControlToValidate="ddCurrency" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
						</div>
					</div>
				</div>
				<div class="row">
					<div class="col-sm-6">
						<div class="form-group">
							<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.localesettings.DisplayOrder %>" />:
							<asp:TextBox ID="txtOrder" runat="Server" CssClass="form-control" ValidationGroup="gAdd">1</asp:TextBox>
							<asp:RequiredFieldValidator CssClass="text-danger" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.common.EnterDisplayOrder %>" ValidationGroup="gAdd" ControlToValidate="txtOrder" />
						</div>
					</div>
				</div>
				<asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" CssClass="text-danger" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
			</div>
			<div class="item-action-bar">
				<asp:Button ID="btnCancel" runat="server" CssClass="btn btn-default" Text="Cancel" OnClick="btnCancel_Click" />
				<asp:Button ValidationGroup="gAdd" ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" CssClass="btn btn-primary" Text="<%$Tokens:StringResource, admin.common.Save %>" />
			</div>
		</asp:Panel>
	</div>
</asp:Content>
