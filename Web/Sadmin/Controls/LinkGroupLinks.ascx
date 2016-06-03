<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.LinkGroupLinks" Codebehind="LinkGroupLinks.ascx.cs" %>
<asp:Repeater runat="server" ID="LinkGroup">
	<ItemTemplate>
		<a href='<%# Eval("Key") %>' 
			class='btn btn-default <%# StringComparer.OrdinalIgnoreCase.Equals(SelectedLink, (string)Eval("Key")) ? "disabled" : String.Empty %>'>

			<asp:Literal runat="server" Text='<%# Eval("Value") %>' />
		</a>
	</ItemTemplate>
</asp:Repeater>