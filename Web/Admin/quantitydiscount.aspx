<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.quantitydiscount" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="quantitydiscount.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<aspdnsf:ReturnUrlTracker runat="server" ID="ReturnUrlTracker" DefaultReturnUrl="quantitydiscounts.aspx" />
	<div class="admin-module">
	<aspdnsf:AlertMessage ID="ctlAlertMessage" runat="server" />
		<h1>
			<i class="fa fa-table"></i>
			<asp:Literal ID="litHeader" runat="server" />
		</h1>
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.quantitydiscount.QuantityDiscountInfo %>" />		
		<div class="item-action-bar">
			<asp:HyperLink runat="server" ID="btnCloseTop" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnSaveAndCloseTop" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmitTop" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>
		<div class="admin-row">
			<div id="divEditDiscount" class="white-ui-box">
				<div class="row">
					<div class="col-md-4">
						<div class="form-inline">
							<asp:Label AssociatedControlID="litDiscountId" for="litDiscountId" runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />:
							<asp:Literal ID="litDiscountId" runat="server" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtDiscountName" for="txtDiscountName" runat="server" Text="<%$Tokens:StringResource, admin.common.name %>" />:
							<asp:TextBox ID="txtDiscountName" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator CssClass="text-danger" Display="Dynamic" ErrorMessage="<%$Tokens:StringResource, admin.editquantitydiscount.EnterQuantityDiscount %>" ControlToValidate="txtDiscountName" ID="RequiredFieldValidator9" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator>
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="txtDisplayOrder" for="txtDisplayOrder" runat="server" Text="<%$Tokens:StringResource, admin.Common.DisplayOrder %>" />
							<asp:TextBox ID="txtDisplayOrder" CssClass="form-control" runat="server" />
							<asp:RequiredFieldValidator CssClass="text-danger" Display="Dynamic" ErrorMessage="<%$Tokens:StringResource, admin.quantitydiscount.FillInDisplayOrder %>" ControlToValidate="txtDisplayOrder" ID="RequiredFieldValidator2" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator>
							<asp:CompareValidator ControlToValidate="txtDisplayOrder" Operator="DataTypeCheck" Type="Integer" ValidateRequestMode="Enabled" CssClass="text-danger" 
								Display="Dynamic" runat="server" ID="cmpDisplayOrder" ValidationGroup="gAdd" ErrorMessage="<%$Tokens:StringResource, admin.editquantitydiscounttable.EnterInteger %>" />
						</div>
						<div class="form-group">
							<span class="text-danger">*</span><asp:Label AssociatedControlID="ddlDiscountType" for="ddlDiscountType" runat="server" Text="<%$Tokens:StringResource, admin.quickadd.DiscountType %>" />:
							<asp:DropDownList ID="ddlDiscountType" runat="server">
								<asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.Percent %>" />
								<asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.common.FixedAmount %>" />
							</asp:DropDownList>
						</div>
					</div>
				</div>
			</div>
		</div>
		<div id="divDiscountTable" runat="server" visible="false" class="admin-row white-ui-box">
			<div class="white-box-heading">
					<asp:Literal ID="litGridHead" runat="server" Text="<%$Tokens:StringResource, admin.quantitydiscount.TableRows %>" />
				</div>
			<asp:GridView ID="grdDiscountRows" CssClass="table table-detail" ShowHeader="true" PageIndex="0" AllowPaging="true" PageSize="10" OnRowDataBound="grdDiscountRows_OnRowDataBound"
				runat="server" GridLines="None" CellSpacing="-1" OnPageIndexChanging="grdDiscountRows_OnPageIndexChanging" OnRowCommand="grdDiscountRows_RowCommand" AutoGenerateColumns="false">
				<Columns>
					<asp:TemplateField>
						<HeaderTemplate>
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />
						</HeaderTemplate>
						<ItemTemplate>
							<asp:Literal runat="server" ID="litQuantityDiscountTableID" Text='<%# Eval("QuantityDiscountTableID") %>' />
						</ItemTemplate>
						<ItemStyle CssClass="selectData" Width="11%" />
					</asp:TemplateField>
					<asp:TemplateField>
						<HeaderTemplate>
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.quickadd.LowQuantity %>" />
						</HeaderTemplate>
						<ItemTemplate>
							<asp:Textbox ID="txtLowQuantity" CssClass="form-control" runat="server" Text='<%# Eval("LowQuantity") %>' />
							<asp:RegularExpressionValidator Display="Dynamic" ID="revTxtLowQuantity" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.quantitydiscount.LowQuantityMustBeInt %>"
					            ControlToValidate="txtLowQuantity" CssClass="text-danger validator-error-adjustments" ValidationExpression="\d{0,9}" ValidationGroup="gAdd" />
						</ItemTemplate>
                        <ItemStyle CssClass="selectData" Width="26%" />
					</asp:TemplateField>
					<asp:TemplateField>
						<HeaderTemplate>
							<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.quickadd.HighQuantity %>" />
						</HeaderTemplate>
						<ItemTemplate>
							<asp:Textbox ID="txtHighQuantity" runat="server" CssClass="form-control" Text='<%# Eval("HighQuantity") %>' />
							<asp:RegularExpressionValidator Display="Dynamic" ID="revTxtHighQuantity" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.quantitydiscount.HighQuantityMustBeInt %>"
					            ControlToValidate="txtHighQuantity" CssClass="text-danger validator-error-adjustments" ValidationExpression="\d{0,9}" ValidationGroup="gAdd" />
                            <asp:CompareValidator ControlToValidate="txtHighQuantity" Operator="GreaterThanEqual" CssClass="text-danger validator-error-adjustments" Type="Integer"
					            Display="Dynamic" runat="server" ID="cmpTxtHighQuantity" ValidationGroup="gAdd" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.HighQuantitySameOrHigher %>" 
                                ControlToCompare="txtLowQuantity" />	
						</ItemTemplate>
                        <ItemStyle CssClass="selectData" Width="26%" />
					</asp:TemplateField>
					<asp:TemplateField>
						<HeaderTemplate>
							<asp:Label runat="server" ID="lblDiscountTypeLabel" />
						</HeaderTemplate>
						<ItemTemplate>
							<asp:Textbox ID="txtDiscountPercent" CssClass="form-control" runat="server" Text='<%# Eval("DiscountPercent") %>' />
							<asp:RegularExpressionValidator Display="Dynamic" ID="revTxtDiscountPercent" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.ValidAmount %>"
					            ControlToValidate="txtDiscountPercent" ValidationGroup="gAdd" CssClass="text-danger" ValidationExpression="\d*\.?\d*" />	
						</ItemTemplate>
                        <ItemStyle CssClass="selectData" Width="26%" />
					</asp:TemplateField>
					<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Delete %>">
						<ItemTemplate>
							<asp:LinkButton ID="lnkDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("QuantityDiscountTableID") %>' runat="Server" Text='<i class="fa fa-times"></i> Delete' CssClass="delete-link" />
						</ItemTemplate>
						<ItemStyle CssClass="selectData" Width="11%" />
					</asp:TemplateField>
				</Columns>
			</asp:GridView>
			<table id="tblNewRow" runat="server" class="table table-detail" visible="false">
				<tr runat="server" visible="false" id="tblNewRowHeader">
					<th>&nbsp;</th>
					<th><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.quickadd.LowQuantity %>" /></th>
					<th><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.quickadd.HighQuantity %>" /></th>
					<th><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.quickadd.discount %>" /></th>
					<th>&nbsp;</th>
				</tr>
				<tr>
					<td style="width:11%;"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.quantitydiscount.new %>" />:</td>
					<td style="width:26%;">
						<asp:TextBox ID="txtNewLowQuantity" CssClass="form-control" runat="server" />
						<asp:RegularExpressionValidator Display="Dynamic" ID="revTxtNewLowQuantity" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.quantitydiscount.LowQuantityMustBeInt %>"
					        ControlToValidate="txtNewLowQuantity" CssClass="text-danger validator-error-adjustments" ValidationExpression="\d{0,9}" ValidationGroup="gAdd" />
					</td>
					<td style="width:26%;">
						<asp:TextBox ID="txtNewHighQuantity" CssClass="form-control" runat="server" />
						<asp:RegularExpressionValidator Display="Dynamic" ID="revTxtNewHighQuantity" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.quantitydiscount.HighQuantityMustBeInt %>"
					        ControlToValidate="txtNewHighQuantity" CssClass="text-danger validator-error-adjustments" ValidationExpression="\d{0,9}" ValidationGroup="gAdd" />
                        <asp:CompareValidator ControlToValidate="txtNewHighQuantity" Operator="GreaterThanEqual" CssClass="text-danger validator-error-adjustments" Type="Integer"
					        Display="Dynamic" runat="server" ID="cmpTxtNewHighQuantity" ValidationGroup="gAdd" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.HighQuantitySameOrHigher %>" 
                            ControlToCompare="txtNewLowQuantity" />
					</td>
					<td style="width:26%;">
						<asp:TextBox ID="txtNewDiscountPercent" CssClass="form-control" runat="server" />
						<asp:RegularExpressionValidator Display="Dynamic" ID="revTxtNewDiscountPercent" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.quickadd.ValidAmount %>"
					        ControlToValidate="txtNewDiscountPercent" ValidationGroup="gAdd" CssClass="text-danger" ValidationExpression="\d*\.?\d*" />	
					</td>
					<td style="width:11%;">&nbsp;</td>
				</tr>
			</table>
		</div>
		<div class="item-action-bar">
			<asp:HyperLink runat="server" ID="btnClose" CssClass="btn btn-default" NavigateUrl="<%# ReturnUrlTracker.GetHyperlinkReturnUrl() %>" Text="<%$Tokens:StringResource, admin.common.close %>" />
			<asp:Button ID="btnSaveAndClose" runat="server" CssClass="btn btn-default" OnClick="btnSaveAndClose_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.SaveAndClose %>" />
			<asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" ValidationGroup="gAdd" Text="<%$Tokens:StringResource, admin.common.Save %>" />
		</div>
	</div>
</asp:Content>