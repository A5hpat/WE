<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.zipcode" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="zipcode.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="zipcodes.aspx" />
	<div class="admin-module">
		<h1>
			<i class="fa fa-map-marker"></i>
			<asp:Literal ID="litHeader" runat="server" />
		</h1>
		<aspdnsf:AlertMessage runat="server" ID="ctlAlertMessage" />
		<div class="item-action-bar">
			<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" CausesValidation="true" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSaveTop" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click" CausesValidation="true" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>
		<div class="white-ui-box">
			<div class="admin-row">
				<div id="divEditZip" class="white-ui-box">
					<div class="row">
						<div class="col-md-4">
							<div class="form-group">
								<span class="text-danger">*</span><asp:Label AssociatedControlID="ddlCountry" runat="server" Text="<%$Tokens:StringResource, admin.countries.Country %>" />
								<asp:DropDownList ID="ddlCountry" CssClass="form-control" runat="server" AutoPostBack="true" EnableViewState="true" />
							</div>
							<div class="form-group">
								<span class="text-danger">*</span><asp:Label AssociatedControlID="txtZipCode" runat="server" Text="Zip Code:" />
								<asp:TextBox ID="txtZipCode" runat="server" CssClass="text-sm" ValidationGroup="Main" MaxLength="10" />
								<asp:RequiredFieldValidator CssClass="text-danger" Display="Dynamic" ErrorMessage="<%$Tokens:StringResource, admin.zipcode.ZipCodeRequired %>" ControlToValidate="txtZipCode"
									ID="RequiredFieldValidator2" ValidationGroup="Main" SetFocusOnError="true" runat="server" />
								<asp:Label runat="server" ID="imgZip" ToolTip="<%$Tokens:StringResource,admin.taxzips.tooltip.imgZip %>" CssClass="hover-help" data-toggle="tooltip">
										<i class="fa fa-question-circle fa-lg"></i>
								</asp:Label>
							</div>
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
								<span class="text-danger">*</span><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.TaxRate %>" />
							</HeaderTemplate>
							<ItemTemplate>
								<asp:TextBox runat="server" ID="txtTaxRate" CssClass="form-inline" ValidationGroup="Main" />
								<asp:RequiredFieldValidator ControlToValidate="txtTaxRate" runat="server" Display="Dynamic" ErrorMessage="<%$Tokens:StringResource, admin.zipcode.TaxRateRequired %>" CssClass="text-danger" ValidateRequestMode="Enabled" ValidationGroup="Main" />
								<asp:CompareValidator ControlToValidate="txtTaxRate" Type="Double" Operator="DataTypeCheck" ValidateRequestMode="Enabled" ValidationGroup="Main" CssClass="text-danger"
									Display="Dynamic" runat="server" ID="cmpTaxRate" ErrorMessage="<%$Tokens:StringResource, admin.common.InvalidPercent %>" />
								<asp:Label runat="server" ID="imgTax" ToolTip="<%$Tokens:StringResource,admin.taxzips.tooltip.imgTax %>" CssClass="hover-help" data-toggle="tooltip">
										<i class="fa fa-question-circle fa-lg"></i>
								</asp:Label>
							</ItemTemplate>
						</asp:TemplateField>
					</Columns>
				</asp:GridView>
			</div>
		</div>
	</div>
	<div class="item-action-bar">
		<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
		<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" CausesValidation="true" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
		<asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click" CausesValidation="true" ValidationGroup="Main" Text="<%$Tokens:StringResource, admin.common.Save %>" />
	</div>
</asp:Content>