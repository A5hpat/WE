<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.Admin_Controls_LocaleField" Codebehind="LocaleField.ascx.cs" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<asp:Panel ID="pnlMultipleLocales" runat="server" Visible="<%#AppLogic.NumLocaleSettingsInstalled() > 1 %>">
	<asp:Repeater ID="rptLocaleFields" runat="server">
		<HeaderTemplate>
			<table class="table">
		</HeaderTemplate>
		<ItemTemplate>
			<div class="form-group">
				<tr>
					<td>
						<asp:HiddenField ID="LocaleName" runat="server" Value='<%# Eval("Name")%>' />
						<div>
							<span runat="server" visible='<%# RequiredValidation %>' class="text-danger">*</span><asp:Label ID="lblDescription" runat="server" Text='<%# Eval("Description") %>' AssociatedControlID="txtValue" />
						</div>
						<div>
							<asp:Label ID="lblName" runat="server" Text='<%# string.Format("({0})", Eval("Name")) %>' AssociatedControlID="txtValue" />
						</div>
					</td>
					<td>
						<asp:TextBox ID="txtValue" Enabled='<%# Enabled %>' CssClass='<%# TextFieldClass %>' Text='<%# XmlCommon.GetLocaleEntry(Text, (string)Eval("Name"), false) %>' runat="server" />
						<asp:RequiredFieldValidator
							ControlToValidate="txtValue"
							CssClass="text-danger"
							ErrorMessage="Value is Required"
							ID="rfvLocaleField"
							SetFocusOnError="true"
							runat="server"
							Display="Dynamic"
							ValidationGroup="<%# ValidationGroup %>"
							Enabled='<%# RequiredValidation %>' />
					</td>
				</tr>
			</div>
		</ItemTemplate>
		<FooterTemplate>
			</table>
		</FooterTemplate>
	</asp:Repeater>
</asp:Panel>
<asp:Panel ID="pnlSingleLocale" runat="server" Visible="<%#AppLogic.NumLocaleSettingsInstalled() <= 1 %>">
	<asp:TextBox ID="txtValue" Enabled='<%# Enabled %>' CssClass='<%# TextFieldClass %>' Text='<%# XmlCommon.GetLocaleEntry(Text, DefaultLocaleSetting, false) %>' runat="server" />
	<asp:RequiredFieldValidator
		ControlToValidate="txtValue"
		CssClass="text-danger"
		ErrorMessage="Value is Required"
		ID="rfvLocaleField"
		SetFocusOnError="true"
		runat="server"
		Display="Dynamic"
		ValidationGroup="<%# ValidationGroup %>" 
		Enabled='<%# RequiredValidation %>' />
</asp:Panel>