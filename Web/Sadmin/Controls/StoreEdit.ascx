<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.Controls.StoreEdit" CodeBehind="StoreEdit.ascx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="AJAX" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" %>
<%@ Register TagPrefix="telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>

<asp:Panel ID="pnlEditStore" runat="server" DefaultButton="cmdSave">
	<div class="white-ui-box container">
		<div class="white-box-heading">
			<%= HeaderText  %>
		</div>
		<div class="alert alert-danger" id="divError" runat="server" visible="false">
			<asp:Label runat="server" ID="lblError" />
		</div>
		<asp:Panel ID="pnlMain" Style="text-align: left;" runat="server" Visible="true">
			<div class="form-group">
				<h3>
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.storecontrol.generalheader %>" />
				</h3>
				<div class="row admin-row">
					<div class="col-xs-2">
						<asp:Label CssClass="edit-store-label" AssociatedControlID="txtStoreName" runat="server" Text="<%$ Tokens:StringResource, Global.StoreName %>" />
					</div>
					<div class="col-xs-4">
						<asp:TextBox ID="txtStoreName" runat="server" class="form-control" Text='<%# Datasource.Name %>' placeholder="<%$ Tokens:StringResource, admin.storecontrol.StoreName.Placeholder %>" />
					</div>
				</div>
				<div class="row admin-row">
					<div class="col-xs-2">
						<asp:Label CssClass="edit-store-label" ID="lblChkPublished" AssociatedControlID="chkPublished" runat="server" Text="<%$ Tokens:StringResource, StoreControl.Published %>" />
					</div>
					<div class="col-xs-10">
						<asp:CheckBox ID="chkPublished" runat="server" Checked="<%# Datasource.Published %>" Enabled="<%# (Datasource.Deleted == false && Datasource.IsDefault == false) %>" />
					</div>
				</div>
				<div class="row admin-row">
					<div class="col-xs-2">
						<asp:Label CssClass="edit-store-label" ID="lblSkinID" AssociatedControlID="cmbSkinID" runat="server" Text="<%$ Tokens:StringResource, admin.common.Skin %>" />
					</div>
					<div class="col-xs-4">
						<asp:DropDownList ID="cmbSkinID" class="text-lg" runat="server" />
					</div>
				</div>
				<div class="row admin-row">
					<div class="col-xs-2">
						<asp:Label CssClass="edit-store-label" ID="lblDescription" AssociatedControlID="txtDescription" runat="server" Text="<%$ Tokens:StringResource, StoreControl.Description %>" />
					</div>
					<div class="col-xs-10">
						<asp:TextBox ID="txtDescription" runat="server" class="form-control" TextMode="MultiLine" Text='<%# Datasource.Description %>' />
					</div>
				</div>

				<h3>
					<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.storecontrol.environmentheader %>" />
				</h3>
				<table class="table">
					<tr>
						<th width="182px"></th>
						<th>
							<asp:Label runat="server" Text="<%$ Tokens:StringResource, admin.storecontrol.environment.domain %>" />
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.storecontrol.domain.help %>">
								<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
						</th>
						<th width="100px">
							<asp:Label runat="server" Text="<%$ Tokens:StringResource, admin.storecontrol.environment.port %>" />
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.storecontrol.Port.Help %>">
								<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
						</th>
						<th>
							<asp:Label runat="server" Text="<%$ Tokens:StringResource, admin.storecontrol.environment.directorypath %>" />
							<asp:Label runat="server" CssClass="hover-help" data-toggle="tooltip" ToolTip="<%$ Tokens:StringResource, admin.storecontrol.DirectoryPath.Help %>">
								<i class="fa fa-question-circle fa-lg"></i>
							</asp:Label>
						</th>
					</tr>
					<tr>
						<th>
							<span class="edit-store-label">Production</span>
						</th>
						<td>
							<asp:TextBox ID="txtProductionURI" runat="server" class="form-control" Text='<%# Datasource.ProductionURI %>' placeholder="<%$ Tokens:StringResource, admin.storecontrol.ProductionURL.Placeholder %>" />
						</td>
						<td>
							<asp:TextBox ID="txtProductionPort" runat="server" CssClass="form-control" Text="80" Enabled="false" />
						</td>
						<td>
							<asp:TextBox ID="txtProductionDirectoryPath" runat="server" class="form-control" Text='<%# Datasource.ProductionDirectoryPath %>' placeholder="<%$ Tokens:StringResource, admin.storecontrol.DirectoryPath.Placeholder %>" />
						</td>
					</tr>
					<tr>
						<th>
							<span class="edit-store-label">Staging</span>
						</th>
						<td>
							<asp:TextBox ID="txtStagingURI" class="form-control" runat="server" Text='<%# Datasource.StagingURI %>' placeholder="<%$ Tokens:StringResource, admin.storecontrol.StagingURL.Placeholder %>" />
						</td>
						<td>
							<asp:TextBox ID="txtStagingPort" runat="server" CssClass="form-control" Text="80" Enabled="false" />
						</td>
						<td>
							<asp:TextBox ID="txtStagingDirectoryPath" runat="server" class="form-control" Text='<%# Datasource.StagingDirectoryPath %>' placeholder="<%$ Tokens:StringResource, admin.storecontrol.DirectoryPath.Placeholder %>" />
						</td>
					</tr>
					<tr>
						<th>
							<span class="edit-store-label">
								Development
							</span>
						</th>
						<td>
							<asp:TextBox ID="txtDevURI" CssClass="form-control" runat="server" Text='<%# Datasource.DevelopmentURI %>' placeholder="<%$ Tokens:StringResource, admin.storecontrol.DevelopmentURL.Placeholder %>" />
						</td>
						<td>
							<asp:TextBox ID="txtDevelopmentPort" runat="server" CssClass="form-control" Text='<%# Datasource.DevelopmentPort %>' />
						</td>
						<td>
							<asp:TextBox ID="txtDevelopmentDirectoryPath" runat="server" class="form-control" Text='<%# Datasource.DevelopmentDirectoryPath %>' placeholder="<%$ Tokens:StringResource, admin.storecontrol.DirectoryPath.Placeholder %>" />
						</td>
					</tr>
				</table>

				<asp:PlaceHolder runat="server" ID="phRegisterWithBuySafe" Visible="false">
					<div class="row admin-row">
						<div class="col-xs-2">
							<asp:Label ID="Label3" AssociatedControlID="cbxBuySafe" runat="server" Text="<%$ Tokens:StringResource, admin.storecontrol.AddToBuySafe %>" />
						</div>
						<div class="col-xs-10">
							<asp:CheckBox ID="cbxBuySafe" runat="server" Checked="true" />
							See <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=10000manual&type=buysafe" target="_blank">Manual</a>
						</div>
					</div>
				</asp:PlaceHolder>
				<div style="text-align: center" class="modal_popup_Footer">
					<asp:Button ID="cmdSave" runat="server" CssClass="btn btn-primary btn-sm" Text="<%$ Tokens:StringResource, admin.common.save %>" CommandName="UpdateStore" CommandArgument="<%# Datasource.StoreID %>" OnClick="cmdSave_Click" />
					<asp:Button ID="cmdCancel" runat="server" CssClass="btn btn-default btn-sm" Text="<%$ Tokens:StringResource, admin.common.cancel %>" />
				</div>
				<%--Target control that will trigger this popup is not contained here so we use the PoupControlId property as reference instead--%>
				<AJAX:ModalPopupExtender ID="extEditStorePanel" runat="server"
					PopupControlID="pnlEditStore"
					BackgroundCssClass="modal_popup_background"
					CancelControlID="cmdCancel">
				</AJAX:ModalPopupExtender>
			</div>
		</asp:Panel>
	</div>
</asp:Panel>
