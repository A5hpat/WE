<%@ Page Language="c#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.storewide" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="storewide.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<asp:Literal ID="ltScript" runat="server"></asp:Literal>
	<asp:Literal ID="ltValid" runat="server"></asp:Literal>
	<h1>
		<i class="fa fa-wrench"></i>
		<asp:Label runat="server" Text="Store-Wide Maintenance" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />
	<div class="white-ui-box">
		<table class="table">
			<tr>
				<td>Set 'On Sale' Prompt:
				</td>
				<td>
					<asp:Panel runat="server" ID="pnlOnSalePrompt" DefaultButton="btnSubmit1">
						<asp:DropDownList ID="ddOnSale" runat="server" CssClass="default"></asp:DropDownList>
						<asp:RequiredFieldValidator InitialValue="0" ValidationGroup="group1" ErrorMessage="!!" ControlToValidate="ddOnSale" Display="dynamic" SetFocusOnError="true" runat="server" ID="RequiredFieldValidator"></asp:RequiredFieldValidator>
						<br />
						For Category:
															<asp:DropDownList ID="ddOnSaleCat" runat="server" CssClass="default parent-entity-dropdown"></asp:DropDownList>
						<br />
						For Department:
															<asp:DropDownList ID="ddOnSaleDep" runat="server" CssClass="default parent-entity-dropdown"></asp:DropDownList>
						<br />
						For Manufacturer:
															<asp:DropDownList ID="ddOnSaleManu" runat="server" CssClass="default parent-entity-dropdown"></asp:DropDownList>
						<br />
						<asp:Button ID="btnSubmit1" ValidationGroup="group1" CssClass="btn btn-primary btn-sm" runat="server" Text="Submit" OnClick="btnSubmit1_Click" />
					</asp:Panel>
				</td>
			</tr>
			<tr>
				<td>Set Quantity Discount Table to be<br />
					used for ALL Products & Variants:
                                                        
				</td>
				<td>
					<asp:Panel runat="server" ID="pnlSetQtyDiscount" DefaultButton="btnSubmit4">
						<asp:DropDownList ID="ddDiscountTable" runat="server"></asp:DropDownList>
						<br />
						<asp:Button ID="btnSubmit4" ValidationGroup="group4" CssClass="btn btn-primary btn-sm" runat="server" Text="Submit" OnClick="btnSubmit4_Click" />
					</asp:Panel>
				</td>
			</tr>
			<tr>
				<td>Reset All Default Variants:
                                                        
				</td>
				<td>
					<asp:Panel runat="server" ID="pnlResetVariants" DefaultButton="btnSubmit6">
						This cannot be undone!
                                                        <br />
						Resets the default variant for each product to the first one by DisplayOrder,Name
                                                        <br />
						<asp:Button ID="btnSubmit6" ValidationGroup="group6" CssClass="btn btn-primary btn-sm" runat="server" Text="Submit" OnClick="btnSubmit6_Click" />
					</asp:Panel>
				</td>
			</tr>
			<tr>
				<td>Reset All product SENames:
                                                        
				</td>
				<td>
					<asp:Panel runat="server" ID="pnlResetSENames" DefaultButton="btnSubmit7">
						This cannot be undone!
                                                        <br />
						Sets the SEName field in the product table for ALL products, this may take a long time if you have a lot of products.
                                                        <br />
						<asp:Button ID="btnSubmit7" ValidationGroup="group7" CssClass="btn btn-primary btn-sm" runat="server" Text="Submit" OnClick="btnSubmit7_Click" />
					</asp:Panel>
				</td>
			</tr>
		</table>
	</div>
</asp:Content>
