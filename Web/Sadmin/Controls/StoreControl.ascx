<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.StoreControl" Codebehind="StoreControl.ascx.cs" %>
<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsf" %>
<%@ Register TagPrefix="AJAX" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="aspdnsf" TagName="StoreEdit" Src="StoreEdit.ascx" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>
<script type="text/javascript">
	function pageLoad() {
		$('[data-toggle="tooltip"]').tooltip({ placement: 'right', container: 'body' });
	};
	function bindConfirmDelete() {

		var deleteConfirmPrompt = "Are you sure you want to delete this store?  This will soft-delete this store but will remove all mappings. Click Ok to continue, otherwise cancel.";
		var unDeleteConfirmPrompt = "Are you sure you want to un-delete this store?";

		$('.delete-box > input[type=checkbox]').on('change', null, deleteConfirmPrompt, confirmAndPostback);
		$('.undelete-box > input[type=checkbox]').on('change', null, unDeleteConfirmPrompt, confirmAndPostback);
		function confirmAndPostback(event) {
			if (confirm(event.data)) {
				// ASP.NET postback
				Sys.WebForms.PageRequestManager.getInstance().beginAsyncPostBack();
			} else {
				// Uncheck
				$(this).prop('checked', !$(this).prop('checked'));

				// Stop the event
				event.preventDefault();
				event.stopPropagation();
				return false;
			}
		};
	}
	jQuery(document).ready(bindConfirmDelete);
	Sys.WebForms.PageRequestManager.getInstance().add_endRequest(bindConfirmDelete);
</script>
<asp:UpdatePanel runat="server" ID="updMain">
	<ContentTemplate>
		<asp:UpdateProgress runat="server">
			<ProgressTemplate>
				<div class="progress">
					<div class="progress-bar progress-bar-striped active loading-bar-positioning" role="progressbar">
						<div class="loading-bar-text">
							<asp:Literal ID="litProcessing" runat="server" Text="<%$ Tokens:StringResource,admin.common.processing %>" />
						</div>
					</div>
				</div>
				<div class="loading-page-cover">
				</div>
			</ProgressTemplate>
		</asp:UpdateProgress>
		<telerik:RadWindowManager ID="rwMan" runat="server" />
		<div class="item-action-bar">
			<div class="col-list-action-bar">
				<asp:HyperLink runat="server" CssClass="btn btn-default" NavigateUrl="skinselector.aspx" Text="<%$ Tokens: StringResource, admin.stores.ManageSkins %>" />
				<asp:Button ID="cmdAddStore" runat="server" Class="btn btn-action" Text="<%$ Tokens:StringResource, StoreControl.AddStore %>" />
			</div>
		</div>
		<aspdnsf:StoreEdit ID="ctrlAddStore" runat="server" CssClass="modal_popup_Content" HeaderText="<%$ Tokens:StringResource, StoreControl.AddStore %>" PopupTargetControlID="cmdAddStore" VisibleOnPageLoad="false" OnUpdatedChanges="ctrlStore_UpdatedChanges" />
		<telerik:RadGrid ID="grdStores" runat="server" CssClass="adnsf-radgrid table" GridLines="None" AllowSorting="true" OnItemCreated="grdStores_ItemCreated" OnSortCommand="grdStores_SortCommand" OnItemCommand="grdStores_ItemCommand" OnItemDataBound="grdStores_ItemDataBound">
			<HeaderContextMenu>
				<CollapseAnimation Type="OutQuint" Duration="200" />
			</HeaderContextMenu>
			<ClientSettings>
				<Resizing AllowColumnResize="false" EnableRealTimeResize="false" ResizeGridOnColumnResize="true" ClipCellContentOnResize="false" />
			</ClientSettings>
			<MasterTableView AutoGenerateColumns="False">
				<RowIndicatorColumn>
					<HeaderStyle Width="20px" />
				</RowIndicatorColumn>
				<ExpandCollapseColumn>
					<HeaderStyle Width="20px" />
				</ExpandCollapseColumn>
				<Columns>
					<telerik:GridTemplateColumn>
						<ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" />
						<HeaderTemplate>
							<asp:Label ID="lblStoreIdHeader" runat="server" Text="<%$ Tokens:StringResource, admin.common.id %>" />
						</HeaderTemplate>
						<ItemTemplate>
							<asp:Label ID="lblStoreID" runat="server" Text='<%# DataItemAs<Store>(Container).StoreID %>' />
						</ItemTemplate>
					</telerik:GridTemplateColumn>
					<telerik:GridTemplateColumn>
						<HeaderStyle Width="20%" />
						<ItemStyle Width="20%" />
						<HeaderTemplate>
							<asp:Label ID="lblStoreNameHeader" runat="server" Text="<%$ Tokens:StringResource, Global.StoreName %>" />
						</HeaderTemplate>
						<ItemTemplate>
							<div style="padding-left: 10px;">
								<asp:LinkButton ID="lnkStoreName" runat="server" Text='<%# DataItemAs<Store>(Container).Name %>' />
							</div>
							<aspdnsf:StoreEdit ID="ctrlEditStore" runat="server"
								CssClass="modal_popup_Content"
								ThisCustomer='<%# ThisCustomer %>'
								HeaderText="Edit Store"
								PopupTargetControlID="lnkStoreName"
								VisibleOnPageLoad="false"
								Datasource='<%# DataItemAs<Store>(Container) %>' />
						</ItemTemplate>
					</telerik:GridTemplateColumn>
					<telerik:GridTemplateColumn>
						<HeaderTemplate>
							<div>
								<asp:Label ID="lblDomains" runat="server" Text="<%$ Tokens:StringResource, admin.storecontrol.Domains %>" />
							</div>
						</HeaderTemplate>
						<ItemTemplate>
							<table>
								<tr class="url_area grid-border-bottom">
									<td class="tdUrlCaption" style="border-style: none;">
										<asp:Literal ID="Literal1" runat="server" Text="<%$ Tokens:StringResource, StoreControl.ProductionURI %>" />
									</td>
									<td class="tdUrl" style="border-style: none;">
										<asp:HyperLink ID="lnkProductionUri" runat="server"
											Text='<%# string.Format("http://{0}{1}{2}",
												DataItemAs<Store>(Container).ProductionURI,
												string.IsNullOrEmpty(DataItemAs<Store>(Container).ProductionPort)
													? string.Empty
													: string.Format(":{0}", DataItemAs<Store>(Container).ProductionPort),
												DataItemAs<Store>(Container).ProductionDirectoryPath) %>'
											NavigateUrl='<%# string.Format("http://{0}{1}{2}",
												DataItemAs<Store>(Container).ProductionURI,
												string.IsNullOrEmpty(DataItemAs<Store>(Container).ProductionPort)
													? string.Empty
													: string.Format(":{0}", DataItemAs<Store>(Container).ProductionPort),
												DataItemAs<Store>(Container).ProductionDirectoryPath) %>'
											Target="_blank">
										</asp:HyperLink>
									</td>
								</tr>
								<tr class="url_area grid-border-bottom">
									<td class="tdUrlCaption" style="border-style: none;">
										<asp:Literal ID="Literal2" runat="server" Text="<%$ Tokens:StringResource, StoreControl.StagingURI %>" />
									</td>
									<td class="tdUrl" style="border-style: none;">
										<asp:HyperLink ID="lnkStagingURI" runat="server"
											Text='<%# string.Format("http://{0}{1}{2}",
												DataItemAs<Store>(Container).StagingURI,
												string.IsNullOrEmpty(DataItemAs<Store>(Container).StagingPort)
													? string.Empty
													: string.Format(":{0}", DataItemAs<Store>(Container).StagingPort),
												DataItemAs<Store>(Container).StagingDirectoryPath) %>'
											NavigateUrl='<%# string.Format("http://{0}{1}{2}",
												DataItemAs<Store>(Container).StagingURI,
												string.IsNullOrEmpty(DataItemAs<Store>(Container).StagingPort)
													? string.Empty
													: string.Format(":{0}", DataItemAs<Store>(Container).StagingPort),
												DataItemAs<Store>(Container).StagingDirectoryPath) %>'
											Target="_blank" />
									</td>
								</tr>
								<tr class="url_area">
									<td class="tdUrlCaption" style="border-style: none;">
										<asp:Literal ID="Literal3" runat="server" Text="<%$ Tokens:StringResource, StoreControl.DevelopmentURI %>" />
									</td>
									<td class="tdUrl" style="border-style: none;">
										<asp:HyperLink ID="lnkDevelopmentURI" runat="server"
											Text='<%# string.Format("http://{0}{1}{2}",
												DataItemAs<Store>(Container).DevelopmentURI,
												string.IsNullOrEmpty(DataItemAs<Store>(Container).DevelopmentPort)
													? string.Empty
													: string.Format(":{0}", DataItemAs<Store>(Container).DevelopmentPort),
												DataItemAs<Store>(Container).DevelopmentDirectoryPath) %>'
											NavigateUrl='<%# string.Format("http://{0}{1}{2}",
												DataItemAs<Store>(Container).DevelopmentURI,
												string.IsNullOrEmpty(DataItemAs<Store>(Container).DevelopmentPort)
													? string.Empty
													: string.Format(":{0}", DataItemAs<Store>(Container).DevelopmentPort),
												DataItemAs<Store>(Container).DevelopmentDirectoryPath) %>'
											Target="_blank" />
										<%--<%# Eval("DevelopmentURI") %>--%>
									</td>
								</tr>
							</table>
						</ItemTemplate>
					</telerik:GridTemplateColumn>
					<%--Default column--%>
					<telerik:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, StoreControl.Default %>">
						<HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemTemplate>
							<aspdnsf:DataCheckBox ID="chkDefault" runat="server"
								Checked='<%# DataItemAs<Store>(Container).IsDefault %>'
								Enabled='<%# Datasource.Count > 1 %>'
								Data='<%# DataItemAs<Store>(Container).StoreID %>'
								AutoPostBack="true"
								Visible='<%# DataItemAs<Store>(Container).Deleted == false && DataItemAs<Store>(Container).Published %>' />
						</ItemTemplate>
					</telerik:GridTemplateColumn>
					<%--Published column--%>
					<telerik:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, StoreControl.Published %>">
						<HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemTemplate>
							<aspdnsf:DataCheckBox ID="cbxPublish" runat="server"
								ToolTip='<%# PublishText(DataItemAs<Store>(Container)) %>'
								CommandArgument='<%# DataItemAs<Store>(Container).StoreID %>'
								Data='<%# DataItemAs<Store>(Container).StoreID %>'
								Checked='<%# PublishText(DataItemAs<Store>(Container)).ToLower()=="un-publish" %>'
								Enabled='<%# Datasource.Count > 1 && DataItemAs<Store>(Container).Deleted == false && DataItemAs<Store>(Container).IsDefault == false %>'
								AutoPostBack="true"
								OnCheckedChanged="cbxPublish_CheckedChanged" />
						</ItemTemplate>
					</telerik:GridTemplateColumn>
					<%--Delete column--%>
					<telerik:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, admin.common.deleted %>">
						<HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemTemplate>
							<aspdnsf:DataCheckBox ID="cbxDelete" runat="server"
								ToolTip='<%# DeleteText(DataItemAs<Store>(Container)) %>'
								CommandArgument='<%# DataItemAs<Store>(Container).StoreID %>'
								Data='<%# DataItemAs<Store>(Container).StoreID %>'
								Checked='<%# DeleteText(DataItemAs<Store>(Container)).ToLower() == "un-delete" %>'
								Enabled='<%# Datasource.Count > 1 && DataItemAs<Store>(Container).IsDefault == false %>'
								OnCheckedChanged="cbxDelete_CheckedChanged" />
						</ItemTemplate>
					</telerik:GridTemplateColumn>
					<%--Clone column--%>
					<telerik:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, Global.CloneButtonText %>">
						<HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemTemplate>
							<asp:LinkButton ID="btnCloneStore" runat="server"
								CssClass="clone-link fa-copy"
								ToolTip="Clone Store"
								CommandName='<%# DataItemAs<Store>(Container).StoreID %>'
								CommandArgument='<%# DataItemAs<Store>(Container).StoreID %>'
								Text="<%$ Tokens: StringResource, admin.common.Clone %>"
								Visible='<%# Datasource.Count > 1 && DataItemAs<Store>(Container).Deleted == false && DataItemAs<Store>(Container).Published  %>' />
							<aspdnsf:StoreEdit ID="StoreEdit1" runat="server"
								CssClass="modal_popup_Content"
								CloneMode="true"
								ThisCustomer='<%# ThisCustomer %>'
								HeaderText="<i class='fa fa-files-o'></i> Clone Store"
								PopupTargetControlID="btnCloneStore"
								VisibleOnPageLoad="false"
								Datasource='<%# DataItemAs<Store>(Container) %>' />
						</ItemTemplate>
					</telerik:GridTemplateColumn>
					<%--Copy to Store column--%>
					<telerik:GridTemplateColumn HeaderText="Copy Store Mappings">
						<HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemTemplate>
							<asp:LinkButton ID="btnCopyStore" runat="server"
								CssClass="clone-link fa-cogs"
								ToolTip="Copy Store Settings"
								Text="<%$ Tokens: StringResource, admin.common.Copy %>"
								Visible='<%# Datasource.Count > 1 && DataItemAs<Store>(Container).Deleted == false && DataItemAs<Store>(Container).Published && DataItemAs<Store>(Container).IsDefault == false %>' />
							<%-- panel to show store selection for which to copy store to --%>
							<div id="pnlCopyFromStore" runat="server" style="display: none;" class="white-ui-box container">
								<div class="white-box-heading"><i class="fa fa-cogs"></i>Copy Store Mappings</div>
								<asp:Panel ID="pnlCopyStoreMain" runat="server" Visible="true">
									<div class="row admin-row">
										<div class="col-sm-12">
											Please select which store mappings you want to copy into the current selected store.
											Keep in mind that that this will <span class="info-just-color"><b>overwrite</b></span> the mappings on the destination store.
											<span class="text-danger"><b>You cannot undo this action</b></span>, click Ok if you wish to proceed.
										</div>
									</div>
									<div class="row admin-row">
										<div class="col-sm-12">
											Copy Mappings From:&nbsp;
											<%--datasource and values will be populated on code-behind upon itemdatabound--%>
											<asp:DropDownList ID="cboCopystoreFrom" runat="server" />
											&nbsp;<i class="fa fa-arrow-right"></i>&nbsp;                                        
											Into:
											&nbsp;<asp:Label ID="lblCopyToStore" runat="server" Text='<%# DataItemAs<Store>(Container).Name %>' />
										</div>
									</div>
								</asp:Panel>
								<div class="modal_popup_Footer">
									<asp:Button ID="btnCopyStoreFrom" runat="server" CssClass="btn btn-primary btn-sm" Text="<%$ Tokens:StringResource, admin.common.save %>"
										CommandName="CopyStore"
										CommandArgument='<%# DataItemAs<Store>(Container).StoreID %>' />
									<asp:Button ID="btnCancelCopyStore" runat="server" CssClass="btn btn-default btn-sm" Text="<%$ Tokens:StringResource, admin.common.cancel %>" />
								</div>
							</div>
							<AJAX:ModalPopupExtender ID="extCopyFromStore" runat="server"
								PopupControlID="pnlCopyFromStore"
								BackgroundCssClass="modal_popup_background"
								CancelControlID="btnCancelCopyStore"
								TargetControlID="btnCopyStore">
							</AJAX:ModalPopupExtender>
						</ItemTemplate>
					</telerik:GridTemplateColumn>
					<%--Edit column--%>
					<telerik:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, admin.common.edit %>">
						<HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
						<ItemTemplate>
							<asp:LinkButton ID="btnEditStore" runat="server" Text="Edit" CssClass="edit-link fa-share" CommandName="E_Edit" CommandArgument='<%# DataItemAs<Store>(Container).StoreID %>' />
							<aspdnsf:StoreEdit ID="ctrlEditStoreUsingButton" runat="server"
								CssClass="modal_popup_Content"
								ThisCustomer='<%# ThisCustomer %>'
								HeaderText="<i class='fa fa-share'></i> Edit Store"
								PopupTargetControlID="btnEditStore"
								VisibleOnPageLoad="false"
								Datasource='<%# DataItemAs<Store>(Container) %>' />
						</ItemTemplate>
					</telerik:GridTemplateColumn>
				</Columns>
			</MasterTableView>
		</telerik:RadGrid>
		<br />
		<asp:HiddenField ID="txtY" runat="server" />
		<asp:HiddenField ID="txtX" runat="server" />
	</ContentTemplate>
</asp:UpdatePanel>