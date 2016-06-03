<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.ManualSearch" Codebehind="ManualSearch.ascx.cs" %>
<div class="help-menu-wrap">
	<div id="help-menu">
		<div class="header-help-wrap">
			<asp:Panel runat="server" CssClass="input-group" DefaultButton="lbtManualSearch">
				<asp:TextBox runat="server" ClientIDMode="Static" ID="txtManualSearch" class="form-control js-help-box" placeholder="Search Help" />
				<span class="input-group-btn">
					<asp:LinkButton runat="server" Text="<%$Tokens:StringResource, admin.common.Go %>" ID="lbtManualSearch" class="btn btn-default js-help-button orange-help-button" />
				</span>
			</asp:Panel>
		</div>
	</div>
</div>
