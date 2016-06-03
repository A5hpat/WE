<%@ Page Title="<%$Tokens:StringResource, admin.title.appconfig %>" Theme="Admin_Default" Language="C#" Inherits="AspDotNetStorefrontAdmin.ConfigEditor" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="config.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
	<script type="text/javascript">
		$(document)
			.ready(function() {
				// Highlight validation errors
				$('.form-group')
					.has('.has-error')
					.addClass('has-error');

				//Setup the advanced toggle
				var advancedToggleKey = "AdvancedToggleVisible";
				var advancedToggleVisible = sessionStorage.getItem(advancedToggleKey) === 'true';
				var advancedToggle = $('#AdvancedToggle');
				if(advancedToggle == null)
					return;

				advancedToggle.toggle(advancedToggleVisible);
				$('#ShowAdvancedToggle').toggle(!advancedToggleVisible);
				$('#HideAdvancedToggle').toggle(advancedToggleVisible);

				$('#ShowAdvancedToggle, #HideAdvancedToggle')
					.click(function(event) {
						advancedToggle
							.slideToggle(function() {
								var isVisible = advancedToggle.is(':visible');

								$('#ShowAdvancedToggle').toggle(!isVisible);
								$('#HideAdvancedToggle').toggle(isVisible);

								sessionStorage.setItem(advancedToggleKey, isVisible.toString());
							});
						event.preventDefault();
					});
			});
	</script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="AppConfigs.aspx" />

	<h1>
		<i class="fa fa-gear f-x3"></i>
		<asp:Literal runat="server" Text="<%# AppLogic.GetString(ConfigEditorContext.GetHeadingStringResourceName(), AppLogic.GetCurrentCustomer().LocaleSetting) %>" />
	</h1>

	<aspdnsf:AlertMessage runat="server" ID="ctrlAlertMessage" />
	<asp:Panel runat="server" DefaultButton="Save">
		<div class="item-action-bar">
			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:PlaceHolder runat="server" ID="SaveButtonsTop">
				<asp:Button runat="server" ID="SaveAndClose" CssClass="btn btn-default" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" CommandName="<%# SaveAndCloseCommand %>" OnCommand="HandleCommands" />
				<asp:Button runat="server" ID="Save" CssClass="btn btn-primary" Text="Save" CommandName="<%# SaveCommand %>" OnCommand="HandleCommands" />
			</asp:PlaceHolder>
		</div>

		<asp:Panel runat="server" ID="EditorPanel" CssClass="white-ui-box">
			<div class="row">
				<div class="col-sm-6">
					<div class="form-group">
						<asp:Label runat="server" AssociatedControlID="Name" CssClass="control-label">Name</asp:Label>
						<asp:TextBox runat="server" ID="Name" ReadOnly="true" CssClass="form-control" />
						<asp:RequiredFieldValidator runat="server"
							ControlToValidate="Name"
							EnableClientScript="false"
							Display="None"
							ErrorMessage="You must enter a name" />
					</div>
					<div class="form-group">
						<label>Value</label>
						<asp:Repeater runat="server" ID="StoreValues" OnItemDataBound="StoreValues_ItemDataBound">
							<HeaderTemplate>
								<table class="table">
							</HeaderTemplate>
							<ItemTemplate>
								<tr>
									<th colspan="2">
										<asp:Literal runat="server" ID="Store" Text='<%# Eval("StoreName") %>' />
									</th>
								</tr>
								<tr>
									<td>
										<asp:PlaceHolder runat="server" ID="ValueEditorPlaceholder" />
									</td>
									<td class="action-column">
										<asp:LinkButton runat="server"
											CssClass="delete-link fa-times"
											CommandName="<%# DeleteCommand %>"
											CommandArgument='<%# Eval("StoreID") %>'
											OnCommand="HandleCommands"
											Text="Use Default"
											Visible='<%# (int)Eval("StoreID") > 0 && (bool)Eval("Exists") %>' />
									</td>
								</tr>

							</ItemTemplate>
							<FooterTemplate>
								</table>
							</FooterTemplate>
						</asp:Repeater>
					</div>
				</div>
				<div class="col-sm-6">
					<div class="form-group">
						<asp:Label runat="server" AssociatedControlID="Description" CssClass="control-label">Description</asp:Label>
						<asp:TextBox runat="server" ID="Description" TextMode="MultiLine" CssClass="form-control" />
					</div>

					<div class="form-group">
						<asp:Label runat="server" AssociatedControlID="Group" CssClass="control-label">Group</asp:Label>
						<asp:DropDownList runat="server" ID="Group" CssClass="form-control" />
					</div>
					<asp:Panel runat="server" Visible="<%# AppLogic.GetCurrentCustomer().IsAdminSuperUser %>">
						<p>
							<a href="#" id="ShowAdvancedToggle"><i class="fa fa-plus-square-o"></i>Show Advanced Settings</a>
							<a href="#" id="HideAdvancedToggle"><i class="fa fa-minus-square-o"></i>Hide Advanced Settings</a>
						</p>
						<div id="AdvancedToggle" class="advanced-options">
							<div class="form-group">
								<asp:Label runat="server" AssociatedControlID="SuperOnly" CssClass="control-label">Super Admin Only</asp:Label>
								<div>
									<asp:RadioButtonList runat="server" ID="SuperOnly" RepeatLayout="Flow" RepeatDirection="Horizontal">
										<asp:ListItem Text="Yes" Value="True" />
										<asp:ListItem Text="No" Value="False" />
									</asp:RadioButtonList>
								</div>
							</div>

							<div class="form-group">
								<asp:Label runat="server" AssociatedControlID="ValueType" CssClass="control-label">Value Type</asp:Label>
								<asp:DropDownList runat="server" ID="ValueType" CssClass="form-control">
									<asp:ListItem Text="String" Value="string" />
									<asp:ListItem Text="Boolean" Value="boolean" />
									<asp:ListItem Text="Integer" Value="integer" />
									<asp:ListItem Text="Decimal" Value="decimal" />
									<asp:ListItem Text="Double" Value="double" />
									<asp:ListItem Text="Enumeration" Value="enum" />
									<asp:ListItem Text="Multi-Select" Value="multiselect" />
									<asp:ListItem Text="Dynamic Invoke" Value="invoke" />
								</asp:DropDownList>
							</div>

							<div runat="server" id="AllowedValuesPanel" class="form-group">
								<asp:Label runat="server" AssociatedControlID="AllowedValues" CssClass="control-label">Allowed Values</asp:Label>
								<asp:TextBox runat="server" ID="AllowedValues" TextMode="MultiLine" CssClass="form-control" />
							</div>

						</div>
					</asp:Panel>
				</div>
			</div>

		</asp:Panel>
		<asp:Panel runat="server" ID="ActionBarBottom" CssClass="item-action-bar">
			<asp:HyperLink runat="server"
				CssClass="btn btn-default"
				NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>"
				Text="<%$Tokens:StringResource, admin.common.close %>" />

			<asp:Button runat="server" ID="SaveAndCloseBottom" CssClass="btn btn-default" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" CommandName="<%# SaveAndCloseCommand %>" OnCommand="HandleCommands" />
			<asp:Button runat="server" ID="SaveBottom" CssClass="btn btn-primary" Text="Save" CommandName="<%# SaveCommand %>" OnCommand="HandleCommands" />
		</asp:Panel>
	</asp:Panel>


</asp:Content>
