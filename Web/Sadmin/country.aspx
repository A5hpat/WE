<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.country" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="country.aspx.cs" %>


<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div class="admin-module">

		<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="Countries.aspx" />

		<aspdnsf:AlertMessage runat="server" ID="ctlAlertMessage" />

		<h1>
			<i class="fa fa-globe"></i>
			<asp:Literal ID="litHeader" runat="server" />
		</h1>
		<div class="item-action-bar">
			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:Button runat="server"
				ID="btnCloseTop"
				CssClass="btn btn-default"
				OnClick="btnClose_Click"
				ValidationGroup="gAdd"
				Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />

			<asp:Button runat="server"
				ID="btnSubmitTop"
				CssClass="btn btn-primary"
				OnClick="btnSubmit_Click"
				ValidationGroup="gAdd"
				Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>

		<div class="admin-row">
			<div id="divEditCountry" class="white-ui-box">
				<div class="row">
					<div class="col-md-4">
						<div class="form-inline">
							<asp:Label AssociatedControlID="litCountryId" runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />:
							<asp:Literal ID="litCountryId" runat="server" />
						</div>
						<div class="form-inline">
							<asp:Label AssociatedControlID="cbxPublished" runat="server" Text="<%$Tokens:StringResource, admin.Common.Published %>" />:
							<asp:CheckBox ID="cbxPublished" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtCountryName" runat="server" Text="<%$Tokens:StringResource, admin.countries.Country %>" />
							<asp:TextBox ID="txtCountryName" runat="server" CssClass="form-control" ValidationGroup="gAdd"></asp:TextBox>
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.countries.tooltip.imgCountryName %>">
								<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
							<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.common.FillinName %>" ControlToValidate="txtCountryName" Display="Dynamic" CssClass="text-danger" ID="RequiredFieldValidator2" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtTwoLetterISOCode" runat="server" Text="<%$Tokens:StringResource, admin.countries.TwoLetterISOCode %>" />
							<asp:TextBox ID="txtTwoLetterISOCode" runat="server" CssClass="form-control" ValidationGroup="gAdd"></asp:TextBox>
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.countries.tooltip.img2LetterISOCode %>">
								<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
							<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.countries.FillInISO %>" ControlToValidate="txtTwoLetterISOCode" Display="Dynamic" CssClass="text-danger" ID="RequiredFieldValidator3" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtThreeLetterISOCode" runat="server" Text="<%$Tokens:StringResource, admin.countries.ThreeLetterISOCode %>" />
							<asp:TextBox ID="txtThreeLetterISOCode" runat="server" CssClass="form-control" ValidationGroup="gAdd"></asp:TextBox>
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.countries.tooltip.img3LetterISOCode %>">
								<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
							<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.countries.FillInISO %>" ControlToValidate="txtThreeLetterISOCode" Display="Dynamic" CssClass="text-danger" ID="RequiredFieldValidator8" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtNumericISOCode" runat="server" Text="<%$Tokens:StringResource, admin.countries.NumericISOCode %>" />
							<asp:TextBox ID="txtNumericISOCode" runat="server" CssClass="form-control" ValidationGroup="gAdd"></asp:TextBox>
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$Tokens:StringResource, admin.countries.tooltip.imgNumericISOCode %>">
								<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
							<asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.countries.FillInISO %>" ControlToValidate="txtNumericISOCode" Display="Dynamic" CssClass="text-danger" ID="RequiredFieldValidator9" ValidationGroup="gAdd" SetFocusOnError="true" runat="server" />
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="cbxPostalCodeRequired" runat="server" Text="<%$Tokens:StringResource, admin.countries.PostalCodeRequired %>" />
							<asp:CheckBox ID="cbxPostalCodeRequired" runat="server" />
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="txtPostalCodeRegEx" runat="server" Text="<%$Tokens:StringResource, admin.countries.PostalCodeRegex %>" />
							<asp:TextBox ID="txtPostalCodeRegEx" runat="Server" CssClass="form-control" ValidationGroup="gAdd"></asp:TextBox>
						</div>
						<div class="form-group">
							<asp:Label AssociatedControlID="txtPostalCodeExample" runat="server" Text="<%$Tokens:StringResource, admin.countries.PostalCodeExample %>" />
							<asp:TextBox ID="txtPostalCodeExample" runat="Server" CssClass="form-control" ValidationGroup="gAdd"></asp:TextBox>
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtDisplayOrder" runat="server" Text="<%$Tokens:StringResource, admin.common.DisplayOrder %>" />
							<asp:TextBox ID="txtDisplayOrder" runat="Server" CssClass="form-control" ValidationGroup="gAdd">1</asp:TextBox>
							<asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.common.EnterDisplayOrder %>" ValidationGroup="gAdd" ControlToValidate="txtDisplayOrder" CssClass="text-danger"></asp:RequiredFieldValidator>
						</div>
					</div>
				</div>
			</div>
			<asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" runat="server" EnableClientScript="true" ShowSummary="false" Enabled="true" />

			<div class="white-ui-box">
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
				<asp:HyperLink runat="server"
					CssClass="btn btn-default"
					NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
					Text="<%$Tokens:StringResource, admin.common.close %>" />

				<asp:Button runat="server" 
					ID="btnClose" 
					CssClass="btn btn-default" 
					OnClick="btnClose_Click" 
					ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
				
				<asp:Button runat="server" 
					ID="btnSubmit" 
					CssClass="btn btn-primary" 
					OnClick="btnSubmit_Click" 
					ValidationGroup="gAdd" 
					Text="<%$Tokens:StringResource, admin.common.Save %>" />
			</div>
		</div>
	</div>
</asp:Content>
