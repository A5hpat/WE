<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.KitEdit" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="kit.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" TagName="KitGroupTemplate" Src="controls/editkitgrouptemplate.ascx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:Content ID="Content1" ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
	<h1>
		<i class="fa fa-th"></i>
		<asp:Label ID="lblHeader" runat="server" Text="<%$Tokens:StringResource, admin.title.editkit2 %>" />
	</h1>
	<div>
		Editing Kit :
		<asp:Literal ID="ltProduct" runat="server" />
		<div>
			Please enter the following information about this kit. Kits are composed of groups,
			and groups are composed of items. Each item can have a price and weight delta applied
			to the base kit (product) price or weight.
		</div>
	</div>
	<div>
		<div>
			<div>
				<asp:UpdatePanel ID="pnlUpdateAllGroups" runat="server" UpdateMode="Conditional">
					<ContentTemplate>
						<asp:Panel ID="pnlLocale" runat="server">
							<div class="item-action-bar clearfix">
								<div class="other-actions">
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" AssociatedControlID="ddLocale" />
									<asp:DropDownList ID="ddLocale" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddLocale_SelectedIndexChanged" />
								</div>
							</div>
						</asp:Panel>
						<asp:DataList ID="dlKitGroups" CssClass="admin-table-no-border" runat="server" OnItemCommand="dlKitGroups_ItemCommand" OnItemCreated="dlKitGroups_ItemCreated" OnItemDataBound="dlKitGroups_ItemDataBound">
							<ItemTemplate>
								<asp:UpdatePanel ID="pnlUpdateKitGroup" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
									<ContentTemplate>
										<asp:Panel ID="pnlKitGroup" runat="server" CssClass="white-ui-box">
											<asp:HiddenField ID="hdfGroupId" runat="server" Value='<%# FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().Id %>' />
											<div>
												<%--Main KitGroup template--%>
												<aspdnsf:KitGroupTemplate ID="ctrlKitGroup" runat="server" KitGroup='<%# FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>() %>' GroupTypes='<%# this.GroupTypes %>' />
												<div class="admin_kit_command">
													<div>
														<div class="col-list-action-bar">
															<asp:LinkButton ID="cmdSave2" runat="server" CommandName="Save" CommandArgument="cmdSave2"
																CssClass='<%# FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().IsNew == false ? "btn btn-primary btn-sm" : "btn btn-action btn-sm" %>'
																Text='<%# FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().IsNew == false ? AppLogic.GetString("admin.common.save", ThisCustomer.LocaleSetting) : AppLogic.GetString("admin.common.addnew", ThisCustomer.LocaleSetting) %>' />
															<asp:LinkButton ID="cmdDelete2" CssClass="btn btn-danger btn-sm" runat="server" CommandName="DeleteGroup"
																Visible='<%# FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().IsNew == false %>'
																Text="<%$Tokens:StringResource, admin.common.delete %>" />
														</div>
													</div>
													<%--
                                                    We'll only show the update notification if the updated group
                                                    is the one we're currently bound upon and the one that triggered
                                                    the save was the save button in this area
													--%>
													<asp:Label ID="lblSaveNotification2" runat="server" Text="[Group Updated....]" ForeColor="#6391AC"
														Visible='<%# ShouldHighlightNotification(FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().Id, "cmdSave2") %>' />
													<ajax:AnimationExtender ID="extSaveNotification2" runat="server" TargetControlID="lblSaveNotification2"
														Enabled='<%# ShouldHighlightNotification(FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().Id, "cmdSave2") %>'>
														<Animations>                                                     
															<OnLoad>
																<Sequence>
																	<Color Duration="2" StartValue="#44a503" EndValue="#FFFFFF" Property="style" PropertyKey="color" />
																	<HideAction AnimationTarget="lblSaveNotification2" />
																</Sequence>
															</OnLoad>
														</Animations>
													</ajax:AnimationExtender>
													<%--
                                                    We'll use animation extender as update progress notification instead
                                                    since it reacts faster than the update progress and we can control
                                                    which particular sections we want to display the progress template 
                                                    based on which button clicked
													--%>
													<div id="divProgressBottom" runat="server" style="display: none">
														<p style="color: #44a503; font-style: italic;">
															Saving in progress...<img alt="saving" runat="server" src="~/App_Themes/Admin_Default/images/kit/ajax-loader.gif" />
														</p>
													</div>
													<ajax:AnimationExtender ID="extSaveActionBottom" runat="server" TargetControlID="cmdSave2"
														Enabled="true">
														<Animations>                                                     
                                                         <OnClick>
                                                            <Sequence>
                                                                <StyleAction AnimationTarget="divProgressBottom" Attribute="display" Value=""/>
                                                            </Sequence>
                                                        </OnClick>
														</Animations>
													</ajax:AnimationExtender>
												</div>
											</div>
										</asp:Panel>
									</ContentTemplate>
								</asp:UpdatePanel>
							</ItemTemplate>
						</asp:DataList>
					</ContentTemplate>
				</asp:UpdatePanel>
				<telerik:RadWindow ID="rwInventoryList" runat="server" VisibleOnPageLoad="false" ClientIDMode="Static"
					ShowContentDuringLoad="false" VisibleStatusbar="false" Behaviors="Maximize, Close, Move, Resize"
					Width="1000px" Height="500px" NavigateUrl="KitVariantItemSelector.aspx" Modal="true">
				</telerik:RadWindow>
				<script type="text/javascript">
					Type.registerNamespace('aspdnsf.Pages');
					Type.registerNamespace('aspdnsf.Controls');
					aspdnsf.Controls.KitItemLineControl = function (id, cmdSelect, txtName, txtDescription, txtVariantId, txtPrice, txtWeight) {
						this._id = id;
						this._cmdSelect = $get(cmdSelect);
						this._txtName = $get(txtName);
						this._txtDescription = $get(txtDescription);
						this._txtVariantId = $get(txtVariantId);
						this._txtPrice = $get(txtPrice);
						this._txtWeight = $get(txtWeight);
						// this controls could already be registered during the first page load
						// check to see if this is still existing, otherwise they've probably been kicked out of the DOM via updatepanel refresh
						// due to line item deletion 
						if (this._cmdSelect) {
							$addHandler(this._cmdSelect, 'click', Function.createDelegate(this, this.onSelectCommand));
						}
						this.selectedHandler = null;
					}
					aspdnsf.Controls.KitItemLineControl.prototype = {
						add_selected: function (handler) {
							this.selectedHandler = handler;
						},
						raiseSelected: function () {
							this.selectedHandler(this);
						},
						onSelectCommand: function () {
							this.raiseSelected();
						},
						initialize: function () {
						},
						dispose: function () {
						},
						get_Id: function () {
							return this._id;
						},
						setVariantData: function (variant) {
							this._txtVariantId.value = variant.Id;
							var allowPopulate = true;
							if (this._txtName.value.trim() !== '') {
								var overwrite = confirm('Do you want to overwrite the existing name, description and price delta?');
								allowPopulate = overwrite;
							}
							if (allowPopulate) {
								this._txtName.value = variant.Name;
								this._txtDescription.value = variant.Description;

								if (variant.SalePrice > 0) {
									this._txtPrice.value = variant.SalePrice;
								}
								else {
									this._txtPrice.value = variant.Price;
								}

								this._txtWeight.value = variant.Weight;
							}
						}
					}
					aspdnsf.Controls.KitItemLineControl.registerClass('aspdnsf.Controls.KitItemLineControl');
					aspdnsf.Pages.$EditKit = function () {
						this.currentControl = null;
						this.currentExtender = null;
						this.kitItemControls = new Array();
					}
					aspdnsf.Pages.$EditKit.registerClass('aspdnsf.Pages.$EditKit');
					aspdnsf.Pages.$EditKit.prototype = {
						addKitItemLineControl: function (ctrl) {
							var handler = Function.createDelegate(this, this.onKitItemLineControl_Selected);
							ctrl.add_selected(handler);
							this.kitItemControls.push(ctrl);
						},
						ensureModalPopup: function () {
						},
						onKitItemLineControl_Selected: function (sender, e) {
							this.currentControl = sender;
							this.showVariantList();
						},
						pushData: function (value) {
							this.hideVariantList();
							var data = eval(value);
							this.currentControl.setVariantData(data);
						},
						showVariantList: function () {
							var rwInventoryList = $find('rwInventoryList');
							if (rwInventoryList) {
								// hack the internal flag to force the window to recompute
								// the x and y display, the variable below is an internal cached
								// of bounds information before hiding.
								// This is so that we always have the popup display on the center locatio
								// of the window regardless on where the customer is currently scrolled to
								rwInventoryList._restoreRect = null;
								rwInventoryList.show();
							}
						},
						hideVariantList: function () {
							var rwInventoryList = $find('rwInventoryList');
							if (rwInventoryList) {
								rwInventoryList.hide();
							}
						}
					}
					aspdnsf.Pages.EditKit = new aspdnsf.Pages.$EditKit();
					window.aspdnsf.Pages.EditKit = aspdnsf.Pages.EditKit;
				</script>
			</div>
		</div>
	</div>
</asp:Content>
