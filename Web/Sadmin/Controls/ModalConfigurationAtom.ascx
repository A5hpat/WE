<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.Controls.ModalConfigurationAtom" Codebehind="ModalConfigurationAtom.ascx.cs" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" TagName="ConfigurationAtom" Src="ConfigurationAtom.ascx" %>

<asp:UpdatePanel ID="upExtender" runat="server" UpdateMode="Always">
	<Triggers>
		<asp:AsyncPostBackTrigger ControlID="btnSave" EventName="Click" />
		<asp:AsyncPostBackTrigger ControlID="btnCancel" EventName="Click" />
	</Triggers>
	<ContentTemplate>
		<asp:LinkButton ID="btnConfigureAtomConfig" runat="server" Text="configure" />
		<ajax:ModalPopupExtender
			ID="mpConfigurationAtom" runat="server"
			PopupControlID="pnlConfigurationAtom"
			TargetControlID="btnConfigureAtomConfig"
			BackgroundCssClass="modal_popup_background"
			CancelControlID="btnCancelConfiguration">
		</ajax:ModalPopupExtender>
	</ContentTemplate>
</asp:UpdatePanel>
<asp:Panel ID="pnlConfigurationAtom" runat="server" CssClass="atom_modal_popup" ScrollBars="None" DefaultButton="defaultButton">
	<asp:UpdatePanel ID="upModalAtom" UpdateMode="Always" ChildrenAsTriggers="true" runat="server">
		<Triggers>
			<asp:AsyncPostBackTrigger ControlID="btnSave" EventName="Click" />
		</Triggers>
		<ContentTemplate>
			<div class="modalHolder">
				<div class="atomFixedHeader" id="modaldiv" runat="server">
					<div class="atomHeader">
						<asp:LinkButton ID="btnCancelConfiguration" OnClick="btnCancelConfiguration_Click" runat="server" CssClass="close modal-close atomModalClose"><span aria-hidden="true">×</span><span class="sr-only">Close</span></asp:LinkButton>
						<h2>
							<i class="fa fa-cog"></i>
							<asp:Literal ID="litTitle" runat="server" />
						</h2>
					</div>
				</div>
				<asp:Panel ID="pnlConfigAtomContainer" runat="server" CssClass="pnlOverAtomFixedButtons">
					<div class="atomModalContent">
						<aspdnsf:ConfigurationAtom runat="server" ID="ConfigurationAtom" ShowSaveButton="false" LoadAdvancedConfigs="true" />
					</div>
				</asp:Panel>
				<div class="atomFixedButtons">		
					<asp:Button ID="btnCancel" Text="<%$Tokens:StringResource, admin.common.close %>" CssClass="btn btn-default" runat="server" />			
					<asp:Button ID="btnToggleAdvanced" runat="server" OnClientClick="$(this).closest('.modalHolder').find('.trConfigAtomAdvanced').toggle();return false;" Text="<%$Tokens:StringResource, admin.wizard.ToggleAdvanced %>" CssClass="btn btn-default" />
					<asp:Button ID="btnSave" Text="<%$Tokens:StringResource, admin.common.saveandclose %>" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click" />
					<div style="clear: both;"></div>
				</div>
			</div>
		</ContentTemplate>
	</asp:UpdatePanel>
	<asp:Button runat="server" ID="defaultButton" OnClientClick="return false;" Style="display: none;" />
</asp:Panel>