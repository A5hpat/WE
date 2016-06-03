<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin._State" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="state.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="states.aspx" />
	<div class="admin-module">
	<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />
		<h1>
			<i class="fa fa-university"></i>
			<asp:Literal ID="litHeader" runat="server" />
		</h1>	
		<div class="item-action-bar">
			<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmitTop" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>

		<div class="admin-row">
			<div id="divEditState" class="white-ui-box">
				<div class="row">
					<div class="col-md-4">
						<div class="form-inline">
							<asp:Label AssociatedControlID="litStateId" for="litStateId" runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />:
							<asp:Literal ID="litStateId" runat="server" />
						</div>
						<div class="form-inline">
							<asp:Label AssociatedControlID="cbxPublished" for="cbxPublished" runat="server" Text="<%$Tokens:StringResource, admin.Common.Published %>" />:
							<asp:CheckBox ID="cbxPublished" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtStateName" for="txtStateName" runat="server" Text="<%$Tokens:StringResource, admin.StateName %>" />:
							<asp:TextBox ID="txtStateName" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator ErrorMessage="Fill in Name!" CssClass="text-danger" ControlToValidate="txtStateName" ID="RequiredFieldValidator9" ValidationGroup="gAdd" Display="Dynamic" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator>
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtStateAbbreviation" for="txtStateAbbreviation" runat="server" Text="<%$Tokens:StringResource, admin.Abbreviation %>" />:
							<asp:TextBox ID="txtStateAbbreviation" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator ErrorMessage="Fill in Abbreviation!" CssClass="text-danger" ControlToValidate="txtStateAbbreviation" ID="RequiredFieldValidator1" Display="Dynamic" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator>
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="txtStateAbbreviation" for="txtStateAbbreviation" runat="server" Text="Country" />:
							<asp:DropDownList runat="server" CssClass="form-control" ID="ddCountry" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtDisplayOrder" for="txtDisplayOrder" runat="server" Text="<%$Tokens:StringResource, admin.Common.DisplayOrder %>" />
							<asp:TextBox ID="txtDisplayOrder" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator ErrorMessage="Fill in Display Order!" CssClass="text-danger" ControlToValidate="txtDisplayOrder" ID="RequiredFieldValidator2" Display="Dynamic" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator>
						</div>
					</div>
				</div>
			</div>
			
			<div id="ZoneGrid" class="white-ui-box">
				<div class="white-box-heading">
						<asp:Literal ID="litGridHead" runat="server" Text="<%$Tokens:StringResource, admin.menu.taxclass %>" />
				</div>
				<asp:GridView ID="grdTaxRates" CssClass="table table-detail" ShowHeader="true" PageIndex="0" AllowPaging="true" PageSize="10" OnRowDataBound="grdTaxRates_OnRowDataBound"
					runat="server" GridLines="None" CellSpacing="-1" OnPageIndexChanging="grdTaxRates_OnPageIndexChanging" AutoGenerateColumns="false">
					<Columns>
						<asp:TemplateField>
							<HeaderTemplate>
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />
							</HeaderTemplate>
							<ItemTemplate>
								<asp:Literal runat="server" ID="litTaxClassId" Text='<%# Eval("TaxClassID") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField>
							<HeaderTemplate>
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.TaxClassName %>" />
							</HeaderTemplate>
							<ItemTemplate>
								<asp:Literal runat="server" ID="litTaxClassName" Text='<%# Eval("Name") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField>
							<HeaderTemplate>
								<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.TaxRate %>" />
							</HeaderTemplate>
							<ItemTemplate>
								<asp:TextBox runat="server" ID="txtTaxRate" CssClass="form-inline" />
							<asp:CompareValidator ControlToValidate="txtTaxRate" Type="Double" ValidateRequestMode="Enabled" CssClass="text-danger" 
								Display="Dynamic" runat="server" ID="cmpTaxRate" ErrorMessage="<%$Tokens:StringResource, admin.common.InvalidPercent %>" />
							</ItemTemplate>
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
			</div>

			<div class="item-action-bar">
				<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
				<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
			</div>
		</div>
	</div>
</asp:Content>
