<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.QuickAddCustomerLevel" Codebehind="QuickAddCustomerLevel.ascx.cs" %>
<asp:Panel ID="pnlMain" runat="server" CssClass="quick-add-wrapper">
	<div class="quick-add-link-open">
		<a href="javascript:void(0);" runat="server" id="linkQuickAdd" onclick="$('.QuickAddCustomerLevel').show();"><asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.CustomerLevelLink%>" /></a>
	</div>
	<asp:Panel runat="server" ID="pnlQuickAddFields" Style="display: none;">
        <div class="form-group">
            <div class="row">
                <div class="col-sm-4">
                    <span class="text-danger">*</span><asp:Literal ID="Literal2" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.CustomerLevelName%>" />:	               
                </div>
                <div class="col-sm-8">
                   <asp:TextBox ID="txtName" runat="server" CssClass="text-md" />	
			       <asp:RequiredFieldValidator ID="rfvName" runat="server" CssClass="text-danger" Display="Dynamic"
				        ControlToValidate="txtName" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.NameRequired %>" />
                </div>
            </div>		
		</div>
        <div class="form-group">
            <div class="row">
                <div class="col-sm-4">
                    <asp:Literal ID="Literal3" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.DiscountAmount%>" />:
				    <asp:Literal ID="litDiscountAmount" runat="server" />	               
                </div>
                <div class="col-sm-8">
                    <asp:TextBox ID="txtDiscountAmount" Text="0.00" runat="server" CssClass="text-md" />
                    <asp:RangeValidator ID="rvDiscount" runat="server" CssClass="text-danger"
				    ControlToValidate="txtDiscountAmount" Display="Dynamic"
				    MinimumValue=".0" MaximumValue="999999999" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.DiscountFormat %>" />
                </div>
            </div>		
		</div>
        <div class="form-group">
            <div class="row">
                <div class="col-sm-4">
                    <asp:Label ID="litDiscountType" AssociatedControlID="ddlDiscountType" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.DiscountType %>" />:
                </div>
                <div class="col-sm-8">
                   <asp:DropDownList CssClass="text-md" ID="ddlDiscountType" runat="server">
				        <asp:ListItem Text="<%$Tokens:StringResource, admin.common.percent %>" Value="Percent" />
				        <asp:ListItem Text="<%$Tokens:StringResource, admin.common.fixedamount %>" Value="Fixed" />
			        </asp:DropDownList>
                </div>
            </div>		
		</div>
		<div class="form-group">		
			<asp:Button ID="btnSubmit" Text="Add" runat="server" OnClick="btnSubmit_Click" CssClass="btn btn-sm btn-primary" />
			<a href="javascript:void(0);" runat="server" id="linkQuickClose" onclick="$('.QuickAddCustomerLevel').hide();">
				<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Close %>" />
			</a>
		</div>
	</asp:Panel>
	<asp:Literal ID="ltScript" runat="server" />
	<div style="clear: both;">
	</div>
</asp:Panel>
