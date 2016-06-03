<%@ Control Language="C#" Inherits="AspDotNetStorefrontControls.Config.DecimalEditor" Codebehind="DecimalEditor.ascx.cs" %>

<asp:TextBox runat="server"
	ID="ValueEditor"
	CssClass="form-control"
	Text="<%# Value %>"
	Placeholder=<%# Exists
		? String.Empty 
		: String.Format("(Default) {0}", DefaultValue) %> />

<ajaxToolkit:FilteredTextBoxExtender runat="server"
	TargetControlID="ValueEditor"
	FilterType="Numbers, Custom"
	ValidChars="." />
