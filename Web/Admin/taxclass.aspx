<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.taxClass" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="taxclass.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div class="breadCrumb3">
		<asp:Literal ID="ltScript" runat="server"></asp:Literal><asp:Literal ID="ltValid" runat="server"></asp:Literal>
	</div>
	<h1>
		<i class="fa fa-university"></i>
		<asp:Literal ID="litHeader" runat="server" Text="<%$Tokens:StringResource, admin.taxclass.taxclassheader %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

	<asp:Panel runat="server" ID="pnlGrid">
		<div class="list-action-bar">
			<asp:Button runat="server" ID="btnInsert" CssClass="btn btn-action" Text="<%$Tokens:StringResource, admin.taxes.CreateTaxClass %>" OnClick="btnInsert_Click" /><br />
		</div>
		<div class="white-ui-box">
			<asp:GridView Width="100%" ID="gMain" runat="server" 
				PagerSettings-Position="TopAndBottom" 
				AutoGenerateColumns="False" 
				AllowPaging="True" 
				PageSize="15" 
				AllowSorting="True" 
				OnRowCancelingEdit="gMain_RowCancelingEdit" 
				OnRowCommand="gMain_RowCommand" 
				OnRowDataBound="gMain_RowDataBound" 
				OnSorting="gMain_Sorting" 
				OnPageIndexChanging="gMain_PageIndexChanging" 
				OnRowUpdating="gMain_RowUpdating" 
				OnRowEditing="gMain_RowEditing" 
				CellPadding="0" 
				GridLines="None" 
				ShowFooter="True"
				CssClass="table">

				<Columns>
					<asp:BoundField DataField="TaxClassID" HeaderText="<%$Tokens:StringResource, admin.taxclass.ID %>" ReadOnly="True" SortExpression="TaxClassID">
						<ItemStyle CssClass="lighterData" />
					</asp:BoundField>
					<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.taxclass.name %>" SortExpression="Name">
						<ItemTemplate>
							<asp:Literal runat="server" ID="ltName" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:Literal>
						</ItemTemplate>
						<EditItemTemplate>
							<%# DataBinder.Eval(Container.DataItem, "EditName") %>
						</EditItemTemplate>
						<ItemStyle CssClass="normalData" />
					</asp:TemplateField>
					<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.taxclass.taxcodeheader %>" SortExpression="TaxCode">
						<ItemTemplate>
							<asp:Literal runat="server" ID="ltTaxCode" Text='<%# DataBinder.Eval(Container.DataItem, "TaxCode") %>'></asp:Literal>
						</ItemTemplate>
						<EditItemTemplate>
							<asp:TextBox ID="txtTaxCode" runat="server" CssClass="singleNormal" Text='<%# DataBinder.Eval(Container.DataItem, "TaxCode") %>'></asp:TextBox>
						</EditItemTemplate>
						<ItemStyle CssClass="lighterData" />
					</asp:TemplateField>
					<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.taxclass.displayorderheader %>" SortExpression="DisplayOrder">
						<ItemTemplate>
							<asp:Literal runat="server" ID="ltDisplayOrder" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>'></asp:Literal>
						</ItemTemplate>
						<EditItemTemplate>
							<asp:TextBox ID="txtDisplayOrder" runat="server" CssClass="singleNormal" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>'></asp:TextBox>
							<asp:CompareValidator ID="cmpValTxtDisplayOrder" runat="server" ControlToValidate="txtDisplayOrder" ValueToCompare="0" Operator="GreaterThanEqual" ErrorMessage="<%$Tokens:StringResource, admin.common.ValidIntegerNumberPrompt%>" Display="Dynamic" Type="Integer" />
						</EditItemTemplate>
						<ItemStyle CssClass="lighterData" />
					</asp:TemplateField>
					<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Delete %>">
						<ItemTemplate>
							<asp:LinkButton ID="lnkDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("TaxClassID") %>' runat="Server" Text='<i class="fa fa-times"></i> Delete' CssClass="delete-link" />
						</ItemTemplate>
						<ItemStyle CssClass="selectData" Width="5%" />
					</asp:TemplateField>
					<asp:CommandField HeaderText="<%$Tokens:StringResource, admin.common.Edit %>" ItemStyle-Width="10%" ButtonType="Link" ShowEditButton="True" ControlStyle-CssClass="edit-link" CancelText='<i class="fa fa-reply"></i> Cancel' EditText='<i class="fa fa-share"></i> Edit' UpdateText='<i class="fa fa-floppy-o"></i> Save' />
					<asp:BoundField Visible="False" DataField="EditName" ReadOnly="True" />
				</Columns>
				<EditRowStyle CssClass="grid-view-edit" />
			</asp:GridView>
		</div>
		<div class="list-action-bar">
			<asp:Button runat="server" ID="btnInsertBottom" CssClass="btn btn-action" Text="<%$Tokens:StringResource, admin.taxes.CreateTaxClass %>" OnClick="btnInsert_Click" /><br />
		</div>
	</asp:Panel>

	<asp:Panel ID="pnlAdd" runat="Server" DefaultButton="btnSubmit">
		<div class="item-action-bar">
			<asp:Button ID="btnCancel" CssClass="btn btn-default" runat="server" Text="Cancel" OnClick="btnCancel_Click" />
			<asp:Button ValidationGroup="gAdd" CausesValidation="true" ID="btnSubmit" CssClass="btn btn-primary" runat="server" OnClick="btnSubmit_Click" Text="Save" />
		</div>
		<div class="white-ui-box">
			<table class="table">
			<tr>
				<td>
					<span><asp:Literal ID="litTaxClass" Text="<%$Tokens:StringResource, admin.taxclass.taxclass %>" runat="server" /></span>
				</td>
				<td>
					<asp:Literal ID="ltTaxClass" runat="server"></asp:Literal>
				</td>
			</tr>
			<tr>
				<td>
					<span><asp:Literal ID="litTaxCode" Text="<%$Tokens:StringResource, admin.taxclass.taxcode %>" runat="server" /></span>
				</td>
				<td>
					<asp:TextBox ID="txtTaxCode" runat="server" CssClass="singleShort" ValidationGroup="gAdd"></asp:TextBox>
				</td>
			</tr>
			<tr>
				<td>
					<span><asp:Literal ID="litDispOrder" Text="<%$Tokens:StringResource, admin.taxclass.displayorder %>" runat="server" /></span>
				</td>
				<td>
					<asp:TextBox ID="txtDisplayOrder" runat="server" CssClass="singleShort" ValidationGroup="gAdd"></asp:TextBox>
					<asp:CompareValidator ID="cmpValTxtDisplayOrder" runat="server" ControlToValidate="txtDisplayOrder" ValueToCompare="0" Operator="GreaterThanEqual" ErrorMessage="<%$Tokens:StringResource, admin.common.ValidIntegerNumberPrompt%>" Display="Dynamic" Type="Integer" />
				</td>
			</tr>
			<tr>
				<td colspan="2">
					<asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
				</td>
			</tr>
		</table>
		</div>
		<div class="item-action-bar">
			<asp:Button ID="btnCancelBottom" CssClass="btn btn-default" runat="server" Text="Cancel" OnClick="btnCancel_Click" />
			<asp:Button ValidationGroup="gAdd" CausesValidation="true" ID="btnSubmitBottom" CssClass="btn btn-primary" runat="server" OnClick="btnSubmit_Click" Text="Save" />
		</div>
	</asp:Panel>

</asp:Content>
