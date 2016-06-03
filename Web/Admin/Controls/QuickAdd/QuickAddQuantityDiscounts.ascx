<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.QuickAddQuantityDiscounts" Codebehind="QuickAddQuantityDiscounts.ascx.cs" %>

<asp:Panel ID="pnlMain" runat="server" CssClass="quick-add-wrapper">
	<div class="quick-add-link-open">
		<a href="javascript:void(0);" runat="server" id="linkQuickAdd"><asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.QuantityDiscountQuickAdd%>" /></a>
	</div>
	<asp:Panel runat="server" ID="pnlQuickAddFields" Style="display: none;">
		<div class="quickAddContainer">
			<div class="form-group">
                <div class="row">
                    <div class="col-sm-4">
                        <span class="text-danger">*</span><asp:Literal ID="Literal2" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.QuantityDiscountName%>" />:				        
                    </div>
                    <div class="col-sm-8">
                        <asp:TextBox ID="txtName" CssClass="text-md" runat="server" />
                        <asp:RequiredFieldValidator ID="rfvName" Display="Dynamic" runat="server" CssClass="text-danger"
					        ControlToValidate="txtName" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.NameRequired %>" />
                    </div>
                </div>		
			</div>
            <div class="form-group">
                <div class="row">
                    <div class="col-sm-4">
                        <asp:Literal ID="litDiscountType" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.DiscountType %>" />:		        
                    </div>
                    <div class="col-sm-8">
                        <asp:DropDownList ID="ddlDiscountType" CssClass="text-md" runat="server">
					        <asp:ListItem Text="<%$Tokens:StringResource, admin.common.percent %>" Value="0" />
					        <asp:ListItem Text="<%$Tokens:StringResource, admin.common.fixedamount %>" Value="1" />
				        </asp:DropDownList>
                    </div>
                </div>		
			</div>
            <div class="form-group">
                <div class="row">
                    <div class="col-sm-4">
                        <asp:Literal ID="Literal3" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.LowQuantity%>" />:				               
                    </div>
                    <div class="col-sm-8">
                        <asp:TextBox ID="txtLowQuantity" runat="server" CssClass="text-md" MaxLengt="9" />
				        <asp:Literal ID="Literal4" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.ExampleFive%>" />
                        <asp:RegularExpressionValidator Display="Dynamic" ID="revTxtLowQuantity" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.LowQuantityAtLeastZero %>"
					        ControlToValidate="txtLowQuantity" CssClass="text-danger" ValidationExpression="\d{0,9}" />
                    </div>
                </div>		
			</div>
            <div class="form-group">
                <div class="row">
                    <div class="col-sm-4">
                        <span class="text-danger">*</span><asp:Literal ID="Literal5" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.HighQuantity%>" />:				        			               
                    </div>
                    <div class="col-sm-8">
                        <asp:TextBox ID="txtHighQuantity" runat="server" CssClass="text-md" MaxLength="9" />
                        <asp:Literal ID="Literal6" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.ExampleTwentyFive%>" />
                        <asp:RequiredFieldValidator ID="rfvHighQuantity" runat="server" CssClass="text-danger"
					        Display="Dynamic" ControlToValidate="txtHighQuantity" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.HighQuantityRequired%>" />
                        <asp:RegularExpressionValidator Display="Dynamic" ID="revTxtHighQuantity" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.HighQuantityMustBeOne%>"
					        ControlToValidate="txtHighQuantity" CssClass="text-danger" ValidationExpression="[1-9][0-9]*" />	
                        <asp:CompareValidator ControlToValidate="txtHighQuantity" Operator="GreaterThanEqual" CssClass="text-danger" Type="Integer" EnableClientScript="true"
					        Display="Dynamic" runat="server" ID="cmpTxtHighQuantity" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.HighQuantitySameOrHigher%>" 
                            ControlToCompare="txtLowQuantity" />	
                    </div>
                </div>		
			</div>
			<div class="quickAddItem">
                <div class="form-group">
                    <div class="row">
                        <div class="col-sm-4">
                            <asp:Literal ID="Literal7" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.discount %>" />:			        			               
                        </div>
                        <div class="col-sm-8">
                            <asp:TextBox ID="txtDiscount" runat="server" CssClass="text-md" />
				            <asp:Literal ID="Literal8" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.ExampleDecimal %>" />
                            <asp:RegularExpressionValidator Display="Dynamic" ID="revTxtDiscount" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.ValidAmount %>"
					            ControlToValidate="txtDiscount" ValidationGroup="QuickAddQuantityDiscount" CssClass="text-danger" ValidationExpression="\d*\.?\d*" />
                        </div>
                    </div>		
			    </div>	
			</div>
			<div>
				<asp:Button ID="btnSubmit" Text="Add" runat="server" OnClick="btnSubmit_Click" CssClass="btn btn-sm btn-primary" />
				<a href="javascript:void(0);" runat="server" id="linkQuickClose"><asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Close %>" /></a>
			</div>
		</div>
	</asp:Panel>
	<asp:Literal ID="ltScript" runat="server" />
	<div style="clear: both;">
	</div>
</asp:Panel>