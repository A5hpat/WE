<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.QuickAddEntity" Codebehind="QuickAddEntity.ascx.cs" %>
<asp:Panel ID="pnlMain" runat="server" CssClass="quick-add-wrapper" DefaultButton="btnSubmit">
	<div class="quick-add-link-open">
		<a href="javascript:void(0);" runat="server" id="linkQuickAdd">
			<asp:Literal ID="ltPreEntity" runat="server" /></a>
	</div>
	<asp:Panel runat="server" ID="pnlQuickAddFields" Style="display: none;">
		<asp:Panel ID="pnlParent" runat="server" CssClass="form-group quick-add-parent">
			<span>*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.quickadd.ChildCategoryQuestion %>" />:</span>
			<asp:DropDownList ID="ddParent" runat="Server" />
		</asp:Panel>
        <div class="form-group">
            <div class="row">
                <div class="col-sm-4">
                    <span class="text-danger">*</span><asp:Label ID="lblEntityName" runat="server" />		               
                </div>
                <div class="col-sm-8">
                    <asp:TextBox CssClass="text-md" ID="txtName" runat="Server" />
			        <asp:RequiredFieldValidator ControlToValidate="txtName" ID="rfvName" CssClass="text-danger"
				        EnableClientScript="true" SetFocusOnError="true" runat="server" Display="Dynamic" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.NameRequired %>" />
                </div>
            </div>		
		</div>
        <div class="form-group">
            <div class="row">
                <div class="col-sm-4">
                    <asp:Literal runat="server" Visible="false" ID="ltEmailAsterisk"><span class="text-danger">*</span></asp:Literal>
			        <asp:Label ID="lblEntityEmail" runat="server" Visible="false" />				               
                </div>
                <div class="col-sm-8">
                    <asp:TextBox ID="txtEmail" CssClass="text-md" runat="server" Visible="false" />
			        <asp:RequiredFieldValidator 
				        ControlToValidate="txtEmail"
				        ID="rfvEmail" 
				        CssClass="text-danger"
				        EnableClientScript="true" 
				        SetFocusOnError="true" 
				        runat="server" 
				        Display="Dynamic"
				        Visible ="false"
				        ErrorMessage="<%$Tokens:StringResource, admin.quickadd.EmailRequired %>" />
			        <aspdnsf:EmailValidator 
				        ErrorMessage="<%$Tokens:StringResource, admin.quickadd.InvalidEmail %>" 
				        ID="valRegExValEmail"
				        ControlToValidate="txtEmail" 
				        CssClass="text-danger"
				        EnableClientScript="true" 
				        SetFocusOnError="true" 
				        runat="server" 
				        Display="Dynamic"
				        Visible="false" />
                </div>
            </div>		
		</div>
		<div>
			<asp:Button ID="btnSubmit" Text="Add" runat="server" OnClick="btnSubmit_Click" CssClass="btn btn-sm btn-primary" />
			<a href="javascript:void(0);" runat="server" id="linkQuickClose"><asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Close %>" /></a>
		</div>
	</asp:Panel>
	<asp:Literal ID="ltQuickAddScript" runat="server" />
	<div style="clear: both;"></div>
</asp:Panel>