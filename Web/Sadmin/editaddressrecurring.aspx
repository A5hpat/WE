<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.editaddressrecurring" Codebehind="editaddressrecurring.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<title>Edit Address Recurring</title>
	<asp:Literal runat="server" ID="ltStyles" />
</head>
<body>
	<form id="frmEntityEdit" runat="server" enctype="multipart/form-data" method="post">
		<asp:Literal Visible="false" ID="ltOriginalRecurringOrderNumber" runat="server" />
		<asp:Panel ID="pnlAddress" runat="server" Visible="true">
			<div class="admin-row">
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblAddressNickName" AssociatedControlID="txtAddressNickName" runat="server" Text="<%$ Tokens: StringResource, address.cs.49%>" />
					</div>
					<div class="col-sm-3">
						<asp:TextBox ID="txtAddressNickName" MaxLength="50" runat="server" class="text-md" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblCreateaccountaspx33" AssociatedControlID="txtFirstName" runat="server" Text="<%$ Tokens: StringResource, address.cs.2%>" />
					</div>
					<div class="col-sm-3">
						<asp:TextBox ID="txtFirstName" MaxLength="50" runat="server" class="text-md" />
						<asp:RequiredFieldValidator ID="valReqFName" ValidationGroup="addressSave" ControlToValidate="txtFirstName" Display="Dynamic" runat="server" CssClass="text-danger validator-error-adjustments" ErrorMessage="<%$ Tokens: StringResource, address.cs.13%>" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblCreateaccountaspx35" AssociatedControlID="txtLastName" runat="server" Text="<%$ Tokens: StringResource, address.cs.3%>" />
					</div>
					<div class="col-sm-3">
						<asp:TextBox ID="txtLastName" MaxLength="50" runat="server" class="text-md" />
						<asp:RequiredFieldValidator ID="valReqLName" ValidationGroup="addressSave" ControlToValidate="txtLastName" Display="Dynamic" runat="server" CssClass="text-danger validator-error-adjustments" ErrorMessage="<%$ Tokens: StringResource, address.cs.14%>" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblCreateaccountaspx37" AssociatedControlID="txtPhone" runat="server" Text="<%$ Tokens: StringResource, address.cs.4%>" />
					</div>
					<div class="col-sm-3">
						<asp:TextBox ID="txtPhone" MaxLength="25" runat="server" class="text-md" />
						<asp:RequiredFieldValidator ID="valReqPhone" ValidationGroup="addressSave" ControlToValidate="txtPhone" runat="server" Display="Dynamic" CssClass="text-danger validator-error-adjustments" ErrorMessage="<%$ Tokens: StringResource, address.cs.15%>" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblCreateaccountaspx40" AssociatedControlID="txtCompany" runat="server" Text="<%$ Tokens: StringResource, address.cs.5%>" />
					</div>
					<div class="col-sm-3">
						<asp:TextBox ID="txtCompany" MaxLength="100" runat="server" class="text-md" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblAddresscs58" runat="server" AssociatedControlID="ddlResidenceType" Text="<%$ Tokens: StringResource, address.cs.58%>" />
					</div>
					<div class="col-sm-3">
						<asp:DropDownList ID="ddlResidenceType" runat="server" class="text-md" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblCreateaccountaspx41" AssociatedControlID="txtAddress1" runat="server" Text="<%$ Tokens: StringResource, address.cs.6%>" />
					</div>
					<div class="col-sm-3">
						<asp:TextBox ID="txtAddress1" MaxLength="100" runat="server" class="text-md" />
						<asp:RequiredFieldValidator ID="valReqAddr1" ValidationGroup="addressSave" ControlToValidate="txtAddress1" runat="server" CssClass="text-danger validator-error-adjustments" Display="Dynamic" ErrorMessage="<%$ Tokens: StringResource, address.cs.16%>" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblCreateaccountaspx43" AssociatedControlID="txtAddress2" runat="server" Text="<%$ Tokens: StringResource, address.cs.7%>" />
					</div>
					<div class="col-sm-3">
						<asp:TextBox ID="txtAddress2" MaxLength="100" runat="server" class="text-md" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblCreateaccountaspx44" AssociatedControlID="txtSuite" runat="server" Text="<%$ Tokens: StringResource, address.cs.8%>" />
					</div>
					<div class="col-sm-3">
						<asp:TextBox ID="txtSuite" MaxLength="50" runat="server" class="text-md" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblCreateaccountaspx45" AssociatedControlID="txtCity" runat="server" Text="<%$ Tokens: StringResource, address.cs.9%>" />
					</div>
					<div class="col-sm-3">
						<asp:TextBox ID="txtCity" MaxLength="50" runat="server" class="text-md" />
						<asp:RequiredFieldValidator ID="valReqCity" ControlToValidate="txtCity" runat="server" ValidationGroup="addressSave" CssClass="text-danger validator-error-adjustments" Display="Dynamic" ErrorMessage="<%$ Tokens: StringResource, address.cs.17%>" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblCreateaccountaspx52" AssociatedControlID="ddlCountry" runat="server" Text="<%$ Tokens: StringResource, address.cs.53%>" />
					</div>
					<div class="col-sm-3">
						<asp:DropDownList ID="ddlCountry" runat="server" OnSelectedIndexChanged="ddlCountry_OnChange" AutoPostBack="True" class="text-md" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblCreateaccountaspx47" AssociatedControlID="ddlState" runat="server" Text="<%$ Tokens: StringResource, address.cs.10%>" />
					</div>
					<div class="col-sm-3">
						<asp:DropDownList ID="ddlState" runat="server" class="text-md" />
					</div>
				</div>
				<div class="row form-group">
					<div class="col-sm-3">
						<asp:Label ID="lblCreateaccountaspx49" AssociatedControlID="txtZip" runat="server" Text="<%$ Tokens: StringResource, address.cs.12%>" />
					</div>
					<div class="col-sm-3">
						<asp:TextBox ID="txtZip" MaxLength="10" runat="server" class="text-md" />
						<asp:RequiredFieldValidator ID="valReqZip" ControlToValidate="txtZip" runat="server" ValidationGroup="addressSave" CssClass="text-danger validator-error-adjustments" Display="Dynamic" ErrorMessage="<%$ Tokens: StringResource, address.cs.18%>" />
					</div>
				</div>
			</div>			
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Literal ID="litCCForm" runat="server" />	
					<asp:Panel ID="pnlErrorMsg" runat="Server" HorizontalAlign="Left" Style="margin-left: 20px;">
						<asp:Label ID="ErrorMsgLabel" runat="server" Font-Bold="true" CssClass="text-danger" />
					</asp:Panel>				
				</div>
			</div>
			<div class="row form-group">
				<div class="col-sm-3">
					<asp:Button ID="btnSaveAddress" runat="server" CssClass="btn btn-primary btn-sm" ValidationGroup="addressSave" OnClick="btnSaveAddress_Click" />
				</div>
			</div>
		</asp:Panel>
	</form>
</body>
</html>