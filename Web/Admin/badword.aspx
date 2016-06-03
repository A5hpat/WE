<%@ Page Language="c#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.BadWord"
	MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" Codebehind="badword.aspx.cs" %>

<%@ Register TagPrefix="aspdnsf" Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls.Listing" %>
<%@ Register TagPrefix="aspdnsf" TagName="StringFilter" Src="Controls/Listing/StringFilter.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="LinkGroupRatings" Src="Controls/LinkGroupRatings.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<asp:Panel runat="server" DefaultButton="btnSubmit">
		<h1>
			<i class="fa fa-thumbs-up"></i>
			<asp:Literal ID="litManageRatingsTitle" runat="server" Text="<%$Tokens:StringResource, admin.menu.Ratings %>" />
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.breadcrumbseparator %>" />
			<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.badword %>" />
		</h1>
		<aspdnsf:AlertMessage runat="server" ID="AlertMessage" />

		<div class="item-action-bar">
			<aspdnsf:LinkGroupRatings runat="server" ID="LinkGroupRatings" SelectedLink="badword.aspx" />
			<asp:Button runat="server" ID="btnSubmit" Text="<%$Tokens:StringResource, admin.common.Save %>" CssClass="btn btn-action" OnClick="btnSubmit_Click" />
		</div>
		<div class="white-ui-box">
			<div class="wrapperTop">
				<div class="row">
					<div class="col-sm-3">
						<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.BadWord.EnterBadWordBelow %>" AssociatedControlID="txtWord" />
						<asp:TextBox runat="Server" ID="txtWord" CssClass="form-control"></asp:TextBox>
						<asp:CheckBox ID="chkBoxNewComments" runat="server" Text="<%$Tokens:StringResource, admin.BadWord.OnlyForNewComments %>" Checked="True" />
					</div>
				</div>
			</div>
		</div>
	</asp:Panel>

	<aspdnsf:FilteredListing runat="server"
		ID="FilteredListing"
		SqlQuery="SELECT {0} * FROM BadWord WHERE {1}"
		SortExpression="Word">
		<Filters>
			<aspdnsf:StringFilter runat="server" Label="Word" FieldName="Word" />
		</Filters>
		<ListingTemplate>
			<div class="white-ui-box">
				<asp:GridView CssClass="table" ID="gMain" runat="server"
					DataSourceID="FilteredListingDataSource"
					PagerSettings-Position="TopAndBottom"
					AutoGenerateColumns="False"
					OnRowCommand="gMain_RowCommand"
					BorderStyle="None"
					BorderWidth="0px"
					CellPadding="0"
					GridLines="None"
					CellSpacing="-1"
					ShowFooter="True">
					<Columns>
						<asp:BoundField
							HeaderText="ID"
							ItemStyle-Width="5%"
							DataField="BadWordID" />

						<asp:BoundField
							HeaderText="Word"
							DataField="Word" />

						<asp:BoundField
							HeaderText="Created On"
							DataField="CreatedOn" />

						<asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Delete %>">
							<ItemTemplate>
								<asp:LinkButton ID="lnkDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("BadWordID") %>' runat="Server" Text='<i class="fa fa-times"></i> Delete' CssClass="delete-link" />
							</ItemTemplate>
							<ItemStyle CssClass="selectData" Width="5%" />
						</asp:TemplateField>
					</Columns>

					<EmptyDataTemplate>
						<div class="alert alert-info">
							<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EmptyDataTemplate.NoResults %>" />
						</div>
					</EmptyDataTemplate>

				</asp:GridView>
			</div>
		</ListingTemplate>
	</aspdnsf:FilteredListing>

</asp:Content>
