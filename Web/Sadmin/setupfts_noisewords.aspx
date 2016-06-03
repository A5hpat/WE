<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.Admin_setupFTS_NoiseWords" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="setupfts_noisewords.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<h1>
		<i class="fa fa-table"></i>
		<asp:Literal runat="server" Text="<%$Tokens:StringResource, setupFTS.aspx.28 %>" />
	</h1>
	<aspdnsf:AlertMessage runat="server" ID="ctlAlertMessage" />
	<div class="item-action-bar">
		<asp:Button ID="btnLinkToSetupFts" runat="server" CssClass="btn btn-default" PostBackUrl="setupFTS.aspx" Text="<%$ Tokens:StringResource,setupFTS.aspx.1 %>" />
	</div>
	<asp:Panel ID="pnlAdd" DefaultButton="btn_AddNewNoiseWord" runat="server">
		<div class="white-ui-box">
			<asp:Button ID="btn_AddNewNoiseWord" CssClass="btn btn-primary" runat="server" ValidationGroup="NewNoiseWord" Text="<%$ Tokens:StringResource,setupFTS.aspx.29 %>"
				OnClick="btn_AddNewNoiseWord_Click" />
			<asp:TextBox ID="txtNewNoiseWord" runat="server" MaxLength="80" CssClass="text-lg" />
			<asp:RequiredFieldValidator ID="RequiredFieldValidator" runat="server" ControlToValidate="txtNewNoiseWord" Display="dynamic" 
				CssClass="text-danger" ValidationGroup="NewNoiseWord" ErrorMessage="!!" />
		</div>
	</asp:Panel>

	<div class="white-ui-box">
		<asp:GridView ID="gMain" runat="server"
			CssClass="table"
			PagerStyle-CssClass="gridview-pager"
			PagerSettings-Position="TopAndBottom"
			GridLines="None"
			AutoGenerateColumns="False"
			AllowPaging="True"
			AllowSorting="True"
			PageSize="25"
			HorizontalAlign="Center"
			OnSorting="gMain_Sorting"
			OnRowCancelingEdit="gMain_RowCancelingEdit"
			OnRowCommand="gMain_RowCommand"
			OnRowDataBound="gMain_RowDataBound"
			OnPageIndexChanging="gMain_PageIndexChanging"
			OnRowUpdating="gMain_RowUpdating"
			OnRowEditing="gMain_RowEditing">
			<Columns>
				<asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="True" Visible="false" SortExpression="ID" />

				<asp:TemplateField HeaderText="Noise Word" SortExpression="word">
					<ItemTemplate>
						<asp:Literal runat="server" ID="ltName" Text='<%# DataBinder.Eval(Container.DataItem, "word") %>' />
					</ItemTemplate>
					<EditItemTemplate>
						<asp:TextBox runat="server" ID="txtNewNoiseWord" name="txtNewNoiseWord" Text='<%# DataBinder.Eval(Container.DataItem, "EditWord") %>' />
						<asp:Label runat="server" ID="lblNewNoiseWordID" Visible="false" Text='<%# Eval("ID") %>' />
					</EditItemTemplate>
				</asp:TemplateField>

				<asp:CommandField ItemStyle-Width="10%" ButtonType="Link" ShowEditButton="True" ControlStyle-CssClass="action-link" CancelText='<i class="fa fa-reply"></i> Cancel' EditText='<i class="fa fa-share"></i> Edit' UpdateText='<i class="fa fa-floppy-o"></i> Save' />

				<asp:TemplateField ItemStyle-Width="10%">
					<ItemTemplate>
						<asp:LinkButton ID="Delete" CommandName="DeleteItem" CommandArgument='<%# Eval("ID") %>' runat="server" Text='<i class="fa fa-times"></i> Delete' CssClass="delete-link" />
					</ItemTemplate>
				</asp:TemplateField>

				<asp:BoundField Visible="false" DataField="EditWord" ReadOnly="true" />

			</Columns>
			<PagerSettings FirstPageText="&amp;lt;&amp;lt;First Page" LastPageText="Last Page&amp;gt;&amp;gt;"
				Mode="NumericFirstLast" PageButtonCount="25" />
			<EditRowStyle CssClass="grid-view-edit" />
			<SelectedRowStyle CssClass="grid-view-action" />
		</asp:GridView>
	</div>
</asp:Content>
