<%@ Control Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontControls.AlertMessageTemplate" Codebehind="AlertMessageTemplate.ascx.cs" %>

<button type='button' class='close' data-dismiss='alert'>
	<span aria-hidden='true'>&times;</span>
	<span class='sr-only'>Close</span>
</button>

<%# DataBinder.Eval(Container, "Message") %>
