<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.KitGroupImageUpload" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/Popup.master" Theme="Admin_Default" Codebehind="kitgroupimageupload.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:Content runat="server" ContentPlaceHolderID="head" />
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div class="pnlMain" align="left">
		<h1><%= KitGroup.Name %> Images</h1>
		<h2>Kit Group Image:</h2>
		<div class="form-group">
			<asp:FileUpload ID="flpGroup" runat="server" />
			<asp:Panel ID="pnlGroupImage" runat="server" Visible="false">
				<asp:LinkButton ID="lnkDeleteGroupImage" runat="server" OnClick="lnkDeleteGroupImage_Click"
					Text="<%$Tokens:StringResource, admin.gallery.DeleteImage %>"></asp:LinkButton>
				<asp:Image ID="imgGroupImage" runat="server" />
			</asp:Panel>
		</div>
		<asp:Repeater ID="rptItemImages" runat="server" OnItemCommand="rptItemImages_ItemCommand">
			<HeaderTemplate>
				<h2>Kit Item Images: (<%= KitGroup.Items.Count %>)</h2>
				<table class="table">
			</HeaderTemplate>
			<ItemTemplate>
				<tr>
					<td class="upload_label" align="left" valign="top" style="padding-top: 10px;">
						<asp:Literal ID="Literal1" runat="server" Text='<%# Localize(Container.DataItemAs<KitItemData>().Name) %>'></asp:Literal>
					</td>
					<td class="upload_image" align="left" valign="top" style="padding-top: 10px;">
						<asp:HiddenField ID="hdfKitItemId" runat="server" Value='<%# Container.DataItemAs<KitItemData>().Id %>' />
						<asp:FileUpload ID="flpKitItem" runat="server" />
						<asp:Panel ID="pnlItemImage" runat="server" Visible='<%# Container.DataItemAs<KitItemData>().HasImage %>'>
							<asp:LinkButton ID="lnkDeleteItemImage" runat="server" CommandName="DeleteKitItemImage"
								CommandArgument='<%# Container.DataItemAs<KitItemData>().Id %>'>Delete image</asp:LinkButton>
							<asp:Image ID="imgGroupImage" runat="server" ImageUrl='<%# Container.DataItemAs<KitItemData>().ImagePath %>' />
						</asp:Panel>
					</td>
				</tr>
			</ItemTemplate>
			<FooterTemplate>
				</table>
			</FooterTemplate>
		</asp:Repeater>
		<div class="list-action-bar">
			<asp:Button ID="btnUploadBottom" runat="server" Text="<%$Tokens:StringResource, admin.common.Upload %>" OnClick="btnUpload_Click" CssClass="btn btn-primary" />
			<a href="javascript:self.close();" class="btn btn-default">
				<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Close %>" /></a>
		</div>
	</div>

</asp:Content>