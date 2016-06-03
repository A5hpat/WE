<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.skinselector" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="skinselector.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-eye"></i>
		<asp:Label ID="lblHeader" runat="server" Text="<%$Tokens:StringResource, admin.menu.SkinSelector %>" />
	</h1>
	<div class="item-action-bar clearfix">
		<div class="other-actions">
			<div class="form-inline">
				<div class="form-group">
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.SkinSelector.StoreLabel %>" />
					<asp:DropDownList runat="server" ID="StoreSelector" AutoPostBack="true" OnSelectedIndexChanged="StoreSelector_SelectedIndexChanged" CssClass="store-select-list form-control" />
					<asp:HiddenField runat="server" ID="SelectedStoreSkinName" />
				</div>
			</div>
		</div>
	</div>
	<div>
		<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	</div>
	<div class="skin-management-page page-wrap">
		<div class="row">
			<div class="col-sm-3">
				<div class="skin-navigation">
					<asp:Repeater runat="server" ID="SkinNavigationList" OnItemDataBound="SkinNavigation_ItemDataBound">
						<ItemTemplate>
							<a href='<%# "#" + Eval("Name") %>' class="skin-item-link" id='<%# "skin-item-link-" + Eval("Name") %>'>
								<asp:Label runat="server" ID="CurrentSkinIndicator" CssClass="skin-indicator" Visible="false"></asp:Label>
								<div class="skin-image-icon-wrap">
									<img id="SkinImageIcon" runat="server" src='<%# Eval("PreviewUrl") %>' alt='<%# Eval("Name") %>' class="skin-image-icon" />
								</div>
								<span class="skin-link-name">
									<asp:Literal runat="server" ID="DisplayName" Text='<%# Eval("DisplayName") %>' />
								</span>
								<div style="clear: both;"></div>
							</a>
						</ItemTemplate>
						<FooterTemplate>
							<div style="clear: both;"></div>
						</FooterTemplate>
					</asp:Repeater>
				</div>
			</div>
			<div class="col-sm-9">
				<asp:Repeater runat="server" ID="SkinInfo" OnItemDataBound="SkinInfo_ItemDataBound">
					<ItemTemplate>
						<div id="<%# Eval("Name") %>" class="skin-info-item">
							<div class="item-action-bar">
								<div class="col-list-action-bar">
									<asp:HyperLink runat="server" ID="PreviewSkin" CssClass="btn btn-default" Text="Preview" Target="_blank" />
									<asp:Button runat="server" ID="SetSkin" OnCommand="SetSkin_Click" OnClientClick="return confirmApplySkin()" CssClass="btn btn-primary" Text="Apply Skin" CommandArgument='<%# Eval("Name") %>' />
								</div>
							</div>
							<div class="skin-description-wrap">
								<span class="skin-name">
									<asp:Literal runat="server" ID="DisplayName" Text='<%# Eval("DisplayName") %>' />
								</span>
								<span id="SkinDescriptionContainer" runat="server" class="skin-description">-
									<asp:Literal runat="server" ID="Description" Text='<%# Eval("Description") %>' />
								</span>
							</div>
							<div class="white-ui-box skin-preview-padding">
								<asp:Panel runat="server" ID="NoPreviewAvailable" Visible="false" CssClass="no-image-preview">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.SkinSelector.NoImage %>" />
								</asp:Panel>
								<img id="SkinImage" runat="server" src='<%# Eval("PreviewUrl") %>' alt='<%# Eval("Name") %>' class="skin-image" />
							</div>
							<div class="item-action-bar">
								<div class="col-list-action-bar">
									<asp:HyperLink runat="server" ID="PreviewSkinBottom" CssClass="btn btn-default" Text="Preview" Target="_blank" />
									<asp:Button runat="server" ID="SetSkinBottom" OnCommand="SetSkin_Click" CssClass="btn btn-primary" Text="Apply Skin" CommandArgument='<%# Eval("Name") %>' />
								</div>
							</div>
						</div>
					</ItemTemplate>
					<FooterTemplate>
						<div style="clear: both;"></div>
					</FooterTemplate>
				</asp:Repeater>
			</div>
		</div>
	</div>
	<script type="text/javascript">
		var selectedStoreSkinName = $('input[id$="_SelectedStoreSkinName"]').val();
		setSkinView(selectedStoreSkinName);

		$('.skin-item-link').click(function (event) {
			var href = $(this).attr('href');
			setSkinView(href.substr(href.indexOf("#") + 1));
			event.preventDefault();
		});

		function setSkinView(skin) {
			$('.skin-info-item').each(function () {
				if ($(this).attr('id') === skin) {
					$(this).show();
				}
				else {
					$(this).hide();
				}
			});

			$('.skin-item-link').each(function () {
				if ($(this).attr('id') === 'skin-item-link-' + skin) {
					$(this).addClass('selected');
				}
				else {
					$(this).removeClass('selected');
				}
			});
		}

		function confirmApplySkin() {
			return confirm('<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.SkinSelector.ApplySkinConfirmMessage %>" />');
		}
	</script>
</asp:Content>