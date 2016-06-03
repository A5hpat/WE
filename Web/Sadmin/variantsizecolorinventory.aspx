<%@ Page Language="c#" Theme="Admin_Default" Inherits="AspDotNetStorefrontAdmin.variantsizecolorinventory" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="variantsizecolorinventory.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="variants.aspx" Blacklist="" />

	<div class="wrapper row" id="divwrapper" runat="server">
		<div class="main-breadcrumb">
			Status: Editing Inventory For:
			<asp:HyperLink ID="lnkEditingInventoryFor" runat="server" />
		</div>
		<h1>
			<i class="fa fa-cube"></i>
			<asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.common.Inventory %>" />
		</h1>

		<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />
		<asp:Panel runat="server" ID="pnlMain" DefaultButton="btnUpdate">
			<div class="list-action-bar">
				<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.saveandclose %>" />
				<asp:Button ID="btnUpdate" CssClass="btn btn-primary" runat="server" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnUpdate_Click" />
			</div>
			<div class="white-ui-box">
				<asp:GridView ID="grdInventory" runat="server"
					AutoGenerateColumns="false"
					GridLines="None"
					CssClass="table table-detail">
					<Columns>
						<asp:TemplateField HeaderText="Combinations">
							<ItemTemplate>
								<asp:HiddenField ID="hdnVariantId" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "VariantId") %>' />
								<asp:Literal ID="ltSize" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Size") %>' />
								<asp:Literal runat="server" Visible='<%# !String.IsNullOrEmpty((string)DataBinder.Eval(Container.DataItem, "Size")) && !String.IsNullOrEmpty((string)DataBinder.Eval(Container.DataItem, "Color")) %>'>,</asp:Literal>
								<asp:Literal ID="ltColor" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Color") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Inventory">
							<ItemTemplate>
								<asp:TextBox ID="txtInventory" runat="server" Columns="8" CausesValidation="true" Text='<%# DataBinder.Eval(Container.DataItem, "Inventory") %>' />
								<asp:RangeValidator ID="rvInventory" ControlToValidate="txtInventory" runat="server" MinimumValue="0" MaximumValue="99999999" ErrorMessage="*" />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="GTIN">
							<ItemTemplate>
								<asp:TextBox ID="txtGTIN" runat="server" MaxLength="14" Columns="14" Text='<%# DataBinder.Eval(Container.DataItem, "GTIN") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Warehouse Location">
							<ItemTemplate>
								<asp:TextBox ID="txtWarehouseLocation" runat="server" Columns="20" Text='<%# DataBinder.Eval(Container.DataItem, "WarehouseLocation") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="VendorId">
							<ItemTemplate>
								<asp:TextBox ID="txtVendorId" runat="server" Columns="8" Text='<%# DataBinder.Eval(Container.DataItem, "VendorId") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Full Vendor SKU">
							<ItemTemplate>
								<asp:TextBox ID="txtFullVendorSku" runat="server" Columns="14" Text='<%# DataBinder.Eval(Container.DataItem, "FullSku") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Weight Delta">
							<ItemTemplate>
								<asp:TextBox ID="txtWeightDelta" runat="server" Columns="8" Text='<%# DataBinder.Eval(Container.DataItem, "WeightDelta") %>' />
								<asp:RangeValidator ID="rvWeightDelta" ControlToValidate="txtWeightDelta" runat="server" MinimumValue="0" MaximumValue="99999999" ErrorMessage="*" />
							</ItemTemplate>
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
			</div>
			<div class="list-action-bar">
				<asp:HyperLink runat="server" ID="btnCloseBottom" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
				<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.saveandclose %>" />
				<asp:Button ID="btnUpdateBottom" CssClass="btn btn-primary" runat="server" Text="<%$Tokens:StringResource, admin.common.save %>" OnClick="btnUpdate_Click" />
			</div>
		</asp:Panel>
	</div>
</asp:Content>
