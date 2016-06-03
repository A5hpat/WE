<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.runsql" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="runsql.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-code"></i>
		<asp:Label ID="lblHeader" runat="server" Text="<%$Tokens:StringResource, admin.menu.RunSQL %>" />
	</h1>
    <div>
		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	</div>
	<div class="item-action-bar">
		<div class="col-list-action-bar">
			<asp:Button runat="server" ID="btnSubmitTop" Text="Submit" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
		</div>
	</div>
    <div id="container">
		<div class="white-ui-box">
			<div class="white-box-heading">				
				<asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.runsql.TextBoxHeader %>" />
			</div>
			<asp:TextBox runat="Server" ID="txtQuery" Rows="25" TextMode="multiline" CssClass="form-control" />
		</div>        
    </div>
	<div class="item-action-bar">
		<div class="col-list-action-bar">
			<asp:Button runat="server" ID="btnSubmit" Text="Submit" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
		</div>
	</div>
</asp:Content>