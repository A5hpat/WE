<%@ Control Language="C#" Inherits="AspDotNetStorefrontControls.Config.IntegerEditor" Codebehind="IntegerEditor.ascx.cs" %>

<asp:TextBox runat="server"
	ID="ValueEditor"
	CssClass="form-control"
	Text="<%# Value %>"
	Placeholder=<%# Exists
		? String.Empty 
		: String.Format("(Default) {0}", DefaultValue) %> />

<asp:CompareValidator runat="server"
	Operator="DataTypeCheck"
	Type="Integer"
	ControlToValidate="ValueEditor" 
	CssClass="text-danger"
	ErrorMessage="<%$Tokens:StringResource, admin.editquantitydiscounttable.EnterInteger %>" />