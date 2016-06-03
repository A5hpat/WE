<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.EntityToStoreMapper" Codebehind="EntityToStoreMapper.ascx.cs" %>
<%@ Register TagPrefix="aspdnsf" TagName="StoreSelector" Src="StoreSelector.ascx" %>


<asp:Literal ID="litWarning" runat="server" />

<aspdnsf:StoreSelector ID="ssMain" runat="server" 
    SelectMode="MultiCheckList" 
    AutoPostBack="false" 
    ListRepeatDirection="Vertical"
    />

