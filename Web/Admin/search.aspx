<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.search"
    MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="search.aspx.cs" %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<h1>
		<i class="fa fa-search"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.search %>" />
	</h1>
	<div class="row">
		<div class="col-lg-7">
			<asp:Literal runat="server" ID="litProductResults" />
		</div>
		<div class="col-lg-5">
			<asp:Literal runat="server" ID="litEntityResults" />
		</div>
	</div>
</asp:Content>
