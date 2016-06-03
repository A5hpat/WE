<%@ Control Language="C#" AutoEventWireup="true" Inherits="Admin_Controls_StoreNavigator" Codebehind="StoreNavigator.ascx.cs" %>
<div class="dropdown store-dropdown">
	<a class="dropdown-toggle" type="button" id="storeDropdown" data-toggle="dropdown">
		<span class="fa fa-shopping-cart"></span>
		Go To Store
		<span class="caret"></span>
	</a>
	<ul class="dropdown-menu" role="menu" aria-labelledby="storeDropdown">
		<asp:Repeater runat="server" ID="storeList">
			<ItemTemplate>
				<li role="presentation">
					<a role="menuitem" tabindex="-1" href="<%# Eval("Url") %>" target="_blank">
						<%# Eval("Name") %>
					</a>
				</li>
			</ItemTemplate>
		</asp:Repeater>
	</ul>
</div>
