<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.creditcards" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="creditcards.aspx.cs" %>


<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-credit-card"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.creditcards.CreditCards %>" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />

	<div id="container">
		<asp:Panel runat="server" ID="pnlGrid" DefaultButton="btnInsert">
			<div class="item-action-bar">
				<asp:Button runat="server" ID="btnInsert" CssClass="btn btn-action" Text="<%$Tokens:StringResource, admin.creditcards.createCCType %>" OnClick="btnInsert_Click" /><br />
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
					CellPadding="0"
					GridLines="None"
					AllowPaging="true">
					<Columns>
						<asp:BoundField DataField="CardTypeID" HeaderText="<%$Tokens:StringResource, admin.common.ID %>" ReadOnly="True" SortExpression="CardTypeID"></asp:BoundField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.creditcards.CardType %>" SortExpression="CardType">
							<ItemTemplate>
								<asp:Literal runat="server" ID="ltName" Text='<%# DataBinder.Eval(Container.DataItem, "CardType") %>'></asp:Literal>
							</ItemTemplate>
							<EditItemTemplate>
								<asp:TextBox ID="txtName" runat="Server" Text='<%# DataBinder.Eval(Container.DataItem, "CardType")%>' CssClass="singleNormal"></asp:TextBox>
							</EditItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Delete %>">
							<ItemTemplate>
								<asp:LinkButton ID="Delete" CommandName="DeleteItem" CommandArgument='<%# Eval("CardTypeID") %>' runat="server" Text='<i class="fa fa-times"></i> Delete' CssClass="delete-link" />
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
				*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.CreditCard %>" />:
				<asp:TextBox ID="txtName" runat="server" CssClass="singleNormal" ValidationGroup="gAdd" />
				<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.common.FillinName %>" ControlToValidate="txtName" ID="RequiredFieldValidator2" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator>
				<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.creditcards.tooltip.imgInfo %>">
				<i class="fa fa-question-circle fa-lg"></i>
				</asp:Label>
				<asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
			</div>
			<div class="item-action-bar">
				<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="creditcards.aspx" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
				<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
			</div>
		</asp:Panel>
	</div>
</asp:Content>
