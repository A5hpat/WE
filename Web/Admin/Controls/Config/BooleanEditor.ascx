<%@ Control Language="C#" Inherits="AspDotNetStorefrontControls.Config.BooleanEditor" Codebehind="BooleanEditor.ascx.cs" %>

<asp:RadioButtonList runat="server"
	ID="ValueEditor"
	CssClass="horizontal-radio-helper"
	RepeatLayout="Flow"
	RepeatDirection="Horizontal">
	<asp:ListItem Text="Yes" Value="true" />
	<asp:ListItem Text="No" Value="false" />
</asp:RadioButtonList>
